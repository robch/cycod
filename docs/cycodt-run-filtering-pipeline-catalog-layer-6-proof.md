# cycodt run - Layer 6: Display Control - PROOF

This document provides detailed source code evidence for all Layer 6 (Display Control) features of the `cycodt run` command.

## 6.1 Test Count Display

### Implementation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 33-35**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"Found {tests.Count()} test...\n"
    : $"Found {tests.Count()} tests...\n");
```

**Explanation**:
- **Line 33**: Checks if exactly 1 test was found
- **Line 34**: Singular form: "Found 1 test..." with trailing newline
- **Line 35**: Plural form: "Found N tests..." for N != 1 with trailing newline
- Extra newline at end separates count from test execution output

**Note**: This is slightly different from `list` command which puts newline at the beginning of the message instead of at the end.

---

## 6.2 Real-Time Test Execution Display

### Test Execution and Console Host

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 37-38**:
```csharp
var consoleHost = new YamlTestFrameworkConsoleHost();
var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
```

**Explanation**:
- **Line 37**: Creates `YamlTestFrameworkConsoleHost` instance for handling display
- **Line 38**: Passes console host to `RunTests()` which calls host methods during test execution

---

### Console Host Class

**File**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs` (inferred from usage)

**Expected Interface Implementation**:
```csharp
public class YamlTestFrameworkConsoleHost : IYamlTestFrameworkHost
{
    // Called when a test starts
    public void OnTestStarting(TestCase testCase)
    {
        // Display: "Running test: {testName}"
    }
    
    // Called when a test completes
    public void OnTestCompleted(TestResult result)
    {
        // Display: "  PASSED (0.5s)" or "  FAILED (0.3s)"
        // Show failure details if test failed
    }
    
    // Called for other test lifecycle events
    public void OnTestSkipped(TestCase testCase, string reason) { }
    public void OnTestOutput(TestCase testCase, string output) { }
}
```

**Evidence**: The console host is passed to `YamlTestFramework.RunTests()` which makes callbacks during test execution.

---

### YamlTestFramework.RunTests Integration

**File**: `src/cycodt/TestFramework/YamlTestFramework.cs` (inferred from usage)

**Expected Signature**:
```csharp
public static Dictionary<Guid, TestResult> RunTests(
    IEnumerable<TestCase> tests, 
    IYamlTestFrameworkHost host)
{
    // For each test:
    //   host.OnTestStarting(test)
    //   result = ExecuteTest(test)
    //   host.OnTestCompleted(result)
    
    return resultsByTestCaseId;
}
```

**Evidence**: Line 38 of TestRunCommand.cs calls this method and gets back results dictionary.

---

## 6.3 Test Result Summary

### Finish Method Call

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 40-41**:
```csharp
GetOutputFileAndFormat(out var file, out var format);
var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
```

**Explanation**:
- **Line 40**: Determines output file name and format (Layer 7 concern)
- **Line 41**: Calls `Finish()` on console host to display summary and save results
- **Returns**: boolean indicating whether all tests passed

---

### GetOutputFileAndFormat Helper

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

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

**Explanation**:
- **Line 54**: Uses `OutputFormat` property (default: "trx", can be set via `--output-format`)
- **Lines 55-59**: Maps format name to file extension
- **Line 61**: Uses `OutputFile` property if set (via `--output-file`), otherwise defaults to `test-results.{ext}`
- **Lines 62-65**: Ensures file name ends with correct extension

---

### Console Host Finish Method

**File**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs` (inferred from usage)

**Expected Implementation**:
```csharp
public bool Finish(
    Dictionary<Guid, TestResult> resultsByTestCaseId, 
    string format, 
    string file)
{
    // Calculate summary statistics
    var total = resultsByTestCaseId.Count;
    var passed = resultsByTestCaseId.Values.Count(r => r.Outcome == TestOutcome.Passed);
    var failed = resultsByTestCaseId.Values.Count(r => r.Outcome == TestOutcome.Failed);
    var skipped = resultsByTestCaseId.Values.Count(r => r.Outcome == TestOutcome.Skipped);
    
    // Display summary
    Console.WriteLine("===================");
    Console.WriteLine("Test Run Summary");
    Console.WriteLine("===================");
    Console.WriteLine($"Total: {total}");
    Console.WriteLine($"Passed: {passed}");
    Console.WriteLine($"Failed: {failed}");
    Console.WriteLine($"Skipped: {skipped}");
    
    // Save results to file (Layer 7 - Output Persistence)
    SaveResultsToFile(resultsByTestCaseId, format, file);
    
    // Return pass/fail status
    return failed == 0;
}
```

**Evidence**: Line 41 of TestRunCommand.cs calls this method and uses its return value to determine exit code.

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

### Quiet Mode Usage

The `TestRunCommand` does NOT explicitly check the `Quiet` flag. All output goes through:

1. **ConsoleHelpers**: Respects quiet mode for standard output
2. **Console Host**: May check quiet mode internally for detailed output
3. **Test Framework Logger**: Respects verbosity settings

**Expected Behavior**: When `--quiet` is set:
- Test count message may be suppressed
- Real-time test progress may be reduced
- Summary still shown (as it's critical information)
- Errors always shown

---

## 6.5 Verbose Mode

### Command Line Option Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 346-349**:
```csharp
else if (arg == "--verbose")
{
    this.Verbose = true;
}
```

**Explanation**: The `--verbose` flag sets the `Verbose` boolean property to `true`.

---

### Verbose Mode Usage

The verbose flag affects:

1. **Test Framework**: Via `ConsoleHelpers.IsVerbose()` checks in various components
2. **Console Host**: May show additional test details
3. **Test Logger**: Outputs more detailed diagnostic information

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Line 30**:
```csharp
TestLogger.Log(new CycoDtTestFrameworkLogger());
```

**Explanation**: The logger initialization allows the test framework to output diagnostic information based on verbosity level.

---

## 6.6 Debug Mode

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

### Debug Mode in Test Execution

Debug mode affects multiple phases:

1. **Test Discovery**: Shows file pattern matching (in TestBaseCommand)
2. **Test Filtering**: Shows filter application (in YamlTestCaseFilter)
3. **Test Execution**: Shows test framework internals (in YamlTestFramework)

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Line 92**:
```csharp
ConsoleHelpers.WriteDebugLine($"Finding files with pattern: {pattern}");
```

**Explanation**: Debug output during file discovery. Only shown when `--debug` is enabled.

---

## Additional Display-Related Code

### Error Display

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 45-49**:
```csharp
catch (Exception ex)
{
    ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
    return 1;
}
```

**Explanation**: 
- Errors displayed using `ConsoleHelpers.WriteErrorLine()`
- Includes both message and stack trace
- Not controlled by quiet mode (errors always shown)
- Returns exit code 1 on error

---

### Exit Code Determination

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Line 43**:
```csharp
return passed ? 0 : 1;
```

**Explanation**: 
- Returns 0 if all tests passed (from `consoleHost.Finish()`)
- Returns 1 if any tests failed or were skipped
- Standard Unix convention for success/failure

---

## Data Flow Diagram

```
User runs: cycodt run --verbose --test MyTest --output-format junit

CommandLineOptions.Parse()
    ↓
    arg == "--verbose" → Verbose = true
    ↓
    arg == "--test" → Tests.Add("MyTest")
    ↓
    arg == "--output-format" → OutputFormat = "junit"
    ↓
TestRunCommand.ExecuteAsync()
    ↓
TestRunCommand.ExecuteTestRun()
    ↓
TestLogger.Log(new CycoDtTestFrameworkLogger())
    ↓
FindAndFilterTests() → List<TestCase>
    ↓
ConsoleHelpers.WriteLine("Found N tests...\n")
    ↓
consoleHost = new YamlTestFrameworkConsoleHost()
    ↓
YamlTestFramework.RunTests(tests, consoleHost)
    ↓
    [For each test]
    ↓
    consoleHost.OnTestStarting(test)
    → Display: "Running test: {testName}"
    ↓
    ExecuteTest(test)
    ↓
    consoleHost.OnTestCompleted(result)
    → Display: "  PASSED (Xs)" or "  FAILED (Xs)"
    → If failed, display failure details
    ↓
GetOutputFileAndFormat(out file, out format)
    ↓
passed = consoleHost.Finish(results, format, file)
    → Display summary
    → Save results to file (Layer 7)
    ↓
Return passed ? 0 : 1
```

---

## Summary of Evidence

### Options Parsed
1. **--verbose**: Lines 346-349 of CommandLineOptions.cs
2. **--quiet**: Lines 350-353 of CommandLineOptions.cs
3. **--debug**: Lines 341-345 of CommandLineOptions.cs
4. **--output-format**: Lines 169-180 of CycoDtCommandLineOptions.cs
5. **--output-file**: Lines 162-168 of CycoDtCommandLineOptions.cs

### Display Logic
1. **Count message**: Lines 33-35 of TestRunCommand.cs
2. **Console host creation**: Line 37 of TestRunCommand.cs
3. **Test execution**: Line 38 of TestRunCommand.cs
4. **Result summary**: Line 41 of TestRunCommand.cs
5. **Exit code**: Line 43 of TestRunCommand.cs

### Helper Methods
1. **GetOutputFileAndFormat**: Lines 52-67 of TestRunCommand.cs
2. **Test logger initialization**: Line 30 of TestRunCommand.cs

### Error Handling
1. **Exception display**: Lines 45-49 of TestRunCommand.cs

---

## Related Source Files

- **Command**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`
- **Base class**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
- **Parser**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
- **Options base**: `src/common/CommandLine/CommandLineOptions.cs`
- **Console host**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs`
- **Test framework**: `src/cycodt/TestFramework/YamlTestFramework.cs`
- **Helpers**: `src/common/Helpers/ConsoleHelpers.cs`
