using Microsoft.Extensions.AI;

namespace CycoDev.CustomTools
{
    /// <summary>
    /// Extension methods for McpFunctionFactory to add custom tool functions.
    /// </summary>
    public static class McpFunctionFactoryExtensions
    {
        /// <summary>
        /// Adds custom tool functions to the function factory.
        /// </summary>
        /// <param name="factory">The function factory to add tools to.</param>
        /// <returns>A CustomToolFunctionFactory instance.</returns>
        public static async Task<CustomToolFunctionFactory> AddCustomToolFunctionsAsync(this McpFunctionFactory factory)
        {
            var toolFactory = new CustomToolFactory();
            var customToolFactory = new CustomToolFunctionFactory(toolFactory);
            
            // Load all custom tools
            await customToolFactory.LoadCustomToolsAsync();
            
            // Register the custom tool factory with the McpFunctionFactory
            return customToolFactory;
        }
    }
}