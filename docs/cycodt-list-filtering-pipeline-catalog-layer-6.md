# cycodt list - Layer 6: Display Control

## Overview

Layer 6 controls **how test results are presented** to the user. For the `list` command, this layer determines the format and verbosity of the test list output.

## Command

```bash
cycodt list [options]
```

## Layer 6 Features

### 6.1 Verbose Mode

**Option**: `--verbose` (inherited from `CommandLineOptions`)

**Purpose**: Controls whether to group tests by file or show a flat list

**Behavior**:
- **Default (non-verbose)**: Shows flat list of fully qualified test names
- **Verbose mode**: Groups tests by source file with visual hierarchy

**Source Code**: See [Layer 6 Proof](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md#61-verbose-mode)

### 6.2 Test Count Display

**Feature**: Automatic count display

**Purpose**: Shows total number of tests found

**Behavior**:
- Displays singular message for 1 test: "Found 1 test..."
- Displays plural message for multiple tests: "Found N tests..."

**Source Code**: See [Layer 6 Proof](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md#62-test-count-display)

### 6.3 Color Coding

**Feature**: Console color support

**Purpose**: Visual differentiation of output elements

**Behavior**:
- Test names and file paths: `ConsoleColor.DarkGray`
- Count message: Default console color

**Source Code**: See [Layer 6 Proof](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md#63-color-coding)

### 6.4 Quiet Mode

**Option**: `--quiet` (inherited from `CommandLineOptions`)

**Purpose**: Suppress non-essential output

**Behavior**:
- Controlled by `ConsoleHelpers` infrastructure
- Not explicitly handled in TestListCommand
- Affects helper methods throughout the codebase

**Source Code**: See [Layer 6 Proof](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md#64-quiet-mode)

### 6.5 Debug Mode

**Option**: `--debug` (inherited from `CommandLineOptions`)

**Purpose**: Show diagnostic information

**Behavior**:
- Enables debug logging throughout test discovery and filtering
- Shows detailed pattern matching and file discovery steps
- Controlled by `ConsoleHelpers.ConfigureDebug(true)`

**Source Code**: See [Layer 6 Proof](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md#65-debug-mode)

## Display Format Examples

### Non-Verbose Mode (Default)

```
TestSuite1.TestCase1
TestSuite1.TestCase2
TestSuite2.TestCase3
TestSuite2.TestCase4

Found 4 tests...
```

### Verbose Mode (--verbose)

```
/path/to/tests/suite1.yaml

  TestSuite1.TestCase1
  TestSuite1.TestCase2

/path/to/tests/suite2.yaml

  TestSuite2.TestCase3
  TestSuite2.TestCase4

Found 4 tests...
```

## Data Flow

```
TestListCommand.ExecuteList()
    ↓
FindAndFilterTests() [from TestBaseCommand]
    ↓
if (ConsoleHelpers.IsVerbose())
    ↓
    Group tests by file
    ↓
    For each group:
        Print file path (DarkGray)
        Print indented test names (DarkGray)
else
    ↓
    For each test:
        Print fully qualified name (DarkGray)
    ↓
Print count message
```

## Related Layers

- **Layer 1 (Target Selection)**: Determines which tests to discover
- **Layer 2 (Container Filter)**: Filters which test files to process
- **Layer 3 (Content Filter)**: Filters which tests to include in output
- **Layer 4 (Content Removal)**: Removes tests matching exclusion patterns

## Implementation Notes

1. **Verbose detection**: Uses `ConsoleHelpers.IsVerbose()` which checks the `Verbose` property set by `--verbose` option
2. **No custom formatting options**: Unlike some other CLI tools, cycodt list does not support custom output formats (JSON, CSV, etc.)
3. **Grouping is file-based**: Verbose mode groups by `CodeFilePath` property of test cases
4. **Deterministic ordering**: Groups are sorted by file path, tests within groups maintain discovery order

## Proof Document

For detailed source code evidence with line numbers and full implementation details, see:
- [Layer 6 Proof Document](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md)
