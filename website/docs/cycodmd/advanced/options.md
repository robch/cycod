---
hide:
- toc
icon: material/tune-vertical
---

--8<-- "snippets/ai-generated.md"

# CYCODMD Command Options

CYCODMD provides a comprehensive set of command options for fine-grained control over how files are processed, content is filtered, and markdown is generated.

## Option Categories

CYCODMD options can be grouped into several functional categories:

| Category | Purpose |
|----------|---------|
| File Filtering | Control which files are included or excluded |
| Line Filtering | Filter content at the line level |
| Line Formatting | Control how lines are displayed in output |
| AI Processing | Apply AI instructions to transform content |
| Output Control | Manage how and where output is saved |

## File/Line Filtering Options

These options help you precisely target content by filtering files and lines based on patterns and regular expressions.

### File Selection

``` bash title="Exclude files by pattern"
cycodmd **/*.cs --exclude "**/bin/" "**/obj/"
```

``` bash title="Match files containing text"
cycodmd **/*.js --file-contains "export"
```

``` bash title="Exclude files containing text"
cycodmd **/*.js --file-not-contains "TODO"
```

### Line Selection

``` bash title="Match files and lines containing text"
cycodmd **/*.md --contains "TODO"
```

``` bash title="Match only specific lines"
cycodmd **/*.cs --line-contains "public class"
```

``` bash title="Remove matching lines"
cycodmd **/*.cs --remove-all-lines "^\s*//"
```

## Line Formatting Options

Control how lines are presented in the generated markdown output.

``` bash title="Include context lines"
cycodmd **/*.md --contains "TODO" --lines 3
```

``` bash title="Include lines before matches"
cycodmd **/*.md --contains "TODO" --lines-before 2
```

``` bash title="Include lines after matches"
cycodmd **/*.md --contains "TODO" --lines-after 5
```

``` bash title="Show line numbers"
cycodmd **/*.cs --file-contains "class" --line-numbers
```

## AI Processing Options

These options allow you to apply AI processing to transform and enhance your content.

``` bash title="Process each file"
cycodmd **/*.json --file-instructions "convert to YAML"
```

``` bash title="Process specific file types"
cycodmd **/* --cs-file-instructions "explain this code"
```

``` bash title="Process final output"
cycodmd **/*.md --instructions "Create a summary table"
```

### Multi-step Instructions

You can provide multiple instruction files for sequential AI processing:

``` bash title="Multi-step processing"
cycodmd **/*.json --file-instructions @step1-instructions.md @step2-instructions.md
```

### File-type Specific Instructions

Apply different AI instructions based on file extensions:

``` bash title="File type-specific instructions"
cycodmd **/* --cs-file-instructions @cs-instructions.md --md-file-instructions @md-instructions.md
```

## Performance Options

Options for controlling processing efficiency and parallel execution.

``` bash title="Control thread count"
cycodmd **/*.md --file-instructions "summarize" --threads 4
```

## Output Options

Control how and where the generated markdown is saved.

``` bash title="Save individual file outputs"
cycodmd **/*.cs --save-file-output "outputs/{fileBase}.md"
```

``` bash title="Save combined output"
cycodmd **/*.md --instructions "summarize" --save-output summary.md
```

``` bash title="Save chat history"
cycodmd **/*.md --file-instructions "explain" --save-chat-history chat.jsonl
```

## Alias Options

Save and reuse common option combinations for efficiency.

``` bash title="Save options as alias"
cycodmd --lines 3 --file-contains "IMPORTANT" --save-alias important
```

``` bash title="Use saved alias"
cycodmd **/*.md --important
```

## Option Combinations

CYCODMD options are designed to work together, allowing for powerful combinations:

``` bash title="Complex filtering and processing"
cycodmd **/*.cs \
    --file-contains "public class" \
    --line-contains "TODO" \
    --lines 3 \
    --line-numbers \
    --file-instructions "Explain the TODOs and suggest implementations"
```

``` bash title="Multi-file type processing"
cycodmd **/*.* \
    --md-file-instructions "Improve formatting" \
    --cs-file-instructions "Add code comments" \
    --js-file-instructions "Check for security issues" \
    --threads 8
```

## See Also

- [File Conversion Basics](../basics/file-conversion.md)
- [AI Processing](../basics/ai-processing.md)
- [File Options Reference](../../reference/cycodmd/options/file-options.md)
- [Line Options Reference](../../reference/cycodmd/options/line-options.md)
- [AI Options Reference](../../reference/cycodmd/options/ai-options.md)