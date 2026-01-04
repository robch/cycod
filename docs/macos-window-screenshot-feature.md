# macOS Window-Specific Screenshot Feature

## Summary

Added the ability to capture screenshots of specific windows and applications on macOS, in addition to the existing full-screen capture functionality.

## New Features

### For LLM/AI Tools:

1. **TakeScreenshotOfWindowWithTitle(title)** - Capture window by title (partial match)
   - Example: `TakeScreenshotOfWindowWithTitle("cycod")`
   - Returns screenshot or helpful error if multiple matches

2. **TakeScreenshotOfApp(appName)** - Capture window by application name
   - Example: `TakeScreenshotOfApp("Warp")`
   - Returns screenshot or helpful error if multiple matches

3. **TakeScreenshotOfDisplay(displayNumber)** - Capture specific display
   - Example: `TakeScreenshotOfDisplay(2)` for secondary monitor

4. **ListWindows()** - Enumerate all visible windows
   - Returns JSON array with window metadata
   - Useful for exploration before capturing

5. **TakeScreenshotOfWindow(windowId)** - Advanced: capture by window ID
   - Requires calling ListWindows() first to get IDs

### Implementation Details:

- **Platform**: macOS only (guarded with `#if OSX`)
- **Method**: Hybrid approach:
  - Uses Core Graphics `CGWindowListCopyWindowInfo` for window enumeration (no permissions required)
  - Uses `screencapture -l <windowid>` for actual capture (no permissions required)
- **Matching**: Case-insensitive, partial matching for app names and titles
- **Filtering**: Only shows normal application windows (layer 0) with titles
- **Error Handling**: Returns helpful messages when multiple windows match

## Files Modified:

1. **src/cycod/Helpers/WindowInfo.cs** (NEW)
   - Data class for window metadata
   - Contains: WindowId, ProcessId, ApplicationName, WindowTitle, Bounds, etc.

2. **src/cycod/Helpers/ScreenshotHelper.cs** (MODIFIED)
   - Added window enumeration methods
   - Added new screenshot capture methods
   - Added Core Foundation P/Invoke for window APIs

3. **src/cycod/FunctionCallingTools/ScreenshotHelperFunctions.cs** (MODIFIED)
   - Added AI tool wrappers for new methods
   - Returns screenshots as DataContent or error messages
   - Returns window list as JSON string

## Usage Examples (from LLM perspective):

```
User: "Take a screenshot of my Warp terminal"
AI: TakeScreenshotOfApp("Warp")

User: "Take a screenshot of the window with 'cycod' in the title"
AI: TakeScreenshotOfWindowWithTitle("cycod")

User: "What windows are open?"
AI: ListWindows()

User: "Take a screenshot of my second monitor"
AI: TakeScreenshotOfDisplay(2)
```

## Benefits:

- ✅ No Screen Recording permissions required
- ✅ LLM can target specific windows easily
- ✅ Simple one-call operations for common use cases
- ✅ Advanced APIs available for power users
- ✅ Helpful error messages when multiple matches exist
- ✅ Clean, maintainable code with proper platform guards

## Testing:

Build succeeded with 0 warnings, 0 errors.

To test manually:
1. Run: `./src/cycod/bin/Debug/net9.0/cycod chat`
2. Ask the AI to list windows: "List all windows"
3. Ask the AI to screenshot specific app: "Take a screenshot of Warp"
