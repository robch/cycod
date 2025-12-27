# cycodmd Web Search - Layer 8: AI Processing

## Purpose

Layer 8 (AI Processing) provides AI-assisted analysis and transformation of web search results. This layer allows users to apply natural language instructions to web page content, enabling automated summarization, extraction, analysis, or any other AI-driven content transformation of web search results.

## Position in Pipeline

Layer 8 occurs **after** web pages are fetched and optionally stripped of HTML, but **before** final output display. It can operate at two levels: per-page and globally across all results.

**Pipeline Flow:**
```
Layer 1: Web Search (find URLs)
    ↓
Layer 2: URL Filtering (--exclude)
    ↓
Fetch Page Content (if --get)
    ↓
Strip HTML (if --strip)
    ↓
Layer 8: AI Processing (THIS LAYER)
    ↓
Display to Console or Save to File
```

## Command-Line Options

### General AI Instructions

#### `--instructions <instruction>`
**Purpose**: Apply general AI instructions to **all combined output** (search results + all page content)  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: After all pages are processed and combined  
**Example**:
```bash
cycodmd web search "async patterns" --get --instructions "Summarize the key concepts"
cycodmd web search "python tutorials" --get --strip --instructions "Create a learning roadmap"
```

### Page-Specific AI Instructions

#### `--page-instructions <instruction>`
**Purpose**: Apply AI instructions to **each web page individually**  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-page, after content is fetched and optionally stripped  
**Example**:
```bash
cycodmd web search "documentation" --get --page-instructions "Extract the main topic"
cycodmd web search "API reference" --get --page-instructions "List all endpoints"
```

**Special Variables**: Page instructions support template variables:
- `{searchTerms}`, `{query}`, `{terms}`, `{q}` - Replaced with the search query

```bash
cycodmd web search "C# async" --get \
  --page-instructions "How does this page relate to '{searchTerms}'?"
```

### URL-Pattern-Specific AI Instructions

#### `--{pattern}-page-instructions <instruction>`
**Purpose**: Apply AI instructions only to pages matching a URL pattern  
**Pattern**: `--{url-pattern}-page-instructions` where `{url-pattern}` can be any substring  
**Type**: Multi-value (can be specified multiple times)  
**When Applied**: Per-page, only for pages whose URLs contain the pattern  
**Example**:
```bash
cycodmd web search "programming" --get \
  --github-page-instructions "Extract repository statistics" \
  --stackoverflow-page-instructions "Extract the accepted answer" \
  --docs-page-instructions "Create a summary of key points"
```

**Matching**: Case-insensitive substring matching on URLs.

### AI Function Access

#### `--built-in-functions`
**Purpose**: Enable AI to use built-in functions (if supported by the AI tool)  
**Type**: Boolean flag  
**Default**: `false`  
**Example**:
```bash
cycodmd web search "tutorials" --get --page-instructions "Extract links" --built-in-functions
```

### Chat History Persistence

#### `--save-chat-history [filename]`
**Purpose**: Save AI interaction history to a file for debugging/review  
**Type**: Optional value  
**Default**: `chat-history-{time}.jsonl` (if flag specified without value)  
**Example**:
```bash
cycodmd web search "AI models" --get \
  --page-instructions "Summarize" \
  --save-chat-history web-analysis.jsonl
```

## Implementation Details

### Two-Level Processing

AI processing in cycodmd web search operates at **two distinct levels**:

1. **Per-Page Processing** (`--page-instructions`, `--{pattern}-page-instructions`)
   - Applied to each web page **individually** after content is fetched
   - Filtered by URL pattern if using pattern-specific instructions
   - Runs for each URL in search results (if `--get` is enabled)

2. **Global Processing** (`--instructions`)
   - Applied to **all combined output** (search results list + all page content)
   - Runs after all per-page processing is complete
   - Delays console output until processing is finished

### Processing Order

```
1. Execute web search (Layer 1)
2. Filter URLs by --exclude patterns (Layer 2)
3. Display search results
   ↓
IF --get flag:
4. Fetch each page content
5. Strip HTML if --strip flag
6. Apply per-page AI instructions (--page-instructions, --{pattern}-page-instructions)
7. Save individual pages if --save-page-folder
   ↓
8. Combine all outputs (search results + all page content)
   ↓
IF --instructions:
9. Apply global AI instructions
   ↓
10. Display final output or save to --save-output
```

### Automatic Content Fetching

**Important Behavior**: If any instructions are specified (`--instructions`, `--page-instructions`, or `--{pattern}-page-instructions`) but `--get` is not explicitly specified, the command automatically:
1. Enables `--get` (fetches page content)
2. Enables `--strip` (strips HTML from pages)

**Source** (`WebSearchCommand.cs:24-33`):
```csharp
var noContent = !GetContent;
var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();

var assumeGetAndStrip = noContent && hasInstructions;
if (assumeGetAndStrip)
{
    GetContent = true;
    StripHtml = true;
}
```

**Example**:
```bash
# These two commands are equivalent:
cycodmd web search "tutorials" --page-instructions "Summarize"
cycodmd web search "tutorials" --get --strip --page-instructions "Summarize"
```

### URL Pattern Matching

Pattern-specific instructions use **case-insensitive substring matching**:

**Matching Logic** (from `Program.cs:663-666`):
```csharp
var instructionsForThisPage = pageInstructionsList
    .Where(x => WebPageMatchesInstructionsCriteria(url, x.Item2))
    .Select(x => x.Item1)
    .ToList();
```

**Examples**:
- `--github-page-instructions` matches URLs containing "github" (e.g., `https://github.com/...`)
- `--docs-page-instructions` matches URLs containing "docs" (e.g., `https://docs.microsoft.com/...`)
- Empty pattern (`--page-instructions`) matches **all** pages

### Template Variable Substitution

Page instructions support template variables that are replaced with the search query:

**Variables**:
- `{searchTerms}`
- `{query}`
- `{terms}`
- `{q}`

All are replaced with the same value: the joined search terms.

**Source** (`Program.cs:283-291`):
```csharp
var pageInstructionsList = command.PageInstructionsList
    .Select(x => Tuple.Create(
        x.Item1
            .Replace("{searchTerms}", query)
            .Replace("{query}", query)
            .Replace("{terms}", query)
            .Replace("{q}", query),
        x.Item2.ToLowerInvariant()))
    .ToList();
```

### AI Tool Integration

Like file search, web search AI processing uses:
- **Primary**: `cycod` (if configured)
- **Fallback**: `ai` (legacy tool)

### Instruction Chaining

Multiple instructions are applied **sequentially** (not in parallel), with each instruction's output becoming the next instruction's input.

## Data Flow

### Input to Layer 8
- **Search Results**: List of URLs from web search
- **Page Content**: HTML or stripped text from each page (if `--get`)
- **Instruction Lists**: General, page-specific, and pattern-specific instructions

### Output from Layer 8
- **Transformed Content**: AI-processed search results and/or page content
- **Error Messages**: If AI processing fails
- **Chat History**: If `--save-chat-history` specified

## Example Usage

### Basic Per-Page Analysis
```bash
cycodmd web search "machine learning tutorials" \
  --page-instructions "What skill level is this tutorial aimed at?"
```

### Pattern-Specific Processing
```bash
cycodmd web search "python frameworks" --get \
  --github-page-instructions "Extract star count and description" \
  --docs-page-instructions "List installation steps" \
  --medium-page-instructions "Extract the key takeaways"
```

### Combined Per-Page and Global Processing
```bash
cycodmd web search "best practices" --get \
  --page-instructions "Extract main recommendations" \
  --instructions "Combine into a unified best practices guide"
```

### Using Search Query Variables
```bash
cycodmd web search "async patterns" --get \
  --page-instructions "Rate how relevant this page is to '{searchTerms}' on a scale of 1-10"
```

### With Chat History for Debugging
```bash
cycodmd web search "API documentation" --get \
  --page-instructions "Extract code examples" \
  --save-chat-history api-extraction.jsonl
```

### Complex Multi-Stage Processing
```bash
cycodmd web search "software architecture" --get --strip \
  --page-instructions "Extract architectural patterns mentioned" \
  --instructions "Group by pattern type" \
  --instructions "Create a comparison matrix"
```

### Automatic --get and --strip
```bash
# These are equivalent:
cycodmd web search "tutorials" --page-instructions "Summarize"
cycodmd web search "tutorials" --get --strip --page-instructions "Summarize"
```

## Differences from File Search

### URL Pattern Matching vs Extension Matching
- **File Search**: Uses file extensions (e.g., `--cs-file-instructions`)
- **Web Search**: Uses URL substring patterns (e.g., `--github-page-instructions`)

### Automatic Content Fetching
- **File Search**: Always has content (files on disk)
- **Web Search**: May only have URLs; instructions trigger automatic `--get --strip`

### Template Variables
- **File Search**: No template variables
- **Web Search**: Supports `{searchTerms}` and variants in page instructions

## Related Layers

- **Layer 1 (Target Selection)**: Web search that provides initial URLs
- **Layer 2 (Container Filter)**: URL exclusion patterns
- **Layer 7 (Output Persistence)**: Saves individual page outputs and combined results
- **Display/Console Output**: Delayed until after AI processing completes

## See Also

- [Layer 8 Proof Document](cycodmd-websearch-layer-8-proof.md) - Source code evidence
- [Layer 7: Output Persistence](cycodmd-websearch-layer-7.md) - Saving results
- [Layer 1: Target Selection](cycodmd-websearch-layer-1.md) - Web search execution
