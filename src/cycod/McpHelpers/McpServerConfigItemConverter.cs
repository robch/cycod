using System.Text.Json;
using System.Text.Json.Serialization;
/// <summary>
/// JSON converter for IMcpServerConfigItem
/// </summary>
public class McpServerConfigItemConverter : JsonConverter<IMcpServerConfigItem>
{
    /// <summary>
    /// Reads and converts JSON to the appropriate MCP server config class
    /// </summary>
    public override IMcpServerConfigItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        
        if (root.TryGetProperty("Type", out var typeElement) || root.TryGetProperty("type", out typeElement))
        {
            var type = typeElement.GetString();
            
            if (type?.ToLowerInvariant() == "sse")
            {
                var sseConfig = new SseServerConfig();
                if (root.TryGetProperty("Url", out var urlElement) || root.TryGetProperty("url", out urlElement))
                {
                    sseConfig.Url = urlElement.GetString();
                }
                return sseConfig;
            }
            else // default to stdio
            {
                var stdioConfig = new StdioServerConfig();
                
                if (root.TryGetProperty("Command", out var commandElement) || root.TryGetProperty("command", out commandElement))
                {
                    stdioConfig.Command = commandElement.GetString() ?? "";
                }
                
                if ((root.TryGetProperty("Args", out var argsElement) || root.TryGetProperty("args", out argsElement)) && 
                    argsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var arg in argsElement.EnumerateArray())
                    {
                        stdioConfig.Args.Add(arg.GetString() ?? "");
                    }
                }
                
                if ((root.TryGetProperty("Env", out var envElement) || root.TryGetProperty("env", out envElement)) && 
                    envElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var env in envElement.EnumerateObject())
                    {
                        stdioConfig.Env[env.Name] = env.Value.GetString() ?? "";
                    }
                }
                
                return stdioConfig;
            }
        }
        
        // Default to stdio if type is not specified
        return new StdioServerConfig();
    }

    /// <summary>
    /// Writes the MCP server config to JSON
    /// </summary>
    public override void Write(Utf8JsonWriter writer, IMcpServerConfigItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Write the Type property first
        writer.WriteString("Type", value.Type);
        
        // Write StdioServerConfig specific properties
        if (value is StdioServerConfig stdioConfig)
        {
            // Write Command property (always include it even if empty)
            writer.WriteString("Command", stdioConfig.Command ?? string.Empty);
            
            // Write Args array if it has items
            if (stdioConfig.Args != null && stdioConfig.Args.Count > 0)
            {
                writer.WritePropertyName("Args");
                writer.WriteStartArray();
                foreach (var arg in stdioConfig.Args)
                {
                    writer.WriteStringValue(arg);
                }
                writer.WriteEndArray();
            }
            
            // Write Env dictionary if it has items
            if (stdioConfig.Env != null && stdioConfig.Env.Count > 0)
            {
                writer.WritePropertyName("Env");
                writer.WriteStartObject();
                foreach (var kvp in stdioConfig.Env)
                {
                    writer.WriteString(kvp.Key, kvp.Value);
                }
                writer.WriteEndObject();
            }
        }
        // Write SseServerConfig specific properties
        else if (value is SseServerConfig sseConfig)
        {
            if (!string.IsNullOrEmpty(sseConfig.Url))
            {
                writer.WriteString("Url", sseConfig.Url);
            }
        }
        
        writer.WriteEndObject();
    }
}