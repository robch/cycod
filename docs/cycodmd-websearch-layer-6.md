# cycodmd WebSearch Command - Layer 6: Display Control

## Purpose

Layer 6 (Display Control) for the WebSearch command determines **how web search results and page content are presented** to the user. Unlike the Files command, WebSearch has minimal display control options as it focuses on retrieving and formatting web content in markdown.

## Command Line Options

### Primary Display Options

#### `--strip`
**Purpose**: Strip HTML formatting from retrieved web pages, showing plain text content.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Effect**: When enabled, HTML tags are removed from page content, leaving only text.

**Example**:
```bash
cycodmd web search "terminal emulator" --get --strip
```

**Source**: [CycoDmdCommandLineOptions.cs:329-332](cycodmd-websearch-layer-6-proof.md#strip-html-parsing)

---

#### `--interactive`
**Purpose**: Enable interactive browser mode for retrieving web pages.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Effect**: When enabled, opens browser in interactive mode (visible window), useful for pages requiring JavaScript or user interaction.

**Example**:
```bash
cycodmd web search "playwright docs" --get --interactive
```

**Source**: [CycoDmdCommandLineOptions.cs:313-316](cycodmd-websearch-layer-6-proof.md#interactive-parsing)

---

### Browser Selection Options

These options control which browser engine is used to retrieve web content:

#### `--chromium`
**Purpose**: Use Chromium browser engine.

**Type**: Boolean flag (no argument)

**Default**: `true` (Chromium is the default)

**Source**: [CycoDmdCommandLineOptions.cs:317-320](cycodmd-websearch-layer-6-proof.md#browser-selection)

---

#### `--firefox`
**Purpose**: Use Firefox browser engine.

**Type**: Boolean flag (no argument)

**Source**: [CycoDmdCommandLineOptions.cs:321-324](cycodmd-websearch-layer-6-proof.md#browser-selection)

---

#### `--webkit`
**Purpose**: Use WebKit browser engine.

**Type**: Boolean flag (no argument)

**Source**: [CycoDmdCommandLineOptions.cs:325-328](cycodmd-websearch-layer-6-proof.md#browser-selection)

---

### Search Provider Options

While these affect what content is retrieved (Layer 1), they also affect how search results are displayed:

- `--google` (default)
- `--bing`
- `--duck-duck-go` / `--duckduckgo`
- `--yahoo`
- `--bing-api`
- `--google-api`

**Source**: [CycoDmdCommandLineOptions.cs:339-362](cycodmd-websearch-layer-6-proof.md#search-provider-parsing)

---

## Automatic Display Behaviors

### Search Results Header

**Purpose**: Display a formatted header for search results.

**Format**:
```markdown
## Web Search for '{query}' using {provider}

{list of URLs}
```

**Implementation**: Program.cs:293

---

### Markdown Wrapping

All web content is automatically wrapped in markdown format with:
- H2 headers for each page
- Code blocks for content
- URL references

**No user control** over markdown wrapping (always enabled).

---

### Console Output

Results are output to console unless:
- `--save-output` is used (output delayed for saving)
- AI instructions are present (output delayed for processing)

**Implementation**: Program.cs:300, 317

---

## Data Flow

### Input to Layer 6

From previous layers:
- **URLs**: List of search result URLs (Layer 1)
- **Page content**: Retrieved HTML or text (Layer 2-3)

From command properties:
- `StripHtml`: bool
- `Interactive`: bool
- `Browser`: BrowserType enum

### Layer 6 Processing

1. **Search Results Display**:
   - Format header with query and provider
   - List URLs line by line
   - Output immediately if not delaying

2. **Page Content Retrieval**:
   - Use selected browser engine
   - Apply interactive mode if enabled
   - Retrieve content

3. **Content Formatting**:
   - Strip HTML if `StripHtml` is true
   - Wrap in markdown format
   - Add page header with URL

### Output from Layer 6

To output stages:
- **Formatted markdown**: String with search results and page content
- **Console output**: Via `ConsoleHelpers.WriteLineIfNotEmpty`

To Layer 7 (Output Persistence):
- **Formatted content**: Same content, optionally saved via `--save-output` or `--save-page-output`

---

## Integration Points

### Layer 5 → Layer 6

WebSearch has no Layer 5 (Context Expansion) - web pages are shown in full.

### Layer 6 → Layer 7

Layer 6 provides:
- **Formatted output string**: With search results and page content
- **Save options**: `SavePageOutput` for per-page saving

Layer 7 (Output Persistence) uses:
- Formatted string for file saving
- `--save-page-output` template for per-page files
- `--save-output` for combined output

### Layer 6 → Console

Direct output via:
- `ConsoleHelpers.WriteLine` for search results (Program.cs:300)
- `ConsoleHelpers.WriteLineIfNotEmpty` for page content (Program.cs:317)

---

## Comparison with Files Command

### What's Missing in WebSearch

WebSearch does **NOT** have:
- ❌ Line numbers
- ❌ Highlighting
- ❌ Content-only mode (equivalent to `--files-only`)
- ❌ Context expansion (before/after lines)
- ❌ Line-level filtering display

### Why These Are Missing

- **Web pages** are typically shown in full, not line-by-line
- **No line matching** - web search retrieves entire pages
- **Markdown format** is the only output format
- **Browser rendering** handles display, not line-level processing

---

## Examples

### Example 1: Basic Web Search with Content
```bash
cycodmd web search "rust async" --get --max 3
```

Output:
```markdown
## Web Search for 'rust async' using Google

https://rust-lang.org/async
https://tokio.rs/
https://docs.rs/async-std

## https://rust-lang.org/async

[Page content in markdown...]

## https://tokio.rs/

[Page content in markdown...]
```

---

### Example 2: Strip HTML
```bash
cycodmd web search "markdown guide" --get --strip
```

Output: Same as above but with HTML tags removed from content.

---

### Example 3: Interactive Browser
```bash
cycodmd web search "javascript tutorial" --get --interactive --firefox
```

Opens Firefox browser in interactive mode to retrieve pages (useful for JavaScript-heavy sites).

---

## Implementation Details

### Display Control Properties

Stored in `WebCommand` base class:
```csharp
public bool Interactive { get; set; }     // Line 23
public BrowserType Browser { get; set; }  // Line 29
public bool StripHtml { get; set; }       // Line 31
```

### Parsing Implementation

In `CycoDmdCommandLineOptions.TryParseWebCommandOptions`:
- Lines 313-316: `--interactive` parsing
- Lines 317-328: Browser selection parsing
- Lines 329-332: `--strip` parsing
- Lines 339-362: Search provider parsing

### Execution Implementation

In `Program.HandleWebSearchCommandAsync`:
- Lines 270-291: Extract display properties from command
- Line 293: Format search results header
- Line 300: Output search results
- Lines 312-322: Process each URL with display settings
- Line 317: Output page content

---

## Special Behaviors

### No Line-Level Control

Unlike the Files command, WebSearch has no line-level display control because:
- Web pages are treated as atomic units
- No line matching or filtering
- Full page content is displayed

### Browser Engine Selection

Different browser engines may render content differently:
- **Chromium**: Fast, good JavaScript support
- **Firefox**: Alternative rendering engine
- **WebKit**: Lighter weight, Safari-compatible

The choice affects how pages are retrieved and displayed.

### Interactive vs. Headless

- **Headless** (default): Faster, no visible window
- **Interactive**: Visible browser, useful for debugging or pages requiring user interaction

---

## See Also

- [Layer 7: Output Persistence](cycodmd-websearch-layer-7.md) - Saving web content
- [Layer 8: AI Processing](cycodmd-websearch-layer-8.md) - AI analysis of web content
- [Source Code Evidence](cycodmd-websearch-layer-6-proof.md) - Detailed line-by-line proof
- [Files Command Layer 6](cycodmd-files-layer-6.md) - Comparison with file display control
