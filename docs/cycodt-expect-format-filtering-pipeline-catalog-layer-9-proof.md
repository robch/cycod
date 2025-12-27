# cycodt `expect format` Command - Layer 9: Actions on Results - PROOF

## Evidence Summary

The `expect format` command performs **ONE PRIMARY ACTION** on its input:
- **ACTION: Transform Input to Regex Patterns** - Convert raw text to regex-escaped patterns suitable for test expectations

---

## Source Code Evidence

### 1. Command Implementation: ExpectFormatCommand.cs

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

#### Complete ExecuteFormat Method (Lines 23-37)

```csharp
23:     private int ExecuteFormat()
24:     {
25:         try
26:         {
27:             var input = FileHelpers.ReadAllText(Input!);
28:             //          ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
29:             //          Load input (Layer 1)
30: 
31:             var formattedText = FormatInput(input);
32:             //                  ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
33:             //                  🔥 ACTION: TRANSFORM TO REGEX PATTERNS 🔥
34: 
35:             WriteOutput(formattedText);
36:             //          ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
37:             //          Output result (Layer 7)
38: 
39:             return 0;  // ← Success exit code
40:         }
41:         catch (Exception ex)
42:         {
43:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
44:             return 1;  // ← Error exit code
45:         }
46:     }
```

**Analysis**:
- **Line 27**: Load input (Layer 1)
- **Line 31**: **ACTION** - `FormatInput()` transforms text to regex patterns
- **Line 35**: Output result (Layer 7)
- **Line 39**: Success exit (always 0 unless exception)
- **NO validation**: Does not check if patterns are correct
- **NO execution**: Does not run processes
- **NO report generation**: Does not create structured reports

---

### 2. ACTION: Transform Input - FormatInput Method

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

#### Method Implementation (Lines 39-54)

```csharp
39:     private string FormatInput(string input)
40:     {
41:         var c = input.Count(c => c == '\n');
42:         ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");
43: 
44:         var lines = input.Split('\n', StringSplitOptions.None);
45:         //          ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
46:         //          Split input into lines
47: 
48:         var formattedLines = new List<string>();
49: 
50:         foreach (var line in lines)
51:         {
52:             var formatted = FormatLine(line, Strict);
53:             //              ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
54:             //              🔥 Transform each line to regex pattern
55:             formattedLines.Add(formatted);
56:         }
57: 
58:         return string.Join("\n", formattedLines);
59:         //     ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
60:         //     Rejoin transformed lines
61:     }
```

**Key Operations**:
1. **Line 44**: Split input into individual lines
2. **Lines 50-56**: Transform each line separately
3. **Line 58**: Rejoin lines with newlines

**Line-by-line transformation**: Each line is independently transformed, maintaining line structure

---

### 3. ACTION: Transform Single Line - FormatLine Method

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

#### Method Implementation (Lines 61-77)

```csharp
61:     private string FormatLine(string line, bool strict)
62:     {
63:         ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");
64: 
65:         var escaped = EscapeSpecialRegExChars(line);
66:         //            ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
67:         //            🔥 TRANSFORMATION STEP 1: Escape regex special chars
68:         ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");
69: 
70:         var escapedCR = strict
71:             ? escaped.Trim('\r').Replace("\r", "\\r")
72:             : escaped.Replace("\r", "\\r");
73:         //  ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
74:         //  🔥 TRANSFORMATION STEP 2: Handle carriage returns
75:         ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");
76: 
77:         var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
78:         //           ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
79:         //           🔥 TRANSFORMATION STEP 3: Add anchors and line ending (strict mode)
80:         ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");
81: 
82:         return result;
83:     }
```

**Transformation Steps**:

#### STEP 1: Escape Special Regex Characters (Line 65)

**Input**: `"Hello.World"`
**Output**: `"Hello\.World"`

**Characters escaped**: `\ ( ) [ ] { } . * + ? | ^ $`

#### STEP 2: Handle Carriage Returns (Lines 70-72)

**Strict mode** (`--strict true`):
- First trim CRs from ends: `"line\r"` → `"line"`
- Then escape any remaining CRs: `"\r"` → `"\\r"`

**Non-strict mode** (`--strict false`):
- Just escape CRs: `"\r"` → `"\\r"`

#### STEP 3: Add Anchors and Line Ending (Line 77)

**Strict mode**:
- Pattern: `^{escaped}\r?$\n`
- Example: `"Hello"` → `"^Hello\\r?$\\n"`

**Non-strict mode**:
- Pattern: `{escaped}` (no anchors)
- Example: `"Hello"` → `"Hello"`

---

### 4. ACTION: Escape Special Characters - EscapeSpecialRegExChars Method

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

#### Method Implementation (Lines 56-59)

```csharp
56:     private static string EscapeSpecialRegExChars(string line)
57:     {
58:         return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
59:         //     ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
60:         //     🔥 Regex replacement: prepend backslash to special chars
61:     }
```

**Regex Pattern**: `([\\()\[\]{}.*+?|^$])`
- Matches ANY of these characters: `\ ( ) [ ] { } . * + ? | ^ $`
- Captures them in group 1

**Replacement**: `\$1`
- Adds backslash before the captured character

**Examples**:

| Input | Output | Explanation |
|-------|--------|-------------|
| `Hello.World` | `Hello\.World` | `.` escaped |
| `$100` | `\$100` | `$` escaped |
| `a*b+c?` | `a\*b\+c\?` | `*`, `+`, `?` escaped |
| `[abc]` | `\[abc\]` | `[`, `]` escaped |
| `(test)` | `\(test\)` | `(`, `)` escaped |
| `a|b` | `a\|b` | `|` escaped |
| `^start` | `\^start` | `^` escaped |
| `end$` | `end\$` | `$` escaped |
| `path\file` | `path\\file` | `\` escaped |

---

### 5. Command Properties: ExpectFormatCommand.cs

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

#### Class Declaration and Properties (Lines 4-11)

```csharp
 4: class ExpectFormatCommand : ExpectBaseCommand
 5: {
 6:     public ExpectFormatCommand() : base()
 7:     {
 8:         Strict = true; // Default to true
 9:     }
10: 
11:     public bool Strict { get; set; }  // ← Controls strict vs non-strict formatting
```

**Analysis**:
- **Line 8**: Default is **strict mode** (`Strict = true`)
- **Line 11**: Only additional property beyond base class
- **NO validation properties**: No regex patterns, no AI instructions
- **NO execution properties**: No process handles, no parallelization

---

### 6. Output Generation: WriteOutput Method

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

#### Method Implementation (Lines 31-41)

```csharp
31:     protected void WriteOutput(string text)
32:     {
33:         if (string.IsNullOrEmpty(Output))
34:         {
35:             ConsoleHelpers.WriteLine(text, overrideQuiet: true);
36:             //              ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
37:             //              Write to stdout
38:         }
39:         else
40:         {
41:             FileHelpers.WriteAllText(Output, text);
42:             //          ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
43:             //          Write to file (--save-output)
44:         }
45:     }
```

**Analysis**:
- **Line 35**: If no `--save-output`, write to stdout
- **Line 41**: If `--save-output` specified, write to file
- **This is Layer 7 (Output Persistence), not Layer 9**
- Layer 9 is the transformation itself (FormatInput)

---

### 7. Transformation Examples with Evidence

#### Example 1: Simple Text (Strict Mode)

**Input**:
```
Hello World
```

**Transformation Steps**:
1. **EscapeSpecialRegExChars**: `"Hello World"` → `"Hello World"` (no special chars)
2. **Handle CRs**: `"Hello World"` → `"Hello World"` (no CRs)
3. **Add Anchors**: `"Hello World"` → `"^Hello World\\r?$\\n"`

**Output**:
```
^Hello World\r?$\n
```

#### Example 2: Special Characters (Strict Mode)

**Input**:
```
Total: $100.00 (50%)
```

**Transformation Steps**:
1. **EscapeSpecialRegExChars**: 
   - `"Total: $100.00 (50%)"` 
   - → `"Total: \$100\.00 \(50%\)"`
   - (Escaped: `$`, `.`, `(`, `)`)
2. **Handle CRs**: `"Total: \$100\.00 \(50%\)"` → (no CRs)
3. **Add Anchors**: `"Total: \$100\.00 \(50%\)"` → `"^Total: \$100\.00 \(50%\)\\r?$\\n"`

**Output**:
```
^Total: \$100\.00 \(50%\)\r?$\n
```

#### Example 3: Regex Pattern (Strict Mode)

**Input**:
```
Pattern: [a-z]+
```

**Transformation Steps**:
1. **EscapeSpecialRegExChars**: 
   - `"Pattern: [a-z]+"`
   - → `"Pattern: \[a-z\]\+"`
   - (Escaped: `[`, `]`, `+`)
2. **Handle CRs**: (no CRs)
3. **Add Anchors**: `"^Pattern: \[a-z\]\+\\r?$\\n"`

**Output**:
```
^Pattern: \[a-z\]\+\r?$\n
```

#### Example 4: Non-Strict Mode

**Input**:
```
Hello World
```

**Transformation Steps**:
1. **EscapeSpecialRegExChars**: `"Hello World"` → `"Hello World"`
2. **Handle CRs**: `"Hello World"` → `"Hello World"`
3. **No Anchors** (non-strict): `"Hello World"` → `"Hello World"`

**Output**:
```
Hello World
```

---

### 8. Call Graph: Complete Transformation Flow

```
ExpectFormatCommand.ExecuteAsync()
    └─> ExecuteFormat()                                     [ExpectFormatCommand.cs:23]
        ├─> FileHelpers.ReadAllText(Input)                 [Load input - Layer 1]
        │
        └─> FormatInput(input)                              [🔥 ACTION: Transform 🔥]
            ├─> input.Split('\n')                           [Split into lines]
            │
            └─> For each line:
                └─> FormatLine(line, Strict)                [Transform line]
                    ├─> EscapeSpecialRegExChars(line)       [🔥 Escape special chars]
                    │   └─> Regex.Replace(...)              [Regex transformation]
                    │
                    ├─> Handle CRs                          [🔥 Escape/trim CRs]
                    │   └─> Trim/Replace operations
                    │
                    └─> Add anchors (if strict)             [🔥 Wrap with ^...$\n]
                        └─> String interpolation
```

**Actions in call graph**: **ONE MAIN ACTION** (Transform input to regex patterns)

**Sub-actions**:
1. Escape special chars
2. Handle carriage returns
3. Add anchors and line endings

---

### 9. Comparison: `expect format` vs Other Commands

| Aspect | `expect format` | `expect check` | `run` |
|--------|----------------|----------------|-------|
| **Primary Action** | Transform input | Validate input | Execute tests |
| **Regex Operations** | ✅ Escape patterns | ✅ Match patterns | ✅ Match patterns |
| **AI Integration** | ❌ No | ✅ Yes (`--instructions`) | ❌ No |
| **Process Spawning** | ❌ No | ❌ No | ✅ Yes (per test) |
| **File Writing** | ⚠️ Optional | ⚠️ Optional | ✅ Always (reports) |
| **Exit Code Reflects** | Format success | Validation pass/fail | Test pass/fail |
| **Output Type** | Regex patterns (text) | Pass/fail (exit code) | Test results (XML) |

---

### 10. File System Impact Analysis

#### Files Read by `expect format`
- Input file (if `--input <file>` specified)
- OR stdin (if `--input -` or piped input)

#### Files Written by `expect format`
- **NONE** (by default - writes to stdout)
- ⚠️ Optional: Output file (if `--save-output` specified)

**Contrast with `run` command**:
- `run` **always** writes report file
- `expect format` **optionally** writes output file
- `expect format` primarily uses stdout

---

### 11. Exit Code Behavior

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs:39, 44`

```csharp
39:             return 0;  // Success - formatting completed
...
44:             return 1;  // Error - exception during formatting
```

**Exit Code Semantics**:
- **0**: Formatting succeeded (always, unless exception)
- **1**: Exception occurred (file not found, invalid input, etc.)

**NOT validation-based**:
```bash
# These all return 0 (success)
echo "anything" | cycodt expect format
echo "more stuff" | cycodt expect format --strict false
echo "{ invalid json }" | cycodt expect format
```

**Contrast with `expect check`**:
- `expect check` returns **0** if validation PASSES, **1** if validation FAILS
- `expect format` returns **0** if formatting COMPLETES, **1** only on ERROR

---

### 12. Integration Example: From Command to Test

#### Step 1: Run Command, Capture Output

```bash
$ my-command --option > raw-output.txt
$ cat raw-output.txt
Processing file: data.txt
Found 42 records
Total: $1,234.56
```

#### Step 2: Format Output to Regex Patterns

```bash
$ cycodt expect format --input raw-output.txt --save-output patterns.txt
$ cat patterns.txt
^Processing file: data\.txt\r?$\n
^Found 42 records\r?$\n
^Total: \$1,234\.56\r?$\n
```

#### Step 3: Use in Test Definition

```yaml
- name: Test my command
  run: my-command --option
  expect-regex:
    - ^Processing file: data\.txt\r?$\n
    - ^Found 42 records\r?$\n
    - ^Total: \$1,234\.56\r?$\n
```

#### Step 4: Run Test

```bash
$ cycodt run --test "Test my command"
Starting test: Test my command
✅ PASS: Test my command
```

---

### 13. Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| **Split lines** | O(n) | n = input length |
| **Escape chars** | O(n×m) | n = lines, m = avg chars per line |
| **Add anchors** | O(n) | Simple string interpolation |
| **Total** | O(n×m) | Linear in total input size |

**Comparison**:
- `run`: O(tests × execution_time) - Much slower (process spawning)
- `expect check`: O(n×p) where p = patterns - Similar (regex matching)
- `expect format`: O(n×m) - Fast (string operations only)

---

## Evidence Summary Table

| Evidence Type | Finding | Source | Layer 9 Action |
|---------------|---------|--------|----------------|
| **Transform Call** | `FormatInput(input)` | ExpectFormatCommand.cs:31 | ✅ ACTION |
| **Escape Special Chars** | `EscapeSpecialRegExChars(line)` | ExpectFormatCommand.cs:65 | ✅ Sub-action |
| **Regex Replacement** | `Regex.Replace(...)` | ExpectFormatCommand.cs:58 | ✅ Core transformation |
| **Handle CRs** | `Trim/Replace('\r')` | ExpectFormatCommand.cs:70-72 | ✅ Sub-action |
| **Add Anchors** | String interpolation | ExpectFormatCommand.cs:77 | ✅ Sub-action (strict mode) |
| **Output Generation** | `WriteOutput(formattedText)` | ExpectFormatCommand.cs:35 | ⚠️ Layer 7, not Layer 9 |
| **Process Spawning** | None | N/A | ❌ No execution |
| **Validation** | None | N/A | ❌ No validation |
| **Report Generation** | None | N/A | ❌ No reports |

---

## Conclusion

**The `expect format` command FULLY implements Layer 9 with ONE primary action:**

1. ✅ **ACTION: Transform Input to Regex Patterns**
   - Escapes regex special characters
   - Handles carriage returns
   - Adds anchors and line endings (strict mode)
   - **Evidence**: `FormatInput()` → `FormatLine()` → `EscapeSpecialRegExChars()`

2. ✅ **Exit Code Semantics**
   - Returns 0 on successful formatting
   - Returns 1 only on exception (file error, etc.)
   - **NOT validation-based** (unlike `expect check`)

3. ⚠️ **Output Options**
   - Default: stdout
   - Optional: file via `--save-output`
   - **Output mechanism is Layer 7, not Layer 9**

4. ❌ **NO Validation**
   - Does not check if patterns are "correct"
   - Pure transformation, no semantic analysis

5. ❌ **NO Execution**
   - Does not run processes (unlike `run`)
   - Does not spawn subprocesses

**Key Distinction**: `expect format` transforms, `expect check` validates, `run` executes.
