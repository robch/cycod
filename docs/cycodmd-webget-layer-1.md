# cycodmd Web Get - Layer 1: Target Selection

**Command**: `cycodmd web get <urls...>`

## Purpose

Layer 1 (Target Selection) specifies **what to retrieve** - the specific URLs to fetch and process. Unlike web search which queries search engines, web get directly retrieves specified pages.

## Options

### Positional Arguments: URLs

**Syntax**: `cycodmd web get <url1> [url2] ...`

**Purpose**: Specify URLs to directly fetch and process.

**Examples**:
```bash
cycodmd web get "https://example.com"
cycodmd web get "https://github.com/microsoft/terminal" "https://github.com/wez/wezterm"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:472-476](cycodmd-webget-layer-1-proof.md#positional-urls)

**Command Property**: `WebGetCommand.Urls` (List<string>)

---

### `--max <N>`

**Syntax**: `cycodmd web get <urls> --max <N>`

**Purpose**: Limit the maximum number of pages to retrieve.

**Default**: 10

**Note**: For web get, this primarily affects behavior when processing lists of URLs or when following links (if that feature is added in the future).

**Examples**:
```bash
cycodmd web get @urls.txt --max 5
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:367-372](cycodmd-webget-layer-1-proof.md#max-option)

**Command Property**: `WebCommand.MaxResults` (int)

---

### `--exclude <patterns...>`

**Syntax**: `cycodmd web get <urls> --exclude <pattern1> [pattern2] ...`

**Purpose**: Exclude URLs matching regex patterns. Useful when providing a list of URLs and wanting to filter some out.

**Pattern Type**: Regex patterns matched against full URL

**Examples**:
```bash
cycodmd web get @urls.txt --exclude "localhost" "127.0.0.1"
cycodmd web get url1 url2 url3 --exclude "test"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:373-379](cycodmd-webget-layer-1-proof.md#exclude-option)

**Command Property**: `WebCommand.ExcludeURLContainsPatternList` (List<Regex>)

---

## Data Flow

```
User Input (URLs, max results, exclusions)
    ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()  [URLs]
CycoDmdCommandLineOptions.TryParseWebCommandOptions()  [options]
    ↓
WebGetCommand properties populated:
    - Urls (List<string>)
    - MaxResults (int)
    - ExcludeURLContainsPatternList (List<Regex>)
    ↓
WebGetCommand.Validate()
    - No special validation logic
    ↓
URL filtering (Layer 2)
    ↓
Page retrieval
```

## Integration with Other Layers

### Feeds Into Layer 2 (Container Filter)
- The URLs become the **candidate set** for Layer 2
- Layer 2 filters URLs based on exclusion patterns
- Remaining URLs are fetched and become web pages for further processing

### Relationship to Layer 3+ (Content Layers)
- Layer 1 determines **which web pages** to fetch
- Layers 3-9 determine **what to show** from those pages

### Comparison to Web Search

| Feature | Web Search | Web Get |
|---------|------------|---------|
| Input | Search terms | Direct URLs |
| Search Provider | Google, Bing, etc. | N/A |
| Discovery | Yes (finds URLs) | No (URLs provided) |
| `--max` | Limits search results | Limits URLs processed |

---

## See Also

- [Proof Document](cycodmd-webget-layer-1-proof.md) - Source code evidence and line numbers
- [Layer 2: Container Filter](cycodmd-webget-layer-2.md) - URL exclusion filtering
- [WebGetCommand Implementation](../src/cycodmd/CommandLineCommands/WebGetCommand.cs)
- [WebCommand Base Class](../src/cycodmd/CommandLineCommands/WebCommand.cs)
