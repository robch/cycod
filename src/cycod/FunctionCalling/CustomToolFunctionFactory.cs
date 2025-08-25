using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Function factory for custom tools, extending McpFunctionFactory to support both MCPs and custom tools.
/// </summary>
    public partial class CustomToolFunctionFactory : McpFunctionFactory
    {
        private readonly Dictionary<string, ToolDefinition> _customTools = new();
        private readonly Dictionary<string, ToolDefinition> _dependencyTools = new();
        
        // Static reference to the current factory for dependency resolution
        internal static CustomToolFunctionFactory? _currentFactory;
        
        /// <summary>
        /// Checks if a dependency tool exists in the current factory.
        /// </summary>
        /// <param name="toolName">The name of the dependency tool to check.</param>
        /// <returns>True if the dependency tool exists, false otherwise.</returns>
        internal bool HasDependencyTool(string toolName)
        {
            return _dependencyTools.ContainsKey(toolName);
        }
        
        /// <summary>
        /// Gets a dependency tool from the current factory.
        /// </summary>
        /// <param name="toolName">The name of the dependency tool to get.</param>
        /// <returns>The dependency tool if found, null otherwise.</returns>
        internal ToolDefinition? GetDependencyTool(string toolName)
        {
            return _dependencyTools.TryGetValue(toolName, out var tool) ? tool : null;
        }
        
        /// <summary>
        /// Creates a new CustomToolFunctionFactory.
        /// </summary>
        public CustomToolFunctionFactory() : base()
        {
            // Set the current factory reference
            _currentFactory = this;
        }
        
        /// <summary>
        /// Adds all custom tools from the specified scope to this factory.
        /// </summary>
        /// <param name="scope">The configuration scope to load tools from.</param>
        public void AddCustomTools(ConfigFileScope scope = ConfigFileScope.Any)
        {
            var tools = ToolFileHelpers.ListTools(scope);
            ConsoleHelpers.WriteDebugLine($"Found {tools.Count} custom tools in scope {scope}");
            
            foreach (var tool in tools)
            {
                AddCustomTool(tool);
            }
        }
        
        /// <summary>
        /// Adds a specific custom tool to this factory.
        /// </summary>
        /// <param name="tool">The tool definition to add.</param>
        public void AddCustomTool(ToolDefinition tool)
        {
            // Skip tools that are already registered
            if (_customTools.ContainsKey(tool.Name) || _dependencyTools.ContainsKey(tool.Name))
            {
                ConsoleHelpers.WriteDebugLine($"Custom tool '{tool.Name}' is already registered, skipping");
                return;
            }
            
            // Register this tool
            var customToolFunction = new CustomToolFunction(tool);
            _customTools[tool.Name] = tool;
            
            // Add the function to the base factory
            base.AddFunction(customToolFunction);
            
            // Load dependencies if any
            LoadToolDependencies(tool);
            
            ConsoleHelpers.WriteDebugLine($"Added custom tool '{tool.Name}' to function factory");
        }
        
        /// <summary>
        /// Loads dependencies for a tool.
        /// </summary>
        /// <param name="tool">The tool whose dependencies should be loaded</param>
        private void LoadToolDependencies(ToolDefinition tool)
        {
            if (tool.Uses == null) return;
            
            // Load tool dependencies
            if (!string.IsNullOrEmpty(tool.Uses.Tool))
            {
                LoadDependencyTool(tool.Uses.Tool);
            }
            
            if (tool.Uses.Tools != null)
            {
                foreach (var depToolName in tool.Uses.Tools)
                {
                    LoadDependencyTool(depToolName);
                }
            }
        }

        /// <summary>
        /// Loads a dependency tool that won't be exposed to the LLM.
        /// </summary>
        /// <param name="toolName">The name of the dependency tool to load</param>
        private void LoadDependencyTool(string toolName)
        {
            // Skip if already loaded
            if (_customTools.ContainsKey(toolName) || _dependencyTools.ContainsKey(toolName))
            {
                return;
            }
            
            // Load the tool
            var depTool = ToolFileHelpers.GetToolDefinition(toolName);
            if (depTool != null)
            {
                _dependencyTools[toolName] = depTool;
                ConsoleHelpers.WriteDebugLine($"Added dependency tool '{toolName}' (not exposed to LLM)");
                
                // Recursively load its dependencies
                LoadToolDependencies(depTool);
            }
        }
        
        /// <summary>
        /// Attempts to call a function with the given name and arguments.
        /// </summary>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="functionArguments">JSON string of function arguments.</param>
        /// <param name="result">The result of the function call, if successful.</param>
        /// <returns>True if the function call was successful, otherwise false.</returns>
        public override bool TryCallFunction(string functionName, string functionArguments, out object? result)
        {
            result = null;
            
            // Check if it's a directly specified tool
            if (!string.IsNullOrEmpty(functionName) && _customTools.TryGetValue(functionName, out var tool))
            {
                ConsoleHelpers.WriteDebugLine($"Found custom tool '{functionName}'");
                try
                {
                    var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(functionArguments, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    arguments ??= new Dictionary<string, object?>();
                    
                    ConsoleHelpers.WriteDebugLine($"Calling custom tool '{functionName}'");
                    var executor = new ToolExecutor(tool, arguments);
                    result = Task.Run(async () => await executor.ExecuteAsync()).Result;
                    
                    ConsoleHelpers.WriteDebugLine($"Custom tool '{functionName}' executed successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    result = $"Error executing custom tool '{functionName}': {ex.Message}";
                    ConsoleHelpers.WriteDebugLine($"Error executing custom tool '{functionName}': {ex.Message}");
                    return false;
                }
            }
            
            // We specifically DO NOT check for dependency tools here
            // This ensures the LLM cannot call them directly
            
            // If not a custom tool, use the base implementation
            return base.TryCallFunction(functionName, functionArguments, out result);
        }

        /// <summary>
        /// Determines if a function is read-only based on its name and tags.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>
        /// True if the function is read-only,
        /// False if the function can write or execute code,
        /// Null if the function's security level is unknown.
        /// </returns>
        public new bool? IsReadOnlyFunction(string functionName)
        {
            // First check if it's a custom tool
            if (_customTools.TryGetValue(functionName, out var tool))
            {
                // Check the tool's security tags
                return GetToolSecurityLevel(tool) == ToolSecurityLevel.Read;
            }
            
            // If it's not a custom tool, use the base implementation
            return base.IsReadOnlyFunction(functionName);
        }

        /// <summary>
        /// Gets the security level of a tool based on its tags.
        /// </summary>
        /// <param name="tool">The tool definition.</param>
        /// <returns>The tool's security level.</returns>
        public ToolSecurityLevel GetToolSecurityLevel(ToolDefinition tool)
        {
            // Check for the run tag first (highest privilege)
            if (tool.Tags.Contains("run"))
            {
                return ToolSecurityLevel.Run;
            }
            
            // Check for the write tag next
            if (tool.Tags.Contains("write"))
            {
                return ToolSecurityLevel.Write;
            }
            
            // Check for the read tag
            if (tool.Tags.Contains("read"))
            {
                return ToolSecurityLevel.Read;
            }
            
            // If no security tags are present, default to Run (most restricted) for safety
            return ToolSecurityLevel.Run;
        }

        /// <summary>
        /// Checks if a tool should be auto-approved based on its tags and the auto-approve settings.
        /// </summary>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="autoApproveList">The list of auto-approved function patterns.</param>
        /// <returns>True if the tool should be auto-approved, false otherwise.</returns>
        public bool ShouldAutoApproveTool(string toolName, IEnumerable<string> autoApproveList)
        {
            // Check if the tool name is directly in the auto-approve list
            if (autoApproveList.Contains(toolName))
            {
                return true;
            }
            
            // Check if all tools are auto-approved
            if (autoApproveList.Contains("*"))
            {
                return true;
            }
            
            // Check if the tool is in the custom tools dictionary
            if (_customTools.TryGetValue(toolName, out var tool))
            {
                // Check for security level approvals
                var securityLevel = GetToolSecurityLevel(tool);
                
                // Check if all run functions are approved (which also approves write and read)
                if (autoApproveList.Contains("run") && securityLevel == ToolSecurityLevel.Run)
                {
                    return true;
                }
                
                // Check if all write functions are approved (which also approves read)
                if (autoApproveList.Contains("write") && 
                    (securityLevel == ToolSecurityLevel.Write || securityLevel == ToolSecurityLevel.Read))
                {
                    return true;
                }
                
                // Check if all read functions are approved
                if (autoApproveList.Contains("read") && securityLevel == ToolSecurityLevel.Read)
                {
                    return true;
                }
                
                // Check if any of the tool's tags are in the auto-approve list
                return tool.Tags.Any(tag => autoApproveList.Contains(tag));
            }
            
            // Default to not auto-approving if we couldn't determine
            return false;
        }

        /// <summary>
        /// Checks if a tool should be auto-denied based on its tags and the auto-deny settings.
        /// </summary>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="autoDenyList">The list of auto-denied function patterns.</param>
        /// <returns>True if the tool should be auto-denied, false otherwise.</returns>
        public bool ShouldAutoDenyTool(string toolName, IEnumerable<string> autoDenyList)
        {
            // Check if the tool name is directly in the auto-deny list
            if (autoDenyList.Contains(toolName))
            {
                return true;
            }
            
            // Check if all tools are auto-denied
            if (autoDenyList.Contains("*"))
            {
                return true;
            }
            
            // Check if the tool is in the custom tools dictionary
            if (_customTools.TryGetValue(toolName, out var tool))
            {
                // Check for security level denials
                var securityLevel = GetToolSecurityLevel(tool);
                
                // Check if all run functions are denied
                if (autoDenyList.Contains("run") && securityLevel == ToolSecurityLevel.Run)
                {
                    return true;
                }
                
                // Check if all write functions are denied
                if (autoDenyList.Contains("write") && securityLevel == ToolSecurityLevel.Write)
                {
                    return true;
                }
                
                // Check if all read functions are denied
                if (autoDenyList.Contains("read") && securityLevel == ToolSecurityLevel.Read)
                {
                    return true;
                }
                
                // Check if any of the tool's tags are in the auto-deny list
                return tool.Tags.Any(tag => autoDenyList.Contains(tag));
            }
            
            // Default to not auto-denying if we couldn't determine
            return false;
        }
    }

    /// <summary>
    /// Defines the security levels for tools.
    /// </summary>
    public enum ToolSecurityLevel
    {
        /// <summary>
        /// Tool only reads data, doesn't modify anything.
        /// </summary>
        Read,
        
        /// <summary>
        /// Tool can modify files or data.
        /// </summary>
        Write,
        
        /// <summary>
        /// Tool can execute arbitrary code.
        /// </summary>
        Run
    }
