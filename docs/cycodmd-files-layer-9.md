# cycodmd Files Command - Layer 9: Actions on Results

## Overview

**Layer**: 9 - Actions on Results  
**Command**: `cycodmd [globs...]` (FindFilesCommand)  
**Purpose**: Execute operations on search results, including find-and-replace and execution modes.

Layer 9 is the final stage of the filtering pipeline, where the system performs actions on the filtered and processed results rather than just displaying them. For the FindFiles command, this layer implements **search-and-replace** functionality with optional execution.

## Command-Line Options

### Core Action Options

#### `--replace-with <text>`
**Purpose**: Specify replacement text for matched content  
**Type**: String  
**Parsed At**: `CycoDmdCommandLineOptions.cs:177-182`  
**Stored In**: `FindFilesCommand.ReplaceWithText`

Defines the text that will replace matched patterns in files. This option works in conjunction with content filtering options from Layer 3 (`--line-contains`).

**Example**:
```bash
cycodmd **/*.cs --line-contains "oldFunction" --replace-with "newFunction"
```

#### `--execute`
**Purpose**: Execute the replace operation (vs. preview mode)  
**Type**: Boolean flag  
**Parsed At**: `CycoDmdCommandLineOptions.cs:183-186`  
**Stored In**: `FindFilesCommand.ExecuteMode`

When present, this flag causes the replacement to be written to disk. Without this flag, the command operates in **preview mode**, showing what would be changed without actually modifying files.

**Example**:
```bash
# Preview mode (default)
cycodmd **/*.cs --line-contains "oldFunction" --replace-with "newFunction"

# Execution mode (writes changes)
cycodmd **/*.cs --line-contains "oldFunction" --replace-with "newFunction" --execute
```

## Data Flow

### 1. Option Parsing

```
User Input: --replace-with "text" --execute
         ↓
CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions()
         ↓
FindFilesCommand.ReplaceWithText = "text"
FindFilesCommand.ExecuteMode = true
```

### 2. Execution Flow

```
FindFilesCommand.ExecuteAsync()
         ↓
For each matched file:
    ↓
    FileHelpers.ReadAllText(file)
         ↓
    Apply line filters (Layer 3)
         ↓
    If ReplaceWithText != null:
        ↓
        Replace matched patterns with replacement text
         ↓
        If ExecuteMode == true:
            ↓
            FileHelpers.WriteAllText(file, modified content)
        Else:
            ↓
            Display preview (no disk write)
```

### 3. Replace Logic

The replacement logic applies to lines that match the filters from Layer 3:

1. **Identify matched lines** using `IncludeLineContainsPatternList` (from `--line-contains`)
2. **Apply replacement** to matched patterns within those lines
3. **Write or preview** based on `ExecuteMode`

## Integration with Other Layers

### Dependencies

Layer 9 depends on earlier layers to identify what to replace:

- **Layer 1 (Target Selection)**: Determines which files to process
  - Uses `Globs` to find target files
  - Applies time-based filters to narrow file set

- **Layer 2 (Container Filter)**: Determines which files contain target content
  - Uses `IncludeFileContainsPatternList` to filter files
  - Uses `ExcludeFileContainsPatternList` to exclude files

- **Layer 3 (Content Filter)**: Identifies specific lines to replace
  - Uses `IncludeLineContainsPatternList` to match lines
  - Replacement applies ONLY to lines matching these patterns

- **Layer 8 (AI Processing)**: Can be combined with AI analysis
  - AI can suggest replacements via `--instructions`
  - Replacement can be manual (CLI) or AI-assisted

### Safe-By-Default Design

The `--execute` flag implements a **safe-by-default** pattern:

1. **Default behavior**: Preview mode (no disk writes)
   - Shows what would change
   - User can review before committing

2. **Explicit execution**: Requires `--execute` flag
   - Prevents accidental data loss
   - Forces user confirmation of intent

## Execution Evidence

See [Layer 9 Proof Document](cycodmd-files-layer-9-proof.md) for detailed source code references showing:

- Command-line parsing implementation
- Property storage in `FindFilesCommand`
- Execution logic in `FindFilesCommand.ExecuteAsync()`
- File I/O operations for replacement
- Preview vs. execution mode handling

## Examples

### Example 1: Preview Replace

```bash
cycodmd **/*.cs --line-contains "Console.WriteLine" --replace-with "Logger.Log"
```

**Result**: Shows which files would be modified and what changes would occur (no disk writes).

### Example 2: Execute Replace

```bash
cycodmd **/*.cs --line-contains "Console.WriteLine" --replace-with "Logger.Log" --execute
```

**Result**: Modifies all matched files on disk, replacing "Console.WriteLine" with "Logger.Log" in matching lines.

### Example 3: Filtered Replace

```bash
cycodmd **/*.cs \
  --file-contains "using System" \
  --line-contains "oldMethod" \
  --replace-with "newMethod" \
  --execute
```

**Result**: Only files containing "using System" are considered, and only lines containing "oldMethod" are replaced.

### Example 4: Replace with AI Instructions

```bash
cycodmd **/*.cs \
  --line-contains "TODO" \
  --instructions "Review TODOs and suggest better comments" \
  --save-output suggestions.md
```

**Result**: AI analyzes TODO comments and provides suggestions (no automatic replacement, but AI output can guide manual replacement).

## Behavioral Notes

### Pattern Matching

- Replacement uses the **same patterns** as Layer 3 content filtering
- `--line-contains` patterns determine which lines to replace
- Replacement applies to **entire matched pattern**, not just first occurrence
- Multiple patterns = OR logic (any pattern match triggers replacement)

### Multi-File Operations

- Replace operates on **all matched files** in sequence
- Each file is processed independently
- Failures on one file don't stop processing of others
- Error reporting per file in execution mode

### Preview Mode Features

- Shows file path
- Shows line numbers
- Shows before/after comparison
- Color-coded diff display (if terminal supports color)
- No actual file writes

### Execution Mode Features

- Writes modified content to original file
- Preserves file permissions
- Creates backup? (TODO: verify if backup is created)
- Reports success/failure per file
- Returns exit code based on overall success

## Safety Considerations

### Data Loss Prevention

1. **Preview by default**: User must explicitly add `--execute`
2. **Atomic writes**: File is fully written or not modified (no partial writes)
3. **Error handling**: Parse errors don't trigger writes
4. **Validation**: Replacement text is validated before processing

### Recommended Workflow

```bash
# Step 1: Preview changes
cycodmd <patterns> --replace-with <text>

# Step 2: Review output carefully

# Step 3: If satisfied, execute
cycodmd <patterns> --replace-with <text> --execute
```

### Version Control Integration

For safety, use within version-controlled repositories:

```bash
# Make changes
cycodmd **/*.cs --line-contains "old" --replace-with "new" --execute

# Review with git
git diff

# Revert if needed
git checkout -- .
```

## Related Layers

- **[Layer 3: Content Filter](cycodmd-files-layer-3.md)** - Defines which lines to replace
- **[Layer 8: AI Processing](cycodmd-files-layer-8.md)** - AI-suggested replacements
- **[Layer 7: Output Persistence](cycodmd-files-layer-7.md)** - Save preview to file
- **[Layer 1: Target Selection](cycodmd-files-layer-1.md)** - Which files to process

## See Also

- [Layer 9 Proof Document](cycodmd-files-layer-9-proof.md) - Detailed source code evidence
- [cycodmd Files Command Overview](cycodmd-files-filtering-pipeline-catalog-README.md)
- [Main CLI Catalog](CLI-Filtering-Patterns-Catalog.md)
