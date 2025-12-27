# cycodmd File Search - Layer 5: Context Expansion

**[← Back to File Search Overview](cycodmd-filtering-pipeline-catalog-README.md#file-search)**

## Purpose

Layer 5 controls how the file search command **expands context around matching lines**. When lines match the content filter patterns (Layer 3), this layer determines whether to show additional lines before and after each match to provide context.

## CLI Options

### Primary Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--lines N` | Symmetric expansion | `0` | Show N lines before AND after each match |
| `--lines-before N` | Asymmetric expansion | `0` | Show N lines before each match |
| `--lines-after N` | Asymmetric expansion | `0` | Show N lines after each match |

### Option Relationships

- **`--lines N`**: Sets both `IncludeLineCountBefore` and `IncludeLineCountAfter` to the same value (symmetric expansion)
- **`--lines-before N`**: Sets only `IncludeLineCountBefore` (asymmetric expansion)
- **`--lines-after N`**: Sets only `IncludeLineCountAfter` (asymmetric expansion)

These options can be combined. For example:
```bash
# Show 3 lines before and 5 lines after each match
cycodmd "**/*.cs" --line-contains "async" --lines-before 3 --lines-after 5
```

## Data Flow

### 1. Parsing Stage

```
User Input: cycodmd **/*.cs --line-contains "async" --lines 3
           ↓
CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions()
           ↓
FindFilesCommand.IncludeLineCountBefore = 3
FindFilesCommand.IncludeLineCountAfter = 3
```

### 2. Execution Stage

```
Program.ProcessFindFilesCommand()
           ↓
For each file with matching lines:
           ↓
LineHelpers.FilterAndExpandContext(
    content,
    includeLineContainsPatternList,
    IncludeLineCountBefore,  // ← Layer 5 parameter
    IncludeLineCountAfter,   // ← Layer 5 parameter
    includeLineNumbers,
    removeAllLineContainsPatternList,
    backticks,
    highlightMatches
)
           ↓
Returns expanded content with context lines
```

## Implementation Details

### Context Expansion Algorithm

The context expansion algorithm (implemented in `LineHelpers.FilterAndExpandContext()`) works as follows:

1. **Identify Matching Lines**: Find all lines that match Layer 3's content filters
2. **Expand Around Matches**: For each matching line index `i`:
   - Add lines `[i - N_before, ..., i - 1]` (before context)
   - Add line `i` (the matching line itself)
   - Add lines `[i + 1, ..., i + N_after]` (after context)
3. **Remove Duplicates**: Use a `HashSet` to avoid including the same line multiple times when match ranges overlap
4. **Apply Removal Filters**: Context lines that match Layer 4's removal patterns are **excluded** from context
5. **Insert Separators**: When there are gaps between expanded ranges, insert separator blocks (```` ``` \n\n ``` ````)

### Key Characteristics

- **Respects Removal Patterns**: Context lines are NOT added if they match `--remove-all-lines` patterns (Layer 4)
- **Smart Overlap Handling**: If two matches are close together, their context ranges merge automatically
- **Boundary Protection**: Never attempts to add lines before index 0 or after the last line
- **Gap Indication**: Visual separators show when line numbers have gaps (non-contiguous ranges)

## Integration with Other Layers

### Dependencies (Consumes From)

- **Layer 3 (Content Filter)**: Requires matching line indices to determine where to expand context
- **Layer 4 (Content Removal)**: Respects removal patterns when adding context lines

### Impacts (Flows To)

- **Layer 6 (Display Control)**: Expanded content is passed to display formatting
  - Line numbers (if `--line-numbers`) are applied to context lines
  - Highlighting (if `--highlight-matches`) distinguishes match lines from context lines

## Examples

### Example 1: Symmetric Expansion

```bash
cycodmd **/*.cs --line-contains "async" --lines 2
```

**Result**: Shows each matching line plus 2 lines before and 2 lines after.

```
File: src/example.cs (matches: 1)
```cs
  10:   public class Example
  11:   {
* 12:     public async Task DoWorkAsync()
  13:     {
  14:       await Task.Delay(100);
```
*(Line 12 is the match, lines 10-11 are before context, lines 13-14 are after context)*

### Example 2: Asymmetric Expansion

```bash
cycodmd **/*.cs --line-contains "async" --lines-before 1 --lines-after 3
```

**Result**: Shows each matching line plus 1 line before and 3 lines after.

### Example 3: Overlapping Ranges

```bash
cycodmd **/*.cs --line-contains "async" --lines 2
```

If two matches are only 1-2 lines apart, their context ranges overlap and merge:

```
File: src/example.cs (matches: 2)
```cs
  10:   public class Example
  11:   {
* 12:     public async Task DoWorkAsync()
  13:     {
* 14:       await Task.Delay(100);
  15:     }
  16:   }
```
*(Lines 12 and 14 both match, context ranges merge into a single continuous block)*

### Example 4: Gap Separators

```bash
cycodmd **/*.cs --line-contains "async" --lines 1
```

If matches are far apart, gaps are indicated with separator blocks:

```
File: src/example.cs (matches: 2)
```cs
  11:   {
* 12:     public async Task DoWorkAsync()
  13:     {
```

```cs
  49:   }
* 50:     public async Task OtherMethodAsync()
  51:     {
```

*(Gap between lines 13 and 49 is indicated by the separator)*

## Special Cases

### No Context Expansion

If `--lines`, `--lines-before`, and `--lines-after` are all omitted (or set to 0), only the exact matching lines are shown with no context.

### Context + No Line Filter

If context expansion options are provided but no `--line-contains` filter is specified, context expansion has no effect (all lines are included anyway).

### Context + Removal Patterns

Context lines that match `--remove-all-lines` patterns are **excluded** from context expansion:

```bash
cycodmd **/*.cs --line-contains "public" --lines 2 --remove-all-lines "private"
```

If a context line contains "private", it will NOT be added to the output, even though it's within the expansion range.

## Performance Considerations

- **O(M × N)** complexity where M = number of matching lines, N = context size
- **Memory**: Uses `HashSet<int>` to track unique line indices (efficient for large files with many matches)
- **Early Exit**: If no lines match Layer 3 filters, context expansion is skipped entirely

---

**[→ See Proof Documentation](cycodmd-files-layer-5-proof.md)** | **[← Back to File Search Overview](cycodmd-filtering-pipeline-catalog-README.md#file-search)**
