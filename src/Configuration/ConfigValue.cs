using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a configuration value which can be a string, number, boolean, or a list of values.
/// </summary>
public class ConfigValue
{
    private object? _value;

    /// <summary>
    /// Creates a new ConfigValue with a null value.
    /// </summary>
    public ConfigValue()
    {
        _value = null;
    }

    /// <summary>
    /// Creates a new ConfigValue with the specified value.
    /// </summary>
    /// <param name="value">The value to store in the ConfigValue.</param>
    public ConfigValue(object? value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the raw value stored in this ConfigValue.
    /// </summary>
    public object? RawValue => _value;

    /// <summary>
    /// Gets the value as a string.
    /// </summary>
    /// <returns>The string value, or null if the value is null.</returns>
    public string? AsString()
    {
        return _value?.ToString();
    }

    /// <summary>
    /// Gets the value as an integer.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the value is not an integer.</param>
    /// <returns>The integer value, or the default value if the conversion fails.</returns>
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

    /// <summary>
    /// Gets the value as a boolean.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the value is not a boolean.</param>
    /// <returns>The boolean value, or the default value if the conversion fails.</returns>
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

    /// <summary>
    /// Gets the value as a list of strings.
    /// </summary>
    /// <returns>The list of strings, or an empty list if the value is not a list.</returns>
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

    /// <summary>
    /// Checks if the value is null or empty.
    /// </summary>
    /// <returns>True if the value is null or empty, false otherwise.</returns>
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

    /// <summary>
    /// Adds a value to the list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the value was added, false otherwise.</returns>
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

    /// <summary>
    /// Removes a value from the list.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the value was removed, false otherwise.</returns>
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

    /// <summary>
    /// Clears the value.
    /// </summary>
    public void Clear()
    {
        _value = null;
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void Set(object? value)
    {
        _value = value;
    }

    /// <summary>
    /// Converts the value to a string representation.
    /// </summary>
    /// <returns>The string representation of the value.</returns>
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
}