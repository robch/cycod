# cycodgr search - Layer 7: OUTPUT PERSISTENCE

## Purpose

Layer 7 defines **where to save results** - file outputs in various formats.

## Command-Line Options

### `--save-output <file>`

**Purpose**: Save markdown output to file

**Default template**: `search-output.md`

---

### `--save-json <file>`

**Purpose**: Save results as JSON

**Default**: `output.json`

---

### `--save-csv <file>`

**Purpose**: Save results as CSV

**Default**: `output.csv`

---

### `--save-table <file>`

**Purpose**: Save results as markdown table

**Default**: `output.md`

---

### `--save-urls <file>`

**Purpose**: Save URLs to file (context-dependent)

**For repository search**: Clone URLs  
**For code search**: File blob URLs

**Default**: `output.txt`

---

### `--save-repos <file>`

**Purpose**: Save repository names (owner/repo format)

**Default**: `repos.txt`

**Use case**: Output can be loaded with `--repos @file`

---

### `--save-file-paths <template>`

**Purpose**: Save file paths grouped by repository

**Default template**: `files-{repo}.txt`

**Variables**: `{repo}` is replaced with repository name

---

### `--save-repo-urls <file>`

**Purpose**: Save repository clone URLs

**Default**: `repo-urls.txt`

---

### `--save-file-urls <template>`

**Purpose**: Save file blob URLs grouped by repository

**Default template**: `file-urls-{repo}.txt`

---

## Template Variables

- `{repo}`: Repository name (e.g., `microsoft-terminal` for `microsoft/terminal`)
- `{time}`: Timestamp (inherited from common layer)
- `{filePath}`, `{fileBase}`: File components

## Data Flow

```
Search Results
  ↓
Format Results
  ↓
SaveAdditionalFormats(command, data, query, searchType)
  ↓
For each --save-* option:
  - Check if option is set
  - Generate filename from template
  - Format data appropriately
  - Write to file
  - Report saved file to user
```

---

For detailed source code evidence, see: [**Layer 7 Proof Document**](cycodgr-search-layer-7-proof.md)
