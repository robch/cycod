# cycodj search Command - Layer 5: Context Expansion

## Overview

The `search` command implements **context expansion** to show surrounding lines around matched content within chat messages. This helps users understand the context of search matches.

## Implementation Summary

The search command provides a single context expansion mechanism:

### `--context N` / `-C N`

Displays N lines before and after each matched line within a message.

**Default**: 2 lines

**Example Usage**:
```bash
# Show 5 lines of context around matches
cycodj search "error" --context 5

# Show no context (only matching lines)
cycodj search "error" --context 0

# Use short form
cycodj search "error" -C 3
```

## How It Works

### 1. Match Detection

When searching message content, the command:
1. Splits message content into lines
2. Searches each line for the query (plain text or regex)
3. Records the line number of each match

### 2. Context Expansion

For each matched line, the command:
1. Calculates which surrounding lines to include based on `ContextLines` value
2. Includes lines where: `|matchedLineNumber - currentLineNumber| <= ContextLines`
3. Displays matched lines with a `>` prefix
4. Displays context lines with plain indentation

### 3. Output Format

```
  [role] Message #N
  > This line contains the MATCH
    This is a context line before/after
```

## Command Line Parsing

See **[Layer 5 Proof](cycodj-search-filtering-pipeline-catalog-layer-5-proof.md)** for detailed source code evidence.

### Parsing Location

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Method**: `TryParseSearchCommandOptions`

**Lines**: 469-478

The parser handles both long and short forms of the option:
- `--context <N>`
- `-C <N>`

## Execution Flow

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

### Key Methods

1. **`SearchText`** (lines 222-262)
   - Splits message content into lines
   - Identifies matching lines
   - Returns list of (lineNumber, line, matchStart, matchLength)

2. **`AppendConversationMatches`** (lines 264-298)
   - Iterates through all lines in matched messages
   - Determines which lines to show based on proximity to matches
   - Applies context expansion logic (line 286)
   - Formats output with appropriate prefixes

### Context Expansion Logic

```csharp
var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);
```

This simple but effective calculation determines whether to show a line:
- If the current line index `i` is within `ContextLines` distance from any matched line, show it
- Uses absolute value to check distance in both directions (before and after)

## Characteristics

### Symmetric Expansion
- Context is expanded equally before AND after matches
- No option for asymmetric expansion (e.g., more before than after)
- This differs from cycodmd which has `--lines-before` and `--lines-after`

### Message-Scoped
- Context expansion is limited to the current message
- Does not cross message boundaries
- Does not show surrounding messages (that would be a different layer)

### Efficient Display
- Only matched lines and their context are shown
- Unmatched lines far from matches are omitted
- This keeps output focused and readable

## Limitations

1. **No asymmetric expansion**: Cannot show different amounts of context before vs. after
2. **No message-level context**: Cannot show surrounding messages (only lines within a message)
3. **Fixed for all matches**: Same context amount for all matches in the search

## Related Options

While not strictly Layer 5 (Context Expansion), these options affect what content is shown:

- **`--user-only`** (Layer 3: Content Filtering) - Only search user messages
- **`--assistant-only`** (Layer 3: Content Filtering) - Only search assistant messages
- **`--messages N`** (Layer 6: Display Control) - Affects message preview, not match context

## Comparison with Other Commands

| Command | Context Expansion | Type | Options |
|---------|------------------|------|---------|
| **search** | ✅ Yes | Line-level | `--context N`, `-C N` |
| **list** | ❌ No | N/A | (only message previews) |
| **show** | ❌ No | N/A | (shows entire messages) |
| **branches** | ❌ No | N/A | (only message previews) |
| **stats** | ❌ No | N/A | (aggregate stats only) |
| **cleanup** | ❌ No | N/A | (file operations only) |

## Example Scenarios

### Scenario 1: Debugging Error Messages

```bash
cycodj search "exception" --context 3
```

Shows 3 lines before and after each line containing "exception", helping identify error causes and effects.

### Scenario 2: Finding API Usage

```bash
cycodj search "OpenAI" --context 5 --case-sensitive
```

Shows 5 lines around "OpenAI" mentions with exact case matching, useful for finding API usage patterns.

### Scenario 3: Minimal Context

```bash
cycodj search "TODO" --context 1
```

Shows just 1 line before/after TODO comments for a compact list.

### Scenario 4: No Context (Match Only)

```bash
cycodj search "error" --context 0
```

Shows only the lines containing "error", no surrounding context.

## Future Enhancement Opportunities

1. **Asymmetric context**: `--context-before N --context-after M`
2. **Message-level context**: `--context-messages N` to show surrounding messages
3. **Adaptive context**: Automatically adjust based on match density
4. **Context highlighting**: Different colors for matched vs. context lines

## Navigation

- [← Layer 4: Content Removal](cycodj-search-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-search-filtering-pipeline-catalog-layer-6.md)
- [↑ search Command Overview](cycodj-search-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)
- [📋 Source Code Proof →](cycodj-search-filtering-pipeline-catalog-layer-5-proof.md)
