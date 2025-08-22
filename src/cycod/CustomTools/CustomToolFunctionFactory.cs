using CycoDev.CustomTools.Models;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace CycoDev.CustomTools
{
    /// <summary>
    /// Factory for creating AITool functions from custom tools.
    /// </summary>
    public class CustomToolFunctionFactory : FunctionFactory
    {
        private readonly CustomToolFactory _toolFactory;
        private readonly CustomToolExecutor _toolExecutor;
        private readonly Dictionary<string, CustomToolDefinition> _registeredTools = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomToolFunctionFactory"/> class.
        /// </summary>
        /// <param name="toolFactory">The custom tool factory.</param>
        public CustomToolFunctionFactory(CustomToolFactory toolFactory)
        {
            _toolFactory = toolFactory;
            _toolExecutor = new CustomToolExecutor();
        }

        /// <summary>
        /// Loads all custom tools and creates functions for them.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadCustomToolsAsync()
        {
            await _toolFactory.LoadAllToolsAsync();
            RegisterCustomToolFunctions();
        }

        /// <summary>
        /// Loads custom tools from a specific scope and creates functions for them.
        /// </summary>
        /// <param name="scope">The scope to load tools from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadCustomToolsFromScopeAsync(ToolScope scope)
        {
            await _toolFactory.LoadToolsFromScopeAsync(scope);
            RegisterCustomToolFunctions();
        }

        /// <summary>
        /// Registers all custom tools as functions.
        /// </summary>
        private void RegisterCustomToolFunctions()
        {
            foreach (var tool in _toolFactory.GetAllTools())
            {
                RegisterCustomToolFunction(tool);
            }
        }

        /// <summary>
        /// Registers a custom tool as a function.
        /// </summary>
        /// <param name="tool">The custom tool to register.</param>
        private void RegisterCustomToolFunction(CustomToolDefinition tool)
        {
            // Skip if this tool is already registered
            if (_registeredTools.ContainsKey(tool.Name))
            {
                return;
            }

            // Create AIFunction
            var methodInfo = GetType().GetMethod(nameof(ExecuteCustomTool), 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                
            var aiFunction = AIFunctionFactory.Create(
                methodInfo,
                tool.Name,
                tool.Description);
                
            // Add function to factory
            AddFunction(aiFunction);
            _registeredTools[tool.Name] = tool;

            ConsoleHelpers.WriteDebugLine($"Registered custom tool '{tool.Name}' as function");
        }

        /// <summary>
        /// Executes a custom tool.
        /// </summary>
        /// <param name="functionName">The name of the function/tool to execute.</param>
        /// <param name="args">The arguments for the function call as a JSON string.</param>
        /// <returns>The result of the tool execution.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private async Task<string> ExecuteCustomTool(string functionName, string args)
        {
            if (!_registeredTools.TryGetValue(functionName, out var tool))
            {
                return $"Error: Custom tool '{functionName}' not found";
            }

            try
            {
                var parameters = JsonSerializer.Deserialize<Dictionary<string, object?>>(args, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                parameters ??= new Dictionary<string, object?>();

                var result = await _toolExecutor.ExecuteToolAsync(tool, parameters);
                return result.IsError
                    ? $"Error: {result.Error}\n{result.Output}"
                    : result.Output;
            }
            catch (Exception ex)
            {
                return $"Error executing custom tool '{functionName}': {ex.Message}";
            }
        }

        /// <summary>
        /// Checks if a function corresponds to a custom tool.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>True if the function is a custom tool, false otherwise.</returns>
        public bool IsCustomTool(string functionName)
        {
            return _registeredTools.ContainsKey(functionName);
        }

        /// <summary>
        /// Executes a custom tool.
        /// </summary>
        /// <param name="functionName">The name of the function/tool to execute.</param>
        /// <param name="functionArguments">The arguments for the function call as a JSON string.</param>
        /// <param name="result">The result of the function call, if successful.</param>
        /// <returns>True if the function was found and called, false otherwise.</returns>
        public override bool TryCallFunction(string functionName, string functionArguments, out object? result)
        {
            result = null;

            if (!string.IsNullOrEmpty(functionName) && IsCustomTool(functionName))
            {
                var task = ExecuteCustomTool(functionName, functionArguments);
                task.Wait();
                result = task.Result;
                return true;
            }

            return base.TryCallFunction(functionName, functionArguments, out result);
        }
    }
}