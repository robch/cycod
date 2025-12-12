# cycodgr - GitHub Search CLI

A command-line tool for searching GitHub repositories and code files.

## Features

- **Search GitHub repositories** by keywords
- **Search code within files** by keywords and file extension
- **Clone repositories** to a local directory
- **Add as git submodules** (optional)
- **Filter by programming language**
- **Sort results** by relevance (default), stars, forks, or updated date
- **Multiple output formats**: detailed, url, filenames, files, repos, table, json, csv

## Installation

```bash
dotnet pack src/cycodgr/cycodgr.csproj
dotnet tool install --global --add-source ./src/cycodgr/nupkg CycoGr
```

## Usage

### Basic Repository Search

Search for repositories containing keywords:

```bash
cycodgr repo dotnet cli tools
```

### Basic Code Search

Search for code containing keywords:

```bash
cycodgr code "AGENTS.md"
```

Search in specific file types:

```bash
cycodgr code Microsoft.Extensions.AI --in-files csproj
```

### Clone Repositories

Clone the top N repositories to a local directory:

```bash
cycodgr repo dotnet cli --clone --max-clone 5
```

### Advanced Options

```bash
cycodgr repo machine-learning \
  --language python \
  --max-results 10 \
  --clone \
  --clone-dir ./external \
  --sort stars
```

```bash
cycodgr code "error handling" \
  --language csharp \
  --format filenames \
  --lines 3
```

## Command-Line Options

### Repo Command

```
cycodgr repo <keywords...> [options]
```

**Options:**

- `--max-results N` - Maximum number of search results (default: 10)
- `--clone` - Clone repositories to local directory
- `--max-clone N` - Maximum number of repositories to clone (default: 10)
- `--clone-dir PATH` - Directory to clone into (default: "./external")
- `--as-submodules` - Add cloned repos as git submodules
- `--language LANG` - Filter by programming language (e.g., csharp, python)
- `--owner ORG` - Filter by owner/organization
- `--sort FIELD` - Sort by: stars, forks, updated, or help-wanted-issues (default: relevance)
- `--include-forks` - Include forked repositories in results
- `--exclude-fork` - Exclude forked repositories
- `--only-forks` - Show only forked repositories
- `--min-stars N` - Filter repositories with at least N stars
- `--format FORMAT` - Output format: detailed, url, table, json, csv (default: detailed)
- `--repo OWNER/REPO` - Search within specific repository (repeatable)
- `--repos REPO1 REPO2 @FILE` - Search within multiple repositories
- `--exclude PATTERN [PATTERN...]` - Exclude results matching regex pattern(s)
- `--save-output FILE` - Save search results to file
- `--save-json FILE` - Save results as JSON
- `--save-csv FILE` - Save results as CSV
- `--save-table FILE` - Save results as Markdown table
- `--save-urls FILE` - Save results as URLs only

### Code Command

```
cycodgr code <keywords...> [options]
```

**Options:**

- `--max-results N` - Maximum number of search results (default: 10)
- `--language LANG` - Filter by programming language (e.g., csharp, python)
- `--owner ORG` - Filter by owner/organization
- `--min-stars N` - Filter repositories with at least N stars
- `--in-files EXT` - Search code in files with extension (e.g., cs, md)
- `--extension EXT` - Alias for --in-files
- `--format FORMAT` - Output format: detailed, filenames, files, repos, urls, json, csv (default: detailed)
- `--lines N` - Context lines before/after matches (default: 5)
- `--lines-before-and-after N` - Alias for --lines
- `--repo OWNER/REPO` - Search within specific repository (repeatable)
- `--repos REPO1 REPO2 @FILE` - Search within multiple repositories
- `--exclude PATTERN [PATTERN...]` - Exclude results matching regex pattern(s)
- `--save-output FILE` - Save search results to file
- `--save-json FILE` - Save results as JSON
- `--save-csv FILE` - Save results as CSV
- `--save-urls FILE` - Save results as URLs only

## Output Formats

### Repo Command
- **detailed** (default) - URL | ⭐ stars | language | description
- **url** - Just repository URLs
- **table** - Markdown table format
- **json** - JSON array
- **csv** - CSV format

### Code Command
- **detailed** (default) - Markdown with code fences, line numbers, and match markers (*)
- **filenames** - File paths grouped by repository
- **files** - Raw file URLs (raw.githubusercontent.com)
- **repos** - Unique repository URLs only
- **urls** - Repository URLs with nested file URLs
- **json** - Structured JSON with matches
- **csv** - Flattened CSV format

## Examples

### Search for repos with "ai" and "dotnet"

```bash
cycodgr repo ai dotnet
```

### Search with table format

```bash
cycodgr repo "dotnet aspire" --format table
```

### Clone top 3 machine learning repos

```bash
cycodgr repo machine-learning --language python --clone --max-clone 3
```

### Add repos as submodules

```bash
cycodgr repo react components --clone --as-submodules --clone-dir ./vendor
```

### Filter by organization

```bash
cycodgr repo "ai agents" --owner microsoft
```

### Filter by minimum stars

```bash
cycodgr repo "dotnet cli" --min-stars 100
```

### Exclude forked repositories

```bash
cycodgr repo "aspire" --exclude-fork
```

### Show only forked repositories

```bash
cycodgr repo "dotnet examples" --only-forks
```

### Search for code in C# files

```bash
cycodgr code "AGENTS.md" --in-files cs
```

### Search within specific repository

```bash
cycodgr code "AGENTS.md" --repo microsoft/vscode
```

### Search within multiple repositories

```bash
cycodgr code "error handling" --repo microsoft/vscode --repo dotnet/aspire
cycodgr code "config" --repos microsoft/vscode dotnet/aspire openai/codex
```

### Load repositories from file

```bash
cycodgr code "AGENTS.md" --repos @my-repos.txt
```

### Get filenames only

```bash
cycodgr code "error handling" --format filenames
```

### Adjust context lines

```bash
cycodgr code config --lines 3
```

### Filter code search by organization and stars

```bash
cycodgr code "error handling" --owner microsoft --min-stars 100
```

## Requirements

- [GitHub CLI (gh)](https://cli.github.com/) must be installed and authenticated
- Git must be installed for cloning functionality

## Authentication

cycodgr uses the GitHub CLI for authentication. Make sure you're logged in:

```bash
gh auth login
```

## License

MIT License - Copyright (c) 2025 Rob Chambers
