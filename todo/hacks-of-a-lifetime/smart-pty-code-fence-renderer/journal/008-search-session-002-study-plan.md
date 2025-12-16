# Search Session 002: Study Plan - What Do We Need to Learn?

**Date:** 2025-12-15  
**Status:** 📋 PLANNING - Before we clone repos, know what we're looking for

---

## The Questions We Need Answered

### Question 1: How do you initialize and configure Tree-sitter?

**What we need to know:**
- How to create a parser (`TSParser`)
- How to load language grammars (JavaScript, Python, C++, etc.)
- Where do grammar files come from? (Dynamic loading? Compiled in?)
- Memory management (creation, cleanup)

**Evidence/Fingerprints to look for:**
```c
ts_parser_new()
ts_parser_set_language()
tree_sitter_javascript()  // Language grammar functions
ts_parser_delete()
```

**Success = We can write:**
```c
TSParser *parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_javascript());
// ... use parser ...
ts_parser_delete(parser);
```

---

### Question 2: How do you parse code into a syntax tree?

**What we need to know:**
- Parse a string of code
- Handle incomplete input (streaming?)
- Incremental parsing (update existing tree)
- Error handling when parse fails

**Evidence/Fingerprints:**
```c
ts_parser_parse_string()
ts_parser_parse()  // For streaming input?
TSTree *tree
ts_tree_delete()
```

**Success = We can write:**
```c
const char *code = "function hello() { }";
TSTree *tree = ts_parser_parse_string(parser, NULL, code, strlen(code));
// ... use tree ...
ts_tree_delete(tree);
```

---

### Question 3: How do you walk the syntax tree?

**What we need to know:**
- Get root node
- Traverse children
- Get node type (identifier, function, string, etc.)
- Get node position (line, column, byte offset)

**Evidence/Fingerprints:**
```c
ts_tree_root_node()
ts_node_child_count()
ts_node_child()
ts_node_type()
ts_node_start_byte()
ts_node_end_byte()
TSTreeCursor  // For efficient traversal?
```

**Success = We can write:**
```c
TSNode root = ts_tree_root_node(tree);
for (uint32_t i = 0; i < ts_node_child_count(root); i++) {
    TSNode child = ts_node_child(root, i);
    const char *type = ts_node_type(child);
    // ... process node ...
}
```

---

### Question 4: How do you map node types to colors?

**What we need to know:**
- What node types exist? (function_name, string_literal, keyword, etc.)
- How to decide "this type = this color"
- Is there a standard mapping? Or custom per-language?
- Do repos use config files for color schemes?

**Evidence/Fingerprints:**
```c
ts_node_type() -> const char*
// Look for: if (strcmp(type, "function") == 0) -> use green
// Look for: Color mapping tables/arrays
// Look for: Config file parsing for themes
```

**Success = We can write:**
```c
const char *type = ts_node_type(node);
if (strcmp(type, "function") == 0) {
    return COLOR_GREEN;
} else if (strcmp(type, "string") == 0) {
    return COLOR_YELLOW;
}
// ... etc
```

---

### Question 5: How do you convert colors to ANSI codes?

**What we need to know:**
- ANSI escape sequence format
- How to apply color to a substring
- Reset codes (so colors don't bleed)
- Handling nested colors (if needed)

**Evidence/Fingerprints:**
```c
"\033[" or "\x1b["  // ANSI escape start
"31m"               // Red
"32m"               // Green
"0m"                // Reset
printf("\033[32m%s\033[0m", text);  // Green text
```

**Success = We can write:**
```c
void print_colored(const char *text, Color color) {
    printf("\033[%dm%s\033[0m", color_code(color), text);
}
```

---

### Question 6: How do you handle streaming/incremental parsing?

**What we need to know:**
- Can Tree-sitter parse partial input?
- How to update tree as new data arrives
- How to handle incomplete syntax (mid-function, mid-string)
- Performance implications of re-parsing

**Evidence/Fingerprints:**
```c
ts_parser_parse_string(parser, old_tree, ...)  // Pass old tree
TSInput // Custom input callback?
ts_parser_set_timeout_micros()
```

**Success = We understand:**
- Whether streaming is practical
- If we need to buffer until complete lines
- How to recover from incomplete input

---

### Question 7: How are language grammars distributed/loaded?

**What we need to know:**
- Are grammars compiled into executables?
- Loaded dynamically from .so/.dll files?
- Need separate tree-sitter-javascript, tree-sitter-python repos?
- How to add new language support?

**Evidence/Fingerprints:**
```c
extern const TSLanguage *tree_sitter_javascript(void);
dlopen() or LoadLibrary()  // Dynamic loading
#include "tree_sitter/javascript.h"
```

**Success = We know:**
- What files we need for each language
- How to compile/include them
- How users add new languages

---

### Question 8: What's the performance like?

**What we need to know:**
- Fast enough for real-time terminal output?
- Memory usage per parse
- Can we parse every chunk of output or need to throttle?

**Evidence/Fingerprints:**
```c
// Look for: Caching strategies
// Look for: Performance comments
// Look for: Buffering before parsing
```

**Success = We understand:**
- If Tree-sitter is fast enough
- If we need optimization strategies

---

## Prioritized Questions

### Must Answer (P0)
1. ✅ How to initialize parser (Q1)
2. ✅ How to parse code (Q2)
3. ✅ How to walk tree (Q3)
4. ✅ How to map types to colors (Q4)
5. ✅ How to output ANSI (Q5)

**These are the CORE.** Can't proceed without them.

### Should Answer (P1)
6. ⚠️ Streaming/incremental (Q6)
7. ⚠️ Grammar distribution (Q7)

**Important but can work around initially.**

### Nice to Know (P2)
8. 💡 Performance characteristics (Q8)

**Optimization can come later.**

---

## What Makes a Repo "Good to Study"?

### Green Flags ✅
- **Terminal output** - Uses ANSI codes, not HTML/GUI
- **Complete examples** - Has working code we can run
- **Well-commented** - Explains WHY, not just WHAT
- **Simple architecture** - Easy to understand flow
- **Multiple languages** - Shows grammar loading pattern
- **Recent activity** - Maintained, modern practices

### Red Flags 🚩
- Embedded in huge codebase (Neovim - too complex)
- GUI-focused (no terminal output)
- Incomplete/experimental
- No examples or tests to run
- Over-abstracted (can't see the Tree-sitter calls)

---

## How to Study Each Repo

### Phase 1: Quick Triage (5 min per repo)
1. Read README - what does it do?
2. Find main.c/main.cpp - entry point
3. Search for `ts_parser_new` - where's the Tree-sitter code?
4. Check if it outputs ANSI - is it terminal-focused?

**Decide:** Clone and study deeply, or skip?

### Phase 2: Deep Study (30 min per repo)
For repos we clone:

1. **Find initialization code**
   - Where does `ts_parser_new()` happen?
   - How are grammars loaded?

2. **Find parsing code**
   - Where does `ts_parser_parse_string()` happen?
   - What input does it take?

3. **Find tree walking code**
   - How do they traverse the tree?
   - What node types do they care about?

4. **Find color mapping code**
   - Where's the type → color logic?
   - Config file? Hardcoded? Theme system?

5. **Find ANSI output code**
   - How do they generate escape codes?
   - How do they apply colors to text?

6. **Document findings**
   - Create docs/study-{repo-name}.md
   - Answer our questions
   - Extract code patterns we can use

### Phase 3: Extract Patterns (20 min)
After studying 2-3 repos:

1. Compare approaches
2. Identify common patterns
3. Document "best practices" we observed
4. Write pseudocode for our implementation

---

## Repo Selection Criteria

From our 29 repos (treesitter-users.txt), prioritize:

### Tier 1: Perfect Matches
- Terminal tools that highlight code
- C/C++ implementations
- Simple, focused codebases
- Active maintenance

### Tier 2: Good Learning
- Rust/Go implementations (translate patterns to C++)
- Editors with clean Tree-sitter integration
- Well-documented code

### Tier 3: Skip (For Now)
- Huge codebases (Neovim, Helix core)
- GUI-only tools
- Language bindings (not usage examples)
- Experimental/incomplete

---

## Success Criteria for This Session

**We've succeeded when:**

✅ We can answer questions 1-5 (P0 questions)
✅ We have code examples for each pattern
✅ We've documented findings in docs/
✅ We know enough to write a spike implementation
✅ We've identified any blockers/unknowns

**Bonus:**
🎯 Answered Q6-7 (streaming, grammars)
🎯 Found a simple repo we can almost copy from
🎯 Discovered unexpected patterns/approaches

---

## Next: Repo Selection

Now that we know WHAT to look for, let's:

1. Review treesitter-users.txt
2. Pick 2-3 repos that best match our criteria
3. Clone them to external/
4. Start answering our questions!

**Ready to pick repos?** 🔍
