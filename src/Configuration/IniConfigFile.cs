using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Handles reading and writing INI-style configuration files.
/// </summary>
public class IniConfigFile : ConfigFile
{
    /// <summary>
    /// Initializes a new instance of the IniConfigFile class.
    /// </summary>
    /// <param name="filePath">The path to the INI configuration file.</param>
    public IniConfigFile(string filePath) : base(filePath)
    {
    }

    /// <summary>
    /// Reads the configuration from the INI file.
    /// </summary>
    /// <returns>A dictionary containing the configuration data.</returns>
    public override Dictionary<string, object> Read()
    {
        var result = new Dictionary<string, object>();

        if (!Exists())
        {
            return result;
        }

        try
        {
            string[] lines = File.ReadAllLines(FilePath);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";"))
                {
                    continue;
                }

                // Parse KEY=VALUE format
                int equalsPos = trimmedLine.IndexOf('=');
                if (equalsPos > 0)
                {
                    string key = trimmedLine.Substring(0, equalsPos).Trim();
                    string value = trimmedLine.Substring(equalsPos + 1).Trim();

                    // Convert from flat format to hierarchical format
                    string dotNotationKey = ConfigPathHelpers.FromEnvVar(key);
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

    /// <summary>
    /// Writes the configuration to the INI file.
    /// </summary>
    /// <param name="data">The configuration data to write.</param>
    public override void Write(Dictionary<string, object> data)
    {
        try
        {
            EnsureDirectoryExists();

            var flatData = FlattenDictionary(data);
            var lines = new StringBuilder();

            foreach (var pair in flatData)
            {
                string key = ConfigPathHelpers.ToEnvVar(pair.Key);
                string value = pair.Value?.ToString() ?? string.Empty;
                lines.AppendLine($"{key}={value}");
            }

            File.WriteAllText(FilePath, lines.ToString());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing INI configuration file: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets a nested value in a dictionary structure.
    /// </summary>
    /// <param name="data">The root dictionary.</param>
    /// <param name="keyParts">The key parts representing the path to the value.</param>
    /// <param name="value">The value to set.</param>
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

    /// <summary>
    /// Flattens a hierarchical dictionary to a flat dictionary with dot notation keys.
    /// </summary>
    /// <param name="data">The hierarchical dictionary to flatten.</param>
    /// <param name="prefix">The prefix to use for the flattened keys.</param>
    /// <returns>A flat dictionary.</returns>
    private Dictionary<string, object> FlattenDictionary(Dictionary<string, object> data, string prefix = "")
    {
        var result = new Dictionary<string, object>();

        foreach (var pair in data)
        {
            string key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

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