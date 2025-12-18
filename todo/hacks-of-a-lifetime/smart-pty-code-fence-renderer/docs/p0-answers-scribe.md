# P0 Questions: abhirag/scribe

**Repository:** https://github.com/abhirag/scribe  
**Study Date:** 2025-01-XX (Session 11)  
**Question Set:** 5 P0 questions from study plan  
**Confirmation:** 11th time answering all 5 questions

---

## Question 1: How to initialize a tree-sitter parser?

### Answer ✅ (11th Confirmation)

**Standard pattern (confirmed 11 times across all repos):**

```c
TSParser* parser = ts_parser_new();
bool success = ts_parser_set_language(parser, tree_sitter_c());
if (!success) {
    // Handle error - language version mismatch
    ts_parser_delete(parser);
    return NULL;
}
```

### Scribe Implementation

**File:** `src/tree_sitter.c`, lines 12-26

```c
TSParser* create_parser(TSLanguage* lang) {
  START_ZONE;
  TSParser* parser = ts_parser_new();
  bool rc = ts_parser_set_language(parser, lang);
  if (!rc) {
    message_fatal("tree_sitter::create_parser failed in setting language");
    goto error_end;
  }
  END_ZONE;
  return parser;
error_end:
  ts_parser_delete(parser);
  END_ZONE;
  return (void*)0;
}
```

**Usage:**

```c
TSParser* parser = create_parser(tree_sitter_c());
if (!parser) {
    // Handle error
}
// Use parser...
ts_parser_delete(parser);
```

### Language Function Declaration

**File:** `src/c_queries.c`, line 12

```c
TSLanguage* tree_sitter_c();
```

**File:** `src/parsers/c_parser.c`, line 75605 (generated grammar)

```c
extern const TSLanguage *tree_sitter_c(void) {
  static const TSLanguage language = {
    .version = LANGUAGE_VERSION,
    .symbol_count = SYMBOL_COUNT,
    // ... massive generated structure ...
  };
  return &language;
}
```

### Build Configuration

**File:** `meson.build`, lines 34, 46-50

```meson
# Grammar source file
c_parser_src = files('src/parsers/c_parser.c')

# Tree-sitter dependency
tree_sitter_sp = subproject('tree_sitter')
tree_sitter = tree_sitter_sp.get_variable('tree_sitter_dep')

# Executable linking both
scribe = executable('scribe', 
                    [c_parser_src, /* other sources */],
                    dependencies: [tree_sitter, /* other deps */])
```

### Key Points

1. **Parser creation:** `ts_parser_new()` - Creates new parser instance
2. **Language assignment:** `ts_parser_set_language(parser, lang)` - Returns bool (true = success)
3. **Error handling:** Check return value, cleanup on failure
4. **Language function:** `tree_sitter_LANG()` returns `TSLanguage*`
5. **Compile-time linking:** Grammar compiled as source, core library as dependency

### Pattern Consistency

**11th confirmation** that this pattern is universal across all tree-sitter projects.

**Same pattern in:**
- tree-sitter-issue-2012 (C)
- doxide (C++)
- tree-sitter CLI (Rust wrapper)
- c-language-server (C++)
- ltreesitter (Lua FFI)
- zig-tree-sitter (Zig FFI)
- knut (C++)
- GTKCssLanguageServer (Vala)
- semgrep-c-sharp (OCaml FFI)
- tree-sitter.el (Emacs C module)
- **scribe (C)** ← This repo

**Conclusion:** This is THE standard initialization pattern.

---

## Question 2: How to parse code and create a syntax tree?

### Answer ✅ (11th Confirmation)

**Standard pattern:**

```c
const char* source_code = "int main() { return 0; }";
TSTree* tree = ts_parser_parse_string(
    parser,           // Parser from Q1
    NULL,            // Old tree (for incremental parsing, NULL = full parse)
    source_code,     // Source code string
    strlen(source_code)  // Length in bytes
);

if (!tree) {
    // Parse failed (very rare - usually even invalid syntax produces a tree with ERROR nodes)
}

TSNode root_node = ts_tree_root_node(tree);
// Use tree...

ts_tree_delete(tree);  // Cleanup
```

### Scribe Implementation

**File:** `src/tree_sitter.c`, lines 28-41

```c
TSTree* parse_string(TSParser* parser, sds src) {
  START_ZONE;
  TSTree* tree = ts_parser_parse_string(parser, (void*)0, src, sdslen(src));
  if (!tree) {
    log_fatal("tree_sitter::parse_string failed in parsing src: %s", src);
    goto error_end;
  }
  END_ZONE;
  return tree;
error_end:
  ts_tree_delete(tree);
  END_ZONE;
  return (void*)0;
}
```

**Usage in c_queries.c:**

```c
sds c_function_definition(JanetString name, JanetString src) {
  sds src_sds = sdsnew(src);
  TSParser* parser = create_parser(tree_sitter_c());
  
  // Parse the source code
  TSTree* tree = parse_string(parser, src_sds);
  if (!tree) {
    message_fatal("failed in parsing");
    goto end;
  }
  
  // Use tree to execute queries...
  
end:
  sdsfree(src_sds);
  ts_parser_delete(parser);
  ts_tree_delete(tree);
  return result;
}
```

### Source Origin

**Scribe-specific:** Source code comes from LMDB database (indexed files).

**File:** `src/db.c` (database operations)

```c
sds db_get(MDB_txn* txn, MDB_dbi db_handle, char const* key) {
  // Retrieve file contents from database
  // key = filename, value = source code
}
```

**Workflow:**
1. Index phase: Scan files → store in database (`scribe index`)
2. Query phase: Load file from database → parse → query (`scribe build`)

### Return Value

**TSTree* or NULL:**
- **Non-NULL:** Parse succeeded (even if syntax errors present - tree will have ERROR nodes)
- **NULL:** Parse failed completely (very rare - usually memory allocation failure or timeout)

**Important:** Tree contains ERROR nodes for syntax errors, but still provides structure.

### Root Node

```c
TSNode root_node = ts_tree_root_node(tree);
```

**TSNode is a value type** (not a pointer):
- Lightweight (small struct, typically 32-48 bytes)
- Can be copied freely
- Lifetime tied to TSTree* (don't use after tree deleted)

### Memory Management

```c
// Create
TSTree* tree = ts_parser_parse_string(...);

// Use
TSNode root = ts_tree_root_node(tree);
// ... traverse, query, extract ...

// Destroy
ts_tree_delete(tree);  // Must delete!
```

### Incremental Parsing

**Scribe doesn't use it**, but tree-sitter supports:

```c
// Initial parse
TSTree* tree1 = ts_parser_parse_string(parser, NULL, source1, len1);

// After edit: tell tree what changed
ts_tree_edit(tree1, &edit_struct);

// Re-parse with old tree (reuses unchanged parts)
TSTree* tree2 = ts_parser_parse_string(parser, tree1, source2, len2);

ts_tree_delete(tree1);
// Use tree2...
ts_tree_delete(tree2);
```

### Key Points

1. **Parse function:** `ts_parser_parse_string(parser, old_tree, source, length)`
2. **Return value:** `TSTree*` or NULL (rare)
3. **Root node:** `ts_tree_root_node(tree)` returns TSNode (value type)
4. **Memory:** Must call `ts_tree_delete(tree)` when done
5. **Incremental:** Pass old tree to reuse unchanged parts (optional)

### Pattern Consistency

**11th confirmation** of standard parsing pattern.

**Conclusion:** Same across all repos, language bindings just change syntax.

---

## Question 3: How to walk/traverse the syntax tree?

### Answer ✅ (11th Confirmation)

**Standard approach:** Query-based traversal using tree-sitter queries.

**11 of 11 repos use queries** for syntax highlighting, code analysis, or extraction.

### Scribe's Query Pattern

**File:** `src/tree_sitter.c`, lines 43-95

**Complete query execution:**

```c
sds* query_tree(sds src, TSLanguage* lang, TSTree* tree, sds query_string) {
  sds* s_arr = (void*)0;  // Dynamic array of results (stb_ds)
  TSQueryCursor* cursor = (void*)0;
  TSQuery* query = (void*)0;
  
  // Step 1: Get root node
  TSNode root_node = ts_tree_root_node(tree);
  
  // Step 2: Create query from string
  TSQueryError err = {0};
  uint32_t err_offset = 0;
  query = ts_query_new(lang, query_string, sdslen(query_string), 
                       &err_offset, &err);
  if (!query) {
    switch (err) {
      case TSQueryErrorSyntax:
        log_fatal("syntax error in query: %s at byte offset: %u",
                  query_string, err_offset);
        break;
      default:
        message_fatal("failed in creating query");
        break;
    }
    goto error_end;
  }
  
  // Step 3: Create query cursor
  cursor = ts_query_cursor_new();
  if (!cursor) {
    message_fatal("failed in creating cursor");
    goto error_end;
  }
  
  // Step 4: Execute query on root node
  ts_query_cursor_exec(cursor, query, root_node);
  
  // Step 5: Iterate through matches
  TSQueryMatch match = {0};
  bool matches_remain = false;
  do {
    matches_remain = ts_query_cursor_next_match(cursor, &match);
    if (matches_remain && (match.capture_count != 0)) {
      // Step 6: Extract node text
      sds captured_src = sdscatsds(sdsempty(), src);
      TSNode captured_node = match.captures->node;
      uint32_t start_byte = ts_node_start_byte(captured_node);
      uint32_t end_byte = ts_node_end_byte(captured_node);
      sdsrange(captured_src, start_byte, end_byte - 1);
      
      // Step 7: Store result
      arrput(s_arr, captured_src);
    }
  } while (matches_remain);
  
  // Step 8: Cleanup
  ts_query_delete(query);
  ts_query_cursor_delete(cursor);
  return s_arr;
  
error_end:
  ts_query_delete(query);
  ts_query_cursor_delete(cursor);
  return (void*)0;
}
```

### Example Query Usage

**File:** `src/c_queries.c`, lines 14-40

**Find function by name:**

```c
sds c_function_definition(JanetString name, JanetString src) {
  // Query string: capture function name and entire definition
  sds query_sds = sdsnew(
      "(function_definition (function_declarator (identifier) @func_name)) "
      "@func_def");
  
  TSParser* parser = create_parser(tree_sitter_c());
  TSTree* tree = parse_string(parser, src_sds);
  
  // Execute filtered query (find function with specific name)
  sds func_def = query_filter_tree(src_sds, tree_sitter_c(), tree, 
                                   query_sds, name, 1);
  
  // Cleanup
  ts_parser_delete(parser);
  ts_tree_delete(tree);
  return func_def;
}
```

### Query Syntax

**Tree-sitter query format:**

```scheme
(function_definition 
  (function_declarator 
    (identifier) @func_name)) @func_def
```

**Explanation:**
- `(function_definition ...)` - Match function_definition nodes
- `(function_declarator ...)` - Match nested declarator
- `(identifier) @func_name` - Capture identifier, name it "func_name"
- `@func_def` - Capture entire function_definition, name it "func_def"

**Result:** Each match has 2 captures: `@func_name` and `@func_def`

### Filtered Query Pattern (Scribe-Specific) ⭐

**File:** `src/tree_sitter.c`, lines 97-163

**Purpose:** Find X named Y, return Z

```c
sds query_filter_tree(sds src, TSLanguage* lang, TSTree* tree,
                      sds query_string,
                      char const* filter_string,  // "db_get"
                      int filter_index) {         // Which capture to filter on
  // ... execute query (same as query_tree) ...
  
  do {
    matches_remain = ts_query_cursor_next_match(cursor, &match);
    if (matches_remain && (match.capture_count == 2)) {
      // Extract the filter capture
      filter_src = sdscatsds(sdsempty(), src);
      TSNode filter_node = match.captures[filter_index].node;
      uint32_t f_start_byte = ts_node_start_byte(filter_node);
      uint32_t f_end_byte = ts_node_end_byte(filter_node);
      sdsrange(filter_src, f_start_byte, f_end_byte - 1);
      
      // Compare to target string
      if (sdscmp(filter_src, filter_string_sds) == 0) {
        // Match! Extract the OTHER capture
        return_src = sdscatsds(sdsempty(), src);
        TSNode return_node = match.captures[!filter_index].node;
        uint32_t r_start_byte = ts_node_start_byte(return_node);
        uint32_t r_end_byte = ts_node_end_byte(return_node);
        sdsrange(return_src, r_start_byte, r_end_byte - 1);
        break;  // Found it!
      }
    }
  } while (matches_remain);
  
  // ... cleanup ...
  return return_src;
}
```

**Example:**
- Query: `(function_definition (function_declarator (identifier) @name)) @func`
- Filter: `filter_string = "db_get"`, `filter_index = 0` (name)
- Result: Returns the `@func` capture (entire function) where `@name == "db_get"`

**Use cases:**
- Find function by name → return body
- Find class by name → return members
- Find variable by name → return initializer

### Query Components

**TSQuery:**
- Created from query string
- Parses tree-sitter query syntax
- Returns error info if syntax invalid

**TSQueryCursor:**
- Iterator for query matches
- Executes query on a node
- Returns matches one at a time

**TSQueryMatch:**
```c
typedef struct {
    uint32_t id;                 // Match ID
    uint16_t pattern_index;      // Which pattern matched
    uint16_t capture_count;      // Number of captures
    const TSQueryCapture *captures;  // Array of captures
} TSQueryMatch;
```

**TSQueryCapture:**
```c
typedef struct {
    TSNode node;      // The captured node
    uint32_t index;   // Capture index (for looking up name)
} TSQueryCapture;
```

### Extracting Text

```c
// Get node byte range
uint32_t start = ts_node_start_byte(node);
uint32_t end = ts_node_end_byte(node);

// Extract text from source
const char* text = &source[start];
size_t length = end - start;

// Or with string library
sds text_sds = sdsempty();
text_sds = sdscatlen(text_sds, &source[start], length);
```

### Alternative: Manual Traversal

**Scribe doesn't use it**, but tree-sitter supports:

```c
void walk_tree(TSNode node, const char* source) {
    const char* type = ts_node_type(node);
    
    if (strcmp(type, "function_definition") == 0) {
        // Process function...
    }
    
    // Recurse to children
    uint32_t child_count = ts_node_child_count(node);
    for (uint32_t i = 0; i < child_count; i++) {
        TSNode child = ts_node_child(node, i);
        walk_tree(child, source);
    }
}
```

**Queries are simpler** for most use cases (proven by 11 repos).

### Key Points

1. **Query-based:** 11 of 11 repos use queries (standard approach)
2. **Query creation:** `ts_query_new(lang, query_string, length, &error_offset, &error_type)`
3. **Cursor creation:** `ts_query_cursor_new()`
4. **Execution:** `ts_query_cursor_exec(cursor, query, root_node)`
5. **Iteration:** `ts_query_cursor_next_match(cursor, &match)`
6. **Text extraction:** Use byte ranges from captured nodes
7. **Cleanup:** Delete query and cursor

### Pattern Consistency

**11th confirmation** that query-based traversal is standard.

**Scribe adds:** Filtered query pattern (find X named Y, return Z).

**Conclusion:** Queries are simpler and more maintainable than manual traversal.

---

## Question 4: How to map syntax node types to colors?

### Answer ⚠️ Not Applicable

**Scribe is not a syntax highlighter** - it's a documentation tool that extracts code fragments.

### What Scribe Does Instead

**Maps queries to code entities:**
- Functions: `(function_definition ...) @func`
- Identifiers: `(identifier) @name`
- Entire blocks: `(statement) @stmt`

**Extracts text** without coloring:
```c
// Find function named "db_get"
sds func_def = query_filter_tree(src, lang, tree, query, "db_get", 0);

// Returns plain text (no ANSI codes):
"int db_get(MDB_txn* txn, ...) {\n    // body\n}"
```

### For Highlighting (From Other Repos)

**Standard approach (ltreesitter):**

1. **Query captures semantic names:**
```scheme
(string_literal) @string
(number_literal) @number
(comment) @comment
["if" "else" "while"] @keyword
```

2. **Theme maps names to colors:**
```lua
theme = {
  string = "31",   -- Red
  number = "33",   -- Yellow
  comment = "37",  -- White
  keyword = "35"   -- Magenta
}
```

3. **Lookup during iteration:**
```c
for each capture:
  capture_name = ts_query_capture_name_for_id(query, capture.index, &len);
  color_code = theme[capture_name];
  decoration[byte_index] = color_code;
```

### Why Scribe Doesn't Do This

**Different domain:**
- **Scribe:** Documentation (extract code fragments)
- **Highlighter:** Visualization (colorize syntax)

**Scribe's output:**
```markdown
```c
int db_get(...) {
    // function body
}
```
```

**Highlighter's output:**
```
\x1b[35mint\x1b[0m db_get(...) {
    \x1b[37m// function body\x1b[0m
}
```

### Key Point

Scribe demonstrates **query-based extraction**, which is the first half of highlighting:
1. ✅ Execute queries to find semantic elements
2. ❌ Map to colors (not applicable)
3. ❌ Output ANSI codes (not applicable)

**For highlighting:** Use ltreesitter's decoration table algorithm.

---

## Question 5: How to output colored text with ANSI escape codes?

### Answer ⚠️ Not Applicable

**Scribe outputs Markdown**, not ANSI-colored terminal text.

### What Scribe Outputs

**File:** `src/substitute.c`, lines 318-337

```c
// Render code block to Markdown
rc = render_verbatim("```", data);           // Start fence
rc = render_verbatim_sds(lang, data);        // Language tag
rc = render_verbatim("\n", data);            // Newline
rc = render_verbatim(jstr, data);            // Code content (plain text)
rc = render_verbatim("\n```", data);         // End fence
```

**Result:**
````markdown
```c
int db_get(MDB_txn* txn, ...) {
    // function body
}
```
````

**No ANSI codes** - just plain text in Markdown code blocks.

### For ANSI Output (From Other Repos)

**Standard approach (ltreesitter):**

**ANSI Escape Sequences:**
```c
// Start color
"\x1b[31m"      // Red
"\x1b[32m"      // Green
"\x1b[35m"      // Magenta

// Reset
"\x1b[0m"       // Back to default
```

**Pattern:**
```c
// Output colored text
printf("\x1b[31m");   // Start red
printf("string");     // Text
printf("\x1b[0m");    // Reset

// Result: "string" appears in red
```

**Common color codes:**
- 30 = Black
- 31 = Red
- 32 = Green
- 33 = Yellow
- 34 = Blue
- 35 = Magenta
- 36 = Cyan
- 37 = White
- 0 = Reset

**Decoration table algorithm (ltreesitter):**

```c
// Phase 1: Build decoration table
std::unordered_map<uint32_t, std::string> decoration;
for (auto& capture : captures) {
    std::string color = theme[capture.name];
    for (uint32_t byte = capture.start; byte < capture.end; byte++) {
        decoration[byte] = color;
    }
}

// Phase 2: Output with ANSI codes
std::string prev_color = "";
for (uint32_t byte = 0; byte < source.length(); byte++) {
    std::string current_color = decoration[byte];
    
    if (current_color != prev_color) {
        std::cout << "\x1b[0m";  // Reset
        if (!current_color.empty()) {
            std::cout << "\x1b[" << current_color << "m";
        }
        prev_color = current_color;
    }
    
    std::cout << source[byte];
}
std::cout << "\x1b[0m";  // Final reset
```

### Why Scribe Doesn't Do This

**Output target:**
- **Scribe:** Markdown files (for documentation)
- **Highlighter:** Terminal (for display)

**Scribe's workflow:**
1. Parse markdown (`doc_in.md`)
2. Execute queries in ```scribe blocks
3. Replace with code results
4. Write markdown (`doc_out.md`)

**No terminal interaction** - files only.

### Key Point

Scribe demonstrates **text extraction**, but not coloring:
1. ✅ Execute queries to find code
2. ✅ Extract text using byte ranges
3. ❌ Map to colors (not applicable)
4. ❌ Output ANSI codes (not applicable)

**For ANSI output:** Use ltreesitter's decoration table algorithm.

---

## Summary: All 5 Questions Answered

| Question | Answer Status | Notes |
|----------|---------------|-------|
| Q1: Initialize parser | ✅ 11th confirmation | Standard: `ts_parser_new()` + `ts_parser_set_language()` |
| Q2: Parse code | ✅ 11th confirmation | Standard: `ts_parser_parse_string()` |
| Q3: Walk tree | ✅ 11th confirmation | Query-based traversal (11 of 11 repos) |
| Q4: Map to colors | ⚠️ N/A | Scribe doesn't do highlighting |
| Q5: Output ANSI | ⚠️ N/A | Scribe outputs Markdown, not terminal |

### Key Findings

**11th confirmation:**
- Parser initialization pattern
- Parse string pattern
- Query-based traversal pattern

**Scribe-specific:**
- ⭐ Filtered query pattern (find X named Y, return Z)
- ⭐ On-demand parsing (parse files when queries execute)
- ⭐ Abstraction layer (Lisp on top of tree-sitter)

**Not applicable:**
- Syntax highlighting
- ANSI output
- Color mapping

### What We Still Have

✅ **Algorithm:** Decoration table (ltreesitter)  
✅ **Architecture:** CMake + C++ (knut)  
✅ **Query patterns:** Standard + filtered (scribe)  
✅ **All P0 questions:** Answered 11 times

---

## Conclusion

**Scribe confirms all standard patterns for the 11th time:**
- Parser initialization
- String parsing
- Query-based traversal

**Scribe adds:**
- Filtered query pattern (useful for advanced scenarios)
- On-demand parsing example
- Abstraction layer example

**Scribe does not add:**
- Syntax highlighting knowledge
- ANSI output patterns
- Color mapping

**For highlighting:** Still use ltreesitter's decoration table algorithm and knut's CMake architecture.

**Next step:** Build the prototype - all questions answered, no gaps remain.
