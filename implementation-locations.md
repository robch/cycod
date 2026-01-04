# Implementation Locations for Console GUI and Speech Features

**Worktree**: /c/src/cycod-console-gui  
**Branch**: robch/2501-jan03-console-gui-components  
**Date**: 2025-01-04

This document specifies exactly where in the cycod codebase to implement the console GUI components and speech recognition features.

---

## 1. Console GUI Components

### Target Location: `src/common/ConsoleGui/`

Create a new directory structure under `src/common/` to house all console GUI components. This mirrors the AI CLI structure and keeps the components available to all cycod tools.

```
src/common/
  ConsoleGui/                          ← NEW DIRECTORY
    Core/                              ← Foundation classes
      Screen.cs
      Window.cs
      Rect.cs
      Cursor.cs
      Colors.cs                        ← Already exists in Helpers, may need to extend
    Controls/                          ← Interactive controls
      ControlWindow.cs
      ScrollingControl.cs
      VirtualListBoxControl.cs
      ListBoxControl.cs
      ListBoxPicker.cs                 ← MOST IMPORTANT - the picker
      SpeedSearchListBoxControl.cs
      EditBoxControl.cs
      EditBoxQuickEdit.cs
      TextViewerControl.cs
      HelpViewer.cs
    InOutPipeServer.cs                 ← For testing support
    README.md                          ← Documentation for the GUI system
```

### Why `src/common/ConsoleGui/`?

1. **Reusable across all tools**: cycod, cycodmd, cycodt, cycodgr, cycodj can all use it
2. **Consistent with existing structure**: Other helpers are in `src/common/Helpers/`
3. **Natural namespace**: `Cycod.Common.ConsoleGui` or just `ConsoleGui`
4. **Easy to find**: Developers expect shared code in `common/`

### Integration with Existing Code

**Colors.cs**: Already exists in `src/common/Helpers/Colors.cs`
- Current: Simple color enum utilities
- Extend: Add `Colors` class for normal/selected color pairs
- Or: Keep separate `ConsoleGui.Colors` to avoid conflicts

**ConsoleHelpers.cs**: Already exists in `src/common/Helpers/ConsoleHelpers.cs`
- Add method: `public static T? PickFromList<T>(List<T> items, Func<T, string> display)`
- Wraps ListBoxPicker for easy use

---

## 2. Speech Recognition for Chat Command

### Target Locations

#### A. Speech Configuration & Helpers
**Location**: `src/cycod/Helpers/SpeechHelpers.cs` ← NEW FILE

```csharp
namespace Cycod.Helpers;

public static class SpeechHelpers
{
    public static SpeechConfig? CreateSpeechConfig()
    {
        // Load from .cycod/speech.key and .cycod/speech.region
        // Return null if files don't exist (graceful fallback)
    }
    
    public static bool IsSpeechConfigured()
    {
        // Check if speech.key and speech.region exist
    }
    
    public static async Task<string?> GetSpeechInputAsync(Action<string>? onInterimResult = null)
    {
        // Main speech recognition implementation
        // Shows "(listening)..." and interim results
        // Returns final recognized text
    }
}
```

**Why**: Separates speech logic from command logic, making it reusable if other commands want speech input.

#### B. Chat Command Integration
**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs` ← MODIFY EXISTING

**Line ~159**: Modify `InteractivelyReadLineOrSimulateInput` method:

```csharp
private string? InteractivelyReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
{
    var input = ReadLineOrSimulateInput(inputInstructions, null);
    if (input != null) return input;

    // NEW: Check if user pressed ENTER on empty line and --speech is enabled
    if (_enableSpeech && string.IsNullOrEmpty(Console.ReadLine()))
    {
        var choice = ShowContextMenu();
        if (choice == "speech")
        {
            return await SpeechHelpers.GetSpeechInputAsync();
        }
        else if (choice == "exit")
        {
            return "exit";
        }
        // ... handle other menu options
    }

    // Existing multiline input handling...
    string? line = Console.ReadLine();
    if (line == null) return defaultOnEndOfInput;

    var isMultiLine = MultilineInputHelper.StartsWithBackticks(line);
    return isMultiLine ? InteractivelyReadMultiLineInput(line) : line;
}

private string? ShowContextMenu()
{
    // Use ListBoxPicker to show context menu
    var choices = _enableSpeech
        ? new[] { "speech", "---", "reset conversation", "exit" }
        : new[] { "reset conversation", "exit" };
    
    return ListBoxPicker.PickString(choices, 
        new Colors(ConsoleColor.White, ConsoleColor.Blue),
        new Colors(ConsoleColor.White, ConsoleColor.Red));
}
```

**OR Better Pattern**: Check for empty input before calling ReadLine:

```csharp
private string? InteractivelyReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
{
    var input = ReadLineOrSimulateInput(inputInstructions, null);
    if (input != null) return input;

    // NEW: Show prompt and wait for input
    var key = Console.ReadKey(intercept: true);
    
    // If user presses ENTER on empty line, show context menu
    if (key.Key == ConsoleKey.Enter)
    {
        var choice = ShowContextMenu();
        if (choice == "speech")
        {
            return await SpeechHelpers.GetSpeechInputAsync();
        }
        else if (choice == "exit")
        {
            return "exit";
        }
        else if (choice == "reset")
        {
            return "/reset"; // or handle directly
        }
        // If null or other, show prompt again
        return InteractivelyReadLineOrSimulateInput(inputInstructions, defaultOnEndOfInput);
    }
    
    // Not ENTER, so build the line with the first key pressed
    var line = BuildLineFromKeyPress(key);
    
    var isMultiLine = MultilineInputHelper.StartsWithBackticks(line);
    return isMultiLine ? InteractivelyReadMultiLineInput(line) : line;
}
```

#### C. Command Line Parsing
**Location**: `src/cycod/CommandLine/ChatCommandLineOptions.cs` ← MODIFY EXISTING

Add `--speech` flag:

```csharp
[Option("speech", Required = false, HelpText = "Enable speech recognition input in interactive mode.")]
public bool EnableSpeech { get; set; }
```

Then in ChatCommand constructor or setup, read this flag:
```csharp
private bool _enableSpeech;

public ChatCommand()
{
    // Set from command line options
    _enableSpeech = /* get from parsed options */;
}
```

---

## 3. Configuration Files for Speech

### Location: `.cycod/speech.key` and `.cycod/speech.region`

These go in the user's `.cycod` config directory (user scope).

**Helper method location**: `src/common/Helpers/ConfigFileHelpers.cs` or new `ScopeFileHelpers.cs`

```csharp
public static string? GetSpeechKey()
{
    return ScopeFileHelpers.ReadFirstFileInScopes("speech.key");
}

public static string? GetSpeechRegion()
{
    return ScopeFileHelpers.ReadFirstFileInScopes("speech.region");
}
```

**User setup commands**:
```bash
cycod config @speech.key --set YOUR_KEY
cycod config @speech.region --set westus2
```

---

## 4. Dependencies

### Add to cycod.csproj

**Location**: `src/cycod/cycod.csproj`

```xml
<ItemGroup>
  <!-- Existing packages... -->
  
  <!-- NEW: Speech recognition -->
  <PackageReference Include="Microsoft.CognitiveServices.Speech" Version="1.38.0" />
</ItemGroup>
```

---

## 5. Testing

### Unit Tests
**Location**: Create new test files

```
tests/
  cycod-tests/                         ← May need to create
    ConsoleGui/
      ListBoxPickerTests.cs
      ScreenTests.cs
    Speech/
      SpeechHelpersTests.cs
```

### Integration Tests with cycodt
**Location**: `tests/cycodt-yaml/chat-speech-tests.yaml` ← NEW FILE

```yaml
test-name: "Chat with speech recognition"
command: cycod chat --speech
inputs:
  - "\n"                              # Press ENTER to trigger menu
  - "speech\n"                        # Select speech option (in test, simulate)
  - "What is the weather?\n"
expect-regex:
  - "speech"
  - "reset conversation"
  - "exit"
```

---

## 6. Documentation

### User-Facing Documentation
**Location**: `docs/chat-speech-recognition.md` ← NEW FILE

Content:
- How to set up Azure Speech Service
- How to configure speech.key and speech.region
- How to use --speech flag
- Troubleshooting tips

### Developer Documentation
**Location**: `src/common/ConsoleGui/README.md` ← NEW FILE

Content:
- Architecture of console GUI system
- How to use ListBoxPicker
- How to create custom controls
- Examples

---

## 7. Implementation Order

### Phase 1: Core Console GUI (Week 1)
1. ✅ Create `src/common/ConsoleGui/Core/` directory
2. ✅ Port Screen.cs, Window.cs, Rect.cs, Cursor.cs
3. ✅ Port or extend Colors.cs
4. ✅ Write basic unit tests
5. ✅ Verify compilation on Windows/macOS/Linux

### Phase 2: ListBoxPicker (Week 1-2)
1. ✅ Port base control classes to `src/common/ConsoleGui/Controls/`
2. ✅ Port ListBoxControl.cs and ListBoxPicker.cs
3. ✅ Create simple demo app to test picker
4. ✅ Add helper method to ConsoleHelpers

### Phase 3: Chat Context Menu (Week 2)
1. ✅ Modify ChatCommand.cs to show context menu on ENTER
2. ✅ Add "reset conversation" and "exit" options
3. ✅ Test interactive flow
4. ✅ Update help documentation

### Phase 4: Speech Recognition (Week 2-3)
1. ✅ Add Microsoft.CognitiveServices.Speech dependency
2. ✅ Create `src/cycod/Helpers/SpeechHelpers.cs`
3. ✅ Implement CreateSpeechConfig and GetSpeechInputAsync
4. ✅ Add "speech" option to context menu (when --speech flag set)
5. ✅ Test speech input flow
6. ✅ Add configuration setup documentation

### Phase 5: Polish & Testing (Week 3)
1. ✅ Add error handling for missing speech config
2. ✅ Add cycodt YAML tests
3. ✅ Test on Windows, macOS, Linux
4. ✅ Update CHANGELOG.md
5. ✅ Create PR with comprehensive description

---

## 8. File Summary

### New Files to Create
```
src/common/ConsoleGui/
  Core/
    Screen.cs
    Window.cs
    Rect.cs
    Cursor.cs
    Colors.cs (or extend existing)
  Controls/
    ControlWindow.cs
    ScrollingControl.cs
    VirtualListBoxControl.cs
    ListBoxControl.cs
    ListBoxPicker.cs           ← HIGHEST PRIORITY
    SpeedSearchListBoxControl.cs
    EditBoxControl.cs
    EditBoxQuickEdit.cs
    TextViewerControl.cs
    HelpViewer.cs
  InOutPipeServer.cs
  README.md

src/cycod/Helpers/
  SpeechHelpers.cs             ← NEW

docs/
  chat-speech-recognition.md   ← NEW
  console-gui-components.md    ← NEW

tests/cycodt-yaml/
  chat-context-menu-tests.yaml ← NEW
  chat-speech-tests.yaml       ← NEW
```

### Files to Modify
```
src/cycod/cycod.csproj                              ← Add Speech SDK dependency
src/cycod/CommandLineCommands/ChatCommand.cs        ← Add context menu & speech
src/cycod/CommandLine/ChatCommandLineOptions.cs     ← Add --speech flag
src/common/Helpers/ConsoleHelpers.cs                ← Add PickFromList helper
CHANGELOG.md                                        ← Document new features
```

---

## 9. Quick Start for Developers

To start implementing:

```bash
# Switch to the worktree
cd /c/src/cycod-console-gui

# Create the directory structure
mkdir -p src/common/ConsoleGui/Core
mkdir -p src/common/ConsoleGui/Controls

# Start with the most important file
# Copy from: ../ai/src/common/details/console/gui/controls/ListBoxPicker.cs
# To: src/common/ConsoleGui/Controls/ListBoxPicker.cs

# Update namespaces from Azure.AI.Details.Common.CLI.ConsoleGui to ConsoleGui

# Build and test
dotnet build src/cycod/cycod.csproj
```

---

## 10. Common Helpers Location Reference

For developers wondering "where do I put this?":

| Component Type | Location | Example |
|---|---|---|
| Shared GUI controls | `src/common/ConsoleGui/` | ListBoxPicker.cs |
| Cycod-specific helpers | `src/cycod/Helpers/` | SpeechHelpers.cs |
| Configuration helpers | `src/common/Configuration/` | ScopeFileHelpers.cs |
| General utilities | `src/common/Helpers/` | ConsoleHelpers.cs |
| Command implementations | `src/cycod/CommandLineCommands/` | ChatCommand.cs |
| Command line parsing | `src/cycod/CommandLine/` | ChatCommandLineOptions.cs |
| Tests | `tests/cycodt-yaml/` | chat-tests.yaml |
| Documentation | `docs/` | chat-speech-recognition.md |

---

**Next Action**: Start with Phase 1 - create the Core directory and port Screen.cs and Window.cs!
