# Study: h20lee/control-flag

**Date:** 2025-12-15  
**Repo:** https://github.com/h20lee/control-flag (Intel Labs)  
**Commit:** Latest from main branch  
**Language:** C++17  
**Build System:** CMake 3.4.3+  
**Tree-sitter Version:** Cloned at build time (latest)

---

## Repository Overview

### What Is ControlFlag?

**ControlFlag** is a self-supervised idiosyncratic pattern detection system developed by Intel Labs. It learns typical patterns in control structures (if statements, loops, etc.) by mining open-source repositories, then uses this knowledge to detect anomalous patterns in user code.

**Primary Use Case:** Typographical error detection, missing NULL checks, control flow anomalies

**Paper:** https://arxiv.org/abs/2011.03616 (MAPS 2020)

### How It Works

**Two-phase system:**

1. **Training Phase (Pattern Mining)**
   - Parse GitHub repositories with tree-sitter
   - Extract control structures (if statements, always blocks)
   - Abstract syntax trees at multiple levels
   - Build decision trees from patterns
   - Store patterns in trie data structure

2. **Scanning Phase (Anomaly Detection)**
   - Parse target code with tree-sitter
   - Extract control structures
   - Compare against learned patterns
   - Flag anomalies (low confidence matches)
   - Suggest corrections using edit distance

### Repository Structure

```
control-flag/
├── src/
│   ├── parser.h                 - Template-based parser wrapper
│   ├── tree_abstraction.{h,cpp} - Multi-level tree abstraction
│   ├── common_util.{h,cpp}      - Tree-sitter utilities
│   ├── train_and_scan_util.{h,cpp} - Training/scanning logic
│   ├── trie.{h,cpp}             - Pattern trie data structure
│   ├── cf_file_scanner.cpp      - Main scanner executable
│   ├── cf_dump_code_blocks.cpp  - Pattern extraction tool
│   └── tree-sitter/
│       ├── CMakeLists.txt       - Grammar build configuration
│       ├── tree-sitter/         - Core library (cloned)
│       ├── tree-sitter-c/       - C grammar (cloned)
│       └── tree-sitter-verilog/ - Verilog grammar (cloned)
├── tests/
│   ├── test_c_parser.cpp        - Parser tests
│   ├── test_expression_compactor.cpp - Abstraction tests
│   └── test_trie.cpp            - Trie tests
├── scripts/
│   ├── mine_patterns.sh         - Training script
│   └── scan_for_anomalies.sh    - Scanning script
├── github/
│   ├── c100.txt                 - Top 100 C repos
│   ├── download_repos.py        - Repo downloader
│   └── deduplicate.py           - Pattern deduplication
└── CMakeLists.txt               - Top-level build
```

---

## Tree-sitter Usage Patterns

### Pattern 1: Template-Based Parser Wrapper ⭐⭐⭐⭐⭐

**What:** RAII wrapper around TSParser with language templating

**Code (parser.h):**
```cpp
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

**Benefits:**
- **RAII:** Automatic cleanup, no manual delete needed
- **Type safety:** Language known at compile time
- **Extensible:** Add new languages by specializing template
- **Move semantics:** Deleted copy, prevents accidents
- **Non-copyable:** One parser per instance

**Usage:**
```cpp
// Create parser for C
ParserBase<LANGUAGE_C> c_parser;
TSParser* parser = c_parser.GetTSParser();

// Create parser for Verilog
ParserBase<LANGUAGE_VERILOG> verilog_parser;
```

### Pattern 2: Thread-Local Parser Optimization ⭐⭐⭐⭐⭐

**What:** Reuse parser across files in multi-threaded scanning

**Code (common_util.cpp):**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source_code,
                        bool report_parse_errors) {
  // Make parser thread-local so we do not need to delete and recreate it
  // for every file to be parsed.
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

**Why thread_local:**
- **Performance:** Create parser once per thread, reuse across files
- **Thread safety:** Each thread has its own parser instance
- **Memory efficiency:** No parser creation/destruction overhead
- **Clean:** Automatic lifetime management

**Usage:**
```cpp
// Thread 1 scans files 1-100
for (auto& file : files_batch_1) {
  auto tree = GetTSTree<LANGUAGE_C>(file, contents);  // Reuses thread_local parser
}

// Thread 2 scans files 101-200
for (auto& file : files_batch_2) {
  auto tree = GetTSTree<LANGUAGE_C>(file, contents);  // Different thread_local parser
}
```

**Scanner implementation:**
```cpp
// Multi-threaded file scanning
std::atomic<size_t> file_index(0);

auto thread_scan_fn = [&](const size_t thread_num, const std::string& log_file) {
  while (file_index.load() < files.size()) {
    std::string& file = files[file_index.load()];
    file_index++;
    
    // Each thread has its own thread_local parser
    switch (language) {
      case LANGUAGE_C:
        ScanFile<LANGUAGE_C>(file, log_file);  // Reuses parser
        break;
    }
  }
};

// Start threads
std::vector<std::thread> scanner_threads;
for (size_t i = 0; i < num_threads; i++) {
  scanner_threads.push_back(std::thread(thread_scan_fn, i, log_files[i]));
}
for (auto& thread : scanner_threads) {
  thread.join();
}
```

### Pattern 3: Multi-Level Tree Abstraction ⭐⭐⭐⭐

**What:** Convert concrete syntax tree to abstract syntax at multiple levels

**Abstraction Levels:**
```cpp
enum TreeLevel {
  LEVEL_MIN = 0,   // Raw tree-sitter output
  LEVEL_ONE = 1,   // With operators (==, !=, %) preserved
  LEVEL_TWO = 2,   // Abstract to EXPR/TERM/VAR/CONST
  LEVEL_MAX = 3,   // Fully abstracted: (VAR), (CONST), (EXPR)
};
```

**Example transformation:**

**Input (C code):**
```c
if (x % 2 == 0)
```

**LEVEL_MIN (raw tree-sitter):**
```
(if_statement
  (parenthesized_expression
    (binary_expression
      left: (binary_expression
        left: (identifier)
        operator: "%"
        right: (number_literal))
      operator: "=="
      right: (number_literal))))
```

**LEVEL_ONE (operators preserved):**
```
(if_statement
  (parenthesized_expression
    (binary_expression ("%")
      (identifier (x))
      (number_literal (2))
    ("==")
      (number_literal (0)))))
```

**LEVEL_TWO (abstracted):**
```
(if_statement
  (EXPR
    (TERM) (BINARY_OP) (TERM)))
```

**LEVEL_MAX (fully abstracted):**
```
(if_statement (EXPR))
```

**Implementation:**
```cpp
template <TreeLevel L, Language G>
inline std::string NodeToString(const TSNode& conditional_expression);

template <>
inline std::string NodeToString<LEVEL_MIN, LANGUAGE_C>(
    const TSNode& conditional_expression) {
  char* node_string = ts_node_string(conditional_expression);
  std::string result = node_string;
  free(node_string);  // ts_node_string mallocs
  return result;
}

template <>
inline std::string NodeToString<LEVEL_ONE, LANGUAGE_C>(
    const TSNode& node) {
  // Include operators and identifiers/literals
  if (IsIdentifier<LANGUAGE_C>(node) || 
      IsLiteral<LANGUAGE_C>(node) || 
      IsPrimitiveType<LANGUAGE_C>(node)) {
    return "(" + ts_node_type(node) + " (" + 
           OriginalSourceExpression(node, source) + "))";
  }
  // Recurse children
  for (uint32_t i = 0; i < ts_node_named_child_count(node); i++) {
    auto child = ts_node_named_child(node, i);
    result += NodeToString<LEVEL_ONE, LANGUAGE_C>(child);
  }
  return result;
}

template <>
inline std::string NodeToString<LEVEL_TWO, LANGUAGE_C>(
    const TSNode& node) {
  if (ts_node_named_child_count(node) == 0) {
    // Terminal: VAR, CONST, etc.
    return ts_node_type(node);
  } else {
    // Non-terminal: EXPR
    return "non_terminal_expression";
  }
}
```

**Why multiple levels:**
- **Training:** Different levels capture different patterns
- **Generalization:** LEVEL_TWO abstracts away variable names
- **Specificity:** LEVEL_ONE preserves exact operators
- **Flexibility:** Choose level based on use case

### Pattern 4: Expression Compacting ⭐⭐⭐⭐

**What:** Convert AST node type strings to numeric IDs for size reduction

**Problem:** 
```
Input:  "(parenthesized_expression (binary_expression ("%") (identifier) (number_literal)))"
Size:   81 characters
```

**Solution:**
```
Output: "(0 (1 ("%") (2) (3)))"
Size:   22 characters (73% reduction!)
```

**Implementation:**
```cpp
class ExpressionCompacter {
 public:
  // Compact: string → IDs
  std::string Compact(const std::string& source);
  
  // Expand: IDs → string
  std::string Expand(const std::string& source);
  
  // Singleton pattern
  static ExpressionCompacter& Get() {
    static ExpressionCompacter instance;
    return instance;
  }

 private:
  using Token = std::string;
  using ID = size_t;
  
  std::string GetID(const Token& token);
  std::string GetToken(const std::string& id_string);
  
  std::shared_mutex mutex_;  // Thread-safe access
  std::atomic<ID> current_id_;
  std::unordered_map<Token, ID> token_id_map_;
  std::unordered_map<ID, Token> id_token_map_;
};

std::string ExpressionCompacter::Compact(const std::string& source) {
  std::string result;
  Token token = "";
  
  for (size_t i = 0; i < source.length(); i++) {
    char c = source[i];
    if (std::isalnum(c) || c == '_') {
      token += c;
    } else if (token.length() > 0) {
      // Token complete, map to ID
      result += GetID(token);
      token = "";
      result += c;
    } else {
      result += c;
    }
  }
  
  if (token.length() > 0)
    result += GetID(token);
  
  return result;
}

std::string ExpressionCompacter::GetID(const Token& token) {
  std::unique_lock lock(mutex_);
  
  auto it = token_id_map_.find(token);
  if (it == token_id_map_.end()) {
    // New token, assign ID
    token_id_map_[token] = current_id_.load();
    id_token_map_[current_id_.load()] = token;
    std::string id = std::to_string(current_id_);
    ++current_id_;
    return id;
  } else {
    return std::to_string(it->second);
  }
}
```

**Usage:**
```cpp
template <TreeLevel L, Language G>
inline std::string NodeToShortString(const TSNode& node) {
  std::string full = NodeToString<L, G>(node);
  return ExpressionCompacter::Get().Compact(full);
}

// Training phase
std::string expression = NodeToShortString<LEVEL_ONE, LANGUAGE_C>(if_condition);
trie.Insert(expression);  // Shorter strings = faster trie operations

// Scanning phase
std::string test_expr = NodeToShortString<LEVEL_ONE, LANGUAGE_C>(if_condition);
bool found = trie.LookUp(test_expr, num_occurrences, confidence);
```

**Benefits:**
- **Size:** 60-80% reduction in string length
- **Speed:** Faster trie operations (shorter strings)
- **Memory:** Less storage for training data
- **Thread-safe:** shared_mutex protects maps
- **Bidirectional:** Can expand IDs back to tokens

### Pattern 5: Manual Tree Traversal (7th Manual Approach) ⭐⭐⭐

**What:** Recursive traversal to collect specific node types

**Code (common_util.cpp):**
```cpp
// Type alias
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
```

**Usage:**
```cpp
ManagedTSTree tree = GetTSTree<LANGUAGE_C>(source_file, contents);
code_blocks_t code_blocks;
CollectCodeBlocksOfInterest<LANGUAGE_C>(tree, code_blocks);

// Process collected blocks
for (auto code_block : code_blocks) {
  std::string pattern = NodeToString<LEVEL_ONE, LANGUAGE_C>(code_block);
  trie.Insert(pattern);
}
```

**Why manual traversal:**
- **Specific nodes:** Only want if conditions, not entire tree
- **Error filtering:** Skip nodes with parse errors
- **Field access:** Use `ts_node_child_by_field_name()` for structured access
- **Language-specific:** Different nodes for C vs Verilog

**Comparison to queries:**
```cpp
// Manual approach (what control-flag does)
void CollectCodeBlocksOfInterest(node, blocks) {
  for (child in node.children) {
    if (IsIfStatement(child)) {
      blocks.push(GetIfConditionNode(child));
    }
    CollectCodeBlocksOfInterest(child, blocks);  // Recurse
  }
}

// Query approach (what highlighting repos use)
const char* query = "(if_statement condition: (_) @condition)";
TSQuery* q = ts_query_new(lang, query, strlen(query), &err_offset, &err);
TSQueryCursor* cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, q, root_node);

while (ts_query_cursor_next_match(cursor, &match)) {
  blocks.push(match.captures[0].node);
}
```

**Manual:** 30-50 lines, full control  
**Queries:** 10-15 lines, simpler

**For highlighting:** Queries are simpler (proven by 9 repos)  
**For semantic analysis:** Manual works fine (control-flag proves it)

### Pattern 6: Language-Specific Template Dispatch ⭐⭐⭐⭐

**What:** Template specialization for language-specific operations

**Pattern:**
```cpp
// Generic template (no implementation)
template <Language L> inline bool IsIfStatement(const TSNode& node);

// C specialization
template <> 
inline bool IsIfStatement<LANGUAGE_C>(const TSNode& node) {
  return IsTSNodeofType(node, "if_statement");
}

// Verilog specialization
template <> 
inline bool IsIfStatement<LANGUAGE_VERILOG>(const TSNode& node) {
  return IsTSNodeofType(node, "conditional_statement");
}
```

**More examples:**
```cpp
template <Language L> inline TSNode GetIfConditionNode(const TSNode& if_stmt);

template <>
inline TSNode GetIfConditionNode<LANGUAGE_C>(const TSNode& if_statement) {
  return ts_node_child_by_field_name(if_statement, "condition", 9);
}

template <>
inline TSNode GetIfConditionNode<LANGUAGE_VERILOG>(const TSNode& if_statement) {
  // Verilog parser doesn't support field names, search by type
  uint32_t count = ts_node_child_count(if_statement);
  for (uint32_t i = 0; i < count; i++) {
    auto child = ts_node_child(if_statement, i);
    if (0 == strcmp("cond_predicate", ts_node_type(child))) {
      return child;
    }
  }
  return TSNode{};  // Not found
}
```

**Benefits:**
- **Type safety:** Compiler enforces language at compile time
- **Zero runtime cost:** Template resolved at compile time
- **Extensible:** Add new languages by adding specializations
- **Clear:** Language differences explicit in code

**Usage:**
```cpp
// Compile-time dispatch
template <Language L>
void ProcessFile(const std::string& file) {
  auto tree = GetTSTree<L>(file, contents);
  code_blocks_t blocks;
  
  // Template parameter determines which specialization
  CollectCodeBlocksOfInterest<L>(tree, blocks);
  
  for (auto block : blocks) {
    if (IsIfStatement<L>(block)) {
      auto condition = GetIfConditionNode<L>(block);
      // ...
    }
  }
}

// Runtime language selection
switch (language) {
  case LANGUAGE_C:
    ProcessFile<LANGUAGE_C>(file);
    break;
  case LANGUAGE_VERILOG:
    ProcessFile<LANGUAGE_VERILOG>(file);
    break;
}
```

### Pattern 7: Build System (ExternalProject) ⭐⭐⭐

**What:** Clone tree-sitter dependencies at configure time

**CMakeLists.txt:**
```cmake
cmake_minimum_required(VERSION 3.4.3)
project(controlflag)
include(ExternalProject)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Clone tree-sitter core
if(NOT EXISTS ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter)
    execute_process(
        COMMAND git clone https://github.com/tree-sitter/tree-sitter.git 
                ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter
    )
endif()

# Clone C grammar
if(NOT EXISTS ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter-c)
    execute_process(
        COMMAND git clone https://github.com/tree-sitter/tree-sitter-c.git 
                ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter-c
    )
endif()

# Clone Verilog grammar
if(NOT EXISTS ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter-verilog)
    execute_process(
        COMMAND git clone https://github.com/tree-sitter/tree-sitter-verilog.git 
                ${PROJECT_SOURCE_DIR}/src/tree-sitter/tree-sitter-verilog
    )
endif()

get_filename_component(TREE_SITTER_INCLUDE 
    src/tree-sitter/tree-sitter/lib/include ABSOLUTE)

add_subdirectory(src)
```

**src/tree-sitter/CMakeLists.txt:**
```cmake
# Core library
add_library(tree-sitter STATIC
  tree-sitter/lib/src/lib.c
)

target_include_directories(tree-sitter
  PRIVATE
  tree-sitter/lib/src
  tree-sitter/lib/include
)

# C grammar
add_library(tree-sitter-c STATIC
  tree-sitter-c/src/parser.c
)

target_include_directories(tree-sitter-c
  PRIVATE
  tree-sitter-c/src
)

# Verilog grammar
add_library(tree-sitter-verilog STATIC
  tree-sitter-verilog/src/parser.c
)

target_include_directories(tree-sitter-verilog
  PRIVATE
  tree-sitter-verilog/src
)
```

**src/CMakeLists.txt:**
```cmake
add_subdirectory(tree-sitter)

add_executable(cf_file_scanner
  cf_file_scanner.cpp
  train_and_scan_util.cpp
  common_util.cpp
  tree_abstraction.cpp
  trie.cpp
  # ...
)

target_include_directories(cf_file_scanner PRIVATE ${TREE_SITTER_INCLUDE})

target_link_libraries(cf_file_scanner
  tree-sitter
  tree-sitter-c
  tree-sitter-verilog
)
```

**Benefits:**
- **Simple:** Just run `cmake . && make`
- **Self-contained:** No manual git cloning needed
- **Version control:** Repos cloned at fixed versions
- **Static linking:** All grammars built as static libraries

**Build process:**
```bash
$ cmake .
-- Cloning tree-sitter...
-- Cloning tree-sitter-c...
-- Cloning tree-sitter-verilog...
-- Configuring done
-- Generating done

$ make -j
[ 10%] Building C object src/tree-sitter/CMakeFiles/tree-sitter.dir/tree-sitter/lib/src/lib.c.o
[ 20%] Building C object src/tree-sitter/CMakeFiles/tree-sitter-c.dir/tree-sitter-c/src/parser.c.o
[ 30%] Building C object src/tree-sitter/CMakeFiles/tree-sitter-verilog.dir/tree-sitter-verilog/src/parser.c.o
[ 40%] Linking C static library libtree-sitter.a
[ 50%] Linking C static library libtree-sitter-c.a
[ 60%] Linking C static library libtree-sitter-verilog.a
[ 70%] Building CXX object src/CMakeFiles/cf_file_scanner.dir/cf_file_scanner.cpp.o
[100%] Linking CXX executable ../bin/cf_file_scanner
```

---

## Key Learnings

### Learning 1: Thread-Local Parser Is Perfect for Multi-Threaded Scanning ⭐⭐⭐⭐⭐

**Discovery:** control-flag uses `thread_local ParserBase<L>` for parser instances

**Why this is brilliant:**
- **Performance:** Parser created once per thread, reused across all files
- **Thread safety:** Each thread has its own parser (no locks needed!)
- **Memory:** No per-file parser creation/destruction overhead
- **Clean:** Automatic lifetime management via RAII

**For our project:**
- If we ever scan multiple files/fences in parallel
- Thread-local parser avoids mutex overhead
- Simple pattern, big performance win

**Comparison:**

| Approach | Parser lifetime | Thread safety | Performance |
|----------|----------------|---------------|-------------|
| Per-file | Create/destroy each file | N/A (single use) | Slow ⚠️ |
| Global + mutex | Once at startup | Manual locking | Bottleneck ⚠️ |
| **Thread-local** | Once per thread | Automatic | **Fast ✅** |

### Learning 2: Template-Based Language Support Is Clean and Extensible ⭐⭐⭐⭐⭐

**Discovery:** control-flag uses template specialization for language-specific operations

**Pattern:**
```cpp
enum Language { LANGUAGE_C, LANGUAGE_VERILOG };

template <Language L> inline bool IsIfStatement(const TSNode&);
template <> inline bool IsIfStatement<LANGUAGE_C>(const TSNode& node) {
  return IsTSNodeofType(node, "if_statement");
}
```

**Benefits:**
- **Compile-time dispatch:** Zero runtime overhead
- **Type safety:** Can't mix C and Verilog nodes
- **Extensible:** Add LANGUAGE_PYTHON by adding specializations
- **Clear:** Language differences explicit in code

**For our project:**
- Use template dispatch for multi-language highlighting
- Or use enum dispatch if runtime flexibility preferred
- Pattern scales to many languages

### Learning 3: Multi-Level Abstraction Enables Flexible Pattern Mining ⭐⭐⭐⭐

**Discovery:** control-flag abstracts ASTs at multiple levels (LEVEL_MIN, LEVEL_ONE, LEVEL_TWO, LEVEL_MAX)

**Use case:** Train at LEVEL_TWO (abstract), scan at LEVEL_ONE (specific)

**Why useful:**
- **Generalization:** LEVEL_TWO ignores variable names → finds patterns
- **Specificity:** LEVEL_ONE preserves operators → detects typos
- **Flexibility:** Choose abstraction level per use case

**For highlighting:** Not directly applicable (we want concrete syntax)  
**For future:** Could enable "highlight variable names differently if they match pattern X"

### Learning 4: Expression Compacting Reduces Size by 60-80% ⭐⭐⭐⭐

**Discovery:** Map node type strings to numeric IDs

**Example:**
- Before: `"(parenthesized_expression (binary_expression ("%") (identifier) (number_literal)))"`
- After: `"(0 (1 ("%") (2) (3)))"`
- Reduction: 73%

**Why useful:**
- **Training data:** Smaller storage for millions of patterns
- **Trie speed:** Shorter strings → faster trie operations
- **Network:** Less data to transfer (if remote training)

**For our project:** Not needed (we're rendering, not storing millions of patterns)

### Learning 5: Manual Traversal Works for Semantic Analysis ⭐⭐⭐

**Discovery:** control-flag uses recursive manual traversal (7th manual approach we've seen)

**Comparison:**

| Approach | Use case | Lines | Repos |
|----------|----------|-------|-------|
| Queries | Highlighting, extraction | 10-50 | 9 repos |
| Manual | Semantic analysis, LSP | 100-1500 | 7 repos |

**When to use manual:**
- Complex logic (skip nodes with errors)
- Language-specific quirks (Verilog doesn't support field names)
- Semantic analysis (control flow, data flow)

**When to use queries:**
- Highlighting (pattern matching)
- Simple extraction (functions, classes)
- Multi-capture (parent/child relationships)

**Verdict:** Queries still better for highlighting (9 vs 7, simpler code)

### Learning 6: Build-Time Git Cloning Is Simple ⭐⭐⭐

**Discovery:** control-flag clones dependencies during `cmake` configure

**Benefits:**
- **Simple:** Just `cmake . && make`
- **Self-contained:** No manual steps
- **Reproducible:** Fixed versions

**Alternative (most other repos):**
- Git submodules
- Or include grammars in repo

**For our project:** Either approach works, choose based on preference

### Learning 7: 17th Confirmation of Static Linking (EXTREMELY REDUNDANT) ⭐

**Discovery:** Yet another repo using static linking

**Pattern:**
```cmake
add_library(tree-sitter STATIC ...)
add_library(tree-sitter-c STATIC ...)
target_link_libraries(cf_file_scanner tree-sitter tree-sitter-c)
```

**Status:** Confirmed 17 times across 17 repos  
**Verdict:** Static linking is universal standard  
**Lesson:** We should have stopped confirming this after Repo 4

---

## What This Repo Does NOT Provide

❌ **No syntax highlighting** - Extracts patterns for ML, not rendering  
❌ **No ANSI output** - Outputs abstract syntax for training  
❌ **No queries** - Uses manual traversal  
❌ **No color mapping** - Maps to abstraction levels, not colors  
❌ **Different domain** - Anomaly detection, not code rendering

**For highlighting:** Use ltreesitter's query-based decoration table  
**For architecture:** Use knut's CMake + C++ patterns

---

## P0 Question Answers

### Q1: How to initialize parser? ✅ (17th confirmation)

**Pattern (RAII wrapper):**
```cpp
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

  TSParser* GetTSParser() { return parser_; }

 private:
  TSParser* parser_ = NULL;
};
```

**Usage:**
```cpp
ParserBase<LANGUAGE_C> parser;
TSParser* p = parser.GetTSParser();
```

**With thread-local:**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source) {
  thread_local ParserBase<L> parser_base;  // Created once per thread
  TSParser* parser = parser_base.GetTSParser();
  
  TSTree* tree = ts_parser_parse_string(parser, nullptr, source.c_str(), source.length());
  return ManagedTSTree(tree);
}
```

**Status:** Confirmed 17 times. Extremely redundant at this point.

### Q2: How to parse code? ✅ (17th confirmation)

**Pattern (same as always):**
```cpp
TSTree *tree = ts_parser_parse_string(parser, nullptr,
                                      source_code.c_str(),
                                      source_code.length());
```

**With file input:**
```cpp
template <Language L>
ManagedTSTree GetTSTree(const std::string& source_file,
                        std::string& file_contents) {
  std::ifstream ifs(source_file.c_str());
  if (!ifs.is_open()) {
    throw cf_file_access_exception("Could not open " + source_file);
  }

  std::stringstream buffer;
  buffer << ifs.rdbuf();
  file_contents = buffer.str();

  static bool kReportParseError = false;
  return GetTSTree<L>(file_contents, kReportParseError);
}
```

**Status:** Confirmed 17 times. No changes.

### Q3: How to walk syntax tree? ✅ (17th confirmation, 7th manual approach)

**Pattern (manual recursive traversal):**
```cpp
template <>
void CollectCodeBlocksOfInterest<LANGUAGE_C>(
    const TSNode& node,
    code_blocks_t& code_blocks) {
  if (ts_node_is_null(node)) { return; }

  uint32_t count = ts_node_child_count(node);
  for (uint32_t i = 0; i < count; i++) {
    auto child = ts_node_child(node, i);
    if (ts_node_is_null(child)) continue;
    
    if (IsIfStatement<LANGUAGE_C>(child)) {
      auto if_condition = GetIfConditionNode<LANGUAGE_C>(child);
      if (!ts_node_has_error(if_condition)) {
        code_blocks.push_back(if_condition);
      }
    }
    
    CollectCodeBlocksOfInterest<LANGUAGE_C>(child, code_blocks);
  }
}
```

**For highlighting:** Use queries (proven simpler by 9 repos):
```cpp
const char* query = "(if_statement condition: (_) @condition)";
TSQuery* q = ts_query_new(lang, query, strlen(query), &err_offset, &err);
// ...
```

**Status:** 7th manual approach seen, queries still simpler (9 vs 7)

### Q4: How to map node types → colors? ⚠️ N/A

**Not applicable** - control-flag maps to abstraction levels, not colors:
```cpp
std::string expression = NodeToString<LEVEL_TWO, LANGUAGE_C>(node);
// Maps to: "(EXPR (TERM) (BINARY_OP) (TERM))"
// NOT to: "\033[33m if \033[0m"
```

**For highlighting:** Use ltreesitter's approach:
```lua
local theme = {
  ["function"] = "\033[33m",  -- Yellow
  ["string"] = "\033[32m",    -- Green
}
```

### Q5: How to output ANSI codes? ⚠️ N/A

**Not applicable** - control-flag outputs abstract syntax for ML:
```
Output: "1,AST_expression_ONE:(if_statement (condition (binary_expression ...)))"
NOT:    "\033[33mif\033[0m \033[37m(\033[0mx > 0)"
```

**For highlighting:** Use ltreesitter's decoration table algorithm

---

## Value for Our Project

### Study Value: 6/10

**What control-flag provides:**
- ⭐⭐⭐⭐⭐ Thread-local parser pattern (excellent for multi-threaded)
- ⭐⭐⭐⭐⭐ Template-based language dispatch (clean multi-language support)
- ⭐⭐⭐⭐ RAII wrappers (automatic cleanup)
- ⭐⭐⭐⭐ Multi-level abstraction (interesting but not applicable)
- ⭐⭐⭐ Expression compacting (not needed for highlighting)
- ⭐⭐⭐ Manual traversal (confirms queries are simpler)
- ⭐ Static linking (17th confirmation - redundant)

**What control-flag does NOT provide:**
- ❌ Syntax highlighting algorithm
- ❌ ANSI output
- ❌ Query examples
- ❌ Color mapping
- ❌ Terminal rendering

**Bottom line:** Good patterns for multi-threaded parsing, but NO highlighting knowledge

### What We Should Use

**✅ Use these patterns:**
1. **Thread-local parser** - If we ever scan multiple files/fences in parallel
2. **Template dispatch** - For multi-language support (if compile-time is acceptable)
3. **RAII wrappers** - Automatic resource management

**❌ Do NOT use:**
1. **Manual traversal** - Queries are simpler (9 repos confirm)
2. **Tree abstraction** - Wrong domain (ML, not rendering)
3. **Expression compacting** - Not needed for highlighting

### Architecture Comparison

| Component | control-flag | Our project |
|-----------|-------------|-------------|
| **Algorithm** | Pattern mining | Highlighting |
| **Traversal** | Manual | **Queries** (ltreesitter) |
| **Parser pattern** | **Thread-local template** | Could use this |
| **Language support** | **Template dispatch** | Could use this |
| **Output** | Abstract syntax | ANSI codes |
| **Build** | ExternalProject | CMake (knut pattern) |

**Key takeaway:** Use parser patterns, ignore domain-specific logic

---

## Session 17 Meta-Analysis

**Time invested:** ~90 minutes (exploration + documentation)  
**Value added:** 6/10 (good patterns, wrong domain)  
**Lesson learned:** Manual traversal has uses (ML/analysis), but queries better for highlighting

**Key insight:**
- control-flag proves manual traversal works for semantic analysis
- Thread-local parsers are brilliant for multi-threaded scanning
- Template-based language support is clean and extensible
- But ZERO new highlighting knowledge (17th confirmation)
- We're WAY past the point of diminishing returns

**Value comparison:**

| Repo | Type | Approach | Value | Why |
|------|------|----------|-------|-----|
| **Repo 5: ltreesitter** | Lua bindings | Query + decoration | ⭐⭐⭐⭐⭐ | THE ALGORITHM |
| **Repo 7: knut** | C++ wrappers | Query + RAII | ⭐⭐⭐⭐⭐ | THE ARCHITECTURE |
| **Repo 16: scopemux** | MCP server | Query organization | ⭐⭐⭐⭐⭐ | QUERY PATTERNS |
| **Repo 13: blockeditor** | Zig editor | Manual + TreeCursor | ⭐⭐⭐⭐ | BEST MANUAL |
| **Repo 17: control-flag** | ML anomaly | Manual + templates | ⭐⭐⭐ | Good patterns |
| **Repo 6: zig-tree-sitter** | Zig FFI | Binding | ⚠️ | Waste |

---

## Updated Statistics

**Repos studied:** 17 of 29 (59%)

1. ✅ tree-sitter-issue-2012 (C) - Basic patterns ⭐⭐⭐
2. ✅ doxide (C++) - Query-based ⭐⭐⭐⭐
3. ✅ tree-sitter CLI (Rust) - Official highlighter ⭐⭐⭐⭐⭐
4. ✅ c-language-server (C++) - Compile-time linking ⭐⭐⭐⭐
5. ✅ **ltreesitter (Lua/C) - THE ALGORITHM!** ⭐⭐⭐⭐⭐
6. ⚠️ zig-tree-sitter (Zig) - Auto-generated ❌
7. ✅ **knut (C++/Qt) - THE ARCHITECTURE!** ⭐⭐⭐⭐⭐
8. ✅ GTKCssLanguageServer (Vala) - Validates queries ⭐⭐⭐
9. ⚠️ semgrep-c-sharp (OCaml) - Auto-generated ❌
10. ✅ tree-sitter.el (Emacs) - Incremental patterns ⭐⭐⭐
11. ✅ scribe (C) - Query patterns ⭐⭐⭐
12. ✅ CodeWizard (C++/Qt) - Manual + colormaps ⭐⭐⭐
13. ✅ **blockeditor (Zig) - BEST manual!** ⭐⭐⭐⭐
14. ✅ minivm (C) - SIMPLEST implementation ⭐⭐⭐
15. ✅ anycode (C++/Qt) - Embedded languages ⭐⭐⭐
16. ✅ **scopemux-core (C) - QUERY ORGANIZATION!** ⭐⭐⭐⭐⭐
17. ✅ **control-flag (C++) - Good patterns, wrong domain** ⭐⭐⭐

**Optimal stopping point:** STILL after Repo 5 (ltreesitter)  
**Study efficiency:** 88% (15 valuable / 17 total)  
**Complete blueprint:** Algorithm (ltreesitter) + Architecture (knut) + Patterns (scopemux) + Optimization (blockeditor)

**Approach comparison:**
- **Query-based:** 9 repos (53%, 10-50 lines)
- **Manual traversal:** 7 repos (41%, 100-1500 lines)
- **Binding-only waste:** 2 repos (12%)
- **Verdict:** Queries win for highlighting (9 vs 7, simpler code)

---

## Conclusion

### What control-flag Teaches

**Domain:** Machine learning anomaly detection for control structures

**Tree-sitter usage:**
1. ⭐⭐⭐⭐⭐ Thread-local parser pattern (brilliant for multi-threaded)
2. ⭐⭐⭐⭐⭐ Template-based language support (clean and extensible)
3. ⭐⭐⭐⭐ Multi-level tree abstraction (interesting but not applicable)
4. ⭐⭐⭐ Manual traversal (7th approach, confirms queries simpler)
5. ⭐ Static linking (17th confirmation - absurdly redundant)

**What it does NOT teach:**
- ❌ Syntax highlighting
- ❌ ANSI output
- ❌ Query usage
- ❌ Color mapping

### For Our Project

**Use:**
- Thread-local parser (if multi-threaded)
- Template dispatch (for multi-language)
- RAII wrappers

**Don't use:**
- Manual traversal (queries simpler)
- Tree abstraction (wrong domain)
- Expression compacting (not needed)

### Session 17 Verdict

**Time:** 90 minutes  
**Value:** 6/10 (good patterns, wrong domain)  
**Lesson:** Manual traversal valid for ML/analysis, but queries better for highlighting  
**Meta-lesson:** 17 repos is WAY too many - should have stopped after Repo 5!

**Should we study more repos?** ❌ **ABSOLUTELY NOT**
- All P0 questions answered (17 times!)
- Algorithm found (ltreesitter)
- Architecture found (knut)
- Query patterns found (scopemux)
- Validation complete (9 repos use queries)
- Optimization patterns found (blockeditor)
- 17 confirmations of static linking (absurd!)
- Time to BUILD, not study!

---

**Next step:** 🚀 **BUILD THE PROTOTYPE** (See RESUME-HERE.md)
