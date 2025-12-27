# cycodmd Web Get - Layer 8: AI Processing

## Purpose

Layer 8 (AI Processing) provides AI-assisted analysis and transformation of web page content fetched from specific URLs. This layer allows users to apply natural language instructions to web pages, enabling automated summarization, extraction, analysis, or any other AI-driven content transformation.

## Position in Pipeline

Layer 8 occurs **after** web pages are fetched and optionally stripped of HTML, but **before** final output display.

**Pipeline Flow:**
```
Layer 1: URL Collection (URLs provided as arguments)
    ↓
Fetch Page Content
    ↓
Strip HTML (if --strip)
    ↓
Layer 8: AI Processing (THIS LAYER)
    ↓
Display to Console or Save to File
```

## Command-Line Options

Web Get supports the **same AI processing options** as Web Search:

### General AI Instructions

#### `--instructions <instruction>`
**Purpose**: Apply general AI instructions to **all combined output** (all page content)  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: After all pages are processed and combined  
**Example**:
```bash
cycodmd web get https://example.com/doc1 https://example.com/doc2 \
  --instructions "Compare these two documents"
```

### Page-Specific AI Instructions

#### `--page-instructions <instruction>`
**Purpose**: Apply AI instructions to **each web page individually**  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-page, after content is fetched and optionally stripped  
**Example**:
```bash
cycodmd web get https://docs.example.com/api \
  --page-instructions "Extract all API endpoints"
```

### URL-Pattern-Specific AI Instructions

#### `--{pattern}-page-instructions <instruction>`
**Purpose**: Apply AI instructions only to pages matching a URL pattern  
**Pattern**: `--{url-pattern}-page-instructions` where `{url-pattern}` can be any substring  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-page, only for pages whose URLs contain the pattern  
**Example**:
```bash
cycodmd web get \
  https://github.com/user/repo1 \
  https://github.com/user/repo2 \
  https://docs.microsoft.com/api \
  --github-page-instructions "Extract repository description" \
  --docs-page-instructions "List all code examples"
```

### AI Function Access

#### `--built-in-functions`
**Purpose**: Enable AI to use built-in functions (if supported by the AI tool)  
**Type**: Boolean flag  
**Default**: `false`

### Chat History Persistence

#### `--save-chat-history [filename]`
**Purpose**: Save AI interaction history to a file for debugging/review  
**Type**: Optional value  
**Default**: `chat-history-{time}.jsonl` (if flag specified without value)

## Implementation Details

### Identical to Web Search

Web Get uses the **exact same** AI processing implementation as Web Search. The only difference is:
- **Web Search**: Includes a search results section (list of URLs) before page content
- **Web Get**: Only includes page content (no search results section)

**All AI processing features are identical:**
- Per-page and global instruction processing
- URL pattern-based filtering
- Case-insensitive substring matching
- Instruction chaining
- AI tool integration

### Two-Level Processing

1. **Per-Page Processing** (`--page-instructions`, `--{pattern}-page-instructions`)
   - Applied to each web page **individually** after content is fetched
   - Filtered by URL pattern if using pattern-specific instructions

2. **Global Processing** (`--instructions`)
   - Applied to **all combined page content**
   - Runs after all per-page processing is complete
   - Delays console output until processing is finished

### Processing Order

```
1. Collect URLs from command-line arguments
2. For each URL:
   a. Fetch page content
   b. Strip HTML if --strip flag
   c. Apply per-page AI instructions (--page-instructions, --{pattern}-page-instructions)
   d. Save individual page if --save-page-folder
3. Combine all page outputs
4. Apply global AI instructions (--instructions) if present
5. Display final output or save to --save-output
```

### URL Pattern Matching

Pattern-specific instructions use **case-insensitive substring matching**:

**Examples**:
- `--github-page-instructions` matches URLs containing "github"
- `--docs-page-instructions` matches URLs containing "docs"
- `--api-page-instructions` matches URLs containing "api"
- Empty pattern (`--page-instructions`) matches **all** pages

**Matching Logic**:
```csharp
var lowerUrl = url.ToLowerInvariant();
var lowerCriteria = criteria.ToLowerInvariant();
return lowerUrl.Contains(lowerCriteria);
```

### No Template Variable Substitution

Unlike Web Search, Web Get does **not** support template variables like `{searchTerms}` because there is no search query. Instructions are used as-is.

## Data Flow

### Input to Layer 8
- **Page Content**: HTML or stripped text from each page
- **Instruction Lists**: General, page-specific, and pattern-specific instructions
- **URL Metadata**: Used for pattern matching

### Output from Layer 8
- **Transformed Content**: AI-processed page content
- **Error Messages**: If AI processing fails
- **Chat History**: If `--save-chat-history` specified

## Example Usage

### Basic Per-Page Analysis
```bash
cycodmd web get https://example.com/tutorial \
  --page-instructions "Summarize this tutorial in 3 bullet points"
```

### Pattern-Specific Processing
```bash
cycodmd web get \
  https://github.com/microsoft/terminal \
  https://github.com/wezterm/wezterm \
  https://docs.microsoft.com/terminal \
  --github-page-instructions "Extract star count and last update date" \
  --docs-page-instructions "Extract installation instructions"
```

### Combined Per-Page and Global Processing
```bash
cycodmd web get \
  https://site1.com/article1 \
  https://site2.com/article2 \
  https://site3.com/article3 \
  --page-instructions "Extract the main thesis" \
  --instructions "Compare and contrast the three theses"
```

### With Chat History for Debugging
```bash
cycodmd web get https://api-docs.example.com \
  --page-instructions "Extract all API endpoints and their parameters" \
  --save-chat-history api-extraction.jsonl
```

### Complex Multi-Stage Processing
```bash
cycodmd web get https://blog.example.com/post1 https://blog.example.com/post2 \
  --page-instructions "Extract key points" \
  --instructions "Create a unified summary" \
  --instructions "Identify common themes"
```

### Strip HTML for Cleaner AI Processing
```bash
cycodmd web get https://example.com/page \
  --strip \
  --page-instructions "Analyze the content structure"
```

## Differences from Web Search

### No Search Results Section
- **Web Search**: Outputs search results list + page content
- **Web Get**: Outputs only page content

### No Template Variables
- **Web Search**: Supports `{searchTerms}`, `{query}`, `{terms}`, `{q}`
- **Web Get**: No template variables (no search query exists)

### URL Source
- **Web Search**: URLs come from search engine results
- **Web Get**: URLs come from command-line arguments

## Differences from File Search

### URL Pattern Matching vs Extension Matching
- **File Search**: Uses file extensions (e.g., `--cs-file-instructions`)
- **Web Get**: Uses URL substring patterns (e.g., `--github-page-instructions`)

### Content Source
- **File Search**: Reads files from disk
- **Web Get**: Fetches content from web via HTTP/browser

### Content Type
- **File Search**: Any file type (binary, text, images, etc.)
- **Web Get**: HTML/web content (optionally stripped)

## Related Layers

- **Layer 1 (Target Selection)**: URLs provided as command-line arguments
- **Layer 7 (Output Persistence)**: Saves individual page outputs and combined results
- **Display/Console Output**: Delayed until after AI processing completes

## Implementation Reference

Web Get uses the **same implementation** as Web Search for AI processing:
- **Handler**: `HandleWebGetCommand` (Program.cs:327-364)
- **Per-Page Processing**: `GetCheckSaveWebPageContentAsync` (Program.cs:636-656)
- **Final Content**: `GetFinalWebPageContentAsync` (Program.cs:659-673)
- **Pattern Matching**: `WebPageMatchesInstructionsCriteria` (Program.cs:675-685)
- **AI Processor**: `AiInstructionProcessor` (AiInstructionProcessor.cs)

The only difference in execution flow is that Web Get skips the search step.

## See Also

- [Layer 8 Proof Document](cycodmd-webget-layer-8-proof.md) - Source code evidence
- [Web Search Layer 8](cycodmd-websearch-layer-8.md) - Detailed explanation (Web Get is identical)
- [Web Search Layer 8 Proof](cycodmd-websearch-layer-8-proof.md) - Complete source code proof
- [Layer 7: Output Persistence](cycodmd-webget-layer-7.md) - Saving results
- [Layer 1: Target Selection](cycodmd-webget-layer-1.md) - URL collection
