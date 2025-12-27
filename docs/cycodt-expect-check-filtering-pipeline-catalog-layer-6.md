# cycodt expect check - Layer 6: Display Control

## Overview

Layer 6 controls **how expectation checking results are presented** to the user. For the `expect check` command, this layer determines the format and verbosity of validation output.

## Command

```bash
cycodt expect check [options]
```

## Layer 6 Features

### 6.1 Progress Indicator

**Feature**: In-place progress message

**Purpose**: Shows that checking is in progress

**Behavior**:
- Displays "Checking expectations..." immediately
- Uses `\r` carriage return to overwrite with result
- No newline initially (allows in-place update)

**Source Code**: See [Layer 6 Proof](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md#61-progress-indicator)

### 6.2 Pass/Fail Display

**Feature**: Clear success/failure indication

**Purpose**: Shows whether expectations were met

**Behavior**:
- **PASS**: Overwrites progress with "Checking expectations... PASS!"
- **FAIL**: Overwrites progress with "Checking expectations... FAILED!" followed by reason
- Uses carriage return for clean display

**Source Code**: See [Layer 6 Proof](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md#62-passfail-display)

### 6.3 Failure Reason Display

**Feature**: Detailed failure information

**Purpose**: Explains why expectations were not met

**Behavior**:
- Shows which expectation failed (regex or instruction-based)
- Includes expected vs. actual comparison
- Multi-line format for readability

**Source Code**: See [Layer 6 Proof](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md#63-failure-reason-display)

### 6.4 Quiet Mode

**Option**: `--quiet` (inherited from `CommandLineOptions`)

**Purpose**: Suppress non-essential output

**Behavior**:
- Controlled by `ConsoleHelpers` infrastructure
- May suppress progress indicator
- Pass/fail result likely still shown
- Failure details still shown

**Source Code**: See [Layer 6 Proof](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md#64-quiet-mode)

### 6.5 Debug Mode

**Option**: `--debug` (inherited from `CommandLineOptions`)

**Purpose**: Show diagnostic information

**Behavior**:
- Enables debug logging throughout checking process
- Shows regex compilation and matching details
- Displays AI processing details (if `--instructions` used)
- Controlled by `ConsoleHelpers.ConfigureDebug(true)`

**Source Code**: See [Layer 6 Proof](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md#65-debug-mode)

## Display Format Examples

### Successful Check

```
Checking expectations... PASS!
```

### Failed Check (Regex)

```
Checking expectations... FAILED!

Expected pattern: "^Hello, World!$"
Did not find match in input
```

### Failed Check (Instructions)

```
Checking expectations... FAILED!

AI instruction: "Output should contain a greeting"
Reason: No greeting found in the output
```

## Data Flow

```
ExpectCheckCommand.ExecuteAsync()
    ↓
ExpectCheckCommand.ExecuteCheck()
    ↓
ConsoleHelpers.Write("Checking expectations...")
    ↓
FileHelpers.ReadAllLines(Input)
    ↓
ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, ...)
    ↓
    [Returns true/false + failure reason]
    ↓
If regex check failed:
    ConsoleHelpers.WriteLine("\r...FAILED!\n\n{reason}")
    Return 1
    ↓
CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, ...)
    ↓
    [Returns true/false + failure reason]
    ↓
If instruction check failed:
    ConsoleHelpers.WriteLine("\r...FAILED!\n\n{reason}")
    Return 1
    ↓
ConsoleHelpers.WriteLine("\r...PASS!")
Return 0
```

## Carriage Return Technique

The command uses `\r` (carriage return) to overwrite the progress message:

1. **Initial**: `Write("Checking expectations...")` - no newline
2. **Success**: `WriteLine("\rChecking expectations... PASS!")` - overwrites from start of line
3. **Failure**: `WriteLine("\rChecking expectations... FAILED!\n\n{reason}")` - overwrites and adds details

This provides a clean, in-place update without leaving residual text.

## Related Layers

- **Layer 1 (Target Selection)**: Determines input source (file or stdin)
- **Layer 3 (Content Filter)**: Applies regex patterns to filter content
- **Layer 4 (Content Removal)**: Applies negative regex patterns
- **Layer 7 (Output Persistence)**: Optionally saves output (via `--save-output`)
- **Layer 8 (AI Processing)**: Applies AI-based expectations (via `--instructions`)
- **Layer 9 (Actions on Results)**: Returns exit code for shell integration

## Implementation Notes

1. **Minimal output**: Unlike `list` and `run`, this command has very minimal output
2. **Exit code semantics**: Returns 0 for pass, 1 for fail (standard Unix convention)
3. **No color coding**: Uses default console colors (could be enhanced)
4. **Progress overwriting**: Uses carriage return for clean display
5. **Separate validation phases**: Checks regex patterns first, then AI instructions

## Proof Document

For detailed source code evidence with line numbers and full implementation details, see:
- [Layer 6 Proof Document](./cycodt-expect-check-filtering-pipeline-catalog-layer-6-proof.md)
