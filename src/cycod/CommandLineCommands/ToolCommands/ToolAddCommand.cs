using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Command to add a new tool.
/// </summary>
class ToolAddCommand : ToolBaseCommand
{
    /// <summary>
    /// The name of the tool to add.
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// The description of the tool.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bash command/script to execute.
    /// </summary>
    public string? Bash { get; set; }

    /// <summary>
    /// CMD command/script to execute.
    /// </summary>
    public string? Cmd { get; set; }

    /// <summary>
    /// PowerShell command/script to execute.
    /// </summary>
    public string? Pwsh { get; set; }

    /// <summary>
    /// Python command/script to execute.
    /// </summary>
    public string? Python { get; set; }

    /// <summary>
    /// Direct command to execute.
    /// </summary>
    public string? Run { get; set; }

    /// <summary>
    /// Script content.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Shell to use with script.
    /// </summary>
    public string? Shell { get; set; }

    /// <summary>
    /// Working directory for the tool.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Timeout for the tool in milliseconds.
    /// </summary>
    public int Timeout { get; set; } = 60000;

    /// <summary>
    /// Arguments to pass to the command.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new();

    /// <summary>
    /// Input data to pass via stdin.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// Parameters for the tool.
    /// </summary>
    public Dictionary<string, ToolParameter> Parameters { get; set; } = new();

    /// <summary>
    /// Platforms the tool supports.
    /// </summary>
    public List<string> Platforms { get; set; } = new();

    /// <summary>
    /// Tags for categorization and security.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Steps for a multi-step tool.
    /// </summary>
    public List<ToolStep> Steps { get; set; } = new();

    /// <summary>
    /// Environment variables for the tool.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// Raw parameter definitions from command line.
    /// </summary>
    public List<string> ParameterDefinitions { get; set; } = new();

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolAddCommand() : base()
    {
        Scope = ConfigFileScope.Local;
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no tool name or description provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(ToolName);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "tool add";
    }

    /// <summary>
    /// Execute the add command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(ToolName))
            {
                ConsoleHelpers.WriteErrorLine("Error: Tool name is required.");
                return 1;
            }

            // Process parameter definitions
            foreach (var paramDef in ParameterDefinitions)
            {
                try 
                {
                    var (name, description, type, required, defaultValue) = ParseParameterDefinition(paramDef);
                    AddParameter(name, description, type, required, defaultValue);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteErrorLine($"Error parsing parameter definition: {ex.Message}");
                    return 1;
                }
            }

            // Check if tool already exists
            var existingToolPath = ToolFileHelpers.FindToolFile(ToolName, Scope ?? ConfigFileScope.Local);
            if (existingToolPath != null)
            {
                ConsoleHelpers.WriteWarningLine($"Warning: Tool '{ToolName}' already exists in {Scope.ToString().ToLower()} scope and will be overwritten.");
            }

            // Create tool definition
            var tool = new ToolDefinition
            {
                Name = ToolName,
                Description = Description,
                Bash = Bash,
                Cmd = Cmd,
                Pwsh = Pwsh,
                Python = Python,
                Run = Run,
                Script = Script,
                Shell = Shell,
                WorkingDirectory = WorkingDirectory,
                Timeout = Timeout,
                Arguments = Arguments,
                Input = Input,
                Parameters = Parameters,
                Platforms = Platforms,
                Tags = Tags,
                Steps = Steps,
                Environment = Environment
            };

            // Validate the tool
            if (!tool.Validate(out var errors))
            {
                ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetToolValidationError(ToolName, errors));
                return 1;
            }

            // Save the tool
            try
            {
                var filePath = ToolFileHelpers.SaveToolDefinition(tool, Scope ?? ConfigFileScope.Local);
                
                // Format output to be consistent with other CYCOD commands
                ConsoleHelpers.WriteLine($"Created tool '{ToolName}' in {Scope.ToString().ToLower()} scope.");
                ConsoleHelpers.WriteLine($"Saved: {filePath}");
                ConsoleHelpers.WriteLine();
                ConsoleHelpers.WriteLine($"USAGE: cycod --use-tool {ToolName} [additional arguments]");
                
                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetFileOperationError("create", ToolName, null, ex.Message));
                return 1;
            }
        });
    }

    /// <summary>
    /// Adds a parameter to the tool.
    /// </summary>
    public void AddParameter(string name, string description, string type = "string", bool required = false, object? defaultValue = null)
    {
        Parameters[name] = new ToolParameter
        {
            Description = description,
            Type = type,
            Required = required,
            Default = defaultValue
        };
    }

    /// <summary>
    /// Parse a parameter definition string into its components.
    /// Format: NAME "DESCRIPTION" type=string required=true default=VALUE
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition string to parse.</param>
    /// <returns>A tuple containing the parsed parameter components.</returns>
    private (string name, string description, string type, bool required, object? defaultValue) ParseParameterDefinition(string parameterDefinition)
    {
        if (string.IsNullOrWhiteSpace(parameterDefinition))
        {
            throw new ArgumentException("Parameter definition cannot be empty");
        }

        // Default values
        string name = string.Empty;
        string description = string.Empty;
        string type = "string";
        bool required = false;
        object? defaultValue = null;

        // Parse parameter name (required)
        var nameMatch = Regex.Match(parameterDefinition, @"^(\w+)");
        if (nameMatch.Success)
        {
            name = nameMatch.Groups[1].Value;
            parameterDefinition = parameterDefinition.Substring(nameMatch.Length).Trim();
        }
        else
        {
            throw new ArgumentException("Parameter definition must start with a valid parameter name");
        }

        // Parse description (quoted string, optional)
        var descriptionMatch = Regex.Match(parameterDefinition, @"^""([^""]*)""\s*");
        if (descriptionMatch.Success)
        {
            description = descriptionMatch.Groups[1].Value;
            parameterDefinition = parameterDefinition.Substring(descriptionMatch.Length).Trim();
        }

        // Parse type, required, and default properties
        var keyValueMatches = Regex.Matches(parameterDefinition, @"(\w+)=([^""\s]+|""[^""]*"")");
        
        foreach (Match match in keyValueMatches)
        {
            var key = match.Groups[1].Value.ToLower();
            var value = match.Groups[2].Value;
            
            // Remove quotes if present
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }
            
            switch (key)
            {
                case "type":
                    type = value;
                    break;
                case "required":
                    required = bool.Parse(value);
                    break;
                case "default":
                    defaultValue = value;
                    
                    // Convert default value to appropriate type
                    if (type == "number" && double.TryParse(value, out var numValue))
                    {
                        defaultValue = numValue;
                    }
                    else if (type == "boolean" && bool.TryParse(value, out var boolValue))
                    {
                        defaultValue = boolValue;
                    }
                    break;
            }
        }

        return (name, description, type, required, defaultValue);
    }

    /// <summary>
    /// Adds a step to the tool.
    /// </summary>
    public void AddStep(string name, Dictionary<string, string?> commandOptions)
    {
        var step = new ToolStep
        {
            Name = name
        };

        foreach (var option in commandOptions)
        {
            switch (option.Key.ToLower())
            {
                case "run":
                    step.Run = option.Value;
                    break;
                case "bash":
                    step.Bash = option.Value;
                    break;
                case "cmd":
                    step.Cmd = option.Value;
                    break;
                case "pwsh":
                    step.Pwsh = option.Value;
                    break;
                case "python":
                    step.Python = option.Value;
                    break;
                case "script":
                    step.Script = option.Value;
                    break;
                case "shell":
                    step.Shell = option.Value;
                    break;
                case "tool":
                    step.Tool = option.Value;
                    break;
            }
        }

        Steps.Add(step);
    }

    /// <summary>
    /// Adds an environment variable to the tool.
    /// </summary>
    public void AddEnvironmentVariable(string name, string value)
    {
        Environment[name] = value;
    }

    /// <summary>
    /// Adds a platform to the tool.
    /// </summary>
    public void AddPlatform(string platform)
    {
        if (!Platforms.Contains(platform))
        {
            Platforms.Add(platform);
        }
    }

    /// <summary>
    /// Adds a tag to the tool.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
        }
    }
}