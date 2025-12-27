# cycodt `run` Command - Layer 9: Actions on Results

## Layer Status: ✅ FULLY IMPLEMENTED

## Purpose

Layer 9 defines what **actions** are performed on the filtered and displayed results. For the `run` command, this means:
- **Executing the tests** (running commands, scripts, processes)
- **Generating test reports** (TRX, JUnit XML formats)
- **Returning pass/fail exit codes** (for CI/CD integration)

## Implementation in `run` Command

The `run` command **fully implements Layer 9** with two primary actions:

### Action 1: Execute Tests

After filtering test cases (Layers 1-4), the `run` command **executes each test** by:
1. Running the command/script specified in the test definition
2. Capturing stdout/stderr output
3. Checking expectations (regex patterns, exit codes)
4. Recording pass/fail results

### Action 2: Generate Test Reports

After executing all tests, the `run` command **generates a test report file** in either:
- **TRX format** (Visual Studio test results)
- **JUnit XML format** (standard test results)

The report includes:
- Test names and outcomes (Passed/Failed)
- Execution times
- Error messages and stack traces
- Output captured from tests

## CLI Options

| Option | Argument | Description | Source | Layer |
|--------|----------|-------------|--------|-------|
| `--output-file` | `<path>` | Path to test report file | Lines 163-167 | Layer 7 + 9 |
| `--output-format` | `trx|junit` | Report format (default: `trx`) | Lines 169-180 | Layer 7 + 9 |

### Option Parsing

#### `--output-file` (CycoDtCommandLineOptions.cs, Lines 163-167)

```csharp
163:         else if (command is TestRunCommand runCommand && arg == "--output-file")
164:         {
165:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
166:             var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
167:             runCommand.OutputFile = outputFile;
```

#### `--output-format` (CycoDtCommandLineOptions.cs, Lines 169-180)

```csharp
169:         else if (command is TestRunCommand runCommand2 && arg == "--output-format")
170:         {
171:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
172:             var format = ValidateString(arg, max1Arg.FirstOrDefault(), "output format");
173:             var allowedFormats = new[] { "trx", "junit" };
174:             if (!allowedFormats.Contains(format))
175:             {
176:                 throw new CommandLineException($"Invalid format for --output-format: {format}. Allowed values: trx, junit");
177:             }
178:             runCommand2.OutputFormat = format!;
179:             i += max1Arg.Count();
180:         }
```

## Data Flow

```
Test Cases (filtered from Layers 1-4)
    ↓
┌─────────────────────────────┐
│  ACTION 1: Execute Tests    │
│  YamlTestFramework.RunTests │
└─────────────────────────────┘
    ↓
Test Results (Dictionary<Guid, TestResult>)
    ↓
┌─────────────────────────────┐
│  ACTION 2: Generate Report  │
│  consoleHost.Finish()       │
└─────────────────────────────┘
    ↓
Report File Written (TRX or JUnit XML)
    ↓
Exit Code (0 = pass, 1 = fail)
```

## Source Code Evidence

See [Layer 9 Proof Document](cycodt-run-filtering-pipeline-catalog-layer-9-proof.md) for detailed source code analysis.

### Key Implementation Details

#### 1. Test Execution (TestRunCommand.cs, Lines 37-38)

```csharp
37:             var consoleHost = new YamlTestFrameworkConsoleHost();
38:             var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
```

**What happens**:
- `YamlTestFramework.RunTests()` iterates through all filtered tests
- Each test's command/script is executed in a shell
- stdout/stderr are captured
- Expectations are checked (regex patterns, exit codes)
- Results are stored in a dictionary keyed by test case GUID

#### 2. Report Generation (TestRunCommand.cs, Lines 42-43)

```csharp
42:             GetOutputFileAndFormat(out var file, out var format);
43:             var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
```

**What happens**:
- `GetOutputFileAndFormat()` determines output file path and format
- `consoleHost.Finish()` generates the test report file:
  - **TRX format**: XML with `<TestRun>`, `<Results>`, `<UnitTestResult>` elements
  - **JUnit format**: XML with `<testsuite>`, `<testcase>`, `<failure>` elements
- Returns `true` if all tests passed, `false` otherwise

#### 3. Exit Code Determination (TestRunCommand.cs, Line 47)

```csharp
47:             return passed ? 0 : 1;
```

**What happens**:
- Exit code **0** if all tests passed (CI/CD success)
- Exit code **1** if any tests failed (CI/CD failure)

## Report Format Details

### TRX Format (Visual Studio Test Results)

**Default format** when `--output-format` is not specified or set to `trx`.

**File extension**: `.trx`

**Structure**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="..." name="..." runUser="...">
  <Times creation="..." queuing="..." start="..." finish="..." />
  <Results>
    <UnitTestResult testId="..." testName="..." outcome="Passed|Failed" duration="...">
      <Output>
        <StdOut>...</StdOut>
        <ErrorInfo>
          <Message>...</Message>
          <StackTrace>...</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
  </Results>
  <TestDefinitions>
    <UnitTest id="..." name="...">
      <Execution id="..." />
      <TestMethod codeBase="..." className="..." name="..." />
    </UnitTest>
  </TestDefinitions>
</TestRun>
```

**Implementation**: `src/cycodt/TestFramework/TrxXmlTestReporter.cs`

### JUnit XML Format

**Format** when `--output-format junit` is specified.

**File extension**: `.xml`

**Structure**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<testsuites>
  <testsuite name="..." tests="N" failures="N" errors="0" time="...">
    <testcase name="..." classname="..." time="...">
      <failure message="..." type="...">
        Stack trace...
      </failure>
      <system-out>...</system-out>
    </testcase>
  </testsuite>
</testsuites>
```

**Implementation**: `src/cycodt/TestFramework/JunitXmlTestReporter.cs`

## File Naming Convention

Default file names are determined by `GetOutputFileAndFormat()` (Lines 52-67):

```csharp
52:     private void GetOutputFileAndFormat(out string file, out string format)
53:     {
54:         format = OutputFormat;
55:         var ext = format switch
56:         {
57:             "trx" => "trx",
58:             "junit" => "xml",
59:             _ => throw new Exception($"Unknown format: {format}")
60:         };
61: 
62:         file = OutputFile ?? $"test-results.{ext}";
63:         if (!file.EndsWith($".{ext}"))
64:         {
65:             file += $".{ext}";
66:         }
67:     }
```

**Default file names**:
- TRX format: `test-results.trx`
- JUnit format: `test-results.xml`

**Custom file names**:
- Specified via `--output-file <path>`
- Extension is automatically appended if missing

## Execution Context

Tests are executed through the test framework with:
- **Working directory**: Can be overridden per-test or globally via `--working-dir`
- **Environment variables**: Inherited from parent process + test-specific vars
- **Shell**: Bash on Unix/macOS, cmd on Windows (configurable per-test)
- **Timeout**: Per-test timeout values (default or specified)
- **Parallelization**: Tests can run in parallel (configurable per-test)

## Comparison with Other Commands

| Command | Execute Tests | Generate Reports | Modify State | Exit Code Reflects |
|---------|---------------|------------------|--------------|-------------------|
| `list` | ❌ No | ❌ No | ❌ No | Success of listing |
| **`run`** | ✅ **Yes** | ✅ **Yes** | ✅ **Yes** | **Test pass/fail** |
| `expect check` | ❌ No | ❌ No | ❌ No | Expectation pass/fail |
| `expect format` | ❌ No | ❌ No | ⚠️ Output only | Success of formatting |

## Exit Code Behavior

The `run` command's exit code is **critically important for CI/CD pipelines**:

```bash
# CI/CD usage example
cycodt run --file tests/**/*.yaml --output-file test-results.trx
if [ $? -eq 0 ]; then
  echo "All tests passed!"
else
  echo "Some tests failed!"
  exit 1
fi
```

**Exit codes**:
- **0**: All tests passed
- **1**: One or more tests failed
- **1**: Exception during test execution

## Related Layers

- **Layer 1** ([Target Selection](cycodt-run-filtering-pipeline-catalog-layer-1.md)): Determines which test files to execute
- **Layer 2** ([Container Filter](cycodt-run-filtering-pipeline-catalog-layer-2.md)): Filters which test files to execute
- **Layer 3** ([Content Filter](cycodt-run-filtering-pipeline-catalog-layer-3.md)): Filters which tests to execute
- **Layer 7** ([Output Persistence](cycodt-run-filtering-pipeline-catalog-layer-7.md)): Coordinates with Layer 9 for report generation
- **Contrast with `list` Layer 9** ([list Actions](cycodt-list-filtering-pipeline-catalog-layer-9.md)): Shows passive listing vs active execution

## CI/CD Integration

The `run` command is designed for CI/CD integration:

### GitHub Actions Example

```yaml
- name: Run Tests
  run: cycodt run --output-file test-results.trx --output-format trx
  
- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Test Results
    path: test-results.trx
    reporter: dotnet-trx
```

### Azure Pipelines Example

```yaml
- task: Bash@3
  displayName: 'Run Tests'
  inputs:
    targetType: 'inline'
    script: 'cycodt run --output-file $(Build.SourcesDirectory)/test-results.trx'
    
- task: PublishTestResults@2
  displayName: 'Publish Test Results'
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/test-results.trx'
```

## Performance Characteristics

- **Sequential execution**: Tests run in order by default
- **Parallel execution**: Tests with `parallelize: true` can run concurrently
- **Timeout handling**: Tests exceeding timeout are killed and marked as failed
- **Resource cleanup**: Processes are terminated, temporary files are cleaned up
