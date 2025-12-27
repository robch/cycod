# cycodgr search - Layer 8: AI PROCESSING - PROOF

## Command Properties

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Lines 82-85**:
```csharp
// AI instruction options
public List<Tuple<string, string>> FileInstructionsList { get; set; }
public List<string> RepoInstructionsList { get; set; }
public List<string> InstructionsList { get; set; }
```

**Constructor initialization (Lines 31-33)**:
```csharp
FileInstructionsList = new List<Tuple<string, string>>();
RepoInstructionsList = new List<string>();
InstructionsList = new List<string>();
```

## Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

### --file-instructions

**Lines 224-232**:
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

### --{ext}-file-instructions

**Lines 233-244**:
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

### --repo-instructions

**Lines 245-252**:
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

### --instructions

**Lines 253-260**:
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

## Processing Implementation

### File-Level Instructions

**File**: `src/cycodgr/Program.cs`

**Lines 858-873** - In `ProcessFileGroupAsync`:
```csharp
// Apply file instructions if any match this file
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
```

### Criteria Matching

**Lines 878-883** - `FileNameMatchesInstructionsCriteria` method:
```csharp
private static bool FileNameMatchesInstructionsCriteria(string fileName, string fileNameCriteria)
{
    return string.IsNullOrEmpty(fileNameCriteria) ||
        fileName.EndsWith($".{fileNameCriteria}") ||
        fileName == fileNameCriteria;
}
```

**Matching logic**:
- Empty criteria → matches all files
- `.{ext}` suffix match (e.g., `.cs`, `.md`)
- Exact file name match

### Repository-Level Instructions

**Lines 706-716** - In `FormatAndOutputCodeResults`:
```csharp
// Apply repo instructions if any
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
```

### Global Instructions

**Lines 726-734**:
```csharp
// Apply final/global instructions if any
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
```

## Key Observations

### No Built-In Functions
All calls use `useBuiltInFunctions: false` - this feature is not exposed as a CLI option.

### No Chat History Saving
All calls use `saveChatHistory: string.Empty` - this feature is not exposed as a CLI option.

### Processing Order
1. File-level (with criteria filtering)
2. Repo-level (all files combined)
3. Global-level (all repos combined)

---

**End of Layer 8 Proof Document**
