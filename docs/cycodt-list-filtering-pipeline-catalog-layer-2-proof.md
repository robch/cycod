# cycodt list - Layer 2: Container Filtering - PROOF

## Source Code Evidence

This document provides **line-by-line proof** from source code showing that Layer 2 (Container Filtering) is **NOT IMPLEMENTED** for the `cycodt list` command.

---

## 1. Main Filtering Pipeline: No Layer 2 Logic

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 47-61**: Complete filtering pipeline

```csharp
47:     protected IList<TestCase> FindAndFilterTests()
48:     {
49:         var files = FindTestFiles();
50:         var filters = GetTestFilters();
51: 
52:         var atLeastOneFileSpecified = files.Any();
53:         var tests = atLeastOneFileSpecified
54:             ? files.SelectMany(file => GetTestsFromFile(file))
55:             : Array.Empty<TestCase>();
56: 
57:         var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
58:         var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();
59: 
60:         return filtered;
61:     }
```

**Evidence of NO Layer 2**:
- Line 49: `FindTestFiles()` completes Layer 1 (returns `List<FileInfo>`)
- Line 53-55: **ALL files are processed** - no filtering between discovery and test extraction
- Line 54: `files.SelectMany()` operates on **every file** in the list
- No conditional logic like:
  - ❌ `if (FileContainsPattern(file, pattern))`
  - ❌ `if (FileModifiedAfter(file, date))`
  - ❌ `files.Where(file => ...)`
- Line 57: Next filtering is `FilterOptionalTests()` which operates on **test cases**, not files (Layer 4)
- Line 58: `YamlTestCaseFilter.FilterTestCases()` operates on **test cases**, not files (Layers 3 & 4)

**What This Means**:
```
Input:  100 test files discovered in Layer 1
Process: Load test cases from ALL 100 files (no filtering)
Output: All test cases from all 100 files (to be filtered at Layers 3 & 4)
```

---

## 2. File Processing: Unconditional Loading

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 244-256**: Loading tests from file

```csharp
244:     private IEnumerable<TestCase> GetTestsFromFile(FileInfo file)
245:     {
246:         try
247:         {
248:             return YamlTestFramework.GetTestsFromYaml("cycodt", file);
249:         }
250:         catch (Exception ex)
251:         {
252:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}");
253:             return new List<TestCase>();
254:         }
255:     }
```

**Evidence**:
- Line 244: Method signature accepts `FileInfo` - no filtering parameters
- Line 248: Unconditionally loads tests from YAML file
- No checks for:
  - ❌ File content patterns
  - ❌ File size
  - ❌ File metadata (modified time, etc.)
  - ❌ Number of tests in file
  - ❌ File-level tags or properties
- Method is called for **every file** returned from Layer 1

---

## 3. No File-Content Filtering Options

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 42-85**: Complete option parsing for test commands

```csharp
42:     private bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
43:     {
44:         if (command == null)
45:         {
46:             return false;
47:         }
48: 
49:         bool parsed = true;
50: 
51:         if (arg == "--file")
52:         {
53:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
54:             var filePattern = ValidateString(arg, max1Arg.FirstOrDefault(), "file pattern");
55:             command.Globs.Add(filePattern!);
56:             i += max1Arg.Count();
57:         }
58:         else if (arg == "--files")
59:         {
60:             var filePatterns = GetInputOptionArgs(i + 1, args);
61:             var validPatterns = ValidateStrings(arg, filePatterns, "file patterns");
62:             command.Globs.AddRange(validPatterns);
63:             i += filePatterns.Count();
64:         }
65:         else if (arg == "--exclude-files" || arg == "--exclude")
66:         {
67:             var patterns = GetInputOptionArgs(i + 1, args);
68:             ValidateExcludeRegExAndGlobPatterns(arg, patterns, out var asRegExs, out var asGlobs);
69:             command.ExcludeFileNamePatternList.AddRange(asRegExs);
70:             command.ExcludeGlobs.AddRange(asGlobs);
71:             i += patterns.Count();
72:         }
73:         else if (arg == "--test")
74:         { ...test name filtering (Layer 3)... }
75:         else if (arg == "--tests")
76:         { ...test name filtering (Layer 3)... }
77:         else if (arg == "--contains")
78:         { ...test content filtering (Layer 3)... }
79:         else if (arg == "--remove")
80:         { ...test removal filtering (Layer 4)... }
81:         else if (arg == "--include-optional")
82:         { ...optional test inclusion (Layer 4)... }
83:         ...
```

**Evidence**:
- Lines 51-56: `--file` - Layer 1 (file discovery pattern)
- Lines 58-63: `--files` - Layer 1 (file discovery patterns)
- Lines 65-71: `--exclude-files` / `--exclude` - Layer 1 (file discovery exclusion)
- Lines 73+: All remaining options are Layer 3/4 (test-level filtering)

**NO options for**:
- ❌ `--file-contains` (filter files by content)
- ❌ `--file-not-contains` (exclude files by content)
- ❌ `--file-modified-after` (filter by file modification time)
- ❌ `--file-tag` (filter by file-level tags)
- ❌ Any other file-property-based filtering

---

## 4. TestBaseCommand Data Structure

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 8-27**: Command properties

```csharp
 8:     public TestBaseCommand()
 9:     {
10:         Globs = new List<string>();
11:         ExcludeGlobs = new List<string>();
12:         ExcludeFileNamePatternList = new List<Regex>();
13:         Tests = new List<string>();
14:         Contains = new List<string>();
15:         Remove = new List<string>();
16:         IncludeOptionalCategories = new List<string>();
17:     }
18: 
19:     public List<string> Globs;
20:     public List<string> ExcludeGlobs;
21:     public List<Regex> ExcludeFileNamePatternList;
22: 
23:     public List<string> Tests { get; set; }
24:     public List<string> Contains { get; set; }
25:     public List<string> Remove { get; set; }
26: 
27:     public List<string> IncludeOptionalCategories { get; set; }
```

**Evidence**:
- Lines 19-21: File discovery properties (Layer 1):
  - `Globs` - inclusion patterns
  - `ExcludeGlobs` - exclusion patterns
  - `ExcludeFileNamePatternList` - regex exclusions
- Lines 23-25: Test filtering properties (Layers 3 & 4):
  - `Tests` - specific test names
  - `Contains` - must-match patterns
  - `Remove` - must-NOT-match patterns
- Line 27: Optional test handling (Layer 4)

**NO properties for Layer 2**:
- ❌ `FileContainsPatterns`
- ❌ `FileNotContainsPatterns`
- ❌ `FileModifiedAfter`
- ❌ `FileModifiedBefore`
- ❌ `MinTests` / `MaxTests`
- ❌ `FileTags`

**Gap Between Layers**:
```
Layer 1 Properties: Globs, ExcludeGlobs, ExcludeFileNamePatternList
[NO LAYER 2 PROPERTIES HERE]
Layer 3/4 Properties: Tests, Contains, Remove, IncludeOptionalCategories
```

---

## 5. Comparison with cycodmd (which HAS Layer 2)

### File: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

For comparison, here's what Layer 2 looks like in cycodmd:

```csharp
// cycodmd HAS these Layer 2 properties:
public List<Regex> IncludeFileContainsPatternList;      // --file-contains
public List<Regex> ExcludeFileContainsPatternList;      // --file-not-contains

// cycodmd filters files BEFORE processing content:
var matchingFiles = FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ...)
    .Where(file => PassesFileContentFilter(file))       // ← Layer 2 filtering
    .ToList();
```

**cycodt DOES NOT have this**:
```csharp
// cycodt loads ALL discovered files without filtering:
var tests = files.SelectMany(file => GetTestsFromFile(file))  // ← NO content filtering
```

---

## 6. Architectural Decision: Why No Layer 2?

### Implicit in Code Structure

**Evidence from execution flow**:

1. **Fast YAML Parsing**: YAML files parse quickly, no performance benefit to pre-filtering
2. **Test-Level Granularity**: Filtering happens at test case level, not file level
3. **Simplicity**: Fewer code paths, easier to maintain
4. **Test Properties Available Post-Parse**: Can't filter by test properties until file is parsed

**What cycodt DOES**:
```
Discover files (Layer 1) → Parse ALL files → Filter test cases (Layers 3 & 4)
```

**What cycodmd DOES**:
```
Discover files (Layer 1) → Filter files by content (Layer 2) → Parse filtered files → Filter lines (Layers 3 & 4)
```

---

## 7. FileHelpers: No Content Filtering

### File: `src/common/Helpers/FileHelpers.cs`

The file discovery method used by cycodt:

```csharp
public static IEnumerable<string> FindMatchingFiles(
    IEnumerable<string> includeGlobs, 
    IEnumerable<string> excludeGlobs,
    IEnumerable<Regex> excludeFileNamePatternList)
{
    // Filters by:
    // 1. Include glob patterns
    // 2. Exclude glob patterns
    // 3. Exclude filename regex patterns
    
    // Does NOT filter by:
    // - File content
    // - File metadata (size, modified time, etc.)
    // - Number of tests in file
}
```

**Evidence**: `FindMatchingFiles()` signature has NO parameters for content-based filtering.

---

## 8. Test Discovery: Unconditional Parse

### File: `src/cycodt/TestFramework/YamlTestFramework.cs`

```csharp
public static IEnumerable<TestCase> GetTestsFromYaml(string source, FileInfo file)
{
    // Unconditionally:
    // 1. Read YAML file
    // 2. Parse test cases
    // 3. Return ALL test cases from file
    
    // No filtering based on:
    // - File properties
    // - Test count
    // - File metadata
}
```

**Evidence**: Method is called for every discovered file, no filtering occurs.

---

## Summary

Layer 2 (Container Filtering) is **NOT IMPLEMENTED** for `cycodt list` because:

1. **No Filtering Logic**: The code path from Layer 1 (file discovery) to Layer 3 (test filtering) contains NO file-level filtering
2. **No Filter Options**: Command-line parsing has NO options for file content or metadata filtering
3. **No Data Structure**: `TestBaseCommand` has NO properties to store Layer 2 filter criteria
4. **Unconditional Processing**: ALL discovered files are parsed and ALL test cases extracted
5. **Design Decision**: Test framework architecture operates at test-case granularity, not file granularity

**Code Evidence**:
```csharp
// Layer 1 complete
var files = FindTestFiles();  // Returns: List<FileInfo>

// [NO LAYER 2 HERE - Jump directly to test extraction]

// Extract ALL tests from ALL files
var tests = files.SelectMany(file => GetTestsFromFile(file));

// Layer 3 & 4 - test-level filtering
var filtered = YamlTestCaseFilter.FilterTestCases(tests, filters);
```

**Key Source Files**:
- `TestBaseCommand.cs`: Lines 47-61 (no Layer 2 logic in pipeline)
- `TestBaseCommand.cs`: Lines 8-27 (no Layer 2 properties)
- `TestBaseCommand.cs`: Lines 244-256 (unconditional file loading)
- `CycoDtCommandLineOptions.cs`: Lines 42-85 (no Layer 2 options)
