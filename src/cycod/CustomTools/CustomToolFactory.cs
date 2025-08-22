using System.Text;
using CycoDev.CustomTools.Models;
using Microsoft.Extensions.AI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CycoDev.CustomTools
{
    /// <summary>
    /// Factory for creating and managing custom tools.
    /// </summary>
    public class CustomToolFactory
    {
        private static readonly string LocalScopePath = Path.Combine(".cycod", "tools");
        private static readonly string UserScopePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cycod", "tools");
        private static readonly string GlobalScopePath = GetGlobalScopePath();

        private readonly Dictionary<string, CustomToolDefinition> _tools = new();
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomToolFactory"/> class.
        /// </summary>
        public CustomToolFactory()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
        }

        /// <summary>
        /// Loads all tools from all scopes.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAllToolsAsync()
        {
            await LoadToolsFromScopeAsync(ToolScope.Local);
            await LoadToolsFromScopeAsync(ToolScope.User);
            await LoadToolsFromScopeAsync(ToolScope.Global);
        }

        /// <summary>
        /// Loads tools from a specific scope.
        /// </summary>
        /// <param name="scope">The scope to load tools from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadToolsFromScopeAsync(ToolScope scope)
        {
            var directory = GetDirectoryForScope(scope);
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(directory, "*.yaml"))
            {
                try
                {
                    var toolName = Path.GetFileNameWithoutExtension(file);
                    var yaml = await File.ReadAllTextAsync(file);
                    var tool = _deserializer.Deserialize<CustomToolDefinition>(yaml);

                    // If name is not specified in the YAML, use the filename
                    if (string.IsNullOrEmpty(tool.Name))
                    {
                        tool.Name = toolName;
                    }

                    if (tool.Validate(out string? errorMessage))
                    {
                        _tools[tool.Name] = tool;
                        ConsoleHelpers.WriteDebugLine($"Loaded custom tool '{tool.Name}' from {file}");
                    }
                    else
                    {
                        ConsoleHelpers.WriteErrorLine($"Invalid tool '{tool.Name}' from {file}: {errorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteErrorLine($"Error loading custom tool from {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Adds a tool definition to the factory.
        /// </summary>
        /// <param name="tool">The tool definition to add.</param>
        public void AddTool(CustomToolDefinition tool)
        {
            if (tool.Validate(out string? errorMessage))
            {
                _tools[tool.Name] = tool;
                ConsoleHelpers.WriteDebugLine($"Added custom tool '{tool.Name}'");
            }
            else
            {
                ConsoleHelpers.WriteErrorLine($"Invalid tool '{tool.Name}': {errorMessage}");
            }
        }

        /// <summary>
        /// Gets a tool by name.
        /// </summary>
        /// <param name="name">The name of the tool.</param>
        /// <returns>The tool definition, or null if not found.</returns>
        public CustomToolDefinition? GetTool(string name)
        {
            return _tools.TryGetValue(name, out var tool) ? tool : null;
        }

        /// <summary>
        /// Gets all tools.
        /// </summary>
        /// <returns>All tools.</returns>
        public IEnumerable<CustomToolDefinition> GetAllTools()
        {
            return _tools.Values;
        }

        /// <summary>
        /// Creates AITool functions for all loaded custom tools.
        /// </summary>
        /// <returns>An enumerable of AITool functions.</returns>
        public IEnumerable<AITool> CreateAITools()
        {
            var tools = new List<AITool>();

            foreach (var tool in _tools.Values)
            {
                var aiTool = CreateAITool(tool);
                if (aiTool != null)
                {
                    tools.Add(aiTool);
                }
            }

            return tools;
        }

        /// <summary>
        /// Creates an AITool function for a custom tool.
        /// </summary>
        /// <param name="tool">The custom tool definition.</param>
        /// <returns>An AITool function, or null if the tool is invalid.</returns>
        private AITool? CreateAITool(CustomToolDefinition tool)
        {
            // TODO: Implement AITool creation from tool definition
            return null;
        }

        /// <summary>
        /// Creates a custom tool from a YAML string.
        /// </summary>
        /// <param name="yaml">The YAML string.</param>
        /// <returns>The custom tool definition.</returns>
        public CustomToolDefinition CreateToolFromYaml(string yaml)
        {
            return _deserializer.Deserialize<CustomToolDefinition>(yaml);
        }

        /// <summary>
        /// Saves a tool definition to disk.
        /// </summary>
        /// <param name="tool">The tool definition to save.</param>
        /// <param name="scope">The scope to save to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveToolAsync(CustomToolDefinition tool, ToolScope scope)
        {
            var directory = GetDirectoryForScope(scope);
            Directory.CreateDirectory(directory);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(tool);
            var filePath = Path.Combine(directory, $"{tool.Name}.yaml");

            await File.WriteAllTextAsync(filePath, yaml);
            _tools[tool.Name] = tool;

            ConsoleHelpers.WriteDebugLine($"Saved custom tool '{tool.Name}' to {filePath}");
        }

        /// <summary>
        /// Removes a tool from disk and from the factory.
        /// </summary>
        /// <param name="name">The name of the tool to remove.</param>
        /// <param name="scope">The scope to remove from.</param>
        /// <returns>True if the tool was removed, false otherwise.</returns>
        public bool RemoveTool(string name, ToolScope scope)
        {
            var directory = GetDirectoryForScope(scope);
            var filePath = Path.Combine(directory, $"{name}.yaml");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _tools.Remove(name);
                ConsoleHelpers.WriteDebugLine($"Removed custom tool '{name}' from {filePath}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all tools from a specific scope.
        /// </summary>
        /// <param name="scope">The scope to get tools from.</param>
        /// <returns>All tools in the scope.</returns>
        public IEnumerable<CustomToolDefinition> GetToolsFromScope(ToolScope scope)
        {
            var directory = GetDirectoryForScope(scope);
            if (!Directory.Exists(directory))
            {
                return Enumerable.Empty<CustomToolDefinition>();
            }

            var tools = new List<CustomToolDefinition>();

            foreach (var file in Directory.GetFiles(directory, "*.yaml"))
            {
                try
                {
                    var yaml = File.ReadAllText(file);
                    var tool = _deserializer.Deserialize<CustomToolDefinition>(yaml);

                    if (string.IsNullOrEmpty(tool.Name))
                    {
                        tool.Name = Path.GetFileNameWithoutExtension(file);
                    }

                    if (tool.Validate(out string? errorMessage))
                    {
                        tools.Add(tool);
                    }
                    else
                    {
                        ConsoleHelpers.WriteErrorLine($"Invalid tool '{tool.Name}' from {file}: {errorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteErrorLine($"Error loading custom tool from {file}: {ex.Message}");
                }
            }

            return tools;
        }

        /// <summary>
        /// Gets the directory for a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>The directory path.</returns>
        private static string GetDirectoryForScope(ToolScope scope)
        {
            return scope switch
            {
                ToolScope.Local => LocalScopePath,
                ToolScope.User => UserScopePath,
                ToolScope.Global => GlobalScopePath,
                _ => throw new ArgumentException($"Invalid scope: {scope}", nameof(scope))
            };
        }

        /// <summary>
        /// Gets the global scope path based on the operating system.
        /// </summary>
        /// <returns>The global scope path.</returns>
        private static string GetGlobalScopePath()
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CYCOD", "tools");
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return "/usr/local/share/cycod/tools";
            }
            else
            {
                // Default to user path for unsupported OS
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cycod", "tools");
            }
        }

        /// <summary>
        /// Executes a custom tool.
        /// </summary>
        /// <param name="name">The name of the tool to execute.</param>
        /// <param name="parameters">The parameters to pass to the tool.</param>
        /// <returns>The result of the tool execution.</returns>
        public async Task<string> ExecuteToolAsync(string name, Dictionary<string, object?> parameters)
        {
            // TODO: Implement tool execution
            return "Not implemented";
        }
    }

    /// <summary>
    /// Scope for custom tools.
    /// </summary>
    public enum ToolScope
    {
        /// <summary>
        /// Local scope (current directory).
        /// </summary>
        Local,

        /// <summary>
        /// User scope (current user).
        /// </summary>
        User,

        /// <summary>
        /// Global scope (all users).
        /// </summary>
        Global,

        /// <summary>
        /// Any scope.
        /// </summary>
        Any
    }
}