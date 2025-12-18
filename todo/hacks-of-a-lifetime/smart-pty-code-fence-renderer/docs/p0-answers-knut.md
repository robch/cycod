# P0 Questions: KDAB/knut Answers

**Repo:** https://github.com/KDAB/knut  
**Study Session:** 7 of 29

---

## Quick Summary

**Key Finding:** Production C++ wrapper architecture with modern idioms (RAII, move semantics, std::optional, exceptions).

**New Discoveries:**
1. ✨ **Scanner.c file required** (not just parser.c!)
2. ✨ **UTF-16 encoding support** for Unicode
3. ✨ **RAII wrapper pattern** for all resources
4. ✨ **Exception-based error handling**
5. ✨ **Predicate system** in queries (advanced filtering)

**Value:** ⭐⭐⭐⭐⭐ **Architecture gold mine!**

---

## P0 Question 1: How to initialize parser?

### Answer: RAII Wrapper Pattern ✅

**Modern C++ approach (NEW!):**

```cpp
class Parser {
public:
    explicit Parser(TSLanguage *language)
        : m_parser(ts_parser_new())
    {
        if (!m_parser) {
            throw std::runtime_error("Failed to create parser");
        }
        if (!ts_parser_set_language(m_parser, language)) {
            ts_parser_delete(m_parser);
            throw std::runtime_error("Failed to set language");
        }
    }
    
    // No copy (resource is unique)
    Parser(const Parser &) = delete;
    Parser &operator=(const Parser &) = delete;
    
    // Move semantics (transfer ownership)
    Parser(Parser &&other) noexcept
        : m_parser(other.m_parser)
    {
        other.m_parser = nullptr;
    }
    
    Parser &operator=(Parser &&other) noexcept {
        if (this != &other) {
            if (m_parser) {
                ts_parser_delete(m_parser);
            }
            m_parser = other.m_parser;
            other.m_parser = nullptr;
        }
        return *this;
    }
    
    // Automatic cleanup
    ~Parser() {
        if (m_parser) {
            ts_parser_delete(m_parser);
        }
    }

private:
    TSParser *m_parser;
};

// Usage:
extern "C" TSLanguage *tree_sitter_cpp();

Parser parser(tree_sitter_cpp());
// No manual cleanup needed - destructor handles it!
```

**Language registry pattern:**

```cpp
extern "C" {
    TSLanguage *tree_sitter_cpp();
    TSLanguage *tree_sitter_qmljs();
    TSLanguage *tree_sitter_c_sharp();
    TSLanguage *tree_sitter_rust();
}

TSLanguage *Parser::getLanguage(DocumentType type) {
    switch (type) {
    case DocumentType::Cpp:    return tree_sitter_cpp();
    case DocumentType::Qml:    return tree_sitter_qmljs();
    case DocumentType::CSharp: return tree_sitter_c_sharp();
    case DocumentType::Rust:   return tree_sitter_rust();
    default: throw std::invalid_argument("Unknown language");
    }
}
```

### New Insights

1. **RAII pattern** - Automatic cleanup, no manual delete
2. **Move-only semantics** - Can't accidentally copy expensive parser
3. **Exception for errors** - Can't ignore initialization failure
4. **Static language registry** - Clean mapping of types to languages

### Comparison to Previous Repos

| Repo | Pattern | Memory Management |
|------|---------|-------------------|
| Repo 1-4 | Manual C API | Manual delete or smart pointers |
| Repo 5 | Lua FFI | Lua GC |
| Repo 6 | Zig FFI | Zig defer |
| **Repo 7** | **C++ RAII** | **Automatic (destructor)** |

**Verdict:** knut shows the BEST C++ pattern!

---

## P0 Question 2: How to parse code?

### Answer: std::optional + UTF Encoding ✅

**With std::optional (NEW!):**

```cpp
class Parser {
public:
    std::optional<Tree> parseString(
        const std::string &text, 
        const Tree *old_tree = nullptr  // For incremental parsing
    ) const {
        auto tree = ts_parser_parse_string(
            m_parser, 
            old_tree ? old_tree->raw() : nullptr,
            text.c_str(), 
            text.length()
        );
        
        // TreeSitter may return nullptr on catastrophic failure
        return tree ? std::optional<Tree>(tree) : std::nullopt;
    }
};

// Usage:
auto tree = parser.parseString(source);
if (!tree) {
    std::cerr << "Parse failed!\n";
    return;
}

// Safe access via operator->
auto root = tree->rootNode();
```

**With UTF-16 encoding (Qt/Unicode):**

```cpp
// From knut - for QString (UTF-16)
auto tree = ts_parser_parse_string_encoding(
    m_parser, 
    old_tree ? old_tree->m_tree : nullptr,
    (const char *)text.constData(),
    static_cast<uint32_t>(text.size() * sizeof(QChar)),  // Byte count!
    TSInputEncodingUTF16  // ← Explicit encoding
);
```

**For our project (std::string - UTF-8):**

```cpp
// Use default (UTF-8) or be explicit
auto tree = ts_parser_parse_string_encoding(
    m_parser, nullptr,
    text.c_str(), text.length(),
    TSInputEncodingUTF8  // ← Explicit UTF-8
);
```

### New Insights

1. **std::optional clearly expresses "may fail"** - No ambiguous nullptr
2. **UTF encoding matters** - Must match string type!
3. **Incremental parsing** - Pass old_tree for efficient re-parse
4. **Byte count vs char count** - UTF-16 requires `size * sizeof(QChar)`

### Encoding Table

| String Type | Encoding | Size Parameter |
|-------------|----------|----------------|
| `std::string` | UTF-8 | `text.length()` |
| `std::u16string` | UTF-16 | `text.length() * 2` |
| `QString` | UTF-16 | `text.size() * sizeof(QChar)` |

---

## P0 Question 3: How to walk syntax tree?

### Answer: Wrapper Methods + Iterators ✅

**Object-oriented approach (NEW!):**

```cpp
class Node {
public:
    // Children access
    uint32_t namedChildCount() const;
    Node namedChild(uint32_t index) const;
    std::vector<Node> namedChildren() const;
    std::vector<Node> children() const;
    
    // Sibling navigation
    Node nextSibling() const;
    Node previousSibling() const;
    Node nextNamedSibling() const;
    Node previousNamedSibling() const;
    
    // Position
    uint32_t startPosition() const;  // Byte offset
    uint32_t endPosition() const;
    Point startPoint() const;        // Line:column
    Point endPoint() const;
    
    // Properties
    std::string type() const;
    bool isNull() const;
    bool hasError() const;
    
    // Text extraction
    std::string textIn(const std::string &source) const {
        uint32_t start = startPosition();
        uint32_t end = endPosition();
        return source.substr(start, end - start);
    }
};

// Usage - Range-based for loops!
auto root = tree->rootNode();
for (const auto &child : root.namedChildren()) {
    std::cout << child.type() << ": " << child.textIn(source) << "\n";
}
```

**Query-based approach (same as before):**

```cpp
class QueryCursor {
public:
    void execute(
        std::shared_ptr<Query> query, 
        const Node &node,
        std::unique_ptr<Predicates> predicates = nullptr
    );
    
    std::optional<QueryMatch> nextMatch();
    std::vector<QueryMatch> allRemainingMatches();
};

// Usage:
auto query = std::make_shared<Query>(language, queryString);
QueryCursor cursor;
cursor.execute(query, root);

while (auto match = cursor.nextMatch()) {
    for (const auto &capture : match->captures()) {
        // Process capture...
    }
}
```

### New Insights

1. **Convenient wrapper methods** - No raw C API exposure
2. **Range-based for loops** - Modern C++ iteration
3. **std::optional for iteration** - Clear "no more matches" signal
4. **Batch retrieval** - `allRemainingMatches()` for bulk processing

### Comparison: Manual vs Query

| Approach | Best For | Code Complexity |
|----------|----------|-----------------|
| Manual traversal | Simple navigation | Low |
| Field-based access | Semantic navigation | Medium |
| Query-based | Pattern matching | High (powerful) |

---

## P0 Question 4: How to map node types → colors?

### Answer: Same as Before + Predicates ✅

**Basic pattern (unchanged):**

```
1. Query defines captures:
   (string_literal) @string
   ["if" "else"] @keyword

2. Theme maps captures to colors:
   theme["string"] = "\x1b[32m"   // Green
   theme["keyword"] = "\x1b[35m"  // Magenta

3. Lookup during iteration:
   color = theme[capture_name]
```

**NEW: Predicates for filtering (advanced!):**

```scheme
(field_expression
    argument: (_) @arg
    field: (_) @field
    (#eq? @arg "object")  ; ← Only match if @arg text is "object"
) @from
```

**Predicate structure:**

```cpp
struct Predicate {
    std::string name;  // e.g., "eq?", "match?"
    std::vector<std::variant<Capture, std::string>> arguments;
};

// Example: (#eq? @arg "object")
// predicate.name = "eq?"
// predicate.arguments[0] = Capture{name: "arg", id: 0}
// predicate.arguments[1] = "object"
```

**Predicate types:**
- `#eq? @capture "text"` - Exact text match
- `#match? @capture "regex"` - Regex pattern match
- Custom predicates (application-specific)

### New Insights

1. **Predicates filter matches** - More precise than just node types
2. **Capture quantifiers** - One capture name → multiple nodes
   ```scheme
   (parameter_list ["," (parameter_declaration) @arg]+)
   ```
3. **Pattern introspection** - Can query what captures exist

### Do We Need Predicates?

**For basic highlighting:** NO
- Simple node type → color mapping works fine
- Predicates add complexity

**For advanced highlighting:** MAYBE
- Context-sensitive coloring (e.g., "this" vs other identifiers)
- Function names vs other identifiers
- Local vs global variables

**Recommendation:** Start without predicates, add if needed.

---

## P0 Question 5: How to output ANSI codes?

### Answer: Same as Before ✅

**No change from previous repos:**

```cpp
// ANSI color codes (standard)
const char *ANSI_RESET      = "\x1b[0m";
const char *ANSI_RED        = "\x1b[31m";
const char *ANSI_GREEN      = "\x1b[32m";
const char *ANSI_YELLOW     = "\x1b[33m";
const char *ANSI_BLUE       = "\x1b[34m";
const char *ANSI_MAGENTA    = "\x1b[35m";
const char *ANSI_CYAN       = "\x1b[36m";

// Output pattern
std::cout << ANSI_GREEN;           // Start color
std::cout << node.textIn(source);  // Text
std::cout << ANSI_RESET;           // Reset
```

**No new insights - ANSI codes are universal.**

---

## 🎯 Bonus Discovery: Scanner.c Files!

### Critical Build Pattern

**WRONG (incomplete):**
```cmake
add_library(TreeSitterCpp STATIC tree-sitter-cpp/src/parser.c)
```

**CORRECT (complete):**
```cmake
add_library(TreeSitterCpp STATIC 
    tree-sitter-cpp/src/parser.c
    tree-sitter-cpp/src/scanner.c)  # ← DON'T FORGET!
```

**Why it matters:**
- Many grammars need scanner.c for context-sensitive parsing
- C++, C#, Rust, QML all have scanner.c
- Missing scanner.c → parse failures on certain constructs
- Not all grammars have it (check before building)

**Check if scanner exists:**
```bash
ls tree-sitter-cpp/src/scanner.*
# If file exists → include in CMakeLists.txt
```

---

## 🎓 Key Learnings Summary

### Architecture Patterns (NEW!)

1. ✨ **RAII wrappers** - Automatic resource management
2. ✨ **Move-only semantics** - No accidental copies
3. ✨ **std::optional** - Clear nullable results
4. ✨ **Exceptions** - Can't ignore errors
5. ✨ **Smart pointers** - Shared ownership when needed

### Build System (NEW!)

1. ✨ **Scanner.c required** - Many grammars need it
2. ✨ **Always optimize** - Grammar parsing is performance-critical
3. ✨ **Separate wrapper lib** - Clean abstraction layer

### Advanced Features (NEW!)

1. ✨ **UTF-16 encoding** - For Unicode support
2. ✨ **Range-based parsing** - For embedded languages
3. ✨ **Predicates** - Advanced query filtering
4. ✨ **Capture quantifiers** - Multiple matches per capture
5. ✨ **Progress callbacks** - UI responsiveness

---

## 📊 Repo Comparison

### ltreesitter (Repo 5) vs knut (Repo 7)

| Aspect | ltreesitter | knut |
|--------|-------------|------|
| **Focus** | Algorithm | Architecture |
| **Strength** | Decoration table | RAII wrappers |
| **Best For** | Understanding WHAT to do | Understanding HOW to structure |
| **Language** | Lua + C | C++ + Qt |
| **Style** | Procedural | Object-oriented |
| **Value** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

**Verdict:** BOTH are essential!
- Use **ltreesitter** for ALGORITHM (decoration table)
- Use **knut** for ARCHITECTURE (C++ wrappers)

---

## 🚀 What Changed from Previous Sessions

### Before Repo 7

**Understanding:**
- Tree-sitter is C API, wrap simply
- Just link parser.c
- Use smart pointers for cleanup
- Return nullptr for errors

**Approach:**
- Basic wrappers around C functions
- Manual lifetime management
- Error code checking

### After Repo 7

**Understanding:**
- Tree-sitter needs COMPREHENSIVE wrapper
- Link BOTH parser.c AND scanner.c
- Use RAII + move semantics
- Exceptions for errors, std::optional for nullable

**Approach:**
- Full OOP wrapper layer
- Automatic lifetime management (RAII)
- Modern C++ idioms throughout
- Production-quality error handling

---

## ✅ Confirmation Status

All P0 questions answered for the **7th time**.

| Question | Times Confirmed | Latest Answer |
|----------|----------------|---------------|
| Q1: Init parser | 7 | RAII wrapper pattern ✨ |
| Q2: Parse code | 7 | std::optional + UTF encoding ✨ |
| Q3: Walk tree | 7 | Wrapper methods + queries |
| Q4: Map colors | 7 | Same + predicates (advanced) |
| Q5: ANSI output | 7 | Same (no change) |

**New critical discovery:** Scanner.c files! ✨

---

## 🎯 Next Action

**❌ DON'T STUDY MORE REPOS!**

We now have:
- ✅ **THE ALGORITHM** (Repo 5 - ltreesitter)
- ✅ **THE ARCHITECTURE** (Repo 7 - knut)
- ✅ Build patterns (Repo 4, 7)
- ✅ Error handling (Repo 7)
- ✅ Memory management (Repo 7)

**This is COMPLETE. Time to BUILD!**

---

## 📚 Quick Reference

**For algorithm:** Read `external/ltreesitter/examples/c-highlight.lua`  
**For architecture:** Read `external/knut/src/treesitter/parser.h`  
**For build:** Read `external/knut/3rdparty/CMakeLists.txt` (lines 65-127)  
**For usage:** Read `external/knut/tests/tst_treesitter.cpp`

---

**Status:** Study phase COMPLETE (7 repos studied)  
**Confidence:** 100%  
**Recommendation:** 🚀 **BUILD THE PROTOTYPE NOW!**
