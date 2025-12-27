# cycodgr search - Layer 6: Display Control

**[← Back to search Command Catalog](cycodgr-search-filtering-pipeline-catalog-README.md)** | **[Proof →](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md)**

## Purpose

Layer 6 controls **how results are presented** to the user, including formatting, visual styling, and output structure.

## Options/Arguments

### `--format <format>`

**Purpose**: Specify the output format for search results

**Values**:
- `detailed` (default) - Full markdown output with code snippets
- `repos` - Repository names only (one per line)
- `urls` - Repository URLs only
- `files` - File URLs only (for code search)
- `filenames` - File paths only (for code search)
- `json` - JSON format
- `csv` - CSV format
- `table` - Markdown table format (repo search only)

**Parsed In**: `CycoGrCommandLineOptions.cs` lines 219-227

**Stored In**: `SearchCommand.Format` (line 24 of SearchCommand.cs, default: "detailed")

**Used By**:
- `FormatRepoOutput()` - Program.cs line 1232
- `FormatCodeOutput()` - Program.cs line 934

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#format-option)

---

### `--max-results <N>`

**Purpose**: Limit the number of results displayed

**Default**: 10

**Parsed In**: `CycoGrCommandLineOptions.cs` lines 205-208

**Stored In**: `SearchCommand.MaxResults` (line 16 of SearchCommand.cs)

**Used By**:
- `GitHubSearchHelpers.SearchRepositoriesAsync()` - line 23, parameter `maxResults`
- `GitHubSearchHelpers.SearchCodeAsync()` - line 37, parameter `maxResults`
- GitHub CLI `--limit` argument - GitHubSearchHelpers.cs lines 256, 335

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#max-results-option)

---

### Language-Based Code Fencing

**Purpose**: Automatically detect file language and use appropriate code fence syntax

**Implementation**: 
- `DetectLanguageFromPath()` - Program.cs lines 900-932
- `GetLanguageForExtension()` - Program.cs lines 1041-1073

**Used In**:
- File output formatting - Program.cs line 804
- Code snippet display - Program.cs line 974

**Supported Languages**:
- C# (`.cs` → `csharp`)
- JavaScript (`.js` → `javascript`)
- TypeScript (`.ts` → `typescript`)
- Python (`.py` → `python`)
- Java (`.java` → `java`)
- C++ (`.cpp`, `.cc`, `.cxx`, `.h`, `.hpp` → `cpp`)
- Go (`.go` → `go`)
- Rust (`.rs` → `rust`)
- Ruby (`.rb` → `ruby`)
- PHP (`.php` → `php`)
- Swift (`.swift` → `swift`)
- Kotlin (`.kt`, `.kts` → `kotlin`)
- Shell (`.sh`, `.bash` → `bash`)
- PowerShell (`.ps1` → `powershell`)
- SQL (`.sql` → `sql`)
- HTML (`.html`, `.htm` → `html`)
- CSS (`.css` → `css`)
- XML (`.xml` → `xml`)
- JSON (`.json` → `json`)
- YAML (`.yaml`, `.yml` → `yaml`)
- Markdown (`.md`, `.markdown` → `markdown`)
- Text (`.txt` → `text`)

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#language-detection)

---

### Star Count Formatting

**Purpose**: Format repository star counts in a human-readable way

**Implementation**: `RepoInfo.FormattedStars` property (Models/RepoInfo.cs)

**Format Examples**:
- 1234 → "1,234"
- 1500 → "1.5k"
- 1000000 → "1.0m"

**Used In**:
- Repository headers - Program.cs lines 193, 656, 960, 1256

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#star-formatting)

---

### Hierarchical Output Structure

**Purpose**: Group and nest results hierarchically by repository and file

**Structure for Code Search**:
```
## {repo/fullname} (⭐ stars) (language)

Repo: {url}
Desc: {description}

Found N file(s) with M matches:
- {file_url} (X matches)
- {file_url} (Y matches)

## {file_path}

```language
{line_number}: {code}
...
```

Raw: {raw_url}
```

**Implementation**:
- Repository grouping - Program.cs line 644
- File grouping - Program.cs line 683
- Header formatting - Program.cs lines 656-674
- File section - Program.cs lines 754-856

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#hierarchical-structure)

---

### Line Numbering

**Purpose**: Display actual line numbers from source files in code snippets

**Implementation**: `LineHelpers.FilterAndExpandContext()` with `includeLineNumbers: true`

**Used In**: Program.cs line 807-816

**Format**: `{line_number}: {code_content}`

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#line-numbering)

---

### Match Highlighting

**Purpose**: Visually highlight matched terms in code output

**Implementation**: 
- `LineHelpers.FilterAndExpandContext()` with `highlightMatches: true` (Program.cs line 815)
- Highlighting logic in LineHelpers (common library)

**Visual Markers**:
- Terminal: ANSI color codes (typically bright/bold)
- Markdown: Backticks or emphasis (depends on LineHelpers implementation)

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#match-highlighting)

---

### Console Output Control

**Purpose**: Control console output verbosity and appearance

**Options** (from CommandLineOptions base class):
- `--verbose` - Detailed output
- `--quiet` - Minimal output
- `--debug` - Debug information

**Color Coding**:
- **Cyan** - Section headers (Program.cs lines 81, 233, 303, 378)
- **Green** - Success messages (Program.cs lines 97, 131, 295, 351, 364, 431, 811)
- **Yellow** - Warnings and "not found" (Program.cs lines 93, 127, 186, 272, 324, 332, 396, 413, 791, 1374)
- **White** - Primary content (Program.cs line 198)
- **Default** - Standard output

**Console Methods**:
- `ConsoleHelpers.WriteLine()` - Standard output
- `ConsoleHelpers.WriteErrorLine()` - Error output
- `ConsoleHelpers.DisplayStatus()` - Status messages
- `ConsoleHelpers.DisplayStatusErase()` - Clear status

**Override Quiet**: Many output calls use `overrideQuiet: true` to ensure critical information is displayed even in quiet mode

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#console-output)

---

### Format-Specific Output Methods

**Repository Formats**:

1. **Detailed Format** - `FormatAsDetailed()` (Program.cs lines 1249-1285)
   - Full repo information with descriptions, topics, update dates
   - Markdown heading structure

2. **URLs Format** - `FormatAsUrls()` (Program.cs lines 1244-1247)
   - Just repository URLs, one per line

3. **Table Format** - `FormatAsTable()` (Program.cs lines 1287-1304)
   - Markdown table with columns: Repository, Stars, Language, Description
   - Description truncated to 50 chars

4. **JSON Format** - `FormatAsJson()` (Program.cs lines 1306-1326)
   - Pretty-printed JSON with all repo metadata

5. **CSV Format** - `FormatAsCsv()` (Program.cs lines 1328-1341)
   - CSV with headers: url, name, owner, stars, language, description

**Code Search Formats**:

1. **Detailed Format** - `FormatCodeAsDetailed()` (Program.cs lines 948-1032)
   - Hierarchical output with code snippets and context

2. **Filenames Format** - `FormatCodeAsFilenames()` (Program.cs lines 1075-1095)
   - Repository headers with file paths

3. **Files Format** - `FormatCodeAsFiles()` (Program.cs lines 1097-1109)
   - Raw file URLs for direct download

4. **Repos Format** - `FormatCodeAsRepos()` (Program.cs lines 1111-1115)
   - Unique repository URLs

5. **URLs Format** - `FormatCodeAsUrls()` (Program.cs lines 1117-1138)
   - Repository URLs with indented file URLs

6. **JSON Format** - `FormatCodeAsJson()` (Program.cs lines 1183-1216)
   - Full code match data with text matches and fragments

7. **CSV Format** - `FormatCodeAsCsv()` (Program.cs lines 1218-1230)
   - CSV with file-level information

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#format-methods)

---

## Display Control Flow

### Repository Search Display

1. Search GitHub API (GitHubSearchHelpers)
2. Apply exclusion filters
3. Format results based on `--format` option
4. Output to console with color coding
5. Clone repositories if `--clone` specified (Layer 9)

### Code Search Display

1. Search GitHub API for code matches
2. Group results by repository
3. For each repository:
   - Display repository header
   - List matched files with counts
   - For each file:
     - Fetch full file content from raw URL
     - Filter lines using LineHelpers
     - Display with line numbers and highlighting
     - Apply AI instructions if specified (Layer 8)
4. Apply global AI instructions if specified (Layer 8)
5. Output to console

### Parallel Processing

Code file processing uses `ThrottledProcessor` to process multiple files in parallel (Program.cs lines 693-697)

- Respects system processor count
- Maintains output order
- Improves performance for multi-file results

**Evidence**: [See Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md#display-flow)

---

## Key Implementation Details

### Status Messages

During long operations, status messages are displayed:
- `ConsoleHelpers.DisplayStatus()` - Shows status
- `ConsoleHelpers.DisplayStatusErase()` - Clears status

Example: "Cloning repository 3/10..." (Program.cs line 799)

### Match Count Display

Code search shows:
- Total files matched
- Total matches across all files
- Matches per file

Example: "Found 3 file(s) with 12 matches:" (Program.cs line 680)

### URL Display

Multiple URL types:
- **Repository URL**: `https://github.com/owner/repo`
- **File blob URL**: `https://github.com/owner/repo/blob/main/path/to/file`
- **Raw file URL**: `https://raw.githubusercontent.com/owner/repo/main/path/to/file`

Raw URLs are shown at the end of each file section (Program.cs line 854)

### Error Handling

If fetching file content fails, falls back to fragment display from GitHub API (Program.cs lines 829-851)

---

## Configuration

No persistent configuration for display options. All controlled by command-line flags.

---

## Summary

Layer 6 (Display Control) provides:

1. **Multiple output formats** (detailed, repos, urls, json, csv, table, files, filenames)
2. **Result limiting** (--max-results)
3. **Language-aware code fencing** (automatic detection from file extensions)
4. **Hierarchical output structure** (repo → file → line)
5. **Line numbering** (actual line numbers from source files)
6. **Match highlighting** (visual emphasis on matched terms)
7. **Color-coded console output** (section headers, success, warnings, errors)
8. **Star count formatting** (human-readable with k/m suffixes)
9. **Match statistics** (file counts, match counts)
10. **Multiple URL formats** (repo, blob, raw)
11. **Parallel processing** (for multi-file code search)
12. **Status messages** (for long operations)

**[View Detailed Proof →](cycodgr-search-filtering-pipeline-catalog-layer-6-proof.md)**
