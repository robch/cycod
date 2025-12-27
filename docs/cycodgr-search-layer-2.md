# cycodgr search - Layer 2: CONTAINER FILTERING

## Purpose

Layer 2 defines **which containers (repositories and files) to include or exclude** based on their content, type, or properties. This is a multi-level filtering layer that operates at:

1. **Repository level**: Filter which repositories to search
2. **File level**: Filter which files within repositories to include
3. **Extension/Language level**: Filter by programming language or file extension

## Overview

Container filtering in cycodgr has **three distinct behavioral modes**:

### Mode 1: Pre-Filtering (Repository Discovery)
Some options **discover repositories** by searching for files with specific content, then use those repos as the target set.

### Mode 2: File-Level Filtering
Some options **filter files within repositories** after repos are already selected.

### Mode 3: Dual Behavior
Some options have **context-dependent behavior** - they pre-filter repos OR filter files, depending on what other options are present.

---

## Repository-Level Container Filtering

These options filter at the **repository level** - determining which repositories to search.

### `--repo-contains <term>`

**Purpose**: Search for repositories whose **metadata** contains the term (name, description, topics, README)

**Examples**:
```bash
cycodgr --repo-contains "terminal emulator"
cycodgr --repo-contains "machine learning" --min-stars 500
```

**Behavior**:
- Searches repository metadata only (not code)
- Uses GitHub's repository search API
- Stored in `SearchCommand.RepoContains` (string)
- Triggers `HandleRepoSearchAsync()` execution path

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#repo-contains)

---

### `--repo-file-contains <term>`

**Purpose**: **Pre-filter repositories** by searching for files containing the term, then return the unique set of repositories

**Examples**:
```bash
cycodgr --repo-file-contains "Microsoft.Extensions" --repo-csproj-file-contains "SDK"
```

**Behavior**:
- **Phase 1**: Uses GitHub code search to find files containing term
- **Phase 2**: Extracts unique repository names from results
- **Phase 3**: Adds repositories to the search scope (`command.Repos`)
- **Subsequent searches**: Use these repos as the target set
- Stored in `SearchCommand.RepoFileContains` (string)
- Executed in `Program.HandleSearchCommandAsync()` lines 76-102

**Key Characteristic**: This is a **pre-filtering stage** - it discovers repos before the main search.

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#repo-file-contains)

---

### `--repo-{ext}-file-contains <term>`

**Purpose**: Pre-filter repositories by searching for files **of a specific extension** containing the term

**Examples**:
```bash
cycodgr --repo-csproj-file-contains "Microsoft.AI"
cycodgr --repo-md-file-contains "installation instructions"
cycodgr --repo-json-file-contains '"type": "module"'
```

**Behavior**:
- Extension-specific variant of `--repo-file-contains`
- Extracts extension from option name: `--repo-{ext}-file-contains` → extension = `{ext}`
- Maps extension to language: `csproj` → `csharp`, `md` → `markdown`, etc.
- Stored in:
  - `SearchCommand.RepoFileContains` (search term)
  - `SearchCommand.RepoFileContainsExtension` (extension/language)
- Passed to `GitHubSearchHelpers.SearchCodeForRepositoriesAsync()`
- Uses GitHub's `--extension` parameter to filter by file type

**Extension Mapping**:
- `csproj` → language filter (csharp project files)
- `md` → language filter (markdown)
- `cs` → language filter (C# code)
- `py` → language filter (Python)
- `json`, `yaml`, `xml`, etc. → respective language filters

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#repo-ext-file-contains)

---

## File-Level Container Filtering

These options filter at the **file level** - determining which files to include in results.

### `--file-contains <term>` (Dual Behavior)

**Purpose**: Search for files containing the term - has **context-dependent behavior**

**Examples**:
```bash
# Behavior 1: Pre-filter repos (no repos specified)
cycodgr --file-contains "ConPTY"

# Behavior 2: Filter files (repos already specified)
cycodgr microsoft/terminal --file-contains "ConPTY"
```

**Behavior - Mode 1 (Pre-filtering)**:
- **Triggered when**: No repos specified (no `RepoPatterns`, no `--repos`, no `--repo-file-contains`)
- **Action**: Uses code search to discover repositories
- **Result**: Adds repos to `command.Repos` list
- **Subsequent**: Then searches those repos for file content
- Executed in `Program.HandleSearchCommandAsync()` lines 106-136

**Behavior - Mode 2 (File filtering)**:
- **Triggered when**: Repos already specified
- **Action**: Searches for files within those repos
- **Result**: Returns code matches from those repos only
- Executed in `HandleCodeSearchAsync()`

**Storage**:
- `SearchCommand.FileContains` (string)
- `SearchCommand.Language` (optional, affects filter)

**Key Insight**: This option has **dual behavior** to provide intuitive UX:
- "Find me repos with this content" (no repo context)
- "Find me files in these repos with this content" (repo context exists)

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#file-contains)

---

### `--{ext}-file-contains <term>`

**Purpose**: Search for files **of a specific extension** containing the term

**Examples**:
```bash
cycodgr --cs-file-contains "async Task"
cycodgr --py-file-contains "import tensorflow"
cycodgr --md-file-contains "# Installation"
```

**Behavior**:
- Extension-specific variant of `--file-contains`
- Extracts extension from option name: `--{ext}-file-contains` → extension = `{ext}`
- Automatically sets `SearchCommand.Language` based on extension
- Stored in:
  - `SearchCommand.FileContains` (search term)
  - `SearchCommand.Language` (derived from extension)
- Uses GitHub's `--language` parameter

**Extension to Language Mapping** (from parser):
```
cs → csharp
js → javascript
ts → typescript
py → python
rb → ruby
rs → rust
kt → kotlin
cpp / c++ → cpp
yml → yaml
md → markdown
(others pass through)
```

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#ext-file-contains)

---

### `--language <lang>` / `--extension <ext>` / `--in-files <ext>`

**Purpose**: Filter files by programming language or extension

**Examples**:
```bash
cycodgr --file-contains "async" --language python
cycodgr --file-contains "import" --extension py
cycodgr --file-contains "const" --in-files ts
```

**Behavior**:
- All three options are **aliases** - they set the same property
- Stored in `SearchCommand.Language` (string)
- Passed to GitHub API `--language` or `--extension` parameter
- Affects both repository and code searches
- Can be set explicitly or implicitly (via `--{ext}-file-contains`)

**Parsing**:
- `--extension` and `--in-files` map extension to language using `MapExtensionToLanguage()`
- `--language` passes value through directly

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#language-extension)

---

### Language Shortcuts

**Purpose**: Convenient shortcuts for common languages

**Tier 1 (Primary)**:
```bash
--cs, --csharp       → language: csharp
--js, --javascript   → language: javascript
--ts, --typescript   → language: typescript
--py, --python       → language: python
--java               → language: java
--go                 → language: go
--md, --markdown     → language: markdown
```

**Tier 2 (Popular)**:
```bash
--rb, --ruby         → language: ruby
--rs, --rust         → language: rust
--php                → language: php
--cpp, --c++         → language: cpp
--swift              → language: swift
--kt, --kotlin       → language: kotlin
```

**Tier 3 (Config/Markup)**:
```bash
--yml, --yaml        → language: yaml
--json               → language: json
--xml                → language: xml
--html               → language: html
--css                → language: css
```

**Behavior**:
- Each shortcut directly sets `SearchCommand.Language`
- Can be combined with `--file-contains` or used standalone
- Affects GitHub API language filtering

**Examples**:
```bash
cycodgr --cs --file-contains "async Task"
cycodgr microsoft/terminal --rust --file-contains "unsafe"
cycodgr --json --file-contains '"type":'
```

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#language-shortcuts)

---

### `--file-path <path>` / `--file-paths <path...>`

**Purpose**: Filter results to specific file paths within repositories

**Examples**:
```bash
cycodgr microsoft/terminal --file-path src/cascadia/TerminalCore/Terminal.cpp
cycodgr --repos @repos.txt --file-paths @paths.txt
```

**Behavior**:
- **Post-search filtering**: Applied AFTER code search results are returned
- Filters `CodeMatch` results by path matching
- Supports multiple matching strategies:
  - Exact match: `m.Path == fp`
  - Suffix match: `m.Path.EndsWith(fp)`
  - Contains match: `m.Path.Contains(fp)`
- `--file-paths` supports `@file` loading (newline-separated paths)
- Stored in `SearchCommand.FilePaths` (List<string>)
- Applied in `Program.HandleCodeSearchAsync()` lines 404-409

**File Loading Behavior**:
- `@file` references are expanded by command-line parser
- Handles both `\n` and `\r\n` line endings
- Trims whitespace from paths
- Skips empty lines

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#file-path)

---

## Exclusion Filtering

### `--exclude <pattern...>`

**Purpose**: Exclude results matching regex patterns

**Examples**:
```bash
cycodgr --file-contains "test" --exclude ".*/tests/.*"
cycodgr --repo-contains "ml" --exclude ".*archive.*"
```

**Behavior**:
- Accepts multiple regex patterns (space-separated)
- **Applied to repository URLs** for filtering
- Uses `ApplyExcludeFilters()` function (Program.cs line 1343)
- Stored in `CycoGrCommand.Exclude` (List<string>)
- Applied **after** search results are returned
- Case-insensitive regex matching

**Application Context**:
- Repository search: filters repo URLs
- Code search: filters repo URLs (not file paths)
- Applied to both `RepoInfo` and `CodeMatch` results

**Source Reference**: See [Layer 2 Proof](cycodgr-search-layer-2-proof.md#exclude)

---

## Interaction Between Options

### Pre-Filtering Chain

```
--repo-file-contains / --repo-{ext}-file-contains
    ↓ (discovers repos via code search)
    ↓ (extracts unique repository names)
    ↓ (adds to command.Repos)
    ↓
--file-contains (if no repos specified)
    ↓ (discovers more repos via code search)
    ↓ (adds to command.Repos)
    ↓
Combined repo list (RepoPatterns + Repos)
    ↓ (used as target for main search)
    ↓
--repo-contains / --file-contains (main search)
```

### File Filtering Chain

```
Repository Selection
    ↓
--language / --{ext} (applied during GitHub search)
    ↓
Code Search Results
    ↓
--file-paths (post-search filtering)
    ↓
--exclude (post-search exclusion)
    ↓
Final Results
```

---

## Search Execution Paths

### Path 1: Repository Metadata Search
**Triggered by**: `--repo-contains` (without code search)

```
User: cycodgr --repo-contains "terminal"
    ↓
Program.HandleSearchCommandAsync() → HandleRepoSearchAsync()
    ↓
GitHubSearchHelpers.SearchRepositoriesAsync()
    ↓
gh search repos "terminal"
    ↓
Repository results
```

### Path 2: Code Search with Pre-Filtering
**Triggered by**: `--repo-file-contains` or `--file-contains` (no repos)

```
User: cycodgr --repo-csproj-file-contains "SDK" --file-contains "ChatClient"
    ↓
Stage 1 (Pre-filter): SearchCodeForRepositoriesAsync()
    ↓ gh search code "SDK" --extension csproj
    ↓ Extract unique repos → command.Repos
    ↓
Stage 2 (Main search): SearchCodeAsync()
    ↓ gh search code "ChatClient" repo:owner1/repo1 repo:owner2/repo2...
    ↓
Code match results
```

### Path 3: Code Search in Specific Repos
**Triggered by**: `--file-contains` (with repos specified)

```
User: cycodgr microsoft/terminal --cs-file-contains "async"
    ↓
Program.HandleSearchCommandAsync() → HandleCodeSearchAsync()
    ↓
GitHubSearchHelpers.SearchCodeAsync()
    ↓
gh search code "async" --language csharp repo:microsoft/terminal
    ↓
Code match results → file-paths filter → exclude filter
```

---

## Key Design Decisions

### 1. Dual Behavior of `--file-contains`
**Rationale**: Provides intuitive UX - discovers repos when none specified, filters files when repos are known.

**Implementation**: Conditional logic in `HandleSearchCommandAsync()` lines 106-136

### 2. Pre-Filtering Architecture
**Rationale**: GitHub API doesn't support nested queries like "repos containing files with X". Solution: Two-stage search.

**Implementation**: 
- Stage 1: Find repos (lines 76-102, 106-136)
- Stage 2: Search within those repos

### 3. Extension-Specific Shortcuts
**Rationale**: Common use case is searching specific file types - shortcuts make this easier.

**Implementation**: Dynamic option parsing in command-line parser, maps to language/extension filters.

### 4. Post-Search Path Filtering
**Rationale**: GitHub API doesn't support exact path filtering. Solution: Fetch results, filter locally.

**Implementation**: `FilePaths` filter in `HandleCodeSearchAsync()` lines 404-409

---

## Performance Considerations

1. **Pre-filtering increases API calls**: `--repo-file-contains` makes 2 GitHub API calls instead of 1
2. **Post-filtering is client-side**: `--file-paths` and `--exclude` filter after fetching results
3. **Language filtering is server-side**: Reduces result set at GitHub API level (efficient)
4. **Max results limit**: Applied at API level, not after filtering (some results may be lost)

---

## Summary Table

| Option | Level | Timing | Behavior |
|--------|-------|--------|----------|
| `--repo-contains` | Repository | Pre-search | Search repo metadata |
| `--repo-file-contains` | Repository | Pre-search | Discover repos via code search |
| `--repo-{ext}-file-contains` | Repository | Pre-search | Discover repos via extension-filtered code search |
| `--file-contains` | File/Repo | Context-dependent | Pre-filter repos OR search files |
| `--{ext}-file-contains` | File | Pre-search | Search files by extension+content |
| `--language` | File | Pre-search | Filter by language (API-side) |
| `--file-path` | File | Post-search | Filter by exact paths (client-side) |
| `--exclude` | Both | Post-search | Exclude by URL regex (client-side) |

---

For detailed source code evidence with line numbers, call stacks, and data flow diagrams, see: [**Layer 2 Proof Document**](cycodgr-search-layer-2-proof.md)
