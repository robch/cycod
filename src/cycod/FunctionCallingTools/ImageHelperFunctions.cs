//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.ComponentModel;
using Microsoft.Extensions.AI;

/// <summary>
/// Helper functions for adding images to conversations with multimodal AI models.
/// </summary>
public class ImageHelperFunctions
{
    public ImageHelperFunctions(ChatCommand chatCommand)
    {
        _chatCommand = chatCommand;
    }

    [Description("Add one or more image files to the current conversation. The images will be included in the next message exchange. Supports individual files or glob patterns like '*.png' to include multiple images at once.")]
    public object AddImageToConversation([Description("Absolute file path or glob pattern to one or more existing image files")] string imageFileNameOrGlobPattern)
    {
        try
        {
            var resolvedImages = ImageResolver.ResolveImagePatterns(new[] { imageFileNameOrGlobPattern.Trim() });
            if (resolvedImages.Count == 0)
            {
                return $"No image files found matching pattern: {imageFileNameOrGlobPattern}";
            }

            // If there's exactly one image, return it as DataContent
            if (resolvedImages.Count == 1)
            {
                var imageFile = resolvedImages[0];
                if (File.Exists(imageFile))
                {
                    try
                    {
                        var imageBytes = File.ReadAllBytes(imageFile);
                        var mediaType = ImageResolver.GetMediaTypeFromFileExtension(imageFile);
                        return new DataContent(imageBytes, mediaType);
                    }
                    catch (Exception ex)
                    {
                        return $"Error loading image {imageFile}: {ex.Message}";
                    }
                }
            }

            return $"More than one image found matching pattern: {imageFileNameOrGlobPattern}";
        }
        catch (InvalidOperationException ex)
        {
            return $"Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error adding image to conversation: {ex.Message}";
        }
    }

    private readonly ChatCommand _chatCommand;
}