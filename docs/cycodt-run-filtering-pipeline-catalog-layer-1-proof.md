# cycodt `run` Command - Layer 1: TARGET SELECTION - Proof

## Source Code Evidence

This document provides detailed evidence for Layer 1 (TARGET SELECTION) features of the `cycodt run` command.

---

## Inheritance Proof

### File: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 1-2:**
```csharp
class TestRunCommand : TestBaseCommand
{
```

**Evidence:**
- `TestRunCommand` inherits from `TestBaseCommand`
- All Layer 1 logic is in `TestBaseCommand`, not `TestRunCommand`

**Implication:** Layer 1 behavior is **100% identical** to `list` command.

---

## Command Entry Point

### File: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 26-49:**
```csharp
private int ExecuteTestRun()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());

        var tests = FindAndFilterTests();  // ← Inherited from TestBaseCommand
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

**Evidence:**
- **Line 32**: Calls `FindAndFilterTests()` - **inherited** from `TestBaseCommand`
- **Line 38**: Executes tests (difference from `list`)
- **Line 40-41**: Generates test report (difference from `list`)

**Layer 1 Proof:**
- **Line 32** demonstrates that `run` uses the **exact same** `FindAndFilterTests()` method as `list`
- This method is defined in `TestBaseCommand` (not `TestRunCommand`)

---

## Shared Implementation Evidence

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 47-61:**
```csharp
protected IList<TestCase> FindAndFilterTests()
{
    var files = FindTestFiles();  // ← Layer 1: TARGET SELECTION
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

**Evidence:**
- **Line 49**: `FindTestFiles()` is Layer 1 implementation
- This method is **protected** (accessible to derived classes)
- Both `TestListCommand` and `TestRunCommand` call this method

**Call Stack Comparison:**

**For `list` command:**
```
TestListCommand.ExecuteAsync()
    ↓
TestListCommand.ExecuteList()
    ↓
TestBaseCommand.FindAndFilterTests()  // ← Line 18 in TestListCommand.cs
    ↓
TestBaseCommand.FindTestFiles()       // ← Layer 1
```

**For `run` command:**
```
TestRunCommand.ExecuteAsync()
    ↓
TestRunCommand.ExecuteTestRun()
    ↓
TestBaseCommand.FindAndFilterTests()  // ← Line 32 in TestRunCommand.cs
    ↓
TestBaseCommand.FindTestFiles()       // ← Layer 1 (IDENTICAL)
```

---

## Complete Layer 1 Implementation (Shared)

All Layer 1 implementation details are **identical** to the `list` command. See:
- [cycodt `list` Command - Layer 1 Proof](cycodt-list-filtering-pipeline-catalog-layer-1-proof.md)

### Key Shared Components

1. **Command Line Parsing**
   - File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
   - Lines 94-187: `TryParseTestCommandOptions()`
   - Applies to both `list` and `run`

2. **Validation (`.cycodtignore` loading)**
   - File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
   - Lines 34-45: `Validate()`
   - Called for both commands

3. **Test File Discovery**
   - File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
   - Lines 63-78: `FindTestFiles()`
   - Shared implementation

4. **Test Directory Config**
   - File: `src/cycodt/TestFramework/YamlTestConfigHelpers.cs`
   - Lines 26-64: `GetTestDirectory()`
   - Used by both commands

---

## Data Structures (Shared)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 8-27:**
```csharp
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

public List<string> Globs;
public List<string> ExcludeGlobs;
public List<Regex> ExcludeFileNamePatternList;
```

**Evidence:**
- `TestRunCommand` does **not** override these properties
- It uses the **inherited** properties from `TestBaseCommand`
- Command line parser populates these properties identically for both `list` and `run`

---

## Run-Specific Properties (Not Layer 1)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 3-9:**
```csharp
public TestRunCommand() : base()
{
    OutputFormat = "trx"; // Default format
}

public string? OutputFile { get; set; }
public string OutputFormat { get; set; }
```

**Evidence:**
- **Line 8-9**: `OutputFile` and `OutputFormat` are **Layer 7** (Output Persistence)
- These properties are **not** involved in Layer 1 (TARGET SELECTION)
- They only affect what happens **after** tests are found and executed

---

## Command Line Parsing (Shared Path)

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 26-30:**
```csharp
override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
{
    return TryParseTestCommandOptions(command as TestBaseCommand, args, ref i, arg) ||
           TryParseExpectCommandOptions(command as ExpectBaseCommand, args, ref i, arg);
}
```

**Evidence:**
- **Line 28**: Casts `command` to `TestBaseCommand`
- This works for **both** `TestListCommand` and `TestRunCommand`
- Same parsing method handles both commands' Layer 1 options

**Lines 94-161:**
```csharp
private bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--file")
    {
        // ... (Lines 103-109)
        command.Globs.Add(filePattern!);
    }
    else if (arg == "--files")
    {
        // ... (Lines 110-116)
        command.Globs.AddRange(validPatterns);
    }
    else if (arg == "--exclude-files" || arg == "--exclude")
    {
        // ... (Lines 117-124)
        command.ExcludeFileNamePatternList.AddRange(asRegExs);
        command.ExcludeGlobs.AddRange(asGlobs);
    }
    // ... more options
}
```

**Evidence:**
- All Layer 1 options (`--file`, `--files`, `--exclude`) are parsed in this **shared** method
- The method populates properties on `TestBaseCommand`
- Both `list` and `run` commands inherit these populated properties

---

## Run-Specific Parsing (Not Layer 1)

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 162-180:**
```csharp
else if (command is TestRunCommand runCommand && arg == "--output-file")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
    runCommand.OutputFile = outputFile;
    i += max1Arg.Count();
}
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

**Evidence:**
- **Line 162**: Type check: `command is TestRunCommand` (specific to `run`)
- **Line 169**: Type check: `command is TestRunCommand` (specific to `run`)
- These options are **Layer 7** (Output Persistence), not Layer 1

---

## Complete Call Stack for Layer 1 (Identical to `list`)

```
CommandLineOptions.Parse(args)
    ↓
CycoDtCommandLineOptions.TryParseTestCommandOptions(command as TestBaseCommand, ...)
    ↓
    Parse --file → command.Globs.Add()
    Parse --files → command.Globs.AddRange()
    Parse --exclude → command.ExcludeGlobs / ExcludeFileNamePatternList
    ↓
TestBaseCommand.Validate()  // ← Inherited by TestRunCommand
    ↓
    FileHelpers.FindFileSearchParents(".cycodtignore")
    FileHelpers.ReadIgnoreFile()
    ExcludeGlobs.AddRange() + ExcludeFileNamePatternList.AddRange()
    ↓
TestRunCommand.ExecuteAsync()
    ↓
    TestRunCommand.ExecuteTestRun()
        ↓
        TestBaseCommand.FindAndFilterTests()  // ← Inherited method
            ↓
            TestBaseCommand.FindTestFiles()  // ← Layer 1 implementation
                ↓
                if Globs.Count == 0:
                    YamlTestConfigHelpers.GetTestDirectory()
                    Build default: {testDirectory}/**/*.yaml
                    Globs.Add(pattern)
                ↓
                FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
                ↓
                Returns List<FileInfo>
            ↓
            foreach file: GetTestsFromFile(file) → YamlTestFramework.GetTestsFromYaml()
            ↓
            Returns IEnumerable<TestCase>
```

---

## Comparison Table: `list` vs `run` Layer 1

| Feature | `list` | `run` | Implementation |
|---------|--------|-------|----------------|
| `--file` option | ✅ Yes | ✅ Yes | Inherited from `TestBaseCommand` |
| `--files` option | ✅ Yes | ✅ Yes | Inherited from `TestBaseCommand` |
| `--exclude` option | ✅ Yes | ✅ Yes | Inherited from `TestBaseCommand` |
| `.cycodtignore` loading | ✅ Yes | ✅ Yes | Inherited from `TestBaseCommand` |
| Default pattern | ✅ Yes | ✅ Yes | Inherited from `TestBaseCommand` |
| Test directory config | ✅ Yes | ✅ Yes | Uses `YamlTestConfigHelpers` (shared) |
| File discovery | ✅ Yes | ✅ Yes | Calls `FileHelpers.FindMatchingFiles()` (shared) |
| Test extraction | ✅ Yes | ✅ Yes | Calls `YamlTestFramework.GetTestsFromYaml()` (shared) |

**Conclusion:** 100% identical Layer 1 behavior.

---

## Differences Begin After Layer 1

### File: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 32-41:**
```csharp
var tests = FindAndFilterTests();  // ← End of Layer 1-4 (shared)
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"Found {tests.Count()} test...\n"
    : $"Found {tests.Count()} tests...\n");

var consoleHost = new YamlTestFrameworkConsoleHost();
var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);  // ← Layer 9: Execute tests

GetOutputFileAndFormat(out var file, out var format);
var passed = consoleHost.Finish(resultsByTestCaseId, format, file);  // ← Layer 7: Save reports
```

**Evidence:**
- **Line 32**: Uses shared method to get tests (Layers 1-4 complete)
- **Line 38**: **Difference**: Executes tests (Layer 9)
- **Line 41**: **Difference**: Generates reports (Layer 7)

**Comparison to `list`:**

**File: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`**

**Lines 18-44:**
```csharp
var tests = FindAndFilterTests();  // ← End of Layer 1-4 (shared)

if (ConsoleHelpers.IsVerbose())
{
    // Display grouped by file
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
```

**Evidence:**
- **Line 18**: Uses shared method to get tests (Layers 1-4 complete)
- **Lines 20-43**: **Difference**: Only displays test names (Layer 6)
- **No execution**, **no reports**

---

## Summary of Proof

### Proven Identical Features (Layer 1):

1. ✅ **Inheritance**: `TestRunCommand` extends `TestBaseCommand` (line 1 in TestRunCommand.cs)
2. ✅ **Shared method call**: Both call `FindAndFilterTests()` (line 32 in TestRunCommand.cs, line 18 in TestListCommand.cs)
3. ✅ **Same data structures**: Both use `Globs`, `ExcludeGlobs`, `ExcludeFileNamePatternList` from base class
4. ✅ **Same parsing logic**: `TryParseTestCommandOptions()` handles both (lines 94-161 in CycoDtCommandLineOptions.cs)
5. ✅ **Same validation**: Both call `Validate()` from base class (lines 34-45 in TestBaseCommand.cs)
6. ✅ **Same file discovery**: Both call `FindTestFiles()` from base class (lines 63-78 in TestBaseCommand.cs)

### Proven Differences (Other Layers):

1. ✅ **Layer 7**: `run` generates reports (lines 40-41 in TestRunCommand.cs), `list` does not
2. ✅ **Layer 9**: `run` executes tests (line 38 in TestRunCommand.cs), `list` only displays names

### Conclusion

The `run` command's Layer 1 implementation is **provably identical** to the `list` command through inheritance. All source code evidence confirms zero differences in TARGET SELECTION behavior.
