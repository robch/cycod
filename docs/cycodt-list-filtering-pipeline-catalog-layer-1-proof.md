# cycodt `list` Command - Layer 1: TARGET SELECTION - Proof

## Source Code Evidence

This document provides detailed line-by-line evidence for all Layer 1 (TARGET SELECTION) features of the `cycodt list` command.

---

## 1. Command Line Parsing

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

#### `--file` Option Parsing

**Lines 103-109:**
```csharp
if (arg == "--file")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var filePattern = ValidateString(arg, max1Arg.FirstOrDefault(), "file pattern");
    command.Globs.Add(filePattern!);
    i += max1Arg.Count();
}
```

**Evidence:**
- **Line 103**: Checks if argument is `--file`
- **Line 105**: Gets at most 1 argument after `--file`
- **Line 106**: Validates it's a non-empty string (throws if empty)
- **Line 107**: Adds validated pattern to `command.Globs` list
- **Line 108**: Advances argument index by number of consumed arguments

**Implication:** Single glob pattern added to target selection list.

---

#### `--files` Option Parsing

**Lines 110-116:**
```csharp
else if (arg == "--files")
{
    var filePatterns = GetInputOptionArgs(i + 1, args);
    var validPatterns = ValidateStrings(arg, filePatterns, "file patterns");
    command.Globs.AddRange(validPatterns);
    i += filePatterns.Count();
}
```

**Evidence:**
- **Line 110**: Checks if argument is `--files`
- **Line 112**: Gets all arguments after `--files` (until next `--` option)
- **Line 113**: Validates all are non-empty strings
- **Line 114**: Adds all validated patterns to `command.Globs` list
- **Line 115**: Advances argument index by number of consumed arguments

**Implication:** Multiple glob patterns added to target selection list.

---

#### `--exclude` / `--exclude-files` Option Parsing

**Lines 117-124:**
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

**Evidence:**
- **Line 117**: Checks if argument is `--exclude-files` OR `--exclude` (aliases)
- **Line 119**: Gets all arguments after option
- **Line 120**: Validates and categorizes patterns:
  - Patterns **without** `/` or `\` → converted to Regex (filename-only matching)
  - Patterns **with** `/` or `\` → kept as glob patterns (path matching)
- **Line 121**: Adds regex patterns to `ExcludeFileNamePatternList`
- **Line 122**: Adds glob patterns to `ExcludeGlobs`
- **Line 123**: Advances argument index

**Implication:** Exclusion patterns stored in two separate lists based on whether they're path-based or filename-based.

---

### Pattern Categorization Logic

**File: `src/common/CommandLine/CommandLineOptions.cs`**

**Lines referenced in ValidateExcludeRegExAndGlobPatterns (approximate line 400-420):**
```csharp
protected void ValidateExcludeRegExAndGlobPatterns(string arg, IEnumerable<string> patterns, out List<Regex> asRegExs, out List<string> asGlobs)
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

**Evidence:**
- **Line with `containsSlash`**: Lambda function checks for path separators
- **`asRegExs` creation**: Patterns **without** slashes → converted to regex
- **`asGlobs` creation**: Patterns **with** slashes → kept as-is

**Implication:** 
- `*.backup` → Regex: `^.*\.backup$` (matches filename only)
- `**/temp/**` → Glob: `**/temp/**` (matches full path)

---

## 2. Validation Phase - .cycodtignore Loading

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 34-45:**
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

**Evidence:**
- **Line 34**: Entry point for validation phase
- **Line 36**: Searches current directory and parents for `.cycodtignore` file
- **Line 37**: Check if ignore file was found
- **Line 39**: Read ignore file, parsing patterns into glob and regex lists
- **Line 40-41**: Append parsed patterns to existing exclusion lists

**Call Stack:**
```
TestBaseCommand.Validate()
    → FileHelpers.FindFileSearchParents(".cycodtignore")
        → Searches up directory tree
    → FileHelpers.ReadIgnoreFile(file, out globs, out regexes)
        → Parses file lines
        → Categorizes by slash presence
```

**Implication:** Ignore file patterns are **added** to command-line exclusions, not replaced.

---

## 3. Test File Discovery

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 63-78:**
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

**Evidence:**

#### Default Pattern Application (Lines 65-69)
- **Line 65**: Check if no glob patterns specified
- **Line 67**: Get test directory from configuration
- **Line 68**: Build pattern: `{testDirectory}/**/*.yaml`
- **Line 69**: Add default pattern to `Globs` list

**Implication:** Zero-config operation - if no `--file` specified, searches test directory recursively.

#### File Matching (Lines 71-74)
- **Line 71-72**: Call `FileHelpers.FindMatchingFiles()` with:
  - `Globs`: Inclusion patterns (from `--file`, `--files`, or default)
  - `ExcludeGlobs`: Path-based exclusions (from `--exclude` and `.cycodtignore`)
  - `ExcludeFileNamePatternList`: Filename-based exclusions (regex)
- **Line 73**: Convert file paths to `FileInfo` objects
- **Line 74**: Materialize to list

**Call Stack:**
```
TestBaseCommand.FindTestFiles()
    → YamlTestConfigHelpers.GetTestDirectory()
        → Finds .cycod.yaml or .cycod-defaults.yaml
        → Reads testDirectory tag
        → Returns DirectoryInfo
    → PathHelpers.Combine(dir, "**", "*.yaml")
    → FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
        → Matches glob patterns
        → Applies exclusion filters
        → Returns matching file paths
```

---

## 4. Test Directory Configuration

### File: `src/cycodt/TestFramework/YamlTestConfigHelpers.cs`

**Lines 26-64:**
```csharp
public static DirectoryInfo GetTestDirectory(DirectoryInfo? checkHereAndParents = null)
{
    checkHereAndParents ??= new DirectoryInfo(Directory.GetCurrentDirectory());

    var file = FindTestConfigFile(checkHereAndParents);
    if (file != null)
    {
        var tagFilePath = Path.Combine(file.Directory!.FullName, ".cycod.yaml");
        if (File.Exists(tagFilePath))
        {
            var tags = FileHelpers.ReadYamlTags(tagFilePath);
            if (tags.ContainsKey("testDirectory"))
            {
                var testDirectory = tags["testDirectory"].FirstOrDefault();
                if (testDirectory != null)
                {
                    testDirectory = PathHelpers.Combine(file.Directory!.FullName, testDirectory);
                    TestLogger.Log($"YamlTestConfigHelpers.GetTestDirectory: Found test directory in config file at {testDirectory}");
                    return new DirectoryInfo(testDirectory!);
                }
            }
        }

        var defaultTagsFilePath = Path.Combine(file.Directory!.FullName, ".cycod-defaults.yaml");
        if (File.Exists(defaultTagsFilePath))
        {
            var tags = FileHelpers.ReadYamlTags(defaultTagsFilePath);
            if (tags.ContainsKey("testDirectory"))
            {
                var testDirectory = tags["testDirectory"].FirstOrDefault();
                if (testDirectory != null)
                {
                    testDirectory = PathHelpers.Combine(file.Directory!.FullName, testDirectory);
                    TestLogger.Log($"YamlTestConfigHelpers.GetTestDirectory: Found test directory in default tags file at {testDirectory}");
                    return new DirectoryInfo(testDirectory!);
                }
            }
        }
    }

    TestLogger.Log($"YamlTestConfigHelpers.GetTestDirectory: No test directory found; using {checkHereAndParents.FullName}");
    return checkHereAndParents;
}
```

**Evidence:**

#### Search Order (Lines 30-61)
1. **Line 30**: Search for config file (recursive parent search)
2. **Lines 33-41**: Check `.cycod.yaml` for `testDirectory` tag
3. **Lines 43-59**: If not found, check `.cycod-defaults.yaml`
4. **Line 62**: If still not found, return current directory

#### Path Resolution (Lines 40, 56)
- **Line 40 / 56**: `PathHelpers.Combine(file.Directory!.FullName, testDirectory)`
- Resolves `testDirectory` value **relative to config file location**

**Example:**
```
Config file: /project/.cycod.yaml
testDirectory: tests/cycodt-yaml
Result: /project/tests/cycodt-yaml
```

---

## 5. Test Extraction from Files

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 244-255:**
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

**Evidence:**
- **Line 248**: Call `YamlTestFramework.GetTestsFromYaml()` with file
- **Line 249-253**: Error handling - returns empty list on failure
- **Purpose**: Extract `TestCase` objects from YAML test file

**Implication:** Each file found in Layer 1 is parsed to extract test cases for further filtering in Layers 2-4.

---

## 6. Data Structures

### TestBaseCommand Class Properties

**File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`**

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

public List<string> Tests { get; set; }
public List<string> Contains { get; set; }
public List<string> Remove { get; set; }

public List<string> IncludeOptionalCategories { get; set; }
```

**Layer 1 Relevant Properties:**
- **Line 19**: `Globs` - Inclusion glob patterns (from `--file`, `--files`, or default)
- **Line 20**: `ExcludeGlobs` - Path-based exclusion patterns (from `--exclude` and `.cycodtignore`)
- **Line 21**: `ExcludeFileNamePatternList` - Filename-based exclusion regex patterns

---

## 7. Complete Call Stack for Layer 1

```
CommandLineOptions.Parse(args)
    ↓
CycoDtCommandLineOptions.TryParseTestCommandOptions()
    ↓
    ├─ Parse --file → Globs.Add(pattern)
    ├─ Parse --files → Globs.AddRange(patterns)
    └─ Parse --exclude → ExcludeGlobs.AddRange() + ExcludeFileNamePatternList.AddRange()
    ↓
TestBaseCommand.Validate()
    ↓
    FileHelpers.FindFileSearchParents(".cycodtignore")
        ↓
        FileHelpers.ReadIgnoreFile()
            ↓
            ExcludeGlobs.AddRange() + ExcludeFileNamePatternList.AddRange()
    ↓
TestListCommand.ExecuteAsync()
    ↓
    TestBaseCommand.FindAndFilterTests()
        ↓
        TestBaseCommand.FindTestFiles()
            ↓
            if Globs.Count == 0:
                ↓
                YamlTestConfigHelpers.GetTestDirectory()
                    ↓
                    FindTestConfigFile() → .cycod.yaml / .cycod-defaults.yaml
                    ↓
                    Read testDirectory tag
                    ↓
                    Return DirectoryInfo
                ↓
                Build default pattern: {testDirectory}/**/*.yaml
                ↓
                Globs.Add(pattern)
            ↓
            FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
                ↓
                Returns List<string> of matching file paths
            ↓
            Convert to List<FileInfo>
        ↓
        foreach file in files:
            ↓
            TestBaseCommand.GetTestsFromFile(file)
                ↓
                YamlTestFramework.GetTestsFromYaml(file)
                    ↓
                    Parse YAML
                    ↓
                    Return IEnumerable<TestCase>
```

---

## 8. Integration Points with Other Layers

### Layer 1 Output → Layer 2 Input
**File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`**

**Lines 47-61:**
```csharp
protected IList<TestCase> FindAndFilterTests()
{
    var files = FindTestFiles();  // ← Layer 1 output
    var filters = GetTestFilters();

    var atLeastOneFileSpecified = files.Any();
    var tests = atLeastOneFileSpecified
        ? files.SelectMany(file => GetTestsFromFile(file))  // ← Layer 2 begins
        : Array.Empty<TestCase>();

    var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
    var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();

    return filtered;
}
```

**Evidence:**
- **Line 49**: Layer 1 completes with `files` (List<FileInfo>)
- **Line 54**: Layer 2 begins with `files.SelectMany(file => GetTestsFromFile(file))`

---

## Summary of Evidence

### Proven Features:

1. ✅ **`--file` option**: Lines 103-109 in CycoDtCommandLineOptions.cs
2. ✅ **`--files` option**: Lines 110-116 in CycoDtCommandLineOptions.cs
3. ✅ **`--exclude` option**: Lines 117-124 in CycoDtCommandLineOptions.cs
4. ✅ **Pattern categorization**: Slash detection logic in CommandLineOptions.cs
5. ✅ **`.cycodtignore` loading**: Lines 34-45 in TestBaseCommand.cs
6. ✅ **Default pattern application**: Lines 65-69 in TestBaseCommand.cs
7. ✅ **Test directory config**: Lines 26-64 in YamlTestConfigHelpers.cs
8. ✅ **File discovery**: Lines 71-74 in TestBaseCommand.cs
9. ✅ **Test extraction**: Lines 244-255 in TestBaseCommand.cs

### Data Flow Confirmed:

```
CLI Args → Parse → Globs / ExcludeGlobs / ExcludeFileNamePatternList
    ↓
Validate → Load .cycodtignore → Append to exclusion lists
    ↓
FindTestFiles → Apply defaults → FileHelpers.FindMatchingFiles()
    ↓
files List<FileInfo> → Extract tests → IEnumerable<TestCase>
```

All Layer 1 features are implemented and evidenced in the source code.
