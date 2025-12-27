# cycodt list - Layer 1: TARGET SELECTION

## Purpose

Specify **what to search** - the primary search space of test files and test cases.

## Implementation Overview

The `list` command's target selection operates at two levels:
1. **File level**: Which test YAML files to search
2. **Test level**: Which specific tests within those files to consider

## Options

### File Pattern Selection

#### `--file <pattern>`
Specifies a single glob pattern for test files to include.

**Example**: `cycodt list --file tests/unit/*.yaml`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 103-108

**Data flow**:
1. Parser validates and adds to `TestBaseCommand.Globs`
2. `TestBaseCommand.FindTestFiles()` uses globs to find matching files
3. Default pattern is `**/*.yaml` in test directory if no globs specified

#### `--files <pattern> [<pattern>...]`
Specifies multiple glob patterns for test files.

**Example**: `cycodt list --files tests/unit/*.yaml tests/integration/*.yaml`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 110-115

### File Pattern Exclusion

#### `--exclude-files <pattern> [<pattern>...]`
#### `--exclude <pattern> [<pattern>...]`
Excludes files matching glob or regex patterns.

**Example**: `cycodt list --exclude **/skip-*.yaml --exclude temp`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 117-123

**Data flow**:
1. Parser calls `ValidateExcludeRegExAndGlobPatterns()`
2. Patterns with `/` or `\` → added to `ExcludeGlobs`
3. Patterns without path separators → converted to regex, added to `ExcludeFileNamePatternList`
4. `FileHelpers.FindMatchingFiles()` applies both glob and regex exclusions

### Test Name Selection

#### `--test <name>`
Specifies a single test name to include.

**Example**: `cycodt list --test "my test name"`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 125-130

**Data flow**: Added to `TestBaseCommand.Tests` list

#### `--tests <name> [<name>...]`
Specifies multiple test names.

**Example**: `cycodt list --tests "test 1" "test 2" "test 3"`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 132-137

### Content-Based Test Selection

#### `--contains <pattern> [<pattern>...]`
Includes tests whose names contain the specified pattern(s).

**Example**: `cycodt list --contains async --contains Task`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 139-144

**Data flow**:
1. Added to `TestBaseCommand.Contains` list
2. `GetTestFilters()` converts to `+pattern` format
3. `YamlTestCaseFilter.FilterTestCases()` applies as inclusion filter

#### `--remove <pattern> [<pattern>...]`
Excludes tests whose names contain the specified pattern(s).

**Example**: `cycodt list --remove skip --remove broken`

**Parser location**: `CycoDtCommandLineOptions.cs` lines 146-151

**Data flow**:
1. Added to `TestBaseCommand.Remove` list
2. `GetTestFilters()` converts to `-pattern` format
3. `YamlTestCaseFilter.FilterTestCases()` applies as exclusion filter

### Optional Test Inclusion

#### `--include-optional [<category>...]`
Includes optional tests, optionally filtered by category.

**Examples**:
- `cycodt list --include-optional` - include ALL optional tests
- `cycodt list --include-optional broken-test` - include only "broken-test" category
- `cycodt list --include-optional broken-test wip` - include multiple categories

**Parser location**: `CycoDtCommandLineOptions.cs` lines 153-160

**Special handling**:
- No args → adds empty string (meaning "include all optional")
- With args → adds specific category names

**Data flow**:
1. Added to `TestBaseCommand.IncludeOptionalCategories` list
2. `FilterOptionalTests()` in TestBaseCommand:
   - If list contains empty string → include ALL optional tests
   - If list is empty → exclude ALL optional tests
   - Otherwise → include only tests with matching optional categories

### `.cycodtignore` File Support

**Automatic loading**: If `.cycodtignore` exists in current directory or parent directories, its patterns are automatically loaded.

**Location**: `TestBaseCommand.Validate()` lines 36-42

**Format**: Same as `.gitignore` (glob patterns and regex patterns)

**Data flow**:
1. `FileHelpers.FindFileSearchParents(".cycodtignore")` locates file
2. `FileHelpers.ReadIgnoreFile()` parses patterns
3. Patterns added to `ExcludeGlobs` and `ExcludeFileNamePatternList`

### Default Behavior

If **no file patterns** are specified (`Globs.Count == 0`):
- Default pattern: `{TestDirectory}/**/*.yaml`
- Test directory determined by `YamlTestConfigHelpers.GetTestDirectory()`

**Location**: `TestBaseCommand.FindTestFiles()` lines 65-70

## Target Selection Pipeline Flow

```
1. Parse command-line options → populate Globs, ExcludeGlobs, etc.
2. Validate command → load .cycodtignore if exists
3. FindTestFiles():
   - Apply default glob if none specified
   - Use FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
   - Returns List<FileInfo>
4. GetTestsFromFile() for each file:
   - YamlTestFramework.GetTestsFromYaml() parses YAML
   - Returns IEnumerable<TestCase>
5. FilterOptionalTests():
   - Applies --include-optional logic
   - Returns filtered test cases
6. YamlTestCaseFilter.FilterTestCases():
   - Applies --test, --contains, --remove filters
   - Returns final filtered list
```

## Key Architectural Points

### Two-Level Filtering
Target selection operates at **two distinct levels**:
1. **File-level**: Glob patterns, exclusions, `.cycodtignore`
2. **Test-level**: Test names, contains/remove patterns, optional categories

This is different from other CLIs (like cycodmd) where target selection is primarily file-level only.

### Optional Test Handling
Optional tests (marked with `optional: <category>` in YAML) are:
- **Excluded by default** (prevents broken/experimental tests from running)
- **Included selectively** via `--include-optional <category>`
- **Included entirely** via `--include-optional` (no args)

### Filter Syntax
Test filters use prefix notation:
- No prefix → exact name match
- `+pattern` → inclusion filter (contains pattern)
- `-pattern` → exclusion filter (remove pattern)

See [Layer 1 Proof](layer-1-proof.md) for detailed source code evidence.
