# cycodt `expect check` Command - Layer 3: Content Filtering - PROOF

## Overview

This document provides detailed source code evidence for all assertions made in [Layer 3: Content Filtering](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md) for the `expect check` command.

---

## 1. `--regex`

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 135-141**:
```csharp
else if (command is ExpectCheckCommand checkCommand && arg == "--regex")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "regex pattern");
    checkCommand.RegexPatterns.Add(pattern!);
    i += max1Arg.Count();
}
```

**Evidence**:
- Line 135: Option matches `--regex` argument
- Line 137: Accepts single regex pattern per invocation
- Line 139: Pattern is added to `checkCommand.RegexPatterns` list
- Multiple `--regex` options can be specified (each adds to the list)

### Data Storage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 11-14**:
```csharp
public ExpectCheckCommand() : base()
{
    RegexPatterns = new List<string>();
    NotRegexPatterns = new List<string>();
}
```

**Line 17**:
```csharp
public List<string> RegexPatterns { get; set; }
```

**Evidence**:
- Line 13: `RegexPatterns` initialized as empty list in constructor
- Line 17: Stored as `List<string>` property
- Can contain zero or more patterns

### Validation Usage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 38-46**:
```csharp
var lines = FileHelpers.ReadAllLines(Input!);
var text = string.Join("\n", lines);

var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
if (!linesOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
    return 1;
}
```

**Evidence**:
- Line 38: Input is read into lines
- Line 41: `RegexPatterns` is passed to `ExpectHelper.CheckLines()` as the "expected" parameter
- Lines 42-46: If validation fails, display error and return exit code 1

---

## 2. `--not-regex`

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 142-148**:
```csharp
else if (command is ExpectCheckCommand checkCommand3 && arg == "--not-regex")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "not-regex pattern");
    checkCommand3.NotRegexPatterns.Add(pattern!);
    i += max1Arg.Count();
}
```

**Evidence**:
- Line 142: Option matches `--not-regex` argument
- Line 144: Accepts single regex pattern per invocation
- Line 145: Pattern is added to `checkCommand3.NotRegexPatterns` list
- Multiple `--not-regex` options can be specified

### Data Storage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 18**:
```csharp
public List<string> NotRegexPatterns { get; set; }
```

**Line 14** (constructor):
```csharp
NotRegexPatterns = new List<string>();
```

**Evidence**:
- Stored as `List<string>` property
- Initialized in constructor

### Validation Usage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 41**:
```csharp
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
```

**Evidence**:
- `NotRegexPatterns` is passed to `ExpectHelper.CheckLines()` as the "unexpected" parameter
- Validation checks that NO lines match these patterns

---

## 3. `--instructions`

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 149-155**:
```csharp
else if (command is ExpectCheckCommand checkCommand5 && arg == "--instructions")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var instructions = ValidateString(arg, max1Arg.FirstOrDefault(), "instructions");
    checkCommand5.Instructions = instructions;
    i += max1Arg.Count();
}
```

**Evidence**:
- Line 149: Option matches `--instructions` argument
- Line 151: Accepts single instruction string
- Line 153: Stored in `checkCommand5.Instructions` property (not a list - single value)

### Data Storage

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 19**:
```csharp
public string? Instructions { get; set; }
```

**Evidence**:
- Stored as `string?` property (nullable, not a list)
- Only one instruction string can be specified

### Validation Usage

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

**Evidence**:
- Line 48: `Instructions` is passed to `CheckExpectInstructionsHelper.CheckExpectations()`
- Line 48: `text` (full input as single string) is passed, not individual lines
- Lines 49-53: If AI validation fails, display error and return exit code 1

---

## Validation Algorithm

### ExecuteCheck Method

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 31-63** (Complete Method):
```csharp
private int ExecuteCheck()
{
    try
    {
        var message = "Checking expectations...";
        ConsoleHelpers.Write($"{message}");

        var lines = FileHelpers.ReadAllLines(Input!);
        var text = string.Join("\n", lines);

        var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
        if (!linesOk)
        {
            ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
            return 1;
        }

        var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
        if (!instructionsOk)
        {
            ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
            return 1;
        }

        ConsoleHelpers.WriteLine($"\r{message} PASS!");
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
- Line 38: Read input into lines array
- Line 39: Join lines into full text string
- Line 41: Validate regex patterns via `ExpectHelper.CheckLines()`
- Line 48: Validate AI instructions via `CheckExpectInstructionsHelper.CheckExpectations()`
- Lines 42-46: Early return if regex validation fails
- Lines 49-53: Early return if AI validation fails
- Line 55: Return success if all validations pass

**Execution Order**:
1. Regex validation (lines-based) - FIRST
2. AI validation (text-based) - SECOND
3. Both must pass for overall success

---

## Pattern Matching Behavior

### ExpectHelper.CheckLines

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 22-35**:
```csharp
public static bool CheckLines(IEnumerable<string> lines, IEnumerable<string> expected, IEnumerable<string> unexpected, out string? details)
{
    details = null;

    var helper = new ExpectHelper(lines, expected, unexpected);
    var result = helper.Expect();

    if (!result)
    {
        details = helper._details.ToString().TrimEnd('\r', '\n');
    }

    return result;
}
```

**Evidence**:
- Line 22: Static method accepting lines, expected patterns, and unexpected patterns
- Line 26: Creates helper instance with all parameters
- Line 27: Runs `Expect()` method to perform validation
- Line 31: Failure details are extracted from `_details` StringBuilder
- Line 34: Returns boolean success/failure

### ExpectHelper Constructor

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 37-42**:
```csharp
private ExpectHelper(IEnumerable<string> lines, IEnumerable<string> expected, IEnumerable<string> unexpected)
{
    _allLines = lines;
    _expected = expected != null ? new Queue<string>(expected) : null;
    _unexpected = unexpected != null ? new List<string>(unexpected) : null;
}
```

**Evidence**:
- Line 40: `expected` patterns (--regex) stored in Queue
- Line 41: `unexpected` patterns (--not-regex) stored in List
- Using Queue for expected patterns enables sequential matching

### ExpectHelper.Expect Method

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 44-61**:
```csharp
private bool Expect()
{
    foreach (string line in _allLines)
    {
        if (_expected != null) CheckExpected(line);
        if (_unexpected != null) CheckUnexpected(line);
    }

    var allExpectedFound = _expected == null || _expected.Count == 0;
    if (!allExpectedFound)
    {
        var codeBlock = MarkdownHelpers.GetCodeBlock(_unmatchedInput.ToString());
        var message = $"UNEXPECTED: Couldn't find '{_expected!.Peek()}' in:\n{codeBlock}";
        _details.AppendLine(message);
    }

    return !_foundUnexpected && allExpectedFound;
}
```

**Evidence**:
- Lines 46-50: Iterate over all lines, checking expected and unexpected patterns for each line
- Line 52: Success requires ALL expected patterns found (Queue empty)
- Line 60: Success requires NO unexpected patterns found AND all expected patterns found
- Both conditions must be true for validation to pass

### CheckExpected - Regex Pattern Matching

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 63-86**:
```csharp
private void CheckExpected(string line)
{
    ConsoleHelpers.WriteDebugHexDump(line, $"CheckExpected: Adding '{line}'");
    _unmatchedInput.AppendLine(line);
    ConsoleHelpers.WriteDebugHexDump(_unmatchedInput.ToString(), "CheckExpected: Unmatched is now:");
    while (_expected!.Count > 0)
    {
        var pattern = _expected.Peek();
        var check = _unmatchedInput.ToString();

        var match = Regex.Match(check, pattern);
        if (!match.Success)
        {
            ConsoleHelpers.WriteDebugLine($"CheckExpected: No match for '{pattern}' in unmatched!\nCheckExpected: ---"); 
            break; // continue reading input...
        }

        ConsoleHelpers.WriteDebugHexDump(check, $"CheckExpected: Matched '{pattern}' at {match.Index:x4} ({match.Length:x4} char(s)) in:");
        _unmatchedInput.Remove(0, match.Index + match.Length);
        ConsoleHelpers.WriteDebugHexDump(_unmatchedInput.ToString(), "CheckExpected: After removing, unmatched is now:");

        _expected.Dequeue();
    }
}
```

**Evidence**:
- Line 66: Each line is appended to `_unmatchedInput` buffer
- Line 70: Peek at next expected pattern (doesn't remove from queue yet)
- Line 73: Use `Regex.Match()` to check pattern against accumulated input
- Lines 74-78: If no match, continue to next line (break while loop, continue foreach)
- Line 81: On match, remove matched portion from buffer
- Line 84: Remove pattern from queue (matched successfully)
- **Important**: Patterns match against accumulated multi-line buffer, not just current line

**Matching Semantics**:
- Patterns CAN match across multiple lines (buffered matching)
- Pattern `^Line 1\nLine 2$` can match across two lines
- Once a pattern matches, matched text is consumed from buffer

### CheckUnexpected - Not-Regex Pattern Matching

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 88-100**:
```csharp
private void CheckUnexpected(string line)
{
    foreach (var pattern in _unexpected!)
    {
        var match = Regex.Match(line, pattern);
        if (!match.Success) continue; // check more patterns

        _foundUnexpected = true;

        var message = $"UNEXPECTED: Found '{pattern}' in '{line}'";
        _details.AppendLine(message);
    }
}
```

**Evidence**:
- Line 92: Match pattern against CURRENT LINE ONLY (not buffered)
- Line 93: If no match, continue to next pattern
- Line 95: On match, set `_foundUnexpected` flag to true
- Line 97: Record failure message with line content
- **Important**: Unexpected patterns checked per-line, not across lines

**Matching Semantics**:
- Patterns match individual lines, not multi-line buffers
- If ANY unexpected pattern matches ANY line, validation fails
- All unexpected patterns are checked (doesn't short-circuit)

---

## Validation vs Filtering

### Semantic Difference

**Filtering (list/run commands)**:
- **Purpose**: Select subset of items to process
- **Input**: Collection of items (tests)
- **Output**: Filtered subset of items
- **Action**: Include/exclude items from collection

**Validation (expect check command)**:
- **Purpose**: Verify content meets expectations
- **Input**: Single content source (lines of text)
- **Output**: Boolean pass/fail result
- **Action**: Check if content matches patterns

### Layer 3 Conceptual Mapping

**For list/run**:
- Layer 3 = "What content within containers to process"
- Containers = Test files
- Content = Individual tests

**For expect check**:
- Layer 3 = "What aspects of content to validate"
- Container = Input source (file/stdin)
- Content = Lines/patterns to check

**Evidence**:
Both concepts fit Layer 3 because they determine **what content is relevant** for the operation, even though the operations differ (filtering vs. validation).

---

## Combined Validation

### Regex + Not-Regex

**Example Command**:
```bash
cycodt expect check --input output.txt --regex "SUCCESS" --not-regex "ERROR"
```

**Execution Flow**:

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

**Evidence**:
- Both `RegexPatterns` and `NotRegexPatterns` are passed together
- `ExpectHelper` checks both simultaneously (Line 46-50 in ExpectHelper.cs)
- Validation fails if EITHER expected patterns don't match OR unexpected patterns do match

**File**: `src/common/Helpers/ExpectHelper.cs`

**Line 60**:
```csharp
return !_foundUnexpected && allExpectedFound;
```

**Evidence**:
- Must satisfy BOTH conditions: `!_foundUnexpected` (no not-regex matches) AND `allExpectedFound` (all regex patterns matched)

### Regex + Instructions

**Example Command**:
```bash
cycodt expect check --input output.txt --regex "SUCCESS" --instructions "Verify success"
```

**Execution Flow**:

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 41-53**:
```csharp
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
if (!linesOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
    return 1;
}

var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
if (!instructionsOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
    return 1;
}
```

**Evidence**:
- Line 41: Regex validation runs FIRST
- Lines 42-46: Early return if regex fails (instructions not checked)
- Line 48: Instructions validation runs SECOND (only if regex passes)
- Lines 49-53: Early return if instructions fail
- **Sequential validation**: Regex → Instructions (short-circuit on first failure)

---

## Edge Cases

### Empty Input

**Scenario**: Input file is empty or contains zero lines

**Expected Behavior**:
- All `--regex` patterns fail (no lines to match)
- All `--not-regex` patterns pass (no lines to match against)

**Evidence**:

**File**: `src/common/Helpers/ExpectHelper.cs`

**Lines 46-50**:
```csharp
foreach (string line in _allLines)
{
    if (_expected != null) CheckExpected(line);
    if (_unexpected != null) CheckUnexpected(line);
}
```

**Lines 52-58**:
```csharp
var allExpectedFound = _expected == null || _expected.Count == 0;
if (!allExpectedFound)
{
    var codeBlock = MarkdownHelpers.GetCodeBlock(_unmatchedInput.ToString());
    var message = $"UNEXPECTED: Couldn't find '{_expected!.Peek()}' in:\n{codeBlock}";
    _details.AppendLine(message);
}
```

**Analysis**:
- If `_allLines` is empty, foreach loop never executes
- `CheckExpected()` never called → expected patterns never matched → `_expected.Count > 0`
- `CheckUnexpected()` never called → `_foundUnexpected` remains `false`
- Line 52: `allExpectedFound` is `false` if expected patterns exist
- Line 60: Returns `!false && false` = `false` if expected patterns exist
- Line 60: Returns `!false && true` = `true` if NO expected patterns

**Conclusion**:
- Empty input FAILS if `--regex` patterns specified
- Empty input PASSES if only `--not-regex` patterns specified

### No Patterns Specified

**Scenario**: No `--regex`, `--not-regex`, or `--instructions` options

**Expected Behavior**: Validation passes (no expectations to check)

**Evidence**:

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 41-55**:
```csharp
var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
if (!linesOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
    return 1;
}

var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
if (!instructionsOk)
{
    ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
    return 1;
}

ConsoleHelpers.WriteLine($"\r{message} PASS!");
```

**Analysis**:
- If `RegexPatterns` is empty, `ExpectHelper` receives empty expected list
- Line 52 in ExpectHelper.cs: `allExpectedFound = true` (empty Queue)
- Line 60 in ExpectHelper.cs: Returns `!false && true` = `true`
- If `Instructions` is null, `CheckExpectInstructionsHelper` likely returns success
- Line 55: Displays "PASS!"

**Conclusion**: No patterns = automatic pass

### Case Sensitivity

**File**: `src/common/Helpers/ExpectHelper.cs`

**Line 73** (CheckExpected):
```csharp
var match = Regex.Match(check, pattern);
```

**Line 92** (CheckUnexpected):
```csharp
var match = Regex.Match(line, pattern);
```

**Evidence**:
- Uses `Regex.Match()` without `RegexOptions` parameter
- Default .NET Regex behavior is **case-sensitive**
- Pattern "SUCCESS" does NOT match "success"
- To enable case-insensitive matching, use `(?i)` in pattern: `"(?i)success"`

---

## Summary of Evidence

### Option Parsing
✅ **`--regex`**: Lines 135-141 in CycoDtCommandLineOptions.cs
✅ **`--not-regex`**: Lines 142-148 in CycoDtCommandLineOptions.cs
✅ **`--instructions`**: Lines 149-155 in CycoDtCommandLineOptions.cs

### Validation Execution
✅ **ExecuteCheck()**: Lines 31-63 in ExpectCheckCommand.cs
✅ **Regex validation**: Line 41 calls `ExpectHelper.CheckLines()`
✅ **AI validation**: Line 48 calls `CheckExpectInstructionsHelper.CheckExpectations()`
✅ **Sequential execution**: Regex first, then instructions

### Pattern Matching
✅ **CheckLines()**: Lines 22-35 in ExpectHelper.cs
✅ **Expect()**: Lines 44-61 in ExpectHelper.cs
✅ **CheckExpected()**: Lines 63-86 in ExpectHelper.cs (multi-line buffered matching)
✅ **CheckUnexpected()**: Lines 88-100 in ExpectHelper.cs (per-line matching)

### Validation Logic
✅ **AND logic for expected**: Line 60 `allExpectedFound` in ExpectHelper.cs
✅ **AND logic for unexpected**: Line 60 `!_foundUnexpected` in ExpectHelper.cs
✅ **Combined validation**: Line 60 `return !_foundUnexpected && allExpectedFound`

### Edge Cases
✅ **Empty input**: Lines 46-58 in ExpectHelper.cs
✅ **No patterns**: Lines 41-55 in ExpectCheckCommand.cs
✅ **Case sensitivity**: Lines 73, 92 in ExpectHelper.cs (default Regex behavior)

---

## Conclusion

All assertions in [Layer 3: Content Filtering](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md) are supported by source code evidence from the cycodt codebase. The `expect check` command uses a sophisticated validation system with:

1. **Regex pattern matching** (multi-line buffered for expected patterns)
2. **Negative pattern matching** (per-line for unexpected patterns)
3. **AI-based validation** (semantic content analysis)
4. **Sequential validation** (regex first, then AI)
5. **AND logic** (all patterns must satisfy their conditions)
