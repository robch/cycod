# cycodgr search - Layer 4: CONTENT REMOVAL

## Purpose

Layer 4 defines **what content to actively remove from display** after filtering. This layer is currently **not implemented** in cycodgr - there are no options to remove specific lines or content from results.

## Current State

cycodgr does **not have Layer 4 functionality**. Unlike cycodmd which has `--remove-all-lines`, cycodgr does not provide a mechanism to explicitly remove lines matching certain patterns.

## Missing Features

Potential Layer 4 features that could be added:

1. **`--remove-lines <pattern>`**: Remove lines matching regex pattern
2. **`--hide-comments`**: Remove comment lines from code
3. **`--hide-whitespace`**: Remove blank lines
4. **`--exclude-line-contains <pattern>`**: Negative line filtering

## Implementation Notes

In `ProcessFileGroupAsync()`, the `excludePatterns` parameter is always an empty list:

**File**: `src/cycodgr/Program.cs`, Line 802:
```csharp
var excludePatterns = new List<System.Text.RegularExpressions.Regex>();
```

This parameter is passed to `LineHelpers.FilterAndExpandContext()` but never populated with actual patterns.

---

## Related Layers

- **Layer 3 (Content Filtering)**: Uses `includePatterns` to show specific lines
- **Layer 4 (Content Removal)**: Would use `excludePatterns` to hide specific lines (NOT IMPLEMENTED)
- **Layer 2 (Container Filtering)**: Uses `--exclude` to filter repositories/files, not lines

---

For source code evidence, see: [**Layer 4 Proof Document**](cycodgr-search-layer-4-proof.md)
