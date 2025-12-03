# Screenshot Feature Implementation Plan

**Branch**: `robch/2512-dec03-add-screenshot-tool-and-slash-command`

## Overview

Add screenshot capability to cycod that:
- Takes a screenshot of the primary screen
- Automatically adds it to the conversation
- Available as both an AI tool and a `/screenshot` slash command
- Works on Windows (gracefully fails on macOS without crashing)

## Architecture

The implementation follows existing patterns in the codebase:

1. **Screenshot Capture Service** - Platform-specific screenshot implementation
2. **AI Tool** (ScreenshotHelperFunctions) - Allows AI to request screenshots
3. **Slash Command** (SlashScreenshotCommandHandler) - User-initiated screenshots via `/screenshot`

## Files to Create

### 1. `src/cycod/Helpers/ScreenshotHelper.cs`
Screenshot capture service with platform detection:
- `TakeScreenshot()` - Captures primary screen on Windows
- Returns temp file path with timestamped filename
- Platform check: Windows only, graceful error on macOS
- Uses `System.Drawing.Common` (Bitmap, Graphics.CopyFromScreen)

### 2. `src/cycod/FunctionCallingTools/ScreenshotHelperFunctions.cs`
AI tool for screenshot capture:
- Method: `TakeScreenshot()` with `[Description]` attribute
- Returns `DataContent` with screenshot bytes (like ImageHelperFunctions)
- Platform-aware: returns error message on non-Windows platforms
- Reuses ScreenshotHelper for actual capture

### 3. `src/cycod/SlashCommands/SlashScreenshotCommandHandler.cs`
Slash command handler for `/screenshot`:
- Implements `IAsyncSlashCommandHandler` (file I/O)
- Captures screenshot and adds to ChatCommand.ImagePatterns
- Displays confirmation message with file path
- Returns `SlashCommandResult.Success()`

## Files to Modify

### 1. `src/cycod/cycod.csproj`
Add NuGet package reference:
```xml
<PackageReference Include="System.Drawing.Common" Version="8.0.0" />
```

### 2. `src/cycod/CommandLineCommands/ChatCommand.cs`
Register new tool and slash command in `ExecuteAsync()`:
```csharp
// Around line 109 - Add screenshot tool
factory.AddFunctions(new ScreenshotHelperFunctions());

// Around line 68 - Add screenshot slash command
_slashCommandRouter.Register(new SlashScreenshotCommandHandler(this));
```

### 3. `src/cycod/CommandLineCommands/ChatCommand.cs` (Help)
Update `/help` command output to include `/screenshot` (around line 432):
```csharp
helpBuilder.AppendLine("  /screenshot  Take screenshot and add to conversation");
```

## Implementation Details

### Screenshot Capture (Windows)
```csharp
// Get primary screen bounds
var bounds = Screen.PrimaryScreen.Bounds;
var bitmap = new Bitmap(bounds.Width, bounds.Height);

// Capture screen
using var graphics = Graphics.FromImage(bitmap);
graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);

// Save to temp file
var fileName = Path.Combine(Path.GetTempPath(), $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss}.png");
bitmap.Save(fileName, ImageFormat.Png);
```

### Platform Detection
```csharp
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    return "Screenshot functionality is only available on Windows";
}
```

### Image Addition Flow
Both the AI tool and slash command use the same approach:
1. Capture screenshot to temp file
2. AI Tool: Return `DataContent` directly for immediate inclusion
3. Slash Command: Add to `ChatCommand.ImagePatterns` for next message

## Testing Strategy

### Manual Testing
1. **Windows**: Run `cycod` and test:
   - `/screenshot` command captures and adds to conversation
   - AI can call `TakeScreenshot()` tool
   - Image appears in conversation

2. **macOS**: Run `cycod` and verify:
   - `/screenshot` returns error message without crash
   - AI tool returns error message without crash

### Test Scenarios
- Screenshot capture in interactive mode
- Screenshot via slash command
- Screenshot via AI tool invocation
- Error handling on non-Windows platforms
- Multiple screenshots in same conversation

## Dependencies

### NuGet Packages
- `System.Drawing.Common` v8.0.0 (for Bitmap, Graphics, Screen classes)

### Platform Requirements
- Windows: Full functionality (uses GDI+ via System.Drawing)
- macOS: Graceful degradation (error message, no crash)
- Linux: Graceful degradation (same as macOS)

## Implementation Steps

1. ✅ Create branch and worktree
2. ⬜ Add System.Drawing.Common package to cycod.csproj
3. ⬜ Create ScreenshotHelper.cs with platform-aware capture
4. ⬜ Create ScreenshotHelperFunctions.cs (AI tool)
5. ⬜ Create SlashScreenshotCommandHandler.cs (slash command)
6. ⬜ Register tool in ChatCommand.cs
7. ⬜ Register slash command in ChatCommand.cs
8. ⬜ Update help text in ChatCommand.cs
9. ⬜ Build and test on Windows
10. ⬜ Verify graceful failure on macOS (if available)
11. ⬜ Update CHANGELOG.md
12. ⬜ Create PR

## Future Enhancements

- Support for capturing specific windows or regions
- macOS support using platform-specific APIs
- Linux support using X11/Wayland
- Screenshot annotation before adding to conversation
- Option to save screenshots to custom location
- Automatic clipboard copy

## References

- Macaroni WindowsScreenCaptureService: `c:\src\macaroni\src\win32_ui\PlatformServices\WindowsScreenCaptureService.cs`
- Existing image tool pattern: `src/cycod/FunctionCallingTools/ImageHelperFunctions.cs`
- Slash command pattern: `src/cycod/SlashCommands/SlashCycoDmdCommandHandler.cs`
