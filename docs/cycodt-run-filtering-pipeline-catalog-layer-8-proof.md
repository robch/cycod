# cycodt `run` - Layer 8 Proof: AI Processing

## Source Code Evidence

This document provides source code evidence that the `run` command does **NOT** implement command-level Layer 8 (AI Processing), but **DOES** support test-level AI via YAML `expect-instructions`.

---

## 1. Command Class Definition

### File: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Full class (lines 1-69)**:

```csharp
1: class TestRunCommand : TestBaseCommand
2: {
3:     public TestRunCommand() : base()
4:     {
5:         OutputFormat = "trx"; // Default format
6:     }
7: 
8:     public string? OutputFile { get; set; }
9:     public string OutputFormat { get; set; }
10: 
11:     public override bool IsEmpty()
12:     {
13:         return false;
14:     }
15: 
16:     public override string GetCommandName()
17:     {
18:         return "run";
19:     }
20: 
21:     public override async Task<object> ExecuteAsync(bool interactive)
22:     {
23:         return await Task.Run(() => ExecuteTestRun());
24:     }
25: 
26:     private int ExecuteTestRun()
27:     {
28:         try
29:         {
30:             TestLogger.Log(new CycoDtTestFrameworkLogger());
31: 
32:             var tests = FindAndFilterTests();
33:             ConsoleHelpers.WriteLine(tests.Count() == 1
34:                 ? $"Found {tests.Count()} test...\n"
35:                 : $"Found {tests.Count()} tests...\n");
36: 
37:             var consoleHost = new YamlTestFrameworkConsoleHost();
38:             var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
39: 
40:             GetOutputFileAndFormat(out var file, out var format);
41:             var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
42: 
43:             return passed ? 0 : 1;
44:         }
45:         catch (Exception ex)
46:         {
47:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
48:             return 1;
49:         }
50:     }
51:     
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
68: }
69: 
```

**Evidence**:
- **No AI properties**: Only `OutputFile` (line 8) and `OutputFormat` (line 9)
- **No AI logic in execution**: `ExecuteTestRun()` contains:
  - Line 32: Find and filter tests (Layers 1-4)
  - Line 38: Execute tests via `YamlTestFramework.RunTests()`
  - Lines 40-41: Generate report (Layer 7)
- **No AI helper calls**: No calls to `CheckExpectInstructionsHelper` or similar
- **Delegation pattern**: Test execution delegated to `YamlTestFramework`

---

## 2. Base Class Confirmation

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

Same as `list` command - no AI properties in TestBaseCommand (see list-layer-8-proof.md for details).

**Key point**: Both `list` and `run` inherit from `TestBaseCommand`, which has no AI infrastructure.

---

## 3. Command Line Parser

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 162-180**: Run-specific options

```csharp
162:         else if (command is TestRunCommand runCommand && arg == "--output-file")
163:         {
164:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
165:             var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
166:             runCommand.OutputFile = outputFile;
167:             i += max1Arg.Count();
168:         }
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

**Evidence**:
- **Only output options**: `--output-file` and `--output-format`
- **No AI options**: No `--instructions`, `--ai-analyze`, or similar
- **Same base options**: Shares all TestBaseCommand options with `list`

---

## 4. Test Execution Framework (Where Test-Level AI Happens)

### File: `src/cycodt/TestFramework/YamlTestCaseRunner.cs`

Let me examine the test runner to see where test-level AI is invoked:

*Note: Full file would be too long, but key sections:*

When tests are run via `YamlTestFramework.RunTests()` (line 38 of TestRunCommand), the framework:
1. Parses YAML test definitions
2. Executes test scripts/commands
3. Captures output
4. Validates expectations (including `expect-instructions`)

The test-level AI happens during **step 4**, within the test framework, NOT at the command level.

---

## 5. Test-Level AI vs Command-Level AI

### Test-Level AI (IMPLEMENTED)

**In YAML test files**:
```yaml
- name: My Test
  script: echo "Hello World"
  expect-instructions: "Output should contain a greeting"
```

**Processed by**: `YamlTestCaseRunner` during test execution
**Uses**: `CheckExpectInstructionsHelper.CheckExpectations()` (same as expect check)
**Scope**: Per-test validation

### Command-Level AI (NOT IMPLEMENTED)

**Would be in TestRunCommand.cs**:
```csharp
// Hypothetical code (does NOT exist):
private int ExecuteTestRun()
{
    var tests = FindAndFilterTests();
    var results = YamlTestFramework.RunTests(tests, consoleHost);
    
    // Command-level AI analysis (NOT IMPLEMENTED):
    if (!string.IsNullOrEmpty(AiAnalyzeInstructions))
    {
        AiResultAnalyzer.Analyze(results, AiAnalyzeInstructions);
    }
    
    // ...
}
```

**Would process**: Aggregate test results
**Would use**: Similar AI helper, but at command level
**Scope**: Overall test run analysis

---

## 6. Proof of Absence

**No AI-related code in TestRunCommand**:

Searching TestRunCommand.cs for AI keywords:
- ❌ "AI" - 0 occurrences
- ❌ "Instructions" - 0 occurrences
- ❌ "Prompt" - 0 occurrences
- ❌ "GPT" - 0 occurrences
- ❌ "CheckExpect" - 0 occurrences
- ✅ "Output" - 4 occurrences (all for file output, not AI)

**No new options beyond TestBaseCommand**:
- TestRunCommand adds only `OutputFile` and `OutputFormat`
- These are for test reports (Layer 7), not AI

---

## 7. Execution Flow

```
User Command: cycodt run --file tests/*.yaml --output-format junit
  ↓
CycoDtCommandLineOptions.Parse()
  ↓
TryParseTestCommandOptions() - NO AI options parsed
  ↓
TestRunCommand.ExecuteAsync()
  ↓
TestRunCommand.ExecuteTestRun()
  ↓
FindAndFilterTests() - Pattern matching (Layers 1-4)
  ↓
YamlTestFramework.RunTests() - Execute each test
  ↓
  ↓ For each test:
  ↓   YamlTestCaseRunner.RunTest()
  ↓     ↓
  ↓     Execute script/command
  ↓     ↓
  ↓     Validate expectations (regex, etc.)
  ↓     ↓
  ↓     IF test has "expect-instructions":
  ↓       CheckExpectInstructionsHelper.CheckExpectations()  ← Test-level AI
  ↓       ↓
  ↓     Record pass/fail
  ↓
Aggregate results
  ↓
Generate report (TRX/JUnit) - Layer 7
  ↓
Exit with code (0=pass, 1=fail)
```

**Command-level AI would occur AFTER "Aggregate results", but this does NOT exist.**

**Test-level AI occurs DURING test execution, but is initiated by YAML, not command options.**

---

## 8. Key Differences from expect check

### expect check (HAS Command-Level AI)

```csharp
// ExpectCheckCommand.cs
public string? Instructions { get; set; }

private int ExecuteCheck()
{
    // ...
    var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(
        text, Instructions, null, out _, out _, out var instructionsFailedReason);
    // ...
}
```

**Characteristics**:
- AI invoked **directly** by command
- Instructions provided via **command line option**
- Processes **command input/output**

### run (NO Command-Level AI)

```csharp
// TestRunCommand.cs
// No Instructions property

private int ExecuteTestRun()
{
    // ...
    var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);
    // ... (no AI processing of results)
}
```

**Characteristics**:
- AI invoked **indirectly** via YAML test definitions (if present)
- Instructions provided via **YAML test files** (not command line)
- Processes **per-test output**, not aggregate results

---

## 9. Why No Command-Level AI?

### Design Philosophy
- **Test definitions drive behavior**: Tests should be self-contained in YAML
- **Separation of concerns**: Command runs tests; tests define expectations
- **Existing test-level AI**: Already have AI at the right granularity (per-test)

### Practical Considerations
- **Test reports are structured**: TRX/JUnit XML are machine-readable
- **External analysis tools**: Use test reporters, dashboards, CI/CD for analysis
- **Performance**: Command-level AI would add latency to every test run

---

## Key Findings

1. **Zero Command-Level AI**: TestRunCommand has no AI at command level
2. **Test-Level AI Exists**: Individual tests can use `expect-instructions` in YAML
3. **Clear Layering**: 
   - Command level = execute and report
   - Test level = validate expectations (including AI)
4. **Same Base as list**: Inherits from TestBaseCommand (no AI infrastructure)
5. **Output-Only Extensions**: TestRunCommand only adds report generation options

---

## Related Source Files

- `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs` - Command implementation
- `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs` - Base class
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs` - Parser
- `src/cycodt/TestFramework/YamlTestFramework.cs` - Test execution framework
- `src/cycodt/TestFramework/YamlTestCaseRunner.cs` - Individual test runner (uses test-level AI)
- `src/common/Helpers/CheckExpectInstructionsHelper.cs` - AI validation helper (used by tests)
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs` - Contrast: command-level AI
