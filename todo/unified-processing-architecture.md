# Unified Processing Architecture for cycodgr and cycodmd

**Status**: ✅ **COMPLETE!** All Phases Done! (Completed: 2025-01-14)  
**Date Started**: 2025-01-11  
**Date Completed**: 2025-01-14

## 🎉 Full Implementation Complete Summary

All phases completed successfully!

### ✅ Phase 1: Shared Components (Complete)
- ✅ FoundTextFile (with lambda content loading)
- ✅ LineHelpers (expanded with filtering, context, line numbers)
- ✅ ParallelProcessor (generic parallel processing with throttling)
- ✅ AiInstructionProcessor (moved to common, shared by both tools)

### ✅ Phase 2: cycodgr Refactored (Complete)
- ✅ Fixed line number bug (real line numbers from full files)
- ✅ Parallel file processing (using ParallelProcessor)
- ✅ File instructions (`--file-instructions`, `--EXT-file-instructions`)
- ✅ Repo instructions (`--repo-instructions`)
- ✅ Final/global instructions (`--instructions`)
- ✅ Instruction chaining (multiple instructions transform sequentially)

### ✅ Phase 3: Tests for Shared Components (Complete)
- ✅ **FoundTextFileTests.cs**: 20 comprehensive tests
  - Basic construction, lazy loading, metadata, edge cases, real-world scenarios
- ✅ **ParallelProcessorTests.cs**: 21 comprehensive tests
  - Basic functionality, parallelism behavior, throttling, error handling, stress tests
- ✅ **LineHelpersTests.cs**: 36 comprehensive tests
  - IsLineMatch, AddLineNumbers, FilterAndExpandContext with all features
- ✅ **Total: 77 passing tests** covering all shared components

### ✅ Phase 4: cycodmd Refactored (Complete)
- ✅ Replaced `GetContentFormattedWithLineNumbers` with `LineHelpers.AddLineNumbers`
- ✅ Replaced `GetContentFilteredAndFormatted` with `LineHelpers.FilterAndExpandContext`
- ✅ Replaced manual `SemaphoreSlim` with `ParallelProcessor` throughout
- ✅ Simplified `Program.cs` (removed ~80 lines of duplicate code)
- ✅ Maintained backward compatibility (all existing functionality works)
- ✅ Both tools now share the same line filtering and parallelism logic

### ✅ Phase 5: Benefits Realized
- ✅ **Code reuse**: Both tools use shared components (LineHelpers, ParallelProcessor, AiInstructionProcessor)
- ✅ **Consistent behavior**: Same filtering and processing logic across tools
- ✅ **Better testability**: Shared components have comprehensive test coverage  
- ✅ **Simpler code**: Cleaner, more maintainable codebase
- ✅ **Unified architecture**: Clear patterns for future enhancements

**Commits Made**: 8 major commits
1. Phase 1 - Created shared components and fixed line numbers
2. Added parallel file processing
3. Added file instructions support
4. Added repo instructions support  
5. Added final/global instructions support
6. Phase 3 - Added comprehensive tests for shared components (77 tests)
7. Phase 4 Part 1-2 - Refactored cycodmd to use LineHelpers
8. Phase 4 Part 3-4 - Refactored cycodmd to use ParallelProcessor

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

## Section 5: Complete Filter Architecture and Save/Resume Workflow

**Status**: 📋 **PLANNED** - Not yet implemented  
**Date Designed**: 2025-01-15

### Overview

This section documents the complete three-level filtering architecture for cycodgr, enabling surgical precision in GitHub searches and progressive refinement through saved results.

### The Three-Level Filter Hierarchy

```
Repos → Files → Lines
  ↓       ↓       ↓
Level 1  Level 2  Level 3
```

### Filter Types and Their Scope

#### 1. Universal Broadcast Filter: `--contains`

**Applies to ALL levels simultaneously:**
- Repo-level: Repos must contain the text
- File-level: Files must contain the text
- Line-level: Lines must contain the text

**Example:**
```bash
cycodgr --contains "authentication"
```

This is the "search everything" filter that broadcasts across the entire hierarchy.

#### 2. Dual-Purpose Filters: `--file-contains` and `--(ext)-file-contains`

**Used for BOTH repo selection AND file selection:**

When repo pre-filtering is active (e.g., `--repo-csproj-file-contains` is present):
- Acts as additional **file selection** within pre-filtered repos

When no repo pre-filtering:
- Acts as **repo selection** (find repos with files containing X)
- AND as **file selection** (find files containing X within those repos)

**Examples:**
```bash
# File selection only (repos already filtered)
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "anthropic"

# Both repo and file selection (no repo pre-filter)
cycodgr --cs-file-contains "anthropic"
```

**Extension-specific variants:**
- `--cs-file-contains` - C# files
- `--js-file-contains` - JavaScript files
- `--py-file-contains` - Python files
- `--md-file-contains` - Markdown files
- etc. (following existing extension shortcuts)

#### 3. Repo Selection Only: `--repo-file-contains` and `--repo-(ext)-file-contains`

**Used ONLY for repo pre-filtering:**

These are pure repo filters that find repositories containing files with specific content. They do NOT affect file or line filtering.

**Generic version:**
```bash
--repo-file-contains "text"  # Any file containing "text"
```

**Extension-specific versions:**
```bash
--repo-csproj-file-contains "Microsoft.Extensions.AI"  # .csproj files only
--repo-json-file-contains "express"                    # .json files only  
--repo-yaml-file-contains "kubernetes"                 # .yaml/.yml files only
--repo-py-file-contains "tensorflow"                   # .py files only
--repo-md-file-contains "Quick Start"                  # .md files only
```

**Use Cases:**
- Find C# projects using a specific NuGet package
- Find Node.js projects using a specific npm package
- Find projects with specific configuration in YAML files
- Find projects with specific documentation

#### 4. Line Filtering Only: `--line-contains`

**Used ONLY for display filtering:**

Filters which lines are shown in the output. Works with `--lines N` to provide context.

**Example:**
```bash
cycodgr --cs-file-contains "anthropic" \
        --line-contains "AsChatClient" \
        --lines 20
```

### Complete Example: Three-Level Search

**Goal**: Find how Anthropic clients are converted to IChatClient in projects using Microsoft.Extensions.AI

```bash
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "anthropic" \
        --line-contains "AsChatClient" \
        --lines 20
```

**What happens:**
1. **Level 1 (Repos)**: Find repos with .csproj files containing "Microsoft.Extensions.AI"
2. **Level 2 (Files)**: Within those repos, find .cs files containing "anthropic"
3. **Level 3 (Lines)**: Within those files, show lines containing "AsChatClient" with 20 lines of context

### Save/Resume Workflow

#### Save Options at Each Level

**Repo-level saves:**
```bash
--save-repos repos.txt         # Saves owner/name format (one per line)
--save-repo-urls urls.txt      # Saves clone URLs (https://github.com/owner/name.git)
```

**File-level saves:**
```bash
--save-file-paths paths.txt    # Saves qualified paths (owner/repo:src/Program.cs)
--save-file-urls urls.txt      # Saves raw GitHub URLs (https://raw.githubusercontent.com/...)
```

**Output saves (existing):**
```bash
--save-output output.md        # Saves formatted markdown output
--save-json data.json          # Saves structured JSON data
--save-csv data.csv            # Saves CSV format
--save-table table.txt         # Saves table format
--save-urls urls.txt           # Saves GitHub file URLs (existing functionality)
```

#### Resume/Restart from Saved Results

**Loading from files using `@` syntax:**

```bash
# Load repos from file
cycodgr @repos.txt --cs-file-contains "something"

# Or using --repos flag
cycodgr --repos @repos.txt --cs-file-contains "something"

# Load file paths from file (future feature)
cycodgr --file-paths @files.txt --line-contains "pattern"

# Load file URLs from file (future feature)
cycodgr --file-urls @urls.txt --line-contains "pattern"
```

**Note**: The `@filename` syntax for `--repos` already exists (see `CycoGrCommandLineOptions.cs` lines 351-362). Need to extend this pattern to file paths and URLs.

#### Progressive Refinement Pipeline

**Phase 1: Discovery - Find relevant repos**
```bash
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "Anthropic" \
        --save-repos ai-anthropic-repos.txt \
        --save-file-paths ai-anthropic-files.txt \
        --save-output phase1-discovery.md
```

**Phase 2: Deep Dive - Analyze specific patterns**
```bash
cycodgr @ai-anthropic-repos.txt \
        --line-contains "AsChatClient" \
        --lines 30 \
        --save-output phase2-conversion-patterns.md
```

**Phase 3: Focused Analysis - Specific files**
```bash
cycodgr --file-paths @ai-anthropic-files.txt \
        --line-contains "Configure" \
        --instructions "Summarize the configuration patterns for Anthropic client setup"
```

**Phase 4: Share Results - Team collaboration**
```bash
# Share repo list with team
cat ai-anthropic-repos.txt
# Team members can use the same list
cycodgr @ai-anthropic-repos.txt --cs-file-contains "different-pattern"
```

### Benefits of This Architecture

1. **Surgical Precision**: Three-level filtering eliminates noise
2. **Efficiency**: Pre-filtering repos saves API calls and time
3. **Composability**: Mix and match filters at each level
4. **Cacheability**: Save expensive searches, reuse results
5. **Reproducibility**: Share repo/file lists with team
6. **Incrementality**: Build complex queries step-by-step
7. **Pipeline-Friendly**: Chain searches with Unix-style composition

### Implementation Notes

#### Existing Support

**Already implemented:**
- `--repo-contains` (repo metadata filtering)
- `--file-contains` (file content filtering)
- `--contains` (general search term)
- `--line-contains` (line filtering for display)
- `--lines` (context lines)
- `--save-repos` loading via `@repos.txt` syntax

#### Needs Implementation

**Repo selection filters:**
- `--repo-file-contains` (generic file content)
- `--repo-csproj-file-contains`
- `--repo-json-file-contains`
- `--repo-yaml-file-contains`
- `--repo-py-file-contains`
- `--repo-md-file-contains`
- etc. (all extension variants)

**Extension-specific file filtering:**
- `--cs-file-contains`
- `--js-file-contains`
- `--py-file-contains`
- `--md-file-contains`
- etc. (all extension variants)

**Save options:**
- `--save-repos` (save format)
- `--save-repo-urls` (clone URLs)
- `--save-file-paths` (qualified paths)
- `--save-file-urls` (raw URLs)

**Load options:**
- `--file-paths @files.txt`
- `--file-urls @urls.txt`

#### Technical Challenges

**Two-stage GitHub API Search:**

For repo pre-filtering, we need:
1. **Stage 1**: GitHub code search to find repos with files containing X
   - Query: `extension:csproj Microsoft.Extensions.AI`
   - Result: List of repo names
2. **Stage 2**: Within those repos, search for files
   - Regular file search within pre-filtered repo list

**Dual behavior of `--file-contains`:**
- Need logic to determine if it acts as repo filter or only file filter
- Decision based on presence of `--repo-*-file-contains` flags

**File format for saved paths:**
- Qualified format: `owner/repo:src/Program.cs`
- Enables reloading specific files across multiple repos
- Need parser for this format

**Rate limiting and parallelism:**
- Repo pre-filtering is sequential (one API call)
- File searches within repos can be parallel
- Need to balance API rate limits with speed

### Future Enhancements

**Additional filter levels:**
- `--repo-stars-min`, `--repo-stars-max` (numeric filters)
- `--repo-updated-after`, `--repo-updated-before` (date filters)
- `--file-size-min`, `--file-size-max` (file size filters)

**Advanced save/load:**
- `--save-results results.json` (complete structured results)
- `--load-results results.json` (resume from checkpoint)
- Binary cache format for faster loading

**Query language:**
- Consider SQL-like syntax for complex queries
- Boolean operators: AND, OR, NOT across filters
- Parenthetical grouping for complex logic

**Pipeline operators:**
- Unix-style pipe chaining: `cycodgr ... | cycodgr-refine ...`
- Streaming results between stages

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
