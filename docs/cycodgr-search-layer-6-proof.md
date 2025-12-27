# cycodgr search - Layer 6: DISPLAY CONTROL - PROOF

## Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 88**:
```csharp
public string Format { get; set; }
```

**Constructor initialization (Line 24)**:
```csharp
Format = "detailed";
```

## Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 217-223** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--format")
{
    var format = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(format))
    {
        throw new CommandLineException($"Missing format for {arg}");
    }
    command.Format = format!;
}
```

## Format Application - Repository Search

**File**: `src/cycodgr/Program.cs`

**Line 337** - In `HandleRepoSearchAsync`:
```csharp
var output = FormatRepoOutput(repos, command.Format);
```

**Lines 1232-1242** - `FormatRepoOutput` method:
```csharp
private static string FormatRepoOutput(List<CycoGr.Models.RepoInfo> repos, string format)
{
    return format switch
    {
        "table" => FormatAsTable(repos),
        "json" => FormatAsJson(repos),
        "csv" => FormatAsCsv(repos),
        "detailed" => FormatAsDetailed(repos),
        _ => FormatAsUrls(repos) // Default: url format
    };
}
```

## Built-In Display Features

### Line Numbers

**File**: `src/cycodgr/Program.cs`

**Line 812** - In `ProcessFileGroupAsync`:
```csharp
var filteredContent = LineHelpers.FilterAndExpandContext(
    foundFile.Content,
    includePatterns,
    contextLines,
    contextLines,
    true,          // ← include line numbers (always enabled)
    excludePatterns,
    backticks,
    true
);
```

### Syntax Highlighting

**Lines 804-805**:
```csharp
var lang = DetectLanguageFromPath(firstMatch.Path);
var backticks = $"```{lang}";
```

**Lines 900-932** - Language detection:
```csharp
private static string DetectLanguageFromPath(string path)
{
    var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
    return ext switch
    {
        ".cs" => "csharp",
        ".js" => "javascript",
        // ... (full list in Layer 3 proof)
        _ => ""
    };
}
```

### Match Highlighting

**Line 815**:
```csharp
true           // ← highlight matches (always enabled)
```

## Format Implementation Examples

### Detailed Format

**Lines 1249-1285** - `FormatAsDetailed` method:
```csharp
private static string FormatAsDetailed(List<CycoGr.Models.RepoInfo> repos)
{
    var output = new StringBuilder();
    
    foreach (var repo in repos)
    {
        // Format: ## owner/repo (⭐ stars) (language)
        var header = $"## {repo.FullName} (⭐ {repo.FormattedStars})";
        if (!string.IsNullOrEmpty(repo.Language))
        {
            header += $" ({repo.Language})";
        }
        output.AppendLine(header);
        output.AppendLine();

        // Repo URL and Description
        output.AppendLine($"Repo: {repo.Url}");
        if (!string.IsNullOrEmpty(repo.Description))
        {
            output.AppendLine($"Desc: {repo.Description}");
        }
        output.AppendLine();

        // Topics and Updated
        if (repo.Topics?.Any() == true)
        {
            output.AppendLine($"Topics: {string.Join(", ", repo.Topics)}");
        }
        if (repo.UpdatedAt.HasValue)
        {
            output.AppendLine($"Updated: {repo.UpdatedAt.Value:yyyy-MM-dd}");
        }
        output.AppendLine();
    }
    
    return output.ToString().TrimEnd();
}
```

### JSON Format

**Lines 1306-1326** - `FormatAsJson` method:
```csharp
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

### Table Format

**Lines 1287-1304** - `FormatAsTable` method:
```csharp
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

---

**End of Layer 6 Proof Document**
