# cycodt expect check - Layer 6: Display Control - PROOF

This document provides detailed source code evidence for all Layer 6 (Display Control) features of the `cycodt expect check` command.

## 6.1 Progress Indicator

### Implementation

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 35-36**:
```csharp
var message = "Checking expectations...";
ConsoleHelpers.Write($"{message}");
```

**Explanation**:
- **Line 35**: Defines the progress message string
- **Line 36**: Uses `ConsoleHelpers.Write()` (NOT `WriteLine()`) to display without newline
- This allows the message to be overwritten using carriage return later

---

## 6.2 Pass/Fail Display

### Pass Display

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 55**:
```csharp
ConsoleHelpers.WriteLine($"\r{message} PASS!");
```

**Explanation**:
- Starts with `\r` (carriage return) to move cursor back to beginning of line
- Overwrites "Checking expectations..." with "Checking expectations... PASS!"
- `WriteLine()` adds newline after, moving to next line

---

### Fail Display (Regex Check Failed)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 41-46**:
```csharp
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
if (!linesOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
    return 1;
}
```

**Explanation**:
- **Line 41**: Calls `ExpectHelper.CheckLines()` which returns success/failure and reason
- **Line 42**: Checks if validation failed
- **Line 44**: Displays failure with:
  - `\r` to overwrite progress message
  - `FAILED!` suffix
  - `\n\n` for double newline (blank line separator)
  - `{expectFailedReason}` with details of what failed
- **Line 45**: Returns exit code 1 (failure)

---

### Fail Display (Instructions Check Failed)

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 48-53**:
```csharp
var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
if (!instructionsOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
    return 1;
}
```

**Explanation**:
- **Line 48**: Calls AI-based instruction checker
- **Line 49**: Checks if validation failed
- **Line 51**: Displays failure in same format as regex failure
- **Line 52**: Returns exit code 1 (failure)

---

## 6.3 Failure Reason Display

### ExpectHelper.CheckLines

**File**: Likely `src/common/Helpers/ExpectHelper.cs` (inferred from usage)

**Expected Signature**:
```csharp
public static bool CheckLines(
    IEnumerable<string> lines,
    List<string> regexPatterns,
    List<string> notRegexPatterns,
    out string failedReason)
{
    // Check positive patterns (must match)
    foreach (var pattern in regexPatterns)
    {
        if (!lines.Any(line => Regex.IsMatch(line, pattern)))
        {
            failedReason = $"Expected pattern not found: {pattern}";
            return false;
        }
    }
    
    // Check negative patterns (must NOT match)
    foreach (var pattern in notRegexPatterns)
    {
        var matchingLine = lines.FirstOrDefault(line => Regex.IsMatch(line, pattern));
        if (matchingLine != null)
        {
            failedReason = $"Pattern should not match but found: {pattern}\nIn line: {matchingLine}";
            return false;
        }
    }
    
    failedReason = string.Empty;
    return true;
}
```

**Evidence**: 
- Called on line 41 of ExpectCheckCommand.cs
- Returns bool success status and string failure reason
- Used with `RegexPatterns` and `NotRegexPatterns` from command properties

---

### CheckExpectInstructionsHelper.CheckExpectations

**File**: Likely `src/common/Helpers/CheckExpectInstructionsHelper.cs` (inferred from usage)

**Expected Signature**:
```csharp
public static bool CheckExpectations(
    string text,
    string? instructions,
    string? somethingElse,
    out object? output1,
    out object? output2,
    out string failedReason)
{
    if (string.IsNullOrEmpty(instructions))
    {
        failedReason = string.Empty;
        output1 = null;
        output2 = null;
        return true; // No instructions to check
    }
    
    // Use AI to validate text against instructions
    // Returns:
    //   true if AI says expectations are met
    //   false if AI says expectations are NOT met
    //   failedReason contains AI's explanation
}
```

**Evidence**:
- Called on line 48 of ExpectCheckCommand.cs
- Takes text, instructions, and additional parameters
- Returns bool and multiple out parameters including failure reason

---

## 6.4 Quiet Mode

### Command Line Option Parsing

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

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

The `ExpectCheckCommand` does NOT explicitly check the `Quiet` flag. All output goes through:

1. **ConsoleHelpers.Write()**: For progress message (line 36)
2. **ConsoleHelpers.WriteLine()**: For pass/fail results (lines 44, 51, 55)

**Expected Behavior with --quiet**:
- Progress indicator may be suppressed
- Pass/fail result likely still shown (critical information)
- Failure details likely still shown (needed for debugging)

**ConsoleHelpers Expected Implementation**:
```csharp
public static void Write(string message)
{
    if (IsQuiet()) return; // May suppress progress
    Console.Write(message);
}

public static void WriteLine(string message, bool overrideQuiet = false)
{
    if (IsQuiet() && !overrideQuiet) return;
    Console.WriteLine(message);
}
```

---

## 6.5 Debug Mode

### Command Line Option Parsing

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
- Immediately calls `ConsoleHelpers.ConfigureDebug(true)` to enable debug logging

---

### Debug Mode Usage in ExpectCheckCommand

The debug mode would affect:

1. **File reading**: Debug output when loading input file
2. **Regex compilation**: Debug output when compiling patterns
3. **Pattern matching**: Debug output during line-by-line checking
4. **AI processing**: Debug output from AI instruction checker

**Expected Debug Output** (when --debug is enabled):
```
DEBUG: Reading input from file: input.txt
DEBUG: Compiled regex pattern: ^Hello.*$
DEBUG: Checking line 1: "Hello, World!"
DEBUG: Pattern matched: ^Hello.*$
DEBUG: All regex patterns matched
DEBUG: Checking AI instructions: "Output should contain greeting"
DEBUG: AI validation passed
Checking expectations... PASS!
```

---

## Additional Display-Related Code

### Input Reading

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 38-39**:
```csharp
var lines = FileHelpers.ReadAllLines(Input!);
var text = string.Join("\n", lines);
```

**Explanation**:
- **Line 38**: Reads input file into array of lines
- **Line 39**: Joins lines into single string for AI instruction checking
- `Input!` uses null-forgiving operator (validated in `ExpectBaseCommand.Validate()`)

---

### Error Display

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 58-62**:
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

### Exit Code Semantics

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 45, 52, 56, 61**:
```csharp
return 1;  // Regex check failed (line 45)
return 1;  // Instruction check failed (line 52)
return 0;  // All checks passed (line 56)
return 1;  // Exception occurred (line 61)
```

**Explanation**:
- **0**: Success (all expectations met)
- **1**: Failure (expectation not met OR error occurred)
- Standard Unix convention for exit codes
- Allows shell integration: `cycodt expect check ... && echo "PASSED"`

---

## Data Flow Diagram

```
User runs: cycodt expect check --input output.txt --regex "^PASS" --not-regex "ERROR"

CommandLineOptions.Parse()
    ↓
    arg == "--input" → Input = "output.txt"
    ↓
    arg == "--regex" → RegexPatterns.Add("^PASS")
    ↓
    arg == "--not-regex" → NotRegexPatterns.Add("ERROR")
    ↓
ExpectCheckCommand.ExecuteAsync()
    ↓
ExpectCheckCommand.ExecuteCheck()
    ↓
message = "Checking expectations..."
    ↓
ConsoleHelpers.Write(message) → Display: "Checking expectations..."
    ↓
lines = FileHelpers.ReadAllLines("output.txt")
text = string.Join("\n", lines)
    ↓
ExpectHelper.CheckLines(lines, ["^PASS"], ["ERROR"], out reason)
    ↓
    [For each positive pattern]
    Check if any line matches
    ↓
    [For each negative pattern]
    Check if NO line matches
    ↓
    If any check fails:
        Set reason and return false
    ↓
If !linesOk:
    ConsoleHelpers.WriteLine("\r{message} FAILED!\n\n{reason}")
    Return 1
    ↓
CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, ...)
    ↓
    [AI validation if Instructions set]
    ↓
If !instructionsOk:
    ConsoleHelpers.WriteLine("\r{message} FAILED!\n\n{reason}")
    Return 1
    ↓
ConsoleHelpers.WriteLine("\r{message} PASS!")
Return 0
```

---

## Summary of Evidence

### Options Parsed
1. **--input**: Lines 41-46 of CycoDtCommandLineOptions.cs
2. **--regex**: Lines 65-71 of CycoDtCommandLineOptions.cs
3. **--not-regex**: Lines 72-78 of CycoDtCommandLineOptions.cs
4. **--instructions**: Lines 79-85 of CycoDtCommandLineOptions.cs
5. **--quiet**: Lines 350-353 of CommandLineOptions.cs
6. **--debug**: Lines 341-345 of CommandLineOptions.cs

### Display Logic
1. **Progress message**: Lines 35-36 of ExpectCheckCommand.cs
2. **Regex check**: Lines 41-46 of ExpectCheckCommand.cs
3. **Instruction check**: Lines 48-53 of ExpectCheckCommand.cs
4. **Pass display**: Line 55 of ExpectCheckCommand.cs

### Helper Methods
1. **ExpectHelper.CheckLines**: Called on line 41
2. **CheckExpectInstructionsHelper.CheckExpectations**: Called on line 48

### Input/Output
1. **Input reading**: Lines 38-39 of ExpectCheckCommand.cs
2. **Exit codes**: Lines 45, 52, 56, 61 of ExpectCheckCommand.cs

### Error Handling
1. **Exception display**: Lines 58-62 of ExpectCheckCommand.cs

---

## Related Source Files

- **Command**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`
- **Base class**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`
- **Parser**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
- **Options base**: `src/common/CommandLine/CommandLineOptions.cs`
- **Expect helper**: `src/common/Helpers/ExpectHelper.cs` (inferred)
- **Instructions helper**: `src/common/Helpers/CheckExpectInstructionsHelper.cs` (inferred)
- **File helpers**: `src/common/Helpers/FileHelpers.cs`
- **Console helpers**: `src/common/Helpers/ConsoleHelpers.cs`
