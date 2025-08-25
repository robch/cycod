using System.Collections.Generic;

/// <summary>
/// Referenced resources used by a tool.
/// </summary>
public class ToolUses
{
    /// <summary>
    /// Single config name.
    /// </summary>
    public string? Config { get; set; }

    /// <summary>
    /// Multiple config names.
    /// </summary>
    public List<string>? Configs { get; set; }

    /// <summary>
    /// Single profile name.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Multiple profile names.
    /// </summary>
    public List<string>? Profiles { get; set; }

    /// <summary>
    /// Single MCP name.
    /// </summary>
    public string? Mcp { get; set; }

    /// <summary>
    /// Multiple MCP names.
    /// </summary>
    public List<string>? Mcps { get; set; }

    /// <summary>
    /// Single tool name.
    /// </summary>
    public string? Tool { get; set; }

    /// <summary>
    /// Multiple tool names.
    /// </summary>
    public List<string>? Tools { get; set; }
}

/// <summary>
/// Inline resources used by a tool.
/// </summary>
public class ToolWith
{
    /// <summary>
    /// Inline config.
    /// </summary>
    public Dictionary<string, object>? Config { get; set; }

    /// <summary>
    /// Inline MCPs.
    /// </summary>
    public List<Dictionary<string, object>>? Mcps { get; set; }

    /// <summary>
    /// Inline tools.
    /// </summary>
    public List<ToolDefinition>? Tools { get; set; }
}