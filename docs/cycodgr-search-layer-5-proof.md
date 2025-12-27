# cycodgr search - Layer 5: CONTEXT EXPANSION - PROOF

## Command Property Definition

**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

**Line 79**:
```csharp
public int LinesBeforeAndAfter { get; set; }
```

**Constructor initialization (Line 25)**:
```csharp
LinesBeforeAndAfter = 5;
```

## Parsing

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 187-191** - In `TryParseSearchCommandOptions`:
```csharp
else if (arg == "--lines-before-and-after" || arg == "--lines")
{
    var linesStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.LinesBeforeAndAfter = ValidateNonNegativeNumber(arg, linesStr);
}
```

**Validation** (Lines 260-270):
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

## Application

**File**: `src/cycodgr/Program.cs`

**Lines 641-697** - `FormatAndOutputCodeResults` method:
```csharp
var fileOutputs = await throttledProcessor.ProcessAsync(
    fileGroups,
    async fileGroup => await ProcessFileGroupAsync(fileGroup, repo, query, contextLines, fileInstructionsList, command, overrideQuiet)
);
```

**Line 696**: `contextLines` parameter is passed to `ProcessFileGroupAsync`

**Lines 741-748** - `ProcessFileGroupAsync` signature and usage:
```csharp
private static async Task<string> ProcessFileGroupAsync(
    IGrouping<string, CycoGr.Models.CodeMatch> fileGroup, 
    CycoGr.Models.RepoInfo repo,
    string query, 
    int contextLines,    // ← Parameter from LinesBeforeAndAfter
    List<Tuple<string, string>> fileInstructionsList,
    CycoGr.CommandLineCommands.SearchCommand command,
    bool overrideQuiet)
```

**Lines 807-816** - Usage in `LineHelpers.FilterAndExpandContext`:
```csharp
var filteredContent = LineHelpers.FilterAndExpandContext(
    foundFile.Content,
    includePatterns,
    contextLines,  // lines before ← Symmetric value
    contextLines,  // lines after  ← Same value
    true,          // include line numbers
    excludePatterns,
    backticks,
    true           // highlight matches
);
```

## Key Point: Symmetric Only

Unlike cycodmd, cycodgr does NOT have separate `--lines-before` and `--lines-after` options. The same value is used for both directions.

**Data flow**:
```
command.LinesBeforeAndAfter (default: 5)
  ↓
contextLines parameter
  ↓
LineHelpers.FilterAndExpandContext(
    ...,
    contextLines,  // lines before
    contextLines,  // lines after  ← Same value
    ...)
```

---

**End of Layer 5 Proof Document**
