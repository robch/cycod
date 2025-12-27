# cycodt list - Layer 2: CONTAINER FILTERING

## Purpose

Filter which **containers** (test files and test cases) to include/exclude based on their properties or content.

## Implementation Overview

In cycodt, "containers" exist at two levels:
1. **Test files** (.yaml files containing test definitions)
2. **Test cases** (individual tests within files)

Layer 2 filtering determines which of these containers make it through to content processing.

## Container Types

### Test File Containers

Test files are filtered based on:
- **File path patterns** (glob patterns via `--file`, `--files`)
- **Exclusion patterns** (via `--exclude-files`, `--exclude`, `.cycodtignore`)

These are **fully implemented in Layer 1** (Target Selection), as file-level filtering is considered part of target selection in cycodt's architecture.

### Test Case Containers

Test cases are filtered based on:
- **Test name** (exact matches via `--test`, `--tests`)
- **Test name content** (substring matches via `--contains`)
- **Optional test categories** (via `--include-optional`)

## Options That Implement Container Filtering

### Exact Test Name Filtering

#### `--test <name>`
#### `--tests <name> [<name>...]`

Selects specific test cases by exact name match.

**Example**: `cycodt list --test "my exact test name"`

**Classification**: This is **container-level filtering** because:
- It operates on the TestCase container object
- It filters based on the container's identity (name), not its content (command, expectations)
- It's an all-or-nothing decision about the entire test container

**Data flow**:
1. Parser adds names to `TestBaseCommand.Tests` list
2. `GetTestFilters()` includes these names without prefix
3. `YamlTestCaseFilter.FilterTestCases()` matches against `test.DisplayName`

### Pattern-Based Test Name Filtering

#### `--contains <pattern> [<pattern>...]`

Includes test cases whose names contain the specified pattern(s).

**Example**: `cycodt list --contains async --contains Task`

**Classification**: This is **container-level filtering** because:
- It filters based on the container's name property
- The pattern match is against the TestCase object's DisplayName
- It determines whether the entire test container is included

**Data flow**:
1. Parser adds patterns to `TestBaseCommand.Contains` list
2. `GetTestFilters()` converts to `+pattern` format
3. `YamlTestCaseFilter.FilterTestCases()` applies as inclusion filter

Note: This is Layer 2 (container filtering), not Layer 3 (content filtering), because it operates on the test **name**, which is a property of the test container itself, not the test's internal content (command, expectations, etc.).

### Optional Test Category Filtering

#### `--include-optional [<category>...]`

Filters test cases based on their optional category trait.

**Examples**:
- `cycodt list --include-optional` - include ALL optional tests
- `cycodt list --include-optional broken-test` - include only "broken-test" category
- (default) - exclude ALL optional tests

**Classification**: This is **container-level filtering** because:
- It operates on the TestCase's traits (container-level metadata)
- The `optional` trait is part of the test case's definition, not its execution content
- It determines which test containers to process

**Data flow**:
1. Parser adds categories to `TestBaseCommand.IncludeOptionalCategories` list
2. `FilterOptionalTests()` in TestBaseCommand examines each test's traits
3. Tests without matching optional category are excluded from the list

**Special handling**: When optional tests are excluded, `RepairTestChain()` is called to maintain test execution order by updating `nextTestCaseId` and `afterTestCaseId` properties on remaining tests.

## Container Filtering vs. Content Filtering

### Why These Are Layer 2 (Container), Not Layer 3 (Content)

In cycodt's architecture:

**Container properties** (Layer 2):
- Test name (DisplayName, FullyQualifiedName)
- Test traits (optional category)
- Test metadata (file path, ID)

**Content properties** (Layer 3):
- Test command/script
- Test expectations (expect-regex, expect-output)
- Test input/environment
- Test description

The key distinction: **Container filtering happens BEFORE the test definition content is examined**. The filters in Layer 2 operate on the TestCase object's identification and classification properties, not on what the test actually does.

## Implementation Architecture

### Filter Application Order

```
Test Files (found in Layer 1)
    ↓
Load TestCase objects from each file
    ↓
FilterOptionalTests() ← Layer 2: Optional category filtering
    ↓
YamlTestCaseFilter.FilterTestCases() ← Layer 2: Name-based filtering
    ↓
Filtered TestCase list
    ↓
(Layer 3 would examine test content, but cycodt list doesn't do this)
```

### Test Name Matching

Test name filters use substring matching, not regex:

**From `YamlTestCaseFilter.FilterTestCases()` implementation**:
- `+pattern`: Test passes if `DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase)`
- `-pattern`: Test passes if `!DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase)`
- No prefix: Test passes if `DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase)`

This means:
- `--contains async` matches "Test async method" and "async Test"
- `--test "my test"` matches only exactly "my test" (case-insensitive)

### Optional Test Filtering Logic

**Three modes** (see Layer 2 Proof for detailed code):

1. **Include all optional** (`--include-optional` with no args):
   - `IncludeOptionalCategories = [""]`
   - ALL tests returned (optional and non-optional)

2. **Exclude all optional** (default, no `--include-optional`):
   - `IncludeOptionalCategories = []` (empty list)
   - Tests with `optional` trait are excluded

3. **Include specific categories** (`--include-optional cat1 cat2`):
   - `IncludeOptionalCategories = ["cat1", "cat2"]`
   - Tests with `optional: cat1` or `optional: cat2` are included
   - Tests with other optional categories are excluded
   - Non-optional tests are always included

### Test Chain Repair

When optional tests are excluded, the test execution chain needs repair:

**Problem**: Tests can reference each other via:
- `afterTestCaseId`: "Run after test X"
- `nextTestCaseId`: "Run test Y next"

If test B is excluded but test A references it, the chain breaks.

**Solution**: `RepairTestChain()` walks the chain and connects remaining tests:
- Test A → Test B (excluded) → Test C
- Becomes: Test A → Test C

This ensures:
- Test execution order is maintained
- Dependencies are preserved
- No references to excluded tests remain

## Layer 2 vs Other Layers

### Comparison to Layer 1 (Target Selection)

**Layer 1** determines:
- Which test **files** to search
- Which test **names** to consider

**Layer 2** determines:
- Which test **cases** from those files to keep
- Based on test case **properties** (name patterns, optional traits)

The boundary: Layer 1 is about finding/specifying, Layer 2 is about filtering/excluding.

### Comparison to Layer 3 (Content Filtering)

**Layer 2** filters based on:
- Test case identity (name, traits)
- Test case classification (optional category)

**Layer 3** would filter based on:
- Test definition content (command contains pattern)
- Test expectations (has specific expect-regex)

cycodt `list` command **does not implement Layer 3** - it shows entire test cases that pass Layer 2.

### Comparison to Layer 4 (Content Removal)

**Layer 2** excludes **entire test cases**.

**Layer 4** would remove **parts** of test case content (e.g., hide certain expectation types).

cycodt `list` command **does not implement Layer 4** - it shows test names only, no content.

## No Content-Based Container Filtering

Unlike cycodmd (which has `--file-contains` to filter files by content), cycodt does NOT have options to filter test files based on their content.

**What's missing**:
- `--test-file-contains <pattern>` - include test files containing pattern
- `--test-command-contains <pattern>` - include tests whose command contains pattern

This is architectural: cycodt treats test files as atomic collections. All tests in a file are loaded, then filtered individually.

## Integration with Other Layers

### Layer 1 Integration

Layer 1's `FindAndFilterTests()` calls Layer 2's filtering methods:

```csharp
protected IList<TestCase> FindAndFilterTests()
{
    var files = FindTestFiles();  // Layer 1
    var tests = files.SelectMany(file => GetTestsFromFile(file));  // Layer 1
    
    var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories);  // Layer 2
    var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters);  // Layer 2
    
    return filtered;
}
```

Layer 2 receives all test cases from all files, then filters them.

### Layer 6 Integration (Display Control)

Layer 6's verbose mode groups by file path:

```csharp
var grouped = tests
    .GroupBy(t => t.CodeFilePath)
    .OrderBy(g => g.Key)
```

This shows how many tests per file **after** Layer 2 filtering.

See [Layer 2 Proof](layer-2-proof.md) for detailed source code evidence.
