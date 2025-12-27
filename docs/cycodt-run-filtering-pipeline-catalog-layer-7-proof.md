# cycodt `run` - Layer 7: OUTPUT PERSISTENCE - Proof

**[← Back to Layer 7 Description](cycodt-run-filtering-pipeline-catalog-layer-7.md)**

## Source Code Evidence

This document provides detailed source code evidence for Layer 7 (Output Persistence) implementation in the `run` command.

---

## 1. Command Properties - Output File and Format

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

### Evidence: Class has output-related properties

```csharp
// Lines 1-10: TestRunCommand class definition
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

**Key Observations**:
- **Line 5**: Default format is `"trx"`
- **Line 8**: `OutputFile` property (nullable string) - stores user-specified or computed file path
- **Line 9**: `OutputFormat` property (required string) - stores format type ("trx" or "junit")

---

## 2. Command Line Parser - Output Options Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

### Evidence: `--output-file` option parsing

```csharp
// Lines 66-71: TryParseTestCommandOptions method (excerpt)
else if (command is TestRunCommand runCommand && arg == "--output-file")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
    runCommand.OutputFile = outputFile;
    i += max1Arg.Count();
}
```

**Key Observations**:
- **Line 66**: Check if command is `TestRunCommand` AND argument is `"--output-file"`
- **Line 68**: Get next argument (file path)
- **Line 69**: Validate string is not empty
- **Line 70**: Set `OutputFile` property on command
- **Line 71**: Advance argument index

---

### Evidence: `--output-format` option parsing

```csharp
// Lines 72-83: TryParseTestCommandOptions method (excerpt)
else if (command is TestRunCommand runCommand2 && arg == "--output-format")
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
```

**Key Observations**:
- **Line 72**: Check if command is `TestRunCommand` AND argument is `"--output-format"`
- **Line 74**: Get next argument (format string)
- **Line 75**: Validate string is not empty
- **Line 76**: Define allowed formats: `"trx"` and `"junit"` only
- **Lines 77-80**: Throw exception if format is invalid
- **Line 81**: Set `OutputFormat` property on command
- **Line 82**: Advance argument index

**Validation Logic**:
- Only `"trx"` and `"junit"` are accepted
- Case-sensitive matching
- Clear error message for invalid formats

---

## 3. Output File and Format Resolution

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

### Evidence: GetOutputFileAndFormat() method

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

**Key Observations**:
- **Line 53**: Use `OutputFormat` property (default "trx" if not set)
- **Lines 54-59**: Map format to file extension
  - `"trx"` → `".trx"`
  - `"junit"` → `".xml"`
  - Any other value → Exception
- **Line 61**: Use `OutputFile` if set, otherwise default to `"test-results.{ext}"`
- **Lines 62-65**: Ensure file ends with correct extension, append if missing

**Default Behavior**:
- If `OutputFile` is null → `"test-results.trx"` (with default format)
- If `OutputFile` is null + format is junit → `"test-results.xml"`
- If `OutputFile` is "myfile" + format is trx → `"myfile.trx"`
- If `OutputFile` is "myfile.trx" + format is trx → `"myfile.trx"` (no double extension)
- If `OutputFile` is "myfile.xml" + format is trx → `"myfile.xml.trx"` (extension appended)

---

## 4. Test Execution and Reporter Invocation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

### Evidence: ExecuteTestRun() method

```csharp
// Lines 26-50: ExecuteTestRun method
private int ExecuteTestRun()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());

        var tests = FindAndFilterTests();
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

**Key Observations**:
- **Line 32**: Find and filter tests (Layer 1/2)
- **Line 37**: Create console host for test execution
- **Line 38**: Run tests and collect results
- **Line 40**: Call `GetOutputFileAndFormat()` to resolve file path and format
- **Line 41**: Call `consoleHost.Finish()` with format and file parameters
- **Line 43**: Return 0 (success) if all tests passed, 1 (failure) otherwise

---

## 5. Reporter Orchestration

**File**: `src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs`

### Evidence: Finish() method signature and invocation

```csharp
// Lines 30-102: Finish method (excerpts)
public bool Finish(IDictionary<string, IList<TestResult>> resultsByTestCaseId, string outputResultsFormat = "trx", string? outputResultsFile = null)
{
    _testRun.EndRun();

    var allResults = resultsByTestCaseId.Values.SelectMany(x => x);
    var failedResults = allResults.Where(x => x.Outcome == TestOutcome.Failed).ToList();
    var passedResults = allResults.Where(x => x.Outcome == TestOutcome.Passed).ToList();
    var skippedResults = allResults.Where(x => x.Outcome == TestOutcome.Skipped).ToList();
    var passed = failedResults.Count == 0;

    // ... console output (failure summary, test summary) ...

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

**Key Observations**:
- **Line 30**: Method signature with format and file parameters (defaults: "trx", null)
- **Line 32**: End test run timing
- **Lines 34-39**: Calculate test outcome statistics
- **Lines 93-96**: If format is "trx", invoke `TrxXmlTestReporter.WriteResultsFile()`
- **Lines 97-100**: If format is "junit", invoke `JunitXmlTestReporter.WriteResultsFile()`
- Both reporters return the file path that was written
- **Line 95, 99**: Call `PrintResultsFile()` to display file path on console

---

### Evidence: PrintResultsFile() method

```csharp
// Lines 105-114: PrintResultsFile method
private static void PrintResultsFile(string resultsFile)
{
    var fi = new FileInfo(resultsFile);
    Console.ResetColor();
    Console.Write("Results: ");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write(fi.FullName);
    Console.ResetColor();
    Console.WriteLine();
}
```

**Key Observations**:
- **Line 107**: Create FileInfo to get full path
- **Lines 108-113**: Print "Results: {full-path}" to console with color
- Example output: `Results: C:\path\to\test-results.trx`

---

## 6. TRX Format Reporter

**File**: `src/cycodt/TestFramework/TrxXmlTestReporter.cs`

### Evidence: WriteResultsFile() method signature

```csharp
// Lines 4-8: TrxXmlTestReporter class and method signature
public static class TrxXmlTestReporter
{
    public static string WriteResultsFile(TestRun testRun, string? resultsFile = null)
    {
        resultsFile ??= "test-results.trx";
        
        // ... XML generation ...
```

**Key Observations**:
- **Line 6**: Static method accepting `TestRun` and optional `resultsFile`
- **Line 8**: Default file name is `"test-results.trx"` if null
- Returns: string (the file path that was written)

---

### Evidence: File writing

```csharp
// Lines 41-49: XML writer creation (excerpt)
XmlWriterSettings settings = new XmlWriterSettings();
settings.Indent = true;
settings.IndentChars = "  ";
settings.NewLineChars = "\n";
settings.NewLineHandling = NewLineHandling.Replace;
settings.OmitXmlDeclaration = false;

var writer = XmlWriter.Create(resultsFile, settings);
writer.WriteStartDocument();
```

**Key Observations**:
- **Line 48**: Create `XmlWriter` targeting `resultsFile` path
- **Line 49**: Begin writing XML document
- File is created/overwritten at this point

---

## 7. JUnit Format Reporter

**File**: `src/cycodt/TestFramework/JunitXmlTestReporter.cs`

### Evidence: WriteResultsFile() method (similar structure to TRX)

```csharp
// Similar to TrxXmlTestReporter
public static class JunitXmlTestReporter
{
    public static string WriteResultsFile(TestRun testRun, string? resultsFile = null)
    {
        resultsFile ??= "test-results.xml";
        
        // ... XML generation ...
        
        var writer = XmlWriter.Create(resultsFile, settings);
        // ... write JUnit XML structure ...
        
        return resultsFile;
    }
}
```

**Key Observations**:
- Default file name is `"test-results.xml"` (not .trx)
- Same pattern: nullable file parameter, default if null
- Returns file path

---

## 8. Data Flow Summary

### Flow Diagram

```
User command line:
  cycodt run --output-file results.xml --output-format junit

    ↓ (parsed by CycoDtCommandLineOptions.cs:72-83)

TestRunCommand properties:
  OutputFile = "results.xml"
  OutputFormat = "junit"

    ↓ (ExecuteTestRun() calls GetOutputFileAndFormat())

GetOutputFileAndFormat() resolves:
  file = "results.xml"
  format = "junit"
  
    ↓ (ExecuteTestRun() calls consoleHost.Finish(format, file))

YamlTestFrameworkConsoleHost.Finish():
  if format == "junit":
    filePath = JunitXmlTestReporter.WriteResultsFile(_testRun, "results.xml")
    
    ↓ (JunitXmlTestReporter.WriteResultsFile())
    
JunitXmlTestReporter:
  XmlWriter.Create("results.xml", settings)
  Write JUnit XML structure
  Return "results.xml"
  
    ↓ (back to Finish())
    
PrintResultsFile():
  Console: "Results: C:\full\path\to\results.xml"
```

---

## 9. Test Case: Default Behavior

**Command**: `cycodt run` (no options)

**Code Path**:
1. **Line 5** (TestRunCommand.cs): `OutputFormat = "trx"` (default)
2. **Line 8** (TestRunCommand.cs): `OutputFile = null`
3. **Line 53** (TestRunCommand.cs): `format = "trx"`
4. **Line 56** (TestRunCommand.cs): `ext = "trx"`
5. **Line 61** (TestRunCommand.cs): `file = "test-results.trx"` (OutputFile is null)
6. **Line 93** (YamlTestFrameworkConsoleHost.cs): `outputResultsFormat == "trx"` → true
7. **Line 95** (YamlTestFrameworkConsoleHost.cs): Call `TrxXmlTestReporter.WriteResultsFile(_testRun, "test-results.trx")`
8. **Line 8** (TrxXmlTestReporter.cs): `resultsFile = "test-results.trx"` (not null, use as-is)
9. **File Written**: `test-results.trx` (TRX format)

---

## 10. Test Case: JUnit Format with Custom File

**Command**: `cycodt run --output-format junit --output-file my-results.xml`

**Code Path**:
1. **Line 81** (CycoDtCommandLineOptions.cs): `OutputFormat = "junit"`
2. **Line 70** (CycoDtCommandLineOptions.cs): `OutputFile = "my-results.xml"`
3. **Line 53** (TestRunCommand.cs): `format = "junit"`
4. **Line 57** (TestRunCommand.cs): `ext = "xml"`
5. **Line 61** (TestRunCommand.cs): `file = "my-results.xml"` (OutputFile is set)
6. **Line 62** (TestRunCommand.cs): `file.EndsWith(".xml")` → true, no change
7. **Line 97** (YamlTestFrameworkConsoleHost.cs): `outputResultsFormat == "junit"` → true
8. **Line 99** (YamlTestFrameworkConsoleHost.cs): Call `JunitXmlTestReporter.WriteResultsFile(_testRun, "my-results.xml")`
9. **File Written**: `my-results.xml` (JUnit format)

---

## 11. Test Case: Extension Appending

**Command**: `cycodt run --output-file results` (no extension, default format)

**Code Path**:
1. **Line 5** (TestRunCommand.cs): `OutputFormat = "trx"` (default)
2. **Line 70** (CycoDtCommandLineOptions.cs): `OutputFile = "results"`
3. **Line 53** (TestRunCommand.cs): `format = "trx"`
4. **Line 56** (TestRunCommand.cs): `ext = "trx"`
5. **Line 61** (TestRunCommand.cs): `file = "results"` (OutputFile is set)
6. **Line 62** (TestRunCommand.cs): `file.EndsWith(".trx")` → false
7. **Line 64** (TestRunCommand.cs): `file = "results.trx"` (extension appended)
8. **File Written**: `results.trx` (TRX format)

---

## Summary of Evidence

| Aspect | Evidence Location | Finding |
|--------|------------------|---------|
| Command properties | `TestRunCommand.cs:8-9` | `OutputFile` and `OutputFormat` properties |
| Property defaults | `TestRunCommand.cs:5` | Default format is "trx" |
| `--output-file` parsing | `CycoDtCommandLineOptions.cs:66-71` | Sets `OutputFile` property |
| `--output-format` parsing | `CycoDtCommandLineOptions.cs:72-83` | Sets `OutputFormat`, validates "trx" or "junit" |
| File resolution | `TestRunCommand.cs:52-67` | Maps format to extension, ensures correct extension |
| Reporter invocation | `TestRunCommand.cs:40-41` | Passes file and format to `Finish()` |
| Format dispatch | `YamlTestFrameworkConsoleHost.cs:93-100` | Conditional invocation of TRX or JUnit reporter |
| TRX file writing | `TrxXmlTestReporter.cs:6-8,48` | Writes TRX XML, default "test-results.trx" |
| JUnit file writing | `JunitXmlTestReporter.cs` | Writes JUnit XML, default "test-results.xml" |
| Console feedback | `YamlTestFrameworkConsoleHost.cs:105-114` | Displays "Results: {path}" |

**Conclusion**: The `run` command has **full native file output capabilities** with format control, intelligent extension handling, and structured XML output (TRX and JUnit).

---

**[← Back to Layer 7 Description](cycodt-run-filtering-pipeline-catalog-layer-7.md)** | **[↑ Back to run Command Catalog](cycodt-run-catalog-README.md)**
