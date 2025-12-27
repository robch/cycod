# cycodt CLI Filtering Pipeline Catalog

## Overview

This document provides a comprehensive catalog of how the **cycodt** (Cycod Test Framework) CLI implements the 9-layer filtering pipeline pattern. Each command type has detailed documentation for all 9 layers with source code evidence.

## Commands in cycodt CLI

The cycodt CLI has **4 main commands**:

1. **`list`** - List tests from test files
2. **`run`** - Execute tests and generate reports
3. **`expect check`** - Check if input matches expectations (regex patterns and/or AI instructions)
4. **`expect format`** - Format text into regex patterns suitable for test expectations

## The 9-Layer Filtering Pipeline

Each command implements a subset of these conceptual layers:

```
Layer 1: TARGET SELECTION    → What to search (test files, input sources)
Layer 2: CONTAINER FILTER    → Which containers to include/exclude (test files)
Layer 3: CONTENT FILTER      → What content within containers to show (specific tests)
Layer 4: CONTENT REMOVAL     → What content to actively remove from display (tests to exclude)
Layer 5: CONTEXT EXPANSION   → How to expand around matches (N/A for cycodt currently)
Layer 6: DISPLAY CONTROL     → How to present results (formatting, verbosity)
Layer 7: OUTPUT PERSISTENCE  → Where to save results (test reports, formatted output)
Layer 8: AI PROCESSING       → AI-assisted analysis of results (expect check instructions)
Layer 9: ACTIONS ON RESULTS  → What to do with results (execute tests, format expectations)
```

## Documentation Structure

For each command, there are 9 layer documentation files and 9 corresponding proof files:

### Command: `list`
- [Layer 1: Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 2: Container Filter](cycodt-list-filtering-pipeline-catalog-layer-2.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-2-proof.md)
- [Layer 3: Content Filter](cycodt-list-filtering-pipeline-catalog-layer-3.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 4: Content Removal](cycodt-list-filtering-pipeline-catalog-layer-4.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodt-list-filtering-pipeline-catalog-layer-5.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-5-proof.md)
- [Layer 6: Display Control](cycodt-list-filtering-pipeline-catalog-layer-6.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodt-list-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-7-proof.md)
- [Layer 8: AI Processing](cycodt-list-filtering-pipeline-catalog-layer-8.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodt-list-filtering-pipeline-catalog-layer-9.md) | [Proof](cycodt-list-filtering-pipeline-catalog-layer-9-proof.md)

### Command: `run`
- [Layer 1: Target Selection](cycodt-run-filtering-pipeline-catalog-layer-1.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 2: Container Filter](cycodt-run-filtering-pipeline-catalog-layer-2.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-2-proof.md)
- [Layer 3: Content Filter](cycodt-run-filtering-pipeline-catalog-layer-3.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 4: Content Removal](cycodt-run-filtering-pipeline-catalog-layer-4.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodt-run-filtering-pipeline-catalog-layer-5.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-5-proof.md)
- [Layer 6: Display Control](cycodt-run-filtering-pipeline-catalog-layer-6.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodt-run-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-7-proof.md)
- [Layer 8: AI Processing](cycodt-run-filtering-pipeline-catalog-layer-8.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodt-run-filtering-pipeline-catalog-layer-9.md) | [Proof](cycodt-run-filtering-pipeline-catalog-layer-9-proof.md)

### Command: `expect check`
- [Layer 1: Target Selection](cycodt-expect-check-filtering-pipeline-catalog-layer-1.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 2: Container Filter](cycodt-expect-check-filtering-pipeline-catalog-layer-2.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-2-proof.md)
- [Layer 3: Content Filter](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 4: Content Removal](cycodt-expect-check-filtering-pipeline-catalog-layer-4.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodt-expect-check-filtering-pipeline-catalog-layer-5.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-5-proof.md)
- [Layer 6: Display Control](cycodt-expect-check-filtering-pipeline-catalog-layer-6.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodt-expect-check-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-7-proof.md)
- [Layer 8: AI Processing](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodt-expect-check-filtering-pipeline-catalog-layer-9.md) | [Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-9-proof.md)

### Command: `expect format`
- [Layer 1: Target Selection](cycodt-expect-format-filtering-pipeline-catalog-layer-1.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-1-proof.md)
- [Layer 2: Container Filter](cycodt-expect-format-filtering-pipeline-catalog-layer-2.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-2-proof.md)
- [Layer 3: Content Filter](cycodt-expect-format-filtering-pipeline-catalog-layer-3.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md)
- [Layer 4: Content Removal](cycodt-expect-format-filtering-pipeline-catalog-layer-4.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodt-expect-format-filtering-pipeline-catalog-layer-5.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-5-proof.md)
- [Layer 6: Display Control](cycodt-expect-format-filtering-pipeline-catalog-layer-6.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-7-proof.md)
- [Layer 8: AI Processing](cycodt-expect-format-filtering-pipeline-catalog-layer-8.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodt-expect-format-filtering-pipeline-catalog-layer-9.md) | [Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-9-proof.md)

## Command Inheritance Structure

```
Command (base)
├── TestBaseCommand
│   ├── TestListCommand (list)
│   └── TestRunCommand (run)
└── ExpectBaseCommand
    ├── ExpectCheckCommand (expect check)
    └── ExpectFormatCommand (expect format)
```

## Shared Options

Some options are shared across commands through base classes:

### Shared by All Commands (via Command base class)
- Global options: `--verbose`, `--quiet`, `--debug`, `--help`, `--version`
- Configuration: `--config`, `--profile`
- Working directory: `--working-dir`, `--folder`, `--dir`, `--cwd`
- Logging: `--log`

### Shared by Test Commands (via TestBaseCommand)
- `--file`, `--files`: Test file patterns (glob)
- `--exclude-files`, `--exclude`: Exclude file patterns
- `--test`, `--tests`: Specific test names
- `--contains`: Include tests containing pattern
- `--remove`: Exclude tests matching pattern
- `--include-optional`: Include optional test categories

### Shared by Expect Commands (via ExpectBaseCommand)
- `--input`: Input source (file or stdin)
- `--save-output`, `--output`: Output destination

## Usage Patterns

### List Command
```bash
# List all tests
cycodt list

# List tests from specific files
cycodt list --file tests/unit/*.yaml

# List tests containing "authentication"
cycodt list --contains authentication

# List including optional tests
cycodt list --include-optional broken-test
```

### Run Command
```bash
# Run all tests
cycodt run

# Run specific test
cycodt run --test "my test name"

# Run with custom output
cycodt run --output-file results.trx --output-format trx
```

### Expect Check Command
```bash
# Check stdin against regex patterns
command-output | cycodt expect check --regex "expected.*pattern"

# Check file with AI instructions
cycodt expect check --input output.txt --instructions "Verify the output is valid JSON"
```

### Expect Format Command
```bash
# Format command output to regex patterns
command-output | cycodt expect format > patterns.txt

# Non-strict formatting
cycodt expect format --input output.txt --strict false
```

## Notes

- **Layer 5 (Context Expansion)** is not currently implemented in cycodt
- **Layer 8 (AI Processing)** is only used in `expect check` command
- Test commands share most filtering logic through `TestBaseCommand`
- Expect commands have simpler filtering (just input source selection)
