using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// Helper class for capturing screenshots on Windows.
/// Provides platform-aware screenshot functionality with graceful degradation on non-Windows platforms.
/// </summary>
public static class ScreenshotHelper
{
    [DllImport("user32.dll")]
    [SupportedOSPlatform("windows6.1")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;  // Width of the primary display monitor
    private const int SM_CYSCREEN = 1;  // Height of the primary display monitor

    /// <summary>
    /// Takes a screenshot of the primary screen and saves it to a temporary file.
    /// </summary>
    /// <returns>The file path to the saved screenshot, or null if the operation failed</returns>
    [SupportedOSPlatform("windows6.1")]
    public static string? TakeScreenshot()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        try
        {
            // Get primary screen dimensions using Win32 API
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);

            if (screenWidth == 0 || screenHeight == 0)
            {
                return null;
            }

            // Create bitmap with screen dimensions
            using var bitmap = new Bitmap(screenWidth, screenHeight);
            
            // Capture screen
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));

            // Save to temp file with timestamp
            var fileName = Path.Combine(Path.GetTempPath(), $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            bitmap.Save(fileName, ImageFormat.Png);

            return fileName;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a user-friendly error message for non-Windows platforms.
    /// </summary>
    public static string GetPlatformErrorMessage()
    {
        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                       RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" :
                       RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Unknown";
        
        return $"Screenshot functionality is currently only available on Windows. Current platform: {platform}";
    }
}
