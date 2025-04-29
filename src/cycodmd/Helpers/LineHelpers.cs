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

        return includeMatch && !excludeMatch;
    }
}