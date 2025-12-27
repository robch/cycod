# cycodmd WebGet Command - Layer 6 Proof: Display Control

## Overview

This document provides **source code evidence** for Display Control (Layer 6) in the cycodmd WebGet command. Since WebGet inherits from `WebCommand`, it shares most parsing and property implementations with WebSearch.

---

## Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

WebGet uses **the same parser method** as WebSearch:

#### Interactive Parsing

**Lines 313-316**: (Same as WebSearch)
```csharp
else if (arg == "--interactive")
{
    command.Interactive = true;
}
```

---

#### Browser Selection

**Lines 317-328**: (Same as WebSearch)
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

---

#### Strip HTML Parsing

**Lines 329-332**: (Same as WebSearch)
```csharp
else if (arg == "--strip")
{
    command.StripHtml = true;
}
```

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

WebGet inherits from `WebCommand` base class:

```csharp
abstract class WebCommand : CycoDmdCommand
```

#### Property Declarations

**Lines 23-31**: (Inherited by WebGet)
```csharp
public bool Interactive { get; set; }

public WebSearchProvider SearchProvider { get; set; }
public List<Regex> ExcludeURLContainsPatternList { get; set; }
public int MaxResults { get; set; }

public BrowserType Browser { get; set; }
public bool GetContent { get; set; }
public bool StripHtml { get; set; }
```

---

#### Constructor Initialization

**Lines 8-21**: (Inherited defaults)
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

**Note**: WebGet doesn't use `SearchProvider` or `MaxResults`, but inherits them.

---

### File: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 5-18**:
```csharp
class WebGetCommand : WebCommand
{
    public WebGetCommand() : base()
    {
        Urls = new();
    }

    override public string GetCommandName()
    {
        return "web get";
    }

    // ... validation methods ...

    public List<string> Urls;
}
```

**Explanation**:
- WebGet extends WebCommand
- Adds `Urls` list for direct URL specification
- Inherits all display control properties

---

## Execution Flow

### File: `src/cycodmd/Program.cs`

#### Main Handler Entry Point

**Line 106**:
```csharp
WebGetCommand webGetCommand => HandleWebGetCommand(commandLineOptions, webGetCommand, processor, delayOutputToApplyInstructions),
```

**Explanation**:
- Pattern match in main command dispatcher
- Routes `WebGetCommand` instances to handler method

---

#### Display Properties Extraction

**Lines 329-337**:
```csharp
private static List<Task<string>> HandleWebGetCommand(CommandLineOptions commandLineOptions, WebGetCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
{
    var urls = command.Urls;
    var stripHtml = command.StripHtml;
    var saveToFolder = command.SaveFolder;
    var browserType = command.Browser;
    var interactive = command.Interactive;
    var pageInstructionsList = command.PageInstructionsList;
    var useBuiltInFunctions = command.UseBuiltInFunctions;
    var saveChatHistory = command.SaveChatHistory;
    var savePageOutput = command.SavePageOutput;
```

**Display Control Variables Extracted**:
- Line 330: `stripHtml` - Whether to strip HTML tags
- Line 332: `browserType` - Which browser engine
- Line 333: `interactive` - Interactive vs. headless mode

---

#### URL Validation Display

**Lines 339-346**:
```csharp
var badUrls = command.Urls.Where(l => !l.StartsWith("http")).ToList();
if (badUrls.Any())
{
    var message = (badUrls.Count == 1)
        ? $"Invalid URL: {badUrls[0]}"
        : "Invalid URLs:\n" + string.Join(Environment.NewLine, badUrls.Select(url => "  " + url));
    return new List<Task<string>>() { Task.FromResult(message) };
}
```

**Explanation**:
1. **Validate**: Check all URLs start with "http"
2. **Format Error**: Single URL vs. multiple URLs
3. **Display**: Return error message immediately
4. **Stop**: No further processing if validation fails

**Display Control**:
- Error format is fixed (not user-configurable)
- Single URL: `"Invalid URL: {url}"`
- Multiple URLs: List with indentation

---

#### Page Content Retrieval with Display Settings

**Line 351**:
```csharp
var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
```

**Display Parameters Passed**:
- `stripHtml` - Remove HTML tags
- `browserType` - Browser engine selection
- `interactive` - Browser mode

**Note**: Same function as WebSearch, called from different context (no search results header).

---

#### Page Content Output

**Lines 352-360**:
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

**Display Control**:
- Same output mechanism as WebSearch
- No search results header prepended

---

## Data Flow Summary

### Parsing Phase

```
Command Line: cycodmd web get https://example.com --strip --interactive --firefox
                                                    ↓
CycoDmdCommandLineOptions.TryParseWebCommandOptions()
    Lines 313-316: Parse --interactive → WebCommand.Interactive = true
    Lines 321-324: Parse --firefox → WebCommand.Browser = BrowserType.Firefox
    Lines 329-332: Parse --strip → WebCommand.StripHtml = true
                                                    ↓
WebGetCommand object created (inherits display properties from WebCommand)
```

### Execution Phase

```
Program.Main()
    Line 106: Dispatch to HandleWebGetCommand()
                                                    ↓
HandleWebGetCommand()
    Lines 329-333: Extract display properties
    Lines 339-346: Validate URLs, display error if invalid
    Line 351: Pass display settings to GetCheckSaveWebPageContentAsync()
                                                    ↓
GetCheckSaveWebPageContentAsync()
    - Use browserType and interactive mode to retrieve page
    - Apply stripHtml setting to content
    - Format as markdown
    - Return formatted string
                                                    ↓
Back to HandleWebGetCommand()
    Line 356: ConsoleHelpers.WriteLineIfNotEmpty() - output to console
```

---

## Comparison with WebSearch

### Same Display Options

Both WebSearch and WebGet share:
- ✅ `--strip` (HTML removal)
- ✅ `--interactive` (browser mode)
- ✅ Browser selection (`--chromium`, `--firefox`, `--webkit`)
- ✅ Same page content formatting
- ✅ Same console output mechanism

### Different Display Behavior

**WebSearch has**:
- ✅ Search results header: `## Web Search for '{query}' using {provider}`
- ✅ URL list display
- ✅ Search provider display

**WebGet has**:
- ✅ URL validation error display
- ❌ No search results header
- ❌ No search provider

**Rationale**:
- WebGet retrieves explicit URLs (not search results)
- No search provider involved
- URLs are command arguments, not displayed as results

---

## Testing Scenarios

### Scenario 1: Default Display (Headless Chromium)
```bash
cycodmd web get https://example.com
```

**Expected State**:
- `Interactive`: `false`
- `Browser`: `Chromium` (default)
- `StripHtml`: `false`

**Output**: Page content in markdown with HTML preserved.

---

### Scenario 2: Strip HTML
```bash
cycodmd web get https://example.com --strip
```

**Expected State**:
- `StripHtml`: `true`

**Output**: Page content with HTML tags removed.

---

### Scenario 3: Interactive Firefox
```bash
cycodmd web get https://app.example.com --interactive --firefox
```

**Expected State**:
- `Interactive`: `true`
- `Browser`: `Firefox`

**Output**: Firefox browser opens visibly, retrieves page, displays content.

---

### Scenario 4: Invalid URL Error
```bash
cycodmd web get example.com badurl
```

**Expected State**:
- URL validation fails

**Output**:
```
Invalid URLs:
  example.com
  badurl
```

No page content is retrieved (validation fails first).

---

### Scenario 5: Multiple URLs
```bash
cycodmd web get https://example.com https://example.org
```

**Expected State**:
- Both URLs valid
- Sequential retrieval

**Output**:
```markdown
## https://example.com

[Page content...]

## https://example.org

[Page content...]
```

---

## Related Components

### WebCommand Base Class

WebGet inherits all display properties from `WebCommand`:
- File: `src/cycodmd/CommandLineCommands/WebCommand.cs`
- Lines 23-31: Display properties
- Lines 8-21: Default initialization

### GetCheckSaveWebPageContentAsync

**Function**: Retrieves and formats page content
- Parameters include: `stripHtml`, `browserType`, `interactive`
- Shared by both WebSearch and WebGet
- Returns markdown-formatted string

### URL Validation

**Logic**: `command.Urls.Where(l => !l.StartsWith("http"))`
- Simple string prefix check
- No protocol parsing (accepts both http and https)
- Case-sensitive check

---

## Summary

### Key Implementation Points

1. **Inherited properties** from `WebCommand` base class (WebCommand.cs:23-31)
2. **Shared parser** with WebSearch (`TryParseWebCommandOptions`, lines 313-332)
3. **Display property extraction** in `Program.HandleWebGetCommand` (lines 329-333)
4. **URL validation display** (lines 339-346)
5. **Page content retrieval** with display settings (line 351)
6. **Console output** (line 356)

### Display Control Options Summary

| Option | Purpose | Default | Property | Shared with WebSearch |
|--------|---------|---------|----------|----------------------|
| `--strip` | Remove HTML tags | `false` | `StripHtml` | ✅ Yes |
| `--interactive` | Show browser | `false` | `Interactive` | ✅ Yes |
| `--chromium` | Use Chromium | `true` | `Browser` | ✅ Yes |
| `--firefox` | Use Firefox | `false` | `Browser` | ✅ Yes |
| `--webkit` | Use WebKit | `false` | `Browser` | ✅ Yes |

### Unique WebGet Display Features

- ✅ URL validation error display (lines 339-346)
- ❌ No search results header (unlike WebSearch)

---

## Verification

To verify Layer 6 is working correctly:

1. **Strip HTML**: Run with `--strip`, verify no HTML tags in output
2. **Interactive**: Run with `--interactive`, verify browser window appears
3. **Browser Selection**: Run with `--firefox`, verify Firefox is used
4. **URL Validation**: Run with invalid URL, verify error message format
5. **Multiple URLs**: Run with multiple URLs, verify sequential output
6. **Output Timing**: Verify immediate console output unless `--save-output` or `--instructions` used
