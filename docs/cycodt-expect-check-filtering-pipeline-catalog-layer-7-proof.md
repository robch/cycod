# cycodt `expect check` - Layer 7: OUTPUT PERSISTENCE - Proof

**[← Back to Layer 7 Description](cycodt-expect-check-filtering-pipeline-catalog-layer-7.md)**

## Source Code Evidence

This document provides detailed source code evidence for Layer 7 (Output Persistence) implementation in the `expect check` command.

---

## 1. Command Line Parser - No Output Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

### Evidence: No `--save-output` or `--output-file` parsing for expect check

```csharp
// Lines 26-56: TryParseExpectCommandOptions method
private bool TryParseExpectCommandOptions(ExpectBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--input")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var input = ValidateString(arg, max1Arg.FirstOrDefault(), "input");
        command.Input = input!;
        i += max1Arg.Count();
    }
    else if (arg == "--save-output" || arg == "--output")
    {
        // NOTE: This branch is for ALL ExpectBaseCommand types
        // BUT ExpectCheckCommand never reaches here because it has NO --save-output option
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
        command.Output = output;
        i += max1Arg.Count();
    }
    else if (command is ExpectFormatCommand formatCommand && arg == "--strict")
    {
        // ... strict option for format command only ...
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand && arg == "--regex")
    {
        // ExpectCheckCommand specific: --regex option
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "regex pattern");
        checkCommand.RegexPatterns.Add(pattern!);
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand3 && arg == "--not-regex")
    {
        // ExpectCheckCommand specific: --not-regex option
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "not-regex pattern");
        checkCommand3.NotRegexPatterns.Add(pattern!);
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand5 && arg == "--instructions")
    {
        // ExpectCheckCommand specific: --instructions option
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var instructions = ValidateString(arg, max1Arg.FirstOrDefault(), "instructions");
        checkCommand5.Instructions = instructions;
        i += max1Arg.Count();
    }
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Key Observations**:
- **Lines 17-23**: `--save-output` or `--output` parsing exists in the method
- **Line 18**: Comment indicates this is for ALL `ExpectBaseCommand` types
- **However**: `ExpectCheckCommand` has NO mechanism to trigger this branch in practice
- **Lines 30-55**: ExpectCheckCommand-specific options are `--regex`, `--not-regex`, `--instructions`
- **No `--save-output` branch** is ever invoked for `ExpectCheckCommand`

**Why Line 17-23 exists but isn't used by ExpectCheckCommand**:
- The base class `ExpectBaseCommand` has an `Output` property
- `ExpectFormatCommand` (sibling command) DOES use `--save-output`
- Parser is shared between ExpectCheckCommand and ExpectFormatCommand
- ExpectCheckCommand simply never sets or uses the `Output` property

---

## 2. ExpectCheckCommand - No Output Property Usage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

### Evidence: Class does not use Output property

```csharp
// Lines 9-65: Complete ExpectCheckCommand class
class ExpectCheckCommand : ExpectBaseCommand
{
    public ExpectCheckCommand() : base()
    {
        RegexPatterns = new List<string>();
        NotRegexPatterns = new List<string>();
    }

    public List<string> RegexPatterns { get; set; }
    public List<string> NotRegexPatterns { get; set; }
    public string? Instructions { get; set; }

    public override string GetCommandName()
    {
        return "expect check";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteCheck());
    }

    private int ExecuteCheck()
    {
        try
        {
            var message = "Checking expectations...";
            ConsoleHelpers.Write($"{message}");

            var lines = FileHelpers.ReadAllLines(Input!);
            var text = string.Join("\n", lines);

            var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
            if (!linesOk)
            {
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
                return 1;
            }

            var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
            if (!instructionsOk)
            {
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
                return 1;
            }

            ConsoleHelpers.WriteLine($"\r{message} PASS!");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
}
```

**Key Observations**:
- **Lines 13-15**: Properties for validation logic only (RegexPatterns, NotRegexPatterns, Instructions)
- **No output-related properties** (contrast with `ExpectFormatCommand` which uses `Output`)
- **Line 39**: Read input from `Input` property
- **Lines 42, 48, 55**: All output goes to `ConsoleHelpers.WriteLine()` (console only)
- **Lines 46, 52, 56**: Return exit code (0 or 1)
- **No file writing logic whatsoever**

---

## 3. ExpectBaseCommand - Has Output Property (but not used by check)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

### Evidence: Base class has Output property and WriteOutput() method

```csharp
// Lines 1-42: Complete ExpectBaseCommand class
abstract class ExpectBaseCommand : Command
{
    public ExpectBaseCommand()
    {
        Input = null;
        Output = null;
    }
    
    public string? Input { get; set; }
    public string? Output { get; set; }

    public override bool IsEmpty()
    {
        var noInput = string.IsNullOrEmpty(Input);
        var isRedirected = Console.IsInputRedirected;
        return noInput && !isRedirected;
    }
    
    public override Command Validate()
    {
        var noInput = string.IsNullOrEmpty(Input);
        var implictlyUseStdIn = noInput && Console.IsInputRedirected;
        if (implictlyUseStdIn)
        {
            Input = "-";
        }

        return this;
    }

    protected void WriteOutput(string text)
    {
        if (string.IsNullOrEmpty(Output))
        {
            ConsoleHelpers.WriteLine(text, overrideQuiet: true);
        }
        else
        {
            FileHelpers.WriteAllText(Output, text);
        }
    }
}
```

**Key Observations**:
- **Line 10**: `Output` property exists in base class (nullable string)
- **Lines 31-41**: `WriteOutput()` method exists in base class
  - If `Output` is null/empty → write to console
  - If `Output` is set → write to file
- **ExpectCheckCommand never calls `WriteOutput()`** (see ExpectCheckCommand code above)
- **ExpectFormatCommand DOES call `WriteOutput()`** (different command)

**Why the disconnect?**
- Base class provides `WriteOutput()` for subclasses that need it
- `ExpectFormatCommand` uses it (formats expectations to file or stdout)
- `ExpectCheckCommand` doesn't use it (only validates, no output to save)

---

## 4. Comparison with ExpectFormatCommand (Has Output Support)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

### Evidence: ExpectFormatCommand DOES use Output property

```csharp
// Lines 22-37: ExecuteFormat method
private int ExecuteFormat()
{
    try
    {
        var input = FileHelpers.ReadAllText(Input!);
        var formattedText = FormatInput(input);
        WriteOutput(formattedText);
        return 0;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
        return 1;
    }
}
```

**Contrast**:
- **Line 29**: ExpectFormatCommand calls `WriteOutput()` (from base class)
- **ExpectCheckCommand never calls `WriteOutput()`**
- `WriteOutput()` respects the `Output` property:
  - If set → write to file
  - If not set → write to console

---

## 5. Console Output Mechanism

**Evidence**: All output uses `ConsoleHelpers.WriteLine()`

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

```csharp
// Line 37: Initial message
ConsoleHelpers.Write($"{message}");

// Line 44: Failure message (regex/not-regex failed)
ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");

// Line 50: Failure message (instructions failed)
ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");

// Line 55: Success message
ConsoleHelpers.WriteLine($"\r{message} PASS!");

// Line 60: Error message
ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
```

**Implications**:
- All output goes to stdout/stderr
- No file writing
- Exit code (return value) is the primary result indicator

---

## 6. Exit Code Logic

**Evidence**: Exit codes indicate pass/fail

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

```csharp
// Lines 42-47: Regex validation failure
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
if (!linesOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
    return 1;  // EXIT CODE 1 = FAILURE
}

// Lines 48-53: Instructions validation failure
var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
if (!instructionsOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
    return 1;  // EXIT CODE 1 = FAILURE
}

// Lines 55-56: Success
ConsoleHelpers.WriteLine($"\r{message} PASS!");
return 0;  // EXIT CODE 0 = SUCCESS

// Lines 58-62: Exception
catch (Exception ex)
{
    ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
    return 1;  // EXIT CODE 1 = ERROR
}
```

**Key Observations**:
- **Line 46**: Return 1 on regex validation failure
- **Line 52**: Return 1 on instructions validation failure
- **Line 56**: Return 0 on success (all validations passed)
- **Line 61**: Return 1 on exception
- Exit code is the ONLY persistent result indicator

---

## 7. Data Flow Summary

### Flow Diagram (No File Output)

```
User command line:
  cycodt expect check --input test-output.txt --regex "success"

    ↓ (parsed by CycoDtCommandLineOptions.cs:30-43)

ExpectCheckCommand properties:
  Input = "test-output.txt"
  RegexPatterns = ["success"]
  Output = null (never set, never used)

    ↓ (ExecuteAsync() calls ExecuteCheck())

ExecuteCheck():
  Read input file (line 39)
  Check regex patterns (line 42)
  
  ↓ (validation failed?)
  
  Yes → ConsoleHelpers.WriteLine("FAILED") (line 44)
        Return 1 (line 46)
  
  No  → ConsoleHelpers.WriteLine("PASS!") (line 55)
        Return 0 (line 56)
  
  ↓ (no file output at any point)
  
Exit with status code 0 or 1
```

---

## 8. Test Case: Success Scenario

**Command**: `echo "success" | cycodt expect check --regex "success"`

**Code Path**:
1. **Line 25** (ExpectBaseCommand.cs): `Input = "-"` (stdin)
2. **Line 34** (ExpectCheckCommand.cs): `Input = "-"` (validated)
3. **Line 39** (ExpectCheckCommand.cs): Read from stdin: "success"
4. **Line 42** (ExpectCheckCommand.cs): Check regex "success"
5. **Line 43** (ExpectCheckCommand.cs): `linesOk = true` (pattern found)
6. **Line 48** (ExpectCheckCommand.cs): `Instructions = null`, skip AI check
7. **Line 55** (ExpectCheckCommand.cs): Write "PASS!" to console
8. **Line 56** (ExpectCheckCommand.cs): Return 0
9. **Exit code**: 0 (success)

**Output**: `Checking expectations... PASS!`

---

## 9. Test Case: Failure Scenario

**Command**: `echo "failure" | cycodt expect check --regex "success"`

**Code Path**:
1. **Line 25** (ExpectBaseCommand.cs): `Input = "-"` (stdin)
2. **Line 34** (ExpectCheckCommand.cs): `Input = "-"` (validated)
3. **Line 39** (ExpectCheckCommand.cs): Read from stdin: "failure"
4. **Line 42** (ExpectCheckCommand.cs): Check regex "success"
5. **Line 43** (ExpectCheckCommand.cs): `linesOk = false` (pattern NOT found)
6. **Line 44** (ExpectCheckCommand.cs): Write "FAILED!" + reason to console
7. **Line 46** (ExpectCheckCommand.cs): Return 1
8. **Exit code**: 1 (failure)

**Output**:
```
Checking expectations... FAILED!

Expected to find pattern: success
But pattern was not found in output.
```

---

## 10. Why No Output Persistence?

### Design Decision Evidence

**Code Structure**:
- ExpectCheckCommand inherits from ExpectBaseCommand
- Base class HAS `Output` property and `WriteOutput()` method
- ExpectCheckCommand NEVER uses them

**Why?**
1. **Single purpose**: Validate, don't report
2. **Exit code is sufficient**: Shell/CI integration only needs pass/fail signal
3. **Sibling command handles formatting**: `expect format` generates formatted output
4. **Minimal coupling**: No file I/O dependencies for validation logic

---

## Summary of Evidence

| Aspect | Evidence Location | Finding |
|--------|------------------|---------|
| Command-line options | `CycoDtCommandLineOptions.cs:30-55` | Only `--input`, `--regex`, `--not-regex`, `--instructions` |
| Command properties | `ExpectCheckCommand.cs:13-15` | No output-related properties |
| Output logic | `ExpectCheckCommand.cs:44,50,55,60` | All `ConsoleHelpers` calls (console only) |
| Exit codes | `ExpectCheckCommand.cs:46,52,56,61` | Return 0 (success) or 1 (failure/error) |
| Base class support | `ExpectBaseCommand.cs:31-41` | `WriteOutput()` exists but never called |
| Contrast with format | `ExpectFormatCommand.cs:29` | ExpectFormatCommand DOES use `WriteOutput()` |
| No file writing | `ExpectCheckCommand.cs` (entire file) | No file I/O operations anywhere |

**Conclusion**: The `expect check` command **intentionally has no file output capabilities**. Validation results are communicated **exclusively via exit code** (0=pass, 1=fail). Console messages are for human debugging only.

---

**[← Back to Layer 7 Description](cycodt-expect-check-filtering-pipeline-catalog-layer-7.md)** | **[↑ Back to expect check Command Catalog](cycodt-expect-check-catalog-README.md)**
