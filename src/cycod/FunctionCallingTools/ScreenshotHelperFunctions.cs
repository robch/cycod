using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.AI;

/// <summary>
/// AI tool for capturing screenshots and adding them to the conversation.
/// </summary>
public class ScreenshotHelperFunctions
{
    [Description("Take a screenshot of the primary screen and add it to the conversation. The screenshot will be included in the next message exchange. Only works on Windows.")]
    public object TakeScreenshot()
    {
        // Check platform support
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ScreenshotHelper.GetPlatformErrorMessage();
        }

        try
        {
            // Capture screenshot
            var filePath = ScreenshotHelper.TakeScreenshot();
            if (filePath == null)
            {
                return "Failed to capture screenshot. Please check that the display is accessible.";
            }

            // Read the image and return as DataContent for immediate inclusion in conversation
            if (File.Exists(filePath))
            {
                try
                {
                    var imageBytes = File.ReadAllBytes(filePath);
                    var mediaType = "image/png";
                    
                    // Return DataContent so the image is immediately added to the conversation
                    return new DataContent(imageBytes, mediaType);
                }
                catch (Exception ex)
                {
                    return $"Screenshot captured to {filePath}, but failed to load image: {ex.Message}";
                }
            }

            return $"Screenshot saved to {filePath}, but file not found.";
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot: {ex.Message}";
        }
    }
}
