# Layer 1: Target Selection - cycodgr search

## Purpose

Layer 1 defines **what to search** - the primary search space. For cycodgr, this means specifying which GitHub repositories to search in, or defining criteria for discovering repositories.

## Conceptual Overview

Target selection in cycodgr operates at the **repository level** and includes:

1. **Explicit repository specification** - Direct names or patterns
2. **Owner/organization filtering** - Limit by owner
3. **Quality filtering** - Minimum star count
4. **Fork handling** - Include, exclude, or only search forks
5. **Sorting** - Order of results
6. **Result limiting** - Maximum number of results

## Command Line Options

### Positional Arguments: Repository Patterns

**Syntax**: `cycodgr [repo-pattern...]`

Repository patterns can be:
- Full names: `microsoft/terminal`
- Wildcards: `microsoft/*`, `*/terminal`
- Exact organization matches: `rust-lang/*`

**Parsed by**: `TryParseOtherCommandArg` (CycoGrCommandLineOptions.cs:328-337)

**Stored in**: `SearchCommand.RepoPatterns` (List<string>)

**Examples**:
```bash
cycodgr microsoft/terminal
cycodgr microsoft/* --max-results 20
cycodgr */terminal
```

### --repo <repo-name>

**Purpose**: Explicitly specify a single repository

**Syntax**: `--repo <owner/repo>`

**Parsed by**: `TryParseSharedCycoGrCommandOptions` (CycoGrCommandLineOptions.cs:367-375)

**Stored in**: `CycoGrCommand.Repos` (List<string>)

**Example**:
```bash
cycodgr --repo microsoft/terminal --file-contains "ConPTY"
```

### --repos <repo-name...> or --repos @file

**Purpose**: Specify multiple repositories or load from file

**Syntax**: 
- Multiple: `--repos owner/repo1 owner/repo2 ...`
- From file: `--repos @repos.txt`

**File format** (for @file):
```
microsoft/terminal
rust-lang/rust
golang/go
```

**Parsed by**: `TryParseSharedCycoGrCommandOptions` (CycoGrCommandLineOptions.cs:376-400)

**File loading**: Lines 381-392 (reads file, filters empty lines, trims)

**Stored in**: `CycoGrCommand.Repos` (List<string>)

**Examples**:
```bash
cycodgr --repos microsoft/terminal wezterm/wezterm
cycodgr --repos @terminal-repos.txt --file-contains "OSC 133"
```

### --owner <owner-name>

**Purpose**: Filter results to a specific owner/organization

**Syntax**: `--owner <name>`

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:218-226)

**Stored in**: `SearchCommand.Owner` (string)

**Usage**: Passed to `SearchRepositoriesAsync` and `SearchCodeAsync` (Program.cs:244, 256, 389)

**Examples**:
```bash
cycodgr --owner microsoft --repo-contains "terminal"
cycodgr --owner rust-lang --file-contains "async"
```

### --min-stars <count>

**Purpose**: Filter repositories by minimum star count

**Syntax**: `--min-stars <number>`

**Validation**: Must be non-negative integer (ValidateNonNegativeNumber:556-569)

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:248-252)

**Stored in**: `SearchCommand.MinStars` (int, default: 0)

**Usage**: Passed to search helpers (Program.cs:245, 257, 390)

**Examples**:
```bash
cycodgr --repo-contains "terminal" --min-stars 100
cycodgr --file-contains "ConPTY" --min-stars 50
```

### Fork Handling Options

#### --include-forks

**Purpose**: Include forked repositories in search (default: exclude)

**Syntax**: `--include-forks`

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:236-239)

**Stored in**: `SearchCommand.IncludeForks` (bool, default: false)

#### --exclude-fork

**Purpose**: Explicitly exclude forked repositories

**Syntax**: `--exclude-fork`

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:240-243)

**Stored in**: `SearchCommand.ExcludeForks` (bool, default: false)

#### --only-forks

**Purpose**: Search only forked repositories

**Syntax**: `--only-forks`

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:244-247)

**Stored in**: `SearchCommand.OnlyForks` (bool, default: false)

**Usage**: All three passed to `SearchRepositoriesAsync` (Program.cs:247-249)

**Examples**:
```bash
cycodgr --repo-contains "dotfiles" --include-forks
cycodgr --repo-contains "terminal" --exclude-fork
cycodgr --only-forks --repo-contains "awesome-list"
```

### --sort <field>

**Purpose**: Sort repository results by field

**Syntax**: `--sort <field>`

**Valid values**: `stars`, `updated`, `forks`, etc. (GitHub API values)

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:227-235)

**Stored in**: `SearchCommand.SortBy` (string, default: empty = relevance)

**Usage**: Passed to `SearchRepositoriesAsync` (Program.cs:246)

**Examples**:
```bash
cycodgr --repo-contains "terminal" --sort stars
cycodgr --repo-contains "emulator" --sort updated
```

### --max-results <count>

**Purpose**: Limit maximum number of search results

**Syntax**: `--max-results <number>`

**Validation**: Must be positive integer (ValidatePositiveNumber:541-554)

**Parsed by**: `TryParseSearchCommandOptions` (CycoGrCommandLineOptions.cs:107-111)

**Stored in**: `SearchCommand.MaxResults` (int, default: 10)

**Usage**: Passed to all search helpers (Program.cs:250, 259, 319, 392)

**Examples**:
```bash
cycodgr --file-contains "async" --max-results 50
cycodgr --repo-contains "terminal" --max-results 5
```

## Data Flow

### 1. Command Line Parsing

```
User Input → CycoGrCommandLineOptions.Parse()
  ├─ Positional args → SearchCommand.RepoPatterns (List<string>)
  ├─ --repo → CycoGrCommand.Repos.Add()
  ├─ --repos → CycoGrCommand.Repos.AddRange() [with optional @file loading]
  ├─ --owner → SearchCommand.Owner (string)
  ├─ --min-stars → SearchCommand.MinStars (int)
  ├─ --include-forks → SearchCommand.IncludeForks (bool)
  ├─ --exclude-fork → SearchCommand.ExcludeForks (bool)
  ├─ --only-forks → SearchCommand.OnlyForks (bool)
  ├─ --sort → SearchCommand.SortBy (string)
  └─ --max-results → SearchCommand.MaxResults (int)
```

### 2. Execution (Program.cs:HandleSearchCommandAsync)

```
HandleSearchCommandAsync()
  ├─ Line 106: Combine RepoPatterns + Repos → allRepos
  │    var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
  │
  ├─ Line 76-102: Pre-filter repos if --repo-file-contains specified (Layer 2)
  │    [If specified, adds repos to command.Repos]
  │
  ├─ Line 109-136: Dual behavior: Use --file-contains for repo discovery (Layer 2)
  │    [If no repo patterns/repos and --file-contains, discovers repos]
  │
  ├─ Line 144-146: Check if repo patterns only (metadata mode)
  │    if (hasRepoPatterns && !hasFileContains && !hasRepoContains && !hasContains)
  │
  ├─ Pass to search helpers:
  │    ├─ SearchRepositoriesAsync(query, allRepos, Language, Owner, MinStars, SortBy, 
  │    │                          IncludeForks, ExcludeForks, OnlyForks, MaxResults)
  │    │
  │    └─ SearchCodeAsync(query, allRepos, Language, Owner, MinStars, "", MaxResults)
  │
  └─ Search helpers use these parameters to construct GitHub API queries
```

### 3. Repository Metadata Mode (Program.cs:176-228)

```
ShowRepoMetadataAsync()
  └─ For each repoPattern in RepoPatterns:
       ├─ GetRepositoryMetadataAsync(repoPattern)
       └─ Display repo information (no search query needed)
```

## Integration with Other Layers

### Layer 2: Container Filtering

Target selection works **in conjunction** with Layer 2:

- **RepoPatterns + Repos** define the **initial search space**
- **Layer 2 options** (`--repo-file-contains`, `--file-contains` without repos) **expand** the search space by discovering additional repos
- The **combined list** (RepoPatterns + Repos + discovered repos) becomes the final target set

Example flow:
```bash
cycodgr microsoft/* --repo-csproj-file-contains "Microsoft.Extensions"
```

1. **Layer 1**: RepoPatterns = ["microsoft/*"]
2. **Layer 2**: Pre-filter using --repo-csproj-file-contains
   - Discovers: microsoft/extensions, microsoft/runtime, etc.
   - Adds to Repos list
3. **Combined**: allRepos = RepoPatterns ∪ Repos (distinct)
4. Search proceeds with this combined target set

### Layer 6: Display Control

`--max-results` affects both target selection and display:
- Limits API query results (fewer repos fetched)
- Limits displayed output (fewer results shown)

### Layer 9: Actions on Results

`--clone` and related options operate on the **result set** from Layer 1:
- Repos discovered by target selection are the candidates for cloning
- `--max-clone` further limits which repos are cloned

## Special Behaviors

### Repository Wildcard Matching

Wildcards in RepoPatterns are handled by the **GitHub API**:
- `microsoft/*` - All repos under microsoft organization
- `*/terminal` - All repos named "terminal" regardless of owner
- Exact matching required for owner/repo format

### File-Based Repository Lists (--repos @file)

File loading features:
- **Line-based**: Each line is a repository
- **Whitespace handling**: Empty lines ignored, lines trimmed
- **No comments**: No comment syntax (# not treated specially)
- **UTF-8 encoding**: File must be UTF-8

Implementation: CycoGrCommandLineOptions.cs:381-392

### Distinct Repository Handling

When combining RepoPatterns + Repos:
```csharp
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
```

- Eliminates duplicates
- Order: RepoPatterns first, then Repos
- Case-sensitive comparison (GitHub repo names are case-insensitive, but this is string-based)

## Validation

### Positive Number Validation (--max-results, --max-clone)

Method: `ValidatePositiveNumber` (CycoGrCommandLineOptions.cs:541-554)

Rules:
- Must not be null/empty
- Must be valid integer
- Must be > 0

Error message: `"Invalid value for {arg}: '{value}' (must be a positive integer)"`

### Non-Negative Number Validation (--min-stars)

Method: `ValidateNonNegativeNumber` (CycoGrCommandLineOptions.cs:556-569)

Rules:
- Must not be null/empty
- Must be valid integer
- Must be >= 0

Error message: `"Invalid value for {arg}: '{value}' (must be a non-negative integer)"`

### Repository Name Validation

No explicit validation in parser. Invalid repo names will fail at:
- GitHub API call (returns 404)
- Handled gracefully in ShowRepoMetadataAsync (Program.cs:183-191)

## Performance Considerations

### API Rate Limiting

GitHub API has rate limits:
- Authenticated: 5000 requests/hour
- Unauthenticated: 60 requests/hour

`--max-results` helps manage rate limit usage by fetching fewer results.

### Pre-filtering Strategy

Pre-filtering (Layer 2) can multiply API calls:
```bash
cycodgr --repo-file-contains "term" --max-results 10
```

1. API call to find repos with files containing "term" (10+ results)
2. For each repo, additional API calls for metadata/code search

Consider: Fetching `MaxResults * 2` during pre-filter (Program.cs:89) to account for subsequent filtering.

## Examples with Execution Flow

### Example 1: Simple Repo Pattern

```bash
cycodgr microsoft/terminal
```

**Layer 1 Processing**:
1. RepoPatterns = ["microsoft/terminal"]
2. Repos = [] (empty)
3. No owner, min-stars, or fork options
4. MaxResults = 10 (default)
5. Mode: Repository Metadata (no content search)

**Result**: Shows metadata for microsoft/terminal

### Example 2: Organization Search with Filters

```bash
cycodgr --owner rust-lang --min-stars 1000 --sort stars --max-results 20
```

**Layer 1 Processing**:
1. RepoPatterns = [] (empty)
2. Repos = [] (empty)
3. Owner = "rust-lang"
4. MinStars = 1000
5. SortBy = "stars"
6. MaxResults = 20
7. Mode: Repository Search (requires --repo-contains or --contains)

**Note**: This example would show help/error since no search term provided. Needs `--repo-contains "something"`.

### Example 3: Multiple Repos from File

```bash
cycodgr --repos @terminal-repos.txt --file-contains "ConPTY"
```

**terminal-repos.txt**:
```
microsoft/terminal
wezterm/wezterm
alacritty/alacritty
```

**Layer 1 Processing**:
1. RepoPatterns = [] (empty)
2. Repos loaded from file = ["microsoft/terminal", "wezterm/wezterm", "alacritty/alacritty"]
3. MaxResults = 10 (default)
4. Mode: Code Search (--file-contains specified)

**Result**: Searches for "ConPTY" in those 3 repos

### Example 4: Combining Patterns and Explicit Repos

```bash
cycodgr microsoft/* google/* --repo rust-lang/rust --min-stars 500 --max-results 50
```

**Layer 1 Processing**:
1. RepoPatterns = ["microsoft/*", "google/*"]
2. Repos = ["rust-lang/rust"]
3. MinStars = 500
4. MaxResults = 50
5. allRepos = RepoPatterns ∪ Repos = ["microsoft/*", "google/*", "rust-lang/rust"] (distinct)
6. Mode: Depends on content flags

**Result**: Combined search across all specified repos

## See Also

- [Layer 1 Proof Documentation](search-layer-1-proof.md) - Source code evidence
- [Layer 2: Container Filtering](search-layer-2.md) - How target selection interacts with repo discovery
- [Search Command README](search-README.md) - Overall command documentation
