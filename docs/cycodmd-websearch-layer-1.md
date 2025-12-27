# cycodmd Web Search - Layer 1: Target Selection

**Command**: `cycodmd web search <terms...>`

## Purpose

Layer 1 (Target Selection) specifies **what to search** - the search terms and web search parameters that determine which web pages will be retrieved and considered for processing.

## Options

### Positional Arguments: Search Terms

**Syntax**: `cycodmd web search <term1> [term2] ...`

**Purpose**: Specify search terms to query web search engines.

**Examples**:
```bash
cycodmd web search "machine learning"
cycodmd web search python tutorial
cycodmd web search "how to" use playwright C#
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:467-470](cycodmd-websearch-layer-1-proof.md#positional-search-terms)

**Command Property**: `WebSearchCommand.Terms` (List<string>)

---

### `--max <N>`

**Syntax**: `cycodmd web search <terms> --max <N>`

**Purpose**: Limit the maximum number of search results to retrieve.

**Default**: 10

**Examples**:
```bash
cycodmd web search "terminal emulator" --max 5
cycodmd web search playwright --max 20
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:367-372](cycodmd-websearch-layer-1-proof.md#max-option)

**Command Property**: `WebCommand.MaxResults` (int)

---

### Search Provider Options

These options select which search engine to use.

#### `--google`

**Syntax**: `cycodmd web search <terms> --google`

**Purpose**: Use Google as the search provider (scraping-based).

**Default**: Google is the default provider

**Parser Location**: [CycoDmdCommandLineOptions.cs:347-350](cycodmd-websearch-layer-1-proof.md#google-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.Google`

---

#### `--bing`

**Syntax**: `cycodmd web search <terms> --bing`

**Purpose**: Use Bing as the search provider (scraping-based).

**Parser Location**: [CycoDmdCommandLineOptions.cs:339-342](cycodmd-websearch-layer-1-proof.md#bing-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.Bing`

---

#### `--duck-duck-go`, `--duckduckgo`

**Syntax**: `cycodmd web search <terms> --duck-duck-go`

**Purpose**: Use DuckDuckGo as the search provider (scraping-based).

**Parser Location**: [CycoDmdCommandLineOptions.cs:343-346](cycodmd-websearch-layer-1-proof.md#duckduckgo-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.DuckDuckGo`

---

#### `--yahoo`

**Syntax**: `cycodmd web search <terms> --yahoo`

**Purpose**: Use Yahoo as the search provider (scraping-based).

**Parser Location**: [CycoDmdCommandLineOptions.cs:351-354](cycodmd-websearch-layer-1-proof.md#yahoo-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.Yahoo`

---

#### `--bing-api`

**Syntax**: `cycodmd web search <terms> --bing-api`

**Purpose**: Use Bing Search API (requires API key).

**Parser Location**: [CycoDmdCommandLineOptions.cs:355-358](cycodmd-websearch-layer-1-proof.md#bing-api-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.BingAPI`

---

#### `--google-api`

**Syntax**: `cycodmd web search <terms> --google-api`

**Purpose**: Use Google Custom Search API (requires API key).

**Parser Location**: [CycoDmdCommandLineOptions.cs:359-362](cycodmd-websearch-layer-1-proof.md#google-api-option)

**Command Property**: `WebCommand.SearchProvider = WebSearchProvider.GoogleAPI`

---

### `--exclude <patterns...>`

**Syntax**: `cycodmd web search <terms> --exclude <pattern1> [pattern2] ...`

**Purpose**: Exclude URLs matching regex patterns.

**Pattern Type**: Regex patterns matched against full URL

**Examples**:
```bash
cycodmd web search "python tutorial" --exclude "youtube.com"
cycodmd web search playwright --exclude "stackoverflow" "reddit"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:373-379](cycodmd-websearch-layer-1-proof.md#exclude-option)

**Command Property**: `WebCommand.ExcludeURLContainsPatternList` (List<Regex>)

---

## Data Flow

```
User Input (search terms, provider, max results, exclusions)
    ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()  [search terms]
CycoDmdCommandLineOptions.TryParseWebCommandOptions()  [options]
    ↓
WebSearchCommand properties populated:
    - Terms (List<string>)
    - SearchProvider (WebSearchProvider enum)
    - MaxResults (int)
    - ExcludeURLContainsPatternList (List<Regex>)
    ↓
WebSearchCommand.Validate()
    - Auto-enables GetContent and StripHtml if instructions provided
    ↓
Web search execution (retrieves URLs)
    ↓
URL filtering (Layer 2)
```

## Integration with Other Layers

### Feeds Into Layer 2 (Container Filter)
- The search results (URLs) become the **candidate set** for Layer 2
- Layer 2 filters URLs based on exclusion patterns
- Remaining URLs are fetched and become web pages for further processing

### Relationship to Layer 3+ (Content Layers)
- Layer 1 determines **which web pages** to fetch
- Layers 3-9 determine **what to show** from those pages

### Special Validation Logic

**File**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 23-36**:
```csharp
override public CycoDmdCommand Validate()
{
    var noContent = !GetContent;
    var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();

    var assumeGetAndStrip = noContent && hasInstructions;
    if (assumeGetAndStrip)
    {
        GetContent = true;
        StripHtml = true;
    }

    return this;
}
```

**Explanation**: If user provides AI instructions but doesn't explicitly request content retrieval, automatically enable `--get` and `--strip` for convenience.

---

## See Also

- [Proof Document](cycodmd-websearch-layer-1-proof.md) - Source code evidence and line numbers
- [Layer 2: Container Filter](cycodmd-websearch-layer-2.md) - URL exclusion filtering
- [WebSearchCommand Implementation](../src/cycodmd/CommandLineCommands/WebSearchCommand.cs)
- [WebCommand Base Class](../src/cycodmd/CommandLineCommands/WebCommand.cs)
