# cycodmd Run Command - Layer 6 Proof: Display Control

## Overview

This document provides **source code evidence** for Display Control (Layer 6) in the cycodmd Run command. Run has the simplest Layer 6 implementation of all cycodmd commands, with no command-specific display options.

---

## Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Run Command Options Parsing

**Lines 56-98**:
```csharp
private bool TryParseRunCommandOptions(RunCommand? command, string[] args, ref int i, string arg)
{
    bool parsed = true;

    if (command == null)
    {
        parsed = false;
    }
    else if (arg == "--script")
    {
        var scriptArgs = GetInputOptionArgs(i + 1, args);
        command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
        command.Type = RunCommand.ScriptType.Default;
        i += scriptArgs.Count();
    }
    else if (arg == "--cmd")
    {
        var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
        command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
        command.Type = RunCommand.ScriptType.Cmd;
        i += scriptArgs.Count();
    }
    else if (arg == "--bash")
    {
        var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
        command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
        command.Type = RunCommand.ScriptType.Bash;
        i += scriptArgs.Count();
    }
    else if (arg == "--powershell")
    {
        var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
        command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
        command.Type = RunCommand.ScriptType.PowerShell;
        i += scriptArgs.Count();
    }
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Explanation**:
- **NO display control parsing** in Run-specific options
- Only script content (`--script`, `--cmd`, `--bash`, `--powershell`)
- Shell type selection affects execution (Layer 9), not display (Layer 6)

---

#### Save Output Parsing (Inherited)

**Lines 427-433** (Shared CycoDmd options):
```csharp
else if (arg == "--save-output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var saveOutput = max1Arg.FirstOrDefault() ?? DefaultSaveOutputTemplate;
    command.SaveOutput = saveOutput;
    i += max1Arg.Count();
}
```

**Explanation**:
- Inherited from `CycoDmdCommand` base class
- Affects output persistence (Layer 7), not display formatting
- Delays console output if specified

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/RunCommand.cs`

#### Complete Class Definition

**Lines 1-37**:
```csharp
using System;

class RunCommand : CycoDmdCommand
{
    public enum ScriptType
    {
        Default, // Uses cmd on Windows, bash on Linux/Mac
        Cmd,
        Bash,
        PowerShell
    }

    public RunCommand() : base()
    {
        ScriptToRun = string.Empty;
        Type = ScriptType.Default;
    }

    override public string GetCommandName()
    {
        return "run";
    }

    override public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(ScriptToRun);
    }

    override public CycoDmdCommand Validate()
    {
        return this;
    }

    public string ScriptToRun { get; set; }
    public ScriptType Type { get; set; }
}
```

**Display Control Properties**: **NONE**

**Explanation**:
- Only `ScriptToRun` and `Type` properties
- No line numbers, highlighting, formatting flags, etc.
- Simplest command in cycodmd

---

## Execution Flow

### File: `src/cycodmd/Program.cs`

#### Main Handler Entry Point

**Line 107**:
```csharp
RunCommand runCommand => HandleRunCommand(commandLineOptions, runCommand, processor, delayOutputToApplyInstructions),
```

**Explanation**:
- Pattern match in main command dispatcher
- Routes `RunCommand` instances to handler method

---

#### Run Command Handler

**Lines 366-381**:
```csharp
private static List<Task<string>> HandleRunCommand(CommandLineOptions commandLineOptions, RunCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
{
    var tasks = new List<Task<string>>();
    var getCheckSaveTask = GetCheckSaveRunCommandContentAsync(command);
    
    var taskToAdd = delayOutputToApplyInstructions
        ? getCheckSaveTask
        : getCheckSaveTask.ContinueWith(t =>
        {
            ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
            return t.Result;
        });

    tasks.Add(taskToAdd);
    return tasks;
}
```

**Explanation**:
1. **Create Task**: Call `GetCheckSaveRunCommandContentAsync` with command
2. **Output Decision**: If not delaying for instructions, attach console output
3. **Return**: Task for awaiting

**Display Control**:
- No display properties extracted (none exist)
- Simple pass-through to formatting function
- Console output via `ConsoleHelpers.WriteLineIfNotEmpty` (line 375)

---

#### Content Formatting Wrapper

**Lines 419-431**:
```csharp
private static async Task<string> GetFinalRunCommandContentAsync(RunCommand command)
{
    var formatted = await GetFormattedRunCommandContentAsync(command);

    // var afterInstructions = command.InstructionsList.Any()
    //     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
    //     : formatted;

    // return afterInstructions;

    return formatted;
}
```

**Explanation**:
- Wrapper for future AI instruction support (currently commented out)
- Simply returns formatted content
- No display control processing

---

#### Fixed Markdown Formatting

**Lines 433-470**:
```csharp
private static async Task<string> GetFormattedRunCommandContentAsync(RunCommand command)
{
    try
    {
        var script = command.ScriptToRun;
        var shell = command.Type switch
        {
            RunCommand.ScriptType.Cmd => "cmd",
            RunCommand.ScriptType.Bash => "bash",
            RunCommand.ScriptType.PowerShell => "pwsh",
            _ => OS.IsWindows() ? "cmd" : "bash",
        };

        var result = await ProcessHelpers.RunShellScriptAsync(shell, script);
        var output = result.MergedOutput;
        var exitCode = result.ExitCode;

        var isMultiLine = script.Contains("\n");
        var header = isMultiLine ? "## Run\n\n" : $"## `{script}`\n\n";

        var backticks = new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(output));
        var scriptPart = isMultiLine ? $"Run:\n{backticks}\n{script.TrimEnd()}\n{backticks}\n\n" : string.Empty;
        var outputPart = $"Output:\n{backticks}\n{output.TrimEnd()}\n{backticks}\n\n";
        var exitCodePart = exitCode != 0 ? $"Exit code: {exitCode}\n\n" : string.Empty;

        var sb = new StringBuilder();
        sb.Append(header);
        sb.Append(scriptPart);
        sb.Append(outputPart);
        sb.Append(exitCodePart);

        return sb.ToString().TrimEnd();
    }
    catch (Exception ex)
    {
        return $"## Error executing command: {command.ScriptToRun}\n\n{ex.Message}\n{ex.StackTrace}";
    }
}
```

**Fixed Display Format Logic**:

**Line 450**: Single-line vs. multi-line detection
```csharp
var isMultiLine = script.Contains("\n");
```

**Line 451**: Header format decision
```csharp
var header = isMultiLine ? "## Run\n\n" : $"## `{script}`\n\n";
```

**Line 453**: Backtick escaping for code blocks
```csharp
var backticks = new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(output));
```

**Line 454**: Script display (only for multi-line)
```csharp
var scriptPart = isMultiLine ? $"Run:\n{backticks}\n{script.TrimEnd()}\n{backticks}\n\n" : string.Empty;
```

**Line 455**: Output section (always)
```csharp
var outputPart = $"Output:\n{backticks}\n{output.TrimEnd()}\n{backticks}\n\n";
```

**Line 456**: Exit code (only if non-zero)
```csharp
var exitCodePart = exitCode != 0 ? $"Exit code: {exitCode}\n\n" : string.Empty;
```

**Lines 458-462**: Build final string
```csharp
var sb = new StringBuilder();
sb.Append(header);
sb.Append(scriptPart);
sb.Append(outputPart);
sb.Append(exitCodePart);
```

---

#### Error Formatting

**Lines 466-469**:
```csharp
catch (Exception ex)
{
    return $"## Error executing command: {command.ScriptToRun}\n\n{ex.Message}\n{ex.StackTrace}";
}
```

**Explanation**:
- Fixed error format
- Includes command, error message, and stack trace
- No user control over error formatting

---

## Display Format Examples

### Single-Line Script

**Input**: `cycodmd run "echo Hello"`

**Code Path**:
- `isMultiLine` = `false`
- `header` = `` ## `echo Hello` ``
- `scriptPart` = `""` (empty)
- `outputPart` = `"Output:\n```\nHello\n```\n\n"`
- `exitCodePart` = `""` (exit code 0)

**Output**:
````markdown
## `echo Hello`

Output:
```
Hello
```
````

---

### Multi-Line Script

**Input**: `cycodmd run --bash "echo Start\nls\necho Done"`

**Code Path**:
- `isMultiLine` = `true`
- `header` = `"## Run\n\n"`
- `scriptPart` = `"Run:\n```\necho Start\nls\necho Done\n```\n\n"`
- `outputPart` = `"Output:\n```\n...\n```\n\n"`
- `exitCodePart` = `""` (exit code 0)

**Output**:
````markdown
## Run

Run:
```
echo Start
ls
echo Done
```

Output:
```
Start
file1.txt
file2.txt
Done
```
````

---

### Script with Error

**Input**: `cycodmd run "exit 1"`

**Code Path**:
- `isMultiLine` = `false`
- `header` = `` ## `exit 1` ``
- `scriptPart` = `""` (empty)
- `outputPart` = `"Output:\n```\n\n```\n\n"` (no output)
- `exitCodePart` = `"Exit code: 1\n\n"` (non-zero exit)

**Output**:
````markdown
## `exit 1`

Output:
```
```

Exit code: 1
````

---

### Execution Exception

**Input**: `cycodmd run "nonexistent-command"`

**Code Path**: Exception caught

**Output**:
```markdown
## Error executing command: nonexistent-command

Command not found: nonexistent-command

[Stack trace]
```

---

## Helper Functions

### MarkdownHelpers.GetCodeBlockBacktickCharCountRequired

**Purpose**: Determine how many backticks to use for code block fence.

**Logic**:
- Scan output for consecutive backticks
- Return max consecutive count + 1

**Example**:
- Output contains "```" (3 backticks)
- Function returns 4
- Code block uses "````" (4 backticks)

**Rationale**: Ensures code blocks are properly escaped, even if output contains markdown code blocks.

---

### ProcessHelpers.RunShellScriptAsync

**Purpose**: Execute shell script and capture output.

**Returns**:
- `MergedOutput`: Combined stdout and stderr
- `ExitCode`: Process exit code

**Relevance to Display**:
- Provides raw content for formatting
- Merged output simplifies display (no separate sections)

---

## Data Flow Summary

### Parsing Phase

```
Command Line: cycodmd run "echo Hello" --save-output output.md
                                    ↓
CycoDmdCommandLineOptions.TryParseRunCommandOptions()
    Lines 64-70: Parse script content
    Line 68: Set ScriptToRun = "echo Hello"
    Line 69: Set Type = ScriptType.Default
                                    ↓
CycoDmdCommandLineOptions.TryParseSharedCycoDmdCommandOptions()
    Lines 427-433: Parse --save-output → SaveOutput = "output.md"
                                    ↓
RunCommand object created (NO display properties)
```

### Execution Phase

```
Program.Main()
    Line 107: Dispatch to HandleRunCommand()
                                    ↓
HandleRunCommand()
    Line 369: Call GetCheckSaveRunCommandContentAsync(command)
                                    ↓
GetCheckSaveRunCommandContentAsync()
    → GetFinalRunCommandContentAsync()
        → GetFormattedRunCommandContentAsync()
            Lines 437-449: Execute script
            Lines 450-462: Format output with FIXED markdown structure
            Lines 447: Get merged output
            Line 450: Detect single/multi-line
            Line 451: Create header
            Line 453: Determine backtick count
            Lines 454-456: Build parts
            Lines 458-462: Assemble final string
                                    ↓
Back to HandleRunCommand()
    Line 375: ConsoleHelpers.WriteLineIfNotEmpty() - output to console
```

---

## Summary

### Key Points

1. **NO display control properties** in `RunCommand` class
2. **NO display-specific parsing** in command options
3. **Fixed markdown format** hard-coded in `GetFormattedRunCommandContentAsync`
4. **Automatic formatting decisions**: Single/multi-line, exit code display
5. **Console output**: Simple immediate display (unless delaying)

### Display Format is Fixed

| Element | Control | User Option |
|---------|---------|-------------|
| Header format | Hard-coded | ❌ None |
| Script display | Automatic (multi-line only) | ❌ None |
| Output format | Fixed markdown code block | ❌ None |
| Exit code | Automatic (if non-zero) | ❌ None |
| Error format | Fixed markdown | ❌ None |
| Backtick count | Automatic escaping | ❌ None |

### Why This is Sufficient

Run command's purpose is to **execute scripts and show output**. Users expect:
- ✅ See the command that ran
- ✅ See the output
- ✅ Know if it failed (exit code)
- ✅ Minimal formatting overhead

Fancy display options would add complexity without value for this use case.

---

## Comparison with Other Commands

### Display Option Count

| Command | Display Options | Auto Behaviors | Total Complexity |
|---------|----------------|----------------|------------------|
| Files | 4 explicit | 3 automatic | High (7 features) |
| WebSearch | 5 explicit | 2 automatic | Medium (7 features) |
| WebGet | 5 explicit (inherited) | 2 automatic | Medium (7 features) |
| **Run** | **0 explicit** | **4 automatic** | **Low (4 features)** |

### Rationale for Simplicity

- **Files**: Line-level analysis requires granular control
- **Web**: Format conversion (HTML) requires options
- **Run**: Execute and display - no transformation needed

---

## Verification

To verify Layer 6 is working correctly:

1. **Single-Line Format**: Run single-line script, verify header format
2. **Multi-Line Format**: Run multi-line script, verify script is displayed
3. **Exit Code**: Run failing command, verify exit code appears
4. **Success No Exit**: Run successful command, verify no exit code
5. **Error Handling**: Run invalid command, verify error format
6. **Console Output**: Verify immediate output unless `--save-output` used
7. **Backtick Escaping**: Run script that outputs backticks, verify code block escaping works
