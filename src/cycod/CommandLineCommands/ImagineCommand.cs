#pragma warning disable MEAI001 // Microsoft.Extensions.AI types are experimental

using Microsoft.Extensions.AI;
using Azure;

public class ImagineCommand : CommandWithVariables
{
    public List<string> Prompts { get; set; } = [];
    public int Count { get; set; } = 1;
    public string Size { get; set; } = "1024x1024";
    public string Style { get; set; } = "vivid";
    public string Quality { get; set; } = "standard";
    public string OutputDirectory { get; set; } = ".";
    public string Format { get; set; } = "png";
    public string? Provider { get; set; } = null;

    public override string GetCommandName()
    {
        return "imagine";
    }

    public override bool IsEmpty()
    {
        return Prompts.Count == 0;
    }

    public override CommandWithVariables Clone()
    {
        var clone = new ImagineCommand
        {
            Prompts = new List<string>(this.Prompts),
            Count = this.Count,
            Size = this.Size,
            Style = this.Style,
            Quality = this.Quality,
            OutputDirectory = this.OutputDirectory,
            Format = this.Format,
            Provider = this.Provider,
            Variables = new Dictionary<string, string>(this.Variables),
            ForEachVariables = new List<ForEachVariable>(this.ForEachVariables)
        };
        return clone;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        try
        {
            if (Prompts.Count == 0)
            {
                ConsoleHelpers.WriteErrorLine("No prompts provided. Use --input or provide prompts as arguments.");
                return 1;
            }

            ConsoleHelpers.WriteLine($"Generating {Prompts.Count} image{(Prompts.Count == 1 ? "" : "s")}...\n", ConsoleColor.Cyan);

            // Create a working image generator (using the pattern from ImageAIExample)
            var imageGenerator = CreateWorkingImageGenerator();
            
            // Ensure output directory exists
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
                ConsoleHelpers.WriteDebugLine($"Created output directory: {OutputDirectory}");
            }

            int totalGenerated = 0;
            foreach (var prompt in Prompts)
            {
                ConsoleHelpers.WriteLine($"Generating {Count} image{(Count == 1 ? "" : "s")} for: \"{prompt}\"", ConsoleColor.DarkGray);
                
                // Generate and save each image immediately for consistent behavior
                for (int imageIndex = 0; imageIndex < Count; imageIndex++)
                {
                    var options = new ImageGenerationOptions
                    {
                        Count = 1, // Always generate one image at a time
                        ModelId = "dall-e-3",
                        AdditionalProperties = new Microsoft.Extensions.AI.AdditionalPropertiesDictionary
                        {
                            ["size"] = Size,
                            ["style"] = Style,
                            ["quality"] = Quality,
                            ["format"] = Format
                        }
                    };

                    var response = await imageGenerator.GenerateAsync(new ImageGenerationRequest(prompt), options);
                    
                    // Save this single image immediately
                    totalGenerated += await SaveGeneratedImages(response, prompt, totalGenerated);
                }
            }

            ConsoleHelpers.WriteLine($"\nSuccessfully generated {totalGenerated} image{(totalGenerated == 1 ? "" : "s")}!", ConsoleColor.Green);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error generating images: {ex.Message}");
            Logger.Error($"Image generation failed: {ex}");
            return 1;
        }
    }

    private IImageGenerator CreateWorkingImageGenerator()
    {
        // Try Azure OpenAI first (following ChatClientFactory pattern)
        var azureApiKey = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY");
        var azureEndpoint = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_ENDPOINT");
        
        if (!string.IsNullOrEmpty(azureApiKey) && !string.IsNullOrEmpty(azureEndpoint))
        {
            return CreateAzureOpenAIImageGenerator(azureEndpoint, azureApiKey);
        }
        
        // Fall back to standard OpenAI
        var openAIApiKey = EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(openAIApiKey))
        {
            return CreateStandardOpenAIImageGenerator(openAIApiKey);
        }
        
        throw new EnvVarSettingException("Either AZURE_OPENAI_API_KEY + AZURE_OPENAI_ENDPOINT or OPENAI_API_KEY environment variables are required for image generation");
    }

    private IImageGenerator CreateAzureOpenAIImageGenerator(string endpoint, string apiKey)
    {
        try
        {
            ConsoleHelpers.WriteDebugLine("Creating Azure OpenAI image generator");
            
            // Use the DALL-E deployment we just created
            var deploymentName = "dall-e-3";
            
            // Create Azure OpenAI client the same way ChatClientFactory does it
            var client = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            var imageClient = client.GetImageClient(deploymentName);
            
            // Wrap it in our adapter
            return new OpenAIImageGeneratorWrapper(imageClient);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Failed to create Azure OpenAI image generator: {ex.Message}");
            throw;
        }
    }

    private IImageGenerator CreateStandardOpenAIImageGenerator(string apiKey)
    {
        try
        {
            ConsoleHelpers.WriteDebugLine("Creating standard OpenAI image generator");
            
            var model = "dall-e-3";
            var endpoint = EnvironmentHelpers.FindEnvVar("OPENAI_ENDPOINT"); // Optional custom endpoint
            
            // Create OpenAI client
            var options = new OpenAI.OpenAIClientOptions();
            if (!string.IsNullOrEmpty(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }
            
            var openAIClient = new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), options);
            var imageClient = openAIClient.GetImageClient(model);
            
            return new OpenAIImageGeneratorWrapper(imageClient);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Failed to create standard OpenAI image generator: {ex.Message}");
            throw;
        }
    }

    private IImageGenerator CreateMockImageGenerator()
    {
        // Use the pattern from ImageAIExample that actually works
        return new TestImageGenerator
        {
            GenerateImagesAsyncCallback = (request, options, ct) =>
            {
                // Always generate one image (consistent with new approach)
                var mockImageBytes = CreateMockImageData(request.Prompt ?? "unknown");
                var content = new DataContent(mockImageBytes, "image/png") { Name = "generated_image.png" };
                var response = new ImageGenerationResponse([content]);
                return Task.FromResult(response);
            }
        };
    }

    private async Task<int> SaveGeneratedImages(ImageGenerationResponse response, string prompt, int startIndex)
    {
        int saved = 0;
        
        for (int i = 0; i < response.Contents.Count; i++)
        {
            var content = response.Contents[i];
            
            if (content is DataContent dataContent)
            {
                // Save binary image data
                var fileName = GenerateFileName(prompt, startIndex + i + 1);
                var filePath = Path.Combine(OutputDirectory, fileName);
                
                await File.WriteAllBytesAsync(filePath, dataContent.Data.ToArray());
                ConsoleHelpers.WriteLine($"Saved: {fileName}", ConsoleColor.DarkGray);
                saved++;
            }
            else if (content is UriContent uriContent)
            {
                // Download and save from URL
                using var httpClient = new HttpClient();
                var imageData = await httpClient.GetByteArrayAsync(uriContent.Uri);
                
                var fileName = GenerateFileName(prompt, startIndex + i + 1);
                var filePath = Path.Combine(OutputDirectory, fileName);
                
                await File.WriteAllBytesAsync(filePath, imageData);
                ConsoleHelpers.WriteLine($"Saved: {fileName}", ConsoleColor.DarkGray);
                saved++;
            }
            else
            {
                ConsoleHelpers.WriteWarning($"Unexpected content type: {content.GetType().Name}");
            }
        }
        
        return saved;
    }

    private string GenerateFileName(string prompt, int index)
    {
        // Create a safe filename from the prompt
        var safePrompt = string.Concat(prompt
            .Take(20)
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
            .Replace(' ', '_');
        
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{safePrompt}_{timestamp}_{index:D2}.{Format}";
    }

    private byte[] CreateMockImageData(string prompt)
    {
        // Create a simple but valid PNG file
        // This is a 1x1 transparent PNG with the prompt as metadata
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG signature
        var ihdrChunk = new byte[] { 
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
            0x49, 0x48, 0x44, 0x52, // IHDR
            0x00, 0x00, 0x00, 0x01, // Width: 1
            0x00, 0x00, 0x00, 0x01, // Height: 1 
            0x08, 0x06, 0x00, 0x00, 0x00, // Bit depth: 8, Color type: 6 (RGBA), Compression, Filter, Interlace
            0x1F, 0x15, 0xC4, 0x89  // CRC
        };
        var idatChunk = new byte[] {
            0x00, 0x00, 0x00, 0x0B, // IDAT chunk length
            0x49, 0x44, 0x41, 0x54, // IDAT
            0x78, 0x9C, 0x62, 0x00, 0x02, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4 // Compressed data + CRC
        };
        var iendChunk = new byte[] {
            0x00, 0x00, 0x00, 0x00, // IEND chunk length
            0x49, 0x45, 0x4E, 0x44, // IEND
            0xAE, 0x42, 0x60, 0x82  // CRC
        };

        var result = new List<byte>();
        result.AddRange(pngHeader);
        result.AddRange(ihdrChunk);
        result.AddRange(idatChunk);
        result.AddRange(iendChunk);
        
        return result.ToArray();
    }
}

// Test implementation (from ImageAIExample working code)
public sealed class TestImageGenerator : IImageGenerator
{
    public Func<ImageGenerationRequest, ImageGenerationOptions?, CancellationToken, Task<ImageGenerationResponse>>? GenerateImagesAsyncCallback { get; set; }

    public Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        return GenerateImagesAsyncCallback?.Invoke(request, options, cancellationToken) ??
            Task.FromResult(new ImageGenerationResponse());
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose() { }
}

// Real OpenAI wrapper that implements Microsoft.Extensions.AI.IImageGenerator
public sealed class OpenAIImageGeneratorWrapper : IImageGenerator
{
    private readonly OpenAI.Images.ImageClient _imageClient;

    public OpenAIImageGeneratorWrapper(OpenAI.Images.ImageClient imageClient)
    {
        _imageClient = imageClient;
    }

    public async Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create OpenAI-specific image generation request
            var prompt = request.Prompt ?? "A beautiful image";
            var count = options?.Count ?? 1;
            
            // Since we now always generate one image at a time, warn if count != 1
            if (count != 1)
            {
                ConsoleHelpers.WriteWarning($"OpenAI wrapper called with count={count}, but will only generate 1 image. Use command-level Count handling instead.");
            }
            
            // Get custom options from AdditionalProperties
            var sizeStr = options?.AdditionalProperties?.GetValueOrDefault("size") as string ?? "1024x1024";
            var style = options?.AdditionalProperties?.GetValueOrDefault("style") as string ?? "vivid";
            var quality = options?.AdditionalProperties?.GetValueOrDefault("quality") as string ?? "standard";
            var format = options?.AdditionalProperties?.GetValueOrDefault("format") as string ?? "png";
            
            ConsoleHelpers.WriteDebugLine($"Calling OpenAI DALL-E API with prompt: '{prompt}', size: {sizeStr}, style: {style}, quality: {quality}");
            
            // Parse size string to OpenAI enum
            var imageSize = sizeStr switch
            {
                "256x256" => OpenAI.Images.GeneratedImageSize.W256xH256,
                "512x512" => OpenAI.Images.GeneratedImageSize.W512xH512,
                "1024x1024" => OpenAI.Images.GeneratedImageSize.W1024xH1024,
                "1792x1024" => OpenAI.Images.GeneratedImageSize.W1792xH1024,
                "1024x1792" => OpenAI.Images.GeneratedImageSize.W1024xH1792,
                _ => OpenAI.Images.GeneratedImageSize.W1024xH1024
            };
            
            // Parse style
            var imageStyle = style.ToLower() switch
            {
                "natural" => OpenAI.Images.GeneratedImageStyle.Natural,
                "vivid" => OpenAI.Images.GeneratedImageStyle.Vivid,
                _ => OpenAI.Images.GeneratedImageStyle.Vivid
            };
            
            // Parse quality
            var imageQuality = quality.ToLower() switch
            {
                "hd" => OpenAI.Images.GeneratedImageQuality.High,
                "standard" => OpenAI.Images.GeneratedImageQuality.Standard,
                _ => OpenAI.Images.GeneratedImageQuality.Standard
            };
            
            // Parse format (note: API returns URLs, actual format is handled at download)
            var responseFormat = format.ToLower() == "b64_json" 
                ? OpenAI.Images.GeneratedImageFormat.Bytes 
                : OpenAI.Images.GeneratedImageFormat.Uri;
            
            // Create image generation options
            var imageOptions = new OpenAI.Images.ImageGenerationOptions()
            {
                Size = imageSize,
                Style = imageStyle,
                Quality = imageQuality,
                ResponseFormat = responseFormat
            };
            
            // Generate single image
            var response = await _imageClient.GenerateImagesAsync(prompt, 1, imageOptions, cancellationToken);
            var allContents = new List<AIContent>();
            
            foreach (var image in response.Value)
            {
                if (!string.IsNullOrEmpty(image.ImageUri?.ToString()))
                {
                    // Image URL returned - will be downloaded when saved
                    allContents.Add(new UriContent(image.ImageUri, $"image/{format}"));
                    ConsoleHelpers.WriteDebugLine($"Generated image URL: {image.ImageUri}");
                }
                else if (image.ImageBytes != null)
                {
                    // Base64 data returned
                    allContents.Add(new DataContent(image.ImageBytes.ToArray(), $"image/{format}"));
                    ConsoleHelpers.WriteDebugLine($"Generated image bytes: {image.ImageBytes.Length} bytes");
                }
            }

            return new ImageGenerationResponse(allContents);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"OpenAI image generation failed: {ex.Message}", ex);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose()
    {
        // ImageClient doesn't implement IDisposable
    }
}