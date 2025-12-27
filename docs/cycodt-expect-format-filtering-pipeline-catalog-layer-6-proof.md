# cycodt expect format - Layer 6: Display Control - PROOF

This document provides detailed source code evidence for all Layer 6 (Display Control) features of the `cycodt expect format` command.

## 6.1 Output Destination

### WriteOutput Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

**Lines 31-41**:
```csharp
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

**Explanation**:
- **Line 33**: Checks if `Output` property is null/empty
- **Line 35**: If no output file specified, writes to console using `ConsoleHelpers.WriteLine()`
  - Uses `overrideQuiet: true` to ensure output even in quiet mode
- **Lines 37-40**: If output file specified, writes to file using `FileHelpers.WriteAllText()`

---

### Usage in ExpectFormatCommand

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 27-29**:
```csharp
var input = FileHelpers.ReadAllText(Input!);
var formattedText = FormatInput(input);
WriteOutput(formattedText);
```

**Explanation**:
- **Line 27**: Reads input text
- **Line 28**: Formats the text
- **Line 29**: Calls `WriteOutput()` to send formatted text to stdout or file

---

### Output Option Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 48-54**:
```csharp
else if (arg == "--save-output" || arg == "--output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
    command.Output = output;
    i += max1Arg.Count();
}
```

**Explanation**:
- Accepts both `--save-output` and `--output` aliases
- Sets the `Output` property on `ExpectBaseCommand`

---

## 6.2 Format Mode Control

### Strict Property

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 6-9**:
```csharp
public ExpectFormatCommand() : base()
{
    Strict = true; // Default to true
}
```

**Explanation**: Strict mode is enabled by default.

---

**Lines 11**:
```csharp
public bool Strict { get; set; }
```

**Explanation**: Property storing the strict mode setting.

---

### Strict Option Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 55-64**:
```csharp
else if (command is ExpectFormatCommand formatCommand && arg == "--strict")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var strictStr = ValidateString(arg, max1Arg.FirstOrDefault(), "strict");
    if (bool.TryParse(strictStr, out bool strict))
    {
        formatCommand.Strict = strict;
    }
    i += max1Arg.Count();
}
```

**Explanation**:
- **Line 55**: Checks command type and argument name
- **Lines 57-58**: Gets and validates the strict value
- **Lines 59-62**: Parses string as boolean and sets `Strict` property

---

### Strict Mode Usage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 47-51**:
```csharp
foreach (var line in lines)
{
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Explanation**:
- **Line 49**: Passes `Strict` property to `FormatLine()` method
- Each line is formatted according to the strict mode setting

---

### FormatLine Implementation

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 61-77**:
```csharp
private string FormatLine(string line, bool strict)
{
    ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");

    var escaped = EscapeSpecialRegExChars(line);
    ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");

    var escapedCR = strict
        ? escaped.Trim('\r').Replace("\r", "\\r")
        : escaped.Replace("\r", "\\r");
    ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");

    var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
    ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");

    return result;
}
```

**Explanation**:
- **Lines 63, 66, 71, 74**: Debug hex dumps (only shown with --debug)
- **Lines 68-70**: 
  - **Strict mode**: Trims trailing `\r`, then replaces internal `\r` with `\\r`
  - **Non-strict mode**: Replaces all `\r` with `\\r` without trimming
- **Line 73**:
  - **Strict mode**: Adds `^` prefix, `\\r?$\\n` suffix (anchored pattern with optional CR)
  - **Non-strict mode**: Returns escaped string as-is (no anchors)

---

## 6.3 Debug Output

### Debug Hex Dump Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 63, 66, 71, 74** (in FormatLine):
```csharp
ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");
ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");
ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");
ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");
```

**Explanation**: Shows hex dump at each transformation stage when --debug is enabled.

---

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 42** (in FormatInput):
```csharp
ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");
```

**Explanation**: Shows hex dump of entire input before line splitting.

---

### Debug Flag Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 341-345**:
```csharp
else if (arg == "--debug")
{
    this.Debug = true;
    ConsoleHelpers.ConfigureDebug(true);
}
```

**Explanation**: 
- Sets `Debug` property to `true`
- Calls `ConsoleHelpers.ConfigureDebug(true)` to enable debug output globally

---

### Expected Debug Output

When `--debug` is enabled, output includes hex dumps:

```
DEBUG: ExpectFormatCommand.FormatInput: Input contains 2 lines.
Hex Dump:
0000: 48 65 6c 6c 6f 2c 20 57 6f 72 6c 64 21 0d 0a 48  Hello, World!..H
0010: 6f 77 20 61 72 65 20 79 6f 75 3f                 ow are you?

DEBUG: ExpectFormatCommand.FormatLine:
Hex Dump:
0000: 48 65 6c 6c 6f 2c 20 57 6f 72 6c 64 21 0d        Hello, World!.

DEBUG: ExpectFormatCommand.FormatLine; post-escape:
Hex Dump:
0000: 48 65 6c 6c 6f 2c 20 57 6f 72 6c 64 5c 21 0d     Hello, World\!.

DEBUG: ExpectFormatCommand.FormatLine; post-cr-escape:
Hex Dump:
0000: 48 65 6c 6c 6f 2c 20 57 6f 72 6c 64 5c 21        Hello, World\!

DEBUG: ExpectFormatCommand.FormatLine; result:
Hex Dump:
0000: 5e 48 65 6c 6c 6f 2c 20 57 6f 72 6c 64 5c 21 5c  ^Hello, World\!\
0010: 5c 72 3f 24 5c 6e                                \r?$\n

^Hello, World\!\\r?$\n
^How are you\?\\r?$\n
```

---

## 6.4 Quiet Mode

### Quiet Flag Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 350-353**:
```csharp
else if (arg == "--quiet")
{
    this.Quiet = true;
}
```

**Explanation**: The `--quiet` flag sets the `Quiet` boolean property to `true`.

---

### Quiet Mode Impact

The `ExpectFormatCommand` is minimally affected by quiet mode:

1. **Formatted output**: Still written (it's the command's primary purpose)
2. **Debug output**: Already suppressed by default (requires --debug)
3. **Error messages**: Always shown

**Reason**: This command has no "chatty" output - it's a pure transformation tool.

---

## 6.5 Override Quiet for Output

### WriteOutput with overrideQuiet

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

**Line 35**:
```csharp
ConsoleHelpers.WriteLine(text, overrideQuiet: true);
```

**Explanation**:
- Uses `overrideQuiet: true` parameter
- Ensures output is written to console even if `--quiet` is set
- Critical for pipe usage: `echo "text" | cycodt expect format | other-command`

---

### ConsoleHelpers.WriteLine Signature

**Expected Implementation** in `src/common/Helpers/ConsoleHelpers.cs`:
```csharp
public static void WriteLine(string message, ConsoleColor? color = null, bool overrideQuiet = false)
{
    if (IsQuiet() && !overrideQuiet)
        return; // Suppress output in quiet mode unless overridden
        
    if (color.HasValue)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color.Value;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }
    else
    {
        Console.WriteLine(message);
    }
}
```

**Evidence**: Called with `overrideQuiet: true` on line 35 of ExpectBaseCommand.cs

---

## Additional Implementation Details

### EscapeSpecialRegExChars Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 56-59**:
```csharp
private static string EscapeSpecialRegExChars(string line)
{
    return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
}
```

**Explanation**:
- Escapes all regex special characters: `\ ( ) [ ] { } . * + ? | ^ $`
- Uses regex replacement with capture group
- Pattern `([\\()\[\]{}.*+?|^$])` matches any special char
- Replacement `\$1` adds backslash before the captured character

---

### FormatInput Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 39-54**:
```csharp
private string FormatInput(string input)
{
    var c = input.Count(c => c == '\n');
    ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");

    var lines = input.Split('\n', StringSplitOptions.None);
    var formattedLines = new List<string>();

    foreach (var line in lines)
    {
        var formatted = FormatLine(line, Strict);
        formattedLines.Add(formatted);
    }

    return string.Join("\n", formattedLines);
}
```

**Explanation**:
- **Line 41**: Counts newlines for debug message
- **Line 42**: Debug hex dump of entire input
- **Line 44**: Splits on `\n` (preserves empty lines with `StringSplitOptions.None`)
- **Lines 47-51**: Formats each line individually
- **Line 53**: Joins formatted lines with `\n`

---

### Error Handling

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 32-36**:
```csharp
catch (Exception ex)
{
    ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
    return 1;
}
```

**Explanation**:
- Catches any unexpected exceptions
- Displays error using `ConsoleHelpers.WriteErrorLine()`
- Includes both message and stack trace
- Returns exit code 1 (failure)

---

### Success Return

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Line 30**:
```csharp
return 0;
```

**Explanation**: Returns exit code 0 on successful formatting.

---

## Data Flow Diagram

```
User runs: cycodt expect format --input test.txt --output formatted.txt --strict true --debug

CommandLineOptions.Parse()
    ↓
    arg == "--debug" → Debug = true, ConfigureDebug(true)
    ↓
    arg == "--input" → Input = "test.txt"
    ↓
    arg == "--output" → Output = "formatted.txt"
    ↓
    arg == "--strict" → Strict = true
    ↓
ExpectFormatCommand.ExecuteAsync()
    ↓
ExpectFormatCommand.ExecuteFormat()
    ↓
input = FileHelpers.ReadAllText("test.txt")
    ↓
formattedText = FormatInput(input)
    ↓
    ConsoleHelpers.WriteDebugHexDump(input, ...)
    ↓
    lines = input.Split('\n', StringSplitOptions.None)
    ↓
    For each line:
        FormatLine(line, Strict=true)
            ↓
            ConsoleHelpers.WriteDebugHexDump(line, "FormatLine:")
            ↓
            escaped = EscapeSpecialRegExChars(line)
                ↓
                Regex.Replace(line, special chars, add backslash)
            ↓
            ConsoleHelpers.WriteDebugHexDump(escaped, "post-escape:")
            ↓
            escapedCR = escaped.Trim('\r').Replace("\r", "\\r")
            ↓
            ConsoleHelpers.WriteDebugHexDump(escapedCR, "post-cr-escape:")
            ↓
            result = $"^{escapedCR}\\r?$\\n"
            ↓
            ConsoleHelpers.WriteDebugHexDump(result, "result:")
            ↓
            return result
    ↓
    Join formatted lines with "\n"
    ↓
WriteOutput(formattedText)
    ↓
    Since Output = "formatted.txt":
        FileHelpers.WriteAllText("formatted.txt", formattedText)
    ↓
Return 0
```

---

## Summary of Evidence

### Options Parsed
1. **--input**: Lines 41-46 of CycoDtCommandLineOptions.cs
2. **--output / --save-output**: Lines 48-54 of CycoDtCommandLineOptions.cs
3. **--strict**: Lines 55-64 of CycoDtCommandLineOptions.cs
4. **--debug**: Lines 341-345 of CommandLineOptions.cs
5. **--quiet**: Lines 350-353 of CommandLineOptions.cs

### Command Properties
1. **Strict default**: Lines 6-9 of ExpectFormatCommand.cs
2. **Strict property**: Line 11 of ExpectFormatCommand.cs

### Display Logic
1. **WriteOutput method**: Lines 31-41 of ExpectBaseCommand.cs
2. **FormatInput method**: Lines 39-54 of ExpectFormatCommand.cs
3. **FormatLine method**: Lines 61-77 of ExpectFormatCommand.cs
4. **EscapeSpecialRegExChars**: Lines 56-59 of ExpectFormatCommand.cs

### Debug Output
1. **Input hex dump**: Line 42 of ExpectFormatCommand.cs
2. **FormatLine hex dumps**: Lines 63, 66, 71, 74 of ExpectFormatCommand.cs

### Error Handling
1. **Exception catch**: Lines 32-36 of ExpectFormatCommand.cs
2. **Success return**: Line 30 of ExpectFormatCommand.cs

---

## Related Source Files

- **Command**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`
- **Base class**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`
- **Parser**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
- **Options base**: `src/common/CommandLine/CommandLineOptions.cs`
- **File helpers**: `src/common/Helpers/FileHelpers.cs`
- **Console helpers**: `src/common/Helpers/ConsoleHelpers.cs`
