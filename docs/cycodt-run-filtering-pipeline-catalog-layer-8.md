# cycodt `run` - Layer 8: AI Processing

## Overview

The `run` command does **NOT** implement AI Processing (Layer 8).

## Implementation Status

❌ **Not Implemented**

## Rationale

The `run` command's purpose is to execute tests from YAML files and generate test reports. Test execution is deterministic and relies on predefined expectations in the YAML files themselves.

## Existing Test-Level AI Support

While the `run` command itself doesn't have AI processing, **individual tests** within YAML files CAN use AI via:

1. **`expect-instructions` in YAML**:
   ```yaml
   - name: My Test
     script: some-command.sh
     expect-instructions: "Output should contain success message"
   ```

2. **This is Layer 9 (Actions on Results)**, not Layer 8:
   - Layer 8 would be AI at the **command level** (analyzing all test results)
   - The YAML-level AI is **test-level** (validating individual test output)

## What Would Command-Level AI Look Like?

If Layer 8 were implemented for `run`, it could potentially:

1. **Test Result Analysis**: AI-powered analysis of test failures
   ```bash
   cycodt run --ai-analyze-failures
   # Output: AI identifies common failure patterns, root causes
   ```

2. **Test Execution Planning**: AI-driven test ordering
   ```bash
   cycodt run --ai-optimize-order
   # Run tests in optimal order based on historical failure rates
   ```

3. **Flaky Test Detection**: AI identifies non-deterministic tests
   ```bash
   cycodt run --ai-detect-flaky
   # Mark tests that pass/fail inconsistently
   ```

4. **Test Report Summarization**: Natural language test summaries
   ```bash
   cycodt run --ai-summarize-report
   # Generate English summary of test run
   ```

## Current Alternatives

Without command-level AI, users have:
- **Test-level AI**: `expect-instructions` in YAML (processed during execution)
- **Report formats**: TRX, JUnit XML (Layer 7)
- **Manual analysis**: Read test output and failure messages

## Comparison with Other Commands

| Command | AI Processing | Purpose |
|---------|---------------|---------|
| list | ❌ None | Display test names |
| run | ❌ None (command-level) | Execute tests |
| | ✅ Available (test-level) | Via YAML `expect-instructions` |
| expect check | ✅ `--instructions` | Validate output with AI |
| expect format | ❌ None | Transform text to regex |

## Relationship with Test-Level AI

```
Layer 8 (Command-Level AI) - NOT IMPLEMENTED
  ↓
Layer 9 (Actions: Execute Tests) - IMPLEMENTED
  ↓
  ↓ For each test in YAML:
  ↓
  Layer 8 (Test-Level AI) - IMPLEMENTED via expect-instructions
    ↓
    Validate test output against natural language expectations
```

The AI capabilities exist, but at the **test level**, not the **command level**.

## Related Layers

- **Layer 1 (Target Selection)**: Selects which tests to run
- **Layer 7 (Output Persistence)**: Generates test reports (TRX, JUnit)
- **Layer 9 (Actions on Results)**: Executes tests (which may use test-level AI)

## See Also

- [Layer 8 Proof](cycodt-run-filtering-pipeline-catalog-layer-8-proof.md) - Source code evidence
- [Layer 9: Actions on Results](cycodt-run-filtering-pipeline-catalog-layer-9.md) - Test execution (with test-level AI)
- [expect check Layer 8](cycodt-expect-check-filtering-pipeline-catalog-layer-8.md) - Command-level AI implementation
- [Test Framework README](../src/cycodt/TestFramework/README.md) - YAML test format with expect-instructions
