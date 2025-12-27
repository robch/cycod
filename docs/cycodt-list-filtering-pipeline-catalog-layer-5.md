# cycodt list - Layer 5: Context Expansion

## Layer Purpose

**Layer 5: Context Expansion** controls how results are expanded to show surrounding context around matches. This could include showing lines before/after a match, or expanding to show related tests in a chain.

## Implementation in `cycodt list`

### Status: ❌ NOT IMPLEMENTED

The `list` command in cycodt **does not implement context expansion**. It lists test names with optional grouping by file, but does not provide options to expand context around individual tests.

### What Context Expansion Could Mean for `list`

If implemented, context expansion for `list` might include:

1. **Test Chain Expansion**: Show tests before/after in the execution chain
   - Example: `--before-tests 2 --after-tests 2` to show 2 tests before and after each matched test

2. **File Context**: Show surrounding tests from the same file
   - Example: `--context-tests 3` to show 3 tests before and after from same file

3. **Dependency Expansion**: Show tests that depend on or are depended upon by matched tests
   - Example: `--show-dependencies` to expand to include dependent tests

4. **Property Expansion**: Show additional test properties/metadata
   - Example: `--expand-properties` to show full test configuration

### Current Display Options (Layer 6, not Layer 5)

The `list` command does have **display control** options (Layer 6), but these are not context expansion:

- `--verbose`: Groups tests by file (display formatting, not context expansion)
- Standard mode: Simple list of test names

## Cross-Tool Comparison

### cycodmd (files command)
✅ **Has context expansion**:
- `--lines N`: Show N lines before AND after matches
- `--lines-before N`: Show N lines before matches
- `--lines-after N`: Show N lines after matches

### cycodj (search command)
✅ **Has context expansion**:
- `--context N`, `-C N`: Show N messages before and after match

### cycodgr (search command)
✅ **Has context expansion**:
- `--lines-before-and-after N`: Show N lines around matches
- `--lines N`: Alias for above

### cycodt list
❌ **No context expansion**: Lists tests without surrounding context

## Potential Enhancement Opportunities

1. **Add test chain context**:
   ```bash
   cycodt list --test "my-test" --context-tests 2
   # Would show 2 tests before and 2 tests after "my-test" in execution order
   ```

2. **Add file-based context**:
   ```bash
   cycodt list --contains "authentication" --show-file-context
   # Would show all tests from files containing "authentication" tests
   ```

3. **Add dependency graph expansion**:
   ```bash
   cycodt list --test "setup-test" --show-dependents
   # Would show all tests that depend on "setup-test"
   ```

## Evidence References

See the [proof document](cycodt-list-filtering-pipeline-catalog-layer-5-proof.md) for detailed source code evidence showing the absence of context expansion features.

## Related Layers

- **[Layer 1: Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md)** - Finding test files
- **[Layer 2: Container Filter](cycodt-list-filtering-pipeline-catalog-layer-2.md)** - Filtering specific tests
- **[Layer 6: Display Control](cycodt-list-filtering-pipeline-catalog-layer-6.md)** - How results are formatted (exists but different from context expansion)

---

**Status**: Not Implemented  
**Complexity if Added**: Medium (would require test ordering/chain analysis)  
**Cross-Tool Pattern**: Common feature in search-oriented tools (cycodmd, cycodj, cycodgr)
