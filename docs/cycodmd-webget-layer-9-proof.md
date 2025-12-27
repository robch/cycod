# cycodmd WebGet Command - Layer 9 Proof: Actions on Results

## Overview

This document provides **source code evidence** for Layer 9 (Actions on Results) of the cycodmd WebGet command, tracing the implementation from command-line parsing through execution.

---

## 1. Command-Line Parsing

### Positional Arguments (URLs)

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 472-476**:
```csharp
else if (command is WebGetCommand webGetCommand)
{
    webGetCommand.Urls.Add(arg);
    parsedOption = true;
}
```

**Evidence**:
- Parsed in `TryParseOtherCommandArg()` method
- All non-option arguments are treated as URLs
- Each URL is added to `WebGetCommand.Urls` list
- Enables natural CLI syntax: `cycodmd web get URL1 URL2 URL3`

---

### Inherited Options from WebCommand

WebGet inherits all WebCommand options, parsed in `TryParseWebCommandOptions()`:

#### `--interactive` (Lines 313-316)
```csharp
else if (arg == "--interactive")
{
    command.Interactive = true;
}
```

#### `--save-page-folder` (Lines 333-338)
```csharp
else if (arg == "--save-page-folder")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, 1);
    command.SaveFolder = max1Arg.FirstOrDefault() ?? "web-pages";
    i += max1Arg.Count();
}
```

#### `--strip` (Lines 329-332)
```csharp
else if (arg == "--strip")
{
    command.StripHtml = true;
}
```

#### Browser options (Lines 317-328)
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

**Evidence**: All WebCommand options are available to WebGetCommand through inheritance.

---

## 2. Command Properties

### WebGetCommand Specific Properties

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 6-11**:
```csharp
public WebGetCommand()
{
    Urls = new List<string>();
}

public List<string> Urls { get; set; }
```

**Evidence**:
- `Urls`: List of URLs to fetch (populated from positional args)
- Initialized as empty list in constructor
- Public property accessible from parsing and execution

---

### Inherited Properties from WebCommand

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 23, 29-31, 33**:
```csharp
public bool Interactive { get; set; }
// ...
public BrowserType Browser { get; set; }
public bool GetContent { get; set; }
public bool StripHtml { get; set; }
// ...
public string? SaveFolder { get; set; }
```

**Evidence**:
- All Layer 9 action-related properties inherited from `WebCommand`
- WebGet uses same infrastructure as WebSearch

---

### Property Initialization

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 9, 16-18**:
```csharp
Interactive = false;
// ...
Browser = BrowserType.Chromium;
GetContent = false;
StripHtml = false;
```

**Evidence**:
- `Interactive` defaults to `false` (headless)
- `Browser` defaults to `Chromium`
- `GetContent` defaults to `false` (but irrelevant for WebGet)
- `StripHtml` defaults to `false` (preserve HTML)

**Note**: `GetContent` is not relevant for WebGet since it always fetches content (its purpose).

---

## 3. Validation Method

### No Special Validation

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 24-27**:
```csharp
override public CycoDmdCommand Validate()
{
    return this;
}
```

**Evidence**:
- WebGet has **no special validation logic**
- Unlike WebSearch, it doesn't auto-enable `GetContent` based on AI instructions
- Rationale: WebGet always fetches content (no conditional logic needed)
- Simplest validation of all cycodmd commands

---

## 4. Execution Logic

### Entry Point

**File**: `src/cycodmd/Program.cs`

**Lines 326-339**:
```csharp
private static async Task<List<Task<string>>> ProcessWebGetCommand(WebGetCommand command)
{
    var urls = command.Urls;
    var stripHtml = command.StripHtml;
    var saveToFolder = command.SaveFolder;
    var browserType = command.Browser;
    var interactive = command.Interactive;
    var pageInstructionsList = command.PageInstructionsList;
    var useBuiltInFunctions = command.UseBuiltInFunctions;
    var saveChatHistory = command.SaveChatHistory;
    var saveOutput = command.SaveOutput;
    var savePageOutput = command.SavePageOutput;
    var instructionsList = command.InstructionsList;
```

**Evidence**:
- Method extracts all relevant properties from command
- `urls`: The list of URLs to fetch (Layer 1)
- Layer 9 properties: `saveToFolder`, `browserType`, `interactive`
- Layer 8 properties: `pageInstructionsList`, `instructionsList`
- No `GetContent` check (always fetches for WebGet)

---

### Content Fetching Loop

**File**: `src/cycodmd/Program.cs`

**Lines 346-355**:
```csharp
var delayOutputToApplyInstructions = instructionsList.Any();

var tasks = new List<Task<string>>();
foreach (var url in urls)
{
    var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
    var taskToAdd = delayOutputToApplyInstructions
        ? getCheckSaveTask
        : getCheckSaveTask.ContinueWith(async t => { ConsoleHelpers.WriteLine(await t); return await t; }).Unwrap();
    tasks.Add(taskToAdd);
```

**Evidence**:
- **Critical**: No conditional check like WebSearch's `if (!getContent)`
- **Always executes** content fetching for every URL
- Uses same `GetCheckSaveWebPageContentAsync` method as WebSearch
- Passes all Layer 9 parameters: `saveToFolder`, `browserType`, `interactive`
- Creates async task per URL for fetching

---

## 5. Shared Fetching Infrastructure

### GetCheckSaveWebPageContentAsync

**File**: `src/cycodmd/Program.cs`

**Lines 636-642**:
```csharp
private static async Task<string> GetCheckSaveWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? savePageOutput)
{
    try
    {
        ConsoleHelpers.DisplayStatus($"Processing: {url} ...");
        var finalContent = await GetFinalWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory);
```

**Evidence**:
- **Same method** used by WebSearch and WebGet
- Accepts all Layer 9 parameters: `saveToFolder`, `browserType`, `interactive`
- Demonstrates code reuse and consistency

---

### GetFinalWebPageContentAsync

**File**: `src/cycodmd/Program.cs`

**Lines 659-662**:
```csharp
private static async Task<string> GetFinalWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
{
    var formatted = await GetFormattedWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive);
```

**Evidence**:
- Continues parameter chain from previous method
- Calls `GetFormattedWebPageContentAsync` to fetch content
- Then applies AI instructions (Layer 8)

---

### GetFormattedWebPageContentAsync

**File**: `src/cycodmd/Program.cs`

**Lines 682-687**:
```csharp
private static async Task<string> GetFormattedWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive)
{
    try
    {
        var (content, title) = await PlaywrightHelpers.GetPageAndTitle(url, stripHtml, saveToFolder, browserType, interactive);
```

**Evidence**:
- Delegates to `PlaywrightHelpers.GetPageAndTitle`
- **Same code path** as WebSearch content fetching
- Layer 9 parameters passed to browser automation layer

---

## 6. Browser Automation (Shared with WebSearch)

### PlaywrightHelpers.GetPageAndTitle

**File**: `src/cycodmd/Helpers/PlaywrightHelpers.cs`

**Lines 51-57**:
```csharp
public static async Task<(string, string)> GetPageAndTitle(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive)
{
    // Initialize Playwright
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await GetBrowser(browserType, interactive, playwright);
    var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();
```

**Evidence**:
- Receives Layer 9 parameters: `saveToFolder`, `browserType`, `interactive`
- Creates browser with `GetBrowser` method
- `interactive` parameter controls headless mode

---

### Browser Launch with Interactive Control

**File**: `src/cycodmd/Helpers/PlaywrightHelpers.cs`

**Lines 228-230**:
```csharp
BrowserType.Chromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
BrowserType.Firefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
BrowserType.Webkit => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
```

**Evidence**:
- **Critical**: `Headless = !interactive`
- If `interactive == true`, browser shows GUI
- If `interactive == false` (default), browser runs headlessly
- **Same browser launch code** used by both WebSearch and WebGet

---

## 7. Data Flow Trace

### Complete Call Stack for WebGet

```
1. User Command:
   cycodmd web get https://example.com https://test.com --interactive --save-page-folder pages

2. CycoDmdCommandLineOptions.Parse()
   ├─ PeekCommandName() detects "web get"
   ├─ NewCommandFromName("web get") → new WebGetCommand()
   ├─ TryParseOtherCommandArg() for each URL:
   │  ├─ "https://example.com" → Urls.Add("https://example.com")
   │  └─ "https://test.com" → Urls.Add("https://test.com")
   └─ TryParseWebCommandOptions():
      ├─ "--interactive" → Interactive = true
      └─ "--save-page-folder pages" → SaveFolder = "pages"

3. WebGetCommand.Validate()
   └─ No special logic, returns this

4. Program.ProcessWebGetCommand(command)
   ├─ Extract properties:
   │  ├─ urls = ["https://example.com", "https://test.com"]
   │  ├─ interactive = true
   │  └─ saveToFolder = "pages"
   ├─ No conditional check (unlike WebSearch)
   └─ For each URL:
      └─ GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, ...)

5. GetCheckSaveWebPageContentAsync(url, ..., saveToFolder, browserType, interactive, ...)
   └─ GetFinalWebPageContentAsync(url, ..., saveToFolder, browserType, interactive, ...)

6. GetFinalWebPageContentAsync(url, ..., saveToFolder, browserType, interactive)
   └─ GetFormattedWebPageContentAsync(url, ..., saveToFolder, browserType, interactive)

7. GetFormattedWebPageContentAsync(url, ..., saveToFolder, browserType, interactive)
   └─ PlaywrightHelpers.GetPageAndTitle(url, stripHtml, saveToFolder, browserType, interactive)

8. PlaywrightHelpers.GetPageAndTitle(url, ..., saveToFolder, browserType, interactive)
   ├─ GetBrowser(browserType, interactive, playwright)
   │  └─ Launch browser: Headless = !interactive = !true = false (visible GUI)
   ├─ Navigate to URL
   ├─ Extract content
   ├─ If saveToFolder != null:
   │  └─ Save to "pages/" directory
   └─ Return (content, title)
```

---

## 8. Comparison with WebSearch

### Key Differences

**WebSearch** (Program.cs:302-305):
```csharp
if (urls.Count == 0 || !getContent)
{
    return new List<Task<string>>() { Task.FromResult(searchSection) };
}
```

**WebGet** (Program.cs:349-351):
```csharp
foreach (var url in urls)
{
    var getCheckSaveTask = GetCheckSaveWebPageContentAsync(...);
```

**Evidence**:
- **WebSearch**: Has conditional gate (`if (!getContent)`)
- **WebGet**: No conditional gate (always fetches)
- **Rationale**: WebGet's purpose IS to fetch content

---

### Shared Code Paths

**Evidence of Code Reuse**:

1. Both use `GetCheckSaveWebPageContentAsync` (Program.cs:636)
2. Both use `GetFinalWebPageContentAsync` (Program.cs:659)
3. Both use `GetFormattedWebPageContentAsync` (Program.cs:682)
4. Both use `PlaywrightHelpers.GetPageAndTitle` (PlaywrightHelpers.cs:51)
5. Both use same browser launch logic (PlaywrightHelpers.cs:228-230)

**Analysis**:
- WebGet and WebSearch share **100% of content fetching code**
- Only difference is **when** fetching occurs (conditional vs. always)
- Excellent code reuse and maintainability

---

## 9. No Container Filtering

### WebSearch Has URL Filtering

**File**: `src/cycodmd/Program.cs` (WebSearch)

**Line 295**:
```csharp
var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
```

**Evidence**: WebSearch passes `excludeURLContainsPatternList` to filter results.

---

### WebGet Has No URL Filtering

**File**: `src/cycodmd/Program.cs` (WebGet)

**Lines 329, 349**:
```csharp
var urls = command.Urls;
// ...
foreach (var url in urls)
```

**Evidence**:
- WebGet directly uses `command.Urls` without filtering
- No `excludeURLContainsPatternList` involved
- All specified URLs are fetched (no exclusions)

**Rationale**: User explicitly provided URLs; if they don't want a URL fetched, they shouldn't provide it.

---

## 10. Integration Points

### With Layer 1 (Target Selection)

**Evidence**: URLs are the direct target

**Code** (CycoDmdCommandLineOptions.cs:472-476):
```csharp
else if (command is WebGetCommand webGetCommand)
{
    webGetCommand.Urls.Add(arg);
    parsedOption = true;
}
```

**Analysis**:
- Layer 1 for WebGet is simply the list of URLs
- No search, no provider, no filtering
- Direct input → direct action

---

### With Layer 6 (Display Control)

**Evidence**: Browser and stripping options affect fetching

**Properties Used**:
- `browserType` (Chromium/Firefox/Webkit)
- `stripHtml` (text extraction)
- `interactive` (visible browser)

**Analysis**: Layer 6 display options directly impact Layer 9 action execution.

---

### With Layer 7 (Output Persistence)

**Evidence**: `saveToFolder` enables local caching

**Property**: `command.SaveFolder`

**Analysis**: Layer 9 fetches, Layer 7 persists.

---

### With Layer 8 (AI Processing)

**Evidence**: AI instructions processed after fetching

**Code** (Program.cs:663-667):
```csharp
var instructionsForThisPage = pageInstructionsList
    .Where(x => string.IsNullOrEmpty(x.Item2) || url.Contains(x.Item2, StringComparison.OrdinalIgnoreCase))
    .Select(x => x.Item1)
    .ToList();

// ... AI processing ...
```

**Analysis**: Layer 9 fetches content, Layer 8 processes it.

---

## 11. Empty Command Check

### IsEmpty Implementation

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 18-22**:
```csharp
override public bool IsEmpty()
{
    return !Urls.Any();
}
```

**Evidence**:
- Command is empty if no URLs provided
- Simplest `IsEmpty()` check of all cycodmd commands
- Only checks one property: `Urls`

---

## 12. Summary of Evidence

### Parser Evidence
✅ Positional URL parsing at CycoDmdCommandLineOptions.cs:472-476  
✅ Inherited options from WebCommand base class:
  - `--interactive` at CycoDmdCommandLineOptions.cs:313-316
  - `--save-page-folder` at CycoDmdCommandLineOptions.cs:333-338
  - `--strip` at CycoDmdCommandLineOptions.cs:329-332
  - Browser options at CycoDmdCommandLineOptions.cs:317-328

### Property Evidence
✅ `Urls` property defined at WebGetCommand.cs:11  
✅ Inherited properties from WebCommand.cs:23, 29-31, 33  
✅ Default values initialized at WebCommand.cs:9, 16-18

### Validation Evidence
✅ No special validation at WebGetCommand.cs:24-27  
✅ Always fetches (no conditional logic)

### Execution Evidence
✅ Property extraction at Program.cs:329-339  
✅ **No conditional gate** (unlike WebSearch at Program.cs:302-305)  
✅ Fetch loop at Program.cs:349-351  
✅ Shared fetching infrastructure:
  - GetCheckSaveWebPageContentAsync at Program.cs:636
  - GetFinalWebPageContentAsync at Program.cs:659
  - GetFormattedWebPageContentAsync at Program.cs:682
  - PlaywrightHelpers.GetPageAndTitle at PlaywrightHelpers.cs:51

### Integration Evidence
✅ 100% code reuse with WebSearch for fetching logic  
✅ No URL filtering (Layer 2) unlike WebSearch  
✅ Direct Layer 1 → Layer 9 flow (no intermediate filtering)

---

## 13. Behavioral Evidence

### Always-Fetch Pattern

**Negative Evidence** (absence of conditional check):

**WebSearch has** (Program.cs:302):
```csharp
if (urls.Count == 0 || !getContent)
```

**WebGet does NOT have** (no equivalent conditional in Program.cs:349-355)

**Analysis**: Absence of conditional check proves WebGet always fetches.

---

### Simplicity Evidence

**Command Complexity Comparison**:

| Command | Validation Logic | Conditional Actions | Auto-Enable |
|---------|------------------|---------------------|-------------|
| FindFiles | Adds default glob | Replace requires execute | No |
| WebSearch | Auto-enables GetContent | Fetch requires --get | Yes |
| **WebGet** | **None** | **None** | **No** |
| Run | None | Execute script | No |

**Evidence**: WebGet is the simplest command implementation.

---

## Conclusion

The evidence conclusively demonstrates that WebGet Layer 9 (Actions on Results) is implemented through:

1. **Always-fetch design**: No conditional logic (unlike WebSearch)
2. **Positional URL arguments**: Natural CLI syntax
3. **Inherited options**: All WebCommand Layer 9 options available
4. **100% code reuse**: Shares all fetching infrastructure with WebSearch
5. **No container filtering**: All specified URLs are fetched
6. **Simplest validation**: Just `return this;`
7. **Direct action**: Layer 1 (URLs) → Layer 9 (fetch) with minimal intermediate layers

The **key characteristic** of WebGet Layer 9 is its **unconditional execution** - content fetching always happens because that's the command's core purpose. This contrasts with WebSearch's conditional fetching via `--get` flag.

All claims in the [Layer 9 documentation](cycodmd-webget-layer-9.md) are supported by the source code evidence presented above.
