#if OSX
/// <summary>
/// Represents metadata about a window on macOS.
/// </summary>
public class WindowInfo
{
    /// <summary>
    /// Unique window identifier.
    /// </summary>
    public int WindowId { get; set; }

    /// <summary>
    /// Process ID of the application that owns this window.
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Name of the application that owns this window (e.g., "Warp", "Code", "Microsoft Edge").
    /// </summary>
    public string ApplicationName { get; set; } = "";

    /// <summary>
    /// Title of the window (may be empty for some windows).
    /// </summary>
    public string WindowTitle { get; set; } = "";

    /// <summary>
    /// X coordinate of the window's top-left corner.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y coordinate of the window's top-left corner.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Width of the window in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the window in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Whether the window is currently visible on screen.
    /// </summary>
    public bool IsOnScreen { get; set; }

    /// <summary>
    /// Window layer (0 = normal application windows, higher values = UI elements).
    /// </summary>
    public int Layer { get; set; }

    public override string ToString()
    {
        var title = string.IsNullOrEmpty(WindowTitle) ? "(no title)" : WindowTitle;
        return $"Window #{WindowId}: {ApplicationName} - {title} [{Width}x{Height}]";
    }
}
#endif
