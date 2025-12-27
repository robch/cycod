# cycodt `list` Command - Layer 3: Content Filtering - PROOF

## Overview

This document provides detailed source code evidence for all assertions made in [Layer 3: Content Filtering](cycodt-list-filtering-pipeline-catalog-layer-3.md) for the `list` command.

---

## 1. `--test` / `--tests`

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 99-106**:
```csharp
else if (arg == "--test")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var testName = ValidateString(arg, max1Arg.FirstOrDefault(), "test name");
    command.Tests.Add(testName!);
    i += max1Arg.Count();
}
```

**Lines 107-113**:
```csharp
else if (arg == "--tests")
{
    var testNames = GetInputOptionArgs(i + 1, args);
    var validTests = ValidateStrings(arg, testNames, "test names");
    command.Tests.AddRange(validTests);
    i += testNames.Count();
}
```

**Evidence**:
- Line 102: `--test` accepts a single test name (max: 1)
- Line 104: Test name is added to `command.Tests` list
- Line 109: `--tests` accepts multiple test names
- Line 111: All test names are added to `command.Tests` list

### Data Storage

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 23**:
```csharp
public List<string> Tests { get; set; }
```

**Evidence**:
- Tests are stored in a `List<string>` property on TestBaseCommand
- This property is initialized in the constructor (line 13)

### Filter Building

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 97-113**:
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

**Evidence**:
- Line 101: `Tests` are added to filters WITHOUT any prefix
- This makes them "source criteria" in YamlTestCaseFilter terminology
- Source criteria use OR logic (test matches if ANY test name matches)

### Filter Application

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 29-42**:
```csharp
var sourceCriteria = new List<string>();
var mustMatchCriteria = new List<string>();
var mustNotMatchCriteria = new List<string>();

foreach (var criterion in criteria)
{
    var isMustMatch = criterion.StartsWith("+");
    var isMustNotMatch = criterion.StartsWith("-");
    var isSource = !isMustMatch && !isMustNotMatch;

    if (isSource) sourceCriteria.Add(criterion);
    if (isMustMatch) mustMatchCriteria.Add(criterion.Substring(1));
    if (isMustNotMatch) mustNotMatchCriteria.Add(criterion.Substring(1));
}
```

**Evidence**:
- Line 36: Criteria without prefix are classified as "source criteria"
- Line 41: Source criteria are added to `sourceCriteria` list

**Lines 44-48**:
```csharp
var unfiltered = sourceCriteria.Count > 0
    ? tests.Where(test =>
        sourceCriteria.Any(criterion =>
            TestContainsText(test, criterion)))
    : tests;
```

**Evidence**:
- Line 44-48: If source criteria exist, filter tests to those matching ANY criterion (OR logic)
- Line 46: `.Any()` implements OR logic
- Line 47: Uses `TestContainsText()` for matching

---

## 2. `--contains`

### Command Line Parsing

**File**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 114-120**:
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
- Line 116: `--contains` accepts multiple patterns (no max limit)
- Line 118: All patterns are added to `command.Contains` list

### Data Storage

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Line 24**:
```csharp
public List<string> Contains { get; set; }
```

**Evidence**:
- Contains patterns are stored in a `List<string>` property
- Initialized in constructor (line 14)

### Filter Building

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 102-106**:
```csharp
foreach (var item in Contains)
{
    filters.Add($"+{item}");
}
```

**Evidence**:
- Line 104: Each contains pattern is prefixed with `+`
- This makes them "must-match criteria" in YamlTestCaseFilter
- Must-match criteria use AND logic (test must match ALL patterns)

### Filter Application

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 50-55**:
```csharp
if (mustMatchCriteria.Count > 0)
{
    unfiltered = unfiltered.Where(test =>
        mustMatchCriteria.All(criterion =>
            TestContainsText(test, criterion)));
}
```

**Evidence**:
- Line 50-55: If must-match criteria exist, filter tests to those matching ALL criteria (AND logic)
- Line 53: `.All()` implements AND logic
- Line 54: Uses `TestContainsText()` for matching

---

## Filtering Algorithm

### High-Level Flow

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 13-20**:
```csharp
private int ExecuteList()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());
        var tests = FindAndFilterTests();
        
        if (ConsoleHelpers.IsVerbose())
```

**Evidence**:
- Line 18: `FindAndFilterTests()` is called to get filtered test list

### FindAndFilterTests Implementation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 47-61**:
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

**Evidence**:
- Line 49: `FindTestFiles()` - Layer 1/2 (target selection and container filtering)
- Line 50: `GetTestFilters()` - Build filter criteria for Layer 3/4
- Line 54: Load all tests from files
- Line 57: `FilterOptionalTests()` - Layer 4 (content removal)
- Line 58: `YamlTestCaseFilter.FilterTestCases()` - Layer 3 (content filtering)

**Order of Operations**:
1. Find test files (Layer 1/2)
2. Load tests from files
3. Filter optional tests (Layer 4) - BEFORE Layer 3!
4. Apply Layer 3 filters (--test, --contains)

### YamlTestCaseFilter.FilterTestCases

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 6-65** (Complete Algorithm):
```csharp
public static IEnumerable<TestCase> FilterTestCases(IEnumerable<TestCase> tests, IEnumerable<string> criteria)
{
    // example 1: "ai init openai" "ai init speech" -skip -nightly
    // > test must contain either:
    // >   * "ai", "init", and "openai" in EXACTLY that order in any one single field/property, or
    // >   * "ai", "init", and "speech" in EXACTLY that order in any one single field/property
    // > test must not contain "skip" in any field/property
    // > test must not contain "nightly" in any field/property

    // example 2: +ai +init +openai -skip -nightly
    // > test must contain ALL three of "ai", "init", and "openai" in any field/property
    // >   * they do NOT need to be in the same field/property
    // > test must not contain "skip" in any field/property
    // > test must not contain "nightly" in any field/property

    // example 3: "ai dev new" "ai init speech" +java +build -skip
    // > test must contain, either:
    // >   * "ai", "init", and "openai" in EXACTLY that order in any one single field/property, or
    // >   * "ai", "init", and "speech" in EXACTLY that order in any one single field/property
    // > test must contain "java" in any field/property
    // > test must contain "build" in any field/property
    // > test must not contain "skip" in any field/property

    var sourceCriteria = new List<string>();
    var mustMatchCriteria = new List<string>();
    var mustNotMatchCriteria = new List<string>();

    foreach (var criterion in criteria)
    {
        var isMustMatch = criterion.StartsWith("+");
        var isMustNotMatch = criterion.StartsWith("-");
        var isSource = !isMustMatch && !isMustNotMatch;

        if (isSource) sourceCriteria.Add(criterion);
        if (isMustMatch) mustMatchCriteria.Add(criterion.Substring(1));
        if (isMustNotMatch) mustNotMatchCriteria.Add(criterion.Substring(1));
    }

    var unfiltered = sourceCriteria.Count > 0
        ? tests.Where(test =>
            sourceCriteria.Any(criterion =>
                TestContainsText(test, criterion)))
        : tests;

    if (mustMatchCriteria.Count > 0)
    {
        unfiltered = unfiltered.Where(test =>
            mustMatchCriteria.All(criterion =>
                TestContainsText(test, criterion)));
    }

    if (mustNotMatchCriteria.Count > 0)
    {
        unfiltered = unfiltered.Where(test =>
            mustNotMatchCriteria.All(criterion =>
                !TestContainsText(test, criterion)));
    }

    return unfiltered;
}
```

**Evidence**:
- Lines 8-27: Detailed comments explaining filter syntax and logic
- Lines 29-42: Parse criteria into three categories
- Lines 44-48: Apply source criteria (OR logic)
- Lines 50-55: Apply must-match criteria (AND logic)
- Lines 57-62: Apply must-not-match criteria (AND NOT logic)

---

## Property-Based Filtering

### TestContainsText Implementation

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 138-147**:
```csharp
private static bool TestContainsText(TestCase test, string text)
{
    var fqn = test.FullyQualifiedName;
    var fqnStripped = StripHash(fqn);
    return test.DisplayName.Contains(text)
        || fqn.Contains(text)
        || fqnStripped.Contains(text)
        || test.Traits.Any(x => x.Name == text || x.Value.Contains(text))
        || supportedFilterProperties.Any(property => GetPropertyValue(test, property)?.ToString()?.Contains(text) == true);
}
```

**Evidence**:
- Line 142: Searches `DisplayName`
- Line 143: Searches `FullyQualifiedName`
- Line 144: Searches `FullyQualifiedName` without hash suffix
- Line 145: Searches all trait names and values
- Line 146: Searches all supported filter properties (via `GetPropertyValue()`)

### GetPropertyValue Implementation

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 92-129**:
```csharp
private static object? GetPropertyValue(TestCase test, string name)
{
    switch (name.ToLower())
    {
        case "name":
        case "displayname": return test.DisplayName;

        case "fqn":
        case "fullyqualifiedname": return test.FullyQualifiedName;
        case "fullyqualifiednamebase": return StripHash(test.FullyQualifiedName);

        case "cli": return YamlTestProperties.Get(test, "cli");
        case "run": return YamlTestProperties.Get(test, "run");
        case "script": return YamlTestProperties.Get(test, "script");
        case "bash": return YamlTestProperties.Get(test, "bash");

        case "matrix": return YamlTestProperties.Get(test, "matrix");
        case "foreach": return YamlTestProperties.Get(test, "foreach");
        case "arguments": return YamlTestProperties.Get(test, "arguments");
        case "input": return YamlTestProperties.Get(test, "input");

        case "expect": return YamlTestProperties.Get(test, "expect");
        case "expect-regex": return YamlTestProperties.Get(test, "expect-regex");
        case "not-expect-regex": return YamlTestProperties.Get(test, "not-expect-regex");
        case "expect-exit-code": return YamlTestProperties.Get(test, "expect-exit-code");

        case "parallelize": return YamlTestProperties.Get(test, "parallelize");
        case "skipOnFailure": return YamlTestProperties.Get(test, "skipOnFailure");

        case "timeout": return YamlTestProperties.Get(test, "timeout");
        case "working-directory": return YamlTestProperties.Get(test, "working-directory");
    }

    var tags = test.Traits.Where(x => x.Name == name || name == "tags");
    if (tags.Count() == 0) return null;

    return tags.Select(x => x.Value).ToArray();
}
```

**Evidence**:
- Lines 96-97: Core name properties
- Lines 99-101: Fully qualified name variants
- Lines 103-106: Execution properties (cli, run, script, bash)
- Lines 108-111: Matrix/iteration properties
- Lines 113-116: Expectation properties
- Lines 118-119: Control properties (parallelize, skipOnFailure)
- Lines 121-122: Other properties (timeout, working-directory)
- Lines 125-128: Trait-based properties (tags, categories, etc.)

### Supported Filter Properties

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Line 150**:
```csharp
private static readonly string[] supportedFilterProperties = { "DisplayName", "FullyQualifiedName", "fullyQualifiedNameBase", "Category", "cli", "run", "script", "bash", "foreach", "arguments", "input", "expect", "expect-regex", "not-expect-regex", "expect-exit-code", "parallelize", "skipOnFailure" };
```

**Evidence**:
- All properties listed in this array are searchable via `TestContainsText()`
- This list corresponds to the properties handled in `GetPropertyValue()`

---

## Filter Syntax

### Source Criteria Example

**Scenario**: `cycodt list --tests "test1" "test2"`

**Filter Building** (GetTestFilters):
```csharp
filters.AddRange(Tests); // ["test1", "test2"]
```

**Result**: `["test1", "test2"]`

**Filter Application** (YamlTestCaseFilter.FilterTestCases):
```csharp
var sourceCriteria = ["test1", "test2"];
var unfiltered = tests.Where(test =>
    sourceCriteria.Any(criterion => TestContainsText(test, criterion)));
```

**Outcome**: Tests matching "test1" OR "test2"

### Must-Match Criteria Example

**Scenario**: `cycodt list --contains "auth" "api"`

**Filter Building** (GetTestFilters):
```csharp
foreach (var item in Contains) // ["auth", "api"]
{
    filters.Add($"+{item}");
}
```

**Result**: `["+auth", "+api"]`

**Filter Application** (YamlTestCaseFilter.FilterTestCases):
```csharp
var mustMatchCriteria = ["auth", "api"]; // + prefix stripped
var unfiltered = unfiltered.Where(test =>
    mustMatchCriteria.All(criterion => TestContainsText(test, criterion)));
```

**Outcome**: Tests matching "auth" AND "api"

### Combined Example

**Scenario**: `cycodt list --tests "login" "signup" --contains "success" --remove "slow"`

**Filter Building** (GetTestFilters):
```csharp
filters.AddRange(Tests);           // ["login", "signup"]
foreach (var item in Contains)     // ["success"]
    filters.Add($"+{item}");       // ["+success"]
foreach (var item in Remove)       // ["slow"]
    filters.Add($"-{item}");       // ["-slow"]
```

**Result**: `["login", "signup", "+success", "-slow"]`

**Filter Application** (YamlTestCaseFilter.FilterTestCases):
```csharp
var sourceCriteria = ["login", "signup"];
var mustMatchCriteria = ["success"];
var mustNotMatchCriteria = ["slow"];

// Step 1: Source criteria (OR)
var unfiltered = tests.Where(test =>
    sourceCriteria.Any(criterion => TestContainsText(test, criterion)));

// Step 2: Must-match criteria (AND)
unfiltered = unfiltered.Where(test =>
    mustMatchCriteria.All(criterion => TestContainsText(test, criterion)));

// Step 3: Must-not-match criteria (AND NOT)
unfiltered = unfiltered.Where(test =>
    mustNotMatchCriteria.All(criterion => !TestContainsText(test, criterion)));
```

**Outcome**: Tests matching ("login" OR "signup") AND "success" AND NOT "slow"

---

## Edge Cases

### Empty Filter Results

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 46-48**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"\nFound {tests.Count()} test..."
    : $"\nFound {tests.Count()} tests...");
```

**Evidence**:
- When `tests.Count()` is 0, displays "Found 0 tests..."
- No special error handling for empty results

### Case Sensitivity

**File**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Line 142**:
```csharp
return test.DisplayName.Contains(text)
```

**Evidence**:
- Uses `.Contains()` method without `StringComparison` parameter
- Default behavior is case-sensitive matching
- "Login" and "login" are treated as different

### Partial Matching

**Example**: Test name: "User login API test"

**Matches**:
- `--test "login"` ✓ (contains "login")
- `--test "API"` ✓ (contains "API")
- `--test "User login"` ✓ (contains "User login")
- `--test "test"` ✓ (contains "test")

**Does Not Match**:
- `--test "logout"` ✗ (doesn't contain "logout")
- `--test "LOGIN"` ✗ (case-sensitive)

**Evidence**: All matching uses `.Contains()` which is substring matching, not exact matching.

---

## Summary of Evidence

### Option Parsing
✅ **`--test`**: Lines 99-106 in CycoDtCommandLineOptions.cs
✅ **`--tests`**: Lines 107-113 in CycoDtCommandLineOptions.cs
✅ **`--contains`**: Lines 114-120 in CycoDtCommandLineOptions.cs

### Filter Building
✅ **GetTestFilters()**: Lines 97-113 in TestBaseCommand.cs
✅ **Prefix logic**: Line 101 (no prefix), 104 (+prefix), 109 (-prefix)

### Filter Application
✅ **FilterTestCases()**: Lines 6-65 in YamlTestCaseFilter.cs
✅ **Source criteria (OR)**: Lines 44-48
✅ **Must-match criteria (AND)**: Lines 50-55
✅ **Must-not-match criteria (AND NOT)**: Lines 57-62

### Property Matching
✅ **TestContainsText()**: Lines 138-147 in YamlTestCaseFilter.cs
✅ **GetPropertyValue()**: Lines 92-129 in YamlTestCaseFilter.cs
✅ **Supported properties**: Line 150 in YamlTestCaseFilter.cs

### Execution Flow
✅ **TestListCommand.ExecuteList()**: Lines 13-57 in TestListCommand.cs
✅ **FindAndFilterTests()**: Lines 47-61 in TestBaseCommand.cs
✅ **Layer ordering**: Optional filter (Layer 4) → Content filter (Layer 3)

---

## Conclusion

All assertions in [Layer 3: Content Filtering](cycodt-list-filtering-pipeline-catalog-layer-3.md) are supported by source code evidence from the cycodt codebase. The filtering mechanism is well-defined, with clear separation between OR logic (source criteria), AND logic (must-match criteria), and AND NOT logic (must-not-match criteria).
