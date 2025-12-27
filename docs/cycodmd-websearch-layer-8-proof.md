# cycodmd Web Search - Layer 8: AI Processing (PROOF)

## Source Code Evidence

This document provides detailed evidence from the source code showing how AI Processing (Layer 8) is implemented for the cycodmd web search command.

---

## 1. Command-Line Option Parsing

### Location: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### `--page-instructions` and Pattern-Specific Variants (Lines 382-393)

```csharp
Line 382:     else if (arg.StartsWith("--") && arg.EndsWith("page-instructions"))
Line 383:     {
Line 384:         var instructions = GetInputOptionArgs(i + 1, args);
Line 385:         if (instructions.Count() == 0)
Line 386:         {
Line 387:             throw new CommandLineException($"Missing instructions for {arg}");
Line 388:         }
Line 389:         var webPageCriteria = arg != "--page-instructions"
Line 390:             ?arg.Substring(2, arg.Length - 20)
Line 391:             : string.Empty;
Line 392:         var withCriteria = instructions.Select(x => Tuple.Create(x, webPageCriteria));
Line 393:         command.PageInstructionsList.AddRange(withCriteria);
Line 394:         i += instructions.Count();
Line 395:     }
```

**Evidence**: 
- Pattern extraction: `--github-page-instructions` → `"github"`
- Empty pattern for `--page-instructions` → `string.Empty` (matches all)
- Stores as `Tuple<string, string>` where Item1 = instruction, Item2 = URL pattern

#### `--instructions`, `--built-in-functions`, `--save-chat-history` (Shared with File Search)

These options are handled by `TryParseSharedCycoDmdCommandOptions` (Lines 418-444), identical to file search. See file search proof document for details.

---

## 2. Data Structures

### Location: `src/cycodmd/CommandLineCommands/WebCommand.cs`

#### Base Web Command Properties (Lines 1-38)

```csharp
Line 5:  abstract class WebCommand : CycoDmdCommand
Line 6:  {
Line 7:      public WebCommand()
Line 8:      {
Line 9:          Interactive = false;
Line 10:
Line 11:         SearchProvider = WebSearchProvider.Google;
Line 12:         MaxResults = 10;
Line 13:
Line 14:         ExcludeURLContainsPatternList = new();
Line 15:
Line 16:         Browser = BrowserType.Chromium;
Line 17:         GetContent = false;
Line 18:         StripHtml = false;
Line 19:
Line 20:         PageInstructionsList = new();
Line 21:     }
...
Line 35:     public List<Tuple<string, string>> PageInstructionsList;
```

**Evidence**: 
- `PageInstructionsList` stores page-specific instructions
- Inherits `InstructionsList` from `CycoDmdCommand` (for global instructions)
- `GetContent` controls whether pages are fetched
- `StripHtml` controls HTML stripping

### Location: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

#### Web Search Command (Lines 1-37)

```csharp
Line 4:  class WebSearchCommand : WebCommand
Line 5:  {
Line 6:      public WebSearchCommand()
Line 7:      {
Line 8:          Terms = new List<string>();
Line 9:      }
Line 10:
Line 11:     public List<string> Terms { get; set; }
```

**Evidence**: Search terms stored in `Terms` list.

---

## 3. Automatic Content Fetching Logic

### Location: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

#### Validation and Auto-Enable (Lines 23-36)

```csharp
Line 23:     override public CycoDmdCommand Validate()
Line 24:     {
Line 25:         var noContent = !GetContent;
Line 26:         var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();
Line 27:
Line 28:         var assumeGetAndStrip = noContent && hasInstructions;
Line 29:         if (assumeGetAndStrip)
Line 30:         {
Line 31:             GetContent = true;
Line 32:             StripHtml = true;
Line 33:         }
Line 34:
Line 35:         return this;
Line 36:     }
```

**Evidence**: 
- If ANY instructions are present (`PageInstructionsList` OR `InstructionsList`)
- AND content fetching is not explicitly enabled (`!GetContent`)
- THEN automatically enable both `GetContent` and `StripHtml`

**Result**: Users don't need to specify `--get --strip` if they're using AI instructions.

---

## 4. Execution Flow

### Location: `src/cycodmd/Program.cs`

#### Web Search Handler (Lines 268-325)

```csharp
Line 268:    private static async Task<List<Task<string>>> HandleWebSearchCommandAsync(CommandLineOptions commandLineOptions, WebSearchCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
Line 269:    {
Line 270:        var provider = command.SearchProvider;
Line 271:        var query = string.Join(" ", command.Terms);
Line 272:        var maxResults = command.MaxResults;
Line 273:        var excludeURLContainsPatternList = command.ExcludeURLContainsPatternList;
Line 274:        var getContent = command.GetContent;
Line 275:        var stripHtml = command.StripHtml;
Line 276:        var saveToFolder = command.SaveFolder;
Line 277:        var browserType = command.Browser;
Line 278:        var interactive = command.Interactive;
Line 279:        var useBuiltInFunctions = command.UseBuiltInFunctions;
Line 280:        var saveChatHistory = command.SaveChatHistory;
Line 281:
Line 282:        var savePageOutput = command.SavePageOutput;
Line 283:        var pageInstructionsList = command.PageInstructionsList
Line 284:            .Select(x => Tuple.Create(
Line 285:                x.Item1
Line 286:                    .Replace("{searchTerms}", query)
Line 287:                    .Replace("{query}", query)
Line 288:                    .Replace("{terms}", query)
Line 289:                    .Replace("{q}", query),
Line 290:                x.Item2.ToLowerInvariant()))
Line 291:            .ToList();
...
```

**Evidence**: 
- Template variables (`{searchTerms}`, `{query}`, `{terms}`, `{q}`) are **all** replaced with the search query
- URL pattern criteria are converted to **lowercase** for case-insensitive matching

#### Search Execution and URL Collection (Lines 293-305)

```csharp
Line 293:        var searchSectionHeader = $"## Web Search for '{query}' using {provider}";
Line 294:
Line 295:        var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
Line 296:        var searchSection = urls.Count == 0
Line 297:            ? $"{searchSectionHeader}\n\nNo results found\n"
Line 298:            : $"{searchSectionHeader}\n\n" + string.Join("\n", urls) + "\n";
Line 299:
Line 300:        if (!delayOutputToApplyInstructions) ConsoleHelpers.WriteLine(searchSection);
Line 301:
Line 302:        if (urls.Count == 0 || !getContent)
Line 303:        {
Line 304:            return new List<Task<string>>() { Task.FromResult(searchSection) };
Line 305:        }
```

**Evidence**: 
- Search results are displayed immediately (unless global instructions present)
- If no URLs found OR content fetching disabled, return early
- Search results section is always included in output

#### Per-Page Processing Loop (Lines 307-324)

```csharp
Line 307:        var tasks = new List<Task<string>>();
Line 308:        tasks.Add(Task.FromResult(searchSection));
Line 309:
Line 310:        foreach (var url in urls)
Line 311:        {
Line 312:            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(
Line 313:                url, 
Line 314:                stripHtml, 
Line 315:                saveToFolder, 
Line 316:                browserType, 
Line 317:                interactive, 
Line 318:                pageInstructionsList,  // <-- AI instructions passed here
Line 319:                useBuiltInFunctions, 
Line 320:                saveChatHistory, 
Line 321:                savePageOutput);
Line 322:            var taskToAdd = delayOutputToApplyInstructions
Line 323:                ? getCheckSaveTask
Line 324:                : getCheckSaveTask.ContinueWith(t =>
Line 325:                {
Line 326:                    ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
Line 327:                    return t.Result;
Line 328:                });
Line 329:
Line 330:            tasks.Add(taskToAdd);
Line 331:        }
Line 332:
Line 333:        return tasks;
Line 334:    }
```

**Evidence**: 
- Each URL is processed individually
- Page instructions passed to `GetCheckSaveWebPageContentAsync`
- Output is either displayed immediately OR delayed for global instructions
- All tasks collected and returned

---

## 5. Per-Page AI Processing Implementation

### Location: `src/cycodmd/Program.cs`

#### Main Per-Page Function (Lines 636-656)

```csharp
Line 636:    private static async Task<string> GetCheckSaveWebPageContentAsync(
                 string url, 
                 bool stripHtml, 
                 string? saveToFolder, 
                 BrowserType browserType, 
                 bool interactive, 
                 List<Tuple<string, string>> pageInstructionsList,  // <-- Instructions parameter
                 bool useBuiltInFunctions, 
                 string? saveChatHistory, 
                 string? savePageOutput)
Line 637:    {
Line 638:        try
Line 639:        {
Line 640:            ConsoleHelpers.DisplayStatus($"Processing: {url} ...");
Line 641:            var finalContent = await GetFinalWebPageContentAsync(
                        url, 
                        stripHtml, 
                        saveToFolder, 
                        browserType, 
                        interactive, 
                        pageInstructionsList,  // <-- Instructions passed through
                        useBuiltInFunctions, 
                        saveChatHistory);
...
```

#### URL Pattern Matching and AI Application (Lines 659-672)

```csharp
Line 659:    private static async Task<string> GetFinalWebPageContentAsync(
                 string url, 
                 bool stripHtml, 
                 string? saveToFolder, 
                 BrowserType browserType, 
                 bool interactive, 
                 List<Tuple<string, string>> pageInstructionsList, 
                 bool useBuiltInFunctions, 
                 string? saveChatHistory)
Line 660:    {
Line 661:        var formatted = await GetFormattedWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive);
Line 662:
Line 663:        var instructionsForThisPage = pageInstructionsList
Line 664:            .Where(x => WebPageMatchesInstructionsCriteria(url, x.Item2))
Line 665:            .Select(x => x.Item1)
Line 666:            .ToList();
Line 667:
Line 668:        var afterInstructions = instructionsForThisPage.Any()
Line 669:            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisPage, formatted, useBuiltInFunctions, saveChatHistory)
Line 670:            : formatted;
Line 671:
Line 672:        return afterInstructions;
Line 673:    }
```

**Evidence**: 
1. Page content is fetched and formatted first
2. Instructions are filtered by URL pattern matching
3. If matching instructions exist, AI processing is applied
4. Otherwise, original formatted content is returned

#### URL Pattern Matching Logic (Lines 675-687)

```csharp
Line 675:    private static bool WebPageMatchesInstructionsCriteria(string url, string criteria)
Line 676:    {
Line 677:        if (string.IsNullOrEmpty(criteria))
Line 678:        {
Line 679:            return true;  // Empty criteria matches all pages
Line 680:        }
Line 681:
Line 682:        var lowerUrl = url.ToLowerInvariant();
Line 683:        var lowerCriteria = criteria.ToLowerInvariant();
Line 684:        return lowerUrl.Contains(lowerCriteria);
Line 685:    }
```

**Evidence**: 
- Empty criteria (from `--page-instructions`) matches **all** pages
- Non-empty criteria (from `--github-page-instructions`) uses **case-insensitive substring matching**
- Simple `Contains` check (no regex or complex patterns)

**Example Matching**:
- `--github-page-instructions` → criteria = `"github"`
- URL: `https://github.com/user/repo` → `lowerUrl.Contains("github")` → **TRUE**
- URL: `https://stackoverflow.com/...` → `lowerUrl.Contains("github")` → **FALSE**

---

## 6. Global AI Processing

### Location: `src/cycodmd/Program.cs`

#### Global Processing (Lines 114-131)

```csharp
Line 114:    var shouldSaveOutput = cycoDmdCommand != null && !string.IsNullOrEmpty(cycoDmdCommand.SaveOutput);
Line 115:    if (shouldSaveOutput || delayOutputToApplyInstructions)
Line 116:    {
Line 117:        await Task.WhenAll(tasksThisCommand.ToArray());
Line 118:        var commandOutput = string.Join("\n", tasksThisCommand.Select(t => t.Result));
Line 119:
Line 120:        if (delayOutputToApplyInstructions)
Line 121:        {
Line 122:            commandOutput = AiInstructionProcessor.ApplyAllInstructions(
Line 123:                cycoDmdCommand!.InstructionsList, 
Line 124:                commandOutput, 
Line 125:                cycoDmdCommand.UseBuiltInFunctions, 
Line 126:                cycoDmdCommand.SaveChatHistory);
Line 127:            ConsoleHelpers.WriteLine(commandOutput);
Line 128:        }
...
```

**Evidence**: 
- **Identical** to file search global processing
- All task results (search section + all page content) are joined
- Global instructions applied to combined output
- Output displayed after AI processing completes

---

## 7. Web Get Command Implementation

### Location: `src/cycodmd/Program.cs`

#### Web Get Handler (Lines 327-364)

```csharp
Line 327:    private static List<Task<string>> HandleWebGetCommand(CommandLineOptions commandLineOptions, WebGetCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
Line 328:    {
Line 329:        var urls = command.Urls;
Line 330:        var stripHtml = command.StripHtml;
Line 331:        var saveToFolder = command.SaveFolder;
Line 332:        var browserType = command.Browser;
Line 333:        var interactive = command.Interactive;
Line 334:        var pageInstructionsList = command.PageInstructionsList;
Line 335:        var useBuiltInFunctions = command.UseBuiltInFunctions;
Line 336:        var saveChatHistory = command.SaveChatHistory;
Line 337:        var savePageOutput = command.SavePageOutput;
...
Line 349:        foreach (var url in urls)
Line 350:        {
Line 351:            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(
                        url, 
                        stripHtml, 
                        saveToFolder, 
                        browserType, 
                        interactive, 
                        pageInstructionsList,  // <-- Same AI processing
                        useBuiltInFunctions, 
                        saveChatHistory, 
                        savePageOutput);
Line 352:            var taskToAdd = delayOutputToApplyInstructions
Line 353:                ? getCheckSaveTask
Line 354:                : getCheckSaveTask.ContinueWith(t =>
Line 355:                {
Line 356:                    ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
Line 357:                    return t.Result;
Line 358:                });
Line 359:
Line 360:            tasks.Add(taskToAdd);
Line 361:        }
Line 362:
Line 363:        return tasks;
Line 364:    }
```

**Evidence**: 
- **Web Get uses the same AI processing** as Web Search
- Same function: `GetCheckSaveWebPageContentAsync`
- Same instruction filtering and application
- Only difference: no search results section

**Conclusion**: AI Processing Layer 8 is **identical** for Web Search and Web Get, except Web Search includes a search results section in the output.

---

## 8. Call Stack Summary

### Web Search Per-Page AI Processing

```
Main (Program.cs:97-132)
  └─> HandleWebSearchCommandAsync (Program.cs:268-334)
        ├─> WebSearchHelpers.GetWebSearchResultUrlsAsync (get URLs)
        └─> For each URL:
              └─> GetCheckSaveWebPageContentAsync (Program.cs:636-656)
                    └─> GetFinalWebPageContentAsync (Program.cs:659-673)
                          ├─> GetFormattedWebPageContentAsync (fetch + format)
                          ├─> WebPageMatchesInstructionsCriteria (Program.cs:675-685)
                          └─> AiInstructionProcessor.ApplyAllInstructions (AiInstructionProcessor.cs:10-21)
```

### Web Search Global AI Processing

```
Main (Program.cs:97-132)
  ├─> Collect all task results (search section + all pages) (Line 118)
  └─> AiInstructionProcessor.ApplyAllInstructions (Line 122)
        └─> Sequential processing of instruction list
              └─> AI Tool Execution for each instruction
```

### Web Get (Same as Web Search, minus search results section)

```
Main (Program.cs:97-132)
  └─> HandleWebGetCommand (Program.cs:327-364)
        └─> For each URL:
              └─> GetCheckSaveWebPageContentAsync (Program.cs:636-656)
                    └─> [Same as Web Search per-page processing]
```

---

## 9. Template Variable Expansion

### Location: `src/cycodmd/Program.cs`

#### Variable Substitution (Lines 283-291)

```csharp
Line 283:        var pageInstructionsList = command.PageInstructionsList
Line 284:            .Select(x => Tuple.Create(
Line 285:                x.Item1
Line 286:                    .Replace("{searchTerms}", query)
Line 287:                    .Replace("{query}", query)
Line 288:                    .Replace("{terms}", query)
Line 289:                    .Replace("{q}", query),
Line 290:                x.Item2.ToLowerInvariant()))
Line 291:            .ToList();
```

**Evidence**: 
- Four template variables: `{searchTerms}`, `{query}`, `{terms}`, `{q}`
- **All replaced with the same value**: the search query
- Replacement happens **before** instructions are passed to AI processing
- URL criteria converted to lowercase for case-insensitive matching

**Example**:
```bash
cycodmd web search "async await" --page-instructions "Rate relevance to '{searchTerms}'"
```

After replacement:
```
Instruction: "Rate relevance to 'async await'"
```

---

## 10. Key Differences from File Search

### Pattern Matching
- **File Search**: Extension matching (`"cs"` matches `*.cs`)
- **Web Search**: URL substring matching (`"github"` matches `*github*` in URL)

### Automatic Behavior
- **File Search**: Always has content (files exist on disk)
- **Web Search**: May only have URLs; instructions trigger `--get --strip` automatically

### Template Variables
- **File Search**: No template variable support
- **Web Search**: Supports `{searchTerms}`, `{query}`, `{terms}`, `{q}`

### Search Results Section
- **File Search**: No search results section (just file content)
- **Web Search**: Includes search results section (list of URLs) before page content

### Criteria Case Conversion
- **File Search**: No case conversion (Windows is case-insensitive, Linux is case-sensitive)
- **Web Search**: URL criteria converted to lowercase for consistent matching (Line 290)

---

## 11. AI Tool Integration

Web Search and Web Get use the **same** `AiInstructionProcessor` as File Search. See file search proof document for complete details on:
- AI tool selection (`cycod` vs `ai`)
- Instruction chaining (sequential processing)
- Error handling and retries
- Chat history persistence
- Configuration integration

**Reference**: `src/common/AiInstructionProcessor.cs` (Lines 1-228)

---

## Conclusion

This proof document demonstrates that AI Processing (Layer 8) in cycodmd web search is implemented through:

1. **Command-line parsing** that captures page-specific and global instructions
2. **URL pattern-based filtering** for targeted AI processing
3. **Template variable expansion** for dynamic instructions based on search query
4. **Automatic content fetching** when instructions are present
5. **Case-insensitive substring matching** for URL patterns
6. **Sequential instruction chaining** for multi-stage transformations
7. **Integration with the same AI tools** as file search (`cycod` or `ai`)
8. **Shared implementation** between Web Search and Web Get commands

All claims in the layer documentation are supported by direct source code references with line numbers.
