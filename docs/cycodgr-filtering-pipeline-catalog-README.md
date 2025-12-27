# cycodgr Filtering Pipeline Catalog

## Overview

This catalog documents the **complete filtering pipeline** for `cycodgr` (GitHub Search and Repository Management CLI), providing detailed evidence-based analysis of how each of the 9 conceptual layers is implemented.

## Command Structure

cycodgr has **one primary command**:
- **search** (default command) - Search GitHub repositories and code

## Navigation

### Search Command Documentation

The search command implements all 9 filtering pipeline layers:

1. [Layer 1: Target Selection](cycodgr-search-layer-1.md)  
   [Proof](cycodgr-search-layer-1-proof.md)

2. [Layer 2: Container Filtering](cycodgr-search-layer-2.md)  
   [Proof](cycodgr-search-layer-2-proof.md)

3. [Layer 3: Content Filtering](cycodgr-search-layer-3.md)  
   [Proof](cycodgr-search-layer-3-proof.md)

4. [Layer 4: Content Removal](cycodgr-search-layer-4.md)  
   [Proof](cycodgr-search-layer-4-proof.md)

5. [Layer 5: Context Expansion](cycodgr-search-layer-5.md)  
   [Proof](cycodgr-search-layer-5-proof.md)

6. [Layer 6: Display Control](cycodgr-search-layer-6.md)  
   [Proof](cycodgr-search-layer-6-proof.md)

7. [Layer 7: Output Persistence](cycodgr-search-layer-7.md)  
   [Proof](cycodgr-search-layer-7-proof.md)

8. [Layer 8: AI Processing](cycodgr-search-layer-8.md)  
   [Proof](cycodgr-search-layer-8-proof.md)

9. [Layer 9: Actions on Results](cycodgr-search-layer-9.md)  
   [Proof](cycodgr-search-layer-9-proof.md)

## Quick Reference

### Command Line Options by Layer

| Layer | Options |
|-------|---------|
| **1. Target Selection** | Positional args (repo patterns), `--repo`, `--repos`, `--owner`, `--min-stars`, `--max-results`, `--include-forks`, `--exclude-fork`, `--only-forks`, `--sort` |
| **2. Container Filtering** | `--repo-contains`, `--repo-file-contains`, `--repo-{ext}-file-contains`, `--file-contains`, `--{ext}-file-contains`, `--language`, `--extension`, `--in-files`, `--{lang}` shortcuts, `--file-path`, `--file-paths` |
| **3. Content Filtering** | `--contains`, `--file-contains`, `--line-contains` |
| **4. Content Removal** | `--exclude` |
| **5. Context Expansion** | `--lines-before-and-after`, `--lines` |
| **6. Display Control** | `--format` (detailed, repos, urls, files, json, csv, table) |
| **7. Output Persistence** | `--save-output`, `--save-json`, `--save-csv`, `--save-table`, `--save-urls`, `--save-repos`, `--save-file-paths`, `--save-repo-urls`, `--save-file-urls` |
| **8. AI Processing** | `--instructions`, `--file-instructions`, `--{ext}-file-instructions`, `--repo-instructions` |
| **9. Actions on Results** | `--clone`, `--max-clone`, `--clone-dir`, `--as-submodules` |

## Key Features

### Multi-Level Search Hierarchy

cycodgr operates on a **4-level hierarchy**:
1. **Organization/Owner** level (e.g., `microsoft`)
2. **Repository** level (e.g., `microsoft/terminal`)
3. **File** level (e.g., `src/cascadia/TerminalCore/Terminal.cpp`)
4. **Line** level (specific lines of code within files)

### Dual-Purpose --file-contains

The `--file-contains` option has **dual behavior**:
- **Without repo pre-filtering**: Acts as Layer 2 (finds repos containing matching files)
- **With repo pre-filtering**: Acts as Layer 3 (filters content within already-selected repos)

See [Layer 2 Proof](cycodgr-search-layer-2-proof.md) and [Layer 3 Proof](cycodgr-search-layer-3-proof.md) for details.

### Search Execution Modes

Based on which options are specified, cycodgr executes in different modes:

1. **Repo Metadata Mode**: Positional repo patterns only (no content search)
2. **Unified Search Mode**: `--contains` (searches both repos AND code)
3. **Code Search Mode**: `--file-contains` (searches code within repos)
4. **Repo Search Mode**: `--repo-contains` (searches repo metadata only)

See Program.cs lines 143-166 for the decision logic.

## Source Code Structure

### Key Files

- **CommandLineOptions**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`
- **SearchCommand**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`
- **CycoGrCommand**: `src/cycodgr/CommandLine/CycoGrCommand.cs` (base class)
- **Program.cs**: `src/cycodgr/Program.cs` (execution logic)
- **GitHubSearchHelpers**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

### Data Models

- **RepoInfo**: `src/cycodgr/Models/RepoInfo.cs`
- **CodeMatch**: `src/cycodgr/Models/CodeMatch.cs`

## Documentation Conventions

### Proof Files

Each proof file contains:
- **Line-by-line source code references** with exact line numbers
- **Data flow analysis** showing how options affect behavior
- **Code snippets** demonstrating implementation
- **Call stacks** showing how components interact

### Evidence Format

```
Option: --example-option
Parser Location: CycoGrCommandLineOptions.cs, lines X-Y
Property Assignment: SearchCommand.cs, line Z
Execution Logic: Program.cs, lines A-B
Implementation: GitHubSearchHelpers.cs, lines C-D
```

## Version Information

- **Tool**: cycodgr (GitHub Search CLI)
- **Documentation Date**: 2025-01-XX
- **Source Code**: Reflects current state of repository
- **Analysis Scope**: All 9 filtering pipeline layers for search command

---

[Back to Main Catalog](CLI-Filtering-Patterns-Catalog.md)
