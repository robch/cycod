using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.AI;

/// <summary>
/// AI tool for capturing screenshots and adding them to the conversation.
/// </summary>
public class ScreenshotHelperFunctions
{
    public ScreenshotHelperFunctions(ChatCommand chatCommand)
    {
        _chatCommand = chatCommand;
    }

#if WINDOWS || OSX
    [Description("Take a screenshot of the primary screen and add it to the conversation. The screenshot will be included in the next message exchange. Works on Windows and macOS.")]
    public object TakeScreenshot()
    {
        // Check platform support
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ScreenshotHelper.GetPlatformErrorMessage();
        }

        try
        {
            // Capture screenshot
            var filePath = ScreenshotHelper.TakeScreenshot();
            var fileExists = FileHelpers.FileExists(filePath);
            if (!fileExists) return "Failed to capture screenshot. Please check that the display is accessible.";

            // Load the screenshot and return as DataContent for immediate inclusion
            try
            {
                if (filePath == null) return "Failed to capture screenshot - no file path returned.";
                var imageBytes = File.ReadAllBytes(filePath);
                var mediaType = ImageResolver.GetMediaTypeFromFileExtension(filePath);
                return new DataContent(imageBytes, mediaType);
            }
            catch (Exception ex)
            {
                return $"Error loading screenshot {filePath}: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot: {ex.Message}";
        }
    }
#endif

    private readonly ChatCommand _chatCommand;
}
