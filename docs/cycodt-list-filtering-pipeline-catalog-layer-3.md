# cycodt `list` Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** determines what content WITHIN selected test files (containers) to include in the output. For the `list` command, this means filtering which individual tests from the loaded YAML files should be displayed.

## Command: `list`

The `list` command displays test names from YAML test files. Layer 3 filtering determines which tests are shown based on their properties, names, and traits.

## Options That Affect Layer 3

### 1. `--test` / `--tests`

**Purpose**: Include only tests with specific names

**Syntax**:
```bash
cycodt list --test "test name"
cycodt list --tests "test 1" "test 2" "test 3"
```

**Behavior**:
- Filters tests to ONLY those matching specified names (exact substring match)
- Multiple test names act as OR (test must match at least one name)
- Applied as "source criteria" in YamlTestCaseFilter

**See**: [Layer 3 Proof](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#1-test--tests)

---

### 2. `--contains`

**Purpose**: Include tests containing specific text patterns

**Syntax**:
```bash
cycodt list --contains "authentication"
cycodt list --contains "login" "api"
```

**Behavior**:
- Searches across all test properties: DisplayName, FullyQualifiedName, traits, CLI commands, expect patterns, etc.
- Multiple contains patterns act as AND (test must contain ALL patterns)
- Prefixed with `+` internally to indicate "must match all"
- Applied after `--test` filtering

**Pattern Matching**:
- Tests are matched against: DisplayName, FullyQualifiedName, traits (name and value), and all test properties (cli, run, script, bash, expect, expect-regex, etc.)

**See**: [Layer 3 Proof](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#2-contains)

---

### 3. Test Name Filtering (Positional - Not Currently Implemented)

**Note**: Unlike other commands, `list` does not currently accept positional arguments for test name filtering. All filtering must be done via explicit flags.

---

## Filtering Algorithm

The Layer 3 filtering process follows this algorithm:

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
5. Return filtered test list
```

**Source Code Flow**:
```
TestListCommand.ExecuteList()
  → TestBaseCommand.FindAndFilterTests()
    → TestBaseCommand.GetTestFilters()
    → YamlTestCaseFilter.FilterTestCases(tests, filters)
```

**See**: [Layer 3 Proof - Filtering Algorithm](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#filtering-algorithm)

---

## Filter Syntax

### Source Criteria (OR Logic)

Specified via `--test` / `--tests`:
```bash
cycodt list --tests "test1" "test2"
# Includes tests containing "test1" OR "test2"
```

### Must-Match Criteria (AND Logic)

Specified via `--contains`:
```bash
cycodt list --contains "auth" "api"
# Includes tests containing "auth" AND "api"
```

### Combined Filtering

```bash
cycodt list --tests "login" "signup" --contains "success" --remove "slow"
# Includes tests containing:
#   ("login" OR "signup") AND "success" AND NOT "slow"
```

**See**: [Layer 3 Proof - Filter Syntax Examples](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#filter-syntax)

---

## Property-Based Filtering

Tests are matched against a comprehensive set of properties:

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

**See**: [Layer 3 Proof - Searchable Properties](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md#property-based-filtering)

---

## How Filtering Interacts with Other Layers

### Relationship to Layer 1 (Target Selection)
- Layer 1 determines WHICH test files to load
- Layer 3 determines WHICH tests from those files to show

### Relationship to Layer 2 (Container Filtering)
- Layer 2 excludes entire test files
- Layer 3 filters individual tests within included files

### Relationship to Layer 4 (Content Removal)
- Layer 4 (--remove, --include-optional) is applied BEFORE Layer 3
- This is because optional test filtering requires test chain repair
- After Layer 4, Layer 3 applies positive filters (--test, --contains)

### Relationship to Layer 6 (Display Control)
- Layer 3 determines WHAT tests to show
- Layer 6 determines HOW to display them (verbose vs. compact)

---

## Examples

### Example 1: Filter by Test Name
```bash
cycodt list --test "authentication test"
```
Shows only tests whose DisplayName or FullyQualifiedName contains "authentication test"

### Example 2: Filter by Multiple Patterns (AND)
```bash
cycodt list --contains "login" "api" "success"
```
Shows only tests containing ALL three patterns: "login", "api", AND "success"

### Example 3: Filter by Test Names (OR)
```bash
cycodt list --tests "login" "logout" "signup"
```
Shows tests containing ANY of: "login", "logout", OR "signup"

### Example 4: Complex Filtering
```bash
cycodt list --file "tests/auth/*.yaml" --tests "login" "signup" --contains "success" --remove "slow"
```
1. Load tests from tests/auth/*.yaml (Layer 1)
2. Remove tests containing "slow" (Layer 4)
3. Filter to tests containing ("login" OR "signup") (Layer 3)
4. Further filter to tests containing "success" (Layer 3)

### Example 5: Property-Specific Filtering
```bash
cycodt list --contains "timeout" --remove "10000"
```
Shows tests that have timeout properties but not with value "10000"

---

## Edge Cases and Special Behaviors

### 1. Empty Filter Results
If no tests match the filters, the command displays:
```
Found 0 tests...
```

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

---

## Implementation Notes

### Source Code Locations

**Command Line Parsing**:
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs:99-112` - Parsing --test, --tests, --contains, --remove

**Filter Building**:
- `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs:97-113` - GetTestFilters()

**Filter Application**:
- `src/cycodt/TestFramework/YamlTestCaseFilter.cs:6-65` - FilterTestCases() main algorithm
- `src/cycodt/TestFramework/YamlTestCaseFilter.cs:138-147` - TestContainsText() property matching

**Test Discovery**:
- `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs:47-61` - FindAndFilterTests()

---

## Related Documentation

- [Layer 1: Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md) - How test files are selected
- [Layer 2: Container Filtering](cycodt-list-filtering-pipeline-catalog-layer-2.md) - How test files are filtered
- [Layer 4: Content Removal](cycodt-list-filtering-pipeline-catalog-layer-4.md) - How tests are excluded
- [Layer 6: Display Control](cycodt-list-filtering-pipeline-catalog-layer-6.md) - How filtered tests are displayed

---

## Proof

For detailed source code evidence of all assertions in this document, see:
- **[Layer 3 Proof Document](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md)**
