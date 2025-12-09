# CI Failure Analysis - Run 20046335559

**Date**: December 8, 2025  
**PR**: #73 - Fix test name parsing to use :: separator instead of dots  
**Branch**: `robch/2512-dec04-fix-log-multi-test-updated`  
**CI Run**: https://github.com/robch/cycod/actions/runs/20046335559  
**Head Commit**: `de3b2cf9` (merge commit including PR #74 matrix fix)

## Executive Summary

CI failed in the "Publish test results" step with error:
```
##[error]Processing test results from ./TestResults/test-results-cycodt.trx failed
##[error]TypeError: Cannot read properties of undefined (reading 'TestMethod')
```

**Root Cause**: The TRX file has **385 UnitTestResult entries** but only **384 UnitTest definitions**, causing GitHub's test reporter to crash when trying to access the missing TestMethod.

**Critical Finding**: This is **NOT the matrix bug** that PR #74 was supposed to fix. The missing definition is for a regular test, not a matrix test.

## Investigation Process

### Step 1: Download CI Artifacts

```bash
gh run download 20046335559 --name test-results --dir ./ci-test-results
```

Downloaded files:
- `test-results-cycodt.trx` (956,061 bytes) - **The smoking gun**
- `test-results.trx` (325,334 bytes) - Regular dotnet test results
- `test-results-cycodt.log` (98,459,492 bytes) - Full test execution log

### Step 2: Count TRX Entries

```bash
# Count UnitTestResult entries (test executions)
grep -c "<UnitTestResult " ./ci-test-results/test-results-cycodt.trx
# Result: 385

# Count UnitTest definitions (test metadata)
grep -c "<UnitTest " ./ci-test-results/test-results-cycodt.trx
# Result: 384
```

**🚨 Mismatch Found**: 385 results but only 384 definitions

### Step 3: Identify the Missing Test

Extract and compare test IDs:

```bash
# Extract all testId values from UnitTestResult entries
grep '<UnitTestResult ' ./ci-test-results/test-results-cycodt.trx | \
  sed -n 's/.*testId="\([^"]*\)".*/\1/p' | \
  sort > /tmp/result-ids.txt

# Extract all id values from UnitTest definitions
grep '<UnitTest ' ./ci-test-results/test-results-cycodt.trx | \
  sed -n 's/.*id="\([^"]*\)".*/\1/p' | \
  sort > /tmp/definition-ids.txt

# Find the difference
diff /tmp/result-ids.txt /tmp/definition-ids.txt
```

**Result**:
```
272d271
< b26c248a-79d7-a508-a280-0c70af8905cd
```

### Step 4: Identify the Culprit Test

```bash
grep "b26c248a-79d7-a508-a280-0c70af8905cd" ./ci-test-results/test-results-cycodt.trx
```

**The Missing Test Definition**:
- **Test ID**: `b26c248a-79d7-a508-a280-0c70af8905cd`
- **Test Name**: `yaml.cycod-slash-title-commands.slash-title-commands::01.Test medium length title (150 chars)`
- **Execution ID**: `90ddab7b-96bf-4e9f-bcb6-4e1211c78f40`
- **Outcome**: Passed
- **Duration**: 17.88 seconds
- **Test Type**: Regular test (NOT a matrix test)

## Key Findings

### 1. The Matrix Fix is Working

The matrix tests (`yaml.simple5.simple5::timeouts@17b36b[sleep=1,timeout=1500]`, etc.) appear correctly in the log output with proper matrix parameter expansion. This suggests PR #74's matrix expansion fix is functioning as intended.

### 2. A Different Bug Exists

The missing test definition is for a **regular, non-matrix test**: "Test medium length title (150 chars)". This indicates there's a separate bug in the TRX generation logic that causes certain test definitions to be omitted.

### 3. The Bug is Reproducible

The test consistently:
- ✅ Executes (has a UnitTestResult entry)
- ✅ Passes (outcome="Passed")
- ❌ Missing its UnitTest definition in the TestDefinitions section

### 4. GitHub's Test Reporter Fails Gracefully... Not

When encountering a UnitTestResult without a corresponding UnitTest definition, the test reporter attempts to read `TestMethod` from `undefined`, causing:
```
TypeError: Cannot read properties of undefined (reading 'TestMethod')
```

## TRX File Structure Analysis

### Expected Structure
```xml
<TestRun>
  <Results>
    <UnitTestResult testId="abc-123" ... />  <!-- N entries -->
  </Results>
  <TestDefinitions>
    <UnitTest id="abc-123" ... >             <!-- N matching entries -->
      <TestMethod ... />
    </UnitTest>
  </TestDefinitions>
</TestRun>
```

### Actual Structure (Broken)
- **Results section**: 385 UnitTestResult entries ✓
- **TestDefinitions section**: 384 UnitTest entries ✗
- **Missing**: UnitTest definition for test ID `b26c248a-79d7-a508-a280-0c70af8905cd`

## Next Steps

1. **Reproduce Locally**
   - Run cycodt tests with TRX output in the PR #73 worktree
   - Verify the bug reproduces consistently
   - Examine the TRX generation code path for this specific test

2. **Debug TRX Generation**
   - Check `TrxXmlTestReporter.cs` for logic that writes TestDefinitions
   - Look for conditions that might skip certain test definitions
   - Investigate if there's a race condition or duplicate test name handling issue

3. **Check Test File**
   - Examine `tests/cycodt-yaml/cycod-slash-title-commands.yaml`
   - Look for anything unique about the "Test medium length title (150 chars)" test
   - Check if there are duplicate test names or special characters causing issues

4. **Verify Matrix Tests**
   - Confirm that matrix tests have proper 1:1 correspondence between Results and Definitions
   - Validate that PR #74's fix is actually working correctly
   - Check inception test TRX files before they're deleted in the cleanup step

## Evidence Files

This directory contains:
- **test-results-cycodt.trx**: The actual TRX file from CI showing the bug
- **test-results.trx**: Regular dotnet test TRX file (for comparison)
- **ANALYSIS.md**: This document

## Reproduction Commands

To analyze the TRX file yourself:

```bash
# Count entries
grep -c "<UnitTestResult " test-results-cycodt.trx  # Should be 385
grep -c "<UnitTest " test-results-cycodt.trx        # Should be 384

# Find the missing test
grep '<UnitTestResult ' test-results-cycodt.trx | \
  sed -n 's/.*testId="\([^"]*\)".*/\1/p' | sort > result-ids.txt
grep '<UnitTest ' test-results-cycodt.trx | \
  sed -n 's/.*id="\([^"]*\)".*/\1/p' | sort > definition-ids.txt
diff result-ids.txt definition-ids.txt

# Identify the culprit
grep "b26c248a-79d7-a508-a280-0c70af8905cd" test-results-cycodt.trx
```

## Conclusion

The CI failure is caused by a missing UnitTest definition in the TRX file, but **this is not the matrix expansion bug** that PR #74 addressed. This is a separate issue affecting regular tests, specifically one test in the slash-title-commands suite.

The matrix fix appears to be working based on log evidence showing proper matrix parameter expansion. However, the TRX generation has a different bug that needs to be investigated and fixed before this PR can be merged.
