# cycodt `list` - Layer 7: OUTPUT PERSISTENCE - Proof

**[← Back to Layer 7 Description](cycodt-list-filtering-pipeline-catalog-layer-7.md)**

## Source Code Evidence

This document provides detailed source code evidence for Layer 7 (Output Persistence) implementation in the `list` command.

---

## 1. Command Line Parser - No Output Options

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

### Evidence: No `--save-output` or `--output-file` parsing for list command

```csharp
// Lines 44-87: TryParseTestCommandOptions
private bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--file")
    {
        // ... file pattern parsing ...
        i += max1Arg.Count();
    }
    else if (arg == "--files")
    {
        // ... files parsing ...
        i += filePatterns.Count();
    }
    else if (arg == "--exclude-files" || arg == "--exclude")
    {
        // ... exclude patterns ...
        i += patterns.Count();
    }
    else if (arg == "--test")
    {
        // ... test name parsing ...
        i += max1Arg.Count();
    }
    else if (arg == "--tests")
    {
        // ... test names parsing ...
        i += testNames.Count();
    }
    else if (arg == "--contains")
    {
        // ... contains patterns ...
        i += containPatterns.Count();
    }
    else if (arg == "--remove")
    {
        // ... remove patterns ...
        i += removePatterns.Count();
    }
    else if (arg == "--include-optional")
    {
        // ... optional categories ...
        i += optionalCategories.Count();
    }
    else if (command is TestRunCommand runCommand && arg == "--output-file")
    {
        // NOTE: --output-file is ONLY parsed for TestRunCommand, NOT TestListCommand
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
        runCommand.OutputFile = outputFile;
        i += max1Arg.Count();
    }
    else if (command is TestRunCommand runCommand2 && arg == "--output-format")
    {
        // NOTE: --output-format is ONLY parsed for TestRunCommand, NOT TestListCommand
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

**Key Observations**:
- Lines 66-71: `--output-file` check specifically for `TestRunCommand`, NOT `TestListCommand`
- Lines 72-83: `--output-format` check specifically for `TestRunCommand`, NOT `TestListCommand`
- **No output file options are parsed for TestListCommand**

---

## 2. TestListCommand - No Output File Properties

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

### Evidence: Class has no output-related properties

```csharp
// Lines 1-59: Complete TestListCommand class
class TestListCommand : TestBaseCommand
{
    public override string GetCommandName()
    {
        return "list";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteList());
    }

    private int ExecuteList()
    {
        try
        {
            TestLogger.Log(new CycoDtTestFrameworkLogger());
            var tests = FindAndFilterTests();
            
            if (ConsoleHelpers.IsVerbose())
            {
                var grouped = tests
                    .GroupBy(t => t.CodeFilePath)
                    .OrderBy(g => g.Key)
                    .ToList();
                for (var i = 0; i < grouped.Count; i++)
                {
                    if (i > 0) ConsoleHelpers.WriteLine();

                    var group = grouped[i];
                    ConsoleHelpers.WriteLine($"{group.Key}\n", ConsoleColor.DarkGray);
                    foreach (var test in group)
                    {
                        ConsoleHelpers.WriteLine($"  {test.FullyQualifiedName}", ConsoleColor.DarkGray);
                    }
                }
            }
            else
            {
                foreach (var test in tests)
                {
                    ConsoleHelpers.WriteLine(test.FullyQualifiedName, ConsoleColor.DarkGray);
                }
            }

            ConsoleHelpers.WriteLine(tests.Count() == 1
                ? $"\nFound {tests.Count()} test..."
                : $"\nFound {tests.Count()} tests...");

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

**Key Observations**:
- **No properties** for output file or format (compare with TestRunCommand which has `OutputFile` and `OutputFormat`)
- Line 20-27: Verbose mode writes to console via `ConsoleHelpers.WriteLine()`
- Line 40-42: Non-verbose mode writes to console via `ConsoleHelpers.WriteLine()`
- Line 46-48: Summary writes to console
- **All output goes to console only**

---

## 3. Comparison with TestRunCommand (Has Output Persistence)

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

### Evidence: TestRunCommand HAS output properties

```csharp
// Lines 1-10: TestRunCommand class with output properties
class TestRunCommand : TestBaseCommand
{
    public TestRunCommand() : base()
    {
        OutputFormat = "trx"; // Default format
    }

    public string? OutputFile { get; set; }
    public string OutputFormat { get; set; }
    
    // ... rest of class ...
}
```

**Contrast**:
- Line 8: `OutputFile` property exists in TestRunCommand
- Line 9: `OutputFormat` property exists in TestRunCommand
- **TestListCommand has neither property**

---

### Evidence: TestRunCommand writes to file

```csharp
// Lines 51-67: GetOutputFileAndFormat method
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

**Contrast**:
- Lines 52-67: TestRunCommand has logic for determining output file and format
- Line 62: Default output file: `test-results.{ext}`
- **TestListCommand has no equivalent logic**

---

### Evidence: TestRunCommand calls file reporter

```csharp
// Lines 40-41: ExecuteTestRun method
GetOutputFileAndFormat(out var file, out var format);
var passed = consoleHost.Finish(resultsByTestCaseId, format, file);
```

**File**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs`

```csharp
// Lines 30, 93-100: Finish method
public bool Finish(IDictionary<string, IList<TestResult>> resultsByTestCaseId, string outputResultsFormat = "trx", string? outputResultsFile = null)
{
    _testRun.EndRun();

    // ... console output ...

    if (outputResultsFormat == "trx")
    {
        PrintResultsFile(TrxXmlTestReporter.WriteResultsFile(_testRun, outputResultsFile));
    }
    else if (outputResultsFormat == "junit")
    {
        PrintResultsFile(JunitXmlTestReporter.WriteResultsFile(_testRun, outputResultsFile));
    }

    return passed;
}
```

**Contrast**:
- Lines 93-100: TestRunCommand triggers file writing via reporters
- **TestListCommand never calls any file reporting logic**

---

## 4. TestBaseCommand - No Output Methods

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

### Evidence: Base class has no output file methods

```csharp
// Lines 6-256: Complete TestBaseCommand class (excerpts)
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
        // NO OUTPUT-RELATED PROPERTIES
    }

    public List<string> Globs;
    public List<string> ExcludeGlobs;
    public List<Regex> ExcludeFileNamePatternList;
    public List<string> Tests { get; set; }
    public List<string> Contains { get; set; }
    public List<string> Remove { get; set; }
    public List<string> IncludeOptionalCategories { get; set; }
    // NO OUTPUT FILE PROPERTIES

    // ... methods for finding/filtering tests ...
    
    // NO METHODS FOR WRITING OUTPUT FILES
}
```

**Key Observations**:
- Lines 8-16: Constructor initializes filtering properties only
- Lines 19-27: All properties are for filtering, none for output
- **No output file writing methods in base class**

---

## 5. Console Output Mechanism

**Evidence**: All output uses `ConsoleHelpers.WriteLine()`

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

```csharp
// Line 31: File path output in verbose mode
ConsoleHelpers.WriteLine($"{group.Key}\n", ConsoleColor.DarkGray);

// Line 34: Test name output in verbose mode
ConsoleHelpers.WriteLine($"  {test.FullyQualifiedName}", ConsoleColor.DarkGray);

// Line 42: Test name output in non-verbose mode
ConsoleHelpers.WriteLine(test.FullyQualifiedName, ConsoleColor.DarkGray);

// Lines 46-48: Summary output
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"\nFound {tests.Count()} test..."
    : $"\nFound {tests.Count()} tests...");
```

**Implications**:
- All output goes to stdout
- Can be redirected with shell operators (`>`, `>>`, `|`)
- No programmatic file writing
- No format control (always text)

---

## Summary of Evidence

| Aspect | Evidence Location | Finding |
|--------|------------------|---------|
| Command-line options | `CycoDtCommandLineOptions.cs:66-83` | No `--output-file` or `--output-format` for list |
| Command properties | `TestListCommand.cs:1-59` | No output-related properties |
| Output logic | `TestListCommand.cs:31,34,42,46-48` | All `ConsoleHelpers.WriteLine()` calls |
| Base class support | `TestBaseCommand.cs:6-256` | No output methods in base |
| Contrast with run | `TestRunCommand.cs:8-9,52-67` | TestRunCommand HAS output support |
| File reporter usage | `TestListCommand.cs` (entire file) | Never calls any file reporter |

**Conclusion**: The `list` command has **no native file output capabilities**. All output is console-only and must be redirected via shell if file persistence is needed.

---

**[← Back to Layer 7 Description](cycodt-list-filtering-pipeline-catalog-layer-7.md)** | **[↑ Back to list Command Catalog](cycodt-list-catalog-README.md)**
