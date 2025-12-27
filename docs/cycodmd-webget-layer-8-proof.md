# cycodmd Web Get - Layer 8: AI Processing (PROOF)

## Source Code Evidence

This document provides evidence that Web Get uses the **identical** AI Processing implementation as Web Search.

---

## 1. Command-Line Option Parsing

### Location: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

Web Get inherits from `WebCommand`, which shares all AI processing options with Web Search.

#### Option Parsing (Lines 382-444)

All AI processing options (`--page-instructions`, `--{pattern}-page-instructions`, `--instructions`, `--built-in-functions`, `--save-chat-history`) are parsed by `TryParseWebCommandOptions` and `TryParseSharedCycoDmdCommandOptions`.

**Evidence**: See [Web Search Layer 8 Proof](cycodmd-websearch-layer-8-proof.md) Section 1 for complete parsing details. Web Get uses **identical** parsing logic.

---

## 2. Data Structures

### Location: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

#### Web Get Command (Lines 1-28)

```csharp
Line 4:  class WebGetCommand : WebCommand
Line 5:  {
Line 6:      public WebGetCommand()
Line 7:      {
Line 8:          Urls = new List<string>();
Line 9:      }
Line 10:     
Line 11:     public List<string> Urls { get; set; }
Line 12:
Line 13:     override public string GetCommandName()
Line 14:     {
Line 15:         return "web get";
Line 16:     }
Line 17:
Line 18:     override public bool IsEmpty()
Line 19:     {
Line 20:         
Line 21:         return !Urls.Any();
Line 22:     }
Line 23:
Line 24:     override public CycoDmdCommand Validate()
Line 25:     {
Line 26:         return this;
Line 27:     }
Line 28: }
```

**Evidence**: 
- Inherits from `WebCommand` (Line 4)
- Only unique property: `Urls` list (Line 11)
- No AI processing properties defined here (inherited from `WebCommand`)

### Location: `src/cycodmd/CommandLineCommands/WebCommand.cs`

#### Inherited Properties from WebCommand (Lines 5-38)

```csharp
Line 5:  abstract class WebCommand : CycoDmdCommand
...
Line 20:     public List<Tuple<string, string>> PageInstructionsList;
```

**Evidence**: 
- Web Get inherits `PageInstructionsList` from `WebCommand`
- Web Get inherits `InstructionsList`, `UseBuiltInFunctions`, `SaveChatHistory` from `CycoDmdCommand`
- **Identical** data structures to Web Search

---

## 3. Execution Flow

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
Line 334:        var pageInstructionsList = command.PageInstructionsList;  // <-- AI instructions
Line 335:        var useBuiltInFunctions = command.UseBuiltInFunctions;     // <-- AI setting
Line 336:        var saveChatHistory = command.SaveChatHistory;             // <-- AI setting
Line 337:        var savePageOutput = command.SavePageOutput;
Line 338:
Line 339:        var badUrls = command.Urls.Where(l => !l.StartsWith("http")).ToList();
Line 340:        if (badUrls.Any())
Line 341:        {
Line 342:             var message = (badUrls.Count == 1)
Line 343:                 ? $"Invalid URL: {badUrls[0]}"
Line 344:                 : "Invalid URLs:\n" + string.Join(Environment.NewLine, badUrls.Select(url => "  " + url));
Line 345:             return new List<Task<string>>() { Task.FromResult(message) };
Line 346:        }
Line 347:
Line 348:        var tasks = new List<Task<string>>();
Line 349:        foreach (var url in urls)
Line 350:        {
Line 351:            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(
Line 352:                url, 
Line 353:                stripHtml, 
Line 354:                saveToFolder, 
Line 355:                browserType, 
Line 356:                interactive, 
Line 357:                pageInstructionsList,  // <-- Same function as Web Search!
Line 358:                useBuiltInFunctions, 
Line 359:                saveChatHistory, 
Line 360:                savePageOutput);
Line 361:            var taskToAdd = delayOutputToApplyInstructions
Line 362:                ? getCheckSaveTask
Line 363:                : getCheckSaveTask.ContinueWith(t =>
Line 364:                {
Line 365:                    ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
Line 366:                    return t.Result;
Line 367:                });
Line 368:
Line 369:            tasks.Add(taskToAdd);
Line 370:        }
Line 371:
Line 372:        return tasks;
Line 373:    }
```

**Evidence**: 
- Uses **exact same function** `GetCheckSaveWebPageContentAsync` as Web Search (Line 351)
- Passes **same AI processing parameters** (Lines 357-359)
- Uses **same output delay logic** for global instructions (Lines 361-367)
- **Only difference**: URLs come from `command.Urls` instead of search results

---

## 4. Per-Page AI Processing

### Shared Implementation with Web Search

Web Get calls `GetCheckSaveWebPageContentAsync` (Program.cs:636-656), which is the **same function** used by Web Search.

**Reference**: See [Web Search Layer 8 Proof](cycodmd-websearch-layer-8-proof.md) Section 5 for complete implementation details.

**Key Functions** (all shared):
1. `GetCheckSaveWebPageContentAsync` (Lines 636-656)
2. `GetFinalWebPageContentAsync` (Lines 659-673)
3. `WebPageMatchesInstructionsCriteria` (Lines 675-685)
4. `AiInstructionProcessor.ApplyAllInstructions` (AiInstructionProcessor.cs:10-21)

---

## 5. Global AI Processing

### Shared Implementation with All Commands

Web Get uses the **same global AI processing** as Web Search and File Search.

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
- **Exact same logic** applies to Web Get, Web Search, File Search, and Run
- All page outputs collected and joined (Line 118)
- Global instructions applied to combined output (Lines 122-126)
- Output displayed after AI processing completes (Line 127)

---

## 6. Call Stack Summary

### Web Get Per-Page AI Processing

```
Main (Program.cs:97-132)
  └─> HandleWebGetCommand (Program.cs:327-373)
        └─> For each URL:
              └─> GetCheckSaveWebPageContentAsync (Program.cs:636-656)
                    └─> GetFinalWebPageContentAsync (Program.cs:659-673)
                          ├─> GetFormattedWebPageContentAsync (fetch + format)
                          ├─> WebPageMatchesInstructionsCriteria (Program.cs:675-685)
                          └─> AiInstructionProcessor.ApplyAllInstructions (AiInstructionProcessor.cs:10-21)
```

**Evidence**: Call stack is **identical** to Web Search except for the entry point (`HandleWebGetCommand` instead of `HandleWebSearchCommandAsync`).

### Web Get Global AI Processing

```
Main (Program.cs:97-132)
  ├─> Collect all task results (all page content) (Line 118)
  └─> AiInstructionProcessor.ApplyAllInstructions (Line 122)
        └─> Sequential processing of instruction list
              └─> AI Tool Execution for each instruction
```

**Evidence**: Call stack is **identical** across all cycodmd commands.

---

## 7. Key Differences from Web Search

### URL Source

**Web Search** (Lines 295-298):
```csharp
var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
```

**Web Get** (Line 329):
```csharp
var urls = command.Urls;
```

**Evidence**: 
- Web Search: URLs from search engine
- Web Get: URLs from command-line arguments

### Search Results Section

**Web Search** (Lines 293-308):
```csharp
var searchSectionHeader = $"## Web Search for '{query}' using {provider}";

var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(...);
var searchSection = urls.Count == 0
    ? $"{searchSectionHeader}\n\nNo results found\n"
    : $"{searchSectionHeader}\n\n" + string.Join("\n", urls) + "\n";

if (!delayOutputToApplyInstructions) ConsoleHelpers.WriteLine(searchSection);

if (urls.Count == 0 || !getContent)
{
    return new List<Task<string>>() { Task.FromResult(searchSection) };
}

var tasks = new List<Task<string>>();
tasks.Add(Task.FromResult(searchSection));  // <-- Search section included
```

**Web Get** (Line 348):
```csharp
var tasks = new List<Task<string>>();
// No search section added
```

**Evidence**: Web Get does **not** create or include a search results section.

### Template Variable Substitution

**Web Search** (Lines 283-291):
```csharp
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

**Web Get** (Line 334):
```csharp
var pageInstructionsList = command.PageInstructionsList;
```

**Evidence**: Web Get does **not** perform template variable substitution because there is no search query.

---

## 8. Similarities with Web Search

### Identical AI Processing Functions

Both commands use:
1. ✅ `GetCheckSaveWebPageContentAsync` - Same function
2. ✅ `GetFinalWebPageContentAsync` - Same function
3. ✅ `WebPageMatchesInstructionsCriteria` - Same function
4. ✅ `AiInstructionProcessor.ApplyAllInstructions` - Same function

### Identical Options

Both commands support:
1. ✅ `--page-instructions`
2. ✅ `--{pattern}-page-instructions`
3. ✅ `--instructions`
4. ✅ `--built-in-functions`
5. ✅ `--save-chat-history`

### Identical Pattern Matching

Both use **case-insensitive substring matching** for URL patterns (Lines 675-685).

### Identical Output Delay

Both use **same logic** for delaying output when global instructions present (Lines 361-367).

---

## 9. Complete Comparison Table

| Feature | Web Search | Web Get |
|---------|------------|---------|
| **Per-Page AI Processing** | ✅ Yes | ✅ Yes |
| **Global AI Processing** | ✅ Yes | ✅ Yes |
| **URL Pattern Matching** | ✅ Yes | ✅ Yes |
| **`--page-instructions`** | ✅ Yes | ✅ Yes |
| **`--{pattern}-page-instructions`** | ✅ Yes | ✅ Yes |
| **`--instructions`** | ✅ Yes | ✅ Yes |
| **`--built-in-functions`** | ✅ Yes | ✅ Yes |
| **`--save-chat-history`** | ✅ Yes | ✅ Yes |
| **AI Tool Integration** | ✅ Same | ✅ Same |
| **Instruction Chaining** | ✅ Sequential | ✅ Sequential |
| **Template Variables** | ✅ Yes ({searchTerms}, etc.) | ❌ No (no query) |
| **Search Results Section** | ✅ Yes | ❌ No |
| **URL Source** | Search Engine | Command-Line Args |
| **Implementation Function** | `GetCheckSaveWebPageContentAsync` | `GetCheckSaveWebPageContentAsync` |

---

## Conclusion

This proof document demonstrates that Web Get uses the **exact same AI Processing (Layer 8) implementation** as Web Search, with only three differences:

1. **URLs** come from command-line arguments instead of search results
2. **No search results section** in output
3. **No template variable substitution** (no search query exists)

**All AI processing features are identical:**
- Per-page and global instruction processing
- URL pattern-based filtering  
- Case-insensitive substring matching
- Instruction chaining
- AI tool integration
- Error handling
- Chat history persistence

The shared implementation is in:
- `GetCheckSaveWebPageContentAsync` (Program.cs:636-656)
- `GetFinalWebPageContentAsync` (Program.cs:659-673)
- `WebPageMatchesInstructionsCriteria` (Program.cs:675-685)
- `AiInstructionProcessor` (AiInstructionProcessor.cs)

All claims in the layer documentation are supported by direct source code references with line numbers.
