# Investigation Summary

**Investigation Date**: December 8, 2025  
**PR**: #73 - Fix test name parsing and TRX matrix bug  
**CI Run**: 20046335559  
**Status**: HYPOTHESIS FORMED - Ready for Validation

## Quick Navigation

1. **START HERE**: [ANALYSIS.md](ANALYSIS.md) - The smoking gun investigation
2. **THEN READ**: [HYPOTHESIS.md](HYPOTHESIS.md) - Proposed root cause and fix
3. **RAW EVIDENCE**: 
   - `test-results-cycodt.trx` - The broken TRX file from CI
   - `test-results.trx` - Regular dotnet tests for comparison

## The Journey

### 1. Initial Problem
PR #73 CI failed with:
```
##[error]Processing test results from ./TestResults/test-results-cycodt.trx failed
##[error]TypeError: Cannot read properties of undefined (reading 'TestMethod')
```

### 2. Download and Analysis
- Downloaded CI artifacts
- Counted TRX entries: **385 results vs 384 definitions**
- Found missing test: "Test medium length title (150 chars)"
- **Key Finding**: NOT the matrix bug, but a different issue

### 3. Local Reproduction  
- Ran tests locally: **385 results vs 383 definitions** (2 missing!)
- Missing tests:
  - "Create and make readonly file"
  - "Test medium length title (150 chars)"
- Both from `cycod-slash-title-commands.yaml`

### 4. Code Investigation
- Found existing evidence collection code in `TrxXmlTestReporter.cs`
- Discovered unsynchronized `List<TestCase>` access in `TestRun.cs`
- Found parallel execution evidence in logs (Thread 9, Thread 10 at 666ms)

### 5. Hypothesis Formation
**Race condition** in `TestRun._testCases` due to missing thread synchronization.

## The Hypothesis

**File**: `src/cycodt/TestFramework/TestRun.cs`

**Problem**: 
- `_testCases` is a `List<TestCase>` accessed by multiple threads
- `List<T>.Add()` is NOT thread-safe
- Parallel test execution causes simultaneous `Add()` calls
- List corruption leads to missing test cases

**Evidence**:
- `_testToExecutionMap` HAS locks (code already knows about threading)
- `_testCases` and `_testResults` DON'T have locks
- Log shows parallel execution at identical timestamps
- Existing code suspects this issue (has evidence gathering)

**Confidence**: 70% (Medium-High)

## Proposed Fix

Add locks to `_testCases` and `_testResults` operations:

```csharp
private readonly object _testCasesLock = new object();
private readonly object _testResultsLock = new object();

public void StartTest(TestCase testCase, Guid? guid = null)
{
    _startTime ??= DateTime.Now;
    lock (_testCasesLock) { _testCases.Add(testCase); }
    SetExecutionId(testCase, guid ?? Guid.NewGuid());
}
```

## Validation Steps

1. ✅ Documented the problem (ANALYSIS.md)
2. ✅ Reproduced locally
3. ✅ Formed hypothesis (HYPOTHESIS.md)
4. ⏳ Test hypothesis by running multiple times to check randomness
5. ⏳ Implement the fix
6. ⏳ Verify fix resolves the issue
7. ⏳ Run in CI to confirm

## Important Notes

### This is NOT the Matrix Bug
PR #74 fixed the matrix expansion bug (tests now generate properly with matrix parameters). The matrix fix is working correctly. **This is a separate, pre-existing bug** in TRX generation that affects regular tests under parallel execution.

### Why It Matters
- Blocks PR #73 from merging (CI fails)
- Affects ALL test runs with parallel execution
- Can cause random CI failures
- GitHub test reporter crashes when it encounters malformed TRX

### Why It's Hard to Spot
- Race conditions are probabilistic (timing-dependent)
- Only fails when threads collide at exact moment
- Most tests pass successfully
- Missing tests vary between runs (probably)

## Files in This Directory

- **README.md** (parent) - Evidence directory overview
- **SUMMARY.md** (this file) - Quick investigation summary
- **ANALYSIS.md** - Detailed analysis of the smoking gun
- **HYPOTHESIS.md** - Root cause hypothesis and proposed fix
- **test-results-cycodt.trx** - Broken TRX from CI (385/384)
- **test-results.trx** - Normal dotnet test TRX for comparison

## Commands to Reproduce

```bash
# Run tests locally
cd /c/src/cycod-2512-dec05-fix-trx-matrix-bug
cycodt run --output-file ./test-run-N.trx --log ./test-run-N.log

# Count TRX entries
grep -c "<UnitTestResult " test-run-N.trx  # Should be 385
grep -c "<UnitTest " test-run-N.trx        # Might be 383-385

# Find missing tests
grep '<UnitTestResult ' test-run-N.trx | sed -n 's/.*testId="\([^"]*\)".*/\1/p' | sort > results.txt
grep '<UnitTest ' test-run-N.trx | sed -n 's/.*id="\([^"]*\)".*/\1/p' | sort > definitions.txt
diff results.txt definitions.txt

# Check evidence in log
grep "\[TRX EVIDENCE\]" test-run-N.log
```

## Next Actions

The "save cube" is complete and staged. We can now:

1. **Test the hypothesis** - Run tests multiple times to check for randomness
2. **Implement the fix** - Add locks to TestRun.cs
3. **Verify the fix** - Confirm TRX files are correct
4. **Update PR #73** - Commit the fix and re-run CI

---

*This investigation was conducted systematically following the evidence from CI failure to local reproduction to code analysis to hypothesis formation. The evidence is preserved for future reference and verification.*
