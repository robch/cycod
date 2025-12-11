# cycodgh - GitHub Search CLI

A command-line tool for searching GitHub repositories and optionally cloning them.

## Features

- **Search GitHub repositories** by keywords
- **Search code within files** (e.g., search for packages in .csproj files)
- **Clone repositories** to a local directory
- **Add as git submodules** (optional)
- **Filter by programming language**
- **Sort results** by stars, forks, or updated date

## Installation

```bash
dotnet pack src/cycodgh/cycodgh.csproj
dotnet tool install --global --add-source ./src/cycodgh/nupkg CycoGh
```

## Usage

### Basic Search

Search for repositories containing keywords:

```bash
cycodgh search dotnet cli tools
```

### Search Code in Specific Files

Search for code within specific file types (e.g., .csproj files):

```bash
cycodgh search Microsoft.Extensions.AI --in-files csproj
```

### Clone Repositories

Clone the top N repositories to a local directory:

```bash
cycodgh search dotnet cli --clone --max-clone 5
```

### Advanced Options

```bash
cycodgh search machine-learning \
  --language python \
  --max-results 10 \
  --clone \
  --clone-dir ./external \
  --sort stars
```

## Command-Line Options

### Search Command

```
cycodgh search <keywords...> [options]
```

**Options:**

- `--max-results N` - Maximum number of search results (default: 10)
- `--clone` - Clone repositories to local directory
- `--max-clone N` - Maximum number of repositories to clone (default: 10)
- `--clone-dir PATH` - Directory to clone into (default: "./external")
- `--as-submodules` - Add cloned repos as git submodules
- `--language LANG` - Filter by programming language (e.g., csharp, python)
- `--in-files EXT` - Search code in files with extension (e.g., csproj, json)
- `--file-extension EXT` - Alias for --in-files
- `--sort FIELD` - Sort by: stars, forks, or updated (default: stars)
- `--include-forks` - Include forked repositories in results
- `--save-output FILE` - Save search results to file

## Examples

### Search for repos with "ai" and "dotnet"

```bash
cycodgh search ai dotnet
```

### Find C# projects using Microsoft.Extensions.AI

```bash
cycodgh search Microsoft.Extensions.AI --in-files csproj --language csharp
```

### Clone top 3 machine learning repos

```bash
cycodgh search machine-learning --language python --clone --max-clone 3
```

### Add repos as submodules

```bash
cycodgh search react components --clone --as-submodules --clone-dir ./vendor
```

## Requirements

- [GitHub CLI (gh)](https://cli.github.com/) must be installed and authenticated
- Git must be installed for cloning functionality

## Authentication

cycodgh uses the GitHub CLI for authentication. Make sure you're logged in:

```bash
gh auth login
```

## License

MIT License - Copyright (c) 2025 Rob Chambers
