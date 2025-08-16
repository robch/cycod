---
hide:
- toc
icon: material/speedometer
---

--8<-- "snippets/ai-generated.md"

# Performance Optimization for CYCODMD

This guide covers techniques for optimizing CYCODMD's performance, particularly when dealing with large amounts of content or complex operations.

## Multi-Threading with `--threads`

CYCODMD supports parallel processing through the `--threads` option, which can significantly improve performance when processing multiple files or web pages.

``` bash title="Process files with multiple threads"
cycodmd "**/*.md" --threads 8 --file-instructions "summarize this content"
```

``` bash title="Process web search results with threads"
cycodmd web search "performance optimization" --get --threads 4
```

### Thread Count Best Practices

- **Default**: When not specified, CYCODMD uses the number of available CPU cores
- **CPU-bound tasks** (complex AI instructions): Use `CPU cores - 1` to leave resources for the OS
- **IO-bound tasks** (web searches, file reading): Can use higher thread counts (1.5-2x CPU cores)
- **Memory constraints**: Reduce thread count if you experience high memory usage

## Efficient File Filtering

Using appropriate filtering options can dramatically improve performance by reducing the amount of content that needs to be processed.

### Pattern Filtering

``` bash title="Use specific file patterns"
cycodmd "src/**/*.cs" --exclude "**/obj/**" "**/bin/**"
```

``` bash title="Filter by file content"
cycodmd "**/*.md" --file-contains "# Performance" --file-instructions "extract optimization tips"
```

### Line Filtering

``` bash title="Filter specific lines"
cycodmd "**/*.log" --line-contains "ERROR" --lines-after 5
```

``` bash title="Remove unnecessary lines"
cycodmd "**/*.md" --remove-all-lines "^\s*```" --file-instructions "summarize content"
```

## Optimizing AI Processing

AI processing is the most resource-intensive operation in CYCODMD. Optimize it by:

### Chunking Large Files

When dealing with large files, process them in manageable chunks:

``` bash title="Process large files in chunks"
cycodmd large-file.md --lines 200 --file-instructions "summarize this section"
```

``` bash title="Save chunked output"
cycodmd "**/*.md" --lines 300 --file-instructions "summarize" --save-file-output "summaries/{fileBase}-summary.md"
```

### Reducing AI Context Size

``` bash title="Remove unnecessary content"
cycodmd "**/*.js" --remove-all-lines "^\s*//" --file-instructions "explain this code"
```

``` bash title="Focus on specific sections"
cycodmd "**/*.py" --line-contains "class" --lines-after 20 --file-instructions "explain this class"
```

## Web Processing Optimization

When working with web content, optimize performance with these techniques:

### Search Optimization

``` bash title="Limit search results"
cycodmd web search "performance tips" --max 3 --get
```

``` bash title="Exclude unnecessary sites"
cycodmd web search "coding patterns" --exclude "youtube.com" "pinterest.com" --get
```

### Content Processing

``` bash title="Strip HTML to reduce load"
cycodmd web get "https://example.com/article" --strip
```

``` bash title="Try different browser engines"
cycodmd web get "https://example.com" --firefox  # Try different browser engines if one is slow
```

## Output Optimization

Optimize how output is saved to improve overall performance:

``` bash title="Process files separately"
cycodmd "**/*.md" --file-instructions "format as technical documentation" --save-file-output "docs/{fileBase}.md"
```

``` bash title="Combine processed results"
cycodmd processed-files/*.md --instructions "create a combined summary"
```

## Memory Management

CYCODMD can be memory-intensive when processing large files or multiple web pages. Manage memory by:

- **Batching operations**: Process files in smaller batches rather than all at once
- **Lowering thread count**: Reduce parallel processing when memory is limited
- **Using stricter filters**: Narrow down content before processing
- **Saving intermediate results**: For very large operations, save partial results

## Real-World Performance Examples

### Processing Large Codebases

``` bash title="Efficient code documentation"
cycodmd "src/**/*.cs" --exclude "tests/**" "**/obj/**" --file-contains "public class" \
  --threads 8 --file-instructions "generate documentation" \
  --save-file-output "docs/{filePath}/{fileName}.md"
```

### Web Research Optimization

``` bash title="Efficient web research"
cycodmd web search "microservices architecture patterns" --max 10 --get --strip \
  --threads 6 --page-instructions "extract key concepts" \
  --save-page-output "research/{pageTitle}.md" \
  --instructions "combine all findings into a cohesive summary"
```

### Batch Processing Script

For very large operations, consider using a script to process in batches:

``` bash title="Process in batches"
# Process files in batches of 50
find src -name "*.cs" | split -l 50 - batch_
for batch in batch_*; do
  cycodmd @$batch --file-instructions "document this code" \
    --threads 4 --save-file-output "docs/{filePath}.md"
done
```

## Monitoring Performance

To identify performance bottlenecks:

1. Use `--debug` flag to see processing time for each operation
2. Monitor system resource usage during execution
3. Test with different thread counts to find optimal settings
4. Compare performance with different filtering strategies

## Conclusion

Optimizing CYCODMD performance involves balancing filtering techniques, thread count, and AI processing strategies based on your specific use case. Start with the techniques in this guide and adjust based on your specific environment and requirements.

For extremely large operations, consider breaking the work into smaller, more manageable chunks and processing them separately.