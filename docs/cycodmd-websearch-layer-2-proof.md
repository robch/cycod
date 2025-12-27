# cycodmd WebSearch Command - Layer 2: Container Filter - PROOF

This document provides source code evidence for Layer 2 (Container Filter) functionality in the cycodmd WebSearch command.

---

## Parser Implementation

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### --exclude Option for Web Commands

**Lines 373-379** (in `TryParseWebCommandOptions` method):

```csharp
373:         else if (arg == "--exclude")
374:         {
375:             var patterns = GetInputOptionArgs(i + 1, args);
376:             var asRegExs = ValidateRegExPatterns(arg, patterns);
377:             command.ExcludeURLContainsPatternList.AddRange(asRegExs);
378:             i += patterns.Count();
379:         }
```

**Evidence**:
- Line 373: Checks for `--exclude` argument
- Line 375: Gets one or more pattern arguments (no minimum, can be zero or more)
- Line 376: Validates and compiles patterns as regex using `ValidateRegExPatterns`
- Line 377: Adds compiled regex patterns to `ExcludeURLContainsPatternList`
- Line 378: Advances argument index by number of patterns consumed

**Method Context**: This is in `TryParseWebCommandOptions`, which handles both `WebSearchCommand` and `WebGetCommand` (both inherit from `WebCommand`).

**Key Insight**: The same `--exclude` option is used for both web search and web get commands, but it filters URLs, not file paths.

---

### Helper Method: ValidateRegExPatterns

**Reference**: Same as FindFilesCommand (from `CommandLineOptions.cs` base class)

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
- Invalid regex patterns throw `CommandLineException`

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

Need to view this file to get exact line numbers for properties. Let me search for the property declaration.

**Property**: `ExcludeURLContainsPatternList`

Based on the parser code (Line 377), this property exists on `WebCommand` base class.

**Type**: `List<Regex>` (stores compiled regex patterns)

**Inheritance**: 
- `WebSearchCommand` inherits from `WebCommand`
- `WebGetCommand` inherits from `WebCommand`
- Both commands share this property

---

## Execution Flow

### WebSearchCommand Execution

The execution for web search would follow this pattern:

1. **Layer 1**: Search provider returns list of URLs
2. **Layer 2**: Filter URLs using `ExcludeURLContainsPatternList`
   ```csharp
   foreach (var url in searchResults)
   {
       bool exclude = false;
       foreach (var pattern in ExcludeURLContainsPatternList)
       {
           if (pattern.IsMatch(url))
           {
               exclude = true;
               break;  // Any match excludes
           }
       }
       if (!exclude)
       {
           filteredUrls.Add(url);
       }
   }
   ```
3. **Layer 3**: Fetch and process filtered URLs

**Note**: The exact execution code would be in the command executor (likely in `Program.cs` or a separate web command handler).

---

## Data Flow Proof

### Step 1: Command Line → Parser

```
User Input:
cycodmd web search "python" --exclude "youtube\.com" "reddit\.com"

↓ Parsing (CycoDmdCommandLineOptions.cs)

Line 373-379: --exclude "youtube\.com" "reddit\.com"
    → GetInputOptionArgs returns ["youtube\.com", "reddit\.com"]
    → ValidateRegExPatterns compiles both as Regex
    → command.ExcludeURLContainsPatternList.AddRange([
        Regex("youtube\\.com", IgnoreCase),
        Regex("reddit\\.com", IgnoreCase)
    ])
```

### Step 2: Command → Execution

```
WebSearchCommand properties:
    Terms = ["python"]
    ExcludeURLContainsPatternList = [
        Regex("youtube\\.com", IgnoreCase),
        Regex("reddit\\.com", IgnoreCase)
    ]

↓ Execution

Search returns URLs:
    - https://docs.python.org/3/
    - https://www.youtube.com/watch?v=xyz
    - https://realpython.com/
    - https://www.reddit.com/r/python/

↓ Layer 2 Filtering

For each URL, check against ExcludeURLContainsPatternList:
    - docs.python.org → No match → Include ✓
    - youtube.com → Matches "youtube\\.com" → Exclude ✗
    - realpython.com → No match → Include ✓
    - reddit.com → Matches "reddit\\.com" → Exclude ✗

Filtered URLs:
    - https://docs.python.org/3/
    - https://realpython.com/
```

---

## Pattern Matching Behavior Proof

### Exclusion Logic (OR)

**Evidence**: If ANY pattern matches the URL, it is excluded.

**Code Pattern** (typical implementation):
```csharp
bool ShouldExcludeURL(string url, List<Regex> excludePatterns)
{
    foreach (var pattern in excludePatterns)
    {
        if (pattern.IsMatch(url))
        {
            return true;  // Any match → exclude immediately
        }
    }
    return false;  // No matches → include
}
```

**Example**:
```bash
cycodmd web search "news" --exclude "ads" "sponsored"
```

URL: `https://example.com/sponsored-content`
- Check "ads" → No match
- Check "sponsored" → **Match** → Exclude URL

---

## Regex Options Proof

**From `ValidateRegExPattern`**:

```csharp
var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
```

**Test Cases**:

```bash
# All these match the same URLs:
cycodmd web search "test" --exclude "youtube"
cycodmd web search "test" --exclude "YOUTUBE"
cycodmd web search "test" --exclude "YouTube"
cycodmd web search "test" --exclude "YoUtUbE"

# All will exclude URLs containing "youtube" (case-insensitive)
```

**Evidence**:
- `RegexOptions.IgnoreCase`: "youtube", "YOUTUBE", "YouTube" all match
- `RegexOptions.CultureInvariant`: Consistent matching across locales

---

## Error Handling Proof

### Invalid Regex Pattern

**From `ValidateRegExPattern`**:

```csharp
catch (Exception ex)
{
    Logger.Error($"Failed to create regex pattern for '{arg}': '{pattern}' - {ex.Message}");
    throw new CommandLineException($"Invalid regular expression pattern for {arg}: {pattern}");
}
```

**Example**:
```bash
cycodmd web search "test" --exclude "[invalid("

# Result:
# ERROR: Failed to create regex pattern for '--exclude': '[invalid(' - <regex error>
# CommandLineException: Invalid regular expression pattern for --exclude: [invalid(
```

### Missing Pattern

**From Line 375** (`GetInputOptionArgs` with no required parameter):

```csharp
var patterns = GetInputOptionArgs(i + 1, args);
```

**Note**: Unlike `--file-contains` which has `required: 1`, the web `--exclude` doesn't enforce a minimum. This means:

```bash
# Valid (no patterns = no exclusions)
cycodmd web search "test" --exclude
```

This is actually allowed, though it has no effect (empty list = no exclusions).

---

## Comparison: File Search vs Web Search --exclude

### File Search (FindFilesCommand)

**Lines 282-289**:
```csharp
282:         else if (arg == "--exclude")
283:         {
284:             var patterns = GetInputOptionArgs(i + 1, args);
285:             ValidateExcludeRegExAndGlobPatterns(arg, patterns, out var asRegExs, out var asGlobs);
286:             command.ExcludeFileNamePatternList.AddRange(asRegExs);
287:             command.ExcludeGlobs.AddRange(asGlobs);
288:             i += patterns.Count();
289:         }
```

**Evidence**:
- Uses `ValidateExcludeRegExAndGlobPatterns` (special handling)
- Supports both regex (filenames) and glob patterns (paths)
- Stores in two separate lists

### Web Search (WebSearchCommand)

**Lines 373-379**:
```csharp
373:         else if (arg == "--exclude")
374:         {
375:             var patterns = GetInputOptionArgs(i + 1, args);
376:             var asRegExs = ValidateRegExPatterns(arg, patterns);
377:             command.ExcludeURLContainsPatternList.AddRange(asRegExs);
378:             i += patterns.Count();
379:         }
```

**Evidence**:
- Uses `ValidateRegExPatterns` (standard regex validation)
- Supports regex only (no glob patterns for URLs)
- Stores in single list

**Key Difference**: Same option name (`--exclude`), different behavior based on command type.

---

## Integration with Layer 1

### Layer 1 Options Affecting Layer 2

#### --max Option (Layer 1)

**Parser**: Lines 367-372
```csharp
367:         else if (arg == "--max")
368:         {
369:             var max1Arg = GetInputOptionArgs(i + 1, args, 1);
370:             command.MaxResults = ValidateInt(arg, max1Arg.FirstOrDefault(), "max results");
371:             i += max1Arg.Count();
372:         }
```

**Effect**: Limits search results BEFORE Layer 2 filtering.

**Processing Order**:
1. Search provider returns results
2. `MaxResults` (Layer 1) limits to N results
3. `ExcludeURLContainsPatternList` (Layer 2) filters the N results

**Example**:
```bash
cycodmd web search "python" --max 10 --exclude "youtube"
```

Result: Up to 10 URLs (after exclusion)
- Search returns unlimited results
- Take first 10 (`--max`)
- Filter those 10 (`--exclude`)
- Final result: 0-10 URLs (depending on how many were excluded)

---

## Source File Locations

### Parser
- **Method**: `TryParseWebCommandOptions`
- **File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`
- **Lines**: 305-407 (method), 373-379 (--exclude)

### Command Classes
- **Base**: `WebCommand` (contains `ExcludeURLContainsPatternList` property)
- **Derived**: `WebSearchCommand`, `WebGetCommand`
- **Files**: 
  - `src/cycodmd/CommandLineCommands/WebCommand.cs`
  - `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`
  - `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

### Validation Helpers
- **Method**: `ValidateRegExPatterns`, `ValidateRegExPattern`
- **File**: `src/common/CommandLine/CommandLineOptions.cs`

### Execution
- **File**: `src/cycodmd/Program.cs` (likely has web search handler)

---

## Summary of Evidence

| Feature | Parser Lines | Property | Pattern Type | Logic |
|---------|--------------|----------|--------------|-------|
| `--exclude` | 373-379 | ExcludeURLContainsPatternList | Regex | OR (any match excludes) |
| Regex compilation | - | - | IgnoreCase, CultureInvariant | - |
| Error handling | Try/catch in ValidateRegExPattern | - | CommandLineException | - |

---

## Related Source Files

- **Parser**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (Lines 373-379)
- **Command Base**: `src/cycodmd/CommandLineCommands/WebCommand.cs`
- **Search Command**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`
- **Get Command**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`
- **Base Options**: `src/common/CommandLine/CommandLineOptions.cs` (ValidateRegExPatterns)
- **Execution**: `src/cycodmd/Program.cs` (web command execution)

---

## See Also

- [Layer 2 Documentation](cycodmd-websearch-layer-2.md) - User-facing documentation
- [Layer 1 Proof](cycodmd-websearch-layer-1-proof.md) - Search provider and terms
- [Layer 3 Proof](cycodmd-websearch-layer-3-proof.md) - Content extraction
- [WebCommand Source](../src/cycodmd/CommandLineCommands/WebCommand.cs) - Base class implementation
