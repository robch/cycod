using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Handles reading and writing YAML configuration files.
/// </summary>
public class YamlConfigFile : ConfigFile
{
    /// <summary>
    /// Initializes a new instance of the YamlConfigFile class.
    /// </summary>
    /// <param name="filePath">The path to the YAML configuration file.</param>
    public YamlConfigFile(string filePath) : base(filePath)
    {
    }

    /// <summary>
    /// Reads the configuration from the YAML file.
    /// </summary>
    /// <returns>A dictionary containing the configuration data.</returns>
    public override Dictionary<string, object> Read()
    {
        if (!Exists())
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string yamlContent = File.ReadAllText(FilePath);
            var result = deserializer.Deserialize<Dictionary<string, object>>(yamlContent) ?? new Dictionary<string, object>();
            
            // Convert nested dictionaries to proper format (YamlDotNet gives us Dictionary<object, object>)
            return NormalizeNestedDictionaries(result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading YAML configuration file: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Writes the configuration to the YAML file.
    /// </summary>
    /// <param name="data">The configuration data to write.</param>
    public override void Write(Dictionary<string, object> data)
    {
        try
        {
            EnsureDirectoryExists();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string yamlContent = serializer.Serialize(data);
            File.WriteAllText(FilePath, yamlContent);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing YAML configuration file: {ex.Message}");
        }
    }

    /// <summary>
    /// Normalizes nested dictionaries to ensure consistent type usage.
    /// </summary>
    /// <param name="data">The dictionary to normalize.</param>
    /// <returns>A normalized dictionary.</returns>
    private Dictionary<string, object> NormalizeNestedDictionaries(Dictionary<string, object> data)
    {
        var result = new Dictionary<string, object>();

        foreach (var pair in data)
        {
            if (pair.Value is Dictionary<object, object> nestedDict)
            {
                // Convert Dictionary<object, object> to Dictionary<string, object>
                var convertedDict = new Dictionary<string, object>();
                foreach (var nestedPair in nestedDict)
                {
                    var key = nestedPair.Key.ToString() ?? string.Empty;
                    if (nestedPair.Value is Dictionary<object, object> deepNestedDict)
                    {
                        // Handle deeply nested dictionaries recursively
                        convertedDict[key] = NormalizeNestedDictionaries(
                            ConvertToStringDictionary(deepNestedDict));
                    }
                    else
                    {
                        convertedDict[key] = nestedPair.Value ?? string.Empty;
                    }
                }
                result[pair.Key] = convertedDict;
            }
            else
            {
                result[pair.Key] = pair.Value ?? string.Empty;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a Dictionary<object, object> to Dictionary<string, object>.
    /// </summary>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>A converted dictionary.</returns>
    private Dictionary<string, object> ConvertToStringDictionary(Dictionary<object, object> dict)
    {
        var result = new Dictionary<string, object>();
        foreach (var pair in dict)
        {
            var key = pair.Key.ToString() ?? string.Empty;
            result[key] = pair.Value ?? string.Empty;
        }
        return result;
    }
}