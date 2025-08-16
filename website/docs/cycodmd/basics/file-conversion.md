---
hide:
- toc
icon: material/file-replace
---

--8<-- "snippets/ai-generated.md"

# File Conversion with CYCODMD

CYCODMD offers powerful file conversion capabilities, allowing you to transform various file types into well-formatted markdown. This makes it easy to include file content in your AI contexts, documentation, or reports.

## Basic File Conversion

At its simplest, CYCODMD can convert one or more files to markdown with just a file path or pattern:

``` bash title="Convert a single file"
cycodmd README.md
```

``` bash title="Convert multiple files"
cycodmd document.docx presentation.pptx
```

``` bash title="Convert all C# files"
cycodmd "**/*.cs"
```

## Supported File Types

CYCODMD handles a wide range of file types:

| File Type | Extensions | Notes |
|-----------|------------|-------|
| Text/Code | .md, .txt, .cs, .js, .py, .java, etc. | Preserves formatting, adds syntax highlighting |
| Documents | .docx, .doc, .rtf | Preserves structure, converts tables and lists |
| Presentations | .pptx, .ppt | Converts slides to markdown sections |
| PDFs | .pdf | Extracts text and basic structure |
| Images | .png, .jpg, .gif, .bmp | Extracts visual descriptions using AI vision (requires Azure OpenAI) |
| Spreadsheets | .xlsx, .csv | Converts to markdown tables |

## File Filtering Options

CYCODMD provides powerful filtering capabilities to select only the files and content you need:

### Pattern Matching

Use glob patterns to select files:

``` bash title="Select all markdown files"
cycodmd "**/*.md"
```

``` bash title="Select multiple file types"
cycodmd "src/**/*.{cs,js}"
```

``` bash title="Use multiple patterns"
cycodmd "docs/*.md" "*.txt"
```

### Excluding Files

Exclude files that match specific patterns:

``` bash title="Exclude binary directories"
cycodmd "**/*.cs" --exclude "**/bin/" "**/obj/"
```

``` bash title="Exclude node_modules"
cycodmd "**/*.js" --exclude "**/node_modules/"
```

### Content-Based Filtering

Filter files based on their content:

``` bash title="Find files with specific content"
cycodmd "**/*.cs" --file-contains "public class"
```

``` bash title="Exclude files with specific content"
cycodmd "**/*.md" --file-not-contains "DRAFT"
```

## Line Filtering and Formatting

CYCODMD allows you to filter and format individual lines within files:

### Line Selection

``` bash title="Select lines with specific content"
cycodmd "**/*.cs" --line-contains "TODO"
```

``` bash title="Remove lines with specific content"
cycodmd "**/*.log" --remove-all-lines "DEBUG:"
```

### Context Lines

Include lines before and after matching lines:

``` bash title="Include context around matches"
cycodmd "**/*.cs" --line-contains "TODO" --lines 2
```

``` bash title="Include lines after matches"
cycodmd "**/*.cs" --line-contains "TODO" --lines-after 5
```

``` bash title="Include lines before matches"
cycodmd "**/*.cs" --line-contains "TODO" --lines-before 2
```

### Line Numbers

Add line numbers to your output:

``` bash title="Add line numbers"
cycodmd "**/*.cs" --line-numbers
```

## AI Processing for Files

One of CYCODMD's most powerful features is its ability to apply AI processing to your files:

``` bash title="Process each file with AI instructions"
cycodmd "**/*.json" --file-instructions "convert to YAML format"
```

``` bash title="Apply different instructions to different file types"
cycodmd --cs-file-instructions "Explain what this code does" --md-file-instructions "Summarize this content"
```

``` bash title="Multi-step instructions from files"
cycodmd "**/*.cs" --file-instructions @step1.md @step2.md
```

## Output Management

Control how your converted files are output:

``` bash title="Save individual file outputs"
cycodmd "**/*.cs" --save-file-output "outputs/{fileBase}-processed.md"
```

``` bash title="Process multiple files and apply AI instructions"
cycodmd "**/*.cs" --instructions "Create a summary table of all these files"
```

## Examples

### Basic Code Documentation

Convert all C# files to markdown, including line numbers:

``` bash title="Convert code with line numbers"
cycodmd "**/*.cs" --line-numbers
```

### Finding TODOs in Code

Extract all TODO comments with context:

``` bash title="Find TODOs with context"
cycodmd "**/*.{cs,js,py}" --line-contains "TODO" --lines 2 --line-numbers
```

### Code Review Assistance

Process code files for a review:

``` bash title="Code review assistance"
cycodmd "src/**/*.cs" --file-instructions "Identify potential bugs and performance issues in this code"
```

### Document Summarization

Summarize a collection of documents:

``` bash title="Summarize documents"
cycodmd "docs/**/*.docx" --file-instructions "Summarize the key points" --instructions "Create an executive summary of all documents"
```

### Technical Documentation Generation

Generate technical documentation from code:

``` bash title="Generate API documentation"
cycodmd "src/api/**/*.cs" --file-instructions "Extract public interfaces and document parameters" --save-file-output "docs/api/{fileBase}.md"
```
