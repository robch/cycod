# Console GUI Framework Developer Guide

This guide documents the Console GUI framework in CycoD, providing developers with information on how to use and extend the interactive console components.

## Overview

The Console GUI framework provides a set of reusable components for building interactive terminal user interfaces. It's organized into two main categories:

- **Core Components**: Low-level building blocks for console manipulation
- **Controls**: High-level interactive UI components

## Architecture

```
src/common/ConsoleGui/
  Core/                      # Foundation components
    Screen.cs                # Console screen management
    Window.cs                # Window rendering and borders
    Rect.cs                  # Rectangle utilities
    Cursor.cs                # Cursor positioning
  Controls/                  # Interactive controls
    ControlWindow.cs         # Base class for all controls
    ScrollingControl.cs      # Base for scrollable content
    VirtualListBoxControl.cs # Base for efficient list rendering
    ListBoxControl.cs        # Concrete list implementation
    ListBoxPicker.cs         # Interactive list picker ⭐
    SpeedSearchListBoxControl.cs # Type-to-filter search
    EditBoxControl.cs        # Text input control
    EditBoxQuickEdit.cs      # Quick modal text input
    TextViewerControl.cs     # Text viewing with column selection
    HelpViewer.cs            # Interactive help viewer
  InOutPipeServer.cs         # Testing infrastructure
```

## Core Components

### Screen.cs

Manages the console screen dimensions and provides buffer operations.

**Key Methods:**
- `GetBufferWidth()` / `GetBufferHeight()` - Get console buffer dimensions
- `GetWindowHeight()` / `GetWindowWidth()` - Get visible window size
- `Clear()` - Clear the screen
- `Flush()` - Flush output buffer

**Example:**
```csharp
var width = Screen.GetBufferWidth();
var height = Screen.GetBufferHeight();
Console.WriteLine($"Screen: {width}x{height}");
```

### Window.cs

Renders windows with borders and manages content display.

**Key Features:**
- Automatic border drawing (single, double, or none)
- Content clipping to window boundaries
- Color management
- Title support

**Example:**
```csharp
var window = new Window(
    new Rect(10, 5, 40, 15),  // x, y, width, height
    BorderKind.DoubleLine,
    ConsoleColor.White,
    ConsoleColor.Blue
);
window.WriteAt(2, 2, "Hello, Window!");
window.Display();
```

### Rect.cs

Utility class for rectangle operations.

**Key Properties:**
- `X`, `Y` - Top-left corner
- `Width`, `Height` - Dimensions
- `Left`, `Right`, `Top`, `Bottom` - Boundaries

**Example:**
```csharp
var rect = new Rect(10, 5, 30, 10);
var center = rect.Center;  // Point(25, 10)
var area = rect.Width * rect.Height;  // 300
```

### Cursor.cs

Manages cursor positioning and appearance.

**Key Methods:**
- `GetPosition()` - Get current cursor position
- `SetPosition(x, y)` - Move cursor
- `SetShape(shape)` - Change cursor appearance
- `SavePosition()` / `RestorePosition()` - Stack-based cursor management

**Example:**
```csharp
var (x, y) = Cursor.GetPosition();
Cursor.SavePosition();
Cursor.SetPosition(20, 10);
Console.Write("Text at (20, 10)");
Cursor.RestorePosition();
```

## Interactive Controls

### ListBoxPicker

The most commonly used control - an interactive list picker with keyboard navigation.

**Key Methods:**
- `PickIndexOf(items, title, preselected)` - Pick an item index
- `PickString(items, title, preselected)` - Pick an item string

**Features:**
- Arrow key navigation (Up, Down, Page Up, Page Down, Home, End)
- Type-to-filter search (inherited from SpeedSearchListBoxControl)
- Enter to select, Escape to cancel
- Automatic sizing and centering
- Custom colors and borders

**Example:**
```csharp
var options = new[] { "Option 1", "Option 2", "Option 3" };
var selectedIndex = ListBoxPicker.PickIndexOf(
    options,
    "Choose an option",
    preselectedIndex: 0
);

if (selectedIndex >= 0)
{
    Console.WriteLine($"Selected: {options[selectedIndex]}");
}
else
{
    Console.WriteLine("Cancelled");
}
```

**Real-world Usage:**
See `src/cycod/CommandLineCommands/ChatCommand.cs` for the context menu implementation:
```csharp
var menuItems = new[] { 
    "Continue chatting", 
    UseSpeechInput ? "Speech input" : "Continue chatting",
    "Reset conversation", 
    "Exit" 
};
var selection = ListBoxPicker.PickIndexOf(menuItems, "What would you like to do?");
```

### SpeedSearchListBoxControl

Extends list boxes with type-to-filter search functionality.

**Search Strategies:**
- **Starts-with**: Type characters to filter items starting with that text
- **Contains**: Find items containing the search text anywhere
- **Regex**: Use regular expressions for advanced filtering
- **Char-sequence**: Find items containing characters in order (e.g., "abc" matches "a quick brown cat")

**Search Activation:**
- Press `?` to activate search mode
- Press `Ctrl+F` to activate search mode
- Just start typing to filter

**Navigation:**
- `Tab` / `Shift+Tab` to cycle through matches
- Type more characters to refine search
- `Escape` to clear search

**Note:** `ListBoxPicker` automatically includes this functionality.

### EditBoxControl

A full-featured text input control with editing capabilities.

**Key Features:**
- Text navigation: Home, End, Left, Right, Ctrl+Left, Ctrl+Right
- Editing: Insert/overwrite mode, Backspace, Delete
- Input validation with picture formats
- Cursor management with shape changes
- Horizontal scrolling for long text

**Picture Formats:**
- `@#` - Numeric digits only
- `@A` - Alphabetic characters only
- Custom validation with regex patterns

**Example:**
```csharp
var editBox = new EditBoxControl(
    new Rect(10, 10, 40, 1),
    "Initial text",
    pictureFormat: "@#####",  // 5 digits only
    fgColor: ConsoleColor.White,
    bgColor: ConsoleColor.Blue
);

editBox.Display();
var key = Console.ReadKey(true);
while (!editBox.ProcessKey(key))
{
    editBox.Display();
    key = Console.ReadKey(true);
}

var result = editBox.Text;
```

### EditBoxQuickEdit

A simplified modal text input dialog.

**Key Method:**
- `Edit(text, title, format)` - Show modal edit dialog

**Returns:**
- The edited text on Enter
- `null` on Escape (cancelled)

**Example:**
```csharp
var name = EditBoxQuickEdit.Edit(
    "John Doe",
    "Enter your name",
    pictureFormat: null  // No restrictions
);

if (name != null)
{
    Console.WriteLine($"Hello, {name}!");
}
```

### TextViewerControl

A text viewer with column selection and syntax highlighting.

**Key Features:**
- Vertical scrolling with arrow keys, Page Up/Down, Home/End
- Horizontal navigation with Left/Right arrows for column selection
- Syntax highlighting with backtick markers
- Search functionality (inherited from SpeedSearchListBoxControl)
- Modal display with Enter to select, Escape to cancel

**Example:**
```csharp
var lines = new[] {
    "Line 1: This is some text",
    "Line 2: With `highlighted` words",
    "Line 3: More content here"
};

var (row, col, width) = TextViewerControl.Display(
    lines,
    "View Text",
    allowColumnSelection: true
);

if (row >= 0)
{
    Console.WriteLine($"Selected: Line {row}, Column {col}, Width {width}");
}
```

**Syntax Highlighting:**
Text between backticks `` `like this` `` is automatically highlighted with different colors.

### HelpViewer

An interactive help viewer with command execution and URL launching.

**Key Features:**
- Interactive help links with `>` prefix
- URL detection and browser launching
- "Try it" command execution
- Custom key bindings (Ctrl+H, Tab, F3)
- Two display modes: single help text or multiple topics

**Example:**
```csharp
var helpText = new[] {
    "Welcome to CycoD Help",
    "",
    "> Click here to try: cycod --help",
    "Visit: https://github.com/robch/cycod",
    "",
    "Press Ctrl+H to see more options"
};

HelpViewer.DisplayHelpText(
    helpText,
    "CycoD Help",
    onTryItCallback: (command) => {
        Console.WriteLine($"Executing: {command}");
        // Execute the command
    }
);
```

**Interactive Help Links:**
Lines starting with `>` are treated as executable commands. When selected:
1. The command is extracted
2. The `onTryItCallback` is invoked
3. The command can be executed in the application

**URL Launching:**
URLs starting with `http://` or `https://` are automatically detected. When selected, they open in the default browser.

## Testing Infrastructure

### InOutPipeServer

Provides automated testing support for interactive controls via JSON protocol.

**Key Methods:**
- `GetInputFromUser(prompt)` - Get input via pipe or console
- `GetSelectionFromUser(prompt, choices)` - Get selection via pipe or console

**Environment Variable:**
- `CYCOD_IN_OUT_PIPE_SERVER` - Set to pipe name to enable pipe mode

**Example:**
```csharp
var input = InOutPipeServer.GetInputFromUser("Enter value:");
var selection = InOutPipeServer.GetSelectionFromUser(
    "Choose option:",
    new[] { "Option 1", "Option 2", "Option 3" }
);
```

## Design Patterns

### Virtual Scrolling

`VirtualListBoxControl` implements virtual scrolling for efficient rendering of large lists:

```csharp
// Only visible items are rendered
protected abstract int GetItemCount();
protected abstract string GetItemText(int index);

// Rendering happens only for items in view
for (int i = firstVisibleIndex; i < lastVisibleIndex; i++)
{
    var text = GetItemText(i);
    RenderLine(i, text);
}
```

### Inheritance Hierarchy

```
Window
  └─ ControlWindow
       ├─ ScrollingControl
       │    └─ VirtualListBoxControl
       │         ├─ ListBoxControl
       │         └─ SpeedSearchListBoxControl
       │              ├─ ListBoxPicker
       │              └─ TextViewerControl
       │                   └─ HelpViewer
       └─ EditBoxControl
            └─ EditBoxQuickEdit
```

### Control Lifecycle

1. **Construction**: Initialize control with position, size, colors
2. **Display**: Render control to console
3. **Event Loop**: Process keyboard input with `ProcessKey()`
4. **Completion**: Return result or null on cancellation

Example pattern:
```csharp
var control = new SomeControl(rect, options);
control.Display();

var key = Console.ReadKey(true);
while (!control.ProcessKey(key))
{
    control.Display();
    key = Console.ReadKey(true);
}

var result = control.GetResult();
```

## Best Practices

### 1. Use Static Helper Methods

For simple use cases, prefer static helper methods like `ListBoxPicker.PickIndexOf()` over manually managing control lifecycle.

✅ Good:
```csharp
var index = ListBoxPicker.PickIndexOf(items, "Choose:");
```

❌ Avoid:
```csharp
var control = new ListBoxControl(...);
control.Display();
// Manual event loop...
```

### 2. Respect Console State

Always preserve and restore console state when using controls:

```csharp
var originalCursorVisible = Console.CursorVisible;
try
{
    Console.CursorVisible = false;
    var result = ListBoxPicker.PickIndexOf(items, "Choose:");
}
finally
{
    Console.CursorVisible = originalCursorVisible;
}
```

### 3. Handle Cancellation

Always check for cancellation (Escape key) in interactive operations:

```csharp
var selection = ListBoxPicker.PickIndexOf(items, "Choose:");
if (selection < 0)
{
    // User cancelled - handle gracefully
    return;
}
```

### 4. Use Appropriate Borders

Choose border styles based on context:
- `BorderKind.DoubleLine` - Important dialogs, primary UI
- `BorderKind.SingleLine` - Secondary UI, nested windows
- `BorderKind.None` - Minimal UI, embedded content

### 5. Consider Screen Size

Always check console dimensions before creating large windows:

```csharp
var maxWidth = Screen.GetBufferWidth() - 4;  // Leave margin
var maxHeight = Screen.GetBufferHeight() - 4;

var width = Math.Min(desiredWidth, maxWidth);
var height = Math.Min(desiredHeight, maxHeight);

var rect = new Rect(2, 2, width, height);
```

## Platform Considerations

### Windows
- Full support for all features
- Console API provides precise control
- Borders render correctly with box-drawing characters

### macOS / Linux
- Core functionality works via ANSI escape codes
- Some features may degrade gracefully:
  - Buffer manipulation uses simpler approaches
  - Cursor shape changes may not work in all terminals
- Test in target terminal emulator

### Console Redirection
- Framework detects when output is redirected
- Automatically disables interactive features
- Falls back to text-based input/output

## Testing

### Unit Tests

Component tests are located in `tests/` directory:
- `tests/RectTests.cs` - Rectangle utilities
- `tests/ScreenTests.cs` - Screen operations
- `tests/WindowTests.cs` - Window rendering
- `tests/ControlWindowTests.cs` - Base control
- `tests/ListBoxPickerTests.cs` - Interactive list picker
- And more...

Run tests with:
```bash
dotnet test
```

### YAML Integration Tests

See `tests/cycodt-yaml/console-gui-tests.yaml` for integration tests:
```bash
cycodt run --file tests/cycodt-yaml/console-gui-tests.yaml
```

### Manual Testing

For interactive features, use the manual test mode in tests:
```bash
dotnet run --project tests/ListBoxPickerTests.csproj -- manual
```

## Examples from the Codebase

### Context Menu in ChatCommand

Location: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
private int ShowContextMenu()
{
    var menuItems = new List<string>
    {
        "Continue chatting",
        UseSpeechInput ? "Speech input" : null,
        "Reset conversation",
        "Exit"
    }.Where(x => x != null).ToArray();

    var selection = ListBoxPicker.PickIndexOf(
        menuItems,
        "What would you like to do?"
    );

    return selection;
}
```

## Contributing

When adding new console GUI components:

1. **Location**: Place in `src/common/ConsoleGui/Controls/`
2. **Namespace**: Use `ConsoleGui.Controls`
3. **Inheritance**: Extend appropriate base class
4. **Tests**: Create comprehensive unit tests
5. **Documentation**: Update this guide with examples
6. **Commit**: Follow the established pattern

## Related Documentation

- [Getting Started](getting-started.md) - User-facing interactive features
- [Speech Setup](speech-setup.md) - Voice input configuration
- [CHANGELOG.md](../CHANGELOG.md) - Feature history

## Support

For issues or questions about the Console GUI framework:
1. Check existing tests for usage examples
2. Review this documentation
3. Open an issue on GitHub

---

**Version**: 0.6.0  
**Last Updated**: 2025-01-05
