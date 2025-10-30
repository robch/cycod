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

    [Description("Add one image file to the current conversation. The image will be included in the next message exchange.")]
    public object AddImageToConversation([Description("Absolute file path to one image file; width and height must be less than 8000px")] string imageFileName)
    {
        try
        {
            var resolvedImages = ImageResolver.ResolveImagePatterns(new[] { imageFileName.Trim() });
            if (resolvedImages.Count == 0)
            {
                return $"No image files found matching pattern: {imageFileName}";
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

            return $"More than one image found matching pattern: {imageFileName}";
        }
        catch (Exception ex)
        {
            return $"Error adding image to conversation: {ex.Message}";
        }
    }

    private readonly ChatCommand _chatCommand;
}