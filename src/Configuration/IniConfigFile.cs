using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Handles reading and writing INI-style configuration files.
/// </summary>
public class IniConfigFile : ConfigFile
{
    public IniConfigFile(string filePath) : base(filePath)
    {
    }

    public override Dictionary<string, object> Read()
    {
        var result = new Dictionary<string, object>();

        var exists = FileHelpers.FileExists(FilePath);
        if (!exists) return result;

        try
        {
            var lines = File.ReadAllLines(FilePath);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";"))
                {
                    continue;
                }

                // Parse KEY=VALUE format
                int equalsPos = trimmedLine.IndexOf('=');
                if (equalsPos > 0)
                {
                    var key = trimmedLine.Substring(0, equalsPos).Trim();
                    var value = trimmedLine.Substring(equalsPos + 1).Trim();

                    // Convert from flat format to hierarchical format
                    var dotNotationKey = ConfigPathHelpers.FromEnvVar(key);
                    SetNestedValue(result, dotNotationKey.Split('.'), value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading INI configuration file: {ex.Message}");
        }

        return result;
    }

    public override void Write(Dictionary<string, object> data)
    {
        try
        {
            var flatData = FlattenDictionary(data);
            var lines = new StringBuilder();

            foreach (var pair in flatData)
            {
                var key = ConfigPathHelpers.ToEnvVar(pair.Key);
                var value = pair.Value?.ToString() ?? string.Empty;
                lines.AppendLine($"{key}={value}");
            }

            FileHelpers.WriteAllText(FilePath, lines.ToString());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing INI configuration file: {ex.Message}");
        }
    }

    private void SetNestedValue(Dictionary<string, object> data, string[] keyParts, object value)
    {
        if (keyParts.Length == 1)
        {
            data[keyParts[0]] = value;
            return;
        }

        if (!data.ContainsKey(keyParts[0]))
        {
            data[keyParts[0]] = new Dictionary<string, object>();
        }

        if (data[keyParts[0]] is Dictionary<string, object> nestedDict)
        {
            SetNestedValue(nestedDict, keyParts[1..], value);
        }
        else
        {
            // Replace non-dictionary value with a dictionary
            var newDict = new Dictionary<string, object>();
            data[keyParts[0]] = newDict;
            SetNestedValue(newDict, keyParts[1..], value);
        }
    }

    private Dictionary<string, object> FlattenDictionary(Dictionary<string, object> data, string prefix = "")
    {
        var result = new Dictionary<string, object>();

        foreach (var pair in data)
        {
            var key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

            if (pair.Value is Dictionary<string, object> nestedDict)
            {
                // Recursively flatten nested dictionaries
                foreach (var nestedPair in FlattenDictionary(nestedDict, key))
                {
                    result[nestedPair.Key] = nestedPair.Value;
                }
            }
            else if (pair.Value is List<string> stringList)
            {
                // Handle list values
                // In INI format, we'll just concatenate list items with a separator
                // This is a simplification - in a real implementation, you might want to handle this differently
                result[key] = string.Join(",", stringList);
            }
            else
            {
                result[key] = pair.Value;
            }
        }

        return result;
    }
}