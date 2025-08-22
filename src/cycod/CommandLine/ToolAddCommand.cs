using CycoDev.CustomTools;
using CycoDev.CustomTools.Models;

namespace CycoDev.CommandLine
{
    /// <summary>
    /// Command for adding a tool.
    /// </summary>
    public class ToolAddCommand : ToolBaseCommand
    {
        /// <summary>
        /// Gets or sets the name of the tool to add.
        /// </summary>
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tool description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the bash command.
        /// </summary>
        public string? BashCommand { get; set; }

        /// <summary>
        /// Gets or sets the cmd command.
        /// </summary>
        public string? CmdCommand { get; set; }

        /// <summary>
        /// Gets or sets the PowerShell command.
        /// </summary>
        public string? PowerShellCommand { get; set; }

        /// <summary>
        /// Gets or sets the run command.
        /// </summary>
        public string? RunCommand { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        public string? Script { get; set; }

        /// <summary>
        /// Gets or sets the shell.
        /// </summary>
        public string? Shell { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, CustomToolParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Gets or sets the platforms.
        /// </summary>
        public List<string> Platforms { get; set; } = new();

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolAddCommand"/> class.
        /// </summary>
        public ToolAddCommand() : base()
        {
            CommandName = "tool add";
            HelpText = "Create a new tool";
            Local = true; // Default to adding to local scope
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

            if (string.IsNullOrEmpty(Description))
            {
                Console.WriteLine("Error: Tool description is required.");
                return 1;
            }

            var toolFactory = new CustomToolFactory();
            var scope = GetScope();

            // Create the tool definition
            var tool = new CustomToolDefinition
            {
                Name = ToolName,
                Description = Description ?? "No description provided",
                BashCommand = BashCommand,
                CmdCommand = CmdCommand,
                PowerShellCommand = PowerShellCommand,
                RunCommand = RunCommand,
                Script = Script,
                Shell = Shell,
                Parameters = Parameters,
                Tags = Tags,
                Platforms = Platforms,
                Timeout = Timeout,
                WorkingDirectory = WorkingDirectory
            };

            // Validate the tool
            if (!tool.Validate(out var validationError))
            {
                Console.WriteLine($"Error: {validationError}");
                return 1;
            }

            // Add the tool
            toolFactory.AddTool(tool);
            
            // Save the tool
            await toolFactory.SaveToolAsync(tool, scope);
            
            Console.WriteLine($"Tool '{ToolName}' added successfully to scope: {scope}");
            return 0;
            {
                Console.WriteLine($"Failed to add tool '{ToolName}' to scope: {scope}");
                return 1;
            }
        }
    }
}