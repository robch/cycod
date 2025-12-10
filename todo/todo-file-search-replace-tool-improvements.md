# TODO: File Search and Replace Tool Improvements

## Summary
Improve the file discovery and replacement tools to reduce user confusion and add bulk replacement capabilities.

## Current Issues

### QueryFiles Confusion
- **Problem**: `QueryFiles` mixes file discovery with content display, causing confusion when users want file paths but get content output
- **Evidence**: User experience shows frequent confusion when looking for files vs searching within files
- **Parameter Analysis**: About half of QueryFiles parameters are irrelevant for file-discovery-only use cases

### ReplaceOneInFile Limitations  
- **Problem**: `ReplaceOneInFile` only works on single files with literal strings
- **Gap**: No bulk replacement capability across multiple files
- **Workaround**: Users fall back to shell commands for bulk operations

## Proposed Solution

### Split QueryFiles into Focused Tools

**FindFiles** (File Discovery)
```
FindFiles(filePatterns, excludePatterns, fileContains, fileNotContains, 
          modifiedAfter, modifiedBefore, maxFiles)
→ Output: File paths only
```

**SearchInFiles** (Content Search)  
```
SearchInFiles(filePatterns, excludePatterns, fileContains, fileNotContains,
            modifiedAfter, modifiedBefore, searchPattern, lineContains, 
            removeAllLines, linesBeforeAndAfter, lineNumbers,
            maxFiles, maxCharsPerLine, maxTotalChars)
→ Output: File content with highlighted matches
```

### Add Bulk Replacement Tool

**ReplaceAllInFiles** (Bulk Replacement)
```
ReplaceAllInFiles(filePatterns, excludePatterns, fileContains, fileNotContains,
                  modifiedAfter, modifiedBefore, maxFiles,
                  old, new, useRegex=false, preview=true)
→ Bulk replacement with safety features
```

## Design Decisions Made

### Display Format
- **Git diff style** with `-`/`+` prefixes for removed/added lines
- **Grouped format**: All removed lines together, then all added lines together
- **Context lines**: Show surrounding code for verification (no prefix)
- **Single-line replacements only** (multiline support can be added later)

### Tool Architecture  
- **cycodmd extension**: Add `--replace-with` parameter to existing `--contains` functionality
- **Natural diff output**: When both search and replacement are specified, output is automatically diff format
- **Clean separation**: cycodmd shows changes, cycod executes changes

### Final Tool Signatures

**FindFiles** (File Discovery - paths only)
```
FindFiles(filePatterns, excludePatterns, fileContains, fileNotContains, 
          modifiedAfter, modifiedBefore, maxFiles)
→ Output: File paths only
```

**SearchInFiles** (Content Search - with highlighting)  
```
SearchInFiles(filePatterns, excludePatterns, fileContains, fileNotContains,
            modifiedAfter, modifiedBefore, searchPattern, lineContains, 
            removeAllLines, linesBeforeAndAfter, lineNumbers,
            maxFiles, maxCharsPerLine, maxTotalChars)
→ Output: File content with highlighted matches (current QueryFiles functionality)
```

**ReplaceAllInFiles** (Bulk Replacement)
```
ReplaceAllInFiles(filePatterns, excludePatterns, fileContains, fileNotContains,
                  modifiedAfter, modifiedBefore, maxFiles,
                  old, new, useRegex=false, preview=true)
→ Preview: calls cycodmd with --contains/--replace-with for diff output
→ Execute: performs replacements + shows results via cycodmd
```

## Implementation Impact

### What Changes
- Add 3 new tools with focused responsibilities
- Keep existing `ReplaceOneInFile` for single-file operations
- Replace `QueryFiles` with `FindFiles` and `SearchInFiles`

### What Stays
- All existing file modification tools (`CreateFile`, `Insert`, `UndoEdit`)
- Single-file `ReplaceOneInFile` (works great for its use case)
- All shell and process management tools

## Benefits

### User Experience
- **Predictable outputs**: Tool name tells you exactly what format you'll get
- **Reduced cognitive load**: Choose tool based on intent (find files vs search content)
- **Familiar patterns**: Names match VS Code and other IDEs

### Capability
- **File discovery**: Clean file paths without content noise
- **Bulk operations**: Replace across multiple files with safety preview
- **Progressive power**: Safe defaults, power when explicitly requested

## Evolution Strategy

This represents a focused improvement addressing real friction points without over-engineering:
- 3 targeted tools, not a complete redesign
- Addresses specific user confusion and capability gaps
- Maintains backward compatibility with existing workflows
- Sets pattern for future tool evolution: watch for repetitive shell command patterns

## Next Steps

1. Implement `FindFiles` with file-discovery-only parameters
2. Implement `FindInFiles` with full content search capabilities  
3. Implement `ReplaceAllInFiles` with bulk replacement and safety features
4. Replace `QueryFiles` with the new focused tools
5. Update documentation and examples

## Implementation Approach - Updated

### Final Architecture Decision: Extend cycodmd with replacement capability

Based on the broader architectural vision of evolving cycodmd into a comprehensive file processing tool, the cleanest approach is to add `--replace-with` functionality directly to cycodmd with preview/execute modes.

### Architectural Vision

**cycodmd evolution path:**
- **Current**: Read-only file exploration and documentation generation
- **Next step**: Add replacement capability with preview/execute modes  
- **Future**: AI-powered "sweep mode" for intelligent file modification

**This approach aligns with the bigger picture of eventually splitting cycodmd into focused CLIs:**
- File CLI (with read-only and read/write sweep modes)
- Web CLI (research and content extraction)
- Run CLI (command execution)

The replacement feature becomes a natural stepping stone toward the file CLI's read/write capabilities.

### Code Analysis Findings

**Current cycodmd structure:**
- `FindFilesCommand.cs` already has comprehensive filtering (IncludeLineContainsPatternList, context, etc.)
- `Program.cs` contains the content formatting logic in `GetFormattedFileContent()` and `GetContentFilteredAndFormatted()`
- Line matching/filtering is handled by `LineHelpers.IsLineMatch()`
- Current `--contains` parameter maps to both `IncludeFileContainsPatternList` and `IncludeLineContainsPatternList`

### Required Changes

**1. Add --replace-with parameter to cycodmd**
```csharp
// In CycoDmdCommandLineOptions.cs (around line 108)
else if (arg == "--replace-with")
{
    var replacementText = GetInputOptionArgs(i + 1, args, required: 1).First();
    command.ReplaceWithText = replacementText;
    i += 1;
}

// In FindFilesCommand.cs
public string? ReplaceWithText { get; set; }
```

### cycodmd Implementation

**1. Add --replace-with parameter and execution modes**
```bash
# Preview mode (default) - shows diff of planned changes
cycodmd find-files *.cs --contains "oldFunction" --replace-with "newFunction"

# Execute mode - performs changes and shows what was done
cycodmd find-files *.cs --contains "oldFunction" --replace-with "newFunction" --execute
```

**2. Required changes in cycodmd:**
- Add `--replace-with` and `--execute` parameters to command line parsing
- Add `ReplaceWithText` and `ExecuteMode` properties to `FindFilesCommand`
- Modify `GetContentFilteredAndFormatted()` for diff output when replacement text is present
- Implement actual file replacement logic when in execute mode
- Implement diff formatting logic (git diff style with `-`/`+` grouped format)

**3. Workflow:**
- **Preview mode**: Show diff output documenting planned changes
- **Execute mode**: Perform replacements with undo history, then show diff of actual changes made

### Integration with cycod - UPDATED

**Simplified approach with cycodmd handling replacement:**

Since cycodmd now handles both preview and execution of replacements, the cycod integration becomes much simpler:

**ReplaceAllInFiles becomes a wrapper:**
```csharp
// In StrReplaceEditorHelperFunctions
public async Task<string> ReplaceAllInFiles(...)
{
    if (preview)
        return await CallCycodmdForPreview(...);
    else  
        return await CallCycodmdForExecution(...);
}
```

**FindFiles/FindInFiles remain as planned:**
- Added to `CodeExplorationHelperFunctions` 
- Focused versions of existing QueryFiles functionality
- Call cycodmd for file discovery and content search

**Benefits of unified cycodmd approach:**
- Single tool handles complete replacement workflow
- No coordination needed between cycodmd and cycod
- Natural progression toward AI sweep mode capabilities
- Consistent user experience (discover → preview → execute all in one tool)

**Benefits of this approach:**
- Leverages all existing cycodmd file finding/filtering logic
- Consistent formatting with existing search results
- Natural separation: cycodmd shows changes, cycod makes changes
- No code duplication between projects

## Implementation Status

### ✅ COMPLETED
1. **FindFiles/FindInFiles** (cycod - focused file discovery and content search)
2. **cycodmd --replace-with** (cycodmd - replacement with preview/execute modes)  
3. **ReplaceAllInFiles wrapper** (cycod - orchestrates preview + execute with undo)

### Key Features Implemented

**Context lines in diff output:**
- Shows 3 lines of context before and after changes
- Git-style diff format with `-` (removed) and `+` (added) lines
- Context lines prefixed with spaces for clarity

**Undo history integration:**  
- ReplaceAllInFiles stores original content for each modified file in EditHistory
- Only files that actually have changes get undo history entries
- Users can revert individual files using existing UndoEdit function
- Execute mode reports how many files have undo history available

**Complete workflow:**
- Preview mode: Shows diff via cycodmd (no file changes)
- Execute mode: Stores undo → calls cycodmd for replacement → reports results
- Seamless integration with existing function calling infrastructure

---

*Created: 2025-12-05*  
*Branch: robch/2512-dec05-search-replace-ai-tools*