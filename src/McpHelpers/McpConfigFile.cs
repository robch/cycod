using System.Text.Json;
using System.Text.Json.Serialization;
/// <summary>
/// Class representing an MCP configuration file that contains multiple servers
/// </summary>
public class McpConfigFile
{
    public const string DefaultFolderName = "mcp";
    public const string DefaultFileName = "mcp.json";

    /// <summary>
    /// Loads an MCP configuration file from a specific scope
    /// </summary>
    /// <param name="scope">The scope to load from</param>
    /// <returns>The loaded configuration file, or null if not found</returns>
    public static McpConfigFile? FromScope(ConfigFileScope scope)
    {
        var mcpDirectory = McpFileHelpers.FindMcpDirectoryInScope(scope);
        if (mcpDirectory == null)
        {
            return null;
        }
        
        var mcpFilePath = Path.Combine(mcpDirectory, McpConfigFile.DefaultFileName);
        if (!File.Exists(mcpFilePath))
        {
            return null;
        }
        
        try
        {
            var jsonString = File.ReadAllText(mcpFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new McpServerConfigItemConverter() }
            };
            
            var mcpConfig = JsonSerializer.Deserialize<McpServerConfig>(jsonString, options);
            if (mcpConfig == null)
            {
                return null;
            }
            
            var configFile = new McpConfigFile
            {
                Scope = scope,
                FileName = mcpFilePath,
                Servers = mcpConfig.McpServers
            };
            
            // Set the ConfigFile property on each server config
            foreach (var server in configFile.Servers.Values)
            {
                server.ConfigFile = configFile;
            }
            
            return configFile;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error loading MCP configuration file: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Saves the configuration file
    /// </summary>
    /// <returns>True if saved successfully, false otherwise</returns>
    public bool Save()
    {
        if (string.IsNullOrEmpty(FileName))
        {
            ConsoleHelpers.WriteDebugLine("Cannot save MCP configuration file: filename is not set");
            return false;
        }
        
        try
        {
            // Create a McpServerConfig to maintain format compatibility
            var mcpConfig = new McpServerConfig
            {
                McpServers = Servers
            };
            
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
            
            // Add a converter to handle polymorphic serialization of server configurations
            jsonOptions.Converters.Add(new McpServerConfigItemConverter());
            
            string jsonString = JsonSerializer.Serialize(mcpConfig, jsonOptions);
            FileHelpers.WriteAllText(FileName, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error saving MCP configuration file: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates a new MCP configuration file in the specified scope
    /// </summary>
    /// <param name="scope">The scope to create the file in</param>
    /// <returns>The created configuration file</returns>
    public static McpConfigFile Create(ConfigFileScope scope)
    {
        var mcpDirectory = McpFileHelpers.FindMcpDirectoryInScope(scope, create: true)!;
        var mcpFilePath = Path.Combine(mcpDirectory, McpConfigFile.DefaultFileName);
        
        return new McpConfigFile
        {
            Scope = scope,
            FileName = mcpFilePath
        };
    }

    /// <summary>
    /// The scope where this configuration file is located
    /// </summary>
    [JsonIgnore]
    public ConfigFileScope Scope { get; set; }
    
    /// <summary>
    /// The full path to the configuration file
    /// </summary>
    [JsonIgnore]
    public string FileName { get; set; } = "";
    
    /// <summary>
    /// Dictionary of server name to server configuration
    /// </summary>
    [JsonPropertyName("McpServers")]
    public Dictionary<string, IMcpServerConfigItem> Servers { get; set; } = new Dictionary<string, IMcpServerConfigItem>();
    
}
