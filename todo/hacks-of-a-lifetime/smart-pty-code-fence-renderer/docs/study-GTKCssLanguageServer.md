# Study Report: JCWasmx86/GTKCssLanguageServer

**Repo:** https://github.com/JCWasmx86/GTKCssLanguageServer  
**Study Date:** 2025-01-XX  
**Study Session:** 8 of 29 repos  
**Language:** Vala (binds to Tree-sitter C API)  
**LoC:** ~2,000 lines Vala  
**Study Value:** ⭐⭐⭐ (Alternative approach, but validates query-based method)

---

## Executive Summary

GTKCssLanguageServer is a production language server for GTK CSS used in GNOME Builder and Workbench. It uses Tree-sitter for parsing but takes a **manual traversal + AST conversion** approach rather than queries. This provides an interesting contrast to all previous repos.

**Key Insight:** This repo demonstrates that manual tree traversal + custom AST is viable but significantly more complex than query-based approaches. It **validates our decision to use queries for highlighting**.

**For our highlighting project:** Not directly useful (no highlighting code), but educational. Shows that queries are the right choice for our use case.

---

## What This Repo Is

### Purpose
A Language Server Protocol (LSP) implementation for GTK CSS that provides:
- Hover documentation for CSS properties and colors
- Go-to-definition for color references and animations
- Diagnostics (e.g., undefined animation names)
- Document symbols (colors, keyframes)

### Architecture
```
Input CSS → Tree-sitter Parse → Manual Tree Walk → Custom AST → Visitor Pattern → LSP Features
```

Contrast with highlighting approach:
```
Input Code → Tree-sitter Parse → Query Execution → Capture Names → Theme Lookup → ANSI Output
```

### Technology Stack
- **Language:** Vala (compiles to C)
- **Parser:** Tree-sitter with tree-sitter-css grammar
- **Build:** Meson
- **Integration:** GNOME Builder, Workbench

---

## Key Files Examined

### 1. `vapi/ts.vapi` - Vala Tree-sitter Bindings
```vala
[CCode (cheader_filename = "tree_sitter/api.h")]
namespace TreeSitter {
    [CCode (cname = "tree_sitter_css")]
    public static TSLanguage tree_sitter_css ();

    [Compact]
    [CCode (cname = "TSParser", free_function = "ts_parser_delete")]
    public class TSParser {
        [CCode (cname = "ts_parser_new")]
        public TSParser ();

        [CCode (cname = "ts_parser_set_language")]
        public void set_language (TSLanguage tsl);

        [CCode (cname = "ts_parser_parse_string")]
        public TSTree? parse_string (TSTree? old_tree, string s, uint32 length);
    }

    [Compact]
    [CCode (cname = "TSTree")]
    public class TSTree {
        [CCode (cname = "ts_tree_root_node")]
        public TSNode root_node ();

        [CCode (cname = "ts_tree_delete")]
        public void free ();
    }

    [SimpleType]
    [CCode (cname = "TSNode")]
    public struct TSNode {
        [CCode (cname = "ts_node_child")]
        public TSNode child (uint index);

        [CCode (cname = "ts_node_type")]
        public unowned string type ();

        [CCode (cname = "ts_node_child_count")]
        public uint32 child_count ();

        [CCode (cname = "ts_node_start_byte")]
        public uint32 start_byte ();

        [CCode (cname = "ts_node_end_byte")]
        public uint32 end_byte ();

        [CCode (cname = "ts_node_start_point")]
        public TSPoint start_point ();

        [CCode (cname = "ts_node_end_point")]
        public TSPoint end_point ();
    }

    public struct TSPoint {
        public uint32 row;
        public uint32 column;
    }
}
```

**Notes:**
- Clean Vala interface to C API
- Memory management via `[free_function]` attribute
- Same core functions we've seen in C/C++/Lua/Zig/OCaml
- Vala's object-oriented syntax makes it cleaner than raw C

---

### 2. `src/parsecontext.vala` - Main Parsing Logic

**Parser initialization and usage:**
```vala
internal class ParseContext {
    internal ParseContext (Diagnostic[] diags, string text, string uri) {
        // Get parser (externally initialized)
        var t = get_parser();
        
        // Parse string
        var tree = t.parse_string(null, text, text.length);
        
        if (tree != null) {
            // Get root and convert to custom AST
            var root = tree.root_node();
            this.sheet = to_node(root, text);
            
            // Set parent pointers for traversal
            this.sheet.set_parents();
            
            // Free Tree-sitter tree (AST is now independent)
            tree.free();
            
            // Extract semantic data via visitor pattern
            this.extractor = new DataExtractor(text);
            this.sheet.visit(this.extractor);
        }
        
        // Load documentation JSON files
        // ... documentation setup code ...
    }
}
```

**Key pattern:**
1. Parse with Tree-sitter
2. Convert Tree-sitter tree → Custom AST
3. Free Tree-sitter tree immediately
4. Work with custom AST for all subsequent operations

**Why this approach:**
- LSP features need persistent AST between requests
- Custom AST can have parent pointers, metadata
- Tree-sitter trees are immutable and harder to augment
- Queries not needed for their semantic analysis

---

### 3. `src/ast/ast.vala` - AST Conversion

**The conversion function:**
```vala
public static Node to_node (TreeSitter.TSNode node, string text) {
    debug("Converting node of type '%s' at [%u:%u]->[%u:%u]",
          node.type(),
          node.start_point().row, node.start_point().column,
          node.end_point().row, node.end_point().column);
    
    switch (node.type()) {
    case "stylesheet":
        return new StyleSheet(node, text);
    case "import_statement":
        return new ImportStatement(node, text);
    case "rule_set":
        return new RuleSet(node, text);
    case "declaration":
        return new Declaration(node, text);
    case "property_name":
    case "identifier":
        return new Identifier(node, text);
    // ... 50+ more cases ...
    default:
        return new ErrorNode(node, text);
    }
}
```

**Each AST node class:**
```vala
public class Declaration : Node {
    public Identifier name;
    public Node value;
    
    public Declaration(TreeSitter.TSNode t, string text) {
        this.init_from_node(t);  // Copy range, positions
        
        // Manually extract children by position
        var prop = t.child(0);  // Property name
        this.name = to_node(prop, text) as Identifier;
        
        var val = t.child(1);   // Value
        this.value = to_node(val, text);
    }
    
    public override void visit(ASTVisitor v) {
        v.visit_declaration(this);
        if (this.name != null) this.name.visit(v);
        if (this.value != null) this.value.visit(v);
    }
}
```

**Pattern:**
- Each Tree-sitter node type → Custom class
- Constructor converts TSNode → typed object
- Recursive conversion for children
- Visitor pattern for analysis

---

### 4. `src/ast/dataextractor.vala` - Semantic Analysis

**Visitor pattern for extracting semantic data:**
```vala
internal class DataExtractor : Object, ASTVisitor {
    public Gee.HashMap<string, Position> colors;
    public Gee.ArrayList<ColorReference> color_references;
    public Gee.ArrayList<PropertyUse> property_uses;
    public Gee.ArrayList<Keyframe> keyframes;
    
    public void visit_declaration(Declaration d) {
        // Track property uses
        this.property_uses.add(new PropertyUse() {
            name = d.name.id,
            range = d.range,
            node = d
        });
    }
    
    public void visit_define_color_statement(DefineColorStatement d) {
        // Track color definitions
        this.colors[d.color_name] = d.range.start;
    }
    
    public void visit_identifier(Identifier i) {
        // Check if identifier references a color
        if (this.colors.has_key(i.id)) {
            this.color_references.add(new ColorReference() {
                name = i.id,
                range = i.range
            });
        }
    }
    
    // ... more visit methods ...
}
```

**After extraction, use the data:**
```vala
internal Location? find_declaration(Position p) {
    // Go-to-definition for color references
    foreach (var color_ref in this.extractor.color_references) {
        if (color_ref.range.contains(p)) {
            var def_pos = this.extractor.colors[color_ref.name];
            return new Location() {
                uri = this.uri,
                range = new Range() {
                    start = def_pos,
                    end = def_pos
                }
            };
        }
    }
    return null;
}
```

---

## Architecture Comparison

### Query-Based Approach (ltreesitter, tree-sitter CLI)
```
Source Code
    ↓
Tree-sitter Parse → TSTree
    ↓
Query Execution → Captures
    ↓
For each capture:
    - Get capture name (e.g., "@keyword")
    - Look up theme color
    - Mark byte range in decoration table
    ↓
Output with ANSI codes
```

**Pros:**
- Simple: Queries are declarative
- Concise: 10-20 lines for basic highlighting
- Flexible: Edit queries without recompiling
- Fast: Tree-sitter optimizes query execution

**Cons:**
- Less control over traversal order
- Query language has learning curve
- Can't easily add custom logic mid-traversal

---

### Manual AST Approach (GTKCssLanguageServer)
```
Source Code
    ↓
Tree-sitter Parse → TSTree
    ↓
Manual Traversal:
    For each node:
        - Check node.type()
        - Create custom AST node
        - Recursively process children
    ↓
Custom AST with parent pointers, metadata
    ↓
Visitor Pattern:
    Walk AST with DataExtractor
    Extract semantic information
    ↓
Use data for LSP features
```

**Pros:**
- Full control over traversal and data structures
- Can add parent pointers, caching, metadata
- Custom AST persists between operations
- Arbitrary analysis logic

**Cons:**
- Lots of code: ~1,500 lines just for AST
- Maintenance: Keep AST in sync with grammar
- Performance: Extra allocation/conversion overhead
- Complexity: Harder to understand

---

## Key Learnings

### Learning 1: Manual Traversal Pattern ⭐⭐⭐

**When to use manual traversal:**
1. Building a language server (need persistent AST)
2. Complex semantic analysis (symbol resolution, type checking)
3. Multiple analysis passes (optimization, transformations)
4. Need parent pointers or bidirectional navigation

**When NOT to use it:**
1. Simple syntax highlighting (queries are better)
2. One-pass analysis (queries handle it)
3. Rapid prototyping (queries are faster to write)

**For our highlighting project:** Queries are the right choice. Manual traversal is overkill.

---

### Learning 2: Visitor Pattern for AST Analysis ⭐⭐⭐⭐

**The pattern:**
```vala
// Define visitor interface
public interface ASTVisitor {
    public abstract void visit_declaration(Declaration d);
    public abstract void visit_identifier(Identifier i);
    // ... one method per node type ...
}

// Each node implements visit()
public class Declaration : Node {
    public override void visit(ASTVisitor v) {
        v.visit_declaration(this);
        this.name.visit(v);   // Visit children
        this.value.visit(v);
    }
}

// Implement visitor for specific analysis
public class DataExtractor : ASTVisitor {
    public void visit_declaration(Declaration d) {
        // Extract property usage
    }
}
```

**Why this is elegant:**
- Separates traversal logic from analysis logic
- Can add new analyses without modifying AST classes
- Type-safe (compiler checks all cases)
- Reusable (same traversal, different visitors)

**Application to our project:** Not needed for highlighting, but good pattern to know for future tools.

---

### Learning 3: Incremental vs. Full Parsing ⭐⭐

**This repo's approach:**
```vala
// Always parse from scratch
var tree = t.parse_string(null, text, text.length);
                          // ↑ null = no old tree
```

**Incremental parsing (not used here):**
```vala
// Parse with old tree for efficiency
var new_tree = t.parse_string(old_tree, text, text.length);
```

**Why they use full parsing:**
- CSS files are small (<1000 lines usually)
- Full parse is fast enough (<10ms)
- Simpler code (no tree management)

**For our project:** 
- Code fences are tiny (10-100 lines)
- Always parse from scratch
- No need for incremental parsing

---

### Learning 4: Vala Bindings Show Same C API ⭐⭐

**The pattern (8th confirmation):**

| Language | Syntax | Underlying API |
|----------|--------|----------------|
| C | `ts_parser_new()` | Tree-sitter C API |
| C++ | `ts_parser_new()` | Tree-sitter C API |
| Rust | `Parser::new()` | Tree-sitter C API (wrapped) |
| Lua | `ffi.C.ts_parser_new()` | Tree-sitter C API (FFI) |
| Zig | `ts.ts_parser_new()` | Tree-sitter C API (FFI) |
| OCaml | `octs_create_parser_c_sharp()` | Tree-sitter C API (FFI) |
| Vala | `new TSParser()` | Tree-sitter C API |

**Lesson:** Every language uses the same C API underneath. Studying more bindings adds no value.

---

### Learning 5: Error Handling in Production Code ⭐⭐⭐

**Check for null trees:**
```vala
var tree = t.parse_string(null, text, text.length);
if (tree != null) {
    // Process tree
    tree.free();
} else {
    // Parse failed completely (very rare)
    // Silently skip - no AST, no features
}
```

**Graceful degradation:**
- If parse fails → No LSP features
- If node type unknown → ErrorNode
- If data not found → Return null

**For our project:**
- Check for null tree
- Handle ERROR nodes gracefully
- Don't crash on bad input

---

## P0 Questions: 8th Confirmation

### Q1: How to initialize parser? ✅ (8th time)

**Vala syntax:**
```vala
// In C helper (load.c):
TSParser *get_parser() {
    static TSParser *parser = NULL;
    if (!parser) {
        parser = ts_parser_new();
        ts_parser_set_language(parser, tree_sitter_css());
    }
    return parser;
}

// Called from Vala:
[CCode (cname = "get_parser")]
public static extern TreeSitter.TSParser get_parser();

// Usage:
var parser = get_parser();
```

**Same pattern as all previous repos.** Parser singleton, set language once.

---

### Q2: How to parse code? ✅ (8th time)

**Vala syntax:**
```vala
var tree = parser.parse_string(null, text, text.length);
if (tree != null) {
    var root = tree.root_node();
    // Process...
    tree.free();  // Manual cleanup
}
```

**Same pattern as all previous repos.** Parse string, get root, process, free.

---

### Q3: How to walk syntax tree? ✅ (8th time - NEW APPROACH!)

**This repo uses manual traversal:**
```vala
public static Node to_node(TreeSitter.TSNode node, string text) {
    switch (node.type()) {
    case "declaration":
        return new Declaration(node, text);
    // ... more cases ...
    }
}

public class Declaration : Node {
    public Declaration(TreeSitter.TSNode t, string text) {
        // Extract children manually
        var child0 = t.child(0);
        var child1 = t.child(1);
        
        // Recursively convert
        this.name = to_node(child0, text);
        this.value = to_node(child1, text);
    }
}
```

**Comparison:**
- **Repos 1-7:** Queries or simple recursion
- **Repo 8:** Full AST conversion with visitor pattern

**For highlighting:** Queries are still better (simpler, less code).

---

### Q4: How to map node types → colors? ⚠️ N/A

**Not applicable** - This is a language server, not a highlighter.

**For highlighting:** Use query captures + theme lookup (as in ltreesitter).

---

### Q5: How to output ANSI codes? ⚠️ N/A

**Not applicable** - Language servers provide data to editors, not colored output.

**For highlighting:** Use decoration table algorithm (as in ltreesitter).

---

## What This Repo Does NOT Provide

❌ **No syntax highlighting** - LSP provides semantics, not colors  
❌ **No ANSI output** - Communicates via JSON-RPC  
❌ **No query examples** - Uses manual traversal instead  
❌ **No theme system** - Not applicable to LSP  
❌ **No decoration table** - Different domain

**Conclusion:** Educational for understanding alternatives, but doesn't change our highlighting plan.

---

## Comparison to Previous Repos

| Aspect | ltreesitter (Repo 5) | GTKCssLanguageServer (Repo 8) |
|--------|----------------------|--------------------------------|
| **Purpose** | Syntax highlighting | Language server features |
| **Approach** | Query-based | Manual traversal + AST |
| **Complexity** | Low (136 lines) | High (~2000 lines) |
| **Tree handling** | Process directly | Convert to custom AST |
| **Analysis** | Single pass | Visitor pattern |
| **Best for** | Highlighting, simple analysis | Semantic analysis, LSP |

**For our project:** ltreesitter's approach is the right one.

---

## Session Meta-Analysis

### Study Value: ⭐⭐⭐ (Medium-High)

**Valuable:**
- ✅ Shows alternative approach (manual traversal + AST)
- ✅ Demonstrates visitor pattern elegantly
- ✅ Production code with good error handling
- ✅ Confirms queries are simpler for highlighting

**Not valuable:**
- ❌ No highlighting code
- ❌ No queries
- ❌ No ANSI output
- ❌ Approach is overkill for our use case

### Time Investment: ~60 minutes

- Cloning + initial exploration: 10 min
- Reading key files: 30 min
- Understanding architecture: 20 min

**Worth it?** Yes, but confirms we should stop studying and start building.

---

## Key Takeaway

**This repo validates our query-based approach!**

We've now seen:
- **7 repos using queries** (Repos 1-5, 7) → Simple, concise, effective
- **1 repo using manual traversal** (Repo 8) → Complex, lots of code, flexible but overkill

**For syntax highlighting:** Queries are clearly the right choice.

**The decision tree:**
```
Need syntax highlighting?
    ├─ YES → Use queries (10-20 lines)
    └─ NO → Building LSP or complex analyzer?
        ├─ YES → Consider manual AST (1000+ lines)
        └─ NO → Use queries anyway (simpler)
```

---

## Should We Study More Repos?

**❌ NO! THIS IS THE 8TH REPO!**

**What we've confirmed 8 times:**
1. Parser init: `ts_parser_new()` + `ts_parser_set_language()`
2. Parsing: `ts_parser_parse_string()`
3. Tree walking: Queries (7 repos) or manual (1 repo)
4. Highlighting: Query captures → theme → ANSI codes
5. All languages use the same C API

**New from Repo 8:**
- Manual traversal + AST approach
- Visitor pattern for analysis
- Validates that queries are simpler

**Repos studied so far:**
1. tree-sitter-issue-2012 (C) - Basic patterns ⭐⭐⭐
2. doxide (C++) - Queries ⭐⭐⭐⭐
3. tree-sitter CLI (Rust) - Official highlighter ⭐⭐⭐⭐⭐
4. c-language-server (C++) - Compile-time linking ⭐⭐⭐⭐
5. **ltreesitter (Lua/C) - THE ALGORITHM!** ⭐⭐⭐⭐⭐
6. zig-tree-sitter (Zig) - No value (bindings) ❌
7. **knut (C++/Qt) - THE ARCHITECTURE!** ⭐⭐⭐⭐⭐
8. GTKCssLanguageServer (Vala) - Alternative approach ⭐⭐⭐

**Hit rate:** 87.5% (7 valuable / 8 total)

**We have:**
- ✅ Perfect algorithm (decoration table - ltreesitter)
- ✅ Perfect architecture (CMake - knut)
- ✅ All P0 questions answered 8 times
- ✅ Query approach validated
- ✅ Manual approach confirmed as overkill
- ✅ Error handling patterns
- ✅ Production examples

**We DON'T need:**
- ❌ More repos
- ❌ More confirmation
- ❌ More alternatives

**🚀 TIME TO BUILD THE PROTOTYPE! 🚀**

---

## Recommended Next Actions

### Option 1: BUILD THE PROTOTYPE (RECOMMENDED!)

**Why:** We have everything. 8 repos is more than enough.

**Steps:**
1. Clone tree-sitter-cpp grammar
2. Translate ltreesitter's c-highlight.lua to C++
3. Use knut's CMake pattern
4. Test with sample C++ code
5. Iterate based on real results

**Time:** 2-3 hours

---

### Option 2: Study More Repos (DON'T!)

**Why not:**
- 8 repos studied
- All questions answered 8 times
- Binding repos add no value (proven twice)
- Manual approach validated as overkill
- Further study = procrastination

**If you must:**
- Avoid: Binding libraries (zig, OCaml, etc.)
- Avoid: Non-highlighting tools
- Maybe: Another production highlighter (but we already have tree-sitter CLI + ltreesitter)

---

## Files to Reference When Building

**Primary:**
- `external/ltreesitter/examples/c-highlight.lua` - THE algorithm ⭐⭐⭐⭐⭐
- `external/knut/3rdparty/CMakeLists.txt` - THE architecture ⭐⭐⭐⭐⭐
- `docs/study-ltreesitter.md` - Detailed algorithm documentation
- `docs/study-knut.md` - CMake and C++ patterns

**Secondary:**
- `docs/study-c-language-server.md` - Compile-time linking
- `docs/study-doxide-and-tree-sitter-cli.md` - Query examples
- `docs/study-GTKCssLanguageServer.md` - This document (what NOT to do)

---

## Conclusion

GTKCssLanguageServer demonstrates that manual tree traversal + custom AST is a viable approach for complex semantic analysis (language servers), but it's **significantly more complex** than query-based approaches.

**For syntax highlighting:** Queries are clearly superior (simpler, less code, faster to write).

**This study reinforces our decision to use ltreesitter's query-based decoration table algorithm.**

**Action item:** STOP STUDYING. START BUILDING.

---

**End of Study Report**
