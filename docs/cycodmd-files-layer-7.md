# cycodmd FindFiles - Layer 7: Output Persistence

**[← Back to FindFiles Overview](cycodmd-filtering-pipeline-catalog-README.md#1-file-search-default-command)** | **[Proof →](cycodmd-files-layer-7-proof.md)**

## Purpose

Layer 7 (Output Persistence) controls **where and how results are saved** to files. This layer handles:
- Saving combined output to a single file
- Saving per-file output to multiple files using templates
- Saving AI chat history for review/debugging

## Command-Line Options

### `--save-output [file]`

**Type**: Shared option (all commands)  
**Default**: `output.md`  
**Purpose**: Save the combined markdown output to a file

**Behavior**:
- If no value provided, uses default `output.md`
- Creates or overwrites the specified file
- Contains all processed content in markdown format
- Applied after all filtering and processing layers

**Examples**:
```bash
# Save to default output.md
cycodmd "**/*.cs" --save-output

# Save to custom file
cycodmd "**/*.cs" --save-output analysis.md

# Save with timestamp (via config/template)
cycodmd "**/*.cs" --save-output "report-$(date +%Y%m%d).md"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 427-432 (shared option parsing)

---

### `--save-file-output [template]`

**Type**: FindFiles-specific option  
**Default**: `{filePath}/{fileBase}-output.md`  
**Purpose**: Save output for each file separately using a template

**Template Variables**:
- `{filePath}`: Directory containing the file
- `{fileBase}`: Filename without extension
- `{fileName}`: Full filename with extension
- Other variables may be supported (see proof for complete list)

**Behavior**:
- Creates one output file per processed input file
- Uses template to generate output filenames
- Each output file contains only the content for that specific input file
- Output files preserve directory structure based on template

**Examples**:
```bash
# Save each file's output in its directory
cycodmd "**/*.cs" --save-file-output

# Custom template - save to output directory
cycodmd "**/*.cs" --save-file-output "output/{fileBase}-processed.md"

# Save with source directory structure
cycodmd "**/*.cs" --save-file-output "{filePath}/output/{fileBase}.md"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 290-296 (FindFiles option parsing)

---

### `--save-chat-history [file]`

**Type**: Shared option (all commands with AI processing)  
**Default**: `chat-history-{time}.jsonl`  
**Purpose**: Save AI interaction history in JSONL format

**Template Variables**:
- `{time}`: Timestamp of the conversation

**Behavior**:
- Creates a JSONL file containing all AI interactions
- Each line is a JSON object representing one message/turn
- Useful for debugging AI instructions
- Useful for reviewing AI decisions
- Can be loaded into other tools for analysis

**Examples**:
```bash
# Save with default timestamp
cycodmd "**/*.cs" --instructions "Analyze code" --save-chat-history

# Save to specific file
cycodmd "**/*.cs" --instructions "Analyze code" --save-chat-history ai-log.jsonl

# Organized by date
cycodmd "**/*.cs" --instructions "Analyze code" --save-chat-history "logs/chat-$(date +%Y%m%d).jsonl"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 434-440 (shared option parsing)

---

## Option Interactions

### Precedence and Combination

1. **`--save-output` + `--save-file-output`**: Both can be used together
   - `--save-output`: Contains all files combined
   - `--save-file-output`: Creates individual files per input

2. **AI Processing Required for `--save-chat-history`**:
   - Only saves history if `--instructions` or AI-related options are used
   - Otherwise, no chat history to save

3. **Output Timing**:
   - Files are written AFTER all processing layers complete
   - Content has been filtered, transformed, and formatted
   - AI processing results (if any) are included

### Common Patterns

```bash
# Full audit trail: combined output + per-file + AI history
cycodmd "**/*.cs" --file-contains "bug" \
  --save-output bugs-report.md \
  --save-file-output "analysis/{fileBase}-bugs.md" \
  --instructions "Find potential bugs" \
  --save-chat-history ai-analysis.jsonl

# Batch processing with organized output
cycodmd "**/*.md" --file-contains "TODO" \
  --save-file-output "todos/{filePath}/{fileBase}-todos.md" \
  --save-output todo-summary.md
```

## Data Flow

### Input to Layer 7
From **Layer 6 (Display Control)**:
- Formatted content (markdown)
- File metadata (names, paths)
- Processing results
- AI analysis (if Layer 8 was used)

### Processing in Layer 7
1. **Collect all processed content**
2. **Apply output templates** (for `--save-file-output`)
3. **Generate output filenames** using template variables
4. **Write files to disk**:
   - Combined output (`--save-output`)
   - Per-file output (`--save-file-output`)
   - Chat history (`--save-chat-history`)
5. **Ensure directories exist** (create if needed)

### Output from Layer 7
- Files written to disk
- Status messages (if not `--quiet`)
- Error messages (if file write fails)

## Integration with Other Layers

### Dependencies (Inputs)
- **Layer 1-6**: All content must be collected and processed first
- **Layer 8**: If AI processing occurred, chat history is available

### Influences (Outputs)
- **Layer 9**: Actions like replace/execute may reference saved files
- **External Tools**: Saved files can be consumed by other processes

## Implementation Details

### File Writing
- Uses `File.WriteAllText()` or similar for output
- Creates parent directories if they don't exist
- Overwrites existing files without warning
- No atomic write guarantees (may leave partial files on crash)

### Template Expansion
- Template variables are replaced at write time
- Variables come from `FileInfo` and processing context
- Unknown variables are left as-is (e.g., `{unknown}` stays literal)

### Error Handling
- If output directory is not writable, error is logged
- Failed file writes do not stop processing of other files
- Partial output may be written before error occurs

## Related Options

### Global Options Affecting Output
- `--quiet`: Suppresses output file creation messages
- `--verbose`: Shows detailed file writing progress
- `--working-dir`: Changes base directory for relative output paths

### Layer 8 Options Affecting Output
- `--instructions`: Enables AI processing, making `--save-chat-history` meaningful
- `--built-in-functions`: Affects AI behavior, reflected in chat history

## Examples

### Example 1: Basic Output Saving
```bash
cycodmd "**/*.cs" --line-contains "async" --save-output async-methods.md
```

**Result**: Creates `async-methods.md` with all matched lines from all files.

---

### Example 2: Per-File Analysis
```bash
cycodmd "src/**/*.cs" \
  --file-contains "class" \
  --save-file-output "analysis/{fileBase}-classes.md"
```

**Result**: Creates one file per input file in `analysis/` directory, e.g.:
- `analysis/Program-classes.md`
- `analysis/Command-classes.md`

---

### Example 3: Complete Audit Trail
```bash
cycodmd "**/*.cs" \
  --file-contains "TODO" \
  --instructions "Categorize TODOs by priority" \
  --save-output todos-report.md \
  --save-file-output "per-file/{fileBase}-todos.md" \
  --save-chat-history ai-categorization.jsonl
```

**Result**: Creates:
1. `todos-report.md` - Combined report
2. `per-file/*-todos.md` - One file per input
3. `ai-categorization.jsonl` - AI interaction log

---

## See Also

- **[Layer 6: Display Control](cycodmd-files-layer-6.md)** - Formats content before saving
- **[Layer 8: AI Processing](cycodmd-files-layer-8.md)** - Generates AI analysis to save
- **[Proof Document](cycodmd-files-layer-7-proof.md)** - Source code evidence and implementation details

---

**[← Back to FindFiles Overview](cycodmd-filtering-pipeline-catalog-README.md#1-file-search-default-command)** | **[Proof →](cycodmd-files-layer-7-proof.md)**
