# P0 Answers: live-keys/livekeys

**Date:** 2025-12-15  
**Repo:** https://github.com/live-keys/livekeys  
**Session:** 18 of 29 repos  
**Confirmation:** 18th time answering all P0 questions

---

## Status: All P0 Questions ✅ ANSWERED (18th Confirmation)

This is the **18th repo** confirming all 5 P0 questions. All answers remain consistent with previous 17 repos.

**What's NEW in this repo:**
- ⭐⭐⭐⭐⭐ Clean opaque pointer wrapper pattern (void*)
- ⭐⭐⭐⭐⭐ Query predicate support implementation
- ⭐⭐⭐⭐ Incremental parsing with TSInput callbacks
- ⭐⭐⭐ AST structural comparison functions

**What's SAME in this repo:**
- ✅ Same parser initialization (ts_parser_new)
- ✅ Same parsing API (ts_parser_parse_string)
- ✅ Same query-based traversal (10th repo using queries!)
- ✅ Same compile-time linking (18th confirmation - redundant!)

---

## Q1: How to initialize parser? ✅ (18th Confirmation)

### Livekeys's C++ Wrapper Style

**Opaque pointer RAII pattern:**

```cpp
class LanguageParser {
public:
    using Language = void;  // TSLanguage as opaque type
    
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
        , m_engine(nullptr)
    {
        ts_parser_set_language(m_parser, 
                               reinterpret_cast<const TSLanguage*>(language));
    }
    
    ~LanguageParser(){
        ts_parser_delete(m_parser);
    }
    
    // Delete copy, allow move
    LanguageParser(const LanguageParser&) = delete;
    LanguageParser& operator=(const LanguageParser&) = delete;
    
    TSParser* m_parser;
    Language* m_language;
    Engine* m_engine;
};

// Usage
auto parser = LanguageParser::createForElements();
// Automatic cleanup when parser goes out of scope
```

**Why opaque pointers (void*):**
- Hides implementation details from API users
- Can change TSLanguage internals without breaking API
- Cleaner header files (no need to include tree_sitter/api.h)
- Type-safe (can't mix up different opaque types)

### Underlying C API (18th Confirmation)

```c
extern "C" {
    TSLanguage* tree_sitter_elements();
    TSLanguage* tree_sitter_cpp();
    TSLanguage* tree_sitter_javascript();
}

TSParser* parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_elements());

// ... use parser ...

ts_parser_delete(parser);
```

**Status:** Confirmed 18 times across C, C++, Rust, Lua, Zig, Vala, OCaml, Emacs Lisp. Same API, different syntax.

---

## Q2: How to parse code? ✅ (18th Confirmation)

### Livekeys's Two Parsing Approaches

**Approach 1: Simple string parsing (most common):**

```cpp
class LanguageParser {
public:
    using AST = void;  // TSTree as opaque type
    
    AST* parse(const std::string& source) const{
        return reinterpret_cast<AST*>(
            ts_parser_parse_string(
                m_parser,
                nullptr,  // No old tree (first parse)
                source.c_str(),
                static_cast<uint32_t>(source.size())
            )
        );
    }
    
    void destroy(AST* ast) const{
        if (ast)
            ts_tree_delete(reinterpret_cast<TSTree*>(ast));
    }
};

// Usage
std::string source = "int main() { return 0; }";
auto ast = parser->parse(source);

// ... use ast ...

parser->destroy(ast);
```

**Approach 2: Incremental parsing with TSInput (advanced):**

```cpp
class LanguageParser {
public:
    void editParseTree(AST*& ast, TSInputEdit& edit, TSInput& input){
        TSTree* tree = reinterpret_cast<TSTree*>(ast);
        
        if (tree){
            // Apply edit to existing tree
            ts_tree_edit(tree, &edit);
        }
        
        // Re-parse with edited tree (incremental)
        TSTree* new_tree = ts_parser_parse(m_parser, tree, input);
        
        ast = reinterpret_cast<AST*>(new_tree);
    }
};

// TSInputEdit tells tree-sitter what changed
TSInputEdit edit = {
    .start_byte = 10,
    .old_end_byte = 15,
    .new_end_byte = 20,
    .start_point = {1, 5},
    .old_end_point = {1, 10},
    .new_end_point = {1, 15}
};

// TSInput provides on-demand reading
const char* read_callback(void* payload, uint32_t byte_index,
                          TSPoint position, uint32_t* bytes_read) {
    Document* doc = static_cast<Document*>(payload);
    return doc->read_at(byte_index, bytes_read);
}

TSInput input = {
    .payload = &document,
    .read = read_callback,
    .encoding = TSInputEncodingUTF8
};

// Incremental parse
parser->editParseTree(ast, edit, input);
```

### Underlying C API (18th Confirmation)

**String parsing:**
```c
const char* source = "int main() { return 0; }";
TSTree* tree = ts_parser_parse_string(parser, NULL, source, strlen(source));

if (!tree){
    fprintf(stderr, "Parse failed!\n");
    return;
}

// ... use tree ...

ts_tree_delete(tree);
```

**Incremental parsing:**
```c
// First parse
TSTree* old_tree = ts_parser_parse_string(parser, NULL, old_source, old_len);

// Apply edit
TSInputEdit edit = { /* ... */ };
ts_tree_edit(old_tree, &edit);

// Re-parse with edited tree
TSInput input = { /* ... */ };
TSTree* new_tree = ts_parser_parse(parser, old_tree, input);

ts_tree_delete(old_tree);  // Old tree no longer needed
```

**Status:** Confirmed 18 times. Both string and TSInput approaches shown.

---

## Q3: How to walk syntax tree? ✅ (18th Confirmation - 10th Query-Based!)

### Livekeys's Query Wrapper

**Clean C++ API for queries:**

```cpp
class LanguageQuery {
public:
    using Ptr = std::shared_ptr<LanguageQuery>;
    
    // Create query from string
    static Ptr create(Language* language, const std::string& queryString){
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
            THROW_EXCEPTION(LanguageQueryException, 
                            "Language query error.", errorType);
        }
        
        return Ptr(new LanguageQuery(static_cast<void*>(query)));
    }
    
    ~LanguageQuery(){
        TSQuery* query = reinterpret_cast<TSQuery*>(m_query);
        ts_query_delete(query);
    }
    
    // Query metadata
    uint32_t captureCount() const;
    std::string captureName(uint32_t captureIndex) const;
    
    // Execute query
    Cursor::Ptr exec(AST* ast);
    Cursor::Ptr exec(AST* ast, uint32_t start, uint32_t end);  // Byte range
    
    // Predicate support
    bool predicateMatch(const Cursor::Ptr& cursor, void* payload);
    void addPredicate(const std::string& name, PredicateCallback callback);
    
private:
    void* m_query;  // TSQuery*
    std::map<std::string, PredicateCallback> m_predicates;
};

// Query cursor for iteration
class LanguageQuery::Cursor {
public:
    bool nextMatch();
    uint16_t totalMatchCaptures() const;
    uint16_t matchPatternIndex() const;
    
    SourceRange captureRange(uint16_t captureIndex);
    uint32_t captureId(uint16_t captureIndex);
    
private:
    void* m_cursor;        // TSQueryCursor*
    void* m_currentMatch;  // TSQueryMatch*
};
```

### Usage Example

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
    
    (string_literal) @string
    (number_literal) @number
    (comment) @comment
)";

auto query = LanguageQuery::create(language, queryString);

// 3. Execute query (optionally with byte range)
auto cursor = query->exec(ast);
// Or: auto cursor = query->exec(ast, start_byte, end_byte);

// 4. Iterate matches
while (cursor->nextMatch()){
    uint16_t captures = cursor->totalMatchCaptures();
    
    for (uint16_t i = 0; i < captures; ++i){
        uint32_t captureId = cursor->captureId(i);
        std::string captureName = query->captureName(captureId);
        SourceRange range = cursor->captureRange(i);
        
        std::string text = source.substr(range.position(), range.length());
        
        std::cout << captureName << " at " << range.position() 
                  << ": " << text << std::endl;
    }
}

// 5. Cleanup (automatic via RAII)
parser->destroy(ast);
```

### NEW: Query Predicates

**Livekeys supports custom predicates:**

```cpp
// Register custom predicate function
query->addPredicate("is-uppercase", 
    [](const std::vector<PredicateData>& args, void* payload) {
        if (args.empty()) return false;
        std::string text = /* extract from args[0].m_range */;
        return std::all_of(text.begin(), text.end(), ::isupper);
    });

// Use in query
std::string queryString = R"(
    (identifier) @id
    (#is-uppercase? @id)  ; Only match uppercase identifiers
    
    (function_definition
        name: (identifier) @name
    ) @function
    (#eq? @name "main")   ; Only match function named "main"
)";
```

**Built-in predicates** (standard tree-sitter):
- `#eq?` - String equality
- `#match?` - Regex match
- `#not-eq?` - String inequality
- `#not-match?` - Regex non-match

**Predicate implementation:**

```cpp
bool LanguageQuery::predicateMatch(const Cursor::Ptr& cursor, void* payload){
    TSQuery* query = reinterpret_cast<TSQuery*>(m_query);
    uint32_t length;
    const TSQueryPredicateStep* step = 
        ts_query_predicates_for_pattern(query, cursor->matchPatternIndex(), &length);
    
    if (length == 0)
        return true;  // No predicates to check
    
    std::string functionName;
    std::vector<PredicateData> args;
    
    // Parse predicate steps
    for (uint32_t i = 0; i < length; ++i){
        if (functionName.empty()){
            // First step is function name
            functionName = ts_query_string_value_for_id(query, step[i].value_id, &len);
        } else if (step[i].type == TSQueryPredicateStepTypeString){
            // String argument
            args.push_back(/* string value */);
        } else if (step[i].type == TSQueryPredicateStepTypeCapture){
            // Capture argument
            args.push_back(/* capture range */);
        } else if (step[i].type == TSQueryPredicateStepTypeDone){
            // Execute predicate
            auto it = m_predicates.find(functionName);
            if (it == m_predicates.end()){
                throw Exception("Unknown predicate: " + functionName);
            }
            if (!it->second(args, payload))
                return false;  // Predicate failed
            functionName = "";
        }
    }
    
    return true;  // All predicates passed
}
```

### Underlying C API (18th Confirmation)

```c
// Create query
const char* query_string = "(function_definition name: (identifier) @name) @function";
uint32_t error_offset;
TSQueryError error_type;

TSQuery* query = ts_query_new(
    language,
    query_string,
    strlen(query_string),
    &error_offset,
    &error_type
);

if (error_type != TSQueryErrorNone){
    fprintf(stderr, "Query error at offset %u\n", error_offset);
    ts_query_delete(query);
    return;
}

// Create cursor
TSQueryCursor* cursor = ts_query_cursor_new();

// Optional: set byte range
ts_query_cursor_set_byte_range(cursor, start_byte, end_byte);

// Execute query
TSNode root = ts_tree_root_node(tree);
ts_query_cursor_exec(cursor, query, root);

// Iterate matches
TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)){
    for (uint16_t i = 0; i < match.capture_count; ++i){
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        // Get capture name
        uint32_t name_length;
        const char* name = ts_query_capture_name_for_id(
            query, capture_id, &name_length);
        
        // Get byte range
        uint32_t start = ts_node_start_byte(node);
        uint32_t end = ts_node_end_byte(node);
        
        // Extract text
        printf("%.*s at %u-%u: %.*s\n",
               name_length, name,
               start, end,
               end - start, &source[start]);
    }
}

// Cleanup
ts_query_cursor_delete(cursor);
ts_query_delete(query);
```

**Status:** Confirmed 18 times. 10th repo using query-based approach (vs 7 manual). **Queries are clearly the standard.**

---

## Q4: How to map node types → colors? ⚠️ (N/A - Not a Highlighter)

### Not Applicable for livekeys

Livekeys is a **code editor**, not a **syntax highlighter**. It uses tree-sitter for:
- Code analysis
- AST navigation
- Structural comparison
- Refactoring support

It does NOT use tree-sitter for syntax coloring.

### For Our Terminal Highlighting Project

**Use ltreesitter's approach:**

```cpp
// 1. Query captures semantic names
std::string queryString = R"(
    (function_definition) @function
    (string_literal) @string
    [ "if" "else" "while" "for" ] @keyword
    (comment) @comment
    (number_literal) @number
)";

// 2. Theme maps semantic names to ANSI codes
std::unordered_map<std::string, std::string> theme = {
    {"function", "\033[33m"},   // Yellow
    {"string",   "\033[32m"},   // Green
    {"keyword",  "\033[35m"},   // Magenta
    {"comment",  "\033[90m"},   // Gray
    {"number",   "\033[36m"}    // Cyan
};

// 3. Build decoration table (byte → color)
std::unordered_map<uint32_t, std::string> decoration;

auto cursor = query->exec(ast);
while (cursor->nextMatch()){
    for (uint16_t i = 0; i < cursor->totalMatchCaptures(); ++i){
        uint32_t captureId = cursor->captureId(i);
        std::string captureName = query->captureName(captureId);
        SourceRange range = cursor->captureRange(i);
        
        std::string color = theme[captureName];
        
        // Mark each byte in range with color
        for (uint32_t byte = range.position(); 
             byte < range.position() + range.length(); 
             ++byte){
            decoration[byte] = color;
        }
    }
}

// 4. Output with ANSI codes
std::string prev_color;
for (uint32_t i = 0; i < source.length(); ++i){
    std::string curr_color = decoration[i];
    
    if (curr_color != prev_color){
        if (!prev_color.empty()){
            std::cout << "\033[0m";  // Reset
        }
        if (!curr_color.empty()){
            std::cout << curr_color;
        }
        prev_color = curr_color;
    }
    
    std::cout << source[i];
}

if (!prev_color.empty()){
    std::cout << "\033[0m";  // Final reset
}
```

**Status:** Not applicable for livekeys (not a highlighter), but pattern confirmed by other repos.

---

## Q5: How to output ANSI codes? ⚠️ (N/A - GUI Application)

### Not Applicable for livekeys

Livekeys is a **Qt GUI application**, not a **terminal application**. It outputs to:
- Qt widgets (QTextEdit, QPlainTextEdit)
- OpenGL rendering
- Image/video files

It does NOT output to terminal with ANSI codes.

### For Our Terminal Highlighting Project

**Use ltreesitter's decoration table algorithm (see Q4).**

**ANSI escape sequences:**

```cpp
// Color codes
#define ANSI_RESET      "\033[0m"
#define ANSI_BLACK      "\033[30m"
#define ANSI_RED        "\033[31m"
#define ANSI_GREEN      "\033[32m"
#define ANSI_YELLOW     "\033[33m"
#define ANSI_BLUE       "\033[34m"
#define ANSI_MAGENTA    "\033[35m"
#define ANSI_CYAN       "\033[36m"
#define ANSI_WHITE      "\033[37m"
#define ANSI_GRAY       "\033[90m"  // Bright black

// Effects
#define ANSI_BOLD       "\033[1m"
#define ANSI_DIM        "\033[2m"
#define ANSI_ITALIC     "\033[3m"
#define ANSI_UNDERLINE  "\033[4m"

// Combined
#define ANSI_BOLD_RED   "\033[1;31m"

// 256 colors: \033[38;5;<n>m
// RGB: \033[38;2;<r>;<g>;<b>m
```

**Output pattern:**

```cpp
void output_colored_text(const std::string& source,
                         const std::unordered_map<uint32_t, std::string>& decoration)
{
    std::string prev_color;
    
    for (uint32_t i = 0; i < source.length(); ++i){
        auto it = decoration.find(i);
        std::string curr_color = (it != decoration.end()) ? it->second : "";
        
        if (curr_color != prev_color){
            // Color change
            if (!prev_color.empty()){
                std::cout << ANSI_RESET;
            }
            if (!curr_color.empty()){
                std::cout << curr_color;
            }
            prev_color = curr_color;
        }
        
        std::cout << source[i];
    }
    
    // Final reset
    if (!prev_color.empty()){
        std::cout << ANSI_RESET;
    }
}
```

**Status:** Not applicable for livekeys (GUI app), but pattern confirmed by other repos.

---

## Summary: 18th Confirmation

### What's Confirmed (18th Time)

✅ **Q1: Parser init** - `ts_parser_new()` + `ts_parser_set_language()`  
✅ **Q2: Parsing** - `ts_parser_parse_string()` or `ts_parser_parse()`  
✅ **Q3: Tree walking** - Query-based (10th repo!) or manual (7 repos)  
✅ **Q4: Type → color** - Query captures + theme lookup + decoration table  
✅ **Q5: ANSI output** - Decoration table algorithm (from ltreesitter)

### What's NEW in livekeys

⭐⭐⭐⭐⭐ **Opaque pointer wrapper pattern** - Cleanest C++ API design  
⭐⭐⭐⭐⭐ **Query predicates** - Custom filter functions  
⭐⭐⭐⭐ **Incremental parsing** - TSInputEdit + TSInput callbacks  
⭐⭐⭐ **AST comparison** - Structural equality checking

### What's SAME in livekeys

✅ Same parser API (18th confirmation)  
✅ Same parsing functions (18th confirmation)  
✅ Query-based traversal (10th repo using queries)  
✅ Compile-time linking (18th confirmation - ABSURDLY REDUNDANT!)

### Statistics After 18 Repos

**Query vs Manual:**
- Query-based: 10 repos (56%)
- Manual traversal: 7 repos (39%)
- **Verdict: Queries win for highlighting**

**Study efficiency:** 88.9% (16 valuable / 18 total)

**Complete knowledge:**
- ✅ Algorithm: ltreesitter (decoration table)
- ✅ Architecture: knut (CMake) OR livekeys (opaque wrappers)
- ✅ Query organization: scopemux (separate .scm files)
- ✅ Multi-threading: control-flag (thread-local parsers)
- ✅ All P0 questions: Answered 18 times!

---

## Next Action

**🚀 BUILD THE PROTOTYPE NOW! 🚀**

We have answered all questions **18 times**. Further study is procrastination.

**Reference files:**
1. `external/ltreesitter/examples/c-highlight.lua` - THE ALGORITHM
2. `external/knut/3rdparty/CMakeLists.txt` - THE ARCHITECTURE
3. `external/livekeys/lib/lvelements/src/languagequery.cpp` - CLEANEST WRAPPERS
4. `external/scopemux-core/queries/` - QUERY ORGANIZATION
5. `external/control-flag/src/common_util.cpp` - MULTI-THREADING

**Everything needed to build is documented. Time to BUILD!**
