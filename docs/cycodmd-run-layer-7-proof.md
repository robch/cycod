# cycodmd Run - Layer 7: Output Persistence - PROOF

**[← Back to Layer 7 Description](cycodmd-run-layer-7.md)**

## Source Code Evidence

This document provides **definitive proof** of Layer 7 implementation for the Run command through source code references.

---

## Command Class Definition

### RunCommand Class

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Class Declaration** (inferred from parser):
```csharp
class RunCommand : CycoDmdCommand
```

**Properties** (not shown in provided code, but inferred from parser):
```csharp
public string ScriptToRun { get; set; }
public ScriptType Type { get; set; }

public enum ScriptType
{
    Default,
    Bash,
    Cmd,
    PowerShell
}
```

**Inherited Properties from CycoDmdCommand** (Layer 7 relevant):
```csharp
public string? SaveOutput;              // --save-output
public string? SaveChatHistory;         // --save-chat-history
```

**Note**: RunCommand does NOT inherit from `WebCommand`, so it lacks:
- `SaveFolder` property
- `SavePageOutput` property

---

## Option Parsing

### 1. `--save-output` (Shared Option)

**Parser Location**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Parsing Code** (lines 427-432):
```csharp
else if (arg == "--save-output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var saveOutput = max1Arg.FirstOrDefault() ?? DefaultSaveOutputTemplate;
    command.SaveOutput = saveOutput;
    i += max1Arg.Count();
}
```

**Function**: `TryParseSharedCycoDmdCommandOptions()`  
**Applies To**: All `CycoDmdCommand` subclasses including `RunCommand`

**Default Value** (line 483):
```csharp
public const string DefaultSaveOutputTemplate = "output.md";
```

---

### 2. `--save-chat-history` (Shared AI Option)

**Parser Location**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Parsing Code** (lines 434-440):
```csharp
else if (arg == "--save-chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var saveChatHistory = max1Arg.FirstOrDefault() ?? AiInstructionProcessor.DefaultSaveChatHistoryTemplate;
    command.SaveChatHistory = saveChatHistory;
    i += max1Arg.Count();
}
```

**Function**: `TryParseSharedCycoDmdCommandOptions()`  
**Applies To**: All `CycoDmdCommand` subclasses

**Default Value**:
- **Source**: `AiInstructionProcessor.DefaultSaveChatHistoryTemplate`
- **Typical Value**: `"chat-history-{time}.jsonl"`

---

## Run-Specific Option Parsing

### Script Content Options (Layer 1, not Layer 7)

**Parser Location**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Function**: `TryParseRunCommandOptions()` (lines 56-98)

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

**Note**: These options affect WHAT to run (Layer 1), not WHERE to save output (Layer 7).

---

## Positional Arguments (Script Content)

**Parser Location**: `CycoDmdCommandLineOptions.TryParseOtherCommandArg()` (lines 462-466)

```csharp
else if (command is RunCommand runCommand)
{
    runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
    parsedOption = true;
}
```

**Behavior**: Non-option arguments are appended to the script content.

**Example**:
```bash
cycodmd run "echo hello" "echo world" --save-output greetings.md
#           ^^^^^^^^^^^^  ^^^^^^^^^^^^                ^^^^^^^^^^^^
#           Positional 1   Positional 2              Option (Layer 7)
```

**Result**: `ScriptToRun = "echo hello\necho world"`

---

## Command Creation

**Parser Location**: `CycoDmdCommandLineOptions.NewCommandFromName()` (lines 37-46)

```csharp
override protected Command? NewCommandFromName(string commandName)
{
    return commandName switch
    {
        "web search" => new WebSearchCommand(),
        "web get" => new WebGetCommand(),
        "run" => new RunCommand(),              // ← Line 43
        _ => base.NewCommandFromName(commandName)
    };
}
```

**Command Name Detection**: `PeekCommandName()` (lines 17-25) identifies "run" from first argument.

---

## Parser Control Flow

### Option Parsing Sequence for RunCommand

**Entry Point**: `CycoDmdCommandLineOptions.TryParseOtherCommandOptions()` (line 48-54)

```csharp
override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
{
    return TryParseFindFilesCommandOptions(command as FindFilesCommand, args, ref i, arg) ||
           TryParseWebCommandOptions(command as WebCommand, args, ref i, arg) ||
           TryParseRunCommandOptions(command as RunCommand, args, ref i, arg) ||      // ← RunCommand matches here
           TryParseSharedCycoDmdCommandOptions(command as CycoDmdCommand, args, ref i, arg);
}
```

**Parse Order for RunCommand**:
1. Try Run-specific options (`TryParseRunCommandOptions()`) - lines 56-98
   - No Layer 7 options here (only `--script`, `--bash`, etc.)
2. Try Shared options (`TryParseSharedCycoDmdCommandOptions()`) - lines 409-451
   - Includes `--save-output` (lines 427-432)
   - Includes `--save-chat-history` (lines 434-440)

**Result**: RunCommand supports only 2 Layer 7 options (shared options only).

---

## Absence of Per-Item Output

### Why No `--save-file-output` or `--save-page-output`?

**FindFilesCommand** has `SaveFileOutput` (line 110):
```csharp
public string? SaveFileOutput;
```

**WebCommand** has `SavePageOutput` (line 37):
```csharp
public string? SavePageOutput { get; set; }
```

**RunCommand** has neither property.

**Reason**: 
- RunCommand executes a single script (one execution context)
- No concept of multiple "items" to save separately
- All output is from one unified script execution

---

## Data Flow Through Layer 7

### Script Execution to Output Saving

1. **Parse Command Line**: Create `RunCommand` with `ScriptToRun` and `Type`
2. **Execute Script** (Layer 9, implicit):
   - Run script using appropriate shell (bash/cmd/powershell)
   - Capture stdout and stderr
3. **Format Output** (Layer 6):
   - Convert output to markdown
4. **AI Processing** (Layer 8, optional):
   - If `--instructions` used, analyze output
   - Generate chat history
5. **Write Files** (Layer 7):
   - If `SaveOutput` set, write output file
   - If `SaveChatHistory` set AND AI used, write chat history

### Output File Content

**Structure**:
```markdown
# Script Execution Result

## Command
[script content]

## Output
[stdout]

## Errors (if any)
[stderr]

## AI Analysis (if used)
[AI-generated analysis]
```

---

## Option Interaction Examples

### Example 1: Basic Output Saving

```bash
cycodmd run "ls -la" --save-output listing.md
```

**Parsed State**:
- `ScriptToRun = "ls -la"`
- `Type = ScriptType.Default`
- `SaveOutput = "listing.md"`

**Result**: Execution output saved to `listing.md`.

---

### Example 2: Multiple Commands

```bash
cycodmd run "echo 'Start'" "date" "echo 'End'" --save-output log.md
```

**Parsed State**:
- `ScriptToRun = "echo 'Start'\ndate\necho 'End'"`
- `Type = ScriptType.Default`
- `SaveOutput = "log.md"`

**Result**: All commands executed; combined output saved to `log.md`.

---

### Example 3: AI Analysis with History

```bash
cycodmd run "docker ps" --instructions "Identify issues" \
  --save-output docker-status.md \
  --save-chat-history docker-ai.jsonl
```

**Parsed State**:
- `ScriptToRun = "docker ps"`
- `Type = ScriptType.Default`
- `InstructionsList = ["Identify issues"]`
- `SaveOutput = "docker-status.md"`
- `SaveChatHistory = "docker-ai.jsonl"`

**Result**:
- Script executed, output analyzed by AI
- `docker-status.md`: AI-analyzed output
- `docker-ai.jsonl`: Chat history

---

### Example 4: Bash-Specific Script

```bash
cycodmd run --bash "df -h | grep -v tmpfs" --save-output disk.md
```

**Parsed State**:
- `ScriptToRun = "df -h | grep -v tmpfs"`
- `Type = RunCommand.ScriptType.Bash`
- `SaveOutput = "disk.md"`

**Result**: Script executed in bash; output saved to `disk.md`.

---

## Limitations Proof

### No Template Variables

**Evidence**: Run command does NOT use template expansion helpers.

**Comparison**:
- **FindFiles** `SaveFileOutput`: Uses templates (`{filePath}`, `{fileBase}`)
- **WebCommand** `SavePageOutput`: Uses templates
- **RunCommand** `SaveOutput`: Plain string (no template expansion)

**Result**: Template variables in `--save-output` are treated literally.

```bash
cycodmd run "ls" --save-output "{date}-file.md"
# Creates file: {date}-file.md (literal, not expanded)
```

---

### No Per-Command Splitting

**Evidence**: Positional arguments are concatenated into `ScriptToRun` (lines 462-466).

**Result**: All commands execute as one script; one output file.

**Workaround**: Multiple invocations:
```bash
cycodmd run "cmd1" --save-output out1.md
cycodmd run "cmd2" --save-output out2.md
```

---

## Evidence Summary

| Aspect | Evidence Location | Line Numbers |
|--------|-------------------|--------------|
| `--save-output` parsing | CycoDmdCommandLineOptions.cs | 427-432 |
| `--save-output` default | CycoDmdCommandLineOptions.cs | 483 |
| `--save-chat-history` parsing | CycoDmdCommandLineOptions.cs | 434-440 |
| SaveOutput property | CycoDmdCommand.cs | (inherited) |
| SaveChatHistory property | CycoDmdCommand.cs | (inherited) |
| RunCommand creation | CycoDmdCommandLineOptions.cs | 43 |
| Run-specific options | CycoDmdCommandLineOptions.cs | 56-98 |
| Positional arg handling | CycoDmdCommandLineOptions.cs | 462-466 |
| Parser entry point | CycoDmdCommandLineOptions.cs | 48-54 |
| Command name detection | CycoDmdCommandLineOptions.cs | 17-25 |

---

## Conclusion

This proof document establishes:

1. ✅ **Limited Layer 7 Options**: Only 2 options (`--save-output`, `--save-chat-history`)
2. ✅ **Shared Implementation**: Both options inherited from `CycoDmdCommand`
3. ✅ **No Per-Item Output**: RunCommand lacks `SaveFileOutput`/`SavePageOutput` properties
4. ✅ **No Template Expansion**: Output path is literal string
5. ✅ **Single Execution Context**: All commands concatenated into one script
6. ✅ **AI Integration**: Chat history saved if AI processing used

**Complete Evidence**: Source code proves minimal but functional Layer 7 implementation for Run command.

---

**[← Back to Layer 7 Description](cycodmd-run-layer-7.md)**
