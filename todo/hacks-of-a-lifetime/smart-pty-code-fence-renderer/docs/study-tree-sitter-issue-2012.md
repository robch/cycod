# Study: tree-sitter-issue-2012

**Date:** 2025-12-15  
**Repo:** https://github.com/sogaiu/tree-sitter-issue-2012  
**Purpose:** Bug reproduction case for Tree-sitter issue #2012  
**Language:** C  
**Tree-sitter Usage:** Basic parsing and tree traversal  

---

## Executive Summary

This is a **minimal, focused example** showing core Tree-sitter usage for parsing Clojure code. Perfect for learning the basics but doesn't demonstrate syntax highlighting or ANSI output.

**What it teaches:**
✅ Parser initialization  
✅ Parsing strings into syntax trees  
✅ Basic tree node access  
✅ How language grammars are compiled in  

**What it doesn't teach:**
❌ Syntax highlighting / color mapping  
❌ ANSI escape codes  
❌ Hierarchical tree traversal (only shows byte-based access)  

**Verdict:** Excellent starting point, but we need more repos for complete picture.

---

## Answers to P0 Questions

### Q1: How to Initialize Parser? ✅ ANSWERED

**Code pattern:**
```c
#include <tree_sitter/api.h>

// Declare the language grammar function
TSLanguage *tree_sitter_clojure();

int main() {
    // Create parser
    TSParser *parser = ts_parser_new();
    
    // Set language
    ts_parser_set_language(parser, tree_sitter_clojure());
    
    // Use parser...
    
    // Cleanup (not shown in example, but should exist)
    // ts_parser_delete(parser);
}
```

**Key findings:**
1. **Header:** `#include <tree_sitter/api.h>` - single header for all Tree-sitter functions
2. **Parser creation:** `ts_parser_new()` - allocates and returns `TSParser*`
3. **Language setup:** `ts_parser_set_language(parser, tree_sitter_LANGUAGE())`
4. **Language function:** Each language has a function like `tree_sitter_clojure()` returning `TSLanguage*`
5. **No error handling shown** - need to investigate if these can fail

**Memory management:**
- Parser is heap-allocated (returns pointer)
- Likely needs `ts_parser_delete(parser)` for cleanup
- Not demonstrated in this minimal example

---

### Q2: How to Parse Code? ✅ ANSWERED

**Code pattern:**
```c
// Source code to parse
char *source = 
    "(def x a)\n"
    "\n"
    "{def y 2}\n"
    "\n"
    "[def z 3]\n";

// Parse it
TSTree *tree = ts_parser_parse_string(parser, NULL, source, strlen(source));

// Use tree...

// Cleanup (should exist)
// ts_tree_delete(tree);
```

**Function signature:**
```c
TSTree *ts_parser_parse_string(
    TSParser *parser,
    const TSTree *old_tree,    // NULL for first parse, previous tree for incremental
    const char *source,
    uint32_t length
);
```

**Key findings:**
1. **Input:** Raw string of source code + length
2. **Previous tree:** Second parameter is `NULL` for initial parse
3. **Incremental parsing:** Pass previous tree for updates (optimization)
4. **Returns:** `TSTree*` - pointer to parsed syntax tree
5. **Error handling:** Not shown - can tree be NULL on parse error?

**For our use case:**
- We can parse code fence content as a string
- Can use incremental parsing for real-time updates
- Need to know source length upfront

---

### Q3: How to Walk Syntax Tree? ⚠️ PARTIALLY ANSWERED

**Code pattern:**
```c
// Get root node
TSNode root = ts_tree_root_node(tree);

// Check if node is valid
if (!ts_node_is_null(root)) {
    // Get node type
    const char *type = ts_node_type(root);
    
    // Get string representation
    const char *str = ts_node_string(root);
    
    printf("Node type: %s\n", type);
    printf("Node tree: %s\n", str);
}

// Traverse by byte position
for (int i = 0; i < source_length; i++) {
    TSNode child = ts_node_first_child_for_byte(root, i);
    if (!ts_node_is_null(child)) {
        printf("Child at byte %d: %s\n", i, ts_node_type(child));
    }
}
```

**Key findings:**
1. **Root node:** `ts_tree_root_node(tree)` returns `TSNode` (VALUE, not pointer!)
2. **Node validation:** `ts_node_is_null(node)` - check if node is valid
3. **Node type:** `ts_node_type(node)` - returns string like "sym_lit", "map_lit"
4. **Debug output:** `ts_node_string(node)` - returns S-expression representation
5. **Byte-based access:** `ts_node_first_child_for_byte(node, byte_offset)`

**Node types seen in Clojure example:**
- `source` - root node type
- `map_lit` - map literal `{...}`
- `sym_lit` - symbol literal
- `sym_name` - symbol name
- `num_lit` - numeric literal

**What's missing:**
- How to get child count: probably `ts_node_child_count(node)`
- How to get specific child: probably `ts_node_child(node, index)`
- How to get node position: probably `ts_node_start_byte()`, `ts_node_end_byte()`
- How to traverse recursively through all descendants
- How to get parent node

**For our use case:**
We need hierarchical traversal (visit all nodes) not byte-based access. This example shows Tree-sitter can do it, but not the full pattern.

---

### Q4: How to Map Node Types to Colors? ❌ NOT ANSWERED

**What we learned:**
- Node types are strings: "sym_lit", "map_lit", "num_lit", etc.
- These are language-specific (Clojure has different types than JavaScript)

**What we need to find:**
- How to decide: "function" → green, "string" → yellow, "keyword" → blue
- Is there a standard mapping or is it custom?
- Do repos use configuration files for color schemes?
- How to handle language differences?

**Not demonstrated in this repo** - it only prints node types, doesn't color them.

---

### Q5: How to Output ANSI Codes? ❌ NOT ANSWERED

**What we saw:**
```c
printf("Child at %i is %s %s\n", i, ts_node_type(child), ts_node_string(child));
```

Plain printf, no ANSI codes.

**What we need to find:**
- How to generate `\033[32m` (green) or `\x1b[32m`
- How to apply color to substring
- How to reset colors `\033[0m`
- How to handle nested colors

**Not demonstrated in this repo** - purely parsing, no coloring.

---

## Bonus Findings

### Q7: How are Language Grammars Compiled/Loaded? ✅ ANSWERED

**Build command from `build-repro.sh`:**
```bash
gcc \
  -Ilib/include -Ilib/src lib/src/lib.c \
  -I../tree-sitter-clojure/src ../tree-sitter-clojure/src/parser.c \
  -o ../main-repro \
  ../main.c
```

**Architecture:**
```
tree-sitter/
  lib/
    include/
      tree_sitter/api.h   ← Main API header
    src/
      lib.c               ← Core Tree-sitter implementation

tree-sitter-clojure/
  grammar.js             ← Grammar definition (input)
  src/
    parser.c             ← Generated parser (output)
```

**How it works:**
1. Grammar author writes `grammar.js` defining language syntax
2. Tree-sitter CLI generates `src/parser.c` from grammar
3. `parser.c` contains `tree_sitter_clojure()` function
4. Application compiles in both:
   - Tree-sitter core (`lib.c`)
   - Language parser (`parser.c`)
5. At runtime: `ts_parser_set_language(parser, tree_sitter_clojure())`

**Key insights:**
- **Static linking:** Grammar is compiled INTO executable
- **One function per language:** `tree_sitter_clojure()`, `tree_sitter_javascript()`, etc.
- **Separate repos:** Each language has own repo: `tree-sitter-LANGUAGE`
- **Generated code:** `parser.c` is auto-generated, not hand-written

**For our use case - two approaches:**

**Option A: Static linking (simpler)**
- Compile in all languages we want to support
- Larger binary but no runtime complexity
- Languages fixed at compile time

**Option B: Dynamic loading (flexible)**
- Load `.so`/`.dll` files at runtime
- Smaller binary, more complexity
- Users can add new languages without recompile

This example uses static linking (Option A).

---

## Code Analysis

### Full main.c Walkthrough

```c
#include <string.h>
#include <stdio.h>
#include <tree_sitter/api.h>

// External declaration of language grammar function
// This function is defined in tree-sitter-clojure/src/parser.c
TSLanguage *tree_sitter_clojure();

// Helper to print source code (for debugging)
void print_src(char *source) {
  printf("\n---------\n");
  // Print source in chunks (manual formatting)
  char form1[11] = { 0 };
  strncpy(form1, source, 10);
  printf("%s", form1);

  char form2[12] = { 0 };
  strncpy(form2, source+10, 11);
  printf("%s", form2);

  char form3[13] = { 0 };
  strncpy(form3, source+21, 12);
  printf("%s", form3);
  printf("---------\n\n");
  return;
}

int main() {
  // STEP 1: Create parser
  TSParser *parser = ts_parser_new();
  
  // STEP 2: Set language (Clojure)
  ts_parser_set_language(parser, tree_sitter_clojure());

  // STEP 3: Define source code
  char *source =
    "(def x a)\n"  // 0-9
    "\n"           // 10
    "{def y 2}\n"  // 11-20
    "\n"           // 21
    "[def z 3]\n"; // 22-31

  print_src(source);
  
  // STEP 4: Parse source into syntax tree
  TSTree *tree = ts_parser_parse_string(parser, NULL, source, strlen(source));

  // STEP 5: Get root node
  TSNode root = ts_tree_root_node(tree);
  TSNode child;

  // STEP 6: Traverse by byte position
  // (This is testing a specific Tree-sitter feature for the bug report)
  for (int i = 0; i < (int)strlen(source); ++i) {
    child = ts_node_first_child_for_byte(root, i);
    if (!ts_node_is_null(child)) {
      printf("Child at %i is %s %s\n", i, ts_node_type(child), ts_node_string(child));
    } else {
      printf("Child at %i is null\n", i);
    }
  }
  
  // STEP 7: Print entire parse tree for debugging
  printf("\nParse tree:\n%s\n", ts_node_string(root));
  
  // Missing: Cleanup
  // ts_tree_delete(tree);
  // ts_parser_delete(parser);
  
  return 0;
}
```

**What's excellent:**
- Simple, focused example
- Shows complete parse workflow
- Easy to understand and modify

**What's missing:**
- No cleanup (memory leaks in this example)
- No error handling
- Only shows byte-based traversal, not hierarchical
- No syntax highlighting

---

## Performance Observations

**From this example:**
- Parse seems fast (no buffering or async shown)
- Tree structure is lightweight (`TSNode` is a value, not pointer)
- String representations are generated on-demand (`ts_node_string`)

**Questions for other repos:**
- How fast is parsing for large files?
- Can we parse incrementally as data streams in?
- What's memory overhead per parse?

---

## Comparison to Our Needs

### What We're Building
Smart PTY that detects code fences and syntax highlights them:

```
$ cat README.md
```markdown
```javascript
function hello() {
  console.log("world");
}
```
```

Output should show `function`, `console`, `log` in different colors.

### How This Example Helps

✅ **We now know:**
1. How to create a parser
2. How to parse a string
3. How to access nodes and their types
4. How grammars are compiled in

❌ **We still need:**
1. How to map node types to colors
2. How to generate ANSI codes
3. Better tree traversal patterns
4. How to handle multiple languages dynamically

---

## Next Steps

### Immediate
1. ✅ Document this repo (you are here!)
2. 🔜 Pick another repo that does syntax highlighting
3. 🔜 Look for ANSI terminal output examples
4. 🔜 Find hierarchical tree traversal patterns

### For Complete Understanding
- Study a terminal highlighter (if one exists)
- Study an editor with Tree-sitter (Neovim, Helix)
- Study Tree-sitter's own examples/tests
- Compare 2-3 repos to find common patterns

### Questions to Answer Next
- What node types exist for common languages (JS, Python, C++)?
- Is there a standard color scheme or is it per-tool?
- How do tools handle unknown/new languages?
- Can we stream parse (update tree as PTY outputs)?

---

## Files to Reference

**In this repo:**
- `main.c` - Complete working example
- `build-repro.sh` - Build process
- `README` - Bug context

**Related Tree-sitter repos:**
- `tree-sitter/tree-sitter` - Core library
- `tree-sitter/tree-sitter-clojure` - Clojure grammar

---

## Key Takeaways

1. **Tree-sitter is straightforward** - Basic usage is ~10 lines of code
2. **Grammars are separate** - Each language needs its own parser.c
3. **API is C-style** - Simple functions, no heavy OOP
4. **Nodes are lightweight** - `TSNode` is a value type (good for performance)
5. **Type strings are language-specific** - "sym_lit" in Clojure, "function_declaration" in JS

**This repo proves:** Tree-sitter is feasible for our use case!

**But we need:** Real syntax highlighting examples to complete the picture.

---

## Code Patterns We Can Use

### Pattern 1: Initialize Parser
```c
TSParser *parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_javascript());
```

### Pattern 2: Parse Code
```c
TSTree *tree = ts_parser_parse_string(parser, NULL, code, strlen(code));
```

### Pattern 3: Access Nodes
```c
TSNode root = ts_tree_root_node(tree);
const char *type = ts_node_type(root);
if (!ts_node_is_null(root)) {
    // Process node
}
```

### Pattern 4: Cleanup (inferred, not shown)
```c
ts_tree_delete(tree);
ts_parser_delete(parser);
```

---

## Confidence Level

**For implementing our tool:**

| Question | Confidence | Notes |
|----------|-----------|-------|
| Q1: Parser init | 🟢 High | Clear and simple |
| Q2: Parse code | 🟢 High | Straightforward |
| Q3: Walk tree | 🟡 Medium | Need better traversal patterns |
| Q4: Type→Color | 🔴 Low | Not demonstrated |
| Q5: ANSI output | 🔴 Low | Not demonstrated |
| Q7: Grammar loading | 🟢 High | Understand compile-in approach |

**Overall:** Good start, but need 1-2 more repos to answer remaining questions.

---

## Search Strategy Update

**What to look for in next repo:**
- Terminal-based tool (not GUI)
- Actually does syntax highlighting
- Generates ANSI escape codes
- Preferably C/C++ (easier to translate)
- Not too large/complex

**Candidates from our list:**
- Check repos with "highlight" or "syntax" in name
- Look for terminal utilities
- Avoid massive codebases (Neovim)

---

**End of Study**

This was an excellent first repo! Minimal, clear, and demonstrates core Tree-sitter usage. Ready to study next repo for the missing pieces (coloring + ANSI output).
