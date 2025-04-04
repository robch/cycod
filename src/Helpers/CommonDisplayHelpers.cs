using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Provides common display methods for consistent output formatting across commands.
/// </summary>
public static class CommonDisplayHelpers
{
    // Constants for content display
    public const int DefaultIndentLevel = 2;
    public const int DefaultMaxContentLines = 2;
    public const int DefaultMaxContentWidth = 60;

    /// <summary>
    /// Formats a location header with scope information.
    /// </summary>
    /// <param name="directoryPath">Directory path to display</param>
    /// <param name="scope">The configuration scope</param>
    /// <returns>Formatted location string</returns>
    public static string FormatLocationHeader(string directoryPath, ConfigFileScope scope)
    {
        return $"{directoryPath} ({scope.ToString().ToLower()})";
    }

    /// <summary>
    /// Writes a location header to the console.
    /// </summary>
    /// <param name="locationHeader">The formatted location header</param>
    public static void WriteLocationHeader(string locationHeader)
    {
        ConsoleHelpers.WriteLine($"LOCATION: {locationHeader}", overrideQuiet: true);
        ConsoleHelpers.WriteLine("", overrideQuiet: true);
    }

    /// <summary>
    /// Displays a list of items with a location header.
    /// </summary>
    /// <param name="locationHeader">The location header to display</param>
    /// <param name="items">List of item names to display</param>
    /// <param name="itemPrefix">Optional prefix to add before each item (e.g., "/" for prompts)</param>
    /// <param name="indentLevel">Indentation level</param>
    public static void DisplayItemList(string locationHeader, List<string> items, string itemPrefix = "", int indentLevel = DefaultIndentLevel)
    {
        // Display location header
        WriteLocationHeader(locationHeader);
        
        var indent = new string(' ', indentLevel);
        
        // Display items or "No items found" message
        if (items.Count == 0)
        {
            ConsoleHelpers.WriteLine($"{indent}No items found in this scope", overrideQuiet: true);
            return;
        }

        foreach (var item in items)
        {
            ConsoleHelpers.WriteLine($"{indent}{itemPrefix}{item}", overrideQuiet: true);
        }
    }

    /// <summary>
    /// Displays a single item with its details.
    /// </summary>
    /// <param name="name">Name of the item</param>
    /// <param name="location">Location where the item is stored</param>
    /// <param name="value">Content of the item</param>
    /// <param name="usage">Usage pattern to display</param>
    /// <param name="nameHeader">Header to use for the item name (e.g., "PROMPT", "ALIAS")</param>
    /// <param name="limitValue">Whether to show a preview of content rather than full content</param>
    public static void DisplayItem(
        string name, 
        string? value = null, 
        string? location = null, 
        bool limitValue = false,
        int indentLevel = 0)
    {
        // Display location
        ConsoleHelpers.WriteLine(IndentContent($"LOCATION: {location}", indentLevel), overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);
        indentLevel += 2;

        // Display item name
        ConsoleHelpers.WriteLine(IndentContent(name, indentLevel), overrideQuiet: true);
        indentLevel += 2;
        
        // Display item value
        if (value != null)
        {
            var content = limitValue
                ? TruncateContent(value, indentLevel)
                : IndentContent(value, indentLevel);
            ConsoleHelpers.WriteLine($"\n{content}", overrideQuiet: true);
        }
    }

    /// <summary>
    /// Truncates content to a specified number of lines and width.
    /// </summary>
    /// <param name="content">Content to truncate</param>
    /// <param name="maxLines">Maximum number of lines to show</param>
    /// <param name="maxWidth">Maximum width per line</param>
    /// <param name="indentLevel">Indentation level for content</param>
    /// <returns>Truncated content string</returns>
    public static string TruncateContent(
        string content, 
        int maxLines = DefaultMaxContentLines, 
        int maxWidth = DefaultMaxContentWidth,
        int indentLevel = DefaultIndentLevel)
    {
        var contentLines = content
            .Split(new[] { '\n', '\r' }, StringSplitOptions.None)
            .Select(line => line.Trim());
            
        if (!contentLines.Any())
        {
            return new string(' ', indentLevel) + "(empty)";
        }
        
        var indent = new string(' ', indentLevel);
        var linesToShow = contentLines
            .Take(maxLines)
            .Select(line => line.Length > maxWidth ? line.Substring(0, maxWidth - 3) + "..." : line)
            .Select(line => indent + line)
            .ToList();
            
        // Remove trailing empty lines
        while (linesToShow.Count > 0 && string.IsNullOrWhiteSpace(linesToShow.Last()))
        {
            linesToShow.RemoveAt(linesToShow.Count - 1);
        }
        
        var totalLineCount = contentLines.Count();
        var result = string.Join("\n", linesToShow);
        
        // Add indicator of additional lines if content was truncated
        if (totalLineCount > maxLines)
        {
            result += $"\n{indent}... ({totalLineCount - maxLines} more lines)";
        }
        
        return result;
    }

    /// <summary>
    /// Indents all lines of content with consistent spacing.
    /// </summary>
    /// <param name="content">Content to indent</param>
    /// <param name="indentLevel">Indentation level for content</param>
    /// <returns>Consistently indented content string</returns>
    public static string IndentContent(
        string content, 
        int indentLevel = DefaultIndentLevel)
    {
        // Handle empty content
        if (string.IsNullOrEmpty(content))
        {
            return new string(' ', indentLevel) + "(empty)";
        }
        
        var indent = new string(' ', indentLevel);
        var contentLines = content
            .Split(new[] { '\n', '\r' }, StringSplitOptions.None)
            .Select(line => indent + line)
            .ToList();
            
        return string.Join("\n", contentLines);
    }

    /// <summary>
    /// Displays information about saved files.
    /// </summary>
    /// <param name="filesSaved">List of saved files</param>
    /// <param name="usagePattern">Usage pattern to display</param>
    public static void DisplaySavedFiles(List<string> filesSaved, string usagePattern)
    {
        if (filesSaved.Count == 0) return;
        
        var firstFileSaved = filesSaved.First();
        var additionalFiles = filesSaved.Skip(1).ToList();
        
        ConsoleHelpers.WriteLine($"Saved: {firstFileSaved}", overrideQuiet: true);
        ConsoleHelpers.WriteLine("", overrideQuiet: true);
        
        if (additionalFiles.Any())
        {
            foreach (var additionalFile in additionalFiles)
            {
                ConsoleHelpers.WriteLine($"  and: {additionalFile}", overrideQuiet: true);
            }
            
            ConsoleHelpers.WriteLine("", overrideQuiet: true);
        }
        
        var itemName = Path.GetFileNameWithoutExtension(firstFileSaved);
        ConsoleHelpers.WriteLine($"USAGE: {usagePattern.Replace("{name}", itemName)}", overrideQuiet: true);
    }
}