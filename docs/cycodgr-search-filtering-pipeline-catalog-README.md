# cycodgr search Command - Filtering Pipeline Catalog

## Overview

The `search` command is the default command for cycodgr. It searches GitHub repositories and code with advanced filtering capabilities across 9 conceptual layers.

## Complete Pipeline Documentation

| Layer | Name | Description | Catalog | Proof |
|-------|------|-------------|---------|-------|
| 1 | Target Selection | What to search (repos, code, patterns) | [Layer 1](cycodgr-search-filtering-pipeline-catalog-layer-1.md) | [Proof 1](cycodgr-search-filtering-pipeline-catalog-layer-1-proof.md) |
| 2 | Container Filter | Which repos/files to include/exclude | [Layer 2](cycodgr-search-filtering-pipeline-catalog-layer-2.md) | [Proof 2](cycodgr-search-filtering-pipeline-catalog-layer-2-proof.md) |
| 3 | Content Filter | What content within files to show | [Layer 3](cycodgr-search-filtering-pipeline-catalog-layer-3.md) | [Proof 3](cycodgr-search-filtering-pipeline-catalog-layer-3-proof.md) |
| 4 | Content Removal | What content to actively remove | [Layer 4](cycodgr-search-filtering-pipeline-catalog-layer-4.md) | [Proof 4](cycodgr-search-filtering-pipeline-catalog-layer-4-proof.md) |
| 5 | Context Expansion | How to expand around matches | [Layer 5](cycodgr-search-filtering-pipeline-catalog-layer-5.md) | [Proof 5](cycodgr-search-filtering-pipeline-catalog-layer-5-proof.md) |
| 6 | Display Control | How to present results | [Layer 6](cycodgr-search-filtering-pipeline-catalog-layer-6.md) | [Proof 6](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md) |
| 7 | Output Persistence | Where to save results | [Layer 7](cycodgr-search-filtering-pipeline-catalog-layer-7.md) | [Proof 7](cycodgr-search-filtering-pipeline-catalog-layer-7-proof.md) |
| 8 | AI Processing | AI-assisted analysis | [Layer 8](cycodgr-search-filtering-pipeline-catalog-layer-8.md) | [Proof 8](cycodgr-search-filtering-pipeline-catalog-layer-8-proof.md) |
| 9 | Actions on Results | What to do with results | [Layer 9](cycodgr-search-filtering-pipeline-catalog-layer-9.md) | [Proof 9](cycodgr-search-filtering-pipeline-catalog-layer-9-proof.md) |

## Command Properties

From `SearchCommand.cs`:

```csharp
public class SearchCommand : CycoGrCommand
{
    // Target Selection (Layer 1)
    public List<string> RepoPatterns { get; set; }
    public int MaxResults { get; set; }

    // Container Filter (Layer 2)
    public string Contains { get; set; }
    public string FileContains { get; set; }
    public string RepoContains { get; set; }
    public string RepoFileContains { get; set; }
    public string RepoFileContainsExtension { get; set; }
    public List<string> FilePaths { get; set; }
    public string Language { get; set; }
    public string Owner { get; set; }
    public int MinStars { get; set; }
    public string SortBy { get; set; }
    public bool IncludeForks { get; set; }
    public bool ExcludeForks { get; set; }
    public bool OnlyForks { get; set; }

    // Content Filter (Layer 3)
    public List<string> LineContainsPatterns { get; set; }

    // Context Expansion (Layer 5)
    public int LinesBeforeAndAfter { get; set; }

    // Display Control (Layer 6)
    public string Format { get; set; }

    // AI Processing (Layer 8)
    public List<Tuple<string, string>> FileInstructionsList { get; set; }
    public List<string> RepoInstructionsList { get; set; }
    public List<string> InstructionsList { get; set; }

    // Actions on Results (Layer 9)
    public bool Clone { get; set; }
    public int MaxClone { get; set; }
    public string CloneDirectory { get; set; }
    public bool AsSubmodules { get; set; }
}
```

Inherited from `CycoGrCommand`:

```csharp
// Output Persistence (Layer 7)
public string SaveOutput;
public string SaveJson;
public string SaveCsv;
public string SaveTable;
public string SaveUrls;
public string SaveRepos;
public string SaveFilePaths;
public string SaveRepoUrls;
public string SaveFileUrls;

// Container Filter (Layer 2) & Content Removal (Layer 4)
public List<string> Repos;
public List<string> Exclude;
```

## Execution Flow

From `Program.cs:HandleSearchCommandAsync()`:

1. **Stage 1**: Pre-filter repos using `--repo-file-contains` (lines 76-102)
2. **Stage 2**: Dual behavior for `--file-contains` (lines 104-136)
3. **Stage 3**: Execute main search based on search mode:
   - Repository metadata only (lines 144-147)
   - Unified search (--contains) (lines 149-152)
   - Code search (--file-contains) (lines 154-157)
   - Repository search (--repo-contains) (lines 159-162)

## Search Modes

### 1. Repository Metadata Display
**Trigger**: Positional repo patterns without content search flags
```bash
cycodgr microsoft/terminal wezterm/wezterm
```
**Handler**: `ShowRepoMetadataAsync()` (lines 176-228)

### 2. Unified Search
**Trigger**: `--contains` flag
```bash
cycodgr --contains "terminal emulator"
```
**Handler**: `HandleUnifiedSearchAsync()` (lines 230-297)
**Searches**: Both repositories AND code

### 3. Code Search
**Trigger**: `--file-contains` flag (with repo pre-filtering)
```bash
cycodgr --file-contains "ConPTY" --language cpp
```
**Handler**: `HandleCodeSearchAsync()` (lines 371-436)

### 4. Repository Search
**Trigger**: `--repo-contains` flag
```bash
cycodgr --repo-contains "emulator" --min-stars 1000
```
**Handler**: `HandleRepoSearchAsync()` (lines 299-369)

## Multi-Level Hierarchy

cycodgr operates on a 4-level hierarchy:

1. **Owner/Organization** (`--owner`)
2. **Repository** (positional args, `--repo`, `--repos`, pre-filtering)
3. **File** (`--file-contains`, `--language`, `--file-path`)
4. **Line** (`--line-contains`, context expansion)

## Key Helper Methods

### GitHub API Interaction
- `GitHubSearchHelpers.SearchRepositoriesAsync()` - Search repos by keywords
- `GitHubSearchHelpers.SearchCodeAsync()` - Search code in files
- `GitHubSearchHelpers.SearchCodeForRepositoriesAsync()` - Extract repo list from code search
- `GitHubSearchHelpers.GetRepositoryMetadataAsync()` - Fetch single repo metadata
- `GitHubSearchHelpers.CloneRepositoriesAsync()` - Clone repos locally

### Output Formatting
- `FormatRepoOutput()` - Format repository results
- `FormatAndOutputCodeResults()` - Format code search results
- `FormatAsJson()`, `FormatAsCsv()`, `FormatAsTable()` - Multiple output formats
- `ApplyExcludeFilters()` - Apply exclusion patterns

### File Processing
- `ProcessFileGroupAsync()` - Process individual file matches
- `LineHelpers.FilterAndExpandContext()` - Filter lines and expand context
- `DetectLanguageFromPath()` - Infer language from file extension
- `ConvertToRawUrl()` - Convert GitHub blob URLs to raw URLs

## Back to Main Catalog

[← Back to cycodgr Main Catalog](cycodgr-filter-pipeline-catalog-README.md)
