# cycodmd FindFilesCommand - Layer 4: CONTENT REMOVAL - Proof

[🔙 Back to Layer 4](cycodmd-findfiles-layer-4.md) | [📄 Back to FindFilesCommand](cycodmd-findfiles-catalog-README.md)

## Source Code Evidence

This document provides detailed source code evidence for Layer 4 (CONTENT REMOVAL) implementation in FindFilesCommand, with exact line numbers and explanations.

---

## 1. Property Definition

### FindFilesCommand.cs

**Property Declaration**

```csharp
// Lines 106
public List<Regex> RemoveAllLineContainsPatternList;
```

**Initialization in Constructor**

```csharp
// Lines 27
RemoveAllLineContainsPatternList = new();
```

**Evidence**: The property is declared as a `List<Regex>` and initialized to an empty list in the constructor.

**Validation Check in IsEmpty()**

```csharp
// Lines 61
!RemoveAllLineContainsPatternList.Any() &&
```

**Evidence**: The `IsEmpty()` method checks if the list has any patterns to determine if the command is "empty" (no meaningful operations specified).

---

## 2. Command-Line Parsing

### CycoDmdCommandLineOptions.cs

**Option Parsing Logic**

```csharp
// Lines 152-160 in TryParseFindFilesCommandOptions()
else if (arg == "--remove-all-lines")
{
    var patterns = GetInputOptionArgs(i + 1, args);
    var asRegExs = ValidateRegExPatterns(arg, patterns);
    command.RemoveAllLineContainsPatternList.AddRange(asRegExs);
    i += patterns.Count();
}
```

**Evidence**:
1. **Line 152**: Checks if the argument is `--remove-all-lines`
2. **Line 154**: Gets all following arguments (until next `--option`) as patterns
3. **Line 155**: Validates each pattern as a regex using `ValidateRegExPatterns()`
4. **Line 156**: Adds all validated regex patterns to the command's list
5. **Line 157**: Advances the parser index by the number of patterns consumed

**Validation Method** (from CommandLineOptions.cs base class)

```csharp
protected IEnumerable<Regex> ValidateRegExPatterns(string arg, IEnumerable<string> patterns)
{
    patterns = patterns.ToList();
    if (!patterns.Any())
    {
        throw new CommandLineException($"Missing regular expression patterns for {arg}");
    }

    return patterns.Select(x => ValidateRegExPattern(arg, x));
}

protected Regex ValidateRegExPattern(string arg, string pattern)
{
    try
    {
        Logger.Info($"Creating regex pattern for '{arg}': '{pattern}'");
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return regex;
    }
    catch (Exception ex)
    {
        Logger.Error($"Failed to create regex pattern for '{arg}': '{pattern}' - {ex.Message}");
        throw new CommandLineException($"Invalid regular expression pattern for {arg}: {pattern}");
    }
}
```

**Evidence**:
- Patterns must be valid regex syntax
- Uses case-insensitive matching (`RegexOptions.IgnoreCase`)
- Invalid patterns throw `CommandLineException` with clear error message
- Logs pattern creation for debugging

---

## 3. Execution Flow

### Program.cs - Command Dispatch

**HandleFindFileCommand() - Passing Remove Patterns to Processor**

```csharp
// Lines 227-246
Func<string, Task<string>> getCheckSaveFileContent = async file =>
{
    var onlyOneFile = files.Count == 1 && commandLineOptions.Commands.Count == 1;
    var skipMarkdownWrapping = onlyOneFile && FileConverters.CanConvert(file);
    var wrapInMarkdown = !skipMarkdownWrapping;

    return await GetCheckSaveFileContentAsync(
        file,
        wrapInMarkdown,
        findFilesCommand.IncludeLineContainsPatternList,
        findFilesCommand.IncludeLineCountBefore,
        findFilesCommand.IncludeLineCountAfter,
        findFilesCommand.IncludeLineNumbers,
        findFilesCommand.RemoveAllLineContainsPatternList,  // ← Line 240: Passed here
        actualHighlightMatches,
        findFilesCommand.FileInstructionsList,
        findFilesCommand.UseBuiltInFunctions,
        findFilesCommand.SaveChatHistory,
        findFilesCommand.SaveFileOutput);
};
```

**Evidence**: Line 240 passes `RemoveAllLineContainsPatternList` as the 7th parameter to the file processing function.

---

### Program.cs - File Content Processing

**GetCheckSaveFileContentAsync() Signature**

```csharp
// Line 472
private static Task<string> GetCheckSaveFileContentAsync(
    string fileName, 
    bool wrapInMarkdown, 
    List<Regex> includeLineContainsPatternList, 
    int includeLineCountBefore, 
    int includeLineCountAfter, 
    bool includeLineNumbers, 
    List<Regex> removeAllLineContainsPatternList,  // ← Parameter here
    bool highlightMatches, 
    List<Tuple<string, string>> fileInstructionsList, 
    bool useBuiltInFunctions, 
    string? saveChatHistory, 
    string? saveFileOutput)
```

**Evidence**: The remove patterns parameter is part of the core file processing function signature.

**GetCheckSaveFileContent() - Synchronous Wrapper**

```csharp
// Line 490
private static string GetCheckSaveFileContent(
    string fileName, 
    bool wrapInMarkdown, 
    List<Regex> includeLineContainsPatternList, 
    int includeLineCountBefore, 
    int includeLineCountAfter, 
    bool includeLineNumbers, 
    List<Regex> removeAllLineContainsPatternList,  // ← Parameter here
    bool highlightMatches, 
    List<Tuple<string, string>> fileInstructionsList, 
    bool useBuiltInFunctions, 
    string? saveChatHistory, 
    string? saveFileOutput)
```

**Logging Logic**

```csharp
// Lines 505-512
if (removeAllLineContainsPatternList.Count > 0)
{
    Logger.Info($"Using {removeAllLineContainsPatternList.Count} exclude regex patterns on '{fileName}':");
    foreach (var pattern in removeAllLineContainsPatternList)
    {
        Logger.Info($"  Exclude pattern: '{pattern}'");
    }
}
```

**Evidence**: 
- Logs when remove patterns are active
- Shows count and each pattern for debugging
- Only logs if patterns are present (count > 0)

**Passing to GetFinalFileContent()**

```csharp
// Lines 515-524
var content = GetFinalFileContent(
    fileName,
    wrapInMarkdown,
    includeLineContainsPatternList,
    includeLineCountBefore,
    includeLineCountAfter,
    includeLineNumbers,
    removeAllLineContainsPatternList,  // ← Line 521: Passed here
    highlightMatches,
    fileInstructionsList,
    useBuiltInFunctions,
    saveChatHistory);
```

**Evidence**: Remove patterns flow from `GetCheckSaveFileContent()` → `GetFinalFileContent()`.

---

**GetFinalFileContent() Signature**

```csharp
// Line 542
private static string GetFinalFileContent(
    string fileName, 
    bool wrapInMarkdown, 
    List<Regex> includeLineContainsPatternList, 
    int includeLineCountBefore, 
    int includeLineCountAfter, 
    bool includeLineNumbers, 
    List<Regex> removeAllLineContainsPatternList,  // ← Parameter here
    bool highlightMatches, 
    List<Tuple<string, string>> fileInstructionsList, 
    bool useBuiltInFunctions, 
    string? saveChatHistory)
```

**Passing to GetFormattedFileContent()**

```csharp
// Lines 544-552
var formatted = GetFormattedFileContent(
    fileName,
    wrapInMarkdown,
    includeLineContainsPatternList,
    includeLineCountBefore,
    includeLineCountAfter,
    includeLineNumbers,
    removeAllLineContainsPatternList,  // ← Line 551: Passed here
    highlightMatches);
```

**Evidence**: Remove patterns flow from `GetFinalFileContent()` → `GetFormattedFileContent()`.

---

**GetFormattedFileContent() - Core Filtering Logic**

```csharp
// Line 573
private static string GetFormattedFileContent(
    string fileName, 
    bool wrapInMarkdown, 
    List<Regex> includeLineContainsPatternList, 
    int includeLineCountBefore, 
    int includeLineCountAfter, 
    bool includeLineNumbers, 
    List<Regex> removeAllLineContainsPatternList,  // ← Parameter here
    bool highlightMatches)
```

**Filtering Decision**

```csharp
// Line 584
var filterContent = includeLineContainsPatternList.Any() || removeAllLineContainsPatternList.Any();
```

**Evidence**: Filtering is triggered if EITHER include patterns OR remove patterns are present.

**Calling LineHelpers.FilterAndExpandContext()**

```csharp
// Lines 585-596
if (filterContent)
{
    content = LineHelpers.FilterAndExpandContext(
        content,
        includeLineContainsPatternList,
        includeLineCountBefore,
        includeLineCountAfter,
        includeLineNumbers,
        removeAllLineContainsPatternList,  // ← Line 593: Passed to core filter logic
        backticks,
        highlightMatches);

    if (content == null) return "";
}
```

**Evidence**: 
- Line 584: Checks if filtering is needed
- Line 593: Passes remove patterns to the core filtering helper
- If content is null (no matches), returns empty string

---

## 4. Core Filtering Logic

### LineHelpers.cs

**FilterAndExpandContext() Signature**

```csharp
// Lines 48-56
public static string? FilterAndExpandContext(
    string content, 
    List<Regex> includeLineContainsPatternList, 
    int includeLineCountBefore, 
    int includeLineCountAfter, 
    bool includeLineNumbers, 
    List<Regex> removeAllLineContainsPatternList,  // ← Parameter here
    string backticks, 
    bool highlightMatches = false)
```

**Finding Matched Lines**

```csharp
// Lines 58-64
// Find the matching lines/indices (line numbers are 1-based, indices are 0-based)
var allLines = content.Split('\n');
var matchedLineIndices = allLines.Select((line, index) => new { line, index })
    .Where(x => IsLineMatch(x.line, includeLineContainsPatternList, removeAllLineContainsPatternList))  // ← Line 61
    .Select(x => x.index)
    .ToList();
if (matchedLineIndices.Count == 0) return null;
```

**Evidence**: 
- Line 61 calls `IsLineMatch()` with BOTH include and remove patterns
- Only lines passing both filters are considered "matched"
- If no lines match, returns `null`

**Context Expansion with Removal Check (Before)**

```csharp
// Lines 71-82
for (var idxBefore = matchedLineIndex - 1; idxBefore >= matchedLineIndex - includeLineCountBefore && idxBefore >= 0; idxBefore--)
{
    if (!linesToInclude.Contains(idxBefore))
    {
        // Only add context lines that wouldn't be removed
        var contextLine = allLines[idxBefore];
        var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));  // ← Line 77
        if (!shouldRemoveContextLine)
        {
            linesToInclude.Add(idxBefore);
        }
    }
}
```

**Evidence**:
- Line 77 checks if context line matches any remove pattern
- If it matches, the line is NOT added to context
- This prevents removed lines from appearing as "context"

**Context Expansion with Removal Check (After)**

```csharp
// Lines 85-96
for (var idxAfter = matchedLineIndex + 1; idxAfter <= matchedLineIndex + includeLineCountAfter && idxAfter < allLines.Length; idxAfter++)
{
    if (!linesToInclude.Contains(idxAfter))
    {
        // Only add context lines that wouldn't be removed  
        var contextLine = allLines[idxAfter];
        var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));  // ← Line 92
        if (!shouldRemoveContextLine)
        {
            linesToInclude.Add(idxAfter);
        }
    }
}
```

**Evidence**: 
- Line 92 checks if context line matches any remove pattern
- Same logic as "before" context, but for lines after the match

---

**IsLineMatch() - Core Matching Logic**

```csharp
// Lines 8-11
public static bool IsLineMatch(
    string line, 
    List<Regex> includeLineContainsPatternList, 
    List<Regex> removeAllLineContainsPatternList)
{
    var includeMatch = includeLineContainsPatternList.All(regex => regex.IsMatch(line));
    var excludeMatch = removeAllLineContainsPatternList.Count > 0 && 
                       removeAllLineContainsPatternList.Any(regex => regex.IsMatch(line));
```

**Evidence**:
- **Line 10**: `includeMatch` = true if line matches ALL include patterns (or no include patterns exist)
- **Line 11**: `excludeMatch` = true if:
  - Remove patterns list is NOT empty, AND
  - Line matches ANY remove pattern

**Verbose Logging for Debugging**

```csharp
// Lines 13-44
// Log detailed information at verbose level
if (ConsoleHelpers.IsVerbose())
{
    var includePatternStrings = string.Join(", ", includeLineContainsPatternList.Select(r => r.ToString()));
    var excludePatternStrings = string.Join(", ", removeAllLineContainsPatternList.Select(r => r.ToString()));
    
    Logger.Verbose($"Line match check for: '{line}'");
    Logger.Verbose($"  Include patterns ({includeLineContainsPatternList.Count}): {includePatternStrings}");
    Logger.Verbose($"  Include match: {includeMatch}");
    Logger.Verbose($"  Exclude patterns ({removeAllLineContainsPatternList.Count}): {excludePatternStrings}");
    
    if (excludeMatch)
    {
        var matchedPatterns = removeAllLineContainsPatternList
            .Where(regex => regex.IsMatch(line))
            .Select(regex => regex.ToString())
            .ToList();
        Logger.Verbose($"  Exclude match: True (matched patterns: {string.Join(", ", matchedPatterns)})");
    }
    else
    {
        Logger.Verbose($"  Exclude match: False");
    }
    
    var finalMatch = includeMatch && !excludeMatch;
    Logger.Verbose($"  Final result: {finalMatch} (line will be {(finalMatch ? "shown" : "excluded")})");
}
```

**Evidence**: When verbose mode is enabled, logs:
- The line being checked
- All include patterns and whether line matches
- All exclude patterns and whether line matches
- Which specific exclude patterns matched (if any)
- Final result (shown or excluded)

**Return Logic**

```csharp
// Lines 46-47
return includeMatch && !excludeMatch;
```

**Evidence**: 
- Returns `true` ONLY if:
  - Line matches ALL include patterns (if any), AND
  - Line does NOT match ANY exclude patterns
- This implements "AND" logic for includes, "OR" logic for excludes

---

## 5. Data Flow Summary

```
User Input (CLI)
    ↓
--remove-all-lines pattern1 pattern2
    ↓
CycoDmdCommandLineOptions.cs (Lines 152-160)
    ↓ [Parse & validate regex patterns]
FindFilesCommand.RemoveAllLineContainsPatternList
    ↓ [Property: List<Regex>]
Program.HandleFindFileCommand() (Line 240)
    ↓ [Pass to file processor]
Program.GetCheckSaveFileContentAsync() (Line 472)
    ↓ [Log patterns, Line 505-512]
Program.GetFinalFileContent() (Line 521)
    ↓ [Pass to formatter]
Program.GetFormattedFileContent() (Line 593)
    ↓ [Check if filtering needed, Line 584]
LineHelpers.FilterAndExpandContext() (Line 54)
    ↓ [Core filtering logic]
LineHelpers.IsLineMatch() (Lines 8-11)
    ↓ [Check line against patterns]
RESULT: true (show) or false (exclude)
    ↓
Context expansion (Lines 71-96)
    ↓ [Check context lines too]
LineHelpers.IsLineMatch() again
    ↓ [For each context line]
RESULT: Final filtered content with context
```

---

## 6. Key Algorithms

### Remove Pattern Matching Algorithm

```
FOR each line in file:
    includeMatch = line matches ALL include patterns (or no include patterns)
    excludeMatch = line matches ANY remove pattern AND remove patterns exist
    
    IF includeMatch AND NOT excludeMatch:
        line is KEPT
    ELSE:
        line is EXCLUDED
```

### Context Expansion with Removal

```
FOR each matched line:
    FOR each context line BEFORE match (up to includeLineCountBefore):
        IF context line does NOT match any remove pattern:
            add to output
        ELSE:
            skip (do not add to output)
    
    FOR each context line AFTER match (up to includeLineCountAfter):
        IF context line does NOT match any remove pattern:
            add to output
        ELSE:
            skip (do not add to output)
```

---

## 7. Test Cases (Implied by Code)

Based on the source code, these behaviors are implemented:

1. ✅ **Multiple patterns are OR'd** (Line 11: `Any()`)
2. ✅ **Remove takes precedence over include** (Line 47: `!excludeMatch`)
3. ✅ **Context lines are also checked** (Lines 77, 92)
4. ✅ **Empty list means no removal** (Line 11: `Count > 0` check)
5. ✅ **Case-insensitive matching** (Validation: `RegexOptions.IgnoreCase`)
6. ✅ **Logging shows matched patterns** (Lines 29-32)

---

## 8. Related Source Files

| File | Lines | Purpose |
|------|-------|---------|
| `FindFilesCommand.cs` | 27, 61, 106 | Property definition and initialization |
| `CycoDmdCommandLineOptions.cs` | 152-160 | Command-line parsing |
| `Program.cs` | 240, 472, 490, 521, 551, 584, 593 | Execution flow and passing parameters |
| `LineHelpers.cs` | 8-96 | Core filtering and matching logic |
| `CommandLineOptions.cs` | (base class) | Regex validation methods |

---

[🔙 Back to Layer 4](cycodmd-findfiles-layer-4.md) | [📄 Back to FindFilesCommand](cycodmd-findfiles-catalog-README.md)
