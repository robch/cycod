# cycodt Layer 7 (OUTPUT PERSISTENCE) - Summary

## Overview

This document summarizes Layer 7 (Output Persistence) implementation across all cycodt commands.

## Quick Reference

| Command | Layer 7 Status | Options | Default Output | File Formats |
|---------|---------------|---------|----------------|--------------|
| **list** | ⚠️ Limited | None | stdout only | N/A (text) |
| **run** | ✅ Full | `--output-file`, `--output-format` | `test-results.trx` | trx, junit |
| **expect check** | ❌ None | None | Exit code only | N/A |
| **expect format** | ✅ Full | `--save-output`, `--output` | stdout | Plain text |

## Detailed Analysis

### list Command - Console Only

**Status**: ⚠️ Limited  
**Documentation**: [Layer 7](cycodt-list-layer-7.md) | [Proof](cycodt-list-layer-7-proof.md)

**Features**:
- Console output via `ConsoleHelpers.WriteLine()`
- No native file output options
- Can redirect with shell operators (`>`, `>>`)

**Typical Usage**:
```bash
# Console output
cycodt list

# Redirect to file
cycodt list > test-list.txt
```

**Why No File Output?**:
- Discovery tool, not export tool
- Simple and focused
- Shell redirection sufficient

---

### run Command - Full Structured Output

**Status**: ✅ Full  
**Documentation**: [Layer 7](cycodt-run-layer-7.md) | [Proof](cycodt-run-layer-7-proof.md)

**Features**:
- `--output-file <path>` - Custom output file
- `--output-format <format>` - Format selection (trx, junit)
- Default: `test-results.trx` (TRX format)
- Automatic extension handling

**Supported Formats**:
- **TRX**: Visual Studio Test Results format (.trx)
- **JUnit**: JUnit-compatible XML format (.xml)

**Typical Usage**:
```bash
# Default TRX output
cycodt run

# Custom file
cycodt run --output-file my-results.trx

# JUnit format
cycodt run --output-format junit

# Both specified
cycodt run --output-file results.xml --output-format junit
```

**Implementation**:
- Properties: `OutputFile`, `OutputFormat`
- Resolution: `GetOutputFileAndFormat()` method
- Writers: `TrxXmlTestReporter`, `JunitXmlTestReporter`
- Orchestration: `YamlTestFrameworkConsoleHost.Finish()`

---

### expect check Command - Exit Code Only

**Status**: ❌ None  
**Documentation**: [Layer 7](cycodt-expect-check-layer-7.md) | [Proof](cycodt-expect-check-layer-7-proof.md)

**Features**:
- Exit code signaling (0=pass, 1=fail)
- Console messages for debugging
- No file output options

**Typical Usage**:
```bash
# Check expectations
some-command | cycodt expect check --regex "success"
if [ $? -eq 0 ]; then
    echo "Validation passed"
fi

# File-based
cycodt expect check --input output.txt --regex "expected"
```

**Why No File Output?**:
- Validation tool, not reporting tool
- Exit codes are standard shell mechanism
- Simplicity and composability

**Workarounds**:
```bash
# Capture exit code
cycodt expect check --input out.txt --regex "success"
echo "Result: $?" > validation-result.txt

# Capture console output
cycodt expect check --input out.txt --regex "success" 2>&1 | tee validation.log
```

---

### expect format Command - Full Text Output

**Status**: ✅ Full  
**Documentation**: [Layer 7](cycodt-expect-format-layer-7.md) | [Proof](cycodt-expect-format-layer-7-proof.md)

**Features**:
- `--save-output <file>` - Save to file
- `--output <file>` - Alias for `--save-output`
- Default: stdout
- Plain text output (formatted regex patterns)

**Typical Usage**:
```bash
# Console output
echo "Hello" | cycodt expect format

# File output
echo "Hello" | cycodt expect format --save-output patterns.txt

# File to file
cycodt expect format --input actual.txt --save-output expected.txt

# Using alias
cycodt expect format --input in.txt --output out.txt
```

**Implementation**:
- Property: `Output` (inherited from `ExpectBaseCommand`)
- Method: `WriteOutput()` (inherited)
  - If `Output` is null → console
  - If `Output` is set → file
- Invocation: `WriteOutput(formattedText)` in `ExecuteFormat()`

---

## Implementation Patterns

### Pattern 1: No Output Options (list, expect check)

**Characteristics**:
- No `--output-file` or `--save-output` parsing
- No output-related properties
- Direct console output via `ConsoleHelpers.WriteLine()`
- No file writing logic

**Code Pattern**:
```csharp
// Execute method
ConsoleHelpers.WriteLine(result);
return exitCode;
```

---

### Pattern 2: Format-Based Structured Output (run)

**Characteristics**:
- Multiple properties: `OutputFile`, `OutputFormat`
- Format validation and mapping
- Multiple format-specific writers
- Automatic extension handling

**Code Pattern**:
```csharp
// Properties
public string? OutputFile { get; set; }
public string OutputFormat { get; set; }

// Resolution
private void GetOutputFileAndFormat(out string file, out string format)
{
    format = OutputFormat;
    var ext = format switch { "trx" => "trx", "junit" => "xml", ... };
    file = OutputFile ?? $"test-results.{ext}";
    if (!file.EndsWith($".{ext}")) file += $".{ext}";
}

// Invocation
GetOutputFileAndFormat(out var file, out var format);
host.Finish(results, format, file);

// Writer selection
if (format == "trx") TrxXmlTestReporter.WriteResultsFile(...);
else if (format == "junit") JunitXmlTestReporter.WriteResultsFile(...);
```

---

### Pattern 3: Simple File/Console Toggle (expect format)

**Characteristics**:
- Single property: `Output`
- Base class abstraction: `WriteOutput()`
- Conditional output destination
- No format options (single output type)

**Code Pattern**:
```csharp
// Base class
protected void WriteOutput(string text)
{
    if (string.IsNullOrEmpty(Output))
        ConsoleHelpers.WriteLine(text, overrideQuiet: true);
    else
        FileHelpers.WriteAllText(Output, text);
}

// Subclass
var result = ProcessInput(input);
WriteOutput(result);
```

---

## Cross-Command Comparison

### Complexity Levels

**Simplest** (expect check):
- No output options
- Exit code only
- Single purpose (validation)

**Simple** (expect format):
- Single output option (`--save-output`)
- File or console
- Inherited abstraction

**Moderate** (list):
- Console only by design
- Minimal output
- Shell-redirectable

**Complex** (run):
- Multiple options (`--output-file`, `--output-format`)
- Multiple formats (trx, junit)
- Format-specific writers
- Extension handling

---

## Design Philosophies

### When to Provide File Output

**YES** (file output provided):
- **run**: Test results need persistence for CI/CD, reporting
- **expect format**: Generated patterns need to be saved for YAML tests

**NO** (file output not provided):
- **list**: Discovery/preview tool, shell redirection sufficient
- **expect check**: Validation tool, exit code is the result

### Key Decision Factors

1. **Primary use case**: Is persistence a core requirement?
2. **Output nature**: Structured data or transient status?
3. **Integration context**: Shell/CI usage vs. file-based workflows?
4. **Tool philosophy**: Single-purpose validator vs. result generator?

---

## Enhancement Opportunities

### list Command

Potential additions:
- `--save-output <file>` for consistency
- `--format json` for structured output
- `--format csv` for spreadsheet import

### expect check Command

Potential additions:
- `--save-report <file>` for validation report
- `--report-format json` (structured results)
- `--report-format junit` (test result format)

### run Command

Already comprehensive. Possible additions:
- `--format json` for JSON results
- `--format markdown` for human-readable reports
- Template-based filenames (`results-{time}.trx`)

### expect format Command

Potential additions:
- `--append` mode (add to existing file)
- `--format yaml` (direct YAML output)
- Batch processing (multiple files)

---

## Files Created

This documentation set includes:

1. **Main README**: [cycodt-filtering-pipeline-catalog-README.md](cycodt-filtering-pipeline-catalog-README.md)

2. **Command READMEs**:
   - [cycodt-list-catalog-README.md](cycodt-list-catalog-README.md)
   - [cycodt-run-catalog-README.md](cycodt-run-catalog-README.md)
   - [cycodt-expect-check-catalog-README.md](cycodt-expect-check-catalog-README.md)
   - [cycodt-expect-format-catalog-README.md](cycodt-expect-format-catalog-README.md)

3. **Layer 7 Documentation**:
   - [cycodt-list-layer-7.md](cycodt-list-layer-7.md)
   - [cycodt-run-layer-7.md](cycodt-run-layer-7.md)
   - [cycodt-expect-check-layer-7.md](cycodt-expect-check-layer-7.md)
   - [cycodt-expect-format-layer-7.md](cycodt-expect-format-layer-7.md)

4. **Layer 7 Proof Documents**:
   - [cycodt-list-layer-7-proof.md](cycodt-list-layer-7-proof.md)
   - [cycodt-run-layer-7-proof.md](cycodt-run-layer-7-proof.md)
   - [cycodt-expect-check-layer-7-proof.md](cycodt-expect-check-layer-7-proof.md)
   - [cycodt-expect-format-layer-7-proof.md](cycodt-expect-format-layer-7-proof.md)

---

## Related Documentation

- **[Main CLI Filtering Patterns Catalog](CLI-Filtering-Patterns-Catalog.md)** - Cross-tool analysis
- **[cycodt Main README](cycodt-filtering-pipeline-catalog-README.md)** - All layers, all commands

---

**Last Updated**: 2024 (based on source code analysis)
