# Repository Filtering and Save/Load Workflow

**Status**: 🚧 **IN PROGRESS** - Phase A & B Complete, Phase C Starting  
**Date**: 2025-12-15  
**Last Updated**: 2025-12-15

## Overview

This document defines the implementation plan for three-level filtering (repos → files → lines) and progressive refinement workflow in cycodgr. The feature enables surgical precision in GitHub searches by pre-filtering repositories before searching files, and supports saving/loading results for iterative refinement.

## Feature Phases

### Phase A: Core Repo Pre-Filtering (MVP)

**Status:** ✅ **IMPLEMENTED** (2025-12-15)

**Delivers:** Basic repository filtering and save/load workflow

**Features:**
1. `--repo-file-contains "text"` - Find repos containing files with specified text (any file type)
2. `--save-repos repos.txt` - Save found repositories to file (`owner/name` format, one per line)
3. `@repos.txt` syntax - Load repositories from file (verify/fix existing implementation)

**Success Criteria:**
- Can pre-filter GitHub search to specific repositories
- Can save repository list to flat text file
- Can reload saved repositories for subsequent searches
- Dramatically reduces noise and search time for focused queries

**Example Usage:**
```bash
# Find and save repos
cycodgr --repo-file-contains "Microsoft.Extensions.AI" --save-repos ai-repos.txt

# Reuse saved repos
cycodgr @ai-repos.txt --cs-file-contains "anthropic"
```

**Implementation Details:**
- Added `RepoFileContains` property to `SearchCommand`
- Added `--repo-file-contains` command-line parsing
- Implemented `SearchCodeForRepositoriesAsync` in `GitHubSearchHelpers` for two-stage GitHub search
- Integrated repo pre-filtering into `HandleSearchCommandAsync` 
- Added `SaveRepos` property to `CycoGrCommand` base class
- Added `--save-repos` command-line parsing
- Implemented `FormatAsRepoList` and `FormatCodeAsRepoList` formatters
- Verified `@repos.txt` loading (already existed in codebase)

**Files Modified:**
- `src/cycodgr/CommandLineCommands/SearchCommand.cs`
- `src/cycodgr/CommandLine/CycoGrCommand.cs`
- `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`
- `src/cycodgr/Helpers/GitHubSearchHelpers.cs`
- `src/cycodgr/Program.cs`

**Testing:**
- See `todo/phase-a-manual-tests.md` for manual test procedures
- Automated tests TBD

### Phase B: Extension-Specific Repo Filtering

**Status:** ✅ **IMPLEMENTED** (2025-12-15)

**Delivers:** Precision targeting by file type

**Features:**
1. `--repo-csproj-file-contains "text"` - .NET projects (.csproj files)
2. `--repo-json-file-contains "text"` - Node.js projects (package.json, etc.)
3. `--repo-yaml-file-contains "text"` - Kubernetes/CI/CD configs (.yaml, .yml)
4. `--repo-py-file-contains "text"` - Python projects (.py files)
5. Additional extensions as needed (follows existing pattern)

**Implementation Note:** Generic pattern `--(repo-)(ext-)file-contains` makes this straightforward once core filtering works.

**Success Criteria:**
- Can target specific project types by extension
- All extension shortcuts work (--csproj, --json, --yaml, --py, etc.)
- Filters work independently and in combination

**Example Usage:**
```bash
# Find .NET projects using specific package
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --save-repos dotnet-ai-projects.txt

# Find Node.js projects using specific npm package
cycodgr --repo-json-file-contains "express" \
        --save-repos express-projects.txt

# Find projects with Kubernetes configs
cycodgr --repo-yaml-file-contains "kind: Deployment" \
        --save-repos k8s-projects.txt
```

**Implementation Details:**
- Added `RepoFileContainsExtension` property to `SearchCommand`
- Added extension-specific parsing pattern in `CycoGrCommandLineOptions`
- Pattern: `--repo-EXTENSION-file-contains` (e.g., --repo-csproj-file-contains)
- Reuses existing `MapExtensionToLanguage` for extension mapping
- Uses GitHub's `--extension` flag in code search
- Works with ANY extension (not just predefined ones)

**Files Modified:**
- `src/cycodgr/CommandLineCommands/SearchCommand.cs`
- `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`
- `src/cycodgr/Program.cs`

**Testing:**
- See `todo/phase-b-complete.md` for comprehensive test results
- Tested: csproj, json, yaml, py - all working ✅

### Phase C: Three-Level Precision (Complete Hierarchy)

**Delivers:** Full three-level filtering (repos → files → lines)

**Features:**
1. `--(ext)-file-contains` variants for file filtering
   - `--cs-file-contains "text"` - C# files
   - `--js-file-contains "text"` - JavaScript files
   - `--py-file-contains "text"` - Python files
   - All extension variants (follows existing pattern)
2. Complete three-level query support (repo + file + line filters)

**Success Criteria:**
- Can filter at all three levels in single query
- Extension-specific filters work at both repo and file levels
- Results are precisely targeted with minimal noise

**Example Usage:**
```bash
# Three-level query: repos → files → lines
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "anthropic" \
        --line-contains "AsChatClient" \
        --lines 20
```

**Result:** Shows how Anthropic clients are converted to IChatClient in projects using Microsoft.Extensions.AI

### Phase D: Progressive Refinement (Save/Load Files)

**Delivers:** Advanced workflow for multi-session refinement

**Features:**
1. `--save-file-paths paths.txt` - Save file paths in qualified format
   - Format: `owner/repo:src/Program.cs`
   - One path per line (flat text for @ syntax)
2. `--file-paths @paths.txt` - Load and search specific files
3. `--save-repo-urls urls.txt` - Save clone URLs
   - Format: `https://github.com/owner/name.git`
   - One URL per line

**Success Criteria:**
- Can save file paths from search results
- Can reload specific files for focused analysis
- Can save clone URLs for repository access
- All save formats are flat text compatible with @ syntax

**Example Usage:**
```bash
# Phase 1: Discover and save
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "Anthropic" \
        --save-repos repos.txt \
        --save-file-paths files.txt \
        --save-repo-urls clone-urls.txt

# Phase 2: Refine search on saved files
cycodgr --file-paths @files.txt \
        --line-contains "Configure" \
        --lines 30

# Phase 3: Clone repositories for local work
cat clone-urls.txt | xargs -n 1 git clone
```

### Phase E: Smart Behaviors (Polish)

**Delivers:** Intelligent filter behavior and consistency

**Features:**
1. Dual behavior for `--file-contains`
   - Without `--repo-*-file-contains`: acts as both repo and file filter
   - With `--repo-*-file-contains`: acts as file filter only
2. Universal `--contains` broadcasting
   - Applies to repos, files, and lines simultaneously
3. Consistency pass on all `--save-*` options

**Success Criteria:**
- Tool does "what I mean" not just "what I say"
- Consistent naming and behavior across all filter options
- Documentation is clear on filter interactions

**Example Usage:**
```bash
# --file-contains as dual filter (no repo filter specified)
cycodgr --cs-file-contains "anthropic"
# Acts as: find repos with .cs files containing "anthropic"
#      AND: find .cs files containing "anthropic" in those repos

# --file-contains as file filter only (repo filter specified)
cycodgr --repo-csproj-file-contains "Package.X" \
        --cs-file-contains "anthropic"
# Acts as: find repos with .csproj containing "Package.X"
#      THEN: find .cs files containing "anthropic" in those repos
```

## Implementation Approach

### Generic Pattern for Extension Filters

**Key Insight:** The codebase architecture makes extension-specific filters straightforward.

**Pattern:**
- `--(repo-)?(ext-)?file-contains` 
- Where `(ext-)` follows existing extension shortcuts (cs, js, py, json, yaml, md, etc.)

**Implementation:**
- Reuse existing extension mapping logic (see `MapExtensionToLanguage` in `CycoGrCommandLineOptions.cs`)
- Apply same pattern at repo level and file level
- GitHub API supports filtering by extension via `extension:csproj` in search queries

### Save Format Requirements

**All save formats must be flat text:**
- Compatible with `@file.txt` syntax (treat as single arg per line)
- Compatible with `@@file.txt` syntax (treat as single arg, entire file)
- One item per line
- No headers, no formatting, just data

**Formats:**
- `--save-repos`: `owner/name` (e.g., `microsoft/semantic-kernel`)
- `--save-repo-urls`: `https://github.com/owner/name.git`
- `--save-file-paths`: `owner/repo:path/to/file.cs`

### GitHub API Considerations

**Two-stage search for repo filtering:**
1. Code search with extension filter: `extension:csproj Microsoft.Extensions.AI`
2. Extract unique repository names from results
3. Use repo list for subsequent file searches

**Rate limiting:**
- Repo filtering is sequential (one API call per filter)
- File searches within repos can be parallelized (using existing `ParallelProcessor`)
- Balance speed with API rate limits

### Testing Strategy

**Test as we go (continuous validation):**
- Each phase should have test cases before marking complete
- Use cycodt YAML tests for feature validation
- Test combinations of filters, not just individual features
- Test save/load round-trips
- Test edge cases (empty results, special characters, large result sets)

**Test locations:**
- `tests/cycodt-yaml/cycodgr-repo-filtering.yaml` (new file)
- Integration tests for three-level queries
- Round-trip tests for save/load workflows

## Success Metrics

**Phase A Success:**
- Repo filtering reduces search time by 50%+ for targeted queries
- Save/load workflow works reliably
- Can demonstrate progressive refinement use case

**Phase B Success:**
- Extension-specific filtering works for .NET, Node.js, Python, Kubernetes
- Developers can target specific project types with precision

**Phase C Success:**
- Three-level queries work as demonstrated in examples
- Can answer complex questions like "how is X used in projects using Y"

**Phase D Success:**
- Multi-session workflows are practical and efficient
- File-level save/load enables deep-dive analysis

**Phase E Success:**
- Tool behavior is intuitive
- Documentation clearly explains filter interactions
- No confusion about when filters apply at which level

## Implementation Order

**Phase A → Phase B → Phase C → Phase D → Phase E**

**Rationale:**
1. **Phase A first:** Core value (repo filtering) + foundation for everything else
2. **Phase B next:** High-value extensions prove the pattern works
3. **Phase C completes vision:** Three-level hierarchy fully functional
4. **Phase D enables power users:** Advanced workflows for iterative refinement
5. **Phase E polish:** Smart behaviors after solid foundation exists

**Note:** Within Phase B, implement `--repo-csproj-file-contains` first (highest immediate value), then others.

## Future Enhancements (Post-Phase E)

**Not part of current plan, but documented for consideration:**
- Boolean operators (AND, OR, NOT) across filters
- Numeric filters (`--repo-stars-min`, `--repo-updated-after`)
- File size filters (`--file-size-min`, `--file-size-max`)
- Binary cache format for faster save/load
- SQL-like query language for complex filtering
- Pipeline operators for Unix-style composition

---

## Related Documentation

- [Unified Processing Architecture](./unified-processing-architecture.md) - Overall cycodgr/cycodmd architecture
- Section 5 of architecture doc documents the complete filter hierarchy design

---

**Ready to implement.** Start with Phase A.
