# Bug Fix: --line-contains Implementation

**Date:** 2025-12-16  
**Status:** ✅ **FIXED**

## Problem

`--line-contains` flag was documented everywhere but never actually implemented in cycodgr. When used, it was treated as a positional repository argument, causing GitHub API errors.

**Error example:**
```bash
cycodgr --repo robch/cycod --cs-file-contains "IChatClient" --line-contains "Create" --lines 5
```
**Result:** Error - HTTP 422 with invalid query `repo:--line-contains repo:Create`

## Root Cause

- `--line-contains` was never added to the command-line parser
- No `LineContainsPatterns` property in `SearchCommand`
- Line filtering was hardcoded to use the search query

## Solution

Implemented `--line-contains` properly by following cycodmd's pattern:

### Changes Made

1. **Added property to SearchCommand** (SearchCommand.cs):
   ```csharp
   public List<string> LineContainsPatterns { get; set; }
   ```

2. **Added command-line parsing** (CycoGrCommandLineOptions.cs):
   ```csharp
   else if (arg == "--line-contains" && command is SearchCommand searchCmd3)
   {
       var patterns = GetInputOptionArgs(i + 1, args, required: 1);
       searchCmd3.LineContainsPatterns.AddRange(patterns);
       i += patterns.Count();
   }
   ```

3. **Updated line filtering logic** (Program.cs, ProcessFileGroupAsync):
   ```csharp
   // Use explicit --line-contains patterns if specified, otherwise fallback to search query
   List<Regex> includePatterns;
   if (command.LineContainsPatterns.Any())
   {
       includePatterns = command.LineContainsPatterns
           .Select(p => new Regex(p, RegexOptions.IgnoreCase))
           .ToList();
   }
   else
   {
       includePatterns = new List<Regex> { 
           new Regex(Regex.Escape(query), RegexOptions.IgnoreCase) 
       };
   }
   ```

4. **Updated method signatures** to pass command through:
   - `FormatAndOutputCodeResults` - added command parameter
   - `ProcessFileGroupAsync` - added command parameter

## Testing

**Test command:**
```bash
cycodgr --repo robch/cycod --cs-file-contains "IChatClient" --line-contains "Create" --lines 3 --max-results 1
```

**Result:** ✅ Success!
- Found file with "IChatClient" 
- Filtered lines to only show those containing "Create"
- Displayed 3 lines of context around each match
- No API errors

## Behavior

**Without `--line-contains`:**
- Uses the search query for line filtering (existing behavior maintained)

**With `--line-contains`:**
- Uses the specified pattern(s) for line filtering
- Allows different pattern than the search query
- Example: Search for "IChatClient" but only show lines with "Create"

## Comparison with cycodmd

cycodgr now matches cycodmd's implementation:
- ✅ `--line-contains` accepts patterns (supports regex like cycodmd)
- ✅ Multiple patterns supported (just like cycodmd)
- ✅ Falls back to search query if not specified
- ⏳ cycodmd also has `--remove-lines-contains` (exclude patterns) - not yet in cycodgr

## Files Modified

- `src/cycodgr/CommandLineCommands/SearchCommand.cs` - Added LineContainsPatterns property
- `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs` - Added --line-contains parsing
- `src/cycodgr/Program.cs` - Updated line filtering logic and method signatures

## Impact

This fixes all the documented examples in:
- `todo/phase-a-complete.md`
- `todo/phase-b-complete.md`
- `todo/phase-c-complete.md`
- `todo/phase-d-complete.md`
- `todo/phase-e-complete.md`
- `todo/PHASES-COMPLETE.md`
- `todo/repo-filtering-and-save-workflow.md`
- `todo/unified-processing-architecture.md`

All examples that used `--line-contains` will now work as documented!

## Status

✅ **Bug fixed and tested**  
✅ **Builds successfully**  
✅ **Matches cycodmd behavior**  
✅ **Backward compatible** (falls back to search query if not specified)
