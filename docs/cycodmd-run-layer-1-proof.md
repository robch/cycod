# cycodmd Run - Layer 1: Target Selection - PROOF

This document provides source code evidence for all Layer 1 (Target Selection) features of the cycodmd run command.

---

## Positional Script Lines

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 462-465**:
```csharp
462:         else if (command is RunCommand runCommand)
463:         {
464:             runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
465:             parsedOption = true;
466:         }
```

**Explanation**: 
- Method `TryParseOtherCommandArg()` handles non-option arguments
- For `RunCommand`, each argument is appended to `ScriptToRun` with a newline separator
- Multiple arguments build a multi-line script: `cycodmd run "line1" "line2"` → `"line1\nline2"`
- `.Trim()` removes leading/trailing whitespace

### Command Property

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 14-16, 34**:
```csharp
14:     public RunCommand() : base()
15:     {
16:         ScriptToRun = string.Empty;
17:         Type = ScriptType.Default;
18:     }
...
34:     public string ScriptToRun { get; set; }
```

**Explanation**: 
- `ScriptToRun` is initialized as empty string in the constructor
- Stores the complete script content (potentially multi-line)

### Empty Check

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 24-27**:
```csharp
24:     override public bool IsEmpty()
25:     {
26:         return string.IsNullOrWhiteSpace(ScriptToRun);
27:     }
```

**Explanation**: 
- Command is considered empty if `ScriptToRun` is null, empty, or whitespace
- Validation will fail if no script content is provided

---

## `--script` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 64-70**:
```csharp
64:         else if (arg == "--script")
65:         {
66:             var scriptArgs = GetInputOptionArgs(i + 1, args);
67:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
68:             command.Type = RunCommand.ScriptType.Default;
69:             i += scriptArgs.Count();
70:         }
```

**Explanation**: 
- Reads all arguments following `--script` (until next option or end of args)
- Calls `ValidateJoinedString()` to:
  - Join the new script args with existing `ScriptToRun` content
  - Use `"\n"` as the separator (newline)
  - Validate that the result is not empty
- Sets `Type = ScriptType.Default` (platform-dependent shell)

### Validation Helper

**File**: `src/common/CommandLine/CommandLineOptions.cs`

Referenced in parser (base class method):
```csharp
protected static string ValidateJoinedString(string arg, string seed, IEnumerable<string> values, string separator, string argDescription)
{
    seed = string.Join(separator, values.Prepend(seed)).Trim();
    if (string.IsNullOrEmpty(seed))
    {
        throw new CommandLineException($"Missing {argDescription} for {arg}");
    }
    return seed;
}
```

**Explanation**: 
- Prepends existing `seed` (current script content) to new values
- Joins with separator (newline)
- Trims whitespace
- Throws exception if result is empty

---

## `--cmd` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 71-77**:
```csharp
71:         else if (arg == "--cmd")
72:         {
73:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
74:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
75:             command.Type = RunCommand.ScriptType.Cmd;
76:             i += scriptArgs.Count();
77:         }
```

**Explanation**: 
- Similar to `--script`, but `GetInputOptionArgs(i + 1, args, 1)` limits to 1 argument (max: 1)
- Sets `Type = ScriptType.Cmd` to force Windows Command Prompt
- Script content is joined with newline separator

---

## `--bash` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 78-84**:
```csharp
78:         else if (arg == "--bash")
79:         {
80:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
81:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
82:             command.Type = RunCommand.ScriptType.Bash;
83:             i += scriptArgs.Count();
84:         }
```

**Explanation**: 
- Similar to `--cmd`
- Limits to 1 argument (max: 1)
- Sets `Type = ScriptType.Bash` to force Bash shell

---

## `--powershell` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 85-91**:
```csharp
85:         else if (arg == "--powershell")
86:         {
87:             var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
88:             command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
89:             command.Type = RunCommand.ScriptType.PowerShell;
90:             i += scriptArgs.Count();
91:         }
```

**Explanation**: 
- Similar to `--cmd` and `--bash`
- Limits to 1 argument (max: 1)
- Sets `Type = ScriptType.PowerShell` to force PowerShell

---

## Script Type Enum

### Definition

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 5-11**:
```csharp
5:     public enum ScriptType
6:     {
7:         Default, // Uses cmd on Windows, bash on Linux/Mac
8:         Cmd,
9:         Bash,
10:         PowerShell
11:     }
```

**Explanation**: 
- Enum defines four possible script execution types
- `Default` is platform-dependent:
  - Windows → cmd.exe
  - Linux/Mac → bash

### Command Property

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 17, 35**:
```csharp
17:         Type = ScriptType.Default;
...
35:     public ScriptType Type { get; set; }
```

**Explanation**: 
- `Type` property stores which shell to use
- Defaults to `ScriptType.Default` in constructor

---

## Validation

### No Special Validation

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 29-32**:
```csharp
29:     override public CycoDmdCommand Validate()
30:     {
31:         return this;
32:     }
```

**Explanation**: 
- No special validation logic for RunCommand
- The empty check in `IsEmpty()` is sufficient

---

## Command Inheritance Hierarchy

### RunCommand Inheritance

```
Command (base class)
    ↓
CycoDmdCommand (cycodmd-specific base)
    ↓
RunCommand (script execution)
```

**File**: `src/cycodmd/CommandLineCommands/RunCommand.cs`

**Lines 3-17**:
```csharp
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
```

**Explanation**: 
- Extends `CycoDmdCommand`, so it inherits shared cycodmd options (like `--instructions`, `--save-output`)
- Defines its own `ScriptType` enum for shell selection

---

## Parsing Behavior Differences

### Option Argument Limits

| Option | Max Arguments | Reason |
|--------|--------------|--------|
| `--script` | Unlimited | Allows multi-line scripts: `--script "line1" "line2" "line3"` |
| `--cmd` | 1 | Single command for cmd.exe |
| `--bash` | 1 | Single command for bash |
| `--powershell` | 1 | Single command for PowerShell |
| Positional args | Unlimited | Each arg becomes a line in the script |

**Code Evidence**:

`--script` uses:
```csharp
var scriptArgs = GetInputOptionArgs(i + 1, args);  // No max limit
```

`--cmd`, `--bash`, `--powershell` use:
```csharp
var scriptArgs = GetInputOptionArgs(i + 1, args, 1);  // max: 1
```

---

## Script Accumulation

### Multiple Options Can Accumulate

Because `ValidateJoinedString()` prepends existing content, users can combine options:

```bash
cycodmd run --script "line1" --script "line2" --bash "line3"
```

This would result in:
- ScriptToRun = "line1\nline2\nline3"
- Type = ScriptType.Bash (last one wins)

**Code Evidence**:

Each call to `ValidateJoinedString()` includes the current value:
```csharp
command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
//                                              ^^^^^^^^^^^^^^^^^^^
//                                              existing content
```

---

## Summary of Evidence

| Feature | Parser Location | Command Property | Default Value |
|---------|----------------|------------------|---------------|
| Positional script lines | CycoDmdCommandLineOptions.cs:462-465 | RunCommand.ScriptToRun | string.Empty |
| Script type | Multiple locations | RunCommand.Type | ScriptType.Default |
| `--script` | CycoDmdCommandLineOptions.cs:64-70 | ScriptToRun, Type=Default | - |
| `--cmd` | CycoDmdCommandLineOptions.cs:71-77 | ScriptToRun, Type=Cmd | - |
| `--bash` | CycoDmdCommandLineOptions.cs:78-84 | ScriptToRun, Type=Bash | - |
| `--powershell` | CycoDmdCommandLineOptions.cs:85-91 | ScriptToRun, Type=PowerShell | - |
| Validation | RunCommand.cs:29-32 | N/A | No special validation |

---

## Related Files

- **RunCommand**: `src/cycodmd/CommandLineCommands/RunCommand.cs` - Run command implementation
- **CycoDmdCommand**: `src/cycodmd/CommandLineCommands/CycoDmdCommand.cs` - Base class for cycodmd commands
- **CommandLineOptions**: `src/common/CommandLine/CommandLineOptions.cs` - Base parser functionality
