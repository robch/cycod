# CLI Filtering Patterns Catalog

## Overview

This document catalogs the **layered filtering patterns** across all CLI tools in the codebase, identifying the conceptual "pipeline" that users flow through when searching, filtering, and displaying results.

## The Conceptual Pipeline

Most commands follow this general pattern:

```
1. TARGET SELECTION    → What to search (files, repos, conversations, web pages)
2. CONTAINER FILTER    → Which containers to include/exclude (file-level, repo-level)
3. CONTENT FILTER      → What content within containers to show (line-level, message-level)
4. CONTENT REMOVAL     → What content to actively remove from display
5. CONTEXT EXPANSION   → How to expand around matches (before/after lines)
6. DISPLAY CONTROL     → How to present results (formatting, numbering, highlighting)
7. OUTPUT PERSISTENCE  → Where to save results (files, formats)
8. AI PROCESSING       → AI-assisted analysis of results
9. ACTIONS ON RESULTS  → What to do with results (replace, clone, execute)
```

---

## Layer 1: TARGET SELECTION

**Purpose**: Specify what to search IN (the primary search space)

### cycodmd (File Search)
- Positional args: glob patterns (e.g., `**/*.cs`, `*.md`)
- `--exclude`: glob/regex patterns to exclude files
- Time-based:
  - `--modified`, `--modified-after`, `--modified-before`, `--after`, `--before`, `--time-after`, `--time-before`
  - `--created`, `--created-after`, `--created-before`
  - `--accessed`, `--accessed-after`, `--accessed-before`
  - `--anytime`, `--anytime-after`, `--anytime-before`

### cycodj (Conversation Search)
- `--date`, `-d`: specific date
- `--last`: N conversations OR time specification (e.g., `7d`, `-7d`)
- `--today`: shortcut for today
- `--yesterday`: shortcut for yesterday
- `--after`, `--time-after`: time specification
- `--before`, `--time-before`: time specification
- `--date-range`, `--time-range`: range specification

### cycodgr (GitHub Search)
- Positional args: repository patterns (e.g., `microsoft/terminal`, `wezterm/*`)
- `--repo`, `--repos`: explicit repository names
- `--repos @file`: load repo list from file
- `--owner`: filter by owner/organization
- `--min-stars`: minimum star count
- `--include-forks`, `--exclude-fork`, `--only-forks`: fork filtering
- `--sort`: sort results (stars, updated, etc.)

### cycodmd Web Search
- Positional args (WebSearchCommand): search terms
- Positional args (WebGetCommand): URLs
- `--max`: maximum results to fetch
- `--bing`, `--duck-duck-go`, `--google`, `--yahoo`: search provider
- `--bing-api`, `--google-api`: API-based search
- `--exclude`: regex patterns to exclude URLs

### cycodt (Test Framework)
- `--file`, `--files`: test file patterns (glob)
- `--exclude-files`, `--exclude`: patterns to exclude test files
- `--test`, `--tests`: specific test names
- `--contains`: filter tests containing pattern
- `--remove`: remove tests matching pattern
- `--include-optional`: include optional test categories

---

## Layer 2: CONTAINER FILTERING

**Purpose**: Filter which containers (files, repos, conversations) to include/exclude based on their properties or content

### cycodmd (File Search)
- `--file-contains`: include files containing pattern (regex)
- `--file-not-contains`: exclude files containing pattern (regex)
- `--contains`: shorthand for both `--file-contains` AND `--line-contains`
- Extension-specific shortcuts:
  - `--cs-file-contains`, `--py-file-contains`, `--js-file-contains`, etc.
  - Pattern: `--{extension}-file-contains`

### cycodj (Conversation Search)
- `--conversation`, `-c`: specific conversation ID (branches command)
- Time filtering acts as container filtering (see Layer 1)
- No explicit content-based container filtering yet

### cycodgr (GitHub Search)
- Repository-level filtering:
  - `--repo-contains`: repositories containing term
  - `--repo-file-contains`: repos with files containing term
  - `--repo-{ext}-file-contains`: repos with {ext} files containing term
    - Examples: `--repo-csproj-file-contains`, `--repo-md-file-contains`
- File-level filtering within repos:
  - `--file-contains`: files containing term
  - `--{ext}-file-contains`: files of type {ext} containing term
    - Examples: `--cs-file-contains`, `--py-file-contains`
- Language/extension filtering:
  - `--language`: explicit language name
  - `--extension`, `--in-files`: filter by file extension
  - Language shortcuts (Tier 1 - Primary):
    - `--cs`, `--csharp`
    - `--js`, `--javascript`
    - `--ts`, `--typescript`
    - `--py`, `--python`
    - `--java`
    - `--go`
    - `--md`, `--markdown`
  - Language shortcuts (Tier 2 - Popular):
    - `--rb`, `--ruby`
    - `--rs`, `--rust`
    - `--php`
    - `--cpp`, `--c++`
    - `--swift`
    - `--kt`, `--kotlin`
  - Language shortcuts (Tier 3 - Config/Markup):
    - `--yml`, `--yaml`
    - `--json`
    - `--xml`
    - `--html`
    - `--css`
- File path filtering:
  - `--file-path`, `--file-paths`: specific file paths within repos
  - `--file-paths @file`: load paths from file

### cycodmd Web Search
- URL-level filtering:
  - `--exclude`: regex patterns to exclude URLs containing pattern
- Page-level filtering:
  - (appears to be less developed than file/repo filtering)

### cycodt (Test Framework)
- Test-level filtering already covered in Layer 1 (part of target selection)

---

## Layer 3: CONTENT FILTERING

**Purpose**: Filter what content WITHIN selected containers to show/highlight

### cycodmd (File Search)
- `--line-contains`: include only lines matching pattern (regex)
- `--contains`: includes both file AND line filtering (dual-purpose!)
- `--highlight-matches`: visual highlighting of matched content
- `--no-highlight-matches`: disable highlighting

### cycodj (Conversation/Message Search)
- `--user-only`, `-u`: show only user messages
- `--assistant-only`, `-a`: show only assistant messages
- Query-based content search (positional arg in SearchCommand)
- `--case-sensitive`, `-c`: case-sensitive search
- `--regex`, `-r`: regex pattern search

### cycodgr (GitHub Code Search)
- `--contains`: content search term
- `--file-contains`: content within files
- `--line-contains`: specific line patterns (appears to be multiple patterns supported)
- Language acts as implicit content filter

### cycodmd Web Search
- Content filtering appears less developed
- Would benefit from `--page-contains` or similar

### cycodt (Test Framework)
- Expectation checking:
  - `expect-regex`: content must match
  - `not-expect-regex`: content must NOT match
  - `--regex` (ExpectCheckCommand): add regex pattern
  - `--not-regex` (ExpectCheckCommand): add NOT regex pattern

---

## Layer 4: CONTENT REMOVAL

**Purpose**: Actively REMOVE content from display (post-match filtering)

### cycodmd (File Search)
- `--remove-all-lines`: remove lines matching pattern (regex)
- Applied BEFORE other line filters

### cycodj (Conversation Search)
- No explicit removal mechanism (could benefit from `--remove-lines` or `--hide-tool-calls`)

### cycodgr (GitHub Search)
- `--exclude`: excludes at multiple levels (repos, URLs, etc.)
- No explicit line removal mechanism

### cycodt (Test Framework)
- `--remove`: remove tests matching pattern
- `removeAllLines` parameter in various helpers

---

## Layer 5: CONTEXT EXPANSION

**Purpose**: Expand results to show context around matches

### cycodmd (File Search)
- `--lines`: show N lines before AND after (shorthand)
- `--lines-before`: show N lines before matches
- `--lines-after`: show N lines after matches

### cycodj (Conversation Search)
- `--context`, `-C`: show N messages before/after match
- Only in SearchCommand

### cycodgr (GitHub Search)
- `--lines-before-and-after`, `--lines`: show N lines around matches
- Appears to be symmetric only (no separate before/after)

### cycodt (Test Framework)
- `linesBeforeAndAfter` parameter in various functions
- Not exposed as CLI option

---

## Layer 6: DISPLAY CONTROL

**Purpose**: Control HOW results are presented

### cycodmd (File Search)
- `--line-numbers`: show line numbers
- `--files-only`: show only file names (no content)
- `--highlight-matches` / `--no-highlight-matches`: control highlighting

### cycodj (Conversation Display)
- `--messages [N|all]`: limit message count
  - No value = use command default
  - N = show N messages
  - `all` = show all messages
- `--stats`: show statistics
- `--branches`: show conversation branches
- `--show-tool-calls`: show tool call details
- `--show-tool-output`: show tool output
- `--max-content-length`: truncate content display
- `--verbose`, `-v`: verbose output (branches command)

### cycodgr (GitHub Search)
- `--format`: output format
  - `repos`: list of repository names
  - `urls`: list of URLs
  - `files`: list of file names
  - `json`: JSON output
  - `csv`: CSV output
  - `table`: table format
- `--max-results`: limit result count

### cycodt (Test Framework)
- `--output-format`: test result format (`trx`, `junit`)
- Various display parameters in test framework

### Shared Across Tools
- `--verbose`: detailed output (multiple tools)
- `--quiet`: minimal output (cycod, CommandLineOptions)
- `--debug`: debug information (CommandLineOptions)

---

## Layer 7: OUTPUT PERSISTENCE

**Purpose**: Save results to files

### cycodmd (File Search)
- `--save-output`: save combined output to file
  - Default: `output.md`
- `--save-file-output`: save per-file output
  - Default template: `{filePath}/{fileBase}-output.md`
- `--save-chat-history`: save AI processing history
  - Default: from `AiInstructionProcessor.DefaultSaveChatHistoryTemplate`

### cycodmd Web Search
- `--save-page-output`, `--save-web-output`, `--save-web-page-output`: save web page output
  - Default template: `{filePath}/{fileBase}-output.md`
- `--save-page-folder`: folder for saving pages
  - Default: `web-pages`
- `--save-chat-history`: AI processing history

### cycodj (Conversation Operations)
- `--save-output`: save command output to file
- `--save-chat-history`: save AI processing chat history

### cycodgr (GitHub Search)
- `--save-output`: save markdown output
  - Default: `search-output.md`
- `--save-json`: save results as JSON
- `--save-csv`: save results as CSV
- `--save-table`: save results as table
- `--save-urls`: save URLs to file
- `--save-repos`: save repository names to file
- `--save-file-paths`: save file paths
  - Default template: `files-{repo}.txt`
- `--save-repo-urls`: save repository URLs
  - Default: `repo-urls.txt`
- `--save-file-urls`: save file URLs
  - Default template: `file-urls-{repo}.txt`

### cycodt (Test Framework)
- `--output-file`: test results output file
- `--output-format`: test results format

### cycod (Chat)
- `--chat-history`: both input and output
  - Default: `chat-history.jsonl`
- `--input-chat-history`: load existing history
- `--output-chat-history`: save new history
  - Default template: `chat-history-{time}.jsonl`
- `--output-trajectory`: save trajectory
  - Default template: `trajectory-{time}.md`

### Shared Patterns
- Template variables:
  - `{time}`: timestamp
  - `{filePath}`, `{fileBase}`: file components
  - `{repo}`: repository name
  - `{ProgramName}`: program name

---

## Layer 8: AI PROCESSING

**Purpose**: AI-assisted analysis and processing of results

### cycodmd (File/Web Search)
- `--instructions`: general AI instructions (multiple values allowed)
- `--file-instructions`: AI instructions for all files
- `--{ext}-file-instructions`: AI instructions for specific file types
  - Examples: `--cs-file-instructions`, `--md-file-instructions`
  - Pattern: `--{criteria}-file-instructions`
- `--page-instructions`: AI instructions for web pages
- `--{criteria}-page-instructions`: filtered page instructions
- `--built-in-functions`: enable AI to use built-in functions
- `--save-chat-history`: save AI interaction history

### cycodj (Conversation Operations)
- `--instructions`: AI instructions for processing
- `--use-built-in-functions`: enable AI functions
- `--save-chat-history`: save AI interaction

### cycodgr (GitHub Search)
- `--instructions`: general AI instructions
- `--file-instructions`: AI instructions for files
- `--{ext}-file-instructions`: file type-specific instructions
- `--repo-instructions`: AI instructions for repositories
- No `--built-in-functions` option yet
- No `--save-chat-history` option yet

### cycodt (Test Framework)
- `--instructions` (ExpectCheckCommand): AI-based expectation checking
- Limited AI integration compared to other tools

### Shared Patterns
- Multiple instructions can be provided
- Extension/criteria-based instruction filtering
- Chat history persistence for debugging/review

---

## Layer 9: ACTIONS ON RESULTS

**Purpose**: Perform operations on search results

### cycodmd (File Search)
- `--replace-with`: replacement text for matched patterns
- `--execute`: execute the replacement (vs. preview)

### cycodgr (GitHub Search)
- `--clone`: clone matching repositories
- `--max-clone`: limit number of repos to clone
- `--clone-dir`: directory for cloned repos
- `--as-submodules`: add as git submodules

### cycodj (Cleanup Operations)
- `--find-duplicates`: find duplicate conversations
- `--remove-duplicates`: remove duplicates
- `--find-empty`: find empty conversations
- `--remove-empty`: remove empty conversations
- `--older-than-days`: filter by age
- `--execute`: execute cleanup (vs. dry-run)
- Default: `--dry-run` (safe mode)

### cycodt (Test Framework)
- Test execution is the implicit action
- No separate "preview" vs "execute" mode

---

## Cross-Tool Analysis

### Naming Consistency

#### Strong Patterns (Consistent)
1. **Scope flags** (config/alias/prompt/mcp commands):
   - `--global`, `-g`
   - `--user`, `-u`
   - `--local`, `-l`
   - `--any`, `-a`
   - ✅ **Perfectly consistent across all commands**

2. **Time filtering basics**:
   - `--after`, `--before`
   - `--today`, `--yesterday`
   - ✅ **Consistent where implemented**

3. **AI processing**:
   - `--instructions`
   - `--save-chat-history`
   - ✅ **Consistent naming pattern**

4. **Output control**:
   - `--save-output`
   - `--verbose`
   - `--quiet`
   - ✅ **Consistent across tools**

#### Weak Patterns (Inconsistent)

1. **`--exclude` overloading**:
   - cycodmd: glob/regex patterns for files
   - cycodmd web: regex patterns for URLs
   - cycodgr: multiple meanings (repos, URLs, patterns)
   - cycodt: test file patterns
   - ⚠️ **Same option name, different semantics**

2. **Time filtering variations**:
   - `--after` vs `--time-after` vs `--modified-after`
   - `--before` vs `--time-before` vs `--modified-before`
   - ⚠️ **Redundant aliases, unclear when to use which**

3. **Context expansion**:
   - cycodmd: `--lines`, `--lines-before`, `--lines-after` (3 options)
   - cycodgr: `--lines-before-and-after`, `--lines` (2 options, symmetric only)
   - cycodj: `--context`, `-C` (1 option, symmetric only)
   - ⚠️ **Different granularity levels**

4. **Format output**:
   - cycodgr: `--format` (multiple values: repos, urls, files, json, csv, table)
   - cycodt: `--output-format` (values: trx, junit)
   - ⚠️ **Different option names for similar concept**

5. **Result limiting**:
   - cycodmd web: `--max`
   - cycodgr: `--max-results`, `--max-clone`
   - cycodj: `--messages [N|all]`, `--last`
   - ⚠️ **No consistent naming pattern**

6. **`--contains` semantics**:
   - cycodmd: applies to BOTH files AND lines (dual-layer)
   - cycodgr: applies to content search
   - ⚠️ **Different layer scope**

### Missing Cross-Pollination Opportunities

#### cycodj ← cycodmd
- ❌ `--line-numbers` → could show message numbers
- ❌ `--remove-all-lines` → could hide certain message types
- ❌ `--highlight-matches` → could highlight search terms in messages
- ❌ `--lines-before`, `--lines-after` → only has symmetric `--context`

#### cycodmd ← cycodj
- ❌ `--stats` → could show file statistics (file count, total lines, match count)
- ❌ Time-relative filtering is partial (`--modified`, etc.) but not as rich as cycodj's `--last 7d`

#### cycodgr ← cycodmd
- ❌ `--remove-all-lines` → could remove boilerplate from code display
- ❌ `--files-only` mode exists, but no `--repos-only` mode

#### cycodgr ← cycodj
- ❌ `--stats` → could show repo statistics
- ❌ Time-relative filtering → could filter repos by update time

#### Web Search ← cycodmd/cycodgr
- ❌ `--line-numbers` → could number search results
- ❌ `--page-contains` → explicit page-level content filtering
- ❌ More structured content filtering layers

#### cycodt ← All
- ❌ `--stats` → could show test execution statistics
- ❌ `--line-numbers` → could show line numbers in test output

### Pattern Gaps (Present in Some, Missing in Others)

| Pattern | cycodmd | cycodj | cycodgr | Web | cycodt | cycod |
|---------|---------|--------|---------|-----|--------|-------|
| **Container filtering** | ✅ `--file-contains` | ⚠️ Limited | ✅ `--repo-contains` | ⚠️ Limited | ❌ | N/A |
| **Content filtering** | ✅ `--line-contains` | ✅ Message search | ✅ `--line-contains` | ⚠️ Limited | ✅ `expect-regex` | N/A |
| **Content removal** | ✅ `--remove-all-lines` | ❌ | ❌ | ❌ | ⚠️ Limited | N/A |
| **Context expansion** | ✅ Asymmetric | ✅ Symmetric | ✅ Symmetric | ❌ | ⚠️ Internal | N/A |
| **Line numbers** | ✅ | ❌ | ❌ | ❌ | ❌ | N/A |
| **Highlighting** | ✅ | ❌ | ❌ | ❌ | ❌ | N/A |
| **Statistics** | ❌ | ✅ | ❌ | ❌ | ❌ | N/A |
| **Time filtering** | ✅ File times | ✅ Rich | ❌ | ❌ | ❌ | ❌ |
| **AI processing** | ✅ Rich | ✅ Basic | ✅ Rich | ❌ | ⚠️ Limited | N/A |
| **Replace/Execute** | ✅ | ❌ | ✅ Clone | ❌ | ⚠️ Tests | N/A |
| **Multiple formats** | ❌ | ❌ | ✅ 6 formats | ❌ | ⚠️ 2 formats | N/A |

### Fractal/Recursive Patterns

#### Level 1: Single-Layer Filtering
- cycodt: Test → Expectation
- Simple: find X, show X

#### Level 2: Two-Layer Filtering
- cycodj: Conversation → Message
- Find conversations, then filter messages within them

#### Level 3: Three-Layer Filtering
- cycodmd: Filesystem → File → Line
- Find files by glob → filter files by content → filter lines within files

#### Level 4: Four-Layer Filtering
- cycodgr: Organization → Repository → File → Line
- Filter by org/owner → filter repos → filter files in repos → filter lines in files

#### Level 5: Multi-Branch Hierarchies
- cycodj: Date → Conversation → Branch → Message
- Time-based grouping with branch navigation

#### Recursive Opportunities (Not Yet Explored)

1. **Nested Repository Search**:
   - Find repos containing submodules matching pattern
   - Search within submodules recursively
   - `--recursive-submodules`, `--max-depth`

2. **Conversation Thread Traversal**:
   - Search conversation trees by branch patterns
   - Follow branch paths matching criteria
   - `--branch-pattern`, `--branch-depth`

3. **File Include Chains**:
   - Search files that include/import other files matching pattern
   - Transitive dependency search
   - `--follow-imports`, `--max-include-depth`

4. **Web Link Following**:
   - Search pages linking to pages matching pattern
   - Recursive link traversal
   - `--follow-links`, `--max-link-depth`

5. **Test Dependency Chains**:
   - Find tests that depend on tests matching pattern
   - Transitive test dependencies
   - `--follow-dependencies`

### Innovation Opportunities

#### 1. Unified Filtering Language
Create a consistent syntax across all tools:
```bash
# Unified syntax proposal
<tool> [targets] \
  --at-level <container> --contains <pattern> \
  --at-level <content> --contains <pattern> \
  --expand <before>:<after> \
  --show <format>
```

Example:
```bash
cycodgr microsoft/* \
  --at-level repo --contains "machine learning" \
  --at-level file --extension py --contains "tensorflow" \
  --at-level line --contains "import" \
  --expand 2:2 \
  --show line-numbers
```

#### 2. Composable Filter Pipelines
Allow users to build pipelines explicitly:
```bash
cycodmd "**/*.cs" | filter-lines --contains "async" | expand-context 3 | save output.md
```

#### 3. Filter Presets/Profiles
Save complex filter combinations:
```bash
cycodmd --save-filter-preset "find-async-cs" \
  --cs-file-contains "async" \
  --line-contains "Task" \
  --lines-before 2 --lines-after 2

# Later use:
cycodmd --filter-preset "find-async-cs"
```

#### 4. Negative Filtering Consistency
Standardize negative filtering across all layers:
- `--not-file-contains`
- `--not-line-contains`
- `--not-repo-contains`
- `--not-message-contains`

#### 5. Statistical Aggregation
Add stats to all search operations:
```bash
cycodmd "**/*.cs" --stats
# Output:
# Files found: 152
# Files after filter: 48
# Lines matched: 326
# Total lines: 15,832
```

#### 6. Multi-Level Context Expansion
Allow context expansion at multiple levels:
```bash
cycodgr microsoft/terminal \
  --file-contains "ConPTY" \
  --line-contains "Create" \
  --expand-files 1:1 \     # Show 1 file before/after matched file
  --expand-lines 5:5       # Show 5 lines before/after matched line
```

#### 7. Regex Library/Aliases
Common patterns as named aliases:
```bash
cycodmd "**/*.cs" --line-contains @email-address
# Where @email-address → /[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}/
```

#### 8. Cross-Tool Result Chaining
Use output from one tool as input to another:
```bash
cycodgr --repo-contains "terminal" --save-repos repos.txt
cycodmd @repos.txt --file-contains "Unicode"
```

#### 9. Temporal Comparison
Compare results across time:
```bash
cycodj --today --save-output today.md
cycodj --yesterday --save-output yesterday.md
cycodj --compare today.md yesterday.md
```

#### 10. Interactive Filter Refinement
Start broad, refine interactively:
```bash
cycodmd "**/*.cs" --interactive
# Shows results, then prompts:
# > Add filter: --line-contains "async"
# > Expand context: --lines 3
# > Save as preset? yes → "async-search"
```

---

## Extension-Specific Shortcuts Analysis

### Current Implementation

cycodmd:
- `--cs-file-contains`
- `--py-file-contains`
- `--js-file-contains`
- etc. (any extension)

cycodgr:
- Language shortcuts: `--cs`, `--py`, `--js`, `--rs`, etc.
- File-contains shortcuts: `--cs-file-contains`, `--md-file-contains`
- Repo-file-contains shortcuts: `--repo-csproj-file-contains`

### Pattern Recognition

1. **Single-extension pattern**: `--{ext}-{operation}`
   - Examples: `--cs-file-contains`, `--py-file-contains`

2. **Multi-level pattern**: `--{scope}-{ext}-{operation}`
   - Examples: `--repo-csproj-file-contains`

3. **Standalone shortcuts**: `--{ext}`
   - Examples: `--cs`, `--py`, `--rust`

### Fractal Extension Opportunity

Could support arbitrary depth:
```bash
# Current: 2 levels
--repo-csproj-file-contains "SDK"

# Proposed: 3+ levels
--org-microsoft-repo-csproj-file-contains "SDK"
--team-azure-org-microsoft-repo-csproj-file-contains "SDK"
```

This may be over-engineering, but illustrates the fractal nature.

---

## Time Specification Patterns

### Current Implementation

#### cycodmd
- Absolute: `--modified 2024-01-01`
- Range: `--modified 2024-01-01..2024-12-31`
- Relative: `--modified 7d` (last 7 days)
- Shortcuts: (none)

#### cycodj
- Absolute: `--date 2024-01-01`
- Range: `--date-range 2024-01-01..2024-12-31`
- Relative: `--last 7d`
- Shortcuts: `--today`, `--yesterday`
- Smart detection: `--last 7d` (time) vs `--last 10` (count)

### Inconsistencies

1. cycodmd has NO shortcuts like `--today`
2. cycodj's `--last` is smart (time OR count) - others are not
3. Different option names:
   - cycodmd: `--modified`
   - cycodj: `--date`, `--date-range`
   - No shared vocabulary

### Standardization Opportunity

Unified time filtering:
```bash
# All tools support:
--when <timespec>              # Primary
--today, --yesterday           # Shortcuts
--last <N|timespec>            # Smart detection
--after <time>, --before <time>
```

---

## Semantic Grouping of Options

### By Purpose

#### Discovery Options (What to find)
- Glob patterns
- `--repo`, `--repos`
- `--file-path`, `--file-paths`
- `--date`, `--last`
- Search terms, URLs

#### Inclusion Options (What to keep)
- `--contains`, `--file-contains`, `--line-contains`
- `--{ext}-file-contains`
- `--language`, `--{ext}`
- `--user-only`, `--assistant-only`
- `--min-stars`

#### Exclusion Options (What to reject)
- `--exclude`
- `--file-not-contains`
- `--remove-all-lines`
- `--not-regex`
- `--exclude-fork`

#### Context Options (What to expand)
- `--lines`, `--lines-before`, `--lines-after`
- `--context`
- `--lines-before-and-after`

#### Display Options (How to show)
- `--line-numbers`
- `--highlight-matches`
- `--files-only`
- `--messages`, `--stats`, `--branches`
- `--verbose`, `--quiet`, `--debug`
- `--format`

#### Output Options (Where to save)
- `--save-output`
- `--save-file-output`
- `--save-json`, `--save-csv`, `--save-table`
- `--output-file`, `--output-format`

#### Processing Options (How to transform)
- `--instructions`
- `--built-in-functions`
- `--replace-with`, `--execute`
- `--clone`, `--as-submodules`

---

## Command Symmetry Analysis

### List Commands
- cycodj: `list` - list conversations
- cycodj: `branches` - list branches within conversation (sub-list)
- cycodt: `list` - list tests
- cycodgr: (implicit in search with `--format repos`)

**Pattern**: Primary list → Secondary list (drilling down)

**Gaps**:
- cycodmd has no `list` command (only direct search)
- No unified `list` command signature

### Show Commands
- cycodj: `show` - show single conversation
- cycodt: (no show command - tests are executed, not shown)
- cycod: (no show command - chats are loaded)

**Pattern**: `show <ID>` with display options

**Gaps**:
- Inconsistent across tools
- Could benefit from `cycodgr show <repo>` or `cycodmd show <file>`

### Search Commands
- cycodj: `search` - search within conversations
- cycodgr: `search` (default) - search GitHub
- cycodmd: (default) - search files
- cycodmd: `web search` - search web

**Pattern**: Content-based search with refinement options

**Consistency**: ✅ Good naming alignment

### Stats Commands
- cycodj: `stats` - show conversation statistics
- (Others lack stats commands)

**Gap**: Stats should be universal

### Cleanup Commands
- cycodj: `cleanup` - clean up conversations
- (Others lack cleanup commands)

**Gap**: Could apply to cached web pages, cloned repos, test artifacts

---

## Recommendations for Standardization

### Priority 1: Critical Inconsistencies
1. **Unify `--exclude` semantics**
   - Separate options per layer: `--exclude-files`, `--exclude-lines`, `--exclude-repos`, `--exclude-urls`
   - Keep `--exclude` as shorthand for most common case per tool

2. **Standardize time filtering**
   - All tools use: `--when`, `--after`, `--before`, `--today`, `--yesterday`
   - Use `TimeSpecHelpers` universally

3. **Unify format options**
   - All tools use `--format` (not `--output-format`)
   - Support common formats: `json`, `csv`, `table`, `markdown`

4. **Standardize result limiting**
   - All tools use `--max-results` (not `--max`, not `--messages`)

### Priority 2: Feature Gaps
1. **Add `--stats` to all search tools**
2. **Add `--line-numbers` to all content display tools**
3. **Add `--context` / `--lines` to all tools showing line-level content**
4. **Add `--remove-all-lines` equivalent to all tools with line display**

### Priority 3: Innovation
1. **Implement filter presets** (save/load complex filter combinations)
2. **Add cross-tool result chaining** (pipe outputs)
3. **Create unified filtering DSL** (domain-specific language)
4. **Add interactive refinement mode**

---

## Appendix: Complete Option Inventory

### cycodmd

#### Target Selection
- Positional: glob patterns
- `--exclude`: glob/regex patterns
- Time: `--modified`, `--created`, `--accessed`, `--anytime` + `-after`/`-before` variants

#### Container Filter
- `--file-contains`, `--file-not-contains`, `--contains`
- `--{ext}-file-contains`

#### Content Filter
- `--line-contains`, `--contains`
- `--highlight-matches`, `--no-highlight-matches`

#### Content Removal
- `--remove-all-lines`

#### Context
- `--lines`, `--lines-before`, `--lines-after`

#### Display
- `--line-numbers`, `--files-only`

#### Output
- `--save-output`, `--save-file-output`, `--save-chat-history`

#### AI Processing
- `--instructions`, `--file-instructions`, `--{ext}-file-instructions`
- `--built-in-functions`

#### Actions
- `--replace-with`, `--execute`

#### Web-Specific
- `--interactive`, `--chromium`, `--firefox`, `--webkit`
- `--strip`, `--save-page-folder`
- `--bing`, `--google`, `--duck-duck-go`, `--yahoo`
- `--bing-api`, `--google-api`
- `--get`, `--max`
- `--page-instructions`, `--{criteria}-page-instructions`
- `--save-page-output`

### cycodj

#### Target Selection
- `--date`, `-d`
- `--last`: N conversations OR timespec
- `--today`, `--yesterday`
- `--after`, `--before`, `--time-after`, `--time-before`
- `--date-range`, `--time-range`

#### Container Filter
- `--conversation`, `-c` (branches command)

#### Content Filter
- Positional: query (search command)
- `--user-only`, `--assistant-only`, `-u`, `-a`
- `--case-sensitive`, `--regex`, `-c`, `-r`

#### Context
- `--context`, `-C`

#### Display
- `--messages [N|all]`
- `--stats`, `--branches`
- `--show-tool-calls`, `--show-tool-output`
- `--max-content-length`
- `--verbose`, `-v`

#### Output
- `--save-output`, `--save-chat-history`

#### AI Processing
- `--instructions`, `--use-built-in-functions`

#### Actions (Cleanup)
- `--find-duplicates`, `--remove-duplicates`
- `--find-empty`, `--remove-empty`
- `--older-than-days`
- `--execute` (vs default dry-run)

### cycodgr

#### Target Selection
- Positional: repo patterns
- `--repo`, `--repos` (+ `@file` support)
- `--owner`
- `--min-stars`
- `--include-forks`, `--exclude-fork`, `--only-forks`
- `--sort`

#### Container Filter (Repo-level)
- `--repo-contains`
- `--repo-file-contains`
- `--repo-{ext}-file-contains`

#### Container Filter (File-level)
- `--file-contains`
- `--{ext}-file-contains`
- `--language`, `--extension`, `--in-files`
- Language shortcuts: `--cs`, `--py`, `--js`, etc. (see Layer 2 for full list)
- `--file-path`, `--file-paths` (+ `@file` support)

#### Content Filter
- `--contains`
- `--line-contains`

#### Context
- `--lines-before-and-after`, `--lines`

#### Display
- `--format`: repos, urls, files, json, csv, table
- `--max-results`

#### Output
- `--save-output`
- `--save-json`, `--save-csv`, `--save-table`
- `--save-urls`, `--save-repos`
- `--save-file-paths`, `--save-repo-urls`, `--save-file-urls`

#### AI Processing
- `--instructions`
- `--file-instructions`, `--{ext}-file-instructions`
- `--repo-instructions`

#### Actions
- `--clone`, `--max-clone`, `--clone-dir`, `--as-submodules`

#### Exclusion
- `--exclude`

### cycodt

#### Target Selection
- `--file`, `--files`: glob patterns
- `--exclude-files`, `--exclude`
- `--test`, `--tests`
- `--contains`, `--remove`
- `--include-optional`

#### Content Filter (Expectations)
- `expect-regex`, `not-expect-regex` (in YAML)
- `--regex`, `--not-regex` (ExpectCheckCommand)

#### Output
- `--output-file`, `--output-format` (trx, junit)
- `--save-output` (ExpectCheckCommand)

#### AI Processing
- `--instructions` (ExpectCheckCommand)

### cycod

#### Chat Options
- `--input`, `--instruction`, `--question`, `-q`
- `--inputs`, `--instructions`, `--questions`
- `--system-prompt`, `--add-system-prompt`, `--add-user-prompt`, `--prompt`
- `--chat-history`, `--input-chat-history`, `--output-chat-history`, `--continue`
- `--output-trajectory`
- `--var`, `--vars`
- `--foreach`
- `--use-templates`, `--no-templates`
- `--use-mcps`, `--mcp`, `--no-mcps`, `--with-mcp`
- `--image`
- `--auto-generate-title`

#### Provider Selection
- `--use-anthropic`, `--use-azure-anthropic`
- `--use-aws`, `--use-bedrock`, `--use-aws-bedrock`
- `--use-azure-openai`, `--use-azure`
- `--use-google`, `--use-gemini`, `--use-google-gemini`
- `--use-grok`, `--use-x.ai`
- `--use-openai`
- `--use-test`
- `--use-copilot`, `--use-copilot-token`

#### Config/Alias/Prompt/MCP (Shared Pattern)
- Scope: `--global`, `--user`, `--local`, `--any`, `-g`, `-u`, `-l`, `-a`
- Config-specific: `--file`
- MCP-specific: `--command`, `--url`, `--arg`, `--args`, `--env`, `-e`

---

## Summary

This catalog reveals:

1. **Strong consistency** in some areas (scope flags, AI processing)
2. **Significant inconsistencies** in naming (exclude, time, format, limiting)
3. **Missing features** across tools (stats, line numbers, context expansion)
4. **Fractal opportunities** for recursive/nested filtering
5. **Innovation potential** in filter presets, pipelines, and cross-tool chaining

The next steps should focus on:
- **Standardizing** critical inconsistencies (Priority 1)
- **Filling gaps** with missing features (Priority 2)
- **Experimenting** with innovative filtering patterns (Priority 3)
