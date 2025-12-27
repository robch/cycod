# cycodj search - Layer 7: Output Persistence

[← Back to search command](cycodj-search-filtering-pipeline-catalog-README.md) | [→ Proof Document](cycodj-search-filtering-pipeline-catalog-layer-7-proof.md)

## Overview

**Layer 7: Output Persistence** controls where and how the output of the `search` command is saved. By default, output goes to the console (stdout), but users can optionally save it to a file.

## Purpose

The output persistence layer allows users to:
- Save search results to files for later review
- Create search result snapshots for comparison
- Document search patterns and their matches
- Feed search results to other tools for further analysis

## Implementation

The `search` command implements output persistence identically to other cycodj commands through:

1. **Shared base class functionality** from `CycoDjCommand`
2. **Shared option parsing** in `CycoDjCommandLineOptions`
3. **Standard execution flow** that checks if output should be saved

### Command-Line Options

| Option | Argument | Description |
|--------|----------|-------------|
| `--save-output` | `<file>` | Save command output to the specified file instead of printing to console |

### Data Flow

```
User specifies: cycodj search "async" --save-output results.md
                                          ↓
    CycoDjCommandLineOptions.TryParseDisplayOptions()
      (line 171-180 in CycoDjCommandLineOptions.cs)
                                          ↓
             Sets: command.SaveOutput = "results.md"
                                          ↓
         SearchCommand.ExecuteAsync() generates search results
                                          ↓
   CycoDjCommand.SaveOutputIfRequested(output) is called
      (line 58-75 in CycoDjCommand.cs)
                                          ↓
      If SaveOutput is not null: writes to file & returns true
                                          ↓
          Command exits without printing to console
```

## Execution Pattern

The `search` command follows the standard cycodj execution pattern:

```csharp
public override async Task<int> ExecuteAsync()
{
    var output = GenerateSearchOutput();           // Generate search results
    var finalOutput = ApplyInstructionsIfProvided(output);  // Layer 8: AI processing
    if (SaveOutputIfRequested(finalOutput))        // Layer 7: Output persistence
    {
        return await Task.FromResult(0);
    }
    ConsoleHelpers.WriteLine(finalOutput);          // Default: console output
    return await Task.FromResult(0);
}
```

## File Output Details

### File Content
The saved output includes:
- Search query header
- Time range filter information (if applied)
- List of matching conversations with:
  - Conversation title and timestamp
  - Number of matches per conversation
  - Matched messages with context lines
  - Highlighted match locations
- Total match count summary
- Statistics (if `--stats` was specified)

### File Naming
- The file name is used **exactly as specified** by the user
- No template expansion

### File Creation
- Overwrites existing file if it exists
- Displays confirmation message: `"Output saved to: {fileName}"` (in green)

## Example Usage

```bash
# Save search results to a file
cycodj search "error" --save-output errors.md

# Save with context and filtering
cycodj search "async" --context 3 --user-only --save-output async-usage.md

# With statistics and AI analysis
cycodj search "TODO" --stats --instructions "Categorize by priority" --save-output todos.md

# Time-filtered search results
cycodj search "deployment" --today --save-output today-deployments.md
```

## Integration with Other Layers

### Layer 1 (Target Selection)
Time filtering affects which conversations are searched:
```bash
cycodj search "error" --today --save-output today-errors.md
cycodj search "bug" --last 10 --save-output recent-bugs.md
```

### Layer 3 (Content Filter)
Content filtering options affect what matches are found:
```bash
cycodj search "test" --user-only --save-output user-tests.md
cycodj search "pattern" --regex --case-sensitive --save-output patterns.md
```

### Layer 5 (Context Expansion)
Context lines around matches are included in saved output:
```bash
cycodj search "error" --context 5 --save-output error-context.md
```

### Layer 6 (Display Control)
Display options affect the saved results:
```bash
cycodj search "TODO" --stats --save-output todo-stats.md
cycodj search "query" --branches --messages 5 --save-output query-results.md
```

### Layer 8 (AI Processing)
AI processing happens BEFORE output persistence:
```bash
cycodj search "decision" --instructions "Extract decisions and reasoning" --save-output decisions.md
```

## Behavioral Notes

1. **Console vs. File**: When `--save-output` is specified, output goes ONLY to the file
2. **Confirmation**: Success message is printed to console even when saving
3. **Overwrite**: Existing files are silently overwritten
4. **Complete Results**: All matches with context are saved, respecting context expansion settings

## Source Code References

See the [proof document](cycodj-search-filtering-pipeline-catalog-layer-7-proof.md) for detailed source code evidence including:
- Line numbers for option parsing (identical to other commands)
- Line numbers for implementation (identical pattern)
- Complete call stack
- Integration with search-specific formatting

## Related Layers

- [Layer 3: Content Filter](cycodj-search-filtering-pipeline-catalog-layer-3.md) - Determines what matches are found
- [Layer 5: Context Expansion](cycodj-search-filtering-pipeline-catalog-layer-5.md) - Controls context around matches
- [Layer 6: Display Control](cycodj-search-filtering-pipeline-catalog-layer-6.md) - Controls what's in the output before saving
- [Layer 8: AI Processing](cycodj-search-filtering-pipeline-catalog-layer-8.md) - Transforms output before saving
