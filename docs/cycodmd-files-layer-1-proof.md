# cycodmd File Search - Layer 1: Target Selection - PROOF

This document provides source code evidence for all Layer 1 (Target Selection) features of the cycodmd file search command.

---

## Positional Glob Patterns

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 457-460**:
```csharp
457:     override protected bool TryParseOtherCommandArg(Command? command, string arg)
458:     {
459:         var parsedOption = false;
460: 
461:         if (command is FindFilesCommand findFilesCommand)
462:         {
463:             findFilesCommand.Globs.Add(arg);
464:             parsedOption = true;
465:         }
```

**Explanation**: 
- Method `TryParseOtherCommandArg()` is called for each non-option argument (arguments not starting with `--`)
- If the command is a `FindFilesCommand`, the argument is added to the `Globs` list
- This allows multiple glob patterns: `cycodmd "*.cs" "*.md" "*.js"`

### Command Property

**File**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

**Lines 9-11, 91**:
```csharp
9:     public FindFilesCommand()
10:     {
11:         Globs = new();
...
91:     public List<string> Globs;
```

**Explanation**: 
- `Globs` is initialized as an empty list in the constructor
- Stores all positional glob patterns provided by the user

### Default Value

**File**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

**Lines 73-78**:
```csharp
73:     override public CycoDmdCommand Validate()
74:     {
75:         if (!Globs.Any()) 
76:         {
77:             Globs.Add("**");
78:         }
```

**Explanation**: 
- If no globs are provided, the `Validate()` method adds `**` (all files recursively) as the default
- This ensures there's always at least one glob pattern to process

---

## `--exclude` Option

### Parser Implementation

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

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

**Explanation**: 
- Reads all arguments following `--exclude` (until next option or end of args)
- Calls `ValidateExcludeRegExAndGlobPatterns()` to split patterns into regex and glob categories
- Patterns with `/` or `\` → globs (path-based matching)
- Patterns without path separators → regex (filename-only matching)

### Pattern Validation

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines (from base class)**: Referenced in `ValidateExcludeRegExAndGlobPatterns()`

The method checks if patterns contain path separators:
```csharp
var containsSlash = (string x) => x.Contains('/') || x.Contains('\\');
asRegExs = patterns
    .Where(x => !containsSlash(x))
    .Select(x => ValidateFilePatternToRegExPattern(arg, x))
    .ToList();
asGlobs = patterns
    .Where(x => containsSlash(x))
    .ToList();
```

### Command Properties

**File**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

**Lines 12-13, 92-93**:
```csharp
12:         ExcludeGlobs = new();
13:         ExcludeFileNamePatternList = new();
...
92:     public List<string> ExcludeGlobs;
93:     public List<Regex> ExcludeFileNamePatternList;
```

**Explanation**: 
- `ExcludeGlobs` stores path-based glob patterns
- `ExcludeFileNamePatternList` stores regex patterns for filename matching

---

## Time-Based Filtering Options

### `--modified` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 188-195**:
```csharp
188:         else if (arg == "--modified")
189:         {
190:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
191:             var (after, before) = ValidateTimeSpecRange(arg, timeSpec);
192:             command.ModifiedAfter = after;
193:             command.ModifiedBefore = before;
194:             i++;
195:         }
```

**Explanation**: 
- Reads one argument after `--modified`
- Calls `ValidateTimeSpecRange()` to parse the timespec into a (after, before) tuple
- Sets both `ModifiedAfter` and `ModifiedBefore` properties

### Time Validation Helper

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 486-499**:
```csharp
486:     private (DateTime? After, DateTime? Before) ValidateTimeSpecRange(string arg, string? timeSpec)
487:     {
488:         if (string.IsNullOrEmpty(timeSpec))
489:             throw new CommandLineException($"Missing time specification for {arg}");
490:             
491:         try
492:         {
493:             return TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
494:         }
495:         catch (Exception ex) when (!(ex is CommandLineException))
496:         {
497:             throw new CommandLineException($"Invalid time specification for {arg}: {ex.Message}");
498:         }
499:     }
```

**Explanation**: 
- Delegates to `TimeSpecHelpers.ParseTimeSpecRange()` for actual parsing
- Wraps exceptions to provide user-friendly error messages

---

### `--modified-after`, `--after`, `--time-after` Options

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 212-217**:
```csharp
212:         else if (arg == "--modified-after" || arg == "--after" || arg == "--time-after")
213:         {
214:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
215:             command.ModifiedAfter = ValidateSingleTimeSpec(arg, timeSpec, isAfter: true);
216:             i++;
217:         }
```

**Explanation**: 
- Three aliases for the same option
- Calls `ValidateSingleTimeSpec()` with `isAfter: true` to parse a single boundary time
- Only sets `ModifiedAfter` (not `ModifiedBefore`)

### Single Time Validation Helper

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 501-514**:
```csharp
501:     private DateTime? ValidateSingleTimeSpec(string arg, string? timeSpec, bool isAfter)
502:     {
503:         if (string.IsNullOrEmpty(timeSpec))
504:             throw new CommandLineException($"Missing time specification for {arg}");
505:             
506:         try
507:         {
508:             return TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter);
509:         }
510:         catch (Exception ex) when (!(ex is CommandLineException))
511:         {
512:             throw new CommandLineException($"Invalid time specification for {arg}: {ex.Message}");
513:         }
514:     }
```

**Explanation**: 
- Delegates to `TimeSpecHelpers.ParseSingleTimeSpec()`
- The `isAfter` parameter determines how relative times are interpreted

---

### `--modified-before`, `--before`, `--time-before` Options

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 218-223**:
```csharp
218:         else if (arg == "--modified-before" || arg == "--before" || arg == "--time-before")
219:         {
220:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
221:             command.ModifiedBefore = ValidateSingleTimeSpec(arg, timeSpec, isAfter: false);
222:             i++;
223:         }
```

**Explanation**: 
- Three aliases for the same option
- Calls `ValidateSingleTimeSpec()` with `isAfter: false`
- Only sets `ModifiedBefore` (not `ModifiedAfter`)

---

### `--created` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 196-203**:
```csharp
196:         else if (arg == "--created")
197:         {
198:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
199:             var (after, before) = ValidateTimeSpecRange(arg, timeSpec);
200:             command.CreatedAfter = after;
201:             command.CreatedBefore = before;
202:             i++;
203:         }
```

**Explanation**: 
- Similar to `--modified`, but sets `CreatedAfter` and `CreatedBefore`

---

### `--created-after` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 224-229**:
```csharp
224:         else if (arg == "--created-after")
225:         {
226:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
227:             command.CreatedAfter = ValidateSingleTimeSpec(arg, timeSpec, isAfter: true);
228:             i++;
229:         }
```

---

### `--created-before` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 230-235**:
```csharp
230:         else if (arg == "--created-before")
231:         {
232:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
233:             command.CreatedBefore = ValidateSingleTimeSpec(arg, timeSpec, isAfter: false);
234:             i++;
235:         }
```

---

### `--accessed` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 204-211**:
```csharp
204:         else if (arg == "--accessed")
205:         {
206:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
207:             var (after, before) = ValidateTimeSpecRange(arg, timeSpec);
208:             command.AccessedAfter = after;
209:             command.AccessedBefore = before;
210:             i++;
211:         }
```

---

### `--accessed-after` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 236-241**:
```csharp
236:         else if (arg == "--accessed-after")
237:         {
238:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
239:             command.AccessedAfter = ValidateSingleTimeSpec(arg, timeSpec, isAfter: true);
240:             i++;
241:         }
```

---

### `--accessed-before` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 242-247**:
```csharp
242:         else if (arg == "--accessed-before")
243:         {
244:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
245:             command.AccessedBefore = ValidateSingleTimeSpec(arg, timeSpec, isAfter: false);
246:             i++;
247:         }
```

---

### `--anytime` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 248-255**:
```csharp
248:         else if (arg == "--anytime")
249:         {
250:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
251:             var (after, before) = ValidateTimeSpecRange(arg, timeSpec);
252:             command.AnyTimeAfter = after;
253:             command.AnyTimeBefore = before;
254:             i++;
255:         }
```

**Explanation**: 
- Sets `AnyTimeAfter` and `AnyTimeBefore` instead of specific timestamp properties
- Allows matching files where ANY of the three timestamps (modified/created/accessed) fall within range

---

### `--anytime-after` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 256-261**:
```csharp
256:         else if (arg == "--anytime-after")
257:         {
258:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
259:             command.AnyTimeAfter = ValidateSingleTimeSpec(arg, timeSpec, isAfter: true);
260:             i++;
261:         }
```

---

### `--anytime-before` Option

**File**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

**Lines 262-267**:
```csharp
262:         else if (arg == "--anytime-before")
263:         {
264:             var timeSpec = GetInputOptionArgs(i + 1, args, required: 1).FirstOrDefault();
265:             command.AnyTimeBefore = ValidateSingleTimeSpec(arg, timeSpec, isAfter: false);
266:             i++;
267:         }
```

---

### Command Time Properties

**File**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

**Lines 31-38, 113-120**:
```csharp
31:         // Initialize time constraints to null
32:         ModifiedAfter = null;
33:         ModifiedBefore = null;
34:         CreatedAfter = null;
35:         CreatedBefore = null;
36:         AccessedAfter = null;
37:         AccessedBefore = null;
38:         AnyTimeAfter = null;
39:         AnyTimeBefore = null;
...
113:     // Time-based filtering properties
114:     public DateTime? ModifiedAfter;
115:     public DateTime? ModifiedBefore;
116:     public DateTime? CreatedAfter;
117:     public DateTime? CreatedBefore;
118:     public DateTime? AccessedAfter;
119:     public DateTime? AccessedBefore;
120:     public DateTime? AnyTimeAfter;
121:     public DateTime? AnyTimeBefore;
```

**Explanation**: 
- All time properties are nullable `DateTime?`
- Initialized to `null` (no time constraints)
- Set by parser when user provides time options

---

## `.cycodmdignore` File

### Implementation

**File**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

**Lines 80-86**:
```csharp
80:         var ignoreFile = FileHelpers.FindFileSearchParents(".cycodmdignore");
81:         if (ignoreFile != null)
82:         {
83:             FileHelpers.ReadIgnoreFile(ignoreFile, out var excludeGlobs, out var excludeFileNamePatternList);
84:             ExcludeGlobs.AddRange(excludeGlobs);
85:             ExcludeFileNamePatternList.AddRange(excludeFileNamePatternList);
86:         }
```

**Explanation**: 
- Called during `Validate()` method (after parsing, before execution)
- `FindFileSearchParents()` searches current directory and all parent directories for `.cycodmdignore`
- `ReadIgnoreFile()` parses the file and returns globs and regex patterns
- Patterns are merged with any `--exclude` patterns provided on command line

---

## Summary of Evidence

| Feature | Parser Location | Command Property | Validation/Default |
|---------|----------------|------------------|-------------------|
| Positional globs | CycoDmdCommandLineOptions.cs:457-460 | FindFilesCommand.Globs | FindFilesCommand.cs:75-78 |
| `--exclude` | CycoDmdCommandLineOptions.cs:282-289 | ExcludeGlobs, ExcludeFileNamePatternList | Base class validation |
| `--modified` | CycoDmdCommandLineOptions.cs:188-195 | ModifiedAfter, ModifiedBefore | TimeSpecHelpers |
| `--modified-after` etc. | CycoDmdCommandLineOptions.cs:212-217 | ModifiedAfter | TimeSpecHelpers |
| `--modified-before` etc. | CycoDmdCommandLineOptions.cs:218-223 | ModifiedBefore | TimeSpecHelpers |
| `--created` | CycoDmdCommandLineOptions.cs:196-203 | CreatedAfter, CreatedBefore | TimeSpecHelpers |
| `--created-after` | CycoDmdCommandLineOptions.cs:224-229 | CreatedAfter | TimeSpecHelpers |
| `--created-before` | CycoDmdCommandLineOptions.cs:230-235 | CreatedBefore | TimeSpecHelpers |
| `--accessed` | CycoDmdCommandLineOptions.cs:204-211 | AccessedAfter, AccessedBefore | TimeSpecHelpers |
| `--accessed-after` | CycoDmdCommandLineOptions.cs:236-241 | AccessedAfter | TimeSpecHelpers |
| `--accessed-before` | CycoDmdCommandLineOptions.cs:242-247 | AccessedBefore | TimeSpecHelpers |
| `--anytime` | CycoDmdCommandLineOptions.cs:248-255 | AnyTimeAfter, AnyTimeBefore | TimeSpecHelpers |
| `--anytime-after` | CycoDmdCommandLineOptions.cs:256-261 | AnyTimeAfter | TimeSpecHelpers |
| `--anytime-before` | CycoDmdCommandLineOptions.cs:262-267 | AnyTimeBefore | TimeSpecHelpers |
| `.cycodmdignore` | FindFilesCommand.cs:80-86 | ExcludeGlobs, ExcludeFileNamePatternList | FileHelpers |

---

## Related Files

- **TimeSpecHelpers**: `src/common/Helpers/TimeSpecHelpers.cs` - Time specification parsing
- **FileHelpers**: `src/common/Helpers/FileHelpers.cs` - File operations and ignore file parsing
- **CommandLineOptions**: `src/common/CommandLine/CommandLineOptions.cs` - Base parser functionality
