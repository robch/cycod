using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Helper class for finding AGENTS.md files and other agent instruction files.
/// </summary>
public static class AgentsFileHelpers
{
    /// <summary>
    /// Default file names to search for (in order of priority)
    /// </summary>
    private static readonly string[] DefaultAgentFileNames = new[] 
    { 
        "AGENTS.md", 
        "CLAUDE.md", 
        "GEMINI.md", 
        ".cursorrules",
        "AGENT.md",
        "agent.md",
        ".windsurfrules"
    };

    /// <summary>
    /// Finds an agent file in the current directory or parent directories.
    /// </summary>
    /// <param name="fileNames">List of file names to search for (in order of priority)</param>
    /// <param name="searchParents">Whether to search in parent directories</param>
    /// <returns>Path to the agent file if found, null otherwise</returns>
    public static string? FindAgentsFile(string[]? fileNames = null, bool searchParents = true)
    {
        fileNames ??= DefaultAgentFileNames;
        
        var agentsFile = FileHelpers.FindFirstExistingFileFromNames(fileNames, searchParents);
        
        ConsoleHelpers.WriteDebugLine(agentsFile != null 
            ? $"Found agents file: {agentsFile}" 
            : $"No agents file found.");
            
        return agentsFile;
    }
}