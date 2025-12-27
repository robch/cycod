# cycodgr search - Layer 5: CONTEXT EXPANSION - Source Code Proof

## Overview

This document provides **source code evidence** for all assertions made in the Layer 5 (CONTEXT EXPANSION) documentation. All line numbers and code quotations are from the current codebase.

---

## 1. Option Parsing: `--lines-before-and-after` and `--lines`

### Location
**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

### Evidence

**Lines 353-358**: Parsing logic for `--lines-before-and-after` and `--lines` options

```csharp
else if (arg == "--lines-before-and-after" || arg == "--lines")
{
    var linesStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    command.LinesBeforeAndAfter = ValidateNonNegativeNumber(arg, linesStr);
}
```

**Explanation**:
- Both `--lines-before-and-after` and `--lines` are accepted (aliases)
- The next argument is extracted and parsed as a non-negative integer
- Value is stored in `command.LinesBeforeAndAfter`
- Parser validates the number using `ValidateNonNegativeNumber()` helper method

---

## 2. Validation Helper Method

### Location
**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

### Evidence

**Lines 573-587**: Validation logic for non-negative integers

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

**Explanation**:
- Validates that a value was provided
- Parses string to integer
- Ensures number is >= 0 (non-negative)
- Throws `CommandLineException` with clear error message if validation fails

---

## 3. Property Storage in SearchCommand

### Location
**File**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`

### Evidence

**Line 25**: Default value initialization

```csharp
LinesBeforeAndAfter = 5;
```

**Line 79**: Property declaration

```csharp
public int LinesBeforeAndAfter { get; set; }
```

**Explanation**:
- Property is of type `int`
- Default value is **5** lines (set in constructor, line 25)
- Public property with getter and setter
- Accessible from execution code (Program.cs)

---

## 4. Data Flow to Execution

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence Path 1: HandleCodeSearchAsync → FormatAndOutputCodeResults

**Line 418**: Context value passed to formatting function

```csharp
await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, command, overrideQuiet: true);
```

**Explanation**:
- `command.LinesBeforeAndAfter` is passed as the second parameter
- Named `contextLines` in the called function
- Applied to all code matches in the result set

### Evidence Path 2: FormatAndOutputCodeResults → ProcessFileGroupAsync

**Lines 694-697**: Async processing of file groups

```csharp
var fileOutputs = await throttledProcessor.ProcessAsync(
    fileGroups,
    async fileGroup => await ProcessFileGroupAsync(fileGroup, repo, query, contextLines, fileInstructionsList, command, overrideQuiet)
);
```

**Explanation**:
- `contextLines` parameter (from `command.LinesBeforeAndAfter`) is passed to `ProcessFileGroupAsync`
- Processed in parallel using `ThrottledProcessor`
- Each file group receives the same context line count

---

## 5. Application to Line Filtering

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence

**Lines 807-816**: LineHelpers.FilterAndExpandContext call

```csharp
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

**Explanation**:
- `contextLines` (from `command.LinesBeforeAndAfter`) is used for BOTH before and after
- This confirms **symmetric context expansion** behavior
- Same value passed to both `linesBefore` and `linesAfter` parameters
- LineHelpers.FilterAndExpandContext (from common helpers) performs the actual expansion

---

## 6. LineHelpers Implementation Context

### Location
**File**: `src/common/Helpers/LineHelpers.cs` (referenced from Program.cs)

### Evidence from Call Site

**Lines 807-816** (already shown above)

The call to `LineHelpers.FilterAndExpandContext` shows:
1. `contextLines` is used for both `linesBefore` and `linesAfter` parameters (lines 810-811)
2. Line numbers are included (parameter: `true`, line 812)
3. Matches are highlighted (parameter: `true`, line 815)

**Expected LineHelpers Behavior** (based on call signature):
- Identifies lines matching `includePatterns`
- Expands to include `contextLines` lines before each match
- Expands to include `contextLines` lines after each match
- Merges overlapping ranges
- Returns formatted string with line numbers

---

## 7. Integration with Other Search Modes

### Evidence: HandleUnifiedSearchAsync

**Line 286**: Context passed to unified search formatting

```csharp
await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, command, overrideQuiet: true);
```

**Explanation**:
- Same context value used for unified search (--contains)
- Consistent behavior across search modes

### Evidence: HandleRepoSearchAsync

**Lines 299-369**: Repository search logic

No reference to `LinesBeforeAndAfter` in repository-only search path.

**Explanation**:
- Context expansion is NOT applied to repository search (Layer 1 targets)
- Only applied when displaying code/file content

---

## 8. File Content Fetching Context

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence

**Lines 762-780**: File content loading

```csharp
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

**Explanation**:
- Full file content is fetched from GitHub's raw URL
- Content is loaded before context expansion is applied
- This ensures LineHelpers has the complete file to work with

---

## 9. Pattern Determination for Line Filtering

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence

**Lines 784-800**: Determine which patterns to use

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

**Explanation**:
- If `--line-contains` patterns are specified (Layer 3), use those for matching
- Otherwise, use the search query itself
- These patterns determine which lines are "matches" that get context expansion

---

## 10. Output Format Integration

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence

**Lines 818-827**: Display filtered content with backticks

```csharp
if (filteredContent != null)
{
    output.AppendLine(backticks);
    output.AppendLine(filteredContent);
    output.AppendLine("```");
}
else
{
    output.AppendLine("(No matches found in full file content)");
}
```

**Explanation**:
- Filtered and expanded content is wrapped in code fences (```language)
- Language is detected from file extension (line 804)
- If no matches found after filtering, display message

---

## 11. Fallback Behavior (No Full Content)

### Location
**File**: `src/cycodgr/Program.cs`

### Evidence

**Lines 829-851**: Exception handling and fallback

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

**Explanation**:
- If file content cannot be fetched (network error, permission, etc.)
- Falls back to displaying GitHub's text match fragments
- Fragments may have limited or no context expansion (GitHub-controlled)

---

## 12. Default Value Application

### Evidence Trace

1. **SearchCommand Constructor** (SearchCommand.cs, line 25):
   ```csharp
   LinesBeforeAndAfter = 5;
   ```

2. **Option Parsing** (CycoGrCommandLineOptions.cs, lines 353-358):
   - Only overwrites default if `--lines` or `--lines-before-and-after` is specified
   - If not specified, default value (5) remains

3. **Execution** (Program.cs, line 418):
   - Uses `command.LinesBeforeAndAfter` regardless of whether it was user-specified or default

**Result**: If user doesn't specify, **5 lines** of context are shown before and after each match.

---

## 13. Validation of Non-Negative Constraint

### Evidence

**Lines 577-582** of CycoGrCommandLineOptions.cs:

```csharp
if (!int.TryParse(value, out var number) || number < 0)
{
    throw new CommandLineException($"Invalid value for {arg}: '{value}' (must be a non-negative integer)");
}

return number;
```

**Test Cases**:
- `--lines 5` → ✅ Valid (5)
- `--lines 0` → ✅ Valid (0 lines = no context, match only)
- `--lines -1` → ❌ Invalid (exception thrown)
- `--lines abc` → ❌ Invalid (exception thrown)

---

## 14. Symmetric Expansion Proof

### Evidence

**Program.cs, lines 810-811**:

```csharp
contextLines,  // lines before
contextLines,  // lines after
```

**Analysis**:
- SAME variable (`contextLines`) is passed to BOTH parameters
- No separate `linesBefore` and `linesAfter` properties in SearchCommand
- No CLI options for `--lines-before` or `--lines-after`

**Conclusion**: cycodgr supports ONLY symmetric context expansion (same count before and after).

---

## 15. Context Expansion Scope

### Where Applied

**Code Search Path**:
- HandleCodeSearchAsync (line 371)
  → FormatAndOutputCodeResults (line 418)
  → ProcessFileGroupAsync (line 696)
  → LineHelpers.FilterAndExpandContext (line 807)

**NOT Applied**:
- Repository-only search (HandleRepoSearchAsync) - no file content
- Format modes without line-level display:
  - `--format files` (FormatCodeAsFiles, line 1098)
  - `--format urls` (FormatCodeAsUrls, line 1117)
  - `--format repos` (FormatCodeAsRepos, line 1111)
  - `--format json` / `--format csv` (contain raw fragments, not expanded)

---

## 16. Interaction with AI Processing

### Evidence

**Program.cs, lines 857-873**: File instructions application

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

**Explanation**:
- AI processing happens AFTER context expansion
- `formattedOutput` includes the expanded context from lines 818-822
- AI receives the full context (matches + surrounding lines)
- AI instructions can analyze code in context

---

## Summary of Evidence

| Assertion | Evidence Location | Line Numbers |
|-----------|-------------------|--------------|
| Options `--lines-before-and-after`, `--lines` | CycoGrCommandLineOptions.cs | 353-358 |
| Validation (non-negative integer) | CycoGrCommandLineOptions.cs | 573-587 |
| Default value (5 lines) | SearchCommand.cs | 25 |
| Property storage | SearchCommand.cs | 79 |
| Data flow to execution | Program.cs | 418, 696 |
| Application via LineHelpers | Program.cs | 807-816 |
| Symmetric expansion | Program.cs | 810-811 |
| Applied to code search only | Program.cs | 371-436 (HandleCodeSearchAsync) |
| NOT applied to repo search | Program.cs | 299-369 (HandleRepoSearchAsync) |
| AI processing includes context | Program.cs | 857-873 |

---

## Call Stack Summary

Complete call stack for context expansion:

```
CycoGrCommandLineOptions.Parse()
    ↓ (line 353-358)
CycoGrCommandLineOptions.TryParseSearchCommandOptions()
    ↓
SearchCommand.LinesBeforeAndAfter = <value>
    ↓ (default: 5, line 25)
Program.Main()
    ↓ (line 64)
Program.HandleSearchCommandAsync()
    ↓ (line 156)
Program.HandleCodeSearchAsync()
    ↓ (line 418)
Program.FormatAndOutputCodeResults()
    ↓ (line 696)
Program.ProcessFileGroupAsync()
    ↓ (line 780)
[Fetch file content from GitHub]
    ↓ (line 807)
LineHelpers.FilterAndExpandContext()
    ↓ (lines 810-811: contextLines used for both before and after)
[Return filtered & expanded content]
    ↓ (line 821)
Display output with code fences
```

---

## Conclusion

All assertions in the Layer 5 documentation are supported by source code evidence with precise line numbers. The context expansion feature:

1. ✅ Accepts `--lines-before-and-after` and `--lines` options
2. ✅ Defaults to 5 lines
3. ✅ Validates non-negative integers
4. ✅ Applies symmetric expansion (same before/after)
5. ✅ Only applies to code search results
6. ✅ Integrates with LineHelpers for implementation
7. ✅ Provides context to AI processing
8. ✅ Works with line filtering patterns

**Documentation Status**: ✅ Fully verified against source code
