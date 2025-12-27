# cycodmd WebSearch Command - Layer 9: Actions on Results

## Overview

**Layer**: 9 - Actions on Results  
**Command**: `cycodmd web search <terms...>` (WebSearchCommand)  
**Purpose**: Execute actions on search results, specifically fetching full web page content.

Layer 9 for the WebSearch command implements an **implicit action model** where the primary action is fetching and optionally saving web page content. Unlike the FindFiles command which has explicit replace/execute options, WebSearch's Layer 9 is about whether to retrieve full page content beyond just search result URLs.

## Command-Line Options

### Core Action Options

#### `--get`
**Purpose**: Fetch full content of web pages from search results  
**Type**: Boolean flag  
**Parsed At**: `CycoDmdCommandLineOptions.cs:363-366`  
**Stored In**: `WebCommand.GetContent`

When present, this flag causes the command to fetch the actual content of web pages found in search results, not just display search result URLs.

**Example**:
```bash
# Without --get: Just shows search result links
cycodmd web search "C# async patterns"

# With --get: Fetches and displays full page content
cycodmd web search "C# async patterns" --get
```

#### Auto-Implied by AI Instructions

**Evidence**: `WebSearchCommand.Validate()` method

If `--instructions` or `--page-instructions` is provided without `--get`, the command automatically sets `GetContent = true` and `StripHtml = true` because AI processing requires actual content, not just URLs.

**Example**:
```bash
# Implicitly enables --get and --strip
cycodmd web search "C# patterns" --instructions "Summarize"
```

### Related Options (From Layer 6)

#### `--interactive`
**Purpose**: Open browser interactively for manual navigation  
**Type**: Boolean flag  
**Stored In**: `WebCommand.Interactive`

Enables interactive browser mode where user can manually navigate before content extraction.

#### `--save-page-folder <directory>`
**Purpose**: Save fetched web pages to local directory  
**Type**: String (directory path)  
**Stored In**: `WebCommand.SaveFolder`  
**Default**: `"web-pages"`

Saves each fetched page to the specified directory for offline access.

## Data Flow

### 1. Option Parsing

```
User Input: --get --save-page-folder my-pages
         ↓
CycoDmdCommandLineOptions.TryParseWebCommandOptions()
         ↓
WebCommand.GetContent = true
WebCommand.SaveFolder = "my-pages"
```

### 2. Validation Flow (Auto-Enable)

```
WebSearchCommand.Validate()
         ↓
Check: !GetContent && (PageInstructionsList.Any() || InstructionsList.Any())
         ↓
If true:
    GetContent = true
    StripHtml = true
```

### 3. Execution Flow

```
WebSearchCommand.ExecuteAsync()
         ↓
1. Perform web search
   → Get list of result URLs
         ↓
2. If GetContent == true:
   → For each URL:
      ├─ Fetch page content
      ├─ Optionally strip HTML
      ├─ Optionally save to folder
      └─ Add to results
         ↓
3. Return content/URLs for display
```

## Integration with Other Layers

### Dependencies

Layer 9 depends on earlier layers to determine what to fetch:

- **Layer 1 (Target Selection)**: Determines search terms and provider
  - Uses `Terms` to search
  - Uses `SearchProvider` to choose search engine
  - Uses `MaxResults` to limit result count

- **Layer 2 (Container Filter)**: Filters which URLs to fetch
  - Uses `ExcludeURLContainsPatternList` to skip URLs

- **Layer 6 (Display Control)**: Affects how content is processed
  - Uses `Browser` type for rendering
  - Uses `StripHtml` to extract text
  - Uses `Interactive` for manual navigation

- **Layer 7 (Output Persistence)**: Saves fetched content
  - Uses `SaveFolder` to store pages locally
  - Uses `SavePageOutput` for AI-processed output

- **Layer 8 (AI Processing)**: Processes fetched content
  - Fetching is automatic if AI instructions present
  - AI operates on fetched content, not URLs

### Implicit Action Trigger

The key innovation in WebSearch's Layer 9 is **implicit action triggering**:

```
If user provides AI instructions → Automatically enable content fetching
```

This is implemented in `WebSearchCommand.Validate()`:

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

**Rationale**: AI can't process URLs alone; it needs actual content.

## Execution Evidence

See [Layer 9 Proof Document](cycodmd-websearch-layer-9-proof.md) for detailed source code references showing:

- Command-line parsing for `--get` and related options
- Property storage in `WebCommand` base class
- Validation logic for auto-enabling content fetch
- Execution path for fetching web pages
- Integration with Layer 8 (AI Processing)

## Examples

### Example 1: URL List Only (No Action)

```bash
cycodmd web search "Rust async runtime"
```

**Result**: Displays search result URLs only (no page fetching).

**Layer 9 Action**: None (GetContent = false)

---

### Example 2: Fetch Full Content

```bash
cycodmd web search "Rust async runtime" --get
```

**Result**: Fetches and displays full content of each search result page.

**Layer 9 Action**: Fetch web pages (GetContent = true)

---

### Example 3: Implicit Content Fetch (AI Instructions)

```bash
cycodmd web search "Rust async runtime" --instructions "Compare async approaches"
```

**Result**: Automatically fetches and strips HTML from pages, then processes with AI.

**Layer 9 Action**: Auto-enabled content fetch (GetContent = true, StripHtml = true)

---

### Example 4: Save Fetched Pages

```bash
cycodmd web search "C# design patterns" --get --save-page-folder patterns
```

**Result**: Fetches pages and saves them to `./patterns/` directory for offline access.

**Layer 9 Action**: Fetch and persist pages locally

---

### Example 5: Interactive Fetch

```bash
cycodmd web search "advanced debugging" --get --interactive
```

**Result**: Opens browser interactively, allows manual navigation, then fetches final page content.

**Layer 9 Action**: Interactive content fetch with user navigation

## Behavioral Notes

### Default Behavior

- **Default**: Display search result URLs only (no fetching)
- **Rationale**: Faster, less bandwidth, suitable for link discovery
- **Override**: Use `--get` or provide AI instructions

### Automatic HTML Stripping

When AI instructions are present, HTML is automatically stripped because:
- AI processes text better than HTML
- Removes formatting noise
- Reduces token count
- Focuses on semantic content

### Browser Selection

Content fetching uses headless browser (Chromium by default):
- `--chromium`: Use Chromium (default)
- `--firefox`: Use Firefox
- `--webkit`: Use WebKit
- Browser choice affects JavaScript rendering

### Rate Limiting

**Note**: Multiple page fetches may be rate-limited by:
- Target websites
- Browser automation
- Network conditions

**Best Practice**: Use `--max N` to limit result count for faster execution.

## Comparison with FindFiles Layer 9

| Aspect | FindFiles | WebSearch |
|--------|-----------|-----------|
| **Action Type** | Modify files (replace) | Fetch content |
| **Explicit Flag** | `--execute` required | `--get` optional |
| **Safe-by-Default** | Preview unless `--execute` | URLs unless `--get` |
| **Implicit Trigger** | None | Auto-fetch for AI |
| **Persistence** | Modifies original files | Saves to separate folder |
| **Reversibility** | Requires version control | Doesn't modify originals |

## Safety Considerations

### No Destructive Actions

Unlike FindFiles Layer 9 (which can modify files), WebSearch Layer 9:
- ✅ Never modifies local files
- ✅ Only fetches remote content
- ✅ Saves to separate folder (doesn't overwrite)
- ✅ Read-only operations

### Rate Limiting and Politeness

**Considerations**:
- Respects robots.txt
- Uses browser user-agent
- Sequential fetching (not parallel bombardment)
- Delays between requests

### Bandwidth Usage

**Warning**: `--get` with large result sets can consume significant bandwidth.

**Mitigation**: Use `--max` to limit results.

## Related Layers

- **[Layer 1: Target Selection](cycodmd-websearch-layer-1.md)** - What to search for
- **[Layer 2: Container Filter](cycodmd-websearch-layer-2.md)** - Which URLs to fetch
- **[Layer 6: Display Control](cycodmd-websearch-layer-6.md)** - How to render content
- **[Layer 7: Output Persistence](cycodmd-websearch-layer-7.md)** - Where to save
- **[Layer 8: AI Processing](cycodmd-websearch-layer-8.md)** - Process fetched content

## Design Philosophy

### Implicit vs. Explicit Actions

WebSearch implements a **smart default** pattern:

1. **Explicit user request**: `--get` flag
2. **Implicit need detection**: AI instructions present
3. **Conservative default**: URLs only (fastest, safest)

This balances:
- **Performance**: Don't fetch unless needed
- **Convenience**: Auto-fetch when obviously necessary
- **Transparency**: User can override with explicit `--get`

### Read-Only Philosophy

Layer 9 for WebSearch is **strictly read-only**:
- No file modifications
- No destructive operations
- No irreversible actions

This contrasts with FindFiles Layer 9, which can modify source files.

## See Also

- [Layer 9 Proof Document](cycodmd-websearch-layer-9-proof.md) - Detailed source code evidence
- [WebSearch Command Overview](cycodmd-websearch-filtering-pipeline-catalog-README.md)
- [WebGet Layer 9](cycodmd-webget-layer-9.md) - Similar but for direct URLs
- [FindFiles Layer 9](cycodmd-files-layer-9.md) - Comparison with file modification actions
