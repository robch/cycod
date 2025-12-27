# cycodt list - Layer 4: Content Removal

## Overview

**Layer 4** in the filter pipeline is **Content Removal** - actively excluding tests from results that match certain criteria. This is the negative filtering stage that happens AFTER content selection (Layer 3).

## Purpose

Remove specific tests from the result set based on:
1. Explicit exclusion patterns (`--remove`)
2. Optional test categories (excluded by default)
3. Test filter syntax with `-` prefix

## Command-Line Options

### `--remove <pattern> [<pattern> ...]`

**Source**: `CycoDtCommandLineOptions.cs` lines 71-76

Explicitly removes tests matching the given pattern(s).

**Behavior**:
- Pattern is converted to `-<pattern>` in filter list
- Multiple patterns create multiple exclusion filters
- Tests matching ANY removal pattern are excluded
- Processed by `YamlTestCaseFilter.FilterTestCases()`

**Example**:
```bash
# Remove tests containing "skip"
cycodt list --remove "skip"

# Remove multiple patterns
cycodt list --remove "skip" "slow" "broken"
```

### Filter Syntax: `-<pattern>`

**Source**: `YamlTestCaseFilter.cs` lines 36-42, 57-62

Tests can be excluded using the `-` prefix in filter expressions.

**Behavior**:
- Any criterion starting with `-` becomes a "must NOT match" filter
- Test must NOT contain the pattern in any searchable field
- ALL `-` patterns must be satisfied (test must match NONE of them)

**Fields Searched**:
- `DisplayName`
- `FullyQualifiedName`
- Test traits (tags, categories)
- Test properties (cli, run, script, expect, etc.)

**Example Filter Logic**:
```bash
# Using --contains and --remove
cycodt list --contains "api" --remove "skip"

# Internally becomes filter list: ["+api", "-skip"]
# Meaning: test MUST contain "api" AND must NOT contain "skip"
```

### Optional Test Exclusion

**Source**: `TestBaseCommand.cs` lines 115-138

By default, tests marked as `optional` are EXCLUDED unless explicitly included with `--include-optional`.

**Behavior**:
- Tests with `optional` trait are filtered out by default
- `--include-optional` without arguments includes ALL optional tests
- `--include-optional <category>` includes only specific categories
- When optional tests are excluded, the test execution chain is repaired

**Example**:
```bash
# Exclude all optional tests (default)
cycodt list

# Include all optional tests
cycodt list --include-optional

# Include only "nightly" optional tests
cycodt list --include-optional "nightly"

# Include multiple optional categories
cycodt list --include-optional "nightly" "slow"
```

## Implementation Details

### Filter Processing Order

1. **Source Criteria** (Layer 3): Tests matching positive patterns
2. **Must-Match Criteria** (Layer 3): Tests containing all `+pattern` filters
3. **Must-NOT-Match Criteria** (Layer 4): Tests NOT containing any `-pattern` filters
4. **Optional Test Filtering** (Layer 4): Exclude tests with `optional` trait unless included

### Test Chain Repair

**Source**: `TestBaseCommand.cs` lines 140-231

When optional tests are excluded, the framework repairs the execution chain to maintain test dependencies.

**Process**:
1. Identify excluded tests with `afterTestCaseId` and `nextTestCaseId` properties
2. Find previous non-excluded test in chain
3. Find next non-excluded test in chain
4. Update connections to skip over excluded tests
5. Handle recursive exclusions (chains of multiple excluded tests)

**Properties Updated**:
- `nextTestCaseId`: Points to the next test in execution order
- `afterTestCaseId`: Points to the previous test in execution order

This ensures that excluding test B in sequence A→B→C results in A→C.

## Code Flow

```
TestListCommand.ExecuteList()
  ↓
TestBaseCommand.FindAndFilterTests()
  ↓
TestBaseCommand.FilterOptionalTests()  ← Optional removal
  ↓
  ├─ HasOptionalTrait()               ← Check for optional trait
  ├─ HasMatchingOptionalCategory()    ← Match against included categories
  └─ RepairTestChain()                ← Fix execution order
  ↓
YamlTestCaseFilter.FilterTestCases()
  ↓
  ├─ Parse mustNotMatchCriteria       ← Extract "-pattern" filters
  └─ Filter: !TestContainsText()      ← Ensure test doesn't match exclusions
```

## Examples

### Example 1: Simple Removal

```bash
cycodt list --remove "skip"
```

**Effect**: Excludes any test where:
- `DisplayName` contains "skip", OR
- `FullyQualifiedName` contains "skip", OR
- Any trait name or value contains "skip", OR
- Any test property contains "skip"

### Example 2: Multiple Removals

```bash
cycodt list --remove "skip" "slow" "broken"
```

**Effect**: Excludes tests matching ANY of the patterns (OR logic for removal)

### Example 3: Combined Include/Exclude

```bash
cycodt list --contains "api" --remove "skip"
```

**Effect**:
- INCLUDE: Tests containing "api" (Layer 3)
- EXCLUDE: Tests containing "skip" (Layer 4)
- Result: Tests with "api" but NOT "skip"

### Example 4: Optional Test Handling

```bash
# Test file contains:
# - test-1: normal test
# - test-2: optional: nightly
# - test-3: optional: broken
# - test-4: normal test

# Default (no optional tests)
cycodt list
# Shows: test-1, test-4

# Include all optional
cycodt list --include-optional
# Shows: test-1, test-2, test-3, test-4

# Include only "nightly" optional tests
cycodt list --include-optional "nightly"
# Shows: test-1, test-2, test-4 (test-3 still excluded)
```

## Comparison with Layer 3 (Content Filter)

| Aspect | Layer 3 (Content Filter) | Layer 4 (Content Removal) |
|--------|-------------------------|---------------------------|
| **Purpose** | Include tests matching criteria | Exclude tests matching criteria |
| **Options** | `--test`, `--tests`, `--contains` | `--remove`, `--include-optional` |
| **Syntax** | Pattern or `+pattern` | `-pattern` |
| **Logic** | OR (any) or AND (all) | AND (none) |
| **Default** | No filtering (all tests included) | Optional tests excluded |
| **Processing** | Happens first | Happens second |

## Related Documentation

- [Layer 3: Content Filtering](cycodt-list-filtering-pipeline-catalog-layer-3.md) - Inclusion criteria
- [Layer 4 Proof](cycodt-list-filtering-pipeline-catalog-layer-4-proof.md) - Source code evidence
- [YamlTestCaseFilter Documentation](../../src/cycodt/TestFramework/README.md) - Filter implementation details
