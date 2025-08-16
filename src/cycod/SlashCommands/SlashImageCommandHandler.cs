using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Handles the /image slash command to add images to the chat
/// </summary>
public class SlashImageCommandHandler
{
    /// <summary>
    /// Checks if the handler can process a given command
    /// </summary>
    /// <param name="commandWithArgs">The user's input</param>
    /// <returns>True if the command can be handled</returns>
    public bool IsCommand(string commandWithArgs)
    {
        if (string.IsNullOrWhiteSpace(commandWithArgs) || !commandWithArgs.StartsWith("/"))
        {
            return false;
        }

        var commandName = ExtractCommandName(commandWithArgs);
        return commandName == "/image";
    }
    
    /// <summary>
    /// Extracts the command name from the user prompt
    /// </summary>
    /// <param name="userPrompt">The user's input</param>
    /// <returns>The command name</returns>
    public string GetCommandName(string userPrompt)
    {
        return ExtractCommandName(userPrompt);
    }

    /// <summary>
    /// Handles the /image command and returns the image patterns for processing
    /// </summary>
    /// <param name="userPrompt">The user's input</param>
    /// <returns>The image patterns to be processed</returns>
    public List<string> HandleCommand(string userPrompt)
    {
        var arguments = ExtractArguments(userPrompt);
        
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new List<string>();
        }

        // Split arguments by spaces to support multiple patterns
        var patterns = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        return patterns;
    }

    /// <summary>
    /// Gets a user-friendly message about what images were added
    /// </summary>
    /// <param name="imagePatterns">The image patterns processed</param>
    /// <returns>A status message about the images added</returns>
    public string GetImageAddedMessage(List<string> imagePatterns)
    {
        if (imagePatterns.Count == 0)
        {
            return "No image patterns provided.";
        }

        var totalFiles = 0;
        var allFiles = new List<string>();

        foreach (var pattern in imagePatterns)
        {
            var files = FileHelpers.FilesFromGlob(pattern).Where(IsImageFile).ToList();
            totalFiles += files.Count;
            allFiles.AddRange(files.Select(f => Path.GetFileName(f)));
        }

        if (totalFiles == 0)
        {
            return $"No image files found matching pattern(s): {string.Join(", ", imagePatterns)}";
        }

        var fileList = allFiles.Count <= 3 
            ? string.Join(", ", allFiles)
            : $"{string.Join(", ", allFiles.Take(3))}, and {allFiles.Count - 3} more";

        return totalFiles == 1 
            ? $"Added image file '{fileList}'"
            : $"Added {totalFiles} image files: {fileList}";
    }

    /// <summary>
    /// Extracts the command name from a user prompt
    /// </summary>
    private string ExtractCommandName(string userPrompt)
    {
        var parts = userPrompt.Split(new[] { ' ' }, 2);
        return parts[0].ToLowerInvariant();
    }
    
    /// <summary>
    /// Extracts the arguments from a user prompt
    /// </summary>
    private string ExtractArguments(string userPrompt)
    {
        var parts = userPrompt.Split(new[] { ' ' }, 2);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    /// <summary>
    /// Checks if a file is an image file based on its extension
    /// </summary>
    private static bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" or ".tiff" or ".tif" => true,
            _ => false
        };
    }
}