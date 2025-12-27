# cycodt `run` - Layer 7: OUTPUT PERSISTENCE

**[← Back to run Command Catalog](cycodt-run-catalog-README.md)**

## Overview

The `run` command has **full** output persistence capabilities with support for multiple structured formats and file output options.

## Implementation Status

**Status**: ✅ Full - Structured file output with format control

## Features

### Output File Options

1. **`--output-file <path>`**
   - Specify custom output file path
   - Default: `test-results.{ext}` (extension determined by format)
   - Automatically appends correct extension if missing

2. **`--output-format <format>`**
   - Specify output format
   - Values: `trx` (default), `junit`
   - Determines file extension and structure

### Supported Formats

#### TRX Format
- **Extension**: `.trx`
- **Description**: Visual Studio Test Results XML format
- **Default**: Yes
- **Generator**: `TrxXmlTestReporter`
- **Compatible with**: Visual Studio Test Explorer, Azure DevOps, TRX viewers

#### JUnit Format
- **Extension**: `.xml`
- **Description**: JUnit-compatible XML format
- **Generator**: `JunitXmlTestReporter`
- **Compatible with**: JUnit parsers, CI/CD systems (Jenkins, GitLab CI, GitHub Actions)

## Default Behavior

```bash
# No options specified
cycodt run
# Output: test-results.trx (TRX format)

# Format specified, no file
cycodt run --output-format junit
# Output: test-results.xml (JUnit format)

# File specified, no format
cycodt run --output-file my-results.xml
# Output: my-results.xml.trx (TRX format, extension added)

# Both specified
cycodt run --output-file results.xml --output-format junit
# Output: results.xml (JUnit format)
```

## Usage Patterns

### Basic Usage

```bash
# Default TRX output
cycodt run

# Custom output file
cycodt run --output-file test-results-2024-01-15.trx

# JUnit format
cycodt run --output-format junit

# JUnit with custom file
cycodt run --output-format junit --output-file junit-results.xml
```

### CI/CD Integration

```bash
# Azure DevOps (TRX)
cycodt run --output-file $(Build.ArtifactStagingDirectory)/test-results.trx

# Jenkins/GitLab CI (JUnit)
cycodt run --output-format junit --output-file test-results.xml

# GitHub Actions (JUnit)
cycodt run --output-format junit --output-file test-results/junit.xml
```

## Implementation Details

**Implementation File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs`

**Key Components**:

1. **Properties** (lines 8-9):
   - `OutputFile` - User-specified or default output file path
   - `OutputFormat` - User-specified or default format ("trx" default)

2. **Output Logic** (`GetOutputFileAndFormat()`, lines 52-67):
   - Determines final file path and format
   - Maps format to file extension
   - Appends extension if missing

3. **Reporter Invocation** (`ExecuteTestRun()`, lines 40-41):
   - Passes file and format to `YamlTestFrameworkConsoleHost.Finish()`

4. **Reporter Selection** (`YamlTestFrameworkConsoleHost.Finish()`, lines 93-100):
   - Conditionally invokes `TrxXmlTestReporter` or `JunitXmlTestReporter`

## Console Feedback

After test execution, the results file path is displayed:

```
TEST RESULT SUMMARY:

Passed: 15 (100.0%)

Tests: 15 (1.23s)
Results: C:\path\to\test-results.trx
```

## File Structure Examples

### TRX Format Structure
```xml
<?xml version="1.0"?>
<TestRun id="..." name="..." xmlns="...">
  <TestSettings ... />
  <Results>
    <UnitTestResult executionId="..." testId="..." testName="..." outcome="Passed" ...>
      <!-- Test result details -->
    </UnitTestResult>
    <!-- More results -->
  </Results>
  <TestDefinitions>
    <UnitTest id="..." name="...">
      <!-- Test definition -->
    </UnitTest>
    <!-- More definitions -->
  </TestDefinitions>
</TestRun>
```

### JUnit Format Structure
```xml
<?xml version="1.0"?>
<testsuites>
  <testsuite name="..." tests="15" failures="0" errors="0" time="1.23">
    <testcase name="..." classname="..." time="0.12" />
    <!-- More test cases -->
  </testsuite>
</testsuites>
```

## Comparison with Other Commands

| Command | Layer 7 Support | Options Available |
|---------|----------------|-------------------|
| **list** | ⚠️ Limited | Console only (redirect) |
| **run** | ✅ Full | `--output-file`, `--output-format` (trx, junit) |
| **expect check** | ❌ None | Exit code only |
| **expect format** | ✅ Full | `--save-output`, `--output` |

## Extension Mapping Logic

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestRunCommand.cs` (lines 52-67)

```
Format    → Extension
------    → ---------
"trx"     → ".trx"
"junit"   → ".xml"
(other)   → Exception
```

If output file doesn't end with correct extension, it's automatically appended:
- Input: `results`, Format: `trx` → Output: `results.trx`
- Input: `results.xml`, Format: `trx` → Output: `results.xml.trx`
- Input: `results.trx`, Format: `trx` → Output: `results.trx`

## Opportunities for Enhancement

Potential future enhancements (not currently implemented):

1. **Additional formats**:
   - `--output-format json` (structured JSON results)
   - `--output-format markdown` (human-readable report)
   - `--output-format html` (interactive HTML report)

2. **Template-based file naming**:
   - `--output-file test-results-{time}.trx`
   - `--output-file results-{date}-{user}.xml`

3. **Multiple simultaneous outputs**:
   - `--output-file results.trx --output-file results.xml`

4. **Output directory control**:
   - `--output-dir test-results/`

## Related Layers

- **[Layer 1: Target Selection](cycodt-run-layer-1.md)** - What tests to run
- **[Layer 2: Container Filter](cycodt-run-layer-2.md)** - Which tests to include
- **[Layer 6: Display Control](cycodt-run-layer-6.md)** - Console output formatting
- **[Layer 9: Actions on Results](cycodt-run-layer-9.md)** - Test execution
- **[Proof Document](cycodt-run-filtering-pipeline-catalog-layer-7-proof.md)** - Source code evidence

---

**[View Proof →](cycodt-run-filtering-pipeline-catalog-layer-7-proof.md)**
