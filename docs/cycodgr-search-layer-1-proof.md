# cycodgr search - Layer 1: TARGET SELECTION - PROOF

## Purpose

This document provides **source code evidence** with line numbers, call stacks, and data flows for all Layer 1 (Target Selection) features in cycodgr.

---

## Table of Contents

1. [Positional Arguments (Repository Patterns)](#positional-arguments)
2. [--repo / --repos](#repo-repos)
3. [--owner](#owner)
4. [--min-stars](#min-stars)
5. [Fork Filtering Options](#fork-filtering)
6. [--sort](#sort)
7. [--max-results](#max-results)

---

## Positional Arguments

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 51-52**:
```csharp
// Positional repo patterns (like cycodmd's file patterns)
public List<string> RepoPatterns { get; set; }
```

**Constructor initialization (Line 9)**:
```csharp
RepoPatterns = new List<string>();
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 189-195** - `TryParseOtherCommandArg` method:
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

**Call stack**:
```
Main(args)
  → CycoGrCommandLineOptions.Parse()
    → CommandLineOptions.Parse()
      → CommandLineOptions.ParseInputOptions()
        → CommandLineOptions.TryParseInputOptions()
          → CycoGrCommandLineOptions.TryParseOtherCommandArg()
            → searchCommand.RepoPatterns.Add(arg)
```

---

## --repo / --repos

### Command Property Definition

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 16 & 34**:
```csharp
Repos = new List<string>();  // Constructor
// ...
public List<string> Repos;   // Property
```

### Parsing --repo

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 147-155** - In `TryParseSharedCycoGrCommandOptions`:
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

### Parsing --repos

**Lines 156-171**:
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

**@file loading behavior**:
- Reads all lines from file
- Filters out empty/whitespace lines
- Trims each line
- Adds to `Repos` list

---

## --owner

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 65**:
```csharp
public string Owner { get; set; }
```

**Constructor initialization (Line 18)**:
```csharp
Owner = string.Empty;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 143-152** - In `TryParseSearchCommandOptions`:
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

### Application in Repository Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 216-219** - In `SearchRepositoriesByKeywordsAsync`:
```csharp
// Add owner qualifier if specified
if (!string.IsNullOrEmpty(owner))
{
    query = $"{query} user:{owner}";
}
```

### Application in Code Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 308-311** - In `SearchCodeForMatchesAsync`:
```csharp
// Add owner qualifier if specified
if (!string.IsNullOrEmpty(owner))
{
    query = $"{query} user:{owner}";
}
```

**Effect**: Translates to GitHub search qualifier `user:owner`

---

## --min-stars

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 66**:
```csharp
public int MinStars { get; set; }
```

**Constructor initialization (Line 19)**:
```csharp
MinStars = 0;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 177-180** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--min-stars")
{
    var starsStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.MinStars = ValidateNonNegativeNumber(arg, starsStr);
}
```

### Validation

**Lines 260-270** - `ValidateNonNegativeNumber` method:
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

### Application in Repository Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 235-239** - In `SearchRepositoriesByKeywordsAsync`:
```csharp
// Add stars filter if specified
if (minStars > 0)
{
    query = $"{query} stars:>={minStars}";
}
```

### Application in Code Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 314-318** - In `SearchCodeForMatchesAsync`:
```csharp
// Add stars filter if specified
if (minStars > 0)
{
    query = $"{query} stars:>={minStars}";
}
```

**Effect**: Translates to GitHub search qualifier `stars:>=N`

---

## Fork Filtering

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 70-72**:
```csharp
public bool IncludeForks { get; set; }
public bool ExcludeForks { get; set; }
public bool OnlyForks { get; set; }
```

**Constructor initialization (Lines 21-23)**:
```csharp
IncludeForks = false;
ExcludeForks = false;
OnlyForks = false;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 153-166** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--include-forks")
{
    command.IncludeForks = true;
}
else if (arg == "--exclude-fork")
{
    command.ExcludeForks = true;
}
else if (arg == "--only-forks")
{
    command.OnlyForks = true;
}
```

### Application

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 221-233** - In `SearchRepositoriesByKeywordsAsync`:
```csharp
// Add fork filter
if (onlyForks)
{
    query = $"{query} fork:only";
}
else if (excludeForks)
{
    query = $"{query} fork:false";
}
else if (includeForks)
{
    query = $"{query} fork:true";
}
```

**Effect**: Translates to GitHub search qualifiers:
- `--include-forks` → `fork:true`
- `--exclude-fork` → `fork:false`
- `--only-forks` → `fork:only`

**Note**: Only applies to repository search, not code search.

---

## --sort

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 69**:
```csharp
public string SortBy { get; set; }
```

**Constructor initialization (Line 20)**:
```csharp
SortBy = string.Empty;  // Empty = GitHub's default (relevance)
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 167-175** - In `TryParseSearchCommandOptions`:
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

### Application

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 265-269** - In `SearchRepositoriesByKeywordsAsync`:
```csharp
if (!string.IsNullOrEmpty(sortBy))
{
    args.Add("--sort");
    args.Add(sortBy);
}
```

**Valid values** (from GitHub CLI documentation):
- `stars` - Sort by star count
- `forks` - Sort by fork count
- `updated` - Sort by last update time
- `help-wanted-issues` - Sort by help-wanted issues count

**Note**: Only applies to repository search, not code search.

---

## --max-results

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 63**:
```csharp
public int MaxResults { get; set; }
```

**Constructor initialization (Line 16)**:
```csharp
MaxResults = 10;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 26-29** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--max-results")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.MaxResults = ValidatePositiveNumber(arg, countStr);
}
```

### Validation

**Lines 249-259** - `ValidatePositiveNumber` method:
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

### Application in Repository Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 256-257** - In `SearchRepositoriesByKeywordsAsync`:
```csharp
args.Add("--limit");
args.Add(maxResults.ToString());
```

### Application in Code Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 335-336** - In `SearchCodeForMatchesAsync`:
```csharp
args.Add("--limit");
args.Add(maxResults.ToString());
```

### Application in Pre-Filtering

**File**: `src/cycodgr/Program.cs`

**Line 89** - In `HandleSearchCommandAsync`:
```csharp
command.MaxResults * 2); // Get more repos initially since we'll filter further
```

**Note**: Pre-filtering stages use `maxResults * 2` to get more candidates before subsequent filtering.

---

## Complete Data Flow Example

### Example: `cycodgr --owner microsoft --min-stars 1000 --sort stars --max-results 5`

**Call stack**:

```
1. Main(args)
     ↓
2. CycoGrCommandLineOptions.Parse()
     ↓ Parsing:
     3. --owner → command.Owner = "microsoft"
     4. --min-stars → command.MinStars = 1000 (via ValidateNonNegativeNumber)
     5. --sort → command.SortBy = "stars"
     6. --max-results → command.MaxResults = 5 (via ValidatePositiveNumber)
     ↓
7. Program.HandleSearchCommandAsync(command)
     ↓ (No search terms → error, but for example, assume --repo-contains "terminal")
     8. Program.HandleRepoSearchAsync(command)
     ↓
9. GitHubSearchHelpers.SearchRepositoriesAsync(
     query="terminal",
     repos=[],
     language="",
     owner="microsoft",
     minStars=1000,
     sortBy="stars",
     includeForks=false,
     excludeForks=false,
     onlyForks=false,
     maxResults=5)
     ↓
10. SearchRepositoriesByKeywordsAsync()
     ↓ Build query:
     11. query = "terminal"
     12. query = "terminal user:microsoft"
     13. query = "terminal user:microsoft stars:>=1000"
     ↓
14. Build gh command:
     gh search repos "terminal user:microsoft stars:>=1000" --limit 5 --sort stars --json name,owner,...
     ↓
15. ProcessHelpers.RunProcessAsync(ghCommand)
     ↓
16. Parse JSON response → List<RepoInfo>
     ↓
17. Return to HandleRepoSearchAsync()
     ↓
18. Apply exclude filters (if any)
     ↓
19. Format and display results
```

---

## Key Source Files Summary

| File | Lines | Purpose |
|------|-------|---------|
| `CycoGrCommandLineOptions.cs` | 189-195 | Parse positional repo patterns |
| `CycoGrCommandLineOptions.cs` | 147-175 | Parse --repo, --repos, --owner, --sort |
| `CycoGrCommandLineOptions.cs` | 26-29, 177-180 | Parse --max-results, --min-stars |
| `CycoGrCommandLineOptions.cs` | 153-166 | Parse fork filtering options |
| `CycoGrCommandLineOptions.cs` | 249-270 | Validation methods |
| `SearchCommand.cs` | 9-34, 51-72 | Store all target selection properties |
| `CycoGrCommand.cs` | 16, 34 | Store Repos list |
| `Program.cs` | 299-369 | Execute repository/code searches |
| `GitHubSearchHelpers.cs` | 201-295 | Build and execute repository search |
| `GitHubSearchHelpers.cs` | 297-374 | Build and execute code search |

---

**End of Layer 1 Proof Document**
