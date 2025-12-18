# Tree-sitter Fingerprints - The Real Signatures

**Date:** 2025-12-15  
**Source:** Cloned tree-sitter repo and examined api.h

## Key Include Path

```c
#include <tree_sitter/api.h>
```

**Search fingerprint:** Files that `#include <tree_sitter/api.h>` are definitely using Tree-sitter!

## Core Types

```c
TSParser    // The parser object
TSTree      // Parse result (syntax tree)
TSNode      // Node in the tree
TSLanguage  // Language grammar
```

**Search fingerprint:** Code using `TSParser`, `TSTree`, `TSNode` types

## Essential Functions

### Parser Lifecycle
```c
TSParser *ts_parser_new(void);
void ts_parser_delete(TSParser *self);
bool ts_parser_set_language(TSParser *self, const TSLanguage *language);
```

### Parsing
```c
TSTree *ts_parser_parse_string(
  TSParser *self,
  const TSTree *old_tree,
  const char *string,
  uint32_t length
);
```

### Tree Walking
```c
TSNode ts_tree_root_node(const TSTree *self);
uint32_t ts_node_child_count(TSNode self);
TSNode ts_node_child(TSNode self, uint32_t child_index);
const char *ts_node_type(TSNode self);
```

## Best Search Fingerprints

### Level 1: Include Statement (Most Specific)
```bash
--file-contains "#include <tree_sitter/api.h>"
```
OR
```bash
--file-contains "tree_sitter/api.h"
```

### Level 2: Core Functions (Very Specific)
```bash
--file-contains "ts_parser_new"
--file-contains "ts_tree_root_node"
```

### Level 3: Type Usage (Moderately Specific)
```bash
--file-contains "TSParser"
--file-contains "TSNode"
```

### Level 4: Language Grammars (For finding implementations)
```bash
--file-contains "tree_sitter_javascript"
--file-contains "tree_sitter_python"
```

## NEW Search Strategy

### Query 1: Find C/C++ projects actually using Tree-sitter
```bash
cycodgr --file-contains "tree_sitter/api.h" \
        --max-results 30 \
        --save-repos treesitter-users.txt
```

### Query 2: Find parse + highlight code
```bash
cycodgr --file-contains "ts_parser_new" \
        --file-contains "ts_tree_root_node" \
        --file-contains "color" \
        --max-results 20
```

### Query 3: Find language grammar usage
```bash
cycodgr --file-contains "tree_sitter_" \
        --line-contains "ts_parser_set_language" \
        --max-results 20
```

These are MUCH more specific than our previous "syntax" and "ansi" searches!

---

**Next:** Try these new fingerprint-based searches and see what we find! 🔍
