# cycodj show - Layer 7: Output Persistence

[← Back to show command](cycodj-show-filtering-pipeline-catalog-README.md) | [→ Proof Document](cycodj-show-filtering-pipeline-catalog-layer-7-proof.md)

## Overview

**Layer 7: Output Persistence** controls where and how the output of the `show` command is saved. By default, output goes to the console (stdout), but users can optionally save it to a file.

## Purpose

The output persistence layer allows users to:
- Save detailed conversation views to files for documentation
- Export specific conversations for sharing or archival
- Create conversation snapshots for comparison over time
- Feed conversation details to other tools for analysis

## Implementation

The `show` command implements output persistence identically to all other cycodj commands through:

1. **Shared base class functionality** from `CycoDjCommand`
2. **Shared option parsing** in `CycoDjCommandLineOptions`
3. **Standard execution flow** that checks if output should be saved

### Command-Line Options

| Option | Argument | Description |
|--------|----------|-------------|
| `--save-output` | `<file>` | Save command output to the specified file instead of printing to console |

### Data Flow

```
User specifies: cycodj show abc123 --save-output conversation.md
                                      ↓
    CycoDjCommandLineOptions.TryParseDisplayOptions()
      (line 171-180 in CycoDjCommandLineOptions.cs)
                                      ↓
           Sets: command.SaveOutput = "conversation.md"
                                      ↓
         ShowCommand.ExecuteAsync() generates detailed output
                                      ↓
   CycoDjCommand.SaveOutputIfRequested(output) is called
      (line 58-75 in CycoDjCommand.cs)
                                      ↓
      If SaveOutput is not null: writes to file & returns true
                                      ↓
        Command exits without printing to console
```

## Execution Pattern

The `show` command follows the standard cycodj execution pattern:

```csharp
public override async Task<int> ExecuteAsync()
{
    var output = GenerateShowOutput();            // Generate detailed conversation view
    var finalOutput = ApplyInstructionsIfProvided(output);  // Layer 8: AI processing
    if (SaveOutputIfRequested(finalOutput))       // Layer 7: Output persistence
    {
        return await Task.FromResult(0);
    }
    ConsoleHelpers.WriteLine(finalOutput);         // Default: console output
    return await Task.FromResult(0);
}
```

## File Output Details

### File Naming
- The file name is used **exactly as specified** by the user
- No template expansion
- No automatic timestamp injection
- Path can be relative or absolute

### File Content
The saved output includes:
- Conversation header with title and metadata
- Full timestamp and file location
- Message counts by role (user, assistant, tool, system)
- Branch information (parent/children)
- Complete message history with formatting
- Tool call details (if `--show-tool-calls` was specified)
- Statistics (if `--stats` was specified)

### File Creation
- Overwrites existing file if it exists
- Displays confirmation message: `"Output saved to: {fileName}"` (in green)

## Example Usage

```bash
# Save a specific conversation to a file
cycodj show abc123 --save-output my-conversation.md

# Save with tool details
cycodj show abc123 --show-tool-calls --show-tool-output --save-output detailed.md

# With statistics and AI summary
cycodj show abc123 --stats --instructions "Summarize key decisions" --save-output summary.md
```

## Integration with Other Layers

### Layer 1 (Target Selection)
The conversation ID is the target selection for show:
```bash
cycodj show abc123 --save-output output.md
```

### Layer 6 (Display Control)
All display options affect the saved output:
```bash
cycodj show abc123 --show-tool-calls --max-content-length 1000 --save-output detailed.md
```

Display options that affect saved output:
- `--show-tool-calls` - Include tool call details
- `--show-tool-output` - Show full tool outputs (not truncated)
- `--max-content-length` - Control truncation threshold
- `--stats` - Include conversation statistics

### Layer 8 (AI Processing)
AI processing happens BEFORE output persistence:
```bash
cycodj show abc123 --instructions "Extract action items" --save-output actions.md
```

## Behavioral Notes

1. **Console vs. File**: When `--save-output` is specified, output goes ONLY to the file
2. **Confirmation**: Success message is printed to console even when saving
3. **Overwrite**: Existing files are silently overwritten
4. **Complete Content**: The entire formatted conversation is saved, including all selected display options

## Source Code References

See the [proof document](cycodj-show-filtering-pipeline-catalog-layer-7-proof.md) for detailed source code evidence including:
- Line numbers for option parsing (identical to list)
- Line numbers for implementation (identical pattern)
- Complete call stack
- Integration with show-specific formatting

## Related Layers

- [Layer 6: Display Control](cycodj-show-filtering-pipeline-catalog-layer-6.md) - Controls what's in the output before saving
- [Layer 8: AI Processing](cycodj-show-filtering-pipeline-catalog-layer-8.md) - Transforms output before saving
