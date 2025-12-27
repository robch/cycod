# cycodgr Search Command - Layer 8: AI Processing

## Overview

**Layer 8: AI Processing** enables AI-assisted analysis and transformation of search results. This layer allows users to apply instructions to search results at three different levels: individual files, entire repositories, or all combined output.

## Purpose

Process search results through AI models to:
- Extract insights from code
- Summarize findings
- Transform output format
- Answer questions about search results
- Generate documentation from code

## Implementation Summary

cycodgr implements a **three-tier AI processing hierarchy**:

1. **File-Level Instructions** - Process individual files matching criteria
2. **Repo-Level Instructions** - Process entire repository outputs
3. **Global Instructions** - Process all combined output

### Processing Order

```
Search Results
    ↓
File-Level AI Processing (per file)
    ↓
Repo-Level AI Processing (per repo)
    ↓
Global AI Processing (all output)
    ↓
Final Output
```

## Command Line Options

###1. `--instructions <instruction>`

Apply AI instructions to ALL combined output (global level).

**Example:**
```bash
cycodgr microsoft/terminal --file-contains "ConPTY" \
  --instructions "Summarize the key ConPTY implementation patterns"
```

**Characteristics:**
- Applied LAST (after file and repo processing)
- Processes entire combined output
- Can be specified multiple times (each applied sequentially)

---

### 2. `--file-instructions <instruction>`

Apply AI instructions to EACH individual file in search results.

**Example:**
```bash
cycodgr microsoft/terminal --file-contains "Unicode" \
  --file-instructions "Explain what this code does"
```

**Characteristics:**
- Applied to every file in results
- Processes files in parallel for performance
- No file-type filtering (applies to all files)

---

### 3. `--{ext}-file-instructions <instruction>`

Apply AI instructions to files matching specific extension criteria.

**Extension Pattern:**
```
--cs-file-instructions     → .cs files
--py-file-instructions     → .py files
--md-file-instructions     → .md files
--{ext}-file-instructions  → .{ext} files
```

**Example:**
```bash
cycodgr microsoft/semantic-kernel --file-contains "ChatClient" \
  --cs-file-instructions "Document the public API methods" \
  --md-file-instructions "Extract key documentation points"
```

**Characteristics:**
- Filters by file extension
- Multiple extension-specific instructions can be combined
- Applied to files in parallel

---

### 4. `--repo-instructions <instruction>`

Apply AI instructions to EACH repository's complete output.

**Example:**
```bash
cycodgr microsoft/* --file-contains "authentication" \
  --repo-instructions "What authentication mechanisms are used?"
```

**Characteristics:**
- Applied after all files in a repo are processed
- Sees complete repo context (all files together)
- Can be specified multiple times

---

## Processing Flow

### Stage 1: File-Level Processing

For each file in search results:

```
1. Fetch file content from GitHub
2. Filter lines based on --line-contains or search query
3. Apply context expansion (--lines)
4. Format as markdown with code fence
5. Match against file instructions criteria:
   - Check extension-specific instructions (--{ext}-file-instructions)
   - Check general file instructions (--file-instructions)
6. Apply matching AI instructions
7. Return processed file output
```

**Parallel Execution**: Files are processed concurrently using `ThrottledProcessor` (Environment.ProcessorCount threads).

---

### Stage 2: Repo-Level Processing

For each repository:

```
1. Collect all processed file outputs
2. Combine into single repo output with:
   - Repo header (name, stars, language)
   - Repo URL and description
   - File count and match statistics
   - All processed file contents
3. Apply repo instructions if any
4. Return processed repo output
```

---

### Stage 3: Global Processing

After all repositories processed:

```
1. Combine all repo outputs
2. Apply global instructions if any
3. Output final result to console
4. Save to files if requested
```

---

## AI Instruction Matching Logic

### File Instructions Matching

```csharp
bool FileNameMatchesInstructionsCriteria(string fileName, string fileNameCriteria)
{
    return string.IsNullOrEmpty(fileNameCriteria) ||  // No criteria = matches all
           fileName.EndsWith($".{fileNameCriteria}") ||  // Extension match
           fileName == fileNameCriteria;                  // Exact match
}
```

**Examples:**
- `--file-instructions "..."` → matches ALL files (empty criteria)
- `--cs-file-instructions "..."` → matches `*.cs` files
- `--Program.cs-file-instructions "..."` → matches only `Program.cs`

---

## Data Structures

### FileInstructionsList

```csharp
List<Tuple<string, string>> FileInstructionsList
```

**Structure:**
- `Item1`: The instruction text
- `Item2`: File name criteria (extension or filename)

**Population:**
- `--file-instructions "X"` → Add `("X", "")`
- `--cs-file-instructions "X"` → Add `("X", "cs")`

---

### RepoInstructionsList

```csharp
List<string> RepoInstructionsList
```

Simple list of instruction strings, applied sequentially to each repository's output.

---

### InstructionsList

```csharp
List<string> InstructionsList
```

Simple list of instruction strings, applied sequentially to the final combined output.

---

## AI Processing Integration

### AiInstructionProcessor

cycodgr uses the shared `AiInstructionProcessor` class for actual AI interactions:

```csharp
string ApplyAllInstructions(
    List<string> instructions,
    string content,
    bool useBuiltInFunctions,
    string saveChatHistory)
```

**Parameters used by cycodgr:**
- `instructions`: List of instruction strings
- `content`: The text to process (file, repo, or combined output)
- `useBuiltInFunctions`: Always `false` (not exposed in cycodgr CLI)
- `saveChatHistory`: Always `string.Empty` (not exposed in cycodgr CLI)

---

## Current Limitations

### Not Implemented

1. **`--built-in-functions`** - Not exposed in cycodgr (unlike cycodmd/cycodj)
2. **`--save-chat-history`** - Not exposed in cycodgr (unlike cycodmd/cycodj)
3. **`--use-mcps`** - Not exposed in cycodgr (unlike cycod)

### Potential Enhancements

1. **Streaming AI responses** - Currently blocks until complete
2. **AI model selection** - Uses system default
3. **Cost estimation** - No token usage tracking
4. **Caching** - No result caching for repeated instructions
5. **Interactive refinement** - No iterative instruction refinement

---

## Examples

### Example 1: File-Level Processing

```bash
cycodgr microsoft/terminal --file-contains "ConPTY" \
  --file-instructions "Summarize this file's purpose in one sentence"
```

**Processing:**
1. Find all files containing "ConPTY"
2. For EACH file: Apply AI instruction
3. Output processed files grouped by repo

---

### Example 2: Extension-Specific Processing

```bash
cycodgr microsoft/semantic-kernel --file-contains "ChatClient" \
  --cs-file-instructions "Extract the class name and main methods" \
  --md-file-instructions "Extract setup instructions"
```

**Processing:**
1. Find all files containing "ChatClient"
2. For each `.cs` file: Extract class/methods
3. For each `.md` file: Extract setup instructions
4. Other file types: No AI processing

---

### Example 3: Multi-Level Processing

```bash
cycodgr microsoft/terminal microsoft/winget-cli \
  --file-contains "logging" \
  --file-instructions "What logging framework is used?" \
  --repo-instructions "Compare logging approaches" \
  --instructions "Which repo has better logging?"
```

**Processing:**
1. File-level: Each file analyzed for logging framework
2. Repo-level: Each repo's files compared
3. Global: Both repos compared
4. Output shows progression of analysis

---

### Example 4: Documentation Generation

```bash
cycodgr myorg/myproject --file-contains "public class" \
  --cs-file-instructions "Generate API documentation for public members" \
  --save-output api-docs.md
```

**Result:**
- Each C# file's public API documented
- Combined into single markdown file
- Saved to `api-docs.md`

---

## Performance Considerations

### Parallel Processing

Files processed in parallel (up to `Environment.ProcessorCount` concurrent):
- **Benefit**: Faster for many files
- **Cost**: Multiple simultaneous AI API calls

### Sequential Processing

Repos and global instructions processed sequentially:
- **Benefit**: Maintains order and context
- **Cost**: Slower for many repos

### Optimization Strategies

1. **Limit result count** (`--max-results`) to reduce AI processing
2. **Use extension-specific instructions** to process only relevant files
3. **Apply repo instructions** instead of file instructions when appropriate
4. **Batch similar instructions** (processor handles sequential application)

---

## Error Handling

### File Processing Errors

If file fetching or AI processing fails:
- Error logged
- File skipped
- Processing continues with remaining files

### Instruction Errors

If AI instruction fails:
- Error logged
- Original content returned (unprocessed)
- Processing continues

---

## Integration with Other Layers

### Layer 3 (Content Filtering)

AI sees filtered content:
- Only matched lines (+ context from Layer 5)
- Not entire file contents

### Layer 5 (Context Expansion)

Context lines included in AI input:
- More context = better AI understanding
- Less context = faster processing

### Layer 7 (Output Persistence)

AI-processed output saved to files:
- `--save-output` includes AI processing
- Other save formats (json, csv) bypass AI

---

## Related Layers

- **Layer 7**: [Output Persistence](cycodgr-search-filtering-pipeline-catalog-layer-7.md) - Saving AI-processed results
- **Layer 3**: [Content Filtering](cycodgr-search-filtering-pipeline-catalog-layer-3.md) - What content AI processes
- **Layer 5**: [Context Expansion](cycodgr-search-filtering-pipeline-catalog-layer-5.md) - Context provided to AI

---

## Source Code Evidence

See [Layer 8 Proof](cycodgr-search-filtering-pipeline-catalog-layer-8-proof.md) for detailed line-by-line source code references.
