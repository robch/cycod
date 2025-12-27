# cycodmd Web Search - Layer 5: Context Expansion

**[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#web-search)**

## Purpose

Layer 5 controls how content is expanded to show context around matches. This layer is responsible for showing additional lines before and after matching content.

## Implementation Status

⚠️ **NOT IMPLEMENTED** for Web Search command.

The Web Search command operates at the **page level**, not the line level. It:
1. Searches for web pages matching search terms
2. Optionally fetches full page content (`--get`)
3. Optionally strips HTML (`--strip`)
4. Optionally processes pages with AI (`--page-instructions`)

There is **no line-level filtering or context expansion** in the Web Search command.

## Why Layer 5 Doesn't Apply

The Web Search command's data model is:
```
Search Terms → URLs → Web Pages (as whole documents)
```

There is no concept of:
- Line-level matching within pages
- Context lines before/after matches
- Line number display
- Gap separators

## Related Functionality

While Layer 5 (Context Expansion) is not implemented, related functionality exists at the **page level**:

### Page-Level Context
- **`--max N`**: Limits the number of result pages (similar to limiting results, but at page level)
- **`--get`**: Fetches full page content (provides "context" by getting the entire page)
- **`--exclude <pattern>`**: Filters out URLs containing pattern (page-level filtering)

### Future Possibilities

If line-level filtering were added to Web Search, Layer 5 functionality could include:
- `--lines N`: Show N lines of page content around matching terms
- `--lines-before N`, `--lines-after N`: Asymmetric expansion
- Integration with `--line-contains` (which doesn't currently exist for web commands)

## Comparison with File Search

| Feature | File Search | Web Search |
|---------|-------------|------------|
| Line-level filtering | ✅ `--line-contains` | ❌ Not implemented |
| Context expansion | ✅ `--lines`, `--lines-before`, `--lines-after` | ❌ Not implemented |
| Line numbers | ✅ `--line-numbers` | ❌ Not implemented |
| Content removal | ✅ `--remove-all-lines` | ❌ Not implemented |
| Page/File-level filtering | ✅ `--file-contains` | ⚠️ Limited (URL filtering only) |

## Alternative: Use File Search After Fetching

If you need line-level context expansion for web content:

1. **Fetch pages with Web Search**:
   ```bash
   cycodmd web search "search terms" --get --strip --save-page-folder pages/
   ```

2. **Process saved pages with File Search**:
   ```bash
   cycodmd pages/**/*.md --line-contains "pattern" --lines 3
   ```

This two-step approach gives you full Layer 5 context expansion capabilities.

---

**[→ See Proof Documentation](cycodmd-websearch-layer-5-proof.md)** | **[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#web-search)**
