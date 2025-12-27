# cycodgr search - Layer 5: CONTEXT EXPANSION

## Purpose

Layer 5 controls **how to expand around matches** - showing additional lines before and after matching lines for context.

## Command-Line Options

### `--lines-before-and-after <N>` / `--lines <N>`

**Purpose**: Show N lines before and after each matching line

**Examples**:
```bash
cycodgr --file-contains "ConPTY" --lines 5
cycodgr microsoft/terminal --cs-file-contains "async" --lines-before-and-after 10
```

**Behavior**:
- Both options are **aliases** - they set the same property
- Symmetric expansion: same number of lines before and after
- Stored in `SearchCommand.LinesBeforeAndAfter` (int, default: 5)
- Applied when displaying file content via `LineHelpers.FilterAndExpandContext()`

**Source Reference**: See [Layer 5 Proof](cycodgr-search-layer-5-proof.md)

---

## Key Characteristics

1. **Symmetric only**: Unlike cycodmd (which has `--lines-before` and `--lines-after`), cycodgr only supports symmetric expansion
2. **Applied during display**: Context expansion happens when formatting code results, not during search
3. **Default value**: 5 lines before and after

## Data Flow

```
command.LinesBeforeAndAfter (int, default: 5)
  ↓
ProcessFileGroupAsync(fileGroup, ..., contextLines, ...)
  ↓
LineHelpers.FilterAndExpandContext(
    content,
    includePatterns,
    contextLines,  // lines before
    contextLines,  // lines after  ← Same value (symmetric)
    includeLineNumbers,
    excludePatterns,
    codeBlockStart,
    highlightMatches)
  ↓
Displayed output with context
```

---

## Related Layers

- **Layer 3 (Content Filtering)**: Determines which lines match
- **Layer 5 (Context Expansion)**: Expands around those matching lines
- **Layer 6 (Display Control)**: Formats the expanded context

---

For detailed source code evidence, see: [**Layer 5 Proof Document**](cycodgr-search-layer-5-proof.md)
