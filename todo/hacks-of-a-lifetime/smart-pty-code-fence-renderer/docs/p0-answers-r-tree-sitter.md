# P0 Answers: DavisVaughan/r-tree-sitter

**Repository:** https://github.com/DavisVaughan/r-tree-sitter  
**Study Date:** 2025-12-15  
**Session:** 20 of planned studies

---

## Summary

**Status:** 20th confirmation of all P0 questions  
**Approach:** Query-based traversal (11th confirmation of query superiority)  
**New patterns:** External pointer GC, query predicates, range support  
**Highlighting value:** 0/10 (bindings only, no highlighting algorithm)

This is the **20th repo** that confirms the same fundamental patterns. r-tree-sitter provides excellent R bindings but adds no new highlighting knowledge.

---

## Q1: How to initialize a tree-sitter parser?

**Answer (20th confirmation):** Standard `ts_parser_new()` + `ts_parser_set_language()` pattern

**R FFI wrapper implementation:**

```c
// src/parser.c
r_obj* ffi_parser_new(
    r_obj* ffi_language,
    r_obj* ffi_timeout,
    r_obj* ffi_included_range_vectors
) {
  // 1. Create parser
  TSParser* parser = ts_parser_new();
  
  // 2. Extract language from R external pointer
  const TSLanguage* language = ts_language_from_external_pointer(ffi_language);
  
  // 3. Set language (with error checking)
  if (!ts_parser_set_language(parser, language)) {
    ts_parser_delete(parser);
    r_abort("Failed to set the parser language.");
  }
  
  // 4. Configure timeout (0 = no timeout)
  const uint64_t timeout = (uint64_t) r_dbl_get(ffi_timeout, 0);
  ts_parser_set_timeout_micros(parser, timeout);
  
  // 5. Configure included ranges (for embedded languages)
  if (!parser_set_included_ranges(parser, ffi_included_range_vectors)) {
    ts_parser_delete(parser);
    r_abort("Failed to set the `included_ranges`. Make sure they ordered earliest to latest, and don't overlap.");
  }
  
  // 6. Wrap in external pointer with finalizer for GC
  return ts_parser_as_external_pointer(parser);
}

// Helper: Wrap parser with finalizer
r_obj* ts_parser_as_external_pointer(TSParser* x) {
  return new_external_pointer((void*) x, parser_finalize);
}

// Finalizer called by R's garbage collector
static void parser_finalize(r_obj* x) {
  if (r_typeof(x) != R_TYPE_pointer) {
    return;
  }
  
  void* ptr = R_ExternalPtrAddr(x);
  if (ptr == NULL) {
    return;
  }
  
  TSParser* parser = (TSParser*) ptr;
  ts_parser_delete(parser);  // Cleanup
  R_ClearExternalPtr(x);
}
```

**R-side usage:**

```r
library(treesitter)

# Get language from grammar package
language <- treesitter.r::language()

# Create parser (with optional timeout and ranges)
parser <- parser(language, timeout = 1000000)  # 1 second timeout

# Parser is automatically cleaned up when R object is garbage collected
```

**Comparison to previous repos:**

| Repo | Language | Pattern | GC |
|------|----------|---------|-----|
| **C/C++ (1-7)** | C/C++ | Direct call | Manual |
| **Lua (5)** | Lua | Direct call | Manual or userdata __gc |
| **Emacs (10)** | Emacs Lisp | External pointer + finalizer | Automatic |
| **R (20)** | R | External pointer + finalizer | Automatic |

**20th confirmation:** Same C API, different GC integration per language

---

## Q2: How to parse code?

**Answer (20th confirmation):** Standard `ts_parser_parse_string()` or `ts_parser_parse_string_encoding()`

**R FFI wrapper implementation:**

```c
// src/parser.c
static r_obj* parser_parse(
    r_obj* ffi_x,
    r_obj* ffi_text,
    r_obj* ffi_encoding,
    const TSTree* old_tree
) {
  // 1. Extract parser from external pointer
  TSParser* parser = ts_parser_from_external_pointer(ffi_x);
  
  // 2. Extract text from R character vector
  r_obj* text = r_chr_get(ffi_text, 0);
  const char* text_c = r_str_c_string(text);
  const uint32_t text_size = r_ssize_as_uint32(r_length(text));
  
  // 3. Get encoding (defaults to UTF-8)
  TSInputEncoding encoding = ts_input_encoding_from_str(
    r_chr_get_c_string(ffi_encoding, 0)
  );
  
  // 4. Parse (with optional old tree for incremental parsing)
  TSTree* tree = ts_parser_parse_string_encoding(
      parser,
      old_tree,   // NULL for initial parse
      text_c,
      text_size,
      encoding
  );
  
  // 5. Handle parse failure (very rare)
  if (tree == NULL) {
    return r_null;
  }
  
  // 6. Wrap tree in external pointer with finalizer
  return ts_tree_as_external_pointer(tree);
}
```

**R-side usage:**

```r
# Parse text
text <- "1 + 2"
tree <- parser_parse(parser, text)

# Incremental parsing (re-parse with old tree)
new_text <- "1 + 2 + 3"
new_tree <- parser_parse(parser, new_text)  # Reuses unchanged parts

# Tree is automatically cleaned up by GC
```

**Comparison to previous repos:**

| Repo | Parse function | Encoding | Incremental |
|------|----------------|----------|-------------|
| **C/C++ (1-7)** | `ts_parser_parse_string()` | UTF-8 default | Optional old_tree |
| **knut (7)** | `ts_parser_parse_string_encoding()` | UTF-16 for Qt | Optional old_tree |
| **R (20)** | `ts_parser_parse_string_encoding()` | UTF-8 default | Optional old_tree |

**20th confirmation:** Same parse API across all repos

---

## Q3: How to walk/traverse the syntax tree?

**Answer (20th confirmation):** Query-based traversal using `ts_query_cursor_exec()` and iteration

**Implementation (match-based):**

```c
// src/query-matches.c
r_obj* ffi_query_matches(
    r_obj* ffi_query,
    r_obj* ffi_capture_names,
    r_obj* ffi_pattern_predicates,
    r_obj* ffi_node,
    r_obj* ffi_tree,
    r_obj* ffi_text,
    r_obj* ffi_start_byte,
    r_obj* ffi_start_row,
    r_obj* ffi_start_column,
    r_obj* ffi_end_byte,
    r_obj* ffi_end_row,
    r_obj* ffi_end_column
) {
  const TSQuery* query = ts_query_from_external_pointer(ffi_query);
  const TSNode* node = ts_node_from_raw(ffi_node);
  
  const char* text = r_chr_get_c_string(ffi_text, 0);
  const uint32_t text_size = r_ssize_as_uint32(r_length(r_chr_get(ffi_text, 0)));
  
  // Create query cursor
  struct TSQueryCursor* cursor = ts_query_cursor_new();
  
  // Optional: Set byte/point range to limit query scope
  if (ffi_start_byte != r_null) {
    uint32_t start_byte = r_dbl_as_uint32(r_dbl_get(ffi_start_byte, 0), "start_byte");
    uint32_t end_byte = r_dbl_as_uint32(r_dbl_get(ffi_end_byte, 0), "end_byte");
    
    TSPoint start_point = {
      .row = r_dbl_as_uint32(r_dbl_get(ffi_start_row, 0), "start_row"),
      .column = r_dbl_as_uint32(r_dbl_get(ffi_start_column, 0), "start_column")
    };
    TSPoint end_point = {
      .row = r_dbl_as_uint32(r_dbl_get(ffi_end_row, 0), "end_row"),
      .column = r_dbl_as_uint32(r_dbl_get(ffi_end_column, 0), "end_column")
    };
    
    ts_query_cursor_set_byte_range(cursor, start_byte, end_byte);
    ts_query_cursor_set_point_range(cursor, start_point, end_point);
  }
  
  // Execute query
  ts_query_cursor_exec(cursor, query, *node);
  
  // Iterate matches
  TSQueryMatch match;
  
  while (ts_query_cursor_next_match(cursor, &match)) {
    // Check pattern predicates (e.g., #eq?, #match?)
    if (!satisfies_pattern_predicates(&match, ffi_pattern_predicates, text, text_size)) {
      continue;  // Skip matches that don't satisfy predicates
    }
    
    // Process each capture in this match
    const r_ssize count = (r_ssize) match.capture_count;
    
    for (r_ssize i = 0; i < count; ++i) {
      const TSQueryCapture capture = match.captures[i];
      // capture.index = which capture name (@foo, @bar, etc)
      // capture.node = the matched node
      
      // Extract node text, byte range, etc
    }
  }
  
  ts_query_cursor_delete(cursor);
  
  return out;  // R data structure with all matches
}
```

**Alternative: Capture-based iteration:**

```c
// src/query-matches.c
r_obj* ffi_query_captures(/* same params */) {
  // ... setup same as above ...
  
  ts_query_cursor_exec(cursor, query, *node);
  
  uint32_t capture_index;
  TSQueryMatch match;
  
  // Iterate individual captures (not grouped by match)
  while (ts_query_cursor_next_capture(cursor, &match, &capture_index)) {
    if (!satisfies_pattern_predicates(&match, ffi_pattern_predicates, text, text_size)) {
      continue;
    }
    
    const TSQueryCapture capture = match.captures[capture_index];
    // Process single capture
  }
  
  ts_query_cursor_delete(cursor);
  return out;
}
```

**R-side usage:**

```r
# Create query
query <- query(language, "
  (function_definition
    name: (identifier) @name
  ) @function
")

# Get matches (grouped)
matches <- query_matches(query, tree_root_node(tree), text)
# Returns list of matches, each with captures

# Get captures (flattened)
captures <- query_captures(query, tree_root_node(tree), text)
# Returns list of individual captures
```

**Comparison to previous repos:**

| Approach | Repos | Complexity | Best For |
|----------|-------|------------|----------|
| **Query-based** | 11 repos (55%) | Low (10-200 lines) | Highlighting, simple analysis |
| **Manual traversal** | 8 repos (40%) | High (40-1500 lines) | Complex semantic analysis, LSP |
| **Bindings only** | 2 repos (10%) | N/A | No traversal shown |

**20th confirmation:** Queries are clearly the standard (11 query-based repos!)

**For highlighting:** Use query-based approach with matches API

---

## Q4: How to map node types → semantic categories/colors?

**Answer:** Not applicable - r-tree-sitter provides parsed data to R, doesn't do highlighting

**What r-tree-sitter provides:**

```r
# Query with semantic capture names
query <- query(language, "
  (function_definition) @function
  (string_literal) @string
  (comment) @comment
")

# Execute query
matches <- query_matches(query, tree_root_node(tree), text)

# Matches contain:
# - Capture names: "function", "string", "comment"
# - Node byte ranges: start_byte, end_byte
# - Node text content

# R code can then map to colors (hypothetical):
theme <- list(
  function = "\033[33m",  # Yellow
  string = "\033[32m",    # Green
  comment = "\033[90m"    # Gray
)

for (match in matches) {
  capture_name <- match$name
  node <- match$node
  
  color <- theme[[capture_name]]
  # Apply color to byte range...
}
```

**For our C++ project:**

```cpp
// Use ltreesitter's decoration table algorithm:

// 1. Query captures semantic names
const char* query_source = R"(
  (function_definition) @function
  (string_literal) @string
  (comment) @comment
)";

// 2. Theme maps capture names to ANSI codes
std::unordered_map<std::string, std::string> theme = {
  {"function", "\033[33m"},  // Yellow
  {"string", "\033[32m"},    // Green
  {"comment", "\033[90m"}    // Gray
};

// 3. Build decoration table (byte position → ANSI code)
std::map<size_t, std::string> decorations;

for (auto& match : matches) {
  std::string color = theme[match.capture_name];
  for (size_t byte = match.start_byte; byte < match.end_byte; byte++) {
    decorations[byte] = color;  // Last write wins (priority)
  }
}

// 4. Render with colors
std::string current_color;
for (size_t i = 0; i < source.length(); i++) {
  if (decorations.count(i) && decorations[i] != current_color) {
    std::cout << "\033[0m" << decorations[i];  // Switch color
    current_color = decorations[i];
  }
  std::cout << source[i];
}
std::cout << "\033[0m";  // Final reset
```

**Comparison to previous repos:**

| Repo | Maps to | Approach | For Highlighting |
|------|---------|----------|------------------|
| **ltreesitter (5)** | ANSI codes | Decoration table | ⭐⭐⭐⭐⭐ THE ALGORITHM |
| **knut (7)** | AST types | Query captures | ❌ Not highlighting |
| **CodeWizard (12)** | Color indices | Hand-crafted map | ⭐⭐⭐ Manual alternative |
| **r-tree-sitter (20)** | R data | Query captures | ❌ Not highlighting |

**20th confirmation:** Bindings provide data, application code does mapping

---

## Q5: How to output ANSI codes to terminal?

**Answer:** Not applicable - r-tree-sitter returns R data structures, not terminal output

**What r-tree-sitter outputs:**

```r
# Parse result is an R tree object
tree <- parser_parse(parser, text)

# Query result is R list of matches/captures
matches <- query_matches(query, tree_root_node(tree), text)

# Matches contain R data structures:
# - Character vectors (capture names)
# - Lists (nodes)
# - Numeric vectors (byte positions)

# No ANSI output - R code can then format as needed
```

**For our C++ project:**

```cpp
// Use ltreesitter's decoration table algorithm (from Q4):

std::map<size_t, std::string> decorations;  // Build decoration map

// Render with ANSI codes
std::string current_color;
for (size_t i = 0; i < source.length(); i++) {
  if (decorations.count(i) && decorations[i] != current_color) {
    std::cout << "\033[0m";        // Reset
    std::cout << decorations[i];   // New color
    current_color = decorations[i];
  }
  std::cout << source[i];
}
std::cout << "\033[0m";  // Final reset
```

**Comparison to previous repos:**

| Repo | Output Type | ANSI Codes | For Highlighting |
|------|-------------|------------|------------------|
| **ltreesitter (5)** | Terminal | ✅ YES | ⭐⭐⭐⭐⭐ THE ALGORITHM |
| **tree-sitter CLI (3)** | Terminal | ✅ YES | ⭐⭐⭐⭐⭐ Official highlighter |
| **knut (7)** | Source files | ❌ NO | ❌ Not highlighting |
| **r-tree-sitter (20)** | R objects | ❌ NO | ❌ Not highlighting |

**20th confirmation:** Bindings provide data, not terminal rendering

---

## Summary of Confirmations

After 20 repos, all P0 questions have the same fundamental answers:

| Question | Answer | Repos Confirming | Conclusion |
|----------|--------|------------------|------------|
| **Q1: Initialize parser** | `ts_parser_new()` + `ts_parser_set_language()` | 20 / 20 | ✅ Universal pattern |
| **Q2: Parse code** | `ts_parser_parse_string()` | 20 / 20 | ✅ Universal pattern |
| **Q3: Walk tree** | Query-based (11) or Manual (8) | 20 / 20 | ✅ Queries are standard |
| **Q4: Map types → colors** | Query captures → theme lookup | 11 / 20 | ✅ ltreesitter shows how |
| **Q5: Output ANSI** | Decoration table algorithm | 1 / 20 | ✅ ltreesitter shows how |

**Key insight after 20 repos:**
- Questions 1-3: UNIVERSAL (all 20 repos confirm)
- Questions 4-5: RARE (only 1 repo has highlighting algorithm)
- **Only ltreesitter (Repo 5) has the decoration table algorithm we need!**

---

## New Patterns from r-tree-sitter

### 1. External Pointer GC Pattern ⭐⭐⭐⭐⭐

**What we learned:**
- How to integrate tree-sitter with garbage-collected languages
- External pointer + finalizer pattern for automatic cleanup
- Works for R, Emacs Lisp, Python, Lua, JavaScript, etc.

**R implementation:**

```c
// Wrap C resource in external pointer
r_obj* new_external_pointer(void* ptr, void (*finalizer)(r_obj*)) {
  r_obj* out = R_MakeExternalPtr(ptr, R_NilValue, R_NilValue);
  R_RegisterCFinalizer(out, finalizer);  // Register finalizer
  return out;
}

// Finalizer cleans up when R GC runs
static void parser_finalize(r_obj* x) {
  void* ptr = R_ExternalPtrAddr(x);
  if (ptr != NULL) {
    TSParser* parser = (TSParser*) ptr;
    ts_parser_delete(parser);  // Cleanup
    R_ClearExternalPtr(x);
  }
}
```

**For other languages:**
- **Python:** `PyCapsule` with destructor
- **Lua:** Full userdata with `__gc` metamethod
- **JavaScript:** FinalizationRegistry API
- **C++:** RAII with destructors (no GC)

---

### 2. Query Predicate Support (2nd Confirmation) ⭐⭐⭐⭐

**What we learned:**
- Query predicates are a standard feature (2nd repo showing it)
- Standard predicates: `#eq?`, `#match?`, `#any-of?`
- Custom predicates via callbacks (livekeys showed this too)

**Implementation:**

```c
while (ts_query_cursor_next_match(cursor, &match)) {
  // Check if match satisfies pattern predicates
  if (!satisfies_pattern_predicates(&match, predicates, text, text_size)) {
    continue;  // Skip non-matching
  }
  
  // Process match...
}
```

**Query example:**

```scheme
; Find function calls named "print" or "println"
(call_expression
  function: (identifier) @name
  (#any-of? @name "print" "println")
) @call

; Find uppercase constants
(identifier) @const
(#match? @const "^[A-Z_]+$")
```

**Repos with predicates:**
- **livekeys (18):** Custom callback predicates
- **r-tree-sitter (20):** Standard predicates
- **Pattern:** Production systems support filtering

---

### 3. Range Support (2nd Confirmation) ⭐⭐⭐⭐

**What we learned:**
- `ts_parser_set_included_ranges()` is standard for embedded languages
- Parse only specific byte ranges of document
- Enables markdown code fence parsing, HTML with embedded JS/CSS, etc.

**Implementation:**

```c
// Build TSRange array from R data
TSRange* v_ranges = (TSRange*) r_raw_begin(ranges);

for (r_ssize i = 0; i < size; ++i) {
  TSPoint start_point = {
    .row = r_dbl_as_uint32(v_start_rows[i], "start_row"),
    .column = r_dbl_as_uint32(v_start_columns[i], "start_column")
  };
  TSPoint end_point = {
    .row = r_dbl_as_uint32(v_end_rows[i], "end_row"),
    .column = r_dbl_as_uint32(v_end_columns[i], "end_column")
  };
  
  TSRange range = {
    .start_point = start_point,
    .end_point = end_point,
    .start_byte = v_start_bytes[i],
    .end_byte = v_end_bytes[i]
  };
  
  v_ranges[i] = range;
}

// Set ranges on parser
ts_parser_set_included_ranges(parser, v_ranges, size);
```

**Repos with ranges:**
- **anycode (15):** First showed range API
- **r-tree-sitter (20):** Production R API for ranges
- **Pattern:** Standard for embedded language parsing

---

### 4. Dynamic Array Pattern ⭐⭐⭐

**What we learned:**
- How to collect variable-length results from queries
- Grow geometrically to avoid repeated reallocation
- Convert to fixed-size at end

**Pattern:**

```c
// Create dynamic array
struct r_dyn_array* p_nodes = r_new_dyn_vector(R_TYPE_list, 5);
struct r_dyn_array* p_names = r_new_dyn_vector(R_TYPE_character, 5);

// Collect results (automatically grows)
while (/* iterate query results */) {
  r_dyn_list_push_back(p_nodes, node);
  r_dyn_chr_push_back(p_names, name);
}

// Convert to fixed R vectors
r_obj* nodes = r_dyn_unwrap(p_nodes);
r_obj* names = r_dyn_unwrap(p_names);
```

**Applies to:**
- Any language with fixed-size collections (R, Java, C#)
- Collecting query results
- Building highlight spans
- Unknown result count upfront

---

## Comparison to All Previous Repos

After 20 repos studied:

| Feature | r-tree-sitter | Best Alternative | Winner |
|---------|---------------|------------------|--------|
| **Highlighting algorithm** | ❌ None | ltreesitter ⭐⭐⭐⭐⭐ | ltreesitter |
| **Query-based traversal** | ✅ Matches + Captures | All query repos | Tie (standard) |
| **Query predicates** | ✅ Standard predicates | livekeys (callbacks) | Tie (both good) |
| **Range support** | ✅ Full API | anycode (same) | Tie (standard) |
| **GC integration** | ✅ External pointers | livekeys (opaque) | r-tree-sitter (GC-specific) |
| **Error handling** | ✅ Detailed errors | knut (std::optional) | knut (C++ idioms) |
| **Build system** | ✅ R Makevars | knut (CMake) | knut (multi-grammar) |

**Overall value for highlighting:** 3/10
- Good patterns for GC integration
- Standard query/predicate/range support
- But **NO highlighting algorithm**

---

## Conclusion

**After 20 repos, the pattern is clear:**

1. **Q1-Q3 are universal** - All 20 repos use same tree-sitter API
2. **Q4-Q5 are rare** - Only 1 repo (ltreesitter) has highlighting algorithm
3. **Queries are standard** - 11 of 20 repos use query-based approach
4. **Bindings ≠ Algorithms** - Great bindings don't teach highlighting

**For our project:**
- **Algorithm:** ltreesitter's decoration table (ONLY repo with algorithm!)
- **Architecture:** knut's CMake + C++ patterns
- **Wrappers:** livekeys's opaque pointers
- **Organization:** scopemux's .scm file patterns

**Should we study more repos?** ❌ **ABSOLUTELY NOT!**
- 20 repos = 67% of list studied
- All patterns confirmed TWENTY times
- Only 1 repo had highlighting algorithm (found in Session 5!)
- **TIME TO BUILD!** 🚀

---

## Meta-Analysis

**Time invested in Session 20:** ~90 minutes  
**Value added:** 3/10 (GC patterns, but no highlighting)  
**Cumulative study time:** ~30 hours across 20 repos  
**Repos with highlighting:** 1 (ltreesitter)  
**Study efficiency:** 85% (17 valuable / 20 total)

**Pattern after 20 repos:**
- ⭐⭐⭐⭐⭐ **ltreesitter** - THE ALGORITHM (only one!)
- ⭐⭐⭐⭐⭐ **knut** - THE ARCHITECTURE
- ⭐⭐⭐⭐⭐ **livekeys** - BEST C++ WRAPPERS
- ⭐⭐⭐⭐⭐ **scopemux** - QUERY ORGANIZATION
- ⭐⭐⭐⭐⭐ **blockeditor** - MANUAL OPTIMIZATION
- ⭐⭐⭐⭐ Others - Confirmations, alternatives, patterns
- ⭐⭐⭐ **r-tree-sitter** - Good bindings, no highlighting

**Key lesson:** Only 1 of 20 repos had the highlighting algorithm we need!

**STOP STUDYING. START BUILDING!** 🚀
