# cycodt `expect format` Command - Layer 1: TARGET SELECTION

## Purpose

Layer 1 (TARGET SELECTION) determines **what to search** - the primary search space for the expect format command. For `cycodt expect format`, this means selecting the input source to format into regex patterns.

## Command Overview

```bash
cycodt expect format [options]
```

The `expect format` command transforms input text into regex patterns suitable for use in test expectations, escaping special characters and adding regex anchors.

## Inheritance

The `expect format` command inherits from `ExpectBaseCommand`, which means it shares **identical Layer 1 behavior** with the `expect check` command.

```
Command (base)
    ↓
ExpectBaseCommand (shared implementation)
    ↓
ExpectFormatCommand (expect format command)
```

## Layer 1 Features

### Shared with `expect check` Command

Since `ExpectFormatCommand` extends `ExpectBaseCommand`, all Layer 1 features are **identical** to the `expect check` command:

1. **Input Source Specification** via `--input`
2. **Stdin Support (Implicit)** when stdin is redirected
3. **No File Discovery** or glob patterns
4. **Empty Command Detection** for help display
5. **Stdin Auto-Detection** during validation

### Detailed Feature Documentation

For complete details on each feature, see:
- [cycodt `expect check` Command - Layer 1](cycodt-expect-check-filtering-pipeline-catalog-layer-1.md)

### Quick Reference

#### Option
- `--input <file>`: Specify input file to format

#### Stdin Support
When `--input` is not provided AND stdin is redirected:
- Automatically uses stdin as input source

#### Examples
```bash
# Format explicit file
cycodt expect format --input output.txt

# Format stdin (implicit)
command-output | cycodt expect format

# Format stdin (explicit)
command-output | cycodt expect format --input -

# Save formatted output
cycodt expect format --input output.txt --save-output patterns.txt
```

## Differences from `expect check` Command

### Layer 1 Differences: NONE

The `expect format` command has **zero differences** in Layer 1 (TARGET SELECTION) compared to `expect check`. Both commands:
- Use the same base class (`ExpectBaseCommand`)
- Have the same `Input` property
- Apply the same stdin auto-detection logic
- Support only single input streams (no multi-file)

### Differences in Other Layers

The `expect format` command differs from `expect check` in:
- **Layer 6 (Display Control)**: `format` has `--strict` option for strict/non-strict regex formatting
- **Layer 9 (Actions on Results)**: `format` transforms input into regex patterns, `check` validates input against patterns

## Implementation Details

### Processing Order

**Identical to `expect check` command:**

1. **Command Line Parsing**
   - Parse `--input` option

2. **Validation Phase**
   - Auto-detect stdin if no input specified

3. **Execution Phase**
   - Read input from file or stdin
   - Transform input (difference from `check`)

### Data Flow

```
Command Line Args
       ↓
[Parse Options]
       ↓
  Input: string? (null, "-", or file path)
       ↓
[Validate Command]
       ↓
  if Input is null AND stdin redirected:
      Input = "-"
       ↓
[Execute Format]
       ↓
  Read input:
      if Input == "-": read stdin
      else: read file at Input path
       ↓
  text: string (input content)
       ↓
[Format Input] ← Only difference: format instead of check
       ↓
  formatted: string (regex patterns)
       ↓
[Output] (stdout or --save-output file)
```

## Source Code References

### Key Files

1. **Command Definition**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`
   - Lines 4-5: Inheritance from `ExpectBaseCommand`

2. **Base Command**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`
   - Lines 12-17: `IsEmpty()` method (empty detection)
   - Lines 19-29: `Validate()` method (stdin auto-detection)

3. **Command Line Parsing**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
   - Lines 32-92: `TryParseExpectCommandOptions()` method

4. **Format Command Entry Point**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`
   - Lines 23-37: `ExecuteFormat()` method
   - **Line 27**: Calls `FileHelpers.ReadAllText(Input!)` (inherited property)

### Proof Document

See [Layer 1 Proof Document](cycodt-expect-format-filtering-pipeline-catalog-layer-1-proof.md) for:
- Line-by-line code references (identical to `expect check` proof for Layer 1)
- Call stack details
- Data structure definitions
- Additional `format`-specific execution flow

## Related Layers

- **Layer 6 (Display Control)**: `--strict` option controls formatting style
- **Layer 7 (Output Persistence)**: `--save-output` saves formatted patterns
- **Layer 9 (Actions on Results)**: Transforms input into regex patterns

## Usage Patterns

### File Input
```bash
# Format file
cycodt expect format --input output.txt

# Format file with non-strict mode
cycodt expect format --input output.txt --strict false

# Format file and save
cycodt expect format --input output.txt --save-output patterns.txt
```

### Stdin Input (Implicit)
```bash
# Pipe command output
command-to-test | cycodt expect format

# Heredoc
cycodt expect format << EOF
test data
more test data
EOF

# Shell redirection
cycodt expect format < output.txt
```

### Stdin Input (Explicit)
```bash
# Explicitly specify stdin
command-to-test | cycodt expect format --input -
```

### Chaining with `expect check`
```bash
# Generate patterns from good output, check later
command-to-test | cycodt expect format > expected-patterns.txt

# Later, check new output against patterns
command-to-test | cycodt expect check --regex "$(cat expected-patterns.txt)"
```

## Summary

The `expect format` command's Layer 1 (TARGET SELECTION) is **100% identical** to the `expect check` command because both inherit from `ExpectBaseCommand`. The same mechanisms apply:

1. **Single input stream** specification via `--input`
2. **Stdin-first design** for pipe-friendly operation
3. **Auto-detection** of stdin when available
4. **No glob patterns** or multi-file support
5. **No exclusion mechanisms**

The only difference is what happens **after** Layer 1 completes:
- `expect check` validates input against expectations
- `expect format` transforms input into regex patterns

This demonstrates excellent code reuse through inheritance - the input handling logic is written once in `ExpectBaseCommand` and shared by both commands, ensuring consistent behavior.

## Comparison with Test Commands

| Feature | Test Commands (`list`, `run`) | Expect Commands (`check`, `format`) |
|---------|-------------------------------|-------------------------------------|
| **Target type** | Multiple files (glob patterns) | Single input stream |
| **Default target** | Test directory + `**/*.yaml` | Stdin (if redirected) |
| **Exclusion patterns** | Yes (`--exclude`, `.cycodtignore`) | No |
| **Multiple targets** | Yes (`--files`) | No (single input only) |
| **Configuration files** | Yes (`.cycod.yaml`) | No |
| **Stdin support** | No | Yes (primary use case) |
| **File discovery** | Yes (`FindMatchingFiles`) | No (direct read) |

The expect commands prioritize **simplicity** and **composability** with Unix pipes, while test commands prioritize **flexibility** for test suite management.
