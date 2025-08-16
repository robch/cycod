---
hide:
- toc
icon: material/robot
---

--8<-- "snippets/ai-generated.md"

# AI Processing with CYCODMD

CYCODMD provides powerful AI processing capabilities that allow you to transform content into well-formatted markdown using natural language instructions. You can use AI processing with files, command outputs, and web content.

## Understanding AI Processing Options

CYCODMD offers several options for AI processing:

| Option | Description |
|--------|-------------|
| `--file-instructions` | Apply AI instructions to each individual file |
| `--EXT-file-instructions` | Apply AI instructions to files with specific extensions |
| `--instructions` | Apply AI instructions to the final combined output |

## Processing Individual Files

The `--file-instructions` option allows you to apply AI processing to each file independently before combining them.

``` bash title="Process each file separately"
cycodmd "**/*.md" --file-instructions "Summarize this markdown file in bullet points"
```

This applies the instruction to each markdown file separately before combining the results.

## Processing Files by Extension

You can target files with specific extensions using the `--EXT-file-instructions` option, where `EXT` is the file extension.

``` bash title="Process files by extension"
cycodmd "**/*.cs" "**/*.js" --cs-file-instructions "Explain this C# code" --js-file-instructions "Explain this JavaScript code"
```

This applies different instructions based on the file type:
- C# files are processed with "Explain this C# code"
- JavaScript files are processed with "Explain this JavaScript code"

## Processing Combined Output

The `--instructions` option applies AI processing to the final combined output from all files or commands.

``` bash title="Process combined output"
cycodmd "**/*.md" --instructions "Create a table of contents from these markdown files"
```

This first collects all matching markdown files, then applies the instruction to the combined content.

## Multi-Step Processing

You can chain multiple instructions for more complex transformations:

``` bash title="Apply multiple instructions in sequence"
cycodmd "**/*.cs" --file-instructions "Extract class names" "List public methods" "Identify dependencies"
```

This applies each instruction in sequence:
1. "Extract class names" processes the original content
2. "List public methods" processes the result from step 1
3. "Identify dependencies" processes the result from step 2

## Using Instructions from Files

For longer or reusable instructions, you can load them from files using the `@` prefix:

``` bash title="Load instructions from file"
cycodmd "**/*.js" --file-instructions @javascript-analysis.md
```

This loads instructions from the `javascript-analysis.md` file and applies them to each JavaScript file.

## Combining File and Output Instructions

You can use both `--file-instructions` and `--instructions` together for two-stage processing:

``` bash title="Two-stage processing"
cycodmd "**/*.cs" --file-instructions "Document this class" --instructions "Create a project overview from these class documentations"
```

This first processes each C# file with "Document this class", then processes the combined result with "Create a project overview".

## Example: Code Documentation

Let's create a documentation workflow for a codebase:

``` bash title="Generate code documentation"
cycodmd "src/**/*.cs" --file-instructions "Generate documentation for this C# file with:
- A brief class/file summary
- List of public methods with parameters and return types
- Examples for each method" --instructions "Organize these docs into a developer guide" --save-output "docs/developer-guide.md"
```

This processes each C# file individually with detailed documentation instructions, then organizes all the documentation into a cohesive developer guide.

## Example: Using with Web Content

You can combine AI processing with web content:

``` bash title="Process web search results"
cycodmd web search "Python list comprehension" --get --max 3 --page-instructions "Extract code examples" --instructions "Create a tutorial with all the examples"
```

This searches for "Python list comprehension", gets the top 3 results, extracts code examples from each page, and then combines them into a tutorial.

## Example: Technical Report Generation

Generate a technical report from code files:

``` bash title="Generate technical report"
cycodmd "**/*.py" --file-instructions @code-analysis.md --instructions "Create a technical report with:
- Executive summary
- Key findings
- Recommendations for improvement" --save-output "reports/technical-analysis.md"
```

Where `code-analysis.md` contains detailed instructions for analyzing Python code.

## Best Practices

1. **Start simple**: Begin with clear, concise instructions before attempting complex multi-step processing
2. **Be specific**: Provide detailed instructions about what you want the AI to focus on
3. **Use context**: Mention the type of content being processed in your instructions
4. **Test iteratively**: Try instructions on a small sample before processing large collections
5. **Consider token limits**: Very complex instructions or large files might exceed token limits

## Related Options

- `--save-chat-history`: Save the chat history from AI processing
- `--threads`: Control parallel processing when working with multiple files
- `--save-file-output`: Save each processed file to a separate output file
