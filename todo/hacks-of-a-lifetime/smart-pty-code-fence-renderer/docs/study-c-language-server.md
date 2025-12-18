# Study Report: dgawlik/c-language-server

**Date:** 2025-12-15 (Session 3)  
**Repository:** https://github.com/dgawlik/c-language-server  
**Location:** `external/c-language-server/`  
**Language:** C++17  
**Purpose:** Code navigation server for C language (find definitions, usages, etc.)  

---

## Executive Summary

This is a **production C++ application** using Tree-sitter for semantic analysis of C code. It provides LSP-like functionality (go-to-definition, find-usages) through a VSCode plugin. The project reveals critical insights about **compile-time grammar linking**, **C++ wrapper patterns**, and **manual tree traversal** approaches.

**Key Discovery:** This repo uses **compile-time linking of parser.c** rather than dynamic loading - a simpler and more common approach than runtime .so/.dll loading!

---

## Repository Overview

### What It Does
- Indexes C source files into a semantic graph (Stack Graph)
- Finds symbol definitions across files
- Tracks type references and struct/function definitions
- Provides "Go to Definition" and "Find Usages" for VSCode

### Architecture
```
VSCode Plugin (Node.js)
    ↓ (stdin/stdout, JSON)
C++ Language Server
    ├─ TSParser (Tree-sitter C API)
    ├─ Stack Graph Builder (semantic tree)
    ├─ Cross-linker (resolve references)
    └─ Query engine (find definitions/usages)
```

### Performance Characteristics
From the README:
- **Indexing speed:** ~50 files/second
- **Large codebases:** Linux kernel in 3-4 minutes
- **Optimized:** "optimized away all bottlenecks"

This proves Tree-sitter is **production-ready** for real-time use!

---

## Key Files Examined

| File | Lines | Purpose |
|------|-------|---------|
| `app/main.cpp` | 176 | JSON command loop, integrates engine |
| `lib/src/stack-graph-engine.cpp` | 504 | File loading, parsing, indexing, resolution |
| `lib/src/stack-graph-tree.cpp` | 402 | Tree traversal, semantic graph building |
| `lib/include/stack-graph-tree.h` | 72 | Node types, API definitions |
| `deps/tree-sitter-c/parser.c` | 75,639 | Generated C grammar (compiled in) |
| `tests/syntax-tree-test.cpp` | 147 | Unit tests showing usage patterns |
| `CMakeLists.txt` | 42 | Build configuration (reveals linking strategy) |

---

## P0 Questions: How This Repo Answers Them

### ✅ Q1: How to initialize parser?

**Answer:** Same pattern seen in repos 1 & 2, now confirmed in 3rd codebase!

**From `stack-graph-engine.cpp` lines 47-50:**
```cpp
TSParser *parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_c());
```

**Language declaration (line 23):**
```cpp
extern "C" TSLanguage *tree_sitter_c();
```

**Key insight:** The `tree_sitter_c()` function is:
- Declared in C++ with `extern "C"` linkage
- Defined in `deps/tree-sitter-c/parser.c` (line 75605)
- Returns a pointer to static `TSLanguage` struct
- Compiled directly into the executable (see CMakeLists.txt line 25)

---

### ✅ Q2: How to parse code?

**Answer:** Confirmed pattern, with lifecycle details.

**From `stack-graph-engine.cpp` lines 35-76:**
```cpp
bool StackGraphEngine::loadFile(string path)
{
    // Read file into string
    std::ifstream file_stream(path);
    std::stringstream buffer;
    buffer << file_stream.rdbuf();
    auto contents = buffer.str();
    auto source_code = contents.c_str();
    file_stream.close();

    // Create parser
    TSParser *parser = ts_parser_new();
    ts_parser_set_language(parser, tree_sitter_c());

    // Parse
    TSTree *tree = ts_parser_parse_string(
        parser,
        NULL,                    // old_tree (for incremental parsing)
        source_code,
        strlen(source_code));

    // Get root node
    TSNode root_node = ts_tree_root_node(tree);

    // Process tree...
    auto sg_tree = build_stack_graph_tree(root_node, source_code);
    
    if (sg_tree != nullptr) {
        // Success - store results
        this->translation_units[path] = sg_tree;
        _index(path, sg_tree, this->node_table);
        ret = true;
    }

    // Cleanup
    ts_tree_delete(tree);
    return ret;
}
```

**Key insights:**
- Parser created per-file (not reused across files)
- NULL for `old_tree` parameter = full parse (not incremental)
- Tree can fail to parse but still return non-NULL with ERROR nodes
- Tree deletion is critical (prevents memory leak)
- **Note:** Parser isn't deleted here - potential memory leak! Should add `ts_parser_delete(parser)`

---

### ✅ Q3: How to walk syntax tree?

**Answer:** NEW APPROACH! This repo uses **manual traversal** instead of queries.

#### Approach: Recursive Descent with State Machine

**From `stack-graph-tree.cpp` lines 178-392:**
```cpp
void build_stack_graph(
    vector<shared_ptr<StackGraphNode>> &stack,  // Scope stack
    string &code,                                // Source code
    TSNodeWrapper node,                          // Current node
    _Context &ctx)                               // Traversal context
{
    // Skip error nodes
    if (strcmp(node.type(), "ERROR") == 0) {
        return;
    }
    
    // Handle specific node types
    else if (strcmp(node.type(), "function_definition") == 0) {
        auto function_node = new StackGraphNode(...);
        stack.push_back(function_node_ptr);
        
        auto declarator_node = node.childByFieldName("declarator");
        build_stack_graph(stack, code, *declarator_node, ctx);
        
        auto body_node = node.childByFieldName("body");
        build_stack_graph(stack, code, *body_node, ctx);
        
        stack.pop_back();
    }
    
    // ... more node type handlers ...
    
    else {
        // Default: recurse into children
        for (uint32_t i = 0; i < node.child_count(); i++) {
            build_stack_graph(stack, code, *node.child(i), ctx);
        }
    }
}
```

#### Context Pattern

**From `stack-graph-tree.cpp` lines 139-152:**
```cpp
struct _Context
{
    string state;                   // What we're looking for
    shared_ptr<StackGraphNode> jump_to;  // Type definition pointer
    string type;                    // Current type name
    Point location;                 // Position in source
};
```

**Context states used:**
- `"function_declarator"` - parsing function name
- `"declaration"` - parsing variable declarations
- `"populate_type"` - extracting type names
- `"reference"` - tracking symbol references
- `"skip_compound"` - don't create new scope

This carries semantic information through the recursion!

#### Field Access Pattern

**The key to clean traversal:**
```cpp
auto declarator = node.childByFieldName("declarator");
auto parameters = node.childByFieldName("parameters");
auto body = node.childByFieldName("body");
auto type_spec = node.childByFieldName("type");
```

**Why this is better than index-based access:**
- Self-documenting code
- Resilient to grammar changes
- Grammar defines field names in its rules
- Returns `nullptr` if field doesn't exist

---

### ❌ Q4: How to map node types → colors?

**Not applicable** - This is not a syntax highlighting tool.

Already fully answered by **Repo 3 (tree-sitter CLI)** - see `docs/study-doxide-and-tree-sitter-cli.md`.

---

### ❌ Q5: How to output ANSI codes?

**Not applicable** - No terminal output in this tool.

Already fully answered by **Repo 3 (tree-sitter CLI)** - see `docs/study-doxide-and-tree-sitter-cli.md`.

---

## Critical Discovery: Compile-Time Grammar Linking

### The Build Configuration

**From `CMakeLists.txt` lines 23-27:**
```cmake
add_executable(c_language_server 
    app/main.cpp 
    deps/tree-sitter-c/parser.c    # ← Grammar compiled directly in!
    lib/src/stack-graph-tree.cpp 
    lib/src/stack-graph-engine.cpp)
```

**What this means:**
1. The `parser.c` file (75K lines) is compiled as part of the executable
2. No `.so` or `.dll` loading at runtime
3. Grammar is statically linked
4. Simple, fast, no dynamic loading complexity

### The Grammar File Structure

**From `deps/tree-sitter-c/parser.c`:**

```c
// Line 1: Include Tree-sitter parser header
#include <tree_sitter/parser.h>

// Lines 8-17: Grammar metadata
#define LANGUAGE_VERSION 13
#define STATE_COUNT 1459
#define SYMBOL_COUNT 267
// ... more constants ...

// Lines 19-75604: Generated parse tables, lexer, symbol names
enum { sym_identifier = 1, ... };
static const TSSymbol ts_symbol_map[] = { ... };
static const TSParseActionEntry ts_parse_actions[] = { ... };
// ... thousands of lines of generated data ...

// Lines 75605-75635: Exported function
extern const TSLanguage *tree_sitter_c(void) {
    static const TSLanguage language = {
        .version = LANGUAGE_VERSION,
        .symbol_count = SYMBOL_COUNT,
        .parse_table = &ts_parse_table[0][0],
        // ... all the data structures ...
    };
    return &language;
}
```

**This is how Tree-sitter grammars work:**
- Grammar defined in `grammar.js`
- Tree-sitter CLI generates `parser.c` from grammar
- `parser.c` is a complete, self-contained C parser
- Export one function returning `const TSLanguage*`
- Compile it into your app!

### Why This Is Better Than Dynamic Loading

| Aspect | Compile-Time | Runtime (.so/.dll) |
|--------|--------------|-------------------|
| Simplicity | ✅ Just add .c file | ❌ Complex loading logic |
| Portability | ✅ Works everywhere | ❌ Platform-specific |
| Performance | ✅ Static linking | ⚠️ Minor overhead |
| Distribution | ✅ Single executable | ❌ Multiple files |
| Debugging | ✅ Easier | ❌ Harder |
| Flexibility | ❌ Rebuild to add language | ✅ Add at runtime |

**For our use case:** Compile-time is perfect! We know which languages we want to support upfront.

---

## C++ Wrapper Pattern: TSNodeWrapper

### Why Wrap TSNode?

Tree-sitter's C API uses **value types** for `TSNode`:
```c
TSNode node = ts_tree_root_node(tree);  // Value, not pointer
```

This is awkward in C++:
- Can't use RAII easily
- No methods, only free functions
- Verbose API calls

### The Wrapper Solution

**From `stack-graph-tree.cpp` lines 28-88:**

```cpp
struct TSNodeWrapper
{
    TSNode tsnode;  // Store the value type

    TSNodeWrapper(TSNode tsnode) : tsnode(tsnode) {}
    
    // Copy and move constructors
    TSNodeWrapper(TSNodeWrapper &other) : tsnode(other.tsnode) {}
    TSNodeWrapper(TSNodeWrapper &&other) : tsnode(std::move(other.tsnode)) {}

    // Convenient methods wrapping C API
    const char *type() {
        return ts_node_type(this->tsnode);
    }

    shared_ptr<TSNodeWrapper> childByFieldName(const char *name) {
        auto node = ts_node_child_by_field_name(
            this->tsnode, name, strlen(name));
        return ts_node_is_null(node) 
            ? nullptr 
            : shared_ptr<TSNodeWrapper>(new TSNodeWrapper(node));
    }

    uint32_t child_count() {
        return ts_node_child_count(this->tsnode);
    }

    shared_ptr<TSNodeWrapper> child(uint32_t ind) {
        return shared_ptr<TSNodeWrapper>(
            new TSNodeWrapper(ts_node_child(this->tsnode, ind)));
    }

    string text(string &code) {
        return code.substr(this->range().start, 
                          this->range().end - this->range().start);
    }

    Point editorPosition() {
        TSPoint point = ts_node_start_point(this->tsnode);
        return {.line = point.row, .column = point.column};
    }

    Range range() {
        return {.start = ts_node_start_byte(this->tsnode), 
                .end = ts_node_end_byte(this->tsnode)};
    }

    string repr() {
        stringstream ss;
        _do_print_repr(ss, std::move(*this), 0);
        return ss.str();
    }
};
```

### Benefits of This Pattern

**Before (C API):**
```cpp
const char *type = ts_node_type(node);
uint32_t count = ts_node_child_count(node);
TSNode child = ts_node_child(node, 0);
uint32_t start = ts_node_start_byte(node);
TSPoint point = ts_node_start_point(node);
```

**After (C++ Wrapper):**
```cpp
auto type = node.type();
auto count = node.child_count();
auto child = node.child(0);
auto start = node.range().start;
auto point = node.editorPosition();
```

Much cleaner and more idiomatic C++!

---

## Code Patterns for Our Implementation

### Pattern 1: Basic Parse Loop

```cpp
extern "C" TSLanguage *tree_sitter_cpp();
extern "C" TSLanguage *tree_sitter_javascript();
// ... declare more languages ...

class CodeHighlighter {
    TSParser *parser;
    
public:
    CodeHighlighter() {
        parser = ts_parser_new();
    }
    
    ~CodeHighlighter() {
        ts_parser_delete(parser);
    }
    
    void highlightCode(const char* lang_name, const char* source_code) {
        // Select language
        const TSLanguage* lang = nullptr;
        if (strcmp(lang_name, "cpp") == 0) {
            lang = tree_sitter_cpp();
        } else if (strcmp(lang_name, "javascript") == 0) {
            lang = tree_sitter_javascript();
        }
        // ... more languages ...
        
        if (!lang) {
            std::cerr << "Unknown language: " << lang_name << std::endl;
            return;
        }
        
        // Set language and parse
        ts_parser_set_language(parser, lang);
        TSTree *tree = ts_parser_parse_string(
            parser, NULL, source_code, strlen(source_code));
        
        if (!tree) {
            std::cerr << "Parse failed" << std::endl;
            return;
        }
        
        TSNode root = ts_tree_root_node(tree);
        
        // TODO: Run highlight query and output colored text
        
        ts_tree_delete(tree);
    }
};
```

### Pattern 2: TSNode C++ Wrapper (Simplified)

```cpp
class TSNodeWrapper {
    TSNode node;
    
public:
    explicit TSNodeWrapper(TSNode n) : node(n) {}
    
    const char* type() const {
        return ts_node_type(node);
    }
    
    bool isNull() const {
        return ts_node_is_null(node);
    }
    
    TSNodeWrapper childByField(const char* name) const {
        return TSNodeWrapper(
            ts_node_child_by_field_name(node, name, strlen(name)));
    }
    
    uint32_t childCount() const {
        return ts_node_child_count(node);
    }
    
    TSNodeWrapper child(uint32_t index) const {
        return TSNodeWrapper(ts_node_child(node, index));
    }
    
    std::string getText(const char* source) const {
        uint32_t start = ts_node_start_byte(node);
        uint32_t end = ts_node_end_byte(node);
        return std::string(source + start, end - start);
    }
    
    TSPoint startPoint() const {
        return ts_node_start_point(node);
    }
    
    // Allow implicit conversion back to TSNode for C API calls
    operator TSNode() const { return node; }
};
```

### Pattern 3: Error Handling

```cpp
void processNode(TSNodeWrapper node, const char* source) {
    // Skip error nodes
    if (strcmp(node.type(), "ERROR") == 0) {
        return;
    }
    
    // Process valid nodes
    // ...
}
```

### Pattern 4: Field-Based Navigation

```cpp
void processFunctionDefinition(TSNodeWrapper func, const char* source) {
    // Get specific parts of the function
    auto declarator = func.childByField("declarator");
    auto params = func.childByField("parameters");
    auto body = func.childByField("body");
    
    if (!declarator.isNull()) {
        std::cout << "Function name: " 
                  << declarator.getText(source) << std::endl;
    }
    
    // Process each part...
}
```

---

## Comparison: Manual Traversal vs. Queries

### Manual Traversal (This Repo's Approach)

**Pros:**
- Full control over traversal order
- Can maintain complex state through recursion
- No query parsing overhead
- Can skip entire subtrees efficiently
- Good for semantic analysis with cross-references

**Cons:**
- More code to write
- Must handle every node type explicitly
- Harder to maintain when grammar changes
- Need deep knowledge of grammar structure

**Example:**
```cpp
if (strcmp(node.type(), "function_definition") == 0) {
    auto declarator = node.childByFieldName("declarator");
    auto body = node.childByFieldName("body");
    // Custom processing...
}
```

### Query-Based Traversal (Repo 2 & 3's Approach)

**Pros:**
- Declarative - describe what to find
- Grammar maintainers provide queries
- Less code to write
- Easier to add new patterns
- Standard approach for highlighting

**Cons:**
- Less control over traversal
- Query compilation overhead (one-time)
- Can't easily maintain complex state
- Matches might overlap or nest

**Example:**
```c
const char *query = "(function_definition name: (identifier) @name)";
TSQuery *q = ts_query_new(lang, query, ...);
TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, q, root);
// Process matches...
```

### Which Should We Use?

**For syntax highlighting:** Use **queries**!
- Highlight queries already exist for each language
- They're maintained by grammar authors
- Standard semantic names (@keyword, @string, etc.)
- Tree-sitter CLI proves this approach works

**For semantic analysis:** Manual traversal might be better
- Building symbol tables
- Type checking
- Cross-file references
- Complex scope tracking

**Our prototype should use queries** to leverage existing highlight definitions.

---

## Testing Insights

### From `tests/syntax-tree-test.cpp`

**Test structure shows usage patterns:**

```cpp
class StackGraphTest : public ::testing::Test {
protected:
    void SetUp() override {
        // Create parser
        TSParser *parser = ts_parser_new();
        ts_parser_set_language(parser, tree_sitter_c());

        // Parse test code
        const char *source_code = R"raw(
            struct Address {
                string line1;
                string line2;
            };
            // ... more test code ...
        )raw";

        TSTree *tree = ts_parser_parse_string(
            parser, NULL, source_code, strlen(source_code));

        TSNode root_node = ts_tree_root_node(tree);

        // Build semantic graph
        sg_root = stack_graph::build_stack_graph_tree(root_node, source_code);
        
        // Enumerate all nodes for assertions
        _enumerate_nodes(sg_root, all_nodes);
    }

    shared_ptr<StackGraphNode> sg_root;
    vector<shared_ptr<StackGraphNode>> all_nodes;
};

TEST_F(StackGraphTest, ContainsAllNamedScopes) {
    EXPECT_TRUE(_contains(all_nodes, "Address", StackGraphNodeKind::NAMED_SCOPE));
    EXPECT_TRUE(_contains(all_nodes, "Organization", StackGraphNodeKind::NAMED_SCOPE));
    // ... more assertions ...
}
```

**Key insights:**
- Use raw string literals (`R"raw(...)raw"`) for multi-line test code
- Parse once in SetUp, test multiple aspects
- Tree traversal produces a data structure you can query
- No need to re-parse for each test

---

## Dependencies & Build System

### Libraries Used

From `CMakeLists.txt` and code:

1. **Tree-sitter** (`deps/tree-sitter/`)
   - Core parsing library
   - Linked from prebuilt .a/.lib file

2. **Tree-sitter-c** (`deps/tree-sitter-c/parser.c`)
   - C grammar implementation
   - Compiled directly into executable

3. **nlohmann/json** (`deps/json/include`)
   - Header-only JSON library
   - Used for VSCode plugin communication

4. **Google RE2** (`deps/re2/`)
   - Regex engine (fast, safe)
   - Used for pattern matching in code

5. **Google Test** (`deps/google-test/`)
   - Unit testing framework
   - Test discovery and assertions

### Build Configuration

```cmake
# Include directories
include_directories(
    lib/include
    deps/tree-sitter/include      # Tree-sitter C API headers
    deps/tree-sitter-c/include    # Not used? Grammar in parser.c
    deps/json/include             # nlohmann/json
    deps/re2/include              # RE2 regex
)

# Find prebuilt libraries
find_library(TREE_SITTER tree-sitter HINTS deps/tree-sitter/lib)
find_library(RE2 re2 HINTS deps/re2/lib)

# Compile executable with parser.c included
add_executable(c_language_server 
    app/main.cpp 
    deps/tree-sitter-c/parser.c    # ← Grammar compiled in
    lib/src/stack-graph-tree.cpp 
    lib/src/stack-graph-engine.cpp)

# C++17 standard
set_target_properties(c_language_server PROPERTIES CXX_STANDARD 17)

# Link libraries
target_link_libraries(c_language_server 
    Threads::Threads 
    ${TREE_SITTER}    # Core library
    ${RE2})           # Regex engine
```

**For our project, we'd need:**
1. Tree-sitter core library (libtree-sitter)
2. parser.c files for each language (cpp, javascript, etc.)
3. Highlight query files (.scm) for each language
4. ANSI color output (no external library needed)
5. JSON for themes (optional, can hardcode initially)

---

## Performance Considerations

### From README Benchmarks

- **Small files:** Parse time negligible (< 1ms)
- **Medium files:** 50 files/sec = 20ms per file (including I/O)
- **Large codebase:** Linux kernel (60K+ files) in 3-4 minutes

### Implications for Our Use Case

**Our scenario:** Parse code fences as they appear in output
- Typical code fence: 10-100 lines
- Parse time: < 1ms per fence
- No need for caching initially
- Parse on-demand is totally viable

**Optimization opportunities (if needed later):**
- Cache parsed trees if same code repeats
- Reuse TSParser across fences (don't recreate)
- Incremental parsing if code is streamed line-by-line

**Verdict:** Don't optimize prematurely. Parse-on-demand is fine.

---

## What We Learned That's New

### 1. Compile-Time Grammar Linking Is Standard ⭐⭐⭐

**This is the biggest takeaway!**
- No need for complex .so/.dll loading
- Just compile `parser.c` into executable
- One parser.c per language you want
- Declare: `extern "C" TSLanguage *tree_sitter_LANG();`
- Call that function, pass to `ts_parser_set_language()`

**This simplifies our implementation massively!**

### 2. C++ Wrapper Pattern Is Very Useful

- Wraps TSNode value type in a class
- Provides methods instead of free functions
- Makes code much more readable
- RAII-friendly
- We should absolutely use this pattern

### 3. Field Access Is Powerful

- `ts_node_child_by_field_name()` is cleaner than indexed access
- Grammars define field names like "declarator", "body", "parameters"
- Self-documenting code
- Resilient to grammar changes
- Returns null TSNode if field doesn't exist (check with `ts_node_is_null()`)

### 4. Manual Traversal Is Viable

- Don't HAVE to use queries for everything
- Recursive descent with state machine works
- Good for semantic analysis
- But queries are still better for highlighting

### 5. Error Handling Is Simple

- Just check `if (strcmp(node.type(), "ERROR") == 0)` and skip
- Tree-sitter never crashes on bad input
- Can still analyze partially valid code

### 6. Parser Per File Is OK

- Don't need to reuse parser across files
- Create, use, delete per operation is fine
- But DO remember to call `ts_parser_delete()` to avoid leaks!

---

## How This Compares to Previous Repos

### vs. Repo 1 (tree-sitter-issue-2012) - Minimal C Example

| Aspect | Repo 1 | Repo 4 (This) |
|--------|--------|---------------|
| Language | C | C++17 |
| Complexity | Minimal (90 lines) | Production (1000+ lines) |
| Approach | Manual traversal | Manual traversal |
| Grammar | Compile-time | Compile-time |
| Insights | Basic API | C++ patterns, production use |

**Similarity:** Both use compile-time grammar linking!

### vs. Repo 2 (doxide) - C++ Documentation Generator

| Aspect | Repo 2 | Repo 4 (This) |
|--------|--------|---------------|
| Language | C++ | C++17 |
| Approach | Query-based | Manual traversal |
| Grammar | Compile-time | Compile-time |
| Purpose | Extract docs | Semantic analysis |
| Complexity | Medium | High |

**Key difference:** Queries vs. manual traversal!

### vs. Repo 3 (tree-sitter CLI) - Official Highlighter

| Aspect | Repo 3 | Repo 4 (This) |
|--------|--------|---------------|
| Language | Rust | C++17 |
| Purpose | Syntax highlighting | Code navigation |
| Approach | Query-based (highlight queries) | Manual traversal |
| Output | ANSI colored text | JSON (definitions/usages) |
| Grammar | Runtime loading | Compile-time |

**Key difference:** Highlighting vs. semantic analysis, Rust vs. C++!

---

## Answers to P0 Questions - Summary

| Question | Answered? | Source |
|----------|-----------|--------|
| Q1: How to initialize parser? | ✅ CONFIRMED | Lines 47-50 of engine.cpp |
| Q2: How to parse code? | ✅ CONFIRMED | Lines 35-76 of engine.cpp |
| Q3: How to walk syntax tree? | ✅ NEW APPROACH | Lines 178-392 of tree.cpp |
| Q4: Map node types → colors? | ❌ N/A | Not a highlighting tool |
| Q5: Output ANSI codes? | ❌ N/A | Not a highlighting tool |

**Still have everything we need!** Q4 and Q5 fully answered by Repo 3.

---

## Implications for Our Prototype

### What to Adopt from This Repo

1. ✅ **Compile-time grammar linking**
   - Add `parser.c` files to CMakeLists.txt
   - Declare `extern "C" TSLanguage *tree_sitter_cpp();`
   - Call function and pass to `ts_parser_set_language()`

2. ✅ **C++ wrapper class for TSNode**
   - Makes API much cleaner
   - Implement similar to `TSNodeWrapper` (lines 28-88)
   - Add methods for common operations

3. ✅ **Field-based navigation**
   - Use `ts_node_child_by_field_name()` when possible
   - Cleaner than iterating by index
   - Self-documenting code

4. ⚠️ **Manual traversal** (optional)
   - Good to know it's viable
   - But queries are better for highlighting
   - Might use for fence detection logic

5. ✅ **Error handling pattern**
   - Skip ERROR nodes gracefully
   - Don't crash on bad syntax
   - Simple `strcmp` check

### What NOT to Adopt

1. ❌ **Stack graph building**
   - Too complex for our needs
   - We just need highlighting, not semantic analysis

2. ❌ **Cross-file linking**
   - We process one code fence at a time
   - No cross-reference tracking needed

3. ❌ **RE2 regex dependency**
   - We don't need regex for highlighting
   - Tree-sitter handles parsing

4. ❌ **JSON communication**
   - We're not a language server
   - Direct ANSI output to stdout

---

## Code Snippets Ready to Use

### Snippet 1: Multi-Language Support

```cpp
extern "C" {
    TSLanguage *tree_sitter_c();
    TSLanguage *tree_sitter_cpp();
    TSLanguage *tree_sitter_javascript();
    TSLanguage *tree_sitter_python();
    // ... more languages ...
}

const TSLanguage* getLanguage(const char* name) {
    if (strcmp(name, "c") == 0) return tree_sitter_c();
    if (strcmp(name, "cpp") == 0 || strcmp(name, "c++") == 0) 
        return tree_sitter_cpp();
    if (strcmp(name, "javascript") == 0 || strcmp(name, "js") == 0) 
        return tree_sitter_javascript();
    if (strcmp(name, "python") == 0 || strcmp(name, "py") == 0) 
        return tree_sitter_python();
    return nullptr;
}
```

### Snippet 2: Parse with Error Handling

```cpp
TSTree* parseCode(TSParser* parser, const char* lang, const char* code) {
    const TSLanguage* language = getLanguage(lang);
    if (!language) {
        std::cerr << "Unsupported language: " << lang << std::endl;
        return nullptr;
    }
    
    if (!ts_parser_set_language(parser, language)) {
        std::cerr << "Failed to set language" << std::endl;
        return nullptr;
    }
    
    TSTree* tree = ts_parser_parse_string(parser, NULL, code, strlen(code));
    if (!tree) {
        std::cerr << "Parse failed" << std::endl;
        return nullptr;
    }
    
    return tree;
}
```

### Snippet 3: TSNode Wrapper (Essential Methods)

```cpp
class Node {
    TSNode node;
    
public:
    explicit Node(TSNode n) : node(n) {}
    
    bool isNull() const { return ts_node_is_null(node); }
    const char* type() const { return ts_node_type(node); }
    bool isError() const { return strcmp(type(), "ERROR") == 0; }
    
    Node child(uint32_t i) const {
        return Node(ts_node_child(node, i));
    }
    
    Node childByField(const char* name) const {
        return Node(ts_node_child_by_field_name(node, name, strlen(name)));
    }
    
    uint32_t childCount() const { return ts_node_child_count(node); }
    
    uint32_t startByte() const { return ts_node_start_byte(node); }
    uint32_t endByte() const { return ts_node_end_byte(node); }
    
    std::string text(const char* source) const {
        return std::string(source + startByte(), endByte() - startByte());
    }
    
    operator TSNode() const { return node; }  // Implicit conversion
};
```

---

## Potential Issues & Solutions

### Issue 1: Missing Parser Deletion

**Problem:** `stack-graph-engine.cpp` doesn't call `ts_parser_delete(parser)`

**Solution:** Always delete parser when done:
```cpp
TSParser* parser = ts_parser_new();
// ... use parser ...
ts_parser_delete(parser);
```

Or better - use RAII:
```cpp
class Parser {
    TSParser* parser;
public:
    Parser() : parser(ts_parser_new()) {}
    ~Parser() { ts_parser_delete(parser); }
    TSParser* get() { return parser; }
};
```

### Issue 2: Large Parser.c Files

**Problem:** Each `parser.c` is 40K-75K lines, slow to compile

**Solutions:**
1. Use precompiled static libraries instead (compile parser.c separately)
2. Use dynamic loading after all (trade simplicity for compile speed)
3. Accept slow compile time (only rebuild when grammar changes)

**Recommendation:** Start with compile-time, switch if it's too slow.

### Issue 3: Grammar Updates

**Problem:** If grammar changes, must recompile everything

**Solution:**
- Grammars are very stable (rarely change)
- Pin to specific grammar versions in production
- Accept rebuild cost for development

---

## Next Steps for Prototype

Based on this study:

1. ✅ **Use compile-time linking strategy**
   - Download `parser.c` from tree-sitter-cpp repo
   - Add to CMakeLists.txt
   - Declare `extern "C" TSLanguage *tree_sitter_cpp();`

2. ✅ **Create C++ wrapper class**
   - `class Node` wrapping `TSNode`
   - Methods for type(), child(), childByField(), etc.
   - Makes code much cleaner

3. ✅ **Use queries for highlighting** (not manual traversal)
   - Download `queries/highlights.scm` from tree-sitter-cpp
   - Use query API from Repo 2's patterns
   - Output ANSI codes from Repo 3's patterns

4. ✅ **Simple error handling**
   - Check for null tree
   - Skip ERROR nodes
   - Log warnings, continue anyway

5. ✅ **Parser lifecycle**
   - Create parser once, reuse for multiple fences
   - Delete trees after use
   - Use RAII wrapper for safety

---

## Final Assessment

### How Confident Are We Now?

**Very confident!** This is the 4th repo we've studied:
- ✅ Seen Tree-sitter used in C (Repo 1)
- ✅ Seen Tree-sitter used in C++ with queries (Repo 2)
- ✅ Seen official Rust implementation with highlighting (Repo 3)
- ✅ Seen production C++ with compile-time linking (Repo 4)

**All approaches converge on similar patterns:**
- Parser initialization is trivial
- Parsing is one function call
- Tree walking has two approaches (queries or manual)
- Error handling is simple
- Performance is excellent

### What's Still Unknown?

1. ⚠️ **Theme system details** - how to load/parse JSON themes
   - Repo 3 showed structure, but not implementation
   - Can hardcode initially, add JSON later

2. ⚠️ **PTY integration** - where to intercept output
   - Need to study 2shell code
   - Out of scope for this study phase

3. ⚠️ **Fence detection** - state machine for ` ``` ` markers
   - Not Tree-sitter related
   - Simple line-by-line parsing

Everything Tree-sitter related is **fully understood**!

### Should We Build the Prototype?

**YES! Absolutely!** We have everything we need:
- ✅ Know how to initialize parser
- ✅ Know how to parse code
- ✅ Know two ways to walk trees
- ✅ Know how to map nodes to colors (Repo 3)
- ✅ Know how to output ANSI (Repo 3)
- ✅ Know how to link grammars (this repo!)
- ✅ Proven production performance

**No more studying needed for basic prototype.** Time to code!

---

## References

- **Repository:** https://github.com/dgawlik/c-language-server
- **Tree-sitter C docs:** https://tree-sitter.github.io/tree-sitter/using-parsers
- **Related studies:**
  - `docs/study-tree-sitter-issue-2012.md` (Repo 1)
  - `docs/study-doxide-and-tree-sitter-cli.md` (Repos 2-3)
- **CMake docs:** https://cmake.org/cmake/help/latest/
- **Google Test:** https://github.com/google/googletest

---

**Study completed:** 2025-12-15  
**Researcher:** AI Assistant (Claude)  
**Status:** ✅ P0 questions answered, ready for prototype!
