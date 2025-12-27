# cycodt `expect check` Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** determines what content WITHIN the input source (container) to validate against expectations. For the `expect check` command, this means filtering which lines or patterns in the input are checked against regex patterns and AI instructions.

## Command: `expect check`

The `expect check` command verifies that input (from file or stdin) matches expected patterns. Unlike the `list` and `run` commands which filter tests, `expect check` filters/validates lines within a single input source.

## Options That Affect Layer 3

### 1. `--regex`

**Purpose**: Add regex pattern that input MUST match

**Syntax**:
```bash
cycodt expect check --input output.txt --regex "SUCCESS"
cycodt expect check --input output.txt --regex "^Line \d+$" --regex "PASS"
```

**Behavior**:
- Each regex pattern must match at least one line in the input
- Multiple `--regex` options act as AND (input must match ALL patterns)
- Patterns are matched against individual lines
- Uses .NET Regex matching (case-insensitive by default in implementation)

**See**: [Layer 3 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#1-regex)

---

### 2. `--not-regex`

**Purpose**: Add regex pattern that input MUST NOT match

**Syntax**:
```bash
cycodt expect check --input output.txt --not-regex "ERROR"
cycodt expect check --input output.txt --not-regex "FAIL" --not-regex "Exception"
```

**Behavior**:
- Each not-regex pattern must NOT match any line in the input
- Multiple `--not-regex` options act as AND (input must NOT match ANY pattern)
- Patterns are matched against individual lines
- Uses .NET Regex matching

**See**: [Layer 3 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#2-not-regex)

---

### 3. `--instructions`

**Purpose**: Provide AI instructions for expectation checking

**Syntax**:
```bash
cycodt expect check --input output.txt --instructions "Verify the output contains a success message"
```

**Behavior**:
- Uses AI to evaluate whether input meets expectations described in natural language
- Can be combined with regex patterns (both must pass)
- AI has access to full input text, not just individual lines
- Leverages CheckExpectInstructionsHelper

**See**: [Layer 3 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#3-instructions)

---

## Content Filtering vs. Content Validation

**Important Distinction**: Unlike `list` and `run` commands, `expect check` doesn't "filter" content in the traditional sense. Instead, it performs **content validation**:

- **Filtering** (list/run): Select subset of items to show/process
- **Validation** (expect check): Verify that content matches expectations

However, this still fits into Layer 3 because it determines **what aspects of the content** are evaluated:

- `--regex`: Line-level pattern matching
- `--not-regex`: Line-level pattern exclusion
- `--instructions`: Semantic/AI-based content validation

**See**: [Layer 3 Proof - Validation vs Filtering](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#validation-vs-filtering)

---

## Validation Algorithm

The Layer 3 validation process follows this algorithm:

```
1. Load input from file or stdin (Layer 1)
2. Split input into lines
3. Validate regex patterns (ExpectHelper.CheckLines):
   a. For each --regex pattern: At least one line must match
   b. For each --not-regex pattern: No lines may match
   c. If any validation fails: Return failure reason
4. Validate AI instructions (CheckExpectInstructionsHelper):
   a. Pass full input text to AI
   b. AI evaluates whether expectations are met
   c. If validation fails: Return failure reason
5. If all validations pass: Return success
```

**Source Code Flow**:
```
ExpectCheckCommand.ExecuteCheck()
  → FileHelpers.ReadAllLines(Input)
  → ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, ...)
  → CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, ...)
```

**See**: [Layer 3 Proof - Validation Algorithm](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#validation-algorithm)

---

## Pattern Matching Behavior

### Regex Pattern Matching

**Line-by-Line**:
- Each regex pattern is checked against each line individually
- A pattern passes if it matches ANY line
- Matching stops after first match (short-circuit)

**Example**:
```
Input:
Line 1: Building project...
Line 2: Tests passed: 10
Line 3: SUCCESS

--regex "SUCCESS"  ← Matches Line 3 ✓
--regex "passed"   ← Matches Line 2 ✓
--regex "failed"   ← No matches ✗
```

### Not-Regex Pattern Matching

**Line-by-Line Exclusion**:
- Each not-regex pattern is checked against each line
- A pattern passes if it matches NO lines
- Matching stops after first match (short-circuit failure)

**Example**:
```
Input:
Line 1: Building project...
Line 2: Tests passed: 10
Line 3: SUCCESS

--not-regex "ERROR"      ← No matches ✓
--not-regex "failed"     ← No matches ✓
--not-regex "SUCCESS"    ← Matches Line 3 ✗
```

**See**: [Layer 3 Proof - Pattern Matching](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#pattern-matching-behavior)

---

## Combined Validation

### Regex + Not-Regex

Both types of patterns can be combined:

```bash
cycodt expect check --input output.txt \
  --regex "SUCCESS" \
  --regex "passed: 10" \
  --not-regex "ERROR" \
  --not-regex "FAIL"
```

**Validation Logic**:
1. All `--regex` patterns must match (AND logic)
2. All `--not-regex` patterns must NOT match (AND logic)
3. Both conditions must be satisfied

### Regex + Instructions

Regex patterns and AI instructions can also be combined:

```bash
cycodt expect check --input output.txt \
  --regex "SUCCESS" \
  --instructions "Verify the output indicates successful completion"
```

**Validation Logic**:
1. Regex patterns are checked first
2. If regex validation passes, AI instructions are checked
3. Both validations must pass

**See**: [Layer 3 Proof - Combined Validation](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md#combined-validation)

---

## How Filtering Interacts with Other Layers

### Relationship to Layer 1 (Target Selection)
- Layer 1 determines WHAT to read (file or stdin)
- Layer 3 determines HOW to validate the content

### Relationship to Layer 4 (Content Removal)
- `expect check` does not remove content
- Content removal happens via `--not-regex` validation (Layer 3), not filtering (Layer 4)

### Relationship to Layer 6 (Display Control)
- Layer 3 determines validation results
- Layer 6 displays PASS/FAIL message based on Layer 3 validation

### Relationship to Layer 8 (AI Processing)
- `--instructions` option bridges Layer 3 (content filtering) and Layer 8 (AI processing)
- AI processes the content based on Layer 3 instructions

---

## Examples

### Example 1: Simple Regex Validation
```bash
cycodt expect check --input test-output.txt --regex "Test passed"
```
**Validation**: Checks if at least one line contains "Test passed"

### Example 2: Multiple Regex Patterns (AND)
```bash
cycodt expect check --input build-log.txt \
  --regex "Build succeeded" \
  --regex "0 errors" \
  --regex "0 warnings"
```
**Validation**: Checks that input contains ALL three patterns

### Example 3: Negative Validation
```bash
cycodt expect check --input test-output.txt \
  --not-regex "ERROR" \
  --not-regex "Exception" \
  --not-regex "FAIL"
```
**Validation**: Checks that input does NOT contain any of these patterns

### Example 4: Combined Positive and Negative
```bash
cycodt expect check --input test-output.txt \
  --regex "SUCCESS" \
  --regex "10 tests passed" \
  --not-regex "ERROR" \
  --not-regex "FAIL"
```
**Validation**: Checks that input contains success patterns AND does NOT contain error patterns

### Example 5: AI-Based Validation
```bash
cycodt expect check --input report.txt \
  --instructions "Verify that all tests passed and there were no errors"
```
**Validation**: AI evaluates whether the semantic meaning of the input meets expectations

### Example 6: Hybrid Validation (Regex + AI)
```bash
cycodt expect check --input test-results.txt \
  --regex "Tests: \d+" \
  --not-regex "ERROR" \
  --instructions "Confirm that test results show 100% pass rate"
```
**Validation**: Regex checks structural patterns, AI checks semantic meaning

---

## Edge Cases and Special Behaviors

### 1. Empty Input
If input is empty:
- All `--regex` patterns fail (no lines to match)
- All `--not-regex` patterns pass (no lines to match against)
- `--instructions` validation depends on AI interpretation

### 2. No Patterns Specified
If no `--regex`, `--not-regex`, or `--instructions` are specified:
- Validation passes automatically (no expectations to validate)
- Effectively a no-op

### 3. Partial Line Matching
- Regex patterns use `.Contains()` semantics for lines
- Pattern "ERROR" matches lines containing "ERROR" anywhere
- Use anchors (^, $) for exact matching: "^ERROR$"

### 4. Case Sensitivity
- Regex matching respects the regex pattern's case sensitivity
- By default, .NET regex is case-sensitive unless `(?i)` is used
- Example: `--regex "(?i)success"` for case-insensitive matching

### 5. Multi-Line Patterns
- Regex patterns are checked line-by-line
- For multi-line patterns, use `--instructions` with AI instead

---

## Implementation Notes

### Source Code Locations

**Command Line Parsing**:
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs:135-147` - Parsing --regex, --not-regex
- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs:148-154` - Parsing --instructions

**Validation Execution**:
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs:31-63` - ExecuteCheck()
- Line 38: Read input lines
- Line 41: Validate regex patterns
- Line 48: Validate AI instructions

**Regex Validation**:
- `src/common/Helpers/ExpectHelper.cs` - CheckLines() method

**AI Validation**:
- `src/common/Helpers/CheckExpectInstructionsHelper.cs` - CheckExpectations() method

---

## Related Documentation

- [Layer 1: Target Selection](cycodt-expect-check-filtering-pipeline-catalog-layer-1.md) - How input source is selected
- [Layer 6: Display Control](cycodt-expect-check-filtering-pipeline-catalog-layer-6.md) - How validation results are displayed
- [Layer 8: AI Processing](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md) - How AI instructions are processed

---

## Proof

For detailed source code evidence of all assertions in this document, see:
- **[Layer 3 Proof Document](cycodt-expect-check-filtering-pipeline-catalog-layer-3-proof.md)**
