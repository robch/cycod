# P0 Questions - Quick Reference (ltreesitter - Repo 5)

**Repo:** euclidianAce/ltreesitter  
**Key Discovery:** Perfect highlighting example (`c-highlight.lua`)  
**Status:** All 5 questions CONFIRMED + NEW algorithm discovered

---

## Q1: How to initialize parser? ✅ CONFIRMED

**Standard approach (same as Repos 1-4):**

```c
TSParser *parser = ts_parser_new();
TSLanguage const *lang = /* get language */;
ts_parser_set_language(parser, lang);
```

**NEW: Dynamic grammar loading (alternative to compile-time):**

```c
// Load .so/.dll at runtime
DynLibHandle handle = dlopen("c.so", RTLD_NOW | RTLD_LOCAL);
void *sym = dlsym(handle, "tree_sitter_c");

TSLanguage *(*lang_fn)(void) = (TSLanguage *(*)(void))sym;
TSLanguage const *lang = lang_fn();

// Verify version
uint32_t version = ts_language_version(lang);
if (version < TREE_SITTER_MIN_COMPATIBLE_LANGUAGE_VERSION ||
    version > TREE_SITTER_LANGUAGE_VERSION) {
    // Error: incompatible version
}

// Use with parser
ts_parser_set_language(parser, lang);
```

---

## Q2: How to parse code? ✅ CONFIRMED

**Same as before:**

```c
const char *source = "int main() { return 0; }";
TSTree *tree = ts_parser_parse_string(parser, NULL, source, strlen(source));

if (!tree) {
    // Catastrophic failure (very rare)
}

TSNode root = ts_tree_root_node(tree);
```

**Also supports UTF-16 encoding:**

```c
TSTree *tree = ts_parser_parse_string_encoding(
    parser, 
    old_tree,  // NULL for first parse
    source, 
    length,
    TSInputEncodingUTF8  // or UTF16LE, UTF16BE
);
```

---

## Q3: How to walk syntax tree? ✅ CONFIRMED

**Query-based iteration (standard for highlighting):**

**From c-highlight.lua:**

```lua
for cap, name in query:capture(tree:root()) do
    local color = ansi_colors[name]
    if color then
        for i = cap:start_index(), cap:end_index() do
            decoration[i] = color
        end
    end
end
```

**C equivalent:**

```c
TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; i++) {
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        uint32_t name_len;
        const char *name = ts_query_capture_name_for_id(
            query, capture_id, &name_len);
        
        // Process capture...
    }
}

ts_query_cursor_delete(cursor);
```

---

## Q4: How to map node types → colors? ✅ CONFIRMED

**Query captures → theme lookup:**

**From c-highlight.lua:**

```lua
-- Query maps syntax to semantic names
local query = c:query[[
    [ "return" "if" "else" ] @keyword
    (string_literal) @string
    (comment) @comment
]]

-- Theme maps semantic names to ANSI codes
local ansi_colors = {
    keyword = "35",   -- Magenta
    string = "31",    -- Red
    comment = "37",   -- White
}

-- Lookup during iteration
for cap, name in query:capture(root) do
    local color = ansi_colors[name]  -- <-- Theme lookup
    -- Apply color...
end
```

**C++ equivalent:**

```cpp
std::unordered_map<std::string, std::string> theme = {
    {"keyword", "35"},
    {"string", "31"},
    {"comment", "37"},
};

// During query iteration
std::string capture_name = /* from query */;
std::string color = theme[capture_name];
```

---

## Q5: How to output ANSI codes? ✅ CONFIRMED

**PERFECT example from c-highlight.lua:**

```lua
local csi = string.char(27) .. "["  -- ESC [

-- Start color
io.write(csi .. "31" .. "m")    -- Red: \x1b[31m

-- Write text
io.write(text)

-- Reset color
io.write(csi .. "0" .. "m")     -- Reset: \x1b[0m
```

**C++ equivalent:**

```cpp
// Start color
std::cout << "\x1b[31m";    // Red

// Write text
std::cout << text;

// Reset
std::cout << "\x1b[0m";
```

---

## 🌟 BONUS: The Decoration Table Algorithm

**NEW discovery from c-highlight.lua - THE algorithm we should use!**

### Phase 1: Build Decoration Table

```lua
local decoration = {}  -- Map: byte_index → ANSI_code

for cap, name in query:capture(tree:root()) do
    local color = ansi_colors[name]
    if color then
        for i = cap:start_index(), cap:end_index() do
            decoration[i] = color
        end
    end
end
```

**C++ equivalent:**

```cpp
std::unordered_map<uint32_t, std::string> decoration;

TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; i++) {
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        uint32_t name_len;
        const char *name = ts_query_capture_name_for_id(
            query, capture_id, &name_len);
        std::string capture_name(name, name_len);
        
        std::string color = theme[capture_name];
        if (!color.empty()) {
            uint32_t start = ts_node_start_byte(node);
            uint32_t end = ts_node_end_byte(node);
            
            for (uint32_t j = start; j < end; j++) {
                decoration[j] = color;
            }
        }
    }
}

ts_query_cursor_delete(cursor);
```

### Phase 2: Output with Colors

```lua
local previous_color
local last_emitted_index = 0

for i = 1, #contents do
    if decoration[i] ~= previous_color then
        -- Emit pending text
        if (last_emitted_index or 0) < i - 1 then
            io.write(contents:sub(last_emitted_index + 1, i - 1))
            last_emitted_index = i - 1
        end
        
        -- Emit color change
        if decoration[i] then
            io.write(csi .. decoration[i] .. "m")
        else
            io.write(csi .. "0m")
        end
        
        previous_color = decoration[i]
    end
end

-- Emit remaining text
if last_emitted_index < #contents then
    if previous_color then
        io.write(csi .. "0m")
    end
    io.write(contents:sub(last_emitted_index + 1, -1))
end
```

**C++ equivalent:**

```cpp
std::string previous_color;
uint32_t last_emitted = 0;

for (uint32_t i = 0; i <= source_len; i++) {
    auto it = decoration.find(i);
    std::string current_color = (it != decoration.end()) ? it->second : "";
    
    if (current_color != previous_color || i == source_len) {
        // Emit pending text
        if (last_emitted < i) {
            std::cout.write(&source[last_emitted], i - last_emitted);
        }
        
        // Emit color change (if not at end)
        if (i < source_len) {
            if (!current_color.empty()) {
                std::cout << "\x1b[" << current_color << "m";
            } else {
                std::cout << "\x1b[0m";
            }
        }
        
        previous_color = current_color;
        last_emitted = i;
    }
}

// Final reset
if (!previous_color.empty()) {
    std::cout << "\x1b[0m";
}
```

---

## Why This Algorithm is Perfect

1. **Simple** - Two clear phases, easy to understand
2. **Efficient** - Single pass through captures, single pass through source
3. **Handles overlaps** - Later captures overwrite earlier ones naturally
4. **Minimal state** - Just `previous_color` and `last_emitted`
5. **Correct ANSI** - Emits color changes only when needed
6. **Proven** - Working code in production Lua binding

---

## ANSI Color Codes Reference

From c-highlight.lua:

```
31 - Red           (numbers, strings, constants)
91 - Bright Red    (builtin constants)
92 - Bright Green  (functions)
96 - Bright Cyan   (types)
35 - Magenta       (keywords)
36 - Cyan          (operators)
37 - White         (punctuation, comments)
0  - Reset         (default color)
```

**Format:** `\x1b[<code>m`

**Examples:**
- `\x1b[31m` - Red foreground
- `\x1b[1;31m` - Bold red
- `\x1b[0m` - Reset all attributes

---

## Complete Workflow (Lua → C++ Translation)

**Lua (c-highlight.lua):**

```lua
local ts = require "ltreesitter"
local c = ts.require "c"
local parser = c:parser()
local query = c:query[[ (comment) @c ]]

local tree = parser:parse_string(source)
local decoration = {}

for cap, name in query:capture(tree:root()) do
    local color = ansi_colors[name]
    if color then
        for i = cap:start_index(), cap:end_index() do
            decoration[i] = color
        end
    end
end

-- Output colored text...
```

**C++ translation:**

```cpp
extern "C" TSLanguage *tree_sitter_cpp();

// 1. Setup parser
TSParser *parser = ts_parser_new();
ts_parser_set_language(parser, tree_sitter_cpp());

// 2. Load query
const char *query_src = "(comment) @c";
TSQuery *query = ts_query_new(tree_sitter_cpp(), query_src, 
                               strlen(query_src), &err_offset, &err_type);

// 3. Parse
TSTree *tree = ts_parser_parse_string(parser, NULL, source, strlen(source));
TSNode root = ts_tree_root_node(tree);

// 4. Build decoration table
std::unordered_map<uint32_t, std::string> decoration;
TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, root);

TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; i++) {
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        uint32_t name_len;
        const char *name = ts_query_capture_name_for_id(query, capture_id, &name_len);
        std::string capture_name(name, name_len);
        
        std::string color = theme[capture_name];
        if (!color.empty()) {
            uint32_t start = ts_node_start_byte(node);
            uint32_t end = ts_node_end_byte(node);
            for (uint32_t j = start; j < end; j++) {
                decoration[j] = color;
            }
        }
    }
}

// 5. Output colored text (see algorithm above)

// 6. Cleanup
ts_query_cursor_delete(cursor);
ts_query_delete(query);
ts_tree_delete(tree);
ts_parser_delete(parser);
```

---

## Key Files to Reference

- 📄 `examples/c-highlight.lua` - **THE perfect example**
- 📄 `csrc/language.c` - Dynamic loading (lines 79-143)
- 📄 `csrc/dynamiclib.c` - Cross-platform abstraction
- 📄 `csrc/query.c` - Query API (lines 283-300+)

---

## Summary

**All 5 P0 questions:** ✅ CONFIRMED (5th time!)

**NEW discoveries:**
1. ✅ Decoration table algorithm - Simple and perfect
2. ✅ Dynamic grammar loading - Alternative approach
3. ✅ Complete working example - c-highlight.lua
4. ✅ Two-phase approach - Build table, then output

**Status:** 100% ready to build prototype!

**Next step:** Translate c-highlight.lua to C++ → working highlighter!

---

*This is the 5th repo studied. Every question is answered. Time to CODE!* 🚀
