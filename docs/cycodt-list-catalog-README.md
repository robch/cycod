# cycodt `list` Command - Filtering Pipeline Catalog

## Overview

The `list` command displays tests found in YAML test files. It provides filtering and grouping options to help users navigate test suites.

**Command**: `cycodt list [options]`

## Purpose

List all tests (or filtered subset) from YAML test files in the current directory tree.

## Layer Implementation Summary

| Layer | Implemented | Key Features |
|-------|-------------|--------------|
| 1. TARGET SELECTION | ✅ Yes | `--file`, `--files`, `--exclude`, `--test`, `--tests`, `--contains`, `--remove`, `--include-optional` |
| 2. CONTAINER FILTER | ✅ Yes | Test name filtering, optional category filtering |
| 3. CONTENT FILTER | ❌ No | N/A - lists test names only |
| 4. CONTENT REMOVAL | ❌ No | N/A - minimal output |
| 5. CONTEXT EXPANSION | ❌ No | N/A - displays test names |
| 6. DISPLAY CONTROL | ⚠️ Partial | `--verbose` for grouping by file |
| 7. OUTPUT PERSISTENCE | ⚠️ Limited | Console output only (can redirect stdout) |
| 8. AI PROCESSING | ❌ No | N/A |
| 9. ACTIONS ON RESULTS | ❌ No | Read-only listing |

## Layer Details

- **[Layer 1: Target Selection](cycodt-list-layer-1.md)** - [Proof](cycodt-list-layer-1-proof.md)
- **[Layer 2: Container Filter](cycodt-list-layer-2.md)** - [Proof](cycodt-list-layer-2-proof.md)
- **[Layer 3: Content Filter](cycodt-list-layer-3.md)** - [Proof](cycodt-list-layer-3-proof.md)
- **[Layer 4: Content Removal](cycodt-list-layer-4.md)** - [Proof](cycodt-list-layer-4-proof.md)
- **[Layer 5: Context Expansion](cycodt-list-layer-5.md)** - [Proof](cycodt-list-layer-5-proof.md)
- **[Layer 6: Display Control](cycodt-list-layer-6.md)** - [Proof](cycodt-list-layer-6-proof.md)
- **[Layer 7: Output Persistence](cycodt-list-layer-7.md)** - [Proof](cycodt-list-layer-7-proof.md)
- **[Layer 8: AI Processing](cycodt-list-layer-8.md)** - [Proof](cycodt-list-layer-8-proof.md)
- **[Layer 9: Actions on Results](cycodt-list-layer-9.md)** - [Proof](cycodt-list-layer-9-proof.md)

## Command Line Options

### Target Selection (Layer 1)
- `--file <pattern>` - Single test file pattern
- `--files <pattern1> <pattern2> ...` - Multiple test file patterns
- `--exclude <pattern>` / `--exclude-files <pattern>` - Exclude file patterns
- `--test <name>` - Specific test name
- `--tests <name1> <name2> ...` - Multiple test names
- `--contains <pattern>` - Include tests containing pattern
- `--remove <pattern>` - Remove tests matching pattern
- `--include-optional [category]` - Include optional tests (all or specific category)

### Display Control (Layer 6)
- `--verbose` - Group tests by file with file paths

### Global Options
- `--debug` - Debug output
- `--quiet` - Suppress non-essential output

## Usage Examples

```bash
# List all tests
cycodt list

# List tests from specific file
cycodt list --file tests/my-test.yaml

# List tests matching pattern
cycodt list --contains "database"

# List tests grouped by file
cycodt list --verbose

# List including optional tests
cycodt list --include-optional

# List specific optional category
cycodt list --include-optional broken-test
```

## Source Code

**Primary Implementation**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Supporting Classes**:
- `TestBaseCommand.cs` - Base class with filtering logic
- `CycoDtCommandLineOptions.cs` - Command-line parsing
- `YamlTestFramework.cs` - Test discovery
- `YamlTestCaseFilter.cs` - Test filtering

## Related Commands

- **[run](cycodt-run-catalog-README.md)** - Execute the listed tests
- **[expect check](cycodt-expect-check-catalog-README.md)** - Verify test expectations
- **[expect format](cycodt-expect-format-catalog-README.md)** - Format test expectations
