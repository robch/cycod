using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class GitHubCopilotModelsHelpers
{
    // API endpoint for Copilot models
    private const string MODELS_API_URL = "https://api.githubcopilot.com/models";
    
    private readonly HttpClient _httpClient;

    public GitHubCopilotModelsHelpers()
    {
        _httpClient = new HttpClient(new LogTrafficHttpMessageHandler());
    }

    /// <summary>
    /// Gets the available models from the GitHub Copilot API
    /// </summary>
    /// <param name="copilotToken">The Copilot authentication token</param>
    /// <returns>The list of available models</returns>
    public async Task<ModelsResponse> GetModelsAsync(string copilotToken, string editorVersion = "vscode/1.85.0")
    {
        // Create new request message to control headers exactly
        var request = new HttpRequestMessage(HttpMethod.Get, MODELS_API_URL);
        
        // Add the authorization header with the Copilot token
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", copilotToken);
        
        // Add required Editor-Version header
        request.Headers.Add("Editor-Version", editorVersion);
        request.Headers.Add("Accept", "application/json");
        
        try
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            ConsoleHelpers.WriteDebugLine($"Copilot models response: {responseJson}");
            
            var modelsData = JsonSerializer.Deserialize<ModelsResponse>(responseJson);
            
            if (modelsData == null)
            {
                throw new InvalidOperationException("Failed to parse Copilot models response");
            }
            
            return modelsData;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 401))
        {
            // Token has expired or is invalid
            ConsoleHelpers.WriteLine("Your GitHub Copilot authentication has expired or is invalid.", ConsoleColor.Yellow, overrideQuiet: true);
            ConsoleHelpers.WriteLine("Please run 'cycod github login' to re-authenticate with GitHub.", ConsoleColor.Yellow, overrideQuiet: true);
            
            throw new GitHubTokenExpiredException("GitHub Copilot authentication has expired. Please run 'cycod github login' to re-authenticate.", ex);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400") || (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 400))
        {
            // API request is malformed
            throw new InvalidOperationException("Bad request to GitHub Copilot API. This might be due to missing or invalid headers.", ex);
        }
    }

    /// <summary>
    /// Gets the available models from the GitHub Copilot API (sync version)
    /// </summary>
    /// <param name="copilotToken">The Copilot authentication token</param>
    /// <returns>The list of available models</returns>
    public ModelsResponse GetModelsSync(string copilotToken, string editorVersion = "vscode/1.85.0")
    {
        try
        {
            return GetModelsAsync(copilotToken, editorVersion).GetAwaiter().GetResult();
        }
        catch (GitHubTokenExpiredException)
        {
            // Just re-throw GitHubTokenExpiredException directly since it already has the user-friendly message
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get Copilot models: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Formats the models list for display
    /// </summary>
    public string FormatModelsForDisplay(ModelsResponse modelsResponse)
    {
        if (modelsResponse.Data == null || modelsResponse.Data.Count == 0)
        {
            return "No models available.";
        }
        
        var modelsByVendor = new Dictionary<string, List<ModelInfo>>();
        foreach (var model in modelsResponse.Data)
        {
            var vendor = model.Vendor ?? "Unknown";
            if (!modelsByVendor.ContainsKey(vendor))
            {
                modelsByVendor[vendor] = new List<ModelInfo>();
            }
            modelsByVendor[vendor].Add(model);
        }
        
        var sb = new StringBuilder();
        foreach (var vendor in modelsByVendor.Keys)
        {
            sb.AppendLine(vendor.ToUpper());
            sb.AppendLine();
            
            var sortedModels = modelsByVendor[vendor].OrderBy(m => m.Name).ToList();
            foreach (var model in sortedModels)
            {
                var previewTag = model.Preview ? " (Preview)" : "";
                sb.AppendLine($"  {model.Name}{previewTag} [{model.Id}]");
                
                if (model.Capabilities != null)
                {
                    var limits = model.Capabilities.Limits;
                    if (limits != null)
                    {
                        var limitsList = new List<string>();
                        
                        if (limits.MaxContextWindowTokens.HasValue)
                            limitsList.Add($"Context: {limits.MaxContextWindowTokens} tokens");
                            
                        if (limits.MaxPromptTokens.HasValue)
                            limitsList.Add($"Prompt: {limits.MaxPromptTokens} tokens");
                            
                        if (limits.MaxOutputTokens.HasValue)
                            limitsList.Add($"Output: {limits.MaxOutputTokens} tokens");

                        if (limitsList.Count > 0)
                        {
                            sb.AppendLine($"  - Limits: {string.Join(", ", limitsList)}");
                        }
                    }
                    
                    if (model.Capabilities.Supports != null)
                    {
                        var supports = model.Capabilities.Supports;
                        var featuresList = new List<string>();
                        
                        if (supports.Streaming == true) featuresList.Add("streaming");
                        if (supports.ToolCalls == true) featuresList.Add("tool calls");
                        if (supports.ParallelToolCalls == true) featuresList.Add("parallel tool calls");
                        if (supports.Vision == true) featuresList.Add("vision");
                        if (supports.StructuredOutputs == true) featuresList.Add("structured outputs");
                        
                        if (featuresList.Count > 0)
                        {
                            sb.AppendLine($"  - Features: {string.Join(", ", featuresList)}");
                        }
                    }
                }
                
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }

    // Response classes for JSON deserialization
    public class ModelsResponse
    {
        [JsonPropertyName("data")]
        public List<ModelInfo>? Data { get; set; }
        
        [JsonPropertyName("object")]
        public string? Object { get; set; }
    }

    public class ModelInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("object")]
        public string? Object { get; set; }
        
        [JsonPropertyName("vendor")]
        public string? Vendor { get; set; }
        
        [JsonPropertyName("version")]
        public string? Version { get; set; }
        
        [JsonPropertyName("preview")]
        public bool Preview { get; set; }
        
        [JsonPropertyName("model_picker_enabled")]
        public bool ModelPickerEnabled { get; set; }
        
        [JsonPropertyName("capabilities")]
        public ModelCapabilities? Capabilities { get; set; }
    }

    public class ModelCapabilities
    {
        [JsonPropertyName("family")]
        public string? Family { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        [JsonPropertyName("tokenizer")]
        public string? Tokenizer { get; set; }
        
        [JsonPropertyName("supports")]
        public ModelSupports? Supports { get; set; }
        
        [JsonPropertyName("limits")]
        public ModelLimits? Limits { get; set; }
    }

    public class ModelSupports
    {
        [JsonPropertyName("streaming")]
        public bool? Streaming { get; set; }
        
        [JsonPropertyName("tool_calls")]
        public bool? ToolCalls { get; set; }
        
        [JsonPropertyName("parallel_tool_calls")]
        public bool? ParallelToolCalls { get; set; }
        
        [JsonPropertyName("vision")]
        public bool? Vision { get; set; }
        
        [JsonPropertyName("structured_outputs")]
        public bool? StructuredOutputs { get; set; }
    }

    public class ModelLimits
    {
        [JsonPropertyName("max_context_window_tokens")]
        public int? MaxContextWindowTokens { get; set; }
        
        [JsonPropertyName("max_prompt_tokens")]
        public int? MaxPromptTokens { get; set; }
        
        [JsonPropertyName("max_output_tokens")]
        public int? MaxOutputTokens { get; set; }
    }
}