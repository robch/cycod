# cycodt expect format - Layer 5: Context Expansion

## Layer Purpose

**Layer 5: Context Expansion** controls how results are expanded to show surrounding context around matches or processing points. For a text formatting tool, this might include showing additional lines for debugging or preserving context in formatted output.

## Implementation in `cycodt expect format`

### Status: ❌ NOT IMPLEMENTED

The `expect format` command in cycodt **does not implement context expansion**. It formats input text line-by-line into regex patterns suitable for expectation matching, but does not provide options to include surrounding context lines in the output.

### What is `expect format`?

The `expect format` command:
1. Reads input text (from file or stdin)
2. Escapes special regex characters in each line
3. Converts each line into a strict regex pattern
4. Outputs formatted patterns suitable for use in test expectations

**Purpose**: Create regex patterns from actual output for use in test YAML files.

### What Context Expansion Could Mean for `expect format`

If implemented, context expansion for `expect format` might include:

1. **Context Lines Around Formatted Lines**: Include unformatted context for readability
   - Example: `--preserve-context 2` to keep 2 lines before/after as comments

2. **Group Related Lines**: Format lines with their surrounding context as a group
   - Example: `--group-context 3` to group formatted lines with 3 lines of context

3. **Selective Formatting**: Format only certain lines while preserving others as context
   - Example: `--format-matching "error" --keep-context 2` to format only error lines with context

4. **Annotated Output**: Include original text alongside formatted patterns
   - Example: `--show-original` to output both original and formatted side-by-side

5. **Debug Mode with Context**: Show transformation steps with context
   - Example: `--debug-with-context` to show original → escaped → formatted with surrounding lines

### Current Behavior

The `expect format` command:

1. **Reads input** (Layer 1)
2. **Formats ALL lines** - line-by-line transformation:
   ```
   Input:  "Hello, world!"
   Output: "^Hello, world!\\r?$\\n"
   ```
3. **Outputs formatted text** (Layer 6 & 7)

**Key Characteristic**: Processes EVERY line independently, no context preservation.

**No Options For**:
- Preserving some lines unformatted
- Including context comments
- Selective formatting
- Grouping with context

## Cross-Tool Comparison

### sed (Unix tool) - Can preserve context
```bash
sed -n '10,20p' file.txt    # Process only lines 10-20, implicit context
sed '/error/!d' file.txt    # Delete non-matching (selective processing)
```

### awk (Unix tool) - Can include context
```awk
awk '/error/ {print prev1; print prev2; print $0}' file.txt  # Print with context
```

### cycodmd (files command) - HAS context expansion
- `--lines N`: Show N lines before AND after matches
- Allows selective display with context

### cycodt expect format - DOES NOT HAVE context expansion
- Formats ALL lines equally
- No selective formatting
- No context preservation
- No grouping with context

## Potential Enhancement Opportunities

1. **Add selective formatting with context**:
   ```bash
   cycodt expect format --input output.txt --format-matching "ERROR" --context 2
   # Format only lines containing "ERROR", keep 2 lines before/after as comments
   
   Output:
   # Line 10: Running tests...
   # Line 11: Test 1 passed
   ^ERROR: Test 2 failed\\r?$\\n    <-- Formatted
   # Line 13: Continuing...
   # Line 14: Test 3 passed
   ```

2. **Add context comments**:
   ```bash
   cycodt expect format --input output.txt --preserve-context 1
   
   Output:
   # Original line 1: Starting process...
   ^Starting process\.\.\.\\r?$\\n
   # Original line 2: Loading data...
   ^Loading data\.\.\.\\r?$\\n
   ```

3. **Add grouped formatting**:
   ```bash
   cycodt expect format --input output.txt --group-by-blank-lines
   
   Output:
   # Group 1 (lines 1-3)
   ^Line 1\\r?$\\n
   ^Line 2\\r?$\\n
   ^Line 3\\r?$\\n
   
   # Group 2 (lines 5-7)
   ^Line 5\\r?$\\n
   ^Line 6\\r?$\\n
   ^Line 7\\r?$\\n
   ```

4. **Add debug mode with original**:
   ```bash
   cycodt expect format --input output.txt --show-original
   
   Output:
   # Original: Hello, world!
   ^Hello, world!\\r?$\\n
   
   # Original: Error (code: 42)
   ^Error \\(code: 42\\)\\r?$\\n
   ```

5. **Add range-based formatting**:
   ```bash
   cycodt expect format --input output.txt --lines 10-20 --context 2
   # Format only lines 10-20, keep 2 lines before/after as context
   ```

## Example Use Cases Where Context Would Help

### Use Case 1: Formatting Error Output

**Current**: Format entire log file (hundreds of lines)
```bash
cycodt expect format --input app.log > expected.txt
# Result: 500 formatted lines - hard to manage
```

**With Context**: Format only errors with context
```bash
cycodt expect format --input app.log --format-matching "ERROR|WARN" --context 1
# Result: ~20 formatted lines with 1 line context each - manageable
```

### Use Case 2: Debugging Format Issues

**Current**: No way to see original alongside formatted
```bash
cycodt expect format --input output.txt
# Can't easily verify if formatting is correct
```

**With Context**: Show original for comparison
```bash
cycodt expect format --input output.txt --show-original
# Original: Hello (world)
# Formatted: ^Hello \\(world\\)\\r?$\\n
# Easy to verify escaping is correct
```

### Use Case 3: Partial Log Formatting

**Current**: Format entire log or nothing
```bash
cycodt expect format --input test.log
# All 200 lines formatted - too much for test expectations
```

**With Context**: Format only relevant section
```bash
cycodt expect format --input test.log --lines 50-60 --preserve-context 2
# Format lines 50-60 (main test output), keep 2 lines before/after as comments
```

## Current Workaround (Manual)

Users currently must:
1. Run `expect format` on entire input
2. Manually extract relevant formatted lines
3. Manually add context comments if needed
4. Manually verify formatting correctness

**With Layer 5**: Tool could handle context automatically.

## Evidence References

See the [proof document](cycodt-expect-format-filtering-pipeline-catalog-layer-5-proof.md) for detailed source code evidence showing the absence of context expansion features.

## Related Layers

- **[Layer 1: Target Selection](cycodt-expect-format-filtering-pipeline-catalog-layer-1.md)** - Input source selection
- **[Layer 3: Content Filter](cycodt-expect-format-filtering-pipeline-catalog-layer-3.md)** - N/A (formats all)
- **[Layer 4: Content Removal](cycodt-expect-format-filtering-pipeline-catalog-layer-4.md)** - CR trimming (strict mode)
- **[Layer 6: Display Control](cycodt-expect-format-filtering-pipeline-catalog-layer-6.md)** - Formatted output display
- **[Layer 7: Output Persistence](cycodt-expect-format-filtering-pipeline-catalog-layer-7.md)** - Output destination

---

**Status**: Not Implemented  
**Complexity if Added**: Low to Medium (would require line selection and context preservation logic)  
**Cross-Tool Pattern**: Common in Unix text processing tools (sed, awk, grep)  
**User Benefit**: Medium to High - would make formatted output more manageable and debuggable  
**Implementation Note**: Could leverage existing line-by-line processing to selectively format/preserve lines  
**Backward Compatibility**: Could default to current behavior (format all) if no context options specified
