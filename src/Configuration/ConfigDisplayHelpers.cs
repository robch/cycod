using System;
using System.Collections.Generic;
using System.Linq;

public static class ConfigDisplayHelpers
{
    public static void DisplayConfigSettings(string? location, Dictionary<string, ConfigValue> config, int indentLevel = 2)
    {
        if (location != null)
        {
            Console.WriteLine($"LOCATION: {location}\n");
        }

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
    
    public static void DisplayConfigValue(string key, ConfigValue value, int indentLevel = 2, bool showLocation = false)
    {
        if (showLocation)
        {
            var location = value.Source switch
            {
                ConfigSource.CommandLine => "Command line (specified)",
                ConfigSource.EnvironmentVariable => "Environment variable (specified)",
                ConfigSource.ConfigFileName => $"{value.File?.FileName} (specified)",
                ConfigSource.LocalConfig => $"{value.File?.FileName} (local)",
                ConfigSource.UserConfig => $"{value.File?.FileName} (user)",
                ConfigSource.GlobalConfig => $"{value.File?.FileName} (global)",
                _ => null
            };

            showLocation = !string.IsNullOrEmpty(location);
            if (showLocation) Console.WriteLine($"LOCATION: {location}\n");
        }

        var indent = new string(' ', indentLevel);
        
        // If it's a list type in memory (actual List objects)
        if (value.Value is List<object> || value.Value is List<string>)
        {
            var list = value.AsList();
            DisplayList(key, list, indentLevel);
            return;
        }
        
        // Get value to display, obfuscating if it's a secret
        var displayValue = value.IsSecret
            ? value.AsObfuscated() ?? "(empty)"
            : !value.IsNotFoundNullOrEmpty()
                ? value.Value?.ToString() ?? "(null)"
                : "(not found or empty)";
                            
        ConsoleHelpers.WriteLine($"{indent}{key}: {displayValue}", overrideQuiet: true);
    }
    
    public static void DisplayList(string key, List<string> list, int indentLevel = 2)
    {
        var keyIndent = new string(' ', indentLevel);
        var valueIndent = new string(' ', indentLevel + 2);
        
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