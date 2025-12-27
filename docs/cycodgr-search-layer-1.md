# cycodgr search - Layer 1: TARGET SELECTION

## Purpose

Layer 1 defines **what to search** - the primary search space for GitHub operations. This includes repository patterns, explicit repository lists, and owner/organization filtering.

## Command-Line Options

### Positional Arguments: Repository Patterns

**Syntax**: `cycodgr [repo-pattern...]`

**Examples**:
```bash
cycodgr microsoft/terminal
cycodgr wezterm/*
cycodgr microsoft/* google/*
```

**Behavior**:
- Accepts zero or more repository patterns
- Patterns can be exact (`owner/repo`) or wildcards (`owner/*`)
- Stored in `SearchCommand.RepoPatterns` (List<string>)
- Combined with `--repo` and `--repos` options in execution

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#positional-arguments)

### `--repo <name>` / `--repos <name...>`

**Purpose**: Explicitly specify repository names

**Examples**:
```bash
cycodgr --repo microsoft/terminal --file-contains "ConPTY"
cycodgr --repos microsoft/terminal google/chrome
```

**Behavior**:
- `--repo`: Single repository
- `--repos`: Multiple repositories (space-separated)
- Supports `@file` loading: `--repos @repos.txt`
- Stored in `CycoGrCommand.Repos` (List<string>)
- Combined with positional `RepoPatterns` during execution

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#repo-repos)

### `--owner <name>`

**Purpose**: Filter results by repository owner/organization

**Examples**:
```bash
cycodgr --file-contains "ConPTY" --owner microsoft
```

**Behavior**:
- Filters GitHub search results to specific owner
- Translated to GitHub search qualifier: `user:owner`
- Stored in `SearchCommand.Owner` (string)
- Applied during GitHub API calls

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#owner)

### `--min-stars <number>`

**Purpose**: Filter repositories by minimum star count

**Examples**:
```bash
cycodgr --repo-contains "terminal" --min-stars 100
```

**Behavior**:
- Only returns repositories with >= N stars
- Translated to GitHub search qualifier: `stars:>=N`
- Stored in `SearchCommand.MinStars` (int, default: 0)
- Applied during GitHub API calls

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#min-stars)

### Fork Filtering Options

#### `--include-forks`
Include forked repositories in search results (default: exclude)

#### `--exclude-fork`
Explicitly exclude forked repositories

#### `--only-forks`
Only search forked repositories

**Behavior**:
- Mutually exclusive options
- Translated to GitHub search qualifiers:
  - `--include-forks` → `fork:true`
  - `--exclude-fork` → `fork:false`
  - `--only-forks` → `fork:only`
- Stored in `SearchCommand.IncludeForks`, `ExcludeForks`, `OnlyForks` (bool)
- Default behavior: excludes forks (no qualifier)

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#fork-filtering)

### `--sort <field>`

**Purpose**: Sort search results by specific field

**Examples**:
```bash
cycodgr --repo-contains "ai" --sort stars
cycodgr --repo-contains "ml" --sort updated
```

**Behavior**:
- Valid values: `stars`, `forks`, `updated`, `help-wanted-issues`, etc.
- Passed to GitHub API `--sort` parameter
- Stored in `SearchCommand.SortBy` (string, default: empty = relevance)
- Only applies to repository search

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#sort)

### `--max-results <number>`

**Purpose**: Limit the number of search results returned

**Examples**:
```bash
cycodgr --file-contains "ConPTY" --max-results 20
```

**Behavior**:
- Limits results from GitHub API
- Passed to GitHub CLI `--limit` parameter
- Stored in `SearchCommand.MaxResults` (int, default: 10)
- Applies to both repository and code searches

**Source Reference**: See [Layer 1 Proof](cycodgr-search-layer-1-proof.md#max-results)

## Data Flow

```
Command Line Args
    ↓
CycoGrCommandLineOptions.Parse()
    ↓ (positional args → RepoPatterns)
    ↓ (--repo/--repos → Repos)
    ↓ (--owner → Owner)
    ↓ (--min-stars → MinStars)
    ↓ (fork options → IncludeForks/ExcludeForks/OnlyForks)
    ↓ (--sort → SortBy)
    ↓ (--max-results → MaxResults)
    ↓
SearchCommand properties
    ↓
Program.HandleSearchCommandAsync()
    ↓ (combines RepoPatterns + Repos)
    ↓
GitHubSearchHelpers.SearchRepositoriesAsync()
    or
GitHubSearchHelpers.SearchCodeAsync()
    ↓ (builds gh command with qualifiers)
    ↓
GitHub CLI execution
```

## Search Scenarios

### Scenario 1: Repository Patterns Only
```bash
cycodgr microsoft/terminal
```
- Uses positional `RepoPatterns`
- Fetches repository metadata
- No content search

### Scenario 2: Owner + Content Search
```bash
cycodgr --owner microsoft --file-contains "terminal"
```
- Uses `Owner` to filter
- Searches code within owner's repositories
- Returns code matches

### Scenario 3: Explicit Repos + Filtering
```bash
cycodgr --repos microsoft/terminal wezterm/wezterm --min-stars 1000
```
- Uses explicit `Repos` list
- Applies star filter
- Can be combined with content search

### Scenario 4: Load Repos from File
```bash
cycodgr --repos @repos.txt --file-contains "async"
```
- Loads repository list from file
- Each line is a repository name
- Searches code within those repositories

## Key Characteristics

1. **Multiple input sources**: Positional args, `--repo`, `--repos`, `@file` loading
2. **Combination logic**: All sources are merged (distinct union)
3. **Filtering applied early**: Owner, stars, forks filter at GitHub API level
4. **Default behavior**: No repos specified = search all of GitHub
5. **Sorting**: Only affects repository search, not code search

## Related Layers

- **Layer 2 (Container Filtering)**: Further refines which repos/files to include
- **Layer 6 (Display Control)**: Controls result count and formatting
- **Layer 9 (Actions)**: Uses target selection for cloning operations

---

For detailed source code evidence with line numbers and call traces, see: [**Layer 1 Proof Document**](cycodgr-search-layer-1-proof.md)
