using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

/// <summary>
/// Represents a step in a multi-step tool.
/// </summary>
public class ToolStep
{
    /// <summary>
    /// Name of the step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Command to run.
    /// </summary>
    public string? Run { get; set; }

    /// <summary>
    /// CMD script.
    /// </summary>
    public string? Cmd { get; set; }

    /// <summary>
    /// Bash script.
    /// </summary>
    public string? Bash { get; set; }

    /// <summary>
    /// PowerShell script.
    /// </summary>
    public string? Pwsh { get; set; }

    /// <summary>
    /// Python script.
    /// </summary>
    public string? Python { get; set; }

    /// <summary>
    /// Generic script.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Shell to use with script.
    /// </summary>
    public string? Shell { get; set; }

    /// <summary>
    /// Tool to use.
    /// </summary>
    public string? Tool { get; set; }

    /// <summary>
    /// Arguments for the command.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new();

    /// <summary>
    /// Input data to pass via stdin.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// Timeout in milliseconds.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// Working directory.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Environment variables.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// Output from the step (populated during execution).
    /// </summary>
    [YamlIgnore]
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Standard output from the step (populated during execution).
    /// </summary>
    [YamlIgnore]
    public string Stdout { get; set; } = string.Empty;

    /// <summary>
    /// Standard error from the step (populated during execution).
    /// </summary>
    [YamlIgnore]
    public string Stderr { get; set; } = string.Empty;

    /// <summary>
    /// Exit code from the step (populated during execution).
    /// </summary>
    [YamlIgnore]
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets the effective command type.
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
        return "unknown";
    }

    /// <summary>
    /// Gets the effective command content.
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
            _ => string.Empty
        };
    }

    /// <summary>
    /// Validates the step definition.
    /// </summary>
    public bool Validate(out string error)
    {
        var commandOptions = new List<string?> { Run, Cmd, Bash, Pwsh, Python, Script, Tool };
        bool hasCommandOption = commandOptions.Any(opt => !string.IsNullOrWhiteSpace(opt));

        if (!hasCommandOption)
        {
            error = "Step must specify a command option (run, cmd, bash, pwsh, python, script, tool).";
            return false;
        }

        // Check for conflicting command types
        if (commandOptions.Count(opt => !string.IsNullOrWhiteSpace(opt)) > 1)
        {
            error = "Step has multiple command options specified. Only one command type is allowed per step.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}