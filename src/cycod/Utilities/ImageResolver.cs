using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Utility class for resolving image patterns to actual image files
/// </summary>
public static class ImageResolver
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    /// <summary>
    /// Resolves image patterns (including globs) to actual image file paths
    /// </summary>
    /// <param name="imagePatterns">Collection of file paths or glob patterns</param>
    /// <returns>List of resolved image file paths</returns>
    /// <exception cref="InvalidOperationException">Thrown when patterns don't match any files or files are invalid</exception>
    public static List<string> ResolveImagePatterns(IEnumerable<string> imagePatterns)
    {
        var resolvedImages = new List<string>();
        
        foreach (var pattern in imagePatterns)
        {
            var noPattern = string.IsNullOrWhiteSpace(pattern);
            if (noPattern) continue;
                
            var trimmed = pattern.Trim();
            if (IsGlobPattern(trimmed))
            {
                var matchedFiles = FileHelpers.FilesFromGlob(trimmed)
                    .Where(IsImageFile)
                    .ToList();
                resolvedImages.AddRange(matchedFiles);
            }
            else
            {
                var okToAdd = File.Exists(trimmed) && IsImageFile(trimmed);
                if (okToAdd) resolvedImages.Add(trimmed);
            }
        }
        
        return resolvedImages;
    }

    /// <summary>
    /// Checks if a file is a supported image type based on its extension
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if the file is a supported image type</returns>
    public static bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return SupportedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Gets the media type for an image file based on its extension
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>Media type string</returns>
    public static string GetMediaTypeFromFileExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static bool IsGlobPattern(string pattern)
    {
        return pattern.Contains('*') || pattern.Contains('?');
    }
}