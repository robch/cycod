# cycodt `expect format` Command - Layer 1: TARGET SELECTION - Proof

## Source Code Evidence

This document provides detailed evidence for Layer 1 (TARGET SELECTION) features of the `cycodt expect format` command.

---

## Inheritance Proof

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 1-9:**
```csharp
using System.Text;
using System.Text.RegularExpressions;

class ExpectFormatCommand : ExpectBaseCommand
{
    public ExpectFormatCommand() : base()
    {
        Strict = true; // Default to true
    }
```

**Evidence:**
- **Line 4**: `ExpectFormatCommand` inherits from `ExpectBaseCommand`
- **Line 6**: Calls base constructor

**Implication:** All Layer 1 logic is in `ExpectBaseCommand`, not `ExpectFormatCommand`.

---

## Shared Implementation Evidence

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

All Layer 1 implementation is **identical** to `expect check`. See:
- [cycodt `expect check` Command - Layer 1 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-1-proof.md)

### Key Shared Components

1. **Input Property** (Lines 9-10 in ExpectBaseCommand.cs)
```csharp
public string? Input { get; set; }
public string? Output { get; set; }
```

2. **Empty Detection** (Lines 12-17 in ExpectBaseCommand.cs)
```csharp
public override bool IsEmpty()
{
    var noInput = string.IsNullOrEmpty(Input);
    var isRedirected = Console.IsInputRedirected;
    return noInput && !isRedirected;
}
```

3. **Stdin Auto-Detection** (Lines 19-29 in ExpectBaseCommand.cs)
```csharp
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
```

**Evidence:** All three methods are in `ExpectBaseCommand`, so both `expect check` and `expect format` use them.

---

## Command Entry Point

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 18-37:**
```csharp
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
```

**Evidence:**
- **Line 27**: Reads input using `FileHelpers.ReadAllText(Input!)`
- Uses **inherited** `Input` property from `ExpectBaseCommand`
- The `!` operator asserts `Input` is not null (validated earlier)

**Comparison to `expect check`:**

**File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`**

**Line 38:**
```csharp
var lines = FileHelpers.ReadAllLines(Input!);
```

**Difference:**
- `expect format` uses `ReadAllText()` (returns single string)
- `expect check` uses `ReadAllLines()` (returns string array)
- Both use the **same** `Input` property
- Both read from **same** source (file or stdin based on `Input`)

---

## Call Stack Comparison: `expect check` vs `expect format`

### `expect check` Call Stack:
```
CommandLineOptions.Parse(args)
    ↓
CycoDtCommandLineOptions.TryParseExpectCommandOptions(command as ExpectBaseCommand, ...)
    ↓
    Parse --input → command.Input = value
    ↓
ExpectBaseCommand.Validate()  // ← Inherited
    ↓
    if (Input == null && stdin): Input = "-"
    ↓
ExpectCheckCommand.ExecuteAsync()
    ↓
    ExpectCheckCommand.ExecuteCheck()
        ↓
        FileHelpers.ReadAllLines(Input)  // ← Layer 1 ends
        ↓
        [Layers 3-4-8: Check expectations]
```

### `expect format` Call Stack:
```
CommandLineOptions.Parse(args)
    ↓
CycoDtCommandLineOptions.TryParseExpectCommandOptions(command as ExpectBaseCommand, ...)
    ↓
    Parse --input → command.Input = value  // ← IDENTICAL
    ↓
ExpectBaseCommand.Validate()  // ← IDENTICAL (inherited)
    ↓
    if (Input == null && stdin): Input = "-"  // ← IDENTICAL
    ↓
ExpectFormatCommand.ExecuteAsync()
    ↓
    ExpectFormatCommand.ExecuteFormat()
        ↓
        FileHelpers.ReadAllText(Input)  // ← Layer 1 ends (same source, different read method)
        ↓
        [Layer 9: Format input]
```

**Evidence:** Layer 1 call stack is **100% identical** except for the final read method choice (text vs lines).

---

## Data Structures

### ExpectFormatCommand Class

**File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 6-11:**
```csharp
public ExpectFormatCommand() : base()
{
    Strict = true; // Default to true
}

public bool Strict { get; set; }
```

**Layer 1 Properties:** None in this class.

**Layer 1 Inherited Properties (from ExpectBaseCommand):**
- `public string? Input { get; set; }` (Line 9 in ExpectBaseCommand.cs)

**Layer 6 Property:**
- **Line 11**: `Strict` - controls formatting strictness (not Layer 1)

---

## Command Line Parsing (Shared)

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 32-54:**
```csharp
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
        command.Input = input!;  // ← Sets Input on ExpectBaseCommand
        i += max1Arg.Count();
    }
    else if (arg == "--save-output" || arg == "--output")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
        command.Output = output;  // ← Sets Output on ExpectBaseCommand
        i += max1Arg.Count();
    }
```

**Evidence:**
- **Line 45**: Sets `command.Input` where `command` is `ExpectBaseCommand`
- **Line 52**: Sets `command.Output` where `command` is `ExpectBaseCommand`
- Both `ExpectCheckCommand` and `ExpectFormatCommand` pass through this code

**Parsing for `--strict` (format-specific, not Layer 1):**

**Lines 55-63:**
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

**Evidence:**
- **Line 55**: Type check: `command is ExpectFormatCommand` (specific to `format`)
- This is **Layer 6** (Display Control), not Layer 1

---

## Format-Specific Execution (Not Layer 1)

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 39-77:**
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

private static string EscapeSpecialRegExChars(string line)
{
    return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
}

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

**Evidence:**
- All this code is **Layer 9** (Actions on Results)
- It operates on input **after** Layer 1 has provided it
- Does not affect how input is selected or read

---

## Comparison Table: `expect check` vs `expect format` Layer 1

| Feature | `expect check` | `expect format` | Implementation |
|---------|----------------|-----------------|----------------|
| Base class | `ExpectBaseCommand` | `ExpectBaseCommand` | Shared |
| `Input` property | ✅ Yes | ✅ Yes | Inherited from base |
| `--input` option | ✅ Yes | ✅ Yes | Parsed in `TryParseExpectCommandOptions` |
| Stdin auto-detection | ✅ Yes | ✅ Yes | `ExpectBaseCommand.Validate()` |
| `IsEmpty()` logic | ✅ Yes | ✅ Yes | `ExpectBaseCommand.IsEmpty()` |
| Single input only | ✅ Yes | ✅ Yes | Only one `Input` property |
| Glob patterns | ❌ No | ❌ No | Not in `ExpectBaseCommand` |
| Exclusion patterns | ❌ No | ❌ No | Not in `ExpectBaseCommand` |
| Multi-file support | ❌ No | ❌ No | No list properties |
| Config files | ❌ No | ❌ No | No config loading |

**Conclusion:** 100% identical Layer 1 behavior.

---

## Differences Begin After Layer 1

### Input Reading Method

**`expect check` (Line 38 in ExpectCheckCommand.cs):**
```csharp
var lines = FileHelpers.ReadAllLines(Input!);
```

**`expect format` (Line 27 in ExpectFormatCommand.cs):**
```csharp
var input = FileHelpers.ReadAllText(Input!);
```

**Difference:**
- Different read methods (`ReadAllLines` vs `ReadAllText`)
- Same input source (`Input` property)
- **Not a Layer 1 difference** (input selection is identical)

### Processing Logic

**`expect check`:**
- Validates input against regex patterns and/or AI instructions
- Returns pass/fail

**`expect format`:**
- Transforms input into regex patterns
- Outputs formatted text

**Evidence:** Processing logic is **Layer 9** (Actions on Results), not Layer 1.

---

## Complete Layer 1 Implementation (Shared)

All Layer 1 implementation details are **identical** to the `expect check` command. See:
- [cycodt `expect check` Command - Layer 1 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-1-proof.md)

### Proven Shared Components:

1. ✅ **Inheritance**: `ExpectFormatCommand` extends `ExpectBaseCommand` (line 4 in ExpectFormatCommand.cs)
2. ✅ **Input property**: Inherited from base class (line 9 in ExpectBaseCommand.cs)
3. ✅ **`--input` parsing**: Lines 41-46 in CycoDtCommandLineOptions.cs
4. ✅ **Stdin detection**: Lines 12-17 (IsEmpty), 19-29 (Validate) in ExpectBaseCommand.cs
5. ✅ **Input reading**: Line 27 in ExpectFormatCommand.cs (uses inherited `Input`)

---

## Summary of Proof

### Proven Identical Features (Layer 1):

1. ✅ **Inheritance**: Both commands extend `ExpectBaseCommand`
2. ✅ **Input property**: Same `Input` property from base class
3. ✅ **Parsing logic**: Same `TryParseExpectCommandOptions()` method
4. ✅ **Validation logic**: Same `Validate()` method
5. ✅ **Empty detection**: Same `IsEmpty()` method
6. ✅ **Stdin support**: Same stdin auto-detection logic

### Proven Differences (Other Layers):

1. ✅ **Layer 6**: `format` has `--strict` option (lines 55-63 in CycoDtCommandLineOptions.cs)
2. ✅ **Layer 9**: Different processing logic (check vs format)

### Conclusion

The `expect format` command's Layer 1 implementation is **provably identical** to the `expect check` command through inheritance. All source code evidence confirms zero differences in TARGET SELECTION behavior.

Both commands:
- Inherit from `ExpectBaseCommand`
- Use the same `Input` property
- Parse the same `--input` option
- Apply the same stdin auto-detection
- Support the same single-input-stream model

The only differences are in **what they do** with the input (Layer 9), not **how they select** the input (Layer 1).
