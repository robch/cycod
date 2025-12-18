# cycodgr Redesign Spec: Verb-Based → Flag-Based (Symmetry with cycodmd)

## Overview - What We're Doing

**Redesigning cycodgr to match cycodmd's pattern:**

- **Remove** `repo` and `code` verbs
- **Add** content filter flags: `--contains`, `--file-contains`, `--repo-contains`, `--EXT-file-contains`
- **Add** positional repo patterns (like cycodmd's file patterns)
- **Keep** all existing filtering/output options
- **Create** single unified SearchCommand (default command)

**The Symmetry:**

| Tool | Positional Args | Content Filters | Result |
|------|----------------|-----------------|--------|
| **cycodmd** | File patterns (`**/*.cs`) | `--contains`, `--cs-file-contains` | Files with content |
| **cycodgr** | Repo patterns (`owner/repo`) | `--contains`, `--repo-contains`, `--cs-file-contains` | Repos/code with content |

---

## Option-by-Option Analysis

### Search Target Options

#### Positional Arguments

**CURRENT:**
```bash
cycodgr repo jwt auth              # Keywords for repo search
cycodgr code "AddJwtBearer"        # Keywords for code search
```
- Positional args = search keywords
- Verb determines if searching repos or code

**PROPOSED:**
```bash
cycodgr microsoft/vscode dotnet/aspire    # Repo patterns (where to search)
cycodgr @my-repos.txt                     # Repo list from file
cycodgr --contains "jwt"                   # No repos = search all of GitHub
```
- Positional args = repository patterns (where to search)
- Content flags determine what to search for

**CHANGE:** YES - Fundamental shift from keywords to repo patterns

---

#### --contains (NEW)

**CURRENT:** Doesn't exist

**PROPOSED:**
```bash
cycodgr --contains "jwt auth"                    # Search BOTH repo metadata AND code
cycodgr owner/repo --contains "authentication"   # Search both in specific repo
```
- Searches in: repo name, description, topics, README, AND code files
- Shows: Mixed results (repos + code matches)
- Default format: repos section, then code section

**CHANGE:** ADD - Core new feature enabling unified search

---

#### --file-contains (NEW - replaces `code` verb)

**CURRENT:**
```bash
cycodgr code "AddJwtBearer" --extension cs
```
- Requires `code` verb
- Keywords are positional args

**PROPOSED:**
```bash
cycodgr --file-contains "AddJwtBearer" --extension cs
cycodgr owner/repo --file-contains "TODO"
```
- No verb needed
- Content is in flag value
- Works with or without repo patterns

**CHANGE:** REPLACE - Replaces `code` verb, makes search intent explicit

---

#### --repo-contains (NEW - replaces `repo` verb)

**CURRENT:**
```bash
cycodgr repo "jwt authentication"
```
- Requires `repo` verb
- Keywords are positional args

**PROPOSED:**
```bash
cycodgr --repo-contains "jwt authentication"
cycodgr owner/repo --repo-contains "oauth"
```
- No verb needed
- Searches only in: repo name, description, topics, README
- Does NOT search code files

**CHANGE:** REPLACE - Replaces `repo` verb, makes search intent explicit

---

#### --EXT-file-contains (NEW - symmetry with cycodmd)

**CURRENT:**
```bash
cycodgr code "public class" --extension cs
cycodgr code "import" --extension js
```
- Requires `--extension` flag with `code` verb

**PROPOSED:**
```bash
cycodgr --cs-file-contains "public class"
cycodgr --js-file-contains "import"
cycodgr --py-file-contains "def main"
```
- Extension-specific search (like cycodmd's `--cs-file-contains`)
- Cleaner, more explicit

**CHANGE:** ADD - Provides extension-specific shorthand, symmetry with cycodmd

---

#### --repo OWNER/REPO, --repos (Current - becomes positional)

**CURRENT:**
```bash
cycodgr code "TODO" --repo microsoft/vscode
cycodgr code "auth" --repos microsoft/vscode dotnet/aspire
```
- Flag-based repo targeting

**PROPOSED:**
```bash
cycodgr microsoft/vscode --file-contains "TODO"
cycodgr microsoft/vscode dotnet/aspire --file-contains "auth"
```
- Positional repo patterns (like cycodmd's file patterns)

**CHANGE:** MOVE - From flag to positional args

---

### Filtering Options (KEEP - work with new model)

#### --language LANG
**CURRENT:** Filter by programming language
**PROPOSED:** Same, plus add shorthand aliases
**CHANGE:** ENHANCE - Add language-specific shortcuts

**New shortcuts:**
```bash
# Instead of --language csharp:
cycodgr --cs --contains "jwt"
cycodgr --csharp --contains "jwt"

# Other common languages:
cycodgr --js --file-contains "import"
cycodgr --javascript --file-contains "import"
cycodgr --ts --file-contains "useState"
cycodgr --typescript --file-contains "useState"
cycodgr --py --file-contains "def main"
cycodgr --python --file-contains "def main"
cycodgr --go --contains "goroutine"
cycodgr --java --contains "Spring"
cycodgr --rb --contains "Rails"
cycodgr --ruby --contains "Rails"
cycodgr --rs --contains "tokio"
cycodgr --rust --contains "tokio"
```

**Implementation:** Map shortcuts to language names
- `--cs`, `--csharp` → `--language csharp`
- `--js`, `--javascript` → `--language javascript`
- `--ts`, `--typescript` → `--language typescript`
- `--py`, `--python` → `--language python`
- `--go` → `--language go`
- `--java` → `--language java`
- `--rb`, `--ruby` → `--language ruby`
- `--rs`, `--rust` → `--language rust`
- `--cpp`, `--c++` → `--language cpp`

---

#### --owner ORG
**CURRENT:** Filter by owner/organization
**PROPOSED:** Same
**CHANGE:** NO - Works fine with flag-based search

---

#### --min-stars N
**CURRENT:** Filter by minimum stars
**PROPOSED:** Same
**CHANGE:** NO - Works fine with flag-based search

---

#### --sort FIELD
**CURRENT:** Sort repo results (stars, forks, updated, best-match)
**PROPOSED:** Same - sorts repo portion of results
**CHANGE:** NO - Works fine, only applies to repo results

---

#### --include-forks, --exclude-fork, --only-forks
**CURRENT:** Fork filtering for repos
**PROPOSED:** Same - only applies to repo results
**CHANGE:** NO - Works fine with new model

---

#### --exclude PATTERN [...]
**CURRENT:** Post-search regex filtering
**PROPOSED:** Same
**CHANGE:** NO - Works fine with new model

---

### Output Formatting Options

#### --format FORMAT
**CURRENT:**
- Repo command: detailed, url, table, json, csv
- Code command: detailed, filenames, files, repos, urls, json, csv

**PROPOSED:**
- When `--repo-contains` only: detailed, url, table, json, csv
- When `--file-contains` only: detailed, filenames, files, repos, urls, json, csv
- When `--contains` (both): NEW formats needed:
  - `detailed` - Repos section, then code section
  - `repos` - Only show repo results
  - `files` - Only show code results
  - `json` - Combined JSON
  - `csv` - Combined CSV

**CHANGE:** PARTIAL - Keep existing formats, ADD new mixed result formats

---

#### --lines N, --lines-before-and-after N
**CURRENT:** Context lines for code output
**PROPOSED:** Same - only applies when showing code results
**CHANGE:** NO - Works fine with new model

---

#### --max-results N
**CURRENT:**
- Limits repo results to N
- Limits code results to N

**PROPOSED:**
- With `--repo-contains`: Limit repo results to N
- With `--file-contains`: Limit code results to N
- With `--contains` (both): ???
  - Option A: N total results (split between repos and code)
  - Option B: N repos AND N code (20 total with N=10)
  - Option C: Add `--max-repos N` and `--max-files N`

**CHANGE:** CLARIFY - Need to define behavior for mixed searches

---

### Save Options (KEEP - work with new model)

#### --save-json, --save-csv, --save-urls, --save-table, --save-output
**CURRENT:** Save results in various formats
**PROPOSED:** Same - handle mixed results appropriately
**CHANGE:** NO - Keep as-is, ensure mixed result handling

---

### Cloning Options (KEEP - repo-only)

#### --clone, --max-clone, --clone-dir, --as-submodules
**CURRENT:** Clone repository results
**PROPOSED:** Same - only applies to repo results
**CHANGE:** NO - Keep as-is

---

## New Behavior Examples

### No arguments
```bash
cycodgr
# Shows: usage.txt (welcome message)
# Current: Shows repo.txt (confusing)
```

### Positional repo patterns only
```bash
cycodgr microsoft/vscode dotnet/aspire
# Shows: Information about these repos (metadata, stats)
# Could also error: "Specify what to search for (--contains, --file-contains, --repo-contains)"
```

### Content filter only (search all GitHub)
```bash
cycodgr --contains "jwt auth"
# Searches: All GitHub repos and code for "jwt auth"
# Shows: Top repos + top code matches

cycodgr --file-contains "AddJwtBearer" --language csharp
# Searches: All C# code on GitHub
# Shows: Code matches only

cycodgr --repo-contains "authentication" --owner microsoft
# Searches: Microsoft repo metadata
# Shows: Repos only
```

### Combined (repos + content)
```bash
cycodgr microsoft/vscode --file-contains "TODO" --extension ts
# Searches: TypeScript files in vscode repo for "TODO"
# Shows: Code matches only

cycodgr dotnet/aspire --contains "oauth"
# Searches: Both aspire repo metadata AND code for "oauth"
# Shows: Repo info + code matches
```

### Extension-specific shortcuts
```bash
cycodgr --cs-file-contains "AddJwtBearer"
# Equivalent to: --file-contains "AddJwtBearer" --extension cs

cycodgr microsoft/vscode --ts-file-contains "use client"
# Searches: TypeScript files in vscode for "use client"

# Language shortcuts also work:
cycodgr --cs --contains "authentication"
# Equivalent to: --contains "authentication" --language csharp
```

---

## Implementation Status

### Already in Helpers ✅
- `GitHubSearchHelpers.SearchRepositoriesByKeywordsAsync()`
- `GitHubSearchHelpers.SearchCodeForMatchesAsync()`
- All the search logic is ready!

### Need to Create
1. Single unified `SearchCommand` (replaces RepoCommand and CodeCommand)
2. Parser updates for new flags
3. Logic to determine which search(es) to run based on flags
4. Mixed result formatting
5. Updated help files

### Need to Remove
- RepoCommand.cs
- CodeCommand.cs
- Verb-based parsing in CycoGrCommandLineOptions

---

## Questions to Resolve - RESOLVED! ✅

### 1. Positional repo patterns with no content flags
**RESOLVED:** Show repo metadata (stars, description, language, topics, etc.)
```bash
cycodgr microsoft/vscode dotnet/aspire
# Output: Quick info lookup - name, stars, description, language, topics, updated date
```
This is useful for batch checking repo status!

### 2. `--max-results` with `--contains` (both repos and code)
**RECOMMENDATION:** N repos + N code (e.g., --max-results 10 gives 10 repos + 10 code matches)
- Most intuitive - users get N of each type
- Alternative: Add `--max-repos N` and `--max-files N` for fine control

### 3. Default format for `--contains` (mixed results)
**RESOLVED:** Repos section first, then code section
```markdown
# Repositories

# owner/repo1 (⭐ stars | language)
description...

# owner/repo2 (⭐ stars | language)
description...

# Code Matches

## repo1/path/to/file.ext
```lang
* matching line with marker
  context lines
```

## repo2/path/to/other.ext
...
```

### 4. Extension shortcuts - Which languages?
**RESOLVED:** Top 18 languages (95%+ coverage)

**Tier 1: Primary (7):**
- `--cs`, `--csharp` → csharp
- `--js`, `--javascript` → javascript
- `--ts`, `--typescript` → typescript
- `--py`, `--python` → python
- `--java` → java
- `--go` → go
- `--md`, `--markdown` → markdown ✅

**Tier 2: Popular (6):**
- `--rb`, `--ruby` → ruby
- `--rs`, `--rust` → rust
- `--php` → php
- `--cpp`, `--c++` → cpp
- `--swift` → swift
- `--kt`, `--kotlin` → kotlin

**Tier 3: Config/Markup (5):**
- `--yml`, `--yaml` → yaml
- `--json` → json
- `--xml` → xml
- `--html` → html
- `--css` → css

### 5. Default output when searching code
**RESOLVED:** `detailed` format with:
- Code fences with language detection
- Line numbers
- `*` markers on matching lines
- Context lines (default 5, configurable with `--lines`)
- Unless `--format` explicitly overrides

---

## Decision Needed

**Is this redesign worth it?**

**PROS:**
- ✅ Symmetry with cycodmd (consistent user experience)
- ✅ More powerful (can search both repo+code simultaneously)
- ✅ More explicit (clear what you're searching)
- ✅ Better welcome message (usage.txt)

**CONS:**
- ❌ Breaks what we just built (repo and code verbs)
- ❌ More complex implementation (mixed results, multiple content flags)
- ❌ Different from gh CLI (which uses verbs: `gh search repos`, `gh search code`)
- ❌ More verbose for simple searches

**Your call!** Should we proceed with this redesign?

---

