# TODO: Console GUI Components for cycod

**Created**: 2025-01-04  
**Branch**: robch/2501-jan03-console-gui-components  
**Worktree**: /c/src/cycod-console-gui  
**Source Reference**: ../ai (sibling Azure AI CLI tool)

## Objective

Port the console GUI components from the Azure AI CLI tool to cycod, enabling interactive UI features like:
- Interactive list pickers with keyboard navigation
- Context menus for enhanced user interaction
- Speech recognition integration (optional future feature)
- Text viewers and edit boxes

## Source Code Location

The Azure AI CLI tool has a complete console GUI framework at:
```
../ai/src/common/details/console/gui/
```

### Core Components to Port

#### 1. **Foundation Classes** (Priority: HIGH)
- [ ] `Screen.cs` - Screen management and cursor positioning
- [ ] `Window.cs` - Base windowing system with border support
- [ ] `Rect.cs` - Rectangle/positioning utilities  
- [ ] `Cursor.cs` - Cursor manipulation helpers
- [ ] `Colors.cs` - Color pair management (normal/selected states)

#### 2. **Base Control Classes** (Priority: HIGH)
- [ ] `Control.cs` - Base control window class (`ControlWindow`)
- [ ] `ScrollingControl.cs` - Abstract scrolling control base
- [ ] `VirtualListBoxControl.cs` - Virtual list box base

#### 3. **Interactive Controls** (Priority: MEDIUM-HIGH)
- [ ] `ListBoxControl.cs` - Basic list box functionality
- [ ] `ListBoxPicker.cs` - Interactive picker with selection
- [ ] `SpeedSearchListBoxControl.cs` - List with type-to-filter
- [ ] `InOutPipeServer.cs` - Support for pipe-based UI (for testing)

#### 4. **Additional Controls** (Priority: MEDIUM)
- [ ] `EditBoxControl.cs` - Text input boxes
- [ ] `EditBoxQuickEdit.cs` - Quick edit functionality
- [ ] `TextViewerControl.cs` - Text viewing with scrolling
- [ ] `HelpViewer.cs` - Help text display

## Target Integration Points

### Initial Use Cases for cycod

1. **Interactive Command Selection**
   - Replace simple prompts with `ListBoxPicker`
   - Example: File selection, command options, resource selection

2. **Context Menus**
   - Similar to AI CLI's chat context menu
   - Example: Press ENTER for options during interactive sessions

3. **Configuration Wizards**
   - Interactive setup similar to `AzCliConsoleGui` pickers
   - Example: Resource group selection, deployment configuration

4. **Help System Enhancement**
   - Use `HelpViewer` for better help display
   - Navigate help topics with keyboard

5. **Speech Recognition in Chat** ⭐ **HIGH PRIORITY**
   - Add `--speech` flag to `cycod chat` command
   - Show context menu with "speech" option when user presses ENTER
   - Real-time speech-to-text with interim results
   - Similar to AI CLI's implementation

## Speech Recognition Integration

### Overview

The AI CLI includes a sophisticated speech recognition feature that allows users to input chat messages via voice. This is a **high-priority feature** to port to cycod's chat command.

### How It Works in AI CLI

**Command Line Usage**:
```bash
ai chat --speech
```

**Interactive Flow**:
1. User presses ENTER on empty input
2. Context menu appears with options:
   ```
   ┌─────────────────────┐
   │ speech              │  ← Arrow keys to select
   │ ---                 │
   │ reset conversation  │
   │ exit                │
   └─────────────────────┘
   ```
3. Selecting "speech" triggers voice recognition
4. Shows "(listening)..." and real-time interim results
5. Returns final recognized text

### Source Code Location

**Main Implementation**: `../ai/src/ai/commands/chat_command.cs`
- Lines 324-334: `CreateSpeechConfig()` - Configuration setup
- Lines 336-369: `GetSpeechInputAsync()` - Speech input implementation
- Lines 140-150: Integration with context menu
- Lines 371-391: `PickInteractiveContextMenu()` - Menu with speech option

### Key Components to Port

#### 1. **Azure Speech SDK Dependency**
Add to cycod.csproj:
```xml
<PackageReference Include="Microsoft.CognitiveServices.Speech" Version="[latest]" />
```

#### 2. **Speech Configuration** (Lines 324-334)
```csharp
private SpeechConfig CreateSpeechConfig()
{
    // Load from config files
    var existing = FileHelpers.DemandFindFileInDataPath("speech.key", _values, "speech.key");
    var key = FileHelpers.ReadAllText(existing, Encoding.Default);
    
    existing = FileHelpers.DemandFindFileInDataPath("speech.region", _values, "speech.region");
    var region = FileHelpers.ReadAllText(existing, Encoding.Default);
    
    return SpeechConfig.FromSubscription(key, region);
}
```

**Configuration Files**:
- `.cycod/speech.key` - Azure Speech Service API key
- `.cycod/speech.region` - Azure region (e.g., "westus2")

#### 3. **Speech Input Method** (Lines 336-369)
```csharp
private async Task<string> GetSpeechInputAsync()
{
    Console.Write("\r");
    DisplayUserChatPromptLabel();
    Console.ForegroundColor = ColorHelpers.MapColor(ConsoleColor.DarkGray);
    
    var text = "(listening)";
    Console.Write($"{text} ...");
    var lastTextDisplayed = text;
    
    var config = CreateSpeechConfig();
    var recognizer = new SpeechRecognizer(config);
    
    // Show interim results in real-time
    recognizer.Recognizing += (s, e) =>
    {
        Console.Write("\r");
        DisplayUserChatPromptLabel();
        Console.ForegroundColor = ColorHelpers.MapColor(ConsoleColor.DarkGray);
        
        Console.Write($"{e.Result.Text} ...");
        if (e.Result.Text.Length < lastTextDisplayed.Length) 
            Console.Write(new string(' ', lastTextDisplayed.Length - e.Result.Text.Length));
        lastTextDisplayed = text;
    };
    
    // Get final result
    var result = await recognizer.RecognizeOnceAsync();
    
    // Clear and return
    Console.Write("\r");
    DisplayUserChatPromptLabel();
    Console.Write(new string(' ', result.Text.Length + 4));
    Console.Write("\r");
    DisplayUserChatPromptLabel();
    
    return result.Text;
}
```

**Key Features**:
- ✅ Real-time interim results via `Recognizing` event
- ✅ Automatic cleanup of console output
- ✅ Shows progress while listening
- ✅ Returns final recognized text

#### 4. **Context Menu Integration** (Lines 140-150, 371-391)
```csharp
private async Task ChatInteractively()
{
    var speechInput = _values.GetOrDefault("chat.speech.input", false);
    var userPrompt = _values["chat.message.user.prompt"];
    
    while (true)
    {
        DisplayUserChatPromptLabel();
        var text = ReadLineOrSimulateInput(ref userPrompt, "exit");
        
        if (text.ToLower() == "")
        {
            // Show context menu on empty input
            text = PickInteractiveContextMenu(speechInput);
            if (text == null) continue;
            
            var fromSpeech = false;
            if (text == "speech")
            {
                text = await GetSpeechInputAsync();
                fromSpeech = true;
            }
            
            DisplayUserChatPromptText(text, fromSpeech);
        }
        
        // Process the input...
    }
}

private string PickInteractiveContextMenu(bool allowSpeechInput)
{
    if (Console.CursorTop > 0)
    {
        var x = _quiet ? 0 : 11;
        Console.SetCursorPosition(x, Console.CursorTop - 1);
    }
    
    var choices = allowSpeechInput
        ? new string[] { "speech", "---", "reset conversation", "exit" }
        : new string[] { "reset conversation", "exit" };
    var select = allowSpeechInput ? 0 : choices.Length - 1;
    
    var choice = ListBoxPicker.PickString(
        choices, 20, choices.Length + 2,
        new Colors(ConsoleColor.White, ConsoleColor.Blue),
        new Colors(ConsoleColor.White, ConsoleColor.Red),
        select);
    
    return choice switch
    {
        "speech" => "speech",
        "exit" => "exit",
        _ => "reset"
    };
}
```

#### 5. **Command Parser Configuration**
From `../ai/src/ai/commands/parsers/chat_command_parser.cs` (line 156):
```csharp
new TrueFalseNamedValueTokenParser("chat.speech.input", "010"),
```

**For cycod**: Add similar parser token to enable `--speech` flag

### Implementation Checklist

- [ ] Add Microsoft.CognitiveServices.Speech NuGet package
- [ ] Create speech configuration helpers (CreateSpeechConfig)
- [ ] Implement GetSpeechInputAsync method
- [ ] Add speech option to context menu
- [ ] Add `--speech` command line flag parser
- [ ] Create config file structure for speech.key and speech.region
- [ ] Add error handling for missing/invalid credentials
- [ ] Test with different accents and languages
- [ ] Add documentation for Azure Speech Service setup
- [ ] Consider fallback for users without Speech Service

### Configuration Setup

Users will need to configure Azure Speech Service:

```bash
# Set up speech credentials
cycod config @speech.key --set YOUR_SPEECH_API_KEY
cycod config @speech.region --set westus2

# Or manually create files
echo "YOUR_SPEECH_API_KEY" > .cycod/speech.key
echo "westus2" > .cycod/speech.region
```

### Testing Approach

1. **Unit Tests**: Mock SpeechRecognizer for testing
2. **Integration Tests**: Test with actual Speech Service (optional)
3. **Manual Tests**: 
   - Test with clear speech
   - Test with background noise
   - Test with different languages
   - Test cancellation (how to cancel mid-recognition?)

### User Experience Design

**Visual Feedback**:
```
user@CHAT: (listening) ...
user@CHAT: what is the weather today ...  (interim)
user@CHAT: What is the weather today?     (final)
```

**Error Handling**:
- Missing speech.key → Clear error message with setup instructions
- Network issues → Retry or fallback to text input
- No audio input → Detect and provide helpful message

### Alternative: Local Speech Recognition

Consider supporting local speech recognition (not requiring Azure):
- **Windows**: System.Speech or Windows Speech Recognition
- **macOS**: NSSpeechRecognizer
- **Linux**: CMU Sphinx or Vosk

This could be a Phase 7 enhancement for offline capabilities.

### Dependencies and Considerations

**NuGet Package**:
```xml
<PackageReference Include="Microsoft.CognitiveServices.Speech" Version="1.38.0" />
```

**Supported Platforms**:
- ✅ Windows (x64, x86, ARM64)
- ✅ macOS (x64, ARM64)
- ✅ Linux (x64, ARM64)

**Runtime Dependencies**:
- Native libraries bundled with NuGet package
- No additional system dependencies needed

### Future Enhancements (Post-MVP)

1. **Language Selection**: Allow users to specify recognition language
2. **Custom Vocabulary**: Add domain-specific terms
3. **Noise Cancellation**: Improve recognition in noisy environments
4. **Voice Activity Detection**: Auto-start/stop listening
5. **Transcription History**: Save recognized text for review
6. **Multiple Input Sources**: Select microphone device

---

## Implementation Plan

### Phase 1: Foundation (Week 1)
1. Create namespace structure: `Azure.AI.Details.Common.CLI.ConsoleGui` → `Cycod.ConsoleGui`
2. Port core classes (Screen, Window, Rect, Cursor, Colors)
3. Write basic unit tests for positioning/rendering
4. Verify cross-platform compatibility (Windows/macOS/Linux)

### Phase 2: Basic Controls (Week 1-2)
1. Port base control classes (ControlWindow, ScrollingControl)
2. Port VirtualListBoxControl and ListBoxControl
3. Port ListBoxPicker (the most useful component)
4. Create demo app to test picker functionality

### Phase 3: Enhanced Controls (Week 2)
1. Port SpeedSearchListBoxControl for type-to-filter
2. Port EditBox components
3. Port TextViewerControl
4. Integration testing with existing cycod commands

### Phase 4: Integration (Week 3)
1. Identify cycod commands that would benefit from interactive UI
2. Create helper methods in ConsoleHelpers for easy integration
3. Update existing prompts to use ListBoxPicker where appropriate
4. Add examples to documentation

### Phase 5: Speech Recognition Integration (High Priority)
1. Add Azure Speech SDK dependency
2. Create speech configuration helpers
3. Implement speech input method for chat
4. Integrate with interactive context menu
5. Add real-time recognition display

### Phase 6: Advanced Features (Future)
1. Custom themes/color schemes
2. Mouse support (if needed)
3. Additional control types based on needs

## Technical Considerations

### Namespace Changes
```csharp
// Original (AI CLI):
namespace Azure.AI.Details.Common.CLI.ConsoleGui

// Target (cycod):
namespace Cycod.ConsoleGui
```

### Dependencies to Review
- Check for Azure-specific dependencies
- Verify System.Console compatibility across platforms
- Review P/Invoke calls for Windows-specific functionality
- Ensure graceful fallback for non-interactive terminals

### Testing Strategy
1. **Unit Tests**: Test core positioning and rendering logic
2. **Integration Tests**: Test with cycodt YAML framework
3. **Manual Testing**: Test on Windows, macOS, Linux
4. **Accessibility**: Ensure keyboard-only navigation works well

## Example Usage Pattern

Based on AI CLI's implementation:

```csharp
// Simple picker
var choices = new[] { "Option 1", "Option 2", "Option 3" };
var selected = ListBoxPicker.PickString(choices);
if (selected != null)
{
    Console.WriteLine($"You selected: {selected}");
}

// Advanced picker with colors
var picked = ListBoxPicker.PickIndexOf(
    choices,
    width: 40,
    height: 10,
    normal: new Colors(ConsoleColor.White, ConsoleColor.Blue),
    selected: new Colors(ConsoleColor.White, ConsoleColor.Red),
    select: 0  // Pre-select first item
);
```

## Files to Create in cycod

```
src/
  common/
    ConsoleGui/
      Core/
        Screen.cs
        Window.cs
        Rect.cs
        Cursor.cs
        Colors.cs
      Controls/
        ControlWindow.cs
        ScrollingControl.cs
        VirtualListBoxControl.cs
        ListBoxControl.cs
        ListBoxPicker.cs
        SpeedSearchListBoxControl.cs
        EditBoxControl.cs
        TextViewerControl.cs
        HelpViewer.cs
      InOutPipeServer.cs
      
tests/
  ConsoleGui/
    ListBoxPickerTests.cs
    ScreenTests.cs
    WindowTests.cs
```

## Reference Implementation

The AI CLI's chat command shows excellent integration:

**File**: `../ai/src/ai/commands/chat_command.cs`

```csharp
private string PickInteractiveContextMenu(bool allowSpeechInput)
{
    var choices = allowSpeechInput
        ? new string[] { "speech", "---", "reset conversation", "exit" }
        : new string[] { "reset conversation", "exit" };
    
    var choice = ListBoxPicker.PickString(
        choices, 20, choices.Length + 2,
        new Colors(ConsoleColor.White, ConsoleColor.Blue),
        new Colors(ConsoleColor.White, ConsoleColor.Red),
        select: 0);
    
    return choice switch
    {
        "speech" => "speech",
        "exit" => "exit",
        _ => "reset"
    };
}
```

## Key Features to Preserve

1. ✅ **Arrow key navigation** (Up/Down/PageUp/PageDown)
2. ✅ **Enter to select, Escape to cancel**
3. ✅ **Speed search** - type to filter items
4. ✅ **Custom colors** for normal/selected states
5. ✅ **Auto-sizing** based on content
6. ✅ **Border styles** (single line, double line, none)
7. ✅ **Scrolling** for long lists
8. ✅ **Pipe server support** for automated testing

## Success Criteria

### Core GUI Components
- [ ] All core components compile and run on Windows/macOS/Linux
- [ ] ListBoxPicker works with keyboard navigation
- [ ] Speed search functionality works correctly
- [ ] Colors render properly in different terminal types
- [ ] At least one cycod command uses the new UI components
- [ ] Unit tests cover core functionality
- [ ] Documentation includes usage examples
- [ ] No regressions in existing cycod functionality

### Speech Recognition Integration
- [ ] `--speech` flag works in cycod chat command
- [ ] Speech option appears in context menu when enabled
- [ ] Real-time interim results display while listening
- [ ] Final recognized text integrates with chat flow
- [ ] Configuration files (speech.key, speech.region) work correctly
- [ ] Clear error messages for missing/invalid credentials
- [ ] Documentation covers Azure Speech Service setup
- [ ] Works on Windows, macOS, and Linux

## Notes

- The AI CLI's console GUI system is well-architected and battle-tested
- The code is relatively self-contained with minimal external dependencies
- The MIT license is compatible with cycod's license
- Consider keeping the same file structure initially for easier maintenance
- Plan for gradual rollout - not all commands need interactive UI immediately

## Related Files to Study

For implementation patterns and integration examples:

```
../ai/src/common/details/azcli/AzCliConsoleGui.cs
../ai/src/common/details/azcli/AzCliConsoleGui_ResourceGroupPicker.cs
../ai/src/common/details/azcli/AzCliConsoleGui_SubscriptionPicker.cs
../ai/src/ai/commands/chat_command.cs (lines 371-391)
```

## Future Enhancements (Post-MVP)

- Mouse support for clicking items
- Multi-select capabilities
- Tree view controls for hierarchical data
- Progress indicators and status bars
- Modal dialog boxes
- Custom keyboard shortcuts
- Terminal theme detection and adaptation
- Performance optimization for very large lists

---

**Next Steps**: 
1. Review the source code files in `../ai/src/common/details/console/gui/`
2. Set up the namespace structure in cycod
3. Start with Screen.cs and Window.cs as foundation
4. Build incrementally with tests at each step
