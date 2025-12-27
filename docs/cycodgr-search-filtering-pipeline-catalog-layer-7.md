# cycodgr Search Command - Layer 7: Output Persistence

**[< Layer 6: Display Control](cycodgr-search-filtering-pipeline-catalog-layer-6.md) | [Layer 8: AI Processing >](cycodgr-search-filtering-pipeline-catalog-layer-8.md)**

**[Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-7-proof.md)** | **[Back to README](cycodgr-search-filtering-pipeline-catalog-README.md)**

---

## Purpose

Layer 7 controls **where and how** search results are persisted to files. cycodgr provides comprehensive output options across multiple formats (markdown, JSON, CSV, plain text) and specialized output types (repos, URLs, file paths).

---

## Command-Line Options

### Primary Output

#### `--save-output <file>`
**Purpose**: Save the main formatted output to a markdown file

**Template Variables**: Supports `{time}` and other FileHelpers templates

**Default Behavior**: 
- If not specified, output goes to console only
- Default template used: `"unified-output.md"`, `"repo-output.md"`, or `"code-output.md"` depending on search mode

**Example**:
```bash
cycodgr --repo-contains "terminal emulator" --save-output search-results.md
```

---

### Structured Data Formats

#### `--save-json <file>`
**Purpose**: Save results in JSON format for machine consumption

**Template Variables**: Supports `{time}` template  
**Default Template**: `"output.json"`

**Output Structure**:
- **For repository search**: Array of repository objects with metadata
- **For code search**: Array of code match objects with repository, path, and text matches

**Example**:
```bash
cycodgr --file-contains "ChatClient" --cs --save-json results.json
```

---

#### `--save-csv <file>`
**Purpose**: Save results in CSV format for spreadsheet import

**Template Variables**: Supports `{time}` template  
**Default Template**: `"output.csv"`

**CSV Headers**:
- **Repository search**: `url,name,owner,stars,language,description`
- **Code search**: `repo_url,repo_name,repo_owner,repo_stars,file_path,file_url`

**Example**:
```bash
cycodgr --repo-contains "machine learning" --py --save-csv ml-repos.csv
```

---

#### `--save-table <file>`
**Purpose**: Save results in markdown table format

**Template Variables**: Supports `{time}` template  
**Default Template**: `"output.md"`

**Availability**: Repository search only (not available for code search)

**Table Format**:
```markdown
| Repository | Stars | Language | Description |
|------------|-------|----------|-------------|
| owner/repo | 1.2k  | Python   | Description |
```

**Example**:
```bash
cycodgr --repo-contains "devops" --save-table repos-table.md
```

---

### Specialized List Outputs

#### `--save-urls <file>`
**Purpose**: Save URLs in plain text format (context-dependent)

**Template Variables**: Supports `{time}` template  
**Default Template**: `"output.txt"`

**Contextual Behavior**:
- **Repository search**: Saves repository clone URLs (e.g., `https://github.com/owner/repo.git`)
- **Code search**: Saves file URLs (e.g., `https://github.com/owner/repo/blob/main/path/to/file.cs`)

**Example**:
```bash
# Clone URLs for repos
cycodgr --repo-contains "kubernetes" --save-urls clone-urls.txt

# File URLs for code search
cycodgr --file-contains "import torch" --py --save-urls file-urls.txt
```

---

#### `--save-repos <file>`
**Purpose**: Save repository names in `owner/repo` format (one per line)

**Template Variables**: Supports `{time}` template  
**Default Template**: `"repos.txt"`

**Format**: Compatible with `--repos @file` for chaining searches

**Example**:
```bash
# Step 1: Find repos with csproj files mentioning "SDK"
cycodgr --repo-csproj-file-contains "Microsoft.NET.Sdk" --save-repos dotnet-repos.txt

# Step 2: Search those repos for specific code
cycodgr --repos @dotnet-repos.txt --file-contains "ConfigureServices"
```

---

#### `--save-file-paths <template>`
**Purpose**: Save file paths per repository (code search only)

**Template Variables**: 
- `{repo}`: Repository name (with `/` replaced by `-`)
- `{time}`: Timestamp

**Default Template**: `"files-{repo}.txt"`

**Behavior**: 
- Creates **one file per repository** found in search results
- Each file contains paths of matching files within that repo
- Paths are written with CRLF line endings and UTF-8 without BOM for `@file` compatibility

**Example**:
```bash
cycodgr --file-contains "ObservableCollection" --cs --save-file-paths files-{repo}.txt
# Creates: files-microsoft-terminal.txt, files-dotnet-runtime.txt, etc.
```

**Integration**:
```bash
# Can be used with --file-paths for targeted searches
cycodgr microsoft/terminal --file-paths @files-microsoft-terminal.txt --line-contains "async"
```

---

#### `--save-repo-urls <file>`
**Purpose**: Save repository clone URLs (works for both repo and code search)

**Template Variables**: Supports `{time}` template  
**Default Template**: `"repo-urls.txt"`

**Format**: Git clone URLs (e.g., `https://github.com/owner/repo.git`)

**Example**:
```bash
cycodgr --repo-contains "compiler" --rs --save-repo-urls rust-compiler-repos.txt
```

---

#### `--save-file-urls <template>`
**Purpose**: Save GitHub file URLs per repository (code search only)

**Template Variables**: 
- `{repo}`: Repository name (with `/` replaced by `-`)
- `{time}`: Timestamp

**Default Template**: `"file-urls-{repo}.txt"`

**Behavior**: 
- Creates **one file per repository**
- Contains GitHub blob URLs (clickable in browsers)
- Format: `https://github.com/owner/repo/blob/main/path/to/file`

**Example**:
```bash
cycodgr --file-contains "CUDA" --cpp --save-file-urls urls-{repo}.txt
# Creates: urls-tensorflow-tensorflow.txt, urls-pytorch-pytorch.txt, etc.
```

---

## Output Combinations

Multiple output options can be combined in a single search:

```bash
cycodgr --repo-contains "neural network" --py \
  --save-output search-results.md \
  --save-json results.json \
  --save-csv results.csv \
  --save-repos repo-list.txt \
  --save-repo-urls clone-urls.txt
```

All specified outputs will be generated and saved.

---

## Template Processing

All output file names support template variables processed by `FileHelpers.GetFileNameFromTemplate()`:

**Supported Variables**:
- `{time}`: Current timestamp (e.g., `2025-01-15-143022`)
- `{repo}`: Repository name with `/` replaced by `-` (file path specific outputs)
- `{ProgramName}`: `cycodgr`

**Example**:
```bash
--save-output "search-{time}.md"
# Becomes: search-2025-01-15-143022.md
```

---

## Search Mode Specific Availability

| Option | Repo Metadata | Repo Search | Code Search | Unified Search |
|--------|---------------|-------------|-------------|----------------|
| `--save-output` | ✅ | ✅ | ✅ | ✅ |
| `--save-json` | ❌ | ✅ | ✅ | ⚠️ Partial |
| `--save-csv` | ❌ | ✅ | ✅ | ⚠️ Partial |
| `--save-table` | ❌ | ✅ | ❌ | ⚠️ Partial |
| `--save-urls` | ❌ | ✅ | ✅ | ✅ |
| `--save-repos` | ❌ | ✅ | ✅ | ✅ |
| `--save-file-paths` | ❌ | ❌ | ✅ | ⚠️ Partial |
| `--save-repo-urls` | ❌ | ✅ | ✅ | ✅ |
| `--save-file-urls` | ❌ | ❌ | ✅ | ⚠️ Partial |

**Legend**:
- ✅ Fully supported
- ❌ Not applicable for this search mode
- ⚠️ Partial: Works for applicable result types within unified search

---

## Success Feedback

When files are saved successfully, cycodgr displays:
```
Saved to: <filename>
```

in green text (unless `--quiet` is enabled).

Multiple files are reported one per line when using combined output options.

---

## Implementation Notes

### File Creation
- Files are created using `FileHelpers.WriteAllText()`
- UTF-8 encoding with BOM for markdown/JSON/CSV
- UTF-8 without BOM for plain text lists (better @ file compatibility)

### CRLF vs LF
- File path lists use `\r\n` (CRLF) for Windows compatibility
- Ensures `File.ReadAllLines()` works correctly when loading with `@file`

### Error Handling
- File write errors are logged but don't stop the search
- If a save operation fails, the search continues and other outputs are still saved

---

## Examples

### Example 1: Research Workflow
```bash
# Find ML repos and save everything
cycodgr --repo-contains "machine learning" --py --min-stars 1000 \
  --save-output ml-search.md \
  --save-repos ml-repos.txt \
  --save-json ml-repos.json \
  --save-table ml-repos-table.md
```

### Example 2: Code Analysis Pipeline
```bash
# Step 1: Find repos with specific file content
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
  --save-repos ai-repos.txt

# Step 2: Search those repos for specific patterns
cycodgr --repos @ai-repos.txt \
  --file-contains "ChatClient" \
  --save-file-paths files-{repo}.txt \
  --save-output ai-chat-analysis.md

# Step 3: Deep dive on specific repo's files
cycodgr microsoft/semantic-kernel \
  --file-paths @files-microsoft-semantic-kernel.txt \
  --line-contains "ConfigureAwait" \
  --save-output sk-configureawait-review.md
```

### Example 3: Clone Preparation
```bash
# Find repos to clone
cycodgr --repo-contains "terminal emulator" --min-stars 500 \
  --save-repo-urls clone-list.txt

# Then clone them manually or with script:
# cat clone-list.txt | xargs -I {} git clone {}
```

---

## Related Layers

- **[Layer 6: Display Control](cycodgr-search-filtering-pipeline-catalog-layer-6.md)**: Controls format before saving
- **[Layer 8: AI Processing](cycodgr-search-filtering-pipeline-catalog-layer-8.md)**: Can process output before saving
- **[Layer 9: Actions on Results](cycodgr-search-filtering-pipeline-catalog-layer-9.md)**: Clone repos found in search

---

**[< Layer 6: Display Control](cycodgr-search-filtering-pipeline-catalog-layer-6.md) | [Layer 8: AI Processing >](cycodgr-search-filtering-pipeline-catalog-layer-8.md)**

**[Proof Document](cycodgr-search-filtering-pipeline-catalog-layer-7-proof.md)** | **[Back to README](cycodgr-search-filtering-pipeline-catalog-README.md)**
