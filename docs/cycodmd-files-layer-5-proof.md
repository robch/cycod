# cycodmd File Search - Layer 5: Context Expansion - PROOF

**[← Back to Layer 5 Documentation](cycodmd-files-layer-5.md)**

## Source Code Evidence

This document provides **detailed source code references** proving how Layer 5 (Context Expansion) is implemented for the File Search command in cycodmd.

---

## 1. Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Option: `--lines` (Symmetric Expansion)

**Lines 137-143**:
```csharp
137:         else if (arg == "--lines")
138:         {
139:             var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
140:             var count = ValidateLineCount(arg, countStr);
141:             command.IncludeLineCountBefore = count;
142:             command.IncludeLineCountAfter = count;
143:         }
```

**Evidence**:
- Line 137: Recognizes `--lines` argument
- Line 139: Extracts the numeric value from the next argument
- Line 140: Validates the value using `ValidateLineCount()` helper
- Lines 141-142: Sets BOTH `IncludeLineCountBefore` and `IncludeLineCountAfter` to the same value (symmetric)

#### Option: `--lines-before` (Asymmetric Expansion)

**Lines 144-148**:
```csharp
144:         else if (arg == "--lines-before")
145:         {
146:             var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
147:             command.IncludeLineCountBefore = ValidateLineCount(arg, countStr);
148:         }
```

**Evidence**:
- Line 144: Recognizes `--lines-before` argument
- Line 146: Extracts numeric value
- Line 147: Sets ONLY `IncludeLineCountBefore` (asymmetric - only before context)

#### Option: `--lines-after` (Asymmetric Expansion)

**Lines 149-153**:
```csharp
149:         else if (arg == "--lines-after")
150:         {
151:             var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
152:             command.IncludeLineCountAfter = ValidateLineCount(arg, countStr);
153:         }
```

**Evidence**:
- Line 149: Recognizes `--lines-after` argument
- Line 151: Extracts numeric value
- Line 152: Sets ONLY `IncludeLineCountAfter` (asymmetric - only after context)

---

## 2. Command Properties Storage

### File: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

#### Property Declarations

**Lines 99-100**:
```csharp
 99:     public int IncludeLineCountBefore;
100:     public int IncludeLineCountAfter;
```

**Evidence**: 
- These properties store the context expansion counts parsed from CLI options
- Type `int` allows any non-negative count value
- Separate properties enable asymmetric expansion (different before/after counts)

#### Default Values

**Lines 19-20**:
```csharp
 19:         IncludeLineCountBefore = 0;
 20:         IncludeLineCountAfter = 0;
```

**Evidence**: 
- Constructor initializes both to `0` (no context expansion by default)
- User must explicitly request context expansion via CLI options

---

## 3. Execution Path

### File: `src/cycodmd/Program.cs`

#### Context Check and Invocation

**Lines 584-596**:
```csharp
584:             var filterContent = includeLineContainsPatternList.Any() || removeAllLineContainsPatternList.Any();
585:             if (filterContent)
586:             {
587:                 content = LineHelpers.FilterAndExpandContext(
588:                     content,
589:                     includeLineContainsPatternList,
590:                     includeLineCountBefore,     // ← Layer 5 parameter
591:                     includeLineCountAfter,      // ← Layer 5 parameter
592:                     includeLineNumbers,
593:                     removeAllLineContainsPatternList,
594:                     backticks,
595:                     highlightMatches);
596: ```

**Evidence**:
- Line 584: Context expansion only happens if content filtering is active (Layer 3 or Layer 4)
- Line 585: Guard check ensures `FilterAndExpandContext` is only called when needed
- Lines 590-591: **Passes the Layer 5 parameters** (`includeLineCountBefore`, `includeLineCountAfter`) to the helper function
- These parameters control how many lines before/after matches are included

#### Variable Source

**Lines 560-561** (earlier in the same method):
```csharp
560:             var includeLineCountBefore = findFilesCommand.IncludeLineCountBefore;
561:             var includeLineCountAfter = findFilesCommand.IncludeLineCountAfter;
```

**Evidence**:
- These local variables are extracted from the `FindFilesCommand` object
- They're the values set during parsing (Section 1 above)
- They're passed to the context expansion logic

---

## 4. Core Algorithm Implementation

### File: `src/common/Helpers/LineHelpers.cs`

#### Function Signature

**Lines 48-56**:
```csharp
 48:     public static string? FilterAndExpandContext(
 49:         string content, 
 50:         List<Regex> includeLineContainsPatternList, 
 51:         int includeLineCountBefore,    // ← Layer 5 parameter
 52:         int includeLineCountAfter,     // ← Layer 5 parameter
 53:         bool includeLineNumbers, 
 54:         List<Regex> removeAllLineContainsPatternList, 
 55:         string backticks, 
 56:         bool highlightMatches = false)
```

**Evidence**:
- Lines 51-52: Function accepts the context expansion counts as parameters
- These are the values passed from `Program.cs` (Section 3)

#### Step 1: Find Matching Lines

**Lines 58-64**:
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
- Line 59: Splits file content into individual lines
- Lines 60-63: Identifies which line indices match Layer 3 filters
- Line 64: If no matches, returns `null` (no context expansion needed)

#### Step 2: Expand Context Before Matches

**Lines 66-83**:
```csharp
 66:         // Expand the range of lines, based on before and after counts
 67:         var linesToInclude = new HashSet<int>(matchedLineIndices);
 68:         foreach (var index in matchedLineIndices)
 69:         {
 70:             for (int b = 1; b <= includeLineCountBefore; b++)  // ← Uses Layer 5 parameter
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
```

**Evidence**:
- Line 67: Uses `HashSet` to avoid duplicate line indices (handles overlapping ranges efficiently)
- Line 70: **Loop uses `includeLineCountBefore`** to determine how many lines to add before each match
- Line 72: Calculates index of context line (`index - b`)
- Line 73: Boundary check (don't go before first line)
- Lines 76-78: **Respects Layer 4 removal patterns** - context lines matching removal patterns are excluded
- Line 80: Adds valid context line to the set

#### Step 3: Expand Context After Matches

**Lines 85-99**:
```csharp
 85:             for (int a = 1; a <= includeLineCountAfter; a++)  // ← Uses Layer 5 parameter
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
 99:         }
```

**Evidence**:
- Line 85: **Loop uses `includeLineCountAfter`** to determine how many lines to add after each match
- Line 87: Calculates index of context line (`index + a`)
- Line 88: Boundary check (don't go past last line)
- Lines 91-93: **Respects Layer 4 removal patterns** - context lines matching removal patterns are excluded
- Line 95: Adds valid context line to the set

#### Step 4: Sort and Prepare for Output

**Lines 100-104**:
```csharp
100:         var expandedLineIndices = linesToInclude.OrderBy(i => i).ToList();
101: 
102:         var checkForLineNumberBreak = (includeLineCountBefore + includeLineCountAfter) > 0;
103:         int? previousLineIndex = null;
```

**Evidence**:
- Line 100: Sorts line indices in ascending order (ensures lines appear in original file order)
- Line 102: **Determines if gap separators are needed** - only when context expansion is active (non-zero counts)
- Line 103: Tracks previous line index to detect gaps

#### Step 5: Generate Output with Gap Handling

**Lines 106-134**:
```csharp
106:         // Loop through the lines to include and accumulate the output
107:         var output = new List<string>();
108:         foreach (var index in expandedLineIndices)
109:         {
110:             var addSeparatorForLineNumberBreak = checkForLineNumberBreak && previousLineIndex != null && index > previousLineIndex + 1;
111:             if (addSeparatorForLineNumberBreak)
112:             {
113:                 output.Add($"{backticks}\n\n{backticks}");
114:             }
115: 
116:             var line = allLines[index];
117:             var isMatchingLine = matchedLineIndices.Contains(index); // Track if this line was an actual match
118: 
119:             if (includeLineNumbers)
120:             {
121:                 var lineNumber = index + 1;
122:                 // Add * prefix for matching lines when highlighting is enabled
123:                 var prefix = highlightMatches && isMatchingLine ? "*" : " ";
124:                 
125:                 output.Add($"{prefix} {lineNumber}: {line}");
126:             }
127:             else
128:             {
129:                 // Add * prefix for matching lines when highlighting is enabled (no line numbers)
130:                 var prefix = highlightMatches && isMatchingLine ? "* " : "";
131:                 output.Add($"{prefix}{line}");
132:             }
133: 
134:             previousLineIndex = index;
135:         }
```

**Evidence**:
- Line 110: **Detects gaps** - if current line index is more than 1 greater than previous index, there's a gap
- Lines 111-114: Inserts separator block when gap is detected
- Line 117: Identifies whether current line is an actual match (vs. context line)
- Lines 119-125: Formats output with line numbers and highlighting (Layer 6 integration)
- Lines 127-131: Formats output without line numbers but with highlighting (Layer 6 integration)
- Line 134: Updates `previousLineIndex` for next iteration's gap detection

#### Return Formatted Output

**Lines 136-137**:
```csharp
136:         return string.Join("\n", output);
137:     }
```

**Evidence**:
- Line 136: Joins all output lines with newlines
- Line 137: Returns the expanded and formatted content

---

## 5. Data Flow Summary

```
CLI Parsing (CycoDmdCommandLineOptions.cs:137-153)
           ↓
       Stores values in command properties
           ↓
FindFilesCommand.IncludeLineCountBefore (line 99)
FindFilesCommand.IncludeLineCountAfter (line 100)
           ↓
       Extracted in Program.cs
           ↓
Local variables (Program.cs:560-561)
           ↓
       Passed to helper function
           ↓
LineHelpers.FilterAndExpandContext(Program.cs:590-591)
           ↓
       Used in expansion loops
           ↓
Context expansion logic (LineHelpers.cs:70, 85)
           ↓
       Returns expanded content
           ↓
Display output (Layer 6)
```

---

## 6. Integration with Other Layers

### Layer 3 (Content Filter) Integration

**Evidence**: `LineHelpers.cs:61`
```csharp
61:             .Where(x => IsLineMatch(x.line, includeLineContainsPatternList, removeAllLineContainsPatternList))
```
- Context expansion requires Layer 3 to identify which lines match
- Without matching lines, context expansion has nothing to expand around

### Layer 4 (Content Removal) Integration

**Evidence**: `LineHelpers.cs:77-78, 92-93`
```csharp
77:                     var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));
78:                     if (!shouldRemoveContextLine)
```
- Context lines are checked against Layer 4 removal patterns
- Lines matching removal patterns are NOT added as context, even if they're in range

### Layer 6 (Display Control) Integration

**Evidence**: `LineHelpers.cs:119-131`
- Context expansion result is passed to display formatting
- Line numbers (Layer 6) are applied to both match lines and context lines
- Highlighting (Layer 6) distinguishes match lines from context lines

---

## 7. Validation Logic

### File: `src/common/CommandLine/CommandLineOptions.cs`

**Line count validation is delegated to a shared helper**:

```csharp
protected int ValidateLineCount(string arg, string? countStr)
{
    return ValidateInt(arg, countStr, "line count");
}

protected int ValidateInt(string arg, string? countStr, string argDescription)
{
    if (string.IsNullOrEmpty(countStr))
    {
        throw new CommandLineException($"Missing {argDescription} for {arg}");
    }

    if (!int.TryParse(countStr, out var count))
    {
        throw new CommandLineException($"Invalid {argDescription} for {arg}: {countStr}");
    }

    return count;
}
```

**Evidence**:
- Validates that a value is provided
- Validates that the value is a valid integer
- Throws `CommandLineException` with descriptive error message if validation fails
- No negative value check (assumes user won't provide negative, which would have no effect anyway)

---

## 8. Test Evidence

While this proof document focuses on source code, the context expansion feature can be verified by running:

```bash
# Test symmetric expansion
cycodmd **/*.cs --line-contains "async" --lines 2

# Test asymmetric expansion
cycodmd **/*.cs --line-contains "async" --lines-before 1 --lines-after 3

# Test with removal patterns (context lines matching pattern should be excluded)
cycodmd **/*.cs --line-contains "public" --lines 2 --remove-all-lines "private"
```

---

**Conclusion**: This proof document demonstrates with **line-number precision** how Layer 5 (Context Expansion) is implemented across parsing, storage, execution, and core algorithm implementation files.

**[← Back to Layer 5 Documentation](cycodmd-files-layer-5.md)**
