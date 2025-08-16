---
hide:
- toc
icon: material/web
---

--8<-- "snippets/ai-generated.md"

# Web Search & Content

CYCODMD provides powerful capabilities for retrieving and processing web content through two main commands:

- `cycodmd web search`: Search the web and create markdown from results
- `cycodmd web get`: Retrieve content from specific URLs and convert to markdown

These features enable you to bring external web information directly into your markdown documents, with optional AI processing to summarize, format, or analyze the content.

## Web Search

The `web search` command lets you search the web using various search engines and create markdown from the results.

### Basic Search Syntax

``` bash title="Basic web search"
cycodmd web search "SEARCH TERMS"
```

### Search Engine Options

CYCODMD supports multiple search engines:

``` bash title="Default search engine (Google)"
cycodmd web search "yaml tutorial"
```

``` bash title="Specify Google search"
cycodmd web search "yaml tutorial" --google
```

``` bash title="Specify Bing search"
cycodmd web search "yaml tutorial" --bing
```

``` bash title="Specify DuckDuckGo search"
cycodmd web search "yaml tutorial" --duckduckgo
```

``` bash title="Specify Yahoo search"
cycodmd web search "yaml tutorial" --yahoo
```

### Retrieving Content

By default, web search only returns links. To retrieve the actual content from search results:

``` bash title="Get content from search results"
cycodmd web search "yaml tutorial" --get --max 3
```

### Filtering HTML

Web content often contains HTML tags. You can strip these out:

``` bash title="Strip HTML tags"
cycodmd web search "yaml tutorial" --get --strip
```

### Excluding Sites

You can exclude certain sites from your search results:

``` bash title="Exclude specific sites"
cycodmd web search "yaml tutorial" --exclude "youtube.com reddit.com"
```

### Advanced Site Search

Use site-specific search syntax:

``` bash title="Search specific site"
cycodmd web search "yaml site:learnxinyminutes.com" --get --strip
```

### API Integration

For more reliable searches, CYCODMD supports search APIs:

=== "Bing API"

    ``` bash title="Use Bing Search API"
    # Set up Bing API environment variables in .env file or shell:
    # BING_SEARCH_V7_ENDPOINT=https://api.bing.microsoft.com/v7.0/search
    # BING_SEARCH_V7_KEY=your-api-key
    
    cycodmd web search "yaml tutorial" --bing-api
    ```

=== "Google API"

    ``` bash title="Use Google Search API"
    # Set up Google API environment variables in .env file or shell:
    # GOOGLE_SEARCH_API_KEY=your-api-key
    # GOOGLE_SEARCH_ENGINE_ID=your-engine-id
    # GOOGLE_SEARCH_ENDPOINT=https://www.googleapis.com/customsearch/v1
    
    cycodmd web search "yaml tutorial" --google-api
    ```

## Web Content Retrieval

The `web get` command lets you retrieve content from specific URLs and convert it to markdown.

### Basic Syntax

``` bash title="Get web content"
cycodmd web get "URL"
```

### Multiple URLs

You can retrieve content from multiple URLs at once:

``` bash title="Get content from multiple URLs"
cycodmd web get "https://example.com" "https://another-site.com"
```

### Stripping HTML

Remove HTML tags from the content:

``` bash title="Strip HTML from content"
cycodmd web get "https://example.com" --strip
```

### Browser Options

CYCODMD supports different browser engines for content retrieval:

``` bash title="Using default (Chromium) browser"
cycodmd web get "https://example.com"
```

``` bash title="Using Firefox"
cycodmd web get "https://example.com" --firefox
```

``` bash title="Using WebKit"
cycodmd web get "https://example.com" --webkit
```

### Interactive Mode

For pages that require JavaScript execution or more complex rendering:

``` bash title="Use interactive mode"
cycodmd web get "https://dynamic-site.com" --interactive
```

## AI Processing of Web Content

One of the most powerful features of CYCODMD is the ability to apply AI processing to web content:

### Process Individual Pages

``` bash title="Apply instructions to each page"
cycodmd web search "yaml tutorial" --get --strip --page-instructions "Extract key concepts and examples"
```

``` bash title="Apply site-specific instructions"
cycodmd web search "programming" --get --strip --github-page-instructions "Focus on code examples"
```

### Process Final Output

``` bash title="Process combined output"
cycodmd web search "climate change news" --get --strip --max 5 --instructions "Create a summary of recent developments"
```

### Multi-step Instructions

For complex processing, you can chain multiple instruction steps:

``` bash title="Apply multi-step instructions"
cycodmd web search "yaml tutorial" --get --strip --page-instructions @step1.txt @step2.txt
```

## Saving Output

Save processed web content to files:

``` bash title="Save final output"
cycodmd web search "yaml tutorial" --get --strip --save-output "yaml-guide.md"
```

``` bash title="Save each page separately"
cycodmd web search "programming languages" --get --strip --save-page-output "pages/{fileBase}.md"
```

## Creating Aliases

Create aliases for commonly used web search or retrieval patterns:

``` bash title="Create an alias"
cycodmd web search "yaml tutorial" --get --strip --max 3 --save-alias yaml-search
```

``` bash title="Use the alias"
cycodmd --yaml-search
```

## Common Examples

### Research Summary

``` bash title="Create research summary"
cycodmd web search "quantum computing advancements" --get --strip --max 5 --instructions "Create a comprehensive summary with recent developments, key technologies, and future prospects"
```

### Technical Documentation

``` bash title="Create technical documentation"
cycodmd web search "javascript fetch api site:developer.mozilla.org" --get --strip --instructions "Create a concise guide with examples"
```

### Comparative Analysis

``` bash title="Create comparative analysis"
cycodmd web search "react vs vue" --get --strip --max 10 --instructions "Create a comparison table highlighting pros, cons, and use cases for each framework"
```
