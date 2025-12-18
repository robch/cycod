# P0 Answers: JCWasmx86/GTKCssLanguageServer

**Quick reference for the 5 critical questions.**

---

## Q1: How to initialize parser? ✅

**8th confirmation - Same pattern in Vala:**

```vala
// Vala bindings (ts.vapi)
[CCode (cheader_filename = "tree_sitter/api.h")]
namespace TreeSitter {
    [CCode (cname = "tree_sitter_css")]
    public static TSLanguage tree_sitter_css();
    
    [Compact]
    [CCode (cname = "TSParser", free_function = "ts_parser_delete")]
    public class TSParser {
        [CCode (cname = "ts_parser_new")]
        public TSParser();
        
        [CCode (cname = "ts_parser_set_language")]
        public void set_language(TSLanguage tsl);
    }
}

// Usage (from load.c):
TSParser *get_parser() {
    static TSParser *parser = NULL;
    if (!parser) {
        parser = ts_parser_new();
        ts_parser_set_language(parser, tree_sitter_css());
    }
    return parser;
}

// Called from Vala:
var parser = get_parser();
```

**Key points:**
- Same C API underneath (`ts_parser_new`, `ts_parser_set_language`)
- Vala's `[CCode]` attributes bind to C functions
- Memory managed by `[free_function]` attribute
- Parser typically cached as singleton

---

## Q2: How to parse code? ✅

**8th confirmation - Same pattern in Vala:**

```vala
// Vala bindings
[CCode (cname = "ts_parser_parse_string")]
public TSTree? parse_string(TSTree? old_tree, string s, uint32 length);

// Usage (from parsecontext.vala):
var parser = get_parser();
var tree = parser.parse_string(null, text, text.length);

if (tree != null) {
    var root = tree.root_node();
    
    // Convert to custom AST
    this.sheet = to_node(root, text);
    
    // Free Tree-sitter tree (AST is independent)
    tree.free();
} else {
    // Parse failed (very rare)
    // Gracefully degrade: no LSP features
}
```

**Key points:**
- `parse_string(null, text, length)` - null means no old tree
- Returns `TSTree?` - nullable (can be null if parse fails)
- Always check for null before using
- Free tree when done (Vala: `tree.free()`, C: `ts_tree_delete(tree)`)

**This repo's unique approach:**
- Parse → Convert to custom AST → Free Tree-sitter tree
- Most repos keep Tree-sitter tree and work with it directly
- LSP needs persistent AST, so they convert immediately

---

## Q3: How to walk syntax tree? ✅

**8th confirmation - NEW APPROACH (manual traversal + AST):**

```vala
// Vala bindings for node access
[SimpleType]
[CCode (cname = "TSNode")]
public struct TSNode {
    [CCode (cname = "ts_node_child")]
    public TSNode child(uint index);
    
    [CCode (cname = "ts_node_type")]
    public unowned string type();
    
    [CCode (cname = "ts_node_child_count")]
    public uint32 child_count();
    
    [CCode (cname = "ts_node_start_byte")]
    public uint32 start_byte();
    
    [CCode (cname = "ts_node_end_byte")]
    public uint32 end_byte();
}

// Manual traversal + AST conversion (ast.vala):
public static Node to_node(TreeSitter.TSNode node, string text) {
    switch (node.type()) {
    case "stylesheet":
        return new StyleSheet(node, text);
    case "rule_set":
        return new RuleSet(node, text);
    case "declaration":
        return new Declaration(node, text);
    case "identifier":
        return new Identifier(node, text);
    // ... 50+ more cases ...
    default:
        return new ErrorNode(node, text);
    }
}

// Each AST class converts its children:
public class Declaration : Node {
    public Identifier name;
    public Node value;
    
    public Declaration(TreeSitter.TSNode t, string text) {
        this.init_from_node(t);  // Copy position info
        
        // Extract children by index
        var child0 = t.child(0);  // Property name
        var child1 = t.child(1);  // Value
        
        // Recursively convert
        this.name = to_node(child0, text) as Identifier;
        this.value = to_node(child1, text);
    }
}
```

**Approach comparison:**

| Repos 1-7 | Repo 8 (GTKCssLanguageServer) |
|-----------|-------------------------------|
| **Query-based traversal** | **Manual traversal + AST** |
| Execute query on tree | Walk tree manually |
| Get captures automatically | Extract children by index |
| Process capture nodes | Build custom AST |
| 10-20 lines of code | 1500+ lines of code |
| **Simple, declarative** | **Complex, imperative** |
| **Good for:** Highlighting, simple analysis | **Good for:** LSP, complex semantic analysis |

**For our highlighting project:** Queries are better (simpler, less code).

**Why this repo uses manual approach:**
- Need persistent AST (between LSP requests)
- Want parent pointers, metadata
- Complex semantic analysis (symbol resolution)
- Multiple analysis passes

**Example with visitor pattern:**
```vala
// Visitor interface
public interface ASTVisitor {
    public abstract void visit_declaration(Declaration d);
    public abstract void visit_identifier(Identifier i);
}

// AST nodes accept visitors
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
        // Track property usage
        this.property_uses.add(new PropertyUse() {
            name = d.name.id,
            range = d.range
        });
    }
}
```

---

## Q4: How to map node types → colors? ⚠️

**Not applicable** - This is a language server, not a syntax highlighter.

Language servers provide:
- Hover documentation
- Go-to-definition
- Diagnostics
- Symbol information

Syntax highlighting is done by the editor itself using its own mechanisms.

**For our highlighting project:** Use query captures + theme lookup (as in ltreesitter).

**The pattern we're using:**
```
1. Query: (string_literal) @string
2. Theme: {"string": "31"}  // Red
3. Lookup: color = theme["string"]
4. Output: "\x1b[31m" + text + "\x1b[0m"
```

---

## Q5: How to output ANSI codes? ⚠️

**Not applicable** - Language servers communicate via JSON-RPC over stdin/stdout, not colored terminal output.

**Example LSP response:**
```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "result": {
        "contents": {
            "kind": "markdown",
            "value": "CSS property: `color`\n\nSets the foreground color..."
        },
        "range": {
            "start": {"line": 10, "character": 5},
            "end": {"line": 10, "character": 10}
        }
    }
}
```

**For our highlighting project:** Use decoration table algorithm from ltreesitter:

```
Phase 1: Build decoration table
    For each query capture:
        color = theme[capture_name]
        For byte in capture.range:
            decoration[byte] = color

Phase 2: Output with ANSI codes
    For byte in source:
        If decoration[byte] != prev_color:
            Emit pending text
            Emit ANSI: "\x1b[" + color + "m"
```

---

## Summary: How This Repo Compares

### Same as Previous Repos (Repos 1-7)

✅ **Q1: Parser init** - Same C API: `ts_parser_new()` + `ts_parser_set_language()`  
✅ **Q2: Parsing** - Same C API: `ts_parser_parse_string()`  
✅ **Error handling** - Check for null tree, handle ERROR nodes

### Different from Previous Repos

⚠️ **Q3: Tree walking** - Manual traversal + AST conversion (not queries)  
❌ **Q4: Node → color** - N/A (language server, not highlighter)  
❌ **Q5: ANSI output** - N/A (JSON-RPC communication)

### Unique Contributions

1. **Manual traversal approach** - Shows alternative to queries
2. **Visitor pattern** - Clean way to analyze custom AST
3. **LSP integration** - How Tree-sitter fits into language servers
4. **Vala bindings** - 8th language binding (all use same C API)

### Key Insight

**This repo validates our query-based approach!**

- Manual approach: ~1500 lines for AST + analysis
- Query approach: ~20 lines for highlighting
- **For syntax highlighting, queries are clearly superior**

---

## For Our Project

**Use:**
- ✅ Parser init pattern (Q1)
- ✅ Parsing pattern (Q2)
- ✅ Error handling (check null tree)

**Don't use:**
- ❌ Manual traversal (queries are simpler)
- ❌ Custom AST (unnecessary for highlighting)
- ❌ Visitor pattern (overkill for our use case)

**Stick with:**
- ✅ Query-based approach (ltreesitter)
- ✅ Decoration table algorithm
- ✅ CMake build pattern (knut)

---

## Confirmation Count

After 8 repos studied:

| Question | Times Confirmed | Latest Answer Source |
|----------|----------------|----------------------|
| Q1: Parser init | 8 | Same C API (Vala bindings) |
| Q2: Parsing | 8 | Same C API |
| Q3: Tree walk | 8 | Manual (new!) vs. queries (7 repos) |
| Q4: Node → color | 5 (N/A × 3) | ltreesitter (queries + theme) |
| Q5: ANSI output | 5 (N/A × 3) | ltreesitter (decoration table) |

**New insight:** Manual traversal is viable but complex. Queries are simpler for highlighting.

---

## Recommendation

**STOP STUDYING. BUILD THE PROTOTYPE.**

We've now seen:
- 7 repos using queries → Simple, effective
- 1 repo using manual traversal → Complex, overkill for highlighting

**The evidence is clear: Queries are the right choice for syntax highlighting.**

---

**End of P0 Answers**
