# Layer 1 Proof: Target Selection - Source Code Evidence

## Overview

This document provides **detailed source code evidence** for all Layer 1 (Target Selection) features in the cycodgr search command.

## Source File Index

| File | Purpose | Lines Referenced |
|------|---------|-----------------|
| `CycoGrCommandLineOptions.cs` | Command line parsing | 1-571 |
| `SearchCommand.cs` | Command data structure | 1-90 |
| `CycoGrCommand.cs` | Base command properties | 1-37 |
| `Program.cs` | Execution logic | 1-1401 |

---

## 1. Positional Arguments: Repository Patterns

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 328-337**:
```csharp
override protected bool TryParseOtherCommandArg(Command? command, string arg)
{
    if (command is SearchCommand searchCommand)
    {
        searchCommand.RepoPatterns.Add(arg);
        return true;
    }

    return false;
}
```

**Analysis**:
- Any non-option argument (doesn't start with `--`) is treated as a repository pattern
- Directly added to `SearchCommand.RepoPatterns` list
- No validation at parse time (validated later by GitHub API)

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 9, 52**:
```csharp
public SearchCommand()
{
    RepoPatterns = new List<string>();
    // ... other initializations
}

// Positional repo patterns (like cycodmd's file patterns)
public List<string> RepoPatterns { get; set; }
```

**Analysis**:
- Initialized as empty list in constructor
- Public property allows direct access
- Comment explicitly compares to cycodmd's glob patterns

### Usage in Execution

**File**: `src/cycodgr/Program.cs`

**Lines 141, 236-237, 306, 383**:
```csharp
// Line 141: Check if repo patterns provided
var hasRepoPatterns = command.RepoPatterns.Any();

// Lines 236-237: Combine with explicit repos
// Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();

// Line 306: Similar combination for repo search
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();

// Line 383: Similar combination for code search
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
```

**Analysis**:
- RepoPatterns are combined with Repos from other sources
- `.Distinct()` eliminates duplicates
- Used across all search modes

---

## 2. --repo Option

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 367-375**:
```csharp
else if (arg == "--repo")
{
    var repo = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(repo))
    {
        throw new CommandLineException($"Missing repository name for {arg}");
    }
    command.Repos.Add(repo!);
}
```

**Analysis**:
- Gets next argument as repo name
- Validates not null/whitespace
- Throws `CommandLineException` if missing
- Adds to `command.Repos` list

### Data Structure

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 16, 34**:
```csharp
public CycoGrCommand()
{
    // ... other initializations
    Repos = new List<string>();
    // ...
}

public List<string> Repos;
```

**Analysis**:
- Part of base `CycoGrCommand` class
- Shared across all potential commands (though only SearchCommand exists)
- Initialized as empty list

---

## 3. --repos Option (with @file support)

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 376-400**:
```csharp
else if (arg == "--repos")
{
    var repoArgs = GetInputOptionArgs(i + 1, args);
    foreach (var repoArg in repoArgs)
    {
        if (repoArg.StartsWith("@"))
        {
            // Load repos from file
            var fileName = repoArg.Substring(1);
            if (!FileHelpers.FileExists(fileName))
            {
                throw new CommandLineException($"Repository list file not found: {fileName}");
            }
            var reposFromFile = FileHelpers.ReadAllLines(fileName)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());
            command.Repos.AddRange(reposFromFile);
        }
        else
        {
            command.Repos.Add(repoArg);
        }
    }
    i += repoArgs.Count();
}
```

**Analysis**:
- `GetInputOptionArgs` collects all following non-option arguments
- For each argument:
  - If starts with `@`: Load from file
    - Remove `@` prefix
    - Check file exists (throw if not)
    - Read all lines
    - Filter out empty/whitespace-only lines
    - Trim each line
    - Add all to Repos list
  - Otherwise: Add directly to Repos
- Increment loop counter by number of args consumed

**File Loading Details**:
- Uses `FileHelpers.FileExists` for validation
- Uses `FileHelpers.ReadAllLines` for reading
- LINQ filtering: `.Where(line => !string.IsNullOrWhiteSpace(line))`
- LINQ transformation: `.Select(line => line.Trim())`

---

## 4. --owner Option

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 218-226**:
```csharp
else if (arg == "--owner")
{
    var owner = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(owner))
    {
        throw new CommandLineException($"Missing owner/organization for {arg}");
    }
    command.Owner = owner!;
}
```

**Analysis**:
- Gets next argument as owner name
- Validates not null/whitespace
- Error message mentions "owner/organization"
- Stores as single string (not a list)

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 18, 65**:
```csharp
public SearchCommand()
{
    // ...
    Owner = string.Empty;
    // ...
}

public string Owner { get; set; }
```

**Analysis**:
- Initialized to empty string
- Single string property (not a list)
- Can only filter by one owner at a time

### Usage in Execution

**File**: `src/cycodgr/Program.cs`

**Lines 86, 119, 244, 256, 313, 389**:
```csharp
// Line 86: Pre-filtering with owner
var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
    command.RepoFileContains,
    command.Language,
    command.Owner,  // <-- passed to pre-filter
    command.MinStars,
    command.RepoFileContainsExtension,
    command.MaxResults * 2);

// Line 119: Dual behavior pre-filtering
var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
    command.FileContains,
    command.Language,
    command.Owner,  // <-- passed to pre-filter
    command.MinStars,
    string.Empty,
    command.MaxResults * 2);

// Line 244: Repo search in unified mode
var repoTask = CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,  // <-- filter repo search
    command.MinStars,
    command.SortBy,
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);

// Line 256: Code search in unified mode
var codeTask = CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,  // <-- filter code search
    command.MinStars,
    "",
    command.MaxResults);

// Line 313: Repo search
var repos = await CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,  // <-- filter repo search
    command.MinStars,
    command.SortBy,
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);

// Line 389: Code search
var codeMatches = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,  // <-- filter code search
    command.MinStars,
    "",
    command.MaxResults);
```

**Analysis**:
- Owner parameter passed to ALL search operations
- Consistent parameter position across all helper methods
- Applies to both repository and code searches
- Applies to pre-filtering operations

---

## 5. --min-stars Option

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 248-252**:
```csharp
else if (arg == "--min-stars")
{
    var starsStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.MinStars = ValidateNonNegativeNumber(arg, starsStr);
}
```

**Validation Method (Lines 556-569)**:
```csharp
private int ValidateNonNegativeNumber(string arg, string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new CommandLineException($"Missing value for {arg}");
    }

    if (!int.TryParse(value, out var number) || number < 0)
    {
        throw new CommandLineException($"Invalid value for {arg}: '{value}' (must be a non-negative integer)");
    }

    return number;
}
```

**Analysis**:
- Gets next argument
- Calls validation method
- Validation ensures:
  - Not null/empty
  - Valid integer
  - Greater than or equal to 0
- Returns validated integer

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 19, 66**:
```csharp
public SearchCommand()
{
    // ...
    MinStars = 0;
    // ...
}

public int MinStars { get; set; }
```

**Analysis**:
- Default: 0 (no minimum)
- Integer type (not nullable)

### Usage in Execution

Same pattern as Owner - passed to all search operations (lines 88, 121, 245, 257, 314, 390 in Program.cs)

---

## 6. Fork Handling Options

### --include-forks Parser

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 236-239**:
```csharp
else if (arg == "--include-forks")
{
    command.IncludeForks = true;
}
```

### --exclude-fork Parser

**Lines 240-243**:
```csharp
else if (arg == "--exclude-fork")
{
    command.ExcludeForks = true;
}
```

### --only-forks Parser

**Lines 244-247**:
```csharp
else if (arg == "--only-forks")
{
    command.OnlyForks = true;
}
```

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 21-23, 70-72**:
```csharp
public SearchCommand()
{
    // ...
    IncludeForks = false;
    ExcludeForks = false;
    OnlyForks = false;
    // ...
}

public bool IncludeForks { get; set; }
public bool ExcludeForks { get; set; }
public bool OnlyForks { get; set; }
```

**Analysis**:
- All three default to false
- Boolean flags (not mutually exclusive in parser, but should be used exclusively)

### Usage in Execution

**File**: `src/cycodgr/Program.cs`

**Lines 247-249, 316-318**:
```csharp
// Unified search - repo portion
var repoTask = CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,
    command.IncludeForks,  // <--
    command.ExcludeForks,  // <--
    command.OnlyForks,     // <--
    command.MaxResults);

// Repo search
var repos = await CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,
    command.IncludeForks,  // <--
    command.ExcludeForks,  // <--
    command.OnlyForks,     // <--
    command.MaxResults);
```

**Analysis**:
- Only passed to `SearchRepositoriesAsync` (not code search)
- All three flags passed together
- GitHubSearchHelpers responsible for interpreting the flags

---

## 7. --sort Option

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 227-235**:
```csharp
else if (arg == "--sort")
{
    var sort = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(sort))
    {
        throw new CommandLineException($"Missing sort field for {arg}");
    }
    command.SortBy = sort!;
}
```

**Analysis**:
- No enumeration validation (accepts any string)
- GitHub API will validate the value
- Common values: "stars", "updated", "forks"

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 20, 69**:
```csharp
public SearchCommand()
{
    // ...
    SortBy = string.Empty;  // Empty = GitHub's default (relevance)
    // ...
}

public string SortBy { get; set; }
```

**Analysis**:
- Default: empty string (GitHub uses relevance)
- Comment explicitly states default behavior

### Usage in Execution

**File**: `src/cycodgr/Program.cs`

**Lines 246, 315**:
```csharp
// Unified search
var repoTask = CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,  // <--
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);

// Repo search
var repos = await CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,  // <--
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);
```

**Analysis**:
- Only used in repository searches (not code searches)
- Code searches don't support sorting (GitHub API limitation)

---

## 8. --max-results Option

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 107-111**:
```csharp
else if (arg == "--max-results")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.MaxResults = ValidatePositiveNumber(arg, countStr);
}
```

**Validation Method (Lines 541-554)**:
```csharp
private int ValidatePositiveNumber(string arg, string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new CommandLineException($"Missing value for {arg}");
    }

    if (!int.TryParse(value, out var number) || number <= 0)
    {
        throw new CommandLineException($"Invalid value for {arg}: '{value}' (must be a positive integer)");
    }

    return number;
}
```

**Analysis**:
- Must be positive (> 0)
- Different from --min-stars (which allows 0)

### Data Structure

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 16, 63**:
```csharp
public SearchCommand()
{
    // ...
    MaxResults = 10;
    // ...
}

public int MaxResults { get; set; }
```

**Analysis**:
- Default: 10 results
- Integer, not nullable

### Usage in Execution

**File**: `src/cycodgr/Program.cs`

**Lines 89, 123, 250, 259, 319, 392**:
```csharp
// Pre-filtering with multiplier
var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
    command.RepoFileContains,
    command.Language,
    command.Owner,
    command.MinStars,
    command.RepoFileContainsExtension,
    command.MaxResults * 2);  // <-- Multiplied for pre-filter

// Dual behavior pre-filtering
var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
    command.FileContains,
    command.Language,
    command.Owner,
    command.MinStars,
    string.Empty,
    command.MaxResults * 2);  // <-- Multiplied for pre-filter

// Unified search - repo search
var repoTask = CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);  // <-- Direct usage

// Unified search - code search
var codeTask = CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    "",
    command.MaxResults);  // <-- Direct usage

// Repo search
var repos = await CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    command.SortBy,
    command.IncludeForks,
    command.ExcludeForks,
    command.OnlyForks,
    command.MaxResults);  // <-- Direct usage

// Code search
var codeMatches = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
    query,
    allRepos,
    command.Language,
    command.Owner,
    command.MinStars,
    "",
    command.MaxResults);  // <-- Direct usage
```

**Analysis**:
- **Pre-filtering**: Uses `MaxResults * 2` to get more repos since further filtering will reduce count
- **Direct searches**: Uses `MaxResults` exactly
- Passed to both repository and code searches
- Controls API query size, not just display

---

## 9. Repository Combination Logic

### Execution Flow

**File**: `src/cycodgr/Program.cs`

**Multiple locations (lines 236-237, 306, 383)**:
```csharp
// Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
```

**Analysis**:
- `.Concat()`: Combines two lists sequentially
- `.Distinct()`: Removes duplicates (uses string equality)
- `.ToList()`: Materializes to List<string>
- Order: RepoPatterns first, then Repos

**Distinct Behavior**:
- Case-sensitive comparison (C# default string equality)
- GitHub repo names are case-insensitive, but this code treats them as case-sensitive
- Potential issue: "Microsoft/Terminal" and "microsoft/terminal" would both be included

### Pre-filtering Additions

**File**: `src/cycodgr/Program.cs`

**Lines 100-101**:
```csharp
// Add to repos list (will be used by subsequent searches)
command.Repos.AddRange(preFilteredRepos);
```

**Lines 134-135**:
```csharp
// Add to repos list for subsequent file search
command.Repos.AddRange(preFilteredRepos);
```

**Analysis**:
- Pre-filtered repos are added to `command.Repos` (not RepoPatterns)
- This means they'll be included in the `.Concat(command.Repos)` operation
- Mutations happen before the combination logic runs

---

## 10. Search Mode Determination

### Mode Detection Logic

**File**: `src/cycodgr/Program.cs`

**Lines 139-166**:
```csharp
// Stage 3: Determine what type of search based on flags
var hasRepoContains = !string.IsNullOrEmpty(command.RepoContains);
var hasContains = !string.IsNullOrEmpty(command.Contains);
var hasRepoPatterns = command.RepoPatterns.Any();

// Scenario 1: Repo patterns only (no content search) - Show repo metadata
if (hasRepoPatterns && !hasFileContains && !hasRepoContains && !hasContains)
{
    await ShowRepoMetadataAsync(command);
}
// Scenario 2: --contains (unified search - both repos and code)
else if (hasContains)
{
    await HandleUnifiedSearchAsync(command);
}
// Scenario 3: --file-contains (code search, possibly with dual behavior already applied)
else if (hasFileContains)
{
    await HandleCodeSearchAsync(command);
}
// Scenario 4: --repo-contains only (repo search)
else if (hasRepoContains)
{
    await HandleRepoSearchAsync(command);
}
else
{
    ConsoleHelpers.WriteErrorLine("No search criteria specified. Use --contains, --file-contains, or --repo-contains");
}
```

**Analysis**:
- **Metadata mode**: Repo patterns without any content flags
  - `hasRepoPatterns && !hasFileContains && !hasRepoContains && !hasContains`
  - Only shows repository information, no search
- **Unified mode**: --contains specified
  - Searches both repos and code
- **Code search mode**: --file-contains specified
  - Dual behavior may have already run (lines 109-136)
- **Repo search mode**: --repo-contains specified
  - Only searches repository metadata
- **Error**: No flags specified
  - Shows error message

### Repository Metadata Mode

**File**: `src/cycodgr/Program.cs`

**Lines 176-228**:
```csharp
private static async Task ShowRepoMetadataAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    foreach (var repoPattern in command.RepoPatterns)
    {
        try
        {
            var repo = await CycoGr.Helpers.GitHubSearchHelpers.GetRepositoryMetadataAsync(repoPattern);
            
            if (repo == null)
            {
                ConsoleHelpers.WriteLine($"{repoPattern}", ConsoleColor.Yellow, overrideQuiet: true);
                ConsoleHelpers.WriteLine($"  Not found", overrideQuiet: true);
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                continue;
            }

            // Format: ## owner/repo (⭐ stars) (language)
            var header = $"## {repo.FullName} (⭐ {repo.FormattedStars})";
            if (!string.IsNullOrEmpty(repo.Language))
            {
                header += $" ({repo.Language})";
            }
            ConsoleHelpers.WriteLine(header, ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            // Repo URL and Description
            ConsoleHelpers.WriteLine($"Repo: {repo.Url}", overrideQuiet: true);
            if (!string.IsNullOrEmpty(repo.Description))
            {
                ConsoleHelpers.WriteLine($"Desc: {repo.Description}", overrideQuiet: true);
            }
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            // Topics and Updated
            if (repo.Topics?.Any() == true)
            {
                ConsoleHelpers.WriteLine($"Topics: {string.Join(", ", repo.Topics)}", overrideQuiet: true);
            }
            if (repo.UpdatedAt.HasValue)
            {
                ConsoleHelpers.WriteLine($"Updated: {repo.UpdatedAt.Value:yyyy-MM-dd}", overrideQuiet: true);
            }
            ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteLine($"{repoPattern}", ConsoleColor.Yellow, overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  Error: {ex.Message}", overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);
            Logger.Error($"Failed to fetch metadata for {repoPattern}: {ex.Message}");
        }
    }
}
```

**Analysis**:
- Iterates over `RepoPatterns` only (not combined allRepos)
- Calls `GetRepositoryMetadataAsync` for each pattern
- Handles null result (repo not found)
- Handles exceptions gracefully
- Displays formatted output
- **Note**: Does NOT use --max-results, --owner, --min-stars, etc.
  - These filters only apply when searching, not when showing metadata

---

## Summary of Source Code Evidence

### Parser Entry Points

1. **TryParseSearchCommandOptions** (lines 32-326)
   - Handles search-specific options
   - Lines 107-111: `--max-results`
   - Lines 218-226: `--owner`
   - Lines 227-235: `--sort`
   - Lines 236-247: Fork options
   - Lines 248-252: `--min-stars`

2. **TryParseSharedCycoGrCommandOptions** (lines 359-539)
   - Handles shared CycoGrCommand options
   - Lines 367-375: `--repo`
   - Lines 376-400: `--repos` (with @file support)

3. **TryParseOtherCommandArg** (lines 328-337)
   - Handles positional arguments
   - Adds to RepoPatterns

### Data Flow

```
Command Line
  ↓
CycoGrCommandLineOptions.Parse()
  ├─ TryParseSearchCommandOptions() → SearchCommand properties
  ├─ TryParseSharedCycoGrCommandOptions() → CycoGrCommand properties
  └─ TryParseOtherCommandArg() → SearchCommand.RepoPatterns
  ↓
SearchCommand object populated
  ↓
Program.HandleSearchCommandAsync()
  ├─ Lines 76-102: Pre-filter repos (Layer 2)
  ├─ Lines 109-136: Dual behavior for --file-contains (Layer 2)
  ├─ Lines 139-166: Determine search mode
  ├─ Line 236/306/383: Combine RepoPatterns + Repos → allRepos
  ↓
Pass to GitHubSearchHelpers
  ├─ SearchRepositoriesAsync(...)
  ├─ SearchCodeAsync(...)
  └─ GetRepositoryMetadataAsync(...)
  ↓
Results filtered, formatted, displayed
```

### Validation Functions

1. **ValidatePositiveNumber** (lines 541-554)
   - Used by: `--max-results`, `--max-clone`
   - Rule: value > 0

2. **ValidateNonNegativeNumber** (lines 556-569)
   - Used by: `--min-stars`, `--lines-before-and-after`
   - Rule: value >= 0

### Key Integration Points

- **Line 236-237**: Repository combination logic (repeated at 306, 383)
- **Line 89, 123**: Pre-filtering uses `MaxResults * 2`
- **Lines 244-250, 256-259**: All Layer 1 parameters passed to search helpers
- **Lines 313-319, 385-392**: Same parameters passed in different search modes

## Conclusion

All Layer 1 options are thoroughly implemented with:
- ✅ Command line parsing with validation
- ✅ Data structure storage
- ✅ Execution integration across all search modes
- ✅ Error handling for invalid inputs
- ✅ File-based repository lists (@file support)
- ✅ Combination logic for multiple sources (RepoPatterns + Repos)
- ✅ Pre-filtering integration with Layer 2
