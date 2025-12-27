# cycodt expect format - Layer 5: Context Expansion - PROOF

## Evidence: No Context Expansion Implementation

This document provides source code evidence that the `cycodt expect format` command **does NOT implement Layer 5 (Context Expansion)** features for preserving context or selectively formatting lines.

---

## Source Code Analysis

### 1. ExpectFormatCommand Implementation

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

```csharp
// Lines 1-79 (entire file)
using System.Text;
using System.Text.RegularExpressions;

class ExpectFormatCommand : ExpectBaseCommand
{
    public ExpectFormatCommand() : base()
    {
        Strict = true; // Default to true
    }

    public bool Strict { get; set; }  // Layer 4: CR handling, NOT context expansion

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
            var input = FileHelpers.ReadAllText(Input!);  // Line 27: Read entire input
            var formattedText = FormatInput(input);       // Line 28: Format all
            WriteOutput(formattedText);                   // Line 29: Write all
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }

    private string FormatInput(string input)
    {
        var c = input.Count(c => c == '\n');
        ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");

        var lines = input.Split('\n', StringSplitOptions.None);  // Line 44: Split into lines
        var formattedLines = new List<string>();

        // Lines 46-51: Format EVERY line, no filtering or context logic
        foreach (var line in lines)
        {
            var formatted = FormatLine(line, Strict);  // Format each line independently
            formattedLines.Add(formatted);
        }

        return string.Join("\n", formattedLines);  // Line 53: Join all formatted lines
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

        // Lines 68-70: Strict mode affects CR handling (Layer 4), not context
        var escapedCR = strict
            ? escaped.Trim('\r').Replace("\r", "\\r")
            : escaped.Replace("\r", "\\r");
        ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");

        var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
        ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");

        return result;
    }
}
```

**Analysis**:
- **Property**: Only `Strict` (Layer 4 - CR handling) - NO context expansion properties
- **NO properties** for:
  - `PreserveContext`
  - `FormatMatchingPattern`
  - `ContextLines`
  - `ShowOriginal`
  - `SelectiveFormatting`
  - `LineRange`
- **Line 27**: Reads entire input at once - no line selection
- **Line 44**: Splits into lines
- **Lines 46-51**: Formats EVERY line unconditionally - no filtering or context logic
- **Line 53**: Joins ALL formatted lines - no context preservation

### 2. FormatInput Method - No Filtering Logic

```csharp
private string FormatInput(string input)
{
    var lines = input.Split('\n', StringSplitOptions.None);
    var formattedLines = new List<string>();

    foreach (var line in lines)  // Processes EVERY line
    {
        var formatted = FormatLine(line, Strict);
        formattedLines.Add(formatted);  // Adds EVERY formatted line
    }

    return string.Join("\n", formattedLines);
}
```

**Analysis**:
- **NO logic** for:
  - Skipping lines
  - Preserving unformatted context lines
  - Selective formatting based on pattern matching
  - Grouping lines with context
  - Adding context comments
- **Every line** is formatted and added to output
- **No conditional logic** based on line content or position

### 3. FormatLine Method - Line-by-Line, No Context

```csharp
private string FormatLine(string line, bool strict)
{
    var escaped = EscapeSpecialRegExChars(line);
    var escapedCR = strict
        ? escaped.Trim('\r').Replace("\r", "\\r")
        : escaped.Replace("\r", "\\r");
    var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
    return result;
}
```

**Analysis**:
- Processes a single line in isolation
- NO access to:
  - Previous lines (for context before)
  - Next lines (for context after)
  - Line number information
  - Pattern matching for selective formatting
- Returns only the formatted line - no context information

### 4. ExpectBaseCommand - No Context Properties

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

```csharp
abstract class ExpectBaseCommand : Command
{
    public ExpectBaseCommand()
    {
        Input = null;    // Layer 1
        Output = null;   // Layer 7
    }
    
    public string? Input { get; set; }   // Layer 1: Input source
    public string? Output { get; set; }  // Layer 7: Output destination
    
    // NO Layer 5 context properties
}
```

**Analysis**:
- Only input/output properties
- **NO properties** for context expansion such as:
  - `ContextLinesBefore`
  - `ContextLinesAfter`
  - `SelectiveFormatPattern`
  - `PreserveContextComments`
  - `LineRangeStart`, `LineRangeEnd`

### 5. Command Line Parser - No Context Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

```csharp
// Lines 15-55 (TryParseExpectCommandOptions - relevant parts)
private bool TryParseExpectCommandOptions(ExpectBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--input")  // Layer 1
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var input = ValidateString(arg, max1Arg.FirstOrDefault(), "input");
        command.Input = input!;
        i += max1Arg.Count();
    }
    else if (arg == "--save-output" || arg == "--output")  // Layer 7
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
        command.Output = output;
        i += max1Arg.Count();
    }
    else if (command is ExpectFormatCommand formatCommand && arg == "--strict")  // Layer 4
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var strictStr = ValidateString(arg, max1Arg.FirstOrDefault(), "strict");
        if (bool.TryParse(strictStr, out bool strict))
        {
            formatCommand.Strict = strict;
        }
        i += max1Arg.Count();
    }
    // ... (other expect check options) ...
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Analysis**:
- Parses only: `--input`, `--output`, `--strict`
- **NO parsing** for Layer 5 context expansion options such as:
  - `--context N`
  - `--preserve-context N`
  - `--format-matching PATTERN`
  - `--keep-context N`
  - `--lines START-END`
  - `--show-original`
  - `--group-by-blank-lines`
  - `--selective-format`

---

## Comparison with Tools That Have Context/Selective Processing

### sed (Unix tool) - HAS selective processing

```bash
# Process only lines 10-20
sed -n '10,20p' file.txt

# Process only lines matching pattern
sed -n '/error/p' file.txt

# Transform only matching lines
sed '/error/s/error/ERROR/' file.txt  # Uppercase only 'error' lines
```

### awk (Unix tool) - HAS context and selective processing

```awk
# Format only lines with "error", keep context
awk '
  /error/ { print "# Context:", prev1; print format($0); print "# Context:", next_line }
  { prev1 = $0 }
'
```

### cycodmd FindFilesCommand - HAS selective display with context

**Properties**:
```csharp
public List<Regex> IncludeLineContainsPatternList { get; set; }  // Selective display
public int IncludeLineCountBefore { get; set; }                  // Context before
public int IncludeLineCountAfter { get; set; }                   // Context after
```

**Usage**: Shows only matching lines with N lines of context before/after

### cycodt expect format - DOES NOT HAVE selective processing or context

**NO equivalent properties or options.**

---

## Processing Flow Analysis

### Current Flow (No Context, All Lines)

```
Input (file or stdin)
    ↓
ReadAllText()
    ↓
Split into lines
    ↓
ForEach line:
    EscapeSpecialRegExChars(line)
    ↓
    Handle CR (strict mode)
    ↓
    Add ^ and $ anchors (strict mode)
    ↓
    Add to output list
    ↓
Join all lines
    ↓
WriteOutput()
```

**Every line is processed identically.**

### Enhanced Flow WITH Context (Hypothetical)

```
Input (file or stdin)
    ↓
ReadAllText()
    ↓
Split into lines (with line numbers)
    ↓
IF --format-matching specified:
    Filter lines matching pattern
    ↓
    For each matched line:
        Get context lines before (N)
        Get context lines after (N)
        ↓
        Add context as comments
        Format matched line
        Add context as comments
ELSE:
    ForEach line:
        Format line
    ↓
Join all lines
    ↓
WriteOutput()
```

**This logic does NOT exist in current implementation.**

---

## Example: What Current vs. Enhanced Would Look Like

### Input (test-output.txt)
```
Starting test suite...
Running test 1...
Test 1: PASSED
Running test 2...
ERROR: Test 2 failed with code 42
Test 2: FAILED
Running test 3...
Test 3: PASSED
All tests completed
```

### Current Output (ALL lines formatted)
```bash
$ cycodt expect format --input test-output.txt
```

Output (9 formatted lines):
```
^Starting test suite\.\.\.\\r?$\\n
^Running test 1\.\.\.\\r?$\\n
^Test 1: PASSED\\r?$\\n
^Running test 2\.\.\.\\r?$\\n
^ERROR: Test 2 failed with code 42\\r?$\\n
^Test 2: FAILED\\r?$\\n
^Running test 3\.\.\.\\r?$\\n
^Test 3: PASSED\\r?$\\n
^All tests completed\\r?$\\n
```

### Enhanced Output WITH Context (Hypothetical)
```bash
$ cycodt expect format --input test-output.txt --format-matching "ERROR|FAILED" --context 1
```

Output (formatted error lines with 1 line context as comments):
```
# Context: Running test 2...
^ERROR: Test 2 failed with code 42\\r?$\\n
^Test 2: FAILED\\r?$\\n
# Context: Running test 3...
```

**Much more manageable for test expectations!**

But this feature **does NOT exist**.

---

## Code Evidence: No Line Selection Mechanism

### Missing: Line Number Tracking

Current code has NO line number tracking:

```csharp
foreach (var line in lines)  // No index, no line number
{
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

To support selective formatting, would need:
```csharp
for (int i = 0; i < lines.Length; i++)
{
    if (ShouldFormatLine(lines[i], i, pattern))
    {
        AddContextBefore(i, lines, formattedLines);
        formattedLines.Add(FormatLine(lines[i], Strict));
        AddContextAfter(i, lines, formattedLines);
    }
}
```

**This does NOT exist.**

### Missing: Pattern Matching for Selective Formatting

No pattern matching properties:

```csharp
// Current: Only Strict property
public bool Strict { get; set; }

// Would need (NOT present):
public string? FormatMatchingPattern { get; set; }
public int ContextLines { get; set; }
```

### Missing: Context Preservation Logic

No methods for context handling:

```csharp
// Would need (NOT present):
private void AddContextBefore(int lineIndex, string[] lines, List<string> output, int contextLines)
private void AddContextAfter(int lineIndex, string[] lines, List<string> output, int contextLines)
private bool ShouldFormatLine(string line, int index, string pattern)
private string FormatAsComment(string line)
```

**None of these helper methods exist.**

---

## Conclusion

The source code analysis definitively shows that **cycodt expect format command does NOT implement Layer 5 (Context Expansion)**:

1. ✅ **ExpectFormatCommand.cs** - No context properties, formats ALL lines unconditionally
2. ✅ **FormatInput() method** - No filtering logic, processes every line
3. ✅ **FormatLine() method** - Processes lines in isolation, no context access
4. ✅ **ExpectBaseCommand.cs** - No context properties defined
5. ✅ **CycoDtCommandLineOptions.cs** - No context options parsed
6. ✅ **Processing Flow** - Linear, all-lines processing with no conditional logic
7. ✅ **Missing Infrastructure** - No line number tracking, pattern matching, or context preservation
8. ✅ **Comparison** - Other tools (sed, awk, cycodmd) have selective processing; cycodt does not

**Verdict**: Layer 5 is **NOT IMPLEMENTED** for the `expect format` command.

The `expect format` command transforms input text line-by-line into regex patterns with NO options to:
- Preserve context lines
- Selectively format matching lines only
- Include original text alongside formatted
- Group lines with surrounding context
- Add context comments for readability

**All input lines are formatted unconditionally and output as a complete set.**

---

**Related Files**:
- [Layer 5 Catalog](cycodt-expect-format-filtering-pipeline-catalog-layer-5.md)
- [Layer 1 Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 3 Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 4 Proof (CR Handling)]( - to be created)
- [Layer 6 Proof (Display)]( - to be created)
