# cycodt `expect format` Command - Filtering Pipeline Catalog

## Overview

The `expect format` command converts raw output into regex patterns suitable for use in YAML test expectations.

**Command**: `cycodt expect format [options]`

## Purpose

Format raw command output into escaped regex patterns for use in YAML test files' `expect-regex` sections.

## Layer Implementation Summary

| Layer | Implemented | Key Features |
|-------|-------------|--------------|
| 1. TARGET SELECTION | ✅ Yes | `--input` or stdin |
| 2. CONTAINER FILTER | ❌ No | N/A - single input source |
| 3. CONTENT FILTER | ❌ No | N/A - formats all input |
| 4. CONTENT REMOVAL | ❌ No | N/A - preserves all content |
| 5. CONTEXT EXPANSION | ❌ No | N/A - line-by-line formatting |
| 6. DISPLAY CONTROL | ⚠️ Limited | `--strict` mode control |
| 7. OUTPUT PERSISTENCE | ✅ Full | `--save-output`, `--output` |
| 8. AI PROCESSING | ❌ No | N/A - deterministic formatting |
| 9. ACTIONS ON RESULTS | ✅ Yes | Text transformation |

## Layer Details

- **[Layer 1: Target Selection](cycodt-expect-format-layer-1.md)** - [Proof](cycodt-expect-format-layer-1-proof.md)
- **[Layer 2: Container Filter](cycodt-expect-format-layer-2.md)** - [Proof](cycodt-expect-format-layer-2-proof.md)
- **[Layer 3: Content Filter](cycodt-expect-format-layer-3.md)** - [Proof](cycodt-expect-format-layer-3-proof.md)
- **[Layer 4: Content Removal](cycodt-expect-format-layer-4.md)** - [Proof](cycodt-expect-format-layer-4-proof.md)
- **[Layer 5: Context Expansion](cycodt-expect-format-layer-5.md)** - [Proof](cycodt-expect-format-layer-5-proof.md)
- **[Layer 6: Display Control](cycodt-expect-format-layer-6.md)** - [Proof](cycodt-expect-format-layer-6-proof.md)
- **[Layer 7: Output Persistence](cycodt-expect-format-layer-7.md)** - [Proof](cycodt-expect-format-layer-7-proof.md)
- **[Layer 8: AI Processing](cycodt-expect-format-layer-8.md)** - [Proof](cycodt-expect-format-layer-8-proof.md)
- **[Layer 9: Actions on Results](cycodt-expect-format-layer-9.md)** - [Proof](cycodt-expect-format-layer-9-proof.md)

## Command Line Options

### Target Selection (Layer 1)
- `--input <file>` - Input file to format (or `-` for stdin)
- If not specified and stdin is redirected, automatically uses stdin

### Display Control (Layer 6)
- `--strict <true|false>` - Strict mode (default: `true`)
  - **Strict mode**: Adds line anchors (`^...$`) and requires exact line matching
  - **Non-strict mode**: Simple regex escaping without anchors

### Output Persistence (Layer 7)
- `--save-output <file>` - Save formatted output to file
- `--output <file>` - Alias for `--save-output`
- If not specified, outputs to stdout

### Global Options
- `--debug` - Debug output (hex dumps of transformation steps)
- `--quiet` - Suppress non-essential output

## Usage Examples

```bash
# Format output to stdout (strict mode)
echo "Hello, world!" | cycodt expect format

# Format output to file
echo "Test passed" | cycodt expect format --save-output expected.txt

# Format from file to file
cycodt expect format --input output.txt --save-output formatted.txt

# Non-strict mode (no line anchors)
echo "partial match" | cycodt expect format --strict false

# Typical workflow: capture output, format, use in YAML test
my-command > actual.txt
cycodt expect format --input actual.txt --save-output expected.txt
# Then copy expected.txt contents into YAML test's expect-regex section
```

## Formatting Behavior

### Strict Mode (default)

**Input**:
```
Hello, world!
Test passed
```

**Output**:
```
^Hello, world\!\\r?$\n
^Test passed\\r?$\n
```

**Characteristics**:
- Escapes regex special characters: `\ ( ) [ ] { } . * + ? | ^ $`
- Adds `^` (line start anchor) and `$` (line end anchor)
- Escapes CR as `\\r` and makes it optional (`\\r?`)
- Adds `\n` at end of pattern
- Suitable for exact line matching in YAML tests

### Non-Strict Mode

**Input**:
```
Hello, world!
Test passed
```

**Output**:
```
Hello, world\!\\r
Test passed\\r
```

**Characteristics**:
- Escapes regex special characters
- Escapes CR as `\\r` but NO line anchors
- No `^`, `$`, or `\n` additions
- Suitable for substring matching

## Design Philosophy

The `expect format` command bridges the gap between:
- **Human-readable output** (what commands produce)
- **Regex patterns** (what YAML test framework expects)

### Problem It Solves

Manually writing regex patterns for test expectations is error-prone:
- Forgetting to escape special characters (`.`, `*`, `$`, etc.)
- Handling Windows line endings (CR+LF)
- Ensuring consistent pattern format

### Solution

Automated transformation that:
1. Escapes all regex special characters
2. Handles line ending variations
3. Optionally adds anchors for strict matching

## Workflow Integration

```bash
# Step 1: Run the command you want to test
my-command > actual-output.txt

# Step 2: Format the output into regex patterns
cycodt expect format --input actual-output.txt --save-output expected-patterns.txt

# Step 3: Copy patterns into YAML test file
# my-test.yaml:
# - test: my command test
#   command: my-command
#   expect-regex:
#     - <paste content from expected-patterns.txt>
```

## Source Code

**Primary Implementation**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Supporting Classes**:
- `ExpectBaseCommand.cs` - Base class with input/output handling
- `CycoDtCommandLineOptions.cs` - Command-line parsing

## Related Commands

- **[list](cycodt-list-catalog-README.md)** - List available tests
- **[run](cycodt-run-catalog-README.md)** - Execute tests (which use formatted expectations)
- **[expect check](cycodt-expect-check-catalog-README.md)** - Validate output against expectations (formatted patterns)
