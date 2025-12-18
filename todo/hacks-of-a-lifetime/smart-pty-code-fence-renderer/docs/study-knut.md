# Study Report: KDAB/knut - Tree-sitter Production C++ Wrapper

**Date:** 2025-01-XX  
**Repo:** https://github.com/KDAB/knut  
**Cloned to:** `external/knut/`  
**Language:** C++17 + Qt 6  
**Study Session:** 7 of 29 repos  

---

## 🌟 Executive Summary

**What it is:** Professional code transformation/migration tool by KDAB (Qt consultancy) with **comprehensive C++ wrapper library** for Tree-sitter.

**Why it matters:** This is the FIRST repo showing production-quality C++ **architecture** for Tree-sitter integration. While ltreesitter (Repo 5) showed us THE ALGORITHM, knut shows us THE ARCHITECTURE.

**Key Discovery:** Complete C++ wrapper with modern idioms:
- RAII resource management
- Move-only semantics  
- Exception-based error handling
- std::optional for nullable results
- Template specializations for std::hash

**Study Value:** ⭐⭐⭐⭐⭐ **GOLDMINE for architecture patterns!**

---

## 📋 What is Knut?

From the README:

> Knut is an automation tool for code transformation using scripts. The main use case is for migration, but it could be used elsewhere.

**Supported Languages:**
- C/C++ (full support: TreeSitter + Code Items + LSP)
- C#, Rust (TreeSitter only)
- QML (TreeSitter + Code Items + File Viewer)
- JSON, Qt .ts/.ui files, MFC rc files (various support levels)

**Key Features:**
- Script-based automation (JavaScript/QML)
- Tree-sitter for syntax understanding
- LSP for semantic analysis
- GUI and CLI interfaces

**Production Status:** Active KDAB project, professionally maintained

---

## 🎯 Tree-sitter Integration Architecture

### File Structure

```
src/treesitter/
├── CMakeLists.txt       # Build configuration
├── languages.h          # Language function declarations
├── parser.h / .cpp      # Parser wrapper
├── tree.h / .cpp        # Tree wrapper
├── node.h / .cpp        # Node wrapper
├── query.h / .cpp       # Query + QueryCursor wrappers
├── tree_cursor.h / .cpp # TreeCursor wrapper
└── predicates.h / .cpp  # Predicate evaluation

3rdparty/
├── tree-sitter/         # Core library
├── tree-sitter-cpp/     # C++ grammar
├── tree-sitter-qmljs/   # QML/JS grammar
├── tree-sitter-c-sharp/ # C# grammar
└── tree-sitter-rust/    # Rust grammar
```

**Key insight:** Complete abstraction layer - user code never calls C API directly!

---

## 🔥 Critical Discovery #1: Scanner.c Files!

### The Build Pattern

Previous repos showed:
```cmake
# Incomplete!
add_library(TreeSitterCpp STATIC tree-sitter-cpp/src/parser.c)
```

**KDAB shows the COMPLETE pattern:**
```cmake
# CORRECT - Both files needed!
add_library(TreeSitterCpp STATIC 
    tree-sitter-cpp/src/parser.c
    tree-sitter-cpp/src/scanner.c)  # ← DON'T FORGET THIS!
target_link_libraries(TreeSitterCpp PRIVATE TreeSitter)
```

**Why scanner.c matters:**
- Many grammars need context-sensitive parsing
- scanner.c provides lexical context tracking
- Not all grammars have it (but C/C++/Rust/C# do!)
- Missing scanner.c → parse failures on certain constructs

**Languages that need scanner.c:**
- ✅ C++ (has scanner.c)
- ✅ C# (has scanner.c)  
- ✅ Rust (has scanner.c)
- ✅ QML/JS (has scanner.c)

**Check before compiling:** `ls tree-sitter-LANG/src/scanner.*`

---

## 🔥 Critical Discovery #2: UTF-16 Encoding Support

### The Problem

UTF-8 encoding (what we've seen so far):
```cpp
// Works for ASCII, breaks for emoji/Unicode
ts_parser_parse_string(parser, NULL, 
    source.c_str(), source.length(), 
    // Implicit UTF-8
);
```

**The issue:** Multi-byte characters cause byte offset mismatches!

### KDAB's Solution

```cpp
auto tree = ts_parser_parse_string_encoding(
    m_parser, 
    old_tree ? old_tree->m_tree : nullptr,
    (const char *)text.constData(),
    static_cast<uint32_t>(text.size() * sizeof(QChar)),  // ← Size in BYTES!
    TSInputEncodingUTF16  // ← Explicit encoding!
);
```

**Why this matters:**
- Qt uses UTF-16 (QChar = 16 bits)
- Byte offsets must account for multi-byte chars
- `TSInputEncodingUTF8` vs `TSInputEncodingUTF16`

**For our project (std::string):**
- If using `std::string` (UTF-8) → Use default or `TSInputEncodingUTF8`
- If using `std::u16string` (UTF-16) → Use `TSInputEncodingUTF16`
- **CRITICAL:** Match encoding to string type!

---

## 🏗️ Architecture Pattern: RAII Wrapper Classes

### The Pattern (All classes follow this)

```cpp
class Parser {
public:
    // Constructor acquires resource
    Parser(TSLanguage *language)
        : m_parser(ts_parser_new())
    {
        ts_parser_set_language(m_parser, language);
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

    Parser &operator=(Parser &&other) noexcept
    {
        Parser(std::move(other)).swap(*this);
        return *this;
    }

    // Destructor releases resource
    ~Parser()
    {
        if (m_parser) {
            ts_parser_delete(m_parser);
        }
    }

    // Swap for move-assign implementation
    void swap(Parser &other) noexcept
    {
        std::swap(m_parser, other.m_parser);
    }

private:
    TSParser *m_parser;
};
```

**Why this is perfect:**
- ✅ Automatic cleanup (RAII)
- ✅ Move-only prevents copying expensive resources
- ✅ No manual memory management in user code
- ✅ Exception-safe (destructor always runs)
- ✅ Standard C++ idiom (works with STL containers)

**Classes using this pattern:**
- `Parser` (wraps TSParser)
- `Tree` (wraps TSTree)
- `Query` (wraps TSQuery)
- `QueryCursor` (wraps TSQueryCursor)

**Node is different:** Value type (lightweight, copyable) because TSNode is just a struct with pointers.

---

## 🎯 Modern C++ Patterns

### 1. std::optional for Nullable Results

**OLD WAY (C style):**
```cpp
TSTree *tree = ts_parser_parse_string(...);
if (!tree) {
    // Handle error
}
```

**KDAB WAY (modern C++):**
```cpp
std::optional<Tree> Parser::parseString(const QString &text, const Tree *old_tree) const
{
    auto tree = ts_parser_parse_string_encoding(...);
    // TreeSitter may return nullptr
    return tree ? Tree(tree) : std::optional<Tree>{};
}

// Usage:
auto tree = parser.parseString(source);
if (!tree) {
    // Handle parse failure
}
auto root = tree->rootNode();  // Safe access via operator->
```

**Benefits:**
- Clear intent: "may not have a value"
- Prevents nullptr dereference
- Composable with other std APIs
- Self-documenting code

### 2. Exception-Based Error Handling

**Query construction:**
```cpp
struct Error {
    uint32_t utf8_offset;
    QString description;
};

// throws a Query::Error if the query is ill-formed
Query::Query(const TSLanguage *language, const QString &query)
{
    m_utf8_text = query.toUtf8();
    uint32_t error_offset;
    TSQueryError error_type;
    m_query = ts_query_new(language, m_utf8_text.data(), 
                          m_utf8_text.size(), 
                          &error_offset, &error_type);
    
    if (!m_query) {
        static QStringList errorNames {
            "No Error", "Syntax Error", "Invalid node type",
            "Invalid field", "Capture Error", "Structure Error"
        };
        
        auto description = error_type < errorNames.size() 
            ? errorNames.at(error_type) 
            : "Unknown Error";
            
        throw Error {
            .utf8_offset = error_offset,
            .description = description,
        };
    }
}
```

**Usage:**
```cpp
try {
    auto query = std::make_shared<treesitter::Query>(language, queryString);
    // Use query...
} catch (treesitter::Query::Error &error) {
    spdlog::error("Query error: {} at offset {}", 
                  error.description, error.utf8_offset);
    return;
}
```

**Benefits:**
- Errors can't be ignored
- Clean separation of happy path from error handling
- Rich error information (offset + description)
- Natural C++ idiom

### 3. Smart Pointers for Shared Ownership

**Query lifecycle management:**
```cpp
class QueryCursor {
private:
    // Query must outlive cursor - use shared_ptr!
    std::shared_ptr<Query> m_query;
    TSQueryCursor *m_cursor;
};

// Usage:
auto query = std::make_shared<treesitter::Query>(language, queryString);
QueryCursor cursor;
cursor.execute(query, root, predicates);  // cursor keeps query alive
```

**Benefits:**
- Automatic lifetime management
- Query stays alive as long as any cursor uses it
- No dangling pointers
- Thread-safe reference counting

### 4. Template Specialization for Integration

**Making Node hashable:**
```cpp
template <>
struct std::hash<treesitter::Node>
{
    std::size_t operator()(const treesitter::Node &node) const noexcept
    {
        std::size_t result = 0;
        for (unsigned int i : node.m_node.context) {
            result ^= std::hash<uint32_t>{}(i);
        }
        return result ^ std::hash<const void *>{}(node.m_node.id) 
                      ^ std::hash<const TSTree *>{}(node.m_node.tree);
    }
};
```

**Enables:**
```cpp
std::unordered_map<treesitter::Node, ColorInfo> nodeColors;
std::unordered_set<treesitter::Node> visitedNodes;
```

**Benefit:** Node can be used in hash-based containers!

---

## 🔧 Advanced Tree-sitter Features

### 1. Range-Based Parsing

**The API:**
```cpp
bool Parser::setIncludedRanges(const QList<Range> &ranges)
{
    return ts_parser_set_included_ranges(m_parser, ranges.data(), ranges.size());
}
```

**Use case:** Parse only specific byte ranges (e.g., C++ embedded in QML):
```cpp
// Parse only C++ blocks in QML file
QList<Range> cppRanges = findCppBlocks(qmlContent);
parser.setIncludedRanges(cppRanges);
auto tree = parser.parseString(qmlContent);
// tree contains only C++ syntax nodes
```

**Requirements (from api.h):**
- Ranges must be ordered (earliest to latest)
- Ranges must not overlap
- `ranges[i].end_byte <= ranges[i+1].start_byte`

**For our project:** Not needed (we parse complete code fences), but good to know!

### 2. Incremental Parsing

**The pattern:**
```cpp
// First parse
auto tree1 = parser.parseString(source1);

// ... user edits code ...

// Re-parse efficiently (reuses unchanged subtrees)
auto tree2 = parser.parseString(source2, &tree1);
```

**Benefits:**
- Much faster for small changes
- Tree-sitter reuses unchanged subtrees
- Essential for real-time editing

**For our project:** Not needed (one-shot parsing of code fences), but demonstrates capability.

### 3. Predicates in Queries

**Discovered in knut! Previous repos didn't show this!**

**Query with predicates:**
```scheme
(field_expression
    argument: (_) @arg
    field: (_) @field
    (#eq? @arg "object")     ; ← Predicate: filter by text match
) @from
```

**Predicate types:**
- `#eq? @capture "text"` - Exact text match
- `#match? @capture "regex"` - Regex match
- Custom predicates (knut implements these)

**Structure:**
```cpp
struct Predicate {
    QString name;  // e.g., "eq?", "match?"
    QVector<std::variant<Capture, QString>> arguments;
};
```

**Usage pattern:**
```cpp
auto query = std::make_shared<Query>(language, queryString);
auto patterns = query->patterns();

for (const auto &pattern : patterns) {
    for (const auto &predicate : pattern.predicates) {
        // predicate.name == "eq?"
        // predicate.arguments[0] == Capture("arg")
        // predicate.arguments[1] == "object"
    }
}
```

**For highlighting:** Probably not needed (simple node type → color mapping), but powerful for advanced queries!

### 4. Capture Quantifiers

**Also discovered in knut!**

**Query with quantifier:**
```scheme
(parameter_list
    ["," (parameter_declaration) @arg]+)  ; ← + means "one or more"
```

**Result:**
- Single match can have MULTIPLE captures with same name
- Each `@arg` capture added to match

**Test example (from tst_treesitter.cpp):**
```cpp
// Query matches parameter lists
auto matches = cursor.allRemainingMatches();

// Each match can have multiple @arg captures
QCOMPARE(matches[0].captures().size(), 2);  // Two parameters
QCOMPARE(matches[3].captures().size(), 6);  // Six parameters
```

**Quantifiers:**
- `@capture+` - One or more
- `@capture*` - Zero or more  
- `@capture?` - Zero or one

**For highlighting:** Not needed (we capture each node once), but explains why `capturesNamed()` returns a vector!

---

## 📚 API Reference: Key Methods

### Parser Class

```cpp
class Parser {
public:
    // Construct with language
    Parser(TSLanguage *language);
    
    // Parse source code
    std::optional<Tree> parseString(
        const QString &text, 
        const Tree *old_tree = nullptr  // For incremental parsing
    ) const;
    
    // Set ranges to parse (for embedded languages)
    bool setIncludedRanges(const QList<Range> &ranges);
    
    // Get current language
    const TSLanguage *language() const;
    
    // Static helper: map document type to language
    static TSLanguage *getLanguage(Core::Document::Type type);
};
```

### Tree Class

```cpp
class Tree {
public:
    // Get root node
    Node rootNode() const;
    
    // ... (knut wraps minimal TSTree API)
};
```

### Node Class

```cpp
class Node {
public:
    // Type information
    QString type() const;
    const char *rawType() const;
    
    // Children access
    uint32_t namedChildCount() const;
    Node namedChild(uint32_t index) const;
    QVector<Node> namedChildren() const;
    QVector<Node> children() const;
    
    // Sibling navigation
    Node nextSibling() const;
    Node previousSibling() const;
    Node nextNamedSibling() const;
    Node previousNamedSibling() const;
    
    // Position information
    uint32_t startPosition() const;  // Byte offset
    uint32_t endPosition() const;
    Point startPoint() const;        // Line:column
    Point endPoint() const;
    
    // Node properties
    bool isExtra() const;
    bool isMissing() const;
    bool isNamed() const;
    bool isNull() const;
    bool hasError() const;
    
    // Text extraction
    QString textIn(const QString &source) const;
    QString textExcept(const QString &source, 
                      const QVector<QString> &nodeTypes) const;
    
    // Tree navigation
    Node descendantForRange(uint32_t left, uint32_t right) const;
    Node parent() const;
    
    // Field access
    QString fieldNameForChild(const Node &child) const;
};
```

### Query Class

```cpp
class Query {
public:
    struct Capture {
        QString name;
        uint32_t id;
    };
    
    struct Error {
        uint32_t utf8_offset;
        QString description;
    };
    
    // throws Query::Error if invalid
    Query(const TSLanguage *language, const QString &query);
    
    // Query introspection
    QVector<Pattern> patterns() const;
    QVector<Capture> captures() const;
    Capture captureAt(uint32_t index) const;
};
```

### QueryCursor Class

```cpp
class QueryCursor {
public:
    QueryCursor();
    
    // Execute query on node
    void execute(
        std::shared_ptr<Query> query, 
        const Node &node,
        std::unique_ptr<Predicates> &&predicates
    );
    
    // Get matches one at a time
    std::optional<QueryMatch> nextMatch();
    
    // Get all remaining matches at once
    QVector<QueryMatch> allRemainingMatches();
    
    // For long queries in UI apps
    void setProgressCallback(std::function<void()> callback);
};
```

### QueryMatch Class

```cpp
class QueryMatch {
public:
    struct Capture {
        uint32_t id;
        Node node;
    };
    
    uint32_t id() const;
    uint32_t patternIndex() const;
    
    QVector<Capture> captures() const;
    QVector<Capture> capturesWithId(uint32_t id) const;
    QVector<Capture> capturesNamed(const QString &name) const;
    
    std::shared_ptr<Query> query() const;
};
```

---

## 🏗️ Build System Patterns

### Grammar Library Pattern

```cmake
# Core Tree-sitter library
add_library(TreeSitter STATIC tree-sitter/lib/src/lib.c)
target_include_directories(TreeSitter PUBLIC tree-sitter/lib/include)
target_include_directories(TreeSitter PRIVATE tree-sitter/lib/src)

# IMPORTANT: Always optimize tree-sitter!
enable_optimizations(TreeSitter)

# C++ Grammar (note: BOTH parser.c and scanner.c!)
add_library(TreeSitterCpp STATIC 
    tree-sitter-cpp/src/parser.c
    tree-sitter-cpp/src/scanner.c)
target_link_libraries(TreeSitterCpp PRIVATE TreeSitter)
enable_optimizations(TreeSitterCpp)

# Repeat for each language...
```

### Wrapper Library Pattern

```cmake
project(knut-treesitter LANGUAGES CXX)

set(PROJECT_SOURCES
    node.cpp node.h
    parser.cpp parser.h
    query.cpp query.h
    tree.cpp tree.h
    tree_cursor.cpp tree_cursor.h
    predicates.cpp predicates.h)

add_library(${PROJECT_NAME} STATIC ${PROJECT_SOURCES})

target_link_libraries(
    ${PROJECT_NAME}
    TreeSitter           # Core library
    TreeSitterCpp        # C++ grammar
    TreeSitterQmlJs      # QML grammar
    TreeSitterCSharp     # C# grammar
    TreeSitterRust       # Rust grammar
    Qt::Core             # For QString, QVector
)
```

**Key insight:** Separate library for wrappers, links against grammar libraries.

---

## 💡 Usage Patterns from Knut Codebase

### Pattern 1: Lazy Parsing

**Don't parse until needed:**
```cpp
class TreeSitterHelper {
private:
    std::optional<treesitter::Parser> m_parser;
    std::optional<treesitter::Tree> m_tree;
    
public:
    std::optional<treesitter::Tree> &syntaxTree() {
        if (!m_tree) {  // ← Parse on first access
            auto &parser = this->parser();
            m_tree = parser.parseString(document->text());
            if (!m_tree) {
                spdlog::warn("Parse failed!");
            }
        }
        return m_tree;
    }
};
```

**Why:** Saves time if document opened but never analyzed.

### Pattern 2: Query Construction with Error Handling

```cpp
std::shared_ptr<treesitter::Query> constructQuery(const QString &queryString)
{
    try {
        return std::make_shared<treesitter::Query>(
            parser().language(), 
            queryString
        );
    } catch (treesitter::Query::Error &error) {
        spdlog::error("Query error: {} at offset {}", 
                     error.description, error.utf8_offset);
        return {};  // Return empty shared_ptr
    }
}
```

**Why:** Isolate error handling, return nullable result.

### Pattern 3: Finding Nodes in Range

```cpp
QList<treesitter::Node> nodesInRange(uint32_t start, uint32_t end)
{
    QList<treesitter::Node> nodesToVisit;
    QList<treesitter::Node> result;
    
    nodesToVisit.push_back(tree->rootNode());
    
    while (!nodesToVisit.isEmpty()) {
        auto node = nodesToVisit.takeLast();
        
        if (node.startPosition() >= start && node.endPosition() <= end) {
            result.push_back(node);  // Fully contained
        } else if (node.startPosition() < end && node.endPosition() > start) {
            nodesToVisit.append(node.children());  // Overlaps, check children
        }
    }
    
    return result;
}
```

**Why:** Efficient range search without checking all nodes.

### Pattern 4: Query Execution

```cpp
void highlightCode(const Node &root, const QString &source)
{
    // Load query
    auto query = constructQuery(highlightQueryString);
    if (!query) return;
    
    // Execute query
    treesitter::QueryCursor cursor;
    cursor.execute(query, root, nullptr);  // nullptr = no predicates
    
    // Process matches
    while (auto match = cursor.nextMatch()) {
        for (const auto &capture : match->captures()) {
            auto captureName = query->captureAt(capture.id).name;
            auto node = capture.node;
            
            // Get color for capture name
            auto color = theme[captureName];
            
            // Output colored text
            outputColored(node.textIn(source), color);
        }
    }
}
```

**Clean, self-documenting flow.**

---

## 🎓 What Knut Teaches Us

### About C++ Wrapper Design

1. **RAII for resource management** - No manual cleanup
2. **Move-only for expensive resources** - Parser, Tree, Query
3. **Value types for lightweight data** - Node (just pointers)
4. **std::optional for nullable results** - Clear, safe
5. **Exceptions for errors** - Don't ignore problems
6. **Smart pointers for shared ownership** - Query lifecycle
7. **Template specialization for integration** - std::hash<Node>

### About Tree-sitter Usage

1. **Scanner.c is required** - Many grammars need it
2. **UTF-16 encoding matters** - For international code
3. **Optimize grammar builds** - Performance-critical
4. **Lazy parsing** - Don't parse until needed
5. **Range-based parsing exists** - For embedded languages
6. **Predicates filter matches** - Advanced query features
7. **Capture quantifiers work** - Multiple captures per name

### About Production Patterns

1. **Separate wrapper library** - Clean abstraction
2. **Comprehensive error handling** - Catch all failure modes
3. **Progress callbacks** - For UI responsiveness
4. **Rich API surface** - Convenience methods
5. **Testing strategy** - Extensive unit tests

---

## 📊 Comparison to Previous Repos

| Aspect | ltreesitter (Repo 5) | knut (Repo 7) |
|--------|---------------------|---------------|
| **Focus** | Algorithm (what to do) | Architecture (how to structure) |
| **Language** | Lua + C | C++ + Qt |
| **Style** | Procedural | Object-oriented |
| **Key Strength** | Decoration table algorithm | RAII wrapper classes |
| **Error Handling** | Return codes | Exceptions |
| **Memory Management** | Manual | Automatic (RAII) |
| **API Exposure** | Direct C API | Full C++ wrapper |
| **Best For** | Understanding algorithm | Production code structure |
| **Value** | ⭐⭐⭐⭐⭐ THE ALGORITHM | ⭐⭐⭐⭐⭐ THE ARCHITECTURE |

**Conclusion:** Both repos are essential!
- **ltreesitter:** Shows WHAT to do (decoration table)
- **knut:** Shows HOW to structure code (C++ wrappers)

---

## 🚀 What to Copy for Our Project

### Must Copy (Essential)

1. **RAII wrapper pattern** for Parser, Tree, Query
2. **Move-only semantics** for resource classes
3. **std::optional for nullable results** (parseString)
4. **Exception-based error handling** (Query construction)
5. **Scanner.c inclusion** in CMake builds
6. **Optimization flags** for grammar builds

### Should Copy (Recommended)

1. **Language registry pattern** (enum → language function)
2. **Lazy parsing** (don't parse until needed)
3. **Smart pointers for queries** (shared ownership)
4. **Comprehensive Node wrapper** (convenience methods)

### Nice to Have (Optional)

1. **std::hash specialization** (if using unordered containers)
2. **Range-based parsing** (if supporting embedded languages)
3. **Progress callbacks** (if parsing large files)
4. **Predicate system** (if doing complex filtering)

---

## ⚠️ What NOT to Copy

1. **Qt dependencies** (QString, QVector) → Use std::string, std::vector
2. **Full API surface** → Start minimal, add as needed
3. **GUI integration** → We're terminal-focused
4. **LSP integration** → Not needed for highlighting
5. **Predicates** → Too complex for basic highlighting

---

## 🔍 Code Snippets Ready to Adapt

### Snippet 1: Parser Wrapper (Core Pattern)

```cpp
// Adapted from knut/src/treesitter/parser.h
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
    
    Parser(const Parser &) = delete;
    Parser(Parser &&other) noexcept
        : m_parser(other.m_parser)
    {
        other.m_parser = nullptr;
    }
    
    Parser &operator=(const Parser &) = delete;
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
    
    ~Parser() {
        if (m_parser) {
            ts_parser_delete(m_parser);
        }
    }
    
    std::optional<Tree> parseString(const std::string &text) const {
        auto tree = ts_parser_parse_string(
            m_parser, nullptr, 
            text.c_str(), text.length()
        );
        return tree ? std::optional<Tree>(tree) : std::nullopt;
    }

private:
    TSParser *m_parser;
};
```

### Snippet 2: Query Wrapper with Error Handling

```cpp
// Adapted from knut/src/treesitter/query.h
class Query {
public:
    struct Error {
        uint32_t offset;
        std::string description;
    };
    
    // Throws Query::Error if invalid
    Query(const TSLanguage *language, const std::string &queryString)
        : m_query(nullptr)
    {
        uint32_t error_offset;
        TSQueryError error_type;
        
        m_query = ts_query_new(
            language, 
            queryString.c_str(), 
            queryString.length(),
            &error_offset, 
            &error_type
        );
        
        if (!m_query) {
            static const char *errorNames[] = {
                "No Error", "Syntax Error", "Invalid node type",
                "Invalid field", "Capture Error", "Structure Error"
            };
            
            const char *desc = error_type < 6 
                ? errorNames[error_type] 
                : "Unknown Error";
                
            throw Error { error_offset, desc };
        }
    }
    
    Query(const Query &) = delete;
    Query(Query &&other) noexcept : m_query(other.m_query) {
        other.m_query = nullptr;
    }
    
    ~Query() {
        if (m_query) {
            ts_query_delete(m_query);
        }
    }
    
    TSQuery *raw() const { return m_query; }

private:
    TSQuery *m_query;
};
```

### Snippet 3: Language Registry

```cpp
// Adapted from knut/src/treesitter/parser.cpp
extern "C" {
    TSLanguage *tree_sitter_cpp();
    TSLanguage *tree_sitter_javascript();
    TSLanguage *tree_sitter_python();
}

enum class Language {
    Cpp,
    JavaScript,
    Python
};

TSLanguage *getLanguage(Language lang) {
    switch (lang) {
        case Language::Cpp:        return tree_sitter_cpp();
        case Language::JavaScript: return tree_sitter_javascript();
        case Language::Python:     return tree_sitter_python();
        default: 
            throw std::invalid_argument("Unknown language");
    }
}

// Language detection from fence marker
std::optional<Language> detectLanguage(const std::string &marker) {
    static const std::unordered_map<std::string, Language> langMap = {
        {"cpp", Language::Cpp},
        {"c++", Language::Cpp},
        {"js", Language::JavaScript},
        {"javascript", Language::JavaScript},
        {"py", Language::Python},
        {"python", Language::Python},
    };
    
    auto it = langMap.find(marker);
    return it != langMap.end() ? std::optional(it->second) : std::nullopt;
}
```

### Snippet 4: CMake Pattern (Adapted)

```cmake
# Core Tree-sitter library
add_library(TreeSitter STATIC 
    external/tree-sitter/lib/src/lib.c)
target_include_directories(TreeSitter 
    PUBLIC external/tree-sitter/lib/include
    PRIVATE external/tree-sitter/lib/src)

# Always optimize (performance critical)
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_compile_options(TreeSitter PRIVATE 
        $<$<CXX_COMPILER_ID:GNU,Clang>:-O2>
        $<$<CXX_COMPILER_ID:MSVC>:/O2>)
endif()

# C++ grammar (BOTH files!)
add_library(TreeSitterCpp STATIC 
    external/tree-sitter-cpp/src/parser.c
    external/tree-sitter-cpp/src/scanner.c)  # DON'T FORGET!
target_link_libraries(TreeSitterCpp PRIVATE TreeSitter)

# Our wrapper library
add_library(TreeSitterWrapper STATIC
    src/parser.cpp src/parser.h
    src/tree.cpp src/tree.h
    src/query.cpp src/query.h)
target_link_libraries(TreeSitterWrapper
    PUBLIC TreeSitter TreeSitterCpp)
```

---

## 📋 Files Worth Deep Reading

### Core Wrapper Implementation
- ✅ `src/treesitter/parser.cpp` - RAII pattern reference
- ✅ `src/treesitter/query.cpp` - Exception handling pattern
- ✅ `src/treesitter/node.h` - Complete API surface
- ✅ `3rdparty/CMakeLists.txt` (lines 65-127) - Build patterns

### Usage Examples
- ✅ `tests/tst_treesitter.cpp` - Comprehensive test suite
- ✅ `src/core/codedocument_p.cpp` - Production usage
- ✅ `tests/tst_cppdocument_treesitter.cpp` - C++ document tests

### Build System
- ✅ `src/treesitter/CMakeLists.txt` - Wrapper library
- ✅ `3rdparty/CMakeLists.txt` - Grammar compilation

---

## 🎯 Key Takeaways

### For Algorithm
- **ltreesitter (Repo 5) is still THE reference** - Decoration table algorithm
- knut doesn't show highlighting (focuses on code transformation)
- Use ltreesitter's c-highlight.lua as algorithm guide

### For Architecture
- **knut is THE reference for C++ structure**
- RAII wrappers for all resources
- Modern C++ idioms throughout
- Production-quality error handling
- Comprehensive test suite

### For Build System
- **Both parser.c AND scanner.c required**
- Always optimize grammar builds
- Separate wrapper library pattern
- Multiple language support is straightforward

---

## ⚡ Session Impact

**Value Added:** 🔥🔥🔥🔥🔥 **MASSIVE!**

**New Discoveries:**
1. ✨ Scanner.c file requirement (CRITICAL!)
2. ✨ UTF-16 encoding support pattern
3. ✨ RAII wrapper architecture
4. ✨ Predicate system in queries
5. ✨ Capture quantifiers
6. ✨ Range-based parsing capability
7. ✨ Production build optimization strategy

**Compared to Repo 6 (zig-tree-sitter):**
- Repo 6: ZERO value (just FFI bindings)
- Repo 7: MASSIVE value (architecture + patterns)
- **Proof:** Not all repos are equal!

**Relation to Repo 5 (ltreesitter):**
- Repo 5: THE ALGORITHM (decoration table)
- Repo 7: THE ARCHITECTURE (C++ wrappers)
- **Together:** Complete picture!

---

## 🎓 Handoff Notes

### What Changed from Previous Understanding

**Before Repo 7:**
- Tree-sitter is C API, wrap it simply
- Just link parser.c
- Manual memory management with smart pointers
- Return nullptr for errors

**After Repo 7:**
- Tree-sitter needs COMPREHENSIVE wrapper
- Link BOTH parser.c AND scanner.c
- RAII + move semantics for resources
- Exceptions for errors, std::optional for nullable results
- UTF-16 encoding matters for Unicode
- Predicates and quantifiers in queries (advanced features)

### Architecture Decision

**Our implementation should:**
1. Copy RAII wrapper pattern from knut
2. Copy decoration table algorithm from ltreesitter
3. Use std::string (not QString) for portability
4. Start minimal, expand as needed
5. Include scanner.c in builds

**Best of both worlds:**
- knut's architecture (HOW)
- ltreesitter's algorithm (WHAT)

---

## 🚦 Should We Study More Repos?

**❌ ABSOLUTELY NOT!**

**We now have:**
1. ✅ Basic patterns (Repo 1)
2. ✅ Query-based traversal (Repo 2)
3. ✅ Official highlighter (Repo 3)
4. ✅ Compile-time linking (Repo 4)
5. ✅ **THE ALGORITHM** (Repo 5 - ltreesitter)
6. ⚠️ Wasted time (Repo 6 - zig-tree-sitter)
7. ✅ **THE ARCHITECTURE** (Repo 7 - knut)

**This is COMPLETE:**
- ✅ Algorithm: Decoration table (Repo 5)
- ✅ Architecture: RAII wrappers (Repo 7)
- ✅ Build system: Compile-time linking (Repo 4, 7)
- ✅ Error handling: Exceptions (Repo 7)
- ✅ Memory management: RAII (Repo 7)
- ✅ API coverage: Comprehensive (Repo 7)
- ✅ Usage examples: Multiple repos (1-5, 7)

**Studying more = procrastination!**

Repo 7 (knut) validates that Repo 6 (zig) was indeed useless. But Repo 7 itself is HIGHLY valuable - it's a redemption arc!

**Time to BUILD!** We have both the algorithm AND the architecture. No more excuses.

---

## 📚 References

- **Knut Repository:** https://github.com/KDAB/knut
- **Knut Documentation:** https://kdab.github.io/knut/
- **Tree-sitter API Reference:** https://tree-sitter.github.io/tree-sitter/
- **ltreesitter (Algorithm):** `external/ltreesitter/examples/c-highlight.lua`
- **c-language-server (Build):** `external/c-language-server/CMakeLists.txt`

---

**Next Action:** 🚀 **BUILD THE PROTOTYPE!** 🚀

Use:
- **knut for architecture** (this repo)
- **ltreesitter for algorithm** (Repo 5)
- **c-language-server for CMake patterns** (Repo 4)

We have ALL the pieces. Time to assemble them!
