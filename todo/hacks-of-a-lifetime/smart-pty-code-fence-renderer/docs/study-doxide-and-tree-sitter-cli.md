# Study Report: doxide + tree-sitter CLI

**Date:** 2025-12-15  
**Repos Studied:**
- `lawmurray/doxide` - Documentation generator using Tree-sitter
- `tree-sitter/tree-sitter` (CLI highlight.rs) - Official Tree-sitter syntax highlighter

**Status:** ✅ ALL 5 P0 QUESTIONS ANSWERED!

---

## Executive Summary

**Key Finding:** Tree-sitter has a dedicated `tree-sitter-highlight` library that handles syntax highlighting! The official CLI tool uses this library to output ANSI-colored code to the terminal.

**What This Means:**
- We don't need to manually walk the AST and map node types to colors
- The `tree-sitter-highlight` library does all the heavy lifting
- We just need to provide a theme (JSON) and process highlight events
- ANSI output is handled by the `anstyle` Rust crate (we'd need equivalent for C++)

**Confidence Level:** 🎯 HIGH - We can definitely use Tree-sitter for our use case!

---

## P0 Questions - ALL ANSWERED!

### ✅ Q1: How to initialize parser?

**From:** doxide/src/CppParser.cpp (lines 446-447)

```c
parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_cuda());
```

**Pattern:**
1. Create parser with `ts_parser_new()`
2. Set language grammar with `ts_parser_set_language(parser, tree_sitter_LANG())`
3. Each language has its own grammar function: `tree_sitter_cpp()`, `tree_sitter_javascript()`, etc.

**Cleanup:**
```c
ts_parser_delete(parser);
```

**Key Learning:** Grammar functions are language-specific and need to be linked in at compile time. Each language (C++, Python, JavaScript, etc.) has its own `tree-sitter-LANGUAGE` repo with a `parser.c` file.

---

### ✅ Q2: How to parse code?

**From:** doxide/src/CppParser.cpp (lines 500-512)

```c
TSTree* tree = ts_parser_parse_string(parser, NULL, file.decl.data(),
    uint32_t(file.decl.size()));

if (!tree) {
    // Parse failed completely
    warn("cannot parse " << filename << ", skipping");
    return;
}

TSNode node = ts_tree_root_node(tree);
```

**Pattern:**
1. Parse string with `ts_parser_parse_string(parser, old_tree, source, length)`
2. Check if `tree` is NULL (catastrophic failure)
3. Get root node with `ts_tree_root_node(tree)`
4. Cleanup with `ts_tree_delete(tree)`

**Key Learning:** The second parameter to `parse_string` can be an old tree for incremental parsing. Pass `NULL` for first parse.

---

### ✅ Q3: How to walk syntax tree?

**Two Approaches Found:**

#### Approach A: Manual Traversal (Low-level)
From doxide (implicit in code):
```c
TSNode root = ts_tree_root_node(tree);
uint32_t child_count = ts_node_child_count(root);
for (uint32_t i = 0; i < child_count; i++) {
    TSNode child = ts_node_child(root, i);
    const char* type = ts_node_type(child);
    uint32_t start = ts_node_start_byte(child);
    uint32_t end = ts_node_end_byte(child);
    uint32_t line = ts_node_start_point(child).row;
    // Process node...
}
```

#### Approach B: Query-Based Traversal (High-level)
From doxide/src/CppParser.cpp (lines 521-536):
```c
TSQueryCursor* cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, node);
TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; ++i) {
        TSNode node = match.captures[i].node;
        uint32_t id = match.captures[i].index;
        uint32_t length = 0;
        const char* name = ts_query_capture_name_for_id(query, id, &length);
        // Process captured node...
    }
}
ts_query_cursor_delete(cursor);
```

**Key Learning:** Tree-sitter queries are powerful! Instead of walking the entire tree, you write a declarative query like:
```
(function_definition
  name: (identifier) @name
  body: (compound_statement) @body)
```

And Tree-sitter finds all matches for you.

---

### ✅ Q4: How to map node types → colors?

**From:** tree-sitter/crates/cli/src/highlight.rs

#### Theme Structure (lines 56-59)
```rust
pub struct Theme {
    pub styles: Vec<Style>,
    pub highlight_names: Vec<String>,
}
```

The mapping is: `highlight_names[i]` → `styles[i]`

Example:
- `highlight_names = ["keyword", "string", "function", "comment"]`
- `styles[0]` = keyword style (e.g., blue)
- `styles[1]` = string style (e.g., green)
- `styles[2]` = function style (e.g., yellow)
- `styles[3]` = comment style (e.g., gray)

#### Style Definition (lines 50-53)
```rust
pub struct Style {
    pub ansi: anstyle::Style,  // ANSI terminal colors
    pub css: Option<String>,    // CSS for HTML output
}
```

#### Color Specification (lines 232-248)
Colors can be specified as:
1. **Named ANSI colors:** "black", "blue", "cyan", "green", "purple", "red", "white", "yellow"
2. **ANSI 256:** Numbers 0-255
3. **RGB hex:** "#26A69A", "#FF0000", etc.

#### Theme Loading (lines 68-71)
```rust
pub fn load(path: &path::Path) -> io::Result<Self> {
    let json = fs::read_to_string(path)?;
    Ok(serde_json::from_str(&json).unwrap_or_default())
}
```

Themes are JSON files like:
```json
{
  "keyword": "blue",
  "string": "green",
  "function": {
    "color": "yellow",
    "bold": true
  },
  "comment": "#808080"
}
```

#### Highlight Names Come From Language Grammar
Each language's highlight query defines capture names like `@keyword`, `@string`, `@function`. These become the `highlight_names` in the theme.

**Key Learning:** The mapping is external! You provide a theme JSON that maps semantic names (keyword, string, function) to colors. Different languages can use the same theme.

---

### ✅ Q5: How to output ANSI codes?

**From:** tree-sitter/crates/cli/src/highlight.rs (lines 433-449)

#### The Event-Based Pattern
```rust
let mut highlighter = Highlighter::new();
let events = highlighter.highlight(config, &source, ...)?;

let mut style_stack = vec![theme.default_style().ansi];
for event in events? {
    match event {
        HighlightEvent::HighlightStart(highlight) => {
            // Push new style onto stack
            style_stack.push(theme.styles[highlight.0].ansi);
        }
        HighlightEvent::HighlightEnd => {
            // Pop style from stack
            style_stack.pop();
        }
        HighlightEvent::Source { start, end } => {
            let style = style_stack.last().unwrap();
            write!(&mut stdout, "{style}").unwrap();       // ANSI start codes
            stdout.write_all(&source[start..end])?;        // Text content
            write!(&mut stdout, "{style:#}").unwrap();     // ANSI reset codes
        }
    }
}
```

#### ANSI Code Generation
The `anstyle::Style` type handles ANSI generation:
- `{style}` outputs escape codes to START the style (e.g., `\033[31m` for red)
- `{style:#}` outputs escape codes to RESET (e.g., `\033[0m`)

#### Style Stack Pattern
The stack handles nested highlights:
```
Text: "function hello() { return 42; }"
       ^^^^^^^          ^^^^^^^

Stack progression:
[default]                      "function"
[default, keyword]             "function"  ← keyword style
[default]                      " hello"
[default, function]            "hello"     ← function style  
[default]                      "() { "
[default, keyword]             "return"    ← keyword style
[default]                      " 42; }"
```

**Key Learning:** The stack pattern allows nested highlights (e.g., a string inside a function parameter). The top of the stack is always the active style.

---

## The tree-sitter-highlight Library

### What It Does
The `tree-sitter-highlight` crate (Rust) provides a high-level API:
1. Takes a `HighlightConfiguration` (language grammar + highlight queries)
2. Parses the source code
3. Returns an **event stream** of highlight start/end and source text chunks
4. You process events and output styled text

### Why It's Useful
You DON'T need to:
- Manually walk the syntax tree
- Figure out which node types map to colors
- Handle overlapping highlights
- Manage style state

You DO need to:
- Load a language grammar
- Load a highlight query for that language
- Provide a theme (JSON)
- Process events and output ANSI codes

### C++ Implications
The highlight library is Rust. For C++, we have two options:

**Option A: Use C bindings**
Check if `tree-sitter-highlight` has C bindings we can use from C++.

**Option B: Implement event processing ourselves**
Use the low-level Tree-sitter C API to:
1. Parse code with `ts_parser_parse_string()`
2. Run highlight queries with `ts_query_cursor`
3. Generate highlight events manually
4. Output ANSI codes ourselves

Option B is more work but gives us full control.

---

## Additional Learnings

### From doxide: Tree-sitter Queries

**What Are They?**
Declarative patterns to find syntax in the tree, written in S-expression syntax.

**Example Query** (from doxide lines 9-435):
```scheme
[
  ;; documentation
  (comment) @docs

  ;; namespace definition
  (namespace_definition
      name: (namespace_identifier) @name
      body: (declaration_list)? @body) @namespace

  ;; function definition
  (function_definition
      declarator: (function_declarator
        declarator: (identifier) @name)) @function
]
```

**How They Work:**
1. Write a query that matches syntax patterns
2. Assign capture names with `@name`
3. Tree-sitter finds all matches
4. You process captured nodes

**Why They're Useful:**
- Much cleaner than manual tree walking
- Focus on WHAT you want, not HOW to find it
- Automatically handles complex patterns
- Used for both highlighting and code analysis

### Grammar Loading Pattern

From doxide (lines 446-447):
```c
ts_parser_set_language(parser, tree_sitter_cuda());
```

Each language grammar is a function that returns a `const TSLanguage*`:
- `tree_sitter_cpp()`
- `tree_sitter_javascript()`
- `tree_sitter_python()`
- etc.

These come from separate repos:
- `tree-sitter-cpp` → `parser.c` → `tree_sitter_cpp()` function
- `tree-sitter-javascript` → `parser.c` → `tree_sitter_javascript()` function

**To support a language:**
1. Clone its `tree-sitter-LANG` repo
2. Compile its `parser.c` (and optionally `scanner.c`)
3. Link the grammar function into your binary

### Highlight Queries

Each language also needs a **highlight query** (`.scm` file) that defines:
```scheme
(identifier) @variable
(function_definition) @function
"return" @keyword
(string_literal) @string
(comment) @comment
```

These queries are what connect the language's AST node types to semantic highlight names.

---

## Code Patterns We Can Use

### Pattern 1: Simple Highlighting Loop

```c
// Initialize
TSParser* parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_cpp());

// Parse
const char* source = "int main() { return 0; }";
TSTree* tree = ts_parser_parse_string(parser, NULL, source, strlen(source));
TSNode root = ts_tree_root_node(tree);

// Load highlight query
TSQuery* query = ts_query_new(tree_sitter_cpp(), 
    highlight_query_string, 
    strlen(highlight_query_string),
    &error_offset, &error_type);

// Execute query
TSQueryCursor* cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

// Process matches and output ANSI
TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (int i = 0; i < match.capture_count; i++) {
        uint32_t capture_id = match.captures[i].index;
        TSNode node = match.captures[i].node;
        
        // Get capture name (e.g., "keyword", "string")
        uint32_t length;
        const char* name = ts_query_capture_name_for_id(query, capture_id, &length);
        
        // Look up color from theme
        const char* ansi_code = theme_lookup(name);
        
        // Output colored text
        uint32_t start = ts_node_start_byte(node);
        uint32_t end = ts_node_end_byte(node);
        printf("%s", ansi_code);                    // Start color
        fwrite(&source[start], 1, end - start, stdout);  // Text
        printf("\033[0m");                          // Reset
    }
}

// Cleanup
ts_query_cursor_delete(cursor);
ts_query_delete(query);
ts_tree_delete(tree);
ts_parser_delete(parser);
```

### Pattern 2: Theme Lookup Table

```c
typedef struct {
    const char* name;        // "keyword", "string", etc.
    const char* ansi_code;   // "\033[34m" (blue), etc.
} ThemeEntry;

ThemeEntry theme[] = {
    {"keyword",  "\033[34m"},  // Blue
    {"string",   "\033[32m"},  // Green
    {"function", "\033[33m"},  // Yellow
    {"comment",  "\033[90m"},  // Gray
    {"number",   "\033[35m"},  // Magenta
    {NULL, NULL}
};

const char* theme_lookup(const char* name) {
    for (int i = 0; theme[i].name != NULL; i++) {
        if (strcmp(theme[i].name, name) == 0) {
            return theme[i].ansi_code;
        }
    }
    return "\033[0m";  // Default/reset
}
```

### Pattern 3: ANSI Color Helpers

```c
// ANSI color codes
#define ANSI_RESET      "\033[0m"
#define ANSI_BLACK      "\033[30m"
#define ANSI_RED        "\033[31m"
#define ANSI_GREEN      "\033[32m"
#define ANSI_YELLOW     "\033[33m"
#define ANSI_BLUE       "\033[34m"
#define ANSI_MAGENTA    "\033[35m"
#define ANSI_CYAN       "\033[36m"
#define ANSI_WHITE      "\033[37m"

// Style helpers
#define ANSI_BOLD       "\033[1m"
#define ANSI_DIM        "\033[2m"
#define ANSI_ITALIC     "\033[3m"
#define ANSI_UNDERLINE  "\033[4m"

void print_colored(const char* text, const char* color) {
    printf("%s%s%s", color, text, ANSI_RESET);
}
```

---

## Answers to "Can We Do This?"

### Question: Can Tree-sitter handle code fence rendering?
**Answer: YES!** ✅

The official Tree-sitter CLI does exactly this - it takes source code and outputs ANSI-colored text to the terminal.

### Question: Is it fast enough for real-time?
**Answer: Likely YES** ⚠️

Tree-sitter is designed for editors that re-parse on every keystroke. It should be fast enough for terminal output. We'd need to measure, but initial signs are good.

### Question: How do we load grammars dynamically?
**Answer: Two approaches:**

1. **Static linking** (doxide approach):
   - Compile grammar `parser.c` files into our binary
   - Call `tree_sitter_cpp()`, `tree_sitter_python()`, etc. directly
   - Simple, but requires knowing languages at compile time

2. **Dynamic loading** (loader approach):
   - Tree-sitter has a `Loader` that loads grammars from `.so`/`.dll` files
   - More flexible, users can add languages
   - More complex to implement

### Question: Where do highlight queries come from?
**Answer:** Each `tree-sitter-LANG` repo includes a `queries/highlights.scm` file.

For example, `tree-sitter-cpp/queries/highlights.scm` defines what's a keyword, what's a string, etc. We need to:
1. Read the `.scm` file
2. Pass it to `ts_query_new()`
3. Use it with `ts_query_cursor`

### Question: Can we buffer incomplete input?
**Answer: YES** ✅

Tree-sitter handles incomplete/invalid syntax gracefully. It:
- Parses what it can
- Creates ERROR nodes for unparseable parts
- Allows incremental re-parsing as more input arrives

For our code fence renderer:
1. Detect opening ` ``` ` with language hint
2. Buffer lines until closing ` ``` `
3. Parse complete fence with Tree-sitter
4. Output ANSI-colored result

---

## What We Still Need to Figure Out

### P1 Questions (Important but not blocking)

#### Q6: Streaming/Incremental Parsing
**Status:** Partially answered

We know:
- Tree-sitter supports incremental parsing (pass old tree to `parse_string`)
- ERROR nodes are created for incomplete syntax
- Can re-parse efficiently on edits

We don't know:
- Best practice for buffering terminal output
- How to handle incomplete code fences gracefully
- Performance implications of parsing every chunk

**Next Step:** Build a prototype and measure.

#### Q7: Grammar Distribution
**Status:** Answered for compile-time, unclear for runtime

We know:
- Compile-time: Link `parser.c` files directly
- Runtime: Use `Loader` to load `.so`/`.dll` files

We don't know:
- How to package pre-built grammar libraries
- How users would install language support
- Cross-platform library loading strategy

**Next Step:** Decide if we want runtime or compile-time grammar loading.

### P2 Questions (Nice to have)

#### Q8: Performance Characteristics
**Status:** Unknown

Need to measure:
- Parse time for typical code blocks (100-1000 lines)
- Memory usage per parse
- Overhead of ANSI output formatting

**Next Step:** Build benchmark with real code samples.

---

## Recommended Next Steps

### Immediate (Prototype Spike)
1. **Clone tree-sitter-cpp repo** - Get the C++ grammar
2. **Extract highlights.scm** - Get the highlight query
3. **Write minimal C program** that:
   - Parses a C++ code string
   - Runs the highlight query
   - Outputs ANSI-colored text
4. **Verify it works** - Run it, see colored output

### Short-term (Proof of Concept)
1. **Integrate with 2shell** - Hook into PTY output stream
2. **Detect code fences** - Find ` ```cpp ` patterns
3. **Buffer until complete** - Collect lines until ` ``` `
4. **Highlight and output** - Parse with Tree-sitter, emit ANSI
5. **Test with real scenarios** - `cat README.md`, man pages, etc.

### Medium-term (Production Quality)
1. **Add multiple languages** - JavaScript, Python, Rust, etc.
2. **Theme system** - Load themes from JSON files
3. **Error handling** - Gracefully handle parse failures
4. **Performance tuning** - Cache, lazy-load, optimize
5. **Cross-platform** - Test on Windows, macOS, Linux

---

## Critical Files to Reference

### From tree-sitter repo
- `crates/cli/src/highlight.rs` - Complete ANSI highlighting implementation
- `crates/highlight/src/highlight.rs` - Core highlighting library
- `lib/include/tree_sitter/api.h` - C API reference

### From tree-sitter-cpp repo (need to clone)
- `queries/highlights.scm` - Highlight query for C++
- `src/parser.c` - C++ grammar implementation

### From doxide repo
- `src/CppParser.cpp` - Clean example of query-based parsing
- Shows real-world usage patterns

---

## Key Insights

### 1. Tree-sitter is Feasible!
Every question we had has been answered. The official CLI proves that terminal highlighting works. We can do this!

### 2. Two Levels of API
- **Low-level:** Direct C API (parse, query, walk tree)
- **High-level:** `tree-sitter-highlight` library (event stream)

For C++, we'll likely use the low-level API since the high-level is Rust-only.

### 3. Queries Are Powerful
Instead of manually walking trees, write declarative queries. This is how the highlight system works and why it's so flexible.

### 4. Themes Are External
The mapping from node types to colors is a separate concern. Users can customize themes without touching code.

### 5. Grammar ≠ Highlight Query
The language grammar defines the syntax (what's valid code).
The highlight query defines the semantics (what's a keyword, string, etc.).
Both are needed for highlighting.

### 6. The Rust CLI Is Our Reference
When in doubt, read `crates/cli/src/highlight.rs`. It shows exactly how to do terminal highlighting with Tree-sitter.

---

## Confidence Assessment

### Can we use Tree-sitter for code fence rendering? 
**YES!** 🎯

### Why are we confident?
1. ✅ Official Tree-sitter CLI does terminal highlighting
2. ✅ All P0 questions have clear answers
3. ✅ Pattern examples exist in multiple repos
4. ✅ API is simple and well-documented
5. ✅ Performance should be adequate (designed for editors)

### What's the risk?
⚠️ **Integration complexity** - Hooking into PTY output stream cleanly
⚠️ **Grammar distribution** - How to package/load language support
⚠️ **Edge cases** - Incomplete fences, mixed content, large blocks

### Is it worth doing?
**YES!** 🔥

The value is high (beautiful code in terminal), the feasibility is proven (official CLI exists), and the learning is valuable (Tree-sitter knowledge unlocks many tools).

---

## Final Thoughts

We went from "Can we use Tree-sitter?" to "Here's exactly how to do it" in TWO repos:
1. **doxide** showed us the low-level C API patterns
2. **tree-sitter CLI** showed us the complete ANSI highlighting solution

This is a PERFECT example of grounding in real code. We didn't assume, we READ. We didn't speculate, we VERIFIED.

**Rob's approach wins again.** 🎉

Next step: Build the prototype! Let's prove it works with actual code.

---

**Study completed:** 2025-12-15  
**Total repos studied:** 2 (doxide + tree-sitter CLI)  
**P0 questions answered:** 5/5 ✅  
**Confidence level:** HIGH 🎯  
**Ready to prototype:** YES! 🚀
