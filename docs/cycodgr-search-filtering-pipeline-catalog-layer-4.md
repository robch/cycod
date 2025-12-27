# cycodgr Search Command - Layer 4: Content Removal

[← Back to README](cycodgr-filtering-pipeline-catalog-README.md) | [Proof →](cycodgr-search-filtering-pipeline-catalog-layer-4-proof.md)

## Overview

**Layer 4: Content Removal** actively removes or excludes content from display AFTER initial matching. This layer operates post-filter, removing items that match exclusion criteria.

## Purpose

Remove unwanted results from the pipeline without affecting the initial search criteria. This allows users to:
- Exclude specific repositories or URLs by pattern
- Filter out noise from results
- Remove known uninteresting matches

## Implementation in cycodgr Search

### Primary Mechanism: --exclude

cycodgr implements content removal through a single, powerful option:

#### `--exclude <pattern> [<pattern> ...]`

**Purpose**: Exclude results matching regex pattern(s)

**Behavior**:
- Accepts multiple patterns (applied as OR logic - match any pattern = excluded)
- Applies regex matching (case-insensitive)
- Applied AFTER search results are retrieved
- Context-sensitive: applies to repository URLs in repo search, file URLs in code search

**Examples**:
```bash
# Exclude forks (by URL pattern)
cycodgr --repo-contains "terminal" --exclude "fork"

# Exclude specific organizations
cycodgr --file-contains "async" --exclude "microsoft" --exclude "google"

# Exclude archived repositories (URL contains 'archived')
cycodgr --contains "rust" --exclude "archived"

# Multiple patterns in one call
cycodgr "microsoft/*" --file-contains "ConPTY" --exclude "test" "sample" "demo"
```

### Context-Sensitive Application

The `--exclude` filter applies differently depending on search type:

1. **Repository Search** (`--repo-contains` or `--contains`):
   - Filters on `RepoInfo.Url`
   - Removes entire repositories from results

2. **Code Search** (`--file-contains` or `--contains` with code results):
   - Filters on `CodeMatch.Repository.Url`
   - Removes files from repositories matching the pattern
   - Entire repos are filtered out if all their files are excluded

### Implementation Details

**Storage**:
- Property: `CycoGrCommand.Exclude` (List<string>)
- Defined in: `src/cycodgr/CommandLine/CycoGrCommand.cs`
- Inherited by: `SearchCommand`

**Application**:
- Method: `ApplyExcludeFilters<T>(items, excludePatterns, urlGetter)`
- Location: `src/cycodgr/Program.cs`, lines 1343-1377
- Called from:
  - `HandleUnifiedSearchAsync()` - line 267-268
  - `HandleRepoSearchAsync()` - line 328
  - `HandleCodeSearchAsync()` - line 401

**Filtering Algorithm**:
1. If no exclude patterns, return items unchanged
2. For each item, extract URL using provided `urlGetter` function
3. Test URL against each exclude pattern (regex, case-insensitive)
4. Exclude item if ANY pattern matches
5. Report count of excluded items to user

## Characteristics

### Strengths
- **Simple**: Single option for all exclusion needs
- **Flexible**: Regex patterns allow complex matching
- **Context-aware**: Adapts to search type
- **Multiple patterns**: Can specify many patterns

### Limitations
- **No fine-grained control**: Cannot exclude at different levels (repo vs file vs line)
- **URL-only**: Only filters based on URLs, not other metadata
- **No positive exclusion**: Cannot exclude "everything except X"
- **No line-level removal**: Unlike cycodmd's `--remove-all-lines`, cannot remove specific lines from code display

## Data Flow

```
Search Results (RepoInfo[] or CodeMatch[])
    ↓
ApplyExcludeFilters()
    ↓
For each item:
    Extract URL (repo URL or file's repo URL)
    ↓
    Test against each exclude pattern
    ↓
    If ANY match → EXCLUDE
    ↓
Filtered Results
    ↓
Display/Save
```

## Comparison to Other Tools

### cycodmd
- Has `--remove-all-lines` for line-level removal
- Has `--file-not-contains` for file-level exclusion
- `--exclude` works on filename patterns (not URLs)

### cycodj
- No explicit content removal layer
- Could benefit from similar exclusion mechanism

### Pattern
cycodgr's `--exclude` is conceptually similar to cycodmd's `--exclude`, but:
- cycodgr: URL-based (for web resources)
- cycodmd: filename/path-based (for local files)

## Missing Features

Compared to other tools, cycodgr could benefit from:

1. **Line-level removal**: `--remove-lines <pattern>` to exclude specific lines from code display
2. **File-level exclusion**: `--exclude-files <pattern>` separate from repo exclusion
3. **Metadata-based exclusion**: `--exclude-language`, `--exclude-below-stars`, etc.
4. **Positive exclusion**: `--only <pattern>` (inverse of exclude)

## Usage Patterns

### Common Use Cases

1. **Exclude test repositories**:
   ```bash
   cycodgr --repo-contains "machine learning" --exclude "test|sample|demo"
   ```

2. **Exclude specific organizations**:
   ```bash
   cycodgr --file-contains "OpenAI" --exclude "deprecated" --exclude "archive"
   ```

3. **Exclude forks**:
   ```bash
   cycodgr "awesome-list" --exclude "fork"
   ```

4. **Exclude by domain** (if searching external URLs in future):
   ```bash
   cycodgr --contains "tutorial" --exclude "example\.com"
   ```

## Related Layers

- **Layer 2: Container Filtering** - Determines which repos/files to search in the first place
- **Layer 3: Content Filtering** - Determines what content to show within selected containers
- **Layer 4: Content Removal** (this layer) - Removes unwanted results after filtering
- **Layer 5: Context Expansion** - Shows context around remaining matches

Layer 4 operates AFTER Layers 2 and 3, removing items from the final result set.

---

[Detailed Proof with Line Numbers →](cycodgr-search-filtering-pipeline-catalog-layer-4-proof.md)

[← Back to README](cycodgr-filtering-pipeline-catalog-README.md)
