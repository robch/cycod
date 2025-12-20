#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif
using System.Runtime.InteropServices;
using System.Text;

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

            return fileName;
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

        IntPtr image = IntPtr.Zero;
        IntPtr url = IntPtr.Zero;
        IntPtr pngType = IntPtr.Zero;
        IntPtr destination = IntPtr.Zero;
        
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            // Capture the main display using Core Graphics
            image = CGDisplayCreateImage(kCGDisplayMainID);
            if (image == IntPtr.Zero)
            {
                Logger.Error("Failed to capture screen image");
                return null;
            }
            
            // Create output file path
            var fileName = Path.Combine(
                Path.GetTempPath(), 
                $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            
            // Convert file path to CFURL
            var pathBytes = Encoding.UTF8.GetBytes(fileName);
            url = CFURLCreateFromFileSystemRepresentation(
                IntPtr.Zero, 
                pathBytes, 
                pathBytes.Length, 
                false);
            
            if (url == IntPtr.Zero)
            {
                Logger.Error("Failed to create URL for screenshot path");
                return null;
            }
            
            // Create CFString for PNG type identifier
            // Using UTI (Uniform Type Identifier) for PNG: "public.png"
            pngType = CFStringCreateWithCString(
                IntPtr.Zero, 
                "public.png", 
                kCFStringEncodingUTF8);
            
            if (pngType == IntPtr.Zero)
            {
                Logger.Error("Failed to create PNG type identifier");
                return null;
            }
            
            // Create image destination for writing PNG
            destination = CGImageDestinationCreateWithURL(
                url, 
                pngType, 
                1, // 1 image
                IntPtr.Zero); // No options
            
            if (destination == IntPtr.Zero)
            {
                Logger.Error("Failed to create image destination");
                return null;
            }
            
            // Add the captured image to the destination
            CGImageDestinationAddImage(destination, image, IntPtr.Zero);
            
            // Finalize and write to disk
            if (!CGImageDestinationFinalize(destination))
            {
                Logger.Error("Failed to finalize and write screenshot");
                return null;
            }
            
            return fileName;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to capture screenshot: {ex.Message}");
            return null;
        }
        finally
        {
            // Critical: Release all Core Foundation objects to prevent memory leaks
            if (image != IntPtr.Zero) CFRelease(image);
            if (url != IntPtr.Zero) CFRelease(url);
            if (pngType != IntPtr.Zero) CFRelease(pngType);
            if (destination != IntPtr.Zero) CFRelease(destination);
        }
#else
        return null;
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
    // CoreGraphics - Screen capture
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGDisplayCreateImage(uint displayID);

    // CoreFoundation - Memory management and URL handling
    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cf);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFURLCreateFromFileSystemRepresentation(
        IntPtr allocator, 
        byte[] buffer, 
        long bufLen, 
        bool isDirectory);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFStringCreateWithCString(
        IntPtr alloc,
        string str,
        uint encoding);

    // ImageIO - PNG writing
    [DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
    private static extern IntPtr CGImageDestinationCreateWithURL(
        IntPtr url, 
        IntPtr type, 
        nuint count, 
        IntPtr options);

    [DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
    private static extern void CGImageDestinationAddImage(
        IntPtr idst, 
        IntPtr image, 
        IntPtr properties);

    [DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
    private static extern bool CGImageDestinationFinalize(IntPtr idst);

    // Constants
    private const uint kCFStringEncodingUTF8 = 0x08000100;
    private const uint kCGDisplayMainID = 0; // Main display
#pragma warning restore CA1416 // Validate platform compatibility
#endif
#endregion
}
