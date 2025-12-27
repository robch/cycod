# cycodmd WebGetCommand - Layer 4: CONTENT REMOVAL

[🔙 Back to WebGetCommand](cycodmd-webget-catalog-README.md) | [🔍 View Proof](cycodmd-webget-layer-4-proof.md)

## Purpose

Layer 4 implements **active content removal** - the ability to explicitly remove lines from display even if they would otherwise be included by earlier filtering layers.

## Implementation Status

❌ **Not Implemented**

WebGetCommand does not implement Layer 4 (CONTENT REMOVAL). It inherits from WebCommand and has the same limitations as WebSearchCommand - no line-level content removal within web pages.

## Command-Line Options

### No Layer 4 Options Available

WebGetCommand does not provide command-line options for removing specific lines or patterns from web page content after retrieval.

## Rationale

WebGetCommand focuses on:
1. **Layer 1**: Fetching specific URLs
2. **Layer 6**: Controlling display (via `--strip`, browser options)
3. **Layer 8**: AI processing (via `--page-instructions`)

Content removal (Layer 4) is not implemented for the same reasons as WebSearchCommand:
- Web pages are processed as complete units
- HTML stripping serves a similar purpose (Layer 6)
- AI instructions can filter/summarize content (Layer 8)
- Line-level filtering is more relevant for source code

## Workarounds

If you need to remove specific content from web pages, you can:

### 1. Use AI Instructions (Layer 8)
```bash
cycodmd web get https://example.com --page-instructions "Summarize, excluding ads and navigation"
```

### 2. Pipe to FindFilesCommand
```bash
cycodmd web get https://example.com --save-output page.md
cycodmd page.md --remove-all-lines "Advertisement|Sponsored"
```

## Comparison with FindFilesCommand

| Feature | FindFilesCommand | WebGetCommand |
|---------|------------------|---------------|
| Layer 4 Option | `--remove-all-lines` | ❌ Not available |
| Purpose | Remove lines from source code | N/A |
| Regex Patterns | ✅ Multiple patterns | ❌ Not supported |
| Context Awareness | ✅ Excludes from context | N/A |

## Future Enhancement Possibility

Layer 4 could potentially be added to WebGetCommand with the same options as proposed for WebSearchCommand, but these are **not currently implemented**.

## Source Code Evidence

See [Layer 4 Proof](cycodmd-webget-layer-4-proof.md) for detailed evidence showing:
- WebGetCommand inherits from WebCommand with no Layer 4 properties
- No content removal properties or methods
- No `--remove-*` options in command-line parser
- No line-level filtering in web page processing code

## Related Layers

- [Layer 1: TARGET SELECTION](cycodmd-webget-layer-1.md) - What URLs to fetch
- [Layer 2: CONTAINER FILTER](cycodmd-webget-layer-2.md) - URL validation
- [Layer 3: CONTENT FILTER](cycodmd-webget-layer-3.md) - What content to retrieve
- **Layer 4: CONTENT REMOVAL** ← You are here (Not Implemented)
- [Layer 5: CONTEXT EXPANSION](cycodmd-webget-layer-5.md) - Not applicable
- [Layer 6: DISPLAY CONTROL](cycodmd-webget-layer-6.md) - How to format output
- [Layer 7: OUTPUT PERSISTENCE](cycodmd-webget-layer-7.md) - Where to save results
- [Layer 8: AI PROCESSING](cycodmd-webget-layer-8.md) - AI-assisted analysis
- [Layer 9: ACTIONS ON RESULTS](cycodmd-webget-layer-9.md) - Not applicable

---

[🔙 Back to WebGetCommand](cycodmd-webget-catalog-README.md) | [🔍 View Proof](cycodmd-webget-layer-4-proof.md)
