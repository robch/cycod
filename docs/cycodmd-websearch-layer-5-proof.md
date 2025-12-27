# cycodmd Web Search - Layer 5: Context Expansion - PROOF

**[← Back to Layer 5 Documentation](cycodmd-websearch-layer-5.md)**

## Source Code Evidence

This document provides **detailed source code references** proving that Layer 5 (Context Expansion) is **NOT IMPLEMENTED** for the Web Search command in cycodmd.

---

## 1. Command Class Definition

### File: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 1-38** (entire file):
```csharp
  1: using System.Collections.Generic;
  2: using System.Linq;
  3: 
  4: class WebSearchCommand : WebCommand
  5: {
  6:     public WebSearchCommand()
  7:     {
  8:         Terms = new List<string>();
  9:     }
 10: 
 11:     public List<string> Terms { get; set; }
 12: 
 13:     override public string GetCommandName()
 14:     {
 15:         return "web search";
 16:     }
 17: 
 18:     override public bool IsEmpty()
 19:     {
 20:         return !Terms.Any();
 21:     }
 22: 
 23:     override public CycoDmdCommand Validate()
 24:     {
 25:         var noContent = !GetContent;
 26:         var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();
 27: 
 28:         var assumeGetAndStrip = noContent && hasInstructions;
 29:         if (assumeGetAndStrip)
 30:         {
 31:             GetContent = true;
 32:             StripHtml = true;
 33:         }
 34: 
 35:         return this;
 36:     }
 37: }
```

**Evidence**:
- **Line 11**: Only property is `Terms` (search terms)
- **No properties** for: `IncludeLineCountBefore`, `IncludeLineCountAfter`, `IncludeLineContainsPatternList`, etc.
- The command has **no mechanism** to store context expansion settings

---

## 2. Parent Class Definition

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 1-39** (entire file):
```csharp
  1: using System;
  2: using System.Collections.Generic;
  3: using System.Text.RegularExpressions;
  4: 
  5: abstract class WebCommand : CycoDmdCommand
  6: {
  7:     public WebCommand()
  8:     {
  9:         Interactive = false;
 10: 
 11:         SearchProvider = WebSearchProvider.Google;
 12:         MaxResults = 10;
 13: 
 14:         ExcludeURLContainsPatternList = new();
 15: 
 16:         Browser = BrowserType.Chromium;
 17:         GetContent = false;
 18:         StripHtml = false;
 19: 
 20:         PageInstructionsList = new();
 21:     }
 22: 
 23:     public bool Interactive { get; set; }
 24: 
 25:     public WebSearchProvider SearchProvider { get; set; }
 26:     public List<Regex> ExcludeURLContainsPatternList { get; set; }
 27:     public int MaxResults { get; set; }
 28: 
 29:     public BrowserType Browser { get; set; }
 30:     public bool GetContent { get; set; }
 31:     public bool StripHtml { get; set; }
 32: 
 33:     public string? SaveFolder { get; set; }
 34: 
 35:     public List<Tuple<string, string>> PageInstructionsList;
 36: 
 37:     public string? SavePageOutput { get; set; }
 38: }
```

**Evidence**:
- Properties focus on **page-level operations**: `SearchProvider`, `MaxResults`, `Browser`, `GetContent`, `StripHtml`
- **Line 26**: `ExcludeURLContainsPatternList` - URL-level filtering, not line-level
- **No properties** for line-level filtering or context expansion
- **No inheritance** of context expansion properties from parent class

---

## 3. Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 305-407** (`TryParseWebCommandOptions` method):
```csharp
305:     private bool TryParseWebCommandOptions(WebCommand? command, string[] args, ref int i, string arg)
306:     {
...
313:         else if (arg == "--interactive") { command.Interactive = true; }
...
317:         else if (arg == "--chromium") { command.Browser = BrowserType.Chromium; }
...
329:         else if (arg == "--strip") { command.StripHtml = true; }
...
339:         else if (arg == "--bing") { command.SearchProvider = WebSearchProvider.Bing; }
...
367:         else if (arg == "--max")
368:         {
369:             var max1Arg = GetInputOptionArgs(i + 1, args, 1);
370:             command.MaxResults = ValidateInt(arg, max1Arg.FirstOrDefault(), "max results");
371:             i += max1Arg.Count();
372:         }
373:         else if (arg == "--exclude")
374:         {
375:             var patterns = GetInputOptionArgs(i + 1, args);
376:             var asRegExs = ValidateRegExPatterns(arg, patterns);
377:             command.ExcludeURLContainsPatternList.AddRange(asRegExs);
378:             i += patterns.Count();
379:         }
...
407:     }
```

**Evidence**:
- **No parsing** for `--lines`, `--lines-before`, or `--lines-after`
- **No parsing** for `--line-contains` (required for context expansion to make sense)
- **Line 373-378**: `--exclude` filters URLs, not lines
- Parser does NOT route web commands to `TryParseFindFilesCommandOptions` (where context expansion options are handled)

---

## 4. Execution Path

### File: `src/cycodmd/Program.cs`

**Lines 268-325** (`HandleWebSearchCommandAsync` method):
```csharp
268:     private static async Task<List<Task<string>>> HandleWebSearchCommandAsync(CommandLineOptions commandLineOptions, WebSearchCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
269:     {
270:         var provider = command.SearchProvider;
271:         var query = string.Join(" ", command.Terms);
272:         var maxResults = command.MaxResults;
273:         var excludeURLContainsPatternList = command.ExcludeURLContainsPatternList;
274:         var getContent = command.GetContent;
275:         var stripHtml = command.StripHtml;
...
295:         var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
...
310:         foreach (var url in urls)
311:         {
312:             var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
...
320:             tasks.Add(taskToAdd);
321:         }
322: 
323:         return tasks;
324:     }
```

**Evidence**:
- **Lines 270-276**: Extracts page-level properties from command
- **Line 295**: Calls `GetWebSearchResultUrlsAsync` - returns URLs, not filtered content
- **Line 312**: Calls `GetCheckSaveWebPageContentAsync` - processes entire pages
- **No extraction** of `IncludeLineCountBefore`, `IncludeLineCountAfter` (these properties don't exist on WebSearchCommand)
- **No call** to `LineHelpers.FilterAndExpandContext()` (the function that implements Layer 5 for File Search)

---

## 5. Comparison with File Search

### File: `src/cycodmd/Program.cs`

**Lines 587-595** (File Search command execution - for contrast):
```csharp
587:                 content = LineHelpers.FilterAndExpandContext(
588:                     content,
589:                     includeLineContainsPatternList,
590:                     includeLineCountBefore,     // ← Layer 5 parameter (File Search has this)
591:                     includeLineCountAfter,      // ← Layer 5 parameter (File Search has this)
592:                     includeLineNumbers,
593:                     removeAllLineContainsPatternList,
594:                     backticks,
595:                     highlightMatches);
```

**Evidence**:
- File Search command **calls** `LineHelpers.FilterAndExpandContext()` with context parameters
- Web Search command **does NOT call** this function
- Web Search has **no equivalent** context expansion logic

---

## 6. Helper Function Not Used

### File: `src/common/Helpers/LineHelpers.cs`

The `FilterAndExpandContext` function exists but is **never called** for Web Search commands.

**Lines 48-56** (function signature):
```csharp
 48:     public static string? FilterAndExpandContext(
 49:         string content, 
 50:         List<Regex> includeLineContainsPatternList, 
 51:         int includeLineCountBefore,
 52:         int includeLineCountAfter,
 53:         bool includeLineNumbers, 
 54:         List<Regex> removeAllLineContainsPatternList, 
 55:         string backticks, 
 56:         bool highlightMatches = false)
```

**Evidence**:
- This function implements Layer 5 for File Search
- Searching the codebase shows it's called from `Program.cs` for **File Search only**
- **Not called** for Web Search, Web Get, or Run commands

---

## 7. Data Flow Summary

```
Web Search Execution Path (NO LAYER 5):

User Input: cycodmd web search "machine learning"
           ↓
CycoDmdCommandLineOptions.TryParseWebCommandOptions()
           ↓
WebSearchCommand.Terms = ["machine", "learning"]
WebSearchCommand.MaxResults = 10
           ↓
Program.HandleWebSearchCommandAsync()
           ↓
WebSearchHelpers.GetWebSearchResultUrlsAsync()
           ↓
Returns: List<string> urls (whole URLs, no line filtering)
           ↓
For each URL:
   GetCheckSaveWebPageContentAsync()
           ↓
   Returns: string (whole page content, no context expansion)
           ↓
Output: Full page content (no line-level filtering or expansion)
```

**Contrast with File Search**:
```
File Search includes Layer 5 step:
           ↓
LineHelpers.FilterAndExpandContext(
    includeLineCountBefore,  ← Layer 5 parameter
    includeLineCountAfter    ← Layer 5 parameter
)
           ↓
Returns: Filtered + expanded content
```

---

## Conclusion

**Layer 5 (Context Expansion) is NOT IMPLEMENTED for Web Search** due to:

1. **No command properties**: `WebSearchCommand` and `WebCommand` lack `IncludeLineCountBefore` / `IncludeLineCountAfter` properties
2. **No CLI options**: Parser doesn't recognize `--lines`, `--lines-before`, `--lines-after` for web commands
3. **No execution path**: `HandleWebSearchCommandAsync()` doesn't call `LineHelpers.FilterAndExpandContext()`
4. **No line-level data model**: Web Search operates on **pages** (URLs), not lines

This is a **design decision**, not a bug. Web Search is designed for page-level operations, while File Search is designed for line-level operations.

**[← Back to Layer 5 Documentation](cycodmd-websearch-layer-5.md)**
