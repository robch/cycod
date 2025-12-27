# cycodmd File Search - Layer 8: AI Processing

## Purpose

Layer 8 (AI Processing) provides AI-assisted analysis and transformation of search results. This layer allows users to apply natural language instructions to file content, enabling automated summarization, reformatting, analysis, or any other AI-driven content transformation.

## Position in Pipeline

Layer 8 occurs **after** all filtering, context expansion, and display formatting, but **before** final output persistence and display. It operates on the formatted content produced by previous layers.

**Pipeline Flow:**
```
Layer 1-7: Select, Filter, Format Content
    ↓
Layer 8: AI Processing (THIS LAYER)
    ↓
Display to Console or Save to File
```

## Command-Line Options

### General AI Instructions

#### `--instructions <instruction>`
**Purpose**: Apply general AI instructions to **all combined output**  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: After all files are processed and combined  
**Example**:
```bash
cycodmd "**/*.cs" --instructions "Summarize the code structure"
cycodmd "**/*.md" --instructions "Extract all headings" --instructions "Create a table of contents"
```

### File-Specific AI Instructions

#### `--file-instructions <instruction>`
**Purpose**: Apply AI instructions to **each file individually**  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-file, after content filtering and formatting  
**Example**:
```bash
cycodmd "**/*.cs" --file-instructions "Explain what this file does"
cycodmd "**/*.py" --file-instructions "List all functions and their purposes"
```

### Extension-Specific AI Instructions

#### `--{extension}-file-instructions <instruction>`
**Purpose**: Apply AI instructions only to files matching a specific extension  
**Pattern**: `--{ext}-file-instructions` where `{ext}` can be any extension (cs, py, md, etc.)  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-file, only for files matching the extension criteria  
**Example**:
```bash
cycodmd "**/*" \
  --cs-file-instructions "Identify design patterns used" \
  --md-file-instructions "Extract action items" \
  --json-file-instructions "Validate schema"
```

### AI Function Access

#### `--built-in-functions`
**Purpose**: Enable AI to use built-in functions (if supported by the AI tool)  
**Type**: Boolean flag  
**Default**: `false`  
**Example**:
```bash
cycodmd "**/*.cs" --file-instructions "Find all TODO comments" --built-in-functions
```

### Chat History Persistence

#### `--save-chat-history [filename]`
**Purpose**: Save AI interaction history to a file for debugging/review  
**Type**: Optional value  
**Default**: `chat-history-{time}.jsonl` (if flag specified without value)  
**Example**:
```bash
cycodmd "**/*.cs" --file-instructions "Document this code" --save-chat-history
cycodmd "**/*.md" --instructions "Summarize" --save-chat-history analysis-history.jsonl
```

## Implementation Details

### Two-Level Processing

AI processing in cycodmd file search operates at **two distinct levels**:

1. **Per-File Processing** (`--file-instructions`, `--{ext}-file-instructions`)
   - Applied to each file **individually** after content formatting
   - Filtered by file extension criteria if using extension-specific instructions
   - Runs in parallel (throttled) if instructions are present

2. **Global Processing** (`--instructions`)
   - Applied to **all combined output** from all files
   - Runs after all per-file processing is complete
   - Delays console output until processing is finished

### Processing Order

```
1. Find files (Layer 1)
2. Filter files by content (Layer 2)
3. Filter lines within files (Layer 3)
4. Remove unwanted lines (Layer 4)
5. Expand context around matches (Layer 5)
6. Format with line numbers, highlighting (Layer 6)
7. Save individual file outputs (Layer 7)
   ↓
8. Apply per-file AI instructions (--file-instructions, --{ext}-file-instructions)
   ↓
9. Combine all file outputs
   ↓
10. Apply global AI instructions (--instructions)
   ↓
11. Display final output or save to --save-output
```

### Extension Matching

Extension-specific instructions use a flexible matching system:

- **Exact match**: `--cs-file-instructions` matches `.cs` files
- **Case-insensitive**: `--CS-file-instructions` also matches `.cs` files
- **Multiple extensions**: Multiple `--{ext}-file-instructions` can be combined

**Matching Logic** (from `Program.cs:554-557`):
```csharp
var instructionsForThisFile = fileInstructionsList
    .Where(x => FileNameMatchesInstructionsCriteria(fileName, x.Item2))
    .Select(x => x.Item1)
    .ToList();
```

### AI Tool Integration

The AI processing layer integrates with external AI tools:

- **Primary**: `cycod` (if available)
- **Fallback**: `ai` (legacy tool)

The processor automatically detects which tool to use and constructs appropriate arguments.

### Instruction Chaining

Multiple instructions are applied **sequentially** (not in parallel):

```csharp
// From AiInstructionProcessor.cs:15
instructionsList.Aggregate(content, (current, instruction) => 
    ApplyInstructions(instruction, current, useBuiltInFunctions, saveChatHistory, retries));
```

**Example Flow:**
```bash
--instructions "Extract headings" --instructions "Sort alphabetically"
```
Result: First extracts headings from content, then sorts those extracted headings.

### Throttling Behavior

AI processing triggers **throttled execution** for file processing:

```csharp
// From Program.cs:217
var needsThrottling = findFilesCommand.FileInstructionsList.Any();
```

When `--file-instructions` or `--{ext}-file-instructions` is present:
- Files are processed with concurrency control
- Prevents overwhelming the AI service with parallel requests
- Uses `ThrottledProcessor` with configurable parallelism

## Data Flow

### Input to Layer 8
- **Formatted content** from Layer 6 (with line numbers, highlighting, etc.)
- **File metadata** (filename, extension)
- **Instruction lists** (general, file-specific, extension-specific)

### Output from Layer 8
- **Transformed content** (AI-processed text)
- **Error messages** (if AI processing fails)
- **Chat history** (if `--save-chat-history` specified)

## Example Usage

### Basic Per-File Analysis
```bash
cycodmd "**/*.cs" --file-instructions "Summarize the purpose of this file in one sentence"
```

### Extension-Specific Processing
```bash
cycodmd "**/*" \
  --cs-file-instructions "List all public methods" \
  --md-file-instructions "Extract all links" \
  --json-file-instructions "Summarize the schema"
```

### Combined Per-File and Global Processing
```bash
cycodmd "**/*.cs" \
  --file-instructions "Extract class names" \
  --instructions "Create a dependency graph"
```

### With Chat History for Debugging
```bash
cycodmd "**/*.py" \
  --file-instructions "Identify potential bugs" \
  --save-chat-history bug-analysis.jsonl
```

### Complex Multi-Stage Processing
```bash
cycodmd "**/*.md" \
  --file-instructions "Extract all TODO items" \
  --instructions "Group by priority" \
  --instructions "Create a prioritized task list"
```

## Related Layers

- **Layer 7 (Output Persistence)**: Saves individual file outputs before AI processing
- **Display/Console Output**: Delayed until after AI processing completes
- **Layer 9 (Actions on Results)**: N/A for file search (no actions performed)

## See Also

- [Layer 8 Proof Document](cycodmd-files-layer-8-proof.md) - Source code evidence
- [Layer 7: Output Persistence](cycodmd-files-layer-7.md) - Saving results
- [Layer 1: Target Selection](cycodmd-files-layer-1.md) - Finding files to process
