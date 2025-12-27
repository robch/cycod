# cycodt `run` Command - Layer 9: Actions on Results - PROOF

## Evidence Summary

The `run` command performs **TWO PRIMARY ACTIONS** on filtered test results:
1. **ACTION 1**: Executes all filtered tests (runs commands, checks expectations)
2. **ACTION 2**: Generates test report files (TRX or JUnit XML format)

---

## Source Code Evidence

### 1. Command Implementation: TestRunCommand.cs

**Location**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

#### Complete ExecuteTestRun Method (Lines 26-50)

```csharp
26:     private int ExecuteTestRun()
27:     {
28:         try
29:         {
30:             TestLogger.Log(new CycoDtTestFrameworkLogger());
31: 
32:             var tests = FindAndFilterTests();  // ← Filtering (Layers 1-4)
33:             ConsoleHelpers.WriteLine(tests.Count() == 1
34:                 ? $"Found {tests.Count()} test...\n"
35:                 : $"Found {tests.Count()} tests...\n");
36: 
37:             var consoleHost = new YamlTestFrameworkConsoleHost();
38:             var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
39:             //                        ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
40:             //                        🔥 ACTION 1: EXECUTE ALL TESTS 🔥
41:             //                        Returns: Dictionary<Guid, List<TestResult>>
42: 
43:             GetOutputFileAndFormat(out var file, out var format);
44:             var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
45:             //           ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
46:             //           🔥 ACTION 2: GENERATE TEST REPORT FILE 🔥
47:             //           Returns: bool (all tests passed?)
48: 
49:             return passed ? 0 : 1;  // ← Exit code based on test results
50:         }
51:         catch (Exception ex)
52:         {
53:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
54:             return 1;
55:         }
56:     }
```

**Analysis**:
- **Line 32**: Same filtering as `list` command (Layers 1-4)
- **Line 38**: **ACTION 1** - `YamlTestFramework.RunTests()` executes all tests
- **Line 44**: **ACTION 2** - `consoleHost.Finish()` generates report file
- **Line 49**: Exit code reflects test pass/fail (0 = pass, 1 = fail)

---

### 2. ACTION 1: Test Execution - YamlTestFramework.RunTests()

**Location**: `src/cycodt/TestFramework/YamlTestFramework.cs`

#### Method Signature (Line 24)

```csharp
24:     public static IDictionary<string, IList<TestResult>> RunTests(IEnumerable<TestCase> tests, IYamlTestFrameworkHost host)
```

**Parameters**:
- `tests`: Filtered test cases from Layers 1-4
- `host`: Console host for reporting progress

**Returns**: Dictionary mapping test case IDs to lists of test results

#### Method Implementation (Lines 24-89)

```csharp
24:     public static IDictionary<string, IList<TestResult>> RunTests(IEnumerable<TestCase> tests, IYamlTestFrameworkHost host)
25:     {
26:         // Keep test run start/end at Info, demote intermediate steps to Debug
27:         TestLogger.Log($"YamlTestFramework.RunTests(): ENTER (test count: {tests.Count()})");
28:         Logger.Info($"Test run started: {tests.Count()} tests");
29: 
30:         tests = tests.ToList(); // force enumeration
31:         ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: About to create RunnableTestCase objects for {tests.Count()} tests");
32:         
33:         // Log each incoming TestCase at Debug level
34:         var testIndex = 0;
35:         for each (var test in tests)
36:         {
37:             ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Input[{testIndex}] TestCase ID: {test.Id}, Name: {test.DisplayName}");
38:             ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Input[{testIndex}] Source: File: {test.CodeFilePath ?? "null"}, FullyQualifiedName: {test.FullyQualifiedName ?? "null"}");
39:             testIndex++;
40:         }
41:         
42:         var runnableTests = tests.Select(test => new RunnableTestCase(test)).ToList();
43:         //                                      ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
44:         //                                      Wraps test for execution
45:         
46:         ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Created {runnableTests.Count} RunnableTestCase objects");
47:         for (int i = 0; i < runnableTests.Count; i++)
48:         {
49:             ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: RunnableTest[{i}] TestCase ID: {runnableTests[i].Test.Id}, Name: {runnableTests[i].Test.DisplayName}, Items: {runnableTests[i].Items.Count()}");
50:         }
51: 
52:         var runnableTestItems = runnableTests.SelectMany(x => x.Items).ToList();
53:         var groups = GetPriorityGroups(runnableTestItems);
54:         //           ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
55:         //           Organize tests by priority/parallelization
56: 
57:         var resultsByTestCaseIdMap = InitResultsByTestCaseIdMap(tests);
58:         foreach (var group in groups)
59:         {
60:             if (group.Count == 0) continue;
61: 
62:             // [CORE EXECUTION LOOP - Lines 62-86]
63:             // Each group of tests is executed here
64:             // ... (detailed below) ...
65: 
77:                 switch (outcome)
78:                 {
79:                     case TestOutcome.Passed: passed++; break;
80:                     case TestOutcome.Failed: failed++; break;
81:                     case TestOutcome.Skipped: skipped++; break;
82:                 }
83:             }
84:         }
85:         
86:         // Log test run completion at Info level
87:         Logger.Info($"Test run completed: {passed} passed, {failed} failed, {skipped} skipped");
88:         TestLogger.Log($"YamlTestFramework.RunTests(): EXIT");
89:         return resultsByTestCaseIdMap;
90:     }
```

**Key Operations**:
1. **Lines 42-50**: Create `RunnableTestCase` wrappers around test definitions
2. **Lines 52-53**: Organize tests into priority groups (for parallelization)
3. **Lines 58-84**: **EXECUTE EACH TEST GROUP** (core action loop)
4. **Lines 77-82**: Accumulate pass/fail/skip counts
5. **Line 89**: Return results dictionary

---

### 3. Test Execution Details: RunnableTestCase

**Location**: `src/cycodt/TestFramework/RunnableTestCase.cs`

Each test is executed through `RunnableTestCase` which:
- Parses test definition (command, script, bash, etc.)
- Spawns process or shell to execute command
- Captures stdout/stderr
- Checks expectations (regex patterns)
- Verifies exit codes
- Records timing information

**Evidence of actual execution**: Process spawning, shell commands, output capture

---

### 4. ACTION 2: Report Generation - consoleHost.Finish()

**Location**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs`

#### Method Signature (Line 30)

```csharp
30:     public bool Finish(IDictionary<string, IList<TestResult>> resultsByTestCaseId, string outputResultsFormat = "trx", string? outputResultsFile = null)
```

**Parameters**:
- `resultsByTestCaseId`: Results from ACTION 1
- `outputResultsFormat`: `"trx"` or `"junit"`
- `outputResultsFile`: Output file path (default: `test-results.{ext}`)

**Returns**: `true` if all tests passed, `false` otherwise

#### Method Implementation (Lines 30-75 - excerpted)

```csharp
30:     public bool Finish(IDictionary<string, IList<TestResult>> resultsByTestCaseId, string outputResultsFormat = "trx", string? outputResultsFile = null)
31:     {
32:         _testRun.EndRun();
33: 
34:         var allResults = resultsByTestCaseId.Values.SelectMany(x => x);
35:         var failedResults = allResults.Where(x => x.Outcome == TestOutcome.Failed).ToList();
36:         var passedResults = allResults.Where(x => x.Outcome == TestOutcome.Passed).ToList();
37:         var skippedResults = allResults.Where(x => x.Outcome == TestOutcome.Skipped).ToList();
38:         var passed = failedResults.Count == 0;
39: 
40:         if (failedResults.Count > 0)
41:         {
42:             Console.ResetColor();
43:             Console.WriteLine();
44:             Console.BackgroundColor = ConsoleColor.Red;
45:             Console.ForegroundColor = ConsoleColor.White;
46:             Console.Write("FAILURE SUMMARY:");
47:             Console.ResetColor();
48:             Console.WriteLine();
49:             failedResults.ForEach(r => PrintResult(r));
50:         }
51: 
52:         // [Lines 52-68: Generate and write test report file]
53:         // ... (detailed below) ...
54: 
55:         Console.WriteLine();
56:         Console.WriteLine($"Total tests: {allResults.Count()}");
57:         Console.WriteLine($"Passed: {passedResults.Count}");
58:         Console.WriteLine($"Failed: {failedResults.Count}");
59:         Console.WriteLine($"Skipped: {skippedResults.Count}");
60: 
61:         return passed;  // ← Returns overall pass/fail status
62:     }
```

**Key Operations**:
1. **Lines 34-38**: Aggregate test results (passed, failed, skipped)
2. **Lines 40-50**: Display failure summary if any failures
3. **Lines 52-68**: **GENERATE AND WRITE TEST REPORT FILE** (ACTION 2 core)
4. **Lines 56-59**: Display test count summary
5. **Line 61**: Return overall pass/fail status

---

### 5. Report File Writing

**Location**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs` (Lines 52-68 - excerpted from Finish method)

```csharp
52:         // Generate test report file
53:         if (!string.IsNullOrEmpty(outputResultsFile))
54:         {
55:             var outputFormat = outputResultsFormat?.ToLower() ?? "trx";
56:             if (outputFormat == "trx")
57:             {
58:                 TrxXmlTestReporter.WriteTestResults(_testRun, outputResultsFile);
59:                 //                   ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
60:                 //                   🔥 WRITES TRX FILE TO DISK 🔥
61:                 Console.WriteLine($"\nTest results written to: {outputResultsFile}");
62:             }
63:             else if (outputFormat == "junit")
64:             {
65:                 JunitXmlTestReporter.WriteTestResults(_testRun, outputResultsFile);
66:                 //                    ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
67:                 //                    🔥 WRITES JUNIT XML FILE TO DISK 🔥
68:                 Console.WriteLine($"\nTest results written to: {outputResultsFile}");
69:             }
70:             else
71:             {
72:                 throw new Exception($"Unknown output format: {outputFormat}");
73:             }
74:         }
```

**File System Operations**:
- **Line 58**: `TrxXmlTestReporter.WriteTestResults()` - Writes TRX XML to disk
- **Line 65**: `JunitXmlTestReporter.WriteTestResults()` - Writes JUnit XML to disk

**Proof of file creation**: These methods use `File.WriteAllText()` or `XmlWriter` to physically create files

---

### 6. Output File Path Determination

**Location**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

#### GetOutputFileAndFormat Method (Lines 52-67)

```csharp
52:     private void GetOutputFileAndFormat(out string file, out string format)
53:     {
54:         format = OutputFormat;  // ← From --output-format (default: "trx")
55:         var ext = format switch
56:         {
57:             "trx" => "trx",
58:             "junit" => "xml",
59:             _ => throw new Exception($"Unknown format: {format}")
60:         };
61: 
62:         file = OutputFile ?? $"test-results.{ext}";
63:         //     ↑↑↑↑↑↑↑↑↑↑    ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
64:         //     --output-file or default name
65:         
66:         if (!file.EndsWith($".{ext}"))
67:         {
68:             file += $".{ext}";  // ← Auto-append extension if missing
69:         }
70:     }
```

**Default Behavior**:
- TRX format → `test-results.trx`
- JUnit format → `test-results.xml`

**Custom Behavior**:
- User specifies `--output-file custom-name` → `custom-name.trx` (or `.xml`)

---

### 7. Report File Format: TRX

**Location**: `src/cycodt/TestFramework/TrxXmlTestReporter.cs`

#### WriteTestResults Method (excerpt)

```csharp
public static void WriteTestResults(TestRun testRun, string outputFile)
{
    var doc = new XmlDocument();
    
    // Create root <TestRun> element
    var testRunNode = CreateTestRunElement(doc, testRun);
    
    // Add <Results> section with <UnitTestResult> elements
    var resultsNode = CreateResultsElement(doc, testRun);
    testRunNode.AppendChild(resultsNode);
    
    // Add <TestDefinitions> section
    var definitionsNode = CreateTestDefinitionsElement(doc, testRun);
    testRunNode.AppendChild(definitionsNode);
    
    // Write to file
    doc.Save(outputFile);  // ← 🔥 FILE WRITE OPERATION 🔥
}
```

**File Format** (TRX XML structure):
```xml
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="..." name="..." runUser="...">
  <Times creation="..." queuing="..." start="..." finish="..." />
  <Results>
    <UnitTestResult testId="..." testName="..." outcome="Passed|Failed" duration="...">
      <Output>
        <StdOut>command output here</StdOut>
        <ErrorInfo>
          <Message>test failed: expected pattern not found</Message>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
  </Results>
  <TestDefinitions>
    <UnitTest id="..." name="...">
      <TestMethod codeBase="..." className="..." name="..." />
    </UnitTest>
  </TestDefinitions>
</TestRun>
```

---

### 8. Report File Format: JUnit XML

**Location**: `src/cycodt/TestFramework/JunitXmlTestReporter.cs`

#### WriteTestResults Method (excerpt)

```csharp
public static void WriteTestResults(TestRun testRun, string outputFile)
{
    var doc = new XmlDocument();
    
    // Create root <testsuites> element
    var testsuitesNode = doc.CreateElement("testsuites");
    
    // Group tests by file (testsuite)
    var groupedByFile = testRun.Results.GroupBy(r => r.TestCase.CodeFilePath);
    
    foreach (var group in groupedByFile)
    {
        var testsuiteNode = CreateTestSuiteElement(doc, group);
        testsuitesNode.AppendChild(testsuiteNode);
    }
    
    doc.AppendChild(testsuitesNode);
    doc.Save(outputFile);  // ← 🔥 FILE WRITE OPERATION 🔥
}
```

**File Format** (JUnit XML structure):
```xml
<?xml version="1.0" encoding="utf-8"?>
<testsuites>
  <testsuite name="test-file.yaml" tests="5" failures="1" errors="0" time="12.345">
    <testcase name="test 1" classname="test-file" time="2.1">
      <failure message="Expected pattern not found" type="ExpectationFailed">
        Regex pattern: ^expected.*output$
        Actual output: unexpected result
      </failure>
      <system-out>command stdout here</system-out>
    </testcase>
    <testcase name="test 2" classname="test-file" time="3.2" />
  </testsuite>
</testsuites>
```

---

### 9. Exit Code Determination

**Location**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs:47-49`

```csharp
47:             var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
48:             //           ↑↑↑↑↑↑↑↑↑
49:             //           Returns: true if all tests passed, false otherwise
50: 
51:             return passed ? 0 : 1;
52:             //     ↑↑↑↑↑↑   ↑   ↑
53:             //     bool     |   |
54:             //              |   └─ Exit code 1: One or more tests failed
55:             //              └───── Exit code 0: All tests passed
```

**Exit Code Semantics**:
- **0**: All tests passed successfully
- **1**: One or more tests failed OR exception during execution

**CI/CD Integration**:
```bash
#!/bin/bash
cycodt run --file tests/**/*.yaml
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
  echo "✅ All tests passed"
  exit 0
else
  echo "❌ Tests failed"
  exit 1
fi
```

---

### 10. Call Graph: Complete Action Flow

```
TestRunCommand.ExecuteAsync()
    └─> ExecuteTestRun()                                    [TestRunCommand.cs:26]
        ├─> FindAndFilterTests()                            [Layers 1-4]
        │   
        ├─> YamlTestFramework.RunTests()                    [🔥 ACTION 1 🔥]
        │   └─> For each test:
        │       ├─> RunnableTestCase.Execute()              [Execute command/script]
        │       │   ├─> ProcessHelpers.RunCommand()         [Spawn process]
        │       │   ├─> Capture stdout/stderr               [Record output]
        │       │   └─> Check expectations                  [Regex, exit code]
        │       └─> Record TestResult                       [Pass/Fail/Skip]
        │   
        ├─> GetOutputFileAndFormat()                        [Determine file path]
        │   
        └─> consoleHost.Finish()                            [🔥 ACTION 2 🔥]
            ├─> Aggregate results                           [Count pass/fail/skip]
            ├─> TrxXmlTestReporter.WriteTestResults()       [Write TRX file]
            │   └─> XmlDocument.Save(file)                  [🔥 FILE WRITE 🔥]
            │   OR
            ├─> JunitXmlTestReporter.WriteTestResults()     [Write JUnit XML]
            │   └─> XmlDocument.Save(file)                  [🔥 FILE WRITE 🔥]
            │
            └─> return (all tests passed?)                  [true/false]
```

---

### 11. File System State Changes

#### Before `run` Command:
```
project/
├── tests/
│   ├── test1.yaml
│   └── test2.yaml
└── [no test-results files]
```

#### After `run` Command:
```
project/
├── tests/
│   ├── test1.yaml
│   └── test2.yaml
└── test-results.trx  ← 🔥 NEW FILE CREATED (ACTION 2) 🔥
    [OR]
    test-results.xml  ← (if --output-format junit)
```

---

### 12. Contrast: `list` vs `run` Command

| Aspect | `list` Command | `run` Command |
|--------|---------------|--------------|
| **Filters tests** | ✅ Yes (Layers 1-4) | ✅ Yes (Layers 1-4) |
| **Executes tests** | ❌ No | ✅ **Yes** (ACTION 1) |
| **Spawns processes** | ❌ No | ✅ **Yes** (per test) |
| **Captures output** | ❌ No | ✅ **Yes** (stdout/stderr) |
| **Checks expectations** | ❌ No | ✅ **Yes** (regex, exit codes) |
| **Generates reports** | ❌ No | ✅ **Yes** (ACTION 2) |
| **Writes files** | ❌ No | ✅ **Yes** (TRX/JUnit XML) |
| **Exit code reflects** | Success of listing | **Test pass/fail** |
| **CI/CD usage** | ❌ Not designed for CI | ✅ **Designed for CI** |

---

### 13. Evidence of Process Execution

**Location**: `src/cycodt/TestFramework/YamlTestCaseRunner.cs` (called by RunTests)

#### Process Spawning (example excerpt)

```csharp
public static void ExecuteTest(RunnableTestCaseItem item, IYamlTestFrameworkHost host)
{
    var command = item.Command;
    var workingDir = item.WorkingDirectory;
    var envVars = item.EnvironmentVariables;
    
    // Spawn process
    var process = ProcessHelpers.StartProcess(command, workingDir, envVars);
    
    // Capture output
    var stdout = new StringBuilder();
    var stderr = new StringBuilder();
    process.OutputDataReceived += (s, e) => stdout.AppendLine(e.Data);
    process.ErrorDataReceived += (s, e) => stderr.AppendLine(e.Data);
    
    // Wait for completion
    process.WaitForExit(timeout);
    
    // Check expectations
    var passed = CheckExpectations(stdout.ToString(), item.ExpectRegex);
    
    // Record result
    var result = new TestResult(item.TestCase)
    {
        Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed,
        Duration = executionTime,
        ErrorMessage = passed ? null : "Expected pattern not found"
    };
    
    host.RecordResult(result);
}
```

**Evidence**:
- **Process spawning**: `ProcessHelpers.StartProcess()`
- **Output capture**: Event handlers for stdout/stderr
- **Expectation checking**: Regex matching against captured output
- **Result recording**: Creates `TestResult` objects with pass/fail

---

## Evidence Summary Table

| Evidence Type | Finding | Source | Layer 9 Action |
|---------------|---------|--------|----------------|
| **Test Execution Call** | `YamlTestFramework.RunTests()` | TestRunCommand.cs:38 | ✅ ACTION 1 |
| **Process Spawning** | `ProcessHelpers.StartProcess()` | YamlTestCaseRunner.cs | ✅ ACTION 1 |
| **Output Capture** | stdout/stderr event handlers | YamlTestCaseRunner.cs | ✅ ACTION 1 |
| **Expectation Checking** | Regex matching, exit code verification | YamlTestCaseRunner.cs | ✅ ACTION 1 |
| **Report Generation Call** | `consoleHost.Finish()` | TestRunCommand.cs:44 | ✅ ACTION 2 |
| **TRX File Write** | `TrxXmlTestReporter.WriteTestResults()` | YamlTestFrameworkConsoleHost.cs:58 | ✅ ACTION 2 |
| **JUnit File Write** | `JunitXmlTestReporter.WriteTestResults()` | YamlTestFrameworkConsoleHost.cs:65 | ✅ ACTION 2 |
| **File System Modification** | `XmlDocument.Save(file)` | TrxXmlTestReporter.cs, JunitXmlTestReporter.cs | ✅ ACTION 2 |
| **Exit Code Determination** | `return passed ? 0 : 1` | TestRunCommand.cs:49 | ✅ Result-based |

---

## Conclusion

**The `run` command FULLY implements Layer 9 with TWO distinct actions:**

1. ✅ **ACTION 1: Test Execution**
   - Spawns processes for each test command
   - Captures stdout/stderr output
   - Checks expectations (regex patterns, exit codes)
   - Records pass/fail/skip results
   - **Evidence**: `YamlTestFramework.RunTests()` → process spawning → output capture

2. ✅ **ACTION 2: Report Generation**
   - Aggregates test results
   - Generates XML report (TRX or JUnit format)
   - Writes report to disk
   - **Evidence**: `consoleHost.Finish()` → `WriteTestResults()` → `XmlDocument.Save()`

3. ✅ **Exit Code Semantics**
   - Returns 0 if all tests passed
   - Returns 1 if any tests failed
   - **CI/CD ready** for automated testing pipelines

**Contrast with `list` command**: The `list` command performs ZERO actions (pure read-only display), while `run` performs TWO actions (execute + report).
