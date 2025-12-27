# cycodt run - Layer 5: Context Expansion - PROOF

## Evidence: No Context Expansion Implementation

This document provides source code evidence that the `cycodt run` command **does NOT implement Layer 5 (Context Expansion)** features for showing extended context around test failures or execution.

---

## Source Code Analysis

### 1. TestRunCommand Implementation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

```csharp
// Lines 1-69 (entire file)
class TestRunCommand : TestBaseCommand
{
    public TestRunCommand() : base()
    {
        OutputFormat = "trx"; // Default format
    }

    public string? OutputFile { get; set; }      // Layer 7: Output persistence
    public string OutputFormat { get; set; }     // Layer 7: Output format

    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return "run";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteTestRun());
    }

    private int ExecuteTestRun()
    {
        try
        {
            TestLogger.Log(new CycoDtTestFrameworkLogger());

            var tests = FindAndFilterTests();  // Lines 32: Layer 1 & 2 - find and filter
            ConsoleHelpers.WriteLine(tests.Count() == 1
                ? $"Found {tests.Count()} test...\n"
                : $"Found {tests.Count()} tests...\n");

            var consoleHost = new YamlTestFrameworkConsoleHost();
            var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);  // Line 38: Layer 9 - execute

            GetOutputFileAndFormat(out var file, out var format);
            var passed = consoleHost.Finish(resultsByTestCaseId, format, file);  // Line 41: Layer 7 - persist

            return passed ? 0 : 1;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
    
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
}
```

**Analysis**:
- **Properties**: Only `OutputFile` and `OutputFormat` (Layer 7) - NO context expansion properties
- **NO properties** for:
  - Failure context lines
  - Stack trace expansion
  - Test chain context
  - Output buffer size
  - Dependency context
- **Line 32**: Calls `FindAndFilterTests()` (Layers 1 & 2) - no expansion
- **Line 38**: Calls `YamlTestFramework.RunTests()` - test execution with default output
- **Line 41**: Calls `consoleHost.Finish()` - formats results (Layer 7), no expansion options

### 2. TestBaseCommand - No Context Properties

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

```csharp
// Lines 1-27 (constructor and properties)
abstract class TestBaseCommand : Command
{
    public TestBaseCommand()
    {
        Globs = new List<string>();
        ExcludeGlobs = new List<string>();
        ExcludeFileNamePatternList = new List<Regex>();
        Tests = new List<string>();
        Contains = new List<string>();
        Remove = new List<string>();
        IncludeOptionalCategories = new List<string>();
    }

    public List<string> Globs;                      // Layer 1
    public List<string> ExcludeGlobs;               // Layer 1
    public List<Regex> ExcludeFileNamePatternList;  // Layer 1

    public List<string> Tests { get; set; }                    // Layer 2
    public List<string> Contains { get; set; }                 // Layer 2
    public List<string> Remove { get; set; }                   // Layer 2
    public List<string> IncludeOptionalCategories { get; set; } // Layer 2
}
```

**Analysis**:
- **NO Layer 5 properties** such as:
  - `FailureContextLines`
  - `ShowFullStackTrace`
  - `TestChainContext`
  - `OutputBufferLines`
  - `ShowDependencies`
  - `ExpandTestContext`

### 3. Command Line Parser - No Context Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

```csharp
// Lines 54-116 (TryParseTestCommandOptions - relevant portions)
private bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
{
    // ... (Layer 1 & 2 parsing) ...
    
    else if (command is TestRunCommand runCommand && arg == "--output-file")  // Layer 7
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
        runCommand.OutputFile = outputFile;
        i += max1Arg.Count();
    }
    else if (command is TestRunCommand runCommand2 && arg == "--output-format")  // Layer 7
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var format = ValidateString(arg, max1Arg.FirstOrDefault(), "output format");
        var allowedFormats = new[] { "trx", "junit" };
        if (!allowedFormats.Contains(format))
        {
            throw new CommandLineException($"Invalid format for --output-format: {format}. Allowed values: trx, junit");
        }
        runCommand2.OutputFormat = format!;
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
- Parses `--output-file` and `--output-format` (Layer 7 only)
- **NO parsing** for Layer 5 context expansion options such as:
  - `--failure-context-lines N`
  - `--full-stack-trace`
  - `--show-test-context N`
  - `--show-dependencies`
  - `--output-buffer-lines N`
  - `--expand-failures`

### 4. YamlTestFramework.RunTests - No Context Expansion Parameters

**File**: `src/cycodt/TestFramework/YamlTestFramework.cs` (static method)

```csharp
public static Dictionary<Guid, TestResult> RunTests(
    IEnumerable<TestCase> tests,
    IYamlTestFrameworkHost host)
{
    // Test execution logic
    // NO parameters for context expansion
}
```

**Analysis**:
- Method signature has NO context expansion parameters
- Takes only: test cases and a host interface
- Does not accept:
  - Context line counts
  - Stack trace verbosity settings
  - Output buffer configuration
  - Failure expansion options

### 5. YamlTestFrameworkConsoleHost - Default Output Only

**File**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs`

The console host handles test output but has:
- NO configuration for context expansion
- NO properties for failure context
- NO options for extended stack traces

Test output is determined by:
1. Test framework's default behavior
2. Global `--verbose` flag (not Layer 5 specific)
3. Test result reporter format (TRX/JUnit)

### 6. Test Result Reporters - Fixed Formats

**Files**:
- `src/cycodt/TestFramework/TrxXmlTestReporter.cs`
- `src/cycodt/TestFramework/JunitXmlTestReporter.cs`

These reporters generate fixed-format test result files (TRX or JUnit XML) with:
- Standard test result information
- Stack traces (if present)
- Test timing

But they have:
- **NO options** for expanding context
- **NO parameters** for controlling output detail level
- **NO configuration** for showing additional context around failures

---

## Comparison with Other Tools

### cycodmd FindFilesCommand (HAS Layer 5)

**Properties**:
```csharp
public int IncludeLineCountBefore { get; set; }  // --lines-before N
public int IncludeLineCountAfter { get; set; }   // --lines-after N
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
else if (arg == "--lines-before")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.IncludeLineCountBefore = ValidateLineCount(arg, countStr);
}
else if (arg == "--lines-after")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.IncludeLineCountAfter = ValidateLineCount(arg, countStr);
}
```

### cycodj SearchCommand (HAS Layer 5)

**Property**:
```csharp
public int? ContextLines { get; set; }  // --context N
```

**Parser**:
```csharp
else if (arg == "--context" || arg == "-C")
{
    var lines = i + 1 < args.Length ? args[++i] : null;
    if (string.IsNullOrWhiteSpace(lines) || !int.TryParse(lines, out var n))
    {
        throw new CommandLineException($"Missing or invalid context lines for {arg}");
    }
    command.ContextLines = n;
}
```

**Usage in SearchCommand.cs**:
```csharp
// Shows context messages around matched message
var contextBefore = ContextLines ?? 0;
var contextAfter = ContextLines ?? 0;
// ... expands to show messages before/after match
```

### cycodt run (DOES NOT HAVE Layer 5)

**NO equivalent properties, parsing, or usage.**

---

## Test Execution Flow Analysis

### Current Flow (No Context Expansion)

```
1. FindAndFilterTests()
   ↓
2. YamlTestFramework.RunTests(tests, consoleHost)
   ↓ (runs each test)
   ↓ (outputs to console via consoleHost)
   ↓ (records pass/fail in results)
   ↓
3. consoleHost.Finish(results, format, file)
   ↓ (generates TRX or JUnit report)
   ↓ (returns pass/fail status)
   ↓
4. Return exit code
```

**Where Context Expansion COULD Be Added**:

1. **In YamlTestFramework.RunTests()**: Accept context parameters
   ```csharp
   RunTests(tests, consoleHost, contextConfig: new ContextConfig {
       FailureContextLines = 20,
       ShowFullStackTrace = true,
       OutputBufferLines = 100
   })
   ```

2. **In ConsoleHost**: Configure output verbosity
   ```csharp
   var consoleHost = new YamlTestFrameworkConsoleHost(new HostConfig {
       FailureContextLines = 20,
       TestChainContext = 3
   });
   ```

3. **In Reporters**: Add context to reports
   ```csharp
   var reporter = new TrxXmlTestReporter(new ReporterConfig {
       IncludeFullStackTraces = true,
       IncludeTestChainContext = true
   });
   ```

**But NONE of these exist currently.**

---

## Global Verbosity (Not Layer 5)

The `--verbose` global option affects output, but it's **Layer 6 (Display Control)**, not Layer 5:

**File**: `src/common/CommandLine/CommandLineOptions.cs`

```csharp
else if (arg == "--verbose")
{
    this.Verbose = true;
}
```

This is used by:
```csharp
ConsoleHelpers.IsVerbose()  // Returns true if --verbose was set
```

But this is a **binary flag** (on/off), not a **context expansion parameter** (how much context to show).

Layer 5 would involve **quantitative expansion** (e.g., "show 20 lines of context") not just **qualitative verbosity** (e.g., "show more output").

---

## Conclusion

The source code analysis definitively shows that **cycodt run command does NOT implement Layer 5 (Context Expansion)**:

1. ✅ **TestRunCommand.cs** - No context expansion properties or logic
2. ✅ **TestBaseCommand.cs** - No context expansion properties defined
3. ✅ **CycoDtCommandLineOptions.cs** - No context expansion options parsed
4. ✅ **YamlTestFramework.RunTests()** - No context expansion parameters
5. ✅ **Console Host** - No context expansion configuration
6. ✅ **Test Reporters** - Fixed format output, no context expansion
7. ✅ **Comparison** - Other tools (cycodmd, cycodj) have explicit Layer 5; cycodt does not

**Verdict**: Layer 5 is **NOT IMPLEMENTED** for the `run` command.

The `run` command executes tests with default output behavior and has NO options to expand context around failures, show additional test output, or control the amount of information displayed around execution points.

---

**Related Files**:
- [Layer 5 Catalog](cycodt-run-filtering-pipeline-catalog-layer-5.md)
- [Layer 1 Proof](cycodt-run-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 3 Proof](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 9 Proof (Test Execution)]( - to be created)
