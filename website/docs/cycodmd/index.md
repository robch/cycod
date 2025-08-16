---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# CYCODMD Overview

CYCODMD is an AI-powered command-line tool that generates rich markdown content from various sources, including files, command outputs, web searches, and web pages. It works alongside CYCOD to enhance your AI interactions with dynamic contextual information.

## What is CYCODMD?

CYCODMD serves as a powerful companion tool to CYCOD, focusing specifically on gathering and transforming content into well-formatted markdown. It enables you to:

- Convert various file types (code, documents, PDFs, images) to markdown
- Run shell commands and convert their output to markdown
- Search the web and convert search results to markdown
- Retrieve web page content and convert it to markdown
- Apply AI processing to any of these outputs

## Key Features

### File Conversion
Convert almost any file type to markdown with intelligent formatting:

``` bash title="Convert a single file to markdown"
cycodmd README.md
```

``` bash title="Convert multiple files"
cycodmd *.json *.cs
```

``` bash title="Find files recursively"
cycodmd "**/*.cs" --file-contains "public class"
```

### Command Execution
Run commands and convert their output to markdown:

``` bash title="Run a simple command"
cycodmd run "ls -la" --instructions "Format as a table"
```

``` bash title="Use different shells"
cycodmd run --powershell "Get-Process" --instructions "List CPU-intensive processes"
```

### Web Search & Content Retrieval
Search the web and process the results:

``` bash title="Search for information"
cycodmd web search "yaml tutorial" --max 3 --get
```

``` bash title="Retrieve specific web pages"
cycodmd web get "https://learnxinyminutes.com/docs/yaml/" --strip
```

### AI Processing
Apply AI instructions to transform content:

``` bash title="Process file content with AI"
cycodmd "**/*.cs" --file-instructions "Explain each class's purpose"
```

``` bash title="Process web content with AI"
cycodmd web search "python best practices" --get --instructions "Create a concise cheat sheet"
```

## Basic Usage

The general syntax for using CYCODMD is:

``` bash title="General syntax"
cycodmd [files or patterns] [options]
```

For subcommands:

``` bash title="Subcommand syntax"
cycodmd run [command] [options]
cycodmd web search [query] [options]
cycodmd web get [url] [options]
```
