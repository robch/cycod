# cycodgr search - Layer 5: CONTEXT EXPANSION

## Overview

**Layer 5: CONTEXT EXPANSION** controls how much context is shown around matched lines in search results. This layer determines whether to show just the matching line or to expand the display to include surrounding lines for better understanding.

## Purpose

Context expansion provides users with:
- **Better understanding** of matches by showing surrounding code
- **Code flow visibility** - see what comes before and after a match
- **Consistent expansion** - same number of lines before and after

## Options

### `--lines-before-and-after <N>`

Specifies the number of lines to show both before AND after each matched line.

**Aliases**: `--lines`

**Type**: Integer

**Default**: 5 lines

**Behavior**: Symmetric expansion (same count before and after)

**Examples**:
```bash
# Show 2 lines before and 2 lines after each match
cycodgr --file-contains "async Task" --lines 2

# Show 10 lines of context
cycodgr --file-contains "ConPTY" --lines-before-and-after 10

# Use default (5 lines before/after)
cycodgr --file-contains "CreateProcess"
```

## Implementation Notes

### Symmetric Only

Unlike some other CLIs in this codebase (e.g., cycodmd which has `--lines-before` and `--lines-after`), cycodgr only supports **symmetric context expansion**. The same number of lines is shown before and after matched lines.

### Applied to Code Search Only

Context expansion is applied when searching **code files**, not when searching repositories. Repository search shows full metadata without line-level filtering.

### Integration with Line Filtering

Context expansion works in conjunction with Layer 3 (CONTENT FILTERING):
1. Lines matching the filter are identified
2. Context lines are added around each match
3. The expanded context is displayed with line numbers

## Data Flow

```
SearchCommand.LinesBeforeAndAfter (property)
    ↓
Program.HandleCodeSearchAsync()
    ↓
Program.FormatAndOutputCodeResults()
    ↓
Program.ProcessFileGroupAsync()
    ↓
LineHelpers.FilterAndExpandContext()
    ↓
Display output with context
```

## Where Applied

Context expansion is applied in:

1. **Code search** (`--file-contains`, `--contains` with code results)
2. **File content display** (when fetching full file content from GitHub)
3. **Line filtering** (when `--line-contains` patterns are specified)

Context is NOT applied to:
- Repository metadata display
- File lists (`--format files`)
- URL lists (`--format urls`)

## Source Code References

For detailed source code evidence, see:
- [Layer 5 Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-5-proof.md)

Key files:
- `src/cycodgr/CommandLineCommands/SearchCommand.cs` - LinesBeforeAndAfter property
- `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs` - Option parsing
- `src/cycodgr/Program.cs` - Context application during output
- `src/common/Helpers/LineHelpers.cs` - Context expansion algorithm

## Interaction with Other Layers

### Layer 3: CONTENT FILTERING
Context expansion is applied AFTER content filtering identifies matching lines. The filter determines which lines are "matches", and context expansion adds lines around those matches.

### Layer 6: DISPLAY CONTROL
The display format affects how context is shown:
- `detailed` format: Shows context with syntax highlighting
- `json`/`csv` formats: Context is included in text match fragments
- `files`/`urls` formats: No line-level context

### Layer 8: AI PROCESSING
Context is included in the content passed to AI instructions, providing AI with surrounding code for better analysis.

## Behavior Details

### Overlapping Context Ranges

When two matches are close together, their context windows may overlap. The implementation handles this by:
- Merging overlapping ranges
- Not duplicating lines
- Maintaining sequential line numbers

### File Boundaries

Context expansion respects file boundaries:
- Won't show lines before line 1
- Won't show lines after the last line of the file

### Line Number Display

When context is shown, line numbers are displayed:
- Matched lines are marked (typically with `*` or highlighting)
- Context lines are shown with their actual line numbers
- Line numbers help locate the match in the original file

## Example Output

```bash
$ cycodgr microsoft/terminal --file-contains "ConPTY" --lines 2
```

Output shows:
```
## src/cascadia/TerminalCore/Terminal.cpp

```cpp
  145:     // Initialize the console pipe
* 146:     _hConPTYInput = ConPTY::CreatePipe();
  147:     if (_hConPTYInput == INVALID_HANDLE_VALUE)
  148:     {
  149:         return E_FAIL;
```

The `*` marker indicates line 146 is the match, with lines 145, 147-149 showing context (2 before, 2 after).

## Default Behavior

If `--lines-before-and-after` is not specified, the default is **5 lines** of context before and after each match.

This default provides a good balance between:
- Showing enough context to understand the match
- Not overwhelming users with too much code
- Reasonable output length for typical searches

## Performance Considerations

Context expansion happens AFTER:
- Files are downloaded from GitHub
- Content is filtered by patterns
- Matches are identified

This means context expansion does not affect:
- GitHub API calls
- Network bandwidth (beyond fetching matched files)
- Initial search performance

The performance impact is minimal as it's a simple line extraction operation on already-fetched content.

## Related Options

- Layer 3: `--line-contains` (filters which lines are matches)
- Layer 6: `--format` (controls how context is displayed)
- Layer 8: `--file-instructions` (AI processing includes context)

---

**For detailed source code evidence and line numbers, see**: [cycodgr-search-filtering-pipeline-catalog-layer-5-proof.md](cycodgr-search-filtering-pipeline-catalog-layer-5-proof.md)
