# cycodmd Files Command - Layer 6: Display Control

## Purpose

Layer 6 (Display Control) determines **how results are presented** to the user. This layer controls:
- Line numbering display
- Match highlighting
- Files-only mode (suppress content, show only file paths)
- Markdown wrapping for output formatting
- Console output formatting

## Command Line Options

### Primary Display Options

#### `--line-numbers`
**Purpose**: Include line numbers in the output for each line displayed.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Effect**: When enabled, each line is prefixed with its line number.

**Example**:
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers
```

**Source**: [CycoDmdCommandLineOptions.cs:161-164](cycodmd-files-layer-6-proof.md#line-numbers-parsing)

---

#### `--highlight-matches`
**Purpose**: Explicitly enable highlighting of matched patterns in the output.

**Type**: Boolean flag (no argument)

**Default**: `null` (tri-state: auto-decide based on context)

**Effect**: When enabled, matched patterns are visually highlighted in the output.

**Auto-enabling logic**: If not explicitly set, highlighting is automatically enabled when BOTH:
- `--line-numbers` is set, AND
- Context expansion is used (`--lines`, `--lines-before`, or `--lines-after`)

**Example**:
```bash
cycodmd "**/*.cs" --line-contains "Task" --highlight-matches
```

**Source**: [CycoDmdCommandLineOptions.cs:165-168](cycodmd-files-layer-6-proof.md#highlight-matches-parsing)

---

#### `--no-highlight-matches`
**Purpose**: Explicitly disable highlighting of matched patterns.

**Type**: Boolean flag (no argument)

**Default**: `null`

**Effect**: When specified, prevents highlighting even when auto-enable conditions are met.

**Example**:
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 3 --no-highlight-matches
```

**Source**: [CycoDmdCommandLineOptions.cs:169-172](cycodmd-files-layer-6-proof.md#no-highlight-matches-parsing)

---

#### `--files-only`
**Purpose**: Display only file paths, suppressing all file content.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Effect**: When enabled:
- File paths are listed (one per line)
- No file content is displayed
- Line-level filtering options are ignored
- Output is plain text (not wrapped in markdown)

**Use case**: Getting a list of matching files for further processing

**Example**:
```bash
cycodmd "**/*.cs" --file-contains "async" --files-only
```

**Output format**:
```
src/Commands/ChatCommand.cs
src/Helpers/AsyncHelpers.cs
src/Processing/TaskProcessor.cs
```

**Source**: [CycoDmdCommandLineOptions.cs:173-176](cycodmd-files-layer-6-proof.md#files-only-parsing)

---

### Implicit Display Options

#### Markdown Wrapping
**Purpose**: Wrap file content in markdown code blocks with syntax highlighting.

**Type**: Automatic (not user-controlled via CLI)

**Logic**:
- **Disabled** if:
  - Only one file is being processed, AND
  - Only one command is being executed, AND
  - The file can be converted by FileConverters (e.g., images, PDFs)
- **Enabled** otherwise

**Effect**: When enabled, output is wrapped as:
```markdown
## filename.ext

```language
file content here
```
```

**Source**: [Program.cs:229-231](cycodmd-files-layer-6-proof.md#markdown-wrapping-logic)

---

#### Auto-Highlight Logic
**Purpose**: Automatically enable match highlighting based on context.

**Type**: Automatic (can be overridden by explicit flags)

**Logic**:
```csharp
actualHighlightMatches = HighlightMatches ?? 
    (IncludeLineNumbers && 
     (IncludeLineCountBefore > 0 || IncludeLineCountAfter > 0))
```

**Conditions for auto-enable**:
1. `HighlightMatches` is `null` (not explicitly set), AND
2. `IncludeLineNumbers` is `true`, AND
3. Context expansion is used (before OR after > 0)

**Rationale**: When showing context with line numbers, highlighting helps distinguish matched lines from context lines.

**Source**: [Program.cs:219-224](cycodmd-files-layer-6-proof.md#auto-highlight-logic)

---

## Data Flow

### Input to Layer 6

From previous layers (1-5):
- **Files list**: Filtered list of file paths
- **Line patterns**: Regex patterns for content matching
- **Context counts**: Before/after line counts
- **Removal patterns**: Patterns for line removal

From command properties:
- `IncludeLineNumbers`: bool
- `HighlightMatches`: bool? (nullable tri-state)
- `FilesOnly`: bool

### Layer 6 Processing

1. **Files-Only Mode Check**:
   - If `FilesOnly` is true, bypass all content processing
   - Return joined file paths immediately
   - Skip to output

2. **Highlight Decision**:
   - Evaluate `HighlightMatches` value
   - Apply auto-highlight logic if null
   - Store in `actualHighlightMatches`

3. **Markdown Wrapping Decision**:
   - Check file count and command count
   - Check if file can be converted
   - Determine `wrapInMarkdown` flag

4. **Content Processing**:
   - Pass display flags to `GetCheckSaveFileContent`
   - Apply line numbering during content extraction
   - Apply highlighting during markdown formatting

### Output from Layer 6

To output stages:
- **Formatted content**: String with applied display formatting
- **Console output**: Via `ConsoleHelpers.WriteLineIfNotEmpty`

To Layer 7 (Output Persistence):
- **Formatted content**: Same content, optionally saved to files

---

## Integration Points

### Layer 5 → Layer 6

Layer 5 (Context Expansion) provides:
- Expanded line ranges with context
- Match positions within content
- `IncludeLineCountBefore` and `IncludeLineCountAfter` values

Layer 6 uses these to:
- Apply line numbering to expanded ranges
- Determine auto-highlight conditions
- Format output with context

### Layer 6 → Layer 7

Layer 6 provides:
- **Formatted output string**: With all display formatting applied
- **Display flags**: Passed through to output persistence layer

Layer 7 (Output Persistence) uses:
- Formatted string for file saving
- Display flags for determining save behavior

### Layer 6 → Console

Direct output via:
- `ConsoleHelpers.WriteLineIfNotEmpty(output)` for regular mode
- `ConsoleHelpers.DisplayStatus(...)` for progress updates

---

## Implementation Details

### Display Control Properties

Stored in `FindFilesCommand` class:
```csharp
public bool IncludeLineNumbers;     // Line 101
public bool? HighlightMatches;      // Line 102 (nullable for tri-state)
public bool FilesOnly;              // Line 103
```

### Parsing Implementation

In `CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions`:
- Lines 161-164: `--line-numbers` parsing
- Lines 165-168: `--highlight-matches` parsing
- Lines 169-172: `--no-highlight-matches` parsing
- Lines 173-176: `--files-only` parsing

### Execution Implementation

In `Program.HandleFindFileCommand`:
- Lines 194-206: Files-only mode shortcut
- Lines 219-224: Auto-highlight logic
- Lines 229-231: Markdown wrapping decision
- Lines 233-246: Display flags passed to content processor

### Content Formatting

In `Program.GetCheckSaveFileContent`:
- Lines 514-525: Content processing with display flags
- Display flags forwarded to `GetFinalFileContent`

---

## Special Behaviors

### Files-Only Mode

When `--files-only` is enabled:
1. **Short-circuits processing**: Returns immediately after file discovery
2. **Ignores all content filters**: `--line-contains`, `--remove-all-lines`, etc.
3. **No markdown wrapping**: Plain text output
4. **Simple output format**: One file path per line

**Performance benefit**: Avoids reading file contents entirely.

### Tri-State Highlighting

The `HighlightMatches` property uses nullable bool for three states:
- `true`: Explicitly enabled
- `false`: Explicitly disabled
- `null`: Auto-decide based on context

This allows smart defaults while respecting user preferences.

### Auto-Highlight Heuristic

Rationale for auto-enabling when line numbers + context are used:
- **Line numbers**: User wants precise line references
- **Context lines**: User is comparing matched vs. non-matched lines
- **Highlighting**: Makes visual distinction clearer

Without highlighting, context lines look identical to matched lines.

---

## Examples

### Example 1: Line Numbers Only
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers
```

Output:
```markdown
## src/Commands/ChatCommand.cs

```csharp
42: public async Task<string> ExecuteAsync()
45: var result = await ProcessAsync();
```
```

### Example 2: Highlighting with Context
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 2
```

Output (with auto-enabled highlighting):
```markdown
## src/Commands/ChatCommand.cs

```csharp
40: {
41:     Logger.Info("Starting chat");
42: >>> public async Task<string> ExecuteAsync() <<<
43: >>> { <<<
44:         var result = await ProcessAsync();
45:         return result;
46:     }
```
```

**Note**: `>>>` markers indicate highlighted lines (actual implementation uses console colors or markdown bold).

### Example 3: Explicit No-Highlighting
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 2 --no-highlight-matches
```

Output (highlighting suppressed despite auto-enable conditions):
```markdown
## src/Commands/ChatCommand.cs

```csharp
40: {
41:     Logger.Info("Starting chat");
42: public async Task<string> ExecuteAsync()
43: {
44:     var result = await ProcessAsync();
45:     return result;
46: }
```
```

### Example 4: Files-Only Mode
```bash
cycodmd "**/*.cs" --file-contains "async" --files-only
```

Output (plain text, no markdown):
```
src/Commands/ChatCommand.cs
src/Helpers/AsyncHelpers.cs
src/Processing/TaskProcessor.cs
```

---

## See Also

- [Layer 5: Context Expansion](cycodmd-files-layer-5.md) - Provides expanded line ranges for display
- [Layer 7: Output Persistence](cycodmd-files-layer-7.md) - Saves formatted output
- [Source Code Evidence](cycodmd-files-layer-6-proof.md) - Detailed line-by-line proof
