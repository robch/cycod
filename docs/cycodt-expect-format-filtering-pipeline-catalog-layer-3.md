# cycodt `expect format` Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** for the `expect format` command is **NOT APPLICABLE** in the traditional sense. This command does not filter or validate content based on patterns or criteria. Instead, it performs a transformation operation on ALL input content.

## Command: `expect format`

The `expect format` command transforms input text into regex patterns suitable for use in test expectations. It processes ALL lines without any filtering or selection criteria.

## Why Layer 3 Does Not Apply

### No Filtering Options

The `expect format` command has **zero options** that affect content filtering:

**Available Options**:
1. `--input` - Specifies input source (Layer 1: Target Selection)
2. `--save-output` / `--output` - Specifies output destination (Layer 7: Output Persistence)
3. `--strict` - Controls formatting strictness (Layer 9: Actions on Results - transformation behavior)

**No Layer 3 Options**:
- ❌ No pattern matching options
- ❌ No content selection options
- ❌ No line filtering options
- ❌ No regex validation options

**See**: [Layer 3 Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md#no-filtering-options)

---

### All Content Is Processed

The command processes **every line** of input without exception:

**Source Code Evidence**:

**File**: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Lines 44-51**:
```csharp
var lines = input.Split('\n', StringSplitOptions.None);
var formattedLines = new List<string>();

foreach (var line in lines)
{
    var formatted = FormatLine(line, Strict);
    formattedLines.Add(formatted);
}
```

**Analysis**:
- Line 44: ALL lines are split (no filtering)
- Lines 47-51: EVERY line is formatted (no selection)
- Line 49: Each line is transformed via `FormatLine()`
- Line 50: Each formatted line is added to output
- No conditional logic for including/excluding lines

**See**: [Layer 3 Proof - All Content Processing](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md#all-content-is-processed)

---

## Layer Categorization

### What `expect format` Actually Does

**Layer 1 (Target Selection)**:
- ✅ Selects input source (file or stdin via `--input`)

**Layer 2 (Container Filtering)**:
- ❌ N/A (no containers to filter - single input source)

**Layer 3 (Content Filtering)**:
- ❌ N/A (all content is processed)

**Layer 4 (Content Removal)**:
- ❌ N/A (no content is removed)

**Layer 5 (Context Expansion)**:
- ❌ N/A (no context to expand)

**Layer 6 (Display Control)**:
- ✅ Output is controlled by `--save-output` (but output format is fixed)

**Layer 7 (Output Persistence)**:
- ✅ Can save to file via `--save-output` or output to stdout

**Layer 8 (AI Processing)**:
- ❌ N/A (no AI involvement)

**Layer 9 (Actions on Results)**:
- ✅ Transforms text into regex patterns (main action)
- ✅ `--strict` controls transformation behavior

---

## The Transformation Operation (Layer 9, Not Layer 3)

### What `--strict` Does

The `--strict` option affects **how** content is transformed, not **what** content is transformed:

**Strict Mode (default: `true`)**:
- Escapes special regex characters
- Trims trailing `\r` characters
- Adds `^` anchor at start of line
- Adds `\\r?$\\n` at end of line (allows optional `\r`, requires newline)

**Non-Strict Mode (`--strict false`)**:
- Escapes special regex characters
- Replaces `\r` with `\\r` literal (doesn't trim)
- No anchors added

**See**: [Layer 3 Proof - Strict Mode](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md#strict-mode-transformation)

---

## Why This Doesn't Constitute Filtering

### Filtering Implies Selection

**True Filtering** (as in Layer 3):
- Input: N items
- Process: Select subset based on criteria
- Output: M items (where M ≤ N)

**`expect format` Behavior**:
- Input: N lines
- Process: Transform ALL lines
- Output: N lines (same count, different format)

### Example Comparison

**Filtering Operation (`list --contains "test"` - Layer 3)**:
```
Input:
  test-one
  production-script
  test-two

Output (filtered):
  test-one
  test-two
```
**Result**: 3 input items → 2 output items (filtered)

**Transformation Operation (`expect format` - Layer 9)**:
```
Input:
  Line 1
  Line 2
  Line 3

Output (transformed):
  ^Line 1\\r?$\\n
  ^Line 2\\r?$\\n
  ^Line 3\\r?$\\n
```
**Result**: 3 input items → 3 output items (transformed, not filtered)

**See**: [Layer 3 Proof - Filtering vs Transformation](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md#filtering-vs-transformation)

---

## Conceptual Alternatives (If Layer 3 Were to Be Added)

While `expect format` currently has **no Layer 3 functionality**, hypothetical filtering options could include:

### Hypothetical Option: `--line-contains`

```bash
cycodt expect format --input output.txt --line-contains "ERROR"
```

**Behavior**: Only format lines containing "ERROR"

### Hypothetical Option: `--skip-empty`

```bash
cycodt expect format --input output.txt --skip-empty
```

**Behavior**: Skip formatting of empty lines

### Hypothetical Option: `--line-numbers`

```bash
cycodt expect format --input output.txt --line-numbers 5-10
```

**Behavior**: Only format lines 5 through 10

**Note**: None of these options currently exist in the codebase.

---

## Documentation Accuracy

This Layer 3 document exists for **completeness** of the 9-layer catalog structure. However, for `expect format`, Layer 3 is:

- ✅ Accurately documented as "NOT APPLICABLE"
- ✅ Explained why it doesn't apply (all content processed)
- ✅ Contrasted with commands that DO have Layer 3 filtering

---

## Implementation Notes

### Source Code Locations

**Command Implementation**:
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectFormatCommand.cs`

**Key Methods**:
- `ExecuteFormat()`: Lines 23-37 - Main execution
- `FormatInput()`: Lines 39-54 - Processes all lines
- `FormatLine()`: Lines 61-77 - Transforms individual line

**Evidence of No Filtering**:
- Line 44: `Split('\n', StringSplitOptions.None)` - keeps ALL lines (even empty)
- Lines 47-51: `foreach (var line in lines)` - processes EVERY line
- No conditional logic to skip lines

---

## Related Documentation

- [Layer 1: Target Selection](cycodt-expect-format-filtering-pipeline-catalog-layer-1.md) - How input source is selected
- [Layer 7: Output Persistence](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md) - How formatted output is saved
- [Layer 9: Actions on Results](cycodt-expect-format-filtering-pipeline-catalog-layer-9.md) - How content is transformed

---

## Proof

For detailed source code evidence supporting all assertions in this document, see:
- **[Layer 3 Proof Document](cycodt-expect-format-filtering-pipeline-catalog-layer-3-proof.md)**
