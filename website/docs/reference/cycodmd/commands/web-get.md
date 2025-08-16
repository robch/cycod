---
hide:
- toc
icon: material/web-box
---

--8<-- "snippets/ai-generated.md"

# web get

Use `cycodmd web get` to retrieve content from web pages and convert it to markdown.

``` { .bash .cli-command title="Basic usage" }
cycodmd web get "https://example.com"
```

``` { .plaintext .cli-output }
# Example Domain

This domain is for use in illustrative examples in documents. You may use this domain in literature without prior coordination or asking for permission.

[More information...](https://www.iana.org/domains/example)
```

## Retrieving Web Content

``` { .bash .cli-command title="Get content from one URL" }
cycodmd web get "https://example.com" 
```

``` { .bash .cli-command title="Get content from multiple URLs" }
cycodmd web get "https://example.com" "https://example.org"
```

``` { .bash .cli-command title="Strip HTML tags from content" }
cycodmd web get "https://example.com" --strip
```

??? question "When should I use --strip?"

    Use `--strip` when you want clean markdown content without HTML formatting.
    
    By default, CYCODMD preserves some HTML formatting when converting web pages to markdown.
    
    The `--strip` option removes HTML tags and produces cleaner, plain markdown text.

## Browser Options

CYCODMD supports multiple browser engines for rendering web pages.

``` { .bash .cli-command title="Use Firefox" }
cycodmd web get "https://example.com" --firefox
```

``` { .bash .cli-command title="Use WebKit" }
cycodmd web get "https://example.com" --webkit
```

``` { .bash .cli-command title="Use Chromium (default)" }
cycodmd web get "https://example.com" --chromium
```

``` { .bash .cli-command title="Interactive mode (manual browser interaction)" }
cycodmd web get "https://example.com" --interactive
```

??? question "What is interactive mode?"

    Interactive mode opens the browser and lets you interact with the page before capturing content.
    
    This is useful for:
    - Pages that require login
    - Content that loads dynamically with JavaScript
    - Navigating to specific sections before capturing
    
    When you're ready to capture the content, press Enter in the console.

## AI Processing

Use AI to process web content with instructions.

``` { .bash .cli-command title="Apply AI instructions to each page" }
cycodmd web get "https://example.com" --page-instructions "Summarize this content"
```

``` { .bash .cli-command title="Apply site-specific instructions" }
cycodmd web get "https://docs.python.org/3/tutorial/" "https://developer.mozilla.org/en-US/docs/Web/JavaScript" \
  --python-page-instructions "Extract Python examples" \
  --mozilla-page-instructions "Extract JavaScript best practices"
```

``` { .bash .cli-command title="Apply instructions to combined output" }
cycodmd web get "https://example.com" "https://example.org" \
  --instructions "Compare the content from these two sites"
```

## Saving Output

Save processed content to files.

``` { .bash .cli-command title="Save combined output to a file" }
cycodmd web get "https://example.com" --save-output "example-content.md"
```

``` { .bash .cli-command title="Save each page to a separate file" }
cycodmd web get "https://example.com" "https://example.org" \
  --save-page-output "output/{urlHost}.md"
```

??? question "What template variables are available?"

    When using `--save-page-output`, you can use these template variables:
    
    | Variable | Description |
    |----------|-------------|
    | `{url}` | The full URL |
    | `{urlHost}` | The hostname from the URL |
    | `{urlPath}` | The path from the URL |
    | `{urlBase}` | The filename part of the URL |

## Creating Aliases

Save common options as reusable aliases.

``` { .bash .cli-command title="Save settings as an alias" }
cycodmd web get "https://example.com" --strip --save-alias get-example
```

``` { .bash .cli-command title="Use the saved alias" }
cycodmd --get-example
```

## Options Reference

### Browser Options

| Option | Description |
|--------|-------------|
| `--interactive` | Run in browser interactive mode |
| `--chromium` | Use Chromium browser (default) |
| `--firefox` | Use Firefox browser |
| `--webkit` | Use WebKit browser |
| `--strip` | Strip HTML tags from content |

### AI Processing Options

| Option | Description |
|--------|-------------|
| `--page-instructions "..."` | Apply instructions to each page |
| `--SITE-page-instructions "..."` | Apply site-specific instructions |
| `--instructions "..."` | Apply instructions to combined output |
| `--threads N` | Limit concurrent processing threads |

### Output Options

| Option | Description |
|--------|-------------|
| `--save-page-output [FILE]` | Save each page to templated file |
| `--save-output [FILE]` | Save combined output to file |
| `--save-alias ALIAS` | Save current options as an alias |
| `--save-chat-history [FILE]` | Save AI processing chat history |

## Example Workflows

``` { .bash .cli-command title="Create a concise cheat sheet from tutorials" }
cycodmd web get "https://learnxinyminutes.com/docs/yaml/" \
  --strip \
  --page-instructions "Extract code examples and explain them" \
  --instructions "Format as a concise cheat sheet" \
  --save-output "yaml-cheatsheet.md"
```

``` { .bash .cli-command title="Compare documentation from multiple sources" }
cycodmd web get "https://api.example.com/docs" "https://github.com/example/example/blob/main/README.md" \
  --strip \
  --instructions "Compare these docs and identify any inconsistencies" \
  --save-output "docs-comparison.md"
```

## Related Commands

- [`cycodmd web search`](web-search.md) - Search the web and convert results to markdown
- [`cycodmd run`](run.md) - Execute commands and convert output to markdown

## See Also

- [Web Features](../../cycodmd/basics/web-features.md)
- [API Integration](../../cycodmd/advanced/api-integration.md)