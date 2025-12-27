# cycodt expect format - Layer 6: Display Control

## Overview

Layer 6 controls **how formatted output is presented** to the user. For the `expect format` command, this layer determines where and how the formatted regex patterns are displayed.

## Command

```bash
cycodt expect format [options]
```

## Layer 6 Features

### 6.1 Output Destination

**Feature**: Flexible output routing

**Purpose**: Send formatted output to console or file

**Behavior**:
- **Default (no --output)**: Writes to stdout
- **With --output**: Writes to specified file
- Controlled by `ExpectBaseCommand.WriteOutput()` method

**Source Code**: See [Layer 6 Proof](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md#61-output-destination)

### 6.2 Format Mode Control

**Option**: `--strict <true|false>` (default: true)

**Purpose**: Controls formatting strictness

**Behavior**:
- **Strict mode (default)**: Generates anchored regex patterns (`^pattern\r?$\n`)
- **Non-strict mode**: Generates unanchored patterns (just escaped content)
- Affects line-ending handling and pattern anchoring

**Source Code**: See [Layer 6 Proof](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md#62-format-mode-control)

### 6.3 Debug Output

**Option**: `--debug` (inherited from `CommandLineOptions`)

**Purpose**: Show diagnostic information

**Behavior**:
- Enables debug hex dumps of input/output at each transformation stage
- Shows line-by-line processing details
- Displays escape transformations

**Source Code**: See [Layer 6 Proof](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md#63-debug-output)

### 6.4 Quiet Mode

**Option**: `--quiet` (inherited from `CommandLineOptions`)

**Purpose**: Suppress non-essential output

**Behavior**:
- Has minimal effect (command produces no "chatty" output)
- Formatted output still written (it's the primary function)
- Debug output suppressed by default anyway
- Controlled by `ConsoleHelpers` infrastructure

**Source Code**: See [Layer 6 Proof](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md#64-quiet-mode)

### 6.5 Override Quiet for Output

**Feature**: Force output even in quiet mode

**Purpose**: Ensure formatted patterns are always displayed to stdout when no --output specified

**Behavior**:
- `WriteOutput()` method uses `overrideQuiet: true` for console output
- Guarantees formatted patterns are visible
- Essential for pipe usage: `echo "text" | cycodt expect format`

**Source Code**: See [Layer 6 Proof](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md#65-override-quiet-for-output)

## Display Format Examples

### Strict Mode (Default)

**Input**:
```
Hello, World!
How are you?
```

**Output**:
```
^Hello, World\!\\r?$\n
^How are you\?\\r?$\n
```

### Non-Strict Mode

**Input**:
```
Hello, World!
How are you?
```

**Output**:
```
Hello, World\!\\r
How are you\?\\r
```

## Data Flow

```
ExpectFormatCommand.ExecuteAsync()
    ↓
ExpectFormatCommand.ExecuteFormat()
    ↓
input = FileHelpers.ReadAllText(Input)
    ↓
formattedText = FormatInput(input)
    ↓
    Split into lines
    ↓
    For each line:
        FormatLine(line, Strict)
            ↓
            EscapeSpecialRegExChars(line)
            ↓
            Handle \r (carriage returns)
            ↓
            If strict: Add anchors (^ and $) and \r? for optional CR
    ↓
    Join lines with \n
    ↓
WriteOutput(formattedText)
    ↓
    If Output is null:
        ConsoleHelpers.WriteLine(text, overrideQuiet: true)
        → Stdout
    ↓
    Else:
        FileHelpers.WriteAllText(Output, text)
        → File
    ↓
Return 0 (success)
```

## Formatting Details

### Special Character Escaping

The following regex special characters are escaped:
- `\` → `\\`
- `(` → `\(`
- `)` → `\)`
- `[` → `\[`
- `]` → `\]`
- `{` → `\{`
- `}` → `\}`
- `.` → `\.`
- `*` → `\*`
- `+` → `\+`
- `?` → `\?`
- `|` → `\|`
- `^` → `\^`
- `$` → `\$`

**Pattern**: `([\\()\[\]{}.*+?|^$])` → `\$1`

### Line Ending Handling

**Strict Mode**:
1. Trim trailing `\r` from line
2. Replace internal `\r` with `\\r`
3. Add `^` prefix
4. Add `\\r?$\\n` suffix (allows optional CR, requires LF)

**Non-Strict Mode**:
1. Don't trim `\r`
2. Replace all `\r` with `\\r`
3. No anchors

## Related Layers

- **Layer 1 (Target Selection)**: Determines input source (file or stdin)
- **Layer 7 (Output Persistence)**: Determines output destination (stdout or file)

## Implementation Notes

1. **Line-by-line processing**: Each line is formatted independently
2. **Preserves line structure**: Output has same number of lines as input
3. **Regex-ready output**: Generated patterns can be used directly in expect-regex
4. **Hex dump debugging**: When --debug is enabled, shows byte-level transformations
5. **No color coding**: Uses default console colors
6. **Silent operation**: No progress messages (just transforms input to output)

## Proof Document

For detailed source code evidence with line numbers and full implementation details, see:
- [Layer 6 Proof Document](./cycodt-expect-format-filtering-pipeline-catalog-layer-6-proof.md)
