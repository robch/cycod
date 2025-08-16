---
hide:
  - toc
---

--8<-- "snippets/ai-generated.md"

# CYCODMD

CYCODMD is a command-line utility that generates rich markdown from various sources.

??? question "What is CYCODMD used for?"

    CYCODMD helps you:
    
    - Convert files of many types to well-formatted markdown
    - Run commands and format their output as markdown
    - Search the web and convert results to markdown
    - Retrieve content from URLs and convert to markdown
    - Apply AI processing to enhance any of these outputs

## Core commands

``` bash title="Convert files to markdown"
cycodmd README.md
cycodmd "**/*.cs" --file-contains "public class"
cycodmd "image.png" --file-instructions "Describe this image"
```

``` bash title="Run commands and format output"
cycodmd run "ls -la"
cycodmd run --powershell "Get-Process | Sort-Object CPU -Descending | Select-Object -First 10"
```

``` bash title="Search the web and get content"
cycodmd web search "yaml tutorial" --max 3 --get
cycodmd web get "https://example.com" --strip
```

## File filtering

``` bash title="Filter by file patterns"
cycodmd "**/*.cs" --exclude "**/bin/" "**/obj/"
```

``` bash title="Filter by file content"
cycodmd "**/*.js" --file-contains "export class"
cycodmd "**/*.js" --file-not-contains "TODO"
```

``` bash title="Filter by line content"
cycodmd "**/*.md" --contains "TODO"
cycodmd "**/*.cs" --line-contains "public class"
cycodmd "**/*.cs" --remove-all-lines "^\s*//"
```

## Line formatting

``` bash title="Show context around matches"
cycodmd "**/*.md" --contains "TODO" --lines 3
cycodmd "**/*.md" --contains "TODO" --lines-before 2 --lines-after 5
```

``` bash title="Include line numbers"
cycodmd "**/*.cs" --line-numbers
```

## AI processing

``` bash title="Process individual files"
cycodmd "**/*.json" --file-instructions "convert to YAML"
```

``` bash title="Process specific file types"
cycodmd "**/*" --cs-file-instructions "explain this code" --md-file-instructions "summarize"
```

``` bash title="Process combined output"
cycodmd "**/*.md" --instructions "Create a summary table"
```

``` bash title="Multi-step processing"
cycodmd "**/*.json" --file-instructions @step1.md @step2.md
```

## Output options

``` bash title="Save file outputs separately"
cycodmd "**/*.cs" --file-instructions "document this code" --save-file-output "docs/{fileBase}.md"
```

``` bash title="Save combined output"
cycodmd "**/*.md" --instructions "summarize" --save-output summary.md
```

``` bash title="Save as an alias for reuse"
cycodmd --lines 5 --file-contains "ERROR" --save-alias error-finder
cycodmd --error-finder "**/*.log"
```

## Web search

``` bash title="Search with different engines"
cycodmd web search "yaml tutorial" --google     # Default
cycodmd web search "yaml tutorial" --bing
cycodmd web search "yaml tutorial" --duckduckgo
```

``` bash title="Get content from search results"
cycodmd web search "yaml tutorial" --get --max 3
cycodmd web search "yaml tutorial" --get --strip
```

``` bash title="Use search APIs"
cycodmd web search "yaml tutorial" --bing-api
cycodmd web search "yaml tutorial" --google-api
```

## Web content

``` bash title="Get content from URLs"
cycodmd web get "https://example.com"
cycodmd web get "https://example.com" --strip
```

``` bash title="Use different browsers"
cycodmd web get "https://example.com" --firefox
cycodmd web get "https://example.com" --webkit
```

``` bash title="Interactive mode"
cycodmd web get "https://dynamic-site.com" --interactive
```

## Run commands

``` bash title="Run with different shells"
cycodmd run "dir"                            # Default
cycodmd run --cmd "dir"                      # Windows CMD
cycodmd run --bash "ls -la"                  # Bash
cycodmd run --powershell "Get-ChildItem"     # PowerShell
```

``` bash title="Process command output"
cycodmd run "npm list" --instructions "Format as a dependency table"
```

## Performance options

``` bash title="Control parallel processing"
cycodmd "**/*.cs" --file-instructions "document" --threads 4
```

--8<-- "snippets/ai-generated.md"