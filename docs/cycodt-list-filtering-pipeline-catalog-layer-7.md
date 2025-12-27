# cycodt `list` - Layer 7: OUTPUT PERSISTENCE

**[← Back to list Command Catalog](cycodt-list-catalog-README.md)**

## Overview

The `list` command has **limited** output persistence capabilities. It outputs directly to the console (stdout) without built-in file save options.

## Implementation Status

**Status**: ⚠️ Limited - Console output only, no native file output options

## Features

### What IS Supported

1. **Console Output** (stdout)
   - Test names written to stdout via `ConsoleHelpers.WriteLine()`
   - Can be redirected using shell operators (`>`, `>>`)
   - Output format: one test name per line (or grouped by file in verbose mode)

### What is NOT Supported

1. **No `--save-output` option** - Unlike `run` command
2. **No `--output-file` option**
3. **No format options** (json, xml, csv, etc.)
4. **No built-in file persistence**

## Output Formats

### Default Format
```
test-name-1
test-name-2
test-name-3

Found 3 tests...
```

### Verbose Format (`--verbose`)
```
path/to/test-file-1.yaml

  test-name-1
  test-name-2

path/to/test-file-2.yaml

  test-name-3

Found 3 tests...
```

## Usage Patterns

### Shell Redirection

Since there's no built-in file output, users must use shell redirection:

```bash
# Redirect to file
cycodt list > test-list.txt

# Append to file
cycodt list >> test-list.txt

# With filtering
cycodt list --contains "database" > database-tests.txt

# Verbose grouped output
cycodt list --verbose > test-list-grouped.txt
```

## Implementation Details

**Implementation File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Key Methods**:
- `ExecuteList()` - Main execution, writes to console
- No file-writing logic in the command itself

**Output Mechanism**:
- Uses `ConsoleHelpers.WriteLine()` for all output
- Color coding applied (DarkGray for test names)
- Summary line at end ("Found N tests...")

## Comparison with Other Commands

| Command | Layer 7 Support | Options Available |
|---------|----------------|-------------------|
| **list** | ⚠️ Limited | Console only (redirect) |
| **run** | ✅ Full | `--output-file`, `--output-format` |
| **expect check** | ❌ None | Exit code only |
| **expect format** | ✅ Full | `--save-output`, `--output` |

## Design Rationale

The `list` command is designed as a **discovery tool** rather than a data export tool:
- Primary use case: Quick preview of available tests
- Shell redirection sufficient for most needs
- `run` command provides structured output (TRX, JUnit) for persistence
- Keeps `list` simple and focused

## Opportunities for Enhancement

Potential future enhancements (not currently implemented):

1. **Add `--save-output` option** for consistency with other commands
2. **Support multiple formats**:
   - `--format text` (current default)
   - `--format json` (structured test metadata)
   - `--format csv` (test name, file, line number)
3. **Template-based output** (similar to cycodgr):
   - `--save-output test-list-{time}.txt`

## Related Layers

- **[Layer 1: Target Selection](cycodt-list-layer-1.md)** - What tests to list
- **[Layer 2: Container Filter](cycodt-list-layer-2.md)** - Which tests to include
- **[Layer 6: Display Control](cycodt-list-layer-6.md)** - How to format output
- **[Proof Document](cycodt-list-filtering-pipeline-catalog-layer-7-proof.md)** - Source code evidence

---

**[View Proof →](cycodt-list-filtering-pipeline-catalog-layer-7-proof.md)**
