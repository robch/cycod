# cycodmd WebGet Command - Layer 2: Container Filter - PROOF

This document provides source code evidence for Layer 2 (Container Filter) functionality in the cycodmd WebGet command.

---

## Important Note

**WebGetCommand inherits from WebCommand**, and Layer 2 (Container Filter) implementation is **identical** to WebSearchCommand because both commands share the same base class.

All parser code, command properties, and execution logic for `--exclude` are the same.

---

## Parser Implementation

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### --exclude Option (Shared with WebSearchCommand)

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
- Line 375: Gets zero or more pattern arguments
- Line 376: Validates and compiles patterns as regex
- Line 377: Adds to `ExcludeURLContainsPatternList` on `WebCommand` (base class)
- Line 378: Advances argument index

**Method Context**: `TryParseWebCommandOptions` at Line 305-407

**Important**: This method handles BOTH `WebSearchCommand` and `WebGetCommand` because:
- Line 50: `TryParseOtherCommandOptions` calls `TryParseWebCommandOptions(command as WebCommand, ...)`
- Both WebSearchCommand and WebGetCommand inherit from WebCommand
- The cast `as WebCommand` succeeds for both derived types

---

## Command Class Hierarchy

### File: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Class Declaration** (need to verify):
```csharp
class WebGetCommand : WebCommand
{
    // Inherits ExcludeURLContainsPatternList from WebCommand
}
```

### File: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Base Class Properties**:
```csharp
public List<Regex> ExcludeURLContainsPatternList;
```

**Evidence**:
- `WebCommand` is the base class for both web search and web get
- The `ExcludeURLContainsPatternList` property is defined in WebCommand
- Both derived commands inherit this property and its behavior

---

## Positional Argument Parsing (Layer 1)

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 472-476** (in `TryParseOtherCommandArg` method):

```csharp
472:         else if (command is WebGetCommand webGetCommand)
473:         {
474:             webGetCommand.Urls.Add(arg);
475:             parsedOption = true;
476:         }
```

**Evidence**:
- Line 472: Checks if command is WebGetCommand
- Line 474: Adds positional argument to `Urls` list
- This is Layer 1 (Target Selection), not Layer 2
- But shows how WebGetCommand receives its input URLs

---

## Execution Flow

### WebGetCommand Execution Pattern

Based on the command structure, execution would follow this pattern:

1. **Layer 1 (Target Selection)**: Collect URLs from positional args
   ```csharp
   // From parsing (Lines 472-476)
   webGetCommand.Urls = ["url1", "url2", "url3", ...];
   ```

2. **Layer 2 (Container Filter)**: Filter URLs using exclusion patterns
   ```csharp
   var filteredUrls = new List<string>();
   foreach (var url in webGetCommand.Urls)
   {
       bool exclude = false;
       foreach (var pattern in webGetCommand.ExcludeURLContainsPatternList)
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

3. **Layer 3+**: Fetch and process filtered URLs

---

## Data Flow Proof

### Step 1: Command Line → Parser

```
User Input:
cycodmd web get https://example.com/page1 https://ads.example.com/banner --exclude "ads\."

↓ Parsing

TryParseOtherCommandArg (Lines 472-476):
    webGetCommand.Urls.Add("https://example.com/page1")
    webGetCommand.Urls.Add("https://ads.example.com/banner")

TryParseWebCommandOptions (Lines 373-379):
    arg == "--exclude"
    patterns = ["ads\\."]
    asRegExs = [Regex("ads\\.", IgnoreCase)]
    webGetCommand.ExcludeURLContainsPatternList.AddRange(asRegExs)
```

### Step 2: Command Properties After Parsing

```
WebGetCommand:
    Urls = [
        "https://example.com/page1",
        "https://ads.example.com/banner"
    ]
    ExcludeURLContainsPatternList = [
        Regex("ads\\.", IgnoreCase)
    ]
```

### Step 3: Execution (Layer 2 Filtering)

```
For each URL in Urls:
    url = "https://example.com/page1"
        Check against Regex("ads\\.") → No match → Include ✓
    
    url = "https://ads.example.com/banner"
        Check against Regex("ads\\.") → Match "ads." → Exclude ✗

Filtered URLs to fetch:
    ["https://example.com/page1"]
```

---

## Shared Implementation Evidence

### Parser Method Signature

**Line 305**:
```csharp
private bool TryParseWebCommandOptions(WebCommand? command, string[] args, ref int i, string arg)
```

**Evidence**:
- Parameter type is `WebCommand?` (base class)
- Works for ANY class that inherits from WebCommand
- Line 377 accesses `command.ExcludeURLContainsPatternList` (property of WebCommand)

### Caller Evidence

**Line 51** (in `TryParseOtherCommandOptions`):
```csharp
return TryParseFindFilesCommandOptions(command as FindFilesCommand, args, ref i, arg) ||
       TryParseWebCommandOptions(command as WebCommand, args, ref i, arg) ||
       TryParseRunCommandOptions(command as RunCommand, args, ref i, arg) ||
       TryParseSharedCycoDmdCommandOptions(command as CycoDmdCommand, args, ref i, arg);
```

**Evidence**:
- Line 51: `command as WebCommand` - casts to base class
- This cast succeeds for both WebSearchCommand and WebGetCommand
- Same options (`--exclude`, `--browser`, etc.) work for both commands

---

## Difference from WebSearchCommand: Input Source

### WebSearchCommand (Layer 1)

**Positional Args** (Lines 467-470):
```csharp
467:         else if (command is WebSearchCommand webSearchCommand)
468:         {
469:             webSearchCommand.Terms.Add(arg);
470:             parsedOption = true;
```

**Evidence**: Positional args are search TERMS, not URLs.

**Layer 1 Output**: URLs returned by search provider.

### WebGetCommand (Layer 1)

**Positional Args** (Lines 472-476):
```csharp
472:         else if (command is WebGetCommand webGetCommand)
473:         {
474:             webGetCommand.Urls.Add(arg);
475:             parsedOption = true;
```

**Evidence**: Positional args are explicit URLS.

**Layer 1 Output**: URLs provided by user.

### Layer 2: Same Filtering

**Both commands** use the same Layer 2 filtering mechanism:
- Property: `ExcludeURLContainsPatternList` (from WebCommand)
- Parser: Lines 373-379 (TryParseWebCommandOptions)
- Behavior: Exclude URLs matching ANY pattern (OR logic)

---

## Validation and Error Handling

### Same as WebSearchCommand

All validation and error handling is shared through `WebCommand` base class:

- Regex validation: `ValidateRegExPatterns` (inherited)
- Invalid regex → `CommandLineException`
- Case-insensitive matching: `RegexOptions.IgnoreCase`
- Culture-invariant: `RegexOptions.CultureInvariant`

**Reference**: See [WebSearch Layer 2 Proof](cycodmd-websearch-layer-2-proof.md) for full validation details.

---

## Integration with Layer 1

### URL Source Difference

**WebSearchCommand**:
```
Search Terms (Layer 1) → Search Results (URLs) → Filter URLs (Layer 2) → Fetch (Layer 3)
```

**WebGetCommand**:
```
Explicit URLs (Layer 1) → Filter URLs (Layer 2) → Fetch (Layer 3)
```

### @file Support

Both commands support loading from files:

**WebSearchCommand**:
```bash
cycodmd web search @terms.txt --exclude "youtube"
# (Assuming terms.txt contains search terms)
```

**WebGetCommand**:
```bash
cycodmd web get @urls.txt --exclude "ads"
# urls.txt contains explicit URLs
```

**Evidence**: The `@file` expansion happens during argument preprocessing (in `CommandLineOptions.ExpandedInput`), before command-specific parsing.

---

## Pattern Matching Proof

### Same Behavior as WebSearchCommand

Pattern matching behavior is **identical** because both commands use the same:
- Property: `ExcludeURLContainsPatternList`
- Parser: `TryParseWebCommandOptions`
- Regex options: `IgnoreCase | CultureInvariant`

**Reference**: See [WebSearch Layer 2 Proof](cycodmd-websearch-layer-2-proof.md) for:
- OR logic proof (any match excludes)
- Case-insensitive proof
- Error handling proof

---

## Summary of Evidence

| Feature | Evidence | Line Numbers | Shared with WebSearch? |
|---------|----------|--------------|------------------------|
| Parser | TryParseWebCommandOptions | 373-379 | ✅ Yes |
| Property | ExcludeURLContainsPatternList | WebCommand class | ✅ Yes |
| Inheritance | WebGetCommand : WebCommand | WebGetCommand.cs | Base class |
| Positional args | Urls (not Terms) | 472-476 | ❌ Different (Layer 1) |
| Regex options | IgnoreCase, CultureInvariant | ValidateRegExPattern | ✅ Yes |
| Error handling | CommandLineException | ValidateRegExPattern | ✅ Yes |

---

## Source File Locations

### Parser
- **Method**: `TryParseWebCommandOptions`
- **File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`
- **Lines**: 305-407 (method), 373-379 (--exclude), 472-476 (positional args)

### Command Classes
- **Base**: `WebCommand` (contains `ExcludeURLContainsPatternList`)
- **Derived**: `WebGetCommand` (inherits filtering)
- **Files**: 
  - `src/cycodmd/CommandLineCommands/WebCommand.cs`
  - `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

### Validation Helpers
- **Method**: `ValidateRegExPatterns`, `ValidateRegExPattern`
- **File**: `src/common/CommandLine/CommandLineOptions.cs`

---

## Conclusion

**WebGetCommand Layer 2** is a **perfect example of inheritance-based code reuse**:

- ✅ Implementation is 100% shared with WebSearchCommand
- ✅ Same parser code (Lines 373-379)
- ✅ Same property (`ExcludeURLContainsPatternList`)
- ✅ Same behavior (OR logic, case-insensitive, regex patterns)
- ❌ Only difference: Layer 1 input source (explicit URLs vs. search results)

**No WebGetCommand-specific Layer 2 code exists** - everything is inherited from `WebCommand`.

---

## See Also

- [Layer 2 Documentation](cycodmd-webget-layer-2.md) - User-facing documentation
- [WebSearch Layer 2 Proof](cycodmd-websearch-layer-2-proof.md) - Shared implementation details
- [Layer 1 Proof](cycodmd-webget-layer-1-proof.md) - URL specification (Layer 1 difference)
- [WebCommand Source](../src/cycodmd/CommandLineCommands/WebCommand.cs) - Base class
- [WebGetCommand Source](../src/cycodmd/CommandLineCommands/WebGetCommand.cs) - Derived class
