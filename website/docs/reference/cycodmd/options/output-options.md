---
hide:
- toc
icon: material/file-export
---

--8<-- "snippets/ai-generated.md"

# Output Options

??? question "What are output options?"

    Output options control how and where the results of CYCODMD operations are saved.
    
    They let you save output to files, create dynamic file paths, and store commonly used option combinations as aliases.

## Saving Output

Use these options to control where and how output is saved.

### --save-output

Save the complete command output to a single file.

```bash title="Save combined output to a file"
cycodmd "**/*.cs" --instructions "Generate API documentation" --save-output api-docs.md
```

```bash title="Create a report from search results"
cycodmd web search "climate change" --get --instructions "Create a summary" --save-output report.md
```

### --save-file-output

Save each processed file's output to separate files using template variables.

```bash title="Save each file to a custom location"
cycodmd "**/*.cs" --save-file-output "docs/{fileBase}.md"
```

```bash title="Create an organized documentation structure"
cycodmd "**/*.cs" --file-instructions "Document this code" --save-file-output "docs/{dirName}/{fileBase}.md"
```

### --save-page-output

Save each web page's output to separate files using template variables.

```bash title="Save web pages by domain"
cycodmd web get "https://example.com" "https://example.org" --save-page-output "pages/{urlHost}.md"
```

```bash title="Save search results to organized files"
cycodmd web search "markdown tutorial" --get --save-page-output "search/{urlHost}-{urlBase}.md"
```

## Template Variables

### For File Output

| Variable | Description |
|----------|-------------|
| `{filePath}` | Directory path of the original file |
| `{fileName}` | Full filename with extension |
| `{fileBase}` | Filename without extension |
| `{fileExt}` | File extension |
| `{dirName}` | Name of the directory containing the file |

```bash title="Examples of file template variables"
cycodmd "**/*.cs" --save-file-output "output/{fileName}"
cycodmd "**/*.cs" --save-file-output "docs/{filePath}/{fileBase}.md"
cycodmd "**/*" --save-file-output "output/{fileExt}/{fileBase}.md"
```

### For Web Output

| Variable | Description |
|----------|-------------|
| `{url}` | Full URL |
| `{urlHost}` | Hostname from the URL |
| `{urlPath}` | Path from the URL |
| `{urlBase}` | Filename part of the URL |

```bash title="Examples of web template variables"
cycodmd web search "AI" --get --save-page-output "news/{urlHost}.md"
cycodmd web get "https://example.com/docs/guide.html" --save-page-output "mirror/{urlHost}/{urlPath}"
```

## Aliases

### --save-alias

Save the current set of options for later reuse.

```bash title="Save a common configuration"
cycodmd --lines 5 --file-contains "ERROR" --line-numbers --save-alias error-finder
```

```bash title="Save search settings"
cycodmd web search "example" --bing --max 5 --get --strip --save-alias bing-top5
```

### Using Aliases

```bash title="Apply saved options to new queries"
cycodmd --error-finder "**/*.log"
cycodmd --bing-top5 "markdown tutorial"
```

## Common Use Cases

```bash title="Generate documentation for a codebase"
cycodmd "**/*.cs" \
  --file-instructions "Generate documentation for this file" \
  --save-file-output "docs/{dirName}/{fileBase}.md"
```

```bash title="Create an incident report from logs"
cycodmd "**/*.log" \
  --line-contains "ERROR|WARN" \
  --lines 5 \
  --instructions "Create an incident report" \
  --save-output "incident-report.md"
```

```bash title="Process multiple files with both individual and combined output"
cycodmd "**/*.md" \
  --file-instructions "Extract key points" \
  --save-file-output "summaries/{fileBase}-summary.md" \
  --instructions "Create an executive summary" \
  --save-output "executive-summary.md"
```

## Related Options

- [File Options](file-options.md) - For selecting and filtering files
- [Line Options](line-options.md) - For filtering and formatting specific lines 
- [AI Options](ai-options.md) - For applying AI processing to content