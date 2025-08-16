---
hide:
- toc
icon: material/search-web
---

--8<-- "snippets/ai-generated.md"

# web search

The `cycodmd web search` command allows you to search the web and convert results to markdown.

??? question "Why use web search?"

    Web search allows you to gather information directly from the internet in markdown format.
    
    It's useful for research, documentation, or providing AI tools with up-to-date information.
    
    You can process the search results with AI instructions to extract, summarize, or analyze content.

## Basic Usage

``` { .bash .cli-command title="Basic web search" }
cycodmd web search "search terms"
```

``` { .bash .cli-command title="Search with a specific engine" }
cycodmd web search "yaml tutorial" --google   # default
cycodmd web search "yaml tutorial" --bing
cycodmd web search "yaml tutorial" --duckduckgo
cycodmd web search "yaml tutorial" --yahoo
```

``` { .bash .cli-command title="Get content from search results" }
cycodmd web search "yaml tutorial" --get --max 3
```

``` { .bash .cli-command title="Strip HTML from content" }
cycodmd web search "yaml tutorial" --get --strip
```

## Filtering Results

``` { .bash .cli-command title="Exclude specific sites" }
cycodmd web search "yaml tutorial" --exclude "youtube\.com|reddit\.com"
```

``` { .bash .cli-command title="Search within a specific site" }
cycodmd web search "yaml site:learnxinyminutes.com" --get --strip
```

## AI Processing

``` { .bash .cli-command title="Process combined output" }
cycodmd web search "climate change" --get --strip --max 3 --instructions "Create a summary of key points"
```

``` { .bash .cli-command title="Process each page individually" }
cycodmd web search "javascript promises" --get --strip --page-instructions "Extract code examples"
```

``` { .bash .cli-command title="Apply site-specific instructions" }
cycodmd web search "async await" --get --strip \
  --mozilla-page-instructions "Extract MDN examples" \
  --github-page-instructions "Focus on GitHub code"
```

## Using Search APIs

??? question "Why use search APIs?"

    Search APIs provide more reliable results than browser automation.
    
    They have documented rate limits, don't require a browser installation, and return consistent result formats.
    
    For professional or high-volume use, APIs are recommended over browser-based search.

=== "Bing API"

    ``` { .bash .cli-command title="Search with Bing API" }
    # Set up environment variables first:
    # BING_SEARCH_V7_ENDPOINT=https://api.bing.microsoft.com/v7.0/search
    # BING_SEARCH_V7_KEY=your-api-key
    
    cycodmd web search "quantum computing" --bing-api --max 5 --get
    ```

=== "Google API"

    ``` { .bash .cli-command title="Search with Google API" }
    # Set up environment variables first:
    # GOOGLE_SEARCH_API_KEY=your-api-key
    # GOOGLE_SEARCH_ENGINE_ID=your-engine-id
    # GOOGLE_SEARCH_ENDPOINT=https://www.googleapis.com/customsearch/v1
    
    cycodmd web search "neural networks" --google-api --max 5
    ```

## Saving Output

``` { .bash .cli-command title="Save output to a file" }
cycodmd web search "docker tutorial" --save-output "docker-resources.md"
```

``` { .bash .cli-command title="Save each page separately" }
cycodmd web search "git commands" --get --save-page-output "{fileName}-content.md"
```

## Creating Aliases

``` { .bash .cli-command title="Create and use an alias" }
cycodmd web search "crypto news" --get --strip --max 5 --instructions "Summarize developments" --save-alias crypto-news

# Use it later
cycodmd --crypto-news
```

## Example Use Cases

??? example "Research Summary"

    ``` { .bash .cli-command title="Research and summarize a topic" }
    cycodmd web search "quantum computing advancements" --get --strip --max 5 \
      --instructions "Create a comprehensive summary with recent developments"
    ```

??? example "Technical Documentation"

    ``` { .bash .cli-command title="Create documentation from official sources" }
    cycodmd web search "javascript fetch api site:developer.mozilla.org" --get --strip \
      --instructions "Create a concise guide with examples"
    ```

??? example "Comparative Analysis"

    ``` { .bash .cli-command title="Compare different technologies" }
    cycodmd web search "react vs vue" --get --strip --max 10 \
      --instructions "Create a comparison table highlighting pros, cons, and use cases"
    ```

## Options Reference

### Search Engine Options

| Option | Description |
|--------|-------------|
| `--google` | Use Google search engine (default) |
| `--bing` | Use Bing search engine |
| `--duckduckgo` | Use DuckDuckGo search engine |
| `--yahoo` | Use Yahoo search engine |
| `--bing-api` | Use Bing Search API (requires API key) |
| `--google-api` | Use Google Custom Search API (requires API key) |

### Content Options

| Option | Description |
|--------|-------------|
| `--get` | Download content from search results |
| `--strip` | Strip HTML tags from downloaded content |
| `--exclude REGEX` | Exclude URLs matching the pattern |
| `--max NUMBER` | Maximum number of search results (default: 10) |

### Browser Options

| Option | Description |
|--------|-------------|
| `--interactive` | Run in browser interactive mode |
| `--chromium` | Use Chromium browser (default) |
| `--firefox` | Use Firefox browser |
| `--webkit` | Use WebKit browser |

### AI Processing Options

| Option | Description |
|--------|-------------|
| `--instructions "TEXT"` | Apply AI processing to final output |
| `--page-instructions "TEXT"` | Apply AI processing to each page |
| `--SITE-page-instructions "TEXT"` | Apply specific instructions to matching sites |

## Related Commands

- [web get](web-get.md): Retrieve specific web page content
- [run](run.md): Run commands and convert output to markdown

--8<-- "snippets/ai-generated.md"