using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;

/// <summary>
/// Partial class containing the GetAITools implementation for CustomToolFunctionFactory.
/// </summary>
public partial class CustomToolFunctionFactory
{
    /// <summary>
    /// Gets all AITools including both base functions and custom tools.
    /// </summary>
    /// <returns>A collection of AITool objects including both base functions and custom tools.</returns>
    public override IEnumerable<AITool> GetAITools()
    {
        // Get tools from the base implementation
        var baseTools = base.GetAITools().ToList();
        
        // Get the names of tools already in the base collection
        var existingToolNames = baseTools
            .Select(t => t.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
            
        // Log what we found in the base factory
        ConsoleHelpers.WriteDebugLine($"CustomToolFunctionFactory.GetAITools: Found {baseTools.Count} tools from base factory");
        
        // Check if any of our custom tools are missing from the base tools
        var missingTools = _customTools
            .Where(t => !existingToolNames.Contains(t.Key))
            .ToList();
            
        // Create CustomToolFunction instances for any missing tools
        if (missingTools.Any())
        {
            ConsoleHelpers.WriteDebugLine($"CustomToolFunctionFactory.GetAITools: Found {missingTools.Count} custom tools not in base factory");
            
            // Create and add the missing tools
            var missingToolFunctions = missingTools
                .Select(t => new CustomToolFunction(t.Value))
                .Cast<AITool>();
                
            // Add the missing tools to our result
            baseTools.AddRange(missingToolFunctions);
        }
        else
        {
            ConsoleHelpers.WriteDebugLine("CustomToolFunctionFactory.GetAITools: All custom tools already in base factory");
        }
        
        // Log the final count
        ConsoleHelpers.WriteDebugLine($"CustomToolFunctionFactory.GetAITools: Returning {baseTools.Count} total tools");
        
        // Return the combined set of tools
        return baseTools;
    }
}