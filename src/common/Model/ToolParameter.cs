using System;
using System.Collections.Generic;

/// <summary>
/// Represents a parameter for a custom tool.
/// </summary>
public class ToolParameter
{
    /// <summary>
    /// Description of the parameter.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of the parameter (string, number, boolean, array).
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// Whether the parameter is required.
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Default value for the parameter.
    /// </summary>
    public object? Default { get; set; }

    /// <summary>
    /// For boolean parameters, specifies the flag to use when true (e.g., "-i" for ignore-case).
    /// </summary>
    public string? FlagMapping { get; set; }

    /// <summary>
    /// Validates the parameter definition.
    /// </summary>
    public bool Validate(out string error)
    {
        if (Type != "string" && Type != "number" && Type != "boolean" && Type != "array")
        {
            error = $"Invalid parameter type: {Type}. Valid types are: string, number, boolean, array.";
            return false;
        }

        if (Default != null)
        {
            if (Type == "number" && !double.TryParse(Default.ToString(), out _))
            {
                error = $"Default value '{Default}' is not a valid number.";
                return false;
            }
            else if (Type == "boolean" && !bool.TryParse(Default.ToString(), out _) && 
                        Default.ToString() != "0" && Default.ToString() != "1")
            {
                error = $"Default value '{Default}' is not a valid boolean.";
                return false;
            }
            else if (Type == "array" && !(Default is IEnumerable<object>))
            {
                error = $"Default value '{Default}' is not a valid array.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Validates a parameter value against this parameter definition.
    /// </summary>
    public bool ValidateValue(object? value, out string error)
    {
        if (Required && value == null && Default == null)
        {
            error = "Parameter is required but no value was provided.";
            return false;
        }

        if (value != null)
        {
            if (Type == "number" && !double.TryParse(value.ToString(), out _))
            {
                error = $"Value '{value}' is not a valid number.";
                return false;
            }
            else if (Type == "boolean" && !bool.TryParse(value.ToString(), out _) &&
                        value.ToString() != "0" && value.ToString() != "1")
            {
                error = $"Value '{value}' is not a valid boolean.";
                return false;
            }
            else if (Type == "array" && !(value is IEnumerable<object>))
            {
                error = $"Value '{value}' is not a valid array.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Gets the effective value for this parameter.
    /// </summary>
    public object? GetEffectiveValue(object? value)
    {
        return value ?? Default;
    }

    /// <summary>
    /// Converts a parameter value to the correct type.
    /// </summary>
    public object? ConvertValue(object? value)
    {
        if (value == null)
        {
            return Default;
        }

        try
        {
            return Type switch
            {
                "string" => value.ToString(),
                "number" => double.Parse(value.ToString() ?? "0"),
                "boolean" => ParseBoolean(value.ToString() ?? "false"),
                "array" => value is IEnumerable<object> ? value : new List<object> { value },
                _ => value
            };
        }
        catch
        {
            return value;
        }
    }

    private bool ParseBoolean(string value)
    {
        return value.ToLower() switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            "y" => true,
            "n" => false,
            _ => false
        };
    }
}