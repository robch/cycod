using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Utility class for handling configuration paths with dot notation.
/// </summary>
public static class ConfigPathHelpers
{
    private static readonly Regex EnvVarRegex = new Regex("^[A-Z0-9_]+$", RegexOptions.Compiled);
    
    /// <summary>
    /// Dictionary mapping between dot notation paths and environment variable names.
    /// </summary>
    private static readonly Dictionary<string, string> PathToEnvVarMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Azure OpenAI mappings
        { "Azure.OpenAI.ApiKey", "AZURE_OPENAI_API_KEY" },
        { "Azure.OpenAI.Endpoint", "AZURE_OPENAI_ENDPOINT" },
        { "Azure.OpenAI.ChatDeployment", "AZURE_OPENAI_CHAT_DEPLOYMENT" },
        
        // OpenAI mappings
        { "OpenAI.ApiKey", "OPENAI_API_KEY" },
        { "OpenAI.ChatModelName", "OPENAI_CHAT_MODEL_NAME" },
        { "OpenAI.SystemPrompt", "OPENAI_SYSTEM_PROMPT" },
        
        // GitHub mappings
        { "GitHub.Token", "GITHUB_TOKEN" },
        
        // Copilot mappings
        { "Copilot.ModelName", "COPILOT_MODEL_NAME" },
        { "Copilot.ApiEndpoint", "COPILOT_API_ENDPOINT" },
        { "Copilot.IntegrationId", "COPILOT_INTEGRATION_ID" },
        { "Copilot.HmacKey", "COPILOT_HMAC_KEY" },
        { "Copilot.EditorVersion", "COPILOT_EDITOR_VERSION" }
    };
    
    /// <summary>
    /// Dictionary mapping from environment variable names to dot notation paths.
    /// </summary>
    private static readonly Dictionary<string, string> EnvVarToPathMap;
    
    /// <summary>
    /// Static constructor to initialize the reverse lookup dictionary.
    /// </summary>
    static ConfigPathHelpers()
    {
        // Initialize the reverse mapping dictionary
        EnvVarToPathMap = PathToEnvVarMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a path in dot notation to a flat environment variable format.
    /// Example: "Azure.OpenAI.ApiKey" -> "AZURE_OPENAI_API_KEY"
    /// </summary>
    /// <param name="path">The path in dot notation.</param>
    /// <returns>The path in environment variable format.</returns>
    public static string ToEnvVar(string path)
    {
        if (EnvVarRegex.IsMatch(path))
        {
            return path; // Already in env var format
        }

        // Check for special case mapping
        if (PathToEnvVarMap.TryGetValue(path, out string? envVar))
        {
            return envVar;
        }
        
        // For all other cases, use the general algorithm
        // Split by dots first
        string[] parts = path.Split('.');
        
        // Convert each part to uppercase
        for (int i = 0; i < parts.Length; i++)
        {
            // Insert underscores between camel case words
            parts[i] = Regex.Replace(parts[i], "([a-z])([A-Z])", "$1_$2").ToUpperInvariant();
        }
        
        // Join with underscore
        return string.Join("_", parts);
    }

    /// <summary>
    /// Converts a path from environment variable format to dot notation.
    /// Example: "AZURE_OPENAI_API_KEY" -> "Azure.OpenAI.ApiKey"
    /// </summary>
    /// <param name="envVar">The path in environment variable format.</param>
    /// <returns>The path in dot notation.</returns>
    public static string FromEnvVar(string envVar)
    {
        if (!EnvVarRegex.IsMatch(envVar))
        {
            return envVar; // Not in env var format
        }

        // Check for special case mapping
        if (EnvVarToPathMap.TryGetValue(envVar, out string? path))
        {
            return path;
        }
            
        var parts = envVar.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
            }
        }

        return string.Join(".", parts);
    }

    /// <summary>
    /// Normalizes a configuration path to ensure consistent casing.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public static string Normalize(string path)
    {
        if (EnvVarRegex.IsMatch(path))
        {
            // Convert from env var format to dot notation first
            path = FromEnvVar(path);
        }

        var parts = path.Split('.');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }
        }

        return string.Join(".", parts);
    }
}