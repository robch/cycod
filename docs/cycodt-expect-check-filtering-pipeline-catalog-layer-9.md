# cycodt `expect check` Command - Layer 9: Actions on Results

## Layer Status: ✅ FULLY IMPLEMENTED

## Purpose

Layer 9 defines what **actions** are performed on the filtered and displayed results. For the `expect check` command, the primary action is:
- **Validating input** against expectations (regex patterns and/or AI instructions)
- **Returning pass/fail exit codes** (for automated workflows)

## Implementation in `expect check` Command

The `expect check` command **fully implements Layer 9** with a single primary action:

### Action: Validate Expectations

After loading input (Layer 1), the `expect check` command **validates the input** by:
1. Checking regex patterns (both positive and negative assertions)
2. Optionally checking AI-generated instructions
3. Returning exit code 0 (pass) or 1 (fail)

Unlike test execution commands, `expect check` does **not**:
- ❌ Execute external processes
- ❌ Generate complex reports
- ❌ Modify files (except optional `--save-output`)

The action is **validation-only**: check if input matches expectations.

## CLI Options

| Option | Argument | Description | Source | Layer |
|--------|----------|-------------|--------|-------|
| `--regex` | `<pattern>` | Regex pattern that MUST match input | Lines 65-70 | Layer 3 + 9 |
| `--not-regex` | `<pattern>` | Regex pattern that MUST NOT match | Lines 72-77 | Layer 3 + 9 |
| `--instructions` | `<text>` | AI instructions for validation | Lines 79-84 | Layer 8 + 9 |

### Option Parsing

#### `--regex` (CycoDtCommandLineOptions.cs, Lines 65-70)

```csharp
65:         else if (command is ExpectCheckCommand checkCommand && arg == "--regex")
66:         {
67:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
68:             var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "regex pattern");
69:             checkCommand.RegexPatterns.Add(pattern!);
70:             i += max1Arg.Count();
```

#### `--not-regex` (CycoDtCommandLineOptions.cs, Lines 72-77)

```csharp
72:         else if (command is ExpectCheckCommand checkCommand3 && arg == "--not-regex")
73:         {
74:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
75:             var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "not-regex pattern");
76:             checkCommand3.NotRegexPatterns.Add(pattern!);
77:             i += max1Arg.Count();
```

#### `--instructions` (CycoDtCommandLineOptions.cs, Lines 79-84)

```csharp
79:         else if (command is ExpectCheckCommand checkCommand5 && arg == "--instructions")
80:         {
81:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
82:             var instructions = ValidateString(arg, max1Arg.FirstOrDefault(), "instructions");
83:             checkCommand5.Instructions = instructions;
84:             i += max1Arg.Count();
```

## Data Flow

```
Input (from --input or stdin)
    ↓
Read as lines
    ↓
┌─────────────────────────────────────────┐
│  ACTION: Validate Expectations          │
│  1. Check regex patterns (positive)     │
│  2. Check not-regex patterns (negative) │
│  3. Check AI instructions (optional)    │
└─────────────────────────────────────────┘
    ↓
Pass or Fail determination
    ↓
Console output (PASS! or FAILED!)
    ↓
Exit Code (0 = pass, 1 = fail)
```

## Source Code Evidence

See [Layer 9 Proof Document](cycodt-expect-check-filtering-pipeline-catalog-layer-9-proof.md) for detailed source code analysis.

### Key Implementation Details

#### 1. Regex Pattern Checking (ExpectCheckCommand.cs, Lines 41-46)

```csharp
41:             var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
42:             if (!linesOk)
43:             {
44:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
45:                 return 1;  // ← Exit code 1: Expectations not met
46:             }
```

**What happens**:
- `ExpectHelper.CheckLines()` validates all regex patterns
- **Positive patterns** (`--regex`): At least one line must match each pattern
- **Negative patterns** (`--not-regex`): No lines must match these patterns
- Returns `false` if any expectations fail

#### 2. AI Instructions Checking (ExpectCheckCommand.cs, Lines 48-53)

```csharp
48:             var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
49:             if (!instructionsOk)
50:             {
51:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
52:                 return 1;  // ← Exit code 1: AI validation failed
53:             }
```

**What happens**:
- `CheckExpectInstructionsHelper.CheckExpectations()` uses AI to validate input
- AI evaluates whether input meets the specified instructions
- Returns `false` if AI determines expectations are not met

#### 3. Success Exit (ExpectCheckCommand.cs, Lines 55-56)

```csharp
55:             ConsoleHelpers.WriteLine($"\r{message} PASS!");
56:             return 0;  // ← Exit code 0: All expectations met
```

**What happens**:
- If all regex and AI checks pass, display "PASS!"
- Return exit code 0 (success)

## Validation Logic

### Regex Validation (Positive Patterns)

**For each `--regex` pattern**:
1. Compile regex pattern
2. Test against ALL input lines
3. **PASS**: If ANY line matches the pattern
4. **FAIL**: If NO lines match the pattern

**Example**:
```bash
echo "Hello World" | cycodt expect check --regex "^Hello"
# EXIT CODE: 0 (PASS) - "Hello World" matches "^Hello"

echo "Goodbye" | cycodt expect check --regex "^Hello"
# EXIT CODE: 1 (FAIL) - "Goodbye" does NOT match "^Hello"
```

### Regex Validation (Negative Patterns)

**For each `--not-regex` pattern**:
1. Compile regex pattern
2. Test against ALL input lines
3. **PASS**: If NO lines match the pattern
4. **FAIL**: If ANY line matches the pattern

**Example**:
```bash
echo "Hello World" | cycodt expect check --not-regex "Error"
# EXIT CODE: 0 (PASS) - "Hello World" does NOT contain "Error"

echo "Error occurred" | cycodt expect check --not-regex "Error"
# EXIT CODE: 1 (FAIL) - "Error occurred" contains "Error"
```

### AI Validation

**If `--instructions` is provided**:
1. Send input text + instructions to AI
2. AI evaluates whether input meets instructions
3. **PASS**: If AI confirms expectations are met
4. **FAIL**: If AI determines expectations are not met

**Example**:
```bash
echo '{"name": "John", "age": 30}' | cycodt expect check \
  --instructions "Verify this is valid JSON with a name and age field"
# EXIT CODE: 0 (PASS) - AI confirms valid JSON structure

echo 'not json' | cycodt expect check \
  --instructions "Verify this is valid JSON"
# EXIT CODE: 1 (FAIL) - AI detects invalid JSON
```

## Exit Code Behavior

The `expect check` command's exit code is designed for **automated validation workflows**:

```bash
# Shell script example
command-to-test | cycodt expect check --regex "SUCCESS" --not-regex "ERROR"
if [ $? -eq 0 ]; then
  echo "✅ Output is valid"
else
  echo "❌ Output validation failed"
  exit 1
fi
```

**Exit codes**:
- **0**: All expectations met (regex + AI)
- **1**: One or more expectations failed
- **1**: Exception during validation

## Comparison with Other Commands

| Command | Primary Action | Modifies State | Exit Code Reflects |
|---------|----------------|----------------|-------------------|
| `list` | Display tests | ❌ No | Success of listing |
| `run` | Execute tests | ✅ Yes (reports) | Test pass/fail |
| **`expect check`** | **Validate input** | ❌ No | **Expectation pass/fail** |
| `expect format` | Transform input | ⚠️ Output only | Success of formatting |

## Use Cases

### 1. Command Output Validation

```bash
# Validate that a command produces expected output
my-cli-tool --version | cycodt expect check \
  --regex "^version [0-9]+\.[0-9]+\.[0-9]+$"
```

### 2. Log File Validation

```bash
# Check log file for errors
cycodt expect check --input app.log \
  --not-regex "ERROR" \
  --not-regex "FATAL"
```

### 3. JSON/XML Validation

```bash
# Validate API response structure
curl https://api.example.com/data | cycodt expect check \
  --instructions "Verify this is valid JSON with an 'id' and 'status' field"
```

### 4. Test Output Verification

```bash
# Verify test command output
dotnet test | cycodt expect check \
  --regex "Total tests: [0-9]+" \
  --regex "Passed: [0-9]+" \
  --not-regex "Failed: [1-9]"  # Ensure 0 failures
```

### 5. Build Output Validation

```bash
# Validate build succeeded
dotnet build | cycodt expect check \
  --regex "Build succeeded" \
  --not-regex "error" \
  --not-regex "warning"
```

## Related Layers

- **Layer 1** ([Target Selection](cycodt-expect-check-filtering-pipeline-catalog-layer-1.md)): Determines input source (file or stdin)
- **Layer 3** ([Content Filter](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md)): Defines regex patterns to check
- **Layer 6** ([Display Control](cycodt-expect-check-filtering-pipeline-catalog-layer-6.md)): Controls pass/fail message display
- **Layer 7** ([Output Persistence](cycodt-expect-check-filtering-pipeline-catalog-layer-7.md)): Optional output saving
- **Layer 8** ([AI Processing](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md)): AI-based instruction validation
- **Contrast with `run` Layer 9** ([run Actions](cycodt-run-filtering-pipeline-catalog-layer-9.md)): Shows validation vs execution

## Performance Characteristics

- **Fast validation**: Regex checks are O(n×m) where n=lines, m=patterns
- **No external processes**: Unlike `run`, no process spawning
- **Streaming capable**: Can validate stdin in real-time
- **AI validation**: Adds latency if `--instructions` is used (network call)

## Limitations

- **No partial success**: Either ALL expectations pass or command fails
- **No detailed reporting**: Only displays first failure reason
- **No report files**: Unlike `run`, no structured output files (only exit code)
- **Single input source**: Cannot validate multiple files in one invocation
