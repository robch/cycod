# cycodmd Web Search - Layer 1: Target Selection - PROOF

This document provides source code evidence for all Layer 1 (Target Selection) features of the cycodmd web search command.

---

## Positional Search Terms

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 467-470**:
```csharp
467:         else if (command is WebSearchCommand webSearchCommand)
468:         {
469:             webSearchCommand.Terms.Add(arg);
470:             parsedOption = true;
471:         }
```

**Explanation**: 
- Method `TryParseOtherCommandArg()` handles non-option arguments
- For `WebSearchCommand`, each argument is treated as a search term
- Allows multiple terms: `cycodmd web search term1 term2 term3`

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 6-9, 11**:
```csharp
6:     public WebSearchCommand()
7:     {
8:         Terms = new List<string>();
9:     }
10: 
11:     public List<string> Terms { get; set; }
```

**Explanation**: 
- `Terms` is initialized as an empty list in the constructor
- Stores all search terms provided by the user

### Empty Check

**File**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 18-21**:
```csharp
18:     override public bool IsEmpty()
19:     {
20:         return !Terms.Any();
21:     }
```

**Explanation**: 
- Command is considered empty if no search terms are provided
- Validation will fail if Terms is empty

---

## `--max` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 367-372**:
```csharp
367:         else if (arg == "--max")
368:         {
369:             var max1Arg = GetInputOptionArgs(i + 1, args, 1);
370:             command.MaxResults = ValidateInt(arg, max1Arg.FirstOrDefault(), "max results");
371:             i += max1Arg.Count();
372:         }
```

**Explanation**: 
- Reads exactly one argument after `--max`
- Validates it as an integer using `ValidateInt()`
- Sets the `MaxResults` property

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 11-12, 27**:
```csharp
11:         SearchProvider = WebSearchProvider.Google;
12:         MaxResults = 10;
...
27:     public int MaxResults { get; set; }
```

**Explanation**: 
- `MaxResults` defaults to 10 in the base `WebCommand` constructor
- Both `WebSearchCommand` and `WebGetCommand` inherit this default

---

## Search Provider Options

### `--google` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 347-350**:
```csharp
347:         else if (arg == "--google")
348:         {
349:             command.SearchProvider = WebSearchProvider.Google;
350:         }
```

**Explanation**: 
- Sets `SearchProvider` enum to `Google`
- Google is the default, so this option is usually redundant

---

### `--bing` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 339-342**:
```csharp
339:         else if (arg == "--bing")
340:         {
341:             command.SearchProvider = WebSearchProvider.Bing;
342:         }
```

---

### `--duck-duck-go`, `--duckduckgo` Options

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 343-346**:
```csharp
343:         else if (arg == "--duck-duck-go" || arg == "--duckduckgo")
344:         {
345:             command.SearchProvider = WebSearchProvider.DuckDuckGo;
346:         }
```

**Explanation**: 
- Two aliases for the same option (with and without hyphens)

---

### `--yahoo` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 351-354**:
```csharp
351:         else if (arg == "--yahoo")
352:         {
353:             command.SearchProvider = WebSearchProvider.Yahoo;
354:         }
```

---

### `--bing-api` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 355-358**:
```csharp
355:         else if (arg == "--bing-api")
356:         {
357:             command.SearchProvider = WebSearchProvider.BingAPI;
358:         }
```

**Explanation**: 
- Uses Bing's official API instead of web scraping
- Requires API key configuration

---

### `--google-api` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 359-362**:
```csharp
359:         else if (arg == "--google-api")
360:         {
361:             command.SearchProvider = WebSearchProvider.GoogleAPI;
362:         }
```

**Explanation**: 
- Uses Google Custom Search API instead of web scraping
- Requires API key configuration

---

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 11, 25**:
```csharp
11:         SearchProvider = WebSearchProvider.Google;
...
25:     public WebSearchProvider SearchProvider { get; set; }
```

**Explanation**: 
- `SearchProvider` is an enum property
- Defaults to `Google`

---

## `--exclude` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

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

**Explanation**: 
- Reads all arguments following `--exclude` (until next option or end of args)
- Validates each pattern as a regex using `ValidateRegExPatterns()`
- Adds compiled Regex objects to `ExcludeURLContainsPatternList`

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 14, 26**:
```csharp
14:         ExcludeURLContainsPatternList = new();
...
26:     public List<Regex> ExcludeURLContainsPatternList { get; set; }
```

**Explanation**: 
- Initialized as empty list in constructor
- Stores compiled regex patterns for URL filtering

---

## Special Validation Logic

### Auto-Enable Content Retrieval

**File**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs`

**Lines 23-36**:
```csharp
23:     override public CycoDmdCommand Validate()
24:     {
25:         var noContent = !GetContent;
26:         var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();
27: 
28:         var assumeGetAndStrip = noContent && hasInstructions;
29:         if (assumeGetAndStrip)
30:         {
31:             GetContent = true;
32:             StripHtml = true;
33:         }
34: 
35:         return this;
36:     }
```

**Explanation**: 
- Called after parsing, before execution
- If user provides AI instructions (`--instructions`, `--page-instructions`) but doesn't explicitly request content (`--get`)
- Automatically enables `GetContent = true` and `StripHtml = true`
- This is a convenience feature so users don't have to remember to add `--get --strip` when using AI processing

### Related Properties

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 17-18, 30-31**:
```csharp
17:         GetContent = false;
18:         StripHtml = false;
...
30:     public bool GetContent { get; set; }
31:     public bool StripHtml { get; set; }
```

**Explanation**: 
- `GetContent` controls whether page content is retrieved (vs. just URLs)
- `StripHtml` controls whether HTML tags are removed from content
- Both default to `false`

---

## Command Inheritance Hierarchy

### WebSearchCommand Inheritance

```
Command (base class)
    ↓
CycoDmdCommand (cycodmd-specific base)
    ↓
WebCommand (web-specific base)
    ↓
WebSearchCommand (search-specific)
```

### Properties Inherited from WebCommand

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 8-21**:
```csharp
8:     public WebCommand()
9:     {
10:         Interactive = false;
11: 
12:         SearchProvider = WebSearchProvider.Google;
13:         MaxResults = 10;
14: 
15:         ExcludeURLContainsPatternList = new();
16: 
17:         Browser = BrowserType.Chromium;
18:         GetContent = false;
19:         StripHtml = false;
20: 
21:         PageInstructionsList = new();
22:     }
```

**Explanation**: 
- `WebSearchCommand` inherits all these properties and defaults from `WebCommand`
- `WebGetCommand` also inherits from `WebCommand`, so they share most Layer 1 options

---

## Summary of Evidence

| Feature | Parser Location | Command Property | Default Value |
|---------|----------------|------------------|---------------|
| Positional search terms | CycoDmdCommandLineOptions.cs:467-470 | WebSearchCommand.Terms | (empty list) |
| `--max` | CycoDmdCommandLineOptions.cs:367-372 | WebCommand.MaxResults | 10 |
| `--google` | CycoDmdCommandLineOptions.cs:347-350 | WebCommand.SearchProvider | Google (default) |
| `--bing` | CycoDmdCommandLineOptions.cs:339-342 | WebCommand.SearchProvider | - |
| `--duck-duck-go` | CycoDmdCommandLineOptions.cs:343-346 | WebCommand.SearchProvider | - |
| `--yahoo` | CycoDmdCommandLineOptions.cs:351-354 | WebCommand.SearchProvider | - |
| `--bing-api` | CycoDmdCommandLineOptions.cs:355-358 | WebCommand.SearchProvider | - |
| `--google-api` | CycoDmdCommandLineOptions.cs:359-362 | WebCommand.SearchProvider | - |
| `--exclude` | CycoDmdCommandLineOptions.cs:373-379 | WebCommand.ExcludeURLContainsPatternList | (empty list) |
| Auto-enable content | WebSearchCommand.cs:23-36 | GetContent, StripHtml | false (auto-enabled if instructions provided) |

---

## Related Files

- **WebSearchCommand**: `src/cycodmd/CommandLineCommands/WebSearchCommand.cs` - Search command implementation
- **WebCommand**: `src/cycodmd/CommandLineCommands/WebCommand.cs` - Base class for web commands
- **CommandLineOptions**: `src/common/CommandLine/CommandLineOptions.cs` - Base parser functionality
