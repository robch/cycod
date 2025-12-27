# cycodmd WebSearch Command - Layer 9 Proof: Actions on Results

## Overview

This document provides **source code evidence** for Layer 9 (Actions on Results) of the cycodmd WebSearch command, tracing the implementation from command-line parsing through execution.

---

## 1. Command-Line Parsing

### Option: `--get`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 363-366**:
```csharp
else if (arg == "--get")
{
    command.GetContent = true;
}
```

**Evidence**:
- Option is parsed in `TryParseWebCommandOptions()` method
- Boolean flag (no value required)
- Sets `WebCommand.GetContent` to `true`
- Default value is `false` (URLs only, no content fetching)

---

### Option: `--interactive`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 313-316**:
```csharp
else if (arg == "--interactive")
{
    command.Interactive = true;
}
```

**Evidence**:
- Parsed in `TryParseWebCommandOptions()` method
- Boolean flag enabling interactive browser mode
- Sets `WebCommand.Interactive` to `true`
- Allows manual navigation before content extraction

---

### Option: `--save-page-folder`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 333-338**:
```csharp
else if (arg == "--save-page-folder")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, 1);
    command.SaveFolder = max1Arg.FirstOrDefault() ?? "web-pages";
    i += max1Arg.Count();
}
```

**Evidence**:
- Takes optional folder name argument
- Default value: `"web-pages"`
- Stores in `WebCommand.SaveFolder`
- Used to persist fetched pages locally

---

## 2. Command Properties

### Property Storage

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 23, 30, 33**:
```csharp
public bool Interactive { get; set; }
// ...
public bool GetContent { get; set; }
// ...
public string? SaveFolder { get; set; }
```

**Evidence**:
- `Interactive`: Boolean for interactive browser mode
- `GetContent`: Boolean to enable content fetching
- `SaveFolder`: Nullable string for local page storage
- All properties are public and accessible from parsing and execution

---

### Property Initialization

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 9, 17-18, 33**:
```csharp
Interactive = false;
// ...
GetContent = false;
StripHtml = false;
// ...
// SaveFolder is initialized to null (not set in constructor)
```

**Evidence**:
- `Interactive` defaults to `false` (headless browser)
- `GetContent` defaults to `false` (URL list only)
- `StripHtml` defaults to `false` (preserve HTML)
- `SaveFolder` defaults to `null` (no persistence)
- Safe-by-default: minimal action unless explicitly requested

---

## 3. Auto-Enable Logic (Implicit Action)

### Validation Method

**File**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 23-35**:
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

**Evidence**:
- **Condition 1**: `noContent = !GetContent` (user didn't specify `--get`)
- **Condition 2**: `hasInstructions` (user provided AI instructions)
- **Logic**: If both conditions true, automatically enable content fetching
- **Action**: Sets `GetContent = true` and `StripHtml = true`
- **Rationale**: AI can't process URLs alone; needs actual content

**Analysis**:
This is the **key innovation** in WebSearch Layer 9: implicit action triggering based on user intent. If user wants AI processing, content must be fetched, so the system auto-enables it.

---

## 4. Execution Logic

### Property Extraction

**File**: `src/cycodmd/Program.cs`

**Lines 274-278**:
```csharp
var getContent = command.GetContent;
var stripHtml = command.StripHtml;
var saveToFolder = command.SaveFolder;
var browserType = command.Browser;
var interactive = command.Interactive;
```

**Evidence**:
- All Layer 9 properties extracted from command object
- `getContent`: determines if content is fetched
- `saveToFolder`: where to save pages
- `interactive`: whether to show browser UI
- `browserType` and `stripHtml`: related Layer 6 properties

---

### Search Execution

**File**: `src/cycodmd/Program.cs`

**Line 295**:
```csharp
var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
```

**Evidence**:
- First stage: get search result URLs
- Passes `browserType` and `interactive` for browser control
- Returns list of URLs (no content yet)

---

### Content Fetch Conditional

**File**: `src/cycodmd/Program.cs`

**Lines 302-305**:
```csharp
if (urls.Count == 0 || !getContent)
{
    return new List<Task<string>>() { Task.FromResult(searchSection) };
}
```

**Evidence**:
- **Critical conditional**: Only fetch content if `getContent == true`
- **Early exit**: If `!getContent`, return URLs only (no fetching)
- **Layer 9 gate**: This is where Layer 9 action is enabled/disabled
- URLs are returned without fetching page content

---

### Content Fetching Loop

**File**: `src/cycodmd/Program.cs`

**Lines 310-313**:
```csharp
foreach (var url in urls)
{
    var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
    var taskToAdd = delayOutputToApplyInstructions
        ? getCheckSaveTask
        // ...
```

**Evidence**:
- For each URL, create task to fetch content
- Passes all Layer 9 parameters: `saveToFolder`, `browserType`, `interactive`
- Also passes Layer 8 parameters: `pageInstructionsList`, `useBuiltInFunctions`, `saveChatHistory`
- Demonstrates Layer 8-9 integration

---

## 5. Content Fetching Methods

### Top-Level Fetch Method

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
- Method signature includes all Layer 9 parameters
- `saveToFolder`, `browserType`, `interactive` are passed down
- Calls next method in chain: `GetFinalWebPageContentAsync`

---

### Final Content Method

**File**: `src/cycodmd/Program.cs`

**Lines 659-662**:
```csharp
private static async Task<string> GetFinalWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
{
    var formatted = await GetFormattedWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive);
```

**Evidence**:
- Continues parameter chain
- Calls `GetFormattedWebPageContentAsync` to actually fetch content
- Then applies AI instructions if present (Layer 8)

---

### Browser Fetch Method

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
- Passes all Layer 9 browser control parameters
- Returns content and title for formatting

---

## 6. Browser Automation Layer

### PlaywrightHelpers Main Method

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
- Creates browser instance using `GetBrowser` method
- `interactive` parameter affects browser headless mode

---

### Browser Creation

**File**: `src/cycodmd/Helpers/PlaywrightHelpers.cs`

**Lines 222-232**:
```csharp
private static async Task<IBrowser> GetBrowser(BrowserType browserType, bool interactive, IPlaywright playwright)
{
    try
    {
        return browserType switch
        {
            BrowserType.Chromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
            BrowserType.Firefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
            BrowserType.Webkit => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
            _ => throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null)
        };
```

**Evidence**:
- **Critical line**: `Headless = !interactive`
- If `interactive == true`, browser is launched with GUI (headless = false)
- If `interactive == false` (default), browser runs headlessly
- Supports three browser types: Chromium, Firefox, Webkit
- Browser choice is a Layer 6 option, but impacts Layer 9 action execution

---

### Search Result URL Extraction

**File**: `src/cycodmd/Helpers/PlaywrightHelpers.cs`

**Lines 16-22**:
```csharp
public static async Task<List<string>> GetWebSearchResultUrlsAsync(string searchEngine, string query, int maxResults, List<Regex> excludeURLContainsPatternList, BrowserType browserType, bool interactive)
{
    // Initialize Playwright
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await GetBrowser(browserType, interactive, playwright);
    var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();
```

**Evidence**:
- URL extraction also uses `interactive` parameter
- Even search result fetching can be interactive
- Allows user to see search page before extraction

---

### Interactive Delays

**File**: `src/cycodmd/Helpers/PlaywrightHelpers.cs`

**Lines 102, 140, 178, 216**:
```csharp
if (interactive) Task.Delay(10000).Wait();
```

**Evidence**:
- When `interactive == true`, delays are added
- Gives user time to see browser state
- Appears in error handling blocks
- Helps with debugging interactive sessions

---

## 7. Data Flow Trace

### Complete Call Stack for Content Fetching

```
1. User Command:
   cycodmd web search "query" --get --save-page-folder my-pages --interactive

2. CycoDmdCommandLineOptions.Parse()
   ├─ TryParseWebCommandOptions()
   │  ├─ Parse "--get"
   │  │  └─ command.GetContent = true
   │  ├─ Parse "--save-page-folder my-pages"
   │  │  └─ command.SaveFolder = "my-pages"
   │  └─ Parse "--interactive"
   │     └─ command.Interactive = true
   └─ Returns WebSearchCommand

3. WebSearchCommand.Validate()
   └─ Check if AI instructions present (no in this case)
   └─ Return command unchanged (GetContent already true)

4. Program.ProcessWebSearchCommand()
   ├─ Extract properties:
   │  ├─ getContent = true
   │  ├─ saveToFolder = "my-pages"
   │  └─ interactive = true
   ├─ Fetch search URLs:
   │  └─ WebSearchHelpers.GetWebSearchResultUrlsAsync(..., browserType, interactive)
   ├─ Check: if (!getContent) → FALSE (getContent is true)
   └─ For each URL:
      └─ GetCheckSaveWebPageContentAsync(url, ..., saveToFolder, browserType, interactive, ...)

5. GetCheckSaveWebPageContentAsync()
   └─ GetFinalWebPageContentAsync(url, ..., saveToFolder, browserType, interactive, ...)

6. GetFinalWebPageContentAsync()
   └─ GetFormattedWebPageContentAsync(url, ..., saveToFolder, browserType, interactive)

7. GetFormattedWebPageContentAsync()
   └─ PlaywrightHelpers.GetPageAndTitle(url, ..., saveToFolder, browserType, interactive)

8. PlaywrightHelpers.GetPageAndTitle()
   ├─ GetBrowser(browserType, interactive, playwright)
   │  └─ Launch browser with: Headless = !interactive (= !true = false)
   │  └─ Browser launches with GUI visible
   ├─ Navigate to URL
   ├─ Extract content
   ├─ If saveToFolder != null:
   │  └─ Save page to "my-pages/" directory
   └─ Return (content, title)
```

---

## 8. Implicit Auto-Enable Trace

### Example: AI Instructions Without --get

**Command**:
```bash
cycodmd web search "topic" --instructions "Summarize"
```

**Trace**:
```
1. Parse: GetContent = false (no --get flag)
2. Parse: InstructionsList.Add("Summarize")

3. WebSearchCommand.Validate()
   ├─ noContent = !GetContent = !false = true
   ├─ hasInstructions = InstructionsList.Any() = true
   ├─ assumeGetAndStrip = noContent && hasInstructions = true && true = true
   └─ If true:
      ├─ GetContent = true  ← AUTO-ENABLED
      └─ StripHtml = true   ← AUTO-ENABLED

4. Execution:
   ├─ getContent = true (auto-enabled)
   └─ Content is fetched despite no --get flag
```

**Evidence**: This demonstrates the **implicit action trigger** pattern unique to WebSearch Layer 9.

---

## 9. Integration Points

### With Layer 1 (Target Selection)

**Evidence** (Program.cs:295):
```csharp
var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
```

**Analysis**:
- Layer 1's `query` and `provider` determine what to search
- Layer 9's `interactive` and `browserType` determine how to fetch
- Layer 9 operates on URLs produced by Layer 1

---

### With Layer 2 (Container Filter)

**Evidence**: `excludeURLContainsPatternList` is passed to search and fetch methods

**Analysis**:
- Layer 2 filters which URLs to fetch
- Layer 9 only fetches non-excluded URLs
- Filtering happens before action execution

---

### With Layer 6 (Display Control)

**Evidence**:
- `stripHtml` (Layer 6) affects content fetching (Layer 9)
- `browserType` (Layer 6) determines fetch mechanism (Layer 9)

**Analysis**: Strong coupling between display control and action execution.

---

### With Layer 7 (Output Persistence)

**Evidence**: `saveToFolder` enables local page caching

**Analysis**:
- Layer 9 fetches content
- Layer 7 persists it
- Both work together for offline access

---

### With Layer 8 (AI Processing)

**Evidence**: Auto-enable logic in `Validate()` method

**Analysis**:
- Layer 8 presence triggers Layer 9 action
- Most direct layer-to-layer automation in entire pipeline
- Demonstrates intelligent default behavior

---

## 10. WebGetCommand Comparison

### WebGet Uses Same Infrastructure

**File**: `src/cycodmd/Program.cs`

**Lines 329-333**:
```csharp
var urls = command.Urls;
var stripHtml = command.StripHtml;
var saveToFolder = command.SaveFolder;
var browserType = command.Browser;
var interactive = command.Interactive;
```

**Line 351**:
```csharp
var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
```

**Evidence**:
- WebGetCommand uses identical properties
- Same fetching methods
- Difference: URLs are direct input, not search results
- Layer 9 implementation is shared between WebSearch and WebGet

---

## 11. Browser Type Support

### Web Search URL Helpers

**File**: `src/cycodmd/Helpers/WebSearchHelpers.cs`

**Lines 13-15**:
```csharp
WebSearchProvider.BingAPI => await BingApiWebSearchHelpers.GetWebSearchResultUrlsAsync(query, maxResults, excludeURLContainsPatternList, browserType, interactive),
WebSearchProvider.GoogleAPI => await GoogleApiWebSearchHelpers.GetWebSearchResultUrlsAsync(query, maxResults, excludeURLContainsPatternList, browserType, interactive),
_ => await PlaywrightHelpers.GetWebSearchResultUrlsAsync(providerString, query, maxResults, excludeURLContainsPatternList, browserType, interactive)
```

**Evidence**:
- `browserType` and `interactive` are passed to all search providers
- API-based searches also support these parameters (even if they don't use browsers)
- Consistent interface across search providers

---

## 12. Summary of Evidence

### Parser Evidence
✅ `--get` parsed at CycoDmdCommandLineOptions.cs:363-366  
✅ `--interactive` parsed at CycoDmdCommandLineOptions.cs:313-316  
✅ `--save-page-folder` parsed at CycoDmdCommandLineOptions.cs:333-338

### Property Evidence
✅ `GetContent` property defined at WebCommand.cs:30  
✅ `Interactive` property defined at WebCommand.cs:23  
✅ `SaveFolder` property defined at WebCommand.cs:33  
✅ Default values set at WebCommand.cs:9, 17-18

### Validation Evidence
✅ Auto-enable logic at WebSearchCommand.cs:23-35  
✅ Implicit action trigger based on AI instructions

### Execution Evidence
✅ Content fetch conditional at Program.cs:302-305  
✅ Content fetch loop at Program.cs:310-313  
✅ Browser launch with headless control at PlaywrightHelpers.cs:228-230  
✅ Interactive delays at PlaywrightHelpers.cs:102, 140, 178, 216

### Integration Evidence
✅ Layer 8 integration via auto-enable  
✅ Shared infrastructure with WebGetCommand  
✅ Browser type support across all providers

---

## Conclusion

The evidence conclusively demonstrates that WebSearch Layer 9 (Actions on Results) is implemented through:

1. **Primary action**: Content fetching via `GetContent` property
2. **Implicit trigger**: Auto-enable when AI instructions present
3. **Interactive control**: `Interactive` flag for visible browser
4. **Local persistence**: `SaveFolder` for offline page storage
5. **Conditional execution**: Gate at Program.cs:302 determines if action occurs
6. **Shared infrastructure**: Same fetching methods used by WebGetCommand

The **key innovation** is the implicit action trigger in `WebSearchCommand.Validate()`, which intelligently enables content fetching when AI processing is requested, demonstrating a "smart default" design pattern.

All claims in the [Layer 9 documentation](cycodmd-websearch-layer-9.md) are supported by the source code evidence presented above.
