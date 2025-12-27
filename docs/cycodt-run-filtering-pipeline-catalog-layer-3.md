# cycodt `run` Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** determines what content WITHIN selected test files (containers) to execute. For the `run` command, this means filtering which individual tests from the loaded YAML files should be executed.

## Command: `run`

The `run` command executes tests from YAML test files and generates test reports. Layer 3 filtering determines which tests are executed based on their properties, names, and traits.

## Options That Affect Layer 3

The `run` command inherits all Layer 3 filtering options from `TestBaseCommand`, making it **identical to the `list` command** in terms of content filtering.

### 1. `--test` / `--tests`

**Purpose**: Execute only tests with specific names

**Syntax**:
```bash
cycodt run --test "test name"
cycodt run --tests "test 1" "test 2" "test 3"
```

**Behavior**:
- Filters tests to ONLY those matching specified names (exact substring match)
- Multiple test names act as OR (test must match at least one name)
- Applied as "source criteria" in YamlTestCaseFilter

**See**: [Layer 3 Proof](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#1-test--tests)

---

### 2. `--contains`

**Purpose**: Execute tests containing specific text patterns

**Syntax**:
```bash
cycodt run --contains "authentication"
cycodt run --contains "login" "api"
```

**Behavior**:
- Searches across all test properties: DisplayName, FullyQualifiedName, traits, CLI commands, expect patterns, etc.
- Multiple contains patterns act as AND (test must contain ALL patterns)
- Prefixed with `+` internally to indicate "must match all"
- Applied after `--test` filtering

**Pattern Matching**:
- Tests are matched against: DisplayName, FullyQualifiedName, traits (name and value), and all test properties (cli, run, script, bash, expect, expect-regex, etc.)

**See**: [Layer 3 Proof](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#2-contains)

---

## Filtering Algorithm

The Layer 3 filtering process for `run` is **identical to `list`**:

```
1. Load all tests from selected test files (Layer 2)
2. Apply optional test filtering (Layer 4 - done first for test chain repair)
3. Build filter criteria:
   a. Tests → source criteria (OR logic)
   b. Contains → must-match criteria (AND logic, prefixed with +)
   c. Remove → must-not-match criteria (AND logic, prefixed with -, handled in Layer 4)
4. Apply YamlTestCaseFilter.FilterTestCases():
   a. If source criteria exist: include tests matching ANY source criterion
   b. If must-match criteria exist: include tests matching ALL must-match criteria
   c. If must-not-match criteria exist: exclude tests matching ANY must-not-match criterion
5. Return filtered test list for execution
```

**Source Code Flow**:
```
TestRunCommand.ExecuteTestRun()
  → TestBaseCommand.FindAndFilterTests()
    → TestBaseCommand.GetTestFilters()
    → YamlTestCaseFilter.FilterTestCases(tests, filters)
  → YamlTestFramework.RunTests(tests, ...)  [Layer 9 - Actions]
```

**See**: [Layer 3 Proof - Filtering Algorithm](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#filtering-algorithm)

---

## Differences from `list` Command

### Execution Flow

While the filtering logic is identical, `run` does additional work:

**`list` command**:
1. Filter tests (Layer 3)
2. Display test names (Layer 6)

**`run` command**:
1. Filter tests (Layer 3)
2. **Execute filtered tests** (Layer 9)
3. **Generate test report** (Layer 7)

### Source Code Comparison

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Lines 26-43**:
```csharp
private int ExecuteTestRun()
{
    try
    {
        TestLogger.Log(new CycoDtTestFrameworkLogger());

        var tests = FindAndFilterTests();  // ← Same as list command
        ConsoleHelpers.WriteLine(tests.Count() == 1
            ? $"Found {tests.Count()} test...\n"
            : $"Found {tests.Count()} tests...\n");

        var consoleHost = new YamlTestFrameworkConsoleHost();
        var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);  // ← Run tests

        GetOutputFileAndFormat(out var file, out var format);
        var passed = consoleHost.Finish(resultsByTestCaseId, format, file);  // ← Generate report

        return passed ? 0 : 1;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
        return 1;
    }
}
```

**Evidence**:
- Line 32: Uses the same `FindAndFilterTests()` method as `list`
- Line 38: **Difference**: Executes tests with `YamlTestFramework.RunTests()`
- Line 41: **Difference**: Generates test report with `consoleHost.Finish()`

**See**: [Layer 3 Proof - Run Command Execution](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#run-command-execution)

---

## Filter Syntax

### Source Criteria (OR Logic)

Specified via `--test` / `--tests`:
```bash
cycodt run --tests "test1" "test2"
# Executes tests containing "test1" OR "test2"
```

### Must-Match Criteria (AND Logic)

Specified via `--contains`:
```bash
cycodt run --contains "auth" "api"
# Executes tests containing "auth" AND "api"
```

### Combined Filtering

```bash
cycodt run --tests "login" "signup" --contains "success" --remove "slow"
# Executes tests containing:
#   ("login" OR "signup") AND "success" AND NOT "slow"
```

**See**: [Layer 3 Proof - Filter Syntax Examples](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#filter-syntax)

---

## Property-Based Filtering

Tests are matched against the same comprehensive set of properties as `list`:

### Core Properties
- `DisplayName`: Test's display name
- `FullyQualifiedName`: Full test name including file path
- `FullyQualifiedNameBase`: FQN without hash suffix

### Execution Properties
- `cli`: CLI command to run
- `run`: Default run command
- `script`: Script content
- `bash`: Bash script

### Matrix/Iteration Properties
- `matrix`: Matrix variables
- `foreach`: Foreach iteration
- `arguments`: Command arguments
- `input`: Input to command

### Expectation Properties
- `expect`: Expected output (exact match)
- `expect-regex`: Expected regex patterns
- `not-expect-regex`: Patterns that should NOT match
- `expect-exit-code`: Expected exit code

### Control Properties
- `parallelize`: Parallelization setting
- `skipOnFailure`: Skip-on-failure setting
- `timeout`: Timeout value
- `working-directory`: Working directory

### Trait-Based Properties
- All traits (tags, categories, etc.) are searchable by name or value

**See**: [Layer 3 Proof - Searchable Properties](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#property-based-filtering)

---

## How Filtering Interacts with Other Layers

### Relationship to Layer 1 (Target Selection)
- Layer 1 determines WHICH test files to load
- Layer 3 determines WHICH tests from those files to execute

### Relationship to Layer 2 (Container Filtering)
- Layer 2 excludes entire test files
- Layer 3 filters individual tests within included files

### Relationship to Layer 4 (Content Removal)
- Layer 4 (--remove, --include-optional) is applied BEFORE Layer 3
- This is because optional test filtering requires test chain repair
- After Layer 4, Layer 3 applies positive filters (--test, --contains)

### Relationship to Layer 7 (Output Persistence)
- Layer 3 determines WHAT tests to execute
- Layer 7 determines WHERE to save test results (--output-file, --output-format)

### Relationship to Layer 9 (Actions on Results)
- Layer 3 determines WHICH tests to execute
- Layer 9 performs the actual test execution and result collection

---

## Examples

### Example 1: Execute Specific Test
```bash
cycodt run --test "authentication test"
```
Executes only tests whose DisplayName or FullyQualifiedName contains "authentication test"

### Example 2: Execute Tests by Multiple Patterns (AND)
```bash
cycodt run --contains "login" "api" "success"
```
Executes only tests containing ALL three patterns: "login", "api", AND "success"

### Example 3: Execute Tests by Names (OR)
```bash
cycodt run --tests "login" "logout" "signup"
```
Executes tests containing ANY of: "login", "logout", OR "signup"

### Example 4: Complex Filtering with Test Results
```bash
cycodt run --file "tests/auth/*.yaml" --tests "login" "signup" --contains "success" --remove "slow" --output-file auth-results.trx
```
1. Load tests from tests/auth/*.yaml (Layer 1)
2. Remove tests containing "slow" (Layer 4)
3. Filter to tests containing ("login" OR "signup") (Layer 3)
4. Further filter to tests containing "success" (Layer 3)
5. Execute filtered tests (Layer 9)
6. Save results to auth-results.trx (Layer 7)

### Example 5: Execute Property-Specific Tests
```bash
cycodt run --contains "timeout" --remove "10000"
```
Executes tests that have timeout properties but not with value "10000"

---

## Impact on Test Execution

### Test Execution Stats

**Output Example**:
```
Found 5 tests...

Running test: Login with valid credentials
✓ PASS (1.2s)

Running test: Login with invalid password
✓ PASS (0.8s)

Running test: Signup with new user
✓ PASS (1.5s)

Running test: Signup with existing email
✓ PASS (0.9s)

Running test: API login success
✓ PASS (0.7s)

Test run summary: 5 passed, 0 failed, 0 skipped
```

**Evidence**:
- Only the filtered tests (5 in this case) are executed
- Excluded tests are not run or reported

### Test Report Content

**TRX/JUnit XML Report**:
- Contains results only for executed (filtered) tests
- Excluded tests do not appear in the report at all
- Test count reflects the number of filtered tests, not total tests in files

**See**: [Layer 3 Proof - Test Report Impact](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md#test-report-impact)

---

## Edge Cases and Special Behaviors

### 1. Empty Filter Results
If no tests match the filters, the command:
1. Displays: `Found 0 tests...`
2. Does not execute any tests
3. Generates an empty test report
4. Returns exit code 0 (success)

### 2. Filter Priority
Filters are applied in this order:
1. Optional test filtering (Layer 4)
2. Source criteria (--test, --tests)
3. Must-match criteria (--contains)
4. Must-not-match criteria (--remove)

### 3. Case Sensitivity
- All text matching is case-sensitive
- "Login" and "login" are different

### 4. Partial Matching
- All filters use substring matching, not exact matching
- "--test login" matches "login test", "test login api", "user login flow", etc.

### 5. Test Chain Dependencies
- If a test depends on another test (via `afterTestCaseId`), both must pass filters to maintain the chain
- Optional test filtering repairs chains when intermediate tests are excluded
- Layer 3 filtering does NOT repair chains (assumes Layer 4 already did this)

---

## Implementation Notes

### Source Code Locations

**Command Line Parsing**:
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs:99-120` - Parsing --test, --tests, --contains

**Filter Building**:
- `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs:97-113` - GetTestFilters()

**Filter Application**:
- `src/cycodt/TestFramework/YamlTestCaseFilter.cs:6-65` - FilterTestCases() main algorithm
- `src/cycodt/TestFramework/YamlTestCaseFilter.cs:138-147` - TestContainsText() property matching

**Test Discovery and Execution**:
- `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs:47-61` - FindAndFilterTests()
- `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs:26-50` - ExecuteTestRun()

**Test Execution**:
- `src/cycodt/TestFramework/YamlTestFramework.cs` - RunTests() (test execution engine)

---

## Related Documentation

- [Layer 1: Target Selection](cycodt-run-filtering-pipeline-catalog-layer-1.md) - How test files are selected
- [Layer 2: Container Filtering](cycodt-run-filtering-pipeline-catalog-layer-2.md) - How test files are filtered
- [Layer 4: Content Removal](cycodt-run-filtering-pipeline-catalog-layer-4.md) - How tests are excluded
- [Layer 7: Output Persistence](cycodt-run-filtering-pipeline-catalog-layer-7.md) - How test results are saved
- [Layer 9: Actions on Results](cycodt-run-filtering-pipeline-catalog-layer-9.md) - How tests are executed

---

## Proof

For detailed source code evidence of all assertions in this document, see:
- **[Layer 3 Proof Document](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md)**
