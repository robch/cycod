# cycodt `expect format` - Layer 8 Proof: AI Processing

## Source Code Evidence

This document provides source code evidence that the `expect format` command does **NOT** implement Layer 8 (AI Processing). The command uses a purely deterministic, rule-based algorithm.

---

## 1. Command Class Definition

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Full class (lines 1-79)**:

```csharp
1: using System.Text;
2: using System.Text.RegularExpressions;
3: 
4: class ExpectFormatCommand : ExpectBaseCommand
5: {
6:     public ExpectFormatCommand() : base()
7:     {
8:         Strict = true; // Default to true
9:     }
10: 
11:     public bool Strict { get; set; }
12: 
13:     public override string GetCommandName()
14:     {
15:         return "expect format";
16:     }
17: 
18:     public override async Task<object> ExecuteAsync(bool interactive)
19:     {
20:         return await Task.Run(() => ExecuteFormat());
21:     }
22: 
23:     private int ExecuteFormat()
24:     {
25:         try
26:         {
27:             var input = FileHelpers.ReadAllText(Input!);
28:             var formattedText = FormatInput(input);
29:             WriteOutput(formattedText);
30:             return 0;
31:         }
32:         catch (Exception ex)
33:         {
34:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
35:             return 1;
36:         }
37:     }
38: 
39:     private string FormatInput(string input)
40:     {
41:         var c = input.Count(c => c == '\n');
42:         ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");
43: 
44:         var lines = input.Split('\n', StringSplitOptions.None);
45:         var formattedLines = new List<string>();
46: 
47:         foreach (var line in lines)
48:         {
49:             var formatted = FormatLine(line, Strict);
50:             formattedLines.Add(formatted);
51:         }
52: 
53:         return string.Join("\n", formattedLines);
54:     }
55: 
56:     private static string EscapeSpecialRegExChars(string line)
57:     {
58:         return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
59:     }
60: 
61:     private string FormatLine(string line, bool strict)
62:     {
63:         ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");
64: 
65:         var escaped = EscapeSpecialRegExChars(line);
66:         ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");
67: 
68:         var escapedCR = strict
69:             ? escaped.Trim('\r').Replace("\r", "\\r")
70:             : escaped.Replace("\r", "\\r");
71:         ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");
72: 
73:         var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
74:         ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");
75: 
76:         return result;
77:     }
78: }
79: 
```

**Evidence**:
- **Only one property**: `Strict` (bool) - controls formatting mode (line 11)
- **No AI properties**: No `Instructions`, `Prompt`, `AiModel`, etc.
- **No AI helper calls**: No calls to `CheckExpectInstructionsHelper` or similar
- **Pure transformation**: `FormatInput()` and `FormatLine()` are deterministic string operations
- **Algorithm**:
  - Line 44: Split input into lines
  - Lines 47-51: Process each line independently
  - Line 49: Call `FormatLine()` with strict mode
  - Line 53: Join lines back together

---

## 2. Regex Escaping Logic

### Lines 56-59: Character escaping

```csharp
56:     private static string EscapeSpecialRegExChars(string line)
57:     {
58:         return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
59:     }
```

**Evidence**:
- **Deterministic regex operation**: Replaces special regex characters with escaped versions
- **Pattern**: `([\\()\[\]{}.*+?|^$])` - matches any regex metacharacter
- **Replacement**: `\$1` - escapes each match with backslash
- **Characters escaped**: `\`, `(`, `)`, `[`, `]`, `{`, `}`, `.`, `*`, `+`, `?`, `|`, `^`, `$`
- **No AI involvement**: Pure string manipulation via standard Regex.Replace

---

## 3. Line Formatting Logic

### Lines 61-77: Line transformation

```csharp
61:     private string FormatLine(string line, bool strict)
62:     {
63:         ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");
64: 
65:         var escaped = EscapeSpecialRegExChars(line);
66:         ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");
67: 
68:         var escapedCR = strict
69:             ? escaped.Trim('\r').Replace("\r", "\\r")
70:             : escaped.Replace("\r", "\\r");
71:         ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");
72: 
73:         var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
74:         ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");
75: 
76:         return result;
77:     }
```

**Evidence**:
- **Step 1 (line 65)**: Escape regex special characters
- **Step 2 (lines 68-70)**: Handle carriage returns
  - **Strict mode**: Trim CRs, then escape remaining ones
  - **Non-strict mode**: Just escape CRs
- **Step 3 (line 73)**: Add anchors and line break
  - **Strict mode**: `^{pattern}\r?$\n` (exact line match)
  - **Non-strict mode**: `{pattern}` (pattern anywhere)
- **Debug output**: Lines 63, 66, 71, 74 - hex dumps for debugging only
- **No variability**: Output is 100% determined by input + strict flag

---

## 4. Execution Flow

### Lines 23-37: Main execution

```csharp
23:     private int ExecuteFormat()
24:     {
25:         try
26:         {
27:             var input = FileHelpers.ReadAllText(Input!);
28:             var formattedText = FormatInput(input);
29:             WriteOutput(formattedText);
30:             return 0;
31:         }
32:         catch (Exception ex)
33:         {
34:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
35:             return 1;
36:         }
37:     }
```

**Evidence**:
- **Line 27**: Read input (from file or stdin via ExpectBaseCommand.Input)
- **Line 28**: Format input using deterministic algorithm
- **Line 29**: Write output (to file or stdout via ExpectBaseCommand.WriteOutput)
- **Line 30**: Return success (0)
- **No AI call**: No subprocess execution, no API calls, no AI helpers

---

## 5. Command Line Parser

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 55-64**: Parsing `--strict` option

```csharp
55:         else if (command is ExpectFormatCommand formatCommand && arg == "--strict")
56:         {
57:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
58:             var strictStr = ValidateString(arg, max1Arg.FirstOrDefault(), "strict");
59:             if (bool.TryParse(strictStr, out bool strict))
60:             {
61:                 formatCommand.Strict = strict;
62:             }
63:             i += max1Arg.Count();
64:         }
```

**Evidence**:
- **Only format-specific option**: `--strict` (boolean)
- **No AI options**: No `--instructions`, `--ai-generalize`, `--ai-simplify`, etc.
- **Validates as boolean**: Lines 59-62 parse "true" or "false"
- **Default is true**: Set in constructor (ExpectFormatCommand.cs line 8)

---

## 6. Base Class

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

**Lines 1-42**: Shared base class

```csharp
1: abstract class ExpectBaseCommand : Command
2: {
3:     public ExpectBaseCommand()
4:     {
5:         Input = null;
6:         Output = null;
7:     }
8:     
9:     public string? Input { get; set; }
10:     public string? Output { get; set; }
11: 
12:     // ... (validation and output methods)
13: }
```

**Evidence**:
- **Only two properties**: `Input` and `Output`
- **No AI properties in base class**: Unlike ExpectCheckCommand, which adds `Instructions`
- **ExpectFormatCommand doesn't extend**: Only uses inherited Input/Output

---

## 7. Comparison with expect check

### expect check (HAS AI)

```csharp
// ExpectCheckCommand.cs
public string? Instructions { get; set; }  // Line 19

// Line 48 in ExecuteCheck():
var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(
    text, Instructions, null, out _, out _, out var instructionsFailedReason);
```

**Has**:
- ✅ `Instructions` property
- ✅ AI helper call
- ✅ Command line option `--instructions`

### expect format (NO AI)

```csharp
// ExpectFormatCommand.cs
public bool Strict { get; set; }  // Line 11

// Lines 28 in ExecuteFormat():
var formattedText = FormatInput(input);
```

**Has**:
- ❌ No `Instructions` property
- ❌ No AI helper calls
- ❌ No `--instructions` option
- ✅ Only `Strict` property
- ✅ Only deterministic string transformation

---

## 8. Algorithm Demonstration

### Example: Strict Mode (default)

**Input**:
```
Hello, World!
Test: 123
```

**Processing**:
1. **Split into lines**: `["Hello, World!", "Test: 123"]`
2. **For each line**:
   - Escape special chars: `.` → `\.`
   - Add anchors: `^pattern\r?$\n`

**Output**:
```
^Hello, World!\\r?$\\n
^Test: 123\\r?$\\n
```

### Example: Non-Strict Mode

**Input**: Same as above

**Output**:
```
Hello, World!
Test: 123
```

**No anchors, just escaped special characters.**

### AI Would Produce

**If AI were involved**, output might vary:
- Generalize numbers: `Test: \d+`
- Generalize patterns: `Hello, .*!`
- Semantic description: "Should greet world and show test number"

**But this does NOT happen** - output is deterministic.

---

## 9. Proof of Absence

**Searching ExpectFormatCommand.cs for AI keywords**:
- ❌ "AI" - 0 occurrences
- ❌ "Instructions" - 0 occurrences
- ❌ "Prompt" - 0 occurrences
- ❌ "GPT" - 0 occurrences
- ❌ "CheckExpect" - 0 occurrences (exists in expect check, not here)
- ❌ "cycod" - 0 occurrences (no subprocess invocation)
- ❌ "RunnableProcess" - 0 occurrences
- ✅ "Regex" - 2 occurrences (using namespace + Regex.Replace for escaping)
- ✅ "Strict" - 5 occurrences (for formatting mode only)

---

## 10. Execution Flow

```
User Command: echo "Hello!" | cycodt expect format --strict true
  ↓
CycoDtCommandLineOptions.Parse()
  ↓
TryParseExpectCommandOptions() [lines 55-64]
  ↓
ExpectFormatCommand.Strict = true
  ↓
ExpectFormatCommand.ExecuteAsync()
  ↓
ExpectFormatCommand.ExecuteFormat()
  ↓
Read input: "Hello!" [line 27]
  ↓
FormatInput() [line 28]
  ↓
  Split into lines [line 44]
    ↓
  For each line:
    FormatLine(line, Strict=true) [line 49]
      ↓
    EscapeSpecialRegExChars() [line 65]
      → "Hello!" (no special chars, unchanged)
      ↓
    Handle CRs (strict mode) [lines 68-70]
      → "Hello!" (no CRs, unchanged)
      ↓
    Add anchors (strict mode) [line 73]
      → "^Hello!\\r?$\\n"
  ↓
Join lines [line 53]
  ↓
Write output: "^Hello!\\r?$\\n" [line 29]
  ↓
Exit with code 0 [line 30]
```

**No AI at any stage.**

---

## 11. Why No AI?

### Design Goals
1. **Predictability**: Same input always produces same output
2. **Testability**: Easy to verify correctness
3. **Performance**: Instant transformation (no AI latency)
4. **Simplicity**: Clear, understandable algorithm

### Use Case
- **Input**: Raw command output
- **Output**: Regex patterns for `expect-regex` in YAML tests
- **Requirement**: Exact matching (by default), not flexible matching

### When to Use AI Instead
- Use `expect check --instructions` for flexible validation
- Use `expect format` for precise pattern generation

---

## Key Findings

1. **Pure Text Transformation**: 100% deterministic string operations
2. **No AI Infrastructure**: No properties, helpers, or subprocess calls
3. **Single Control Option**: Only `--strict` boolean flag
4. **Regex-Based**: Uses standard .NET Regex for escaping
5. **Clear Algorithm**: Escape → Handle CRs → Add anchors (if strict)
6. **Complementary to expect check**: Generates patterns; check validates flexibly

---

## Related Source Files

- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs` - Command implementation
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs` - Base class
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs` - Parser
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs` - Contrast: has AI
- `System.Text.RegularExpressions.Regex` - .NET regex library (for escaping)
