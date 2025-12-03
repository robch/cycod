using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using System.ClientModel;
using System.Text.Json;
using System.Text;

/// <summary>
/// Custom pipeline policy that adds the Copilot-Vision-Request header
/// when the request payload contains image content
/// </summary>
public class VisionHeaderPolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Processing request");

        // Check if message contains image content
        if (ContainsImageContent(message))
        {
            // Add the vision header
            message.Request.Headers.Set("Copilot-Vision-Request", "true");
            ConsoleHelpers.WriteDebugLine("Added Copilot-Vision-Request header for image content");
        }

        // Proceed with the next policy in the pipeline
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Processing request (async)");

        // Check if message contains image content
        if (ContainsImageContent(message))
        {
            // Add the vision header
            message.Request.Headers.Set("Copilot-Vision-Request", "true");
            ConsoleHelpers.WriteDebugLine("Added Copilot-Vision-Request header for image content");
        }

        // Proceed with the next policy in the pipeline
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private bool ContainsImageContent(PipelineMessage message)
    {
        try
        {
            // Get the content as string
            if (message?.Request?.Content == null)
                return false;

            using MemoryStream contentStream = new();
            message.Request.Content.WriteTo(contentStream);
            contentStream.Position = 0;

            var contentData = BinaryData.FromStream(contentStream);
            var contentString = contentData.ToString();

            ConsoleHelpers.WriteDebugLine($"VisionHeaderPolicy: Checking payload content (first 500 chars): {contentString.Substring(0, Math.Min(500, contentString.Length))}");

            // Check for various image content patterns
            bool hasImageContent = false;

            // Legacy format with "attached content" prefix
            if (contentString.Contains("attached content") && 
                contentString.Contains("\"type\":\"image") && 
                contentString.Contains("\"url\":\"data:image"))
            {
                hasImageContent = true;
                ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Found legacy image format");
            }
            
            // Modern OpenAI format with image_url
            if (contentString.Contains("\"type\":\"image_url\""))
            {
                hasImageContent = true;
                ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Found modern image_url format");
            }
            
            // Base64 encoded images
            if (contentString.Contains("\"image_url\"") && contentString.Contains("data:image/"))
            {
                hasImageContent = true;
                ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Found base64 encoded image");
            }
            
            // Check for image content in messages array
            if (contentString.Contains("\"content\"") && 
                (contentString.Contains("image/png") || contentString.Contains("image/jpeg") || contentString.Contains("image/webp")))
            {
                hasImageContent = true;
                ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Found image content by media type");
            }

            ConsoleHelpers.WriteDebugLine($"VisionHeaderPolicy: Image content detection result: {hasImageContent}");
            return hasImageContent;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error while checking for image content: {ex.Message}");
            return false;
        }
    }
}
