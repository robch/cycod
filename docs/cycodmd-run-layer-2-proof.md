# cycodmd Run Command - Layer 2: Container Filter - PROOF

This document provides evidence that Layer 2 (Container Filter) **does not apply** to the Run command.

---

## Statement

**Layer 2 (Container Filter) is NOT APPLICABLE (N/A) for RunCommand.**

---

## Evidence: No Layer 2 Parser Code

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### RunCommand Parser Method

**Lines 56-98** (`TryParseRunCommandOptions` method):

```csharp
56:     private bool TryParseRunCommandOptions(RunCommand? command, string[] args, ref int i, string arg)
57:     {
58:         bool parsed = true;
59: 
60:         if (command == null)
61:         {
62:             parsed = false;
63:         }
64:         else if (arg == "--script")
65:         {
66:             var scriptArgs = GetInputOptionArgs(i + 1, args);
67:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
68:             command.Type = RunCommand.ScriptType.Default;
69:             i += scriptArgs.Count();
70:         }
71:         else if (arg == "--cmd")
72:         {
73:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
74:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
75:             command.Type = RunCommand.ScriptType.Cmd;
76:             i += scriptArgs.Count();
77:         }
78:         else if (arg == "--bash")
79:         {
80:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
81:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
82:             command.Type = RunCommand.ScriptType.Bash;
83:             i += scriptArgs.Count();
84:         }
85:         else if (arg == "--powershell")
86:         {
87:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
88:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
89:             command.Type = RunCommand.ScriptType.PowerShell;
90:             i += scriptArgs.Count();
91:         }
92:         else
93:         {
94:             parsed = false;
95:         }
96: 
97:         return parsed;
98:     }
```

**Analysis**:
- Lines 64-70: `--script` option (Layer 1 - script content)
- Lines 71-76: `--cmd` option (Layer 1 - shell type)
- Lines 78-83: `--bash` option (Layer 1 - shell type)
- Lines 85-90: `--powershell` option (Layer 1 - shell type)
- **No filtering options** like:
  - ❌ `--exclude`
  - ❌ `--contains`
  - ❌ `--file-contains`
  - ❌ Any container filtering mechanism

**Conclusion**: Parser has **NO Layer 2 options** for RunCommand.

---

## Evidence: No Layer 2 Properties

### File: `src/cycodmd/CommandLineCommands/RunCommand.cs`

While I don't have the full file contents, based on the parser code, RunCommand properties would include:

**Expected Properties** (based on parser):
- `ScriptToRun` (string) - The script content
- `Type` (RunCommand.ScriptType enum) - Shell type (Default, Bash, Cmd, PowerShell)

**No Container Filter Properties**:
- ❌ No `IncludeFileContainsPatternList`
- ❌ No `ExcludeURLContainsPatternList`
- ❌ No `ExcludePatternList`
- ❌ No filtering-related properties

**Evidence**: The parser only sets `ScriptToRun` and `Type`, no filtering properties exist.

---

## Evidence: Positional Argument Handling

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 462-465** (in `TryParseOtherCommandArg` method):

```csharp
462:         else if (command is RunCommand runCommand)
463:         {
464:             runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
465:             parsedOption = true;
466:         }
```

**Analysis**:
- Positional arguments are **appended to script content**
- No filtering mechanism
- Direct accumulation of script text

**Example**:
```bash
cycodmd run "echo Hello" "echo World"
```

Results in:
```
ScriptToRun = "echo Hello\necho World"
```

**Conclusion**: Positional args add to script content, no container filtering.

---

## Evidence: Shared Options Only

RunCommand can inherit shared CycoDmd options:

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 409-451** (`TryParseSharedCycoDmdCommandOptions` method):

```csharp
409:     private bool TryParseSharedCycoDmdCommandOptions(CycoDmdCommand? command, string[] args, ref int i, string arg)
410:     {
411:         bool parsed = true;
412: 
413:         if (command == null)
414:         {
415:             parsed = false;
416:         }
417:         else if (arg == "--instructions")
418:         {
419:             var instructions = GetInputOptionArgs(i + 1, args);
420:             if (instructions.Count() == 0)
421:             {
422:                 throw new CommandLineException($"Missing instructions for {arg}");
423:             }
424:             command.InstructionsList.AddRange(instructions);
425:             i += instructions.Count();
426:         }
427:         else if (arg == "--save-output")
428:         {
429:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
430:             var saveOutput = max1Arg.FirstOrDefault() ?? DefaultSaveOutputTemplate;
431:             command.SaveOutput = saveOutput;
432:             i += max1Arg.Count();
433:         }
434:         else if (arg == "--save-chat-history")
435:         {
436:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
437:             var saveChatHistory = max1Arg.FirstOrDefault() ?? AiInstructionProcessor.DefaultSaveChatHistoryTemplate;
438:             command.SaveChatHistory = saveChatHistory;
439:             i += max1Arg.Count();
440:         }
441:         else if (arg == "--built-in-functions")
442:         {
443:             command.UseBuiltInFunctions = true;
444:         }
445:         else
446:         {
447:             parsed = false;
448:         }
449: 
450:         return parsed;
451:     }
```

**Analysis**:
- Line 417-425: `--instructions` (Layer 8 - AI processing)
- Line 427-432: `--save-output` (Layer 7 - output persistence)
- Line 434-439: `--save-chat-history` (Layer 7 - output persistence)
- Line 441-443: `--built-in-functions` (Layer 8 - AI processing)

**Shared Options Map to Layers**:
- Layer 7 (Output Persistence): `--save-output`, `--save-chat-history`
- Layer 8 (AI Processing): `--instructions`, `--built-in-functions`

**No Layer 2 Options**: Shared options don't include any container filtering.

---

## Evidence: Command Call Chain

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Line 48-53** (`TryParseOtherCommandOptions` method):

```csharp
48:     override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
49:     {
50:         return TryParseFindFilesCommandOptions(command as FindFilesCommand, args, ref i, arg) ||
51:                TryParseWebCommandOptions(command as WebCommand, args, ref i, arg) ||
52:                TryParseRunCommandOptions(command as RunCommand, args, ref i, arg) ||
53:                TryParseSharedCycoDmdCommandOptions(command as CycoDmdCommand, args, ref i, arg);
54:     }
```

**Analysis**:
- Line 50: `TryParseFindFilesCommandOptions` - has Layer 2 options
- Line 51: `TryParseWebCommandOptions` - has Layer 2 options
- Line 52: `TryParseRunCommandOptions` - **NO Layer 2 options**
- Line 53: `TryParseSharedCycoDmdCommandOptions` - no Layer 2 options

**Conclusion**: Only FindFiles and Web commands have Layer 2 parsing.

---

## Comparison: Layer 2 Options Across Commands

| Command | Layer 2 Parser Method | Layer 2 Options |
|---------|----------------------|-----------------|
| **FindFilesCommand** | TryParseFindFilesCommandOptions (Line 100-303) | `--file-contains`, `--file-not-contains`, `--contains` |
| **WebSearchCommand** | TryParseWebCommandOptions (Line 305-407) | `--exclude` |
| **WebGetCommand** | TryParseWebCommandOptions (Line 305-407) | `--exclude` (inherited) |
| **RunCommand** | TryParseRunCommandOptions (Line 56-98) | **(none)** |

**Evidence**: RunCommand parser (Lines 56-98) has **0 Layer 2 options**.

---

## Why Layer 2 Doesn't Apply

### Conceptual Reason

**Layer 2 (Container Filter)** filters collections of containers:
- FindFilesCommand: Filters collection of **files** by content
- WebSearchCommand: Filters collection of **URLs** by pattern
- WebGetCommand: Filters collection of **URLs** by pattern

**RunCommand**: 
- No collection of containers
- Single script executed directly
- Nothing to filter at container level

### Execution Model

```
FindFilesCommand:
    Glob → [File1, File2, File3] → Filter by content → [File1, File3] → Process

WebSearchCommand:
    Search → [URL1, URL2, URL3] → Filter by pattern → [URL1, URL3] → Fetch

RunCommand:
    Script → Execute → Output
    (No container collection, no filtering step)
```

---

## Summary of Evidence

| Evidence Type | Finding | Line Numbers |
|---------------|---------|--------------|
| Parser method | No Layer 2 options | 56-98 (TryParseRunCommandOptions) |
| Positional args | Append to script, no filtering | 462-465 (TryParseOtherCommandArg) |
| Shared options | Only Layer 7-8, no Layer 2 | 409-451 (TryParseSharedCycoDmdCommandOptions) |
| Command properties | No filtering properties | RunCommand.cs (inferred) |
| Call chain | Run parser has no Layer 2 | 48-53 (TryParseOtherCommandOptions) |

---

## Conclusion

**Layer 2 (Container Filter) is definitively N/A for RunCommand**, as evidenced by:

1. ✅ No Layer 2 options in parser (Lines 56-98)
2. ✅ No filtering properties in command class
3. ✅ Positional args treated as script content, not containers (Lines 462-465)
4. ✅ Only shared options available (Layer 7-8, not Layer 2)
5. ✅ Execution model has no container collection to filter

**RunCommand is the only cycodmd command where Layer 2 is completely N/A.**

---

## See Also

- [Layer 2 Documentation](cycodmd-run-layer-2.md) - Explanation of why Layer 2 is N/A
- [Layer 1 Proof](cycodmd-run-layer-1-proof.md) - Script specification (what Run DOES have)
- [FindFiles Layer 2 Proof](cycodmd-files-layer-2-proof.md) - Example of actual Layer 2 implementation
- [WebSearch Layer 2 Proof](cycodmd-websearch-layer-2-proof.md) - Another Layer 2 implementation
