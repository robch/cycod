# cycodt `expect check` Command - Layer 1: TARGET SELECTION

## Purpose

Layer 1 (TARGET SELECTION) determines **what to search** - the primary search space for the expect check command. For `cycodt expect check`, this means selecting the input source to validate against expectations.

## Command Overview

```bash
cycodt expect check [options]
```

The `expect check` command validates input text against regex patterns and/or AI instructions, returning success (0) or failure (1) exit codes.

## Layer 1 Features

### 1. Input Source Specification

The `expect check` command has **one primary target**: the input text to validate.

#### Option
- `--input <file>`: Specify input file to check

#### Stdin Support (Implicit)
When `--input` is not provided AND stdin is redirected:
- Automatically uses stdin as input source
- `Input` property is set to `"-"` (special marker for stdin)

#### Examples
```bash
# Check explicit file
cycodt expect check --input output.txt --regex "expected.*pattern"

# Check stdin (implicit)
command-output | cycodt expect check --regex "expected.*pattern"

# Check stdin (explicit)
command-output | cycodt expect check --input - --regex "expected.*pattern"
```

### 2. No File Discovery or Glob Patterns

Unlike `list` and `run` commands, `expect check` does **not** support:
- ❌ Glob patterns
- ❌ Multiple files
- ❌ File exclusions
- ❌ `.cycodtignore` files
- ❌ Test directory configuration

**Rationale:** `expect check` operates on a **single input stream** (file or stdin), not a collection of files.

### 3. Empty Command Detection

The command is considered "empty" (triggers help) when:
- No `--input` option provided
- AND stdin is not redirected

#### Code Behavior
```csharp
public override bool IsEmpty()
{
    var noInput = string.IsNullOrEmpty(Input);
    var isRedirected = Console.IsInputRedirected;
    return noInput && !isRedirected;
}
```

This ensures users get help instead of an error when running:
```bash
# Shows help (no input, no stdin)
cycodt expect check
```

### 4. Validation Phase - Stdin Auto-Detection

During validation, if no input is specified but stdin is redirected:
- Automatically set `Input = "-"`
- This triggers stdin reading during execution

#### Code Behavior
```csharp
public override Command Validate()
{
    var noInput = string.IsNullOrEmpty(Input);
    var implictlyUseStdIn = noInput && Console.IsInputRedirected;
    if (implictlyUseStdIn)
    {
        Input = "-";
    }

    return this;
}
```

This provides a seamless experience:
```bash
# These are equivalent:
command | cycodt expect check --regex "pattern"
command | cycodt expect check --input - --regex "pattern"
```

## Implementation Details

### Processing Order

1. **Command Line Parsing** (lines 41-46 in CycoDtCommandLineOptions.cs)
   - Parse `--input` option → set `Input` property

2. **Validation Phase** (lines 19-29 in ExpectBaseCommand.cs)
   - Check if stdin is redirected and no input specified
   - If so, set `Input = "-"` (stdin marker)

3. **Execution Phase** (lines 31-63 in ExpectCheckCommand.cs)
   - Read input from file or stdin based on `Input` property
   - Validate against expectations

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
[Execute Check]
       ↓
  Read input:
      if Input == "-": read stdin
      else: read file at Input path
       ↓
  lines: string[] (input content)
       ↓
  [Apply expectations] (Layers 3-4)
```

### Input Reading

**File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`**

**Line 38:**
```csharp
var lines = FileHelpers.ReadAllLines(Input!);
```

**Note:** `FileHelpers.ReadAllLines()` handles both:
- File paths: reads from file system
- `"-"` value: reads from stdin

This abstraction simplifies the code - the command doesn't need to check `Input` value.

## Source Code References

See [Layer 1 Proof Document](cycodt-expect-check-filtering-pipeline-catalog-layer-1-proof.md) for:
- Line-by-line code references
- Call stack details
- Data structure definitions
- Complete implementation evidence

## Differences from Test Commands

| Feature | `list` / `run` | `expect check` |
|---------|----------------|----------------|
| **Target type** | Multiple files (glob patterns) | Single input stream |
| **Default target** | Test directory + `**/*.yaml` | Stdin (if redirected) |
| **Exclusion patterns** | Yes (`--exclude`, `.cycodtignore`) | No |
| **Multiple targets** | Yes (`--files`) | No (single input only) |
| **Configuration files** | Yes (`.cycod.yaml`) | No |
| **Stdin support** | No | Yes (primary use case) |

## Usage Patterns

### File Input
```bash
# Check file against regex
cycodt expect check --input output.txt --regex "SUCCESS"

# Check file against multiple patterns
cycodt expect check --input output.txt --regex "pattern1" --regex "pattern2"

# Check file against AI instructions
cycodt expect check --input output.txt --instructions "Verify output is valid JSON"
```

### Stdin Input (Implicit)
```bash
# Pipe command output
command-to-test | cycodt expect check --regex "expected"

# Heredoc
cycodt expect check --regex "test" << EOF
test data
more test data
EOF

# Shell redirection
cycodt expect check --regex "pattern" < output.txt
```

### Stdin Input (Explicit)
```bash
# Explicitly specify stdin
command-to-test | cycodt expect check --input - --regex "expected"
```

## Related Layers

- **Layer 3 (Content Filter)**: Applies regex patterns to input lines (`--regex`)
- **Layer 4 (Content Removal)**: Applies negative regex patterns (`--not-regex`)
- **Layer 7 (Output Persistence)**: Saves formatted output (`--save-output`)
- **Layer 8 (AI Processing)**: Applies AI instructions to validate input (`--instructions`)

## Summary

Layer 1 for `expect check` is **fundamentally different** from test commands:

1. **Single input stream** instead of multiple files
2. **Stdin-first design** for pipe-friendly operation
3. **No glob patterns** or file discovery
4. **No exclusion mechanisms** (not needed for single input)
5. **Auto-detection** of stdin when available

This makes `expect check` ideal for:
- **CI/CD pipelines**: Validate command output inline
- **Test assertions**: Check test output matches expectations
- **Output validation**: Ensure generated content is correct

The design prioritizes simplicity and composability with other commands through stdin piping.
