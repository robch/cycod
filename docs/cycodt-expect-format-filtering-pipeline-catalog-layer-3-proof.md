# cycodt `expect format` Command - Layer 3: Content Filtering - PROOF

## Overview

This document provides source code evidence demonstrating that the `expect format` command has **NO Layer 3 (Content Filtering) functionality**. All content is processed without filtering or selection.

---

## No Filtering Options

### Command Class Definition

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 1-11** (Complete class properties):
```csharp
using System.Text;
using System.Text.RegularExpressions;

class ExpectFormatCommand : ExpectBaseCommand
{
    public ExpectFormatCommand() : base()
    {
        Strict = true; // Default to true
    }

    public bool Strict { get; set; }
```

**Evidence**:
- Line 11: Only one property: `Strict` (boolean)
- `Strict` controls transformation behavior, not filtering
- No properties for pattern matching, line selection, or content filtering

### Inherited Properties

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

**Lines 1-10**:
```csharp
abstract class ExpectBaseCommand : Command
{
    public ExpectBaseCommand()
    {
        Input = null;
        Output = null;
    }
    
    public string? Input { get; set; }
    public string? Output { get; set; }
```

**Evidence**:
- Line 9: `Input` - input source (Layer 1)
- Line 10: `Output` - output destination (Layer 7)
- No filtering-related properties inherited

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 135-163** (All ExpectCommandOptions parsing):
```csharp
else if (arg == "--input")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var input = ValidateString(arg, max1Arg.FirstOrDefault(), "input");
    command.Input = input!;
    i += max1Arg.Count();
}
else if (arg == "--save-output" || arg == "--output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
    command.Output = output;
    i += max1Arg.Count();
}
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
else if (command is ExpectCheckCommand checkCommand && arg == "--regex")
{
    // ... (ExpectCheckCommand only, not ExpectFormatCommand)
}
```

**Evidence**:
- Lines 135-141: `--input` (shared by all ExpectBaseCommand subclasses)
- Lines 142-147: `--save-output` / `--output` (shared by all ExpectBaseCommand subclasses)
- Lines 148-157: `--strict` (specific to ExpectFormatCommand)
- Lines 158+: `--regex`, `--not-regex`, `--instructions` are for ExpectCheckCommand ONLY
- No filtering options parsed for ExpectFormatCommand

**Conclusion**: `expect format` has ZERO command-line options for Layer 3 filtering.

---

## All Content Is Processed

### ExecuteFormat Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 23-37** (Complete method):
```csharp
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

**Evidence**:
- Line 27: Read ALL input text
- Line 28: Format ALL input text (no filtering parameter)
- Line 29: Write ALL formatted text
- No conditional logic for filtering content

### FormatInput Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 39-54** (Complete method):
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

**Evidence**:
- Line 44: `Split('\n', StringSplitOptions.None)` - keeps ALL lines including empty lines
- Line 45: List to collect formatted lines
- Lines 47-51: `foreach (var line in lines)` - processes EVERY line with no conditions
- Line 49: `FormatLine(line, Strict)` - transforms line (no skipping)
- Line 50: `formattedLines.Add(formatted)` - adds EVERY formatted line
- Line 53: Joins ALL formatted lines back together

**Proof of No Filtering**:
- No `if` conditions to skip lines
- No `Where()` clause to filter lines
- No pattern matching to select lines
- Every input line produces exactly one output line

### Line Count Preservation

**Input-Output Mapping**:
```
Input:  N lines
Split:  N lines (Line 44)
Foreach: N iterations (Lines 47-51)
Format: N formatted lines (Line 49, executed N times)
Add:    N additions (Line 50, executed N times)
Join:   N lines (Line 53)
Output: N lines
```

**Mathematical Proof**:
- Let `N` = number of input lines
- For each line `i` in `[0, N)`:
  - Line is formatted exactly once (Line 49)
  - Formatted line is added exactly once (Line 50)
- Total output lines = N
- Therefore: `Output lines == Input lines` (no filtering)

---

## Strict Mode Transformation

### FormatLine Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 61-77** (Complete method):
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

**Evidence**:
- Line 68: `strict` parameter determines transformation behavior
- Lines 68-70: If strict, trim `\r` then replace remaining `\r` with `\\r`; else just replace
- Line 73: If strict, add `^` prefix and `\\r?$\\n` suffix; else use escaped line as-is

**Strict Mode Effects**:
- ✅ Affects HOW line is transformed
- ❌ Does NOT affect WHETHER line is transformed
- ALL lines are transformed regardless of `strict` value

### EscapeSpecialRegExChars Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 56-59**:
```csharp
private static string EscapeSpecialRegExChars(string line)
{
    return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
}
```

**Evidence**:
- Escapes regex special characters: `\`, `(`, `)`, `[`, `]`, `{`, `}`, `.`, `*`, `+`, `?`, `|`, `^`, `$`
- This is a transformation operation, not a filtering operation
- Returns transformed string (same structure, different content)

---

## Filtering vs Transformation

### Conceptual Comparison

**Filtering Operation Characteristics**:
1. **Selection**: Chooses subset of items based on criteria
2. **Conditional**: Uses if/where/when logic
3. **Count Change**: Output count ≤ Input count
4. **Information Loss**: Some items are excluded

**Transformation Operation Characteristics**:
1. **Mapping**: Applies function to all items
2. **Unconditional**: Processes every item
3. **Count Preservation**: Output count == Input count
4. **Information Preservation**: All items are included (though modified)

### `expect format` Analysis

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 47-51**:
```csharp
foreach (var line in lines)
{
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Checklist**:
- ❌ No selection (processes all lines)
- ❌ No conditional logic (no if statements)
- ✅ Count preservation (N input lines → N output lines)
- ✅ Information preservation (all lines included)
- ✅ Unconditional processing (every line formatted)

**Conclusion**: `expect format` is a **transformation**, not a **filter**.

### Layer Mapping

**Layer 3 (Content Filtering)** should:
- Select which content to process
- Use criteria to include/exclude items
- Reduce content based on patterns

**`expect format` actually does**:
- Process ALL content
- Transform content structure
- Preserve all input items

**Correct Layer Assignment**:
- ❌ Not Layer 3 (Content Filtering)
- ✅ Is Layer 9 (Actions on Results - Transformation)

---

## Comparison with Other Commands

### `list` Command (Has Layer 3)

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 18**:
```csharp
var tests = FindAndFilterTests();
```

**Evidence**: Explicitly filters tests based on criteria (Layer 3 functionality)

### `expect check` Command (Has Layer 3)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 41**:
```csharp
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
```

**Evidence**: Validates lines based on patterns (Layer 3 functionality)

### `expect format` Command (No Layer 3)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 47-51**:
```csharp
foreach (var line in lines)
{
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Evidence**: Transforms ALL lines without filtering (NO Layer 3 functionality)

---

## Hypothetical Layer 3 Extensions

While not currently implemented, Layer 3 filtering COULD be added to `expect format`:

### Option: `--line-contains` (Hypothetical)

**Proposed Behavior**:
```csharp
foreach (var line in lines)
{
    if (LineContains != null && !line.Contains(LineContains))
    {
        continue; // Skip lines that don't match
    }
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Effect**: Would be Layer 3 functionality (content filtering)

### Option: `--skip-empty` (Hypothetical)

**Proposed Behavior**:
```csharp
foreach (var line in lines)
{
    if (SkipEmpty && string.IsNullOrWhiteSpace(line))
    {
        continue; // Skip empty lines
    }
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Effect**: Would be Layer 3 functionality (content filtering)

**Current Reality**: None of these options exist in the codebase.

---

## Summary of Evidence

### No Filtering Options
✅ **Class properties**: Lines 1-11 in ExpectFormatCommand.cs (only `Strict` property)
✅ **Inherited properties**: Lines 1-10 in ExpectBaseCommand.cs (`Input`, `Output` only)
✅ **Command line parsing**: Lines 135-163 in CycoDtCommandLineOptions.cs (no filtering options)

### All Content Processed
✅ **ExecuteFormat()**: Lines 23-37 in ExpectFormatCommand.cs (no filtering)
✅ **FormatInput()**: Lines 39-54 in ExpectFormatCommand.cs (`foreach` all lines)
✅ **Line count preservation**: Input N lines → Output N lines (mathematical proof)

### Transformation, Not Filtering
✅ **FormatLine()**: Lines 61-77 in ExpectFormatCommand.cs (transforms, doesn't filter)
✅ **Strict mode**: Line 73 (affects transformation, not selection)
✅ **Unconditional processing**: Lines 47-51 (no conditions, all lines processed)

### Comparison with Other Commands
✅ **list has Layer 3**: TestListCommand.cs line 18 (`FindAndFilterTests()`)
✅ **expect check has Layer 3**: ExpectCheckCommand.cs line 41 (`CheckLines()`)
✅ **expect format has NO Layer 3**: ExpectFormatCommand.cs lines 47-51 (no filtering)

---

## Conclusion

All assertions in [Layer 3: Content Filtering](cycodt-expect-format-filtering-pipeline-catalog-layer-3.md) are supported by source code evidence. The `expect format` command has **ZERO Layer 3 functionality**:

1. ❌ No filtering options in command-line parser
2. ❌ No filtering properties in command class
3. ❌ No conditional logic in content processing
4. ✅ ALL lines are processed without exception
5. ✅ Line count is preserved (N input → N output)
6. ✅ Operation is transformation (Layer 9), not filtering (Layer 3)

**Layer 3 is NOT APPLICABLE** to the `expect format` command.
