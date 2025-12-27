# cycodt `run` Command - Layer 1: TARGET SELECTION

## Purpose

Layer 1 (TARGET SELECTION) determines **what to search** - the primary search space for the run command. For `cycodt run`, this means selecting which test files to examine for executing tests.

## Command Overview

```bash
cycodt run [options]
```

The `run` command finds and executes tests from YAML test files, generating test result reports.

## Inheritance

The `run` command inherits from `TestBaseCommand`, which means it shares **identical Layer 1 behavior** with the `list` command.

```
Command (base)
    ↓
TestBaseCommand (shared implementation)
    ↓
TestRunCommand (run command)
```

## Layer 1 Features

### Shared with `list` Command

Since `TestRunCommand` extends `TestBaseCommand`, all Layer 1 features are **identical** to the `list` command:

1. **Test File Specification** via `--file` / `--files`
2. **Test File Exclusion** via `--exclude` / `--exclude-files`
3. **`.cycodtignore` File** automatic loading
4. **Test Directory Configuration** via `.cycod.yaml` / `.cycod-defaults.yaml`
5. **Default Pattern Application** when no files specified

### Detailed Feature Documentation

For complete details on each feature, see:
- [cycodt `list` Command - Layer 1](cycodt-list-filtering-pipeline-catalog-layer-1.md)

### Quick Reference

#### Options
- `--file <pattern>`: Specify a single test file pattern (glob)
- `--files <pattern1> <pattern2> ...`: Specify multiple test file patterns
- `--exclude <pattern1> <pattern2> ...`: Exclude files matching patterns
- `--exclude-files <pattern1> <pattern2> ...`: Same as `--exclude`

#### Default Behavior
When no `--file` or `--files` option is provided:
1. Look for test directory configuration
2. Apply default glob pattern: `**/*.yaml` in test directory

#### Examples
```bash
# Run all tests (default)
cycodt run

# Run tests from specific file
cycodt run --file tests/unit/auth-tests.yaml

# Run tests from multiple patterns
cycodt run --files tests/unit/*.yaml tests/integration/*.yaml

# Exclude backup files
cycodt run --exclude "*.backup"

# Exclude temporary directories
cycodt run --exclude "**/temp/**"
```

## Differences from `list` Command

### Layer 1 Differences: NONE

The `run` command has **zero differences** in Layer 1 (TARGET SELECTION) compared to `list`. Both commands:
- Use the same base class (`TestBaseCommand`)
- Call the same `FindTestFiles()` method
- Apply the same default patterns
- Load the same `.cycodtignore` file
- Use the same test directory configuration

### Differences in Other Layers

The `run` command differs from `list` in **Layers 7 and 9**:
- **Layer 7 (Output Persistence)**: `run` generates test reports (`--output-file`, `--output-format`)
- **Layer 9 (Actions on Results)**: `run` executes tests, `list` only displays them

## Implementation Details

### Processing Order

**Identical to `list` command:**

1. **Command Line Parsing**
   - Parse `--file` / `--files` options
   - Parse `--exclude` / `--exclude-files` options

2. **Validation Phase**
   - Load `.cycodtignore` file

3. **Test File Discovery**
   - Apply default pattern if needed
   - Find matching files

4. **Test Extraction**
   - Parse YAML files
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
       ↓
[Execute Tests] ← Only difference: run executes, list just displays
```

## Source Code References

### Key Files

1. **Command Line Parsing**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
   - Lines 94-187: `TryParseTestCommandOptions()` method

2. **Validation**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
   - Lines 34-45: `Validate()` method (loads `.cycodtignore`)

3. **Test File Discovery**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
   - Lines 63-78: `FindTestFiles()` method

4. **Test Directory Config**: `src/cycodt/TestFramework/YamlTestConfigHelpers.cs`
   - Lines 26-64: `GetTestDirectory()` method

5. **Run Command Entry Point**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`
   - Lines 26-49: `ExecuteTestRun()` method
   - **Line 32**: Calls `FindAndFilterTests()` (inherited from `TestBaseCommand`)

### Proof Document

See [Layer 1 Proof Document](cycodt-run-filtering-pipeline-catalog-layer-1-proof.md) for:
- Line-by-line code references (identical to `list` proof)
- Call stack details
- Data structure definitions
- Additional `run`-specific execution flow

## Related Layers

- **Layer 2 (Container Filter)**: Further filters which test files to include based on content
- **Layer 3 (Content Filter)**: Selects specific tests within files
- **Layer 4 (Content Removal)**: Removes tests matching exclusion patterns
- **Layer 7 (Output Persistence)**: Generates test reports (`.trx` or `.xml` files)
- **Layer 9 (Actions on Results)**: Executes the selected tests

## Summary

The `run` command's Layer 1 (TARGET SELECTION) is **100% identical** to the `list` command because both inherit from `TestBaseCommand`. The same mechanisms apply:

1. **Explicit specification** via `--file` / `--files`
2. **Exclusion patterns** via `--exclude` / `--exclude-files`
3. **Ignore file** via `.cycodtignore`
4. **Test directory config** via `.cycod.yaml` / `.cycod-defaults.yaml`
5. **Smart defaults** when no options provided

The only difference is what happens **after** Layer 1 completes:
- `list` displays the test names
- `run` executes the tests and generates reports

This demonstrates excellent code reuse through inheritance - the complex file discovery logic is written once in `TestBaseCommand` and shared by both commands.
