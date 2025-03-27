using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlConfigFile : ConfigFile
{
    public YamlConfigFile(string filePath) : base(filePath)
    {
    }

    public override Dictionary<string, object> Read()
    {
        var exists = FileHelpers.FileExists(FilePath);
        if (!exists) return new Dictionary<string, object>();

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yamlContent = FileHelpers.ReadAllText(FilePath);
            var result = deserializer.Deserialize<Dictionary<string, object>>(yamlContent) ?? new Dictionary<string, object>();
            
            return NormalizeNestedDictionaries(result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading YAML configuration file: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public override void Write(Dictionary<string, object> data)
    {
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yamlContent = serializer.Serialize(data);
            FileHelpers.WriteAllText(FilePath, yamlContent);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing YAML configuration file: {ex.Message}");
        }
    }

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