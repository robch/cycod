# cycodgr Search Command - Layer 7: Output Persistence - PROOF

**[Back to Layer 7 Catalog](cycodgr-search-filtering-pipeline-catalog-layer-7.md)** | **[Back to README](cycodgr-search-filtering-pipeline-catalog-README.md)**

---

## Overview

This document provides **source code evidence** for all Layer 7 (Output Persistence) functionality in cycodgr's search command, with exact line numbers, data flow analysis, and implementation details.

---

## Command-Line Option Parsing

### Base Class Properties

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

```csharp
// Lines 7-18: Output persistence properties defined in base class
public CycoGrCommand()
{
    SaveOutput = string.Empty;          // Line 7
    SaveJson = string.Empty;            // Line 8
    SaveCsv = string.Empty;             // Line 9
    SaveTable = string.Empty;           // Line 10
    SaveUrls = string.Empty;            // Line 11
    SaveRepos = string.Empty;           // Line 12
    SaveFilePaths = string.Empty;       // Line 13
    SaveRepoUrls = string.Empty;        // Line 14
    SaveFileUrls = string.Empty;        // Line 15
    // ... other properties ...
}

// Lines 25-35: Public string properties
public string SaveOutput;       // Line 25
public string SaveJson;         // Line 26
public string SaveCsv;          // Line 27
public string SaveTable;        // Line 28
public string SaveUrls;         // Line 29
public string SaveRepos;        // Line 30
public string SaveFilePaths;    // Line 31
public string SaveRepoUrls;     // Line 32
public string SaveFileUrls;     // Line 33
```

**Evidence**: All output persistence options are declared as public properties in the base `CycoGrCommand` class, inherited by `SearchCommand`.

---

### Command-Line Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

#### `--save-output`
```csharp
// Lines 409-417: Parse --save-output option
else if (arg == "--save-output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var saveOutput = max1Arg.FirstOrDefault() ?? "search-output.md";  // Default template
    command.SaveOutput = saveOutput;
    i += max1Arg.Count();
}
```

**Evidence**: 
- Parsed at line 409-417
- Default value: `"search-output.md"`
- Accepts optional argument (defaults if not provided)

---

#### `--save-json`
```csharp
// Lines 525-533: Parse --save-json option
if (!string.IsNullOrEmpty(command.SaveJson))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("output.json", command.SaveJson)!;
    string jsonContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        jsonContent = FormatAsJson(repos);
    }
    else if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        jsonContent = FormatCodeAsJson(matches);
    }
```

**Parser Location**: CycoGrCommandLineOptions.cs, lines 439-448 (SaveAdditionalFormats call site)

```csharp
// Lines 442-448: Parsing --save-json
if (!string.IsNullOrEmpty(command.SaveJson))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("output.json", command.SaveJson)!;
    // ... JSON formatting logic ...
    FileHelpers.WriteAllText(fileName, jsonContent);
    savedFiles.Add(fileName);
}
```

**Evidence**: Default template is `"output.json"`, supports both RepoInfo and CodeMatch data types.

---

#### `--save-csv`
```csharp
// Lines 464-484: Parse --save-csv option
if (!string.IsNullOrEmpty(command.SaveCsv))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("output.csv", command.SaveCsv)!;
    string csvContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        csvContent = FormatAsCsv(repos);
    }
    else if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        csvContent = FormatCodeAsCsv(matches);
    }
    else
    {
        return;
    }
    
    FileHelpers.WriteAllText(fileName, csvContent);
    savedFiles.Add(fileName);
}
```

**Evidence**: Default template is `"output.csv"`, contextual formatting based on data type.

---

#### `--save-table`
```csharp
// Lines 486-508: Parse --save-table option
if (!string.IsNullOrEmpty(command.SaveTable))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("output.md", command.SaveTable)!;
    string tableContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        tableContent = FormatAsTable(repos);
    }
    else
    {
        // Code search doesn't have table format, skip
        return;
    }
    
    var saveOutput = new StringBuilder();
    saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
    saveOutput.AppendLine();
    saveOutput.AppendLine(tableContent);
    
    FileHelpers.WriteAllText(fileName, saveOutput.ToString());
    savedFiles.Add(fileName);
}
```

**Evidence**: 
- Only works for repository search (returns early for code matches)
- Default template: `"output.md"`
- Wraps table with header

---

#### `--save-urls`
```csharp
// Lines 510-532: Parse --save-urls option (contextual behavior)
if (!string.IsNullOrEmpty(command.SaveUrls))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("output.txt", command.SaveUrls)!;
    string urlsContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        // Contextual: save clone URLs for repo search
        urlsContent = FormatAsRepoCloneUrls(repos);
    }
    else if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        // Contextual: save file URLs for code search
        urlsContent = FormatCodeAsFileUrls(matches);
    }
    else
    {
        return;
    }
    
    FileHelpers.WriteAllText(fileName, urlsContent);
    savedFiles.Add(fileName);
}
```

**Evidence**: 
- **Dual behavior** based on search type
- Repo search → clone URLs
- Code search → file URLs
- Default template: `"output.txt"`

---

#### `--save-repos`
```csharp
// Lines 534-554: Parse --save-repos option
if (!string.IsNullOrEmpty(command.SaveRepos))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("repos.txt", command.SaveRepos)!;
    string reposContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        reposContent = FormatAsRepoList(repos);
    }
    else if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        reposContent = FormatCodeAsRepoList(matches);
    }
    else
    {
        return;
    }
    
    FileHelpers.WriteAllText(fileName, reposContent);
    savedFiles.Add(fileName);
}
```

**Evidence**: 
- Works for both repo and code search
- Default template: `"repos.txt"`
- Format: `owner/repo` (compatible with `--repos @file`)

---

#### `--save-file-paths`
```csharp
// Lines 556-581: Parse --save-file-paths option (per-repo files)
if (!string.IsNullOrEmpty(command.SaveFilePaths))
{
    if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        // Group by repo and save each repo's paths separately
        var repoGroups = matches.GroupBy(m => m.Repository.FullName ?? $"{m.Repository.Owner}/{m.Repository.Name}");
        
        foreach (var repoGroup in repoGroups)
        {
            var repoName = repoGroup.Key.Replace('/', '-');
            var template = command.SaveFilePaths.Replace("{repo}", repoName);
            var fileName = FileHelpers.GetFileNameFromTemplate("files-{repo}.txt", template)!;
            
            var paths = repoGroup
                .Select(m => m.Path)
                .Distinct()
                .OrderBy(p => p);
            
            // Use \r\n for Windows compatibility with File.ReadAllLines
            var content = string.Join("\r\n", paths) + "\r\n";
            // Write without BOM to avoid issues with @ file loading
            File.WriteAllText(fileName, content, new System.Text.UTF8Encoding(false));
            savedFiles.Add(fileName);
        }
    }
}
```

**Evidence**: 
- **One file per repository**
- Template variable `{repo}` replaced with sanitized repo name (`/` → `-`)
- CRLF line endings (`\r\n`) for Windows compatibility
- UTF-8 **without BOM** for `@file` compatibility
- Default template: `"files-{repo}.txt"`

---

#### `--save-repo-urls`
```csharp
// Lines 583-606: Parse --save-repo-urls option
if (!string.IsNullOrEmpty(command.SaveRepoUrls))
{
    var fileName = FileHelpers.GetFileNameFromTemplate("repo-urls.txt", command.SaveRepoUrls)!;
    string urlsContent;
    
    if (data is List<CycoGr.Models.RepoInfo> repos)
    {
        urlsContent = FormatAsRepoCloneUrls(repos);
    }
    else if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        urlsContent = FormatCodeAsRepoCloneUrls(matches);
    }
    else
    {
        urlsContent = string.Empty;
    }
    
    if (!string.IsNullOrEmpty(urlsContent))
    {
        FileHelpers.WriteAllText(fileName, urlsContent);
        savedFiles.Add(fileName);
    }
}
```

**Evidence**: 
- Works for both repo and code search
- Always outputs clone URLs (`.git` format)
- Default template: `"repo-urls.txt"`

---

#### `--save-file-urls`
```csharp
// Lines 608-630: Parse --save-file-urls option (per-repo file URLs)
if (!string.IsNullOrEmpty(command.SaveFileUrls))
{
    if (data is List<CycoGr.Models.CodeMatch> matches)
    {
        // Group by repo for file URLs
        var repoGroups = matches.GroupBy(m => m.Repository.FullName ?? $"{m.Repository.Owner}/{m.Repository.Name}");
        
        foreach (var repoGroup in repoGroups)
        {
            var repoName = repoGroup.Key.Replace('/', '-');
            var template = command.SaveFileUrls.Replace("{repo}", repoName);
            var fileName = FileHelpers.GetFileNameFromTemplate("file-urls-{repo}.txt", template)!;
            
            var urls = repoGroup
                .Select(m => $"https://github.com/{repoGroup.Key}/blob/main/{m.Path}")
                .Distinct()
                .OrderBy(u => u);
            
            FileHelpers.WriteAllText(fileName, string.Join(Environment.NewLine, urls));
            savedFiles.Add(fileName);
        }
    }
}
```

**Evidence**: 
- **One file per repository**
- Code search only
- GitHub blob URLs format
- Template variable `{repo}` replaced with sanitized name
- Default template: `"file-urls-{repo}.txt"`

---

## Execution Flow

### Entry Points by Search Mode

**File**: `src/cycodgr/Program.cs`

#### Repo Search Mode
```csharp
// Lines 299-369: HandleRepoSearchAsync - uses SaveAdditionalFormats
private static async Task HandleRepoSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    // ... search logic ...
    
    // Lines 354-365: Save --save-output if specified
    if (!string.IsNullOrEmpty(command.SaveOutput))
    {
        var saveOutput = new StringBuilder();
        saveOutput.AppendLine($"## GitHub repository search for '{query}'");
        saveOutput.AppendLine();
        saveOutput.AppendLine(output);
        
        var saveFileName = FileHelpers.GetFileNameFromTemplate("repo-output.md", command.SaveOutput)!;
        FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
        ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
    }

    // Line 368: Save additional formats
    SaveAdditionalFormats(command, repos, query, "repository search");
}
```

**Evidence**: 
- `--save-output` is handled inline (lines 354-365)
- All other formats delegated to `SaveAdditionalFormats` (line 368)

---

#### Code Search Mode
```csharp
// Lines 371-436: HandleCodeSearchAsync - uses SaveAdditionalFormats
private static async Task HandleCodeSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    // ... search logic ...
    
    // Lines 420-432: Save --save-output if specified
    if (!string.IsNullOrEmpty(command.SaveOutput))
    {
        var saveOutput = new StringBuilder();
        saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
        saveOutput.AppendLine();
        
        // TODO: Format for saving
        
        var saveFileName = FileHelpers.GetFileNameFromTemplate("code-output.md", command.SaveOutput)!;
        FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
        ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
    }

    // Line 435: Save additional formats
    SaveAdditionalFormats(command, codeMatches, query, searchType);
}
```

**Evidence**: Similar pattern to repo search mode.

---

#### Unified Search Mode
```csharp
// Lines 230-297: HandleUnifiedSearchAsync
private static async Task HandleUnifiedSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
{
    // ... search logic ...
    
    // Lines 290-296: Save --save-output if specified
    if (!string.IsNullOrEmpty(command.SaveOutput))
    {
        // TODO: Build save output
        var saveFileName = FileHelpers.GetFileNameFromTemplate("unified-output.md", command.SaveOutput)!;
        // FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
        ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
    }
}
```

**Evidence**: 
- Unified search has incomplete `--save-output` implementation (TODO comment at line 292)
- Additional formats are not called for unified search (different data structure)

---

### SaveAdditionalFormats Function

**File**: `src/cycodgr/Program.cs`

```csharp
// Lines 438-639: Central handler for all additional formats
private static void SaveAdditionalFormats(CycoGr.CommandLine.CycoGrCommand command, object data, string query, string searchType)
{
    var savedFiles = new List<string>();  // Line 440: Track saved files

    // Lines 442-462: --save-json
    if (!string.IsNullOrEmpty(command.SaveJson)) { ... }

    // Lines 464-484: --save-csv
    if (!string.IsNullOrEmpty(command.SaveCsv)) { ... }

    // Lines 486-508: --save-table
    if (!string.IsNullOrEmpty(command.SaveTable)) { ... }

    // Lines 510-532: --save-urls
    if (!string.IsNullOrEmpty(command.SaveUrls)) { ... }

    // Lines 534-554: --save-repos
    if (!string.IsNullOrEmpty(command.SaveRepos)) { ... }

    // Lines 556-581: --save-file-paths
    if (!string.IsNullOrEmpty(command.SaveFilePaths)) { ... }

    // Lines 583-606: --save-repo-urls
    if (!string.IsNullOrEmpty(command.SaveRepoUrls)) { ... }

    // Lines 608-630: --save-file-urls
    if (!string.IsNullOrEmpty(command.SaveFileUrls)) { ... }

    // Lines 632-638: Display success messages
    if (savedFiles.Any())
    {
        foreach (var file in savedFiles)
        {
            ConsoleHelpers.WriteLine($"Saved to: {file}", ConsoleColor.Green);
        }
    }
}
```

**Evidence**: 
- **Central dispatcher** for all save formats except `--save-output`
- Type-safe handling of `RepoInfo` vs `CodeMatch` data
- Collects filenames and reports all saves at end

---

## Formatting Functions

### Repository Formatting

#### FormatAsRepoList
```csharp
// Lines 1140-1144: Format repos as owner/repo list
private static string FormatAsRepoList(List<CycoGr.Models.RepoInfo> repos)
{
    // Format: owner/name (one per line, for @repos.txt loading)
    return string.Join(Environment.NewLine, repos.Select(r => r.FullName ?? $"{r.Owner}/{r.Name}"));
}
```

**Evidence**: Simple newline-delimited list, compatible with `--repos @file`.

---

#### FormatAsRepoCloneUrls
```csharp
// Lines 1156-1161: Format repos as clone URLs
private static string FormatAsRepoCloneUrls(List<CycoGr.Models.RepoInfo> repos)
{
    // Format: clone URLs (one per line)
    return string.Join(Environment.NewLine, 
        repos.Select(r => $"https://github.com/{r.FullName ?? $"{r.Owner}/{r.Name}"}.git"));
}
```

**Evidence**: Appends `.git` to create clone URLs.

---

#### FormatAsJson
```csharp
// Lines 1306-1326: Format repos as JSON
private static string FormatAsJson(List<CycoGr.Models.RepoInfo> repos)
{
    var items = repos.Select(r => new
    {
        url = r.Url,
        name = r.Name,
        owner = r.Owner,
        fullName = r.FullName,
        stars = r.Stars,
        language = r.Language,
        description = r.Description,
        forks = r.Forks,
        openIssues = r.OpenIssues,
        updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd")
    });

    return System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
}
```

**Evidence**: Uses System.Text.Json with indented formatting.

---

#### FormatAsCsv
```csharp
// Lines 1328-1341: Format repos as CSV
private static string FormatAsCsv(List<CycoGr.Models.RepoInfo> repos)
{
    var output = new StringBuilder();
    output.AppendLine("url,name,owner,stars,language,description");
    
    foreach (var repo in repos)
    {
        var desc = repo.Description?.Replace("\"", "\"\"") ?? "";
        var lang = repo.Language ?? "";
        output.AppendLine($"\"{repo.Url}\",\"{repo.Name}\",\"{repo.Owner}\",{repo.Stars},\"{lang}\",\"{desc}\"");
    }
    
    return output.ToString().TrimEnd();
}
```

**Evidence**: 
- Proper CSV escaping (double-quotes escaped as `""`)
- Headers included
- Numeric values unquoted

---

#### FormatAsTable
```csharp
// Lines 1287-1304: Format repos as markdown table
private static string FormatAsTable(List<CycoGr.Models.RepoInfo> repos)
{
    var output = new StringBuilder();
    output.AppendLine("| Repository | Stars | Language | Description |");
    output.AppendLine("|------------|-------|----------|-------------|");
    
    foreach (var repo in repos)
    {
        var lang = repo.Language ?? "unknown";
        var desc = string.IsNullOrWhiteSpace(repo.Description) 
            ? "(no description)" 
            : (repo.Description.Length > 50 ? repo.Description.Substring(0, 47) + "..." : repo.Description);
        
        output.AppendLine($"| {repo.FullName} | {repo.FormattedStars} | {lang} | {desc} |");
    }
    
    return output.ToString().TrimEnd();
}
```

**Evidence**: 
- Markdown table format
- Description truncated to 50 chars
- Uses `FormattedStars` property (e.g., `1.2k`)

---

### Code Match Formatting

#### FormatCodeAsRepoList
```csharp
// Lines 1146-1154: Format code matches as repo list
private static string FormatCodeAsRepoList(List<CycoGr.Models.CodeMatch> matches)
{
    // Format: owner/name (one per line, for @repos.txt loading)
    var uniqueRepos = matches
        .Select(m => m.Repository.FullName ?? $"{m.Repository.Owner}/{m.Repository.Name}")
        .Distinct()
        .OrderBy(r => r);
    return string.Join(Environment.NewLine, uniqueRepos);
}
```

**Evidence**: Extracts unique repositories from code matches.

---

#### FormatCodeAsRepoCloneUrls
```csharp
// Lines 1163-1171: Format code matches as repo clone URLs
private static string FormatCodeAsRepoCloneUrls(List<CycoGr.Models.CodeMatch> matches)
{
    // Format: clone URLs (one per line)
    var uniqueRepos = matches
        .Select(m => m.Repository.FullName ?? $"{m.Repository.Owner}/{m.Repository.Name}")
        .Distinct()
        .OrderBy(r => r);
    return string.Join(Environment.NewLine, uniqueRepos.Select(r => $"https://github.com/{r}.git"));
}
```

**Evidence**: Similar to repo version, extracts unique repos first.

---

#### FormatCodeAsFileUrls
```csharp
// Lines 1173-1181: Format code matches as GitHub file URLs
private static string FormatCodeAsFileUrls(List<CycoGr.Models.CodeMatch> matches)
{
    // Format: GitHub blob URLs (one per line, clickable)
    var urls = matches
        .Select(m => $"https://github.com/{m.Repository.FullName ?? $"{m.Repository.Owner}/{m.Repository.Name}"}/blob/main/{m.Path}")
        .Distinct()
        .OrderBy(u => u);
    return string.Join(Environment.NewLine, urls);
}
```

**Evidence**: 
- Creates clickable GitHub URLs
- Assumes `main` branch (hardcoded)
- One URL per distinct file

---

#### FormatCodeAsJson
```csharp
// Lines 1183-1216: Format code matches as JSON
private static string FormatCodeAsJson(List<CycoGr.Models.CodeMatch> matches)
{
    var items = matches.Select(m => new
    {
        repository = new
        {
            url = m.Repository.Url,
            name = m.Repository.Name,
            owner = m.Repository.Owner,
            fullName = m.Repository.FullName,
            stars = m.Repository.Stars,
            language = m.Repository.Language
        },
        path = m.Path,
        sha = m.Sha,
        url = m.Url,
        textMatches = m.TextMatches.Select(tm => new
        {
            fragment = tm.Fragment,
            property = tm.Property,
            type = tm.Type,
            matches = tm.Matches.Select(mi => new
            {
                indices = mi.Indices,
                text = mi.Text
            })
        })
    });

    return System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
}
```

**Evidence**: 
- Nested structure: repository → file → text matches → match indices
- Includes all GitHub search metadata (sha, fragments, indices)

---

#### FormatCodeAsCsv
```csharp
// Lines 1218-1230: Format code matches as CSV
private static string FormatCodeAsCsv(List<CycoGr.Models.CodeMatch> matches)
{
    var output = new StringBuilder();
    output.AppendLine("repo_url,repo_name,repo_owner,repo_stars,file_path,file_url");

    foreach (var match in matches.OrderBy(m => m.Repository.Url).ThenBy(m => m.Path))
    {
        var fileUrl = $"{match.Repository.Url}/blob/HEAD/{match.Path}";
        output.AppendLine($"\"{match.Repository.Url}\",\"{match.Repository.Name}\",\"{match.Repository.Owner}\",{match.Repository.Stars},\"{match.Path}\",\"{fileUrl}\"");
    }

    return output.ToString().TrimEnd();
}
```

**Evidence**: 
- Headers: `repo_url,repo_name,repo_owner,repo_stars,file_path,file_url`
- Sorted by repo URL, then file path
- File URLs use `HEAD` branch

---

## Template Variable Processing

**File**: Common helper (referenced but not shown in cycodgr code)

```
FileHelpers.GetFileNameFromTemplate(defaultTemplate, userTemplate)
```

**Behavior** (inferred from usage):
1. If `userTemplate` is empty, use `defaultTemplate`
2. Replace `{time}` with timestamp
3. Replace `{repo}` with sanitized repository name (for per-repo outputs)
4. Replace `{ProgramName}` with `"cycodgr"`

**Evidence**: 
- Used at lines 443, 465, 487, 511, 535, 567, 585, 619 in SaveAdditionalFormats
- Also in HandleRepoSearchAsync (line 362) and HandleCodeSearchAsync (line 429)

---

## File Writing Details

### UTF-8 Encoding Variants

**With BOM** (default `FileHelpers.WriteAllText`):
- Markdown files (`--save-output`, `--save-table`)
- JSON files (`--save-json`)
- CSV files (`--save-csv`)

**Without BOM** (explicit encoding):
```csharp
// Line 577: Write file paths without BOM for @file compatibility
File.WriteAllText(fileName, content, new System.Text.UTF8Encoding(false));
```

**Evidence**: Only `--save-file-paths` uses UTF-8 without BOM to ensure `@file` loading works correctly.

---

### CRLF Line Endings

```csharp
// Line 575: Explicit CRLF for Windows compatibility
var content = string.Join("\r\n", paths) + "\r\n";
```

**Evidence**: `--save-file-paths` uses `\r\n` to ensure `File.ReadAllLines()` works correctly on Windows when loading with `@file`.

---

## Success Reporting

### Individual File Messages

```csharp
// Lines 632-638 in SaveAdditionalFormats: Report all saved files
if (savedFiles.Any())
{
    foreach (var file in savedFiles)
    {
        ConsoleHelpers.WriteLine($"Saved to: {file}", ConsoleColor.Green);
    }
}
```

**Evidence**: 
- Multiple files reported separately
- Green color (unless `--quiet`)
- No output if no files were saved

---

### Primary Output Messages

```csharp
// Line 364 in HandleRepoSearchAsync:
ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);

// Line 431 in HandleCodeSearchAsync:
ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
```

**Evidence**: 
- Different message format: "Saved output to:" vs "Saved to:"
- `overrideQuiet: true` ensures message shows even in quiet mode

---

## Data Flow Summary

### Repository Search
```
HandleRepoSearchAsync (Program.cs:299-369)
  ├─> command.SaveOutput
  │   └─> FileHelpers.WriteAllText(saveFileName, markdown)
  │       └─> Success: "Saved output to: {file}"
  │
  └─> SaveAdditionalFormats (Program.cs:438-639)
      ├─> command.SaveJson → FormatAsJson → WriteAllText
      ├─> command.SaveCsv → FormatAsCsv → WriteAllText
      ├─> command.SaveTable → FormatAsTable → WriteAllText
      ├─> command.SaveUrls → FormatAsRepoCloneUrls → WriteAllText
      ├─> command.SaveRepos → FormatAsRepoList → WriteAllText
      ├─> command.SaveRepoUrls → FormatAsRepoCloneUrls → WriteAllText
      └─> Success: "Saved to: {file}" (for each)
```

---

### Code Search
```
HandleCodeSearchAsync (Program.cs:371-436)
  ├─> command.SaveOutput
  │   └─> FileHelpers.WriteAllText(saveFileName, markdown)
  │       └─> Success: "Saved output to: {file}"
  │
  └─> SaveAdditionalFormats (Program.cs:438-639)
      ├─> command.SaveJson → FormatCodeAsJson → WriteAllText
      ├─> command.SaveCsv → FormatCodeAsCsv → WriteAllText
      ├─> command.SaveUrls → FormatCodeAsFileUrls → WriteAllText
      ├─> command.SaveRepos → FormatCodeAsRepoList → WriteAllText
      ├─> command.SaveFilePaths → (per-repo loop) → File.WriteAllText (UTF-8 no BOM, CRLF)
      ├─> command.SaveRepoUrls → FormatCodeAsRepoCloneUrls → WriteAllText
      ├─> command.SaveFileUrls → (per-repo loop) → WriteAllText
      └─> Success: "Saved to: {file}" (for each)
```

---

## Summary

### All Command-Line Options

| Option | Parser Line(s) | Property | Execution Line(s) | Default Template |
|--------|----------------|----------|-------------------|------------------|
| `--save-output` | 409-417 | `SaveOutput` | 354-365 (repo), 420-432 (code) | `"search-output.md"` |
| `--save-json` | (TryParseSharedCycoGrCommandOptions) | `SaveJson` | 442-462 | `"output.json"` |
| `--save-csv` | (TryParseSharedCycoGrCommandOptions) | `SaveCsv` | 464-484 | `"output.csv"` |
| `--save-table` | (TryParseSharedCycoGrCommandOptions) | `SaveTable` | 486-508 | `"output.md"` |
| `--save-urls` | (TryParseSharedCycoGrCommandOptions) | `SaveUrls` | 510-532 | `"output.txt"` |
| `--save-repos` | (TryParseSharedCycoGrCommandOptions) | `SaveRepos` | 534-554 | `"repos.txt"` |
| `--save-file-paths` | (TryParseSharedCycoGrCommandOptions) | `SaveFilePaths` | 556-581 | `"files-{repo}.txt"` |
| `--save-repo-urls` | (TryParseSharedCycoGrCommandOptions) | `SaveRepoUrls` | 583-606 | `"repo-urls.txt"` |
| `--save-file-urls` | (TryParseSharedCycoGrCommandOptions) | `SaveFileUrls` | 608-630 | `"file-urls-{repo}.txt"` |

### Key Implementation Details

1. **Base class properties**: All save options declared in `CycoGrCommand` (lines 7-35)
2. **Parsing**: Mixed between search-specific and shared command parsing
3. **Execution**: Split between inline (`--save-output`) and centralized (`SaveAdditionalFormats`)
4. **Formatting**: Type-specific formatters for repos vs code matches
5. **Templates**: All support `FileHelpers.GetFileNameFromTemplate` with `{time}` and `{repo}` variables
6. **Encoding**: UTF-8 with BOM (default) or without BOM (file paths)
7. **Line endings**: CRLF for file paths, system default for others
8. **Reporting**: Green "Saved to:" messages for all successful saves

---

**[Back to Layer 7 Catalog](cycodgr-search-filtering-pipeline-catalog-layer-7.md)** | **[Back to README](cycodgr-search-filtering-pipeline-catalog-README.md)**
