---
hide:
  - toc
icon: material/tune
---

--8<-- "snippets/ai-generated.md"

# CYCODMD Options

CYCODMD provides flexible options for processing files, running commands, and generating markdown.

--8<-- "snippets/ai-generated.md"

## Option Categories

Options are organized into functional groups for different aspects of CYCODMD.

### File and Pattern Options

Control which files are processed:

| Option | Description |
|--------|-------------|
| `--exclude PATTERN` | Exclude files that match the pattern |
| `--file-contains REGEX` | Match only files containing the regex pattern |
| `--file-not-contains REGEX` | Exclude files containing the regex pattern |

``` { .bash .cli-command title="Filter files with patterns" }
cycodmd "**/*.cs" --exclude "**/bin/" "**/obj/"
cycodmd "**/*.md" --file-contains "# Overview"
```

### Line Processing Options

Control how lines within files are processed:

| Option | Description |
|--------|-------------|
| `--contains REGEX` | Match only files and lines with the regex pattern |
| `--line-contains REGEX` | Match only lines with the regex pattern |
| `--remove-all-lines REGEX` | Remove matching lines |
| `--lines N` | Include N lines before and after matches |
| `--lines-after N` | Include N lines after matches (default 0) |
| `--lines-before N` | Include N lines before matches (default 0) |
| `--line-numbers` | Include line numbers in output |

``` { .bash .cli-command title="Find and format lines" }
cycodmd "**/*.cs" --line-contains "TODO" --line-numbers --lines 2
```

``` { .bash .cli-command title="Remove comments before processing" }
cycodmd "**/*.js" --remove-all-lines "^\s*\/\/" --file-instructions "explain this code"
```

### AI Processing Options

Apply AI processing to content:

| Option | Description |
|--------|-------------|
| `--file-instructions "..."` | Apply instructions to each file |
| `--EXT-file-instructions "..."` | Apply instructions to files with specific extension |
| `--page-instructions "..."` | Apply instructions to each web page |
| `--SITE-page-instructions "..."` | Apply instructions to pages from specific sites |
| `--instructions "..."` | Apply instructions to overall output |
| `--threads N` | Limit concurrent processing threads |
| `--save-chat-history [FILE]` | Save chat history to specified file |

``` { .bash .cli-command title="Process files with AI" }
cycodmd "**/*.md" --file-instructions "Summarize this document in bullet points"
```

``` { .bash .cli-command title="Process different file types differently" }
cycodmd "**/*" --cs-file-instructions "Explain this code" --md-file-instructions "Summarize this content"
```

### Output Options

Control how output is saved:

| Option | Description |
|--------|-------------|
| `--save-file-output [FILE]` | Save each file output to template file |
| `--save-page-output [FILE]` | Save each web page output to template file |
| `--save-output [FILE]` | Save command output to specified file |
| `--save-alias ALIAS` | Save options as an alias (use via `--{ALIAS}`) |

``` { .bash .cli-command title="Save output to files" }
cycodmd "**/*.md" --file-instructions "summarize" --save-file-output "summaries/{fileBase}.md"
cycodmd "**/*.cs" --instructions "Document the codebase" --save-output "documentation.md"
```

### Web Search Options

Options for `web search` command:

| Option | Description |
|--------|-------------|
| `--bing` | Use Bing search engine |
| `--duckduckgo` | Use DuckDuckGo search engine |
| `--google` | Use Google search engine (default) |
| `--yahoo` | Use Yahoo search engine |
| `--bing-api` | Use Bing search API (requires credentials) |
| `--google-api` | Use Google search API (requires credentials) |
| `--get` | Download content from search results |
| `--max NUMBER` | Maximum search results (default: 10) |

``` { .bash .cli-command title="Search the web and process results" }
cycodmd web search "yaml tutorial" --get --max 3
cycodmd web search "python list comprehension" --bing --get --instructions "Create a tutorial"
```

### Browser Options

Control browser behavior for web commands:

| Option | Description |
|--------|-------------|
| `--interactive` | Run in browser interactive mode |
| `--chromium` | Use Chromium browser (default) |
| `--firefox` | Use Firefox browser |
| `--webkit` | Use WebKit browser |
| `--strip` | Strip HTML tags from downloaded content |

``` { .bash .cli-command title="Get web content with different browsers" }
cycodmd web get "https://example.com" --firefox --strip
cycodmd web get "https://dynamic-site.com" --interactive --webkit
```

### Run Command Options

Options for `run` command:

| Option | Description |
|--------|-------------|
| `--cmd [COMMAND]` | Run command in CMD shell |
| `--bash [COMMAND]` | Run command in Bash shell |
| `--powershell [COMMAND]` | Run command in PowerShell |
| `--script [COMMAND]` | Run command in default shell |

``` { .bash .cli-command title="Run commands and format output" }
cycodmd run "ls -la" --instructions "Create a table of files"
cycodmd run --powershell "Get-Process" --instructions "List CPU-intensive processes"
```

## Common Option Combinations

### Finding and Filtering Files

``` { .bash .cli-command title="Find specific code patterns" }
cycodmd "**/*.cs" --file-contains "public class" --line-contains "TODO" --line-numbers
```

### Line Context Control

``` { .bash .cli-command title="Show context around matches" }
cycodmd "**/*.log" --line-contains "ERROR" --lines-after 3 --lines-before 1
```

### AI Processing

``` { .bash .cli-command title="Multi-step processing" }
cycodmd "**/*.cs" --file-instructions "Extract class names" "List public methods" "Identify dependencies"
```

``` { .bash .cli-command title="Using instruction files" }
cycodmd "**/*.js" --file-instructions @code-analysis.txt @formatting.txt
```

### Output Control

``` { .bash .cli-command title="Create reusable aliases" }
cycodmd "**/*.cs" --file-contains "public class" --line-numbers --save-alias cs-classes
cycodmd --cs-classes --file-instructions "Document these classes"
```

## Detailed Documentation

For more detailed information about specific option categories, see:

- [File Options](file-options.md)
- [Line Options](line-options.md)
- [AI Options](ai-options.md)
- [Output Options](output-options.md)