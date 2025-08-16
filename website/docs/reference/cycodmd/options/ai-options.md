---
hide:
- toc
icon: material/robot
---

--8<-- "snippets/ai-generated.md"

# AI Options

CYCODMD provides AI processing capabilities to transform content into well-formatted markdown.

## --file-instructions

Apply specified instructions to each file individually.

``` { .bash .cli-command title="Apply instructions to each file" }
cycodmd [FILES] --file-instructions "INSTRUCTION1" ["INSTRUCTION2" [...]]
```

``` { .bash .cli-command title="Explain code files" }
cycodmd "**/*.cs" --file-instructions "Explain what this code does"
```

``` { .bash .cli-command title="Apply multi-step processing" }
cycodmd "**/*.json" --file-instructions "Convert to YAML" "Add comments to explain each field"
```

``` { .bash .cli-command title="Load instructions from files" }
cycodmd "**/*.cs" --file-instructions @instructions.md @step2.md
```

## --EXT-file-instructions

Apply specific instructions to files with matching extensions.

``` { .bash .cli-command title="Apply different instructions based on file type" }
cycodmd "**/*" --cs-file-instructions "Explain this C# code" --js-file-instructions "Explain this JavaScript code"
```

``` { .bash .cli-command title="Combine with multi-step instructions" }
cycodmd "**/*" --cs-file-instructions "Explain this code" "Point out best practices"
```

## --instructions

Apply instructions to the overall command output.

``` { .bash .cli-command title="Process combined output of all files" }
cycodmd [FILES] --instructions "INSTRUCTION1" ["INSTRUCTION2" [...]]
```

``` { .bash .cli-command title="Create a summary of all matched files" }
cycodmd "**/*.md" --instructions "Create a summary of these markdown files"
```

``` { .bash .cli-command title="Generate documentation from code files" }
cycodmd "**/*.cs" --instructions "Generate API documentation from these code files"
```

## --page-instructions

Apply instructions to each web page individually.

``` { .bash .cli-command title="Process web pages with custom instructions" }
cycodmd web get "URL" --page-instructions "INSTRUCTION"
```

``` { .bash .cli-command title="Extract specific information from web pages" }
cycodmd web get "https://example.com" --page-instructions "Extract all API endpoints"
```

``` { .bash .cli-command title="Process search results individually" }
cycodmd web search "Python tutorials" --get --page-instructions "Extract code examples"
```

## --SITE-page-instructions

Apply instructions to web pages from specific sites.

``` { .bash .cli-command title="Apply site-specific instructions" }
cycodmd web search "python list" --get --python-page-instructions "Extract code" --stackoverflow-page-instructions "Get answers"
```

## --threads

Control parallel processing with thread limits.

``` { .bash .cli-command title="Process files with limited concurrency" }
cycodmd "**/*.cs" --file-instructions "Explain this code" --threads 4
```

## --save-chat-history

Save the AI processing chat history to a file.

``` { .bash .cli-command title="Save processing history for later review" }
cycodmd "**/*.cs" --file-instructions "Refactor this code" --save-chat-history "refactoring-session.jsonl"
```

## AI Provider Configuration

??? question "How do I configure which AI provider to use?"

    CYCODMD uses the AI provider configured in CYCOD. You can set the provider using environment variables or CYCOD's configuration.
    
    Use the CYCOD configuration commands to set up your preferred AI provider.

``` { .bash .cli-command title="Configure AI provider via environment variables" }
export OPENAI_API_KEY="your-key-here"
export OPENAI_CHAT_MODEL_NAME="gpt-4o"
cycodmd "**/*.cs" --file-instructions "Explain this code"
```

``` { .bash .cli-command title="Configure AI provider via CYCOD" }
cycod config set OpenAI.ApiKey "your-key-here"
cycod config set OpenAI.ChatModelName "gpt-4o"
cycodmd "**/*.cs" --file-instructions "Explain this code"
```

## Best Practices

- **Be specific and clear** in your instructions
- **Use step-by-step instructions** for complex transformations
- **Provide examples** when the desired output format is important
- **Use multi-step instructions** for complex processing
- **Consider token limits** when processing large files

## Related Options

- [File Options](file-options.md) - For selecting and filtering files
- [Line Options](line-options.md) - For filtering and formatting specific lines
- [Output Options](output-options.md) - For saving and formatting the output