/// <summary>
/// Helper class for handling multiline input with backtick code blocks.
/// </summary>
public static class MultilineInputHelper
{
    /// <summary>
    /// Reads multiline input from the console when a line starts with backticks.
    /// </summary>
    /// <param name="firstLine">The first line read, which should start with backticks.</param>
    /// <returns>The content between matching backtick markers.</returns>
    public static string ReadMultilineInput(string firstLine)
    {
        // Count the backticks at the start of the line
        int backtickCount = CountLeadingBackticks(firstLine);
        
        // If there aren't at least 3 backticks, return the original line
        if (backtickCount < 3)
        {
            return firstLine;
        }
        
        // Store the content after the backticks from the first line
        var builder = new System.Text.StringBuilder();
        string contentAfterBackticks = firstLine.Substring(backtickCount);
        
        // Add the content after backticks from the first line (if not empty)
        if (!string.IsNullOrEmpty(contentAfterBackticks))
        {
            builder.AppendLine(contentAfterBackticks);
        }
        
        // Read additional lines until we find a matching closing backtick
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            // Check if this line has the matching number of backticks at the start
            if (CountLeadingBackticks(line) == backtickCount && line.Trim() == new string('`', backtickCount))
            {
                // Closing backtick pattern found, exit the loop
                break;
            }
            
            // Add the line to our content
            builder.AppendLine(line);
        }
        
        return builder.ToString().TrimEnd();
    }
    
    /// <summary>
    /// Counts the number of backtick characters at the start of a string.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <returns>The number of consecutive backticks at the start of the line.</returns>
    private static int CountLeadingBackticks(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return 0;
        }
        
        int count = 0;
        while (count < line.Length && line[count] == '`')
        {
            count++;
        }
        
        return count;
    }
    
    /// <summary>
    /// Checks if a line starts with at least 3 backticks.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <returns>True if the line starts with at least 3 backticks, false otherwise.</returns>
    public static bool StartsWithBackticks(string line)
    {
        return !string.IsNullOrEmpty(line) && CountLeadingBackticks(line) >= 3;
    }
}