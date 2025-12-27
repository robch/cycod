# cycodt list - Filter Pipeline Catalog

## Command Overview

The `list` command displays tests matching specified criteria without executing them. It discovers YAML test files, filters test cases, and displays their names.

**Command Syntax**: `cycodt list [options]`

## Source Code

- **Command Implementation**: [`src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`](../../src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs)
- **Base Command**: [`src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`](../../src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs)
- **Parser**: [`src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`](../../src/cycodt/CommandLine/CycoDtCommandLineOptions.cs) - Lines 42-78

## The 9-Layer Pipeline for `list`

### Layer 1: [Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md)
**What test files to discover**
- Positional args: glob patterns
- `--file`, `--files`: explicit test file patterns
- `--exclude-files`, `--exclude`: patterns to exclude
- `.cycodtignore` file support
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-1-proof.md)

### Layer 2: [Container Filtering](cycodt-list-filtering-pipeline-catalog-layer-2.md)
**Which test files to include/exclude**
- File-level exclusion via globs/regex
- Time-based filtering: NOT IMPLEMENTED
- Content-based filtering: NOT IMPLEMENTED (happens at Layer 3)
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-2-proof.md)

### Layer 3: [Content Filtering](cycodt-list-filtering-pipeline-catalog-layer-3.md)
**Which tests within files to show**
- `--test`, `--tests`: specific test names
- `--contains`: tests containing pattern (must-match ANY)
- `--include-optional`: include optional test categories
- Filter syntax: space-separated phrases, `+` prefix for AND logic
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md)

### Layer 4: [Content Removal](cycodt-list-filtering-pipeline-catalog-layer-4.md) ⭐ **CURRENT FOCUS**
**Which tests to actively exclude**
- `--remove`: tests matching pattern (must-NOT-match)
- Filter syntax: `-` prefix for exclusion
- Optional test exclusion (default behavior without `--include-optional`)
- Test chain repair when optional tests are excluded
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-4-proof.md)

### Layer 5: [Context Expansion](cycodt-list-filtering-pipeline-catalog-layer-5.md)
**Context around matches**
- NOT APPLICABLE for test listing
- Tests are discrete units with no "surrounding context"
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-5-proof.md)

### Layer 6: [Display Control](cycodt-list-filtering-pipeline-catalog-layer-6.md)
**How results are presented**
- `--verbose`: group by file and show file paths
- Default: show fully-qualified test names only
- Color coding: DarkGray for test names
- Summary count: "Found N test(s)..."
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-6-proof.md)

### Layer 7: [Output Persistence](cycodt-list-filtering-pipeline-catalog-layer-7.md)
**Where results are saved**
- Console output ONLY
- No file output options
- No redirection support
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-7-proof.md)

### Layer 8: [AI Processing](cycodt-list-filtering-pipeline-catalog-layer-8.md)
**AI-assisted analysis**
- NOT IMPLEMENTED
- No AI processing for test listing
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-8-proof.md)

### Layer 9: [Actions on Results](cycodt-list-filtering-pipeline-catalog-layer-9.md)
**Operations on matched tests**
- DISPLAY ONLY - no execution or modification
- Read-only operation
- [📄 Proof](cycodt-list-filtering-pipeline-catalog-layer-9-proof.md)

## Example Usage

```bash
# List all tests
cycodt list

# List tests in specific file
cycodt list --file tests/my-tests.yaml

# List tests matching pattern
cycodt list --contains "login"

# List tests with AND logic
cycodt list +api +auth

# List tests excluding pattern
cycodt list --remove "skip"

# Verbose output grouped by file
cycodt list --verbose

# Complex filtering
cycodt list --file "tests/*.yaml" --contains "api" --remove "slow" --include-optional "nightly"
```

## Key Characteristics

- **Read-only**: No side effects on files or tests
- **Fast**: No test execution, just discovery and filtering
- **Debugging**: Useful for understanding what `run` will execute
- **Filtering**: Shares exact filtering logic with `run` command
