# cycodmd CLI - Layer 5 Documentation Status

## Summary

Layer 5 (Context Expansion) documentation has been completed for **all 4 commands** in the cycodmd CLI.

**Date Completed**: 2024-12-XX

---

## Files Created

### File Search Command
✅ **[cycodmd-files-layer-5.md](cycodmd-files-layer-5.md)** - Layer 5 documentation  
✅ **[cycodmd-files-layer-5-proof.md](cycodmd-files-layer-5-proof.md)** - Source code proof

**Status**: **IMPLEMENTED** - Full context expansion functionality with `--lines`, `--lines-before`, `--lines-after`

---

### Web Search Command  
✅ **[cycodmd-websearch-layer-5.md](cycodmd-websearch-layer-5.md)** - Layer 5 documentation  
✅ **[cycodmd-websearch-layer-5-proof.md](cycodmd-websearch-layer-5-proof.md)** - Source code proof

**Status**: **NOT IMPLEMENTED** - Operates at page level, not line level

---

### Web Get Command
✅ **[cycodmd-webget-layer-5.md](cycodmd-webget-layer-5.md)** - Layer 5 documentation  
✅ **[cycodmd-webget-layer-5-proof.md](cycodmd-webget-layer-5-proof.md)** - Source code proof

**Status**: **NOT IMPLEMENTED** - Operates at page level, not line level

---

### Run Command
✅ **[cycodmd-run-layer-5.md](cycodmd-run-layer-5.md)** - Layer 5 documentation  
✅ **[cycodmd-run-layer-5-proof.md](cycodmd-run-layer-5-proof.md)** - Source code proof

**Status**: **NOT APPLICABLE** - Returns complete script output by design

---

## Layer 5: Context Expansion Overview

**Purpose**: Expand output to show additional lines before and after matching content.

**Implementation Details**:

| Command | Layer 5 Status | Reason |
|---------|----------------|--------|
| **File Search** | ✅ Implemented | Line-level filtering is core functionality |
| **Web Search** | ❌ Not Implemented | Page-level operations only |
| **Web Get** | ❌ Not Implemented | Page-level operations only |
| **Run** | ⚠️ Not Applicable | Returns complete output by design |

---

## Key Findings

### File Search Implementation

The File Search command has **full Layer 5 implementation**:

**CLI Options**:
- `--lines N`: Symmetric expansion (N lines before AND after)
- `--lines-before N`: Asymmetric expansion (only before)
- `--lines-after N`: Asymmetric expansion (only after)

**Implementation Location**:
- **Parsing**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (lines 137-153)
- **Storage**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs` (lines 99-100)
- **Execution**: `src/cycodmd/Program.cs` (lines 587-595)
- **Algorithm**: `src/common/Helpers/LineHelpers.cs` (lines 48-137)

**Key Features**:
- Smart overlap handling (merges close matches)
- Respects Layer 4 removal patterns (excluded lines don't appear as context)
- Gap separators when matches are far apart
- Integration with line numbers and highlighting

### Web Commands (Search & Get)

Both Web commands lack Layer 5 implementation because:

1. **No properties**: Command classes lack context expansion properties
2. **No CLI options**: Parser doesn't recognize context options for web commands
3. **No execution path**: `HandleWebSearchCommandAsync()` and `HandleWebGetCommand()` don't call filtering logic
4. **Design**: Operate at **page level**, not line level

**Alternative**: Save web content to files, then process with File Search for line-level operations.

### Run Command

The Run command **intentionally** doesn't filter or expand output:

1. **Philosophy**: Return complete, unmodified script output
2. **Use cases**: Debugging, logging, CI/CD pipelines need full output
3. **Design**: Pass-through executor, not a content filter

**Alternative**: Save run output to file, then process with File Search for filtering.

---

## Documentation Structure

Each command has:

### Layer Documentation (`*-layer-5.md`)
- Purpose of Layer 5
- Implementation status
- CLI options (if applicable)
- Data flow
- Examples
- Integration with other layers
- Alternatives (for non-implemented layers)

### Proof Documentation (`*-layer-5-proof.md`)
- Source file references with line numbers
- Parsing evidence
- Property storage evidence
- Execution path evidence
- Data flow summary
- Comparison with implemented features (for non-implemented layers)

---

## Navigation

- **[Main cycodmd Catalog](cycodmd-filtering-pipeline-catalog-README.md)** - Index of all commands and layers
- **[CLI Filtering Patterns Catalog](CLI-Filtering-Patterns-Catalog.md)** - Cross-tool analysis

---

## Next Steps

To complete the full 9-layer documentation for cycodmd, the following layers still need documentation:

### Per Command, Per Layer

| Command | Layers Completed | Layers Remaining |
|---------|------------------|------------------|
| **File Search** | 1, 2, 5 | 3, 4, 6, 7, 8, 9 |
| **Web Search** | 1, 2, 5 | 3, 4, 6, 7, 8, 9 |
| **Web Get** | 1, 2, 5 | 3, 4, 6, 7, 8, 9 |
| **Run** | 1, 2, 5 | 3, 4, 6, 7, 8, 9 |

**Total Files Remaining**: 4 commands × 6 layers × 2 files (layer + proof) = **48 files**

**Total Files Created So Far**: 4 commands × 3 layers × 2 files = **24 files**

---

## Related Documentation

- [Layer 1 Documentation Status](cycodmd-layer-1-final-verification.md)
- [Layer 2 Documentation Status](cycodmd-layer-2-final-verification.md)
- [Layer 4 Documentation Status](cycodmd-layer-4-final-verification.md)

---

**Generated**: 2024 | **Tool**: cycodmd CLI Documentation Project
