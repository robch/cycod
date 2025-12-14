# Unified Processing Architecture for cycodgr and cycodmd

**Status**: Phase 2 Complete! ✅ (Completed: 2025-01-14)  
**Date Started**: 2025-01-11  
**Date Completed**: 2025-01-14

## 🎉 Implementation Complete Summary

### ✅ Completed (Phase 1 & 2)
**Phase 1: Shared Components** - All created and working
- ✅ FoundTextFile (with lambda content loading)
- ✅ LineHelpers (expanded with filtering, context, line numbers)
- ✅ ParallelProcessor (generic parallel processing with throttling)
- ✅ AiInstructionProcessor (moved to common, shared by both tools)

**Phase 2: cycodgr Refactored** - Complete 3-level instruction pipeline
- ✅ Fixed line number bug (real line numbers from full files)
- ✅ Parallel file processing (using ParallelProcessor)
- ✅ File instructions (`--file-instructions`, `--EXT-file-instructions`)
- ✅ Repo instructions (`--repo-instructions`)
- ✅ Final/global instructions (`--instructions`)
- ✅ Instruction chaining (multiple instructions transform sequentially)

**Commits Made**: 5 major commits pushed
1. Phase 1 - Created shared components and fixed line numbers
2. Added parallel file processing
3. Added file instructions support
4. Added repo instructions support  
5. Added final/global instructions support

### ⏳ Remaining (Future Work)
**Phase 3**: Write tests for shared components
**Phase 4**: Refactor cycodmd to use ParallelProcessor (simplify Program.cs)
**Phase 5**: Documentation, optimization, additional features

---

## Overview

This document describes the processing architecture for both cycodgr (GitHub search) and cycodmd (local filesystem search), identifying their common patterns and designing a unified approach that both tools can share.

**Key Insight**: Both tools follow a hierarchical filtering pattern where they filter at one level, then drill down to filter at the next level, then process content through an AI instruction pipeline.

---

## Section 1: cycodgr Processing Model

### Hierarchical Structure (3 Levels)

```
Repos → Files → Lines
```

#### Level 0: Find Repos
- **Input**: Search query, filters
- **Process**: GitHub API search for repositories
- **Output**: `List<RepoInfo>`
- **Filtering**: 
  - `--repo-contains`: Repo metadata contains text
  - `--repo-file-contains`: Repo has files containing text (pre-filter)
  - `--language`, `--owner`, `--min-stars`, etc.

#### Level 1: Find Files in Repos (PARALLEL)
- **Input**: `List<RepoInfo>`
- **Process**: For each repo, GitHub code search for files
- **Parallelism**: Repos processed in parallel (throttled)
- **Output**: `List<FoundTextFile>` (grouped by repo)
- **Filtering**: 
  - `--file-contains`: Files containing text
  - `--language`, file extension filters

#### Level 2: Process Files (PARALLEL within each repo)
- **Input**: `List<FoundTextFile>` per repo
- **Process**: For each file:
  1. Download content from GitHub raw URL
  2. Filter lines (`--contains`, `--line-contains`)
  3. Expand context (lines before/after)
  4. Format as markdown
  5. Apply `--file-instructions` (if any)
- **Parallelism**: Files within a repo processed in parallel
- **Output**: Formatted file content (string per file)

#### Level 3: Repo Aggregation
- **Input**: All processed files for this repo
- **Process**: 
  1. Combine file outputs
  2. Format repo section (header + metadata + files)
  3. Apply `--repo-instructions` (if any)
- **Output**: Complete repo section (string per repo)

#### Level 4: Final Aggregation
- **Input**: All repo outputs
- **Process**:
  1. Combine all repo sections
  2. Apply `--instructions` (if any)
- **Output**: Final output to display

### Streaming vs Delayed Output

**Without `--instructions`:**
- Each repo outputs as it completes (streaming)
- User sees incremental results
- Better UX for exploratory searches

**With `--instructions`:**
- All repos must complete first
- Combine all outputs
- Apply final AI transformation
- Output once

### Processing Flow Diagram

```
GitHub API Search
    ↓
[Repo 1] [Repo 2] [Repo 3]  ← Parallel
    ↓        ↓        ↓
Find Files in Each Repo
    ↓        ↓        ↓
[File1 File2] [File3] [File4 File5]  ← Parallel within repo
    ↓    ↓      ↓       ↓      ↓
Download & Process Each File
    ↓    ↓      ↓       ↓      ↓
Apply --file-instructions (per file)
    ↓    ↓      ↓       ↓      ↓
Combine Files per Repo
    ↓        ↓        ↓
Apply --repo-instructions (per repo)
    ↓        ↓        ↓
Stream or Hold?
    ↓        ↓        ↓
If --instructions: Combine All → Apply Final AI → Output
If no --instructions: Stream Each Repo as Ready
```

### Data Models

```csharp
// Repo with metadata
class RepoInfo
{
    string Owner, Name, FullName;
    string Url, Description, Language;
    int Stars, Forks;
    // ... other metadata
}

// File found (from GitHub or filesystem)
class FoundTextFile
{
    string Path;                           // Display path
    string? Content;                       // Lazy loaded, cached after first load
    Func<Task<string>> LoadContent;        // How to load content (filesystem or HTTP)
    Dictionary<string, object> Metadata;   // Repo info, SHA, etc.
}

// Usage examples:
// Filesystem:
var file = new FoundTextFile {
    Path = "src/Program.cs",
    LoadContent = async () => await File.ReadAllTextAsync(fullPath)
};

// GitHub:
var file = new FoundTextFile {
    Path = "src/Program.cs",
    LoadContent = async () => await httpClient.GetStringAsync(rawUrl),
    Metadata = new Dictionary<string, object> {
        { "Repository", repoInfo },
        { "Sha", commitSha }
    }
};
```

---

## Section 2: cycodmd Processing Model

### Hierarchical Structure (2 Levels)

```
Files → Lines
```

#### Level 0: Find Files
- **Input**: Glob patterns, filters
- **Process**: Filesystem search
- **Output**: `List<string>` (file paths)
- **Filtering**:
  - Glob patterns
  - `--file-contains`: File content contains text
  - Time filters (modified, created, accessed)

#### Level 1: Process Files (PARALLEL)
- **Input**: `List<string>` (file paths)
- **Process**: For each file:
  1. Read content from filesystem
  2. Filter lines (`--contains`, `--line-contains`)
  3. Expand context (lines before/after)
  4. Format as markdown
  5. Apply `--file-instructions` (if any)
- **Parallelism**: Files processed in parallel (throttled)
- **Output**: Formatted file content (string per file)

#### Level 2: Final Aggregation
- **Input**: All processed file outputs
- **Process**:
  1. Combine all file sections
  2. Apply `--instructions` (if any)
- **Output**: Final output to display

### Streaming vs Delayed Output

**Without `--instructions`:**
- Each file outputs as it completes (streaming)
- User sees incremental results

**With `--instructions`:**
- All files must complete first
- Combine all outputs
- Apply final AI transformation
- Output once

### Processing Flow Diagram

```
Filesystem Glob Search
    ↓
[File1] [File2] [File3] [File4]  ← Parallel
    ↓       ↓       ↓       ↓
Read & Process Each File
    ↓       ↓       ↓       ↓
Apply --file-instructions (per file)
    ↓       ↓       ↓       ↓
Stream or Hold?
    ↓       ↓       ↓       ↓
If --instructions: Combine All → Apply Final AI → Output
If no --instructions: Stream Each File as Ready
```

### Current Implementation Notes

**Location**: `src/cycodmd/Program.cs`

**Key Mechanisms**:
- Line 100: `delayOutputToApplyInstructions = InstructionsList.Any()`
- Lines 238-244: Conditional output (ContinueWith for streaming)
- Lines 115-124: Final aggregation (Task.WhenAll + Join + AI)
- SemaphoreSlim for throttling parallel file processing

**Complexity Issues**:
- All processing logic in Program.cs
- Mixing parallelism, output handling, and business logic
- Hard to reuse or test independently

---

## Section 3: Unified Model - What They Share

### Common Processing Pattern

Both tools follow the same pattern at their core:

1. **Find items** (repos or files)
2. **Process items in parallel** (with throttling)
3. **Apply per-item AI instructions** (optional)
4. **Stream or aggregate** (based on whether final instructions exist)
5. **Apply final AI instructions** (optional)
6. **Output**

### Common Components

#### 1. Parallel Processing Infrastructure
- **Purpose**: Process multiple items concurrently with rate limiting
- **Mechanism**: Task-based parallelism with SemaphoreSlim throttling
- **Behavior**: Stream output immediately OR hold for aggregation

#### 2. Content Pipeline
- **Original content** → **Filter** → **Transform** → **AI process** → **Output**
- Content flows through multiple transformations
- Each stage can modify the content
- Pipeline stages are composable

#### 3. Instruction System (Multi-Level)
- **Per-item instructions**: Applied to each item individually during processing
  - cycodmd: `--file-instructions`, `--cs-file-instructions`, etc.
  - cycodgr: `--repo-instructions`, `--file-instructions`, `--cs-file-instructions`, etc.
- **Final instructions**: Applied to all combined output
  - Both: `--instructions`
- Instructions chain: each transforms the output of the previous one

#### 4. Streaming Optimization
- **No final instructions**: Output each item as it completes (streaming)
- **With final instructions**: Collect all, combine, transform, output once
- Better UX when possible (streaming), necessary aggregation when needed

### Shared Abstractions

### Shared Code - Not New Interfaces, Use What Exists!

**Don't create new abstractions unnecessarily.** We already have helpers that do much of this work.

#### Existing Helpers to Reuse

**For line filtering/processing** (already exists in cycodmd):
- `LineHelpers.IsLineMatch(line, includePatterns, excludePatterns)` - line filtering logic
- Exists in: `src/cycodmd/Helpers/LineHelpers.cs`

**For markdown formatting** (already exists in common):
- `MarkdownHelpers.GetCodeBlock(content, lang, ...)` - code block formatting
- Exists in: `src/common/Helpers/MarkdownHelpers.cs`

**For file operations** (already exists in common):
- `FileHelpers.*` - extensive file operations (read, write, glob matching, etc.)
- Exists in: `src/common/Helpers/FileHelpers.cs`

**For AI instruction processing** (already exists in cycodmd):
- `AiInstructionProcessor.ApplyAllInstructions(instructions, content, ...)` - AI pipeline
- Exists in: `src/cycodmd/AiInstructionProcessor.cs`
- This is a concrete class, just reuse it as-is

#### New Concrete Classes Needed (Not Interfaces!)

**ParallelProcessor** - New concrete class for parallel item processing
```csharp
class ParallelProcessor
{
    private SemaphoreSlim _throttler;
    
    public ParallelProcessor(int maxParallelism)
    {
        _throttler = new SemaphoreSlim(maxParallelism);
    }
    
    public async Task<List<TOutput>> ProcessAsync<TInput, TOutput>(
        List<TInput> items,
        Func<TInput, Task<TOutput>> processor)
    {
        var tasks = new List<Task<TOutput>>();
        
        foreach (var item in items)
        {
            await _throttler.WaitAsync();
            var task = processor(item).ContinueWith(t => {
                _throttler.Release();
                return t.Result;
            });
            tasks.Add(task);
        }
        
        return (await Task.WhenAll(tasks)).ToList();
    }
}
```
- **Why not interface?** Only one implementation needed
- **Where?** Could go in common or shared library
- **Usage:** Both cycodmd and cycodgr instantiate and use directly

**FoundTextFile** - New concrete class (data holder)
```csharp
class FoundTextFile
{
    public string Path { get; set; }                         // Display path
    public string? Content { get; set; }                     // Cached after first load
    public Func<Task<string>> LoadContent { get; set; }      // How to load (filesystem or HTTP)
    public Dictionary<string, object> Metadata { get; set; } // Repo info, SHA, etc.
}
```
- **Why not interface?** It's just a data holder
- **Where?** Shared library (both tools use it)

#### What About Content Processing?

The content processing pipeline (filter lines, expand context, format) already exists scattered across:
- `LineHelpers.IsLineMatch` - line filtering logic (exists)
- `Program.cs` - line expansion and formatting logic (needs extraction)
- `MarkdownHelpers` - code block formatting (exists)

**Solution: Expand LineHelpers (no new class needed)**

`LineHelpers` already exists in `src/cycodmd/Helpers/LineHelpers.cs` for line operations. Just add the missing methods there:

```csharp
class LineHelpers
{
    // Already exists:
    public static bool IsLineMatch(string line, List<Regex> includePatterns, List<Regex> excludePatterns)
    
    // Extract from Program.cs and add:
    public static string AddLineNumbers(string content)
    {
        // Move GetContentFormattedWithLineNumbers logic here
    }
    
    public static string FilterAndExpandContext(
        string content, 
        List<Regex> includePatterns, 
        List<Regex> excludePatterns,
        int linesBefore, 
        int linesAfter,
        bool includeLineNumbers,
        bool highlightMatches)
    {
        // Move GetContentFilteredAndFormatted logic here
        // This is the 81-line method in Program.cs (lines 644-725)
    }
}
```

This consolidates all line-level processing in one place, making it reusable by both tools.

#### Summary: Keep It Simple

- **Reuse existing helpers**: LineHelpers (expand it), MarkdownHelpers, FileHelpers, AiInstructionProcessor
- **Add concrete classes** when needed: ParallelProcessor, FoundTextFile
- **No interfaces** unless we actually need multiple implementations (YAGNI)

### Key Differences

| Aspect | cycodmd | cycodgr |
|--------|---------|---------|
| **Hierarchy** | 2 levels (files → lines) | 3 levels (repos → files → lines) |
| **Source** | Filesystem | GitHub API + Raw URLs |
| **Initial Search** | Glob patterns | GitHub search queries |
| **Repo Instructions** | N/A | `--repo-instructions` |
| **File Instructions** | `--file-instructions` | `--file-instructions` |
| **Final Instructions** | `--instructions` | `--instructions` |
| **Parallelism** | File level only | Repo level + file level |

---

## Section 4: Implementation Plan

### Phase 1: Create Shared Components ✅ COMPLETE

**Goal**: Build the concrete classes and helpers that both tools will use

**Status**: ✅ Complete (2025-01-14)

**Tasks**:
1. ✅ Create `FoundTextFile` class (data holder with lambda for loading content)
2. ✅ Create `ParallelProcessor` class (generic parallel processing with throttling)
3. ✅ Expand `LineHelpers` (extract methods from Program.cs: AddLineNumbers, FilterAndExpandContext)
4. ✅ Move `LineHelpers` to common (so both tools can use it)
5. ✅ Verify `AiInstructionProcessor` can be reused as-is (moved to common)

**Deliverable**: ✅ Concrete classes in common/shared library, ready for both tools to use

### Phase 2: Refactor cycodgr's Processing Logic ✅ COMPLETE

**Goal**: Refactor cycodgr's processing logic to use the new shared architecture

**Status**: ✅ Complete (2025-01-14)

**Why cycodgr first?**
- Newer codebase, less legacy complexity
- Can design it "right" from the start
- Learn what works before refactoring cycodmd

**Tasks**:
1. ✅ Implement `FoundTextFile` instances with lambdas for GitHub content loading
2. ✅ Implement repo finder (GitHub API search) - already existed
3. ✅ Implement file finder (GitHub code search, parallelized by repo using ParallelProcessor)
4. ✅ Implement content processor (download, filter using LineHelpers, expand, format)
5. ✅ Implement instruction pipeline (per-file, per-repo, final using AiInstructionProcessor)
6. ✅ Implement output handler (aggregation for instructions, direct output otherwise)
7. ✅ Fix line number bug (fetch full files, calculate real line numbers)

**Success Criteria**: ✅ ALL MET
- ✅ Correct line numbers in output
- ✅ Parallel processing at file levels (using ParallelProcessor)
- ✅ Complete three-level instruction pipeline working
  - ✅ File instructions (`--file-instructions`, `--EXT-file-instructions`)
  - ✅ Repo instructions (`--repo-instructions`)
  - ✅ Final instructions (`--instructions`)
- ✅ Instruction chaining (multiple instructions transform sequentially)
- ✅ Clean, testable, maintainable code

### Phase 3: Extract Shared Code (Mostly Complete) ⏳

**Goal**: Move any remaining shared code into common library

**Status**: Mostly done during Phase 1, tests remain

**Tasks**:
1. ✅ Ensure ParallelProcessor is in common/shared location
2. ✅ Ensure FoundTextFile is in common/shared location
3. ✅ Ensure LineHelpers (expanded) is in common (not just cycodmd)
4. ✅ Move AiInstructionProcessor to common
5. ⏳ Write MSTest unit tests for shared components in `tests/cycod/` (following existing test structure)
   - `ParallelProcessorTests.cs`
   - `FoundTextFileTests.cs`
   - `Helpers/LineHelpersTests.cs` (expand existing or create new)

**Deliverable**: Clean shared library that both tools reference

### Phase 4: Refactor cycodmd

**Goal**: Refactor cycodmd to use shared components

**Tasks**:
1. Create `FoundTextFile` instances with lambdas for filesystem content loading:
   ```csharp
   var file = new FoundTextFile {
       LoadContent = async () => await File.ReadAllTextAsync(fullPath)
   };
   ```
2. Refactor Program.cs to use ParallelProcessor for file processing
3. Use expanded LineHelpers for line filtering and expansion
4. Use AiInstructionProcessor (already does this)
5. Simplify Program.cs (move logic to helpers)
6. Maintain backward compatibility
7. Ensure all existing tests still pass

**Success Criteria**:
- cycodmd behavior unchanged for users
- Codebase much cleaner
- Sharing significant code with cycodgr (ParallelProcessor, LineHelpers, FoundTextFile)
- Program.cs simplified and focused on orchestration

### Phase 5: Future Enhancements

**Enabled by unified architecture**:
- Unified search (local + GitHub in same query)
- Consistent behavior across tools
- Easier to add new sources (HTTP, databases, etc.)
- Better testability
- Shared documentation

---

## Open Questions

1. **Repo-level parallelism throttling**: Should we limit concurrent repo searches separately from file downloads?

2. **Caching**: Should `FoundTextFile.Content` cache the downloaded content, or always re-fetch?

3. **Error handling**: How do we handle partial failures in parallel processing?

4. **Progress reporting**: How do we show progress when processing many repos/files in parallel?

5. **--repo-file-contains implementation**: This requires two-stage GitHub search:
   - First: Find repos with files containing X
   - Second: Within those repos, find files containing Y
   - How does this fit into the parallelism model?

6. **Instruction processor reuse**: Can we use the existing `AiInstructionProcessor` from cycodmd as-is, or does it need changes?

---

## Notes

- This architecture emerged from understanding the line number bug in cycodgr
- The bug revealed a deeper architectural opportunity
- Both tools have similar needs, should share more code
- Building the right abstractions will make both tools better
- Start with cycodgr (simpler), then refactor cycodmd (more complex)

---

**Next Steps**: Review and refine this document, then begin Phase 1 (Design Shared Abstractions)
