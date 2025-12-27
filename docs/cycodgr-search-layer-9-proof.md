# cycodgr search - Layer 9: ACTIONS ON RESULTS - PROOF

## Command Properties

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 73-76**:
```csharp
public bool Clone { get; set; }
public int MaxClone { get; set; }
public string CloneDirectory { get; set; }
public bool AsSubmodules { get; set; }
```

**Constructor initialization (Lines 27-30)**:
```csharp
Clone = false;
MaxClone = 10;
CloneDirectory = "external";
AsSubmodules = false;
```

## Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 112-133** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--clone")
{
    command.Clone = true;
}
else if (arg == "--max-clone")
{
    var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.MaxClone = ValidatePositiveNumber(arg, countStr);
}
else if (arg == "--clone-dir")
{
    var dir = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(dir))
    {
        throw new CommandLineException($"Missing directory path for {arg}");
    }
    command.CloneDirectory = dir!;
}
else if (arg == "--as-submodules")
{
    command.AsSubmodules = true;
}
```

## Execution

**File**: `src/cycodgr/Program.cs`

**Lines 342-352** - In `HandleRepoSearchAsync`:
```csharp
// Clone if requested
if (command.Clone)
{
    var maxClone = Math.Min(command.MaxClone, repos.Count);
    ConsoleHelpers.WriteLine($"Cloning top {maxClone} repositories to '{command.CloneDirectory}'...", ConsoleColor.Cyan, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    var clonedRepos = await CycoGr.Helpers.GitHubSearchHelpers.CloneRepositoriesAsync(repos, command.AsSubmodules, command.CloneDirectory, maxClone);

    ConsoleHelpers.WriteLine(overrideQuiet: true);
    ConsoleHelpers.WriteLine($"Successfully cloned {clonedRepos.Count} of {maxClone} repositories", ConsoleColor.Green, overrideQuiet: true);
}
```

## Clone Implementation

**File**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

**Lines 766-822** - `CloneRepositoriesAsync` method:
```csharp
public static async Task<List<string>> CloneRepositoriesAsync(
    List<RepoInfo> repos,
    bool asSubmodules,
    string cloneDirectory,
    int maxClone)
{
    var clonedRepos = new List<string>();
    var actualMaxClone = Math.Min(maxClone, repos.Count);
    
    // Create clone directory if it doesn't exist
    if (!Directory.Exists(cloneDirectory))
    {
        Directory.CreateDirectory(cloneDirectory);
        Logger.Info($"Created directory: {cloneDirectory}");
    }
    
    for (int i = 0; i < actualMaxClone; i++)
    {
        var repo = repos[i];
        var repoName = repo.Name;
        var targetPath = Path.Combine(cloneDirectory, repoName);
        
        // Skip if already exists
        if (Directory.Exists(targetPath))
        {
            ConsoleHelpers.WriteLine($"Skipping {repoName} (already exists)", ConsoleColor.Yellow);
            Logger.Warning($"Repository already exists: {targetPath}");
            clonedRepos.Add(targetPath);
            continue;
        }
        
        try
        {
            ConsoleHelpers.DisplayStatus($"Cloning {repoName} ({i + 1}/{actualMaxClone})...");
            
            if (asSubmodules)
            {
                await CloneAsSubmoduleAsync(repo.Url, targetPath);
            }
            else
            {
                await CloneRepositoryAsync(repo.Url, targetPath);
            }
            
            clonedRepos.Add(targetPath);
            ConsoleHelpers.WriteLine($"Cloned: {repoName}", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Failed to clone {repoName}: {ex.Message}");
            Logger.Error($"Error cloning repository {repo.Url}: {ex.Message}");
        }
    }
    
    ConsoleHelpers.DisplayStatusErase();
    return clonedRepos;
}
```

### Regular Clone

**Lines 824-836** - `CloneRepositoryAsync` method:
```csharp
private static async Task CloneRepositoryAsync(string url, string targetPath)
{
    var gitCommand = $"git clone {url} \"{targetPath}\"";
    var result = await ProcessHelpers.RunProcessAsync(gitCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
    
    if (result.ExitCode != 0)
    {
        var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
            ? result.StandardError 
            : "Unknown error executing git clone";
        throw new Exception($"Git clone failed: {errorMsg}");
    }
}
```

### Submodule Clone

**Lines 838-850** - `CloneAsSubmoduleAsync` method:
```csharp
private static async Task CloneAsSubmoduleAsync(string url, string targetPath)
{
    var gitCommand = $"git submodule add {url} \"{targetPath}\"";
    var result = await ProcessHelpers.RunProcessAsync(gitCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
    
    if (result.ExitCode != 0)
    {
        var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
            ? result.StandardError 
            : "Unknown error executing git submodule add";
        throw new Exception($"Git submodule add failed: {errorMsg}");
    }
}
```

## Key Behaviors

### Directory Creation
**Line 777**: Creates clone directory if it doesn't exist

### Skip Existing
**Lines 789-794**: Checks if directory exists, skips if yes (yellow warning, adds to clonedRepos list)

### Progress Display
**Line 799**: Shows status during cloning with counter

### Error Handling
**Lines 797-817**: Try-catch per repository - one failure doesn't stop others

### Return Value
Returns list of cloned/skipped repository paths for further processing

---

**End of Layer 9 Proof Document**
