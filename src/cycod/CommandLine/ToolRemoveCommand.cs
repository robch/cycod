using CycoDev.CustomTools;

namespace CycoDev.CommandLine
{
    /// <summary>
    /// Command for removing a tool.
    /// </summary>
    public class ToolRemoveCommand : ToolBaseCommand
    {
        /// <summary>
        /// Gets or sets the name of the tool to remove.
        /// </summary>
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolRemoveCommand"/> class.
        /// </summary>
        public ToolRemoveCommand() : base()
        {
            CommandName = "tool remove";
            HelpText = "Delete a tool";
            Any = true; // Default to removing from any scope
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<object> ExecuteAsync()
        {
            if (string.IsNullOrEmpty(ToolName))
            {
                Console.WriteLine("Error: Tool name is required.");
                return 1;
            }

            var toolFactory = new CustomToolFactory();
            var scope = GetScope(defaultToAny: true);
            var removed = toolFactory.RemoveTool(ToolName, scope);

            if (removed)
            {
                Console.WriteLine($"Tool '{ToolName}' removed successfully from scope: {scope}");
                return 0;
            }
            else
            {
                Console.WriteLine($"Tool '{ToolName}' not found in scope: {scope}");
                return 1;
            }
        }
    }
}