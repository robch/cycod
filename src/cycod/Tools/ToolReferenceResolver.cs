using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Helper class for resolving tool references.
/// </summary>
public static class ToolReferenceResolver
{
    /// <summary>
    /// Resolves references to other tools in a tool definition.
    /// </summary>
    /// <param name="tool">The tool definition.</param>
    /// <param name="scope">The scope to look for referenced tools.</param>
    /// <returns>A dictionary of resolved tool references.</returns>
    public static Dictionary<string, ToolDefinition> ResolveToolReferences(ToolDefinition tool, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var resolvedTools = new Dictionary<string, ToolDefinition>();
        
        // Skip if no references
        if (tool.Uses == null) return resolvedTools;
        
        // Add single tool reference if specified
        if (!string.IsNullOrEmpty(tool.Uses.Tool))
        {
            AddResolvedTool(tool.Uses.Tool, resolvedTools, scope);
        }
        
        // Add multiple tool references if specified
        if (tool.Uses.Tools != null && tool.Uses.Tools.Count > 0)
        {
            foreach (var toolName in tool.Uses.Tools)
            {
                AddResolvedTool(toolName, resolvedTools, scope);
            }
        }
        
        return resolvedTools;
    }
    
    /// <summary>
    /// Resolves references to MCPs in a tool definition.
    /// </summary>
    /// <param name="tool">The tool definition.</param>
    /// <param name="scope">The scope to look for referenced MCPs.</param>
    /// <returns>A dictionary of resolved MCP configurations.</returns>
    public static Dictionary<string, IMcpServerConfigItem> ResolveMcpReferences(ToolDefinition tool, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var resolvedMcps = new Dictionary<string, IMcpServerConfigItem>();
        
        // Skip if no references
        if (tool.Uses == null) return resolvedMcps;
        
        // Add single MCP reference if specified
        if (!string.IsNullOrEmpty(tool.Uses.Mcp))
        {
            AddResolvedMcp(tool.Uses.Mcp, resolvedMcps, scope);
        }
        
        // Add multiple MCP references if specified
        if (tool.Uses.Mcps != null && tool.Uses.Mcps.Count > 0)
        {
            foreach (var mcpName in tool.Uses.Mcps)
            {
                AddResolvedMcp(mcpName, resolvedMcps, scope);
            }
        }
        
        return resolvedMcps;
    }
    
    /// <summary>
    /// Resolves references to configs in a tool definition.
    /// </summary>
    /// <param name="tool">The tool definition.</param>
    /// <param name="scope">The scope to look for referenced configs.</param>
    /// <returns>A dictionary of resolved config values.</returns>
    public static Dictionary<string, Dictionary<string, string>> ResolveConfigReferences(ToolDefinition tool, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var resolvedConfigs = new Dictionary<string, Dictionary<string, string>>();
        
        // Skip if no references
        if (tool.Uses == null) return resolvedConfigs;
        
        // Add single config reference if specified
        if (!string.IsNullOrEmpty(tool.Uses.Config))
        {
            AddResolvedConfig(tool.Uses.Config, resolvedConfigs, scope);
        }
        
        // Add multiple config references if specified
        if (tool.Uses.Configs != null && tool.Uses.Configs.Count > 0)
        {
            foreach (var configName in tool.Uses.Configs)
            {
                AddResolvedConfig(configName, resolvedConfigs, scope);
            }
        }
        
        return resolvedConfigs;
    }
    
    /// <summary>
    /// Resolves references to profiles in a tool definition.
    /// </summary>
    /// <param name="tool">The tool definition.</param>
    /// <param name="scope">The scope to look for referenced profiles.</param>
    /// <returns>A dictionary of resolved profile values.</returns>
    public static Dictionary<string, Dictionary<string, string>> ResolveProfileReferences(ToolDefinition tool, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var resolvedProfiles = new Dictionary<string, Dictionary<string, string>>();
        
        // Skip if no references
        if (tool.Uses == null) return resolvedProfiles;
        
        // Add single profile reference if specified
        if (!string.IsNullOrEmpty(tool.Uses.Profile))
        {
            AddResolvedProfile(tool.Uses.Profile, resolvedProfiles, scope);
        }
        
        // Add multiple profile references if specified
        if (tool.Uses.Profiles != null && tool.Uses.Profiles.Count > 0)
        {
            foreach (var profileName in tool.Uses.Profiles)
            {
                AddResolvedProfile(profileName, resolvedProfiles, scope);
            }
        }
        
        return resolvedProfiles;
    }
    
    /// <summary>
    /// Adds a resolved tool to the dictionary.
    /// </summary>
    private static void AddResolvedTool(string toolName, Dictionary<string, ToolDefinition> resolvedTools, ConfigFileScope scope)
    {
        // Skip if already resolved (prevents circular references)
        if (resolvedTools.ContainsKey(toolName)) return;
        
        // Load the tool definition
        var tool = ToolFileHelpers.LoadToolDefinition(toolName, scope);
        if (tool == null)
        {
            ConsoleHelpers.WriteErrorLine($"Could not find referenced tool '{toolName}'");
            return;
        }
        
        // Add to the resolved tools dictionary
        resolvedTools[toolName] = tool;
        
        // Recursively resolve references to other tools
        var nestedTools = ResolveToolReferences(tool, scope);
        foreach (var nestedTool in nestedTools)
        {
            if (!resolvedTools.ContainsKey(nestedTool.Key))
            {
                resolvedTools[nestedTool.Key] = nestedTool.Value;
            }
        }
    }
    
    /// <summary>
    /// Adds a resolved MCP to the dictionary.
    /// </summary>
    private static void AddResolvedMcp(string mcpName, Dictionary<string, IMcpServerConfigItem> resolvedMcps, ConfigFileScope scope)
    {
        // Skip if already resolved
        if (resolvedMcps.ContainsKey(mcpName)) return;
        
        // Load the MCP configuration
        var mcps = McpFileHelpers.ListMcpServers(scope);
        if (mcps.TryGetValue(mcpName, out var mcp))
        {
            resolvedMcps[mcpName] = mcp;
        }
        else
        {
            ConsoleHelpers.WriteErrorLine($"Could not find referenced MCP '{mcpName}'");
        }
    }
    
    /// <summary>
    /// Adds a resolved config to the dictionary.
    /// </summary>
    private static void AddResolvedConfig(string configName, Dictionary<string, Dictionary<string, string>> resolvedConfigs, ConfigFileScope scope)
    {
        // Skip if already resolved
        if (resolvedConfigs.ContainsKey(configName)) return;
        
        // Load the config - since we don't have direct access to config content,
        // we'll create a placeholder for now
        resolvedConfigs[configName] = new Dictionary<string, string>();
        
        // In a real implementation, we would load the config content:
        // 1. Find the config file
        var configPath = ConfigFileHelpers.FindConfigFile(scope);
        if (configPath != null)
        {
            ConsoleHelpers.WriteDebugLine($"Found config file at {configPath}");
            // Since we don't have direct access to load a specific config section,
            // we'll just indicate that we found it
            resolvedConfigs[configName]["found"] = "true";
        }
        else
        {
            ConsoleHelpers.WriteErrorLine($"Could not find referenced config '{configName}'");
            resolvedConfigs[configName]["found"] = "false";
        }
    }
    
    /// <summary>
    /// Adds a resolved profile to the dictionary.
    /// </summary>
    private static void AddResolvedProfile(string profileName, Dictionary<string, Dictionary<string, string>> resolvedProfiles, ConfigFileScope scope)
    {
        // Skip if already resolved
        if (resolvedProfiles.ContainsKey(profileName)) return;
        
        // Create a placeholder for the profile
        resolvedProfiles[profileName] = new Dictionary<string, string>();
        
        // In a real implementation, we would:
        // 1. Find the profile file
        var profilePath = ProfileFileHelpers.FindProfileFile(profileName);
        if (profilePath != null)
        {
            ConsoleHelpers.WriteDebugLine($"Found profile at {profilePath}");
            // Since we don't have direct access to load a profile without affecting the global config,
            // we'll just indicate that we found it
            resolvedProfiles[profileName]["name"] = profileName;
            resolvedProfiles[profileName]["found"] = "true";
        }
        else
        {
            ConsoleHelpers.WriteErrorLine($"Could not find referenced profile '{profileName}'");
            resolvedProfiles[profileName]["found"] = "false";
        }
    }
}