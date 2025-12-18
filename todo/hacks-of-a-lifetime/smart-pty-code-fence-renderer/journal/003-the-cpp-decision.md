# Journal Entry 003: The C++ Decision

**Date:** 2025-12-15  
**Status:** ✅ **DECIDED** - Building in C++

## The Decision

We're building this in **C++**, not C#.

## Why C++?

### 1. **Speed** 🚀
Real-time stream parsing demands performance. Every byte that comes through the PTY needs:
- Fence detection (state machine)
- Optional parsing (Tree-sitter)
- Color transformation (ANSI codes)
- Output rendering

This happens **constantly** while commands run. C++ gives us the speed we need.

### 2. **Prove We Can** 💪
We have a C# codebase (cycod, cycodgr). But building legit tools in C++ proves we can work at the systems level. This is **hardcore** territory:
- PTY management
- Raw terminal control
- Real-time parsing
- Cross-platform systems programming

If we can pull this off, we can build ANYTHING.

### 3. **Direct Tree-sitter Integration** 🌳
Tree-sitter IS a C library (`libtree-sitter`). Using it from C++ is:
- Native (no FFI/P/Invoke overhead)
- Well-documented
- Fast
- Proven (used by Neovim, Atom, GitHub)

### 4. **Minimal Adaptation from 2shell** 🔄
2shell already gives us:
- ConPTY implementation (Windows)
- Traditional PTY (Unix)
- Cross-platform abstractions
- Build system (CMake)

We can **copy the `platform/` folder wholesale** and focus on the NEW stuff:
- Fence detection
- Tree-sitter parsing
- Syntax highlighting

## What We're NOT Doing

### ❌ C# with P/Invoke
**Why not:**
- FFI overhead on every byte
- String marshaling kills performance
- Harder to debug PTY issues
- Would need to wrap Tree-sitter anyway

### ❌ Hybrid C++/C# approach
**Why not:**
- Complexity of interop
- Two build systems
- Harder to distribute
- Performance questions at boundaries

## The Tree-sitter C API

Tree-sitter is **pure C** with a clean API:

```c
#include <tree_sitter/api.h>

// Create parser
TSParser *parser = ts_parser_new();

// Load language grammar
ts_parser_set_language(parser, tree_sitter_javascript());

// Parse source code
TSTree *tree = ts_parser_parse_string(
    parser, 
    NULL,           // old_tree (for incremental parsing)
    source_code, 
    strlen(source_code)
);

// Get root node
TSNode root_node = ts_tree_root_node(tree);

// Walk the tree
TSTreeCursor cursor = ts_tree_cursor_new(root_node);
ts_tree_cursor_goto_first_child(&cursor);

// Cleanup
ts_tree_cursor_delete(&cursor);
ts_tree_delete(tree);
ts_parser_delete(parser);
```

**Key features we'll use:**
- **Incremental parsing** - Parse chunks as they arrive
- **Tree walking** - Traverse nodes to find syntax elements
- **Node types** - Map node types to colors (e.g., `function_name` → green)
- **Multiple languages** - Load different grammars based on fence info string

## The cycodgr Strategy

Even though we're building in C++, we can **learn from ANY language**:

**Search strategy:**
1. Find C++ Tree-sitter integrations (direct patterns)
2. Find Rust syntax highlighters (concepts, clean implementation)
3. Find Go terminal tools (concurrent patterns)
4. Find Python examples (algorithms, clearer to read)

Then **translate the best ideas to C++**.

**The meta:** Use cycodgr to find the best implementations in ANY language, extract the patterns, build C++ optimized versions.

## What Makes This Exciting

We're building a tool that:
- Works at the **systems level** (PTY, terminal control)
- Does **real-time parsing** (streaming, state machines)
- Integrates **complex libraries** (Tree-sitter)
- Produces **beautiful output** (syntax highlighting)
- Works **cross-platform** (Windows, Mac, Linux)

This is **real systems programming**. Not web dev. Not CRUD apps. **The hard stuff.**

And we're documenting the ENTIRE journey so others can:
- Learn how we did it
- See our research process
- Understand our decisions
- Build their own versions

## Build Strategy

1. **Start with 2shell foundation** - Copy `platform/` directory
2. **Strip unnecessary parts** - Remove session management, multi-session code
3. **Add fence detection** - State machine for triple-backtick patterns
4. **Integrate Tree-sitter** - Parse fenced code blocks
5. **Add ANSI rendering** - Convert parse trees to colored output
6. **Test incrementally** - Each component in isolation, then together
7. **Polish** - Edge cases, error handling, cross-platform testing

## Success Metrics

**MVP (Minimum Viable Product):**
- Launch a shell (bash, pwsh)
- Detect triple-backtick fences in output
- Syntax highlight ONE language (e.g., JavaScript)
- Output colored code to terminal
- Ctrl-C stops rendering

**V1 (Full Feature):**
- Multiple languages via Tree-sitter grammars
- Auto-detect language from fence info string
- Handle incomplete fences gracefully
- Cross-platform (Windows + Linux tested)
- Proper cleanup and error handling

**V2 (Polish):**
- Custom fence markers (````, etc.)
- Configuration file (color schemes)
- Performance optimization
- Release builds and distribution

## Why This Will Work

1. **Proven foundation** - 2shell already works
2. **Well-documented library** - Tree-sitter has great docs
3. **Clear problem** - Fence detection is straightforward
4. **Incremental approach** - Build piece by piece
5. **cycodgr research** - Find reference implementations

We're not inventing new CS. We're **composing proven techniques** in a new way.

## The Learning Journey

This project will teach us:
- **PTY internals** - How terminals really work
- **Tree-sitter** - Incremental parsing, tree walking
- **State machines** - Real-time stream processing
- **ANSI codes** - Terminal color control
- **C++ systems programming** - Memory management, platform APIs
- **Cross-platform development** - Windows vs Unix differences

Every line of code = learning something new.

## Next: Finding the Right Examples

Before we search with cycodgr, we need to **define what makes a "good" example**:

**What we're looking for:**
- Tree-sitter integration that does syntax highlighting (not just parsing)
- Real-time or streaming parsing (not file-based)
- ANSI terminal output (not HTML/GUI)
- Production quality (not toy examples)
- Well-commented (we're learning)

**Red flags (what to avoid):**
- File-based only (we need streaming)
- GUI-focused (we need terminal)
- Incomplete (abandoned projects)
- Over-engineered (we want simple patterns)

**Next entry:** Defining the "fingerprint" of repos worth cloning and studying.

---

## My Take

C++ was the right call. We're going **deep** on this one.

Building something that:
- Actually matters (daily use)
- Technically challenging (systems level)
- Well-documented (the journey)
- Legitimately useful (not a toy)

This is going to be **epic**. 🔥

Let's find the best examples to learn from! 🔍
