# cycodmd Run Command - Layer 9: Actions on Results

## Overview

**Layer**: 9 - Actions on Results  
**Command**: `cycodmd run [script...]` (RunCommand)  
**Purpose**: Execute shell scripts with specified shell types.

Layer 9 for the Run command implements a **direct execution** model where the primary action is running the provided script. Unlike FindFiles (which has preview/execute modes) or WebSearch (which has fetch/no-fetch modes), the Run command's entire purpose is execution - there is no "preview" mode for scripts.

## Command-Line Options

### Core Action Options

#### Script Type Selection

The Run command supports four script execution modes, selected via option or positional arguments:

##### `--script <commands...>`
**Purpose**: Execute commands using platform default shell  
**Type**: Multiple string arguments  
**Parsed At**: `CycoDmdCommandLineOptions.cs:64-70`  
**Stored In**: `RunCommand.ScriptToRun`, `RunCommand.Type = ScriptType.Default`  
**Default Shell**: CMD on Windows, Bash on Linux/Mac

**Example**:
```bash
cycodmd run --script "echo Hello" "dir"
```

##### `--cmd <command>`
**Purpose**: Execute command using CMD (Windows command prompt)  
**Type**: Single string argument  
**Parsed At**: `CycoDmdCommandLineOptions.cs:71-77`  
**Stored In**: `RunCommand.ScriptToRun`, `RunCommand.Type = ScriptType.Cmd`

**Example**:
```bash
cycodmd run --cmd "dir /s"
```

##### `--bash <command>`
**Purpose**: Execute command using Bash  
**Type**: Single string argument  
**Parsed At**: `CycoDmdCommandLineOptions.cs:78-84`  
**Stored In**: `RunCommand.ScriptToRun`, `RunCommand.Type = ScriptType.Bash`

**Example**:
```bash
cycodmd run --bash "ls -la | grep test"
```

##### `--powershell <command>`
**Purpose**: Execute command using PowerShell (pwsh)  
**Type**: Single string argument  
**Parsed At**: `CycoDmdCommandLineOptions.cs:85-91`  
**Stored In**: `RunCommand.ScriptToRun`, `RunCommand.Type = ScriptType.PowerShell`

**Example**:
```bash
cycodmd run --powershell "Get-ChildItem | Where-Object {$_.Length -gt 1MB}"
```

#### Positional Arguments (Multi-Line Scripts)

**Parsed At**: `CycoDmdCommandLineOptions.cs:462-466`

All non-option arguments are treated as script lines and concatenated with newlines.

**Example**:
```bash
cycodmd run "echo Starting" "echo Middle" "echo Done"
# Becomes: "echo Starting\necho Middle\necho Done"
```

## Data Flow

### 1. Option Parsing

```
User Input: cycodmd run --bash "ls -la"
         ↓
CycoDmdCommandLineOptions.TryParseRunCommandOptions()
         ↓
RunCommand.ScriptToRun = "ls -la"
RunCommand.Type = ScriptType.Bash
```

### 2. Positional Argument Accumulation

```
User Input: cycodmd run "echo A" "echo B"
         ↓
TryParseOtherCommandArg() (called twice)
    First: ScriptToRun = "echo A"
    Second: ScriptToRun = "echo A\necho B"
```

### 3. Execution Flow

```
RunCommand.ExecuteAsync()
         ↓
Program.HandleRunCommand()
         ↓
GetCheckSaveRunCommandContentAsync(command)
         ↓
GetFinalRunCommandContentAsync(command)
         ↓
GetFormattedRunCommandContentAsync(command)
         ↓
ProcessHelpers.RunShellCommand(
    shellType: command.Type → "cmd"|"bash"|"pwsh",
    command: command.ScriptToRun
)
         ↓
Return stdout/stderr output
```

## Integration with Other Layers

### Dependencies

Layer 9 for Run command is relatively independent:

- **Layer 1 (Target Selection)**: Script text IS the target
  - Uses `ScriptToRun` property
  - Uses `Type` property for shell selection
  - No file/URL targets like other commands

- **Layer 6 (Display Control)**: Minimal
  - Output is always displayed
  - No special formatting options
  - Script output is shown as-is

- **Layer 7 (Output Persistence)**: Saves script output
  - Uses `SaveOutput` from base `CycoDmdCommand`
  - Can save execution results to file

- **Layer 8 (AI Processing)**: Can process script output
  - Uses `InstructionsList` from base class
  - AI can analyze command output
  - Currently **commented out** in implementation (see proof document)

### No Preview Mode

Unlike FindFiles Layer 9 (which has `--execute` flag for preview vs. execution):
- Run command **always executes** the script
- No "dry-run" or "preview" mode
- Execution is the command's sole purpose
- If you don't want to execute, don't run the command

## Shell Type Mapping

**Implementation**: `Program.GetFormattedRunCommandContentAsync()` (Lines 438-444)

```csharp
var shell = command.Type switch
{
    RunCommand.ScriptType.Cmd => "cmd",
    RunCommand.ScriptType.Bash => "bash",
    RunCommand.ScriptType.PowerShell => "pwsh",
    _ => OS.IsWindows() ? "cmd" : "bash",  // Default/platform-specific
};
```

**Evidence**:
- Each `ScriptType` maps to a specific shell executable
- Default type uses platform detection
- PowerShell uses `pwsh` (cross-platform PowerShell Core)

## Execution Evidence

See [Layer 9 Proof Document](cycodmd-run-layer-9-proof.md) for detailed source code references showing:

- Command-line parsing for script options
- Positional argument accumulation
- Property storage in `RunCommand`
- Shell type selection logic
- Execution through `ProcessHelpers.RunShellCommand()`
- Output capture and formatting

## Examples

### Example 1: Simple Command (Platform Default)

```bash
cycodmd run "echo Hello World"
```

**Result**: Executes `echo Hello World` using CMD (Windows) or Bash (Linux/Mac).

**Layer 9 Action**: Direct execution with platform default shell.

---

### Example 2: Multi-Line Script

```bash
cycodmd run \
  "echo Starting build" \
  "dotnet build" \
  "echo Build complete"
```

**Result**: Executes three commands sequentially using platform default shell.

**Layer 9 Action**: Multi-line script execution.

---

### Example 3: Bash-Specific Command

```bash
cycodmd run --bash "ls -la | grep .cs | wc -l"
```

**Result**: Uses Bash to execute Unix-style piping and commands.

**Layer 9 Action**: Bash execution with piping.

---

### Example 4: PowerShell Command

```bash
cycodmd run --powershell "Get-Process | Where-Object {$_.CPU -gt 100} | Sort-Object CPU -Descending"
```

**Result**: Uses PowerShell to query and filter processes.

**Layer 9 Action**: PowerShell execution with object pipeline.

---

### Example 5: CMD Command (Windows)

```bash
cycodmd run --cmd "dir /s /b *.cs"
```

**Result**: Uses CMD to recursively list C# files.

**Layer 9 Action**: CMD execution with Windows-specific switches.

---

### Example 6: Save Output

```bash
cycodmd run --bash "df -h" --save-output disk-usage.md
```

**Result**: Executes command and saves output to `disk-usage.md`.

**Layer 9 Action**: Execute and persist output (Layer 7 integration).

---

### Example 7: AI Processing (If Enabled)

```bash
cycodmd run --bash "ps aux" --instructions "Identify processes using most CPU"
```

**Result**: Executes `ps aux` and asks AI to analyze the output.

**Layer 9 Action**: Execute for AI processing (Layer 8 integration).

**Note**: AI processing is currently **commented out** in implementation (see proof).

## Behavioral Notes

### Always-Execute Design

- Run command **always executes** the script (no preview mode)
- Rationale: The command's purpose is execution
- If you don't want execution, don't invoke the command
- No safety flag like FindFiles' `--execute`

### Script Concatenation

- Multiple positional arguments are joined with `\n`
- Each argument becomes a separate line in the script
- Order is preserved
- Empty arguments are trimmed

### Shell Selection Priority

1. **Explicit shell option**: `--bash`, `--cmd`, `--powershell`
2. **Default/`--script`**: Platform-specific (CMD on Windows, Bash elsewhere)

### Error Handling

- Script execution errors are captured
- Stdout and stderr are both returned
- Exit codes are handled by `ProcessHelpers`
- Errors don't stop cycodmd (just reported)

### Cross-Platform Considerations

**Platform-Specific Shells**:
- **Windows**: CMD available by default
- **Linux/Mac**: Bash available by default
- **Cross-platform**: PowerShell (`pwsh`) requires installation

**Best Practice**: Use `--bash` or `--powershell` for cross-platform scripts.

## Comparison with Other Commands' Layer 9

| Command | Action Type | Safety Flag | Preview Mode | Always Executes |
|---------|-------------|-------------|--------------|-----------------|
| **FindFiles** | Modify files | `--execute` required | Yes (default) | No |
| **WebSearch** | Fetch content | `--get` optional | No | No |
| **WebGet** | Fetch content | N/A | No | Yes |
| **Run** | **Execute script** | **None** | **No** | **Yes** |

**Analysis**:
- Run is most similar to WebGet (both always execute their primary action)
- Run has no safety mechanism (unlike FindFiles)
- Run has no conditional execution (unlike WebSearch)

## Safety Considerations

### No Undo for Script Execution

**Warning**: Scripts can modify system state, and there's no undo mechanism.

**Mitigation**:
- Review scripts before execution
- Use version control for file modifications
- Test scripts in safe environment first
- Consider using `--dry-run` flags within scripts themselves (if supported)

### Privilege Escalation

**Warning**: Scripts run with cycodmd's privileges (user's privileges).

**Mitigation**:
- Don't run untrusted scripts
- Understand what each command does
- Use principle of least privilege
- Avoid running cycodmd with elevated privileges unless necessary

### Shell Injection Risks

**Warning**: If script content comes from untrusted sources, shell injection is possible.

**Mitigation**:
- Validate/sanitize input if generating scripts programmatically
- Use explicit shell type to avoid unexpected behavior
- Don't interpolate untrusted data into scripts

## Design Philosophy

### Direct Execution Model

Run command implements the **simplest action model**:

1. User provides script
2. cycodmd executes it
3. Output is returned

No preview, no confirmation, no conditional logic.

### Shell Type Flexibility

Supports four execution modes to handle different scenarios:

1. **Default**: Cross-platform convenience
2. **Cmd**: Windows-specific commands
3. **Bash**: Unix/Linux-specific commands
4. **PowerShell**: Cross-platform modern shell

### Output as Content

Script output is treated as **content** for downstream processing:

- Can be saved (Layer 7)
- Can be AI-processed (Layer 8)
- Can be piped to other tools

This makes Run command a **content generator** rather than just a script runner.

## Future Enhancements (Not Currently Implemented)

Based on commented-out code in `GetFinalRunCommandContentAsync()`:

### AI Processing Integration

**Currently Commented Out** (Program.cs:424-428):
```csharp
// var afterInstructions = command.InstructionsList.Any()
//     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
//     : formatted;
// 
// return afterInstructions;
```

**If Enabled**: Would allow AI to process script output, enabling use cases like:
- "Summarize build errors"
- "Explain process CPU usage"
- "Suggest optimizations based on performance output"

## Related Layers

- **[Layer 1: Target Selection](cycodmd-run-layer-1.md)** - Script input
- **[Layer 7: Output Persistence](cycodmd-run-layer-7.md)** - Saving output
- **[Layer 8: AI Processing](cycodmd-run-layer-8.md)** - Analyzing output (currently disabled)

## See Also

- [Layer 9 Proof Document](cycodmd-run-layer-9-proof.md) - Detailed source code evidence
- [Run Command Overview](cycodmd-run-filtering-pipeline-catalog-README.md)
- [FindFiles Layer 9](cycodmd-files-layer-9.md) - Comparison with conditional execution
- [WebGet Layer 9](cycodmd-webget-layer-9.md) - Similar always-execute pattern
