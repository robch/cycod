# 🚀 QUICK START - Build the Prototype

**Status:** Study phase COMPLETE. Build phase READY.  
**Time needed:** 2-3 hours  
**Difficulty:** Medium (straight translation of working code)

---

## What You're Building

A minimal C++ program that:
1. Parses C++ source code using Tree-sitter
2. Runs syntax highlighting query
3. Outputs ANSI colored text to terminal

**Input:** `./highlighter mycode.cpp`  
**Output:** Colored syntax in terminal (keywords blue, strings red, etc.)

---

## The Algorithm (From c-highlight.lua)

### Phase 1: Build Decoration Table
```
Map: byte_index → ANSI_color_code

For each query capture:
    color = theme[capture_name]
    For byte in capture range:
        decoration[byte] = color
```

### Phase 2: Output Colored Text
```
For byte in source:
    If color changed:
        Emit pending text
        Emit ANSI escape code
```

---

## Step-by-Step Guide

### Step 1: Read These First (30 min)

**MUST READ:**
1. `docs/study-ltreesitter.md` - Full analysis with THE algorithm
2. `external/ltreesitter/examples/c-highlight.lua` - THE code to translate
3. `docs/p0-answers-ltreesitter.md` - Quick algorithm reference

### Step 2: Clone Grammar (5 min)

```bash
cd todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/external
git clone https://github.com/tree-sitter/tree-sitter-cpp
```

### Step 3: Create Project (5 min)

```bash
cd todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer
mkdir -p spike
cd spike
```

Create these files:
- `CMakeLists.txt`
- `main.cpp`

### Step 4: Write CMakeLists.txt (10 min)

```cmake
cmake_minimum_required(VERSION 3.16)
project(highlighter)

# Find tree-sitter
find_package(PkgConfig REQUIRED)
pkg_check_modules(TREE_SITTER REQUIRED tree-sitter)

# Add executable
add_executable(highlighter 
    main.cpp
    ../external/tree-sitter-cpp/src/parser.c
)

# Include directories
target_include_directories(highlighter PRIVATE
    ${TREE_SITTER_INCLUDE_DIRS}
)

# Link libraries
target_link_libraries(highlighter 
    ${TREE_SITTER_LIBRARIES}
)

# C++17
set_property(TARGET highlighter PROPERTY CXX_STANDARD 17)
```

### Step 5: Write main.cpp (90-120 min)

**Translate c-highlight.lua line by line.**

**Structure:**

```cpp
#include <iostream>
#include <fstream>
#include <sstream>
#include <unordered_map>
#include <string>
#include <tree_sitter/api.h>

extern "C" TSLanguage *tree_sitter_cpp();

// Helper: Read file to string
std::string read_file(const char *path) {
    std::ifstream file(path);
    std::stringstream buffer;
    buffer << file.rdbuf();
    return buffer.str();
}

// Helper: Theme lookup
std::string theme_lookup(const std::string& name) {
    static std::unordered_map<std::string, std::string> theme = {
        {"keyword", "35"},       // Magenta
        {"string", "31"},        // Red
        {"number", "31"},        // Red
        {"function", "92"},      // Bright green
        {"type", "96"},          // Bright cyan
        {"comment", "37"},       // White
        {"operator", "36"},      // Cyan
        {"punctuation", "37"},   // White
    };
    
    auto it = theme.find(name);
    return (it != theme.end()) ? it->second : "";
}

int main(int argc, char **argv) {
    if (argc < 2) {
        std::cerr << "Usage: " << argv[0] << " <source.cpp>\n";
        return 1;
    }
    
    // 1. Setup parser
    TSParser *parser = ts_parser_new();
    ts_parser_set_language(parser, tree_sitter_cpp());
    
    // 2. Load query
    std::string query_source = read_file("../external/tree-sitter-cpp/queries/highlights.scm");
    uint32_t error_offset;
    TSQueryError error_type;
    TSQuery *query = ts_query_new(
        tree_sitter_cpp(),
        query_source.c_str(),
        query_source.length(),
        &error_offset,
        &error_type
    );
    
    if (!query) {
        std::cerr << "Query error at offset " << error_offset 
                  << ", type: " << error_type << "\n";
        ts_parser_delete(parser);
        return 1;
    }
    
    // 3. Parse source
    std::string source = read_file(argv[1]);
    TSTree *tree = ts_parser_parse_string(
        parser, NULL, source.c_str(), source.length()
    );
    
    if (!tree) {
        std::cerr << "Parse failed\n";
        ts_query_delete(query);
        ts_parser_delete(parser);
        return 1;
    }
    
    // 4. Build decoration table (PHASE 1)
    std::unordered_map<uint32_t, std::string> decoration;
    
    TSQueryCursor *cursor = ts_query_cursor_new();
    ts_query_cursor_exec(cursor, query, ts_tree_root_node(tree));
    
    TSQueryMatch match;
    while (ts_query_cursor_next_match(cursor, &match)) {
        for (uint16_t i = 0; i < match.capture_count; i++) {
            TSNode node = match.captures[i].node;
            uint32_t capture_id = match.captures[i].index;
            
            // Get capture name
            uint32_t name_len;
            const char *name = ts_query_capture_name_for_id(
                query, capture_id, &name_len
            );
            std::string capture_name(name, name_len);
            
            // Get color from theme
            std::string color = theme_lookup(capture_name);
            
            // Mark all bytes in this node's range
            if (!color.empty()) {
                uint32_t start = ts_node_start_byte(node);
                uint32_t end = ts_node_end_byte(node);
                
                for (uint32_t j = start; j < end; j++) {
                    decoration[j] = color;
                }
            }
        }
    }
    
    // 5. Output colored text (PHASE 2)
    std::string prev_color;
    uint32_t last_emitted = 0;
    
    for (uint32_t i = 0; i <= source.length(); i++) {
        // Get color for this byte
        auto it = decoration.find(i);
        std::string curr_color = (it != decoration.end()) ? it->second : "";
        
        // Color changed or end of source?
        if (curr_color != prev_color || i == source.length()) {
            // Emit pending text
            if (last_emitted < i) {
                std::cout.write(&source[last_emitted], i - last_emitted);
            }
            
            // Emit color change (if not at end)
            if (i < source.length()) {
                if (!curr_color.empty()) {
                    std::cout << "\x1b[" << curr_color << "m";
                } else {
                    std::cout << "\x1b[0m";  // Reset
                }
            }
            
            prev_color = curr_color;
            last_emitted = i;
        }
    }
    
    // Final reset
    if (!prev_color.empty()) {
        std::cout << "\x1b[0m";
    }
    
    // 6. Cleanup
    ts_query_cursor_delete(cursor);
    ts_query_delete(query);
    ts_tree_delete(tree);
    ts_parser_delete(parser);
    
    return 0;
}
```

### Step 6: Build (10 min)

```bash
mkdir build
cd build
cmake ..
make
```

**Common issues:**
- Tree-sitter not found: Install with `brew install tree-sitter` (macOS) or `apt install libtree-sitter-dev` (Ubuntu)
- Parser.c not found: Check path to `external/tree-sitter-cpp/src/parser.c`

### Step 7: Test (10 min)

Create test file:
```cpp
// test.cpp
#include <iostream>

int main() {
    std::string message = "Hello, World!";
    std::cout << message << std::endl;
    return 0;
}
```

Run:
```bash
./highlighter ../test.cpp
```

**Expected output:** Colored text with:
- Keywords (`int`, `return`) colored
- Strings (`"Hello, World!"`) colored
- Comments colored
- Identifiers in default color

---

## Success Criteria

- ✅ Compiles without errors
- ✅ Runs without crashing
- ✅ Outputs colored text
- ✅ Keywords are colored
- ✅ Strings are colored
- ✅ Comments are colored

---

## Troubleshooting

### Build fails: tree-sitter not found
**Solution:** Install tree-sitter development library
```bash
# macOS
brew install tree-sitter

# Ubuntu/Debian
sudo apt install libtree-sitter-dev

# Arch
sudo pacman -S tree-sitter
```

### Parse fails: tree_sitter_cpp not found
**Solution:** Make sure parser.c is in CMakeLists.txt
```cmake
add_executable(highlighter 
    main.cpp
    ../external/tree-sitter-cpp/src/parser.c  # ← Must be here
)
```

### Query error
**Solution:** Check highlights.scm path is correct
```cpp
std::string query_source = read_file(
    "../external/tree-sitter-cpp/queries/highlights.scm"
);
```

### No colors in output
**Solution:** 
1. Check theme_lookup() is returning values
2. Verify ANSI codes are being emitted
3. Test terminal supports ANSI colors: `echo -e "\x1b[31mRed\x1b[0m"`

---

## Next Steps After Prototype Works

1. **Document results** - Create `spike/RESULTS.md` with findings
2. **Test edge cases** - Try complex C++ files
3. **Measure performance** - Time parsing + highlighting
4. **Add more languages** - JavaScript, Python, etc.
5. **Integrate into 2shell** - PTY output interception

---

## Key Files Reference

**Must have open:**
- `external/ltreesitter/examples/c-highlight.lua` - THE reference
- `docs/study-ltreesitter.md` - Full explanation
- `docs/p0-answers-ltreesitter.md` - Quick reference

**Copy patterns from:**
- `external/c-language-server/CMakeLists.txt` - Build config
- `docs/study-ltreesitter.md` Section: "Code Snippets Ready to Use"

---

## Time Breakdown

- Reading documentation: 30 min
- Setup (clone, create files): 10 min
- Write CMakeLists.txt: 10 min
- Write main.cpp: 90-120 min
- Build and debug: 10-20 min
- Test and verify: 10 min

**Total: 2-3 hours**

---

## Remember

- This is a TRANSLATION job (Lua → C++)
- Follow c-highlight.lua line-by-line
- The algorithm is simple (two phases)
- Don't overthink it
- Trust the process

**You have THE perfect reference. Just translate it!**

---

*Created: 2025-12-15*  
*Study phase: COMPLETE*  
*Build phase: READY*  
*Confidence: 100%*
