# cycodt `expect format` Command - Layer 9: Actions on Results

## Layer Status: ✅ FULLY IMPLEMENTED

## Purpose

Layer 9 defines what **actions** are performed on the filtered and displayed results. For the `expect format` command, the primary action is:
- **Transforming input** into regex patterns suitable for test expectations
- **Outputting formatted patterns** to stdout or file

## Implementation in `expect format` Command

The `expect format` command **fully implements Layer 9** with a single primary action:

### Action: Transform Input to Regex Patterns

After loading input (Layer 1), the `expect format` command **transforms the input** by:
1. Escaping regex special characters in each line
2. Optionally handling carriage returns based on `--strict` mode
3. Wrapping lines with regex anchors (`^...$`) in strict mode
4. Appending `\r?\n` line ending patterns in strict mode
5. Outputting the transformed patterns

Unlike validation commands, `expect format` does **not**:
- ❌ Validate expectations (that's `expect check`)
- ❌ Execute processes
- ❌ Generate reports

The action is **transformation-only**: convert raw input to regex patterns.

## CLI Options

| Option | Argument | Description | Default | Source | Layer |
|--------|----------|-------------|---------|--------|-------|
| `--strict` | `true|false` | Enable strict formatting | `true` | Lines 55-63 | Layer 9 |

### Option Parsing

#### `--strict` (CycoDtCommandLineOptions.cs, Lines 55-63)

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
```

## Data Flow

```
Input (from --input or stdin)
    ↓
Read as text
    ↓
Split into lines
    ↓
┌─────────────────────────────────────┐
│  ACTION: Transform to Regex         │
│  For each line:                     │
│  1. Escape regex special chars      │
│  2. Handle carriage returns         │
│  3. Add anchors (if --strict)       │
│  4. Add line ending pattern         │
└─────────────────────────────────────┘
    ↓
Formatted regex patterns
    ↓
Output to stdout or --save-output file
```

## Source Code Evidence

See [Layer 9 Proof Document](cycodt-expect-format-filtering-pipeline-catalog-layer-9-proof.md) for detailed source code analysis.

### Key Implementation Details

#### 1. Format Input (ExpectFormatCommand.cs, Lines 39-54)

```csharp
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
49:             var formatted = FormatLine(line, Strict);  // ← Transform each line
50:             formattedLines.Add(formatted);
51:         }
52: 
53:         return string.Join("\n", formattedLines);
54:     }
```

**What happens**:
- **Line 44**: Split input into lines
- **Lines 47-51**: Transform each line individually
- **Line 53**: Join formatted lines back together

#### 2. Format Line (ExpectFormatCommand.cs, Lines 61-77)

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

**What happens**:
- **Line 65**: Escape special regex characters
- **Lines 68-70**: Handle carriage returns
  - Strict mode: Trim CRs, then escape
  - Non-strict: Just escape
- **Line 73**: Add anchors and line ending pattern
  - Strict mode: `^{escaped}\r?$\n` (anchored with optional CR)
  - Non-strict: Just return escaped text

#### 3. Escape Special Characters (ExpectFormatCommand.cs, Lines 56-59)

```csharp
56:     private static string EscapeSpecialRegExChars(string line)
57:     {
58:         return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
59:     }
```

**What happens**:
- Escapes these regex special characters: `\ ( ) [ ] { } . * + ? | ^ $`
- Example: `Hello.World` → `Hello\.World`
- Example: `Total: $10` → `Total: \$10`

## Formatting Modes

### Strict Mode (Default: `--strict true`)

**Pattern**: `^{escaped}\r?$\n`

**Behavior**:
- **`^`**: Anchor to start of line
- **`{escaped}`**: Escaped line content
- **`\r?`**: Optional carriage return
- **`$`**: Anchor to end of line
- **`\n`**: Newline character

**Use case**: Exact line matching (no extra content allowed)

**Example**:
```bash
Input:  "Hello World"
Output: "^Hello World\\r?$\\n"
```

### Non-Strict Mode (`--strict false`)

**Pattern**: `{escaped}` (no anchors, no line ending)

**Behavior**:
- Just escapes special characters
- No line anchors
- No line ending patterns

**Use case**: Partial line matching (allow extra content before/after)

**Example**:
```bash
Input:  "Hello World"
Output: "Hello World"
```

## Usage Examples

### Example 1: Basic Formatting (Strict Mode)

**Input**:
```bash
echo "Hello World" | cycodt expect format
```

**Output**:
```
^Hello World\r?$\n
```

**Usage in tests**:
```yaml
- name: Test greeting
  run: echo "Hello World"
  expect-regex:
    - ^Hello World\r?$\n
```

### Example 2: Special Characters Escaping

**Input**:
```bash
echo "Total: $100 (50%)" | cycodt expect format
```

**Output**:
```
^Total: \$100 \(50%\)\r?$\n
```

**Notice**: `$`, `(`, `)` are escaped

### Example 3: Non-Strict Mode

**Input**:
```bash
echo "Hello World" | cycodt expect format --strict false
```

**Output**:
```
Hello World
```

**Usage**: Allows partial matching:
```yaml
expect-regex:
  - Hello World  # Matches "Prefix Hello World Suffix"
```

### Example 4: Multiple Lines

**Input**:
```bash
cat << EOF | cycodt expect format
Line 1: Start
Line 2: Middle
Line 3: End
EOF
```

**Output**:
```
^Line 1: Start\r?$\n
^Line 2: Middle\r?$\n
^Line 3: End\r?$\n
```

### Example 5: File Input with Output

**Input**:
```bash
cycodt expect format --input output.txt --save-output patterns.txt
```

**Result**:
- Reads `output.txt`
- Formats each line
- Writes patterns to `patterns.txt`

## Typical Workflow

### 1. Generate Expected Output

```bash
# Run command to get expected output
my-command --option > expected.txt
```

### 2. Convert to Regex Patterns

```bash
# Format for use in tests
cycodt expect format --input expected.txt --save-output patterns.txt
```

### 3. Use in Test Definition

```yaml
- name: Test my command
  run: my-command --option
  expect-regex-file: patterns.txt
```

### Alternative: Inline Pattern Generation

```bash
# Generate patterns inline
my-command --option | cycodt expect format > patterns.txt
```

## Escape Character Reference

| Input Character | Escaped Output | Reason |
|----------------|----------------|--------|
| `\` | `\\` | Escape character |
| `(` | `\(` | Regex group start |
| `)` | `\)` | Regex group end |
| `[` | `\[` | Regex character class start |
| `]` | `\]` | Regex character class end |
| `{` | `\{` | Regex quantifier start |
| `}` | `\}` | Regex quantifier end |
| `.` | `\.` | Regex wildcard (any char) |
| `*` | `\*` | Regex quantifier (0 or more) |
| `+` | `\+` | Regex quantifier (1 or more) |
| `?` | `\?` | Regex quantifier (0 or 1) |
| `|` | `\|` | Regex alternation |
| `^` | `\^` | Regex line start anchor |
| `$` | `\$` | Regex line end anchor |

## Comparison with Other Commands

| Command | Primary Action | Input Type | Output Type |
|---------|----------------|-----------|-------------|
| `list` | Display tests | Test files | Test names (display) |
| `run` | Execute tests | Test files | Test results (report files) |
| `expect check` | Validate input | Text | Pass/fail (exit code) |
| **`expect format`** | **Transform input** | **Text** | **Regex patterns (text)** |

## Exit Code Behavior

The `expect format` command's exit code reflects **formatting success/failure**, not validation:

**Exit codes**:
- **0**: Formatting succeeded
- **1**: Exception during formatting (e.g., file not found, invalid input)

**Not used for validation** (unlike `expect check`):
```bash
# This always returns 0 (unless file error)
echo "anything" | cycodt expect format
echo $?  # Output: 0
```

## Related Layers

- **Layer 1** ([Target Selection](cycodt-expect-format-filtering-pipeline-catalog-layer-1.md)): Determines input source (file or stdin)
- **Layer 6** ([Display Control](cycodt-expect-format-filtering-pipeline-catalog-layer-6.md)): Controls output format (strict vs non-strict)
- **Layer 7** ([Output Persistence](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md)): Where to save formatted output
- **Contrast with `expect check` Layer 9** ([expect check Actions](cycodt-expect-check-filtering-pipeline-catalog-layer-9.md)): Shows transformation vs validation

## Performance Characteristics

- **Fast transformation**: O(n×m) where n=lines, m=avg line length
- **No network calls**: Unlike `expect check` with AI, purely local
- **Streaming capable**: Can process large inputs line-by-line
- **Memory efficient**: Processes one line at a time

## Use Cases

### 1. Test Creation from Command Output

```bash
# Capture expected output and create test
my-new-feature | cycodt expect format > expected-patterns.txt

# Add to test file
cat >> test.yaml << EOF
- name: Test new feature
  run: my-new-feature
  expect-regex-file: expected-patterns.txt
EOF
```

### 2. Updating Test Expectations

```bash
# Run command with new behavior
my-command-v2 | cycodt expect format > new-patterns.txt

# Replace old expectations
mv new-patterns.txt tests/expected/my-command-patterns.txt
```

### 3. Partial Pattern Generation (Non-Strict)

```bash
# Generate flexible patterns
echo "Important: result" | cycodt expect format --strict false
# Output: Important: result

# Matches:
# - "Important: result"
# - "Prefix: Important: result"
# - "Important: result - suffix"
```

### 4. Bulk Pattern Generation

```bash
# Format multiple test outputs
for test in test1 test2 test3; do
  ./$test | cycodt expect format > $test-patterns.txt
done
```

## Limitations

- **No validation**: Does not check if output is "correct", only transforms it
- **Line-by-line only**: Cannot format multi-line regex patterns (e.g., spanning lines)
- **No context awareness**: Does not understand semantic meaning of input
- **Fixed transformation**: Cannot customize escape rules beyond strict/non-strict

## Future Enhancement Opportunities

While not currently implemented, `expect format` could support:
1. **Custom escape rules**: Allow users to specify which characters to escape
2. **Pattern templates**: Pre-defined formatting templates (JSON, XML, etc.)
3. **Smart formatting**: Detect input type and apply appropriate formatting
4. **Multi-line patterns**: Support for patterns that span multiple lines
5. **Variable substitution**: Replace specific values with regex patterns (e.g., timestamps → `\d+`)
