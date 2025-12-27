# cycodt `expect check` Command - Filtering Pipeline Catalog

## Overview

The `expect check` command verifies that output matches expected patterns (regex) and optionally validates against AI-driven instructions.

**Command**: `cycodt expect check [options]`

## Purpose

Validate test output or command output against expectations using regex patterns and/or AI-assisted verification.

## Layer Implementation Summary

| Layer | Implemented | Key Features |
|-------|-------------|--------------|
| 1. TARGET SELECTION | ✅ Yes | `--input` or stdin |
| 2. CONTAINER FILTER | ❌ No | N/A - single input source |
| 3. CONTENT FILTER | ✅ Yes | `--regex`, `--not-regex` patterns |
| 4. CONTENT REMOVAL | ❌ No | N/A - validates all content |
| 5. CONTEXT EXPANSION | ❌ No | N/A - checks entire input |
| 6. DISPLAY CONTROL | ⚠️ Limited | Pass/Fail message, error details |
| 7. OUTPUT PERSISTENCE | ❌ None | Exit code only (0=pass, 1=fail) |
| 8. AI PROCESSING | ✅ Yes | `--instructions` for AI-based validation |
| 9. ACTIONS ON RESULTS | ✅ Yes | Validation check, exit with status code |

## Layer Details

- **[Layer 1: Target Selection](cycodt-expect-check-layer-1.md)** - [Proof](cycodt-expect-check-layer-1-proof.md)
- **[Layer 2: Container Filter](cycodt-expect-check-layer-2.md)** - [Proof](cycodt-expect-check-layer-2-proof.md)
- **[Layer 3: Content Filter](cycodt-expect-check-layer-3.md)** - [Proof](cycodt-expect-check-layer-3-proof.md)
- **[Layer 4: Content Removal](cycodt-expect-check-layer-4.md)** - [Proof](cycodt-expect-check-layer-4-proof.md)
- **[Layer 5: Context Expansion](cycodt-expect-check-layer-5.md)** - [Proof](cycodt-expect-check-layer-5-proof.md)
- **[Layer 6: Display Control](cycodt-expect-check-layer-6.md)** - [Proof](cycodt-expect-check-layer-6-proof.md)
- **[Layer 7: Output Persistence](cycodt-expect-check-layer-7.md)** - [Proof](cycodt-expect-check-layer-7-proof.md)
- **[Layer 8: AI Processing](cycodt-expect-check-layer-8.md)** - [Proof](cycodt-expect-check-layer-8-proof.md)
- **[Layer 9: Actions on Results](cycodt-expect-check-layer-9.md)** - [Proof](cycodt-expect-check-layer-9-proof.md)

## Command Line Options

### Target Selection (Layer 1)
- `--input <file>` - Input file to check (or `-` for stdin)
- If not specified and stdin is redirected, automatically uses stdin

### Content Filtering (Layer 3)
- `--regex <pattern>` - Regex pattern that must match (can specify multiple)
- `--not-regex <pattern>` - Regex pattern that must NOT match (can specify multiple)

### AI Processing (Layer 8)
- `--instructions <text>` - AI-driven validation instructions

### Global Options
- `--debug` - Debug output
- `--quiet` - Suppress non-essential output

## Usage Examples

```bash
# Check output matches regex pattern
some-command | cycodt expect check --regex "success"

# Check output from file
cycodt expect check --input output.txt --regex "test passed"

# Check output does NOT contain error
cycodt expect check --input output.txt --not-regex "ERROR"

# Multiple patterns
cycodt expect check --input output.txt \
  --regex "test 1.*passed" \
  --regex "test 2.*passed" \
  --not-regex "FAIL"

# AI-based validation
cycodt expect check --input output.txt \
  --instructions "Verify the output indicates successful completion"

# Combined regex and AI
cycodt expect check --input output.txt \
  --regex "success" \
  --instructions "Check that no warnings were issued"
```

## Exit Codes

- **0**: All expectations passed
- **1**: One or more expectations failed or error occurred

## Output

### Success
```
Checking expectations... PASS!
```

### Failure (Regex)
```
Checking expectations... FAILED!

Expected to find pattern: success
But pattern was not found in output.
```

### Failure (AI Instructions)
```
Checking expectations... FAILED!

AI validation failed:
The output indicates warnings were present, which violates the expectation.
```

## Validation Logic

1. **Read input** from file or stdin
2. **Check regex patterns** (`--regex` must match, `--not-regex` must NOT match)
3. **Check AI instructions** (if provided)
4. **Report first failure** (short-circuit on first error)
5. **Exit with appropriate code**

## Design Philosophy

The `expect check` command is designed for:
- **Test automation**: Verify command output in CI/CD pipelines
- **Exit code driven**: Success/failure communicated via exit code (shell-friendly)
- **Minimal output**: Only shows failure details (quiet by default)
- **Fast failure**: Stops checking on first failed expectation

## Source Code

**Primary Implementation**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Supporting Classes**:
- `ExpectBaseCommand.cs` - Base class with input/output handling
- `CycoDtCommandLineOptions.cs` - Command-line parsing
- `ExpectHelper.cs` - Regex checking logic
- `CheckExpectInstructionsHelper.cs` - AI-based validation

## Related Commands

- **[list](cycodt-list-catalog-README.md)** - List available tests
- **[run](cycodt-run-catalog-README.md)** - Execute tests (which may use expect check internally)
- **[expect format](cycodt-expect-format-catalog-README.md)** - Format expectations for use in YAML tests
