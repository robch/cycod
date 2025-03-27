using System;
using System.Collections.Generic;
using System.Linq;

public static class ConfigDisplayHelpers
{
    public static void DisplayConfigSettings(Dictionary<string, ConfigValue> config, int indentLevel = 2)
    {
        if (config.Count == 0)
        {
            Console.WriteLine($"{new string(' ', indentLevel)}No configuration settings found.");
            return;
        }

        // Sort the keys with non-dotted keys first, then dotted keys
        var sortedKeys = SortKeysWithNonDottedFirst(config.Keys);
        
        bool hasDisplayedNonDotted = false;
        bool hasDisplayedDotted = false;

        foreach (var key in sortedKeys)
        {
            // If we're switching from non-dotted to dotted keys, add a line break
            if (!hasDisplayedDotted && key.Contains('.'))
            {
                if (hasDisplayedNonDotted)
                {
                    Console.WriteLine();
                }
                hasDisplayedDotted = true;
            }
            
            if (!key.Contains('.'))
            {
                hasDisplayedNonDotted = true;
            }
            
            DisplayConfigValue(key, config[key], indentLevel);
        }
    }
    
    public static void DisplayConfigValue(string key, ConfigValue value, int indentLevel = 2)
    {
        string indent = new string(' ', indentLevel);
        
        // If it's a list type in memory (actual List objects)
        if (value.Value is List<object> || value.Value is List<string>)
        {
            var list = value.AsList();
            DisplayList(key, list, indentLevel);
            return;
        }
        
        // Default: display as simple key-value
        ConsoleHelpers.WriteLine(!value.IsNullOrEmpty()
            ? $"{indent}{key}: {value.Value}"
            : $"{indent}{key}: (not found)",
            overrideQuiet: true);
    }
    
    public static void DisplayList(string key, List<string> list, int indentLevel = 2)
    {
        string keyIndent = new string(' ', indentLevel);
        string valueIndent = new string(' ', indentLevel + 2);
        
        if (list.Count > 0)
        {
            ConsoleHelpers.WriteLine($"{keyIndent}{key}:", overrideQuiet: true);
            foreach (var item in list)
            {
                ConsoleHelpers.WriteLine($"{valueIndent}- {item}", overrideQuiet: true);
            }
        }
        else
        {
            ConsoleHelpers.WriteLine($"{keyIndent}{key}: (empty list)", overrideQuiet: true);
        }
    }

    private static List<string> SortKeysWithNonDottedFirst(IEnumerable<string> keys)
    {
        // Split keys into two groups: non-dotted and dotted
        var nonDottedKeys = new List<string>();
        var dottedKeys = new List<string>();
        
        foreach (var key in keys)
        {
            if (key.Contains('.'))
            {
                dottedKeys.Add(key);
            }
            else
            {
                nonDottedKeys.Add(key);
            }
        }
        
        // Sort each group alphabetically
        nonDottedKeys.Sort(StringComparer.OrdinalIgnoreCase);
        dottedKeys.Sort(StringComparer.OrdinalIgnoreCase);
        
        // Combine the groups with non-dotted keys first
        var sortedKeys = new List<string>();
        sortedKeys.AddRange(nonDottedKeys);
        sortedKeys.AddRange(dottedKeys);
        
        return sortedKeys;
    }
}