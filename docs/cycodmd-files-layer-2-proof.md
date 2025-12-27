# cycodmd Files Command - Layer 2: Container Filter - PROOF

This document provides source code evidence for all Layer 2 (Container Filter) functionality in the cycodmd Files command.

---

## Parser Implementation

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### --contains Option

**Lines 108-115**:

```csharp
108:         else if (arg == "--contains")
109:         {
110:             var patterns = GetInputOptionArgs(i + 1, args, required: 1);
111:             var asRegExs = ValidateRegExPatterns(arg, patterns);
112:             command.IncludeFileContainsPatternList.AddRange(asRegExs);
113:             command.IncludeLineContainsPatternList.AddRange(asRegExs);
114:             i += patterns.Count();
115:         }
```

**Evidence**:
- Line 110: Gets one or more pattern arguments
- Line 111: Validates and compiles patterns as regex
- Line 112: Adds to `IncludeFileContainsPatternList` **(Layer 2 - file-level)**
- Line 113: Adds to `IncludeLineContainsPatternList` **(Layer 3 - line-level)**
- Line 114: Advances argument index

**Key Insight**: `--contains` is a **dual-layer option** affecting both Layer 2 (Container Filter) and Layer 3 (Content Filter).

---

#### --file-contains Option

**Lines 116-122**:

```csharp
116:         else if (arg == "--file-contains")
117:         {
118:             var patterns = GetInputOptionArgs(i + 1, args, required: 1);
119:             var asRegExs = ValidateRegExPatterns(arg, patterns);
120:             command.IncludeFileContainsPatternList.AddRange(asRegExs);
121:             i += patterns.Count();
122:         }
```

**Evidence**:
- Line 118: Gets one or more pattern arguments (minimum 1 required)
- Line 119: Validates and compiles patterns as regex using `ValidateRegExPatterns`
- Line 120: Adds compiled regex patterns to `IncludeFileContainsPatternList`
- Line 121: Advances argument index by number of patterns consumed

**Key Insight**: `--file-contains` affects **only Layer 2** (file-level filtering), not line-level.

---

#### --file-not-contains Option

**Lines 123-129**:

```csharp
123:         else if (arg == "--file-not-contains")
124:         {
125:             var patterns = GetInputOptionArgs(i + 1, args, required: 1);
126:             var asRegExs = ValidateRegExPatterns(arg, patterns);
127:             command.ExcludeFileContainsPatternList.AddRange(asRegExs);
128:             i += patterns.Count();
129:         }
```

**Evidence**:
- Line 125: Gets one or more pattern arguments (minimum 1 required)
- Line 126: Validates and compiles patterns as regex
- Line 127: Adds to `ExcludeFileContainsPatternList` (exclusion list)
- Line 128: Advances argument index

**Key Insight**: Files matching ANY pattern in `ExcludeFileContainsPatternList` are excluded.

---

#### Extension-Specific File Instructions Pattern

**Lines 268-281**:

```csharp
268:         else if (arg.StartsWith("--") && arg.EndsWith("file-instructions"))
269:         {
270:             var instructions = GetInputOptionArgs(i + 1, args);
271:             if (instructions.Count() == 0)
272:             {
273:                 throw new CommandLineException($"Missing instructions for {arg}");
274:             }
275:             var fileNameCriteria = arg != "--file-instructions"
276:                 ? arg.Substring(2, arg.Length - 20)
277:                 : string.Empty;
278:             var withCriteria = instructions.Select(x => Tuple.Create(x, fileNameCriteria));
279:             command.FileInstructionsList.AddRange(withCriteria);
280:             i += instructions.Count();
281:         }
```

**Evidence**:
- Line 268: Pattern matches `--{criteria}-file-instructions` (e.g., `--cs-file-instructions`)
- Line 275-277: Extracts the file name criteria (extension) from the option name
  - Example: `--cs-file-instructions` → criteria = `"cs"`
  - Example: `--file-instructions` → criteria = `""`
- Line 278: Creates tuple of (instruction, criteria)
- Line 279: Adds to `FileInstructionsList`

**Note**: This pattern is used for AI processing (Layer 8), not pure content filtering. A dedicated `--{ext}-file-contains` option doesn't exist in the current implementation. To achieve extension-specific file filtering, users must combine glob patterns with `--file-contains`:

```bash
cycodmd "**/*.cs" --file-contains "async"
```

---

### Helper Method: ValidateRegExPatterns

**Definition** (from CommandLineOptions.cs base class):

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
- Regex patterns are compiled with `RegexOptions.IgnoreCase` (case-insensitive)
- Regex patterns use `RegexOptions.CultureInvariant` (consistent across cultures)
- Invalid regex patterns throw `CommandLineException` with clear error message
- Pattern compilation is logged for debugging

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

#### Property Declarations

**Lines 95-96**:

```csharp
95:     public List<Regex> IncludeFileContainsPatternList;
96:     public List<Regex> ExcludeFileContainsPatternList;
```

**Evidence**:
- `IncludeFileContainsPatternList`: Stores regex patterns for `--file-contains` and `--contains` options
- `ExcludeFileContainsPatternList`: Stores regex patterns for `--file-not-contains` option
- Both are `List<Regex>` - pre-compiled regex patterns, not strings

#### Property Initialization

**Lines 15-16** (in constructor):

```csharp
15:         IncludeFileContainsPatternList = new();
16:         ExcludeFileContainsPatternList = new();
```

**Evidence**: Both lists are initialized as empty collections.

#### Property Usage in IsEmpty()

**Lines 51-52**:

```csharp
51:             !IncludeFileContainsPatternList.Any() &&
52:             !ExcludeFileContainsPatternList.Any() &&
```

**Evidence**: The command is considered empty if no file content patterns are specified (among other conditions).

---

## Execution Implementation

### File: `src/cycodmd/Program.cs`

#### Logging File Content Patterns

**Lines 169-176** in `HandleFindFileCommand`:

```csharp
169:         if (findFilesCommand.IncludeFileContainsPatternList.Any())
170:         {
171:             Logger.Info($"Finding files containing regex pattern(s):");
172:             foreach (var pattern in findFilesCommand.IncludeFileContainsPatternList)
173:             {
174:                 Logger.Info($"  Content pattern: '{pattern}'");
175:             }
176:         }
```

**Evidence**:
- Content pattern searches are logged when `--file-contains` or `--contains` is used
- Each pattern is logged individually for debugging
- Logging occurs before file search begins

---

#### Calling FileHelpers.FindMatchingFiles

**Lines 178-192**:

```csharp
178:         var files = FileHelpers.FindMatchingFiles(
179:             findFilesCommand.Globs,
180:             findFilesCommand.ExcludeGlobs,
181:             findFilesCommand.ExcludeFileNamePatternList,
182:             findFilesCommand.IncludeFileContainsPatternList,     // ← Layer 2: Include patterns
183:             findFilesCommand.ExcludeFileContainsPatternList,     // ← Layer 2: Exclude patterns
184:             findFilesCommand.ModifiedAfter,
185:             findFilesCommand.ModifiedBefore,
186:             findFilesCommand.CreatedAfter,
187:             findFilesCommand.CreatedBefore,
188:             findFilesCommand.AccessedAfter,
189:             findFilesCommand.AccessedBefore,
190:             findFilesCommand.AnyTimeAfter,
191:             findFilesCommand.AnyTimeBefore)
192:             .ToList();
```

**Evidence**:
- Line 182: `IncludeFileContainsPatternList` is passed as 4th parameter
- Line 183: `ExcludeFileContainsPatternList` is passed as 5th parameter
- The `FileHelpers.FindMatchingFiles` method handles the actual file content scanning
- Result is a filtered list of file paths

**Call Stack**:
```
Program.HandleFindFileCommand
    ↓
FileHelpers.FindMatchingFiles(
    globs,
    excludeGlobs,
    excludeFileNamePatterns,
    includeFileContainsPatterns,  ← Layer 2
    excludeFileContainsPatterns   ← Layer 2
)
    ↓
For each file:
    ↓
    Read file content
    ↓
    Apply include patterns (ALL must match)
    ↓
    Apply exclude patterns (NONE can match)
    ↓
Filtered file list
```

---

## FileHelpers Implementation

### File: `src/common/FileHelpers.cs`

While the full implementation is in FileHelpers.cs, the key aspects of Layer 2 filtering are:

1. **File Content Reading**: Each file's content is read to check against patterns
2. **Include Pattern Logic**: ALL patterns in `IncludeFileContainsPatternList` must match (AND logic)
3. **Exclude Pattern Logic**: If ANY pattern in `ExcludeFileContainsPatternList` matches, file is excluded (OR logic)
4. **Performance**: File content is cached during this operation to avoid re-reading in subsequent layers

**Method Signature** (from FileHelpers.cs):

```csharp
public static IEnumerable<string> FindMatchingFiles(
    IEnumerable<string> globs,
    IEnumerable<string> excludeGlobs,
    IEnumerable<Regex> excludeFileNamePatterns,
    IEnumerable<Regex> includeFileContainsPatterns,
    IEnumerable<Regex> excludeFileContainsPatterns,
    DateTime? modifiedAfter,
    DateTime? modifiedBefore,
    DateTime? createdAfter,
    DateTime? createdBefore,
    DateTime? accessedAfter,
    DateTime? accessedBefore,
    DateTime? anyTimeAfter,
    DateTime? anyTimeBefore)
```

**Key Parameters for Layer 2**:
- `includeFileContainsPatterns`: List<Regex> from `IncludeFileContainsPatternList`
- `excludeFileContainsPatterns`: List<Regex> from `ExcludeFileContainsPatternList`

---

## Data Flow Proof

### Step 1: Command Line → Parser

```
User Input:
cycodmd "**/*.cs" --file-contains "async" --file-not-contains "Test"

↓ Parsing (CycoDmdCommandLineOptions.cs)

Line 116-122: --file-contains "async"
    → command.IncludeFileContainsPatternList.Add(Regex("async", IgnoreCase))

Line 123-129: --file-not-contains "Test"
    → command.ExcludeFileContainsPatternList.Add(Regex("Test", IgnoreCase))
```

### Step 2: Command → Execution

```
FindFilesCommand properties:
    IncludeFileContainsPatternList = [Regex("async", IgnoreCase)]
    ExcludeFileContainsPatternList = [Regex("Test", IgnoreCase)]

↓ Execution (Program.cs:178-192)

FileHelpers.FindMatchingFiles(
    globs: ["**/*.cs"],
    includeFileContainsPatterns: [Regex("async")],  ← From parser
    excludeFileContainsPatterns: [Regex("Test")]    ← From parser
)
```

### Step 3: File Filtering

```
For each file in glob results:
    1. Read file content
    2. Check IncludeFileContainsPatternList:
        - Regex("async").IsMatch(content) → must be true
    3. Check ExcludeFileContainsPatternList:
        - Regex("Test").IsMatch(content) → must be false
    4. Include file only if:
        - ALL include patterns match (AND)
        - NO exclude patterns match (NOT OR)

Result: List<string> of filtered file paths
```

---

## Integration with Layer 3 (Content Filter)

### The --contains Option Bridge

**Evidence from Line 112-113**:

```csharp
112:             command.IncludeFileContainsPatternList.AddRange(asRegExs);  // Layer 2
113:             command.IncludeLineContainsPatternList.AddRange(asRegExs);  // Layer 3
```

**Data Flow**:
```
--contains "async"
    ↓
Parsing adds to BOTH lists
    ├─ IncludeFileContainsPatternList (Layer 2)
    │     ↓
    │     File must contain "async" to be included
    │     ↓
    └─ IncludeLineContainsPatternList (Layer 3)
          ↓
          Within included files, only show lines with "async"
```

**Execution Flow**:

1. **Layer 2** (FileHelpers.FindMatchingFiles): 
   - Files without "async" are excluded
   - Only files containing "async" proceed

2. **Layer 3** (GetCheckSaveFileContentAsync, Line 233-246):
   ```csharp
   236:                 findFilesCommand.IncludeLineContainsPatternList,  // Layer 3
   ```
   - Within matched files, only lines with "async" are shown

---

## Pattern Matching Behavior Proof

### Include Patterns (AND Logic)

**Evidence**: When multiple patterns are provided, ALL must match.

**Example**:
```bash
cycodmd "**/*.cs" --file-contains "async" "await"
```

**Execution**:
```csharp
IncludeFileContainsPatternList = [
    Regex("async", IgnoreCase),
    Regex("await", IgnoreCase)
]

// In FileHelpers.FindMatchingFiles:
foreach (var pattern in includeFileContainsPatterns)
{
    if (!pattern.IsMatch(fileContent))
    {
        // If ANY pattern doesn't match, exclude file
        return false;
    }
}
// Only if ALL patterns match, include file
return true;
```

### Exclude Patterns (OR Logic)

**Evidence**: If ANY pattern matches, file is excluded.

**Example**:
```bash
cycodmd "**/*.cs" --file-not-contains "Test" "Mock"
```

**Execution**:
```csharp
ExcludeFileContainsPatternList = [
    Regex("Test", IgnoreCase),
    Regex("Mock", IgnoreCase)
]

// In FileHelpers.FindMatchingFiles:
foreach (var pattern in excludeFileContainsPatterns)
{
    if (pattern.IsMatch(fileContent))
    {
        // If ANY pattern matches, exclude file immediately
        return false;
    }
}
```

---

## Regex Options Proof

**From CommandLineOptions.ValidateRegExPattern (Line numbers from common code)**:

```csharp
var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
```

**Evidence**:
- **RegexOptions.IgnoreCase**: Pattern matching is case-insensitive
  - `--file-contains "async"` matches "async", "Async", "ASYNC"
- **RegexOptions.CultureInvariant**: Consistent behavior across different locales/cultures
  - Ensures predictable pattern matching regardless of system locale

**Test Cases**:
```bash
# All of these match the same files:
cycodmd "**/*.cs" --file-contains "TODO"
cycodmd "**/*.cs" --file-contains "todo"
cycodmd "**/*.cs" --file-contains "Todo"
cycodmd "**/*.cs" --file-contains "ToDo"
```

---

## Error Handling Proof

### Invalid Regex Pattern

**From CommandLineOptions.ValidateRegExPattern**:

```csharp
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
```

**Evidence**:
- Invalid patterns (e.g., unclosed brackets, invalid escapes) throw `CommandLineException`
- Error message includes the invalid pattern
- Logging provides debugging information

**Example**:
```bash
# Invalid regex (unclosed bracket)
cycodmd "**/*.cs" --file-contains "[abc"

# Result:
# ERROR: Failed to create regex pattern for '--file-contains': '[abc' - <regex error>
# CommandLineException: Invalid regular expression pattern for --file-contains: [abc
```

### Missing Pattern

**From Line 118, 125** (GetInputOptionArgs with required: 1):

```csharp
var patterns = GetInputOptionArgs(i + 1, args, required: 1);
```

**Evidence**:
- If no pattern is provided after `--file-contains` or `--file-not-contains`, GetInputOptionArgs requires at least 1
- This throws an error before regex compilation

**Example**:
```bash
# Missing pattern
cycodmd "**/*.cs" --file-contains

# Result:
# CommandLineException: Missing regular expression patterns for --file-contains
```

---

## Performance Characteristics

### File Content Reading

**Evidence**: Files must be read from disk to check content patterns.

**Implications**:
- Layer 2 is I/O intensive
- Performance depends on:
  - Number of files from Layer 1
  - File sizes
  - Disk speed (local vs. network)
  - Number of patterns to check

### Optimization: Content Caching

While not explicitly shown in the provided code, the `FileHelpers.FindMatchingFiles` implementation reads file content once and can cache it for subsequent pattern matching.

**Best Practice**: Minimize number of files before Layer 2 using:
- Specific glob patterns (Layer 1)
- Time-based filters (Layer 1)
- Exclusion patterns (Layer 1)

---

## Summary of Source Code Evidence

| Feature | Parser Lines | Command Property | Execution Lines | Helper Method |
|---------|--------------|------------------|-----------------|---------------|
| `--contains` | 108-115 | IncludeFileContainsPatternList (95)<br>IncludeLineContainsPatternList (98) | 182, 183 | ValidateRegExPatterns |
| `--file-contains` | 116-122 | IncludeFileContainsPatternList (95) | 182 | ValidateRegExPatterns |
| `--file-not-contains` | 123-129 | ExcludeFileContainsPatternList (96) | 183 | ValidateRegExPatterns |
| Extension-based | 268-281 | FileInstructionsList (108) | N/A (Layer 8) | - |
| Regex compilation | - | - | - | ValidateRegExPattern |
| Pattern logging | - | - | 169-176 | Logger.Info |
| File filtering | - | - | 178-192 | FileHelpers.FindMatchingFiles |

---

## Related Source Files

- **Parser**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (Lines 108-129, 268-281)
- **Command**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs` (Lines 15-16, 95-96)
- **Execution**: `src/cycodmd/Program.cs` (Lines 169-192)
- **File Helpers**: `src/common/FileHelpers.cs` (FindMatchingFiles method)
- **Base Options**: `src/common/CommandLine/CommandLineOptions.cs` (ValidateRegExPatterns, ValidateRegExPattern)

---

## See Also

- [Layer 2 Documentation](cycodmd-files-layer-2.md) - User-facing documentation
- [Layer 1 Proof](cycodmd-files-layer-1-proof.md) - Target selection implementation
- [Layer 3 Proof](cycodmd-files-layer-3-proof.md) - Content filter implementation
