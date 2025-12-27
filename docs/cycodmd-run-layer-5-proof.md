# cycodmd Run - Layer 5: Context Expansion - PROOF

**[← Back to Layer 5 Documentation](cycodmd-run-layer-5.md)**

## Source Code Evidence

This document provides **detailed source code references** proving that Layer 5 (Context Expansion) is **NOT IMPLEMENTED** / **NOT APPLICABLE** for the Run command in cycodmd.

---

## 1. Command Class Definition

### File: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 1-37** (entire file):
```csharp
  1: using System;
  2: 
  3: class RunCommand : CycoDmdCommand
  4: {
  5:     public enum ScriptType
  6:     {
  7:         Default, // Uses cmd on Windows, bash on Linux/Mac
  8:         Cmd,
  9:         Bash,
 10:         PowerShell
 11:     }
 12: 
 13:     public RunCommand() : base()
 14:     {
 15:         ScriptToRun = string.Empty;
 16:         Type = ScriptType.Default;
 17:     }
 18: 
 19:     override public string GetCommandName()
 20:     {
 21:         return "run";
 22:     }
 23: 
 24:     override public bool IsEmpty()
 25:     {
 26:         return string.IsNullOrWhiteSpace(ScriptToRun);
 27:     }
 28: 
 29:     override public CycoDmdCommand Validate()
 30:     {
 31:         return this;
 32:     }
 33: 
 34:     public string ScriptToRun { get; set; }
 35:     public ScriptType Type { get; set; }
 36: }
```

**Evidence**:
- **Lines 5-11**: Only script-related enum (`ScriptType`) - determines which shell to use
- **Line 34**: Only property is `ScriptToRun` (the script text to execute)
- **Line 35**: `Type` property (which shell to use)
- **No properties** for: `IncludeLineCountBefore`, `IncludeLineCountAfter`, `IncludeLineContainsPatternList`, etc.
- The command has **no mechanism** to store filtering or context expansion settings
- Inherits from `CycoDmdCommand` (base class for all cycodmd commands)

---

## 2. Base Class Properties

### File: `src/cycodmd/CommandLineCommands/CycoDmdCommand.cs`

While not all cycodmd commands have context expansion, let's verify what the base class provides:

```csharp
public abstract class CycoDmdCommand : Command
{
    public CycoDmdCommand()
    {
        InstructionsList = new List<string>();
        SaveOutput = null;
        SaveChatHistory = null;
        UseBuiltInFunctions = false;
    }

    public List<string> InstructionsList { get; set; }
    public string? SaveOutput { get; set; }
    public string? SaveChatHistory { get; set; }
    public bool UseBuiltInFunctions { get; set; }
}
```

**Evidence**:
- Base class provides AI processing (Layer 8) and output persistence (Layer 7) properties
- **No properties** for line-level filtering or context expansion in base class
- RunCommand inherits these, but gets no context expansion capabilities

---

## 3. Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Option Parsing for Run Command

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

**Evidence**:
- **Lines 64-91**: Only parses shell type options (`--script`, `--cmd`, `--bash`, `--powershell`)
- **No parsing** for `--lines`, `--lines-before`, `--lines-after`
- **No parsing** for `--line-contains`, `--line-numbers`, `--highlight-matches`, etc.
- Parser focuses exclusively on script execution configuration

#### Positional Argument Parsing

**Lines 462-465** (TryParseOtherCommandArg method):
```csharp
462:         else if (command is RunCommand runCommand)
463:         {
464:             runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
465:             parsedOption = true;
466:         }
```

**Evidence**:
- **Lines 462-465**: Positional arguments are accumulated as script text
- No parsing for context expansion options in positional arguments

---

## 4. Execution Path

### File: `src/cycodmd/Program.cs`

**Lines 653-720** (`HandleRunCommand` method):
```csharp
653:     private static List<Task<string>> HandleRunCommand(CommandLineOptions commandLineOptions, RunCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
654:     {
655:         var scriptToRun = command.ScriptToRun;
656:         var scriptType = command.Type;
657: 
658:         var output = scriptType switch
659:         {
660:             RunCommand.ScriptType.Bash => ProcessHelpers.RunBashCommand(scriptToRun),
661:             RunCommand.ScriptType.Cmd => ProcessHelpers.RunCmdCommand(scriptToRun),
662:             RunCommand.ScriptType.PowerShell => ProcessHelpers.RunPowershellCommand(scriptToRun),
663:             RunCommand.ScriptType.Default => ProcessHelpers.IsWindows()
664:                 ? ProcessHelpers.RunCmdCommand(scriptToRun)
665:                 : ProcessHelpers.RunBashCommand(scriptToRun),
666:             _ => throw new InvalidOperationException($"Unknown script type: {scriptType}")
667:         };
668: 
669:         var tasks = new List<Task<string>>();
670: 
671:         var content = $"## Running {scriptType} script\n\n```\n{output}\n```\n";
672:         if (!delayOutputToApplyInstructions)
673:         {
674:             ConsoleHelpers.WriteLine(content);
675:         }
676: 
677:         tasks.Add(Task.FromResult(content));
678:         return tasks;
679:     }
```

**Evidence**:
- **Lines 655-656**: Extracts only script text and script type from command
- **Lines 658-667**: Executes script using appropriate shell helper
  - Calls `ProcessHelpers.RunBashCommand()`, `ProcessHelpers.RunCmdCommand()`, or `ProcessHelpers.RunPowershellCommand()`
  - These helpers return **complete output** without any filtering
- **Line 671**: Wraps output in markdown code block (```` ```\n{output}\n``` ````)
  - Output is **unmodified** except for markdown formatting
  - No filtering, no line selection, no context expansion
- **No extraction** of context expansion properties (they don't exist)
- **No call** to `LineHelpers.FilterAndExpandContext()` (the function that implements Layer 5 for File Search)

---

## 5. Process Execution Helpers

### File: `src/common/Helpers/ProcessHelpers.cs`

The Run command delegates to process execution helpers that return **complete output**:

```csharp
public static string RunBashCommand(string command)
{
    // ... creates process, runs command ...
    return output;  // Complete stdout + stderr
}

public static string RunCmdCommand(string command)
{
    // ... creates process, runs command ...
    return output;  // Complete stdout + stderr
}

public static string RunPowershellCommand(string command)
{
    // ... creates process, runs command ...
    return output;  // Complete stdout + stderr
}
```

**Evidence**:
- These helpers return **complete, unfiltered output** from the executed script
- No filtering, no line selection, no context expansion at this level either

---

## 6. Comparison with File Search

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

**vs. Lines 653-679** (Run command execution):
```csharp
653:     private static List<Task<string>> HandleRunCommand(...)
...
658:         var output = scriptType switch
659:         {
660:             RunCommand.ScriptType.Bash => ProcessHelpers.RunBashCommand(scriptToRun),
...
667:         };
668: 
671:         var content = $"## Running {scriptType} script\n\n```\n{output}\n```\n";
```

**Evidence**:
- File Search **filters and expands** content using `LineHelpers.FilterAndExpandContext()`
- Run command **wraps and returns complete output** without any filtering
- No call to context expansion logic

---

## 7. Helper Function Not Used

### File: `src/common/Helpers/LineHelpers.cs`

The `FilterAndExpandContext` function exists but is **never called** for Run commands.

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

## 8. Data Flow Summary

```
Run Command Execution Path (NO LAYER 5):

User Input: cycodmd run --bash "echo hello\nls -la"
           ↓
CycoDmdCommandLineOptions.TryParseRunCommandOptions()
           ↓
RunCommand.ScriptToRun = "echo hello\nls -la"
RunCommand.Type = ScriptType.Bash
           ↓
Program.HandleRunCommand()
           ↓
ProcessHelpers.RunBashCommand(scriptToRun)
           ↓
Returns: string (complete stdout + stderr output)
           ↓
Wrap in markdown: "## Running Bash script\n\n```\n{output}\n```\n"
           ↓
Output: Complete, unfiltered script output
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

## 9. Design Rationale

The absence of Layer 5 in the Run command is intentional:

### Philosophy: Complete Output
```csharp
// From Program.cs line 671:
var content = $"## Running {scriptType} script\n\n```\n{output}\n```\n";
```

The Run command wraps output in a markdown code block and returns **everything**. This design ensures:
1. **Debugging**: Full context available for errors
2. **Script integrity**: No unexpected output modification
3. **Data preservation**: Scripts outputting structured data (JSON, CSV) work correctly
4. **Pipeline compatibility**: Output can be piped to other tools without loss

### Contrast with File Search Philosophy
File Search is designed to **find and highlight specific content**, so filtering and expansion make sense.  
Run command is designed to **execute and report complete results**, so filtering would be counterproductive.

---

## Conclusion

**Layer 5 (Context Expansion) is NOT IMPLEMENTED / NOT APPLICABLE for Run command** due to:

1. **No command properties**: `RunCommand` lacks `IncludeLineCountBefore` / `IncludeLineCountAfter` properties
2. **No CLI options**: Parser doesn't recognize `--lines`, `--lines-before`, `--lines-after` for run command
3. **No execution path**: `HandleRunCommand()` doesn't call `LineHelpers.FilterAndExpandContext()`
4. **Design philosophy**: Run command returns **complete, unmodified output** by design

This is a **design decision**, not a bug. The Run command is designed for script execution with complete output, while File Search is designed for content discovery with filtering and expansion.

**[← Back to Layer 5 Documentation](cycodmd-run-layer-5.md)**
