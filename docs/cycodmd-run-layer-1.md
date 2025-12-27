# cycodmd Run - Layer 1: Target Selection

**Command**: `cycodmd run [script...]`

## Purpose

Layer 1 (Target Selection) specifies **what to execute** - the shell script or commands to run. The Run command is fundamentally different from file/web commands as it doesn't search for existing content but rather executes provided commands.

## Options

### Positional Arguments: Script Content

**Syntax**: `cycodmd run [line1] [line2] ...`

**Purpose**: Specify script content to execute. Each positional argument is treated as a line of the script.

**Examples**:
```bash
cycodmd run "echo Hello World"
cycodmd run "cd src" "ls -la"
cycodmd run pwd "echo Current directory"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:462-465](cycodmd-run-layer-1-proof.md#positional-script-lines)

**Command Property**: `RunCommand.ScriptToRun` (string)

**Note**: Multiple positional arguments are joined with newlines to form a multi-line script.

---

### `--script <lines...>`

**Syntax**: `cycodmd run --script <line1> [line2] ...`

**Purpose**: Specify script content as option arguments (alternative to positional args).

**Script Type**: Default (uses cmd on Windows, bash on Linux/Mac)

**Examples**:
```bash
cycodmd run --script "echo Hello" "echo World"
cycodmd run --script "git status" "git log --oneline -5"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:64-70](cycodmd-run-layer-1-proof.md#script-option)

**Command Properties**:
- `RunCommand.ScriptToRun` (string)
- `RunCommand.Type = ScriptType.Default`

---

### `--cmd <command>`

**Syntax**: `cycodmd run --cmd <command>`

**Purpose**: Execute a command using Windows Command Prompt (cmd.exe).

**Script Type**: Cmd

**Examples**:
```bash
cycodmd run --cmd "dir /b"
cycodmd run --cmd "echo %PATH%"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:71-77](cycodmd-run-layer-1-proof.md#cmd-option)

**Command Properties**:
- `RunCommand.ScriptToRun` (string)
- `RunCommand.Type = ScriptType.Cmd`

---

### `--bash <command>`

**Syntax**: `cycodmd run --bash <command>`

**Purpose**: Execute a command using Bash shell.

**Script Type**: Bash

**Examples**:
```bash
cycodmd run --bash "ls -la"
cycodmd run --bash "echo $PATH"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:78-84](cycodmd-run-layer-1-proof.md#bash-option)

**Command Properties**:
- `RunCommand.ScriptToRun` (string)
- `RunCommand.Type = ScriptType.Bash`

---

### `--powershell <command>`

**Syntax**: `cycodmd run --powershell <command>`

**Purpose**: Execute a command using PowerShell.

**Script Type**: PowerShell

**Examples**:
```bash
cycodmd run --powershell "Get-ChildItem"
cycodmd run --powershell "Get-Process | Where-Object CPU -gt 100"
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:85-91](cycodmd-run-layer-1-proof.md#powershell-option)

**Command Properties**:
- `RunCommand.ScriptToRun` (string)
- `RunCommand.Type = ScriptType.PowerShell`

---

## Script Type Enum

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 5-11**:
```csharp
public enum ScriptType
{
    Default, // Uses cmd on Windows, bash on Linux/Mac
    Cmd,
    Bash,
    PowerShell
}
```

**Explanation**:
- `Default`: Platform-dependent (cmd on Windows, bash on Unix)
- `Cmd`: Force Windows Command Prompt
- `Bash`: Force Bash shell
- `PowerShell`: Force PowerShell

---

## Data Flow

```
User Input (script content, shell type)
    ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()  [positional script lines]
CycoDmdCommandLineOptions.TryParseRunCommandOptions()  [shell-specific options]
    ↓
RunCommand properties populated:
    - ScriptToRun (string) - joined with newlines
    - Type (ScriptType enum)
    ↓
RunCommand.Validate()
    - No special validation logic
    ↓
Script execution
```

## Integration with Other Layers

### Unique Aspects of Run Command

The Run command differs from File/Web commands:

1. **No Container Filtering** (Layer 2): Not applicable - no files or URLs to filter
2. **No Content Filtering** (Layer 3): The script IS the content
3. **No Content Removal** (Layer 4): Not applicable
4. **No Context Expansion** (Layer 5): Not applicable
5. **Display Control** (Layer 6): Applies to script output
6. **Output Persistence** (Layer 7): Can save script output
7. **AI Processing** (Layer 8): Can process script output with AI
8. **Actions on Results** (Layer 9): The execution itself is the action

### Relationship to Other Layers

- **Layer 1** (Target Selection): Defines what to execute
- **Layers 2-5**: Not applicable (no filtering pipeline)
- **Layers 6-8**: Apply to the script's output after execution
- **Layer 9**: The script execution is the primary action

---

## See Also

- [Proof Document](cycodmd-run-layer-1-proof.md) - Source code evidence and line numbers
- [Layer 6: Display Control](cycodmd-run-layer-6.md) - How script output is displayed
- [Layer 7: Output Persistence](cycodmd-run-layer-7.md) - Saving script output
- [Layer 8: AI Processing](cycodmd-run-layer-8.md) - AI analysis of script output
- [RunCommand Implementation](../src/cycodmd/CommandLineCommands/RunCommand.cs)
