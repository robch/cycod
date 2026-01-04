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
            if (!fileExists)
            {
                // Use macOS-specific error message if on macOS, otherwise generic message
                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? ScreenshotHelper.GetMacOSPermissionErrorMessage()
                    : "Failed to capture screenshot. Please check that the display is accessible.";
            }

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

#if OSX
    [Description("Take a screenshot of a window with matching title (partial match, case-insensitive). Returns the screenshot file path or an error message. Example: title='cycod' or title='Microsoft Edge'.")]
    public object TakeScreenshotOfWindowWithTitle(string title)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window-specific screenshots are only available on macOS.";
        }

        try
        {
            var result = ScreenshotHelper.TakeScreenshotOfWindowWithTitle(title);
            
            // If result is an existing file path, load and return as DataContent
            if (!string.IsNullOrEmpty(result) && File.Exists(result))
            {
                var imageBytes = File.ReadAllBytes(result);
                var mediaType = ImageResolver.GetMediaTypeFromFileExtension(result);
                return new DataContent(imageBytes, mediaType);
            }
            
            // Otherwise it's an error message
            return result;
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot by title: {ex.Message}";
        }
    }

    [Description("Take a screenshot of a window from the specified application (partial match, case-insensitive). Returns the screenshot file path or an error message. Example: appName='Warp' or appName='Code'.")]
    public object TakeScreenshotOfApp(string appName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window-specific screenshots are only available on macOS.";
        }

        try
        {
            var result = ScreenshotHelper.TakeScreenshotOfApp(appName);
            
            // If result starts with '/', it's a file path - load and return as DataContent
            if (result.StartsWith('/') || result.StartsWith(Path.GetTempPath()))
            {
                var imageBytes = File.ReadAllBytes(result);
                var mediaType = ImageResolver.GetMediaTypeFromFileExtension(result);
                return new DataContent(imageBytes, mediaType);
            }
            
            // Otherwise it's an error message
            return result;
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot by app: {ex.Message}";
        }
    }

    [Description("Take a screenshot of a specific display. displayNumber: 1 for main display, 2 for secondary, etc. Returns the screenshot or an error message.")]
    public object TakeScreenshotOfDisplay(int displayNumber)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Display-specific screenshots are only available on macOS.";
        }

        try
        {
            var filePath = ScreenshotHelper.TakeScreenshotOfDisplay(displayNumber);
            if (filePath == null)
            {
                return $"Failed to capture screenshot of display {displayNumber}. Please check that the display exists.";
            }

            var imageBytes = File.ReadAllBytes(filePath);
            var mediaType = ImageResolver.GetMediaTypeFromFileExtension(filePath);
            return new DataContent(imageBytes, mediaType);
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot of display {displayNumber}: {ex.Message}";
        }
    }

    [Description("List all visible application windows with their metadata (window ID, app name, title, position, size). Returns a JSON array of window information. Useful for finding windows before capturing them.")]
    public object ListWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window enumeration is only available on macOS.";
        }

        try
        {
            var windows = ScreenshotHelper.EnumerateWindows();
            
            if (windows.Count == 0)
            {
                return "No application windows found.";
            }

            var json = System.Text.Json.JsonSerializer.Serialize(windows, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return json;
        }
        catch (Exception ex)
        {
            return $"Error listing windows: {ex.Message}";
        }
    }

    [Description("Take a screenshot of a specific window by ID. Use ListWindows() first to get window IDs. Returns the screenshot or an error message. This is an advanced method - prefer TakeScreenshotOfWindowWithTitle or TakeScreenshotOfApp for simpler use.")]
    public object TakeScreenshotOfWindow(int windowId)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window-specific screenshots are only available on macOS.";
        }

        try
        {
            var filePath = ScreenshotHelper.TakeScreenshotOfWindow(windowId);
            if (filePath == null)
            {
                return $"Failed to capture screenshot of window {windowId}. Please check that the window ID is valid.";
            }

            var imageBytes = File.ReadAllBytes(filePath);
            var mediaType = ImageResolver.GetMediaTypeFromFileExtension(filePath);
            return new DataContent(imageBytes, mediaType);
        }
        catch (Exception ex)
        {
            return $"Error capturing screenshot of window {windowId}: {ex.Message}";
        }
    }
#endif

    private readonly ChatCommand _chatCommand;
}
