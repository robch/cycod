# cycodmd Web Get - Layer 1: Target Selection - PROOF

This document provides source code evidence for all Layer 1 (Target Selection) features of the cycodmd web get command.

---

## Positional URLs

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 472-476**:
```csharp
472:         else if (command is WebGetCommand webGetCommand)
473:         {
474:             webGetCommand.Urls.Add(arg);
475:             parsedOption = true;
476:         }
```

**Explanation**: 
- Method `TryParseOtherCommandArg()` handles non-option arguments
- For `WebGetCommand`, each argument is treated as a URL
- Allows multiple URLs: `cycodmd web get url1 url2 url3`

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 6-9, 11**:
```csharp
6:     public WebGetCommand()
7:     {
8:         Urls = new List<string>();
9:     }
10:     
11:     public List<string> Urls { get; set; }
```

**Explanation**: 
- `Urls` is initialized as an empty list in the constructor
- Stores all URLs provided by the user

### Empty Check

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 18-22**:
```csharp
18:     override public bool IsEmpty()
19:     {
20:         
21:         return !Urls.Any();
22:     }
```

**Explanation**: 
- Command is considered empty if no URLs are provided
- Validation will fail if Urls is empty

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
- Same parser logic as WebSearchCommand (shared in base class)
- Reads exactly one argument after `--max`
- Validates it as an integer using `ValidateInt()`
- Sets the `MaxResults` property

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 12, 27**:
```csharp
12:         MaxResults = 10;
...
27:     public int MaxResults { get; set; }
```

**Explanation**: 
- Inherited from `WebCommand` base class
- Defaults to 10
- Both `WebSearchCommand` and `WebGetCommand` share this property

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
- Same parser logic as WebSearchCommand (shared in base class)
- Reads all arguments following `--exclude` (until next option or end of args)
- Validates each pattern as a regex using `ValidateRegExPatterns()`
- Adds compiled Regex objects to `ExcludeURLContainsPatternList`

### Command Property

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 15, 26**:
```csharp
15:         ExcludeURLContainsPatternList = new();
...
26:     public List<Regex> ExcludeURLContainsPatternList { get; set; }
```

**Explanation**: 
- Inherited from `WebCommand` base class
- Initialized as empty list in constructor
- Stores compiled regex patterns for URL filtering

---

## Validation

### No Special Validation

**File**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs`

**Lines 24-27**:
```csharp
24:     override public CycoDmdCommand Validate()
25:     {
26:         return this;
27:     }
```

**Explanation**: 
- Unlike `WebSearchCommand`, `WebGetCommand` has no special validation logic
- No auto-enabling of `GetContent` or `StripHtml`
- Simply returns itself

---

## Command Inheritance Hierarchy

### WebGetCommand Inheritance

```
Command (base class)
    ↓
CycoDmdCommand (cycodmd-specific base)
    ↓
WebCommand (web-specific base)
    ↓
WebGetCommand (direct URL retrieval)
```

### Shared Properties from WebCommand

**File**: `src/cycodmd/CommandLineCommands/WebCommand.cs`

**Lines 8-22**:
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
- `WebGetCommand` inherits all these properties from `WebCommand`
- Many properties (like `SearchProvider`) are not used by WebGetCommand
- Properties like `MaxResults`, `ExcludeURLContainsPatternList`, `Browser`, `GetContent`, `StripHtml`, and `PageInstructionsList` are used

---

## Comparison to WebSearchCommand

### Similarities

Both commands share:
- `--max` option (from WebCommand)
- `--exclude` option (from WebCommand)
- Browser control options (Layer 6)
- AI processing options (Layer 8)
- Output persistence options (Layer 7)

### Differences

| Feature | WebSearchCommand | WebGetCommand |
|---------|------------------|---------------|
| **Positional Args** | Search terms | URLs |
| **Search Provider Options** | Yes (`--google`, `--bing`, etc.) | No (not applicable) |
| **Auto-enable GetContent** | Yes (if instructions provided) | No |
| **Purpose** | Discovery + Retrieval | Direct Retrieval |

**Code Evidence for Search Provider Not Used**:

WebGetCommand doesn't override or use `SearchProvider`, but it's still set to `Google` by default from the base class. This is harmless - the property is simply ignored during execution.

---

## Summary of Evidence

| Feature | Parser Location | Command Property | Default Value |
|---------|----------------|------------------|---------------|
| Positional URLs | CycoDmdCommandLineOptions.cs:472-476 | WebGetCommand.Urls | (empty list) |
| `--max` | CycoDmdCommandLineOptions.cs:367-372 | WebCommand.MaxResults | 10 |
| `--exclude` | CycoDmdCommandLineOptions.cs:373-379 | WebCommand.ExcludeURLContainsPatternList | (empty list) |
| Validation | WebGetCommand.cs:24-27 | N/A | No special validation |

---

## Related Files

- **WebGetCommand**: `src/cycodmd/CommandLineCommands/WebGetCommand.cs` - Get command implementation
- **WebCommand**: `src/cycodmd/CommandLineCommands/WebCommand.cs` - Base class for web commands
- **CommandLineOptions**: `src/common/CommandLine/CommandLineOptions.cs` - Base parser functionality
