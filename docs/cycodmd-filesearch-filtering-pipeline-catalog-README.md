# cycodmd File Search Command - Filtering Pipeline Catalog

**[← Back to cycodmd Main Catalog](cycodmd-filtering-pipeline-catalog-README.md)**

## Command Overview

**Command Name**: File Search (Default Command)  
**CLI Usage**: `cycodmd [globs...]` or `cycodmd [options] [globs...]`  
**Implementation**: `FindFilesCommand` class  
**Source**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

### Purpose

The File Search command is the **default command** for cycodmd. It searches the local filesystem for files matching glob patterns, filters them by content, and processes the results. This is the primary command for working with local files.

### Key Features

- **Glob pattern matching** for file discovery
- **Content-based filtering** at both file and line levels
- **Time-based filtering** (modified, created, accessed)
- **Context expansion** around matching lines
- **Find and replace** functionality with preview/execute modes
- **AI-assisted processing** of matched files
- **Multiple output formats** and persistence options

---

## The 9-Layer Pipeline

### Layer 1: TARGET SELECTION
**Purpose**: Specify which files to search

**[Documentation](cycodmd-files-layer-1.md)** | **[Proof](cycodmd-files-layer-1-proof.md)**

**Key Options**:
- Positional arguments: glob patterns (`**/*.cs`, `src/**/*.md`)
- `--exclude`: patterns to exclude files
- `--modified`, `--created`, `--accessed`: time-based filtering
- `.cycodmdignore`: ignore file support

---

### Layer 2: CONTAINER FILTER
**Purpose**: Filter which files to include/exclude based on their content

**[Documentation](cycodmd-files-layer-2.md)** | **[Proof](cycodmd-files-layer-2-proof.md)**

**Key Options**:
- `--file-contains`: include files containing pattern
- `--file-not-contains`: exclude files containing pattern
- `--contains`: shorthand for both file and line filtering
- `--cs-file-contains`, `--py-file-contains`, etc.: extension-specific shortcuts

---

### Layer 3: CONTENT FILTER
**Purpose**: Filter which lines within files to show

**[Documentation](cycodmd-files-layer-3.md)** | **[Proof](cycodmd-files-layer-3-proof.md)**

**Key Options**:
- `--line-contains`: only show lines matching pattern
- `--contains`: shorthand for both file and line filtering
- `--highlight-matches`: visually highlight matching content

---

### Layer 4: CONTENT REMOVAL
**Purpose**: Remove specific content from display

**[Documentation](cycodmd-files-layer-4.md)** | **[Proof](cycodmd-files-layer-4-proof.md)**

**Key Options**:
- `--remove-all-lines`: remove lines matching pattern
- Applied before context expansion and display

---

### Layer 5: CONTEXT EXPANSION
**Purpose**: Expand output to show lines around matches

**[Documentation](cycodmd-files-layer-5.md)** | **[Proof](cycodmd-files-layer-5-proof.md)**

**Key Options**:
- `--lines N`: show N lines before AND after matches (symmetric)
- `--lines-before N`: show N lines before matches (asymmetric)
- `--lines-after N`: show N lines after matches (asymmetric)

---

### Layer 6: DISPLAY CONTROL
**Purpose**: Control how results are formatted and presented

**[Documentation](cycodmd-files-layer-6.md)** | **[Proof](cycodmd-files-layer-6-proof.md)**

**Key Options**:
- `--line-numbers`: show line numbers
- `--files-only`: show only file names (no content)
- `--highlight-matches` / `--no-highlight-matches`: control highlighting

---

### Layer 7: OUTPUT PERSISTENCE
**Purpose**: Save results to files

**[Documentation](cycodmd-files-layer-7.md)** | **[Proof](cycodmd-files-layer-7-proof.md)**

**Key Options**:
- `--save-output`: save combined output
- `--save-file-output`: save per-file output with templates
- `--save-chat-history`: save AI processing history

---

### Layer 8: AI PROCESSING
**Purpose**: AI-assisted analysis and processing of results

**[Documentation](cycodmd-files-layer-8.md)** | **[Proof](cycodmd-files-layer-8-proof.md)**

**Key Options**:
- `--instructions`: general AI instructions
- `--file-instructions`: AI instructions for all files
- `--cs-file-instructions`, `--py-file-instructions`: extension-specific instructions
- `--built-in-functions`: enable AI to use built-in functions

---

### Layer 9: ACTIONS ON RESULTS
**Purpose**: Perform operations on matched files/content

**[Documentation](cycodmd-files-layer-9.md)** | **[Proof](cycodmd-files-layer-9-proof.md)**

**Key Options**:
- `--replace-with`: specify replacement text for matches
- `--execute`: execute the replacement (vs. preview mode)

---

## Example Usage

### Basic File Search
```bash
cycodmd **/*.cs
```
Find all C# files.

### Content Filtering
```bash
cycodmd **/*.cs --line-contains "async"
```
Find all C# files and show only lines containing "async".

### With Context
```bash
cycodmd **/*.cs --line-contains "async" --lines 3
```
Show matching lines plus 3 lines of context before and after.

### Find and Replace
```bash
# Preview mode
cycodmd **/*.cs --line-contains "oldMethod" --replace-with "newMethod"

# Execute mode
cycodmd **/*.cs --line-contains "oldMethod" --replace-with "newMethod" --execute
```

### AI Processing
```bash
cycodmd **/*.cs --cs-file-instructions "Analyze this C# code for potential bugs"
```

---

## Source Code References

- **Command Implementation**: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`
- **Option Parsing**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (lines 100-303)
- **Positional Arg Parsing**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` (lines 457-460)
- **Execution**: `src/cycodmd/Program.cs` (`ProcessFindFilesCommand` method)

---

**[← Back to cycodmd Main Catalog](cycodmd-filtering-pipeline-catalog-README.md)**
