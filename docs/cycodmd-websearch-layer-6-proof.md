# cycodmd WebSearch Command - Layer 6 Proof: Display Control

## Overview

This document provides **source code evidence** for all Display Control (Layer 6) functionality in the cycodmd WebSearch command.

---

## Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Interactive Parsing

**Lines 313-316**:
```csharp
else if (arg == "--interactive")
{
    command.Interactive = true;
}
```

**Explanation**:
- Checks if argument is `"--interactive"`
- Sets `WebCommand.Interactive` property to `true`
- Enables visible browser window for page retrieval

---

#### Browser Selection

**Lines 317-328**:
```csharp
else if (arg == "--chromium")
{
    command.Browser = BrowserType.Chromium;
}
else if (arg == "--firefox")
{
    command.Browser = BrowserType.Firefox;
}
else if (arg == "--webkit")
{
    command.Browser = BrowserType.Webkit;
}
```

**Explanation**:
- Three mutually exclusive browser options
- Sets `WebCommand.Browser` enum property
- Default is `BrowserType.Chromium` (set in constructor)

---

#### Strip HTML Parsing

**Lines 329-332**:
```csharp
else if (arg == "--strip")
{
    command.StripHtml = true;
}
```

**Explanation**:
- Checks if argument is `"--strip"`
- Sets `WebCommand.StripHtml` property to `true`
- Causes HTML tags to be removed from page content

---

#### Search Provider Parsing

**Lines 339-362**:
```csharp
else if (arg == "--bing")
{
    command.SearchProvider = WebSearchProvider.Bing;
}
else if (arg == "--duck-duck-go" || arg == "--duckduckgo")
{
    command.SearchProvider = WebSearchProvider.DuckDuckGo;
}
else if (arg == "--google")
{
    command.SearchProvider = WebSearchProvider.Google;
}
else if (arg == "--yahoo")
{
    command.SearchProvider = WebSearchProvider.Yahoo;
}
else if (arg == "--bing-api")
{
    command.SearchProvider = WebSearchProvider.BingAPI;
}
else if (arg == "--google-api")
{
    command.SearchProvider = WebSearchProvider.GoogleAPI;
}
```

**Explanation**:
- Sets which search provider to use
- Affects search results header display
- Default is `WebSearchProvider.Google` (set in constructor)

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

#### Property Declarations

**Lines 23-31**:
```csharp
public bool Interactive { get; set; }

public WebSearchProvider SearchProvider { get; set; }
public List<Regex> ExcludeURLContainsPatternList { get; set; }
public int MaxResults { get; set; }

public BrowserType Browser { get; set; }
public bool GetContent { get; set; }
public bool StripHtml { get; set; }
```

**Display Control Properties**:
- `Interactive`: Show browser window vs. headless
- `Browser`: Which browser engine to use
- `StripHtml`: Remove HTML tags vs. keep formatting
- `SearchProvider`: Which search engine (affects header)

---

#### Constructor Initialization

**Lines 8-21**:
```csharp
public WebCommand()
{
    Interactive = false;

    SearchProvider = WebSearchProvider.Google;
    MaxResults = 10;

    ExcludeURLContainsPatternList = new();

    Browser = BrowserType.Chromium;
    GetContent = false;
    StripHtml = false;

    PageInstructionsList = new();
}
```

**Default Values**:
- Interactive: `false` (headless mode)
- Browser: `Chromium`
- StripHtml: `false` (keep HTML)
- SearchProvider: `Google`

---

## Execution Flow

### File: `src/cycodmd/Program.cs`

#### Main Handler Entry Point

**Line 105**:
```csharp
WebSearchCommand webSearchCommand => await HandleWebSearchCommandAsync(commandLineOptions, webSearchCommand, processor, delayOutputToApplyInstructions),
```

**Explanation**:
- Pattern match in main command dispatcher
- Routes `WebSearchCommand` instances to handler method

---

#### Display Properties Extraction

**Lines 270-291**:
```csharp
private static async Task<List<Task<string>>> HandleWebSearchCommandAsync(CommandLineOptions commandLineOptions, WebSearchCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
{
    var provider = command.SearchProvider;
    var query = string.Join(" ", command.Terms);
    var maxResults = command.MaxResults;
    var excludeURLContainsPatternList = command.ExcludeURLContainsPatternList;
    var getContent = command.GetContent;
    var stripHtml = command.StripHtml;
    var saveToFolder = command.SaveFolder;
    var browserType = command.Browser;
    var interactive = command.Interactive;
    var useBuiltInFunctions = command.UseBuiltInFunctions;
    var saveChatHistory = command.SaveChatHistory;

    var savePageOutput = command.SavePageOutput;
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

**Display Control Variables Extracted**:
- Line 275: `stripHtml` - Whether to strip HTML tags
- Line 277: `browserType` - Which browser engine
- Line 278: `interactive` - Interactive vs. headless mode
- Line 270: `provider` - Search provider for header

---

#### Search Results Header Formatting

**Line 293**:
```csharp
var searchSectionHeader = $"## Web Search for '{query}' using {provider}";
```

**Explanation**:
- Creates markdown H2 header
- Includes query and provider name
- Display control: Format is fixed, not user-configurable

---

#### Search Results Display

**Lines 296-300**:
```csharp
var searchSection = urls.Count == 0
    ? $"{searchSectionHeader}\n\nNo results found\n"
    : $"{searchSectionHeader}\n\n" + string.Join("\n", urls) + "\n";

if (!delayOutputToApplyInstructions) ConsoleHelpers.WriteLine(searchSection);
```

**Explanation**:
1. **Format**: Header + URL list (one per line)
2. **Empty results**: "No results found" message
3. **Output**: Immediately to console unless delaying for instructions

**Display Control**:
- No user options for result list formatting
- Fixed markdown format
- URLs listed line-by-line

---

#### Page Content Retrieval with Display Settings

**Line 312**:
```csharp
var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
```

**Display Parameters Passed**:
- `stripHtml` - Remove HTML tags
- `browserType` - Browser engine selection
- `interactive` - Browser mode

---

#### Page Content Output

**Lines 313-322**:
```csharp
var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
var taskToAdd = delayOutputToApplyInstructions
    ? getCheckSaveTask
    : getCheckSaveTask.ContinueWith(t =>
    {
        ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
        return t.Result;
    });

tasks.Add(taskToAdd);
```

**Explanation**:
1. **Retrieve**: Get page content with display settings
2. **Output**: If not delaying, output to console immediately
3. **Return**: Task for later awaiting

---

#### Page Content Processing

**Lines 534-602** (approximate location):
```csharp
private static Task<string> GetCheckSaveWebPageContentAsync(
    string url, 
    bool stripHtml, 
    string? saveToFolder, 
    BrowserType browserType, 
    bool interactive, 
    List<Tuple<string, string>> pageInstructionsList, 
    bool useBuiltInFunctions, 
    string? saveChatHistory, 
    string? savePageOutput)
```

**Display Parameters Used**:
- `stripHtml`: Passed to content retrieval/formatting
- `browserType`: Determines browser engine
- `interactive`: Determines browser visibility

**Implementation**:
- Retrieves page using specified browser and mode
- Formats content as markdown
- Strips HTML if requested
- Returns formatted string

---

## Data Flow Summary

### Parsing Phase

```
Command Line: cycodmd web search "query" --get --strip --interactive --firefox
                                                    ↓
CycoDmdCommandLineOptions.TryParseWebCommandOptions()
    Lines 313-316: Parse --interactive → WebCommand.Interactive = true
    Lines 321-324: Parse --firefox → WebCommand.Browser = BrowserType.Firefox
    Lines 329-332: Parse --strip → WebCommand.StripHtml = true
                                                    ↓
WebSearchCommand object created with display properties set
```

### Execution Phase

```
Program.Main()
    Line 105: Dispatch to HandleWebSearchCommandAsync()
                                                    ↓
HandleWebSearchCommandAsync()
    Lines 270-278: Extract display properties
    Line 293: Format search results header
    Lines 296-300: Display search results
    Line 312: Pass display settings to GetCheckSaveWebPageContentAsync()
                                                    ↓
GetCheckSaveWebPageContentAsync()
    - Use browserType and interactive mode to retrieve page
    - Apply stripHtml setting to content
    - Format as markdown
    - Return formatted string
                                                    ↓
Back to HandleWebSearchCommandAsync()
    Line 317: ConsoleHelpers.WriteLineIfNotEmpty() - output to console
```

---

## Comparison with Files Command

### Display Options in Files Command

Files command has extensive Layer 6 options:
- ✅ `--line-numbers`
- ✅ `--highlight-matches` / `--no-highlight-matches`
- ✅ `--files-only`
- ✅ Automatic markdown wrapping decision
- ✅ Auto-highlight logic

### Display Options in WebSearch Command

WebSearch has minimal Layer 6 options:
- ✅ `--strip` (HTML removal)
- ✅ `--interactive` (browser mode)
- ✅ `--chromium` / `--firefox` / `--webkit` (browser selection)
- ❌ No line numbers
- ❌ No highlighting
- ❌ No content-only mode
- ❌ Markdown wrapping always enabled

### Why the Difference?

**Files Command**: Line-level processing
- Matches specific lines
- Shows context around matches
- Needs line numbers for reference
- Benefits from highlighting

**WebSearch Command**: Page-level processing
- Retrieves entire pages
- No line matching
- Pages displayed in full
- HTML/text formatting is the display concern

---

## Testing Scenarios

### Scenario 1: Default Display (Headless Chromium)
```bash
cycodmd web search "rust" --get --max 2
```

**Expected State**:
- `Interactive`: `false`
- `Browser`: `Chromium` (default)
- `StripHtml`: `false`
- `SearchProvider`: `Google` (default)

**Output**: Search results header + URLs + full page content with HTML (in markdown).

---

### Scenario 2: Strip HTML
```bash
cycodmd web search "markdown" --get --strip
```

**Expected State**:
- `StripHtml`: `true`

**Output**: Same structure but with HTML tags removed from page content.

---

### Scenario 3: Interactive Firefox
```bash
cycodmd web search "playwright" --get --interactive --firefox
```

**Expected State**:
- `Interactive`: `true`
- `Browser`: `Firefox`

**Output**: Firefox browser opens visibly, retrieves pages, displays content.

---

### Scenario 4: Search Results Only (No Content)
```bash
cycodmd web search "terminal emulator" --max 5
```

**Expected State**:
- `GetContent`: `false` (no `--get` flag)

**Output**: Only search results header and URL list, no page content.

---

## Related Components

### WebSearchHelpers

**Function**: `GetWebSearchResultUrlsAsync`
- Uses search provider and browser settings
- Returns list of URLs
- Respects `interactive` and `browserType` parameters

### Browser Engines

**BrowserType Enum**:
```csharp
enum BrowserType
{
    Chromium,
    Firefox,
    Webkit
}
```

Each engine renders pages differently, affecting display output.

### ConsoleHelpers

**Functions Used**:
- `WriteLine(string)`: Output search results (line 300)
- `WriteLineIfNotEmpty(string)`: Output page content (line 317)

---

## Summary

### Key Implementation Points

1. **Display properties** stored in `WebCommand` base class (lines 23-31)
2. **Parsing logic** in `CycoDmdCommandLineOptions.TryParseWebCommandOptions` (lines 313-362)
3. **Display property extraction** in `Program.HandleWebSearchCommandAsync` (lines 270-278)
4. **Search results formatting** in `Program.HandleWebSearchCommandAsync` (line 293)
5. **Page content retrieval** with display settings (line 312)
6. **Console output** (lines 300, 317)

### Display Control Options Summary

| Option | Purpose | Default | Property |
|--------|---------|---------|----------|
| `--strip` | Remove HTML tags | `false` | `StripHtml` |
| `--interactive` | Show browser | `false` | `Interactive` |
| `--chromium` | Use Chromium | `true` | `Browser` |
| `--firefox` | Use Firefox | `false` | `Browser` |
| `--webkit` | Use WebKit | `false` | `Browser` |

---

## Verification

To verify Layer 6 is working correctly:

1. **Strip HTML**: Run with `--strip`, verify no HTML tags in output
2. **Interactive**: Run with `--interactive`, verify browser window appears
3. **Browser Selection**: Run with `--firefox`, verify Firefox is used
4. **Search Results**: Verify header format includes query and provider
5. **Output Timing**: Verify immediate console output unless `--save-output` or `--instructions` used
