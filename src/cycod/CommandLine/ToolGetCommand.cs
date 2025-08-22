using CycoDev.CustomTools;

namespace CycoDev.CommandLine
{
    /// <summary>
    /// Command for getting a tool.
    /// </summary>
    public class ToolGetCommand : ToolBaseCommand
    {
        /// <summary>
        /// Gets or sets the name of the tool to get.
        /// </summary>
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolGetCommand"/> class.
        /// </summary>
        public ToolGetCommand() : base()
        {
            CommandName = "tool get";
            HelpText = "Display the details of a specific tool";
            Any = true; // Default to getting from any scope
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
            await toolFactory.LoadAllToolsAsync();
            var tool = toolFactory.GetTool(ToolName);

            if (tool == null)
            {
                Console.WriteLine($"Tool '{ToolName}' not found.");
                return 1;
            }

            Console.WriteLine($"Name: {tool.Name}");
            Console.WriteLine($"Description: {tool.Description}");
            
            if (!string.IsNullOrEmpty(tool.Version))
            {
                Console.WriteLine($"Version: {tool.Version}");
            }

            if (tool.Tags.Count > 0)
            {
                Console.WriteLine($"Tags: {string.Join(", ", tool.Tags)}");
            }

            if (tool.Platforms.Count > 0)
            {
                Console.WriteLine($"Platforms: {string.Join(", ", tool.Platforms)}");
            }

            Console.WriteLine();
            Console.WriteLine("Command:");
            
            if (!string.IsNullOrEmpty(tool.BashCommand))
            {
                Console.WriteLine($"  Bash: {tool.BashCommand}");
            }
            else if (!string.IsNullOrEmpty(tool.CmdCommand))
            {
                Console.WriteLine($"  CMD: {tool.CmdCommand}");
            }
            else if (!string.IsNullOrEmpty(tool.PowerShellCommand))
            {
                Console.WriteLine($"  PowerShell: {tool.PowerShellCommand}");
            }
            else if (!string.IsNullOrEmpty(tool.RunCommand))
            {
                Console.WriteLine($"  Run: {tool.RunCommand}");
            }
            else if (!string.IsNullOrEmpty(tool.Script))
            {
                Console.WriteLine($"  Script ({tool.Shell}):");
                Console.WriteLine($"  {tool.Script.Replace("\n", "\n  ")}");
            }
            else if (tool.Steps.Count > 0)
            {
                Console.WriteLine("  Multi-step tool:");
                foreach (var step in tool.Steps)
                {
                    Console.WriteLine($"  - {step.Name}:");
                    
                    if (!string.IsNullOrEmpty(step.BashCommand))
                    {
                        Console.WriteLine($"      Bash: {step.BashCommand}");
                    }
                    else if (!string.IsNullOrEmpty(step.CmdCommand))
                    {
                        Console.WriteLine($"      CMD: {step.CmdCommand}");
                    }
                    else if (!string.IsNullOrEmpty(step.PowerShellCommand))
                    {
                        Console.WriteLine($"      PowerShell: {step.PowerShellCommand}");
                    }
                    else if (!string.IsNullOrEmpty(step.RunCommand))
                    {
                        Console.WriteLine($"      Run: {step.RunCommand}");
                    }
                    
                    if (step.ContinueOnError)
                    {
                        Console.WriteLine($"      Continue on error: {step.ContinueOnError}");
                    }
                    
                    if (!string.IsNullOrEmpty(step.RunCondition))
                    {
                        Console.WriteLine($"      Run condition: {step.RunCondition}");
                    }
                }
            }

            if (tool.Parameters.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Parameters:");
                
                foreach (var param in tool.Parameters)
                {
                    Console.Write($"  {param.Key} ({param.Value.Type})");
                    
                    if (param.Value.Required)
                    {
                        Console.Write(" [Required]");
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine($"    Description: {param.Value.Description}");
                    
                    if (param.Value.Default != null)
                    {
                        Console.WriteLine($"    Default: {param.Value.Default}");
                    }
                    
                    if (param.Value.Validation != null)
                    {
                        Console.WriteLine($"    Validation: {param.Value.Validation}");
                    }
                    
                    if (!string.IsNullOrEmpty(param.Value.Transform))
                    {
                        Console.WriteLine($"    Transform: {param.Value.Transform}");
                    }
                    
                    Console.WriteLine();
                }
            }

            if (tool.Timeout > 0)
            {
                Console.WriteLine($"Timeout: {tool.Timeout} ms");
            }

            if (!string.IsNullOrEmpty(tool.WorkingDirectory))
            {
                Console.WriteLine($"Working directory: {tool.WorkingDirectory}");
            }

            return 0;
        }
    }
}