# FindFilesCommand - Layer 3: Content Filtering - PROOF

[← Back to Layer 3 Documentation](cycodmd-findfiles-layer-3.md)

This document provides **source code evidence** with line numbers for all Layer 3 (Content Filtering) implementation details.

---

## Table of Contents

1. [Command Line Parsing](#command-line-parsing)
2. [Command Properties](#command-properties)
3. [Execution Flow](#execution-flow)
4. [Line Filtering Logic](#line-filtering-logic)
5. [Pattern Validation](#pattern-validation)
6. [Highlighting Logic](#highlighting-logic)

---

## Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### `--contains` Option (Dual-Layer!)

**Lines 108-115**: Parses `--contains` and adds patterns to BOTH file and line filtering

```csharp
108:         else if (arg == "--contains")
109:         {
110:             var patterns = GetInputOptionArgs(i + 1, args, required: 1);
111:             var asRegExs = ValidateRegExPatterns(arg, patterns);
112:             command.IncludeFileContainsPatternList.AddRange(asRegExs);  // Layer 2
113:             command.IncludeLineContainsPatternList.AddRange(asRegExs);  // Layer 3 (THIS LAYER!)
114:             i += patterns.Count();
115:         }
```

**Evidence**:
- Line 112: Adds to `IncludeFileContainsPatternList` (Layer 2 - Container Filtering)
- Line 113: Adds to `IncludeLineContainsPatternList` (Layer 3 - Content Filtering)
- **Dual behavior**: Same patterns apply at both layers

---

#### `--line-contains` Option

**Lines 130-136**: Parses `--line-contains` and adds patterns to line filtering ONLY

```csharp
130:         else if (arg == "--line-contains")
131:         {
132:             var patterns = GetInputOptionArgs(i + 1, args, required: 1);
133:             var asRegExs = ValidateRegExPatterns(arg, patterns);
134:             command.IncludeLineContainsPatternList.AddRange(asRegExs);
135:             i += patterns.Count();
136:         }
```

**Evidence**:
- Line 134: Only adds to `IncludeLineContainsPatternList` (Layer 3)
- No Layer 2 (file-level) filtering
- Allows showing all lines matching pattern from ANY file

---

#### `--highlight-matches` / `--no-highlight-matches` Options

**Lines 165-172**: Parses highlighting control options

```csharp
165:         else if (arg == "--highlight-matches")
166:         {
167:             command.HighlightMatches = true;
168:         }
169:         else if (arg == "--no-highlight-matches")
170:         {
171:             command.HighlightMatches = false;
172:         }
```

**Evidence**:
- `HighlightMatches` is nullable boolean (`bool?`)
- `true`: Force enable
- `false`: Force disable
- `null`: Auto-decide (see execution logic below)

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

#### Property Declaration

**Lines 98-102**: Layer 3 properties

```csharp
 98:     public List<Regex> IncludeLineContainsPatternList;
 99:     public int IncludeLineCountBefore;
100:     public int IncludeLineCountAfter;
101:     public bool IncludeLineNumbers;
102:     public bool? HighlightMatches;
```

**Evidence**:
- Line 98: Stores compiled regex patterns for line matching
- Lines 99-100: Context expansion properties (Layer 5, but used with Layer 3)
- Line 101: Line numbering (Layer 6, but affects Layer 3 output)
- Line 102: Tri-state boolean for highlighting control

---

#### Property Initialization

**Lines 18-22**: Constructor initialization

```csharp
 18:         IncludeLineContainsPatternList = new();
 19:         IncludeLineCountBefore = 0;
 20:         IncludeLineCountAfter = 0;
 21:         IncludeLineNumbers = false;
 22:         HighlightMatches = null;
```

**Evidence**:
- Line 18: Empty list = no line filtering by default
- Lines 19-20: No context expansion by default
- Line 21: No line numbers by default
- Line 22: `null` = auto-decide highlighting

---

## Execution Flow

### File: `src/cycodmd/Program.cs`

#### Auto-Enable Highlighting Logic

**Lines 222-224**: Derives actual highlighting value

```csharp
222:         var actualHighlightMatches = findFilesCommand.HighlightMatches ?? 
223:             (findFilesCommand.IncludeLineNumbers && 
224:              (findFilesCommand.IncludeLineCountBefore > 0 || findFilesCommand.IncludeLineCountAfter > 0));
```

**Evidence**:
- Line 222: Uses null-coalescing operator `??`
- **If not null**: Use explicit value from `--highlight-matches` / `--no-highlight-matches`
- **If null (auto)**: Enable when BOTH conditions met:
  - Line 223: Line numbers are enabled (`IncludeLineNumbers`)
  - Line 224: Context expansion is enabled (either before OR after > 0)
- **Rationale**: Highlighting is most useful when you can see line numbers and context

---

#### Passing Parameters to Processing Function

**Lines 233-245**: Passes Layer 3 parameters to processing function

```csharp
233:             return await GetCheckSaveFileContentAsync(
234:                 file,
235:                 wrapInMarkdown,
236:                 findFilesCommand.IncludeLineContainsPatternList,    // Layer 3 patterns
237:                 findFilesCommand.IncludeLineCountBefore,            // Context (Layer 5)
238:                 findFilesCommand.IncludeLineCountAfter,             // Context (Layer 5)
239:                 findFilesCommand.IncludeLineNumbers,                // Display (Layer 6)
240:                 findFilesCommand.RemoveAllLineContainsPatternList,  // Removal (Layer 4)
241:                 actualHighlightMatches,                             // Computed highlight value
242:                 findFilesCommand.FileInstructionsList,              // AI (Layer 8)
243:                 findFilesCommand.UseBuiltInFunctions,               // AI (Layer 8)
244:                 findFilesCommand.SaveChatHistory,                   // Output (Layer 7)
245:                 findFilesCommand.SaveFileOutput);                   // Output (Layer 7)
```

**Evidence**:
- Line 236: Layer 3 patterns passed to processing
- Lines 237-240: Other layers passed simultaneously
- **Pipeline nature**: All layers processed together in single function
- Line 241: Uses computed `actualHighlightMatches` (not raw property)

---

#### Logging Layer 3 Patterns

**Lines 496-503**: Logs patterns being used (for debugging)

```csharp
496:             if (includeLineContainsPatternList.Count > 0)
497:             {
498:                 Logger.Info($"Using {includeLineContainsPatternList.Count} include regex patterns on '{fileName}':");
499:                 foreach (var pattern in includeLineContainsPatternList)
500:                 {
501:                     Logger.Info($"  Include pattern: '{pattern}'");
502:                 }
503:             }
```

**Evidence**:
- Line 496: Only logs if patterns exist
- Lines 498-502: Logs each pattern individually
- **Purpose**: Debug visibility into what patterns are being applied

---

#### Calling Formatting Function

**Lines 514-525**: Calls formatter with Layer 3 parameters

```csharp
514:             var finalContent = GetFinalFileContent(
515:                 fileName,
516:                 wrapInMarkdown,
517:                 includeLineContainsPatternList,        // Layer 3
518:                 includeLineCountBefore,                 // Layer 5
519:                 includeLineCountAfter,                  // Layer 5
520:                 includeLineNumbers,                     // Layer 6
521:                 removeAllLineContainsPatternList,       // Layer 4
522:                 highlightMatches,                       // Computed value
523:                 fileInstructionsList,                   // Layer 8
524:                 useBuiltInFunctions,                    // Layer 8
525:                 saveChatHistory);                       // Layer 7
```

**Evidence**:
- Line 517: Passes Layer 3 patterns
- Shows data flow: Parser → Command → Execution → Formatting

---

#### Determining Whether to Filter

**Lines 584-602**: Decides if filtering should be applied

```csharp
584:             var filterContent = includeLineContainsPatternList.Any() || removeAllLineContainsPatternList.Any();
585:             if (filterContent)
586:             {
587:                 content = LineHelpers.FilterAndExpandContext(
588:                     content,
589:                     includeLineContainsPatternList,      // Layer 3
590:                     includeLineCountBefore,               // Layer 5
591:                     includeLineCountAfter,                // Layer 5
592:                     includeLineNumbers,                   // Layer 6
593:                     removeAllLineContainsPatternList,     // Layer 4
594:                     backticks,
595:                     highlightMatches);                    // Computed
596: 
597:                 // If no content matches the filter criteria, skip this file entirely
598:                 var shouldSkipFile = content == null;
599:                 if (shouldSkipFile) return string.Empty;
600:                 
601:                 wrapInMarkdown = true;
602:             }
```

**Evidence**:
- Line 584: Filtering applied if EITHER Layer 3 OR Layer 4 patterns exist
- **Key insight**: `includeLineContainsPatternList.Any()` determines if Layer 3 is active
- Lines 587-595: Calls actual filtering logic
- Lines 598-599: **Returns empty if NO matches** (file completely filtered out)
- Line 601: Forces markdown wrapping when filtering (for readability)

---

## Line Filtering Logic

### File: `src/common/Helpers/LineHelpers.cs`

#### IsLineMatch Function

**Lines 8-40**: Core matching logic for a single line

```csharp
  8:     public static bool IsLineMatch(string line, List<Regex> includeLineContainsPatternList, List<Regex> removeAllLineContainsPatternList)
  9:     {
 10:         var includeMatch = includeLineContainsPatternList.All(regex => regex.IsMatch(line));
 11:         var excludeMatch = removeAllLineContainsPatternList.Count > 0 && removeAllLineContainsPatternList.Any(regex => regex.IsMatch(line));
 12:         
 13:         // Log detailed information at verbose level
 14:         if (ConsoleHelpers.IsVerbose())
 15:         {
 16:             if (!includeMatch && includeLineContainsPatternList.Count > 0)
 17:             {
 18:                 var failedPatterns = includeLineContainsPatternList
 19:                     .Where(regex => !regex.IsMatch(line))
 20:                     .Select(regex => regex.ToString())
 21:                     .ToList();
 22:                 
 23:                 Logger.Verbose($"Line excluded because it doesn't match include patterns: [{string.Join(", ", failedPatterns)}]");
 24:                 Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
 25:             }
 26:             
 27:             if (excludeMatch)
 28:             {
 29:                 var matchedPatterns = removeAllLineContainsPatternList
 30:                     .Where(regex => regex.IsMatch(line))
 31:                     .Select(regex => regex.ToString())
 32:                     .ToList();
 33:                     
 34:                 Logger.Verbose($"Line excluded because it matches exclude patterns: [{string.Join(", ", matchedPatterns)}]");
 35:                 Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
 36:             }
 37:         }
 38: 
 39:         return includeMatch && !excludeMatch;
 40:     }
```

**Evidence**:
- Line 10: **`All()`** = ALL patterns must match (AND logic for include)
- Line 11: **`Any()`** = ANY pattern matching triggers exclusion (OR logic for exclude)
- Line 39: **Combined logic**: `includeMatch && !excludeMatch`
  - Line passes Layer 3 if: matches all include patterns AND doesn't match any exclude patterns
- Lines 14-37: Verbose logging for debugging why lines are excluded

**Key Insight**: If `includeLineContainsPatternList` is empty, `All()` returns `true` → ALL lines pass (no filtering).

---

#### FilterAndExpandContext Function (Header)

**Lines 48-57**: Function signature

```csharp
 48:     public static string? FilterAndExpandContext(
 49:         string content, 
 50:         List<Regex> includeLineContainsPatternList,  // Layer 3
 51:         int includeLineCountBefore,                   // Layer 5
 52:         int includeLineCountAfter,                    // Layer 5
 53:         bool includeLineNumbers,                      // Layer 6
 54:         List<Regex> removeAllLineContainsPatternList, // Layer 4
 55:         string backticks, 
 56:         bool highlightMatches = false)
 57:     {
```

**Evidence**:
- Line 50: Layer 3 patterns
- Lines 51-52: Context expansion (Layer 5) integrated
- Line 53: Line numbers (Layer 6) integrated
- Line 54: Removal patterns (Layer 4) integrated
- **Design**: All layers processed in single pass for efficiency

---

#### Finding Matching Lines

**Lines 58-64**: Identifies which lines match Layer 3 patterns

```csharp
 58:         // Find the matching lines/indices (line numbers are 1-based, indices are 0-based)
 59:         var allLines = content.Split('\n');
 60:         var matchedLineIndices = allLines.Select((line, index) => new { line, index })
 61:             .Where(x => IsLineMatch(x.line, includeLineContainsPatternList, removeAllLineContainsPatternList))
 62:             .Select(x => x.index)
 63:             .ToList();
 64:         if (matchedLineIndices.Count == 0) return null;
```

**Evidence**:
- Line 59: Splits content into lines
- Line 60: LINQ Select with index tracking
- Line 61: Calls `IsLineMatch` for each line
- Line 62: Collects indices of matched lines
- Line 64: **Returns `null` if NO matches** → triggers "skip file" logic in caller

**Key Insight**: Zero-indexed line tracking for context expansion.

---

#### Context Expansion Logic

**Lines 66-98**: Expands matched lines to include context (Layer 5 integration)

```csharp
 66:         // Expand the range of lines, based on before and after counts
 67:         var linesToInclude = new HashSet<int>(matchedLineIndices);
 68:         foreach (var index in matchedLineIndices)
 69:         {
 70:             for (int b = 1; b <= includeLineCountBefore; b++)
 71:             {
 72:                 var idxBefore = index - b;
 73:                 if (idxBefore >= 0)
 74:                 {
 75:                     // Only add context lines that wouldn't be removed
 76:                     var contextLine = allLines[idxBefore];
 77:                     var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));
 78:                     if (!shouldRemoveContextLine)
 79:                     {
 80:                         linesToInclude.Add(idxBefore);
 81:                     }
 82:                 }
 83:             }
 84: 
 85:             for (int a = 1; a <= includeLineCountAfter; a++)
 86:             {
 87:                 var idxAfter = index + a;
 88:                 if (idxAfter < allLines.Length)
 89:                 {
 90:                     // Only add context lines that wouldn't be removed  
 91:                     var contextLine = allLines[idxAfter];
 92:                     var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));
 93:                     if (!shouldRemoveContextLine)
 94:                     {
 95:                         linesToInclude.Add(idxAfter);
 96:                     }
 97:                 }
 98:             }
```

**Evidence**:
- Line 67: HashSet to avoid duplicate line inclusions
- Lines 70-83: Adds lines BEFORE matched lines
- Lines 85-98: Adds lines AFTER matched lines
- Lines 77, 92: **Context lines also checked against Layer 4 (removal) patterns**
- **Layer interaction**: Context expansion (Layer 5) respects Layer 4 removal patterns

---

## Pattern Validation

### File: `src/common/CommandLine/CommandLineOptions.cs`

#### ValidateRegExPatterns Function

**Lines in CommandLineOptions.cs**: Pattern validation

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
- **Early validation**: Patterns compiled at parse time, not runtime
- **RegexOptions.IgnoreCase**: Case-insensitive by default
- **RegexOptions.CultureInvariant**: Consistent behavior across locales
- **Error handling**: Invalid patterns throw `CommandLineException` immediately

---

## Highlighting Logic

### File: `src/common/Helpers/LineHelpers.cs`

**Note**: The actual highlighting implementation is in `FilterAndExpandContext` (lines after context expansion). The function highlights matches by wrapping them in markdown bold `**...**`.

The highlighting logic:
1. When `highlightMatches = true`
2. For each matched line
3. Replaces regex matches with `**$&**` (wraps in bold)
4. Preserves original line otherwise

**Implementation detail**: The highlighting code is integrated into the line processing loop after context expansion is determined.

---

## Summary of Evidence

### Parsing (Layer 3 Options)
- `--line-contains`: Lines 130-136 of `CycoDmdCommandLineOptions.cs`
- `--contains` (dual-layer): Lines 108-115 of `CycoDmdCommandLineOptions.cs`
- `--highlight-matches` / `--no-highlight-matches`: Lines 165-172 of `CycoDmdCommandLineOptions.cs`

### Properties
- `IncludeLineContainsPatternList`: Line 98 of `FindFilesCommand.cs`
- `HighlightMatches`: Line 102 of `FindFilesCommand.cs`

### Execution
- Auto-highlighting logic: Lines 222-224 of `Program.cs`
- Parameter passing: Lines 233-245 of `Program.cs`
- Filter decision: Line 584 of `Program.cs`
- Filter invocation: Lines 587-595 of `Program.cs`

### Core Filtering
- `IsLineMatch`: Lines 8-40 of `LineHelpers.cs`
- `FilterAndExpandContext`: Lines 48-98+ of `LineHelpers.cs`
- Matched line finding: Lines 59-64 of `LineHelpers.cs`
- Context expansion: Lines 66-98 of `LineHelpers.cs`

### Validation
- Pattern validation: `ValidateRegExPatterns` and `ValidateRegExPattern` in `CommandLineOptions.cs`

---

## See Also

- [Layer 3 Documentation](cycodmd-findfiles-layer-3.md) - Conceptual overview
- [Layer 2 Proof](cycodmd-findfiles-layer-2-proof.md) - Container filtering implementation
- [Layer 4 Proof](cycodmd-findfiles-layer-4-proof.md) - Content removal implementation
- [Layer 5 Proof](cycodmd-findfiles-layer-5-proof.md) - Context expansion implementation

