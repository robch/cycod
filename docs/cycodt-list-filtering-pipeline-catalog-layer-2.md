# cycodt list - Layer 2: Container Filtering

## Overview

**Layer 2** in the filter pipeline is **Container Filtering** - filtering which test files (containers) to include/exclude based on their properties or content.

## Purpose

In the context of `cycodt list`, Layer 2 handles:
1. File-level exclusion patterns
2. `.cycodtignore` file support

**Important**: Unlike cycodmd or cycodgr, cycodt does NOT have file-content-based filtering at Layer 2. File filtering is purely pattern-based from Layer 1.

## Command-Line Options

### File Exclusion (from Layer 1)

The following options affect which test files are discovered:
- `--exclude-files <pattern>` / `--exclude <pattern>`
- `.cycodtignore` file

These are technically Layer 1 (Target Selection) operations, as they determine which files are discovered, not which are filtered after discovery.

### NO Content-Based File Filtering

cycodt does NOT support:
- `--file-contains` (filter files by content)
- `--file-not-contains` (exclude files by content)
- Extension-specific shortcuts (e.g., `--yaml-file-contains`)

**Reason**: Test framework operates on test cases within files, not file content filtering.

## Implementation Details

### Layer 2 is Effectively EMPTY for cycodt

**Source**: `TestBaseCommand.cs` lines 47-61

```csharp
protected IList<TestCase> FindAndFilterTests()
{
    var files = FindTestFiles();          // Layer 1: Target selection
    var filters = GetTestFilters();
    
    var tests = files.SelectMany(file => GetTestsFromFile(file));  // No Layer 2 filtering!
    
    var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();  // Layer 4
    var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();   // Layers 3 & 4
    
    return filtered;
}
```

**Evidence**:
- Line 49: `FindTestFiles()` returns `List<FileInfo>` (Layer 1 complete)
- Line 54: ALL files are loaded with `GetTestsFromFile()` - no filtering between Layer 1 and 3
- No conditional logic between file discovery and test extraction

### Why No Layer 2?

1. **Design Decision**: Test files are YAML configurations, not source code
2. **Performance**: YAML parsing is fast; no benefit to content pre-filtering
3. **Simplicity**: Test names and properties are available after parsing (Layer 3)
4. **Consistency**: Filtering happens at test-case level, not file level

## Comparison with Other Tools

| Tool | Layer 2 Container Filtering |
|------|----------------------------|
| **cycodmd** | `--file-contains`, `--file-not-contains`, `--cs-file-contains` |
| **cycodj** | Time-based (--after, --before), conversation metadata |
| **cycodgr** | `--repo-contains`, `--repo-file-contains`, `--language` |
| **cycodt** | ❌ NONE (jumps from Layer 1 to Layer 3) |

## Code Flow

```
TestListCommand.ExecuteList()
  ↓
TestBaseCommand.FindAndFilterTests()
  ↓
  ├─ FindTestFiles()  ← Layer 1: File discovery with exclusions
  │  └─ Returns: List<FileInfo>
  ↓
  ├─ [NO LAYER 2 FILTERING HERE]  ← SKIP directly to test extraction
  ↓
  ├─ GetTestsFromFile() for ALL files  ← Load test cases
  │  └─ Returns: IEnumerable<TestCase>
  ↓
  └─ FilterOptionalTests() + YamlTestCaseFilter.FilterTestCases()  ← Layers 3 & 4
```

## Potential Future Enhancement

Layer 2 filtering could be added for:

1. **File Metadata Filtering**:
   ```bash
   cycodt list --modified-after 7d    # Files modified in last 7 days
   cycodt list --file-name-contains "api"  # File names containing "api"
   ```

2. **File-Level Tags**:
   ```yaml
   # In test file
   file-tags: [api, integration]
   
   # Command line
   cycodt list --file-tag api
   ```

3. **Test Count Filtering**:
   ```bash
   cycodt list --min-tests 5   # Files with at least 5 tests
   cycodt list --max-tests 20  # Files with at most 20 tests
   ```

However, these are not currently implemented.

## Related Documentation

- [Layer 1: Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md) - File discovery
- [Layer 3: Content Filtering](cycodt-list-filtering-pipeline-catalog-layer-3.md) - Test case filtering
- [Layer 2 Proof](cycodt-list-filtering-pipeline-catalog-layer-2-proof.md) - Source code evidence
