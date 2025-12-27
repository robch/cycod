# cycodmd WebSearch - Layer 7: Output Persistence

**[← Back to WebSearch Overview](cycodmd-filtering-pipeline-catalog-README.md#2-web-search)** | **[Proof →](cycodmd-websearch-layer-7-proof.md)**

## Purpose

Layer 7 (Output Persistence) for WebSearch controls **where and how web search results are saved** to files. This layer handles:
- Saving combined search results to a single file
- Saving per-page content to multiple files using templates
- Saving raw web pages to a folder for offline access
- Saving AI chat history when AI processing is used

## Command-Line Options

### `--save-output [file]`

**Type**: Shared option (all commands)  
**Default**: `output.md`  
**Purpose**: Save the combined markdown output of all search results to a file

**Behavior**:
- If no value provided, uses default `output.md`
- Creates or overwrites the specified file
- Contains all processed web page content in markdown format
- Applied after all filtering, content extraction, and processing layers

**Examples**:
```bash
# Save search results to default output.md
cycodmd web search "terminal emulator" --get --save-output

# Save to custom file
cycodmd web search "AI safety" --get --strip --save-output ai-safety-research.md

# Organized by search topic
cycodmd web search "quantum computing" --save-output "research/quantum-$(date +%Y%m%d).md"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 427-432 (shared option parsing)

---

### `--save-page-output [template]`

**Type**: WebCommand-specific option  
**Aliases**: `--save-web-output`, `--save-web-page-output`  
**Default**: `{filePath}/{fileBase}-output.md`  
**Purpose**: Save processed content for each web page separately using a template

**Template Variables**:
- `{filePath}`: Output directory path (typically derived from URL or page metadata)
- `{fileBase}`: Page identifier (URL slug or sanitized page title)
- `{fileName}`: Full output filename
- Other variables may be supported

**Behavior**:
- Creates one output file per retrieved web page
- Uses template to generate output filenames
- Each output file contains only the content for that specific web page
- Useful for batch processing multiple search results

**Examples**:
```bash
# Save each page's output separately
cycodmd web search "documentation" --get --save-page-output

# Custom template - save to organized directory
cycodmd web search "API reference" --get \
  --save-page-output "docs/{fileBase}-content.md"

# Save with URL-based naming
cycodmd web search "tutorials" --get \
  --save-page-output "pages/{fileBase}.md"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 394-400 (WebCommand option parsing)

---

### `--save-page-folder [directory]`

**Type**: WebCommand-specific option  
**Default**: `web-pages`  
**Purpose**: Save raw/original web pages (HTML or converted) to a folder

**Behavior**:
- Downloads and saves web pages before processing
- Preserves original page content for offline access
- Creates specified directory if it doesn't exist
- Filename generation is automatic (based on URL or page ID)
- Useful for archiving or offline browsing

**Examples**:
```bash
# Save pages to default web-pages/ folder
cycodmd web search "documentation" --get --save-page-folder

# Custom folder
cycodmd web search "references" --get --save-page-folder "archive/references"

# Organized by date
cycodmd web search "news" --get --save-page-folder "archive/$(date +%Y-%m-%d)"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 333-338 (WebCommand option parsing)

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
- Only meaningful when AI processing is used (via `--instructions`)
- Useful for debugging AI behavior on web content
- Can be used to review AI analysis decisions

**Examples**:
```bash
# Save AI analysis history
cycodmd web search "machine learning" --get \
  --page-instructions "Summarize key concepts" \
  --save-chat-history

# Save to specific file
cycodmd web search "research papers" --get \
  --instructions "Extract methodology" \
  --save-chat-history ml-analysis.jsonl

# Organized by search topic
cycodmd web search "AI ethics" --get \
  --instructions "Identify concerns" \
  --save-chat-history "logs/ethics-$(date +%Y%m%d).jsonl"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 434-440 (shared option parsing)

---

## Option Interactions

### Precedence and Combination

1. **`--save-output` + `--save-page-output`**: Both can be used together
   - `--save-output`: Contains all pages combined
   - `--save-page-output`: Creates individual files per page

2. **`--save-page-output` + `--save-page-folder`**: Different purposes
   - `--save-page-folder`: Saves raw/original pages (HTML)
   - `--save-page-output`: Saves processed/converted content (markdown)

3. **`--get` Required for Content Saving**:
   - Without `--get`, only search results (URLs/titles) are collected
   - With `--get`, page content is fetched and can be saved

4. **AI Processing Required for `--save-chat-history`**:
   - Only saves history if `--instructions` or `--page-instructions` are used
   - Otherwise, no chat history to save

### Common Patterns

```bash
# Complete web research workflow: search + fetch + archive + analyze
cycodmd web search "AI research 2024" --get --max 20 \
  --save-output research-summary.md \
  --save-page-output "analysis/{fileBase}-content.md" \
  --save-page-folder "archive/ai-research" \
  --page-instructions "Extract key findings and methodology" \
  --save-chat-history ai-research-analysis.jsonl

# Quick archival: save pages for offline reading
cycodmd web search "documentation" --get \
  --save-page-folder "offline-docs" \
  --save-output "offline-docs/index.md"
```

## Data Flow

### Input to Layer 7
From **Layer 6 (Display Control)**:
- Formatted page content (markdown or HTML)
- Page metadata (URLs, titles)
- Search results
- AI analysis (if Layer 8 was used)

From **Layer 8 (AI Processing)**:
- AI-generated summaries or analysis
- Chat history (if AI was invoked)

### Processing in Layer 7
1. **Collect all processed pages**
2. **Apply output templates** (for `--save-page-output`)
3. **Generate output filenames**:
   - Sanitize URLs to valid filenames
   - Expand template variables
4. **Write files to disk**:
   - Combined output (`--save-output`)
   - Per-page output (`--save-page-output`)
   - Raw pages (`--save-page-folder`)
   - Chat history (`--save-chat-history`)
5. **Ensure directories exist** (create if needed)

### Output from Layer 7
- Files written to disk
- Status messages (if not `--quiet`)
- Error messages (if downloads or writes fail)

## Integration with Other Layers

### Dependencies (Inputs)
- **Layer 1**: Search results (URLs, titles) must be collected
- **Layer 2-6**: Page content must be fetched and processed
- **Layer 8**: If AI processing occurred, chat history is available

### Influences (Outputs)
- **External Tools**: Saved files can be consumed by other processes
- **Offline Access**: Archived pages enable offline browsing
- **Review/Audit**: Chat history enables AI decision review

## Implementation Details

### URL to Filename Sanitization
- Remove or replace invalid filename characters (`/`, `\`, `:`, `*`, `?`, `"`, `<`, `>`, `|`)
- Truncate long URLs to reasonable filename length
- Use URL slug or page title when available

### File Writing
- Uses `File.WriteAllText()` or similar for output
- Creates parent directories if they don't exist
- Overwrites existing files without warning
- No atomic write guarantees (may leave partial files on crash)

### Template Expansion
- Template variables are replaced at write time
- Variables come from page metadata (URL, title, timestamp)
- Unknown variables are left as-is

### Error Handling
- If output directory is not writable, error is logged
- Failed file writes do not stop processing of other pages
- Download failures are logged but may not prevent saving partial content

## Related Options

### Global Options Affecting Output
- `--quiet`: Suppresses output file creation messages
- `--verbose`: Shows detailed file writing progress
- `--working-dir`: Changes base directory for relative output paths

### Layer 1-2 Options Affecting Output
- `--get`: Enables content fetching (required for meaningful output)
- `--strip`: Converts HTML to clean text before saving
- `--max`: Limits number of pages, affecting output file count

### Layer 8 Options Affecting Output
- `--instructions`: Enables AI processing, making `--save-chat-history` meaningful
- `--page-instructions`: Applies AI per-page, reflected in output and chat history

## Examples

### Example 1: Basic Search Result Saving
```bash
cycodmd web search "open source terminal" --get --save-output terminals.md
```

**Result**: Creates `terminals.md` with content from all retrieved pages.

---

### Example 2: Archival for Offline Access
```bash
cycodmd web search "API documentation" --get --max 10 \
  --save-page-folder "docs-archive" \
  --save-output "docs-archive/index.md"
```

**Result**: 
- Saves raw pages to `docs-archive/`
- Creates `docs-archive/index.md` with combined content

---

### Example 3: Research with AI Analysis
```bash
cycodmd web search "climate change research 2024" --get --max 15 \
  --strip \
  --page-instructions "Extract key findings, methodology, and conclusions" \
  --save-output climate-research-summary.md \
  --save-page-output "analysis/{fileBase}-analysis.md" \
  --save-chat-history climate-ai-analysis.jsonl
```

**Result**: Creates:
1. `climate-research-summary.md` - Combined AI-analyzed content
2. `analysis/*-analysis.md` - Per-page AI analysis
3. `climate-ai-analysis.jsonl` - AI interaction log

---

### Example 4: Multiple Output Formats
```bash
cycodmd web search "tutorials" --get \
  --save-page-folder "raw-pages" \
  --save-page-output "processed/{fileBase}.md" \
  --save-output "all-tutorials.md"
```

**Result**:
- Raw HTML pages in `raw-pages/`
- Processed markdown in `processed/`
- Combined summary in `all-tutorials.md`

---

## See Also

- **[Layer 1: Target Selection](cycodmd-websearch-layer-1.md)** - Defines what to search
- **[Layer 2: Container Filter](cycodmd-websearch-layer-2.md)** - Filters URLs
- **[Layer 6: Display Control](cycodmd-websearch-layer-6.md)** - Formats content before saving
- **[Layer 8: AI Processing](cycodmd-websearch-layer-8.md)** - Generates AI analysis to save
- **[Proof Document](cycodmd-websearch-layer-7-proof.md)** - Source code evidence and implementation details

---

**[← Back to WebSearch Overview](cycodmd-filtering-pipeline-catalog-README.md#2-web-search)** | **[Proof →](cycodmd-websearch-layer-7-proof.md)**
