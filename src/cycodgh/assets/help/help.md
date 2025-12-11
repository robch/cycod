# cycodgh - GitHub Search CLI

Search GitHub repositories and optionally clone them.

## Usage

```
cycodgh search <keywords...> [options]
```

## Common Options

- `--help` - Display help information
- `--version` - Display version information
- `--verbose` - Enable verbose output
- `--quiet` - Suppress non-essential output

## Commands

- `search` - Search GitHub repositories or code (default command)
- `help` - Display help for a specific topic
- `version` - Display version information

## Search Options

- `--max-results N` - Maximum search results (default: 10)
- `--clone` - Clone repositories locally
- `--max-clone N` - Max repos to clone (default: 10)
- `--clone-dir PATH` - Clone directory (default: "./external")
- `--in-files EXT` - Search code in file extension
- `--language LANG` - Filter by programming language
- `--sort FIELD` - Sort by: stars, forks, updated
- `--include-forks` - Include forked repositories
- `--as-submodules` - Add as git submodules

## Examples

Search for repositories:
```
cycodgh search dotnet cli tools
```

Search code in specific files:
```
cycodgh search Microsoft.Extensions.AI --in-files csproj
```

Clone top 5 results:
```
cycodgh search machine-learning --clone --max-clone 5
```

## For More Information

Visit: https://github.com/robch/cycod
