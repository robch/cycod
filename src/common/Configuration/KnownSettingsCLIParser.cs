using System;
using System.Text.RegularExpressions;

/// <summary>
/// Parser for command line options related to known settings.
/// </summary>
public static class KnownSettingsCLIParser
{
    /// <summary>
    /// Attempts to parse a command line argument as a known setting.
    /// </summary>
    /// <param name="arg">The command line argument to parse.</param>
    /// <param name="settingName">The setting name in dot notation if successful.</param>
    /// <param name="value">The setting value if successful.</param>
    /// <returns>True if the argument was successfully parsed as a known setting, false otherwise.</returns>
    public static bool TryParseCLIOption(string arg, out string? settingName, out string? value)
    {
        settingName = null;
        value = null;
        
        // If it doesn't start with --, it's not a CLI option
        if (!arg.StartsWith("--")) return false;
        
        // Check for option with attached value (--option=value)
        int equalsPos = arg.IndexOf('=');
        string optionName;
        
        if (equalsPos >= 0)
        {
            optionName = arg.Substring(0, equalsPos);
            value = arg.Substring(equalsPos + 1);
        }
        else
        {
            optionName = arg;
            value = null;
        }
        
        settingName = KnownSettings.ToDotNotation(optionName);
        return KnownSettings.IsKnown(settingName);
    }
}