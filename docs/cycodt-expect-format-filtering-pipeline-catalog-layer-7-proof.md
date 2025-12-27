# cycodt `expect format` - Layer 7: OUTPUT PERSISTENCE - Proof

**[← Back to Layer 7 Description](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md)**

## Source Code Evidence

This document provides detailed source code evidence for Layer 7 (Output Persistence) implementation in the `expect format` command.

---

## 1. Command Line Parser - Output Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

### Evidence: `--save-output` and `--output` parsing

```csharp
// Lines 26-56: TryParseExpectCommandOptions method (excerpts)
private bool TryParseExpectCommandOptions(ExpectBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--input")
    {
        // ... input parsing ...
        i += max1Arg.Count();
    }
    else if (arg == "--save-output" || arg == "--output")
    {
        // This applies to ALL ExpectBaseCommand subclasses
        // ExpectFormatCommand will use this; ExpectCheckCommand won't
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
        command.Output = output;
        i += max1Arg.Count();
    }
    else if (command is ExpectFormatCommand formatCommand && arg == "--strict")
    {
        // ... strict mode parsing ...
        i += max1Arg.Count();
    }
    // ... other options ...
    
    return parsed;
}
```

**Key Observations**:
- **Line 17**: Both `--save-output` and `--output` trigger this branch
- **Line 18**: Comment indicates this is for ALL `ExpectBaseCommand` types
- **Line 20**: Validate output path is not empty
- **Line 21**: Set `Output` property on the command
- **ExpectFormatCommand will use this** (proven below)

---

## 2. ExpectBaseCommand - Output Property and WriteOutput Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

### Evidence: Base class provides Output property

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
- **Line 10**: `Output` property (nullable string)
- **Line 6**: Initialized to `null` in constructor
- **Lines 31-41**: `WriteOutput()` method provides conditional output logic:
  - **Line 33**: If `Output` is null or empty → write to console
  - **Line 35**: `ConsoleHelpers.WriteLine()` with `overrideQuiet: true` (always show)
  - **Line 39**: If `Output` is set → write to file
  - **Line 39**: `FileHelpers.WriteAllText()` overwrites file

---

## 3. ExpectFormatCommand - Uses WriteOutput

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

### Evidence: ExpectFormatCommand calls WriteOutput()

```csharp
// Lines 1-79: Complete ExpectFormatCommand class (excerpts)
class ExpectFormatCommand : ExpectBaseCommand
{
    public ExpectFormatCommand() : base()
    {
        Strict = true; // Default to true
    }

    public bool Strict { get; set; }

    public override string GetCommandName()
    {
        return "expect format";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteFormat());
    }

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

    // ... FormatInput() and FormatLine() methods ...
}
```

**Key Observations**:
- **Line 27**: Read input from file (or stdin if Input is "-")
- **Line 28**: Format the input text
- **Line 29**: **Call `WriteOutput(formattedText)`** (inherited from base class)
  - This is the KEY LINE for Layer 7
  - `WriteOutput()` respects the `Output` property
  - If `Output` is set → writes to file
  - If `Output` is null → writes to console
- **Line 30**: Return 0 (success)

**This is the ONLY place** where output persistence happens for `expect format`.

---

## 4. File Writing Logic (from WriteOutput)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

### Evidence: FileHelpers.WriteAllText() writes to file

```csharp
// Lines 31-41: WriteOutput method
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
```

**File Writing Details**:
- **Line 39**: `FileHelpers.WriteAllText(Output, text)`
  - `Output`: File path (from `--save-output` or `--output` option)
  - `text`: Formatted output string
  - **Behavior**: Overwrites file if exists, creates if doesn't exist

**From FileHelpers implementation** (common helper):
```csharp
public static void WriteAllText(string path, string contents)
{
    File.WriteAllText(path, contents);
}
```

**Standard File.WriteAllText() behavior**:
- Creates file if it doesn't exist
- Overwrites file if it exists
- Throws exception if directory doesn't exist

---

## 5. Console Output Logic (from WriteOutput)

### Evidence: ConsoleHelpers.WriteLine() writes to stdout

```csharp
// Lines 31-41: WriteOutput method
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
```

**Console Writing Details**:
- **Line 35**: `ConsoleHelpers.WriteLine(text, overrideQuiet: true)`
  - `text`: Formatted output string
  - `overrideQuiet: true`: Always show output even if `--quiet` is set
  - **Behavior**: Writes to stdout

**Why `overrideQuiet: true`?**
- Formatted output is the PRIMARY RESULT of the command
- Must be shown even in quiet mode
- Similar to how `--save-output` always writes file regardless of quiet mode

---

## 6. Data Flow Summary

### Flow Diagram: File Output

```
User command line:
  cycodt expect format --input test.txt --save-output out.txt

    ↓ (parsed by CycoDtCommandLineOptions.cs:17-23)

ExpectFormatCommand properties:
  Input = "test.txt"
  Output = "out.txt"
  Strict = true (default)

    ↓ (ExecuteAsync() calls ExecuteFormat())

ExecuteFormat():
  Line 27: input = FileHelpers.ReadAllText("test.txt")
  Line 28: formattedText = FormatInput(input)
  Line 29: WriteOutput(formattedText)
  
    ↓ (WriteOutput() checks Output property)
    
WriteOutput():
  Line 33: Output != null ("out.txt")
  Line 39: FileHelpers.WriteAllText("out.txt", formattedText)
  
    ↓ (File.WriteAllText() called)
    
File System:
  File "out.txt" created/overwritten with formattedText
```

---

### Flow Diagram: Console Output

```
User command line:
  cycodt expect format --input test.txt

    ↓ (no --save-output, Output remains null)

ExpectFormatCommand properties:
  Input = "test.txt"
  Output = null
  Strict = true (default)

    ↓ (ExecuteAsync() calls ExecuteFormat())

ExecuteFormat():
  Line 27: input = FileHelpers.ReadAllText("test.txt")
  Line 28: formattedText = FormatInput(input)
  Line 29: WriteOutput(formattedText)
  
    ↓ (WriteOutput() checks Output property)
    
WriteOutput():
  Line 33: Output == null (empty)
  Line 35: ConsoleHelpers.WriteLine(formattedText, overrideQuiet: true)
  
    ↓ (Console.WriteLine() called)
    
stdout:
  formattedText printed to console
```

---

## 7. Test Case: File Output

**Command**: `echo "Hello" | cycodt expect format --save-output out.txt`

**Code Path**:
1. **Line 6** (ExpectBaseCommand.cs): `Output = null` (constructor)
2. **Line 21** (CycoDtCommandLineOptions.cs): `Output = "out.txt"` (parsed)
3. **Line 25** (ExpectBaseCommand.cs): `Input = "-"` (stdin detected)
4. **Line 27** (ExpectFormatCommand.cs): Read from stdin: "Hello"
5. **Line 28** (ExpectFormatCommand.cs): Format input: `"^Hello\\r?$\n"`
6. **Line 29** (ExpectFormatCommand.cs): Call `WriteOutput("^Hello\\r?$\n")`
7. **Line 33** (ExpectBaseCommand.cs): `Output != null` ("out.txt")
8. **Line 39** (ExpectBaseCommand.cs): `FileHelpers.WriteAllText("out.txt", "^Hello\\r?$\n")`
9. **File System**: `out.txt` created with content `"^Hello\\r?$\n"`

**Result**: File `out.txt` contains `^Hello\\r?$\n`

---

## 8. Test Case: Console Output

**Command**: `echo "Hello" | cycodt expect format`

**Code Path**:
1. **Line 6** (ExpectBaseCommand.cs): `Output = null` (constructor)
2. **No `--save-output` parsing**: `Output` remains null
3. **Line 25** (ExpectBaseCommand.cs): `Input = "-"` (stdin detected)
4. **Line 27** (ExpectFormatCommand.cs): Read from stdin: "Hello"
5. **Line 28** (ExpectFormatCommand.cs): Format input: `"^Hello\\r?$\n"`
6. **Line 29** (ExpectFormatCommand.cs): Call `WriteOutput("^Hello\\r?$\n")`
7. **Line 33** (ExpectBaseCommand.cs): `Output == null`
8. **Line 35** (ExpectBaseCommand.cs): `ConsoleHelpers.WriteLine("^Hello\\r?$\n", overrideQuiet: true)`
9. **stdout**: `^Hello\\r?$\n` printed

**Result**: Console displays `^Hello\\r?$\n`

---

## 9. Test Case: Using `--output` Alias

**Command**: `cycodt expect format --input in.txt --output out.txt`

**Code Path**:
1. **Line 17** (CycoDtCommandLineOptions.cs): `arg == "--output"` → true (alias)
2. **Line 21** (CycoDtCommandLineOptions.cs): `Output = "out.txt"` (same as `--save-output`)
3. **Rest of flow**: Identical to `--save-output` case

**Result**: File `out.txt` created (same behavior as `--save-output`)

---

## 10. Comparison with ExpectCheckCommand (No Output)

### ExpectCheckCommand (No File Output)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

```csharp
// Lines 31-63: ExecuteCheck method (excerpts)
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

        // ... more validation ...

        ConsoleHelpers.WriteLine($"\r{message} PASS!");
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
- **ExpectCheckCommand**: Never calls `WriteOutput()`, only uses `ConsoleHelpers.WriteLine()`
- **ExpectFormatCommand**: Calls `WriteOutput()` which enables file output

---

## 11. Why ExpectFormatCommand Uses WriteOutput

### Design Rationale

**ExpectFormatCommand Purpose**:
- Generate formatted expectations for use in YAML tests
- Output MUST be persistable (copied into YAML files)
- Supports both console output (for preview) and file output (for storage)

**Implementation Strategy**:
- Base class provides `WriteOutput()` abstraction
- Subclasses decide whether to use it
- ExpectFormatCommand: YES (generates reusable output)
- ExpectCheckCommand: NO (only validates, no reusable output)

---

## Summary of Evidence

| Aspect | Evidence Location | Finding |
|--------|------------------|---------|
| Command-line parsing | `CycoDtCommandLineOptions.cs:17-23` | `--save-output` and `--output` set `Output` property |
| Output property | `ExpectBaseCommand.cs:10` | Base class provides nullable `Output` property |
| WriteOutput method | `ExpectBaseCommand.cs:31-41` | Conditional file/console output based on `Output` |
| File writing | `ExpectBaseCommand.cs:39` | `FileHelpers.WriteAllText()` writes to file |
| Console writing | `ExpectBaseCommand.cs:35` | `ConsoleHelpers.WriteLine()` writes to stdout |
| ExpectFormatCommand usage | `ExpectFormatCommand.cs:29` | **Calls `WriteOutput()`** (key line) |
| File behavior | FileHelpers implementation | Overwrites file if exists, creates if not |
| overrideQuiet flag | `ExpectBaseCommand.cs:35` | Always show output even in quiet mode |
| Alias support | `CycoDtCommandLineOptions.cs:17` | Both `--save-output` and `--output` work |

**Conclusion**: The `expect format` command has **full native file output capabilities** via `--save-output` or `--output` options. The `WriteOutput()` method in the base class provides clean abstraction for conditional output destination (file or console).

---

**[← Back to Layer 7 Description](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md)** | **[↑ Back to expect format Command Catalog](cycodt-expect-format-catalog-README.md)**
