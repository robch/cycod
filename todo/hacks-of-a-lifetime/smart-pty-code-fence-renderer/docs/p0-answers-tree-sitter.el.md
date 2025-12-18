# P0 Questions: karlotness/tree-sitter.el

**Date:** 2025-01-XX  
**Repo:** https://github.com/karlotness/tree-sitter.el  
**Status:** 10th confirmation of P0 questions

---

## Quick Summary

**All 5 P0 questions:** Same fundamental answers as previous 9 repos

**New insights:**
- TSInput callback pattern (alternative to parse_string)
- Incremental parsing with ts_tree_edit
- Editor integration hooks (before/after change)

**For highlighting:** Use ltreesitter's decoration table algorithm (no changes)

---

## Q1: How to initialize parser? ✅

**10th confirmation - Same pattern**

### C Implementation (Emacs Module)

```c
// src/parser.c
static emacs_value tsel_parser_new(emacs_env *env, ...) {
  TSElParser *wrapper = malloc(sizeof(TSElParser));
  TSParser *parser = ts_parser_new();  // Tree-sitter C API
  
  wrapper->parser = parser;
  wrapper->lang = NULL;
  
  // Return wrapped parser (auto GC via finalizer)
  return create_emacs_wrapper(env, wrapper);
}

static emacs_value tsel_parser_set_language(emacs_env *env, ...) {
  TSElParser *parser;
  TSElLanguage *lang;
  
  // Extract arguments...
  ts_parser_set_language(parser->parser, lang->ptr);  // Tree-sitter C API
  parser->lang = lang;
  
  return tsel_Qnil;
}
```

### Exposed to Emacs Lisp

```elisp
(setq parser (tree-sitter-parser-new))
(setq lang (tree-sitter-lang-python))
(tree-sitter-parser-set-language parser lang)
```

### For Our C++ Project

```cpp
extern "C" TSLanguage *tree_sitter_cpp();

TSParser *parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_cpp());

// ... use parser ...

ts_parser_delete(parser);
```

**Status:** Confirmed 10 times. No changes.

---

## Q2: How to parse code? ✅

**10th confirmation - NEW: TSInput callback pattern**

### Approach A: parse_string (Simple)

```c
// What we've seen 9 times
const char *source = "int main() { return 0; }";
TSTree *tree = ts_parser_parse_string(parser, NULL, source, strlen(source));

TSNode root = ts_tree_root_node(tree);
// ... use tree ...

ts_tree_delete(tree);
```

### Approach B: TSInput callback (NEW - Flexible)

```c
// src/parser.c - Custom read callback
struct tsel_parser_buffer_payload {
  emacs_env *env;
  emacs_value buffer;
};

static const char *tsel_parser_read_buffer_function(
    void *payload,
    uint32_t byte_index,
    TSPoint position,
    uint32_t *bytes_read) {
  
  struct tsel_parser_buffer_payload *buf_payload = payload;
  
  // Read chunk from Emacs buffer starting at byte_index
  emacs_value str = read_from_buffer(buf_payload->buffer, byte_index);
  
  // Copy to static buffer
  copy_string(str, tsel_parser_char_buffer, &size);
  
  *bytes_read = size - 1;
  return tsel_parser_char_buffer;
}

// Setup TSInput
struct tsel_parser_buffer_payload payload = {
  .env = env,
  .buffer = buffer
};

TSInput input_def = {
  .payload = &payload,
  .encoding = TSInputEncodingUTF8,
  .read = &tsel_parser_read_buffer_function
};

// Parse using callback
TSTree *new_tree = ts_parser_parse(parser->parser,
                                   old_tree,  // For incremental
                                   input_def);
```

### TSInput Structure

```c
typedef struct {
  void *payload;            // User data (anything)
  TSInputEncoding encoding; // UTF-8 or UTF-16
  const char *(*read)(void *payload,
                      uint32_t byte_index,
                      TSPoint position,
                      uint32_t *bytes_read);
} TSInput;
```

### When to Use Each

| Method | Use Case | Pros | Cons |
|--------|----------|------|------|
| **parse_string** | In-memory strings | Simple, direct | Need full source |
| **parse + TSInput** | Files, streams, sockets | Flexible, lazy | More complex |

### For Our Project

**MVP:** Use `parse_string` (simpler, sufficient)

**Future:** TSInput could enable streaming parse from PTY

**Status:** Confirmed 10 times. New insight: TSInput pattern.

---

## Q3: How to walk syntax tree? ✅

**10th confirmation - Manual traversal (vs. queries)**

### This Repo: Manual Traversal

```elisp
;; lisp/tree-sitter-live-preview.el
(defun tree-sitter-live-preview--node (node parent-markers)
  (let* ((name (tree-sitter-node-type node))
         (next-sibling (tree-sitter-node-next-sibling node))
         (next-child (tree-sitter-node-child node 0)))
    ;; Display this node
    (insert (format "%s %s\n" name (node-text node)))
    
    ;; Recurse to children
    (when next-child
      (tree-sitter-live-preview--node next-child markers))
    
    ;; Recurse to siblings
    (when next-sibling
      (tree-sitter-live-preview--node next-sibling markers))))
```

### C Equivalent

```c
void walk_node(TSNode node) {
  const char *type = ts_node_type(node);
  uint32_t child_count = ts_node_child_count(node);
  
  // Process this node
  printf("%s\n", type);
  
  // Recurse to children
  for (uint32_t i = 0; i < child_count; i++) {
    TSNode child = ts_node_child(node, i);
    walk_node(child);
  }
}

// Start from root
TSNode root = ts_tree_root_node(tree);
walk_node(root);
```

### Comparison: Manual vs. Queries

**After studying 10 repos:**

| Approach | Repos Using | Lines | Best For |
|----------|-------------|-------|----------|
| **Queries** | 7 repos | 10-20 | Highlighting, patterns |
| **Manual** | 2 repos | 100+ | Custom analysis, debugging |

**Repos using queries:**
- Repos 1, 2, 3, 4, 5, 7, 9

**Repos using manual:**
- Repo 8 (GTKCssLanguageServer - LSP)
- Repo 10 (tree-sitter.el - visualization)

### For Our Project: Use Queries

```c
// Query-based approach (simpler!)
const char *query_str = "(function_definition) @function";
TSQuery *query = ts_query_new(lang, query_str, strlen(query_str), ...);

TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
  for (uint16_t i = 0; i < match.capture_count; i++) {
    TSNode node = match.captures[i].node;
    // Process captured node
  }
}

ts_query_cursor_delete(cursor);
```

**Status:** Confirmed 10 times (8 query-based, 2 manual). Queries win.

---

## Q4: How to map node types → colors? ⚠️

**N/A for this repo (no highlighting)**

### This Repo: No Highlighting

tree-sitter.el provides:
- Tree parsing
- Tree walking
- Tree visualization

But NOT:
- ❌ Syntax highlighting
- ❌ Color mapping
- ❌ ANSI output

### For Our Project: Use ltreesitter Pattern

**From Repo 5 (ltreesitter):**

```
1. Query captures syntax patterns:
   (string_literal) @string
   (keyword) @keyword

2. Theme maps captures to colors:
   theme["string"] = "31"   // ANSI red
   theme["keyword"] = "35"  // ANSI magenta

3. Build decoration table:
   for each capture:
     color = theme[capture_name]
     for byte in capture.range:
       decoration[byte] = color

4. Output colored text:
   for byte in source:
     if decoration[byte] != prev_color:
       emit "\x1b[" + color + "m"
```

**Status:** Confirmed 5 times (highlighting repos only). No changes.

---

## Q5: How to output ANSI codes? ⚠️

**N/A for this repo (Emacs uses text properties)**

### This Repo: Emacs Text Properties

Emacs doesn't use ANSI codes. It uses text properties:

```elisp
(put-text-property start end 'face 'font-lock-keyword-face)
```

### For Our Project: ANSI Codes

**From Repo 5 (ltreesitter):**

```c
// ANSI color codes
#define ANSI_RESET   "\x1b[0m"
#define ANSI_RED     "\x1b[31m"
#define ANSI_GREEN   "\x1b[32m"
#define ANSI_YELLOW  "\x1b[33m"
#define ANSI_BLUE    "\x1b[34m"
#define ANSI_MAGENTA "\x1b[35m"
#define ANSI_CYAN    "\x1b[36m"

// Decoration table algorithm (Phase 1)
std::unordered_map<uint32_t, std::string> decoration;

for each capture:
  color = theme[capture_name];
  for (uint32_t byte = start; byte < end; byte++) {
    decoration[byte] = color;
  }

// Output with colors (Phase 2)
std::string prev_color;
for (uint32_t i = 0; i < source.length(); i++) {
  std::string curr_color = decoration[i];
  
  if (curr_color != prev_color) {
    // Emit pending text
    // Emit color change
    if (!curr_color.empty()) {
      std::cout << "\x1b[" << curr_color << "m";
    } else {
      std::cout << "\x1b[0m";
    }
    prev_color = curr_color;
  }
}
```

**Status:** Confirmed 5 times (terminal highlighters only). No changes.

---

## NEW: Bonus Insights from This Repo

### Incremental Parsing Pattern ⭐⭐⭐⭐⭐

**How to integrate with editor change tracking:**

```elisp
;; Before change: capture old positions
(defun tree-sitter-live--before-change (beg end)
  (let ((start-byte (position-bytes beg))
        (old-end-byte (position-bytes end))
        (start-point (tree-sitter-position-to-point beg))
        (old-end-point (tree-sitter-position-to-point end)))
    (aset before-change-data 0 start-byte)
    (aset before-change-data 1 old-end-byte)
    (aset before-change-data 2 start-point)
    (aset before-change-data 3 old-end-point)))

;; After change: compute new positions, edit tree
(defun tree-sitter-live--after-change (beg end pre-len)
  (let ((start-byte (aref before-change-data 0))
        (old-end-byte (aref before-change-data 1))
        (new-end-byte (position-bytes end))
        (start-point (aref before-change-data 2))
        (old-end-point (aref before-change-data 3))
        (new-end-point (tree-sitter-position-to-point end)))
    
    ;; Tell tree about edit
    (tree-sitter-tree-edit tree
                           start-byte old-end-byte new-end-byte
                           start-point old-end-point new-end-point)
    
    ;; Schedule re-parse
    (push (current-buffer) pending-buffers)))

;; Re-parse on idle
(defun tree-sitter-live--idle-update ()
  (dolist (buf pending-buffers)
    (with-current-buffer buf
      (setq tree (tree-sitter-parser-parse-buffer parser buf tree)))))
```

**C equivalent:**

```c
// After edit at byte range [start, old_end) → [start, new_end)
ts_tree_edit(tree,
             start_byte, old_end_byte, new_end_byte,
             start_point, old_end_point, new_end_point);

// Re-parse (tree-sitter reuses unchanged parts!)
TSTree *new_tree = ts_parser_parse_string(parser, tree, new_source, length);
```

**Why this matters:**
- Enables streaming/incremental parsing
- Could parse PTY output line-by-line as it arrives
- Huge performance win for large files

**For our project:**
- MVP: Buffer complete fences, parse once
- Future: Use ts_tree_edit for streaming

---

## Summary: All Questions Answered ✅

| Question | Status | Confirmations |
|----------|--------|---------------|
| Q1: Parser init | ✅ Complete | 10 repos |
| Q2: Parse code | ✅ Complete (+ TSInput) | 10 repos |
| Q3: Walk tree | ✅ Complete (queries win) | 10 repos |
| Q4: Node → colors | ✅ Complete (5 highlighting repos) | 5 repos |
| Q5: ANSI output | ✅ Complete (5 highlighting repos) | 5 repos |

**Bonus from Repo 10:**
- ✅ TSInput callback pattern
- ✅ Incremental parsing with ts_tree_edit
- ✅ Editor integration hooks

**All knowledge needed:** ✅  
**Any unknowns remaining:** ❌  
**Ready to build:** ✅ **YES!**

---

## What to Do Next

**🚀 BUILD THE PROTOTYPE 🚀**

**Reference files:**
1. `external/ltreesitter/examples/c-highlight.lua` - THE algorithm
2. `external/knut/3rdparty/CMakeLists.txt` - THE build pattern
3. `docs/study-ltreesitter.md` - Translation guide
4. `docs/study-knut.md` - C++ patterns

**Time to build:** 2-3 hours

**Stop studying. Start coding. NOW.**

---

**End of P0 answers for karlotness/tree-sitter.el**
