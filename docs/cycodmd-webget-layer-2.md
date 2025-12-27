# cycodmd WebGet Command - Layer 2: Container Filter

## Purpose

Layer 2 filters **which containers (web pages/URLs) to include or exclude** based on URL patterns. For the WebGet command, this allows filtering the explicitly specified URLs before fetching and processing them.

## Command

**Command**: `cycodmd web get <url1> [url2] ...`  
**Command Class**: `WebGetCommand` (inherits from `WebCommand`)

## Options

### --exclude

**Syntax**: `--exclude <pattern1> [pattern2] ...`

**Purpose**: Exclude URLs that match the specified regex pattern(s). URLs matching ANY pattern are excluded from being fetched and processed.

**Pattern Type**: Regular expression (regex), case-insensitive

**Multiple Patterns**: Any pattern match excludes the URL (OR logic)

**Examples**:
```bash
# Fetch multiple pages but exclude one by pattern
cycodmd web get \
  https://example.com/page1 \
  https://example.com/page2 \
  https://ads.example.com/promo \
  --exclude "ads\."

# Fetch pages from a list file, excluding certain paths
cycodmd web get @urls.txt --exclude "/archive/|/old/"

# Exclude specific subdomains
cycodmd web get @all-pages.txt --exclude "^https://beta\.|^https://staging\."
```

**Parser Location**: Line 373-379 in `CycoDmdCommandLineOptions.cs` (TryParseWebCommandOptions)  
**Storage**: `WebCommand.ExcludeURLContainsPatternList` (List<Regex>)

**Note**: Same implementation as WebSearchCommand since both inherit from `WebCommand`.

---

## How It Works

### Use Case: Filtering Explicit URLs

Unlike WebSearchCommand where Layer 2 filters search results, WebGetCommand's Layer 2 filters the URLs you explicitly provide. This is useful when:

- Loading URLs from a file (`@urls.txt`) that may contain unwanted URLs
- Providing a range of URLs and wanting to exclude specific patterns
- Automating batch URL processing with pattern-based filtering

### 1. Input Phase (Layer 1)

You provide explicit URLs via positional arguments or file reference:

```bash
cycodmd web get \
  https://docs.example.com/v1/api \
  https://docs.example.com/v1/tutorials \
  https://ads.example.com/banner \
  https://docs.example.com/v2/api \
  --exclude "ads\."
```

### 2. Container Filter Phase (Layer 2)

URLs are filtered against exclusion patterns:

```
https://docs.example.com/v1/api        ✓ Keep (no pattern match)
https://docs.example.com/v1/tutorials  ✓ Keep (no pattern match)
https://ads.example.com/banner         ✗ Exclude (matches "ads\.")
https://docs.example.com/v2/api        ✓ Keep (no pattern match)
```

Filtered result (3 URLs to fetch):
```
https://docs.example.com/v1/api
https://docs.example.com/v1/tutorials
https://docs.example.com/v2/api
```

### 3. Fetching Phase (Layer 3)

Only the filtered URLs are fetched and processed.

---

## Data Flow

```
Layer 1: URLs (positional args or @file)
    ↓
List of explicit URLs
    ↓
Layer 2: Container Filter (--exclude)
    ↓
For each URL:
    ├─ Check against ExcludeURLContainsPatternList
    ├─ If ANY pattern matches URL → Exclude
    └─ If NO patterns match → Include
    ↓
Filtered list of URLs
    ↓
Layer 3: Fetch and extract content
```

---

## Common Use Cases

### Filtering URLs from File

**Scenario**: You have a file with many URLs but want to exclude certain types.

**urls.txt**:
```
https://example.com/docs/api.html
https://example.com/docs/tutorial.html
https://cdn.example.com/assets/script.js
https://example.com/docs/reference.html
https://ads.example.com/banner.html
```

**Command**:
```bash
cycodmd web get @urls.txt --exclude "cdn\.|ads\.|\.js$"
```

**Result**: Fetches only the documentation pages, excluding CDN resources, ads, and JavaScript files.

---

### Excluding Specific Versions

**Scenario**: Fetch documentation for current version only, excluding old versions.

```bash
cycodmd web get @all-docs.txt --exclude "/v[0-9]\.|/old/|/archive/"
```

---

### Excluding Test/Staging Environments

**Scenario**: Filter out non-production URLs from a mixed list.

```bash
cycodmd web get @all-pages.txt --exclude "staging\.|test\.|dev\.|localhost"
```

---

## Pattern Matching Behavior

### Exclusion Logic (OR)

**Logic**: If **ANY** pattern matches the URL, it is excluded.

```bash
cycodmd web get @urls.txt --exclude "ads" "tracking" "analytics"
```

A URL is excluded if it contains:
- "ads" OR
- "tracking" OR  
- "analytics"

### Case-Insensitive Matching

All pattern matching is case-insensitive:

```bash
# These all match the same URLs:
--exclude "example"
--exclude "EXAMPLE"
--exclude "Example"
```

### Regex Pattern Support

Full regex syntax is supported:

```bash
# Exclude URLs ending in .pdf or .zip
cycodmd web get @urls.txt --exclude "\.pdf$|\.zip$"

# Exclude specific subdomains
cycodmd web get @urls.txt --exclude "^https?://(www|cdn|static)\."

# Exclude URLs with dates in path
cycodmd web get @urls.txt --exclude "/\d{4}/\d{2}/\d{2}/"
```

---

## Combining with Other Options

### With Browser Options

```bash
# Fetch pages with Firefox, excluding certain patterns
cycodmd web get @urls.txt \
  --firefox \
  --exclude "popup|modal|overlay"
```

### With Content Stripping

```bash
# Strip HTML and exclude JavaScript/CSS URLs
cycodmd web get @urls.txt \
  --strip \
  --exclude "\.js$|\.css$"
```

### With Page Instructions (AI)

```bash
# Fetch and process with AI, excluding ads
cycodmd web get @urls.txt \
  --exclude "ads\.|sponsored\." \
  --page-instructions "Summarize the main content"
```

---

## Differences from WebSearchCommand

| Feature | WebSearchCommand | WebGetCommand |
|---------|------------------|---------------|
| **Input** | Search terms | Explicit URLs |
| **Layer 1 Output** | Search result URLs | User-provided URLs |
| **Layer 2 Purpose** | Filter search results | Filter explicit URLs |
| **Common Use Case** | Exclude certain domains from search | Exclude patterns from URL list |

**Same Implementation**: Both use `WebCommand.ExcludeURLContainsPatternList`

---

## Examples

```bash
# Basic exclusion
cycodmd web get \
  https://example.com/page1 \
  https://example.com/page2 \
  https://ads.example.com/promo \
  --exclude "ads"

# Multiple exclusions from file
cycodmd web get @urls.txt \
  --exclude "cdn\." "ads\." "tracking\."

# Exclude specific paths
cycodmd web get @all-pages.txt \
  --exclude "/admin/|/api/|/internal/"

# Exclude by subdomain
cycodmd web get @pages.txt \
  --exclude "^https://(dev|test|staging)\."

# Exclude file types
cycodmd web get @resources.txt \
  --exclude "\.pdf$|\.zip$|\.tar\.gz$"

# Combine with content processing
cycodmd web get @documentation.txt \
  --exclude "examples/|demos/" \
  --strip \
  --page-instructions "Extract API reference"
```

---

## Performance Considerations

### Filtering is Fast

- URL pattern matching is very fast (regex on short strings)
- Filtering happens before network fetching
- Reduces network traffic by excluding unwanted pages early

### When to Use

Use `--exclude` when:
- Loading URLs from a file that may contain unwanted entries
- Automating batch processing with pattern-based filtering
- You want to skip certain URL patterns without manually editing URL lists

### When NOT to Use

Don't use `--exclude` if:
- You have full control over input URLs and can provide only desired URLs
- The exclusion patterns are complex and might accidentally exclude wanted URLs
- Better to filter the URL list before passing to cycodmd

---

## See Also

- [Layer 2 Proof Document](cycodmd-webget-layer-2-proof.md) - Source code evidence
- [Layer 1: Target Selection](cycodmd-webget-layer-1.md) - URL specification
- [Layer 3: Content Filter](cycodmd-webget-layer-3.md) - Page content extraction
- [WebSearchCommand Layer 2](cycodmd-websearch-layer-2.md) - Similar filtering for search results
- [WebCommand Source](../src/cycodmd/CommandLineCommands/WebCommand.cs) - Base class for web commands
