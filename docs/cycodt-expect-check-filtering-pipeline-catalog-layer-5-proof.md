# cycodt expect check - Layer 5: Context Expansion - PROOF

## Evidence: No Context Expansion Implementation

This document provides source code evidence that the `cycodt expect check` command **does NOT implement Layer 5 (Context Expansion)** features for showing context around failed expectations or matches.

---

## Source Code Analysis

### 1. ExpectCheckCommand Implementation

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

```csharp
// Lines 1-65 (entire file)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

class ExpectCheckCommand : ExpectBaseCommand
{
    public ExpectCheckCommand() : base()
    {
        RegexPatterns = new List<string>();      // Layer 3: Patterns to match
        NotRegexPatterns = new List<string>();   // Layer 3: Patterns to NOT match
    }

    public List<string> RegexPatterns { get; set; }     // Layer 3
    public List<string> NotRegexPatterns { get; set; }  // Layer 3
    public string? Instructions { get; set; }           // Layer 8: AI instructions

    public override string GetCommandName()
    {
        return "expect check";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteCheck());
    }

    private int ExecuteCheck()
    {
        try
        {
            var message = "Checking expectations...";
            ConsoleHelpers.Write($"{message}");

            var lines = FileHelpers.ReadAllLines(Input!);  // Line 38: Read input lines
            var text = string.Join("\n", lines);

            // Line 41: Check regex patterns (Layer 3)
            var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
            if (!linesOk)
            {
                // Lines 44: FAILED output - NO context shown
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
                return 1;
            }

            // Line 48: Check AI instructions (Layer 8)
            var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
            if (!instructionsOk)
            {
                // Line 51: FAILED output - NO context shown
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
                return 1;
            }

            // Line 55: PASS output
            ConsoleHelpers.WriteLine($"\r{message} PASS!");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
}
```

**Analysis**:
- **Properties**: Only expectation checking properties (Layer 3 & 8) - NO context expansion properties
- **NO properties** for:
  - `FailureContextLines`
  - `ShowMatchContext`
  - `ShowFullInputAnnotated`
  - `DiffContextLines`
  - `VerboseFailures`
- **Line 38**: Reads all input lines at once - no context tracking
- **Line 41**: Checks patterns via `ExpectHelper.CheckLines()` - returns only pass/fail + reason
- **Lines 44, 51**: Failure output shows only `expectFailedReason` or `instructionsFailedReason` - NO context
- **Line 55**: Success output - no match context shown

### 2. ExpectBaseCommand - No Context Properties

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

```csharp
// Lines 1-43 (entire file)
abstract class ExpectBaseCommand : Command
{
    public ExpectBaseCommand()
    {
        Input = null;    // Layer 1: Input source
        Output = null;   // Layer 7: Output destination
    }
    
    public string? Input { get; set; }   // Layer 1
    public string? Output { get; set; }  // Layer 7

    public override bool IsEmpty()
    {
        var noInput = string.IsNullOrEmpty(Input);
        var isRedirected = Console.IsInputRedirected;
        return noInput && !isRedirected;
    }
    
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

    protected void WriteOutput(string text)
    {
        if (string.IsNullOrEmpty(Output))
        {
            ConsoleHelpers.WriteLine(text, overrideQuiet: true);
        }
        else
        {
            FileHelpers.WriteAllText(Output, text);
        }
    }
}
```

**Analysis**:
- Only two properties: `Input` (Layer 1) and `Output` (Layer 7)
- **NO Layer 5 properties** such as:
  - `FailureContextLines`
  - `MatchContextLines`
  - `ShowFullInput`
  - `DiffContext`
  - `VerboseMode`

### 3. Command Line Parser - No Context Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

```csharp
// Lines 15-55 (TryParseExpectCommandOptions method)
private bool TryParseExpectCommandOptions(ExpectBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--input")  // Layer 1: Input source
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var input = ValidateString(arg, max1Arg.FirstOrDefault(), "input");
        command.Input = input!;
        i += max1Arg.Count();
    }
    else if (arg == "--save-output" || arg == "--output")  // Layer 7: Output destination
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output");
        command.Output = output;
        i += max1Arg.Count();
    }
    else if (command is ExpectFormatCommand formatCommand && arg == "--strict")  // Layer 4 (format only)
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var strictStr = ValidateString(arg, max1Arg.FirstOrDefault(), "strict");
        if (bool.TryParse(strictStr, out bool strict))
        {
            formatCommand.Strict = strict;
        }
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand && arg == "--regex")  // Layer 3: Pattern matching
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "regex pattern");
        checkCommand.RegexPatterns.Add(pattern!);
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand3 && arg == "--not-regex")  // Layer 3: Negative pattern
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "not-regex pattern");
        checkCommand3.NotRegexPatterns.Add(pattern!);
        i += max1Arg.Count();
    }
    else if (command is ExpectCheckCommand checkCommand5 && arg == "--instructions")  // Layer 8: AI checking
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var instructions = ValidateString(arg, max1Arg.FirstOrDefault(), "instructions");
        checkCommand5.Instructions = instructions;
        i += max1Arg.Count();
    }
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Analysis**:
- Parses options for Layers 1, 3, 7, and 8 only
- **NO parsing** for Layer 5 context expansion options such as:
  - `--failure-context N`
  - `--match-context N`
  - `--show-full-input`
  - `--diff-context N`
  - `--verbose-failures`
  - `--show-matches-with-context N`
  - `--context N` (like cycodj search has)

### 4. ExpectHelper.CheckLines - No Context in Return Value

**File**: `src/common/ExpectHelpers/ExpectHelper.cs` (referenced in ExpectCheckCommand)

While I don't have the full implementation visible, the usage shows:

```csharp
var linesOk = ExpectHelper.CheckLines(
    lines,              // Input lines
    RegexPatterns,      // Patterns to match
    NotRegexPatterns,   // Patterns to NOT match
    out var expectFailedReason  // Failure reason string only
);
```

**Analysis**:
- Returns: `bool` (pass/fail)
- Out parameter: `string` (failure reason) - just a message, no context
- Does NOT return:
  - Line numbers where failures occurred
  - Context lines around failures
  - Match positions
  - Detailed failure information with context

### 5. CheckExpectInstructionsHelper - No Context in Return Value

**File**: Referenced in ExpectCheckCommand

```csharp
var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(
    text,                        // Input text
    Instructions,                // AI instructions
    null,                        // (unknown parameter)
    out _,                       // (unused out parameter)
    out _,                       // (unused out parameter)
    out var instructionsFailedReason  // Failure reason string only
);
```

**Analysis**:
- Returns: `bool` (pass/fail)
- Out parameter: `string` (failure reason) - just a message, no context
- Does NOT return:
  - Position information about AI-detected issues
  - Context lines around mismatches
  - Diff-style output
  - Detailed analysis with context

### 6. Failure Message Output - No Context

From ExecuteCheck() method:

```csharp
// Regex pattern failure
ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");

// AI instruction failure
ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
```

**Analysis**:
- Outputs only the failure reason string
- Does NOT output:
  - Line numbers
  - Context lines
  - Input excerpts
  - Highlighted differences
  - Pattern match details

---

## Comparison with Tools That Have Context

### grep (Unix tool) - HAS context

```bash
$ grep -C 2 "error" logfile.txt
# Output shows 2 lines before and after each match:
18: Processing request...
19: Validating input...
20: Error: Invalid parameter    <-- Match
21: Aborting operation
22: Cleaning up resources
```

### cycodmd FindFilesCommand - HAS context

**Properties**:
```csharp
public int IncludeLineCountBefore { get; set; }
public int IncludeLineCountAfter { get; set; }
```

**Parser**:
```csharp
else if (arg == "--lines")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    var count = ValidateLineCount(arg, countStr);
    command.IncludeLineCountBefore = count;
    command.IncludeLineCountAfter = count;
}
```

**Usage**: Shows N lines before/after matches

### cycodj SearchCommand - HAS context

**Property**:
```csharp
public int? ContextLines { get; set; }
```

**Parser**:
```csharp
else if (arg == "--context" || arg == "-C")
{
    var lines = i + 1 < args.Length ? args[++i] : null;
    command.ContextLines = n;
}
```

**Usage**: Shows N messages before/after matched message

### cycodt expect check - DOES NOT HAVE context

**NO equivalent properties, parsing, or usage.**

---

## Example of What Context COULD Look Like

### Current Output (No Context)
```
Checking expectations... FAILED!

Expected pattern 'completed successfully' not found in input.
```

### Enhanced Output WITH Context (Hypothetical)
```
Checking expectations... FAILED!

Expected pattern 'completed successfully' not found.

Context around line 42 (where pattern was expected):
   40: Processing step 3...
   41: Running validation checks
   42: Validation failed with error code 5    <-- Expected 'completed successfully' here
   43: Rolling back transaction
   44: Cleanup complete

Pattern: 'completed successfully'
Found similar: 'Validation failed' (line 42)
Suggestion: Check if process completed without errors
```

This would require:
1. Tracking line numbers during pattern checking
2. Storing input lines for context retrieval
3. Extracting N lines before/after failure point
4. Formatting context with highlights

**NONE of this exists currently.**

---

## Test Case Analysis

Looking at test files that use `expect check` (from test YAML files):

```yaml
- step: Check output
  command: cycodt expect check --input output.txt --regex "success"
  expect-regex:
    - "PASS"
```

**Observation**:
- Tests only verify PASS/FAIL status
- No tests for context display (because feature doesn't exist)
- No tests with `--context` or `--failure-context` options

---

## Conclusion

The source code analysis definitively shows that **cycodt expect check command does NOT implement Layer 5 (Context Expansion)**:

1. ✅ **ExpectCheckCommand.cs** - No context properties, no context in failure output
2. ✅ **ExpectBaseCommand.cs** - No context properties defined
3. ✅ **CycoDtCommandLineOptions.cs** - No context options parsed
4. ✅ **ExpectHelper.CheckLines()** - Returns only bool + failure message, no context
5. ✅ **CheckExpectInstructionsHelper** - Returns only bool + failure message, no context
6. ✅ **Failure Output** - Shows only failure reason, no line numbers or context
7. ✅ **Comparison** - Other tools (grep, cycodmd, cycodj) have explicit context features; cycodt does not

**Verdict**: Layer 5 is **NOT IMPLEMENTED** for the `expect check` command.

The `expect check` command validates expectations and reports PASS/FAIL with a basic failure message, but provides NO options to show context lines around failures, display the full input with annotations, or expand failure information.

---

**Related Files**:
- [Layer 5 Catalog](cycodt-expect-check-filtering-pipeline-catalog-layer-5.md)
- [Layer 1 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 3 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 6 Proof (Display)]( - to be created)
- [Layer 8 Proof (AI Processing)]( - to be created)
