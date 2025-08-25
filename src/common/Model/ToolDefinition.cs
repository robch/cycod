using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Represents a CYCOD custom tool definition.
/// </summary>
public class ToolDefinition
{
    /// <summary>
    /// The name of the tool.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the tool.
    /// </summary>
    public string Description { get; set; } = string.Empty;

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
    /// Referenced tools, configs, and MCPs.
    /// </summary>
    public ToolUses? Uses { get; set; }

    /// <summary>
    /// Inline tools, configs, and MCPs.
    /// </summary>
    public ToolWith? With { get; set; }

    /// <summary>
    /// Command to run for a single-step tool.
    /// </summary>
    public string? Run { get; set; }

    /// <summary>
    /// Arguments for the run command.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new();

    /// <summary>
    /// Input data to pass via stdin.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// CMD script for a single-step tool.
    /// </summary>
    public string? Cmd { get; set; }

    /// <summary>
    /// Bash script for a single-step tool.
    /// </summary>
    public string? Bash { get; set; }

    /// <summary>
    /// PowerShell script for a single-step tool.
    /// </summary>
    public string? Pwsh { get; set; }

    /// <summary>
    /// Python script for a single-step tool.
    /// </summary>
    public string? Python { get; set; }

    /// <summary>
    /// Generic script for a single-step tool.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Shell to use with script.
    /// </summary>
    public string? Shell { get; set; } = "default";

    /// <summary>
    /// MCP to use for a single-step tool.
    /// </summary>
    public string? Mcp { get; set; }

    /// <summary>
    /// Tool to use for a single-step tool.
    /// </summary>
    public string? Tool { get; set; }

    /// <summary>
    /// Steps for a multi-step tool.
    /// </summary>
    public List<ToolStep> Steps { get; set; } = new();

    /// <summary>
    /// Default timeout in milliseconds.
    /// </summary>
    public int Timeout { get; set; } = 60000;

    /// <summary>
    /// Working directory for the tool.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Environment variables for the tool.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// Source file path of the tool definition.
    /// </summary>
    [YamlIgnore]
    public string? SourceFilePath { get; set; }

    /// <summary>
    /// Scope of the tool definition.
    /// </summary>
    [YamlIgnore]
    public ConfigFileScope Scope { get; set; } = ConfigFileScope.Local;

    /// <summary>
    /// Validates the tool definition.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        // Validate name
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Tool name is required.");
        }

        // Validate at least one command option is specified
        var commandOptions = new List<string?> { Run, Cmd, Bash, Pwsh, Python, Script, Tool, Mcp };
        bool hasCommandOption = commandOptions.Any(opt => !string.IsNullOrWhiteSpace(opt));
        bool hasSteps = Steps.Count > 0;

        if (!hasCommandOption && !hasSteps)
        {
            errors.Add("Tool must specify either a command option (run, cmd, bash, pwsh, python, script, tool, mcp) or steps.");
        }

        // Validate parameters
        foreach (var param in Parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Key))
            {
                errors.Add("Parameter name cannot be empty.");
                continue;
            }

            if (!param.Value.Validate(out var paramError))
            {
                errors.Add($"Parameter '{param.Key}': {paramError}");
            }
        }

        // Validate steps
        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var stepName = string.IsNullOrWhiteSpace(step.Name) ? $"step{i+1}" : step.Name;

            if (!step.Validate(out var stepError))
            {
                errors.Add($"Step '{stepName}': {stepError}");
            }
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Determines if this is a multi-step tool.
    /// </summary>
    public bool IsMultiStep()
    {
        return Steps.Count > 0;
    }

    /// <summary>
    /// Gets the effective command type for a single-step tool.
    /// </summary>
    public string GetEffectiveCommandType()
    {
        if (!string.IsNullOrWhiteSpace(Run)) return "run";
        if (!string.IsNullOrWhiteSpace(Bash)) return "bash";
        if (!string.IsNullOrWhiteSpace(Cmd)) return "cmd";
        if (!string.IsNullOrWhiteSpace(Pwsh)) return "pwsh";
        if (!string.IsNullOrWhiteSpace(Python)) return "python";
        if (!string.IsNullOrWhiteSpace(Script)) return "script";
        if (!string.IsNullOrWhiteSpace(Tool)) return "tool";
        if (!string.IsNullOrWhiteSpace(Mcp)) return "mcp";
        return "unknown";
    }

    /// <summary>
    /// Gets the effective command content for a single-step tool.
    /// </summary>
    public string GetEffectiveCommandContent()
    {
        var commandType = GetEffectiveCommandType();
        return commandType switch
        {
            "run" => Run ?? string.Empty,
            "bash" => Bash ?? string.Empty,
            "cmd" => Cmd ?? string.Empty,
            "pwsh" => Pwsh ?? string.Empty,
            "python" => Python ?? string.Empty,
            "script" => Script ?? string.Empty,
            "tool" => Tool ?? string.Empty,
            "mcp" => Mcp ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Creates a new tool definition from YAML content.
    /// </summary>
    public static ToolDefinition? FromYaml(string yamlContent, string? filePath = null, ConfigFileScope scope = ConfigFileScope.Local)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            var tool = deserializer.Deserialize<ToolDefinition>(yamlContent);
            if (tool != null)
            {
                tool.SourceFilePath = filePath;
                tool.Scope = scope;

                // If name is not specified and filename is available, use the filename as the name
                if (string.IsNullOrEmpty(tool.Name) && !string.IsNullOrEmpty(filePath))
                {
                    tool.Name = Path.GetFileNameWithoutExtension(filePath);
                }
            }
            return tool;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error parsing tool definition: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts the tool definition to YAML.
    /// </summary>
    public string ToYaml()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

        return serializer.Serialize(this);
    }
}