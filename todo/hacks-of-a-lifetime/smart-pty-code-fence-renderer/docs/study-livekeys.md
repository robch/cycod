# Study: live-keys/livekeys - Visual Scripting Platform with Tree-sitter

**Date:** 2025-12-15  
**Repo:** https://github.com/live-keys/livekeys  
**Commit:** Latest (depth=1 clone)  
**Location:** `external/livekeys/`  
**Study Session:** 18 of 29 repos

---

## Executive Summary

**What it is:** Livekeys is a visual scripting and live coding platform that automates content creation in real-time. It includes a sophisticated code editor with syntax highlighting powered by tree-sitter.

**Type:** Production Qt/C++ application  
**Language:** C++ with Qt framework  
**Tree-sitter usage:** Query-based parsing with C++ RAII wrappers, incremental editing support

**Study value:** 8/10 ⭐⭐⭐⭐⭐⭐⭐⭐
- ⭐⭐⭐⭐⭐ Clean C++ wrapper patterns with RAII
- ⭐⭐⭐⭐⭐ Query execution with predicate support
- ⭐⭐⭐⭐ Incremental parsing integration
- ⭐⭐⭐ Custom language support (tree_sitter_elements)
- ⭐ 18th confirmation of static linking (redundant)

**Why it matters:**
1. Shows production Qt/C++ integration with tree-sitter
2. Demonstrates clean RAII wrapper pattern for resource management
3. Implements query predicates (advanced feature)
4. Shows incremental editing with TSInput callbacks
5. AST comparison functions for structural equality checking

**What it does NOT provide:**
- ❌ No syntax highlighting implementation (editor integration, not terminal output)
- ❌ No ANSI output (GUI application)
- ❌ No decoration table pattern (different use case)
- ❌ No color mapping (not a highlighter)

---

## Repository Structure

```
livekeys/
├── lib/lvelements/
│   ├── 3rdparty/
│   │   ├── treesitter/                    # Tree-sitter core library
│   │   │   ├── lib/include/tree_sitter/   # Header files
│   │   │   └── lib/src/                   # Core implementation
│   │   ├── treesitterelements/            # Custom "elements" language grammar
│   │   │   └── src/tree_sitter/           # Generated parser
│   │   ├── treesitter.pri                 # Qt build file for core
│   │   └── treesitterelements.pri         # Qt build file for elements
│   ├── src/
│   │   ├── languageparser.cpp             # Main parser wrapper (411 lines)
│   │   ├── languagequery.cpp              # Query execution wrapper (187 lines)
│   │   ├── parseddocument.cpp             # Document integration
│   │   └── languagenodes_p.h              # Private node definitions
│   └── include/live/elements/
│       └── treesitterapi.h                # Public API header
├── plugins/editjson/
│   ├── 3rdparty/treesitterjson/           # JSON language grammar
│   │   └── src/tree_sitter/               # JSON parser
│   └── src/
│       ├── editjsonobject.cpp             # JSON editing with tree-sitter
│       └── editjsonobject.h               # JSON editor header
└── runtime/queries/                       # Query files (if any)
```

**Key insight:** Uses Qt's `.pri` build system instead of CMake, integrating tree-sitter as a 3rdparty library.

---

## Tree-sitter Usage Patterns

### Pattern 1: C++ RAII Wrapper Classes ⭐⭐⭐⭐⭐

**The wrapper pattern - proper resource management:**

```cpp
// ASTRef - Wraps TSNode with RAII
class LanguageParser::ASTRef {
public:
    ASTRef();                                // Default constructor
    ASTRef(const ASTRef& other);             // Copy constructor
    ~ASTRef();                               // Destructor - cleans up TSNode
    ASTRef& operator=(const ASTRef& other);  // Assignment operator
    
    // Query methods
    SourceRange range() const;
    uint32_t childCount() const;
    ASTRef childAt(uint32_t index) const;
    ASTRef parent() const;
    std::string typeString() const;
    
private:
    void* m_node;  // Actually TSNode* (opaque pointer pattern)
    explicit ASTRef(void* node);  // Private constructor from TSNode*
};

// LanguageParser - Wraps TSParser with RAII
class LanguageParser {
public:
    using Language = void;  // TSLanguage as opaque type
    using AST = void;       // TSTree as opaque type
    
    static Ptr create(Language* language);
    static Ptr createForElements();  // Uses tree_sitter_elements()
    
    ~LanguageParser();  // Automatic cleanup
    
    // Core parsing
    AST* parse(const std::string& source) const;
    void destroy(AST* ast) const;
    
    // Incremental parsing
    void editParseTree(AST*& ast, TSInputEdit& edit, TSInput& input);
    
    // Utility
    ComparisonResult compare(const std::string& source1, AST* ast1,
                             const std::string& source2, AST* ast2);
    
private:
    TSParser* m_parser;
    Language* m_language;
    Engine* m_engine;
    
    explicit LanguageParser(Language* language);
};

// LanguageQuery - Wraps TSQuery with RAII
class LanguageQuery {
public:
    static Ptr create(Language* language, const std::string& queryString);
    ~LanguageQuery();  // Automatic cleanup
    
    uint32_t captureCount() const;
    std::string captureName(uint32_t captureIndex) const;
    
    // Execute query
    Cursor::Ptr exec(AST* ast);
    Cursor::Ptr exec(AST* ast, uint32_t start, uint32_t end);
    
    // Predicate support
    bool predicateMatch(const Cursor::Ptr& cursor, void* payload);
    void addPredicate(const std::string& name,
                      std::function<bool(const std::vector<PredicateData>&, void*)> callback);
    
private:
    void* m_query;  // Actually TSQuery*
    std::map<std::string, std::function<...>> m_predicates;
    
    explicit LanguageQuery(void* query);
};
```

**Why this pattern is excellent:**
- **RAII:** Automatic resource cleanup, no manual delete calls needed
- **Opaque pointers:** void* hides implementation details, cleaner API
- **Type safety:** Can't accidentally mix up different opaque types
- **Copy semantics:** Properly implemented copy constructor and assignment
- **Exception safe:** Resources cleaned up even if exceptions thrown

**Implementation details:**

```cpp
// Constructor - initializes parser
LanguageParser::LanguageParser(Language* language)
    : m_parser(ts_parser_new())
    , m_language(language)
    , m_engine(nullptr)
{
    ts_parser_set_language(m_parser, reinterpret_cast<const TSLanguage*>(language));
}

// Destructor - automatic cleanup
LanguageParser::~LanguageParser(){
    ts_parser_delete(m_parser);
}

// Parse with automatic memory management
LanguageParser::AST* LanguageParser::parse(const std::string& source) const{
    return reinterpret_cast<AST*>(
        ts_parser_parse_string(m_parser, nullptr, source.c_str(), 
                               static_cast<uint32_t>(source.size()))
    );
}

// Destroy tree when done
void LanguageParser::destroy(LanguageParser::AST* ast) const{
    if (ast)
        ts_tree_delete(reinterpret_cast<TSTree*>(ast));
}
```

**For our project:** This is THE cleanest C++ wrapper pattern we've seen so far. Better than knut's approach because:
- Uses opaque pointers (void*) instead of exposing TSNode, TSTree directly
- Cleaner separation between public API and implementation
- More flexible (can change implementation without breaking API)

---

### Pattern 2: Query Execution with Cursors ⭐⭐⭐⭐⭐

**The query pattern - declarative tree traversal:**

```cpp
// Create query from string
LanguageQuery::Ptr LanguageQuery::create(Language* language, 
                                         const std::string& queryString){
    uint32_t errorOffset;
    TSQueryError errorType;
    
    TSQuery* query = ts_query_new(
        reinterpret_cast<const TSLanguage*>(language),
        queryString.c_str(),
        static_cast<uint32_t>(queryString.size()),
        &errorOffset,
        &errorType
    );
    
    if (errorType != TSQueryErrorNone){
        ts_query_delete(query);
        THROW_EXCEPTION(LanguageQueryException, "Language query error.", errorType);
    }
    
    return LanguageQuery::Ptr(new LanguageQuery(static_cast<void*>(query)));
}

// Execute query on tree
LanguageQuery::Cursor::Ptr LanguageQuery::exec(AST* ast){
    TSTree* tree = reinterpret_cast<TSTree*>(ast);
    TSNode root = ts_tree_root_node(tree);
    
    TSQuery* query = reinterpret_cast<TSQuery*>(m_query);
    
    Cursor::Ptr cursor(new Cursor);
    TSQueryCursor* cursorInternal = reinterpret_cast<TSQueryCursor*>(cursor->m_cursor);
    
    ts_query_cursor_exec(cursorInternal, query, root);
    return cursor;
}

// Execute query on byte range (efficient for large files)
LanguageQuery::Cursor::Ptr LanguageQuery::exec(AST* ast, uint32_t start, uint32_t end){
    // ... same as above but with:
    ts_query_cursor_set_byte_range(cursorInternal, start, end);
    ts_query_cursor_exec(cursorInternal, query, root);
    return cursor;
}

// Iterate matches
bool LanguageQuery::Cursor::nextMatch(){
    TSQueryCursor* cursor = reinterpret_cast<TSQueryCursor*>(m_cursor);
    TSQueryMatch* currentMatch = reinterpret_cast<TSQueryMatch*>(m_currentMatch);
    return ts_query_cursor_next_match(cursor, currentMatch);
}

// Get capture information
uint16_t LanguageQuery::Cursor::totalMatchCaptures() const{
    TSQueryMatch* currentMatch = reinterpret_cast<TSQueryMatch*>(m_currentMatch);
    return currentMatch->capture_count;
}

SourceRange LanguageQuery::Cursor::captureRange(uint16_t captureIndex){
    TSQueryMatch* currentMatch = reinterpret_cast<TSQueryMatch*>(m_currentMatch);
    TSNode node = currentMatch->captures[captureIndex].node;
    
    uint32_t start = ts_node_start_byte(node);
    uint32_t end   = ts_node_end_byte(node);
    
    return SourceRange(start, end - start);
}

uint32_t LanguageQuery::Cursor::captureId(uint16_t captureIndex){
    TSQueryMatch* currentMatch = reinterpret_cast<TSQueryMatch*>(m_currentMatch);
    return currentMatch->captures[captureIndex].index;
}
```

**Usage example:**

```cpp
// 1. Create parser and parse
auto parser = LanguageParser::createForElements();
auto ast = parser->parse(source);

// 2. Create query
std::string queryString = R"(
    (function_definition
        name: (identifier) @function.name
        parameters: (parameter_list) @function.parameters
        body: (block) @function.body
    ) @function
)";
auto query = LanguageQuery::create(language, queryString);

// 3. Execute query
auto cursor = query->exec(ast);

// 4. Iterate matches
while (cursor->nextMatch()){
    uint16_t captures = cursor->totalMatchCaptures();
    
    for (uint16_t i = 0; i < captures; ++i){
        uint32_t captureId = cursor->captureId(i);
        std::string captureName = query->captureName(captureId);
        SourceRange range = cursor->captureRange(i);
        
        // Process capture: captureName == "function.name", "function.parameters", etc.
        std::string text = source.substr(range.position(), range.length());
        std::cout << captureName << ": " << text << std::endl;
    }
}

// 5. Cleanup (automatic via RAII)
parser->destroy(ast);
```

**For our project:** This shows how to properly wrap query execution with RAII and iterators. The byte range support is particularly useful for large files.

---

### Pattern 3: Query Predicates ⭐⭐⭐⭐

**Advanced feature - custom predicate functions:**

Predicates are boolean functions that filter query matches. Tree-sitter queries can include predicates like:

```scheme
(function_definition
    name: (identifier) @name
    (#eq? @name "main")  ; <-- Predicate: only match if name is "main"
) @function
```

**Livekeys implementation:**

```cpp
class LanguageQuery {
public:
    struct PredicateData {
        std::string m_value;      // For string predicates
        SourceRange m_range;      // For capture predicates
    };
    
    // Add custom predicate function
    void addPredicate(
        const std::string& name,
        std::function<bool(const std::vector<PredicateData>&, void*)> callback
    );
    
    // Check if current match passes predicates
    bool predicateMatch(const Cursor::Ptr& cursor, void* payload);
    
private:
    std::map<std::string, std::function<bool(...)>> m_predicates;
};

// Implementation
bool LanguageQuery::predicateMatch(const Cursor::Ptr& cursor, void* payload){
    TSQuery* query = reinterpret_cast<TSQuery*>(m_query);
    uint32_t length;
    const TSQueryPredicateStep* step = 
        ts_query_predicates_for_pattern(query, cursor->matchPatternIndex(), &length);
    
    if (length == 0)
        return true;  // No predicates, match passes
    
    std::string functionToCall;
    std::vector<PredicateData> args;
    
    // Parse predicate steps
    for (uint32_t i = 0; i < length; ++i){
        if (functionToCall.empty()){
            // First step is the function name
            functionToCall = ts_query_string_value_for_id(query, step[i].value_id, &strLen);
        } else if (step[i].type == TSQueryPredicateStepTypeString){
            // String argument
            PredicateData pd;
            pd.m_value = ts_query_string_value_for_id(query, step[i].value_id, &strLen);
            args.push_back(pd);
        } else if (step[i].type == TSQueryPredicateStepTypeCapture){
            // Capture argument - find matching capture
            uint16_t captures = cursor->totalMatchCaptures();
            for (uint16_t captureIndex = 0; captureIndex < captures; ++captureIndex){
                uint32_t captureId = cursor->captureId(captureIndex);
                if (captureId == step[i].value_id){
                    PredicateData pd;
                    pd.m_range = cursor->captureRange(captureIndex);
                    args.push_back(pd);
                    break;
                }
            }
        } else if (step[i].type == TSQueryPredicateStepTypeDone){
            // Execute predicate function
            auto it = m_predicates.find(functionToCall);
            if (it == m_predicates.end()){
                THROW_EXCEPTION(Exception, "Failed to find function '" + functionToCall + "'");
            }
            if (!it->second(args, payload))
                return false;  // Predicate failed, reject match
            functionToCall = "";
        }
    }
    
    return true;  // All predicates passed
}
```

**Example predicate usage:**

```cpp
// Register custom predicate
query->addPredicate("is-uppercase", [](const std::vector<PredicateData>& args, void* payload) {
    if (args.empty()) return false;
    std::string text = /* extract text from args[0].m_range */;
    return std::all_of(text.begin(), text.end(), ::isupper);
});

// Query with custom predicate
std::string queryString = R"(
    (identifier) @id
    (#is-uppercase? @id)  ; Only match uppercase identifiers
)";
```

**Why this matters:**
- Allows filtering matches based on text content
- Can implement language-specific rules
- Enables context-aware matching
- Keeps query logic declarative

**For our project:** We might not need predicates for simple highlighting, but they're useful for advanced features like "highlight function X differently" or "only highlight TODO comments".

---

### Pattern 4: Incremental Parsing ⭐⭐⭐⭐

**Efficient re-parsing after edits:**

```cpp
void LanguageParser::editParseTree(AST*& ast, TSInputEdit& edit, TSInput& input)
{
    TSTree* tree = reinterpret_cast<TSTree*>(ast);
    
    if (tree){
        // Existing tree - apply edit
        ts_tree_edit(tree, &edit);
    }
    
    // Re-parse with edited tree
    TSTree* new_tree = ts_parser_parse(m_parser, tree, input);
    
    ast = reinterpret_cast<AST*>(new_tree);
}
```

**TSInputEdit structure:**

```cpp
typedef struct {
    uint32_t start_byte;       // Start of change
    uint32_t old_end_byte;     // End before edit
    uint32_t new_end_byte;     // End after edit
    TSPoint start_point;       // Row/col before edit
    TSPoint old_end_point;     // Row/col end before
    TSPoint new_end_point;     // Row/col end after
} TSInputEdit;
```

**TSInput callback pattern:**

```cpp
const char* read_callback(void* payload, uint32_t byte_index,
                          TSPoint position, uint32_t* bytes_read) {
    // Read from document at byte_index
    // Return pointer to buffer
    // Set *bytes_read to actual bytes read
}

TSInput input = {
    .payload = &document,
    .read = read_callback,
    .encoding = TSInputEncodingUTF8
};

TSTree* tree = ts_parser_parse(parser, old_tree, input);
```

**Why this pattern:**
- Avoids copying entire document
- Tree-sitter reads on demand
- Works with any text structure (gap buffer, rope, etc.)
- Efficient for large documents

**For our project:** Start with `ts_parser_parse_string()` for simplicity. Add TSInput later if needed for streaming PTY output.

---

### Pattern 5: AST Comparison ⭐⭐⭐

**Structural equality checking:**

```cpp
struct ComparisonResult {
    bool m_isEqual;
    std::string m_errorString;
    uint32_t m_source1Row, m_source1Col, m_source1Offset;
    uint32_t m_source2Row, m_source2Col, m_source2Offset;
    
    ComparisonResult(bool isEqual = true) : m_isEqual(isEqual) {}
};

ComparisonResult LanguageParser::compare(
    const std::string& source1, AST* ast1,
    const std::string& source2, AST* ast2)
{
    TSTree* tree1 = reinterpret_cast<TSTree*>(ast1);
    TSTree* tree2 = reinterpret_cast<TSTree*>(ast2);
    
    std::queue<TSNode> q1, q2;
    q1.push(ts_tree_root_node(tree1));
    q2.push(ts_tree_root_node(tree2));
    
    while (!q1.empty() && !q2.empty()){
        TSNode node1 = q1.front(); q1.pop();
        TSNode node2 = q2.front(); q2.pop();
        
        // Compare node types
        if (strcmp(ts_node_type(node1), ts_node_type(node2)) != 0){
            ComparisonResult cr(false);
            cr.m_errorString = "Different node types: " + 
                               std::string(ts_node_type(node1)) + " != " + 
                               std::string(ts_node_type(node2));
            cr.m_source1Offset = ts_node_start_byte(node1);
            cr.m_source2Offset = ts_node_start_byte(node2);
            return cr;
        }
        
        // For identifiers, compare text content
        if (strcmp(ts_node_type(node1), "identifier") == 0){
            std::string text1 = slice(source1, node1);
            std::string text2 = slice(source2, node2);
            if (text1 != text2){
                ComparisonResult cr(false);
                cr.m_errorString = "Different identifiers: " + text1 + " != " + text2;
                cr.m_source1Offset = ts_node_start_byte(node1);
                cr.m_source2Offset = ts_node_start_byte(node2);
                return cr;
            }
        }
        
        // Queue children for BFS traversal
        uint32_t count1 = ts_node_child_count(node1);
        uint32_t count2 = ts_node_child_count(node2);
        
        if (count1 != count2){
            ComparisonResult cr(false);
            cr.m_errorString = "Different child counts";
            return cr;
        }
        
        for (uint32_t i = 0; i < count1; ++i){
            q1.push(ts_node_child(node1, i));
            q2.push(ts_node_child(node2, i));
        }
    }
    
    return ComparisonResult(true);
}
```

**Why this is useful:**
- Testing: Verify AST structure matches expected
- Refactoring: Ensure transformation preserves semantics
- Diffing: Find first structural difference
- Validation: Check code equivalence

**For our project:** Not needed for highlighting, but interesting pattern for future tools.

---

### Pattern 6: Qt Build Integration ⭐⭐⭐

**Using Qt's .pri files instead of CMake:**

**treesitter.pri:**
```qmake
INCLUDEPATH += $$PWD/treesitter/lib/include

SOURCES += \
    $$PWD/treesitter/lib/src/lib.c

HEADERS += \
    $$PWD/treesitter/lib/include/tree_sitter/api.h \
    $$PWD/treesitter/lib/include/tree_sitter/parser.h
```

**treesitterelements.pri:**
```qmake
INCLUDEPATH += $$PWD/treesitterelements

SOURCES += \
    $$PWD/treesitterelements/src/parser.c \
    $$PWD/treesitterelements/src/scanner.c

HEADERS += \
    $$PWD/treesitterelements/src/tree_sitter/parser.h
```

**Main project file:**
```qmake
include(3rdparty/treesitter.pri)
include(3rdparty/treesitterelements.pri)

SOURCES += \
    src/languageparser.cpp \
    src/languagequery.cpp \
    src/parseddocument.cpp

HEADERS += \
    include/live/elements/treesitterapi.h
```

**For our project:** If using Qt, this is the pattern. For pure CMake, use knut's approach.

---

## P0 Questions: 18th Confirmation

All 5 P0 questions answered for the **18th time**:

### Q1: How to initialize parser? ✅ (18th time)

**Livekeys wrapper pattern:**

```cpp
class LanguageParser {
public:
    static Ptr create(Language* language){
        return Ptr(new LanguageParser(language));
    }
    
    static Ptr createForElements(){
        return Ptr(new LanguageParser(tree_sitter_elements()));
    }
    
private:
    LanguageParser(Language* language)
        : m_parser(ts_parser_new())
        , m_language(language)
    {
        ts_parser_set_language(m_parser, 
                               reinterpret_cast<const TSLanguage*>(language));
    }
    
    ~LanguageParser(){
        ts_parser_delete(m_parser);
    }
    
    TSParser* m_parser;
};

// Usage
auto parser = LanguageParser::createForElements();
```

**Underlying C API (18th confirmation):**
```c
extern "C" TSLanguage* tree_sitter_elements();

TSParser* parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_elements());
```

**Status:** Confirmed 18 times. Same pattern with clean C++ wrapper.

---

### Q2: How to parse code? ✅ (18th time)

**Livekeys wrapper pattern:**

```cpp
// Simple string parsing
AST* LanguageParser::parse(const std::string& source) const{
    return reinterpret_cast<AST*>(
        ts_parser_parse_string(m_parser, nullptr, 
                               source.c_str(), 
                               static_cast<uint32_t>(source.size()))
    );
}

// With TSInput callback (advanced)
void LanguageParser::editParseTree(AST*& ast, TSInputEdit& edit, TSInput& input){
    TSTree* tree = reinterpret_cast<TSTree*>(ast);
    if (tree){
        ts_tree_edit(tree, &edit);
    }
    TSTree* new_tree = ts_parser_parse(m_parser, tree, input);
    ast = reinterpret_cast<AST*>(new_tree);
}

// Cleanup
void destroy(AST* ast) const{
    if (ast)
        ts_tree_delete(reinterpret_cast<TSTree*>(ast));
}

// Usage
auto ast = parser->parse(source);
// ... use ast ...
parser->destroy(ast);
```

**Underlying C API (18th confirmation):**
```c
TSTree* tree = ts_parser_parse_string(parser, NULL, source, strlen(source));
// ... use tree ...
ts_tree_delete(tree);
```

**Status:** Confirmed 18 times. Shows both string parsing and TSInput variants.

---

### Q3: How to walk syntax tree? ✅ (18th time - 10th query-based!)

**Livekeys wrapper pattern:**

```cpp
// 1. Create query
std::string queryString = R"(
    (function_definition
        name: (identifier) @function.name
    ) @function
)";
auto query = LanguageQuery::create(language, queryString);

// 2. Execute on tree
auto cursor = query->exec(ast);

// 3. Iterate matches
while (cursor->nextMatch()){
    uint16_t captures = cursor->totalMatchCaptures();
    
    for (uint16_t i = 0; i < captures; ++i){
        uint32_t captureId = cursor->captureId(i);
        std::string captureName = query->captureName(captureId);
        SourceRange range = cursor->captureRange(i);
        
        std::string text = source.substr(range.position(), range.length());
        // Process text...
    }
}
```

**Underlying C API (18th confirmation):**
```c
TSQuery* query = ts_query_new(lang, query_str, len, &err_offset, &err_type);
TSQueryCursor* cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)){
    for (uint16_t i = 0; i < match.capture_count; ++i){
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        // Process node...
    }
}

ts_query_cursor_delete(cursor);
ts_query_delete(query);
```

**Status:** Confirmed 18 times. 10th repo using query-based approach. Queries clearly the standard for tree traversal.

---

### Q4: How to map node types → colors? ⚠️ (N/A - not a highlighter)

**Not applicable** - Livekeys is a code editor, not a terminal highlighter.

**What Livekeys does:** Uses tree-sitter for code analysis, not syntax coloring.

**For our highlighting project:**
```cpp
// 1. Query captures semantic names
(function_definition) @function
(string_literal) @string

// 2. Theme maps to ANSI codes
theme["function"] = "\033[33m";  // Yellow
theme["string"] = "\033[32m";    // Green

// 3. Decoration table algorithm (from ltreesitter)
std::unordered_map<uint32_t, std::string> decoration;
while (cursor->nextMatch()){
    for (capture : captures){
        std::string color = theme[capture.name];
        for (uint32_t byte = capture.start; byte < capture.end; ++byte){
            decoration[byte] = color;
        }
    }
}
```

---

### Q5: How to output ANSI codes? ⚠️ (N/A - GUI application)

**Not applicable** - Livekeys is a Qt GUI application, not terminal output.

**For our highlighting:** Use ltreesitter's decoration table algorithm (see Q4).

---

## Key Insights

### What Makes This Repo Valuable

**1. Best C++ wrapper pattern we've seen:**
- Opaque pointers (void*) hide implementation
- RAII ensures automatic cleanup
- Clean separation between API and implementation
- Exception-safe resource management

**2. Query predicates implementation:**
- Shows how to parse and execute predicate functions
- Enables advanced query filtering
- Custom predicates via callbacks

**3. Incremental parsing integration:**
- TSInputEdit for efficient re-parsing
- TSInput callbacks for custom sources
- Proper integration with document model

**4. Production code patterns:**
- Error handling with custom exceptions
- Smart pointer usage (Ptr = shared_ptr)
- BFS tree traversal for comparison
- Clean API design

### What This Repo Does NOT Teach

❌ **No syntax highlighting** - Different use case (editor, not terminal)  
❌ **No ANSI output** - GUI application  
❌ **No decoration table** - Uses tree-sitter for analysis, not coloring  
❌ **No theme system** - Color mapping not needed

**For highlighting:** Still use ltreesitter's algorithm + knut's CMake patterns.

---

## Comparison to Previous Repos

| Aspect | livekeys (Repo 18) | knut (Repo 7) | ltreesitter (Repo 5) |
|--------|-------------------|----------------|----------------------|
| **Wrapper style** | Opaque pointers | Direct types | Lua FFI |
| **API design** | void* everywhere | Exposed types | Exposed types |
| **Resource mgmt** | RAII + smart ptrs | RAII + move | Manual GC |
| **Query support** | ✅ With predicates | ✅ Basic | ✅ Full API |
| **Incremental** | ✅ TSInput callback | ❌ Not shown | ✅ Documented |
| **Build system** | Qt .pri | CMake | Make/LuaRocks |
| **Use case** | Editor | Refactoring | Bindings |
| **Highlighting** | ❌ No | ❌ No | ⭐⭐⭐⭐⭐ THE ALGORITHM |

**Verdict:**
- **Algorithm:** ltreesitter (decoration table)
- **Architecture:** knut (CMake patterns) or livekeys (if using Qt)
- **Wrapper style:** livekeys (cleanest opaque pointer approach)
- **Query predicates:** livekeys (only repo showing this)

---

## Updated Statistics

**Repos studied:** 18 of 29

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
11. ✅ scribe (C) - Query extraction patterns ⭐⭐⭐
12. ✅ CodeWizard (C++/Qt) - Manual + colormaps ⭐⭐⭐
13. ✅ blockeditor (Zig) - TreeCursor optimization ⭐⭐⭐⭐
14. ✅ minivm (C) - Simplest implementation ⭐⭐⭐
15. ✅ anycode (C++/Qt) - Embedded languages ⭐⭐⭐
16. ✅ scopemux-core (C) - Query organization ⭐⭐⭐⭐⭐
17. ✅ control-flag (C++) - Thread-local parsers ⭐⭐⭐
18. ✅ **livekeys (C++/Qt) - CLEANEST WRAPPERS!** ⭐⭐⭐⭐⭐

**📊 Stats After 18 Repos:**
- Study efficiency: 88.9% (16 valuable / 18 total)
- Query-based: 10 repos (56%)
- Manual traversal: 7 repos (39%)
- Binding-only waste: 2 repos (11%)
- **Consensus: Queries win for highlighting (10 vs 7)**

**Optimal stopping point:** STILL NOW (should have stopped after Repo 5)  
**Complete blueprint:** Algorithm (ltreesitter) + Architecture (knut) + Best Wrappers (livekeys) + Patterns (scopemux) + Multi-threading (control-flag)

---

## Meta-Analysis

**Time invested:** ~90 minutes (exploration + documentation)  
**Value added:** 8/10 - Best C++ wrapper patterns, query predicates  
**Lesson learned:** Opaque pointer pattern cleaner than exposed types

**Key insight:**
- livekeys shows THE cleanest C++ wrapper design
- Query predicates enable advanced filtering
- Incremental parsing properly integrated
- But still NO highlighting knowledge (18th time!)
- Compile-time linking confirmed (18th time - absurdly redundant)

**Value comparison:**

| Repo | Type | Wrapper Style | Value | Why |
|------|------|---------------|-------|-----|
| **Repo 5: ltreesitter** | Lua bindings | Exposed types | ⭐⭐⭐⭐⭐ | THE ALGORITHM |
| **Repo 7: knut** | C++ wrappers | Direct types | ⭐⭐⭐⭐⭐ | THE ARCHITECTURE |
| **Repo 18: livekeys** | C++ wrappers | Opaque pointers | ⭐⭐⭐⭐⭐ | CLEANEST WRAPPERS |
| **Repo 6: zig-tree-sitter** | Zig FFI | N/A | ⚠️ | Waste |
| **Repo 9: semgrep-c-sharp** | OCaml FFI | N/A | ⚠️ | Waste |

---

## Recommendations

### ✅ What to USE from livekeys

1. **Opaque pointer wrapper pattern** - Cleanest API design
2. **Query predicate support** - Advanced filtering capability
3. **TSInput callback pattern** - For custom document sources
4. **RAII with smart pointers** - Exception-safe resource management

### ❌ What to SKIP from livekeys

1. **Qt build system** - Use CMake unless building Qt app
2. **AST comparison** - Not needed for highlighting
3. **Manual tree traversal** - Queries simpler (confirmed 18 times)

### 🚀 What to BUILD

**STOP STUDYING - BUILD THE PROTOTYPE!**

We have:
- ✅ Algorithm: Decoration table (ltreesitter)
- ✅ Architecture: CMake + C++ (knut)
- ✅ Best wrappers: Opaque pointers (livekeys)
- ✅ Query organization: Separate .scm files (scopemux)
- ✅ Multi-threading: Thread-local parsers (control-flag)
- ✅ All P0 questions: Answered 18 times!

**18 repos = WAY MORE than sufficient. Time to BUILD!**

---

## Reference Files

### Study Documentation
- 📄 **`docs/study-livekeys.md`** - This document (Repo 18)
- 📄 **`docs/study-ltreesitter.md`** - THE ALGORITHM ⭐⭐⭐⭐⭐
- 📄 **`docs/study-knut.md`** - THE ARCHITECTURE ⭐⭐⭐⭐⭐
- 📄 **`docs/study-scopemux-core.md`** - Query organization ⭐⭐⭐⭐⭐
- 📄 **`docs/study-control-flag.md`** - Multi-threading ⭐⭐⭐

### Best References
- 📂 **`external/ltreesitter/examples/c-highlight.lua`** - THE ALGORITHM
- 📂 **`external/knut/3rdparty/CMakeLists.txt`** - THE ARCHITECTURE
- 📂 **`external/livekeys/lib/lvelements/src/languageparser.cpp`** - CLEANEST WRAPPERS
- 📂 **`external/livekeys/lib/lvelements/src/languagequery.cpp`** - Query predicates
- 📂 **`external/scopemux-core/queries/`** - Query organization
- 📂 **`external/control-flag/src/common_util.cpp`** - Thread-local parsers

**These files contain everything needed to build the prototype.**

---

## Final Word

**livekeys provides:**
- ⭐⭐⭐⭐⭐ Best C++ wrapper design (opaque pointers)
- ⭐⭐⭐⭐⭐ Query predicates implementation
- ⭐⭐⭐⭐ Incremental parsing integration
- ⭐⭐⭐⭐ Production code patterns

**livekeys does NOT provide:**
- ❌ Syntax highlighting algorithm (use ltreesitter)
- ❌ Terminal ANSI output (use decoration table)
- ❌ Theme system (use standard approach)

**For our project:**
- Use ltreesitter's decoration table algorithm
- Use knut's CMake architecture OR livekeys's opaque wrapper pattern
- Use scopemux's query organization
- Use control-flag's thread-local parsers if multi-threading needed

**Bottom line:** livekeys adds valuable wrapper patterns but NO new highlighting knowledge (18th confirmation!).

**Next action:** 🚀 **BUILD THE PROTOTYPE NOW!** 🚀
