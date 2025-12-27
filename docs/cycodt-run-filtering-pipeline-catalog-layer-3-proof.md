# cycodt `run` Command - Layer 3: Content Filtering - PROOF

## Overview

This document provides detailed source code evidence for all assertions made in [Layer 3: Content Filtering](cycodt-run-filtering-pipeline-catalog-layer-3.md) for the `run` command.

**Note**: The `run` command uses the exact same filtering logic as the `list` command (both inherit from `TestBaseCommand`). This proof document focuses on demonstrating this shared implementation and the differences in how filtered tests are used.

---

## Shared Implementation with `list` Command

### Command Inheritance

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Line 1**:
```csharp
class TestRunCommand : TestBaseCommand
```

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 1**:
```csharp
class TestListCommand : TestBaseCommand
```

**Evidence**:
- Both commands inherit from `TestBaseCommand`
- All filtering options are defined in `TestBaseCommand`
- Both commands use the same `FindAndFilterTests()` method

---

## 1. `--test` / `--tests`

### Shared Implementation Evidence

The `--test` and `--tests` options are parsed and stored in `TestBaseCommand`, which is inherited by `TestRunCommand`.

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 99-113**:
```csharp
else if (arg == "--test")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var testName = ValidateString(arg, max1Arg.FirstOrDefault(), "test name");
    command.Tests.Add(testName!);
    i += max1Arg.Count();
}
else if (arg == "--tests")
{
    var testNames = GetInputOptionArgs(i + 1, args);
    var validTests = ValidateStrings(arg, testNames, "test names");
    command.Tests.AddRange(validTests);
    i += testNames.Count();
}
```

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Line 23**:
```csharp
public List<string> Tests { get; set; }
```

**Evidence**:
- Options are parsed the same way for both `list` and `run` commands
- Data is stored in the shared `TestBaseCommand.Tests` property
- No command-specific differences in parsing or storage

---

## 2. `--contains`

### Shared Implementation Evidence

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 114-120**:
```csharp
else if (arg == "--contains")
{
    var containPatterns = GetInputOptionArgs(i + 1, args);
    var validContains = ValidateStrings(arg, containPatterns, "contains patterns");
    command.Contains.AddRange(validContains);
    i += containPatterns.Count();
}
```

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Line 24**:
```csharp
public List<string> Contains { get; set; }
```

**Evidence**:
- Parsing is identical for both commands (via `TestBaseCommand` check in parser)
- Data is stored in the shared `TestBaseCommand.Contains` property

---

## Filtering Algorithm

### Shared FindAndFilterTests Implementation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 47-61**:
```csharp
protected IList<TestCase> FindAndFilterTests()
{
    var files = FindTestFiles();
    var filters = GetTestFilters();

    var atLeastOneFileSpecified = files.Any();
    var tests = atLeastOneFileSpecified
        ? files.SelectMany(file => GetTestsFromFile(file))
        : Array.Empty<TestCase>();

    var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
    var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();

    return filtered;
}
```

**Evidence**:
- This method is `protected`, accessible to both `TestListCommand` and `TestRunCommand`
- Both commands call this method to get filtered tests
- The filtering logic is identical

### Usage in `run` Command

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 26-43**:
```csharp
private int ExecuteTestRun()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());

        var tests = FindAndFilterTests();  // ← Same method as list command
        ConsoleHelpers.WriteLine(tests.Count() == 1
            ? $"Found {tests.Count()} test...\n"
            : $"Found {tests.Count()} tests...\n");

        var consoleHost = new YamlTestFrameworkConsoleHost();
        var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);

        GetOutputFileAndFormat(out var file, out var format);
        var passed = consoleHost.Finish(resultsByTestCaseId, format, file);

        return passed ? 0 : 1;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
        return 1;
    }
}
```

**Evidence**:
- Line 32: Calls `FindAndFilterTests()` inherited from `TestBaseCommand`
- Line 38: Uses filtered tests for execution: `YamlTestFramework.RunTests(tests, ...)`
- Line 41: Generates report from executed tests: `consoleHost.Finish(...)`

### Usage in `list` Command

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 13-20**:
```csharp
private int ExecuteList()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());
        var tests = FindAndFilterTests();  // ← Same method as run command
        
        if (ConsoleHelpers.IsVerbose())
```

**Evidence**:
- Line 18: Calls the same `FindAndFilterTests()` method
- The only difference is what happens AFTER filtering (display vs. execute)

---

## Run Command Execution

### Test Execution with Filtered Tests

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Line 38**:
```csharp
var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
```

**Evidence**:
- `tests` parameter is the result of `FindAndFilterTests()` (filtered tests)
- Only filtered tests are passed to the test execution engine
- Tests excluded by Layer 3 filtering are never executed

### Test Report Generation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 40-41**:
```csharp
GetOutputFileAndFormat(out var file, out var format);
var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
```

**Lines 52-67**:
```csharp
private void GetOutputFileAndFormat(out string file, out string format)
{
    format = OutputFormat;
    var ext = format switch
    {
        "trx" => "trx",
        "junit" => "xml",
        _ => throw new Exception($"Unknown format: {format}")
    };

    file = OutputFile ?? $"test-results.{ext}";
    if (!file.EndsWith($".{ext}"))
    {
        file += $".{ext}";
    }
}
```

**Evidence**:
- Line 41: `resultsByTestCaseId` contains results only for executed (filtered) tests
- Line 41: `consoleHost.Finish()` generates the report file
- Lines 52-67: Report file name and format are determined
- Report contains ONLY filtered tests; excluded tests are not in the report

---

## Filter Syntax

### Evidence of Identical Syntax

**GetTestFilters Implementation** (shared by both commands):

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 97-113**:
```csharp
protected List<string> GetTestFilters()
{
    var filters = new List<string>();
    
    filters.AddRange(Tests);
    foreach (var item in Contains)
    {
        filters.Add($"+{item}");
    }
    
    foreach (var item in Remove)
    {
        filters.Add($"-{item}");
    }

    return filters;
}
```

**Evidence**:
- This method is used by BOTH `list` and `run` commands
- Filter syntax is identical:
  - `Tests` → no prefix (OR logic)
  - `Contains` → `+` prefix (AND logic)
  - `Remove` → `-` prefix (AND NOT logic)

### YamlTestCaseFilter Usage

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 6-65**:
```csharp
public static IEnumerable<TestCase> FilterTestCases(IEnumerable<TestCase> tests, IEnumerable<string> criteria)
{
    // ... filtering logic (same as documented in list command proof) ...
}
```

**Evidence**:
- This static method is called by both commands via `TestBaseCommand.FindAndFilterTests()`
- Filter application is identical for both commands

---

## Property-Based Filtering

### Shared Implementation

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 138-147**:
```csharp
private static bool TestContainsText(TestCase test, string text)
{
    var fqn = test.FullyQualifiedName;
    var fqnStripped = StripHash(fqn);
    return test.DisplayName.Contains(text)
        || fqn.Contains(text)
        || fqnStripped.Contains(text)
        || test.Traits.Any(x => x.Name == text || x.Value.Contains(text))
        || supportedFilterProperties.Any(property => GetPropertyValue(test, property)?.ToString()?.Contains(text) == true);
}
```

**Evidence**:
- Property matching is identical for both commands
- All properties are searchable for both `list` and `run`
- See [list command Layer 3 proof](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#property-based-filtering) for complete property details

---

## Test Report Impact

### Empty Test Results

**Scenario**: No tests match filters

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 33-35**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"Found {tests.Count()} test...\n"
    : $"Found {tests.Count()} tests...\n");
```

**Evidence**:
- When `tests.Count()` is 0, displays "Found 0 tests..."
- Subsequent execution with empty test list generates empty report

### Test Report Contents

**TRX Format** (via `TrxXmlTestReporter`):
**File**: `src/cycodt/TestFramework/TrxXmlTestReporter.cs`

The report is generated from `resultsByTestCaseId`, which only contains results for executed tests.

**JUnit Format** (via `JunitXmlTestReporter`):
**File**: `src/cycodt/TestFramework/JunitXmlTestReporter.cs`

Same - report is generated from executed test results only.

**Evidence**:
- Both report formats iterate over test results
- Test results only exist for executed (filtered) tests
- Excluded tests do not appear in reports at all
- Test counts in reports reflect filtered test count, not total

---

## Comparison: list vs run

### Side-by-Side Execution Flow

**list Command Flow**:
```
TestListCommand.ExecuteList()
  ├─ FindAndFilterTests()          [Layers 1-4]
  │   └─ GetTestFilters()          [Layer 3]
  │   └─ YamlTestCaseFilter.FilterTestCases()  [Layer 3]
  └─ Display test names            [Layer 6]
```

**run Command Flow**:
```
TestRunCommand.ExecuteTestRun()
  ├─ FindAndFilterTests()          [Layers 1-4] ← SAME AS LIST
  │   └─ GetTestFilters()          [Layer 3] ← SAME AS LIST
  │   └─ YamlTestCaseFilter.FilterTestCases()  [Layer 3] ← SAME AS LIST
  ├─ YamlTestFramework.RunTests()  [Layer 9] ← DIFFERENT: Execute tests
  └─ consoleHost.Finish()          [Layer 7] ← DIFFERENT: Generate report
```

**Evidence**:
- Layer 3 filtering is IDENTICAL
- Difference is only in post-filtering actions (display vs execute/report)

---

## Edge Cases

### 1. Empty Filter Results with Test Execution

**Scenario**: No tests match filters

**Expected Behavior**:
1. Display: `Found 0 tests...`
2. No tests executed
3. Empty test report generated
4. Exit code: 0 (success)

**Evidence from Source**:

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 32-43**:
```csharp
var tests = FindAndFilterTests();  // Returns empty list
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"Found {tests.Count()} test...\n"
    : $"Found {tests.Count()} tests...\n");  // Displays "Found 0 tests..."

var consoleHost = new YamlTestFrameworkConsoleHost();
var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);  // Runs 0 tests

GetOutputFileAndFormat(out var file, out var format);
var passed = consoleHost.Finish(resultsByTestCaseId, format, file);  // Generates empty report

return passed ? 0 : 1;  // Returns 0 (success) for 0 tests
```

**Note**: The `passed` variable is typically `true` when no tests fail. With 0 tests, there are no failures, so `passed` is `true` and exit code is 0.

### 2. Partial Test Execution

**Scenario**: 10 total tests, 3 match filters

**Expected Behavior**:
1. Display: `Found 3 tests...`
2. Execute only 3 filtered tests
3. Report contains results for 3 tests
4. Exit code: 0 if all 3 pass, 1 if any fail

**Evidence**: See Lines 32-43 above - `tests` variable contains only filtered tests (3 in this case)

### 3. Case Sensitivity Impact on Execution

**Scenario**: Test named "Login Test", filter: `--test "login"`

**Expected Behavior**:
- Test is NOT matched (case-sensitive)
- Test is NOT executed
- Test is NOT in report

**Evidence**: See `TestContainsText()` implementation (uses `.Contains()` without case-insensitive flag)

---

## Performance Implications

### Filtering Before Execution

**Performance Benefit**:
- Filtering happens BEFORE test execution
- Excluded tests are never loaded into the test runner
- No overhead from skipped tests during execution

**Evidence**:

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 32-38**:
```csharp
var tests = FindAndFilterTests();  // ← Filtering happens here
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"Found {tests.Count()} test...\n"
    : $"Found {tests.Count()} tests...\n");

var consoleHost = new YamlTestFrameworkConsoleHost();
var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);  // ← Only filtered tests passed in
```

**Order of Operations**:
1. Filter tests (cheap - string comparisons)
2. Execute filtered tests (expensive - run commands, check expectations)

This is optimal - expensive operations only happen on filtered tests.

---

## Summary of Evidence

### Shared Implementation
✅ **Inheritance**: Lines 1 in TestRunCommand.cs and TestListCommand.cs
✅ **Shared filtering method**: Lines 47-61 in TestBaseCommand.cs
✅ **Identical option parsing**: Lines 99-120 in CycoDtCommandLineOptions.cs
✅ **Identical filter building**: Lines 97-113 in TestBaseCommand.cs
✅ **Identical filter application**: Lines 6-65 in YamlTestCaseFilter.cs

### Execution Differences
✅ **list displays tests**: Lines 20-44 in TestListCommand.cs
✅ **run executes tests**: Line 38 in TestRunCommand.cs
✅ **run generates reports**: Line 41 in TestRunCommand.cs

### Test Report Impact
✅ **Report from filtered tests only**: Lines 38-41 in TestRunCommand.cs
✅ **Empty results handling**: Lines 32-43 in TestRunCommand.cs

### Performance
✅ **Filtering before execution**: Lines 32-38 in TestRunCommand.cs
✅ **No overhead for excluded tests**: Excluded tests never passed to `RunTests()`

---

## Conclusion

All assertions in [Layer 3: Content Filtering](cycodt-run-filtering-pipeline-catalog-layer-3.md) are supported by source code evidence. The `run` command uses **identical** Layer 3 filtering to the `list` command (both inherit from `TestBaseCommand`). The only differences are in post-filtering actions:

- **list**: Display filtered test names
- **run**: Execute filtered tests + Generate test report

The filtering logic, options, syntax, and behavior are 100% shared between the two commands.
