# cycodj branches/stats - Layer 1: TARGET SELECTION

## Overview

The `branches` and `stats` commands have **identical** Layer 1 (TARGET SELECTION) implementation to the `list` command. Both implement **RICH** target selection with time filtering and count limiting.

## Commands Covered

This document covers Layer 1 for:
- **branches** - Shows conversation tree structure
- **stats** - Shows conversation statistics

## Implementation Summary

Both commands implement **RICH** target selection, identical to `list`:

1. **Time-range filtering** (primary, modern approach)
2. **Legacy date filtering** (backward compatibility)
3. **Count limiting** (smart: conversation count OR timespec)
4. **NO default limiting** (unlike list which defaults to 20)

## Target Selection Options

### Time-Range Filtering (Modern)

#### Shortcuts
- `--today` - Today's conversations
- `--yesterday` - Yesterday's conversations

#### Absolute/Relative Times
- `--after <timespec>`, `--time-after <timespec>` - After specific time
- `--before <timespec>`, `--time-before <timespec>` - Before specific time
- `--date-range <range>`, `--time-range <range>` - Within time range

### Legacy Date Filtering

- `--date <date>`, `-d <date>` - Conversations on specific date

### Count Limiting (Smart Detection)

- `--last <N>` - EITHER last N conversations OR timespec
  - Integer: last N conversations
  - Timespec: time-based filter

### Additional Options (branches only)

- `--conversation <id>`, `-c <id>` - Show branches for specific conversation

## Processing Pipeline

### For branches Command

```
Step 1: Find all history files
Step 2: Apply time-range filter (if specified)
Step 3: Apply legacy date filter (if specified)
Step 4: Apply count limit (if specified)
Step 5: Read conversations
Step 6: If --conversation specified, filter to that subtree
Step 7: Build and display branch tree
```

### For stats Command

```
Step 1: Find all history files
Step 2: Apply time-range filter (if specified)
Step 3: Apply legacy date filter (if specified)
Step 4: Apply count limit (if specified)
Step 5: Read conversations
Step 6: Calculate and display statistics
```

## Differences from list Command

| Aspect | list | branches | stats |
|--------|------|----------|-------|
| **Default limit** | 20 conversations | None | None |
| **Time filtering** | ✅ Same | ✅ Same | ✅ Same |
| **Count limiting** | ✅ Same | ✅ Same | ✅ Same |
| **Special filter** | ❌ None | ✅ `--conversation` | ❌ None |
| **Purpose** | Browse | Visualize tree | Analyze patterns |

## Examples

### branches Command Examples

```bash
# All conversations with branch tree
cycodj branches

# Today's conversations
cycodj branches --today

# Last 10 conversations
cycodj branches --last 10

# Last 7 days
cycodj branches --last 7d

# Branches for specific conversation
cycodj branches --conversation abc123
```

### stats Command Examples

```bash
# All conversations
cycodj stats

# Today's statistics
cycodj stats --today

# Last 30 days
cycodj stats --last 30d

# Specific date range
cycodj stats --date-range 2024-01-01..2024-01-31

# With tool usage details
cycodj stats --today --show-tools
```

## Parser Evidence

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

### branches Command Parsing

**Lines 314-366**: BranchesCommand option parsing
```csharp
314:     private bool TryParseBranchesCommandOptions(BranchesCommand command, string[] args, ref int i, string arg)
315:     {
316:         // Try common display options first
317:         if (TryParseDisplayOptions(command, args, ref i, arg))
318:         {
319:             return true;
320:         }
321:         
322:         // Try common time options
323:         if (TryParseTimeOptions(command, args, ref i, arg))
324:         {
325:             return true;
326:         }
327:         
328:         if (arg == "--date" || arg == "-d")
329:         {
330:             var date = i + 1 < args.Length ? args[++i] : null;
331:             if (string.IsNullOrWhiteSpace(date))
332:             {
333:                 throw new CommandLineException($"Missing date value for {arg}");
334:             }
335:             command.Date = date;
336:             return true;
337:         }
338:         else if (arg == "--last")
339:         {
340:             var value = i + 1 < args.Length ? args[++i] : null;
341:             if (string.IsNullOrWhiteSpace(value))
342:             {
343:                 throw new CommandLineException($"Missing value for {arg}");
344:             }
345:             
346:             ParseLastValue(command, arg, value);
347:             return true;
348:         }
349:         else if (arg == "--conversation" || arg == "-c")
350:         {
351:             var conv = i + 1 < args.Length ? args[++i] : null;
352:             if (string.IsNullOrWhiteSpace(conv))
353:             {
354:                 throw new CommandLineException($"Missing conversation value for {arg}");
355:             }
356:             command.Conversation = conv;
357:             return true;
358:         }
359:         else if (arg == "--verbose" || arg == "-v")
360:         {
361:             command.Verbose = true;
362:             return true;
363:         }
364:         
365:         return false;
366:     }
```

### stats Command Parsing

**Lines 483-530**: StatsCommand option parsing
```csharp
483:     private bool TryParseStatsCommandOptions(StatsCommand command, string[] args, ref int i, string arg)
484:     {
485:         // Try common display options first
486:         if (TryParseDisplayOptions(command, args, ref i, arg))
487:         {
488:             return true;
489:         }
490:         
491:         // Try common time options
492:         if (TryParseTimeOptions(command, args, ref i, arg))
493:         {
494:             return true;
495:         }
496:         
497:         if (arg == "--date" || arg == "-d")
498:         {
499:             var date = i + 1 < args.Length ? args[++i] : null;
500:             if (string.IsNullOrWhiteSpace(date))
501:             {
502:                 throw new CommandLineException($"Missing date value for {arg}");
503:             }
504:             command.Date = date;
505:             return true;
506:         }
507:         else if (arg == "--last")
508:         {
509:             var value = i + 1 < args.Length ? args[++i] : null;
510:             if (string.IsNullOrWhiteSpace(value))
511:             {
512:                 throw new CommandLineException($"Missing value for {arg}");
513:             }
514:             
515:             ParseLastValue(command, arg, value);
516:             return true;
517:         }
518:         else if (arg == "--show-tools")
519:         {
520:             command.ShowTools = true;
521:             return true;
522:         }
523:         else if (arg == "--no-dates")
524:         {
525:             command.ShowDates = false;
526:             return true;
527:         }
528:         
529:         return false;
530:     }
```

**Key Pattern**: Both call the same shared methods:
- `TryParseTimeOptions()` - Lines 323, 492
- `ParseLastValue()` - Lines 346, 515
- Same `--date` and `--last` handling

## Execution Evidence

### branches Command Properties

**File**: `src/cycodj/CommandLineCommands/BranchesCommand.cs`

**Lines 10-17**:
```csharp
10: public class BranchesCommand : CycoDjCommand
11: {
12:     public string? Date { get; set; }
13:     public string? Conversation { get; set; }  // UNIQUE to branches
14:     public bool Verbose { get; set; } = false;
15:     public int Last { get; set; } = 0;
16:     public int? MessageCount { get; set; } = null;
17:     public bool ShowStats { get; set; } = false;
```

### stats Command Properties

**File**: `src/cycodj/CommandLineCommands/StatsCommand.cs`

**Lines 8-13**:
```csharp
8:     public class StatsCommand : CommandLine.CycoDjCommand
9:     {
10:         public string? Date { get; set; }
11:         public int? Last { get; set; }  // Nullable (vs branches int)
12:         public bool ShowTools { get; set; }
13:         public bool ShowDates { get; set; } = true;
```

## Complete Documentation

For complete Layer 1 documentation including:
- Full processing pipeline details
- Complete source code line numbers
- Execution flow diagrams
- Comparison tables
- Verification tests

See the detailed proof files:
- **[branches proof](cycodj-branches-stats-layer-1-proof.md#branches-command)**
- **[stats proof](cycodj-branches-stats-layer-1-proof.md#stats-command)**

## Summary

Both `branches` and `stats` commands:
- ✅ Use identical time filtering options as `list`
- ✅ Use identical smart `--last` detection
- ✅ Use same helper methods (`TryParseTimeOptions`, `ParseLastValue`)
- ✅ Have NO default limit (unlike list's 20)
- ✅ Support all timespec formats

**Unique to branches**:
- `--conversation` option to focus on specific subtree

**Unique to stats**:
- `--show-tools` option for tool usage statistics
- `--no-dates` option to hide date breakdown

---

**Next Layer**: [Layer 6: Display Control](cycodj-branches-layer-6.md) for branches, [Layer 6](cycodj-stats-layer-6.md) for stats
