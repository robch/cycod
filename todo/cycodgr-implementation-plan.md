# TODO: cycodgr Implementation Plan

**Status**: Ready to implement  
**Created**: 2025-12-11  
**Goal**: Rename cycodgh → cycodgr and restructure with `repo` and `code` verbs

---

## Overview

Rename `cycodgh` → `cycodgr` (cycod-github-repo / "dig for" repos) and restructure into two focused verbs with better defaults and output formats.

**Name significance**: 
- "cycod-github-repo" 
- "dig for" (digger) - wordplay on searching/digging through repos

---

## Phase 1: Restructure to `repo` and `code` Verbs

### 1.1: Rename Project
- [x] Rename folder: `src/cycodgh` → `src/cycodgr`
- [x] Rename csproj: `cycodgh.csproj` → `cycodgr.csproj`
- [x] Rename namespace: `CycoGh` → `CycoGr`
- [x] Rename all classes: `CycoGh*` → `CycoGr*`
- [x] Update branch name
- [x] Update README references
- [x] Update help files

### 1.2: Implement `cycodgr repo` Verb

**Purpose**: Search for repositories by name/description/topics

**Command**: `cycodgr repo <keywords> [options]`

**Default format**: `detailed` (not `url` - breaking change but more useful)

**Output example**:
```
https://github.com/dotnet/aspire | ⭐ 8.5k | C# | .NET Aspire for cloud-native apps
https://github.com/thangchung/practical-dotnet-aspire | ⭐ 412 | C# | Practical examples...
```

**Tasks**:
- [x] Create `RepoCommand.cs` (copy from SearchCommand, rename)
- [x] Remove old `SearchCommand.cs`
- [x] Update command line parser to recognize `repo` verb
- [x] Change default format from `url` to `detailed`
- [x] Keep existing formats: `url`, `table`, `json`, `csv`
- [x] Update help documentation

**Example usage**:
```bash
cycodgr repo "dotnet aspire" --max-results 10
cycodgr repo "ai agents" --format table
```

### 1.3: Implement `cycodgr code` Verb

**Purpose**: Search for code/files containing text

**Command**: `cycodgr code <keywords> [options]`

**Default format**: `detailed` - Match cycodmd/FindInFiles output style

**Output example**:
```markdown
# microsoft/vscode (⭐ 152k | TypeScript)

## src/vs/platform/agents/common/agentsConfig.ts

````typescript
  45: export const AGENTS_FILE = 'AGENTS.md';
* 46: const agentsPath = path.join(rootPath, AGENTS_FILE);
  47: return fs.readFile(agentsPath, 'utf8');
````

## extensions/agents/src/agentsProvider.ts

````typescript
  23: import { AGENTS_FILE } from './config';
* 24: const content = await loadFile(AGENTS_FILE);
  25: return parseAgents(content);
````
```

**Key formatting requirements**:
- `#` Repo header (name, stars, language)
- `##` File paths (relative to repo root)
- Code fences with language detection (````typescript, ````csharp, etc.)
- Line numbers with `:` separator (right-aligned)
- `*` in column 1 for lines matching search pattern
- Context lines (5 before/after by default)
- Configurable with `--lines-before-and-after N` or `--lines N`

**Tasks**:
- [x] Create `CodeCommand.cs`
- [x] Update command line parser to recognize `code` verb
- [x] Implement detailed formatter with code fences and `*` markers
- [x] Study cycodmd's output format (reference: `FindInFiles` tool output)
- [x] Detect language from file extension for code fence syntax
- [x] Add line number formatting (right-aligned with `:`)
- [x] Add context line support (`--lines-before-and-after N`)
- [x] Implement other formats:
  - [x] `filenames` - Just relative file paths grouped by repo
  - [x] `files` - Raw file URLs (raw.githubusercontent.com)
  - [x] `repos` - Unique repo URLs only
  - [x] `urls` - BOTH repo URLs AND file URLs (nested)
  - [x] `json` - Structured with matches array
  - [x] `csv` - Flattened rows
- [x] Update help documentation

**Example usage**:
```bash
cycodgr code "AGENTS.md" --in-files cs --max-results 10
cycodgr code "error handling" --format filenames
cycodgr code "config" --format files --lines 3
```

**Reference implementation**: 
- `cycodmd README.md --contains clone --lines 5 --line-numbers`
- `FindInFiles` tool with search pattern

---

## Phase 2: Save Format Shortcuts

Add convenient save flags that work alongside `--format`.

**Behavior**: Can save multiple formats at once

**Example**:
```bash
cycodgr repo "dotnet aspire" --format detailed --save-json results.json --save-csv data.csv
```

**Tasks**:
- [x] Add `--save-json FILE` flag
- [x] Add `--save-csv FILE` flag
- [x] Add `--save-table FILE` flag
- [x] Add `--save-urls FILE` flag
- [x] Update command line parser
- [x] Allow multiple save flags simultaneously
- [x] Each saves independently of `--format` setting
- [x] Update help documentation

**Example usage**:
```bash
# Save in multiple formats
cycodgr code "AGENTS.md" --save-json search.json --save-csv search.csv --save-urls urls.txt

# Works with --format
cycodgr repo "aspire" --format table --save-json data.json
```

---

## Phase 3: Repository Targeting

### 3.1: `--repo` / `--repos` Flags

Search WITHIN specific repositories (not "all GitHub then filter").

**Pattern**: Like cycod's `--input` (single) vs `--inputs` (multiple)

**Tasks**:
- [x] Add `--repo OWNER/REPO` flag (single, repeatable)
- [x] Add `--repos REPO1 REPO2 @FILE` flag (multiple in one)
- [x] Support `@file.txt` syntax to load repo list from file
- [x] Update GitHub search query to use `repo:owner/name` qualifier
- [x] Ensure searches ONLY within specified repos
- [x] Update command line parser
- [x] Update help documentation

**Example usage**:
```bash
# Single repo
cycodgr code "AGENTS.md" --repo microsoft/vscode

# Multiple repos (repeatable flag)
cycodgr code "AGENTS.md" --repo microsoft/vscode --repo dotnet/aspire

# Multiple repos (single flag)
cycodgr code "AGENTS.md" --repos microsoft/vscode dotnet/aspire openai/codex

# From file
cycodgr code "AGENTS.md" --repos @my-repos.txt
```

**Implementation note**: Uses GitHub's `repo:` search qualifier - searches within those repos, not all GitHub.

---

## Phase 4: URL Filtering (Post-Search)

Filter results AFTER search using regex patterns.

**Pattern**: Match cycodmd's `--exclude` flag

**Decision needed**: Add `--include` for symmetry, or just `--exclude` like cycodmd?

**Tasks**:
- [x] Add `--exclude PATTERN [PATTERN...]` flag
- [x] Support multiple patterns
- [x] Apply regex matching to repo URLs
- [x] Filter results after GitHub search
- [x] Decide: Add `--include` flag? (not in cycodmd but might be useful) - DECISION: Skip for now, can add later if needed
- [x] Update command line parser
- [x] Update help documentation

**Example usage**:
```bash
# Exclude URLs containing "fork"
cycodgr code "error handling" --exclude "fork"

# Exclude multiple patterns
cycodgr repo "aspire" --exclude "archived" "deprecated"

# Optional: Include only matching patterns
cycodgr code "agents" --include "microsoft" "openai"
```

**Note**: This filters AFTER search. Use `--repo`/`--repos` to target specific repos BEFORE search.

---

## Phase 5: Additional Filters

### GitHub Search Capabilities

**Tasks**:
- [x] Add `--owner ORG` filter (GitHub supports this natively)
- [x] Verify `--language LANG` still works (already implemented)
- [x] Add `--exclude-fork` flag (inverse of existing `--include-forks`)
- [x] Add `--only-forks` flag
- [x] Add `--min-stars N` / `--stars >N` filter
  - [x] Check if GitHub supports in query, otherwise post-filter
- [x] Update command line parser
- [x] Update help documentation

**Example usage**:
```bash
cycodgr repo "aspire" --owner microsoft
cycodgr code "AGENTS.md" --language csharp --min-stars 100
cycodgr repo "ai" --exclude-fork
```

---

## Phase 6: Query Syntax Verification

**Goal**: Make multi-keyword searches "just work" intuitively

**Tasks**:
- [ ] Verify how `gh search` handles multiple keywords (AND vs OR)
- [ ] Verify exact phrase matching (quoted strings)
- [ ] Document behavior in help
- [ ] Ensure we follow GitHub's natural search behavior
- [ ] Don't overthink - match what `gh search` does by default

**Test cases**:
```bash
# Multiple keywords - what happens?
cycodgr repo foo bar

# Quoted phrase - exact match?
cycodgr code "exact phrase match"

# Mixed
cycodgr code "error handling" retry
```

---

## Phase 7: AI Instructions Pipeline

Integrate with cycod for AI-powered analysis, following cycodmd's pattern exactly.

### Pattern from cycodmd

**Reference**: `cycodmd help find instructions --expand`

**Key patterns**:
- `--file-instructions` - Apply to each file
- `--EXTENSION-file-instructions` - Apply to specific file types (e.g., `--cs-file-instructions`)
- `--instructions` - Apply to aggregated output
- `@file.md` syntax to load from file
- Can chain multiple steps

### For cycodgr

**Tasks**:
- [ ] Add `--repo-instructions` flag (apply to each repo found)
- [ ] Add `--file-instructions` flag (apply to each file found in code search)
- [ ] Add `--instructions` flag (apply to final aggregated output)
- [ ] Support `@file.md` syntax
- [ ] Support multiple instruction steps (pipeline)
- [ ] Integrate with cycod like cycodmd does
- [ ] Update command line parser
- [ ] Update help documentation
- [ ] Add help topic: `cycodgr help repo instructions`
- [ ] Add help topic: `cycodgr help code instructions`

**Example usage**:
```bash
# Analyze each repo
cycodgr repo "ai agents" --repo-instructions "analyze code quality and documentation"
cycodgr repo "ai agents" --repo-instructions @repo-analysis.md

# Analyze each file
cycodgr code "AGENTS.md" --file-instructions "summarize how this file is used"
cycodgr code "error handling" --file-instructions @file-summary.md

# Analyze final output
cycodgr repo "aspire" --instructions "create a comparison table"
cycodgr code "AGENTS.md" --instructions "find best practices patterns"

# Multi-step pipeline
cycodgr code "config" --file-instructions @step1.md @step2.md --instructions @final-summary.md
```

---

## Implementation Order Checklist

- [x] **Phase 1**: Rename and restructure verbs
  - [x] 1.1: Rename project (cycodgh → cycodgr)
  - [x] 1.2: Implement `repo` verb with detailed default
  - [x] 1.3: Implement `code` verb with detailed format (code fences, `*` markers)
  
- [x] **Phase 2**: Save format shortcuts
  - [x] Add `--save-json`, `--save-csv`, `--save-table`, `--save-urls`
  
- [x] **Phase 3**: Repository targeting
  - [x] Add `--repo` / `--repos` (search within specific repos)
  
- [x] **Phase 4**: URL filtering
  - [x] Add `--exclude` (post-search filtering)
  
- [ ] **Phase 5**: Additional filters
  - [ ] Add `--owner`, `--exclude-fork`, `--min-stars`, etc.
  
- [ ] **Phase 6**: Query syntax
  - [ ] Verify and document multi-keyword behavior
  
- [ ] **Phase 7**: AI instructions
  - [ ] Add `--repo-instructions`, `--file-instructions`, `--instructions`

---

## Testing Checklist

### Basic Functionality
- [ ] `cycodgr repo "dotnet aspire"` - basic repo search
- [ ] `cycodgr code "AGENTS.md" --in-files cs` - basic code search
- [ ] Default format is `detailed` for both verbs
- [ ] All format options work: url, filenames, files, repos, urls, table, json, csv

### Code Search Formatting
- [ ] Line numbers display correctly
- [ ] `*` appears in column 1 for matching lines
- [ ] Code fences with correct language
- [ ] Context lines (5 before/after)
- [ ] `--lines N` changes context
- [ ] Repo headers with stars/language
- [ ] File paths relative to repo root

### Save Shortcuts
- [ ] `--save-json` creates correct JSON
- [ ] `--save-csv` creates correct CSV
- [ ] Multiple save flags work together
- [ ] Saved files have correct content

### Repository Targeting
- [ ] `--repo owner/name` searches only that repo
- [ ] `--repos repo1 repo2` searches multiple repos
- [ ] `--repos @file.txt` loads from file
- [ ] Results only from specified repos

### Filtering
- [ ] `--exclude pattern` removes matching repos
- [ ] Multiple exclude patterns work
- [ ] `--owner` filters correctly
- [ ] `--min-stars` filters correctly
- [ ] `--exclude-fork` works

### Instructions Pipeline
- [ ] `--repo-instructions` processes each repo
- [ ] `--file-instructions` processes each file
- [ ] `--instructions` processes final output
- [ ] `@file.md` loads instructions from file
- [ ] Multiple steps chain correctly

---

## Questions/Decisions

### Open Questions
1. **Include flag**: Add `--include` for symmetry with `--exclude`? (cycodmd only has `--exclude`)
2. **Stars filtering**: `--stars >100` or `--min-stars 100` or both?
3. **Extension-specific instructions**: Should we support `--cs-file-instructions`, `--md-file-instructions` like cycodmd?

### Decided
- ✅ Tool name: `cycodgr` (cycod-github-repo / "dig for")
- ✅ Remove `search` verb entirely (no backward compat)
- ✅ Default format: `detailed` for both `repo` and `code`
- ✅ Code format: Match cycodmd/FindInFiles style with `*` markers
- ✅ Filtering: Use `--exclude` like cycodmd
- ✅ Repository targeting: `--repo` (single) and `--repos` (multiple)

---

## Reference Files

**Study these for implementation patterns**:
- `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` - Parsing patterns
- `src/cycodmd/CommandLineCommands/*.cs` - Command structure
- `FindInFiles` tool output - Code search format reference
- `src/cycod/CommandLine/CommandLineOptions.cs` - `--input` vs `--inputs` pattern

**Help references**:
- `cycodmd help find instructions --expand` - Instructions pipeline pattern
- `cycodmd help web --expand` - URL filtering with `--exclude`

---

## Notes

- The default format change (url → detailed) is a breaking change, but the tool is new so it's fine
- Code search detailed format is the most complex part - study cycodmd output carefully
- Repository targeting (`--repo`/`--repos`) is the most valuable feature - prioritize this
- Instructions pipeline is powerful but complex - save for last

---

**Status**: Ready to begin Phase 1
**Next action**: Start renaming cycodgh → cycodgr
