# cycodmd FindFilesCommand - Layer 4: CONTENT REMOVAL

[🔙 Back to FindFilesCommand](cycodmd-findfiles-catalog-README.md) | [🔍 View Proof](cycodmd-findfiles-layer-4-proof.md)

## Purpose

Layer 4 implements **active content removal** - the ability to explicitly remove lines from display even if they would otherwise be included by earlier filtering layers. This is applied BEFORE context expansion and after content filtering.

## Implementation Status

✅ **Fully Implemented**

## Command-Line Options

### `--remove-all-lines <patterns...>`

**Purpose**: Remove all lines matching the specified regex patterns from display

**Syntax**:
```bash
cycodmd [globs] --remove-all-lines <pattern1> [pattern2] [...]
```

**Behavior**:
- Accepts one or more regex patterns
- Lines matching **any** of the patterns are removed
- Removal occurs BEFORE context expansion (context lines won't include removed lines)
- Removal applies to both matched lines AND context lines
- Case-insensitive matching (uses `RegexOptions.IgnoreCase`)

**Examples**:

Remove comments from C# files:
```bash
cycodmd "**/*.cs" --line-contains "async" --remove-all-lines "^\s*//.*$"
```

Remove multiple patterns (blank lines and comments):
```bash
cycodmd "**/*.cs" --line-contains "TODO" --remove-all-lines "^\s*$" "^\s*//.*$"
```

Remove log lines while searching:
```bash
cycodmd "**/*.log" --line-contains "ERROR" --remove-all-lines "DEBUG|INFO"
```

## Data Flow

### Parsing Phase
1. User provides `--remove-all-lines <patterns>`
2. Parser (`CycoDmdCommandLineOptions.cs`) validates and converts patterns to `Regex` objects
3. Stores in `FindFilesCommand.RemoveAllLineContainsPatternList` property

### Execution Phase
1. Files matching Layer 1 (Target Selection) are found
2. Files passing Layer 2 (Container Filter) are kept
3. For each file:
   - Content is loaded
   - If line filtering is needed (Layer 3 or Layer 4 active):
     - `LineHelpers.FilterAndExpandContext()` is called with both include AND remove patterns
     - Lines matching remove patterns are excluded from output
     - Context expansion happens AFTER removal (removed lines don't contribute to context)
4. Remaining content is displayed

## Processing Logic

### Order of Operations
```
1. Load file content
2. Split into lines
3. If includeLineContainsPatternList.Any() OR removeAllLineContainsPatternList.Any():
   a. Find matched lines (must match all include patterns, must NOT match any remove patterns)
   b. Expand context around matched lines
   c. For each context line:
      - Check if it matches remove patterns
      - If yes, exclude from context
      - If no, include in output
4. Build final output with remaining lines
```

### Key Helper Function

**`LineHelpers.IsLineMatch(line, includePatterns, removePatterns)`**

Returns `true` if:
- Line matches ALL include patterns (if any)
- AND line does NOT match ANY remove patterns

```csharp
var includeMatch = includeLineContainsPatternList.All(regex => regex.IsMatch(line));
var excludeMatch = removeAllLineContainsPatternList.Count > 0 && 
                   removeAllLineContainsPatternList.Any(regex => regex.IsMatch(line));
return includeMatch && !excludeMatch;
```

## Properties

### FindFilesCommand Properties

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `RemoveAllLineContainsPatternList` | `List<Regex>` | Empty list | Regex patterns for lines to remove |

## Interaction with Other Layers

### Layer 2 (Container Filter)
- Independent - operates at different level (files vs lines)

### Layer 3 (Content Filter)
- **Complementary** - Layer 3 determines what TO show, Layer 4 determines what NOT to show
- Both filters are evaluated together in `LineHelpers.IsLineMatch()`
- A line must:
  - Match ALL Layer 3 include patterns (if any)
  - Match NONE of the Layer 4 remove patterns

### Layer 5 (Context Expansion)
- **Sequential** - Layer 4 runs BEFORE Layer 5
- Removed lines are excluded from context expansion
- When expanding context, each potential context line is checked against remove patterns
- This prevents removed lines from appearing as "context"

### Layer 6 (Display Control)
- Independent - Layer 6 controls formatting, Layer 4 controls content

## Use Cases

### 1. Remove Boilerplate Code
```bash
# Show async methods but hide using statements and namespace declarations
cycodmd "**/*.cs" --line-contains "async Task" --remove-all-lines "^using " "^namespace "
```

### 2. Clean Log Files
```bash
# Find errors but hide debug/info noise
cycodmd "**/*.log" --line-contains "ERROR|EXCEPTION" --remove-all-lines "DEBUG|INFO|TRACE"
```

### 3. Remove Comments from Code Review
```bash
# Show TODO items without the surrounding comment clutter
cycodmd "**/*.cs" --line-contains "TODO" --remove-all-lines "^\s*//.*Copyright" "^\s*//.*License"
```

### 4. Filter Configuration Files
```bash
# Show database config but hide comments
cycodmd "**/appsettings.json" --line-contains "ConnectionString" --remove-all-lines "^\\s*#"
```

### 5. Remove Empty Lines
```bash
# Compact output by removing blank lines
cycodmd "**/*.md" --line-contains "## " --remove-all-lines "^\s*$"
```

## Edge Cases and Behavior

### Remove Without Include
If `--remove-all-lines` is specified WITHOUT `--line-contains`:
- The remove filter is still checked
- But no line filtering occurs (all lines kept by default)
- In practice, this has no effect (use case: none)

### Conflicting Patterns
If a line matches both include AND remove patterns:
- **Remove takes precedence**
- The line is excluded from output
- This allows precise control: "show X unless Y"

Example:
```bash
# Show lines with "TODO" but not ones marked "TODO: DONE"
cycodmd "**/*.cs" --line-contains "TODO" --remove-all-lines "TODO:.*DONE"
```

### Remove in Context Lines
Context lines are also checked against remove patterns:
```bash
# Show errors with 3 lines context, but don't include debug lines in context
cycodmd "**/*.log" --line-contains "ERROR" --lines 3 --remove-all-lines "DEBUG"
```

### Multiple Remove Patterns
Patterns are OR'd together - a line matching ANY pattern is removed:
```bash
# Remove lines matching pattern1 OR pattern2 OR pattern3
cycodmd "**/*.cs" --line-contains "async" --remove-all-lines "pattern1" "pattern2" "pattern3"
```

## Logging

When Layer 4 is active, the following logs are generated:

### Info Level
```
Using N exclude regex patterns on 'filename.ext':
  Exclude pattern: 'pattern1'
  Exclude pattern: 'pattern2'
```

### Verbose Level
For each line checked:
```
Line match check for: 'line content here'
  Include patterns (2): pattern1, pattern2
  Include match: True
  Exclude patterns (1): pattern3
  Exclude match: False
  Final result: True (line will be shown)
```

Or if excluded:
```
Line match check for: 'line content here'
  Include patterns (1): pattern1
  Include match: True
  Exclude patterns (1): pattern2
  Exclude match: True (matched patterns: pattern2)
  Final result: False (line will be excluded)
```

## Performance Considerations

- Regex pattern matching is performed for EVERY line when removal patterns are active
- Multiple patterns = multiple regex evaluations per line
- For large files with many patterns, this can be slow
- Consider using more specific patterns to reduce matching overhead

## Source Code References

See [Layer 4 Proof](cycodmd-findfiles-layer-4-proof.md) for detailed source code evidence including:
- Command-line parsing (`CycoDmdCommandLineOptions.cs` lines 152-160)
- Property definition (`FindFilesCommand.cs` lines 27, 106)
- Execution flow (`Program.cs` lines 240, 472-596)
- Core filtering logic (`LineHelpers.cs` lines 8-96)

## Related Layers

- [Layer 1: TARGET SELECTION](cycodmd-findfiles-layer-1.md) - What files to search
- [Layer 2: CONTAINER FILTER](cycodmd-findfiles-layer-2.md) - Which files to include
- [Layer 3: CONTENT FILTER](cycodmd-findfiles-layer-3.md) - What lines to show
- **Layer 4: CONTENT REMOVAL** ← You are here
- [Layer 5: CONTEXT EXPANSION](cycodmd-findfiles-layer-5.md) - How to expand around matches
- [Layer 6: DISPLAY CONTROL](cycodmd-findfiles-layer-6.md) - How to format output
- [Layer 7: OUTPUT PERSISTENCE](cycodmd-findfiles-layer-7.md) - Where to save results
- [Layer 8: AI PROCESSING](cycodmd-findfiles-layer-8.md) - AI-assisted analysis
- [Layer 9: ACTIONS ON RESULTS](cycodmd-findfiles-layer-9.md) - Actions on results

---

[🔙 Back to FindFilesCommand](cycodmd-findfiles-catalog-README.md) | [🔍 View Proof](cycodmd-findfiles-layer-4-proof.md)
