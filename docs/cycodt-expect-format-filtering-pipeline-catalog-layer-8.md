# cycodt `expect format` - Layer 8: AI Processing

## Overview

The `expect format` command does **NOT** implement AI Processing (Layer 8).

## Implementation Status

❌ **Not Implemented**

## Rationale

The `expect format` command's purpose is to transform text into regex patterns for use in test expectations. This is a deterministic text transformation that doesn't require or benefit from AI:

1. **Input**: Raw text (e.g., command output)
2. **Process**: Escape regex special characters, add line anchors
3. **Output**: Regex patterns suitable for `expect-regex` in tests

The algorithm is rule-based and produces predictable results.

## What Would AI Processing Look Like?

If Layer 8 were implemented for `expect format`, it could potentially:

1. **Smart Pattern Generation**: AI generates more flexible patterns
   ```bash
   echo "User: john@example.com" | cycodt expect format --ai-generalize
   # Output: User: \w+@\w+\.\w+  (generalized email pattern)
   # Instead of: User: john@example\.com
   ```

2. **Pattern Simplification**: AI identifies redundant patterns
   ```bash
   cat complex-output.txt | cycodt expect format --ai-simplify
   # AI consolidates similar lines into pattern groups
   ```

3. **Semantic Expectations**: Convert to natural language
   ```bash
   cat output.txt | cycodt expect format --ai-to-instructions
   # Output: "Should contain email address and timestamp"
   ```

4. **Pattern Validation**: AI checks if patterns are too strict/loose
   ```bash
   cat output.txt | cycodt expect format --ai-validate
   # AI warns: "Pattern too specific, will break if date format changes"
   ```

## Current Algorithm

The command uses a **purely deterministic** approach:

```
For each line:
  1. Escape regex special characters: . * + ? | ( ) [ ] { } ^ $ \
  2. Handle carriage returns: \r → \\r
  3. Add line anchors (strict mode): ^pattern$
  4. Add newline escape: \n
```

No AI, no variability, no inference.

## Comparison with Other Commands

| Command | AI Processing | Purpose |
|---------|---------------|---------|
| list | ❌ None | Display test names |
| run | ❌ None (command-level) | Execute tests |
| expect check | ✅ `--instructions` | Validate output with AI |
| expect format | ❌ None | Transform text to regex |

Only `expect check` implements AI processing in cycodt CLI.

## Relationship with expect check

These two commands are complementary:

```
expect format (NO AI)
  ↓
  Generates: ^exact regex patterns$
  ↓
  Used in: YAML test files as expect-regex
  ↓
expect check (HAS AI)
  ↓
  Validates: actual output against patterns or instructions
```

**expect format** creates precise patterns; **expect check** validates flexibly.

## Related Layers

- **Layer 1 (Target Selection)**: Reads from stdin or file
- **Layer 6 (Display Control)**: `--strict` option controls formatting
- **Layer 7 (Output Persistence)**: `--save-output` writes patterns to file
- **Layer 9 (Actions on Results)**: Transforms text (deterministic operation)

## See Also

- [Layer 8 Proof](cycodt-expect-format-filtering-pipeline-catalog-layer-8-proof.md) - Source code evidence
- [Layer 9: Actions on Results](cycodt-expect-format-filtering-pipeline-catalog-layer-9.md) - Text transformation logic
- [expect check Layer 8](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md) - AI processing for validation
- [Layer 6: Display Control](cycodt-expect-format-filtering-pipeline-catalog-layer-6.md) - Strict mode option
