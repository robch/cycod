# cycodt CLI Filter Pipeline Catalog

## Overview

This document catalogs the **layered filtering patterns** for the `cycodt` CLI tool, which is a test framework for YAML-defined tests. The catalog follows the 9-layer conceptual pipeline model and provides detailed evidence from source code.

## Commands in cycodt CLI

The cycodt CLI has 4 main commands:

1. **[list](cycodt-list-filter-pipeline-catalog-README.md)** - List tests matching criteria
2. **[run](cycodt-run-filter-pipeline-catalog-README.md)** - Run tests matching criteria
3. **[expect-check](cycodt-expect-check-filter-pipeline-catalog-README.md)** - Check expectations against input
4. **[expect-format](cycodt-expect-format-filter-pipeline-catalog-README.md)** - Format expectations for use in tests

## The 9-Layer Pipeline

Each command is analyzed across 9 conceptual layers:

1. **TARGET SELECTION** - What to search/process (test files, input data)
2. **CONTAINER FILTER** - Which containers to include/exclude (test files, test cases)
3. **CONTENT FILTER** - What content within containers to show (specific tests)
4. **CONTENT REMOVAL** - What content to actively remove from results
5. **CONTEXT EXPANSION** - How to expand around matches (not applicable to test framework)
6. **DISPLAY CONTROL** - How to present results (formatting, verbosity)
7. **OUTPUT PERSISTENCE** - Where to save results (test reports, formatted output)
8. **AI PROCESSING** - AI-assisted analysis (only in expect check)
9. **ACTIONS ON RESULTS** - What to do with results (execute tests, format output)

## Command Comparison Matrix

| Layer | list | run | expect-check | expect-format |
|-------|------|-----|--------------|---------------|
| 1. Target Selection | ✅ Test files | ✅ Test files | ✅ Input file/stdin | ✅ Input file/stdin |
| 2. Container Filter | ✅ Test files | ✅ Test files | ❌ N/A | ❌ N/A |
| 3. Content Filter | ✅ Test names | ✅ Test names | ✅ Regex patterns | ❌ N/A |
| 4. Content Removal | ✅ Remove tests | ✅ Remove tests | ✅ Not-regex patterns | ❌ N/A |
| 5. Context Expansion | ❌ N/A | ❌ N/A | ❌ N/A | ❌ N/A |
| 6. Display Control | ✅ Verbose mode | ✅ Test output | ✅ Pass/fail | ✅ Formatted output |
| 7. Output Persistence | ❌ Console only | ✅ Test reports | ✅ Optional file | ✅ Optional file |
| 8. AI Processing | ❌ N/A | ❌ N/A | ✅ Instructions | ❌ N/A |
| 9. Actions on Results | ❌ Display only | ✅ Execute tests | ✅ Validate | ✅ Format |

## Source Code Structure

### Command Line Parsing
- **Main Parser**: [`src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`](../src/cycodt/CommandLine/CycoDtCommandLineOptions.cs)
  - Lines 10-80: Command option parsing methods

### Command Implementations
- **Base Commands**:
  - [`src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`](../src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs) - Base for list/run
  - [`src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`](../src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs) - Base for expect commands

- **Concrete Commands**:
  - [`src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`](../src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs)
  - [`src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`](../src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs)
  - [`src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`](../src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs)
  - [`src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`](../src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs)

### Test Framework Components
- **Filtering**: [`src/cycodt/TestFramework/YamlTestCaseFilter.cs`](../src/cycodt/TestFramework/YamlTestCaseFilter.cs)
- **Test Discovery**: [`src/cycodt/TestFramework/YamlTestFramework.cs`](../src/cycodt/TestFramework/YamlTestFramework.cs)
- **Test Execution**: [`src/cycodt/TestFramework/YamlTestCaseRunner.cs`](../src/cycodt/TestFramework/YamlTestCaseRunner.cs)

## Key Observations

### Shared Infrastructure (list & run commands)
- Both `list` and `run` inherit from `TestBaseCommand`
- They share the same filtering pipeline (Layers 1-4)
- Differences are primarily in Layers 6-9 (display, output, actions)

### Separate Infrastructure (expect commands)
- `expect check` and `expect format` inherit from `ExpectBaseCommand`
- They operate on raw input/output (files or stdin/stdout)
- No concept of "test discovery" - they process single inputs
- Limited filtering - only content-level validation (expect check)

### Optional Test Handling
- Tests can be marked as "optional" with categories
- `--include-optional` enables category-based inclusion
- Complex chain repair logic maintains test execution order when optional tests are excluded

## Navigation

- [Full Catalog Index](CLI-Filtering-Patterns-Catalog.md) - Cross-tool comparison
- Individual command documentation (linked above)

## Layer 6 Documentation (Display Control)

Layer 6 documentation with detailed proof is now available for all commands:

### list Command
- **Layer 6 Catalog**: [cycodt-list-filtering-pipeline-catalog-layer-6.md](./cycodt-list-filtering-pipeline-catalog-layer-6.md)
- **Layer 6 Proof**: [cycodt-list-filtering-pipeline-catalog-layer-6-proof.md](./cycodt-list-filtering-pipeline-catalog-layer-6-proof.md)

### run Command
- **Layer 6 Catalog**: [cycodt-run-filtering-pipeline-catalog-layer-6.md](./cycodt-run-filtering-pipeline-catalog-layer-6.md)
- **Layer 6 Proof**: [cycodt-run-filtering-pipeline-catalog-layer-6-proof.md](./cycodt-run-filtering-pipeline-catalog-layer-6-proof.md)

### expect check Command
- **Layer 6 Catalog**: [cycodt-expect-check-filtering-pipeline-catalog-layer-6.md](./cycodt-expect-check-filtering-pipeline-catalog-layer-6.md)
- **Layer 6 Proof**: [cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md)

### expect format Command
- **Layer 6 Catalog**: [cycodt-expect-format-filtering-pipeline-catalog-layer-6.md](./cycodt-expect-format-filtering-pipeline-catalog-layer-6.md)
- **Layer 6 Proof**: [cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md)

