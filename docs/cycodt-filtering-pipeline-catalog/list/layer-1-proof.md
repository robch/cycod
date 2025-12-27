# cycodt list - Layer 1: TARGET SELECTION - Proof

This document provides detailed source code evidence for Layer 1 (Target Selection) implementation.

## Command-Line Parsing Evidence

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

#### `--file` option parsing

**Lines 103-108**:
```csharp
if (arg == "--file")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var filePattern = ValidateString(arg, max1Arg.FirstOrDefault(), "file pattern");
    command.Globs.Add(filePattern!);
    i += max1Arg.Count();
}
```

**Evidence**:
- Calls `GetInputOptionArgs(i + 1, args, max: 1)` to get exactly 1 argument
- Validates the pattern using `ValidateString()` (throws exception if null/empty)
- Adds to `command.Globs` list (type: `List<string>`)
- Increments `i` to skip consumed arguments

#### `--files` option parsing

**Lines 110-115**:
```csharp
else if (arg == "--files")
{
    var filePatterns = GetInputOptionArgs(i + 1, args);
    var validPatterns = ValidateStrings(arg, filePatterns, "file patterns");
    command.Globs.AddRange(validPatterns);
    i += filePatterns.Count();
}
```

**Evidence**:
- Calls `GetInputOptionArgs(i + 1, args)` with no max limit (gets all non-option args)
- Validates using `ValidateStrings()` (ensures at least one pattern provided)
- Uses `AddRange()` to add multiple patterns to `command.Globs`

#### `--exclude-files` / `--exclude` option parsing

**Lines 117-123**:
```csharp
else if (arg == "--exclude-files" || arg == "--exclude")
{
    var patterns = GetInputOptionArgs(i + 1, args);
    ValidateExcludeRegExAndGlobPatterns(arg, patterns, out var asRegExs, out var asGlobs);
    command.ExcludeFileNamePatternList.AddRange(asRegExs);
    command.ExcludeGlobs.AddRange(asGlobs);
    i += patterns.Count();
}
```

**Evidence**:
- Both `--exclude-files` and `--exclude` map to same behavior
- Calls `ValidateExcludeRegExAndGlobPatterns()` to split patterns:
  - Patterns containing `/` or `\` → `asGlobs` (type: `List<string>`)
  - Other patterns → converted to `Regex` objects → `asRegExs` (type: `List<Regex>`)
- Adds to TWO separate lists: `ExcludeFileNamePatternList` and `ExcludeGlobs`

**See CommandLineOptions.cs for pattern splitting logic:**

File: `src/common/CommandLine/CommandLineOptions.cs`, lines 607-624:
```csharp
protected void ValidateExcludeRegExAndGlobPatterns(string arg, IEnumerable<string> patterns, 
    out List<Regex> asRegExs, out List<string> asGlobs)
{
    if (patterns.Count() == 0)
    {
        throw new CommandLineException($"Missing patterns for {arg}");
    }

    var containsSlash = (string x) => x.Contains('/') || x.Contains('\\');
    asRegExs = patterns
        .Where(x => !containsSlash(x))
        .Select(x => ValidateFilePatternToRegExPattern(arg, x))
        .ToList();
    asGlobs = patterns
        .Where(x => containsSlash(x))
        .ToList();
}
```

**Evidence**: Pattern with path separator → treated as glob, otherwise treated as file name pattern

#### `--test` option parsing

**Lines 125-130**:
```csharp
else if (arg == "--test")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var testName = ValidateString(arg, max1Arg.FirstOrDefault(), "test name");
    command.Tests.Add(testName!);
    i += max1Arg.Count();
}
```

**Evidence**:
- Gets exactly 1 test name
- Adds to `command.Tests` (type: `List<string>`)

#### `--tests` option parsing

**Lines 132-137**:
```csharp
else if (arg == "--tests")
{
    var testNames = GetInputOptionArgs(i + 1, args);
    var validTests = ValidateStrings(arg, testNames, "test names");
    command.Tests.AddRange(validTests);
    i += testNames.Count();
}
```

**Evidence**: Same pattern as `--files`, allows multiple test names

#### `--contains` option parsing

**Lines 139-144**:
```csharp
else if (arg == "--contains")
{
    var containPatterns = GetInputOptionArgs(i + 1, args);
    var validContains = ValidateStrings(arg, containPatterns, "contains patterns");
    command.Contains.AddRange(validContains);
    i += containPatterns.Count();
}
```

**Evidence**: 
- Allows multiple patterns
- Adds to `command.Contains` (type: `List<string>`)

#### `--remove` option parsing

**Lines 146-151**:
```csharp
else if (arg == "--remove")
{
    var removePatterns = GetInputOptionArgs(i + 1, args);
    var validRemove = ValidateStrings(arg, removePatterns, "remove patterns");
    command.Remove.AddRange(validRemove);
    i += removePatterns.Count();
}
```

**Evidence**: 
- Allows multiple patterns
- Adds to `command.Remove` (type: `List<string>`)

#### `--include-optional` option parsing

**Lines 153-160**:
```csharp
else if (arg == "--include-optional")
{
    var optionalCategories = GetInputOptionArgs(i + 1, args);
    var validCategories = optionalCategories.Any()
        ? ValidateStrings(arg, optionalCategories, "optional categories")
        : new List<string> { string.Empty };
    command.IncludeOptionalCategories.AddRange(validCategories);
    i += optionalCategories.Count();
}
```

**Evidence**:
- Gets all non-option args after `--include-optional`
- If **no args** → adds empty string `""` to list (special marker meaning "include all")
- If **has args** → validates and adds specific category names
- This enables three modes:
  1. No `--include-optional` → `IncludeOptionalCategories` is empty → exclude all optional
  2. `--include-optional` (no args) → contains `[""]` → include all optional
  3. `--include-optional cat1 cat2` → contains `["cat1", "cat2"]` → include only those categories

## Test Base Command Evidence

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

#### Property declarations

**Lines 9-27**:
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

public List<string> Tests { get; set; }
public List<string> Contains { get; set; }
public List<string> Remove { get; set; }

public List<string> IncludeOptionalCategories { get; set; }
```

**Evidence**:
- All filter lists initialized as empty in constructor
- `Globs`, `ExcludeGlobs`, `ExcludeFileNamePatternList` are **public fields**
- `Tests`, `Contains`, `Remove`, `IncludeOptionalCategories` are **public properties**

#### `.cycodtignore` file loading

**Lines 34-45** (in `Validate()` method):
```csharp
override public Command Validate()
{
    var ignoreFile = FileHelpers.FindFileSearchParents(".cycodtignore");
    if (ignoreFile != null)
    {
        FileHelpers.ReadIgnoreFile(ignoreFile, out var excludeGlobs, out var excludeFileNamePatternList);
        ExcludeGlobs.AddRange(excludeGlobs);
        ExcludeFileNamePatternList.AddRange(excludeFileNamePatternList);
    }

    return this;
}
```

**Evidence**:
- Called automatically during command validation (before execution)
- `FindFileSearchParents()` searches current directory and all parent directories
- If found, patterns are parsed and **added** to existing exclusion lists (doesn't replace)
- Uses `FileHelpers.ReadIgnoreFile()` to parse the file

**See FileHelpers implementation** in `src/common/Helpers/FileHelpers.cs` for ignore file parsing logic.

#### Finding test files

**Lines 63-78** (in `FindTestFiles()` method):
```csharp
protected List<FileInfo> FindTestFiles()
{
    if (Globs.Count == 0)
    {
        var directory = YamlTestConfigHelpers.GetTestDirectory();
        var globPattern = PathHelpers.Combine(directory.FullName, "**", "*.yaml")!;
        Globs.Add(globPattern);
    }

    var files = FileHelpers
        .FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
        .Select(x => new FileInfo(x))
        .ToList();

    return files;
}
```

**Evidence**:
- **Default glob**: If `Globs.Count == 0`, adds `{TestDirectory}/**/*.yaml`
  - `YamlTestConfigHelpers.GetTestDirectory()` determines test directory
  - `PathHelpers.Combine()` builds cross-platform path
- Calls `FileHelpers.FindMatchingFiles()` with all three filter types:
  - `Globs` - inclusion patterns
  - `ExcludeGlobs` - exclusion glob patterns
  - `ExcludeFileNamePatternList` - exclusion regex patterns
- Returns `List<FileInfo>` (concrete list, not IEnumerable)

#### Finding and filtering tests (main pipeline)

**Lines 47-61** (in `FindAndFilterTests()` method):
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

**Evidence - Pipeline order**:
1. `FindTestFiles()` - gets test files (Layer 1: file-level target selection)
2. `GetTestFilters()` - converts Contains/Remove/Tests into filter format
3. `GetTestsFromFile()` - loads TestCase objects from YAML files
4. `FilterOptionalTests()` - applies optional test filtering (Layer 1: test-level target selection)
5. `YamlTestCaseFilter.FilterTestCases()` - applies name/contains/remove filters (Layers 2-4)

**Evidence - Short-circuit**: If `files.Any()` is false → returns empty array (no tests found)

#### Converting filter lists to filter format

**Lines 97-113** (in `GetTestFilters()` method):
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

**Evidence - Filter syntax**:
- `Tests` items added as-is (exact name matches)
- `Contains` items prefixed with `+` (inclusion filters)
- `Remove` items prefixed with `-` (exclusion filters)

**Example**: Given:
- `Tests = ["test 1", "test 2"]`
- `Contains = ["async"]`
- `Remove = ["skip"]`

Result: `["test 1", "test 2", "+async", "-skip"]`

#### Filtering optional tests

**Lines 115-138** (in `FilterOptionalTests()` method):
```csharp
private IEnumerable<TestCase> FilterOptionalTests(IEnumerable<TestCase> tests, 
    List<string> includeOptionalCategories)
{
    var allTests = tests.ToList();
    
    // If we're including all optional tests, just return everything
    var includeAllOptional = includeOptionalCategories.Count == 1 && 
                            string.IsNullOrEmpty(includeOptionalCategories[0]);
    if (includeAllOptional) return allTests;

    // Determine which tests will be excluded
    var excludeAllOptional = includeOptionalCategories.Count == 0;
    var excludedTests = allTests
        .Where(test => HasOptionalTrait(test) && 
                      (excludeAllOptional || !HasMatchingOptionalCategory(test, includeOptionalCategories)))
        .ToList();

    if (excludedTests.Count > 0)
    {
        // Repair the test chain by updating nextTestCaseId and afterTestCaseId properties
        RepairTestChain(allTests, excludedTests);
    }

    // Return the filtered tests (without excluded ones)
    return allTests.Except(excludedTests);
}
```

**Evidence - Three modes**:
1. **Include all optional**: `includeOptionalCategories.Count == 1 && string.IsNullOrEmpty([0])`
   - Returns all tests unchanged
2. **Exclude all optional**: `includeOptionalCategories.Count == 0`
   - Excludes any test with `HasOptionalTrait()`
3. **Include specific categories**: `includeOptionalCategories.Count > 0` with non-empty values
   - Excludes optional tests that **don't** have matching category

**Evidence - Test chain repair**: Lines 140-231 implement `RepairTestChain()` to maintain test execution order when excluding tests

#### Checking for optional trait

**Lines 233-236** (in `HasOptionalTrait()` method):
```csharp
private bool HasOptionalTrait(TestCase test)
{
    return test.Traits.Any(t => t.Name == "optional");
}
```

**Evidence**: A test is optional if it has **any** trait with name `"optional"` (trait value is the category)

#### Checking for matching optional category

**Lines 238-242** (in `HasMatchingOptionalCategory()` method):
```csharp
private bool HasMatchingOptionalCategory(TestCase test, List<string> categories)
{
    var optionalTraits = test.Traits.Where(t => t.Name == "optional").Select(t => t.Value);
    return optionalTraits.Any(value => categories.Contains(value));
}
```

**Evidence**:
- Gets all trait **values** where trait name is `"optional"`
- Returns true if **any** trait value matches a category in the list
- Example: Test has `optional: broken-test` → `optionalTraits = ["broken-test"]`

#### Loading tests from YAML file

**Lines 244-255** (in `GetTestsFromFile()` method):
```csharp
private IEnumerable<TestCase> GetTestsFromFile(FileInfo file)
{
    try
    {
        return YamlTestFramework.GetTestsFromYaml("cycodt", file);
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}");
        return new List<TestCase>();
    }
}
```

**Evidence**:
- Delegates to `YamlTestFramework.GetTestsFromYaml()`
- First arg `"cycodt"` is the test source identifier
- Returns empty list on error (doesn't throw exception)

## Test Framework Evidence

### File: `src/cycodt/TestFramework/YamlTestFramework.cs`

The actual YAML parsing and TestCase creation is delegated to this class. Key method signature:

```csharp
public static IEnumerable<TestCase> GetTestsFromYaml(string source, FileInfo yamlFile)
```

This method:
1. Reads YAML file
2. Parses test definitions
3. Creates `TestCase` objects with properties like:
   - `DisplayName` (test name)
   - `FullyQualifiedName` (unique identifier)
   - `Traits` (including optional categories)
   - Various test properties (command, expectations, etc.)

### File: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

Test name filtering is delegated to this class. Key method signature:

```csharp
public static IEnumerable<TestCase> FilterTestCases(
    IEnumerable<TestCase> tests, 
    List<string> filters)
```

**Filter format** (as created by `GetTestFilters()`):
- No prefix: Exact name match
- `+pattern`: Include tests containing pattern
- `-pattern`: Exclude tests containing pattern

## Data Flow Summary

```
Command Line
    ↓
CycoDtCommandLineOptions.TryParseTestCommandOptions()
    ↓ (populates)
TestBaseCommand properties:
    - Globs
    - ExcludeGlobs  
    - ExcludeFileNamePatternList
    - Tests
    - Contains
    - Remove
    - IncludeOptionalCategories
    ↓
TestBaseCommand.Validate()
    ↓ (loads and appends)
.cycodtignore patterns → ExcludeGlobs, ExcludeFileNamePatternList
    ↓
TestListCommand.ExecuteList()
    ↓
TestBaseCommand.FindAndFilterTests()
    ↓
    ├─→ FindTestFiles()
    │   ├─→ (if Globs empty) add default "**/*.yaml"
    │   └─→ FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
    │       └─→ List<FileInfo>
    │
    ├─→ GetTestFilters()
    │   └─→ Convert Tests, Contains, Remove → List<string> with +/- prefixes
    │
    ├─→ GetTestsFromFile() for each file
    │   └─→ YamlTestFramework.GetTestsFromYaml()
    │       └─→ IEnumerable<TestCase>
    │
    ├─→ FilterOptionalTests(tests, IncludeOptionalCategories)
    │   ├─→ Check mode (all/none/specific categories)
    │   ├─→ Identify excluded tests
    │   ├─→ RepairTestChain() if needed
    │   └─→ Return filtered tests
    │
    └─→ YamlTestCaseFilter.FilterTestCases(tests, filters)
        └─→ Apply +/- filters
            └─→ IList<TestCase> (final filtered list)
```

## Cross-References

- **FileHelpers.FindMatchingFiles()**: `src/common/Helpers/FileHelpers.cs` - implements glob and regex file matching
- **YamlTestFramework.GetTestsFromYaml()**: `src/cycodt/TestFramework/YamlTestFramework.cs` - parses YAML and creates TestCase objects
- **YamlTestCaseFilter.FilterTestCases()**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs` - applies name-based filters
- **YamlTestConfigHelpers.GetTestDirectory()**: `src/cycodt/TestFramework/YamlTestConfigHelpers.cs` - determines default test directory

## Test Case Structure

A `TestCase` object (from Microsoft.VisualStudio.TestPlatform.ObjectModel) contains:
- `Id`: Unique GUID
- `DisplayName`: Human-readable name (used for filtering)
- `FullyQualifiedName`: Unique name including file path
- `CodeFilePath`: Path to YAML file
- `Traits`: List of name-value pairs (including `optional` trait for optional tests)
- Properties: Custom key-value pairs stored via `SetPropertyValue()`

The `optional` trait is special:
- **Name**: Always `"optional"`
- **Value**: The optional category (e.g., `"broken-test"`, `"wip"`)
