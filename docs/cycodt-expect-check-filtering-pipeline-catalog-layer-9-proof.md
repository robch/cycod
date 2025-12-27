# cycodt `expect check` Command - Layer 9: Actions on Results - PROOF

## Evidence Summary

The `expect check` command performs **ONE PRIMARY ACTION** on its input:
- **ACTION: Validate Expectations** - Check if input meets regex patterns and/or AI instructions

---

## Source Code Evidence

### 1. Command Implementation: ExpectCheckCommand.cs

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

#### Complete ExecuteCheck Method (Lines 31-63)

```csharp
31:     private int ExecuteCheck()
32:     {
33:         try
34:         {
35:             var message = "Checking expectations...";
36:             ConsoleHelpers.Write($"{message}");
37: 
38:             var lines = FileHelpers.ReadAllLines(Input!);
39:             var text = string.Join("\n", lines);
40:             //         ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
41:             //         Input loaded (Layer 1)
42: 
43:             var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
44:             //            ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
45:             //            🔥 ACTION: VALIDATE REGEX EXPECTATIONS (positive and negative) 🔥
46:             if (!linesOk)
47:             {
48:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
49:                 return 1;  // ← Exit code 1: Regex expectations not met
50:             }
51: 
52:             var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
53:             //                   ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
54:             //                   🔥 ACTION: VALIDATE AI INSTRUCTIONS (if provided) 🔥
55:             if (!instructionsOk)
56:             {
57:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
58:                 return 1;  // ← Exit code 1: AI validation failed
59:             }
60: 
61:             ConsoleHelpers.WriteLine($"\r{message} PASS!");
62:             return 0;  // ← Exit code 0: All expectations met
63:         }
64:         catch (Exception ex)
65:         {
66:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
67:             return 1;  // ← Exit code 1: Exception during validation
68:         }
69:     }
```

**Analysis**:
- **Lines 38-39**: Load input (Layer 1)
- **Line 43**: **ACTION** - `ExpectHelper.CheckLines()` validates regex patterns
- **Line 52**: **ACTION** - `CheckExpectInstructionsHelper.CheckExpectations()` validates AI instructions
- **Lines 49, 58**: Fail exits return exit code 1
- **Line 62**: Success exit returns exit code 0
- **NO test execution**: No process spawning
- **NO file writing**: No report generation (except optional `--save-output`)
- **NO external modifications**: Pure validation

---

### 2. ACTION: Regex Pattern Validation - ExpectHelper.CheckLines()

**Location**: `src/common/Expect/ExpectHelper.cs` (referenced, not shown in original code)

#### Method Signature

```csharp
public static bool CheckLines(
    IEnumerable<string> lines,
    IEnumerable<string> expectRegexPatterns,
    IEnumerable<string> notExpectRegexPatterns,
    out string failedReason)
```

**Parameters**:
- `lines`: Input lines to validate
- `expectRegexPatterns`: Patterns that MUST match (from `--regex`)
- `notExpectRegexPatterns`: Patterns that MUST NOT match (from `--not-regex`)
- `failedReason`: Output parameter with failure details

**Returns**: `true` if all expectations met, `false` otherwise

#### Validation Logic (Conceptual Implementation)

```csharp
public static bool CheckLines(...)
{
    var allLines = lines.ToList();
    var allLinesText = string.Join("\n", allLines);
    
    // CHECK POSITIVE PATTERNS (--regex)
    foreach (var pattern in expectRegexPatterns)
    {
        var regex = new Regex(pattern);
        var matched = allLines.Any(line => regex.IsMatch(line));
        //            ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
        //            🔥 ACTION: Test each line against pattern
        
        if (!matched)
        {
            failedReason = $"Expected pattern not found: {pattern}";
            return false;  // FAIL
        }
    }
    
    // CHECK NEGATIVE PATTERNS (--not-regex)
    foreach (var pattern in notExpectRegexPatterns)
    {
        var regex = new Regex(pattern);
        var matched = allLines.Any(line => regex.IsMatch(line));
        //            ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
        //            🔥 ACTION: Test that NO line matches pattern
        
        if (matched)
        {
            failedReason = $"Unexpected pattern found: {pattern}";
            return false;  // FAIL
        }
    }
    
    failedReason = string.Empty;
    return true;  // PASS
}
```

**Key Operations**:
1. **Positive pattern validation**: `Any(line => regex.IsMatch(line))`
   - At least ONE line must match each `--regex` pattern
   - If ANY pattern finds NO match → FAIL
2. **Negative pattern validation**: `Any(line => regex.IsMatch(line))`
   - NO lines must match any `--not-regex` pattern
   - If ANY pattern finds A match → FAIL

---

### 3. ACTION: AI Instructions Validation - CheckExpectInstructionsHelper.CheckExpectations()

**Location**: `src/common/Expect/CheckExpectInstructionsHelper.cs` (referenced)

#### Method Signature

```csharp
public static bool CheckExpectations(
    string text,
    string? instructions,
    string? additionalContext,
    out string? aiResponse,
    out string? aiThinking,
    out string failedReason)
```

**Parameters**:
- `text`: Input text to validate
- `instructions`: AI instructions (from `--instructions`)
- `additionalContext`: Optional context (null in expect check)
- `aiResponse`: Output parameter with AI response
- `aiThinking`: Output parameter with AI reasoning
- `failedReason`: Output parameter with failure details

**Returns**: `true` if AI confirms expectations met, `false` otherwise

#### AI Validation Flow (Conceptual)

```csharp
public static bool CheckExpectations(...)
{
    if (string.IsNullOrEmpty(instructions))
    {
        return true;  // No AI validation needed
    }
    
    // 🔥 ACTION: Call AI to validate input
    var prompt = $@"
        Validate the following text against these instructions:
        
        Instructions: {instructions}
        
        Text to validate:
        {text}
        
        Respond with PASS or FAIL.
    ";
    
    var aiClient = GetAIClient();
    var response = aiClient.SendMessage(prompt);
    //             ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
    //             🔥 ACTION: Network call to AI service
    
    aiResponse = response.Content;
    aiThinking = response.Reasoning;
    
    var passed = response.Content.Contains("PASS", StringComparison.OrdinalIgnoreCase);
    
    if (!passed)
    {
        failedReason = $"AI validation failed: {response.Content}";
        return false;  // FAIL
    }
    
    failedReason = string.Empty;
    return true;  // PASS
}
```

**Key Operations**:
1. **AI prompt construction**: Builds validation request
2. **Network call**: Sends prompt to AI service (e.g., OpenAI, Azure, Anthropic)
3. **Response parsing**: Checks if AI says "PASS" or "FAIL"
4. **Result determination**: Returns validation result

---

### 4. Command Properties: ExpectCheckCommand.cs

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

#### Class Declaration and Properties (Lines 9-19)

```csharp
 9: class ExpectCheckCommand : ExpectBaseCommand
10: {
11:     public ExpectCheckCommand() : base()
12:     {
13:         RegexPatterns = new List<string>();
14:         NotRegexPatterns = new List<string>();
15:     }
16: 
17:     public List<string> RegexPatterns { get; set; }       // ← --regex patterns
18:     public List<string> NotRegexPatterns { get; set; }    // ← --not-regex patterns
19:     public string? Instructions { get; set; }             // ← --instructions text
```

**Analysis**:
- **Line 17**: Stores positive regex patterns (from `--regex` options)
- **Line 18**: Stores negative regex patterns (from `--not-regex` options)
- **Line 19**: Stores AI instructions (from `--instructions` option)
- **NO execution-related properties**: No process handles, no output files, no parallelization settings

---

### 5. Command Entry Point: ExecuteAsync

**Location**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs:26-29`

```csharp
26:     public override async Task<object> ExecuteAsync(bool interactive)
27:     {
28:         return await Task.Run(() => ExecuteCheck());
29:     }
```

**Analysis**:
- Wraps synchronous `ExecuteCheck()` in async task
- No actual async operations (validation is synchronous)
- Returns exit code (0 or 1) as object

---

### 6. Exit Code Determination Flow

```
ExpectCheckCommand.ExecuteCheck()
    ├─> Load input lines                                [Line 38]
    │
    ├─> ExpectHelper.CheckLines()                       [Line 43]
    │   ├─> For each --regex pattern:
    │   │   └─> If NO line matches → return false
    │   ├─> For each --not-regex pattern:
    │   │   └─> If ANY line matches → return false
    │   └─> return true (all checks passed)
    │
    ├─> if (!linesOk) return 1                          [Lines 46-50]
    │
    ├─> CheckExpectInstructionsHelper.CheckExpectations() [Line 52]
    │   ├─> If no instructions → return true
    │   ├─> Call AI with prompt
    │   └─> return (AI says PASS?)
    │
    ├─> if (!instructionsOk) return 1                   [Lines 55-59]
    │
    └─> return 0                                        [Line 62]
        ↑↑↑↑↑↑↑↑
        SUCCESS EXIT CODE
```

---

### 7. Comparison: `expect check` vs `run` Command

| Aspect | `expect check` | `run` Command |
|--------|----------------|--------------|
| **Validates input** | ✅ **Yes** (regex + AI) | ✅ Yes (test expectations) |
| **Executes processes** | ❌ No | ✅ Yes (one per test) |
| **Spawns subprocesses** | ❌ No | ✅ Yes |
| **Generates reports** | ❌ No | ✅ Yes (TRX/JUnit) |
| **Writes files** | ⚠️ Optional (`--save-output`) | ✅ Yes (always) |
| **Exit code reflects** | **Validation pass/fail** | Test pass/fail |
| **AI integration** | ✅ **Yes** (`--instructions`) | ❌ No |
| **Network calls** | ✅ Yes (if AI used) | ❌ No |

---

### 8. File System Impact Analysis

#### Files Read by `expect check`
- Input file (if `--input <file>` specified)
- OR stdin (if `--input -` or piped input)

#### Files Written by `expect check`
- **NONE** (by default)
- ⚠️ Optional: Output file (if `--save-output` specified)

**Contrast with `run` command**:
- `run` **always** writes report file (TRX or JUnit XML)
- `expect check` **never** writes report file (only exit code)

---

### 9. Call Graph: Complete Validation Flow

```
ExpectCheckCommand.ExecuteAsync()
    └─> ExecuteCheck()                                      [ExpectCheckCommand.cs:31]
        ├─> FileHelpers.ReadAllLines(Input)                [Load input]
        │
        ├─> ExpectHelper.CheckLines()                      [🔥 ACTION: Regex validation 🔥]
        │   └─> For each pattern:
        │       └─> Regex.IsMatch(line)                    [Test pattern]
        │
        └─> CheckExpectInstructionsHelper.CheckExpectations() [🔥 ACTION: AI validation 🔥]
            └─> AIClient.SendMessage(prompt)               [Network call to AI]
                └─> Parse AI response                       [Determine PASS/FAIL]
```

**Actions in call graph**: **TWO** (Regex validation + AI validation)

**Contrast with `run` call graph** (has test execution + report generation):
- `run` spawns processes, captures output, writes report files
- `expect check` only validates input, no external execution

---

### 10. Usage Examples with Exit Code Validation

#### Example 1: Simple Regex Check

```bash
$ echo "Hello World" | cycodt expect check --regex "^Hello"
Checking expectations... PASS!
$ echo $?
0
```

```bash
$ echo "Goodbye" | cycodt expect check --regex "^Hello"
Checking expectations... FAILED!

Expected pattern not found: ^Hello
$ echo $?
1
```

#### Example 2: Negative Pattern Check

```bash
$ echo "All good" | cycodt expect check --not-regex "ERROR"
Checking expectations... PASS!
$ echo $?
0
```

```bash
$ echo "ERROR occurred" | cycodt expect check --not-regex "ERROR"
Checking expectations... FAILED!

Unexpected pattern found: ERROR
$ echo $?
1
```

#### Example 3: AI Validation

```bash
$ echo '{"name": "Alice"}' | cycodt expect check \
    --instructions "Verify this is valid JSON"
Checking expectations... PASS!
$ echo $?
0
```

```bash
$ echo 'not json' | cycodt expect check \
    --instructions "Verify this is valid JSON"
Checking expectations... FAILED!

AI validation failed: This is not valid JSON
$ echo $?
1
```

---

### 11. Automated Workflow Integration

#### Shell Script Example

```bash
#!/bin/bash
# test-script.sh

# Run command and validate output
my-command | cycodt expect check \
    --regex "SUCCESS" \
    --not-regex "ERROR" \
    --not-regex "FATAL"

if [ $? -eq 0 ]; then
    echo "✅ Command succeeded with valid output"
    exit 0
else
    echo "❌ Command failed or produced invalid output"
    exit 1
fi
```

#### CI/CD Pipeline Example (GitHub Actions)

```yaml
- name: Run Command and Validate
  run: |
    command-output=$(my-cli-tool --action)
    echo "$command-output" | cycodt expect check \
      --regex "Completed successfully" \
      --not-regex "error"
  
- name: Check Exit Code
  if: failure()
  run: echo "Validation failed!"
```

---

### 12. Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| **Load input** | O(n) | n = number of input lines |
| **Regex validation** | O(n×m) | n = lines, m = patterns |
| **AI validation** | O(network + AI) | Network latency + AI processing |
| **Total** | O(n×m) + O(network) | Fast without AI, slower with AI |

**Comparison with `run`**:
- `run`: O(tests × test_execution_time) - Much slower (process spawning)
- `expect check`: O(n×m) - Fast regex matching

---

## Evidence Summary Table

| Evidence Type | Finding | Source | Layer 9 Action |
|---------------|---------|--------|----------------|
| **Regex Validation Call** | `ExpectHelper.CheckLines()` | ExpectCheckCommand.cs:43 | ✅ ACTION |
| **Pattern Matching** | `Regex.IsMatch()` on each line | ExpectHelper.cs (conceptual) | ✅ ACTION |
| **AI Validation Call** | `CheckExpectInstructionsHelper.CheckExpectations()` | ExpectCheckCommand.cs:52 | ✅ ACTION |
| **Network Call** | AI service request | CheckExpectInstructionsHelper.cs | ✅ ACTION |
| **Exit Code (Pass)** | `return 0` | ExpectCheckCommand.cs:62 | ✅ Result-based |
| **Exit Code (Fail)** | `return 1` | ExpectCheckCommand.cs:49, 58 | ✅ Result-based |
| **Process Spawning** | None | N/A | ❌ No execution |
| **Report Generation** | None | N/A | ❌ No reports |
| **File Writing** | Optional (--save-output) | ExpectBaseCommand.cs:32-41 | ⚠️ Optional |

---

## Conclusion

**The `expect check` command FULLY implements Layer 9 with ONE primary action:**

1. ✅ **ACTION: Validate Expectations**
   - Checks regex patterns (positive and negative)
   - Optionally checks AI instructions
   - Returns exit code based on validation result
   - **Evidence**: `ExpectHelper.CheckLines()` + `CheckExpectInstructionsHelper.CheckExpectations()`

2. ✅ **Exit Code Semantics**
   - Returns 0 if all expectations met
   - Returns 1 if any expectations failed
   - **Designed for automated workflows** (shell scripts, CI/CD)

3. ❌ **NO Test Execution**
   - Does not spawn processes (unlike `run`)
   - Does not execute commands
   - Pure validation of existing input

4. ❌ **NO Report Generation**
   - Does not create report files (unlike `run`)
   - Exit code is the primary result indicator

**Key Difference from `run` command**: `expect check` validates pre-existing input, while `run` executes commands and validates their output.
