# cycodgr search - Layer 7: OUTPUT PERSISTENCE - PROOF

## Command Properties

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 7-15, 25-33**:
```csharp
SaveOutput = string.Empty;
SaveJson = string.Empty;
SaveCsv = string.Empty;
SaveTable = string.Empty;
SaveUrls = string.Empty;
SaveRepos = string.Empty;
SaveFilePaths = string.Empty;
SaveRepoUrls = string.Empty;
SaveFileUrls = string.Empty;

// Properties:
public string SaveOutput;
public string SaveJson;
public string SaveCsv;
public string SaveTable;
public string SaveUrls;
public string SaveRepos;
public string SaveFilePaths;
public string SaveRepoUrls;
public string SaveFileUrls;
```

## Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 223-281** - In `TryParseSharedCycoGrCommandOptions`:
```csharp
else if (arg == "--save-output")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var saveOutput = max1Arg.FirstOrDefault() ?? "search-output.md";
    command.SaveOutput = saveOutput;
    i += max1Arg.Count();
}
else if (arg == "--save-json")
{
    var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(file))
    {
        throw new CommandLineException($"Missing file path for {arg}");
    }
    command.SaveJson = file!;
}
// ... (similar for --save-csv, --save-table, --save-urls, --save-repos)
else if (arg == "--save-file-paths")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var template = max1Arg.FirstOrDefault() ?? "files-{repo}.txt";
    command.SaveFilePaths = template;
    i += max1Arg.Count();
}
else if (arg == "--save-repo-urls")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var template = max1Arg.FirstOrDefault() ?? "repo-urls.txt";
    command.SaveRepoUrls = template;
    i += max1Arg.Count();
}
else if (arg == "--save-file-urls")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var template = max1Arg.FirstOrDefault() ?? "file-urls-{repo}.txt";
    command.SaveFileUrls = template;
    i += max1Arg.Count();
}
```

## Save Implementation

**File**: `src/cycodgr/Program.cs`

**Lines 438-639** - `SaveAdditionalFormats` method:

### --save-json

**Lines 442-462**:
```csharp
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
    else
    {
        return;
    }
    
    FileHelpers.WriteAllText(fileName, jsonContent);
    savedFiles.Add(fileName);
}
```

### --save-file-paths

**Lines 556-581**:
```csharp
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

**Note**: Special handling for:
- Template variable replacement (`{repo}`)
- Multiple files (one per repo)
- Line ending normalization (`\r\n`)
- UTF-8 without BOM encoding

---

**End of Layer 7 Proof Document**
