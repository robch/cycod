# cycodt `expect check` - Layer 7: OUTPUT PERSISTENCE

**[← Back to expect check Command Catalog](cycodt-expect-check-catalog-README.md)**

## Overview

The `expect check` command has **no** output persistence capabilities. It is designed purely for validation with exit code signaling.

## Implementation Status

**Status**: ❌ None - Exit code only, no file output

## Features

### What IS Supported

1. **Exit Code** (process return value)
   - 0 = All expectations passed
   - 1 = One or more expectations failed or error occurred
   - Can be checked in shell scripts: `if cycodt expect check ...; then ...`

2. **Console Messages** (for human debugging)
   - Success message: "Checking expectations... PASS!"
   - Failure message with details
   - Error message with stack trace

### What is NOT Supported

1. **No `--save-output` option**
2. **No `--output-file` option**
3. **No format options** (json, xml, etc.)
4. **No report generation**
5. **No validation result persistence**

## Design Rationale

The `expect check` command is designed as a **validation tool**, not a reporting tool:

### Primary Use Case: Shell Scripting
```bash
#!/bin/bash

# Run command and capture output
some-command > output.txt

# Validate output
if cycodt expect check --input output.txt --regex "success"; then
    echo "Command succeeded!"
    exit 0
else
    echo "Command failed validation!"
    exit 1
fi
```

### CI/CD Pipeline Use Case
```yaml
# .github/workflows/test.yml
- name: Run tests
  run: my-test-command > test-output.txt

- name: Validate output
  run: cycodt expect check --input test-output.txt --regex "ALL PASSED"
  # Step fails if exit code is non-zero
```

### Why No File Output?

1. **Simplicity**: Single-purpose tool (validate, don't report)
2. **Composability**: Can be combined with other tools
3. **Shell-friendly**: Exit codes are the standard validation mechanism
4. **Minimal overhead**: No file I/O for reporting

## Workarounds for Output Persistence

If you need to persist validation results, use shell mechanisms:

### Capture Exit Code
```bash
cycodt expect check --input output.txt --regex "success"
result=$?
echo "Validation result: $result" > validation-result.txt
```

### Capture Console Output
```bash
cycodt expect check --input output.txt --regex "success" 2>&1 | tee validation-output.txt
```

### Structured Logging (with wrapper script)
```bash
#!/bin/bash

# validation-wrapper.sh
timestamp=$(date +%Y-%m-%d_%H-%M-%S)
cycodt expect check "$@" > validation-$timestamp.log 2>&1
exit_code=$?

# Create JSON result
cat > validation-$timestamp.json <<EOF
{
  "timestamp": "$timestamp",
  "command": "cycodt expect check $*",
  "exit_code": $exit_code,
  "passed": $([ $exit_code -eq 0 ] && echo "true" || echo "false")
}
EOF

exit $exit_code
```

## Console Output Examples

### Success (no details)
```
Checking expectations... PASS!
```
**Exit Code**: 0

### Failure (regex pattern not found)
```
Checking expectations... FAILED!

Expected to find pattern: success
But pattern was not found in output.
```
**Exit Code**: 1

### Failure (not-regex pattern found)
```
Checking expectations... FAILED!

Expected NOT to find pattern: ERROR
But pattern was found in output.
```
**Exit Code**: 1

### Failure (AI instructions)
```
Checking expectations... FAILED!

AI validation failed with reason:
The output contains warnings which indicate the process did not complete successfully.
```
**Exit Code**: 1

### Error (exception)
```
ERROR: Input file not found: missing-file.txt
(stack trace)
```
**Exit Code**: 1

## Comparison with Other Commands

| Command | Layer 7 Support | Options Available |
|---------|----------------|-------------------|
| **list** | ⚠️ Limited | Console only (redirect) |
| **run** | ✅ Full | `--output-file`, `--output-format` (trx, junit) |
| **expect check** | ❌ None | Exit code only |
| **expect format** | ✅ Full | `--save-output`, `--output` |

## Typical Usage Pattern

```bash
# Pattern 1: Inline validation
command-to-test | cycodt expect check --regex "expected output"

# Pattern 2: File-based validation
command-to-test > output.txt
cycodt expect check --input output.txt --regex "expected output"

# Pattern 3: Multiple validations
command-to-test > output.txt
cycodt expect check --input output.txt --regex "test 1 passed" || exit 1
cycodt expect check --input output.txt --regex "test 2 passed" || exit 1
cycodt expect check --input output.txt --regex "test 3 passed" || exit 1

# Pattern 4: Conditional logic
if cycodt expect check --input output.txt --regex "success"; then
    echo "Tests passed, deploying..."
    deploy-command
else
    echo "Tests failed, aborting..."
    exit 1
fi
```

## Implementation Details

**Implementation File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Key Methods**:
- `ExecuteCheck()` - Performs validation, returns int (0 or 1)
- No file-writing logic in the command itself

**Output Mechanism**:
- Uses `ConsoleHelpers.Write()` and `ConsoleHelpers.WriteLine()` for messages
- Returns exit code via `return` statement (lines 56, 52, 46)

## Opportunities for Enhancement

Potential future enhancements (not currently implemented):

1. **Add `--save-report` option** for validation report:
   - `--save-report validation-report.json` (JSON format)
   - `--save-report validation-report.txt` (text format)

2. **Support detailed reporting**:
   - Which patterns matched/didn't match
   - Line numbers where patterns were found
   - Match context (surrounding lines)

3. **Support multiple formats**:
   - `--report-format json` (structured results)
   - `--report-format junit` (JUnit-style test result)
   - `--report-format markdown` (human-readable report)

4. **Add `--verbose` option** for debug output:
   - Show all patterns being checked
   - Show input content (truncated)
   - Show match attempts

## Related Layers

- **[Layer 1: Target Selection](cycodt-expect-check-layer-1.md)** - What input to validate
- **[Layer 3: Content Filter](cycodt-expect-check-layer-3.md)** - Regex patterns
- **[Layer 6: Display Control](cycodt-expect-check-layer-6.md)** - Console output
- **[Layer 8: AI Processing](cycodt-expect-check-layer-8.md)** - AI-based validation
- **[Layer 9: Actions on Results](cycodt-expect-check-layer-9.md)** - Validation execution
- **[Proof Document](cycodt-expect-check-filtering-pipeline-catalog-layer-7-proof.md)** - Source code evidence

---

**[View Proof →](cycodt-expect-check-filtering-pipeline-catalog-layer-7-proof.md)**
