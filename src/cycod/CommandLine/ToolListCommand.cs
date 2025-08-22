using CycoDev.CustomTools;

namespace CycoDev.CommandLine
{
    /// <summary>
    /// Command for listing tools.
    /// </summary>
    public class ToolListCommand : ToolBaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolListCommand"/> class.
        /// </summary>
        public ToolListCommand() : base()
        {
            CommandName = "tool list";
            HelpText = "List all available tools";
            Any = true; // Default to listing from all scopes
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<object> ExecuteAsync()
        {
            var toolFactory = new CustomToolFactory();
            var scope = GetScope(defaultToAny: true);
            var tools = toolFactory.GetToolsFromScope(scope);

            Console.WriteLine($"Custom tools from scope: {scope}");
            Console.WriteLine();

            if (tools.Count() == 0)
            {
                Console.WriteLine("No tools found.");
                return 0;
            }

            foreach (var tool in tools)
            {
                Console.WriteLine($"{tool.Name} - {tool.Description}");
            }

            return 0;
        }
    }
}