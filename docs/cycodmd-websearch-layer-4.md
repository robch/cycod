# cycodmd WebSearchCommand - Layer 4: CONTENT REMOVAL

[🔙 Back to WebSearchCommand](cycodmd-websearch-catalog-README.md) | [🔍 View Proof](cycodmd-websearch-layer-4-proof.md)

## Purpose

Layer 4 implements **active content removal** - the ability to explicitly remove lines from display even if they would otherwise be included by earlier filtering layers.

## Implementation Status

❌ **Not Implemented**

WebSearchCommand does not implement Layer 4 (CONTENT REMOVAL). It has URL-level exclusion (`--exclude`) which belongs to Layer 1 (TARGET SELECTION) / Layer 2 (CONTAINER FILTER), but no line-level content removal within web pages.

## Command-Line Options

### No Layer 4 Options Available

WebSearchCommand does not provide command-line options for removing specific lines or patterns from web page content after retrieval.

## Rationale

WebSearchCommand focuses on:
1. **Layer 1**: Searching for web pages (URLs)
2. **Layer 2**: Filtering which URLs to fetch (via `--exclude`)
3. **Layer 3**: Retrieving page content (via `--get`)
4. **Layer 6**: Controlling display (via `--strip`, browser options)
5. **Layer 8**: AI processing (via `--page-instructions`)

Content removal (Layer 4) is not implemented because:
- Web pages are typically processed as complete units
- HTML stripping (`--strip`) serves a similar purpose (Layer 6)
- AI instructions can be used to filter/summarize content (Layer 8)
- Line-level filtering is more relevant for source code (FindFilesCommand)

## Workarounds

If you need to remove specific content from web pages, you can:

### 1. Use AI Instructions (Layer 8)
```bash
cycodmd web search "topic" --get --page-instructions "Summarize, excluding advertisements and navigation"
```

### 2. Use URL Exclusion (Layer 1/2)
```bash
cycodmd web search "topic" --exclude "ads|tracker|popup"
```

### 3. Pipe to FindFilesCommand
```bash
cycodmd web search "topic" --get --save-output pages.md
cycodmd pages.md --remove-all-lines "Advertisement|Sponsored"
```

## Comparison with FindFilesCommand

| Feature | FindFilesCommand | WebSearchCommand |
|---------|------------------|------------------|
| Layer 4 Option | `--remove-all-lines` | ❌ Not available |
| Purpose | Remove lines from source code | N/A |
| Regex Patterns | ✅ Multiple patterns | ❌ Not supported |
| Context Awareness | ✅ Excludes from context | N/A |

## Future Enhancement Possibility

Layer 4 could potentially be added to WebSearchCommand with options like:
- `--remove-page-lines <patterns>`: Remove lines from web page content
- `--remove-html-tags <tags>`: Remove specific HTML tags and their content
- `--remove-scripts`: Remove all `<script>` content
- `--remove-style`: Remove all `<style>` content

However, these are **not currently implemented**.

## Source Code Evidence

See [Layer 4 Proof](cycodmd-websearch-layer-4-proof.md) for detailed evidence showing:
- WebCommand class has no `RemoveAllLineContainsPatternList` property
- WebSearchCommand class has no content removal properties
- Command-line parser has no `--remove-*` options for WebSearchCommand
- No line-level filtering in web page processing code

## Related Layers

- [Layer 1: TARGET SELECTION](cycodmd-websearch-layer-1.md) - What to search
- [Layer 2: CONTAINER FILTER](cycodmd-websearch-layer-2.md) - Which URLs to fetch
- [Layer 3: CONTENT FILTER](cycodmd-websearch-layer-3.md) - What content to retrieve
- **Layer 4: CONTENT REMOVAL** ← You are here (Not Implemented)
- [Layer 5: CONTEXT EXPANSION](cycodmd-websearch-layer-5.md) - How to expand around matches
- [Layer 6: DISPLAY CONTROL](cycodmd-websearch-layer-6.md) - How to format output
- [Layer 7: OUTPUT PERSISTENCE](cycodmd-websearch-layer-7.md) - Where to save results
- [Layer 8: AI PROCESSING](cycodmd-websearch-layer-8.md) - AI-assisted analysis
- [Layer 9: ACTIONS ON RESULTS](cycodmd-websearch-layer-9.md) - Actions on results

---

[🔙 Back to WebSearchCommand](cycodmd-websearch-catalog-README.md) | [🔍 View Proof](cycodmd-websearch-layer-4-proof.md)
