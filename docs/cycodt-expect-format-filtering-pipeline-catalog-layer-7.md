# cycodt `expect format` - Layer 7: OUTPUT PERSISTENCE

**[← Back to expect format Command Catalog](cycodt-expect-format-catalog-README.md)**

## Overview

The `expect format` command has **full** output persistence capabilities with file output control.

## Implementation Status

**Status**: ✅ Full - File output with explicit control

## Features

### Output File Options

1. **`--save-output <file>`**
   - Specify output file path for formatted expectations
   - If not specified, outputs to stdout

2. **`--output <file>`**
   - Alias for `--save-output`
   - Same functionality

### Default Behavior

```bash
# No output file specified
cycodt expect format --input test.txt
# Output: stdout (can be redirected with >, >>)

# Output file specified
cycodt expect format --input test.txt --save-output formatted.txt
# Output: formatted.txt (file written)

# Using alias
cycodt expect format --input test.txt --output formatted.txt
# Output: formatted.txt (file written)
```

## Usage Patterns

### Console Output (Default)

```bash
# Output to stdout
echo "Hello" | cycodt expect format

# Redirect to file using shell
echo "Hello" | cycodt expect format > formatted.txt

# Pipe to other commands
echo "Hello" | cycodt expect format | grep "Hello"
```

### File Output (Explicit)

```bash
# Write to specific file
cycodt expect format --input actual.txt --save-output expected.txt

# Short form
cycodt expect format --input actual.txt --output expected.txt

# From stdin to file
echo "Test output" | cycodt expect format --save-output patterns.txt
```

### Typical Workflow

```bash
# Step 1: Capture command output
my-command > actual-output.txt

# Step 2: Format into regex patterns for YAML test
cycodt expect format --input actual-output.txt --save-output expected-patterns.txt

# Step 3: Copy expected-patterns.txt content into YAML test file
# (Manual step or scripted)
```

## Implementation Details

**Implementation File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Key Components**:

1. **Property** (inherited from `ExpectBaseCommand`):
   - `Output` - User-specified output file path (nullable)

2. **Output Logic** (`ExecuteFormat()`, line 29):
   - Calls `WriteOutput(formattedText)` (inherited method)

3. **WriteOutput() Method** (`ExpectBaseCommand`, lines 31-41):
   - If `Output` is null/empty → write to console (stdout)
   - If `Output` is set → write to file

## Console vs File Output Logic

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectBaseCommand.cs`

```csharp
// Lines 31-41: WriteOutput method
protected void WriteOutput(string text)
{
    if (string.IsNullOrEmpty(Output))
    {
        ConsoleHelpers.WriteLine(text, overrideQuiet: true);
    }
    else
    {
        FileHelpers.WriteAllText(Output, text);
    }
}
```

**Decision Logic**:
- **Output == null or empty** → Console output via `ConsoleHelpers.WriteLine()`
- **Output != null** → File output via `FileHelpers.WriteAllText()`

## Output Format

### Strict Mode Output

**Input**:
```
Hello, world!
```

**Formatted Output**:
```
^Hello, world\!\\r?$\n
```

**File Content** (if `--save-output formatted.txt`):
```
^Hello, world\!\\r?$\n
```

**Console Output** (if no `--save-output`):
```
^Hello, world\!\\r?$\n
```

Same content, different destination.

### Non-Strict Mode Output

**Input**:
```
Test passed
```

**Formatted Output** (with `--strict false`):
```
Test passed\\r
```

## Comparison with Other Commands

| Command | Layer 7 Support | Options Available |
|---------|----------------|-------------------|
| **list** | ⚠️ Limited | Console only (redirect) |
| **run** | ✅ Full | `--output-file`, `--output-format` (trx, junit) |
| **expect check** | ❌ None | Exit code only |
| **expect format** | ✅ Full | `--save-output`, `--output` |

## Differences from `run` Command

### `run` Command (Structured Output)
- `--output-file` + `--output-format` (trx/junit)
- Multiple formats supported
- Structured XML output
- Test result reports

### `expect format` Command (Text Output)
- `--save-output` / `--output`
- Single format (plain text regex patterns)
- Unstructured text output
- Formatted expectations

Both have full Layer 7 support, but serve different purposes.

## File Handling

### File Creation/Overwrite

```bash
# First run: creates file
cycodt expect format --input test.txt --save-output patterns.txt
# Result: patterns.txt created

# Second run: overwrites file
cycodt expect format --input test2.txt --save-output patterns.txt
# Result: patterns.txt overwritten (previous content lost)
```

**Behavior**: File is **always overwritten** if it exists (no append mode).

### File Path Handling

```bash
# Relative path
cycodt expect format --input in.txt --save-output out.txt
# Creates: out.txt in current directory

# Absolute path
cycodt expect format --input in.txt --save-output /path/to/out.txt
# Creates: /path/to/out.txt

# Directory must exist
cycodt expect format --input in.txt --save-output subdir/out.txt
# Error if subdir/ doesn't exist
```

## Integration with YAML Tests

### Workflow Example

**Command**:
```bash
# Run the command to test
my-app --config test.cfg > actual-output.txt

# Format the output
cycodt expect format --input actual-output.txt --save-output expected-regex.txt
```

**Generated File** (`expected-regex.txt`):
```
^Application started\\r?$\n
^Config loaded: test\.cfg\\r?$\n
^Processing\.\.\.\\r?$\n
^Done\\!\\r?$\n
```

**YAML Test File** (`my-test.yaml`):
```yaml
- test: my app test
  command: my-app --config test.cfg
  expect-regex:
    - ^Application started\\r?$\n
    - ^Config loaded: test\.cfg\\r?$\n
    - ^Processing\.\.\.\\r?$\n
    - ^Done\\!\\r?$\n
```

**Execution**:
```bash
# Run test
cycodt run --file my-test.yaml
```

## Opportunities for Enhancement

Potential future enhancements (not currently implemented):

1. **Append mode**:
   - `--append` option to add to existing file instead of overwriting

2. **Multiple output formats**:
   - `--format plain` (current default)
   - `--format yaml` (directly output YAML-formatted expect-regex section)
   - `--format json` (JSON array of patterns)

3. **Template-based output**:
   - `--save-output patterns-{time}.txt` with timestamp substitution

4. **Output directory control**:
   - `--output-dir patterns/` to organize generated files

5. **Batch processing**:
   - Process multiple input files to multiple output files

## Stdin/Stdout Combinations

| Input Source | Output Destination | Usage |
|--------------|-------------------|-------|
| stdin | stdout | `echo "test" \| cycodt expect format` |
| stdin | file | `echo "test" \| cycodt expect format --save-output out.txt` |
| file | stdout | `cycodt expect format --input in.txt` |
| file | file | `cycodt expect format --input in.txt --save-output out.txt` |

**All combinations are supported.**

## Related Layers

- **[Layer 1: Target Selection](cycodt-expect-format-layer-1.md)** - Input source (file or stdin)
- **[Layer 6: Display Control](cycodt-expect-format-layer-6.md)** - Strict mode formatting
- **[Layer 9: Actions on Results](cycodt-expect-format-layer-9.md)** - Text transformation
- **[Proof Document](cycodt-expect-format-filtering-pipeline-catalog-layer-7-proof.md)** - Source code evidence

---

**[View Proof →](cycodt-expect-format-filtering-pipeline-catalog-layer-7-proof.md)**
