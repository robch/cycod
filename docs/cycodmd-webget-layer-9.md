# cycodmd WebGet Command - Layer 9: Actions on Results

## Overview

**Layer**: 9 - Actions on Results  
**Command**: `cycodmd web get <urls...>` (WebGetCommand)  
**Purpose**: Fetch and process web pages from direct URLs.

Layer 9 for the WebGet command implements a **direct fetch** model where the primary action is always fetching web page content, since URLs are explicitly provided. Unlike WebSearch which needs `--get` to fetch content, WebGet's purpose IS to fetch content.

## Command-Line Options

### Implicit Action

WebGet has **no explicit action flag** because fetching content is its core purpose:

- **WebSearch**: URLs are search results → `--get` decides whether to fetch content
- **WebGet**: URLs are direct inputs → always fetches content (no flag needed)

### Supporting Options (Inherited from WebCommand)

#### `--interactive`
**Purpose**: Open browser interactively for manual navigation  
**Type**: Boolean flag  
**Parsed At**: `CycoDmdCommandLineOptions.cs:313-316`  
**Stored In**: `WebCommand.Interactive`

Enables interactive browser mode where user can manually navigate before content extraction.

**Example**:
```bash
cycodmd web get https://example.com --interactive
```

#### `--save-page-folder <directory>`
**Purpose**: Save fetched web pages to local directory  
**Type**: String (directory path)  
**Parsed At**: `CycoDmdCommandLineOptions.cs:333-338`  
**Stored In**: `WebCommand.SaveFolder`  
**Default**: `"web-pages"`

Saves each fetched page to the specified directory for offline access.

**Example**:
```bash
cycodmd web get https://example.com --save-page-folder my-pages
```

#### `--strip`
**Purpose**: Strip HTML and extract text content only  
**Type**: Boolean flag  
**Parsed At**: `CycoDmdCommandLineOptions.cs:329-332`  
**Stored In**: `WebCommand.StripHtml`

Removes HTML markup and extracts plain text.

**Example**:
```bash
cycodmd web get https://example.com --strip
```

#### Browser Selection
- `--chromium`: Use Chromium browser (default)
- `--firefox`: Use Firefox browser
- `--webkit`: Use WebKit browser

**Parsed At**: `CycoDmdCommandLineOptions.cs:317-328`  
**Stored In**: `WebCommand.Browser`

## Data Flow

### 1. Option Parsing

```
User Input: cycodmd web get URL1 URL2 --interactive --save-page-folder pages
         ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()
    (parses positional URL arguments)
         ↓
CycoDmdCommandLineOptions.TryParseWebCommandOptions()
    (parses options)
         ↓
WebGetCommand.Urls = [URL1, URL2]
WebCommand.Interactive = true
WebCommand.SaveFolder = "pages"
```

### 2. Execution Flow

```
WebGetCommand.ExecuteAsync()
         ↓
Program.ProcessWebGetCommand()
         ↓
For each URL in command.Urls:
    ↓
    GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, ...)
         ↓
    GetFinalWebPageContentAsync(...)
         ↓
    GetFormattedWebPageContentAsync(...)
         ↓
    PlaywrightHelpers.GetPageAndTitle(url, stripHtml, saveToFolder, browserType, interactive)
         ↓
    Return content + title
```

## Integration with Other Layers

### Dependencies

Layer 9 depends on earlier layers:

- **Layer 1 (Target Selection)**: URLs are the targets
  - Uses `Urls` list (positional arguments)
  - No search provider needed (URLs are explicit)

- **Layer 2 (Container Filter)**: Not applicable
  - WebGet doesn't filter containers (all specified URLs are fetched)
  - No `ExcludeURLContainsPatternList` check (unlike WebSearch)

- **Layer 6 (Display Control)**: Affects content processing
  - Uses `Browser` type for rendering
  - Uses `StripHtml` to extract text
  - Uses `Interactive` for manual navigation

- **Layer 7 (Output Persistence)**: Saves fetched content
  - Uses `SaveFolder` to store pages locally
  - Uses `SavePageOutput` for AI-processed output

- **Layer 8 (AI Processing)**: Processes fetched content
  - Uses `PageInstructionsList` for per-page instructions
  - Uses `InstructionsList` for general instructions

## Execution Evidence

See [Layer 9 Proof Document](cycodmd-webget-layer-9-proof.md) for detailed source code references showing:

- URL parsing from positional arguments
- Property storage in `WebGetCommand`
- Execution path for fetching web pages
- Shared infrastructure with `WebSearchCommand`
- Integration with AI processing (Layer 8)

## Examples

### Example 1: Fetch Single URL

```bash
cycodmd web get https://example.com
```

**Result**: Fetches and displays content of the specified page.

**Layer 9 Action**: Direct fetch (always happens for WebGet).

---

### Example 2: Fetch Multiple URLs

```bash
cycodmd web get https://example1.com https://example2.com https://example3.com
```

**Result**: Fetches and displays content of all three pages.

**Layer 9 Action**: Sequential fetch of each URL.

---

### Example 3: Strip HTML

```bash
cycodmd web get https://example.com --strip
```

**Result**: Fetches page and extracts plain text (removes HTML markup).

**Layer 9 Action**: Fetch with HTML stripping.

---

### Example 4: Save Pages Locally

```bash
cycodmd web get https://example.com https://docs.example.com --save-page-folder docs
```

**Result**: Fetches pages and saves them to `./docs/` directory for offline access.

**Layer 9 Action**: Fetch and persist locally.

---

### Example 5: Interactive Fetch

```bash
cycodmd web get https://example.com --interactive
```

**Result**: Opens browser visibly, allows manual navigation/login, then fetches content.

**Layer 9 Action**: Interactive fetch with user control.

---

### Example 6: Fetch with AI Processing

```bash
cycodmd web get https://research-paper.com --strip --instructions "Summarize key findings"
```

**Result**: Fetches page, strips HTML, then asks AI to summarize.

**Layer 9 Action**: Fetch for AI processing (Layer 8).

## Behavioral Notes

### Always-Fetch Design

- WebGet **always fetches content** (no `--get` flag needed)
- Rationale: User explicitly provided URLs, intent is clear
- Contrast with WebSearch where URLs are search results (user might only want URLs)

### No Container Filtering

- WebGet fetches **all specified URLs**
- No `ExcludeURLContainsPatternList` check
- User has full control via input URLs
- If you don't want a URL fetched, don't provide it

### Sequential Processing

- URLs are processed **one at a time** (sequential)
- Not parallel to avoid rate limiting issues
- Respects website politeness guidelines
- User sees progress as each URL is fetched

### Error Handling

- If one URL fails, others continue
- Each URL is independent
- Errors are reported per URL
- Exit code reflects overall success/failure

## Comparison with WebSearch Layer 9

| Aspect | WebSearch | WebGet |
|--------|-----------|--------|
| **URL Source** | Search results | Direct input |
| **Fetch Flag** | `--get` required | Always fetches (no flag) |
| **Container Filter** | `--exclude` patterns | Not applicable |
| **Default Behavior** | Show URLs only | Fetch content |
| **Use Case** | Discover URLs | Process known URLs |
| **Layer 2 Integration** | Strong (filters URLs) | None (all URLs fetched) |

## Safety Considerations

### No Destructive Actions

WebGet Layer 9:
- ✅ Never modifies local files
- ✅ Only fetches remote content
- ✅ Saves to separate folder (doesn't overwrite)
- ✅ Read-only operations

### Rate Limiting

**Considerations**:
- Respects robots.txt
- Sequential fetching (not parallel)
- Delays between requests
- Browsers handle rate limiting naturally

### Bandwidth Usage

**Warning**: Fetching many large pages can consume significant bandwidth.

**Mitigation**: Only provide URLs you actually want to process.

## Design Philosophy

### Explicit Intent

WebGet assumes **explicit user intent**:

1. User provides specific URLs
2. User wants content from those URLs
3. No need for confirmation or flags
4. Just fetch and display/process

This differs from WebSearch's **discover-then-decide** model:

1. Search finds potential URLs
2. User decides if content is needed
3. `--get` flag confirms intent
4. Fetching is opt-in

### Simplicity

WebGet is the **simplest** cycodmd web command:

- No search provider configuration
- No result filtering
- No ambiguity about intent
- Direct URL → Content mapping

## Implementation Notes

### Shared Infrastructure

**Evidence**: WebGet uses the same fetching methods as WebSearch:

```
GetCheckSaveWebPageContentAsync()
    ↓
GetFinalWebPageContentAsync()
    ↓
GetFormattedWebPageContentAsync()
    ↓
PlaywrightHelpers.GetPageAndTitle()
```

**Benefit**: Code reuse, consistent behavior, shared bug fixes.

### Positional Arguments

**Evidence** (`CycoDmdCommandLineOptions.cs:472-475`):
```csharp
else if (command is WebGetCommand webGetCommand)
{
    webGetCommand.Urls.Add(arg);
    parsedOption = true;
}
```

**Analysis**: All non-option arguments are treated as URLs.

**User Experience**: Natural CLI usage:
```bash
cycodmd web get URL1 URL2 URL3
# vs.
cycodmd web get --url URL1 --url URL2 --url URL3
```

## Related Layers

- **[Layer 1: Target Selection](cycodmd-webget-layer-1.md)** - URL input
- **[Layer 6: Display Control](cycodmd-webget-layer-6.md)** - Browser, stripping
- **[Layer 7: Output Persistence](cycodmd-webget-layer-7.md)** - Saving pages
- **[Layer 8: AI Processing](cycodmd-webget-layer-8.md)** - Processing content
- **[WebSearch Layer 9](cycodmd-websearch-layer-9.md)** - Comparison with search-based fetching

## See Also

- [Layer 9 Proof Document](cycodmd-webget-layer-9-proof.md) - Detailed source code evidence
- [WebGet Command Overview](cycodmd-webget-filtering-pipeline-catalog-README.md)
- [WebSearch Layer 9](cycodmd-websearch-layer-9.md) - Related command
- [FindFiles Layer 9](cycodmd-files-layer-9.md) - File modification actions
