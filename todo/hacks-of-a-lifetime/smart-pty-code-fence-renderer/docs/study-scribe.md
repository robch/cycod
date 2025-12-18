# Study: abhirag/scribe - Literate Programming Documentation Tool

**Repository:** https://github.com/abhirag/scribe  
**Language:** C11  
**Build System:** Meson  
**Study Date:** 2025-01-XX (Session 11)  
**Tree-sitter Usage:** Query-based code extraction for documentation  
**Lines of Code:** ~3,000 (excluding vendored dependencies)

---

## Executive Summary

**Type:** Literate programming tool that lets you write documentation separately from code, using queries to reference code fragments.

**Core Idea:**
1. Index your codebase in a database
2. Write documentation in Markdown
3. Use Lisp (Janet) queries to reference code entities
4. Scribe replaces queries with actual code from your indexed codebase
5. Detect drift between code and documentation via checksums

**Tree-sitter Role:** Parse files on-demand to execute queries that extract specific code entities (functions, structs, etc.)

**Study Value:** 6/10 - Excellent query extraction examples, but no syntax highlighting

---

## What This Project Does

### Problem Statement

According to research, programmers spend **57.62% of time comprehending code** and **23.96% navigating** = **81.58% total** just trying to figure out the codebase.

**Why?** Even well-commented code is a "grab bag of facts without a narrative." The structure is dictated by compilers, not human comprehension.

### Solution

**Scribe** lets you:
1. Write documentation that controls the narrative
2. Reference code fragments via queries (not copy-paste)
3. Keep code and docs in sync automatically

### Example Workflow

**Traditional approach:**
```c
/* Comment in source file */
int editorRowHasOpenComment(erow *row) {
    // implementation...
}
```

**Scribe approach:**

Write documentation in `doc_in.md`:
````markdown
The function checks if a multi-line comment is open:

```scribe
(let [db-c (core/file-src "./src" "db.c")
      func (c/function-definition "editorRowHasOpenComment" db-c)]
  (core/src-slice func 0 -1))
```
````

Scribe generates `doc_out.md` with actual code:
````markdown
The function checks if a multi-line comment is open:

```c
int editorRowHasOpenComment(erow *row) {
    if (row->hl && row->rsize && row->hl[row->rsize-1] == HL_MLCOMMENT &&
        (row->rsize < 2 || (row->render[row->rsize-2] != '*' ||
                            row->render[row->rsize-1] != '/'))) return 1;
    return 0;
}
```
````

**Checksum stored** - if code changes, Scribe warns about drift.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    User Workflow                            │
├─────────────────────────────────────────────────────────────┤
│  1. scribe index   → Index codebase to LMDB database        │
│  2. Write doc_in.md → Markdown with ```scribe queries       │
│  3. scribe build   → Replace queries, generate doc_out.md   │
│  4. scribe repl    → Interactive query exploration          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    Internal Flow                            │
├─────────────────────────────────────────────────────────────┤
│  Markdown (doc_in.md)                                       │
│         ↓                                                    │
│  md4c parser → Finds ```scribe blocks                       │
│         ↓                                                    │
│  Janet interpreter → Executes Lisp query                    │
│         ↓                                                    │
│  C function wrapper (c_queries.c)                           │
│         ↓                                                    │
│  Tree-sitter query (tree_sitter.c)                          │
│    - Create parser (tree_sitter_c())                        │
│    - Parse file from database                               │
│    - Execute query: "(function_definition ...) @func"       │
│    - Extract matched node text                              │
│         ↓                                                    │
│  Return to Janet → Format result                            │
│         ↓                                                    │
│  Replace ```scribe block with ```c block                    │
│         ↓                                                    │
│  Write doc_out.md                                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Dependencies (All Vendored)

Located in `subprojects/`:

| Dependency | Purpose | Version |
|------------|---------|---------|
| **tree-sitter** | Parse and query code | Submodule |
| **janet** | Embedded Lisp for queries | Submodule |
| **lmdb** | Lightning database for indexing | Submodule |
| **md4c** | Markdown parser | Submodule |
| **sds** | Simple Dynamic Strings library | Submodule |
| **log.c** | Logging | Submodule |
| **mkdirp** | Directory creation | Submodule |

**No external dependencies!** Everything is vendored.

---

## Key Files

### Core Implementation

| File | Lines | Purpose |
|------|-------|---------|
| `src/main.c` | 25 | Main entry point |
| `src/tree_sitter.c` | 174 | Tree-sitter wrapper functions |
| `src/c_queries.c` | 122 | C-specific query functions |
| `src/query.c` | 82 | Query module registration |
| `src/substitute.c` | 640 | Markdown processing and query substitution |
| `src/indexer.c` | - | File indexing to database |
| `src/db.c` | - | LMDB database operations |
| `src/lisp.c` | - | Janet integration |

### Grammar

| File | Lines | Purpose |
|------|-------|---------|
| `src/parsers/c_parser.c` | 75,639 | Generated C grammar (from tree-sitter-c) |

### Build

| File | Purpose |
|------|---------|
| `meson.build` | Main build configuration |
| `subprojects/tree_sitter/meson.build` | Tree-sitter library build |

---

## Tree-sitter Usage Analysis

### 1. Initialization Pattern ⭐⭐⭐

**Function:** `create_parser()` in `tree_sitter.c`

```c
TSParser* create_parser(TSLanguage* lang) {
  TSParser* parser = ts_parser_new();
  bool rc = ts_parser_set_language(parser, lang);
  if (!rc) {
    message_fatal("tree_sitter::create_parser failed in setting language");
    goto error_end;
  }
  return parser;
error_end:
  ts_parser_delete(parser);
  return (void*)0;
}
```

**Pattern:**
1. Create parser with `ts_parser_new()`
2. Set language with `ts_parser_set_language(parser, tree_sitter_c())`
3. Check return value (should be true)
4. Error handling with goto cleanup

**11th confirmation** of this standard pattern.

---

### 2. Parsing Pattern ⭐⭐⭐

**Function:** `parse_string()` in `tree_sitter.c`

```c
TSTree* parse_string(TSParser* parser, sds src) {
  TSTree* tree = ts_parser_parse_string(parser, (void*)0, src, sdslen(src));
  if (!tree) {
    log_fatal("tree_sitter::parse_string failed in parsing src: %s", src);
    goto error_end;
  }
  return tree;
error_end:
  ts_tree_delete(tree);
  return (void*)0;
}
```

**Pattern:**
1. Call `ts_parser_parse_string(parser, NULL, source, length)`
2. Check for NULL (parse failure - rare but possible)
3. Return TSTree* or NULL on error
4. Cleanup on error path

**Note:** Source comes from LMDB database (indexed files).

---

### 3. Query Execution Pattern ⭐⭐⭐⭐⭐

**Function:** `query_tree()` in `tree_sitter.c`

This is the **core pattern** for extracting code fragments:

```c
sds* query_tree(sds src, TSLanguage* lang, TSTree* tree, sds query_string) {
  sds* s_arr = (void*)0;  // Dynamic array of results
  TSQueryCursor* cursor = (void*)0;
  TSQuery* query = (void*)0;
  TSNode root_node = ts_tree_root_node(tree);
  
  TSQueryError err = {0};
  uint32_t err_offset = 0;
  
  // Step 1: Create query from string
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
  
  // Step 2: Create cursor
  cursor = ts_query_cursor_new();
  if (!cursor) {
    message_fatal("failed in creating cursor");
    goto error_end;
  }
  
  // Step 3: Execute query on root node
  ts_query_cursor_exec(cursor, query, root_node);
  
  // Step 4: Iterate matches
  TSQueryMatch match = {0};
  bool matches_remain = false;
  do {
    matches_remain = ts_query_cursor_next_match(cursor, &match);
    if (matches_remain && (match.capture_count != 0)) {
      // Step 5: Extract node text
      sds captured_src = sdscatsds(sdsempty(), src);
      TSNode captured_node = match.captures->node;
      uint32_t start_byte = ts_node_start_byte(captured_node);
      uint32_t end_byte = ts_node_end_byte(captured_node);
      sdsrange(captured_src, start_byte, end_byte - 1);
      
      // Step 6: Add to results array
      arrput(s_arr, captured_src);  // stb_ds dynamic array
    }
  } while (matches_remain);
  
  // Step 7: Cleanup
  ts_query_delete(query);
  ts_query_cursor_delete(cursor);
  return s_arr;
  
error_end:
  ts_query_delete(query);
  ts_query_cursor_delete(cursor);
  return (void*)0;
}
```

**Key Steps:**
1. **Create query** from string using `ts_query_new()`
2. **Error handling** - check TSQueryError and report offset
3. **Create cursor** with `ts_query_cursor_new()`
4. **Execute query** with `ts_query_cursor_exec(cursor, query, root_node)`
5. **Iterate matches** with `ts_query_cursor_next_match(cursor, &match)`
6. **Extract text** using byte ranges: `start_byte` to `end_byte - 1`
7. **Cleanup** - delete query and cursor

**This is the 11th repo confirming query-based traversal!**

---

### 4. Filtered Query Pattern ⭐⭐⭐⭐⭐

**Function:** `query_filter_tree()` in `tree_sitter.c`

This is a **powerful pattern** for "find X named Y and return Z":

```c
sds query_filter_tree(sds src, TSLanguage* lang, TSTree* tree, 
                      sds query_string, char const* filter_string, 
                      int filter_index) {
  // ... setup same as query_tree() ...
  
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

**How It Works:**

**Query:** `(function_definition (function_declarator (identifier) @name)) @func`

**Captures:**
- `@name` - The function name (for filtering)
- `@func` - The entire function (what we want to return)

**Example:**
- `filter_string = "db_get"`
- `filter_index = 0` (name is first capture)
- Result: Returns the entire `db_get` function definition

**Use Cases:**
- Find function named X, return its body
- Find class named X, return its members
- Find variable named X, return its initializer

**Could be useful for highlighting:** "Highlight class members differently" or "Highlight specific function names"

---

### 5. C-Specific Query Wrapper ⭐⭐⭐⭐

**File:** `src/c_queries.c`

**Function:** `c_function_definition()`

```c
sds c_function_definition(JanetString name, JanetString src) {
  sds src_sds = sdsnew(src);
  
  // Tree-sitter query string
  sds query_sds = sdsnew(
      "(function_definition (function_declarator (identifier) @func_name)) "
      "@func_def");
  
  TSParser* parser = (void*)0;
  TSTree* tree = (void*)0;
  
  // Create parser for C
  parser = create_parser(tree_sitter_c());
  if (!parser) {
    message_fatal("c_queries::c_function_definition failed in creating parser");
    goto end;
  }
  
  // Parse source
  tree = parse_string(parser, src_sds);
  if (!tree) {
    message_fatal("c_queries::c_function_definition failed in parsing");
    goto end;
  }
  
  // Execute filtered query
  sds func_def = query_filter_tree(src_sds, tree_sitter_c(), tree, 
                                   query_sds, name, 1);
end:
  sdsfree(src_sds);
  sdsfree(query_sds);
  ts_parser_delete(parser);
  ts_tree_delete(tree);
  return func_def;
}
```

**Pattern:**
1. Create query string for C functions
2. Create parser for C language
3. Parse source code
4. Execute filtered query (find function by name)
5. Clean up resources
6. Return function definition

**Exposed to Janet:**
```c
static Janet cfun_c_function_definition(int32_t argc, Janet* argv) {
  janet_fixarity(argc, 2);  // Expect 2 arguments
  if (!db_exists(".")) {
    janet_panicf("scribe db not found in the current directory");
  }
  JanetString name = janet_getstring(argv, 0);
  JanetString src = janet_getstring(argv, 1);
  
  sds func_def = c_function_definition(name, src);
  if (!func_def) {
    janet_panicf("no result to display");
  }
  
  const uint8_t* jstr = janet_string(func_def, sdslen(func_def));
  sdsfree(func_def);
  return janet_wrap_string(jstr);
}
```

**Registered as Janet module:**
```c
static const JanetReg c_cfuns[] = {
    {"tree-sitter-query", cfun_c_tree_sitter_query,
     "(c/tree-sitter-query)\n\nExecute a tree-sitter query"},
    {"function-definition", cfun_c_function_definition,
     "(c/function-definition)\n\nReturn function defined by name"},
};

void register_c_module(JanetTable* env) {
  lisp_register_module(env, "c", c_cfuns);
}
```

**Abstraction Layer:**
- C functions wrap tree-sitter operations
- Janet provides high-level query syntax
- Users write Lisp, not tree-sitter queries

**Lesson:** Abstraction layers make tree-sitter more accessible.

---

### 6. Language Declaration ⭐⭐⭐

**File:** `src/c_queries.c`, line 12

```c
TSLanguage* tree_sitter_c();
```

**File:** `src/parsers/c_parser.c`, line 75605

```c
extern const TSLanguage *tree_sitter_c(void) {
  static const TSLanguage language = {
    .version = LANGUAGE_VERSION,
    .symbol_count = SYMBOL_COUNT,
    .alias_count = ALIAS_COUNT,
    // ... massive generated structure ...
  };
  return &language;
}
```

**Compile-Time Linking Pattern (11th Confirmation):**

**Meson build:**
```meson
tree_sitter_sp = subproject('tree_sitter')
tree_sitter = tree_sitter_sp.get_variable('tree_sitter_dep')

c_parser_src = files('src/parsers/c_parser.c')

scribe = executable('scribe', 
                    [c_parser_src, /* other sources */],
                    dependencies: [tree_sitter, /* other deps */])
```

**Pattern:**
1. Include `parser.c` as regular source file
2. Link tree-sitter core library as dependency
3. Declare `extern TSLanguage* tree_sitter_LANG();`
4. Call directly: `parser = create_parser(tree_sitter_c());`

**No dynamic loading needed!** This is the 11th repo confirming compile-time linking.

---

## P0 Questions: 11th Confirmation

### Q1: How to initialize parser? ✅ (11th time)

**Answer:** Standard pattern confirmed again.

```c
TSParser* parser = ts_parser_new();
bool success = ts_parser_set_language(parser, tree_sitter_c());
if (!success) {
    // Handle error (language version mismatch)
}
```

**Language function:**
```c
extern TSLanguage* tree_sitter_c();  // From parser.c
```

**Build:**
- Compile `parser.c` as source
- Link tree-sitter core library

**Same pattern as all 10 previous repos.**

---

### Q2: How to parse code? ✅ (11th time)

**Answer:** Standard pattern confirmed again.

```c
const char* source = "int main() { return 0; }";
TSTree* tree = ts_parser_parse_string(parser, NULL, source, strlen(source));
if (!tree) {
    // Parse failed (very rare - usually syntax errors still produce trees)
}
TSNode root = ts_tree_root_node(tree);
```

**Same pattern as all 10 previous repos.**

**Scribe-specific:** Source comes from LMDB database (indexed files).

---

### Q3: How to walk syntax tree? ✅ (11th time)

**Answer:** Query-based traversal (11th confirmation!).

**Standard Query Pattern:**
```c
// 1. Create query
TSQuery* query = ts_query_new(lang, query_string, query_length, 
                               &error_offset, &error_type);

// 2. Create cursor
TSQueryCursor* cursor = ts_query_cursor_new();

// 3. Execute query
ts_query_cursor_exec(cursor, query, root_node);

// 4. Iterate matches
TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; i++) {
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        // Get capture name
        uint32_t name_len;
        const char* name = ts_query_capture_name_for_id(
            query, capture_id, &name_len);
        
        // Extract node text
        uint32_t start = ts_node_start_byte(node);
        uint32_t end = ts_node_end_byte(node);
        const char* text = &source[start];
        size_t length = end - start;
    }
}

// 5. Cleanup
ts_query_cursor_delete(cursor);
ts_query_delete(query);
```

**Example Query (from scribe):**
```
(function_definition (function_declarator (identifier) @func_name)) @func_def
```

**Captures:**
- `@func_name` - The function name
- `@func_def` - The entire function definition

**11th repo confirming query-based approach is standard!**

---

### Q4: How to map node types → colors? ⚠️ (N/A)

**Answer:** Not applicable - scribe doesn't do syntax highlighting.

**What scribe does instead:**
- Maps queries to semantic entities (functions, structs, etc.)
- Extracts code fragments for documentation
- No color mapping

**For highlighting:** Use ltreesitter's approach:
1. Query captures semantic names: `(string_literal) @string`
2. Theme maps names to colors: `theme["string"] = "31"` (red)
3. Build decoration table: `decoration[byte_index] = color`

---

### Q5: How to output ANSI codes? ⚠️ (N/A)

**Answer:** Not applicable - scribe outputs Markdown, not ANSI codes.

**What scribe outputs:**
```markdown
```c
int db_get(...) {
    // function body
}
```
```

**For highlighting:** Use ltreesitter's decoration table algorithm:
1. Build decoration map (byte → color)
2. Output with ANSI codes when color changes
3. Use escape sequences: `\x1b[31m` (start red), `\x1b[0m` (reset)

---

## Key Insights

### 1. Abstraction Layers Work Well ⭐⭐⭐⭐⭐

**Pattern:**
```
User writes Lisp queries (Janet)
         ↓
Janet functions map to C functions
         ↓
C functions execute tree-sitter queries
         ↓
Tree-sitter returns matched nodes
         ↓
C functions extract text
         ↓
Return to Janet as strings
```

**Why This Matters:**
- Users don't need to know tree-sitter query syntax
- Can provide simpler, domain-specific API
- Internally uses full power of tree-sitter

**For Our Project:**
- Could put simple config on top of tree-sitter queries
- Example: `highlight_functions = true` → generates function query
- Or expose tree-sitter queries directly for advanced users

---

### 2. On-Demand Parsing ⭐⭐⭐⭐

**Pattern:**
- Index files (filename → database key)
- Parse files only when queries are executed
- Don't parse entire codebase upfront

**Scribe workflow:**
```
1. Index: Scan files, store paths in database
2. Query: User writes query in Markdown
3. Execute:
   - Load file from database
   - Create parser
   - Parse file
   - Execute query
   - Extract results
   - Delete parser and tree
4. Repeat for each query
```

**Why This Matters:**
- Efficient for large codebases (parse what you need)
- No memory overhead for unused files
- Fast iteration during development

**For Our Project:**
- Parse code fences one at a time (already planned)
- No need to parse entire markdown document upfront
- Create parser, parse fence, highlight, destroy parser

---

### 3. Query-Filter Pattern ⭐⭐⭐⭐⭐

**Pattern: Find X Named Y, Return Z**

**Example:** Find function named "db_get", return its body.

**Query:**
```
(function_definition 
  (function_declarator (identifier) @name)) @func
```

**Algorithm:**
```
for each match:
  if match.captures[@name].text == "db_get":
    return match.captures[@func].text
```

**Implementation:**
```c
sds query_filter_tree(sds src, TSLanguage* lang, TSTree* tree,
                      sds query_string, 
                      const char* filter_string,  // "db_get"
                      int filter_index) {         // 0 for @name
  // ... execute query ...
  
  while (ts_query_cursor_next_match(cursor, &match)) {
    if (match.capture_count == 2) {
      // Extract filter capture
      TSNode filter_node = match.captures[filter_index].node;
      sds filter_text = extract_node_text(src, filter_node);
      
      // Compare
      if (strcmp(filter_text, filter_string) == 0) {
        // Match! Return the other capture
        TSNode return_node = match.captures[!filter_index].node;
        sds result = extract_node_text(src, return_node);
        return result;
      }
    }
  }
  return NULL;  // Not found
}
```

**Use Cases:**
- Find function by name
- Find class by name
- Find variable by name
- Find any entity by identifier

**Could Be Useful For Highlighting:**
- Highlight specific function names differently
- Highlight class members vs global variables
- Context-aware highlighting based on identifiers

---

### 4. Error Handling Pattern ⭐⭐⭐

**Pattern:**
```c
type_t* function() {
  resource1_t* res1 = NULL;
  resource2_t* res2 = NULL;
  result_t* result = NULL;
  
  res1 = acquire_resource1();
  if (!res1) {
    message_fatal("failed to acquire resource1");
    goto error_end;
  }
  
  res2 = acquire_resource2();
  if (!res2) {
    message_fatal("failed to acquire resource2");
    goto error_end;
  }
  
  result = process(res1, res2);
  if (!result) {
    message_fatal("processing failed");
    goto error_end;
  }
  
  // Success path cleanup
  release_resource2(res2);
  release_resource1(res1);
  return result;
  
error_end:
  // Error path cleanup (check for NULL before freeing)
  if (res2) release_resource2(res2);
  if (res1) release_resource1(res1);
  return NULL;
}
```

**Why This Works:**
- Single cleanup path (error_end label)
- No resource leaks
- Clear error messages
- NULL checks before cleanup

**Alternative (C++ RAII):**
```cpp
unique_ptr<Resource1> res1(acquire_resource1());
unique_ptr<Resource2> res2(acquire_resource2());
// Automatic cleanup on exception or return
```

**For Our Project:** Use C++ RAII (unique_ptr, shared_ptr) instead of goto pattern.

---

### 5. Query Error Reporting ⭐⭐⭐⭐

**Pattern:**
```c
TSQueryError error_type;
uint32_t error_offset;

TSQuery* query = ts_query_new(lang, query_string, query_length,
                               &error_offset, &error_type);

if (!query) {
  switch (error_type) {
    case TSQueryErrorNone:
      // Should not happen
      break;
    case TSQueryErrorSyntax:
      log_fatal("Syntax error in query: %s at byte offset: %u",
                query_string, error_offset);
      break;
    case TSQueryErrorNodeType:
      log_fatal("Invalid node type in query at byte offset: %u",
                error_offset);
      break;
    case TSQueryErrorField:
      log_fatal("Invalid field name in query at byte offset: %u",
                error_offset);
      break;
    case TSQueryErrorCapture:
      log_fatal("Invalid capture name in query at byte offset: %u",
                error_offset);
      break;
    default:
      log_fatal("Unknown query error");
      break;
  }
  return NULL;
}
```

**Why This Matters:**
- Query syntax errors are common during development
- Error offset helps locate the problem
- Error type helps understand what's wrong

**For Our Project:**
- Check for query errors when loading highlight queries
- Report detailed errors to help users fix their queries
- Consider validating queries at build time

---

## Comparison to Previous Repos

### Highlighting Approach

| Repo | Highlighting | ANSI Output | Algorithm | Value |
|------|--------------|-------------|-----------|-------|
| **ltreesitter** | ✅ Yes | ✅ Yes | Decoration table | 10/10 |
| **tree-sitter CLI** | ✅ Yes | ✅ Yes | Event-based | 9/10 |
| **scribe** | ❌ No | ❌ No | N/A | 6/10 |

**Conclusion:** scribe is not a highlighting tool, so it doesn't help with our core goal.

---

### Query Usage

| Repo | Uses Queries | Query Examples | Pattern Quality |
|------|--------------|----------------|-----------------|
| **ltreesitter** | ✅ Yes | Simple captures | ⭐⭐⭐⭐⭐ |
| **scribe** | ✅ Yes | Filtered queries | ⭐⭐⭐⭐⭐ |
| **knut** | ✅ Yes | Complex queries | ⭐⭐⭐⭐ |
| **doxide** | ✅ Yes | Documentation | ⭐⭐⭐⭐ |

**Conclusion:** scribe adds **filtered query pattern** to our knowledge base.

---

### Build System

| Repo | Build System | Grammar Linking | Pattern |
|------|--------------|-----------------|---------|
| **knut** | CMake | Compile-time | ⭐⭐⭐⭐⭐ |
| **c-language-server** | CMake | Compile-time | ⭐⭐⭐⭐ |
| **scribe** | Meson | Compile-time | ⭐⭐⭐⭐ |

**Conclusion:** Meson is an alternative to CMake, but both use compile-time linking (11th confirmation).

---

### Architecture

| Repo | Language | Abstraction | Complexity |
|------|----------|-------------|------------|
| **scribe** | C11 | Janet on top of tree-sitter | Medium |
| **ltreesitter** | C + Lua | Lua FFI to tree-sitter | Low |
| **knut** | C++17 | C++ wrappers | Medium |

**Conclusion:** scribe shows that abstraction layers (Lisp on top of tree-sitter) work well.

---

## What Scribe Teaches Us

### ✅ New Knowledge

1. **Query-Filter Pattern** ⭐⭐⭐⭐⭐
   - Find X named Y, return Z
   - Useful for context-aware highlighting

2. **Abstraction Layers** ⭐⭐⭐⭐
   - Lisp on top of tree-sitter queries
   - Could provide simpler config for our tool

3. **On-Demand Parsing** ⭐⭐⭐⭐
   - Parse files only when needed
   - Already planned for our project

4. **Database-Backed Indexing** ⭐⭐
   - LMDB for persistent storage
   - Not needed for our use case

5. **Meson Build System** ⭐⭐⭐
   - Alternative to CMake
   - Both use compile-time linking

### ❌ No New Knowledge For Highlighting

1. **No syntax highlighting** - Different domain (documentation)
2. **No ANSI output** - Outputs Markdown
3. **No decoration table** - Uses extraction, not coloring
4. **No color mapping** - Maps to semantic names, not colors

---

## Confirmation Count After 11 Repos

### All P0 Questions Answered

| Question | Times Confirmed |
|----------|-----------------|
| Q1: How to initialize parser? | **11th time** |
| Q2: How to parse code? | **11th time** |
| Q3: How to walk syntax tree? (queries) | **11th time** |
| Q4: How to map node types → colors? | N/A (only 1 repo: ltreesitter) |
| Q5: How to output ANSI codes? | N/A (only 1 repo: ltreesitter) |

### Universal Patterns

| Pattern | Times Confirmed |
|---------|-----------------|
| Query-based traversal | **11 repos** |
| Compile-time linking | **11 repos** |
| Same C API | **11 repos** |
| RAII wrappers (C++) | 2 repos (knut, c-language-server) |
| Decoration table | **1 repo (ltreesitter)** |

---

## What We Still Have (No Changes)

✅ **Algorithm:** Decoration table (ltreesitter) ⭐⭐⭐⭐⭐  
✅ **Architecture:** CMake + C++ (knut) ⭐⭐⭐⭐⭐  
✅ **Validation:** Queries simpler than manual (GTKCssLanguageServer proof) ⭐⭐⭐  
✅ **Build strategy:** Compile-time linking (11 repos confirm) ⭐⭐⭐⭐  
✅ **All P0 questions:** Answered 11 times ⭐⭐⭐⭐⭐

### New From Scribe

✅ **Filtered query pattern** - Useful for complex highlighting scenarios  
✅ **Abstraction layer example** - Could simplify user-facing API  
✅ **On-demand parsing** - Already planned  
✅ **11th confirmation** - All patterns are standard

---

## Session 11 Meta-Analysis

**Time invested:** ~75 minutes (exploration + documentation)  
**Value added:** 6/10 (good query examples, no highlighting knowledge)  
**Lesson learned:** Query-filter pattern is powerful, but no new highlighting insights  

**Key Insight:**
- Scribe confirms all patterns we've seen (11th time)
- Adds **query-filter pattern** for context-aware extraction
- Shows **abstraction layers** work well (Lisp on top of tree-sitter)
- But **no syntax highlighting knowledge** (different domain)

**Value Comparison:**

| Repo | Type | Examples | Value | Why |
|------|------|----------|-------|-----|
| **ltreesitter** | Lua bindings | ✅ c-highlight.lua | ⭐⭐⭐⭐⭐ | THE ALGORITHM |
| **knut** | C++ wrappers | ✅ Production code | ⭐⭐⭐⭐⭐ | THE ARCHITECTURE |
| **scribe** | Documentation tool | ✅ Query extraction | ⭐⭐⭐ | Query patterns |
| **zig-tree-sitter** | Zig FFI | ❌ None | ⚠️ | Waste |

---

## Updated Statistics After 11 Repos

**Repos studied:** 11 of 29

1. ✅ tree-sitter-issue-2012 (C) - Basic patterns ⭐⭐⭐
2. ✅ doxide (C++) - Query-based traversal ⭐⭐⭐⭐
3. ✅ tree-sitter CLI (Rust) - Official highlighter ⭐⭐⭐⭐⭐
4. ✅ c-language-server (C++) - Compile-time linking ⭐⭐⭐⭐
5. ✅ **ltreesitter (Lua/C) - THE ALGORITHM!** ⭐⭐⭐⭐⭐
6. ⚠️ zig-tree-sitter (Zig) - Auto-generated, no value ❌
7. ✅ **knut (C++/Qt) - THE ARCHITECTURE!** ⭐⭐⭐⭐⭐
8. ✅ GTKCssLanguageServer (Vala) - Validates queries ⭐⭐⭐
9. ⚠️ semgrep-c-sharp (OCaml) - Auto-generated, no value ❌
10. ✅ tree-sitter.el (Emacs) - Incremental patterns ⭐⭐⭐
11. ✅ **scribe (C) - Query extraction patterns** ⭐⭐⭐

**Optimal stopping point:** STILL NOW (should have stopped after Repo 5)  
**Study efficiency:** 81.8% (9 valuable repos / 11 total)  
**Complete blueprint:** Algorithm (ltreesitter) + Architecture (knut) + Patterns (scribe)

---

## Recommendation

### ❌ DO NOT STUDY MORE REPOS!

**Why:**

1. **All P0 questions answered** - 11 times (redundant)
2. **Perfect algorithm found** - Decoration table (ltreesitter)
3. **Perfect architecture found** - CMake + C++ (knut)
4. **Query approach validated** - 11 repos confirm
5. **No gaps in highlighting knowledge** - ltreesitter has it all
6. **Scribe added useful patterns** - But no highlighting insights

**What scribe confirmed:**
- ✅ Query-based approach (11th time)
- ✅ Compile-time linking (11th time)
- ✅ Same C API (11th time)

**What scribe added:**
- ✅ Query-filter pattern (useful for advanced highlighting)
- ✅ Abstraction layer example (could simplify API)
- ✅ On-demand parsing (already planned)

**What scribe did NOT add:**
- ❌ Syntax highlighting knowledge
- ❌ ANSI output patterns
- ❌ Decoration table implementation
- ❌ Color mapping

### 🚀 BUILD THE PROTOTYPE NOW!

**What we have:**
- ✅ Algorithm: Decoration table (ltreesitter)
- ✅ Architecture: CMake + C++ (knut)
- ✅ Patterns: Query extraction (scribe)
- ✅ Validation: 11 repos confirm approach
- ✅ Examples: c-highlight.lua (ltreesitter)

**Time estimate:** 2-3 hours

**Next steps:**
1. Read START-HERE.md for build guide
2. Implement decoration table algorithm
3. Test with simple code fences
4. Iterate and improve

---

## Files to Reference

**Best references for building:**

1. **`external/ltreesitter/examples/c-highlight.lua`** ⭐⭐⭐⭐⭐  
   THE ALGORITHM - Decoration table highlighting (136 lines)

2. **`external/knut/3rdparty/CMakeLists.txt`** ⭐⭐⭐⭐⭐  
   THE ARCHITECTURE - Multi-grammar build pattern

3. **`external/knut/src/treesitter/parser.{h,cpp}`** ⭐⭐⭐⭐  
   C++ RAII wrappers

4. **`external/scribe/src/tree_sitter.c`** ⭐⭐⭐⭐  
   Query execution patterns (especially query_filter_tree)

**These files contain everything needed to build the prototype.**

---

## End of Study

This concludes the study of abhirag/scribe. The repo provides excellent query extraction patterns and confirms all previously discovered patterns for the 11th time. However, it does not add new highlighting knowledge since it's a documentation tool, not a syntax highlighter.

**Next action:** Build the prototype using ltreesitter's algorithm and knut's architecture. Optionally use scribe's query-filter pattern for advanced highlighting features.
