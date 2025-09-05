using System;
using System.Text.RegularExpressions;

public static class TimeSpecHelpers
{
    // Parse a time specification that could be either a single value or a range
    public static (DateTime? After, DateTime? Before) ParseTimeSpecRange(string optionName, string? timeSpec)
    {
        if (string.IsNullOrEmpty(timeSpec))
            throw new CommandLineException($"Missing time specification for {optionName}");

        // Handle range syntax with ".."
        if (timeSpec.Contains(".."))
        {
            var parts = timeSpec.Split(new[] { ".." }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new CommandLineException($"Invalid range format for {optionName}: {timeSpec}");
                
            DateTime? after = null;
            DateTime? before = null;
            
            if (!string.IsNullOrEmpty(parts[0]))
                after = ParseSingleTimeSpec(optionName, parts[0], isAfter: true);
                
            if (!string.IsNullOrEmpty(parts[1]))
                before = ParseSingleTimeSpec(optionName, parts[1], isAfter: false);
                
            return (after, before);
        }
        else
        {
            // For single date/time (represents a time period)
            var parsed = ParseSingleTimeSpec(optionName, timeSpec, isAfter: true);
            
            // If it's a date without time, set before to end of day
            DateTime? before = parsed;
            if (parsed.HasValue && IsDateOnly(parsed.Value))
                before = parsed.Value.AddDays(1).AddTicks(-1);
                
            return (parsed, before);
        }
    }

    // Parse a single time specification (not a range)
    public static DateTime? ParseSingleTimeSpec(string optionName, string? timeSpec, bool isAfter)
    {
        if (string.IsNullOrEmpty(timeSpec))
            throw new CommandLineException($"Missing time specification for {optionName}");

        // Handle special keywords
        if (timeSpec.Equals("today", StringComparison.OrdinalIgnoreCase))
            return isAfter ? DateTime.Today : DateTime.Today.AddDays(1).AddTicks(-1);

        if (timeSpec.Equals("yesterday", StringComparison.OrdinalIgnoreCase))
            return isAfter ? DateTime.Today.AddDays(-1) : DateTime.Today.AddTicks(-1);

        // Handle absolute dates
        if (DateTime.TryParse(timeSpec, out var dateTime))
        {
            // If it's a date without time, adjust to start/end of day as appropriate
            if (IsDateOnly(dateTime))
            {
                return isAfter ? dateTime : dateTime.AddDays(1).AddTicks(-1);
            }
            return dateTime;
        }

        // Handle combined time specs like "2d4h3m"
        var combinedMatch = Regex.Match(timeSpec, @"^-?(\d+[dhms])+$", RegexOptions.IgnoreCase);
        if (combinedMatch.Success)
        {
            return ParseCombinedTimeSpec(timeSpec);
        }

        // Handle simple relative time specs like "3d" or "-2h"
        var match = Regex.Match(timeSpec, @"^(-?)(\d+)([dhms])$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var sign = match.Groups[1].Value == "-" ? -1 : 1;
            var value = int.Parse(match.Groups[2].Value) * sign;
            var unit = match.Groups[3].Value.ToLower();
            
            var now = DateTime.Now;
            
            switch (unit)
            {
                case "d":
                    // For days, use calendar day boundaries
                    var date = now.Date.AddDays(value);
                    return isAfter ? date : date.AddDays(1).AddTicks(-1);
                
                case "h":
                    return now.AddHours(value);
                
                case "m":
                    return now.AddMinutes(value);
                
                case "s":
                    return now.AddSeconds(value);
                
                default:
                    throw new CommandLineException($"Invalid time unit in {optionName}: {unit}");
            }
        }

        throw new CommandLineException($"Invalid time specification for {optionName}: {timeSpec}");
    }

    // Parse a combined time specification like "2d4h3m"
    private static DateTime ParseCombinedTimeSpec(string timeSpec)
    {
        var isNegative = timeSpec.StartsWith("-");
        var pattern = @"(\d+)([dhms])";
        var matches = Regex.Matches(isNegative ? timeSpec.Substring(1) : timeSpec, pattern);
        
        var now = DateTime.Now;
        var result = now;
        
        foreach (Match match in matches)
        {
            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value.ToLower();
            
            // Apply the sign to the value
            if (isNegative)
                value = -value;
                
            switch (unit)
            {
                case "d":
                    result = result.AddDays(value);
                    break;
                case "h":
                    result = result.AddHours(value);
                    break;
                case "m":
                    result = result.AddMinutes(value);
                    break;
                case "s":
                    result = result.AddSeconds(value);
                    break;
            }
        }
        
        return result;
    }

    // Check if a DateTime represents only a date (no time component)
    private static bool IsDateOnly(DateTime dateTime)
    {
        return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }
}