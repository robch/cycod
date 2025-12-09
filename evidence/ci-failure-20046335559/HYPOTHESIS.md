# Hypothesis: Race Condition in TestRun._testCases

**Date**: December 8, 2025  
**Investigator**: AI Assistant  
**Status**: HYPOTHESIS - Needs Verification

## The Problem

TRX file generation is missing UnitTest definitions for some tests:
- **CI Run 20046335559**: 385 results, 384 definitions (1 missing)
- **Local Reproduction**: 385 results, 383 definitions (2 missing)

## Missing Tests

### CI (1 missing):
- `yaml.cycod-slash-title-commands.slash-title-commands::01.Test medium length title (150 chars)`
  - Test ID: `b26c248a-79d7-a508-a280-0c70af8905cd`

### Local (2 missing):
- `yaml.cycod-slash-title-commands.slash-title-commands::01.Create and make readonly file`
  - Test ID: `9edff72f-723a-b0b6-3ad6-49c42e3d2d52`
- `yaml.cycod-slash-title-commands.slash-title-commands::01.Test medium length title (150 chars)`
  - Test ID: `aba72637-4886-b1f5-1c3f-43e223995b3b`

## Hypothesis: Unsafe List Access in TestRun

### Evidence

**Location**: `src/cycodt/TestFramework/TestRun.cs`

```csharp
public class TestRun
{
    public void StartTest(TestCase testCase, Guid? guid = null)
    {
        _startTime ??= DateTime.Now;
        _testCases.Add(testCase);  // ⚠️ NO LOCK!
        SetExecutionId(testCase, guid ?? Guid.NewGuid());
    }

    public void RecordTest(TestResult testResult)
    {
        _testResults.Add(testResult);  // ⚠️ NO LOCK!
    }

    private void SetExecutionId(TestCase testCase, Guid guid)
    {
        lock (_testToExecutionMap)  // ✅ HAS LOCK
        {
            _testToExecutionMap[testCase.Id] = guid;
        }
    }

    private List<TestCase> _testCases = new List<TestCase>();
    private Dictionary<Guid, Guid> _testToExecutionMap = new Dictionary<Guid, Guid>();
    private List<TestResult> _testResults = new List<TestResult>();
}
```

### The Race Condition

**Parallel Execution Context:**
- Tests run in parallel on multiple threads (observed in logs: Thread 9, Thread 10, etc.)
- Multiple threads call `StartTest()` simultaneously
- `List<T>.Add()` is **NOT thread-safe** without external synchronization

**From Log Evidence:**
```
[000009]: 666ms Thread: 9 - CALLING_HOST_RECORDSTART - TestCase: aba72637...
[000010]: 666ms Thread: 10 - CALLING_HOST_RECORDSTART - TestCase: 9edff72f...
```

Both threads called `RecordStart` at **exactly 666ms**, causing simultaneous calls to `StartTest()`.

### Why This Causes Missing Definitions

**Scenario:**

1. Thread A calls `_testCases.Add(testCaseA)`
2. Thread B calls `_testCases.Add(testCaseB)` **simultaneously**
3. List's internal state gets corrupted:
   - Count might be incremented incorrectly
   - Items might overwrite each other
   - Some items might be "lost"

**Result:**
- All tests execute successfully (TestResults are recorded)
- Some TestCases are lost from `_testCases` collection
- TRX generation fails because `testRun.TestCases` is incomplete
- GitHub test reporter crashes: `TypeError: Cannot read properties of undefined (reading 'TestMethod')`

### Inconsistency with Dictionary Locking

The code **already recognizes the need for thread safety** in `_testToExecutionMap`:

```csharp
lock (_testToExecutionMap)
{
    _testToExecutionMap[testCase.Id] = guid;
}
```

But the same protection is **missing** for `_testCases` and `_testResults`.

## Supporting Evidence

### 1. Existing Evidence Collection Code

Someone already suspected collection desynchronization and added logging:

**File**: `src/cycodt/TestFramework/TrxXmlTestReporter.cs` (lines 15-29)

```csharp
// EVIDENCE GATHERING - detect collection desynchronization
var missingTestCases = testResults
    .Where(tr => !testCases.Any(tc => tc.Id == tr.TestCase.Id))
    .ToList();

if (missingTestCases.Any())
{
    ConsoleHelpers.WriteDebugLine($"[TRX EVIDENCE] {missingTestCases.Count} TestResults missing TestCases:");
    foreach (var missing in missingTestCases)
    {
        ConsoleHelpers.WriteDebugLine($"[TRX EVIDENCE] Missing: {missing.TestCase.Id} - {missing.TestCase.DisplayName}");
    }
}

ConsoleHelpers.WriteDebugLine($"[TRX EVIDENCE] Generation: {testResults.Count} results -> {testCases.Count} definitions");
```

This code literally checks for "missing TestCases" and logs them. **The problem was already known or suspected!**

### 2. Log Evidence Confirms the Issue

**From**: `local-test-results.log`

```
[000007]: 41095ms VERBOSE: TrxXmlTestReporter.cs:22 [TRX EVIDENCE] 2 TestResults missing TestCases:
[000007]: 41095ms VERBOSE: TrxXmlTestReporter.cs:25 [TRX EVIDENCE] Missing: 9edff72f... - yaml.cycod-slash-title-commands.slash-title-commands::01.Create and make readonly file@712714
[000007]: 41095ms VERBOSE: TrxXmlTestReporter.cs:25 [TRX EVIDENCE] Missing: aba72637... - yaml.cycod-slash-title-commands.slash-title-commands::01.Test medium length title (150 chars)@b1b285
[000007]: 41095ms VERBOSE: TrxXmlTestReporter.cs:29 [TRX EVIDENCE] Generation: 385 results -> 383 definitions
```

### 3. Pattern Analysis

**Observation**: The missing tests are NOT random:
- Both missing tests are from the **same test file**: `cycod-slash-title-commands.yaml`
- Both ran in **parallel** at the **same timestamp** (666ms)
- This suggests a **timing-based race condition**

**Counter-evidence to consider**: 
- Why don't we see MORE missing tests if this is a race condition?
- Why are the same tests consistently missing (or are they random across runs)?

## Proposed Fix

Add thread synchronization to `_testCases` and `_testResults`:

```csharp
public class TestRun
{
    private readonly object _testCasesLock = new object();
    private readonly object _testResultsLock = new object();
    
    public void StartTest(TestCase testCase, Guid? guid = null)
    {
        _startTime ??= DateTime.Now;
        
        lock (_testCasesLock)
        {
            _testCases.Add(testCase);
        }
        
        SetExecutionId(testCase, guid ?? Guid.NewGuid());
    }

    public void RecordTest(TestResult testResult)
    {
        lock (_testResultsLock)
        {
            _testResults.Add(testResult);
        }
    }
    
    public IList<TestCase> TestCases
    {
        get
        {
            lock (_testCasesLock)
            {
                return _testCases.ToList();
            }
        }
    }
    
    public IList<TestResult> TestResults
    {
        get
        {
            lock (_testResultsLock)
            {
                return _testResults.ToList();
            }
        }
    }

    private List<TestCase> _testCases = new List<TestCase>();
    private List<TestResult> _testResults = new List<TestResult>();
    private Dictionary<Guid, Guid> _testToExecutionMap = new Dictionary<Guid, Guid>();
}
```

## Questions to Validate Hypothesis

### 1. Is the issue reproducible with different runs?
- Run tests multiple times locally
- Check if the SAME tests are always missing, or if it's random
- If random → strongly supports race condition hypothesis
- If consistent → might be a different issue

### 2. Does the issue disappear with sequential execution?
- Modify test execution to run sequentially (disable parallelism)
- If issue disappears → confirms parallel execution is the cause

### 3. Are there timing dependencies?
- The missing tests ran at **exactly the same timestamp** (666ms)
- Is this coincidence or evidence of simultaneous access?

### 4. Why don't ALL parallel tests fail?
- Race conditions are **probabilistic**, not deterministic
- The window for collision is very small
- Most parallel operations complete without collision
- We only see failures when timing aligns **exactly**

## Alternative Hypotheses to Consider

### Hypothesis 2: Duplicate Test IDs
- Perhaps tests with certain characteristics get duplicate IDs
- The duplicate overwrites the original in some collection
- **Evidence against**: Log shows different GUIDs for each test
- **Evidence against**: Tests execute successfully, so IDs must be unique

### Hypothesis 3: Test Name Collisions
- Perhaps tests with similar names cause issues
- **Evidence against**: Test names are unique in the YAML file
- **Evidence against**: DisplayName includes unique suffix (@712714, @b1b285)

### Hypothesis 4: Memory Corruption
- Perhaps something is corrupting the list memory
- **Evidence against**: Would expect more chaotic failures
- **Evidence against**: Dictionary with locks works fine

## Confidence Level

**Confidence**: **MEDIUM-HIGH** (70%)

**Reasons for confidence:**
- Direct evidence of unsynchronized collection access
- Log evidence shows parallel execution at same timestamp
- Existing code already suspected this issue (evidence gathering)
- Matches classic race condition symptoms

**Reasons for doubt:**
- Need to reproduce the issue multiple times to confirm randomness
- Need to test the fix to confirm it resolves the issue
- Small sample size (only 2 missing tests locally)
- Could be a more complex interaction we haven't discovered

## Next Steps

1. **Reproduce** the issue multiple times to check for randomness
2. **Implement** the proposed fix
3. **Test** the fix with multiple runs
4. **Verify** that TRX files now have matching counts
5. **Monitor** CI to ensure the fix works in that environment

## References

- CI Run: https://github.com/robch/cycod/actions/runs/20046335559
- Local TRX: `local-test-results.trx`
- Local Log: `local-test-results.log`
- Source: `src/cycodt/TestFramework/TestRun.cs`
- TRX Reporter: `src/cycodt/TestFramework/TrxXmlTestReporter.cs`
