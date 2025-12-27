# cycodmd Web Get - Layer 5: Context Expansion

**[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#web-get)**

## Purpose

Layer 5 controls how content is expanded to show context around matches. This layer is responsible for showing additional lines before and after matching content.

## Implementation Status

⚠️ **NOT IMPLEMENTED** for Web Get command.

The Web Get command operates at the **page level**, not the line level. It:
1. Fetches specific web pages by URL
2. Optionally strips HTML (`--strip`)
3. Optionally processes pages with AI (`--page-instructions`)

There is **no line-level filtering or context expansion** in the Web Get command.

## Why Layer 5 Doesn't Apply

The Web Get command's data model is:
```
URLs → Web Pages (as whole documents)
```

There is no concept of:
- Line-level matching within pages
- Context lines before/after matches
- Line number display
- Gap separators

## Related Functionality

While Layer 5 (Context Expansion) is not implemented, related functionality exists at the **page level**:

### Page-Level Operations
- **`--strip`**: Strips HTML to get plain text content (provides cleaner "context" by removing markup)
- **`--save-page-folder <dir>`**: Saves pages to a folder for later processing
- **`--page-instructions`**: AI processes entire page (page-level "context")

### Future Possibilities

If line-level filtering were added to Web Get, Layer 5 functionality could include:
- `--lines N`: Show N lines of page content around matching terms
- `--lines-before N`, `--lines-after N`: Asymmetric expansion
- Integration with `--line-contains` (which doesn't currently exist for web commands)

## Comparison with File Search

| Feature | File Search | Web Get |
|---------|-------------|---------|
| Line-level filtering | ✅ `--line-contains` | ❌ Not implemented |
| Context expansion | ✅ `--lines`, `--lines-before`, `--lines-after` | ❌ Not implemented |
| Line numbers | ✅ `--line-numbers` | ❌ Not implemented |
| Content removal | ✅ `--remove-all-lines` | ❌ Not implemented |
| Page/File-level filtering | ✅ `--file-contains` | ❌ Not applicable (URLs are specified explicitly) |

## Alternative: Use File Search After Fetching

If you need line-level context expansion for web content:

1. **Fetch pages with Web Get**:
   ```bash
   cycodmd web get https://example.com/page1 https://example.com/page2 \
     --strip --save-page-folder pages/
   ```

2. **Process saved pages with File Search**:
   ```bash
   cycodmd pages/**/*.md --line-contains "pattern" --lines 3
   ```

This two-step approach gives you full Layer 5 context expansion capabilities.

## Difference from Web Search

While both **Web Search** and **Web Get** lack Layer 5 implementation, they differ in Layer 1 (Target Selection):

| Layer | Web Search | Web Get |
|-------|------------|---------|
| Layer 1 | Search terms → URLs (discovered) | URLs (explicit) |
| Layer 2 | URL filtering (`--exclude`) | No filtering (URLs are explicit) |
| Layer 5 | ❌ Not implemented | ❌ Not implemented |

Web Get is more direct: you specify exact URLs to fetch, while Web Search discovers URLs based on search terms.

---

**[→ See Proof Documentation](cycodmd-webget-layer-5-proof.md)** | **[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#web-get)**
