# Phase D Implementation Plan - FINAL DECISIONS

**Date:** 2025-12-15  
**Status:** ✅ **COMPLETE** - Implemented and tested (see phase-d-complete.md)

## Final Decisions

### 1. Flag Names: Singular vs Plural Pattern

Following existing `--repo` / `--repos` pattern:

**Singular (one value):**
- `--file-path src/Program.cs` - Single file path

**Plural (multiple values or @file):**
- `--file-paths src/Program.cs src/Startup.cs` - Multiple paths
- `--file-paths @files-microsoft-semantic-kernel.txt` - Load from file

### 2. Save Flags with Template Support

**Pattern:** Like `--save-chat-history [template]` with defaults

**Flags:**
- `--save-file-paths [template]` 
  - Default: `files-{repo}.txt`
  - Creates one file per repo with relative paths
  - Example: `files-microsoft-semantic-kernel.txt`
  
- `--save-repo-urls [template]`
  - Default: `repo-urls.txt`
  - Clone URLs: `https://github.com/owner/repo.git`
  
- `--save-file-urls [template]`
  - Default: `file-urls-{repo}.txt`
  - Blob URLs: `https://github.com/owner/repo/blob/main/src/file.cs`

**Template variables:** `{repo}`, `{time}`, etc.

### 3. File Paths Format: Repo-Relative

**Saved format** (in `files-{repo}.txt`):
```
src/Kernel.cs
src/Extensions/KernelExtensions.cs
Models/User.cs
```

**NOT qualified** (no `owner/repo:` prefix)

**Why:** 
- One file per repo via template
- Cleaner, simpler paths
- Must be used with `--repo` to provide context

### 4. Usage Pattern

**Discovery phase:**
```bash
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "Anthropic" \
        --save-repos repos.txt \
        --save-file-paths
```

**Creates:**
- `repos.txt` - List of repo names
- `files-microsoft-semantic-kernel.txt` - Relative paths for that repo
- `files-dotnet-aspnetcore.txt` - Relative paths for that repo
- etc.

**Refinement phase:**
```bash
cycodgr --repo microsoft/semantic-kernel \
        --file-paths @files-microsoft-semantic-kernel.txt \
        --line-contains "Configure" \
        --lines 30
```

### 5. Contextual --save-urls

**Hybrid approach:**
- `--save-urls` → contextual behavior
  - Searching repos → saves clone URLs
  - Searching files → saves file URLs
- `--save-repo-urls` → explicit clone URLs
- `--save-file-urls` → explicit file URLs

### 6. URL Formats

**Clone URLs** (`--save-repo-urls`):
```
https://github.com/microsoft/semantic-kernel.git
https://github.com/dotnet/aspnetcore.git
```

**File URLs** (`--save-file-urls` or contextual `--save-urls`):
```
https://github.com/microsoft/semantic-kernel/blob/main/src/Kernel.cs
https://github.com/microsoft/semantic-kernel/blob/main/src/Extensions/KernelExtensions.cs
```

**Blob URLs (not raw)** - Clickable, viewable in GitHub UI

---

## Implementation Plan

### Step 1: Add Properties to CycoGrCommand

```csharp
public string SaveRepoUrls;      // ✅ Already added
public string SaveFilePaths;     // ✅ Already added  
public string SaveFileUrls;      // NEW
public List<string> FilePaths;   // NEW - for loading
```

### Step 2: Add Command-Line Parsing

**In `CycoGrCommandLineOptions.cs`:**

```csharp
// Singular
else if (arg == "--file-path")
{
    var path = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(path))
    {
        throw new CommandLineException($"Missing file path for {arg}");
    }
    command.FilePaths.Add(path!);
}

// Plural (with @ support)
else if (arg == "--file-paths")
{
    var pathArgs = GetInputOptionArgs(i + 1, args);
    foreach (var pathArg in pathArgs)
    {
        if (pathArg.StartsWith("@"))
        {
            // Load paths from file
            var fileName = pathArg.Substring(1);
            if (!FileHelpers.FileExists(fileName))
            {
                throw new CommandLineException($"File paths file not found: {fileName}");
            }
            var pathsFromFile = FileHelpers.ReadAllLines(fileName)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());
            command.FilePaths.AddRange(pathsFromFile);
        }
        else
        {
            command.FilePaths.Add(pathArg);
        }
    }
    i += pathArgs.Count();
}

// Save flags with template support
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

### Step 3: Update SaveAdditionalFormats

**Handle per-repo file saving:**

```csharp
if (!string.IsNullOrEmpty(command.SaveFilePaths))
{
    if (data is List<CodeMatch> matches)
    {
        // Group by repo
        var repoGroups = matches.GroupBy(m => m.Repository.FullName);
        
        foreach (var repoGroup in repoGroups)
        {
            var repoName = repoGroup.Key.Replace('/', '-');
            var fileName = FileHelpers.GetFileNameFromTemplate(
                "files-{repo}.txt", 
                command.SaveFilePaths,
                new Dictionary<string, string> { ["repo"] = repoName });
            
            var paths = repoGroup
                .Select(m => m.Path)
                .Distinct()
                .OrderBy(p => p);
            
            FileHelpers.WriteAllText(fileName, string.Join(Environment.NewLine, paths));
            savedFiles.Add(fileName);
        }
    }
}
```

**Similar for `--save-file-urls` and `--save-repo-urls`**

### Step 4: Implement URL Formatters

```csharp
private static string FormatAsRepoCloneUrls(List<RepoInfo> repos)
{
    return string.Join(Environment.NewLine, 
        repos.Select(r => $"https://github.com/{r.FullName}.git"));
}

private static string FormatCodeAsFileUrls(List<CodeMatch> matches)
{
    var urls = matches
        .Select(m => $"https://github.com/{m.Repository.FullName}/blob/main/{m.Path}")
        .Distinct()
        .OrderBy(u => u);
    return string.Join(Environment.NewLine, urls);
}
```

### Step 5: Integrate FilePaths Filtering

**In search handlers, filter results:**

```csharp
if (command.FilePaths.Any())
{
    // Post-filter code matches to only include specified paths
    codeMatches = codeMatches
        .Where(m => command.FilePaths.Any(fp => m.Path == fp || m.Path.Contains(fp)))
        .ToList();
}
```

### Step 6: Make --save-urls Contextual

```csharp
if (!string.IsNullOrEmpty(command.SaveUrls))
{
    if (data is List<RepoInfo> repos)
    {
        // Contextual: save clone URLs for repo search
        content = FormatAsRepoCloneUrls(repos);
    }
    else if (data is List<CodeMatch> matches)
    {
        // Contextual: save file URLs for code search (existing behavior)
        content = FormatCodeAsUrls(matches);
    }
}
```

---

## Testing Plan

### Test 1: Save file paths with default template
```bash
cycodgr --repo-csproj-file-contains "Newtonsoft.Json" \
        --save-file-paths \
        --max-results 2
```

**Expected:** Creates `files-{repo}.txt` for each repo found

### Test 2: Save with custom template
```bash
cycodgr --repo microsoft/semantic-kernel \
        --cs-file-contains "Kernel" \
        --save-file-paths "my-files-{time}.txt" \
        --max-results 3
```

**Expected:** Creates `my-files-{timestamp}.txt`

### Test 3: Load and filter by file paths
```bash
cycodgr --repo microsoft/semantic-kernel \
        --file-paths @files-microsoft-semantic-kernel.txt \
        --line-contains "async" \
        --max-results 5
```

**Expected:** Only searches files listed in the file

### Test 4: Save repo URLs
```bash
cycodgr --repo-contains "semantic" \
        --save-repo-urls \
        --max-results 3
```

**Expected:** Creates `repo-urls.txt` with clone URLs

### Test 5: Contextual --save-urls
```bash
# Repo search
cycodgr --repo-contains "semantic" --save-urls --max-results 2

# File search
cycodgr --repo microsoft/semantic-kernel --file-contains "Kernel" --save-urls --max-results 2
```

**Expected:** 
- First creates clone URLs
- Second creates file URLs

---

## Success Criteria

✅ Can save file paths per repo with template  
✅ Can load file paths from file  
✅ Can save repo clone URLs  
✅ Can save file blob URLs  
✅ Contextual --save-urls works for both repo and file searches  
✅ Template expansion works ({repo}, {time}, etc.)  
✅ Singular/plural pattern consistent with --repo/--repos  

---

## Ready to Implement! 🚀

## Understanding Current @ Loading Mechanism

### How @ works currently:

**`@filename` expands to file contents:**
```csharp
// AtFileHelpers.cs line 10-14
public static string ExpandAtFileValue(string atFileValue)
{
    if (atFileValue.StartsWith("@") && FileHelpers.FileExists(atFileValue[1..]))
    {
        return FileHelpers.ReadAllText(atFileValue[1..]);  // Entire file as ONE string
    }
}
```

**`@@filename` expands to file lines:**
```csharp
// CommandLineOptions.cs line 113-121
protected IEnumerable<string> ExpandedInput(string input)
{
    if (input.StartsWith("@@"))
    {
        var fileName = input.Substring(2);
        return File.ReadLines(fileName);  // Each line becomes separate arg
    }
}
```

**For `--repos`:**
```csharp
// CycoGrCommandLineOptions.cs line 375-386
if (repoArg.StartsWith("@"))
{
    var fileName = repoArg.Substring(1);
    var reposFromFile = FileHelpers.ReadAllLines(fileName)  // Read lines
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line.Trim());
    command.Repos.AddRange(reposFromFile);  // Add each line as separate repo
}
```

### Key Insight:
- **`@`** = treat entire file as ONE argument
- **`@@`** = treat each line as SEPARATE argument (via ExpandedInputsFromCommandLine)
- **`--repos @file`** = special handling, reads lines manually (custom behavior)

---

## User's Questions/Requirements

### 1. Naming: `--contains-file-paths` vs `--file-paths`

**User suggested**: `--contains-file-paths @FILE` (like `--file-contains`)

**My thoughts:**
- Clearer that it's a "contains" filter (search within these paths)
- Parallel to `--repo-contains`, `--file-contains`, `--line-contains`

### 2. Save URLs Contextual Behavior

**User said**:
> `--save-urls` should be context-aware:
> - When searching repos → save repo URLs
> - When searching files → save file URLs
> OR have explicit flags:
> - `--save-repo-urls` for clone URLs (explicit)
> - `--save-file-urls` for file URLs (explicit)
> - `--save-urls` for contextual (whatever you're searching)

### 3. Paths Format

**User said**:
> "paths... interesting ... maybe paths are repo specific, but in -repos.txt extension added ... idk..."

They're uncertain about format. Need to propose options.

### 4. @ Loading

**User said**:
> "the @thing probably already works... and if it doesn't... then, you can 'make it work'"

@ already works for repos, should work for file paths too.

---

## Options and Recommendations

### Option A: Contextual --save-urls (RECOMMENDED)

**Behavior:**
- `--save-urls` saves different things based on what you're searching
  - Searching repos (`--repo-contains`) → saves repo clone URLs
  - Searching files (`--file-contains`) → saves file GitHub URLs
- **Pros:**
  - One flag, intuitive behavior
  - Less to remember
  - Existing flag, backward compatible
- **Cons:**
  - Could be confusing (what does it save?)
  - Less explicit

**Implementation:**
```csharp
if (!string.IsNullOrEmpty(command.SaveUrls))
{
    if (data is List<RepoInfo> repos)
    {
        // Save clone URLs
        content = FormatAsRepoCloneUrls(repos);
    }
    else if (data is List<CodeMatch> matches)
    {
        // Save file URLs (existing behavior)
        content = FormatCodeAsUrls(matches);
    }
}
```

---

### Option B: Explicit Flags (MORE CONTROL)

**Flags:**
- `--save-repo-urls` → always saves clone URLs (https://github.com/owner/repo.git)
- `--save-file-urls` → always saves file URLs (https://github.com/owner/repo/blob/main/file.cs)
- `--save-urls` → deprecated or contextual

**Pros:**
- Explicit, no ambiguity
- Can save both simultaneously
- Clear intent

**Cons:**
- More flags to remember
- Longer commands

---

### Option C: Hybrid (BEST OF BOTH)

**Flags:**
- `--save-urls` → contextual (existing behavior)
- `--save-repo-urls` → explicit clone URLs
- `--save-file-urls` → explicit file URLs

**Behavior:**
- If explicit flags used, those take precedence
- If only `--save-urls`, use contextual behavior
- Can use multiple flags to save multiple formats

**Pros:**
- Flexibility
- Backward compatible
- Power users get control, casual users get simplicity

**Cons:**
- Slight complexity in documentation

**RECOMMENDED**: Option C (Hybrid)

---

## File Paths Format Options

### Format A: Qualified Paths (owner/repo:path/to/file.cs)

**Example:**
```
microsoft/semantic-kernel:src/Kernel.cs
dotnet/aspnetcore:src/Http/HttpClient.cs
```

**Pros:**
- Self-contained (includes repo)
- Can search across multiple repos
- Unambiguous

**Cons:**
- Slightly verbose
- Parsing required (split on `:`)

**RECOMMENDED**: Format A

---

### Format B: Just Paths (src/Program.cs)

**Example:**
```
src/Kernel.cs
src/Http/HttpClient.cs
```

**Pros:**
- Concise
- Simple

**Cons:**
- Requires context (which repo?)
- Can't mix repos
- Ambiguous

**NOT RECOMMENDED**

---

### Format C: Full URLs

**Example:**
```
https://github.com/microsoft/semantic-kernel/blob/main/src/Kernel.cs
```

**Pros:**
- Directly usable (clickable)
- Unambiguous

**Cons:**
- Very verbose
- Parsing complex
- Not as "flat text" friendly

**NOT RECOMMENDED for --save-file-paths** (but use this for --save-file-urls)

---

## Loading File Paths - What Should It DO?

### Option A: Use as Search Repo Filter + File Filter

**Behavior:**
1. Parse `owner/repo:path` from file
2. Extract unique repos → use as repo filter
3. Do GitHub search within those repos
4. Post-filter results to only show specified file paths

**Pros:**
- Uses GitHub search API
- Consistent with existing search flow
- Fast (GitHub does the work)

**Cons:**
- Might return files NOT in the list (if other files match)
- Need post-filter

**RECOMMENDED**: Option A

---

### Option B: Fetch Specific Files Directly

**Behavior:**
1. Parse `owner/repo:path` from file
2. Fetch each file via raw GitHub URL
3. Search content locally
4. Return matches

**Pros:**
- Precise (only specified files)
- No extraneous results

**Cons:**
- Many API calls (one per file)
- Rate limiting issues
- Slow for many files
- Different from normal search flow

**NOT RECOMMENDED**

---

## Summary: My Recommendations

### Flags to Add:

**For saving:**
- `--save-repo-urls filename` → Clone URLs (https://github.com/owner/repo.git)
- `--save-file-urls filename` → File URLs (https://github.com/owner/repo/blob/main/file.cs)
- `--save-file-paths filename` → Qualified paths (owner/repo:src/file.cs)
- `--save-urls filename` → Contextual (repo URLs or file URLs based on search type)

**For loading:**
- `--file-paths @filename` → Load qualified paths, use as repo+file filter

### Formats:

**File paths format:** `owner/repo:src/file.cs`
- Clean, parseable, self-contained

**Repo clone URLs format:** `https://github.com/owner/repo.git`
- Standard git clone format

**File URLs format:** `https://github.com/owner/repo/blob/main/src/file.cs`
- Standard GitHub file URL

### Behavior:

**`--file-paths @paths.txt`:**
1. Load file with qualified paths (one per line)
2. Parse to extract repos and file paths
3. Use repos as pre-filter (like `--repos`)
4. Use file paths as post-filter (only show these files in results)

**`--save-urls` contextual:**
- If searching repos → save clone URLs
- If searching files → save file URLs

---

## Implementation Plan

### Step 1: Add Properties to CycoGrCommand
```csharp
public string SaveRepoUrls;  // ✅ Already done
public string SaveFilePaths;  // ✅ Already done
public string SaveFileUrls;   // NEW
public List<string> FilePaths; // NEW - for loading
```

### Step 2: Add Command-Line Parsing
```csharp
// --save-repo-urls
// --save-file-paths
// --save-file-urls
// --file-paths (with @ loading support)
```

### Step 3: Implement Save Functions
```csharp
FormatAsRepoCloneUrls(repos) → "https://github.com/owner/repo.git"
FormatAsFilePaths(matches) → "owner/repo:src/file.cs"
FormatCodeAsFileUrls(matches) → "https://github.com/owner/repo/blob/main/src/file.cs"
```

### Step 4: Implement Load Function
```csharp
// Parse --file-paths @file.txt
// Extract repos → add to command.Repos
// Extract paths → add to command.FilePaths
// Post-filter results to match file paths
```

### Step 5: Make --save-urls Contextual
```csharp
if (data is RepoInfo) → save clone URLs
if (data is CodeMatch) → save file URLs
```

---

## Questions for User:

1. **Naming**: `--file-paths` or `--contains-file-paths`?
   - I lean toward `--file-paths` (shorter, parallel to `--repos`)

2. **Contextual --save-urls**: OK with hybrid approach (contextual + explicit flags)?

3. **File paths format**: OK with `owner/repo:src/file.cs`?

4. **File paths behavior**: OK with repo filter + post-filter approach?

5. **File URLs**: Should `--save-file-urls` save raw URLs or blob URLs?
   - Raw: `https://raw.githubusercontent.com/owner/repo/main/file.cs` (actual file content)
   - Blob: `https://github.com/owner/repo/blob/main/file.cs` (GitHub UI)
   - I recommend blob URLs (more useful for humans, clickable)

---

## Ready to Implement Once Confirmed!

Let me know your preferences and I'll proceed! 🚀
