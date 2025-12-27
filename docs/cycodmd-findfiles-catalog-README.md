# FindFilesCommand - Filter Pipeline Catalog

## Overview

**Command**: `cycodmd [patterns] [options]` (default command)

**Purpose**: Search and process local files using glob patterns with advanced filtering, content searching, and AI-assisted processing capabilities.

**Source**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

## Command Properties

The FindFilesCommand maintains state across all 9 filtering layers through these properties:

```csharp
// Layer 1: TARGET SELECTION
List<string> Globs
List<string> ExcludeGlobs
List<Regex> ExcludeFileNamePatternList
DateTime? ModifiedAfter, ModifiedBefore
DateTime? CreatedAfter, CreatedBefore
DateTime? AccessedAfter, AccessedBefore
DateTime? AnyTimeAfter, AnyTimeBefore

// Layer 2: CONTAINER FILTERING
List<Regex> IncludeFileContainsPatternList
List<Regex> ExcludeFileContainsPatternList

// Layer 3: CONTENT FILTERING
List<Regex> IncludeLineContainsPatternList

// Layer 4: CONTENT REMOVAL
List<Regex> RemoveAllLineContainsPatternList

// Layer 5: CONTEXT EXPANSION
int IncludeLineCountBefore
int IncludeLineCountAfter

// Layer 6: DISPLAY CONTROL
bool IncludeLineNumbers
bool? HighlightMatches
bool FilesOnly

// Layer 7: OUTPUT PERSISTENCE
string? SaveFileOutput

// Layer 8: AI PROCESSING
List<Tuple<string, string>> FileInstructionsList

// Layer 9: ACTIONS ON RESULTS
string? ReplaceWithText
bool ExecuteMode
```

## The 9 Filtering Layers

### [Layer 1: Target Selection](cycodmd-findfiles-layer-1.md)

**What to search** - Specifies which files to search using glob patterns and time-based filtering

**Options**:
- Positional args: glob patterns
- `--exclude`: patterns to exclude files
- Time filters: `--modified`, `--created`, `--accessed`, `--anytime` + `-after`/`-before` variants

[View Details](cycodmd-findfiles-layer-1.md) | [View Proof](cycodmd-findfiles-layer-1-proof.md)

---

### [Layer 2: Container Filtering](cycodmd-findfiles-layer-2.md)

**Which files to include/exclude** - Filters files based on their content before processing

**Options**:
- `--file-contains`: include files containing pattern
- `--file-not-contains`: exclude files containing pattern
- `--contains`: shorthand for both file AND line filtering
- `--{ext}-file-contains`: extension-specific file content filtering

[View Details](cycodmd-findfiles-layer-2.md) | [View Proof](cycodmd-findfiles-layer-2-proof.md)

---

### [Layer 3: Content Filtering](cycodmd-findfiles-layer-3.md)

**What content to show** - Filters which lines within files to display

**Options**:
- `--line-contains`: include only lines matching pattern
- `--contains`: dual-purpose (files AND lines)
- `--highlight-matches` / `--no-highlight-matches`: control highlighting

[View Details](cycodmd-findfiles-layer-3.md) | [View Proof](cycodmd-findfiles-layer-3-proof.md)

---

### [Layer 4: Content Removal](cycodmd-findfiles-layer-4.md)

**What content to remove** - Actively removes matching lines from display

**Options**:
- `--remove-all-lines`: remove lines matching pattern

[View Details](cycodmd-findfiles-layer-4.md) | [View Proof](cycodmd-findfiles-layer-4-proof.md)

---

### [Layer 5: Context Expansion](cycodmd-findfiles-layer-5.md)

**How to expand around matches** - Shows context lines before/after matched lines

**Options**:
- `--lines`: show N lines before AND after matches
- `--lines-before`: show N lines before matches
- `--lines-after`: show N lines after matches

[View Details](cycodmd-findfiles-layer-5.md) | [View Proof](cycodmd-findfiles-layer-5-proof.md)

---

### [Layer 6: Display Control](cycodmd-findfiles-layer-6.md)

**How to present results** - Controls formatting and display options

**Options**:
- `--line-numbers`: show line numbers
- `--highlight-matches` / `--no-highlight-matches`: control match highlighting
- `--files-only`: show only file paths (no content)

[View Details](cycodmd-findfiles-layer-6.md) | [View Proof](cycodmd-findfiles-layer-6-proof.md)

---

### [Layer 7: Output Persistence](cycodmd-findfiles-layer-7.md)

**Where to save results** - Saves processed results to files

**Options**:
- `--save-output`: save combined output
- `--save-file-output`: save per-file output with template
- `--save-chat-history`: save AI processing history

[View Details](cycodmd-findfiles-layer-7.md) | [View Proof](cycodmd-findfiles-layer-7-proof.md)

---

### [Layer 8: AI Processing](cycodmd-findfiles-layer-8.md)

**AI-assisted analysis** - Processes results with AI instructions

**Options**:
- `--instructions`: general AI instructions
- `--file-instructions`: AI instructions for all files
- `--{ext}-file-instructions`: extension-specific AI instructions
- `--built-in-functions`: enable AI to use built-in functions

[View Details](cycodmd-findfiles-layer-8.md) | [View Proof](cycodmd-findfiles-layer-8-proof.md)

---

### [Layer 9: Actions on Results](cycodmd-findfiles-layer-9.md)

**What to do with results** - Performs actions like search-and-replace

**Options**:
- `--replace-with`: replacement text for matched patterns
- `--execute`: execute the replacement (vs. preview)

[View Details](cycodmd-findfiles-layer-9.md) | [View Proof](cycodmd-findfiles-layer-9-proof.md)

---

## Execution Flow

The FindFilesCommand executes through this pipeline:

1. **Parse command line** → Populate command properties (CycoDmdCommandLineOptions.cs)
2. **Validate command** → Apply defaults, load .cycodmdignore (FindFilesCommand.Validate)
3. **Find files** → Apply Layer 1 & 2 filters (FileHelpers.FindMatchingFiles)
4. **Process files** → Apply Layer 3-6 filters (Program.HandleFindFileCommand)
5. **Save outputs** → Layer 7 persistence (GetCheckSaveFileContentAsync)
6. **AI processing** → Layer 8 instructions (AiInstructionProcessor)
7. **Execute actions** → Layer 9 replacements (HandleReplacementMode)

## Source Code References

- **Command Definition**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`
- **Command Parser**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (lines 100-301, 457-460)
- **Execution Logic**: `src/cycodmd/Program.cs` (lines 163-251, 702-719)
- **File Helpers**: `src/common/Helpers/FileHelpers.cs`
- **Time Helpers**: `src/common/Helpers/TimeSpecHelpers.cs`

## Related Documentation

- [Back to cycodmd Catalog](cycodmd-filter-pipeline-catalog-README.md)
- [Compare with WebSearchCommand](cycodmd-websearch-catalog-README.md)
- [Compare with WebGetCommand](cycodmd-webget-catalog-README.md)
- [Compare with RunCommand](cycodmd-run-catalog-README.md)
