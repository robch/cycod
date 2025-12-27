# cycodmd Web Get - Layer 5: Context Expansion - PROOF

**[← Back to Layer 5 Documentation](cycodmd-webget-layer-5.md)**

## Source Code Evidence

This document provides **detailed source code references** proving that Layer 5 (Context Expansion) is **NOT IMPLEMENTED** for the Web Get command in cycodmd.

---

## 1. Command Class Definition

### File: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 1-27** (entire file):
```csharp
  1: using System.Collections.Generic;
  2: using System.Linq;
  3: 
  4: class WebGetCommand : WebCommand
  5: {
  6:     public WebGetCommand()
  7:     {
  8:         Urls = new List<string>();
  9:     }
 10: 
 11:     public List<string> Urls { get; set; }
 12: 
 13:     override public string GetCommandName()
 14:     {
 15:         return "web get";
 16:     }
 17: 
 18:     override public bool IsEmpty()
 19:     {
 20:         return !Urls.Any();
 21:     }
 22: 
 23:     override public CycoDmdCommand Validate()
 24:     {
 25:         return this;
 26:     }
 27: }
```

**Evidence**:
- **Line 11**: Only property specific to WebGetCommand is `Urls` (list of URLs to fetch)
- **No properties** for: `IncludeLineCountBefore`, `IncludeLineCountAfter`, `IncludeLineContainsPatternList`, etc.
- The command has **no mechanism** to store context expansion settings
- Inherits from `WebCommand` (see next section for parent class analysis)

---

## 2. Parent Class Definition

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 1-39** (entire file - same as Web Search, since they share the same parent):
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
- Properties focus on **page-level operations**: `Browser`, `GetContent`, `StripHtml`, `SaveFolder`
- **No properties** for line-level filtering or context expansion
- **No inheritance** of context expansion properties from parent class
- Same parent as `WebSearchCommand`, so same lack of Layer 5 support

---

## 3. Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Positional Argument Parsing

**Lines 472-476** (TryParseOtherCommandArg method):
```csharp
472:         else if (command is WebGetCommand webGetCommand)
473:         {
474:             webGetCommand.Urls.Add(arg);
475:             parsedOption = true;
476:         }
```

**Evidence**:
- **Lines 472-476**: Positional arguments are added to `Urls` list
- No parsing for context expansion options in positional arguments

#### Option Parsing

**Lines 305-407** (`TryParseWebCommandOptions` method - shared with WebSearchCommand):
```csharp
305:     private bool TryParseWebCommandOptions(WebCommand? command, string[] args, ref int i, string arg)
306:     {
...
313:         else if (arg == "--interactive") { command.Interactive = true; }
317:         else if (arg == "--chromium") { command.Browser = BrowserType.Chromium; }
...
329:         else if (arg == "--strip") { command.StripHtml = true; }
333:         else if (arg == "--save-page-folder")
334:         {
335:             var max1Arg = GetInputOptionArgs(i + 1, args, 1);
336:             command.SaveFolder = max1Arg.FirstOrDefault() ?? "web-pages";
337:             i += max1Arg.Count();
338:         }
...
407:     }
```

**Evidence**:
- **No parsing** for `--lines`, `--lines-before`, or `--lines-after`
- **No parsing** for `--line-contains` (prerequisite for context expansion)
- Parser handles page-level options only: `--strip`, `--save-page-folder`, `--interactive`, etc.
- Parser does NOT route web commands to `TryParseFindFilesCommandOptions` (where context expansion is handled)

---

## 4. Execution Path

### File: `src/cycodmd/Program.cs`

**Lines 327-365** (`HandleWebGetCommand` method):
```csharp
327:     private static List<Task<string>> HandleWebGetCommand(CommandLineOptions commandLineOptions, WebGetCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
328:     {
329:         var urls = command.Urls;
330:         var stripHtml = command.StripHtml;
331:         var saveToFolder = command.SaveFolder;
332:         var browserType = command.Browser;
333:         var interactive = command.Interactive;
334:         var pageInstructionsList = command.PageInstructionsList;
335:         var useBuiltInFunctions = command.UseBuiltInFunctions;
336:         var saveChatHistory = command.SaveChatHistory;
337:         var savePageOutput = command.SavePageOutput;
338: 
339:         var badUrls = command.Urls.Where(l => !l.StartsWith("http")).ToList();
340:         if (badUrls.Any())
341:         {
342:             var message = (badUrls.Count == 1)
343:                 ? $"Invalid URL: {badUrls[0]}"
344:                 : "Invalid URLs:\n" + string.Join(Environment.NewLine, badUrls.Select(url => "  " + url));
345:             return new List<Task<string>>() { Task.FromResult(message) };
346:         }
347: 
348:         var tasks = new List<Task<string>>();
349:         foreach (var url in urls)
350:         {
351:             var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
352:             var taskToAdd = delayOutputToApplyInstructions
353:                 ? getCheckSaveTask
354:                 : getCheckSaveTask.ContinueWith(t =>
355:                 {
356:                     ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
357:                     return t.Result;
358:                 });
359: 
360:             tasks.Add(taskToAdd);
361:         }
362: 
363:         return tasks;
364:     }
```

**Evidence**:
- **Lines 329-337**: Extracts page-level properties from command
- **Lines 339-345**: Validates URLs (page-level validation)
- **Lines 349-360**: Iterates over URLs, fetching entire pages
- **Line 351**: Calls `GetCheckSaveWebPageContentAsync` - processes entire pages, not filtered lines
- **No extraction** of `IncludeLineCountBefore`, `IncludeLineCountAfter` (these properties don't exist)
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
- Web Get command **does NOT call** this function
- Web Get has **no equivalent** context expansion logic

---

## 6. Helper Function Not Used

### File: `src/common/Helpers/LineHelpers.cs`

The `FilterAndExpandContext` function exists but is **never called** for Web Get commands.

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
Web Get Execution Path (NO LAYER 5):

User Input: cycodmd web get https://example.com/page1 https://example.com/page2
           ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()
           ↓
WebGetCommand.Urls = ["https://example.com/page1", "https://example.com/page2"]
           ↓
Program.HandleWebGetCommand()
           ↓
For each URL:
   GetCheckSaveWebPageContentAsync()
           ↓
   Returns: string (whole page content, no filtering or expansion)
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

## 8. Difference from Web Search

While both Web Get and Web Search lack Layer 5 implementation, there are minor differences in their execution:

| Aspect | Web Search | Web Get |
|--------|------------|---------|
| **Layer 1 (Target Selection)** | Search terms → discover URLs | Explicit URLs |
| **Layer 2 (Container Filter)** | `--exclude` filters URLs | No filtering |
| **Layer 5 (Context Expansion)** | ❌ Not implemented | ❌ Not implemented |
| **Execution Method** | `HandleWebSearchCommandAsync()` | `HandleWebGetCommand()` |

**Evidence**: Different methods in `Program.cs` (lines 268-325 vs 327-364), but both call the same `GetCheckSaveWebPageContentAsync()` helper for page retrieval.

---

## Conclusion

**Layer 5 (Context Expansion) is NOT IMPLEMENTED for Web Get** due to:

1. **No command properties**: `WebGetCommand` and `WebCommand` lack `IncludeLineCountBefore` / `IncludeLineCountAfter` properties
2. **No CLI options**: Parser doesn't recognize `--lines`, `--lines-before`, `--lines-after` for web commands
3. **No execution path**: `HandleWebGetCommand()` doesn't call `LineHelpers.FilterAndExpandContext()`
4. **No line-level data model**: Web Get operates on **pages** (URLs), not lines

This is a **design decision**, not a bug. Web Get is designed for page-level operations, while File Search is designed for line-level operations.

**[← Back to Layer 5 Documentation](cycodmd-webget-layer-5.md)**
