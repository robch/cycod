---
hide:
- toc
icon: material/book-open-page-variant
---

--8<-- "snippets/ai-generated.md"

# Getting Started with CYCODMD

CYCODMD (Cycod Markdown) is a powerful CLI tool that generates rich markdown content from various sources including files, command outputs, web searches, and web pages. It works standalone or integrates with CYCOD to provide enhanced context for AI interactions.

## Converting Files to Markdown

The most basic usage is to convert one or more files to markdown:

``` bash title="Convert files to markdown"
cycodmd README.md
cycodmd "**/*.cs"
cycodmd image.png presentation.pptx document.pdf
```

CYCODMD can handle many file types including:

- Text files (`.txt`, `.md`, `.cs`, etc.)
- Documents (`.pdf`, `.docx`, etc.)
- Presentations (`.pptx`, etc.)
- Images (`.png`, `.jpg`, etc.)
- And many more

## Running Commands and Converting Output

Execute commands and convert their output to markdown:

``` bash title="Convert command output to markdown"
cycodmd run "ls -la"
cycodmd run --powershell "Get-Process | Sort-Object CPU -Descending | Select-Object -First 10"
```

## Web Search and Content Retrieval

Search the web and convert results to markdown:

``` bash title="Search the web"
cycodmd web search "yaml tutorial"
```

``` bash title="Retrieve web content"
cycodmd web get "https://example.com"
```

## Adding AI Processing

What makes CYCODMD powerful is its ability to apply AI processing to the generated content:

``` bash title="Process a file with AI instructions"
cycodmd README.md --file-instructions "Summarize this document"
```

``` bash title="Process command output"
cycodmd run "ls -la" --instructions "Create a table of files sorted by size"
```

``` bash title="Process web search results"
cycodmd web search "Python best practices" --instructions "Extract the top 10 tips"
```

## Output Options

By default, CYCODMD outputs to the console, but you can save results to files:

``` bash title="Save to a file"
cycodmd README.md --save-output summary.md
```

``` bash title="Save each processed file to its own output file"
cycodmd "**/*.cs" --save-file-output "docs/{fileBase}.md"
```

## Getting Help

CYCODMD has comprehensive help available:

``` bash title="Get general help"
cycodmd help
```

``` bash title="Get help on topics"
cycodmd help topics
```

``` bash title="Get example help"
cycodmd help examples
```

For more specific help on commands:

``` bash title="Get help on specific commands"
cycodmd help run
cycodmd help web search
cycodmd help web get
```
