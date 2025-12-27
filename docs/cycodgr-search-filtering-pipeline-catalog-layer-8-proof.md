# cycodgr Search Command - Layer 8: AI Processing - PROOF

## Overview

This document provides detailed source code evidence for Layer 8 (AI Processing) implementation in cycodgr's search command, with exact line numbers and code references.

---

## Command Line Option Parsing

### Location: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

### Option 1: `--file-instructions`

**Lines 282-290:**
```csharp
else if (arg == "--file-instructions")
{
    var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(instructions))
    {
        throw new CommandLineException($"Missing instructions for {arg}");
    }
    command.FileInstructionsList.Add(new Tuple<string, string>(instructions!, ""));
}
```

**Evidence:**
- Parses `--file-instructions` option
- Requires one argument (the instruction text)
- Adds to `FileInstructionsList` with empty criteria (`""` = matches all files)
- Throws exception if instructions missing

---

### Option 2: `--{ext}-file-instructions`

**Lines 291-301:**
```csharp
else if (arg.StartsWith("--") && arg.EndsWith("-file-instructions"))
{
    // Extract extension: --cs-file-instructions → cs, --md-file-instructions → md
    var ext = arg.Substring(2, arg.Length - 2 - "-file-instructions".Length);
    var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(instructions))
    {
        throw new CommandLineException($"Missing instructions for {arg}");
    }
    command.FileInstructionsList.Add(new Tuple<string, string>(instructions!, ext));
}
```

**Evidence:**
- Pattern matching: `--{anything}-file-instructions`
- Extracts extension from option name (e.g., `--cs-file-instructions` → `"cs"`)
- Adds to `FileInstructionsList` with extension as criteria
- Supports ANY extension dynamically (`--xyz-file-instructions` → `"xyz"`)

**Example transformations:**
- `--cs-file-instructions` → ext = `"cs"`
- `--md-file-instructions` → ext = `"md"`
- `--Program.cs-file-instructions` → ext = `"Program.cs"`

---

### Option 3: `--repo-instructions`

**Lines 302-310:**
```csharp
else if (arg == "--repo-instructions")
{
    var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(instructions))
    {
        throw new CommandLineException($"Missing instructions for {arg}");
    }
    command.RepoInstructionsList.Add(instructions!);
}
```

**Evidence:**
- Parses `--repo-instructions` option
- Adds directly to `RepoInstructionsList` (no criteria needed)
- Can be specified multiple times (list accumulates)

---

### Option 4: `--instructions`

**Lines 311-319:**
```csharp
else if (arg == "--instructions")
{
    var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(instructions))
    {
        throw new CommandLineException($"Missing instructions for {arg}");
    }
    command.InstructionsList.Add(instructions!);
}
```

**Evidence:**
- Parses `--instructions` option (global level)
- Adds to `InstructionsList`
- Can be specified multiple times

---

## Command Class Properties

### Location: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

### Property Declarations

**Lines 82-85:**
```csharp
// AI instruction options
public List<Tuple<string, string>> FileInstructionsList { get; set; }
public List<string> RepoInstructionsList { get; set; }
public List<string> InstructionsList { get; set; }
```

**Evidence:**
- `FileInstructionsList`: Tuple<instruction, criteria>
- `RepoInstructionsList`: Simple string list
- `InstructionsList`: Simple string list

### Property Initialization

**Lines 31-33:**
```csharp
FileInstructionsList = new List<Tuple<string, string>>();
RepoInstructionsList = new List<string>();
InstructionsList = new List<string>();
```

**Evidence:**
- All initialized to empty lists in constructor
- Ready to accumulate multiple instructions

---

## Execution Flow

### Location: `src/cycodgr/Program.cs`

### Entry Point: FormatAndOutputCodeResults

**Lines 641-739:**
```csharp
private static async Task FormatAndOutputCodeResults(
    List<CycoGr.Models.CodeMatch> codeMatches, 
    int contextLines, 
    string query, 
    string format, 
    List<Tuple<string, string>> fileInstructionsList,  // ← File instructions passed in
    List<string> repoInstructionsList,                 // ← Repo instructions passed in
    List<string> instructionsList,                      // ← Global instructions passed in
    CycoGr.CommandLineCommands.SearchCommand command, 
    bool overrideQuiet = false)
{
    // Group by repository
    var byRepo = codeMatches.GroupBy(m => m.Repository.FullName).ToList();

    var allRepoOutputs = new List<string>();

    foreach (var repoGroup in byRepo)
    {
        // ... repo processing ...
        
        // [STAGE 1: File-Level Processing]
        var fileOutputs = await throttledProcessor.ProcessAsync(
            fileGroups,
            async fileGroup => await ProcessFileGroupAsync(
                fileGroup, repo, query, contextLines, 
                fileInstructionsList,  // ← Passed to file processor
                command, overrideQuiet)
        );
        
        // [STAGE 2: Repo-Level Processing]
        if (repoInstructionsList.Any())
        {
            Logger.Info($"Applying {repoInstructionsList.Count} repo instruction(s) to repository: {repo.FullName}");
            repoOutput = AiInstructionProcessor.ApplyAllInstructions(
                repoInstructionsList,
                repoOutput,
                useBuiltInFunctions: false,
                saveChatHistory: string.Empty);
            Logger.Info($"Repo instructions applied successfully to repository: {repo.FullName}");
        }
        
        allRepoOutputs.Add(repoOutput);
    }

    // [STAGE 3: Global Processing]
    var combinedOutput = string.Join("\n", allRepoOutputs);
    
    if (instructionsList.Any())
    {
        Logger.Info($"Applying {instructionsList.Count} final instruction(s) to all combined output");
        combinedOutput = AiInstructionProcessor.ApplyAllInstructions(
            instructionsList,
            combinedOutput,
            useBuiltInFunctions: false,
            saveChatHistory: string.Empty);
        Logger.Info($"Final instructions applied successfully");
    }

    ConsoleHelpers.WriteLine(combinedOutput, overrideQuiet: overrideQuiet);
}
```

**Evidence:**
- Three distinct processing stages clearly separated
- File processing: Lines 693-697
- Repo processing: Lines 707-716
- Global processing: Lines 726-735
- Uses `AiInstructionProcessor.ApplyAllInstructions` for actual AI calls

---

### Stage 1: File-Level Processing

**Function: ProcessFileGroupAsync, Lines 741-876:**

```csharp
private static async Task<string> ProcessFileGroupAsync(
    IGrouping<string, CycoGr.Models.CodeMatch> fileGroup, 
    CycoGr.Models.RepoInfo repo,
    string query, 
    int contextLines,
    List<Tuple<string, string>> fileInstructionsList,  // ← File instructions
    CycoGr.CommandLineCommands.SearchCommand command,
    bool overrideQuiet)
{
    var firstMatch = fileGroup.First();
    var output = new System.Text.StringBuilder();
    
    // File header
    output.AppendLine($"## {firstMatch.Path}");
    output.AppendLine();

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
            // ... metadata ...
        };

        foundFile.Content = await foundFile.LoadContent();

        // Use LineHelpers to filter and display with real line numbers
        // [... line filtering logic ...]
        
        var filteredContent = LineHelpers.FilterAndExpandContext(
            foundFile.Content,
            includePatterns,
            contextLines,
            contextLines,
            true,
            excludePatterns,
            backticks,
            true
        );

        if (filteredContent != null)
        {
            output.AppendLine(backticks);
            output.AppendLine(filteredContent);
            output.AppendLine("```");
        }
    }
    catch (Exception ex)
    {
        output.AppendLine($"Error fetching file content: {ex.Message}");
        // ... fallback logic ...
    }

    output.AppendLine();
    output.AppendLine($"Raw: {rawUrl}");
    output.AppendLine();

    // [AI PROCESSING STARTS HERE]
    var formattedOutput = output.ToString();
    var instructionsForThisFile = fileInstructionsList
        .Where(x => FileNameMatchesInstructionsCriteria(firstMatch.Path, x.Item2))
        .Select(x => x.Item1)
        .ToList();

    if (instructionsForThisFile.Any())
    {
        Logger.Info($"Applying {instructionsForThisFile.Count} instruction(s) to file: {firstMatch.Path}");
        formattedOutput = AiInstructionProcessor.ApplyAllInstructions(
            instructionsForThisFile, 
            formattedOutput, 
            useBuiltInFunctions: false, 
            saveChatHistory: string.Empty);
        Logger.Info($"Instructions applied successfully to file: {firstMatch.Path}");
    }

    return formattedOutput;
}
```

**Evidence:**
- Lines 857-875: File instruction application
- Line 859: Filter instructions matching this file
- Line 866: Apply matching instructions via `AiInstructionProcessor`
- Lines 865, 872: Logging before/after AI processing
- Returns processed output (not original if AI applied)

---

### File Matching Logic

**Function: FileNameMatchesInstructionsCriteria, Lines 878-883:**

```csharp
private static bool FileNameMatchesInstructionsCriteria(string fileName, string fileNameCriteria)
{
    return string.IsNullOrEmpty(fileNameCriteria) ||           // Matches all if no criteria
           fileName.EndsWith($".{fileNameCriteria}") ||        // Extension match
           fileName == fileNameCriteria;                        // Exact filename match
}
```

**Evidence:**
- Line 880: Empty criteria = matches ALL files (`--file-instructions`)
- Line 881: Extension match (e.g., `"cs"` matches `"Program.cs"`)
- Line 882: Exact match (e.g., `"Program.cs"` matches only `"Program.cs"`)

**Test Cases:**
| Criteria | File Name | Match? | Reason |
|----------|-----------|--------|--------|
| `""` | `Program.cs` | ✅ Yes | Empty criteria matches all |
| `"cs"` | `Program.cs` | ✅ Yes | Ends with `.cs` |
| `"cs"` | `README.md` | ❌ No | Doesn't end with `.cs` |
| `"Program.cs"` | `Program.cs` | ✅ Yes | Exact match |
| `"Program.cs"` | `src/Program.cs` | ✅ Yes | Ends with `.Program.cs` |

---

## Parallel Processing

### Location: `src/cycodgr/Program.cs`, Lines 692-697

```csharp
// Process files in parallel using ThrottledProcessor
var throttledProcessor = new ThrottledProcessor(Environment.ProcessorCount);
var fileOutputs = await throttledProcessor.ProcessAsync(
    fileGroups,
    async fileGroup => await ProcessFileGroupAsync(
        fileGroup, repo, query, contextLines, fileInstructionsList, command, overrideQuiet)
);
```

**Evidence:**
- Line 693: Creates throttled processor with CPU core count threads
- Line 694: `ProcessAsync` processes multiple files concurrently
- Each file's AI processing happens in parallel
- Maximum concurrency = `Environment.ProcessorCount`

---

## AI Processor Integration

### AiInstructionProcessor Calls

#### File-Level Call (Line 867-871)

```csharp
formattedOutput = AiInstructionProcessor.ApplyAllInstructions(
    instructionsForThisFile,     // List<string>: Filtered instructions
    formattedOutput,              // string: File content with code
    useBuiltInFunctions: false,   // bool: Not exposed in cycodgr
    saveChatHistory: string.Empty // string: Not exposed in cycodgr
);
```

#### Repo-Level Call (Lines 710-714)

```csharp
repoOutput = AiInstructionProcessor.ApplyAllInstructions(
    repoInstructionsList,         // List<string>: All repo instructions
    repoOutput,                   // string: Complete repo output
    useBuiltInFunctions: false,   // bool: Not exposed
    saveChatHistory: string.Empty // string: Not exposed
);
```

#### Global-Level Call (Lines 729-733)

```csharp
combinedOutput = AiInstructionProcessor.ApplyAllInstructions(
    instructionsList,             // List<string>: All global instructions
    combinedOutput,               // string: All repos combined
    useBuiltInFunctions: false,   // bool: Not exposed
    saveChatHistory: string.Empty // string: Not exposed
);
```

**Evidence:**
- Same function signature for all three levels
- `useBuiltInFunctions` always `false` (not in CLI options)
- `saveChatHistory` always empty (not in CLI options)
- Only `instructions` and `content` vary

---

## Invocation in Search Commands

### Code Search Mode

**Location: `src/cycodgr/Program.cs`, Line 418:**

```csharp
await FormatAndOutputCodeResults(
    codeMatches, 
    command.LinesBeforeAndAfter, 
    query, 
    command.Format, 
    command.FileInstructionsList,   // ← File instructions passed
    command.RepoInstructionsList,   // ← Repo instructions passed
    command.InstructionsList,        // ← Global instructions passed
    command, 
    overrideQuiet: true);
```

**Evidence:**
- Passes all three instruction lists to formatter
- Called from `HandleCodeSearchAsync`
- All AI processing happens in `FormatAndOutputCodeResults`

### Unified Search Mode

**Location: `src/cycodgr/Program.cs`, Line 286:**

```csharp
await FormatAndOutputCodeResults(
    codeMatches, 
    command.LinesBeforeAndAfter, 
    query, 
    command.Format, 
    command.FileInstructionsList,   // ← File instructions
    command.RepoInstructionsList,   // ← Repo instructions
    command.InstructionsList,        // ← Global instructions
    command, 
    overrideQuiet: true);
```

**Evidence:**
- Same call pattern as code search
- Called from `HandleUnifiedSearchAsync`
- Processes code results portion of unified search

---

## Data Flow Diagram

```
Command Line
     │
     ├─ --file-instructions "X"
     │   └─> FileInstructionsList.Add(("X", ""))
     │
     ├─ --cs-file-instructions "Y"
     │   └─> FileInstructionsList.Add(("Y", "cs"))
     │
     ├─ --repo-instructions "Z"
     │   └─> RepoInstructionsList.Add("Z")
     │
     └─ --instructions "W"
         └─> InstructionsList.Add("W")
              │
              ↓
      Search Execution
              │
              ↓
     [Code Search Results]
              │
              ↓
  FormatAndOutputCodeResults
              │
              ├─> Group by Repository
              │
              ├─> For Each Repo:
              │   │
              │   ├─> [STAGE 1: File Processing]
              │   │   │
              │   │   └─> ProcessFileGroupAsync (PARALLEL)
              │   │       │
              │   │       ├─> Fetch file content
              │   │       ├─> Filter lines
              │   │       ├─> Format as markdown
              │   │       ├─> Match FileInstructionsList
              │   │       └─> AiInstructionProcessor.ApplyAllInstructions
              │   │           └─> Returns processed file output
              │   │
              │   ├─> Combine all file outputs
              │   │
              │   ├─> [STAGE 2: Repo Processing]
              │   │   │
              │   │   └─> if (RepoInstructionsList.Any())
              │   │       └─> AiInstructionProcessor.ApplyAllInstructions
              │   │           └─> Returns processed repo output
              │   │
              │   └─> Add to allRepoOutputs
              │
              ├─> Combine all repo outputs
              │
              ├─> [STAGE 3: Global Processing]
              │   │
              │   └─> if (InstructionsList.Any())
              │       └─> AiInstructionProcessor.ApplyAllInstructions
              │           └─> Returns final processed output
              │
              └─> Output to console
```

---

## Call Stack Examples

### Example 1: File Instruction Processing

```
1. Main()
   └─> Line 64: HandleSearchCommandAsync(searchCommand)
       └─> Line 156: HandleCodeSearchAsync(command)
           └─> Line 418: FormatAndOutputCodeResults(...)
               └─> Line 694-697: throttledProcessor.ProcessAsync(...)
                   └─> Line 695: ProcessFileGroupAsync(fileGroup, ...)
                       └─> Line 859: fileInstructionsList.Where(...)
                           └─> Line 866-871: AiInstructionProcessor.ApplyAllInstructions(...)
                               └─> [AI model processes file]
```

### Example 2: Repo Instruction Processing

```
1. Main()
   └─> Line 64: HandleSearchCommandAsync(searchCommand)
       └─> Line 156: HandleCodeSearchAsync(command)
           └─> Line 418: FormatAndOutputCodeResults(...)
               └─> Lines 648-720: foreach (var repoGroup in byRepo)
                   └─> Lines 707-716: if (repoInstructionsList.Any())
                       └─> Lines 710-714: AiInstructionProcessor.ApplyAllInstructions(...)
                           └─> [AI model processes repo]
```

### Example 3: Global Instruction Processing

```
1. Main()
   └─> Line 64: HandleSearchCommandAsync(searchCommand)
       └─> Line 156: HandleCodeSearchAsync(command)
           └─> Line 418: FormatAndOutputCodeResults(...)
               └─> Line 723: var combinedOutput = string.Join("\n", allRepoOutputs);
                   └─> Lines 726-735: if (instructionsList.Any())
                       └─> Lines 729-733: AiInstructionProcessor.ApplyAllInstructions(...)
                           └─> [AI model processes all output]
```

---

## Error Handling

### File Processing Errors

**Location: Lines 829-851:**

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

**Evidence:**
- File fetch errors caught and logged
- Falls back to GitHub's text match fragments
- Processing continues with next file
- AI instructions NOT applied to fallback content (applied after try/catch)

---

## Logging

### Log Locations

#### File-Level Logging

**Line 865:**
```csharp
Logger.Info($"Applying {instructionsForThisFile.Count} instruction(s) to file: {firstMatch.Path}");
```

**Line 872:**
```csharp
Logger.Info($"Instructions applied successfully to file: {firstMatch.Path}");
```

#### Repo-Level Logging

**Line 709:**
```csharp
Logger.Info($"Applying {repoInstructionsList.Count} repo instruction(s) to repository: {repo.FullName}");
```

**Line 715:**
```csharp
Logger.Info($"Repo instructions applied successfully to repository: {repo.FullName}");
```

#### Global-Level Logging

**Line 728:**
```csharp
Logger.Info($"Applying {instructionsList.Count} final instruction(s) to all combined output");
```

**Line 734:**
```csharp
Logger.Info($"Final instructions applied successfully");
```

**Evidence:**
- Logs before AND after each AI processing stage
- Includes context (file name, repo name, level)
- Includes instruction count
- All use `Logger.Info` (not console output)

---

## Missing Features (Not Implemented)

### 1. `--built-in-functions`

**Parser**: No parsing code exists for this option in CycoGrCommandLineOptions.cs
**Property**: No property on SearchCommand
**Usage**: Always passed as `false` to AiInstructionProcessor

### 2. `--save-chat-history`

**Parser**: No parsing code exists for this option
**Property**: No property on CycoGrCommand or SearchCommand  
**Usage**: Always passed as `string.Empty` to AiInstructionProcessor

### 3. Model Selection Options

No command-line options for:
- `--use-anthropic`
- `--use-openai`
- `--use-azure`
- etc.

These exist in cycod but not in cycodgr.

---

## Performance Characteristics

### Parallel File Processing

**Evidence: Line 693**
```csharp
var throttledProcessor = new ThrottledProcessor(Environment.ProcessorCount);
```

**Impact:**
- If `Environment.ProcessorCount` = 8, processes 8 files concurrently
- Each file makes its own AI API call
- Total time ≈ (total files / thread count) × (time per AI call)

### Sequential Repo/Global Processing

**Evidence: Lines 648-720 (foreach loop), 726-735 (sequential global)**

**Impact:**
- Repos processed one at a time
- Global processing waits for all repos
- Total time = sum of (repo processing times) + global processing time

---

## Integration Points

### With Layer 3 (Content Filtering)

**Evidence: Lines 784-800:**
```csharp
// Determine which patterns to use for line filtering
List<System.Text.RegularExpressions.Regex> includePatterns;

if (command.LineContainsPatterns.Any())
{
    // Use explicit --line-contains patterns if specified
    includePatterns = command.LineContainsPatterns
        .Select(p => new System.Text.RegularExpressions.Regex(p, ...))
        .ToList();
}
else
{
    // Fallback to using the search query
    includePatterns = new List<System.Text.RegularExpressions.Regex>
    {
        new System.Text.RegularExpressions.Regex(Regex.Escape(query), ...)
    };
}
```

**Impact:**
- AI sees filtered content (not full file)
- Content filtering happens BEFORE AI processing
- AI gets markdown-formatted code with line numbers

### With Layer 5 (Context Expansion)

**Evidence: Line 807:**
```csharp
var filteredContent = LineHelpers.FilterAndExpandContext(
    foundFile.Content,
    includePatterns,
    contextLines,  // lines before ← from --lines option
    contextLines,  // lines after  ← from --lines option
    true,
    excludePatterns,
    backticks,
    true
);
```

**Impact:**
- AI receives context lines around matches
- More context = better AI understanding
- Context expansion happens BEFORE AI processing

---

## Summary

### Key Implementation Facts

1. **Three-tier hierarchy**: File → Repo → Global processing
2. **Parallel file processing**: Up to CPU core count concurrent
3. **Sequential repo/global**: One at a time
4. **Dynamic extension matching**: `--{ext}-file-instructions` works for ANY extension
5. **No built-in functions**: Unlike cycodmd/cycodj
6. **No chat history**: Unlike cycodmd/cycodj
7. **Shared AI processor**: Uses common `AiInstructionProcessor` class
8. **Error tolerant**: Continues processing on file/AI errors
9. **Comprehensive logging**: Before/after each AI stage
10. **Integrated with filtering**: AI sees filtered+expanded content

---

[Back to Layer 8 Catalog](cycodgr-search-filtering-pipeline-catalog-layer-8.md)  
[Back to README](cycodgr-filtering-pipeline-catalog-README.md)
