# P0 Answers: control-flag

**Date:** 2025-12-15  
**Repo:** h20lee/control-flag  
**Session:** 17 of study phase

---

## P0 Question 1: How to initialize a tree-sitter parser?

### Answer: Template-based RAII wrapper with thread-local optimization ✅

**17th confirmation of same pattern** (EXTREMELY redundant at this point)

### Code Example

**Basic RAII wrapper:**
```cpp
extern "C" const TSLanguage *tree_sitter_c();
extern "C" const TSLanguage *tree_sitter_verilog();

enum Language {
  LANGUAGE_C = 1,
  LANGUAGE_VERILOG = 2
};

template <Language L> 
inline const TSLanguage* GetTSLanguage();

template <> 
inline const TSLanguage* GetTSLanguage<LANGUAGE_C>() {
  return tree_sitter_c();
}

template <> 
inline const TSLanguage* GetTSLanguage<LANGUAGE_VERILOG>() {
  return tree_sitter_verilog();
}

template<Language L>
class ParserBase {
 public:
  ParserBase() {
    parser_ = ts_parser_new();
    bool ret = ts_parser_set_language(parser_, GetTSLanguage<L>());
    assert(ret == true);
  }

  ~ParserBase() {
    if (parser_ != NULL) {
      ts_parser_delete(parser_);
      parser_ = NULL;
    }
  }

  void ResetTSParser() {
    if (parser_) {
      ts_parser_reset(parser_);
    }
  }

  TSParser* GetTSParser() {
    return parser_;
  }

 private:
  ParserBase(const ParserBase&) = delete;
  ParserBase& operator=(const ParserBase&) = delete;

  TSParser* parser_ = NULL;
};
```

**Usage (basic):**
```cpp
// Create parser for C
ParserBase<LANGUAGE_C> c_parser;
TSParser* parser = c_parser.GetTSParser();

// RAII cleanup automatic when c_parser goes out of scope
```

**Thread-local optimization:**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source_code,
                        bool report_parse_errors) {
  // Thread-local: parser created once per thread, reused across files
  thread_local ParserBase<L> parser_base;
  TSParser* parser = parser_base.GetTSParser();

  TSTree *tree = ts_parser_parse_string(parser, nullptr,
                                        source_code.c_str(),
                                        source_code.length());
  parser_base.ResetTSParser();

  TSNode root_node = ts_tree_root_node(tree);

  if (report_parse_errors &&
      (ts_node_is_null(root_node) || ts_node_has_error(root_node))) {
    throw cf_parse_error(source_code);
  }

  return ManagedTSTree(tree);
}

// Multi-threaded usage
auto thread_scan_fn = [&](size_t thread_num) {
  while (has_files_to_scan) {
    std::string file = get_next_file();
    // Each thread has its own thread_local parser
    auto tree = GetTSTree<LANGUAGE_C>(file, contents);
    process_tree(tree);
  }
};

std::vector<std::thread> threads;
for (size_t i = 0; i < num_threads; i++) {
  threads.push_back(std::thread(thread_scan_fn, i));
}
for (auto& t : threads) {
  t.join();
}
```

### Key Points

1. **RAII:** Automatic cleanup via destructor
2. **Template dispatch:** Language known at compile time (zero overhead)
3. **Thread-local:** Parser reused within thread, isolated between threads
4. **Non-copyable:** Deleted copy constructor/assignment prevents accidents
5. **Reset:** Optional reset between parses

### NEW: Thread-Local Pattern ⭐⭐⭐⭐⭐

**Why thread_local is brilliant:**
- **Performance:** Create once per thread, reuse across all files
- **Thread safety:** Each thread has its own parser (no locks!)
- **Memory:** No per-file creation/destruction overhead
- **Clean:** Automatic lifetime management

**Comparison:**

| Approach | Parser lifetime | Thread safety | Performance |
|----------|----------------|---------------|-------------|
| Per-file | Create/destroy each | N/A | Slow ⚠️ |
| Global + mutex | Once at startup | Manual locking | Bottleneck ⚠️ |
| **Thread-local** | Once per thread | Automatic | **Fast ✅** |

### Status: ✅ Confirmed 17 times

**First seen:** Repo 1 (tree-sitter-issue-2012)  
**Pattern:** `ts_parser_new()` + `ts_parser_set_language()`  
**Variations seen:**
1. Basic C (Repos 1-4, 11, 14, 17)
2. C++ RAII wrappers (Repos 4, 7, 12, 17)
3. Zig RAII wrappers (Repos 6, 13)
4. Emacs module (Repo 10)
5. Lua FFI (Repo 5)
6. Template-based (Repo 17) ⭐ NEW
7. Thread-local (Repo 17) ⭐ NEW

**Verdict:** Universal pattern, confirmed 17 times (absurdly redundant)

---

## P0 Question 2: How to parse source code into a syntax tree?

### Answer: ts_parser_parse_string() ✅

**17th confirmation of same API** (no changes)

### Code Example

**Parse from string:**
```cpp
TSTree* tree = ts_parser_parse_string(parser, nullptr,
                                      source_code.c_str(),
                                      source_code.length());
```

**Parse from file:**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source_file,
                        std::string& file_contents) {
  // Read file
  std::ifstream ifs(source_file.c_str());
  if (!ifs.is_open()) {
    throw cf_file_access_exception("Could not open " + source_file);
  }

  std::stringstream buffer;
  buffer << ifs.rdbuf();
  file_contents = buffer.str();

  // Parse string
  static bool kReportParseError = false;
  return GetTSTree<L>(file_contents, kReportParseError);
}
```

**With error checking:**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source_code,
                        bool report_parse_errors) {
  thread_local ParserBase<L> parser_base;
  TSParser* parser = parser_base.GetTSParser();

  TSTree *tree = ts_parser_parse_string(parser, nullptr,
                                        source_code.c_str(),
                                        source_code.length());
  parser_base.ResetTSParser();

  TSNode root_node = ts_tree_root_node(tree);

  if (report_parse_errors &&
      (ts_node_is_null(root_node) || ts_node_has_error(root_node))) {
    throw cf_parse_error(source_code);
  }

  return ManagedTSTree(tree);
}
```

**Multi-threaded parsing:**
```cpp
// Each thread parses files using thread_local parser
auto thread_fn = [&]() {
  while (file_index < files.size()) {
    std::string file = files[file_index++];
    std::string contents;
    
    // Thread-local parser reused
    auto tree = GetTSTree<LANGUAGE_C>(file, contents);
    
    process_tree(tree);
  }
};
```

### Key Points

1. **Same API:** 17 repos, all use `ts_parser_parse_string()`
2. **Thread-safe:** Safe to call from multiple threads (with separate parsers)
3. **Error detection:** Check `ts_node_has_error(root_node)`
4. **Reset optional:** Can call `ts_parser_reset()` between parses
5. **Incremental:** Pass old tree as 2nd arg for incremental parsing

### Status: ✅ Confirmed 17 times

**API signature:**
```c
TSTree* ts_parser_parse_string(
  TSParser *self,
  const TSTree *old_tree,   // NULL for full parse
  const char *string,
  uint32_t length
);
```

**Alternative (streaming):**
```c
TSTree* ts_parser_parse(
  TSParser *self,
  const TSTree *old_tree,
  TSInput input  // Callback-based
);
```

**Verdict:** Universal API, no repo-specific variations

---

## P0 Question 3: How to walk/traverse the syntax tree?

### Answer: Manual recursive traversal (7th manual approach) ✅

**Pattern:** Recursive function collecting specific node types

### Code Example

**Basic traversal:**
```cpp
using code_blocks_t = std::vector<TSNode>;

template <>
void CollectCodeBlocksOfInterest<LANGUAGE_C>(
    const TSNode& node,
    code_blocks_t& code_blocks) {
  if (ts_node_is_null(node)) { return; }

  uint32_t count = ts_node_child_count(node);
  for (uint32_t i = 0; i < count; i++) {
    auto child = ts_node_child(node, i);
    if (ts_node_is_null(child)) continue;
    
    // Check if child is interesting
    if (IsIfStatement<LANGUAGE_C>(child)) {
      auto if_condition = GetIfConditionNode<LANGUAGE_C>(child);
      if (!ts_node_has_error(if_condition)) {
        code_blocks.push_back(if_condition);
      }
    }
    
    // Recurse
    CollectCodeBlocksOfInterest<LANGUAGE_C>(child, code_blocks);
  }
}

template <>
inline bool IsIfStatement<LANGUAGE_C>(const TSNode& node) {
  return IsTSNodeofType(node, "if_statement");
}

inline bool IsTSNodeofType(const TSNode& node, const std::string type) {
  return (!ts_node_is_null(node) &&
          0 == type.compare(ts_node_type(node)));
}

template <>
inline TSNode GetIfConditionNode<LANGUAGE_C>(const TSNode& if_statement) {
  const std::string& kIfCondition = "condition";
  return ts_node_child_by_field_name(if_statement,
                      kIfCondition.c_str(), kIfCondition.length());
}
```

**Verilog variant:**
```cpp
template <>
void CollectCodeBlocksOfInterest<LANGUAGE_VERILOG>(
    const TSNode& node,
    code_blocks_t& code_blocks) {
  if (ts_node_is_null(node)) { return; }

  uint32_t count = ts_node_child_count(node);
  for (uint32_t i = 0; i < count; i++) {
    auto child = ts_node_child(node, i);
    if (ts_node_is_null(child)) continue;
    
    if (IsAlwaysBlock(child)) {
      code_blocks.push_back(child);
    }
    
    CollectCodeBlocksOfInterest<LANGUAGE_VERILOG>(child, code_blocks);
  }
}

inline bool IsAlwaysBlock(const TSNode& node) {
  return IsTSNodeofType(node, "always_construct");
}
```

**Usage:**
```cpp
ManagedTSTree tree = GetTSTree<LANGUAGE_C>(source_file, contents);
code_blocks_t code_blocks;

// Collect all if conditions
CollectCodeBlocksOfInterest<LANGUAGE_C>(tree, code_blocks);

// Process collected blocks
for (auto code_block : code_blocks) {
  std::string pattern = NodeToString<LEVEL_ONE, LANGUAGE_C>(code_block);
  trie.Insert(pattern);
}
```

### Comparison: Manual vs Queries

**Manual approach (control-flag):**
```cpp
void CollectCodeBlocksOfInterest(node, blocks) {
  for (child in node.children) {
    if (IsIfStatement(child)) {
      blocks.push(GetIfConditionNode(child));
    }
    CollectCodeBlocksOfInterest(child, blocks);  // Recurse
  }
}
```

**Lines:** 30-50  
**Complexity:** Medium  
**Benefits:** Full control, error filtering, field access

**Query approach (what highlighting repos use):**
```cpp
const char* query = "(if_statement condition: (_) @condition)";
TSQuery* q = ts_query_new(lang, query, strlen(query), &err_offset, &err);
TSQueryCursor* cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, q, root_node);

while (ts_query_cursor_next_match(cursor, &match)) {
  blocks.push_back(match.captures[0].node);
}
```

**Lines:** 10-15  
**Complexity:** Low  
**Benefits:** Simpler, declarative, less code

### When to Use Each

**Manual traversal (7 repos):**
- Complex logic (skip parse errors)
- Language-specific quirks (Verilog doesn't support field names)
- Semantic analysis (control flow, data flow)
- Deep inspection needed

**Queries (9 repos):**
- Highlighting (pattern matching)
- Simple extraction (functions, classes)
- Multi-capture (parent/child relationships)
- Standard patterns

**For highlighting:** Queries are simpler (proven by 9 repos vs 7)

### Status: ✅ 17th confirmation, 7th manual approach

**Manual approaches seen:**
1. GTKCssLanguageServer (1500 lines, LSP)
2. tree-sitter.el (200 lines, Emacs)
3. CodeWizard (400 lines, editor)
4. blockeditor (420 lines, TreeCursor stack)
5. minivm (40 lines, simplest)
6. anycode (embedded languages)
7. **control-flag (pattern mining)** ⭐ NEW

**Query approaches seen:** 9 repos (simpler for highlighting)

**Verdict:** Manual valid for semantic analysis, queries better for highlighting

---

## P0 Question 4: How to map node types to colors?

### Answer: ⚠️ Not applicable (control-flag maps to abstraction levels, not colors)

### What control-flag Does

**Maps AST → Abstract Syntax for ML:**
```cpp
// Convert concrete syntax to abstract syntax
std::string expression = NodeToString<LEVEL_TWO, LANGUAGE_C>(if_condition);

// Result: "(EXPR (TERM) (BINARY_OP) (TERM))"
// NOT:    "\033[33mif\033[0m \033[37m(\033[0mx > 0)"
```

**Abstraction levels:**
```cpp
enum TreeLevel {
  LEVEL_MIN = 0,   // Raw: "(if_statement (condition (binary_expression ...)))"
  LEVEL_ONE = 1,   // With operators: "(if_statement ("%") (identifier (x)))"
  LEVEL_TWO = 2,   // Abstract: "(EXPR (TERM) (BINARY_OP) (TERM))"
  LEVEL_MAX = 3,   // Fully abstract: "(EXPR)"
};
```

**Purpose:** Train ML models at different abstraction levels

### What Highlighting Does

**Maps AST → ANSI codes (from ltreesitter):**
```lua
local theme = {
  ["function"] = "\033[33m",  -- Yellow
  ["string"] = "\033[32m",    -- Green
  ["keyword"] = "\033[35m",   -- Magenta
  ["comment"] = "\033[90m"    -- Gray
}

-- Query captures semantic names
local query = [[
  (function_definition) @function
  (string_literal) @string
  (if_statement) @keyword
  (comment) @comment
]]

-- Apply theme
for capture, node in query:iter() do
  local color = theme[capture]
  decoration_table[start_byte] = color
end
```

### Status: ⚠️ N/A for 17th time

**Repos that map to colors:** 9 (all use query captures + theme lookup)  
**Repos that don't:** 8 (including control-flag)

**For highlighting:** Use ltreesitter's query + theme approach

---

## P0 Question 5: How to output ANSI escape codes?

### Answer: ⚠️ Not applicable (control-flag outputs abstract syntax for ML, not terminal)

### What control-flag Outputs

**Abstract syntax for ML training:**
```
// Original source
if (x % 2 == 0)

// control-flag output
1,AST_expression_ONE:(if_statement (condition (binary_expression ("%") (identifier (x)) (number_literal (2)) ("==") (number_literal (0)))))
1,AST_expression_TWO:(if_statement (EXPR (TERM) (BINARY_OP) (TERM)))
```

**Purpose:** Store patterns in trie for anomaly detection

**NOT terminal ANSI:**
```
\033[35mif\033[0m \033[37m(\033[0mx \033[37m%\033[0m 2 \033[37m==\033[0m 0\033[37m)\033[0m
```

### What Highlighting Does (from ltreesitter)

**Decoration table approach:**
```lua
local decoration = {}  -- decoration[byte_offset] = "\033[33m"

-- Query matches
for capture, node in query:iter() do
  local color = theme[capture]
  local start_byte = node:start()
  local end_byte = node:end_()
  
  for i = start_byte, end_byte - 1 do
    decoration[i] = color
  end
end

-- Render with decorations
for i = 1, #source do
  if decoration[i] then
    io.write(decoration[i])
  end
  io.write(source:sub(i, i))
end
```

### Status: ⚠️ N/A for 17th time

**Repos that output ANSI:** 3 (tree-sitter CLI, ltreesitter, minivm)  
**Repos that don't:** 14 (editors, LSPs, ML tools)

**For highlighting:** Use ltreesitter's decoration table algorithm

---

## Summary: 17th Confirmation

### All Questions Answered ✅

1. **Initialize parser:** ✅ (17th time) - ParserBase<Language> + thread_local
2. **Parse code:** ✅ (17th time) - ts_parser_parse_string()
3. **Walk tree:** ✅ (17th time, 7th manual) - Recursive CollectCodeBlocksOfInterest
4. **Map types → colors:** ⚠️ N/A - Maps to abstraction levels
5. **Output ANSI:** ⚠️ N/A - Outputs abstract syntax for ML

### What's NEW in control-flag

**⭐⭐⭐⭐⭐ Thread-local parser pattern:**
```cpp
thread_local ParserBase<L> parser_base;  // Create once per thread
TSParser* parser = parser_base.GetTSParser();  // Reuse
```

**⭐⭐⭐⭐⭐ Template-based language dispatch:**
```cpp
template <Language L> inline bool IsIfStatement(const TSNode&);
template <> inline bool IsIfStatement<LANGUAGE_C>(const TSNode& node) {
  return IsTSNodeofType(node, "if_statement");
}
```

**⭐⭐⭐⭐ Multi-level abstraction:**
```cpp
enum TreeLevel { LEVEL_MIN, LEVEL_ONE, LEVEL_TWO, LEVEL_MAX };
std::string abstract = NodeToString<LEVEL_TWO, LANGUAGE_C>(node);
```

**⭐⭐⭐ Expression compacting:**
```cpp
std::string compact = ExpressionCompacter::Get().Compact(expression);
// "(parenthesized_expression (binary_expression))" → "(0 (1))"
```

### What's Still the Same

- Parser initialization: `ts_parser_new()` + `ts_parser_set_language()`
- Parsing: `ts_parser_parse_string()`
- Error detection: `ts_node_has_error()`
- Static linking: 17th confirmation (absurdly redundant!)
- Manual traversal: 7th approach (queries still simpler)

### Value for Our Project

**Use these patterns:**
- ✅ Thread-local parser (if multi-threaded)
- ✅ Template dispatch (for multi-language)
- ✅ RAII wrappers

**Don't use:**
- ❌ Manual traversal (queries simpler)
- ❌ Tree abstraction (wrong domain)
- ❌ Expression compacting (not needed)

### Bottom Line

**Time spent:** 90 minutes  
**New information:** 6/10 (good patterns, wrong domain)  
**Highlighting knowledge:** 0/10 (ZERO - it's an ML tool!)

**Key insight:**
- control-flag proves manual traversal + templates work for semantic analysis
- Thread-local parsers are brilliant for multi-threaded scanning
- But ZERO new highlighting knowledge (17th confirmation)
- We're WAY past the point of diminishing returns

**Verdict:** Should have stopped after Repo 5 (ltreesitter)

---

## Reference Files

**For thread-local pattern:**
- `external/control-flag/src/common_util.cpp` (lines 29-56)

**For template dispatch:**
- `external/control-flag/src/parser.h` (lines 34-58, 102-172)

**For manual traversal:**
- `external/control-flag/src/common_util.cpp` (lines 79-119)

**For abstraction:**
- `external/control-flag/src/tree_abstraction.h` (lines 38-214)

**For build:**
- `external/control-flag/CMakeLists.txt` (lines 34-50)
- `external/control-flag/src/tree-sitter/CMakeLists.txt` (all)

---

**Next step:** 🚀 **BUILD THE PROTOTYPE** (See RESUME-HERE.md)
