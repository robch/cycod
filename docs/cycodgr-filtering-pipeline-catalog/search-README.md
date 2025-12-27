# cycodgr Search Command - Filtering Pipeline

## Command Overview

The `search` command is the **default** and **only** command in cycodgr. It searches GitHub repositories and/or code based on various criteria.

**Command Invocation**:
```bash
cycodgr [options] [repo-patterns...]
```

Since `search` is the default, these are equivalent:
```bash
cycodgr microsoft/terminal --file-contains "ConPTY"
cycodgr search microsoft/terminal --file-contains "ConPTY"
```

## Search Modes

The command operates in different modes depending on which options are provided:

1. **Repository Metadata Mode** - Show repo information only (no content search)
   - Positional repo patterns without content flags
   - Example: `cycodgr microsoft/terminal`

2. **Unified Search Mode** - Search both repos and code
   - Uses `--contains`
   - Example: `cycodgr --contains "terminal emulator"`

3. **Code Search Mode** - Search code files within repos
   - Uses `--file-contains`
   - Example: `cycodgr --file-contains "ConPTY" --cs`

4. **Repository Search Mode** - Search repository metadata only
   - Uses `--repo-contains`
   - Example: `cycodgr --repo-contains "terminal"`

## Dual Behavior: --file-contains

`--file-contains` has **dual behavior**:

1. **With repo pre-filtering** (repo patterns, `--repo`, `--repo-file-contains`):
   - Acts as file content filter within specified repos
   
2. **Without repo pre-filtering**:
   - Acts as repo discovery mechanism (finds repos containing matching files)
   - Then shows file contents from those repos

## Filtering Pipeline Layers

| Layer | Purpose | Key Options |
|-------|---------|-------------|
| [1. Target Selection](search-layer-1.md) | What to search | Positional args, `--repo`, `--repos`, `--owner`, `--min-stars`, fork options |
| [2. Container Filtering](search-layer-2.md) | Which containers to include/exclude | `--repo-contains`, `--repo-file-contains`, `--file-contains`, `--language`, extension shortcuts, `--file-path` |
| [3. Content Filtering](search-layer-3.md) | What content to show | `--contains`, `--file-contains`, `--line-contains` |
| [4. Content Removal](search-layer-4.md) | What content to remove | `--exclude` |
| [5. Context Expansion](search-layer-5.md) | Expand around matches | `--lines`, `--lines-before-and-after` |
| [6. Display Control](search-layer-6.md) | How to present results | `--format`, `--max-results` |
| [7. Output Persistence](search-layer-7.md) | Where to save results | `--save-output`, `--save-json`, `--save-csv`, `--save-table`, `--save-urls`, `--save-repos`, `--save-file-paths`, `--save-repo-urls`, `--save-file-urls` |
| [8. AI Processing](search-layer-8.md) | AI-assisted analysis | `--instructions`, `--file-instructions`, `--{ext}-file-instructions`, `--repo-instructions` |
| [9. Actions on Results](search-layer-9.md) | What to do with results | `--clone`, `--max-clone`, `--clone-dir`, `--as-submodules` |

## Execution Flow

```
1. Parse command line options
2. [Layer 1] Determine target repos (patterns, explicit, pre-filtering)
3. [Layer 2] Pre-filter repos if --repo-file-contains specified
4. [Layer 2] Dual behavior: Use --file-contains for repo discovery if no repos specified
5. Determine search mode (metadata, unified, code, or repo search)
6. Execute appropriate search via GitHubSearchHelpers
7. [Layer 4] Apply exclude filters
8. [Layer 2] Apply file path filters if specified
9. [Layer 3] Apply line-contains filters if specified
10. [Layer 6] Format results according to --format
11. [Layer 5] Apply context expansion for code results
12. [Layer 8] Process with AI instructions if specified
13. [Layer 7] Save outputs in requested formats
14. [Layer 9] Clone repositories if requested
```

## Source Code Entry Points

- **Parser**: `CycoGrCommandLineOptions.cs` lines 32-326 (TryParseSearchCommandOptions)
- **Execution**: `Program.cs` lines 71-174 (HandleSearchCommandAsync)
- **Search Logic**: `GitHubSearchHelpers.cs`

## Example Usage Patterns

```bash
# Metadata only
cycodgr microsoft/terminal

# Code search with language filter
cycodgr --file-contains "ConPTY" --cs

# Repo discovery via file content
cycodgr --repo-csproj-file-contains "Microsoft.Extensions"

# Targeted file search in known repos
cycodgr microsoft/terminal --file-contains "ConPTY" --file-path "src/host/ConPTY.cpp"

# Clone matching repos
cycodgr --repo-contains "terminal emulator" --min-stars 100 --clone
```

## See Also

- [Layer 1: Target Selection - Full Documentation](search-layer-1.md)
- [Layer 1: Target Selection - Source Code Proof](search-layer-1-proof.md)
