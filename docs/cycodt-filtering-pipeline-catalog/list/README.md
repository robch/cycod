# cycodt list - Filtering Pipeline

Lists tests matching specified criteria.

## Command Overview

The `list` command finds test files, filters tests based on various criteria, and displays their fully qualified names.

**Basic usage**: `cycodt list [options]`

**Implementation**: `TestListCommand` (inherits from `TestBaseCommand`)

## 9-Layer Pipeline Implementation

1. [**TARGET SELECTION**](layer-1.md) - ([proof](layer-1-proof.md))
   - File patterns via `--file`, `--files`
   - Exclusion patterns via `--exclude-files`, `--exclude`
   - Test name filtering via `--test`, `--tests`
   - Content-based filtering via `--contains`, `--remove`
   - Optional test inclusion via `--include-optional`

2. [**CONTAINER FILTERING**](layer-2.md) - ([proof](layer-2-proof.md))
   - Test file filtering (glob patterns, exclusions)
   - `.cycodtignore` file support
   - Test case filtering by name
   - Optional test category filtering

3. [**CONTENT FILTERING**](layer-3.md) - ([proof](layer-3-proof.md))
   - Test name pattern matching via `--contains`
   - YamlTestCaseFilter applies inclusion filters

4. [**CONTENT REMOVAL**](layer-4.md) - ([proof](layer-4-proof.md))
   - Test exclusion via `--remove`
   - YamlTestCaseFilter applies exclusion filters

5. [**CONTEXT EXPANSION**](layer-5.md) - ([proof](layer-5-proof.md))
   - Not applicable (full test names shown, no partial matching with context)

6. [**DISPLAY CONTROL**](layer-6.md) - ([proof](layer-6-proof.md))
   - `--verbose`: groups tests by file path
   - Default: simple list of test names
   - Count summary at end

7. [**OUTPUT PERSISTENCE**](layer-7.md) - ([proof](layer-7-proof.md))
   - Console output only (no file persistence)

8. [**AI PROCESSING**](layer-8.md) - ([proof](layer-8-proof.md))
   - Not applicable (no AI processing in list command)

9. [**ACTIONS ON RESULTS**](layer-9.md) - ([proof](layer-9-proof.md))
   - Action: Display test names (read-only operation)

## Example Usage

```bash
# List all tests
cycodt list

# List tests in specific file
cycodt list --file tests/my-tests.yaml

# List tests containing "async"
cycodt list --contains async

# List tests, removing those containing "skip"
cycodt list --remove skip

# List with verbose output (grouped by file)
cycodt list --verbose

# List including optional tests in "broken-test" category
cycodt list --include-optional broken-test
```

## Key Source Files

- Parser: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs` (lines 94-187)
- Base command: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
- List implementation: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`
- Test discovery: `src/cycodt/TestFramework/YamlTestFramework.cs`
- Test filtering: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`
