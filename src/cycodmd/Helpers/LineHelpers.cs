using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

class LineHelpers
{
    public static bool IsLineMatch(string line, List<Regex> includeLineContainsPatternList, List<Regex> removeAllLineContainsPatternList)
    {
        var includeMatch = includeLineContainsPatternList.All(regex => regex.IsMatch(line));
        var excludeMatch = removeAllLineContainsPatternList.Count > 0 && removeAllLineContainsPatternList.Any(regex => regex.IsMatch(line));
        
        // Log detailed information at verbose level
        if (ConsoleHelpers.IsVerbose())
        {
            if (!includeMatch && includeLineContainsPatternList.Count > 0)
            {
                var failedPatterns = includeLineContainsPatternList
                    .Where(regex => !regex.IsMatch(line))
                    .Select(regex => regex.ToString())
                    .ToList();
                
                Logger.Verbose($"Line excluded because it doesn't match include patterns: [{string.Join(", ", failedPatterns)}]");
                Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
            }
            
            if (excludeMatch)
            {
                var matchedPatterns = removeAllLineContainsPatternList
                    .Where(regex => regex.IsMatch(line))
                    .Select(regex => regex.ToString())
                    .ToList();
                    
                Logger.Verbose($"Line excluded because it matches exclude patterns: [{string.Join(", ", matchedPatterns)}]");
                Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
            }
        }

        return includeMatch && !excludeMatch;
    }
}