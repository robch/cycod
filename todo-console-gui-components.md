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

### Phase 5: Advanced Features (Future)
1. Speech recognition integration (if desired)
2. Custom themes/color schemes
3. Mouse support (if needed)
4. Additional control types based on needs

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

- [ ] All core components compile and run on Windows/macOS/Linux
- [ ] ListBoxPicker works with keyboard navigation
- [ ] Speed search functionality works correctly
- [ ] Colors render properly in different terminal types
- [ ] At least one cycod command uses the new UI components
- [ ] Unit tests cover core functionality
- [ ] Documentation includes usage examples
- [ ] No regressions in existing cycod functionality

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
