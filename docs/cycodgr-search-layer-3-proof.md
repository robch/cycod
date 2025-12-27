# cycodgr search - Layer 3: CONTENT FILTERING - PROOF

## Purpose

This document provides **source code evidence** for Layer 3 (Content Filtering) features in cycodgr.

---

## Table of Contents

1. [--contains (Unified Search)](#contains)
2. [--line-contains](#line-contains)
3. [File Content Fetching](#file-content-fetching)
4. [Line Filtering Implementation](#line-filtering-implementation)

---

## --contains

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 55**:
```csharp
public string Contains { get; set; }        // Search both repo metadata AND code
```

**Constructor initialization (Line 10)**:
```csharp
Contains = string.Empty;
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 18-25** - In `TryParseSearchCommandOptions`:
```csharp
if (arg == "--contains")
{
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.Contains = terms!;
}
```

### Execution - Unified Search Path

**File**: `src/cycodgr/Program.cs`

**Lines 148-151** - In `HandleSearchCommandAsync`:
```csharp
// Scenario 2: --contains (unified search - both repos and code)
else if (hasContains)
{
    await HandleUnifiedSearchAsync(command);
}
```

**Lines 230-297** - `HandleUnifiedSearchAsync` method:
```csharp
private static async Task HandleUnifiedSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    var query = command.Contains;
    ConsoleHelpers.WriteLine($"## GitHub unified search for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    // Combine RepoPatterns (positional args) with Repos (--repo, @file, pre-filtered)
    var allRepos = command.RepoPatterns.Concat(command.Repos).Distinct().ToList();

    // Search both repos and code (N of each per spec recommendation)
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
        command.MaxResults);

    var codeTask = CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
        query,
        allRepos,
        command.Language,
        command.Owner,
        command.MinStars,
        "",
        command.MaxResults);

    await Task.WhenAll(repoTask, codeTask);

    var repos = repoTask.Result;
    var codeMatches = codeTask.Result;

    // Apply exclude filters
    repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);
    codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

    if (repos.Count == 0 && codeMatches.Count == 0)
    {
        ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow, overrideQuiet: true);
        return;
    }

    // Output repos that matched (no file sections)
    if (repos.Count > 0)
    {
        var repoOutput = FormatRepoOutput(repos, "detailed");
        ConsoleHelpers.WriteLine(repoOutput, overrideQuiet: true);
    }

    // Output code matches (with file sections under repos)
    if (codeMatches.Count > 0)
    {
        await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, command, overrideQuiet: true);
    }
    
    // ... (save output logic)
}
```

**Key characteristic**: Executes **two parallel searches** - one for repositories, one for code.

---

## --line-contains

### Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 80**:
```csharp
public List<string> LineContainsPatterns { get; set; }  // Patterns for line filtering (separate from search query)
```

**Constructor initialization (Line 26)**:
```csharp
LineContainsPatterns = new List<string>();
```

### Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 181-186** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--line-contains" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd3)
{
    var patterns = GetInputOptionArgs(i + 1, args, required: 1);
    searchCmd3.LineContainsPatterns.AddRange(patterns);
    i += patterns.Count();
}
```

**Behavior**: Accumulates multiple patterns (can specify `--line-contains` multiple times)

---

## File Content Fetching

### Implementation

**File**: `src/cycodgr/Program.cs`

**Lines 758-780** - In `ProcessFileGroupAsync`:
```csharp
// Fetch full file content and display with real line numbers
var rawUrl = ConvertToRawUrl(firstMatch.Url);
try
{
    // Create FoundTextFile with lambda to load content
    var foundFile = new FoundTextFile
    {
        Path = firstMatch.Path,
        LoadContent = async () =>
        {
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CycoGr/1.0");
            return await httpClient.GetStringAsync(rawUrl);
        },
        Metadata = new Dictionary<string, object>
        {
            { "Repository", repo },
            { "Sha", firstMatch.Sha },
            { "Url", firstMatch.Url }
        }
    };

    // Load the content
    foundFile.Content = await foundFile.LoadContent();
```

### URL Conversion

**Lines 890-898** - `ConvertToRawUrl` method:
```csharp
private static string ConvertToRawUrl(string blobUrl)
{
    // Convert https://github.com/owner/repo/blob/sha/path to https://raw.githubusercontent.com/owner/repo/sha/path
    if (blobUrl.Contains("/blob/"))
    {
        return blobUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
    }
    return blobUrl;
}
```

**Example transformation**:
```
Input:  https://github.com/microsoft/terminal/blob/main/src/Terminal.cpp
Output: https://raw.githubusercontent.com/microsoft/terminal/main/src/Terminal.cpp
```

---

## Line Filtering Implementation

### Pattern Selection

**File**: `src/cycodgr/Program.cs`

**Lines 783-800** - In `ProcessFileGroupAsync`:
```csharp
// Use LineHelpers to filter and display with real line numbers
// Determine which patterns to use for line filtering
List<System.Text.RegularExpressions.Regex> includePatterns;

if (command.LineContainsPatterns.Any())
{
    // Use explicit --line-contains patterns if specified
    includePatterns = command.LineContainsPatterns
        .Select(p => new System.Text.RegularExpressions.Regex(p, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        .ToList();
}
else
{
    // Fallback to using the search query
    includePatterns = new List<System.Text.RegularExpressions.Regex>
    {
        new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(query), System.Text.RegularExpressions.RegexOptions.IgnoreCase)
    };
}
```

**Logic**:
1. If `LineContainsPatterns` is not empty → Use those patterns
2. Otherwise → Escape search query and use as pattern (literal match)

### Line Filtering Execution

**Lines 802-816**:
```csharp
var excludePatterns = new List<System.Text.RegularExpressions.Regex>();

var lang = DetectLanguageFromPath(firstMatch.Path);
var backticks = $"```{lang}";

var filteredContent = LineHelpers.FilterAndExpandContext(
    foundFile.Content,
    includePatterns,
    contextLines,  // lines before
    contextLines,  // lines after
    true,          // include line numbers
    excludePatterns,
    backticks,
    true           // highlight matches
);
```

**Parameters**:
- `includePatterns`: Regex list for lines to include
- `contextLines`: Lines before/after (from Layer 5: `command.LinesBeforeAndAfter`)
- `includeLineNumbers`: Always `true` for cycodgr
- `excludePatterns`: Always empty (Layer 4 is not implemented)
- `codeBlockStart`: Language-specific code fence (```lang)
- `highlightMatches`: Always `true`

### Language Detection

**Lines 900-932** - `DetectLanguageFromPath` method:
```csharp
private static string DetectLanguageFromPath(string path)
{
    var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
    return ext switch
    {
        ".cs" => "csharp",
        ".js" => "javascript",
        ".ts" => "typescript",
        ".py" => "python",
        ".java" => "java",
        ".cpp" or ".cc" or ".cxx" => "cpp",
        ".c" => "c",
        ".h" or ".hpp" => "cpp",
        ".go" => "go",
        ".rs" => "rust",
        ".rb" => "ruby",
        ".php" => "php",
        ".swift" => "swift",
        ".kt" or ".kts" => "kotlin",
        ".scala" => "scala",
        ".sh" or ".bash" => "bash",
        ".ps1" => "powershell",
        ".sql" => "sql",
        ".html" or ".htm" => "html",
        ".css" => "css",
        ".xml" => "xml",
        ".json" => "json",
        ".yaml" or ".yml" => "yaml",
        ".md" or ".markdown" => "markdown",
        ".txt" => "text",
        _ => ""
    };
}
```

### Fallback Display

**Lines 829-851** - Fallback when file fetch fails:
```csharp
catch (Exception ex)
{
    output.AppendLine($"Error fetching file content: {ex.Message}");
    output.AppendLine("Falling back to fragment display...");
    
    // Fallback to fragment display
    foreach (var match in fileGroup)
    {
        if (match.TextMatches?.Any() == true)
        {
            var lang = DetectLanguageFromPath(match.Path);
            output.AppendLine($"```{lang}");
            
            foreach (var textMatch in match.TextMatches)
            {
                var fragment = textMatch.Fragment;
                output.AppendLine(fragment);
            }
            
            output.AppendLine("```");
        }
    }
}
```

**Behavior**: Displays GitHub's text match fragments without line filtering

---

## Complete Call Stack

### Example: `cycodgr microsoft/terminal --file-contains "ConPTY" --line-contains "Create.*"`

```
1. HandleSearchCommandAsync(command)
     ↓
2. HandleCodeSearchAsync(command)
     ↓
3. GitHubSearchHelpers.SearchCodeAsync(...)
     ↓
4. FormatAndOutputCodeResults(codeMatches, ...)
     ↓
5. ProcessFileGroupAsync(fileGroup, repo, query, contextLines, ...)
     ↓
6. Fetch file content:
     ConvertToRawUrl(match.Url)
     HttpClient.GetStringAsync(rawUrl)
     ↓
7. Determine line patterns:
     command.LineContainsPatterns.Any() ? → Use those
       : → Escape query and use as pattern
     ↓
8. Create Regex objects (case-insensitive):
     new Regex("Create.*", RegexOptions.IgnoreCase)
     ↓
9. LineHelpers.FilterAndExpandContext(
     content, includePatterns, contextLines, contextLines,
     includeLineNumbers: true, excludePatterns: [],
     codeBlockStart: "```csharp", highlightMatches: true)
     ↓
10. Display filtered content with line numbers
```

---

## Key Source Files Summary

| File | Lines | Purpose |
|------|-------|---------|
| `CycoGrCommandLineOptions.cs` | 18-25, 181-186 | Parse --contains, --line-contains |
| `SearchCommand.cs` | 10, 26, 55, 80 | Store Contains, LineContainsPatterns |
| `Program.cs` | 230-297 | Execute unified search (--contains) |
| `Program.cs` | 758-876 | Fetch file content and filter lines |
| `Program.cs` | 890-898 | Convert GitHub URLs to raw URLs |
| `Program.cs` | 900-932 | Detect language from file path |

---

**End of Layer 3 Proof Document**
