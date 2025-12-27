# cycodt expect check - Layer 5: Context Expansion

## Layer Purpose

**Layer 5: Context Expansion** controls how results are expanded to show surrounding context around matches or failures. For an expectation checking tool, this might include showing context lines around failed matches or providing detailed failure context.

## Implementation in `cycodt expect check`

### Status: ❌ NOT IMPLEMENTED

The `expect check` command in cycodt **does not implement context expansion**. It checks if input matches expectations (regex patterns and/or AI instructions) and reports PASS/FAIL, but does not provide options to show context around failed matches.

### What Context Expansion Could Mean for `expect check`

If implemented, context expansion for `expect check` might include:

1. **Failure Context Lines**: Show lines around failed pattern matches
   - Example: `--failure-context 5` to show 5 lines before/after where pattern was expected but not found

2. **Match Context**: Show context around successful matches (for debugging)
   - Example: `--show-match-context 3` to show 3 lines around each matched pattern

3. **Diff Context**: Show detailed diff for AI instruction failures
   - Example: `--diff-context 10` to show 10 lines around AI-detected mismatches

4. **Full Input Display**: Show complete input with annotations
   - Example: `--show-full-input-with-highlights` to display entire input with failed sections highlighted

5. **Pattern Chain Context**: Show related patterns in context of failures
   - Example: `--show-pattern-chain` to show which patterns passed/failed in sequence

### Current Behavior

The `expect check` command:

1. **Reads input** from file or stdin (Layer 1)
2. **Checks patterns** (Layer 3):
   - Regex patterns that must match (`--regex`)
   - Regex patterns that must NOT match (`--not-regex`)
   - AI-based expectations (`--instructions`)
3. **Reports result** (Layer 6):
   - "PASS" if all expectations met
   - "FAIL" with basic failure reason if expectations not met

**Failure output** is minimal:
```
Checking expectations... FAILED!

<basic failure message>
```

There is **NO option** to show:
- Lines of context around the failure point
- The full input with highlights
- Detailed diffs or explanations
- Related pattern check results

## Cross-Tool Comparison

### grep (Unix tool) - HAS context expansion
```bash
grep -C 3 pattern file    # Show 3 lines before and after
grep -B 5 pattern file    # Show 5 lines before
grep -A 2 pattern file    # Show 2 lines after
```

### cycodmd (files command) - HAS context expansion
- `--lines N`: Show N lines before AND after matches
- `--lines-before N`, `--lines-after N`: Asymmetric expansion

### cycodj (search command) - HAS context expansion
- `--context N`: Show N messages before and after match

### cycodt expect check - DOES NOT HAVE context expansion
- Reports PASS/FAIL with minimal failure context
- No options to control context display

## Potential Enhancement Opportunities

1. **Add failure context display**:
   ```bash
   cycodt expect check --input output.txt --regex "success" --failure-context 5
   # Show 5 lines before/after where "success" was expected but not found
   ```

2. **Add match context for debugging**:
   ```bash
   cycodt expect check --input output.txt --regex ".*completed.*" --show-matches-with-context 3
   # Show successful matches with 3 lines of context (for debugging patterns)
   ```

3. **Add diff-style output**:
   ```bash
   cycodt expect check --input output.txt --instructions "..." --show-diff
   # Show unified diff highlighting mismatches from AI instructions
   ```

4. **Add annotated full input**:
   ```bash
   cycodt expect check --input output.txt --regex "..." --show-full-input-annotated
   # Display entire input with pattern match annotations
   ```

5. **Add verbose failure mode**:
   ```bash
   cycodt expect check --input output.txt --regex "..." --verbose-failures
   # Show detailed failure information with context
   ```

## Current Failure Output Examples

### Regex Pattern Failure (Current - No Context)
```
Checking expectations... FAILED!

Expected pattern 'success' not found in input.
```

### Regex Pattern Failure (With Context - Hypothetical)
```
Checking expectations... FAILED!

Expected pattern 'success' not found.

Context around expected location (line 15):
   13: Processing data...
   14: Running validation...
   15: Validation failed!     <-- Expected 'success' here
   16: Error code: 42
   17: Exiting with failure
```

### AI Instruction Failure (Current - No Context)
```
Checking expectations... FAILED!

AI detected: Output does not contain expected success message.
```

### AI Instruction Failure (With Context - Hypothetical)
```
Checking expectations... FAILED!

AI Analysis: Output does not contain expected success message.

Expected: "Operation completed successfully"
Found: "Operation failed with errors"

Context (lines 20-25):
   20: Starting operation...
   21: Processing files...
   22: Operation failed with errors     <-- Mismatch detected here
   23: Error: File not found
   24: Rolling back changes
   25: Cleanup complete
```

## Evidence References

See the [proof document](cycodt-expect-check-filtering-pipeline-catalog-layer-5-proof.md) for detailed source code evidence showing the absence of context expansion features.

## Related Layers

- **[Layer 1: Target Selection](cycodt-expect-check-filtering-pipeline-catalog-layer-1.md)** - Input source selection
- **[Layer 3: Content Filter](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md)** - Expectation checking (regex, AI)
- **[Layer 6: Display Control](cycodt-expect-check-filtering-pipeline-catalog-layer-6.md)** - Pass/Fail output formatting
- **[Layer 8: AI Processing](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md)** - AI-based expectation checking

---

**Status**: Not Implemented  
**Complexity if Added**: Low to Medium (would require failure position tracking and context extraction)  
**Cross-Tool Pattern**: Very common in search/match tools (grep, ag, ripgrep)  
**User Benefit**: High - especially for debugging why expectations failed  
**Implementation Note**: Could leverage existing line-based input parsing to extract context around failure points
