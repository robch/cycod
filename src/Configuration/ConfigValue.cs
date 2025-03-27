using System;
using System.Collections.Generic;
using System.Linq;

public class ConfigValue
{
    public ConfigValue()
    {
        _value = null;
        Source = ConfigSource.NotFound;
        IsSecret = false;
    }

    public ConfigValue(object? value)
    {
        _value = value;
        Source = ConfigSource.Default;
        IsSecret = false;
    }
    
    public ConfigValue(object? value, ConfigSource source)
    {
        _value = value;
        Source = source;
        IsSecret = false;
    }
    
    public ConfigValue(object? value, ConfigSource source, bool isSecret)
    {
        _value = value;
        Source = source;
        IsSecret = isSecret;
    }

    public object? Value => _value;
    
    /// <summary>
    /// Gets the source of this configuration value.
    /// </summary>
    public ConfigSource Source { get; }
    
    /// <summary>
    /// Gets whether this value should be treated as a secret.
    /// </summary>
    public bool IsSecret { get; }

    public string? AsString()
    {
        return _value?.ToString();
    }
    
    /// <summary>
    /// Returns an obfuscated version of the value if it's a secret.
    /// </summary>
    /// <returns>The obfuscated value if it's a secret, otherwise the normal string representation.</returns>
    public string? AsObfuscated()
    {
        if (!IsSecret || _value == null)
        {
            return AsString();
        }
        
        string valueStr = _value.ToString() ?? string.Empty;
        if (valueStr.Length <= 4)
        {
            return "****";
        }
        
        // Show first 2 and last 2 characters, obfuscate the rest
        return valueStr.Substring(0, 2) + new string('*', valueStr.Length - 4) + valueStr.Substring(valueStr.Length - 2);
    }

    public int AsInt(int defaultValue = 0)
    {
        if (_value is int intValue)
        {
            return intValue;
        }

        if (_value is string stringValue && int.TryParse(stringValue, out var parsedInt))
        {
            return parsedInt;
        }

        return defaultValue;
    }

    public bool AsBool(bool defaultValue = false)
    {
        if (_value is bool boolValue)
        {
            return boolValue;
        }

        if (_value is string stringValue)
        {
            if (bool.TryParse(stringValue, out var parsedBool))
            {
                return parsedBool;
            }

            return stringValue.ToLower() == "true" || stringValue == "1";
        }

        if (_value is int intValue)
        {
            return intValue != 0;
        }

        return defaultValue;
    }

    public List<string> AsList()
    {
        if (_value is List<object> objList)
        {
            return objList.Select(o => o?.ToString() ?? string.Empty).ToList();
        }

        if (_value is List<string> stringList)
        {
            return stringList;
        }

        if (_value is string singleString)
        {
            return new List<string> { singleString };
        }

        return new List<string>();
    }

    public bool IsNullOrEmpty()
    {
        if (_value == null)
        {
            return true;
        }

        if (_value is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue);
        }

        if (_value is List<object> objList)
        {
            return objList.Count == 0;
        }

        if (_value is List<string> stringList)
        {
            return stringList.Count == 0;
        }

        return false;
    }

    public bool AddToList(string value)
    {
        var list = AsList();
        if (!list.Contains(value))
        {
            list.Add(value);
            _value = list;
            return true;
        }

        return false;
    }

    public bool RemoveFromList(string value)
    {
        var list = AsList();
        var removed = list.Remove(value);
        if (removed)
        {
            _value = list;
        }

        return removed;
    }

    public void Clear()
    {
        _value = null;
    }

    public void Set(object? value)
    {
        _value = value;
    }

    public override string ToString()
    {
        if (_value == null)
        {
            return string.Empty;
        }

        if (_value is List<object> objList)
        {
            return string.Join(", ", objList.Select(o => o?.ToString() ?? string.Empty));
        }

        if (_value is List<string> stringList)
        {
            return string.Join(", ", stringList);
        }

        return _value.ToString() ?? string.Empty;
    }

    private object? _value;
}