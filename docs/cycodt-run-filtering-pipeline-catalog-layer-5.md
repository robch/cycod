# cycodt run - Layer 5: Context Expansion

## Layer Purpose

**Layer 5: Context Expansion** controls how results are expanded to show surrounding context around matches or failures. For a test execution tool, this might include showing additional test output, stack traces, or related test results.

## Implementation in `cycodt run`

### Status: ❌ NOT IMPLEMENTED

The `run` command in cycodt **does not implement context expansion** in the sense of showing extended context around test failures or results. It runs tests and reports pass/fail status, but does not provide options to expand context around failures.

### What Context Expansion Could Mean for `run`

If implemented, context expansion for `run` might include:

1. **Failure Context Expansion**: Show more details around failed tests
   - Example: `--failure-context-lines 10` to show 10 lines before/after failure point
   - Example: `--show-full-stack-trace` to expand stack traces

2. **Related Test Expansion**: Show tests that ran before/after failed tests
   - Example: `--show-related-tests 3` to show 3 tests before and after each failure

3. **Dependency Chain Expansion**: Show the full dependency chain for failed tests
   - Example: `--expand-dependencies` to show which setup/teardown tests ran

4. **Output Buffer Expansion**: Show more console output around test execution
   - Example: `--output-context 50` to show 50 lines of output before failure

### Current Behavior

The `run` command:

1. **Finds and filters tests** (Layers 1 & 2)
2. **Executes tests** (Layer 9)
3. **Reports results** to console and file (Layers 6 & 7)

Test output is controlled by:
- The test framework's default output handling
- The `--verbose` global option (affects overall verbosity)
- Test result reporters (TRX, JUnit formats)

But there are **NO specific options** to expand context around test failures or execution points.

## Cross-Tool Comparison

### cycodmd (files command)
✅ **Has context expansion**:
- `--lines N`: Show N lines before AND after matches
- `--lines-before N`, `--lines-after N`: Asymmetric expansion

### cycodj (search command)
✅ **Has context expansion**:
- `--context N`: Show N messages before and after match

### cycodgr (search command)
✅ **Has context expansion**:
- `--lines-before-and-after N`: Show N lines around code matches

### cycodt run
❌ **No context expansion**: Runs tests with default output, no expansion controls

## Potential Enhancement Opportunities

1. **Add failure context expansion**:
   ```bash
   cycodt run --failure-context-lines 20
   # Show 20 lines of output before/after each failure point
   ```

2. **Add stack trace expansion**:
   ```bash
   cycodt run --full-stack-trace
   # Show complete stack traces (not truncated)
   ```

3. **Add test chain context**:
   ```bash
   cycodt run --show-test-context 3
   # Show 3 tests before and after each failed test
   ```

4. **Add dependency context**:
   ```bash
   cycodt run --show-dependencies
   # Show setup/teardown tests that ran for each test
   ```

5. **Add output buffer control**:
   ```bash
   cycodt run --output-buffer-lines 100
   # Keep 100 lines of output history for context
   ```

## Evidence References

See the [proof document](cycodt-run-filtering-pipeline-catalog-layer-5-proof.md) for detailed source code evidence showing the absence of context expansion features.

## Related Layers

- **[Layer 1: Target Selection](cycodt-run-filtering-pipeline-catalog-layer-1.md)** - Finding test files
- **[Layer 2: Container Filter](cycodt-run-filtering-pipeline-catalog-layer-2.md)** - Filtering specific tests
- **[Layer 6: Display Control](cycodt-run-filtering-pipeline-catalog-layer-6.md)** - Test execution output formatting
- **[Layer 7: Output Persistence](cycodt-run-filtering-pipeline-catalog-layer-7.md)** - Test result reports (TRX/JUnit)
- **[Layer 9: Actions on Results](cycodt-run-filtering-pipeline-catalog-layer-9.md)** - Test execution action

---

**Status**: Not Implemented  
**Complexity if Added**: Medium to High (would require test framework integration for output buffering and context tracking)  
**Cross-Tool Pattern**: Common in search tools, rare in test runners (though modern test frameworks often have failure context features)  
**Alternative**: Test frameworks like Jest, pytest have built-in failure context - could be integrated
