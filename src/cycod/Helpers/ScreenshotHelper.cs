#if WINDOWS
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
using System.Runtime.InteropServices;

/// <summary>
/// Helper class for capturing screenshots on Windows and macOS.
/// Provides platform-aware screenshot functionality with graceful degradation on unsupported platforms.
/// </summary>
public static class ScreenshotHelper
{
    /// <summary>
    /// Takes a screenshot of the primary screen and saves it to a temporary file.
    /// </summary>
    /// <returns>The file path to the saved screenshot, or null if the operation failed</returns>
    public static string? TakeScreenshot()
    {
#if WINDOWS
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
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

            // Resize to keep file size manageable
            return ResizeImageIfNeeded(fileName);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot: {ex.Message}");
            return null;
        }
#elif OSX
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            // Create output file path
            var fileName = Path.Combine(
                Path.GetTempPath(), 
                $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            
            // Use the screencapture command-line tool (no permissions required from terminal)
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/sbin/screencapture",
                Arguments = $"-x \"{fileName}\"", // -x = no camera sound
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                Logger.Error("Failed to start screencapture process");
                return null;
            }

            process.WaitForExit(5000); // Wait up to 5 seconds

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Logger.Error($"screencapture failed with exit code {process.ExitCode}: {error}");
                return null;
            }

            // Verify the file was created
            if (!File.Exists(fileName))
            {
                Logger.Error("Screenshot file was not created");
                return null;
            }

            return ResizeImageIfNeeded(fileName);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot: {ex.Message}");
            return null;
        }
#else
        return null;
#endif
    }

#if OSX
    /// <summary>
    /// Captures a screenshot of a window with the specified title (partial match, case-insensitive).
    /// </summary>
    /// <param name="title">Window title to search for (e.g., "cycod", "Microsoft Edge")</param>
    /// <returns>Path to screenshot file, or error message if window not found or capture failed</returns>
    public static string TakeScreenshotOfWindowWithTitle(string title)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window-specific screenshots are only available on macOS.";
        }

        var windows = FindWindowsByTitle(title);
        
        if (windows.Count == 0)
        {
            return $"No windows found with title containing '{title}'.";
        }
        
        if (windows.Count > 1)
        {
            var matches = string.Join("\n", windows.Select(w => 
                $"  - Window {w.WindowId}: {w.ApplicationName} - {w.WindowTitle}"));
            return $"Multiple windows found with title containing '{title}':\n{matches}\n\nPlease be more specific or use TakeScreenshotOfWindow(windowId).";
        }
        
        var result = TakeScreenshotOfWindow(windows[0].WindowId);
        return result ?? $"Failed to capture screenshot of window: {windows[0]}";
    }

    /// <summary>
    /// Captures a screenshot of a window from the specified application.
    /// </summary>
    /// <param name="appName">Application name (e.g., "Warp", "Code", "Microsoft Edge")</param>
    /// <returns>Path to screenshot file, or error message if app window not found or capture failed</returns>
    public static string TakeScreenshotOfApp(string appName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Window-specific screenshots are only available on macOS.";
        }

        var windows = FindWindowsByApp(appName);
        
        if (windows.Count == 0)
        {
            return $"No windows found for application '{appName}'.";
        }
        
        if (windows.Count > 1)
        {
            var matches = string.Join("\n", windows.Select(w => 
                $"  - Window {w.WindowId}: {w.ApplicationName} - {w.WindowTitle}"));
            return $"Multiple windows found for application '{appName}':\n{matches}\n\nPlease be more specific or use TakeScreenshotOfWindow(windowId).";
        }
        
        var result = TakeScreenshotOfWindow(windows[0].WindowId);
        return result ?? $"Failed to capture screenshot of window: {windows[0]}";
    }

    /// <summary>
    /// Captures a screenshot of a specific display.
    /// </summary>
    /// <param name="displayNumber">Display number (1 = main, 2 = secondary, etc.)</param>
    /// <returns>Path to screenshot file, or null if capture failed</returns>
    public static string? TakeScreenshotOfDisplay(int displayNumber)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var fileName = Path.Combine(
                Path.GetTempPath(), 
                $"screenshot-display{displayNumber}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/sbin/screencapture",
                Arguments = $"-x -D {displayNumber} \"{fileName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                Logger.Error("Failed to start screencapture process");
                return null;
            }

            process.WaitForExit(5000);

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Logger.Error($"screencapture failed with exit code {process.ExitCode}: {error}");
                return null;
            }

            if (!File.Exists(fileName))
            {
                Logger.Error("Screenshot file was not created");
                return null;
            }

            return ResizeImageIfNeeded(fileName);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot of display {displayNumber}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Captures a screenshot of a specific window by ID.
    /// Use EnumerateWindows() to get window IDs.
    /// </summary>
    /// <param name="windowId">Window ID to capture</param>
    /// <returns>Path to screenshot file, or null if capture failed</returns>
    public static string? TakeScreenshotOfWindow(int windowId)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var fileName = Path.Combine(
                Path.GetTempPath(), 
                $"screenshot-window{windowId}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/sbin/screencapture",
                Arguments = $"-x -l {windowId} \"{fileName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                Logger.Error("Failed to start screencapture process");
                return null;
            }

            process.WaitForExit(5000);

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Logger.Error($"screencapture failed with exit code {process.ExitCode}: {error}");
                return null;
            }

            if (!File.Exists(fileName))
            {
                Logger.Error("Screenshot file was not created");
                return null;
            }

            return ResizeImageIfNeeded(fileName);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot of window {windowId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Lists all visible application windows with metadata.
    /// </summary>
    /// <returns>List of window information objects</returns>
    public static List<WindowInfo> EnumerateWindows()
    {
        var windows = new List<WindowInfo>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return windows;
        }

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var windowListPtr = CGWindowListCopyWindowInfo(
                kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements, 
                kCGNullWindowID);
            
            if (windowListPtr == IntPtr.Zero)
            {
                Logger.Error("Failed to get window list - CGWindowListCopyWindowInfo returned null");
                return windows;
            }

            var count = CFArrayGetCount(windowListPtr);
            
            for (var i = 0; i < count; i++)
            {
                var windowDictPtr = CFArrayGetValueAtIndex(windowListPtr, i);
                if (windowDictPtr == IntPtr.Zero) continue;

                var windowInfo = ParseWindowInfo(windowDictPtr);
                if (windowInfo != null)
                {
                    // Filter to normal application windows (layer 0) with titles
                    if (windowInfo.Layer == 0 && !string.IsNullOrWhiteSpace(windowInfo.WindowTitle))
                    {
                        windows.Add(windowInfo);
                    }
                }
            }

            CFRelease(windowListPtr);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to enumerate windows: {ex.Message}");
        }

        return windows;
    }

    /// <summary>
    /// Finds windows by application name (partial match, case-insensitive).
    /// </summary>
    /// <param name="appName">Application name to search for</param>
    /// <returns>List of matching windows</returns>
    public static List<WindowInfo> FindWindowsByApp(string appName)
    {
        var allWindows = EnumerateWindows();
        return allWindows
            .Where(w => w.ApplicationName.Contains(appName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Finds windows by title (partial match, case-insensitive).
    /// </summary>
    /// <param name="title">Window title to search for</param>
    /// <returns>List of matching windows</returns>
    public static List<WindowInfo> FindWindowsByTitle(string title)
    {
        var allWindows = EnumerateWindows();
        return allWindows
            .Where(w => !string.IsNullOrWhiteSpace(w.WindowTitle) && 
                       w.WindowTitle.Contains(title, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Parses window information from a Core Foundation dictionary.
    /// </summary>
    private static WindowInfo? ParseWindowInfo(IntPtr windowDictPtr)
    {
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var windowId = GetIntValue(windowDictPtr, "kCGWindowNumber");
            var pid = GetIntValue(windowDictPtr, "kCGWindowOwnerPID");
            var ownerName = GetStringValue(windowDictPtr, "kCGWindowOwnerName") ?? "";
            var windowName = GetStringValue(windowDictPtr, "kCGWindowName") ?? "";
            var layer = GetIntValue(windowDictPtr, "kCGWindowLayer");
            var isOnscreen = GetIntValue(windowDictPtr, "kCGWindowIsOnscreen") == 1;
            
            var boundsDict = GetDictionaryValue(windowDictPtr, "kCGWindowBounds");
            var x = boundsDict != IntPtr.Zero ? (int)GetFloatValue(boundsDict, "X") : 0;
            var y = boundsDict != IntPtr.Zero ? (int)GetFloatValue(boundsDict, "Y") : 0;
            var width = boundsDict != IntPtr.Zero ? (int)GetFloatValue(boundsDict, "Width") : 0;
            var height = boundsDict != IntPtr.Zero ? (int)GetFloatValue(boundsDict, "Height") : 0;

            return new WindowInfo
            {
                WindowId = windowId,
                ProcessId = pid,
                ApplicationName = ownerName,
                WindowTitle = windowName,
                Layer = layer,
                IsOnScreen = isOnscreen,
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to parse window info: {ex.Message}");
            return null;
        }
    }

    private static int GetIntValue(IntPtr dictPtr, string key)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var keyPtr = CFStringCreateWithCString(IntPtr.Zero, key, kCFStringEncodingUTF8);
        if (keyPtr == IntPtr.Zero) return 0;

        var valuePtr = CFDictionaryGetValue(dictPtr, keyPtr);
        CFRelease(keyPtr);

        if (valuePtr == IntPtr.Zero) return 0;

        int value = 0;
        CFNumberGetValue(valuePtr, kCFNumberIntType, ref value);
        return value;
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static float GetFloatValue(IntPtr dictPtr, string key)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var keyPtr = CFStringCreateWithCString(IntPtr.Zero, key, kCFStringEncodingUTF8);
        if (keyPtr == IntPtr.Zero) return 0;

        var valuePtr = CFDictionaryGetValue(dictPtr, keyPtr);
        CFRelease(keyPtr);

        if (valuePtr == IntPtr.Zero) return 0;

        float value = 0;
        CFNumberGetValue(valuePtr, kCFNumberFloatType, ref value);
        return value;
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static string? GetStringValue(IntPtr dictPtr, string key)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var keyPtr = CFStringCreateWithCString(IntPtr.Zero, key, kCFStringEncodingUTF8);
        if (keyPtr == IntPtr.Zero) return null;

        var valuePtr = CFDictionaryGetValue(dictPtr, keyPtr);
        CFRelease(keyPtr);

        if (valuePtr == IntPtr.Zero) return null;

        var length = CFStringGetLength(valuePtr);
        if (length == 0) return "";
        
        var range = new CFRange { location = 0, length = length };
        var bufferSize = CFStringGetMaximumSizeForEncoding(length, kCFStringEncodingUTF8) + 1;
        var buffer = new byte[bufferSize];
        
        if (CFStringGetBytes(valuePtr, range, kCFStringEncodingUTF8, 0, false, buffer, bufferSize, out var usedBytes))
        {
            return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)usedBytes);
        }

        return null;
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static IntPtr GetDictionaryValue(IntPtr dictPtr, string key)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var keyPtr = CFStringCreateWithCString(IntPtr.Zero, key, kCFStringEncodingUTF8);
        if (keyPtr == IntPtr.Zero) return IntPtr.Zero;

        var valuePtr = CFDictionaryGetValue(dictPtr, keyPtr);
        CFRelease(keyPtr);

        return valuePtr;
#pragma warning restore CA1416 // Validate platform compatibility
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CFRange
    {
        public long location;
        public long length;
    }
#endif

    /// <summary>
    /// Resizes an image if it exceeds the specified maximum dimension.
    /// Images are resized proportionally to fit within maxDimension x maxDimension.
    /// </summary>
    /// <param name="imagePath">Path to the image file to resize</param>
    /// <param name="maxDimension">Maximum width or height in pixels (default: 1200)</param>
    /// <returns>The image path (same as input), or null if resize failed</returns>
    private static string? ResizeImageIfNeeded(string imagePath, int maxDimension = 1200)
    {
        if (!File.Exists(imagePath))
        {
            Logger.Error($"Cannot resize image - file not found: {imagePath}");
            return imagePath;
        }

#if WINDOWS
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using var original = new Bitmap(imagePath);
            
            // Check if resize is needed
            if (original.Width <= maxDimension && original.Height <= maxDimension)
            {
                Logger.Verbose($"Image resize not needed - dimensions: {original.Width}x{original.Height}");
                return imagePath;
            }

            // Calculate new dimensions maintaining aspect ratio
            var scale = (double)maxDimension / Math.Max(original.Width, original.Height);
            var newWidth = (int)(original.Width * scale);
            var newHeight = (int)(original.Height * scale);

            Logger.Verbose($"Resizing image from {original.Width}x{original.Height} to {newWidth}x{newHeight}");

            // Create resized bitmap
            using var resized = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(resized);
            
            // Use high quality resizing
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            
            // Save over the original file
            resized.Save(imagePath, ImageFormat.Png);
            
            return imagePath;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to resize image: {ex.Message}");
            return imagePath; // Return original on failure
        }
#elif OSX
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            // Use sips to get current dimensions
            var getDimensionsInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/bin/sips",
                Arguments = $"-g pixelWidth -g pixelHeight \"{imagePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var getDims = System.Diagnostics.Process.Start(getDimensionsInfo);
            if (getDims == null)
            {
                Logger.Error("Failed to start sips process to get dimensions");
                return imagePath;
            }

            getDims.WaitForExit(5000);
            var output = getDims.StandardOutput.ReadToEnd();
            
            // Parse dimensions from output like "  pixelWidth: 2940\n  pixelHeight: 1912"
            var widthMatch = System.Text.RegularExpressions.Regex.Match(output, @"pixelWidth:\s*(\d+)");
            var heightMatch = System.Text.RegularExpressions.Regex.Match(output, @"pixelHeight:\s*(\d+)");
            
            if (widthMatch.Success && heightMatch.Success)
            {
                var width = int.Parse(widthMatch.Groups[1].Value);
                var height = int.Parse(heightMatch.Groups[1].Value);
                
                if (width <= maxDimension && height <= maxDimension)
                {
                    Logger.Verbose($"Image resize not needed - dimensions: {width}x{height}");
                    return imagePath;
                }

                Logger.Verbose($"Resizing image from {width}x{height} using sips");
            }

            // Resize using sips (-Z resizes proportionally to fit within maxDimension)
            var resizeInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/bin/sips",
                Arguments = $"-Z {maxDimension} \"{imagePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(resizeInfo);
            if (process == null)
            {
                Logger.Error("Failed to start sips process for resize");
                return imagePath;
            }

            process.WaitForExit(10000); // Allow up to 10 seconds for resize

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Logger.Error($"sips resize failed with exit code {process.ExitCode}: {error}");
                return imagePath;
            }

            Logger.Verbose($"Image resized successfully");
            return imagePath;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to resize image: {ex.Message}");
            return imagePath; // Return original on failure
        }
#else
        // On other platforms, return the original image without resizing
        Logger.Verbose("Image resizing not supported on this platform");
        return imagePath;
#endif
    }


    /// <summary>
    /// Gets a user-friendly error message for unsupported platforms.
    /// </summary>
    public static string GetPlatformErrorMessage()
    {
        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                       RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" :
                       RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Unknown";
        
        return $"Screenshot functionality is currently only available on Windows and macOS. Current platform: {platform}";
    }

    /// <summary>
    /// Gets a macOS-specific error message about screenshot failures.
    /// </summary>
    public static string GetMacOSPermissionErrorMessage()
    {
        return "Failed to capture screenshot.\n\n" +
               "This could be due to:\n" +
               "- Display settings or permissions issues\n" +
               "- The screencapture utility not being available\n" +
               "- Insufficient disk space in the temp directory\n\n" +
               "Please check the console for more detailed error information.";
    }

#region Windows Interop
#if WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;  // Width of the primary display monitor
    private const int SM_CYSCREEN = 1;  // Height of the primary display monitor
#pragma warning restore CA1416 // Validate platform compatibility
#endif
#endregion

#region macOS Interop
#if OSX
#pragma warning disable CA1416 // Validate platform compatibility
    // CoreGraphics - Window enumeration
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGWindowListCopyWindowInfo(uint options, uint relativeToWindow);

    // CoreFoundation - Array and dictionary handling
    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern long CFArrayGetCount(IntPtr theArray);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFArrayGetValueAtIndex(IntPtr theArray, long idx);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cf);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFStringCreateWithCString(IntPtr alloc, string str, uint encoding);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFDictionaryGetValue(IntPtr theDict, IntPtr key);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern bool CFNumberGetValue(IntPtr number, int theType, ref int valuePtr);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern bool CFNumberGetValue(IntPtr number, int theType, ref float valuePtr);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern long CFStringGetLength(IntPtr theString);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern long CFStringGetMaximumSizeForEncoding(long length, uint encoding);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern bool CFStringGetBytes(
        IntPtr theString, 
        CFRange range, 
        uint encoding, 
        byte lossByte, 
        bool isExternalRepresentation, 
        byte[] buffer, 
        long maxBufLen, 
        out long usedBufLen);

    // Constants
    private const uint kCFStringEncodingUTF8 = 0x08000100;
    private const uint kCGWindowListOptionOnScreenOnly = 1;
    private const uint kCGWindowListExcludeDesktopElements = 16;
    private const uint kCGNullWindowID = 0;
    private const int kCFNumberIntType = 9;
    private const int kCFNumberFloatType = 4;
#pragma warning restore CA1416 // Validate platform compatibility
#endif
#endregion
}
