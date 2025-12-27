# cycodmd Run Command - Layer 9 Proof: Actions on Results

## Overview

This document provides **source code evidence** for Layer 9 (Actions on Results) of the cycodmd Run command, tracing the implementation from command-line parsing through script execution.

---

## 1. Command-Line Parsing

### Option: `--script`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 64-70**:
```csharp
else if (arg == "--script")
{
    var scriptArgs = GetInputOptionArgs(i + 1, args);
    command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
    command.Type = RunCommand.ScriptType.Default;
    i += scriptArgs.Count();
}
```

**Evidence**:
- Parses multiple arguments as script lines
- Joins arguments with `"\n"` (newline)
- Sets `Type` to `ScriptType.Default` (platform-specific shell)
- Uses `ValidateJoinedString` to append to existing script

---

### Option: `--cmd`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 71-77**:
```csharp
else if (arg == "--cmd")
{
    var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
    command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
    command.Type = RunCommand.ScriptType.Cmd;
    i += scriptArgs.Count();
}
```

**Evidence**:
- Takes **single argument** (`max: 1`)
- Sets `Type` to `ScriptType.Cmd` (Windows CMD)
- Appends to `ScriptToRun` (allows combining multiple shell commands)

---

### Option: `--bash`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 78-84**:
```csharp
else if (arg == "--bash")
{
    var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
    command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
    command.Type = RunCommand.ScriptType.Bash;
    i += scriptArgs.Count();
}
```

**Evidence**:
- Takes **single argument** (`max: 1`)
- Sets `Type` to `ScriptType.Bash`
- Consistent structure with `--cmd`

---

### Option: `--powershell`

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 85-91**:
```csharp
else if (arg == "--powershell")
{
    var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
    command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
    command.Type = RunCommand.ScriptType.PowerShell;
    i += scriptArgs.Count();
}
```

**Evidence**:
- Takes **single argument** (`max: 1`)
- Sets `Type` to `ScriptType.PowerShell`
- Consistent structure with other shell options

---

### Positional Arguments

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 462-466**:
```csharp
else if (command is RunCommand runCommand)
{
    runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
    parsedOption = true;
}
```

**Evidence**:
- Non-option arguments are treated as script lines
- Each argument is appended with `\n` separator
- `.Trim()` removes leading/trailing whitespace
- Allows building multi-line scripts from multiple arguments

**Example Flow**:
```
Input: cycodmd run "echo A" "echo B" "echo C"

After "echo A": ScriptToRun = "echo A"
After "echo B": ScriptToRun = "echo A\necho B"
After "echo C": ScriptToRun = "echo A\necho B\necho C"
```

---

## 2. Command Properties

### RunCommand Specific Properties

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 5-11** (ScriptType enum):
```csharp
public enum ScriptType
{
    Default, // Uses cmd on Windows, bash on Linux/Mac
    Cmd,
    Bash,
    PowerShell
}
```

**Evidence**: Four script execution modes defined as enum.

---

**Lines 34-35** (Properties):
```csharp
public string ScriptToRun { get; set; }
public ScriptType Type { get; set; }
```

**Evidence**:
- `ScriptToRun`: The script text to execute
- `Type`: Which shell to use for execution

---

### Property Initialization

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 13-17** (Constructor):
```csharp
public RunCommand() : base()
{
    ScriptToRun = string.Empty;
    Type = ScriptType.Default;
}
```

**Evidence**:
- `ScriptToRun` defaults to empty string
- `Type` defaults to `ScriptType.Default` (platform-specific)
- Simple initialization (no complex defaults)

---

### IsEmpty Check

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 24-27**:
```csharp
override public bool IsEmpty()
{
    return string.IsNullOrWhiteSpace(ScriptToRun);
}
```

**Evidence**:
- Command is empty if no script provided
- Uses `IsNullOrWhiteSpace` (stricter than `IsNullOrEmpty`)
- Simple validation logic

---

### Validation Method

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 29-32**:
```csharp
override public CycoDmdCommand Validate()
{
    return this;
}
```

**Evidence**:
- **No validation logic** (just `return this;`)
- Simplest validation of all cycodmd commands
- No auto-enable logic (unlike WebSearch)
- No default value injection (unlike FindFiles)

---

## 3. Execution Logic

### Entry Point

**File**: `src/cycodmd/Program.cs`

**Lines 107**:
```csharp
RunCommand runCommand => HandleRunCommand(commandLineOptions, runCommand, processor, delayOutputToApplyInstructions),
```

**Evidence**: Pattern matching dispatches to `HandleRunCommand` method.

---

### Handler Method

**File**: `src/cycodmd/Program.cs`

**Lines 366-377**:
```csharp
private static List<Task<string>> HandleRunCommand(CommandLineOptions commandLineOptions, RunCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
{
    var tasks = new List<Task<string>>();
    var getCheckSaveTask = GetCheckSaveRunCommandContentAsync(command);
    
    var taskToAdd = delayOutputToApplyInstructions
        ? getCheckSaveTask
        : getCheckSaveTask.ContinueWith(async t => { ConsoleHelpers.WriteLine(await t); return await t; }).Unwrap();
    tasks.Add(taskToAdd);

    return tasks;
}
```

**Evidence**:
- Creates async task for script execution
- Calls `GetCheckSaveRunCommandContentAsync(command)`
- No conditional checks (unlike WebSearch's `if (!getContent)`)
- **Always executes** the script (no preview mode)
- Single task (not multiple like file/web processing)

---

### Top-Level Execution Method

**File**: `src/cycodmd/Program.cs`

**Lines 398-416**:
```csharp
private static async Task<string> GetCheckSaveRunCommandContentAsync(RunCommand command)
{
    try
    {
        ConsoleHelpers.DisplayStatus($"Executing: {command.ScriptToRun} ...");
        var finalContent = await GetFinalRunCommandContentAsync(command);

        if (!string.IsNullOrEmpty(command.SaveOutput))
        {
            FileHelpers.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(command.SaveOutput)));
            await FileHelpers.WriteAllTextAsync(command.SaveOutput, finalContent);
        }

        return finalContent;
    }
    catch (Exception ex)
    {
        return $"## Error: {ex.Message}\n{ex.StackTrace}";
    }
}
```

**Evidence**:
- Displays status message with script being executed
- Calls `GetFinalRunCommandContentAsync` to run script
- Saves output if `SaveOutput` is set (Layer 7 integration)
- Error handling catches all exceptions
- Returns formatted error message on failure

---

### Final Content Method

**File**: `src/cycodmd/Program.cs`

**Lines 420-431**:
```csharp
private static async Task<string> GetFinalRunCommandContentAsync(RunCommand command)
{
    var formatted = await GetFormattedRunCommandContentAsync(command);

    // var afterInstructions = command.InstructionsList.Any()
    //     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
    //     : formatted;
    // 
    // return afterInstructions;
    return formatted;
}
```

**Evidence**:
- Calls `GetFormattedRunCommandContentAsync` to execute script
- **AI processing is commented out** (Lines 424-428)
- Currently just returns formatted output without AI processing
- **Future feature**: AI can analyze script output

**Analysis**: Layer 8 (AI Processing) integration exists but is disabled.

---

### Script Execution Method

**File**: `src/cycodmd/Program.cs`

**Lines 433-469**:
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

        var output = await ProcessHelpers.RunShellCommand(shell, script);
        
        var sb = new StringBuilder();
        sb.AppendLine($"## Command: {script}");
        sb.AppendLine($"## Shell: {shell}");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(output);
        sb.AppendLine("```");
        
        return sb.ToString();
    }
    catch (Exception ex)
    {
        return $"## Error executing command: {command.ScriptToRun}\n\n{ex.Message}\n{ex.StackTrace}";
    }
}
```

**Evidence**:
- Extracts `script` from `command.ScriptToRun`
- Maps `ScriptType` to shell executable:
  - `Cmd` → `"cmd"`
  - `Bash` → `"bash"`
  - `PowerShell` → `"pwsh"`
  - `Default` → Platform-specific (`"cmd"` on Windows, `"bash"` otherwise)
- Calls `ProcessHelpers.RunShellCommand(shell, script)`
- Formats output as markdown with:
  - Command header
  - Shell type header
  - Output in code block
- Returns formatted string
- Error handling with detailed exception info

---

## 4. Shell Type Mapping

### ScriptType to Shell Executable

**File**: `src/cycodmd/Program.cs`

**Lines 438-444** (within `GetFormattedRunCommandContentAsync`):
```csharp
var shell = command.Type switch
{
    RunCommand.ScriptType.Cmd => "cmd",
    RunCommand.ScriptType.Bash => "bash",
    RunCommand.ScriptType.PowerShell => "pwsh",
    _ => OS.IsWindows() ? "cmd" : "bash",
};
```

**Evidence**:
- **Switch expression** maps enum to executable name
- **PowerShell**: Uses `"pwsh"` (cross-platform PowerShell Core, not Windows PowerShell)
- **Default case**: Platform detection using `OS.IsWindows()`
  - Windows → `"cmd"`
  - Linux/Mac → `"bash"`
- Executable names are passed to `ProcessHelpers.RunShellCommand`

---

## 5. Data Flow Trace

### Complete Call Stack

```
1. User Command:
   cycodmd run --bash "ls -la | grep test"

2. CycoDmdCommandLineOptions.Parse()
   ├─ PeekCommandName() detects "run"
   ├─ NewCommandFromName("run") → new RunCommand()
   └─ TryParseRunCommandOptions():
      ├─ "--bash" → Type = ScriptType.Bash
      └─ "ls -la | grep test" → ScriptToRun = "ls -la | grep test"

3. RunCommand.Validate()
   └─ Just returns this (no validation)

4. Program.Main()
   └─ Dispatches to HandleRunCommand(command)

5. HandleRunCommand(command)
   ├─ Creates task: GetCheckSaveRunCommandContentAsync(command)
   └─ Returns list with single task

6. GetCheckSaveRunCommandContentAsync(command)
   ├─ Display status: "Executing: ls -la | grep test ..."
   ├─ Call GetFinalRunCommandContentAsync(command)
   ├─ If SaveOutput set:
   │  └─ Write output to file
   └─ Return output

7. GetFinalRunCommandContentAsync(command)
   ├─ Call GetFormattedRunCommandContentAsync(command)
   └─ Return formatted output (AI processing commented out)

8. GetFormattedRunCommandContentAsync(command)
   ├─ script = "ls -la | grep test"
   ├─ shell = command.Type → "bash"
   ├─ Call ProcessHelpers.RunShellCommand("bash", "ls -la | grep test")
   ├─ Format output:
   │  ├─ "## Command: ls -la | grep test"
   │  ├─ "## Shell: bash"
   │  └─ "```\n{output}\n```"
   └─ Return formatted string

9. ProcessHelpers.RunShellCommand("bash", "ls -la | grep test")
   └─ Executes command and returns stdout/stderr
```

---

## 6. Positional Argument Accumulation

### Multi-Argument Script Building

**Evidence** (CycoDmdCommandLineOptions.cs:462-466):
```csharp
else if (command is RunCommand runCommand)
{
    runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
    parsedOption = true;
}
```

**Example Trace**:
```
Command: cycodmd run "echo line 1" "echo line 2" "echo line 3"

Step 1 (after "echo line 1"):
    ScriptToRun = "" + "\n" + "echo line 1" = "echo line 1" (after Trim)

Step 2 (after "echo line 2"):
    ScriptToRun = "echo line 1" + "\n" + "echo line 2" = "echo line 1\necho line 2"

Step 3 (after "echo line 3"):
    ScriptToRun = "echo line 1\necho line 2" + "\n" + "echo line 3" 
                = "echo line 1\necho line 2\necho line 3"
```

**Analysis**:
- Each positional argument becomes a line in the script
- Lines are joined with `\n`
- Natural multi-line script building from command-line args

---

## 7. Comparison with Other Commands

### No Conditional Execution

**FindFiles** (Program.cs:209-210):
```csharp
if (!string.IsNullOrEmpty(findFilesCommand.ReplaceWithText) && 
    findFilesCommand.IncludeLineContainsPatternList.Count > 0)
```
Has conditional for replacement mode.

**WebSearch** (Program.cs:302-305):
```csharp
if (urls.Count == 0 || !getContent)
{
    return new List<Task<string>>() { Task.FromResult(searchSection) };
}
```
Has conditional for content fetching.

**Run** (Program.cs:369):
```csharp
var getCheckSaveTask = GetCheckSaveRunCommandContentAsync(command);
```
**No conditional check** - always executes.

**Evidence**: Run command has no gate/conditional like other commands.

---

### Simplest Validation

**Validation Complexity Comparison**:

| Command | Lines of Code | Logic |
|---------|---------------|-------|
| FindFiles | 16 lines | Adds default glob, loads ignore file |
| WebSearch | 13 lines | Auto-enables GetContent if AI instructions |
| WebGet | 3 lines | Just `return this;` |
| **Run** | **3 lines** | **Just `return this;`** |

**Evidence**: Run ties with WebGet for simplest validation.

---

## 8. AI Processing Integration (Disabled)

### Commented-Out Code

**File**: `src/cycodmd/Program.cs`

**Lines 424-428** (within `GetFinalRunCommandContentAsync`):
```csharp
// var afterInstructions = command.InstructionsList.Any()
//     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
//     : formatted;
// 
// return afterInstructions;
```

**Evidence**:
- AI processing logic exists but is commented out
- Would check if `InstructionsList` has any instructions
- Would call `AiInstructionProcessor.ApplyAllInstructions`
- Would pass `UseBuiltInFunctions` and `SaveChatHistory` (Layer 8 properties)

**Analysis**: Feature is partially implemented but disabled.

---

### Why Disabled?

**Hypothesis** (based on code structure):
- May be incomplete implementation
- May have dependencies not yet resolved
- May be awaiting testing
- May be intentionally disabled for future release

**Evidence for Partial Implementation**:
- Method call structure is correct
- Properties exist (`InstructionsList`, `UseBuiltInFunctions`, `SaveChatHistory`)
- Just needs uncommenting to enable

---

## 9. Output Formatting

### Markdown Output Format

**File**: `src/cycodmd/Program.cs`

**Lines 449-456** (within `GetFormattedRunCommandContentAsync`):
```csharp
var sb = new StringBuilder();
sb.AppendLine($"## Command: {script}");
sb.AppendLine($"## Shell: {shell}");
sb.AppendLine();
sb.AppendLine("```");
sb.AppendLine(output);
sb.AppendLine("```");
```

**Evidence**:
- Output is formatted as markdown
- Headers show command and shell type
- Command output is in code block
- Consistent with other cycodmd output formatting

**Example Output**:
```markdown
## Command: ls -la
## Shell: bash

\```
total 64
drwxr-xr-x  10 user  staff   320 Jan 15 10:30 .
drwxr-xr-x  20 user  staff   640 Jan 14 09:15 ..
-rw-r--r--   1 user  staff  1234 Jan 15 10:30 file.txt
\```
```

---

## 10. Error Handling

### Execution-Level Error Handling

**File**: `src/cycodmd/Program.cs`

**Lines 465-469** (within `GetFormattedRunCommandContentAsync`):
```csharp
catch (Exception ex)
{
    return $"## Error executing command: {command.ScriptToRun}\n\n{ex.Message}\n{ex.StackTrace}";
}
```

**Evidence**:
- Catches all exceptions during script execution
- Returns formatted error message
- Includes:
  - Command that failed
  - Exception message
  - Full stack trace
- Doesn't rethrow (non-fatal error handling)

---

### Top-Level Error Handling

**File**: `src/cycodmd/Program.cs`

**Lines 413-416** (within `GetCheckSaveRunCommandContentAsync`):
```csharp
catch (Exception ex)
{
    return $"## Error: {ex.Message}\n{ex.StackTrace}";
}
```

**Evidence**:
- Second layer of error handling
- Catches errors from save operations
- Returns simpler error format
- Ensures execution doesn't crash cycodmd

---

## 11. Integration with ProcessHelpers

### Shell Execution Delegation

**File**: `src/cycodmd/Program.cs`

**Line 447** (within `GetFormattedRunCommandContentAsync`):
```csharp
var output = await ProcessHelpers.RunShellCommand(shell, script);
```

**Evidence**:
- Actual execution delegated to `ProcessHelpers`
- Passes shell type and script text
- Returns combined stdout/stderr
- Async operation (awaited)
- Demonstrates separation of concerns:
  - RunCommand: CLI parsing and formatting
  - ProcessHelpers: Process execution

---

## 12. Summary of Evidence

### Parser Evidence
✅ `--script` parsed at CycoDmdCommandLineOptions.cs:64-70  
✅ `--cmd` parsed at CycoDmdCommandLineOptions.cs:71-77  
✅ `--bash` parsed at CycoDmdCommandLineOptions.cs:78-84  
✅ `--powershell` parsed at CycoDmdCommandLineOptions.cs:85-91  
✅ Positional args parsed at CycoDmdCommandLineOptions.cs:462-466

### Property Evidence
✅ `ScriptToRun` property defined at RunCommand.cs:34  
✅ `Type` property defined at RunCommand.cs:35  
✅ `ScriptType` enum defined at RunCommand.cs:5-11  
✅ Default values set at RunCommand.cs:15-16

### Validation Evidence
✅ No validation logic at RunCommand.cs:29-32 (simplest in codebase)  
✅ `IsEmpty()` check at RunCommand.cs:24-27

### Execution Evidence
✅ Handler at Program.cs:366-377  
✅ **No conditional execution** (always runs)  
✅ Top-level method at Program.cs:398-416  
✅ Final content method at Program.cs:420-431  
✅ **AI processing disabled** at Program.cs:424-428  
✅ Script execution method at Program.cs:433-469  
✅ Shell type mapping at Program.cs:438-444  
✅ ProcessHelpers delegation at Program.cs:447

### Integration Evidence
✅ Layer 7 (Output Persistence) at Program.cs:408-411  
✅ Layer 8 (AI Processing) commented out at Program.cs:424-428  
✅ ProcessHelpers integration at Program.cs:447

### Output Evidence
✅ Markdown formatting at Program.cs:449-456  
✅ Error handling at Program.cs:465-469, 413-416

---

## Conclusion

The evidence conclusively demonstrates that Run Layer 9 (Actions on Results) is implemented through:

1. **Four execution modes**: Default, Cmd, Bash, PowerShell
2. **Always-execute design**: No preview or conditional logic
3. **Multi-line script building**: From positional arguments
4. **Shell type mapping**: Enum to executable name
5. **ProcessHelpers delegation**: Actual execution
6. **Markdown output**: Formatted results
7. **Error handling**: Two-level catch blocks
8. **Simplest validation**: Just `return this;`
9. **AI processing (disabled)**: Commented out but structure exists
10. **Layer 7 integration**: Output persistence

The **key characteristic** of Run Layer 9 is **unconditional execution** - scripts always run when the command is invoked (no preview mode, no safety flag).

All claims in the [Layer 9 documentation](cycodmd-run-layer-9.md) are supported by the source code evidence presented above.
