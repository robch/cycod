# cycodt list - Layer 2: CONTAINER FILTERING - Proof

This document provides detailed source code evidence for Layer 2 (Container Filtering) implementation.

## Container Filtering Architecture

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

#### Main filtering pipeline

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

**Evidence - Layer 2 implementation**:
- Line 57: `FilterOptionalTests()` - First Layer 2 filter (optional category filtering)
- Line 58: `YamlTestCaseFilter.FilterTestCases()` - Second Layer 2 filter (name-based filtering)
- Both operate on `TestCase` objects (containers), not test content
- Order matters: Optional filtering happens BEFORE name filtering

**Evidence - Container vs Content**:
- Layer 2 receives `IEnumerable<TestCase>` (test containers)
- Layer 2 returns `IList<TestCase>` (filtered containers)
- No examination of test content properties (command, expectations)

## Optional Test Filtering (Container-Level)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

#### FilterOptionalTests method

**Lines 115-138**:
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

**Mode 1: Include all optional** (lines 121-123):
```csharp
var includeAllOptional = includeOptionalCategories.Count == 1 && 
                        string.IsNullOrEmpty(includeOptionalCategories[0]);
if (includeAllOptional) return allTests;
```
- Condition: List has exactly 1 element AND that element is empty string
- Created by: `--include-optional` with no arguments (see Layer 1 Proof, lines 153-160)
- Result: ALL tests returned (no filtering)

**Mode 2: Exclude all optional** (line 126):
```csharp
var excludeAllOptional = includeOptionalCategories.Count == 0;
```
- Condition: List is empty
- Created by: No `--include-optional` flag (default)
- Result: All tests with `HasOptionalTrait()` are excluded (line 128)

**Mode 3: Include specific categories** (lines 127-130):
```csharp
var excludedTests = allTests
    .Where(test => HasOptionalTrait(test) && 
                  (excludeAllOptional || !HasMatchingOptionalCategory(test, includeOptionalCategories)))
    .ToList();
```
- Condition: List contains specific category strings
- Created by: `--include-optional cat1 cat2`
- Logic: Exclude test if:
  - It has optional trait (`HasOptionalTrait()`) **AND**
  - Either excluding all optional **OR** it doesn't match a specified category
- Result: Tests with matching categories are included, others excluded

**Evidence - Test chain repair** (lines 132-136):
```csharp
if (excludedTests.Count > 0)
{
    // Repair the test chain by updating nextTestCaseId and afterTestCaseId properties
    RepairTestChain(allTests, excludedTests);
}
```
- Only called if tests were excluded
- Maintains test execution order by updating references

**Evidence - Return filtered list** (line 138):
```csharp
return allTests.Except(excludedTests);
```
- Uses LINQ `Except()` to remove excluded tests
- Returns `IEnumerable<TestCase>` (lazy evaluation)

#### HasOptionalTrait method

**Lines 233-236**:
```csharp
private bool HasOptionalTrait(TestCase test)
{
    return test.Traits.Any(t => t.Name == "optional");
}
```

**Evidence - Container property check**:
- Examines `test.Traits` collection (container-level metadata)
- Trait name `"optional"` identifies optional tests
- Trait **value** is the optional category (not checked here)
- Returns true if test has **any** optional trait

**Example TestCase.Traits**:
```yaml
# In YAML test file:
optional: broken-test
```
Becomes:
```csharp
test.Traits = [
    new Trait("optional", "broken-test")
]
```

#### HasMatchingOptionalCategory method

**Lines 238-242**:
```csharp
private bool HasMatchingOptionalCategory(TestCase test, List<string> categories)
{
    var optionalTraits = test.Traits.Where(t => t.Name == "optional").Select(t => t.Value);
    return optionalTraits.Any(value => categories.Contains(value));
}
```

**Evidence - Category matching logic**:
- Line 240: Gets all trait **values** where trait **name** is `"optional"`
- Line 241: Returns true if ANY trait value is in the categories list
- Uses case-sensitive `Contains()` check

**Example**:
```csharp
// Test has: optional: broken-test
optionalTraits = ["broken-test"]
categories = ["broken-test", "wip"]
// Result: true (match found)

// Test has: optional: slow
optionalTraits = ["slow"]
categories = ["broken-test", "wip"]
// Result: false (no match)
```

## Test Chain Repair (Container Relationship Maintenance)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

#### RepairTestChain method

**Lines 140-231**:
```csharp
private void RepairTestChain(List<TestCase> allTests, List<TestCase> excludedTests)
{
    // Create a dictionary to quickly look up tests by ID
    var testsById = allTests.ToDictionary(test => test.Id.ToString());
    
    // For each excluded test
    foreach (var excludedTest in excludedTests)
    {
        string? prevTestId = YamlTestProperties.Get(excludedTest, "afterTestCaseId");
        string? nextTestId = YamlTestProperties.Get(excludedTest, "nextTestCaseId");
        
        // Skip if no connections to repair
        if (string.IsNullOrEmpty(prevTestId) && string.IsNullOrEmpty(nextTestId))
            continue;
```

**Evidence - Container relationship properties**:
- `afterTestCaseId`: Property indicating "run after test X"
- `nextTestCaseId`: Property indicating "run test Y next"
- Retrieved via `YamlTestProperties.Get()` (custom property storage)
- These are container-level relationships, not content

**Evidence - Repair strategy** (lines 156-186):
```csharp
        // Find previous and next non-excluded tests
        TestCase? prevTest = null;
        if (!string.IsNullOrEmpty(prevTestId) && testsById.TryGetValue(prevTestId, out var tempPrevTest))
        {
            // Only consider this previous test if it's not also being excluded
            if (!excludedTests.Contains(tempPrevTest))
            {
                prevTest = tempPrevTest;
            }
            else
            {
                // If the previous test is also excluded, walk backward until finding a non-excluded test
                string? currentPrevId = prevTestId;
                while (!string.IsNullOrEmpty(currentPrevId))
                {
                    if (testsById.TryGetValue(currentPrevId, out var currentPrevTest) && 
                        !excludedTests.Contains(currentPrevTest))
                    {
                        prevTest = currentPrevTest;
                        break;
                    }
                    
                    // Move to the previous test in the chain
                    var prevPrevId = testsById.TryGetValue(currentPrevId, out var prevPrevTest) 
                        ? YamlTestProperties.Get(prevPrevTest, "afterTestCaseId") 
                        : null;
                        
                    if (string.IsNullOrEmpty(prevPrevId)) break;
                    currentPrevId = prevPrevId;
                }
            }
        }
```

**Evidence - Recursive chain walking**:
- If immediate neighbor is also excluded, walk backward/forward
- Continues until finding non-excluded test or chain end
- Handles cascading exclusions (multiple consecutive excluded tests)

**Evidence - Connection update** (lines 221-229):
```csharp
        // Update the connections to skip over excluded tests
        if (prevTest != null && nextTest != null)
        {
            // Connect previous test to next test
            YamlTestProperties.Set(prevTest, "nextTestCaseId", nextTest.Id.ToString());
            // Connect next test to previous test
            YamlTestProperties.Set(nextTest, "afterTestCaseId", prevTest.Id.ToString());
            
            TestLogger.Log($"Repaired test chain: {prevTest.DisplayName} -> {nextTest.DisplayName} (skipping excluded tests)");
        }
```

**Evidence - Bidirectional linking**:
- Updates **both** `nextTestCaseId` on prevTest AND `afterTestCaseId` on nextTest
- Maintains chain integrity in both directions
- Logs repair action for debugging

**Example chain repair**:
```
Before:
  Test A (nextTestCaseId: B) → Test B [optional: skip] (nextTestCaseId: C) → Test C

After excluding Test B:
  Test A (nextTestCaseId: C) → Test C (afterTestCaseId: A)
```

## Test Name Filtering (Container-Level)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

#### GetTestFilters method

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

**Evidence - Filter format conversion**:
- `Tests` (from `--test`, `--tests`) added without prefix → exact name match
- `Contains` (from `--contains`) prefixed with `+` → inclusion pattern
- `Remove` (from `--remove`) prefixed with `-` → exclusion pattern

**Evidence - Order doesn't matter**:
- All three lists concatenated into single filter list
- `YamlTestCaseFilter` processes all filters

### File: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

While this file's full implementation is not shown in the command files, we can infer its behavior from usage:

```csharp
var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();
```

**Signature**:
```csharp
public static IEnumerable<TestCase> FilterTestCases(
    IEnumerable<TestCase> tests, 
    List<string> filters)
```

**Inferred behavior** (based on filter format):
1. For each filter string:
   - If starts with `+`: Include tests where `DisplayName.Contains(pattern)`
   - If starts with `-`: Exclude tests where `DisplayName.Contains(pattern)`
   - Otherwise: Include tests where `DisplayName.Equals(name)`
2. Apply all inclusion filters (OR logic)
3. Apply all exclusion filters (AND logic)
4. Return filtered test cases

**Evidence - Container-level operation**:
- Operates on `TestCase` objects
- Filters based on `DisplayName` property (container identity)
- Returns filtered `TestCase` objects
- Does NOT examine test content (command, expectations)

## Container Properties vs Content Properties

### Container Properties (Layer 2 filters these)

**From TestCase object**:
- `DisplayName`: Test name (used by name filters)
- `FullyQualifiedName`: Unique identifier
- `Id`: GUID
- `CodeFilePath`: Source YAML file path
- `Traits`: Collection of name-value pairs
  - `optional` trait indicates optional test
  - Trait value is the optional category

**Custom properties** (via YamlTestProperties):
- `afterTestCaseId`: Test dependency
- `nextTestCaseId`: Test sequence

### Content Properties (NOT filtered in Layer 2)

**From YAML test definition**:
- `command`: Shell command to execute
- `script`: Script content
- `expect-regex`: Expected output patterns
- `not-expect-regex`: Unexpected output patterns
- `expect-output`: Expected output file
- `expect-instructions`: AI-based expectations
- `env`: Environment variables
- `input`: Test input
- `description`: Test description

**Evidence these are NOT Layer 2**: None of the Layer 2 methods examine these properties. They would be examined in Layer 3 (Content Filtering), which cycodt `list` does not implement.

## Integration with Layer 1

### Layer 1 provides test cases

**From TestBaseCommand.FindAndFilterTests()** (lines 47-54):
```csharp
var files = FindTestFiles();  // Layer 1: Find test files
var filters = GetTestFilters();  // Layer 2 prep: Convert filter lists

var atLeastOneFileSpecified = files.Any();
var tests = atLeastOneFileSpecified
    ? files.SelectMany(file => GetTestsFromFile(file))  // Layer 1: Load test cases
    : Array.Empty<TestCase>();
```

**Evidence**:
- Layer 1 finds files and loads TestCase objects
- Layer 2 receives `IEnumerable<TestCase>`
- Layer 2 filters based on TestCase properties

### Layer 2 produces filtered test cases

**From TestBaseCommand.FindAndFilterTests()** (lines 57-61):
```csharp
var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();

return filtered;
```

**Evidence**:
- Layer 2 returns `IList<TestCase>` (filtered containers)
- These go to Layer 6 (Display Control) for presentation
- No Layer 3 or Layer 4 processing in `list` command

## Integration with Layer 6 (Display)

### File: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

#### Verbose display grouping

**Lines 20-36** (in `ExecuteList()` method):
```csharp
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
```

**Evidence - Uses Layer 2 results**:
- `tests` is the filtered list from Layer 2
- Groups by `CodeFilePath` (shows which file each test came from)
- After Layer 2 filtering, some files may have 0 tests (excluded entirely)

#### Simple display

**Lines 38-44**:
```csharp
else
{
    foreach (var test in tests)
    {
        ConsoleHelpers.WriteLine(test.FullyQualifiedName, ConsoleColor.DarkGray);
    }
}
```

**Evidence**:
- Simple iteration over filtered tests
- Shows `FullyQualifiedName` (includes file path)

#### Count display

**Lines 46-48**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"\nFound {tests.Count()} test..."
    : $"\nFound {tests.Count()} tests...");
```

**Evidence**:
- Count reflects **post-Layer-2 filtering**
- Shows how many tests passed all container filters

## Call Stack Summary

```
TestListCommand.ExecuteList()
    ↓
TestBaseCommand.FindAndFilterTests()
    ↓
    ├─→ FindTestFiles() [Layer 1]
    │   └─→ FileHelpers.FindMatchingFiles()
    │
    ├─→ GetTestsFromFile(file) [Layer 1]
    │   └─→ YamlTestFramework.GetTestsFromYaml()
    │       └─→ Parse YAML, create TestCase objects
    │
    ├─→ GetTestFilters() [Layer 2 prep]
    │   └─→ Convert Tests/Contains/Remove → filter strings
    │
    ├─→ FilterOptionalTests(tests, IncludeOptionalCategories) [Layer 2]
    │   ├─→ HasOptionalTrait(test) - check for optional trait
    │   ├─→ HasMatchingOptionalCategory(test, categories) - check category match
    │   ├─→ (if exclusions) RepairTestChain(allTests, excludedTests)
    │   │   ├─→ YamlTestProperties.Get(test, "afterTestCaseId")
    │   │   ├─→ YamlTestProperties.Get(test, "nextTestCaseId")
    │   │   └─→ YamlTestProperties.Set() - update connections
    │   └─→ Return filtered tests
    │
    └─→ YamlTestCaseFilter.FilterTestCases(tests, filters) [Layer 2]
        └─→ Apply name-based filters
            └─→ Return filtered tests
```

## Data Structure Flow

```
Command Line Args
    ↓
Parse → TestBaseCommand properties:
    - Tests: List<string>           ["exact name 1", "exact name 2"]
    - Contains: List<string>        ["pattern1", "pattern2"]
    - Remove: List<string>          ["skip", "broken"]
    - IncludeOptionalCategories: List<string>  ["broken-test"] or [""] or []
    ↓
GetTestFilters() → List<string>:
    ["exact name 1", "exact name 2", "+pattern1", "+pattern2", "-skip", "-broken"]
    ↓
FilterOptionalTests() → IEnumerable<TestCase>:
    Input: All TestCase objects from all files
    Filter: Based on Traits collection (optional trait)
    Output: TestCase objects with matching optional categories
    Side effect: RepairTestChain() updates test properties
    ↓
YamlTestCaseFilter.FilterTestCases() → IEnumerable<TestCase>:
    Input: TestCase objects from FilterOptionalTests()
    Filter: Based on DisplayName property
    Output: TestCase objects with matching names
    ↓
Layer 6 (Display)
```

## Cross-References

- **YamlTestCaseFilter.FilterTestCases()**: `src/cycodt/TestFramework/YamlTestCaseFilter.cs` - implements name-based filtering
- **YamlTestProperties**: `src/cycodt/TestFramework/YamlTestProperties.cs` - custom property storage for TestCase
- **TestCase class**: Microsoft.VisualStudio.TestPlatform.ObjectModel - container object
- **Trait class**: Microsoft.VisualStudio.TestPlatform.ObjectModel - test metadata
