using System;
using System.Collections.Generic;

/// <summary>
/// A simple implementation of INamedValues that stores values in a dictionary
/// </summary>
public class TemplateVariables : INamedValues
{
    private readonly Dictionary<string, string> _variables;

    /// <summary>
    /// Creates a new TemplateVariables instance with an empty dictionary
    /// </summary>
    public TemplateVariables()
    {
        _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a new TemplateVariables instance with the provided dictionary
    /// </summary>
    /// <param name="variables">Initial variables to include</param>
    public TemplateVariables(Dictionary<string, string> variables)
    {
        _variables = new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a value by name, or an empty string if not found
    /// </summary>
    /// <param name="name">The name of the value to retrieve</param>
    /// <returns>The value, or an empty string if not found</returns>
    public string? Get(string name)
    {
        // First check the variable dictionary
        if (_variables.TryGetValue(name, out var value))
        {
            return value;
        }

        // Then check configuration
        var configValue = ConfigStore.Instance.GetFromAnyScope(name);
        if (configValue.Source != ConfigSource.NotFound)
        {
            return configValue!.AsString();
        }

        // Then check environment variables
        var envValue = Environment.GetEnvironmentVariable(name);
        if (envValue != null)
        {
            return envValue;
        }

        // Finally check special variables
        var builtInValue = GetBuiltInVariable(name);
        if (builtInValue != null)
        {
            return builtInValue;
        }

        return null;
    }

    /// <summary>
    /// Sets a value by name
    /// </summary>
    /// <param name="name">The name of the value to set</param>
    /// <param name="value">The value to set</param>
    public void Set(string name, string value)
    {
        _variables[name] = value;
    }

    /// <summary>
    /// Checks if a value exists
    /// </summary>
    /// <param name="name">The name of the value to check</param>
    /// <returns>True if the value exists, false otherwise</returns>
    public bool Contains(string name)
    {
        return _variables.ContainsKey(name) ||
               ConfigStore.Instance.GetFromAnyScope(name).Value != null ||
               Environment.GetEnvironmentVariable(name) != null ||
               GetBuiltInVariable(name) != null;
    }

    /// <summary>
    /// Gets a built-in variable value by name
    /// </summary>
    /// <param name="name"></param>
    private static string? GetBuiltInVariable(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "os":
                return Environment.OSVersion.ToString();
            case "osname":
                return Environment.OSVersion.Platform.ToString();
            case "osversion":
                return Environment.OSVersion.Version.ToString();
            case "date":
                return DateTime.Now.ToString("yyyy-MM-dd");
            case "time":
                return DateTime.Now.ToString("HH:mm:ss");
            case "datetime":
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            case "year":
                return DateTime.Now.Year.ToString();
            case "month":
                return DateTime.Now.Month.ToString();
            case "day":
                return DateTime.Now.Day.ToString();
            case "hour":
                return DateTime.Now.Hour.ToString();
            case "minute":
                return DateTime.Now.Minute.ToString();
            case "second":
                return DateTime.Now.Second.ToString();
            case "random":
                return new Random().Next(0, 1000).ToString();
        }

        return null;
    }
}