# FindFilesCommand - Layer 3: Content Filtering

[← Back to FindFilesCommand Overview](cycodmd-findfiles-catalog-README.md)

## Purpose

Layer 3 controls **what content within selected files** should be displayed. After Layer 1 (target selection) identifies files and Layer 2 (container filtering) determines which files to process, Layer 3 filters down to specific lines within those files.

## How It Works

Content filtering in FindFilesCommand operates on **line-level granularity**. It determines which lines from matched files should be included in the output based on regex pattern matching.

### Dual-Layer Nature of `--contains`

Uniquely, the `--contains` option operates at **both Layer 2 and Layer 3**:

1. **Layer 2 (Container)**: Filters which files to process based on content
2. **Layer 3 (Content)**: Filters which lines to display within those files

This dual behavior makes `--contains` a powerful shortcut but can be confusing since it spans two conceptual layers.

## Options

### `--line-contains <pattern> [<pattern>...]`

Filters lines to show only those matching one or more regex patterns.

**Behavior**:
- **Multiple patterns**: Lines matching ANY pattern are included (OR logic)
- **Pattern type**: Regex patterns (case-insensitive by default)
- **Multiple values**: Can provide multiple patterns as separate arguments
- **Interaction with context expansion**: Matched lines trigger context expansion (Layer 5)

**Example**:
```bash
# Show only lines containing "async" or "await"
cycodmd "**/*.cs" --line-contains "async" "await"
```

**Implementation**:
- Patterns stored in: `IncludeLineContainsPatternList` (List<Regex>)
- Applied in: `GetCheckSaveFileContentAsync` function
- Matching logic: Each line tested against all patterns; if any match, line is included

---

### `--contains <pattern> [<pattern>...]`

**Dual-purpose option** that applies filtering at both file level (Layer 2) and line level (Layer 3).

**Behavior**:
- **Layer 2**: Includes only files containing at least one match
- **Layer 3**: Within matched files, shows only matching lines (plus context if specified)
- **Multiple patterns**: Each pattern added to both `IncludeFileContainsPatternList` AND `IncludeLineContainsPatternList`

**Example**:
```bash
# Find files containing "async", then show only lines with "async" in those files
cycodmd "**/*.cs" --contains "async"
```

**Why This Design?**
This dual behavior is efficient for search-focused workflows:
1. Quickly skip files that don't contain the search term (Layer 2)
2. Then highlight only the relevant lines in matched files (Layer 3)

Without `--contains`, you'd need both `--file-contains` and `--line-contains`:
```bash
# Equivalent to: cycodmd "**/*.cs" --contains "async"
cycodmd "**/*.cs" --file-contains "async" --line-contains "async"
```

---

### `--highlight-matches` / `--no-highlight-matches`

Controls visual highlighting of matched content within lines.

**Behavior**:
- `--highlight-matches`: Force enable highlighting (wraps matches in markdown bold `**match**`)
- `--no-highlight-matches`: Force disable highlighting
- **Default (auto)**: Enable highlighting when `--line-numbers` AND context lines present

**Auto-enable logic**:
```csharp
var actualHighlightMatches = findFilesCommand.HighlightMatches ?? 
    (findFilesCommand.IncludeLineNumbers && 
     (findFilesCommand.IncludeLineCountBefore > 0 || 
      findFilesCommand.IncludeLineCountAfter > 0));
```

**Example**:
```bash
# Explicitly enable highlighting
cycodmd "**/*.cs" --line-contains "async" --highlight-matches

# Auto-enable (has line numbers + context)
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 2
```

**Why Auto-Enable?**
Highlighting is most useful when you have context lines (Layer 5) and line numbers (Layer 6), allowing you to quickly spot the matched content among surrounding lines. Without these, highlighting adds noise.

---

## Interaction with Other Layers

### Layer 2 (Container Filtering)
- **`--contains`**: Shares patterns with Layer 2's file filtering
- Files must pass Layer 2 before Layer 3 line filtering is applied

### Layer 4 (Content Removal)
- Content removal (`--remove-all-lines`) is applied **BEFORE** Layer 3 filtering
- Removed lines are never considered for matching

### Layer 5 (Context Expansion)
- Matched lines become "anchor points" for context expansion
- `--lines-before`/`--lines-after` show surrounding lines relative to matches
- Context lines are shown even if they don't match patterns

### Layer 6 (Display Control)
- `--line-numbers`: Works with highlighting to create readable output
- `--files-only`: Bypasses all content filtering (shows only file paths)

### Layer 9 (Actions - Replace)
- `--replace-with`: Requires Layer 3 patterns to identify replacement targets
- Patterns in `IncludeLineContainsPatternList` define what to replace

## Implementation Details

### Pattern Matching Logic

**File**: `src/cycodmd/Program.cs`

The content filtering is applied in `GetCheckSaveFileContentAsync`:

```csharp
var filteredContent = IncludeLineContainsPatternList.Any()
    ? FileHelpers.FilterFileContentByLinePatterns(
        fileContent, 
        IncludeLineContainsPatternList, 
        RemoveAllLineContainsPatternList, 
        IncludeLineCountBefore, 
        IncludeLineCountAfter, 
        IncludeLineNumbers,
        highlightMatches)
    : fileContent;
```

**Key insight**: If `IncludeLineContainsPatternList` is empty, NO line filtering is applied—all lines are shown (subject to Layer 4 removal patterns).

### Extension-Specific Instructions (Layer 8 Bridge)

While technically Layer 8 (AI Processing), the `--{ext}-file-instructions` pattern creates an implicit content filter:
- Files are categorized by extension
- AI instructions are applied per extension category
- This creates a "soft" content filter based on file type

Example:
```bash
# Different instructions for different file types
cycodmd "**/*" \
  --cs-file-instructions "Analyze C# code quality" \
  --md-file-instructions "Extract headings and links"
```

## Pattern Type: Regex

All Layer 3 patterns are **regular expressions**:
- **Case-insensitive** by default (RegexOptions.IgnoreCase)
- **Culture-invariant** (RegexOptions.CultureInvariant)
- Compiled at parse time (early validation)

**Parser code** (`CycoDmdCommandLineOptions.cs:108-135`):
```csharp
else if (arg == "--contains")
{
    var patterns = GetInputOptionArgs(i + 1, args, required: 1);
    var asRegExs = ValidateRegExPatterns(arg, patterns);
    command.IncludeFileContainsPatternList.AddRange(asRegExs);
    command.IncludeLineContainsPatternList.AddRange(asRegExs);  // Dual-layer!
    i += patterns.Count();
}
else if (arg == "--line-contains")
{
    var patterns = GetInputOptionArgs(i + 1, args, required: 1);
    var asRegExs = ValidateRegExPatterns(arg, patterns);
    command.IncludeLineContainsPatternList.AddRange(asRegExs);
    i += patterns.Count();
}
```

## Edge Cases & Special Behaviors

### Empty Pattern List
- **Behavior**: ALL lines shown (no filtering)
- **Why**: Allows showing entire file content with context controls

### Multiple Patterns (OR Logic)
- **Behavior**: A line matching ANY pattern is included
- **Why**: Supports searching for multiple terms simultaneously
- **Example**: `--line-contains "TODO" "FIXME" "HACK"` finds all markers

### Pattern Order
- **Irrelevant**: Patterns tested in list order, but match order doesn't affect output
- **Output order**: Lines appear in file order, not match order

### Matching Entire Line vs. Substring
- **Behavior**: Patterns match **substrings** of lines (not full-line matches)
- **Why**: Most search use cases look for content within lines
- **Full-line match**: Use regex anchors: `^pattern$`

### Unicode & Special Characters
- **Supported**: Regex patterns fully support Unicode
- **Example**: `--line-contains "café|naïve"`

## Performance Considerations

### Pattern Compilation
- **When**: Parse time (during command-line processing)
- **Benefit**: Runtime matching is fast (compiled regex)
- **Cost**: Slight startup delay for complex patterns

### File Processing Order
- **Behavior**: Files processed in order returned by `FindMatchingFiles`
- **Throttling**: Applied when AI instructions present (Layer 8)
- **Parallelization**: Multiple files processed concurrently (when no AI)

### Memory Usage
- **Line-by-line**: Content filtering processes files line-by-line
- **No full-file buffering**: Efficient for large files
- **Exception**: Replacement mode (`--replace-with`) requires full-file buffering

## Common Use Cases

### 1. Simple Text Search
```bash
cycodmd "**/*.cs" --line-contains "TODO"
```

### 2. Multi-Term Search
```bash
cycodmd "**/*.cs" --line-contains "async" "await" "Task"
```

### 3. Search with Context
```bash
cycodmd "**/*.cs" --line-contains "async" --lines 3
```

### 4. Search with Highlighting
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 2 --highlight-matches
```

### 5. Find Files AND Show Matches
```bash
cycodmd "**/*.cs" --contains "async"
# Equivalent to:
# cycodmd "**/*.cs" --file-contains "async" --line-contains "async"
```

### 6. Complex Regex Search
```bash
cycodmd "**/*.cs" --line-contains "public\s+(async\s+)?Task<.*>"
```

## See Also

- [Layer 2: Container Filtering](cycodmd-findfiles-layer-2.md) - File-level content filtering
- [Layer 4: Content Removal](cycodmd-findfiles-layer-4.md) - Removing unwanted lines
- [Layer 5: Context Expansion](cycodmd-findfiles-layer-5.md) - Showing context around matches
- [Layer 6: Display Control](cycodmd-findfiles-layer-6.md) - Formatting match output
- [Layer 9: Actions on Results](cycodmd-findfiles-layer-9.md) - Search-and-replace using Layer 3 patterns

**Proof Documentation**: [View Source Code Evidence](cycodmd-findfiles-layer-3-proof.md)
