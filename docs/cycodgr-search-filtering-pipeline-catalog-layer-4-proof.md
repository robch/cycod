# cycodgr Search Command - Layer 4: Content Removal - PROOF

[← Back to Layer 4](cycodgr-search-filtering-pipeline-catalog-layer-4.md) | [↑ README](cycodgr-filtering-pipeline-catalog-README.md)

## Evidence: Source Code Analysis

This document provides **line-by-line evidence** for all Layer 4 (Content Removal) functionality in cycodgr.

---

## 1. Command Line Option: --exclude

### Parser Implementation

**File**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`

**Lines 341-350**: Parsing `--exclude` option
```csharp
else if (arg == "--exclude")
{
    var excludeArgs = GetInputOptionArgs(i + 1, args);
    if (excludeArgs.Count() == 0)
    {
        throw new CommandLineException($"Missing pattern(s) for {arg}");
    }
    command.Exclude.AddRange(excludeArgs);
    i += excludeArgs.Count();
}
```

**Analysis**:
- **Line 341**: Detects `--exclude` argument
- **Line 343**: Collects all subsequent non-option arguments as exclude patterns
- **Line 344-347**: Validates that at least one pattern is provided
- **Line 348**: Adds all patterns to the `command.Exclude` list
- **Line 349**: Advances the argument index past all consumed patterns

**Key Behavior**:
- Multiple patterns can be specified: `--exclude pattern1 pattern2 pattern3`
- Patterns are collected until the next option (starting with `--`) is encountered
- No limit on number of patterns

---

## 2. Property Storage

### Base Class: CycoGrCommand

**File**: `src/cycodgr/CommandLine/CycoGrCommand.cs`

**Lines 17 and 35**: Exclude property declaration
```csharp
// Line 17 (constructor)
Exclude = new List<string>();

// Line 35 (property)
public List<string> Exclude;
```

**Analysis**:
- **Line 17**: Initializes `Exclude` as empty list in constructor
- **Line 35**: Public field accessible to all code processing the command
- **Type**: `List<string>` - holds regex pattern strings

**Inheritance**:
- `SearchCommand` inherits from `CycoGrCommand`
- All search commands have access to `Exclude` property

---

## 3. Application Logic

### ApplyExcludeFilters Method

**File**: `src/cycodgr/Program.cs`

**Lines 1343-1377**: Core exclusion logic
```csharp
private static List<T> ApplyExcludeFilters<T>(List<T> items, List<string> excludePatterns, Func<T, string> urlGetter)
{
    if (excludePatterns.Count == 0)
    {
        return items;
    }

    var filtered = items.Where(item =>
    {
        var url = urlGetter(item);
        foreach (var pattern in excludePatterns)
        {
            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(url, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return false; // Exclude this item
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Invalid regex pattern '{pattern}': {ex.Message}");
            }
        }
        return true; // Keep this item
    }).ToList();

    var excludedCount = items.Count - filtered.Count;
    if (excludedCount > 0)
    {
        ConsoleHelpers.WriteLine($"Excluded {excludedCount} result(s) matching exclude pattern(s)", ConsoleColor.Yellow);
    }

    return filtered;
}
```

**Analysis**:

**Lines 1343**: Generic method signature
- Type parameter `<T>`: Works with any item type (RepoInfo, CodeMatch, etc.)
- Parameters:
  - `items`: List of items to filter
  - `excludePatterns`: List of regex patterns
  - `urlGetter`: Function to extract URL from each item

**Lines 1345-1347**: Early exit optimization
- If no exclude patterns specified, return items unchanged
- Avoids unnecessary processing

**Lines 1349-1365**: Filtering logic using LINQ
- `items.Where(item => ...)`: Filters items based on predicate
- **Line 1351**: Extract URL from current item using provided function
- **Line 1352**: Loop through each exclude pattern
- **Lines 1354-1357**: Test pattern against URL
  - `Regex.IsMatch()`: Performs regex matching
  - `RegexOptions.IgnoreCase`: Case-insensitive matching
  - Returns `false` to EXCLUDE item if pattern matches
- **Lines 1359-1362**: Error handling for invalid regex patterns
- **Line 1364**: Return `true` to KEEP item if no patterns match
- **Line 1365**: Convert filtered sequence to List

**Lines 1367-1371**: User feedback
- Calculate number of excluded items
- Display message if any items were excluded
- Yellow color indicates filtering action

**Lines 1373**: Return filtered list

**Key Characteristics**:
- **Generic**: Works with any item type
- **Lazy evaluation**: Uses LINQ for efficient filtering
- **Short-circuit**: Excludes on first matching pattern
- **Error-tolerant**: Invalid regex patterns log warning but don't crash
- **User-friendly**: Reports exclusion count

---

## 4. Usage in Unified Search

**File**: `src/cycodgr/Program.cs`

**Lines 267-268**: Apply exclude filters to repos
```csharp
// Apply exclude filters
repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);
codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);
```

**Context**: Inside `HandleUnifiedSearchAsync()` method (lines 230-297)

**Analysis**:
- **Line 267**: Filter repositories
  - `repos`: List<RepoInfo> from repository search
  - `command.Exclude`: Exclude patterns from command line
  - `r => r.Url`: Lambda to extract URL from RepoInfo
  - Filters out repositories whose URL matches any exclude pattern

- **Line 268**: Filter code matches
  - `codeMatches`: List<CodeMatch> from code search
  - `m => m.Repository.Url`: Lambda to extract repository URL from CodeMatch
  - Filters out code matches from repositories matching any exclude pattern
  - **Important**: Filters by REPOSITORY URL, not individual file URLs

**Execution Flow**:
1. Repository search completes → `repos` populated
2. Code search completes → `codeMatches` populated
3. Apply exclusion to repos (line 267)
4. Apply exclusion to code matches (line 268)
5. Check if results empty (lines 270-274)
6. Display remaining results (lines 276-287)

---

## 5. Usage in Repository Search

**File**: `src/cycodgr/Program.cs`

**Lines 327-334**: Apply exclude filters in repo search
```csharp
// Apply exclude filters
repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);

if (repos.Count == 0)
{
    ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow, overrideQuiet: true);
    return;
}
```

**Context**: Inside `HandleRepoSearchAsync()` method (lines 299-369)

**Analysis**:
- **Line 328**: Apply exclusion filters to repository search results
- **Lines 330-334**: Handle case where all results are excluded
  - Display "No results after filtering" message
  - Early return to avoid processing empty results

**Execution Flow**:
1. Repository search completes → `repos` populated (line 309-319)
2. Check if any results (lines 321-325)
3. Apply exclusion (line 328)
4. Check if filtering removed all results (lines 330-334)
5. Display/save remaining results (lines 337-368)

---

## 6. Usage in Code Search

**File**: `src/cycodgr/Program.cs`

**Lines 400-415**: Apply exclude filters in code search
```csharp
// Apply exclude filters (filter by repo URL)
codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

// Apply file paths filter if specified
if (command.FilePaths.Any())
{
    codeMatches = codeMatches
        .Where(m => command.FilePaths.Any(fp => m.Path == fp || m.Path.EndsWith(fp) || m.Path.Contains(fp)))
        .ToList();
}

if (codeMatches.Count == 0)
{
    ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow, overrideQuiet: true);
    return;
}
```

**Context**: Inside `HandleCodeSearchAsync()` method (lines 371-436)

**Analysis**:
- **Line 401**: Apply exclusion by repository URL
  - Comment clarifies: "filter by repo URL"
  - `m => m.Repository.Url`: Extracts repository URL from each CodeMatch
  - Filters out files from excluded repositories

- **Lines 404-409**: Additional file path filtering (Layer 2 feature)
  - This is separate from exclusion (Layer 4)
  - Positive filtering (include specific paths) vs negative filtering (exclude patterns)

- **Lines 411-415**: Handle empty results after filtering

**Execution Flow**:
1. Code search completes → `codeMatches` populated (lines 385-392)
2. Check if any results (lines 394-398)
3. Apply URL exclusion (line 401)
4. Apply file path inclusion filter (lines 404-409)
5. Check if filtering removed all results (lines 411-415)
6. Display/save remaining results (lines 417-435)

---

## 7. Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ Command Line Parsing                                         │
│ CycoGrCommandLineOptions.cs, lines 341-350                  │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓ --exclude <patterns>
┌─────────────────────────────────────────────────────────────┐
│ Property Storage                                             │
│ CycoGrCommand.Exclude (List<string>)                        │
│ CycoGrCommand.cs, lines 17, 35                              │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓ command.Exclude passed to filter method
┌─────────────────────────────────────────────────────────────┐
│ Execution Context                                            │
│ Program.cs - HandleUnifiedSearchAsync() OR                  │
│             HandleRepoSearchAsync() OR                       │
│             HandleCodeSearchAsync()                          │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓ Search results + exclude patterns
┌─────────────────────────────────────────────────────────────┐
│ ApplyExcludeFilters<T>() Method                             │
│ Program.cs, lines 1343-1377                                 │
│                                                              │
│ For each item:                                               │
│   1. Extract URL via urlGetter lambda                        │
│   2. Test against each exclude pattern (regex, case-insens) │
│   3. Exclude if ANY pattern matches                         │
│   4. Keep if NO patterns match                              │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓ Filtered results
┌─────────────────────────────────────────────────────────────┐
│ User Feedback                                                │
│ "Excluded N result(s) matching exclude pattern(s)"          │
│ (Yellow text)                                                │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────────┐
│ Next Layers                                                  │
│ Layer 5: Context Expansion                                   │
│ Layer 6: Display Control                                     │
│ Layer 7: Output Persistence                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. URL Getter Lambdas

The `ApplyExcludeFilters()` method uses different URL extraction functions depending on the data type:

### For RepoInfo (Repository Search)

**Location**: Program.cs, line 267, 328

**Lambda**: `r => r.Url`

**Meaning**: Extract the `Url` property from `RepoInfo` object

**Example**:
```csharp
repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);
```

**Result**: Filters repositories directly by their URL


### For CodeMatch (Code Search)

**Location**: Program.cs, line 268, 401

**Lambda**: `m => m.Repository.Url`

**Meaning**: Extract the `Url` property from the `Repository` property of `CodeMatch` object

**Example**:
```csharp
codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);
```

**Result**: Filters code matches by their containing repository's URL

**Key Insight**: Code matches are excluded based on repository URL, not individual file URLs

---

## 9. Regex Matching Details

**File**: Program.cs, line 1355

**Code**:
```csharp
if (System.Text.RegularExpressions.Regex.IsMatch(url, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
```

**Regex Options**:
- `RegexOptions.IgnoreCase`: Case-insensitive matching

**Examples**:

| Pattern | URL | Match? |
|---------|-----|--------|
| `"microsoft"` | `https://github.com/Microsoft/terminal` | ✅ Yes (case-insensitive) |
| `"fork"` | `https://github.com/user/repo-fork` | ✅ Yes (substring match) |
| `"^https://github\.com/test"` | `https://github.com/test/repo` | ✅ Yes (regex anchor) |
| `"archived\|deprecated"` | `https://github.com/old/archived-repo` | ✅ Yes (regex OR) |
| `"example\.com"` | `https://github.com/example/repo` | ❌ No (dot requires escape) |

**Important**: Patterns are regex, not glob patterns. Special regex characters must be escaped.

---

## 10. Error Handling

**File**: Program.cs, lines 1359-1362

**Code**:
```csharp
catch (Exception ex)
{
    Logger.Warning($"Invalid regex pattern '{pattern}': {ex.Message}");
}
```

**Behavior**:
- If a regex pattern is malformed, logs a warning
- Does NOT throw exception or crash
- Continues processing other patterns
- Invalid pattern is silently skipped for current item

**Example Scenario**:
```bash
# Invalid regex: unclosed bracket
cycodgr --repo-contains "test" --exclude "[invalid"
```

**Result**:
- Warning logged: "Invalid regex pattern '[invalid': <error message>"
- Search continues with other patterns (if any)
- Items are kept (not excluded) if only invalid patterns specified

---

## 11. Integration Testing Evidence

### Example 1: Exclude in Unified Search

**Command**:
```bash
cycodgr --contains "terminal" --exclude "microsoft"
```

**Execution Path**:
1. Parse `--contains "terminal"` → `command.Contains = "terminal"`
2. Parse `--exclude "microsoft"` → `command.Exclude = ["microsoft"]`
3. Execute `HandleUnifiedSearchAsync()` (Program.cs, line 230)
4. Search repos (line 240) → returns repos including microsoft/terminal
5. Search code (line 252) → returns code matches including from microsoft repos
6. **Apply exclusion** (lines 267-268):
   - Filter repos: removes `https://github.com/microsoft/terminal`
   - Filter code matches: removes all matches from microsoft repos
7. Display remaining results (lines 276-287)

### Example 2: Exclude in Repo Search

**Command**:
```bash
cycodgr --repo-contains "rust" --exclude "fork|archived"
```

**Execution Path**:
1. Parse `--repo-contains "rust"` → `command.RepoContains = "rust"`
2. Parse `--exclude "fork|archived"` → `command.Exclude = ["fork|archived"]`
3. Execute `HandleRepoSearchAsync()` (Program.cs, line 299)
4. Search repos (lines 309-319) → returns repos related to rust
5. **Apply exclusion** (line 328):
   - Regex matches URLs containing "fork" OR "archived"
   - Removes matching repos
6. Display remaining results (line 337)

### Example 3: Exclude in Code Search

**Command**:
```bash
cycodgr microsoft/terminal wezterm/wezterm --file-contains "ConPTY" --exclude "test"
```

**Execution Path**:
1. Parse positional args → `command.RepoPatterns = ["microsoft/terminal", "wezterm/wezterm"]`
2. Parse `--file-contains "ConPTY"` → `command.FileContains = "ConPTY"`
3. Parse `--exclude "test"` → `command.Exclude = ["test"]`
4. Execute `HandleCodeSearchAsync()` (Program.cs, line 371)
5. Combine repo patterns with repos list (line 383)
6. Search code (lines 385-392) → returns code matches containing "ConPTY"
7. **Apply exclusion** (line 401):
   - Extracts repository URL from each code match
   - Removes matches from repos with "test" in URL
   - Example: `https://github.com/microsoft/terminal-test` would be excluded
8. Display remaining results (line 418)

---

## 12. Performance Characteristics

### Time Complexity
- **O(n * m)** where:
  - n = number of items (repos or code matches)
  - m = number of exclude patterns

### Space Complexity
- **O(n)** for filtered list (worst case: no items excluded)

### Optimizations
1. **Early exit**: Returns original list if no patterns specified (line 1345)
2. **Short-circuit**: Excludes on first matching pattern, doesn't test remaining patterns
3. **Lazy evaluation**: Uses LINQ `Where()` for deferred execution
4. **Efficient storage**: Reuses same List<string> for all exclusion calls

---

## 13. Limitations and Edge Cases

### Limitation 1: URL-Only Filtering
- **What it is**: Can only filter based on URLs
- **Impact**: Cannot exclude based on:
  - Star count (e.g., "exclude repos with < 100 stars")
  - Language (e.g., "exclude JavaScript repos")
  - Date (e.g., "exclude repos not updated in 2 years")
  - Description content

### Limitation 2: No Line-Level Removal
- **What it is**: Cannot remove specific lines from code display
- **Comparison**: cycodmd has `--remove-all-lines` for this
- **Impact**: Cannot hide common boilerplate like:
  - License headers
  - Import statements
  - Generated code markers

### Limitation 3: Repository-Level Only for Code Search
- **What it is**: Code matches filtered by repository URL, not file URL
- **Impact**: Cannot exclude specific files while keeping repository
- **Example**:
  ```bash
  # Cannot exclude test files while keeping repo
  cycodgr --file-contains "async" --exclude "test"
  # This excludes entire repos with "test" in URL, not just test files
  ```

### Limitation 4: No Positive Exclusion
- **What it is**: Cannot express "keep only these, exclude all others"
- **Example**: Cannot do "show only microsoft repos" via exclusion
- **Workaround**: Use Layer 1 (target selection) with specific repo patterns

### Edge Case 1: Empty Results
- **Scenario**: All results excluded
- **Handling**: Display "No results after filtering" message (lines 330-334, 411-415)
- **User experience**: Clear feedback about why no results shown

### Edge Case 2: Invalid Regex
- **Scenario**: User provides malformed regex pattern
- **Handling**: Log warning, continue processing (lines 1359-1362)
- **User experience**: Warning in log, pattern ignored

### Edge Case 3: Case Sensitivity
- **Behavior**: Always case-insensitive
- **Impact**: Cannot distinguish `Microsoft` from `microsoft`
- **Workaround**: None available

---

## Summary

Layer 4 (Content Removal) in cycodgr is implemented through:

1. **Single option**: `--exclude <pattern> [<pattern> ...]`
2. **Storage**: `CycoGrCommand.Exclude` (List<string>)
3. **Application**: `ApplyExcludeFilters<T>()` generic method
4. **Matching**: Regex, case-insensitive, against URLs
5. **Context**: Applied to repository URLs (repos) or repository URLs of code matches
6. **Timing**: After search results retrieved, before display
7. **Feedback**: Reports count of excluded items

**Key Source Locations**:
- Parser: `CycoGrCommandLineOptions.cs`, lines 341-350
- Storage: `CycoGrCommand.cs`, lines 17, 35
- Application: `Program.cs`, lines 1343-1377
- Usage: `Program.cs`, lines 267-268, 328, 401

---

[← Back to Layer 4](cycodgr-search-filtering-pipeline-catalog-layer-4.md) | [↑ README](cycodgr-filtering-pipeline-catalog-README.md)
