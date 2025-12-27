# cycodmd WebGet Command - Layer 6: Display Control

## Purpose

Layer 6 (Display Control) for the WebGet command determines **how retrieved web page content is presented** to the user. WebGet inherits from `WebCommand` and shares most display options with WebSearch, but doesn't include search result formatting.

## Command Line Options

WebGet uses the **same display control options** as WebSearch command since both inherit from `WebCommand`:

### Primary Display Options

#### `--strip`
**Purpose**: Strip HTML formatting from retrieved web pages.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Example**:
```bash
cycodmd web get https://example.com --strip
```

**Source**: [CycoDmdCommandLineOptions.cs:329-332](cycodmd-webget-layer-6-proof.md#strip-html-parsing)

---

#### `--interactive`
**Purpose**: Enable interactive browser mode.

**Type**: Boolean flag (no argument)

**Default**: `false`

**Example**:
```bash
cycodmd web get https://app.example.com --interactive
```

**Source**: [CycoDmdCommandLineOptions.cs:313-316](cycodmd-webget-layer-6-proof.md#interactive-parsing)

---

### Browser Selection Options

#### `--chromium` (default)
**Purpose**: Use Chromium browser engine.

**Source**: [CycoDmdCommandLineOptions.cs:317-320](cycodmd-webget-layer-6-proof.md#browser-selection)

---

#### `--firefox`
**Purpose**: Use Firefox browser engine.

**Source**: [CycoDmdCommandLineOptions.cs:321-324](cycodmd-webget-layer-6-proof.md#browser-selection)

---

#### `--webkit`
**Purpose**: Use WebKit browser engine.

**Source**: [CycoDmdCommandLineOptions.cs:325-328](cycodmd-webget-layer-6-proof.md#browser-selection)

---

## Automatic Display Behaviors

### Markdown Wrapping

All web content is automatically wrapped in markdown format with:
- H2 headers showing the URL
- Code blocks for content
- Automatic formatting

**No user control** over markdown wrapping (always enabled).

---

### URL Validation Display

Invalid URLs (not starting with "http") are caught and displayed as error messages:

**Format**:
```
Invalid URL: {url}
```

or

```
Invalid URLs:
  {url1}
  {url2}
  ...
```

**Implementation**: Program.cs:339-346

---

### Console Output

Page content is output to console unless:
- `--save-output` is used
- AI instructions are present

**Implementation**: Program.cs:356

---

## Differences from WebSearch

### What's Missing in WebGet

WebGet does **NOT** have:
- ❌ Search provider selection (no searching, just direct retrieval)
- ❌ Search results header
- ❌ URL list display (URLs are command arguments, not search results)

### What's the Same

WebGet **DOES** have (inherited from WebCommand):
- ✅ `--strip` (HTML stripping)
- ✅ `--interactive` (browser mode)
- ✅ Browser selection (`--chromium`, `--firefox`, `--webkit`)
- ✅ `--save-page-folder` (Layer 7)
- ✅ `--save-page-output` (Layer 7)
- ✅ AI processing options (Layer 8)

---

## Data Flow

### Input to Layer 6

From previous layers:
- **URLs**: List of URLs from command arguments (Layer 1)
- **Page content**: Retrieved HTML or text (Layer 2-3)

From command properties:
- `StripHtml`: bool
- `Interactive`: bool
- `Browser`: BrowserType enum

### Layer 6 Processing

1. **URL Validation**:
   - Check that URLs start with "http"
   - Display error for invalid URLs
   - Stop processing if errors found

2. **Page Content Retrieval**:
   - Use selected browser engine
   - Apply interactive mode if enabled
   - Retrieve content for each URL

3. **Content Formatting**:
   - Strip HTML if `StripHtml` is true
   - Wrap in markdown format
   - Add page header with URL

### Output from Layer 6

To output stages:
- **Formatted markdown**: String with page content
- **Console output**: Via `ConsoleHelpers.WriteLineIfNotEmpty`

To Layer 7 (Output Persistence):
- **Formatted content**: Optionally saved via `--save-output` or `--save-page-output`

---

## Integration Points

### Layer 5 → Layer 6

WebGet has no Layer 5 (Context Expansion) - web pages are shown in full.

### Layer 6 → Layer 7

Layer 6 provides:
- **Formatted output string**: With page content in markdown
- **Save options**: `SavePageOutput` for per-page saving

Layer 7 uses these for file persistence.

### Layer 6 → Console

Direct output via:
- `ConsoleHelpers.WriteLineIfNotEmpty` for page content (Program.cs:356)

---

## Examples

### Example 1: Basic Web Get
```bash
cycodmd web get https://example.com https://example.org
```

Output:
```markdown
## https://example.com

[Page content in markdown...]

## https://example.org

[Page content in markdown...]
```

---

### Example 2: Strip HTML
```bash
cycodmd web get https://example.com --strip
```

Output: Same structure but with HTML tags removed.

---

### Example 3: Interactive Firefox
```bash
cycodmd web get https://app.example.com --interactive --firefox
```

Opens Firefox browser interactively to retrieve the page.

---

### Example 4: Invalid URL Error
```bash
cycodmd web get example.com
```

Output:
```
Invalid URL: example.com
```

(URLs must start with "http")

---

## Implementation Details

### Display Control Properties

WebGet inherits all properties from `WebCommand`:
```csharp
public bool Interactive { get; set; }     // Line 23
public BrowserType Browser { get; set; }  // Line 29
public bool StripHtml { get; set; }       // Line 31
```

### Parsing Implementation

WebGet uses the **same parser** as WebSearch:
- `CycoDmdCommandLineOptions.TryParseWebCommandOptions`
- Lines 313-332: Interactive, browser selection, strip HTML

### Execution Implementation

In `Program.HandleWebGetCommand`:
- Lines 329-337: Extract display properties
- Lines 339-346: URL validation with error display
- Line 351: Pass display settings to `GetCheckSaveWebPageContentAsync`
- Line 356: Output page content

---

## Special Behaviors

### URL Validation

WebGet validates URLs before retrieval:
- URLs must start with "http" or "https"
- Invalid URLs cause immediate error return
- No partial processing (all-or-nothing validation)

**Why**: Prevents confusing errors during page retrieval.

---

### No Search Header

Unlike WebSearch, WebGet doesn't display a search results header because:
- URLs are explicit (not search results)
- No search provider involved
- Direct page retrieval only

---

## See Also

- [Layer 7: Output Persistence](cycodmd-webget-layer-7.md) - Saving web content
- [Layer 8: AI Processing](cycodmd-webget-layer-8.md) - AI analysis of web content
- [Source Code Evidence](cycodmd-webget-layer-6-proof.md) - Detailed line-by-line proof
- [WebSearch Command Layer 6](cycodmd-websearch-layer-6.md) - Comparison with web search display
