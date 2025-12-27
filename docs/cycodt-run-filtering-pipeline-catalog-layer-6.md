# cycodt run - Layer 6: Display Control

## Overview

Layer 6 controls **how test execution results are presented** to the user. For the `run` command, this layer determines the format, verbosity, and real-time display of test execution progress and results.

## Command

```bash
cycodt run [options]
```

## Layer 6 Features

### 6.1 Test Count Display

**Feature**: Pre-execution count display

**Purpose**: Shows how many tests will be executed

**Behavior**:
- Displays singular message for 1 test: "Found 1 test..."
- Displays plural message for multiple tests: "Found N tests..."
- Shown before test execution begins

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#61-test-count-display)

### 6.2 Real-Time Test Execution Display

**Feature**: Live test progress output

**Purpose**: Shows test execution in real-time via console host

**Behavior**:
- Handled by `YamlTestFrameworkConsoleHost`
- Displays test start, progress, and completion
- Shows pass/fail status for each test
- Includes test timing information

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#62-real-time-test-execution-display)

### 6.3 Test Result Summary

**Feature**: Execution summary after tests complete

**Purpose**: Summarizes overall test run results

**Behavior**:
- Handled by `YamlTestFrameworkConsoleHost.Finish()`
- Shows total tests, passed, failed, skipped
- Displays execution duration
- Returns pass/fail status for exit code

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#63-test-result-summary)

### 6.4 Quiet Mode

**Option**: `--quiet` (inherited from `CommandLineOptions`)

**Purpose**: Suppress non-essential output

**Behavior**:
- Controlled by `ConsoleHelpers` infrastructure
- Affects output through console host
- Error messages still displayed

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#64-quiet-mode)

### 6.5 Verbose Mode

**Option**: `--verbose` (inherited from `CommandLineOptions`)

**Purpose**: Show detailed test execution information

**Behavior**:
- Enables verbose output in test framework
- Shows additional debugging information
- Displays test setup and teardown steps

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#65-verbose-mode)

### 6.6 Debug Mode

**Option**: `--debug` (inherited from `CommandLineOptions`)

**Purpose**: Show diagnostic information

**Behavior**:
- Enables debug logging throughout test execution
- Shows detailed pattern matching and file discovery
- Displays test framework internals
- Controlled by `ConsoleHelpers.ConfigureDebug(true)`

**Source Code**: See [Layer 6 Proof](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md#66-debug-mode)

## Display Format Examples

### Standard Output

```
Found 3 tests...

Running test: TestSuite1.TestCase1
  PASSED (0.5s)

Running test: TestSuite1.TestCase2
  FAILED (0.3s)
  Expected: "success"
  Actual: "failure"

Running test: TestSuite2.TestCase1
  PASSED (1.2s)

===================
Test Run Summary
===================
Total: 3
Passed: 2
Failed: 1
Duration: 2.0s
```

## Data Flow

```
TestRunCommand.ExecuteAsync()
    ↓
TestRunCommand.ExecuteTestRun()
    ↓
FindAndFilterTests() [from TestBaseCommand]
    ↓
Print count message
    ↓
YamlTestFramework.RunTests(tests, consoleHost)
    ↓
    [For each test]
    ↓
    consoleHost displays test start
    ↓
    Execute test
    ↓
    consoleHost displays test result
    ↓
GetOutputFileAndFormat(out file, out format)
    ↓
consoleHost.Finish(results, format, file)
    ↓
    Display summary
    ↓
    Save results to file (Layer 7)
    ↓
Return exit code (0 = all passed, 1 = failures)
```

## Console Host Architecture

The `YamlTestFrameworkConsoleHost` class implements the `IYamlTestFrameworkHost` interface and provides:

1. **Test Start Notifications**: Called when each test begins execution
2. **Test Progress Updates**: Real-time updates during test execution
3. **Test Completion Notifications**: Called when each test finishes
4. **Summary Display**: Called after all tests complete
5. **Color Coding**: Different colors for pass/fail/skip status

## Related Layers

- **Layer 1 (Target Selection)**: Determines which tests to discover
- **Layer 2 (Container Filter)**: Filters which test files to process
- **Layer 3 (Content Filter)**: Filters which tests to execute
- **Layer 4 (Content Removal)**: Removes tests matching exclusion patterns
- **Layer 7 (Output Persistence)**: Saves test results to file
- **Layer 9 (Actions on Results)**: Executes the tests and returns exit code

## Implementation Notes

1. **Console host separation**: Display logic is separated into `YamlTestFrameworkConsoleHost` for testability and reuse
2. **Real-time updates**: Unlike `list` command, `run` provides live feedback during execution
3. **Exit code semantics**: Returns 0 for success (all tests passed), 1 for failure (any test failed)
4. **Timing information**: Tests are timed and duration is displayed
5. **Test framework logger**: Separate logger (`TestLogger`) used for internal framework diagnostics

## Proof Document

For detailed source code evidence with line numbers and full implementation details, see:
- [Layer 6 Proof Document](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md)
