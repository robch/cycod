# cycodmd File Search - Layer 8: AI Processing (PROOF)

## Source Code Evidence

This document provides detailed evidence from the source code showing how AI Processing (Layer 8) is implemented for the cycodmd file search command.

---

## 1. Command-Line Option Parsing

### Location: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### `--instructions` Parsing (Lines 418-426)

```csharp
Line 418:     else if (arg.StartsWith("--") && arg.EndsWith("file-instructions"))
Line 419:     {
Line 420:         var instructions = GetInputOptionArgs(i + 1, args);
Line 421:         if (instructions.Count() == 0)
Line 422:         {
Line 423:             throw new CommandLineException($"Missing instructions for {arg}");
Line 424:         }
Line 425:         command.InstructionsList.AddRange(instructions);
Line 426:         i += instructions.Count();
```

**Evidence**: The `--instructions` option collects all following arguments (until next option) and adds them to `InstructionsList`.

#### `--file-instructions` Parsing (Lines 263-281)

```csharp
Line 263:     else if (arg.StartsWith("--") && arg.EndsWith("file-instructions"))
Line 264:     {
Line 265:         var instructions = GetInputOptionArgs(i + 1, args);
Line 266:         if (instructions.Count() == 0)
Line 267:         {
Line 268:             throw new CommandLineException($"Missing instructions for {arg}");
Line 269:         }
Line 270:         var fileNameCriteria = arg != "--file-instructions"
Line 271:             ? arg.Substring(2, arg.Length - 20)
Line 272:             : string.Empty;
Line 273:         var withCriteria = instructions.Select(x => Tuple.Create(x, fileNameCriteria));
Line 274:         command.FileInstructionsList.AddRange(withCriteria);
Line 275:         i += instructions.Count();
Line 276:     }
```

**Evidence**: 
- Extension-specific instructions extract the extension from the option name (e.g., `--cs-file-instructions` → `"cs"`)
- Stores instructions as `Tuple<string, string>` where Item1 = instruction, Item2 = extension criteria
- Empty criteria means apply to all files

#### `--built-in-functions` Parsing (Lines 441-444)

```csharp
Line 441:     else if (arg == "--built-in-functions")
Line 442:     {
Line 443:         command.UseBuiltInFunctions = true;
Line 444:     }
```

**Evidence**: Boolean flag, sets `UseBuiltInFunctions` property.

#### `--save-chat-history` Parsing (Lines 429-439)

```csharp
Line 429:     else if (arg == "--save-chat-history")
Line 430:     {
Line 431:         var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
Line 432:         var saveChatHistory = max1Arg.FirstOrDefault() ?? AiInstructionProcessor.DefaultSaveChatHistoryTemplate;
Line 433:         command.SaveChatHistory = saveChatHistory;
Line 434:         i += max1Arg.Count();
Line 435:     }
```

**Evidence**: 
- Optional value (takes next argument if present)
- Defaults to `AiInstructionProcessor.DefaultSaveChatHistoryTemplate` if no value provided
- Template: `"chat-history-{time}.jsonl"` (from `AiInstructionProcessor.cs:8`)

---

## 2. Data Structures

### Location: `src/cycodmd/CommandLine/CycoDmdCommand.cs`

#### Base Command Properties (Lines 1-21)

```csharp
Line 1:  abstract class CycoDmdCommand : Command
Line 2:  {
Line 3:      public CycoDmdCommand()
Line 4:      {
Line 5:          InstructionsList = new List<string>();
Line 6:          UseBuiltInFunctions = false;
Line 7:          SaveChatHistory = string.Empty;
Line 8:          SaveOutput = string.Empty;
Line 9:      }
...
Line 16:     public List<string> InstructionsList;
Line 17:     public bool UseBuiltInFunctions;
Line 18:     public string SaveChatHistory;
Line 19: 
Line 20:     public string SaveOutput;
Line 21: }
```

**Evidence**: All cycodmd commands inherit these AI processing properties.

### Location: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

#### File-Specific Instructions Storage (Lines 108)

```csharp
Line 108:    public List<Tuple<string, string>> FileInstructionsList;
```

**Evidence**: 
- Stores file-specific instructions
- `Tuple.Item1` = instruction text
- `Tuple.Item2` = extension criteria (empty string for all files)

---

## 3. Execution Flow

### Location: `src/cycodmd/Program.cs`

#### Detecting AI Instructions (Lines 99-100)

```csharp
Line 99:     var cycoDmdCommand = command as CycoDmdCommand;
Line 100:    bool delayOutputToApplyInstructions = cycoDmdCommand?.InstructionsList.Any() ?? false;
```

**Evidence**: 
- Checks if general instructions are present
- If true, output is delayed until AI processing completes

#### Throttling Decision (Lines 216-217)

```csharp
Line 216:    // Decide whether to use throttling based on whether AI instructions are present
Line 217:    var needsThrottling = findFilesCommand.FileInstructionsList.Any();
```

**Evidence**: 
- Presence of file-level instructions triggers throttled execution
- Prevents overwhelming AI service with parallel requests

#### Per-File AI Processing Call (Lines 227-246)

```csharp
Line 227:    Func<string, Task<string>> getCheckSaveFileContent = async file =>
Line 228:    {
Line 229:        var onlyOneFile = files.Count == 1 && commandLineOptions.Commands.Count == 1;
Line 230:        var skipMarkdownWrapping = onlyOneFile && FileConverters.CanConvert(file);
Line 231:        var wrapInMarkdown = !skipMarkdownWrapping;
Line 232:
Line 233:        return await GetCheckSaveFileContentAsync(
Line 234:            file,
Line 235:            wrapInMarkdown,
Line 236:            findFilesCommand.IncludeLineContainsPatternList,
Line 237:            findFilesCommand.IncludeLineCountBefore,
Line 238:            findFilesCommand.IncludeLineCountAfter,
Line 239:            findFilesCommand.IncludeLineNumbers,
Line 240:            findFilesCommand.RemoveAllLineContainsPatternList,
Line 241:            actualHighlightMatches,
Line 242:            findFilesCommand.FileInstructionsList,  // <-- AI instructions passed here
Line 243:            findFilesCommand.UseBuiltInFunctions,
Line 244:            findFilesCommand.SaveChatHistory,
Line 245:            findFilesCommand.SaveFileOutput);
Line 246:    };
```

**Evidence**: `FileInstructionsList` is passed to per-file processing function.

#### Global AI Processing (Lines 114-131)

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
- All file outputs are collected and joined
- Global instructions applied to combined output
- Output displayed after AI processing completes

---

## 4. Per-File AI Processing Implementation

### Location: `src/cycodmd/Program.cs`

#### Main Per-File Function Signature (Line 490)

```csharp
Line 490:    private static string GetCheckSaveFileContent(
                 string fileName, 
                 bool wrapInMarkdown, 
                 List<Regex> includeLineContainsPatternList, 
                 int includeLineCountBefore, 
                 int includeLineCountAfter, 
                 bool includeLineNumbers, 
                 List<Regex> removeAllLineContainsPatternList, 
                 bool highlightMatches, 
                 List<Tuple<string, string>> fileInstructionsList,  // <-- Instructions parameter
                 bool useBuiltInFunctions, 
                 string? saveChatHistory, 
                 string? saveFileOutput)
```

#### File Content Formatting and AI Call (Lines 516-526)

```csharp
Line 516:    var finalContent = GetFinalFileContent(
Line 517:        fileName,
Line 518:        wrapInMarkdown,
Line 519:        includeLineContainsPatternList,
Line 520:        includeLineCountBefore,
Line 521:        includeLineCountAfter,
Line 522:        includeLineNumbers,
Line 523:        removeAllLineContainsPatternList,
Line 524:        highlightMatches,
Line 525:        fileInstructionsList,  // <-- Instructions passed through
Line 526:        useBuiltInFunctions,
Line 527:        saveChatHistory);
```

#### Instruction Filtering by Extension (Lines 542-561)

```csharp
Line 542:    private static string GetFinalFileContent(
                 string fileName, 
                 bool wrapInMarkdown, 
                 List<Regex> includeLineContainsPatternList, 
                 int includeLineCountBefore, 
                 int includeLineCountAfter, 
                 bool includeLineNumbers, 
                 List<Regex> removeAllLineContainsPatternList, 
                 bool highlightMatches, 
                 List<Tuple<string, string>> fileInstructionsList, 
                 bool useBuiltInFunctions, 
                 string? saveChatHistory)
Line 543:    {
Line 544:        var formatted = GetFormattedFileContent(
Line 545:            fileName,
Line 546:            wrapInMarkdown,
Line 547:            includeLineContainsPatternList,
Line 548:            includeLineCountBefore,
Line 549:            includeLineCountAfter,
Line 550:            includeLineNumbers,
Line 551:            removeAllLineContainsPatternList,
Line 552:            highlightMatches);
Line 553:
Line 554:        var instructionsForThisFile = fileInstructionsList
Line 555:            .Where(x => FileNameMatchesInstructionsCriteria(fileName, x.Item2))
Line 556:            .Select(x => x.Item1)
Line 557:            .ToList();
Line 558:
Line 559:        var afterInstructions = instructionsForThisFile.Any()
Line 560:            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisFile, formatted, useBuiltInFunctions, saveChatHistory)
Line 561:            : formatted;
Line 562:
Line 563:        return afterInstructions;
Line 564:    }
```

**Evidence**: 
1. Content is formatted first (Layer 6 output)
2. Instructions are filtered by file extension criteria
3. If matching instructions exist, AI processing is applied
4. Otherwise, original formatted content is returned

#### Extension Matching Logic (Lines 566-578)

```csharp
Line 566:    private static bool FileNameMatchesInstructionsCriteria(string fileName, string criteria)
Line 567:    {
Line 568:        if (string.IsNullOrEmpty(criteria))
Line 569:        {
Line 570:            return true;  // Empty criteria matches all files
Line 571:        }
Line 572:
Line 573:        var extension = Path.GetExtension(fileName).TrimStart('.');
Line 574:        return criteria.Equals(extension, StringComparison.OrdinalIgnoreCase);
Line 575:    }
```

**Evidence**: 
- Empty criteria (from `--file-instructions`) matches ALL files
- Non-empty criteria (from `--cs-file-instructions`) matches specific extensions
- Case-insensitive matching

---

## 5. AI Instruction Processor Core

### Location: `src/common/AiInstructionProcessor.cs`

#### Apply All Instructions (Sequential Processing) (Lines 10-21)

```csharp
Line 10:     public static string ApplyAllInstructions(List<string> instructionsList, string content, bool useBuiltInFunctions, string? saveChatHistory, int retries = 1)
Line 11:     {
Line 12:         try
Line 13:         {
Line 14:             ConsoleHelpers.DisplayStatus("Applying instructions ...");
Line 15:             return instructionsList.Aggregate(content, (current, instruction) => ApplyInstructions(instruction, current, useBuiltInFunctions, saveChatHistory, retries));
Line 16:         }
Line 17:         finally
Line 18:         {
Line 19:             ConsoleHelpers.DisplayStatusErase();
Line 20:         }
Line 21:     }
```

**Evidence**: 
- Uses `Aggregate` to apply instructions **sequentially**
- Each instruction transforms the output of the previous one
- Status message displayed during processing

#### Single Instruction Application with Retry (Lines 23-39)

```csharp
Line 23:     public static string ApplyInstructions(string instructions, string content, bool useBuiltInFunctions, string? saveChatHistory, int retries = 1)
Line 24:     {
Line 25:         while (true)
Line 26:         {
Line 27:             ApplyInstructions(instructions, content, useBuiltInFunctions, saveChatHistory, out var returnCode, out var stdOut, out var stdErr, out var exception);
Line 28:
Line 29:             var retryable = retries-- > 0;
Line 30:             var tryAgain = retryable && (returnCode != 0 || exception != null);
Line 31:             if (tryAgain) continue;
Line 32:
Line 33:             return exception != null
Line 34:                 ? $"{stdOut}\n\n## Error Applying Instructions\n\nEXIT CODE: {returnCode}\n\nERROR: {exception.Message}\n\nSTDERR: {stdErr}"
Line 35:                 : returnCode != 0
Line 36:                     ? $"{stdOut}\n\n## Error Applying Instructions\n\nEXIT CODE: {returnCode}\n\nSTDERR: {stdErr}"
Line 37:                     : stdOut;
Line 38:         }
Line 39:     }
```

**Evidence**: 
- Implements retry logic (default: 1 retry)
- Returns formatted error messages if processing fails
- Includes exit code, stderr, and exception details

#### AI Tool Invocation (Lines 41-110)

```csharp
Line 41:     private static void ApplyInstructions(string instructions, string content, bool useBuiltInFunctions, string? saveChatHistory, out int returnCode, out string stdOut, out string stdErr, out Exception? exception)
Line 42:     {
...
Line 48:         var userPromptFileName = Path.GetTempFileName();
Line 49:         var systemPromptFileName = Path.GetTempFileName();
Line 50:         var instructionsFileName = Path.GetTempFileName();
Line 51:         var contentFileName = Path.GetTempFileName();
Line 52:         try
Line 53:         {
Line 54:             var backticks = new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content) + 3);
Line 55:             File.WriteAllText(systemPromptFileName, GetSystemPrompt());
Line 56:             File.WriteAllText(userPromptFileName, GetUserPrompt(backticks, contentFileName, instructionsFileName));
Line 57:             File.WriteAllText(instructionsFileName, instructions);
Line 58:             File.WriteAllText(contentFileName, content);
...
Line 64:             var useCycoD = UseCycoD();
Line 65:             var arguments = useCycoD
Line 66:                 ? $"--input \"@{userPromptFileName}\" --system-prompt \"@{systemPromptFileName}\" --quiet --interactive false --no-templates"
Line 67:                 : $"chat --user \"@{userPromptFileName}\" --system \"@{systemPromptFileName}\" --quiet true";
Line 68:
Line 69:             if (useCycoD) arguments += GetConfiguredAIProviders();
Line 70:
Line 71:             if (useBuiltInFunctions && !useCycoD) arguments += " --built-in-functions";
Line 72:
Line 73:             if (!string.IsNullOrEmpty(saveChatHistory))
Line 74:             {
Line 75:                 var fileName = FileHelpers.GetFileNameFromTemplate(DefaultSaveChatHistoryTemplate, saveChatHistory);
Line 76:                 arguments += $" --output-chat-history \"{fileName}\"";
Line 77:             }
...
Line 88:             var processResult = new RunnableProcessBuilder()
Line 89:                 .WithFileName(useCycoD ? "cycod" : "ai")
Line 90:                 .WithArguments(arguments)
Line 91:                 .WithTimeout(300000) // 5 minute timeout
Line 92:                 .Run();
Line 93:             
Line 94:             stdOut = processResult.StandardOutput;
Line 95:             stdErr = processResult.StandardError;
Line 96:             returnCode = processResult.ExitCode;
...
Line 105:             if (File.Exists(userPromptFileName)) File.Delete(userPromptFileName);
Line 106:             if (File.Exists(systemPromptFileName)) File.Delete(systemPromptFileName);
Line 107:             if (File.Exists(instructionsFileName)) File.Delete(instructionsFileName);
Line 108:             if (File.Exists(contentFileName)) File.Delete(contentFileName);
Line 109:         }
Line 110:     }
```

**Evidence**: 
1. Creates temporary files for prompts, instructions, and content
2. Determines which AI tool to use (`cycod` or `ai`)
3. Constructs appropriate command-line arguments
4. Passes `--built-in-functions` if enabled
5. Adds `--output-chat-history` if specified
6. Executes AI tool with 5-minute timeout
7. Captures stdout, stderr, and exit code
8. Cleans up temporary files

#### AI Tool Selection (Lines 159-175)

```csharp
Line 159:    private static bool UseCycoD()
Line 160:    {
Line 161:        var useCycoDByConfig = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppUseCycoDForInstructions);
Line 162:        var useCycoD = useCycoDByConfig?.Value?.ToString()?.ToLowerInvariant() == "true";
Line 163:
Line 164:        if (!useCycoD)
Line 165:        {
Line 166:            ConsoleHelpers.WriteDebugLine("Using 'ai' CLI for instructions (legacy mode). Set 'App.UseCycoDForInstructions' to 'true' to use 'cycod' instead.");
Line 167:        }
Line 168:
Line 169:        return useCycoD;
Line 170:    }
```

**Evidence**: 
- Checks configuration setting `App.UseCycoDForInstructions`
- Defaults to `ai` tool if not configured
- Provides debug logging for tool selection

---

## 6. Call Stack Summary

### Per-File AI Processing Call Stack

```
Main (Program.cs:97-132)
  └─> HandleFindFileCommand (Program.cs:163-266)
        └─> GetCheckSaveFileContentAsync (Program.cs:472-487)
              └─> GetCheckSaveFileContent (Program.cs:490-538)
                    └─> GetFinalFileContent (Program.cs:542-564)
                          ├─> GetFormattedFileContent (Layers 1-6 output)
                          ├─> FileNameMatchesInstructionsCriteria (Program.cs:566-575)
                          └─> AiInstructionProcessor.ApplyAllInstructions (AiInstructionProcessor.cs:10-21)
                                └─> AiInstructionProcessor.ApplyInstructions (AiInstructionProcessor.cs:23-39)
                                      └─> AI Tool Execution (AiInstructionProcessor.cs:41-110)
```

### Global AI Processing Call Stack

```
Main (Program.cs:97-132)
  ├─> Collect all file task results (Line 118)
  └─> AiInstructionProcessor.ApplyAllInstructions (Line 122)
        └─> Sequential processing of instruction list (Line 15)
              └─> AI Tool Execution for each instruction
```

---

## 7. Key Observations

### Instruction Processing is Sequential
Multiple instructions are applied **one after another**, not in parallel. Each instruction's output becomes the input for the next instruction.

### Two Processing Levels Don't Interact
- Per-file instructions run **first** (in parallel across files, throttled)
- Global instructions run **second** (on combined output)
- They never mix or interfere with each other

### Extension Matching is Flexible
- Empty criteria (`--file-instructions`) matches **all** files
- Non-empty criteria (`--cs-file-instructions`) matches specific extension
- Case-insensitive matching
- Simple exact-match algorithm (no regex or wildcards)

### AI Tool Selection is Configurable
- Can use either `cycod` (modern) or `ai` (legacy)
- Selected via `App.UseCycoDForInstructions` config setting
- Falls back to `ai` if not configured

### Error Handling is Comprehensive
- Captures exit code, stdout, stderr
- Includes exception details in output
- Retry mechanism (default: 1 retry)
- Formats errors clearly in output

### Chat History is Templatized
- Default template: `chat-history-{time}.jsonl`
- Supports custom filenames
- Template expansion happens in `FileHelpers.GetFileNameFromTemplate`

---

## 8. Configuration Integration

### Location: `src/common/AiInstructionProcessor.cs`

#### AI Provider Configuration (Lines 112-150)

```csharp
Line 112:    private static string GetConfiguredAIProviders()
Line 113:    {
Line 114:        var returnArguments = "";
Line 115:
Line 116:        // Check environment variable first (set by parent cycod process command-line flags)
Line 117:        var envProvider = Environment.GetEnvironmentVariable("CYCOD_AI_PROVIDER");
Line 118:
Line 119:        // Fall back to ConfigStore (for persistent user preferences)
Line 120:        var configProvider = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppPreferredProvider);
Line 121:
Line 122:        // Environment variable takes precedence (command-line session), then config (persistent)
Line 123:        var provider = envProvider ?? configProvider?.Value?.ToString();
Line 124:
Line 125:        if (!string.IsNullOrEmpty(provider))
Line 126:        {
Line 127:            // Get the appropriate flag for the provider
Line 128:            var providerFlag = provider.ToLowerInvariant() switch
Line 129:            {
Line 130:                "test" => "--use-test",
Line 131:                "openai" => "--use-openai",
Line 132:                "anthropic" => "--use-anthropic",
Line 133:                "azure-openai" => "--use-azure-openai",
Line 134:                "google-gemini" => "--use-gemini",
Line 135:                "grok" => "--use-grok",
Line 136:                "aws-bedrock" => "--use-bedrock",
Line 137:                "copilot" or "copilot-github" => "--use-copilot",
Line 138:                _ => null
Line 139:            };
Line 140:
Line 141:            if (providerFlag != null)
Line 142:            {
Line 143:                return Arguments += $" {providerFlag}";
Line 144:            }
Line 145:        }
Line 146:        else
Line 147:        {
Line 148:            ConsoleHelpers.WriteDebugLine("No AI Provider specified in environment or ConfigStore.");
Line 149:        }
...
```

**Evidence**: 
- AI provider selection respects configuration hierarchy
- Environment variable `CYCOD_AI_PROVIDER` takes precedence
- Falls back to `App.PreferredProvider` config setting
- Maps provider names to appropriate command-line flags

---

## Conclusion

This proof document demonstrates that AI Processing (Layer 8) in cycodmd file search is implemented through:

1. **Command-line parsing** that captures instructions at two levels
2. **Extension-based filtering** for targeted AI processing
3. **Sequential instruction chaining** for multi-stage transformations
4. **Parallel execution with throttling** for per-file processing
5. **Integration with external AI tools** (`cycod` or `ai`)
6. **Comprehensive error handling** with retries
7. **Configuration-driven behavior** for tool and provider selection

All claims in the layer documentation are supported by direct source code references with line numbers.
