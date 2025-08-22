using CycoDev.CustomTools;
using CycoDev.CommandLine;

namespace CycoDev.CommandLineCommands
{
    /// <summary>
    /// Extension methods for the custom tools feature.
    /// </summary>
    public static class CustomToolExtensions
    {
        /*
        /// <summary>
        /// Adds custom tool functions to a function factory.
        /// </summary>
        /// <param name="factory">The function factory to extend.</param>
        /// <returns>A new function factory with custom tool functions.</returns>
        public static async Task<CustomToolFunctionFactory> AddCustomToolFunctionsAsync(this FunctionFactory factory)
        {
            var toolFactory = new CustomToolFactory();
            var customToolFactory = new CustomToolFunctionFactory(toolFactory);

            // Copy existing functions from the original factory
            foreach (var tool in factory.GetAITools())
            {
                customToolFactory.AddFunction(tool);
            }

            // Load and add custom tool functions
            await customToolFactory.LoadCustomToolsAsync();

            return customToolFactory;
        }

        /// <summary>
        /// Adds the custom tool commands to a root command.
        /// </summary>
        /// <param name="rootCommand">The root command to extend.</param>
        public static void AddCustomToolCommands(this Command rootCommand)
        {
            // Add tool commands to the root command
            var toolListCommand = new ToolListCommand();
            var toolGetCommand = new ToolGetCommand();
            var toolAddCommand = new ToolAddCommand();
            var toolRemoveCommand = new ToolRemoveCommand();

            rootCommand.AddChild(toolListCommand);
            rootCommand.AddChild(toolGetCommand);
            rootCommand.AddChild(toolAddCommand);
            rootCommand.AddChild(toolRemoveCommand);
        }
        */
    }
}