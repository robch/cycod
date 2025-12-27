# cycodt `list` Command - Layer 1: TARGET SELECTION

## Purpose

Layer 1 (TARGET SELECTION) determines **what to search** - the primary search space for the list command. For `cycodt list`, this means selecting which test files to examine for listing tests.

## Command Overview

```bash
cycodt list [options]
```

The `list` command finds and displays tests from YAML test files without executing them.

## Layer 1 Features

### 1. Test File Specification (Glob Patterns)

#### Options
- `--file <pattern>`: Specify a single test file pattern (glob)
- `--files <pattern1> <pattern2> ...`: Specify multiple test file patterns

#### Default Behavior
When no `--file` or `--files` option is provided:
1. Look for test directory configuration in `.cycod.yaml` or `.cycod-defaults.yaml`
2. Use `testDirectory` tag if found
3. Fall back to current directory
4. Apply default glob pattern: `**/*.yaml`

#### Examples
```bash
# List tests from all YAML files (default)
cycodt list

# List tests from specific file
cycodt list --file tests/unit/auth-tests.yaml

# List tests from multiple patterns
cycodt list --files tests/unit/*.yaml tests/integration/*.yaml

# List tests from specific directory
cycodt list --file src/tests/**/*.yaml
```

### 2. Test File Exclusion

#### Options
- `--exclude <pattern1> <pattern2> ...`: Exclude files matching patterns
- `--exclude-files <pattern1> <pattern2> ...`: Same as `--exclude`

#### Pattern Types
Patterns are categorized based on whether they contain path separators:

1. **Filename patterns** (no `/` or `\`):
   - Converted to regex
   - Matched against filename only
   - Example: `*.backup`, `temp*`

2. **Path glob patterns** (contains `/` or `\`):
   - Treated as glob patterns
   - Matched against full file path
   - Example: `**/backup/**`, `temp/*.yaml`

#### Examples
```bash
# Exclude backup files
cycodt list --exclude "*.backup"

# Exclude temporary directories
cycodt list --exclude "**/temp/**"

# Multiple exclusions
cycodt list --exclude "*.backup" "**/temp/**" "draft*"
```

### 3. .cycodtignore File

#### Automatic Loading
The command automatically searches for a `.cycodtignore` file in:
- Current directory
- Parent directories (walking up the tree)

#### File Format
Each line in `.cycodtignore` specifies an exclusion pattern (same format as `--exclude`):
```
# Exclude backup files
*.backup

# Exclude temporary directories
**/temp/**

# Exclude draft tests
draft*
```

#### Behavior
- Patterns from `.cycodtignore` are **added** to patterns from `--exclude` options
- If no `.cycodtignore` is found, only command-line exclusions apply

#### Example
```bash
# With .cycodtignore containing "*.backup"
# This will exclude both *.backup and **/temp/**
cycodt list --exclude "**/temp/**"
```

### 4. Test Directory Configuration

#### Configuration Files
The test directory can be specified in:
1. `.cycod.yaml` (project-specific config)
2. `.cycod-defaults.yaml` (default config)

#### Configuration Format
```yaml
# .cycod.yaml or .cycod-defaults.yaml
testDirectory: tests/cycodt-yaml
```

#### Discovery Process
1. Search current directory and parents for `.cycod.yaml`
2. If found, check for `testDirectory` tag
3. If not found, search for `.cycod-defaults.yaml`
4. If found, check for `testDirectory` tag
5. If still not found, use current directory

#### Relative Paths
The `testDirectory` value is resolved relative to the config file location:
```yaml
# If .cycod.yaml is in /project/
# This resolves to /project/tests/cycodt-yaml
testDirectory: tests/cycodt-yaml
```

#### Example
```bash
# With testDirectory configured as "tests/cycodt-yaml"
# This finds tests in tests/cycodt-yaml/**/*.yaml
cycodt list

# You can still override with explicit patterns
cycodt list --file other-tests/**/*.yaml
```

## Implementation Details

### Processing Order

1. **Command Line Parsing** (lines 103-115 in CycoDtCommandLineOptions.cs)
   - Parse `--file` option → add to `Globs` list
   - Parse `--files` option → add all to `Globs` list
   - Parse `--exclude` / `--exclude-files` → add to `ExcludeGlobs` and `ExcludeFileNamePatternList`

2. **Validation Phase** (lines 34-45 in TestBaseCommand.cs)
   - Search for `.cycodtignore` file
   - Load exclusion patterns from `.cycodtignore`
   - Add to existing exclusion lists

3. **Test File Discovery** (lines 63-78 in TestBaseCommand.cs)
   - If `Globs` is empty, apply default pattern from test directory
   - Call `FileHelpers.FindMatchingFiles()` with globs and exclusions
   - Convert file paths to `FileInfo` objects

4. **Test Extraction** (lines 244-255 in TestBaseCommand.cs)
   - For each file, call `YamlTestFramework.GetTestsFromYaml()`
   - Parse YAML test file
   - Extract test cases

### Data Flow

```
Command Line Args
       ↓
[Parse Options]
       ↓
  Globs List (--file, --files)
  ExcludeGlobs List (--exclude)
  ExcludeFileNamePatternList (--exclude with no path separator)
       ↓
[Validate Command]
       ↓
  + Load .cycodtignore
       ↓
[Find Test Files]
       ↓
  Apply default glob if Globs.Count == 0
       ↓
  FileHelpers.FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
       ↓
  List<FileInfo> (matching files)
       ↓
[Extract Tests from Files]
       ↓
  YamlTestFramework.GetTestsFromYaml()
       ↓
  IEnumerable<TestCase>
```

## Source Code References

See [Layer 1 Proof Document](cycodt-list-filtering-pipeline-catalog-layer-1-proof.md) for:
- Line-by-line code references
- Call stack details
- Data structure definitions
- Complete implementation evidence

## Related Layers

- **Layer 2 (Container Filter)**: Further filters which test files to include based on content
- **Layer 3 (Content Filter)**: Selects specific tests within files
- **Layer 4 (Content Removal)**: Removes tests matching exclusion patterns

## Summary

Layer 1 for `cycodt list` provides multiple mechanisms for selecting test files:
1. **Explicit specification** via `--file` / `--files` with glob patterns
2. **Exclusion patterns** via `--exclude` / `--exclude-files` (glob or regex)
3. **Ignore file** via `.cycodtignore` (automatic discovery)
4. **Test directory config** via `.cycod.yaml` / `.cycod-defaults.yaml`
5. **Smart defaults** when no options provided (test directory + `**/*.yaml`)

This creates a flexible, convention-over-configuration approach where tests "just work" with zero configuration, but can be precisely controlled when needed.
