# cycodmd WebSearch Command - Layer 2: Container Filter

## Purpose

Layer 2 filters **which containers (web pages/URLs) to include or exclude** based on URL patterns. After Layer 1 performs the web search and retrieves a list of result URLs, Layer 2 filters which URLs should be fetched and processed based on exclusion patterns.

## Command

**Command**: `cycodmd web search <terms...>`  
**Command Class**: `WebSearchCommand` (inherits from `WebCommand`)

## Options

### --exclude

**Syntax**: `--exclude <pattern1> [pattern2] ...`

**Purpose**: Exclude URLs that match the specified regex pattern(s). URLs matching ANY pattern are excluded from being fetched and processed.

**Pattern Type**: Regular expression (regex), case-insensitive

**Multiple Patterns**: Any pattern match excludes the URL (OR logic)

**Examples**:
```bash
# Search for "python tutorial" but exclude YouTube videos
cycodmd web search "python tutorial" --exclude "youtube\.com"

# Exclude multiple domains
cycodmd web search "machine learning" --exclude "youtube\.com" "twitter\.com" "reddit\.com"

# Exclude paywalled sites
cycodmd web search "news" --exclude "wsj\.com|nytimes\.com"

# Exclude specific URL patterns
cycodmd web search "documentation" --exclude "/archive/|/old/"
```

**Parser Location**: Line 373-379 in `CycoDmdCommandLineOptions.cs` (TryParseWebCommandOptions)  
**Storage**: `WebCommand.ExcludeURLContainsPatternList` (List<Regex>)

**Execution**: URLs from search results are filtered against these patterns before fetching

---

## How It Works

### 1. Search Phase (Layer 1)

Search provider (Bing, Google, etc.) returns a list of URLs matching the search terms.

Example result:
```
https://docs.python.org/3/tutorial/
https://www.youtube.com/watch?v=example
https://realpython.com/python-basics/
https://www.reddit.com/r/learnpython/
```

### 2. Container Filter Phase (Layer 2)

If `--exclude "youtube\.com" "reddit\.com"` is specified:

```
https://docs.python.org/3/tutorial/        ✓ Keep (no pattern match)
https://www.youtube.com/watch?v=example    ✗ Exclude (matches "youtube\.com")
https://realpython.com/python-basics/      ✓ Keep (no pattern match)
https://www.reddit.com/r/learnpython/      ✗ Exclude (matches "reddit\.com")
```

Filtered result:
```
https://docs.python.org/3/tutorial/
https://realpython.com/python-basics/
```

### 3. Subsequent Processing

Only the filtered URLs are fetched and processed in later layers.

---

## Data Flow

```
Layer 1: Web Search
    ↓
List of search result URLs
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

## Integration with Other Layers

- **← Layer 1 (Target Selection)**: Receives search result URLs from search provider
- **→ Layer 3 (Content Filter)**: Passes filtered URLs for content fetching and extraction
- **↔ Layer 1 `--max`**: The `--max` option (Layer 1) limits results before Layer 2 filtering

---

## Pattern Matching Behavior

### Exclusion Logic (OR)

**Logic**: If **ANY** pattern matches the URL, it is excluded.

```bash
cycodmd web search "news" --exclude "ads" "sponsored" "premium"
```

A URL is excluded if it contains:
- "ads" OR
- "sponsored" OR  
- "premium"

### Case-Insensitive Matching

All pattern matching is case-insensitive:

```bash
# These all match the same URLs:
--exclude "youtube"
--exclude "YOUTUBE"
--exclude "YouTube"
```

### Regex Pattern Support

Full regex syntax is supported:

```bash
# Exclude URLs ending in .pdf or .zip
--exclude "\.pdf$|\.zip$"

# Exclude URLs with numbers in path
--exclude "/\d+/"

# Exclude subdomains
--exclude "^https?://[^/]*\.example\.com"
```

---

## Common Patterns

### Exclude Social Media

```bash
cycodmd web search "topic" \
  --exclude "twitter\.com|facebook\.com|instagram\.com|linkedin\.com"
```

### Exclude Video Sites

```bash
cycodmd web search "tutorial" \
  --exclude "youtube\.com|vimeo\.com|dailymotion\.com"
```

### Exclude Forums and Q&A

```bash
cycodmd web search "documentation" \
  --exclude "reddit\.com|stackoverflow\.com|quora\.com"
```

### Exclude Paywalled Content

```bash
cycodmd web search "research paper" \
  --exclude "ieee\.org|acm\.org|sciencedirect\.com"
```

### Exclude Specific File Types

```bash
cycodmd web search "report" \
  --exclude "\.pdf$|\.doc$|\.ppt$"
```

### Exclude Archive/Old Content

```bash
cycodmd web search "current events" \
  --exclude "/archive/|/\d{4}/\d{2}/|/old/"
```

---

## Combining with Other Options

### With --max (Result Limiting)

```bash
# Get top 20 results, then exclude YouTube
cycodmd web search "python" --max 20 --exclude "youtube\.com"

# Note: --max limits BEFORE --exclude filtering
# Result: Up to 20 results (some may be excluded by --exclude)
```

**Processing Order**:
1. Search returns results
2. `--max` limits to N results
3. `--exclude` filters the limited results

### With Search Provider Selection

```bash
# Use Google, exclude certain domains
cycodmd web search "machine learning" \
  --google \
  --exclude "medium\.com|towardsdatascience\.com"
```

### With Content Fetching

```bash
# Search, filter URLs, and fetch content
cycodmd web search "documentation" \
  --exclude "youtube\.com|reddit\.com" \
  --get
```

---

## Performance Considerations

### URL Filtering is Fast

- Pattern matching against URLs is very fast (regex on short strings)
- Filtering happens before network fetching
- Reduces network traffic by excluding unwanted pages early

### Optimization Strategy

Place expensive operations (page fetching) after cheap operations (URL filtering):

```
Fast:  Layer 1 (Search) → Layer 2 (URL Filter) → Layer 3 (Fetch)
Slow:  Layer 1 (Search) → Layer 3 (Fetch) → Filter later
```

---

## Error Handling

### Invalid Regex Pattern

If an invalid regex pattern is provided, the parser throws an error:

```bash
cycodmd web search "test" --exclude "[invalid"
# Error: Invalid regular expression pattern for --exclude: [invalid
```

### No Matching Results

If all search results are excluded by `--exclude`, no pages are fetched:

```bash
cycodmd web search "youtube" --exclude "youtube\.com"
# Result: No URLs to fetch (all excluded)
```

---

## Differences from File Search --exclude

**File Search** (FindFilesCommand):
- `--exclude` filters file names and paths (globs and regex)
- Operates on filesystem paths

**Web Search** (WebSearchCommand):
- `--exclude` filters URLs (regex only)
- Operates on HTTP/HTTPS URLs

**Common**: Both use regex pattern matching with case-insensitive options.

---

## Examples

```bash
# Basic exclusion
cycodmd web search "python tutorial" --exclude "youtube\.com"

# Multiple exclusions
cycodmd web search "news" \
  --exclude "twitter" "facebook" "reddit"

# Exclude ads and sponsored content
cycodmd web search "product review" \
  --exclude "ads|sponsored|affiliate"

# Only .com domains (exclude others)
cycodmd web search "documentation" \
  --exclude "\.org|\.net|\.edu"

# Exclude old content (URLs with years)
cycodmd web search "technology news" \
  --exclude "/20(1[0-9]|20)/"

# Get top 10 official documentation sites, exclude third-party
cycodmd web search "python documentation" \
  --max 10 \
  --exclude "stackoverflow|reddit|medium"
```

---

## See Also

- [Layer 2 Proof Document](cycodmd-websearch-layer-2-proof.md) - Source code evidence
- [Layer 1: Target Selection](cycodmd-websearch-layer-1.md) - Search terms and provider selection
- [Layer 3: Content Filter](cycodmd-websearch-layer-3.md) - Page content extraction
- [WebCommand Source](../src/cycodmd/CommandLineCommands/WebCommand.cs) - Base class for web commands
