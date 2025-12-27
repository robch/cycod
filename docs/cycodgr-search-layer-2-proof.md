# cycodgr search - Layer 2: CONTAINER FILTERING - PROOF

## Purpose

This document provides **source code evidence** with line numbers, call stacks, and data flows for all Layer 2 (Container Filtering) features in cycodgr.

---

## Table of Contents

1. [Positional Arguments and Repo Options](#positional-arguments-and-repo-options)
2. [--repo-contains](#repo-contains)
3. [--repo-file-contains](#repo-file-contains)
4. [--repo-{ext}-file-contains](#repo-ext-file-contains)
5. [--file-contains (Dual Behavior)](#file-contains)
6. [--{ext}-file-contains](#ext-file-contains)
7. [--language / --extension / --in-files](#language-extension)
8. [Language Shortcuts](#language-shortcuts)
9. [--file-path / --file-paths](#file-path)
10. [--exclude](#exclude)

---

## Positional Arguments and Repo Options

### Command Definition

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

### Parsing Positional Arguments

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
CommandLineOptions.ParseInputOptions()
  → CommandLineOptions.TryParseInputOptions()
    → CycoGrCommandLineOptions.TryParseOtherCommandArg()
      → searchCommand.RepoPatterns.Add(arg)
```

### Parsing `--repo` and `--repos`

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 143-171** - `TryParseSharedCycoGrCommandOptions` method:
```csharp
private bool TryParseSharedCycoGrCommandOptions(CycoGrCommand? command, string[] args, ref int i, string arg)
{
    bool parsed = true;

    if (command == null)
    {
        parsed = false;
    }
    else if (arg == "--repo")
    {
        var repo = i + 1 < args.Count() ? args.ElementAt(++i) : null;
        if (string.IsNullOrWhiteSpace(repo))
        {
            throw new CommandLineException($"Missing repository name for {arg}");
        }
        command.Repos.Add(repo!);
    }
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

**Storage location**:

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 16 & 34**:
```csharp
Repos = new List<string>();  // Constructor
// ...
public List<string> Repos;   // Property
```

### Combining RepoPatterns and Repos in Execution

**File**: `src/cycodgr/Program.cs`

**Example from HandleRepoSearchAsync, Line 306**:
```csharp
// Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
```

**Same pattern appears in**:
- `HandleUnifiedSearchAsync()` - Line 237
- `HandleCodeSearchAsync()` - Line 383

---

## --repo-contains

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 57**:
```csharp
public string RepoContains { get; set; }    // Search repo metadata only
```

**Constructor initialization (Line 12)**:
```csharp
RepoContains = string.Empty;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 44-52** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--repo-contains")
{
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.RepoContains = terms!;
}
```

### Execution Path Detection

**File**: `src/cycodgr/Program.cs`

**Lines 138-162** - In `HandleSearchCommandAsync`:
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
```

### Repository Search Execution

**File**: `src/cycodgr/Program.cs`

**Lines 299-319** - `HandleRepoSearchAsync` method:
```csharp
private static async Task HandleRepoSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    var query = !string.IsNullOrEmpty(command.RepoContains) ? command.RepoContains : command.Contains;
    ConsoleHelpers.WriteLine($"## GitHub repository search for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    // Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
    var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();

    // Search GitHub using new helper signature
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
        command.MaxResults);
```

### GitHub API Call

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 201-295** - `SearchRepositoriesByKeywordsAsync` method:
```csharp
private static async Task<List<RepoInfo>> SearchRepositoriesByKeywordsAsync(
    string query,
    List<string> repos,
    string language,
    string owner,
    int minStars,
    string sortBy,
    bool includeForks,
    bool excludeForks,
    bool onlyForks,
    int maxResults)
{
    // Query is already built - just add qualifiers
    
    // Add owner qualifier if specified
    if (!string.IsNullOrEmpty(owner))
    {
        query = $"{query} user:{owner}";
    }
    
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
    
    // Add stars filter if specified
    if (minStars > 0)
    {
        query = $"{query} stars:>={minStars}";
    }
    
    var args = new List<string>();
    
    args.Add("search");
    args.Add("repos");
    args.Add($"\"{query}\"");
    
    // Add repo qualifiers as separate arguments (not inside quoted query)
    if (repos.Any())
    {
        foreach (var repo in repos)
        {
            args.Add($"repo:{repo}");
        }
    }
    
    args.Add("--limit");
    args.Add(maxResults.ToString());
    
    if (!string.IsNullOrEmpty(language))
    {
        args.Add("--language");
        args.Add(language);
    }
    
    if (!string.IsNullOrEmpty(sortBy))
    {
        args.Add("--sort");
        args.Add(sortBy);
    }
    
    args.Add("--json");
    args.Add("name,owner,url,description,stargazersCount,language,updatedAt,forksCount,openIssuesCount");
    
    var ghCommand = $"gh {string.Join(" ", args)}";
    
    try
    {
        var result = await ProcessHelpers.RunProcessAsync(ghCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
        
        if (result.ExitCode != 0)
        {
            var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                ? result.StandardError 
                : "Unknown error executing gh command";
            throw new Exception($"GitHub search failed: {errorMsg}");
        }
        
        return ParseRepositoryUrls(result.StandardOutput);
    }
    catch (Exception ex)
    {
        Logger.Error($"Error searching GitHub: {ex.Message}");
        throw;
    }
}
```

**Data flow**:
```
command.RepoContains (string)
  → query parameter
  → augmented with qualifiers (user:, fork:, stars:)
  → gh search repos "{query}" repo:... --limit N --language L --sort S
  → JSON response
  → Parsed to List<RepoInfo>
```

---

## --repo-file-contains

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 58-59**:
```csharp
public string RepoFileContains { get; set; } // Search for repos containing files with text (pre-filter)
public string RepoFileContainsExtension { get; set; } // Extension filter for repo pre-filter (empty = any file)
```

**Constructor initialization (Lines 13-14)**:
```csharp
RepoFileContains = string.Empty;
RepoFileContainsExtension = string.Empty;
```

### Parsing

This option is NOT parsed in the standard way. It's only created via the `--repo-{ext}-file-contains` pattern.

### Pre-Filter Execution

**File**: `src/cycodgr/Program.cs`

**Lines 76-102** - In `HandleSearchCommandAsync` (Stage 1):
```csharp
// Stage 1: Pre-filter repos using --repo-file-contains if specified
if (!string.IsNullOrEmpty(command.RepoFileContains))
{
    var extInfo = !string.IsNullOrEmpty(command.RepoFileContainsExtension) 
        ? $" in .{command.RepoFileContainsExtension} files"
        : "";
    ConsoleHelpers.WriteLine($"## Pre-filtering repositories containing files{extInfo} with '{command.RepoFileContains}'", ConsoleColor.Cyan, overrideQuiet: true);
    
    var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
        command.RepoFileContains,
        command.Language,
        command.Owner,
        command.MinStars,
        command.RepoFileContainsExtension,  // Pass extension for filtering
        command.MaxResults * 2); // Get more repos initially since we'll filter further
    
    if (preFilteredRepos.Count == 0)
    {
        ConsoleHelpers.WriteLine("No repositories found matching the pre-filter criteria", ConsoleColor.Yellow, overrideQuiet: true);
        return;
    }
    
    ConsoleHelpers.WriteLine($"Found {preFilteredRepos.Count} repositories matching pre-filter", ConsoleColor.Green, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);
    
    // Add to repos list (will be used by subsequent searches)
    command.Repos.AddRange(preFilteredRepos);
}
```

### Search Code for Repositories Helper

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 43-104** - `SearchCodeForRepositoriesAsync` method:
```csharp
public static async Task<List<string>> SearchCodeForRepositoriesAsync(
    string query,
    string language,
    string owner,
    int minStars,
    string fileExtension,
    int maxResults)
{
    // Search code and extract unique repository names
    // This is used for repo pre-filtering via --repo-file-contains
    
    var args = new List<string>();
    
    args.Add("search");
    args.Add("code");
    args.Add($"\"{query}\"");
    args.Add("--limit");
    args.Add(maxResults.ToString());
    
    if (!string.IsNullOrEmpty(fileExtension))
    {
        args.Add("--extension");
        args.Add(fileExtension.TrimStart('.'));
    }
    
    if (!string.IsNullOrEmpty(language))
    {
        args.Add("--language");
        args.Add(language);
    }
    
    if (!string.IsNullOrEmpty(owner))
    {
        args.Add("--owner");
        args.Add(owner);
    }
    
    args.Add("--json");
    args.Add("repository");
    
    var ghCommand = $"gh {string.Join(" ", args)}";
    
    try
    {
        var result = await ProcessHelpers.RunProcessAsync(ghCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
        
        if (result.ExitCode != 0)
        {
            var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                ? result.StandardError 
                : "Unknown error executing gh command";
            throw new Exception($"GitHub code search failed: {errorMsg}");
        }
        
        return ParseCodeSearchForRepositories(result.StandardOutput, maxResults);
    }
    catch (Exception ex)
    {
        Logger.Error($"Error searching GitHub code for repositories: {ex.Message}");
        throw;
    }
}
```

### Parsing Repository Names from Code Search

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 559-629** - `ParseCodeSearchForRepositories` method:
```csharp
private static List<string> ParseCodeSearchForRepositories(string jsonOutput, int maxResults)
{
    var seenRepos = new HashSet<string>();
    var repoNames = new List<string>();
    
    try
    {
        using var document = JsonDocument.Parse(jsonOutput);
        var root = document.RootElement;
        
        foreach (var result in root.EnumerateArray())
        {
            if (result.TryGetProperty("repository", out var repoElement))
            {
                // Try to get full name (owner/repo format)
                string? fullName = null;
                
                if (repoElement.TryGetProperty("nameWithOwner", out var nameWithOwnerElement))
                {
                    fullName = nameWithOwnerElement.GetString();
                }
                else if (repoElement.TryGetProperty("full_name", out var fullNameElement))
                {
                    fullName = fullNameElement.GetString();
                }
                else
                {
                    // Construct from owner + name
                    string? owner = null;
                    string? name = null;
                    
                    if (repoElement.TryGetProperty("owner", out var ownerElement))
                    {
                        if (ownerElement.TryGetProperty("login", out var loginElement))
                        {
                            owner = loginElement.GetString();
                        }
                    }
                    
                    if (repoElement.TryGetProperty("name", out var nameElement))
                    {
                        name = nameElement.GetString();
                    }
                    
                    if (!string.IsNullOrEmpty(owner) && !string.IsNullOrEmpty(name))
                    {
                        fullName = $"{owner}/{name}";
                    }
                }
                
                if (!string.IsNullOrEmpty(fullName) && !seenRepos.Contains(fullName))
                {
                    repoNames.Add(fullName);
                    seenRepos.Add(fullName);
                    
                    if (repoNames.Count >= maxResults)
                    {
                        break;
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Logger.Error($"Error parsing code search results for repositories: {ex.Message}");
        throw;
    }
    
    return repoNames;
}
```

**Data flow**:
```
command.RepoFileContains (string)
command.RepoFileContainsExtension (string)
  → SearchCodeForRepositoriesAsync(query, lang, owner, stars, ext, max)
  → gh search code "{query}" --extension {ext} --limit {max} --json repository
  → JSON array of code results
  → ParseCodeSearchForRepositories()
    → Extract repository.nameWithOwner or repository.full_name or owner/name
    → Deduplicate using HashSet
    → Return List<string> of repo names
  → command.Repos.AddRange(preFilteredRepos)
```

---

## --repo-{ext}-file-contains

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 61-72** - In `TryParseSearchCommandOptions`:
```csharp
// Extension-specific repo-file-contains shortcuts
else if (arg.StartsWith("--repo-") && arg.EndsWith("-file-contains"))
{
    // Extract extension: --repo-csproj-file-contains → csproj
    var prefix = "--repo-";
    var suffix = "-file-contains";
    var ext = arg.Substring(prefix.Length, arg.Length - prefix.Length - suffix.Length);
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.RepoFileContains = terms!;
    command.RepoFileContainsExtension = MapExtensionToLanguage(ext);
}
```

### Extension Mapping

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 197-224** - `MapExtensionToLanguage` method:
```csharp
private string MapExtensionToLanguage(string ext)
{
    // Map file extension shorthand to GitHub language names
    return ext.ToLower() switch
    {
        "cs" => "csharp",
        "js" => "javascript",
        "ts" => "typescript",
        "py" => "python",
        "rb" => "ruby",
        "rs" => "rust",
        "kt" => "kotlin",
        "cpp" => "cpp",
        "c++" => "cpp",
        "yml" => "yaml",
        "md" => "markdown",
        _ => ext.ToLower()  // Pass through as-is
    };
}
```

**Execution**: Same as `--repo-file-contains` (see above), but with `RepoFileContainsExtension` set.

**GitHub CLI effect**:
```bash
# User command:
cycodgr --repo-csproj-file-contains "Microsoft.AI"

# Resulting gh command:
gh search code "Microsoft.AI" --extension csproj --limit 20 --json repository
```

---

## --file-contains

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 56**:
```csharp
public string FileContains { get; set; }    // Search code files only
```

**Constructor initialization (Line 11)**:
```csharp
FileContains = string.Empty;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 36-43** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--file-contains")
{
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.FileContains = terms!;
}
```

### Dual Behavior Detection

**File**: `src/cycodgr/Program.cs`

**Lines 104-136** - In `HandleSearchCommandAsync` (Stage 2):
```csharp
// Stage 2: Phase E - Dual behavior for --file-contains
// If --file-contains is specified WITHOUT repo pre-filtering, use it to find repos first
var hasRepoPreFiltering = !string.IsNullOrEmpty(command.RepoFileContains) || command.Repos.Any() || command.RepoPatterns.Any();
var hasFileContains = !string.IsNullOrEmpty(command.FileContains);

if (hasFileContains && !hasRepoPreFiltering)
{
    // Dual behavior: Use --file-contains to pre-filter repositories
    var extInfo = !string.IsNullOrEmpty(command.Language) 
        ? $" in {command.Language} files"
        : "";
    ConsoleHelpers.WriteLine($"## Pre-filtering repositories containing files{extInfo} with '{command.FileContains}'", ConsoleColor.Cyan, overrideQuiet: true);
    
    var preFilteredRepos = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
        command.FileContains,
        command.Language,
        command.Owner,
        command.MinStars,
        string.Empty,  // No extension filter (already in Language)
        command.MaxResults * 2); // Get more repos initially
    
    if (preFilteredRepos.Count == 0)
    {
        ConsoleHelpers.WriteLine("No repositories found matching the file content criteria", ConsoleColor.Yellow, overrideQuiet: true);
        return;
    }
    
    ConsoleHelpers.WriteLine($"Found {preFilteredRepos.Count} repositories with matching files", ConsoleColor.Green, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);
    
    // Add to repos list for subsequent file search
    command.Repos.AddRange(preFilteredRepos);
}
```

**Conditional logic**:
- `hasRepoPreFiltering` = true if ANY of:
  - `RepoFileContains` is set
  - `Repos` list is not empty
  - `RepoPatterns` list is not empty
- If `hasFileContains && !hasRepoPreFiltering` → Pre-filter mode (discover repos)
- If `hasFileContains && hasRepoPreFiltering` → File search mode (search within repos)

### File Search Execution

**File**: `src/cycodgr/Program.cs`

**Lines 371-436** - `HandleCodeSearchAsync` method:
```csharp
private static async Task HandleCodeSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    var query = !string.IsNullOrEmpty(command.FileContains) ? command.FileContains : command.Contains;
    var searchType = !string.IsNullOrEmpty(command.Language) 
        ? $"code search in {command.Language} files" 
        : "code search";
        
    ConsoleHelpers.WriteLine($"## GitHub {searchType} for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    // Search GitHub code using new helper signature
    // Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
    var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();
    
    var codeMatches = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
        query,
        allRepos,
        command.Language,
        command.Owner,
        command.MinStars,
        "",
        command.MaxResults);

    if (codeMatches.Count == 0)
    {
        ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow, overrideQuiet: true);
        return;
    }

    // Apply exclude filters (filter by repo URL)
    codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

    // Apply file paths filter if specified
    if (command.FilePaths.Any())
    {
        codeMatches = codeMatches
            .Where(m => command.FilePaths.Any(fp => m.Path == fp || m.Path.EndsWith(fp) || m.Path.Contains(fp)))
            .ToList();
    }

    if (codeMatches.Count == 0)
    {
        ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow, overrideQuiet: true);
        return;
    }

    // Output results grouped by repository
    await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, command, overrideQuiet: true);
    
    // ... (save output logic)
}
```

### GitHub Code Search Helper

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 297-374** - `SearchCodeForMatchesAsync` method:
```csharp
private static async Task<List<CodeMatch>> SearchCodeForMatchesAsync(
    string query,
    List<string> repos,
    string language,
    string owner,
    int minStars,
    string fileExtension,
    int maxResults)
{
    // Query is already built - just add qualifiers
    
    // Add owner qualifier if specified
    if (!string.IsNullOrEmpty(owner))
    {
        query = $"{query} user:{owner}";
    }
    
    // Add stars filter if specified
    if (minStars > 0)
    {
        query = $"{query} stars:>={minStars}";
    }
    
    var args = new List<string>();
    
    args.Add("search");
    args.Add("code");
    args.Add($"\"{query}\"");
    
    // Add repo qualifiers as separate arguments (not inside quoted query)
    if (repos.Any())
    {
        foreach (var repo in repos)
        {
            args.Add($"repo:{repo}");
        }
    }
    
    args.Add("--limit");
    args.Add(maxResults.ToString());
    
    if (!string.IsNullOrEmpty(fileExtension))
    {
        args.Add("--extension");
        args.Add(fileExtension.TrimStart('.'));
    }
    
    if (!string.IsNullOrEmpty(language))
    {
        args.Add("--language");
        args.Add(language);
    }
    
    args.Add("--json");
    args.Add("path,repository,sha,textMatches,url");
    
    var ghCommand = $"gh {string.Join(" ", args)}";
    
    try
    {
        var result = await ProcessHelpers.RunProcessAsync(ghCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
        
        if (result.ExitCode != 0)
        {
            var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                ? result.StandardError 
                : "Unknown error executing gh command";
            throw new Exception($"GitHub code search failed: {errorMsg}");
        }
        
        return ParseCodeSearchResults(result.StandardOutput);
    }
    catch (Exception ex)
    {
        Logger.Error($"Error searching GitHub code: {ex.Message}");
        throw;
    }
}
```

**Data flow**:
```
command.FileContains (string)
  → query parameter (with qualifiers)
  → gh search code "{query}" repo:... --limit N --language L
  → JSON response with path, repository, sha, textMatches, url
  → ParseCodeSearchResults()
  → List<CodeMatch>
  → ApplyExcludeFilters()
  → FilePaths filter (if specified)
  → FormatAndOutputCodeResults()
```

---

## --{ext}-file-contains

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 73-83** - In `TryParseSearchCommandOptions`:
```csharp
// Extension-specific file-contains shortcuts
else if (arg.StartsWith("--") && arg.EndsWith("-file-contains"))
{
    // Extract extension: --cs-file-contains → cs
    var ext = arg.Substring(2, arg.Length - 2 - "-file-contains".Length);
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.FileContains = terms!;
    command.Language = MapExtensionToLanguage(ext);
}
```

**Effect**:
- Sets `SearchCommand.FileContains`
- Sets `SearchCommand.Language` (via `MapExtensionToLanguage()`)

**Example transformation**:
```bash
# User command:
cycodgr --cs-file-contains "async Task"

# Parsed to:
command.FileContains = "async Task"
command.Language = "csharp"

# Resulting gh command:
gh search code "async Task" --language csharp --limit 10 --json path,repository,sha,textMatches,url
```

---

## --language / --extension / --in-files

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 64**:
```csharp
public string Language { get; set; }
```

**Constructor initialization (Line 17)**:
```csharp
Language = string.Empty;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 125-142** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--language")
{
    var lang = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(lang))
    {
        throw new CommandLineException($"Missing language for {arg}");
    }
    command.Language = lang!;
}
// ... (many language shortcuts - see next section)
else if (arg == "--extension" || arg == "--in-files")
{
    var ext = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(ext))
    {
        throw new CommandLineException($"Missing extension for {arg}");
    }
    command.Language = MapExtensionToLanguage(ext!);
}
```

**Usage in GitHub API calls**:

Both repository search and code search use the `Language` property:

**Repository search** (GitHubSearchHelpers.cs, Lines 259-263):
```csharp
if (!string.IsNullOrEmpty(language))
{
    args.Add("--language");
    args.Add(language);
}
```

**Code search** (GitHubSearchHelpers.cs, Lines 344-348):
```csharp
if (!string.IsNullOrEmpty(language))
{
    args.Add("--language");
    args.Add(language);
}
```

---

## Language Shortcuts

### Parsing - Tier 1 (Primary)

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 84-110**:
```csharp
// Language shortcuts - Tier 1 (Primary)
else if (arg == "--cs" || arg == "--csharp")
{
    command.Language = "csharp";
}
else if (arg == "--js" || arg == "--javascript")
{
    command.Language = "javascript";
}
else if (arg == "--ts" || arg == "--typescript")
{
    command.Language = "typescript";
}
else if (arg == "--py" || arg == "--python")
{
    command.Language = "python";
}
else if (arg == "--java")
{
    command.Language = "java";
}
else if (arg == "--go")
{
    command.Language = "go";
}
else if (arg == "--md" || arg == "--markdown")
{
    command.Language = "markdown";
}
```

### Parsing - Tier 2 (Popular)

**Lines 111-124**:
```csharp
// Language shortcuts - Tier 2 (Popular)
else if (arg == "--rb" || arg == "--ruby")
{
    command.Language = "ruby";
}
else if (arg == "--rs" || arg == "--rust")
{
    command.Language = "rust";
}
else if (arg == "--php")
{
    command.Language = "php";
}
else if (arg == "--cpp" || arg == "--c++")
{
    command.Language = "cpp";
}
else if (arg == "--swift")
{
    command.Language = "swift";
}
else if (arg == "--kt" || arg == "--kotlin")
{
    command.Language = "kotlin";
}
```

### Parsing - Tier 3 (Config/Markup)

**Lines 125-142** (partial - language section only):
```csharp
// Language shortcuts - Tier 3 (Config/Markup)
else if (arg == "--yml" || arg == "--yaml")
{
    command.Language = "yaml";
}
else if (arg == "--json")
{
    command.Language = "json";
}
else if (arg == "--xml")
{
    command.Language = "xml";
}
else if (arg == "--html")
{
    command.Language = "html";
}
else if (arg == "--css")
{
    command.Language = "css";
}
```

**Complete list of shortcuts**:
- `--cs`, `--csharp` → "csharp"
- `--js`, `--javascript` → "javascript"
- `--ts`, `--typescript` → "typescript"
- `--py`, `--python` → "python"
- `--java` → "java"
- `--go` → "go"
- `--md`, `--markdown` → "markdown"
- `--rb`, `--ruby` → "ruby"
- `--rs`, `--rust` → "rust"
- `--php` → "php"
- `--cpp`, `--c++` → "cpp"
- `--swift` → "swift"
- `--kt`, `--kotlin` → "kotlin"
- `--yml`, `--yaml` → "yaml"
- `--json` → "json"
- `--xml` → "xml"
- `--html` → "html"
- `--css` → "css"

---

## --file-path / --file-paths

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 60**:
```csharp
public List<string> FilePaths { get; set; } // File paths to filter results (from --file-path / --file-paths)
```

**Constructor initialization (Line 15)**:
```csharp
FilePaths = new List<string>();
```

### Parsing --file-path

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 173-181** - In `TryParseSharedCycoGrCommandOptions`:
```csharp
else if (arg == "--file-path" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd)
{
    var path = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(path))
    {
        throw new CommandLineException($"Missing file path for {arg}");
    }
    searchCmd.FilePaths.Add(path!);
}
```

### Parsing --file-paths

**Lines 182-221**:
```csharp
else if (arg == "--file-paths" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd2)
{
    var pathArgs = GetInputOptionArgs(i + 1, args);
    
    foreach (var pathArg in pathArgs)
    {
        // Handle @ file loading (if @ wasn't already expanded)
        if (pathArg.StartsWith("@"))
        {
            var fileName = pathArg.Substring(1);
            if (!FileHelpers.FileExists(fileName))
            {
                throw new CommandLineException($"File paths file not found: {fileName}");
            }
            var fileContent = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
            var normalized = fileContent.Replace("\r\n", "\n").Replace("\r", "\n");
            var pathsFromFile = normalized
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());
            searchCmd2.FilePaths.AddRange(pathsFromFile);
        }
        // Handle case where @ was already expanded to a single string with embedded newlines
        else if (pathArg.Contains("\n") || pathArg.Contains("\r"))
        {
            var normalized = pathArg.Replace("\r\n", "\n").Replace("\r", "\n");
            var paths = normalized
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());
            searchCmd2.FilePaths.AddRange(paths);
        }
        // Regular path arg
        else
        {
            searchCmd2.FilePaths.Add(pathArg);
        }
    }
    i += pathArgs.Count();
}
```

**Key behaviors**:
- `@file` loading: Reads file, splits by newlines, adds all paths
- Embedded newlines: Handles pre-expanded `@file` content
- Normalization: Handles both `\r\n` and `\n` line endings
- Whitespace: Trims and skips empty lines

### Filtering Application

**File**: `src/cycodgr/Program.cs`

**Lines 404-409** - In `HandleCodeSearchAsync`:
```csharp
// Apply file paths filter if specified
if (command.FilePaths.Any())
{
    codeMatches = codeMatches
        .Where(m => command.FilePaths.Any(fp => m.Path == fp || m.Path.EndsWith(fp) || m.Path.Contains(fp)))
        .ToList();
}
```

**Matching strategies**:
1. **Exact match**: `m.Path == fp`
2. **Suffix match**: `m.Path.EndsWith(fp)` (useful for relative paths)
3. **Contains match**: `m.Path.Contains(fp)` (substring search)

**Example**:
```bash
# User specifies:
cycodgr microsoft/terminal --file-contains "ConPTY" --file-path "Terminal.cpp"

# Matches any of:
# - src/cascadia/TerminalCore/Terminal.cpp (EndsWith match)
# - Terminal.cpp (exact match)
# - src/Terminal.cpp (Contains match)
```

---

## --exclude

### Command Property Definition

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 17 & 35**:
```csharp
Exclude = new List<string>();  // Constructor
// ...
public List<string> Exclude;   // Property
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 282-289** - In `TryParseSharedCycoGrCommandOptions`:
```csharp
else if (arg == "--exclude")
{
    var excludeArgs = GetInputOptionArgs(i + 1, args);
    if (excludeArgs.Count() == 0)
    {
        throw new CommandLineException($"Missing pattern(s) for {arg}");
    }
    command.Exclude.AddRange(excludeArgs);
    i += excludeArgs.Count();
}
```

### Filtering Application

**File**: `src/cycodgr/Program.cs`

**Lines 1343-1377** - `ApplyExcludeFilters` method:
```csharp
private static List<T> ApplyExcludeFilters<T>(List<T> items, List<string> excludePatterns, Func<T, string> urlGetter)
{
    if (excludePatterns.Count == 0)
    {
        return items;
    }

    var filtered = items.Where(item =>
    {
        var url = urlGetter(item);
        foreach (var pattern in excludePatterns)
        {
            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(url, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return false; // Exclude this item
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Invalid regex pattern '{pattern}': {ex.Message}");
            }
        }
        return true; // Keep this item
    }).ToList();

    var excludedCount = items.Count - filtered.Count;
    if (excludedCount > 0)
    {
        ConsoleHelpers.WriteLine($"Excluded {excludedCount} result(s) matching exclude pattern(s)", ConsoleColor.Yellow);
    }

    return filtered;
}
```

**Usage in repo search** (HandleRepoSearchAsync, Line 328):
```csharp
// Apply exclude filters
repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);
```

**Usage in code search** (HandleCodeSearchAsync, Line 401):
```csharp
// Apply exclude filters (filter by repo URL)
codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);
```

**Key characteristics**:
- Generic method: Works with `List<RepoInfo>` or `List<CodeMatch>`
- URL-based filtering: Uses `urlGetter` lambda to extract URL
- Regex matching: Case-insensitive
- Error handling: Logs warnings for invalid patterns, continues execution
- Feedback: Reports excluded count to user

**Data flow**:
```
command.Exclude (List<string> of regex patterns)
  → ApplyExcludeFilters(items, patterns, urlGetter)
    → For each item:
      → Get URL via urlGetter(item)
      → For each pattern:
        → Regex.IsMatch(url, pattern, IgnoreCase)
        → If match → exclude item (return false)
      → If no match → keep item (return true)
  → Filtered list
  → Log excluded count
```

---

## Summary: Complete Call Stack for Layer 2

### Example: `cycodgr --repo-csproj-file-contains "SDK" --cs-file-contains "ChatClient"`

**Call stack**:

```
1. Main(args)
     ↓
2. CycoGrCommandLineOptions.Parse(args, out options, out ex)
     ↓
3. CommandLineOptions.Parse(args, out ex)
     ↓
4. CommandLineOptions.ParseInputOptions(allInputs)
     ↓
5. CommandLineOptions.TryParseInputOptions(ref command, args, ref i, arg)
     ↓
6. CycoGrCommandLineOptions.TryParseOtherCommandOptions(command, args, ref i, arg)
     ↓
7. CycoGrCommandLineOptions.TryParseSearchCommandOptions(searchCommand, args, ref i, arg)
     ↓
     [For "--repo-csproj-file-contains":]
     8a. Extract "csproj" from arg name
     9a. Set command.RepoFileContains = "SDK"
     10a. Set command.RepoFileContainsExtension = MapExtensionToLanguage("csproj") = "csharp"
     ↓
     [For "--cs-file-contains":]
     8b. Extract "cs" from arg name
     9b. Set command.FileContains = "ChatClient"
     10b. Set command.Language = MapExtensionToLanguage("cs") = "csharp"
     ↓
11. Program.HandleSearchCommandAsync(searchCommand)
     ↓
     [Stage 1: Pre-filter repos]
     12. GitHubSearchHelpers.SearchCodeForRepositoriesAsync(
           "SDK", "csharp", owner, stars, "csproj", maxResults*2)
     ↓
     13. ProcessHelpers.RunProcessAsync("gh search code \"SDK\" --extension csproj --language csharp --limit 20 --json repository")
     ↓
     14. ParseCodeSearchForRepositories(jsonOutput)
     ↓
     15. command.Repos.AddRange(preFilteredRepos)
     ↓
     [Stage 3: Code search in pre-filtered repos]
     16. Program.HandleCodeSearchAsync(searchCommand)
     ↓
     17. Combine RepoPatterns + Repos → allRepos
     ↓
     18. GitHubSearchHelpers.SearchCodeAsync(
           "ChatClient", allRepos, "csharp", owner, stars, "", maxResults)
     ↓
     19. ProcessHelpers.RunProcessAsync("gh search code \"ChatClient\" repo:owner1/repo1 repo:owner2/repo2 --language csharp --limit 10 --json path,repository,sha,textMatches,url")
     ↓
     20. ParseCodeSearchResults(jsonOutput)
     ↓
     21. ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url)
     ↓
     22. Filter by FilePaths (if specified)
     ↓
     23. FormatAndOutputCodeResults(...)
```

---

## Key Source Files Summary

| File | Lines | Purpose |
|------|-------|---------|
| `CycoGrCommandLineOptions.cs` | 36-142 | Parse all container filtering options |
| `CycoGrCommandLineOptions.cs` | 197-224 | Map extensions to languages |
| `SearchCommand.cs` | 9-34, 51-60 | Store all filtering properties |
| `Program.cs` | 76-136 | Pre-filtering stages (repo discovery) |
| `Program.cs` | 299-369 | Repository and code search execution |
| `Program.cs` | 404-409 | FilePaths post-search filtering |
| `Program.cs` | 1343-1377 | Exclude pattern filtering (generic) |
| `GitHubSearchHelpers.cs` | 43-104 | Search code for repositories (pre-filter) |
| `GitHubSearchHelpers.cs` | 201-295 | Search repositories by keywords |
| `GitHubSearchHelpers.cs` | 297-374 | Search code for matches |
| `GitHubSearchHelpers.cs` | 559-629 | Parse repositories from code search |

---

**End of Layer 2 Proof Document**
