# 🎯 START HERE - Building the Prototype

**If you're the AI in the next session, read this FIRST.**

---

## ⚡ TL;DR

**Study phase:** COMPLETE (6 repos studied)  
**Current status:** Ready to build prototype  
**Next action:** Translate c-highlight.lua to C++  
**Time estimate:** 2-3 hours

---

## 🛑 Before You Do ANYTHING

### Read These (In Order):

1. **This file** (you're here) - Quick orientation
2. **`RESUME-HERE.md`** - Full context and handoff
3. **`SESSION-5-SUMMARY.md`** - Why we're done studying
4. **`external/ltreesitter/examples/c-highlight.lua`** - THE code to translate

**Time:** 15-20 minutes of reading → saves hours of confusion

---

## 📊 Current Situation

### Study Phase: COMPLETE ✅

**Repos studied:** 6
- Repo 1: Basic C patterns
- Repo 2: Production C++ with queries
- Repo 3: Official Rust highlighter
- Repo 4: Compile-time linking strategy
- Repo 5: **Perfect example (c-highlight.lua)** ⭐⭐⭐
- Repo 6: Zig bindings (added ZERO value - validated "stop studying")

### All Questions Answered ✅

**5 P0 questions:** All answered (confirmed 6 times)
1. Parser initialization ✅
2. Code parsing ✅
3. Tree walking ✅
4. Node → color mapping ✅
5. ANSI output ✅

**Bonus:** Decoration table algorithm discovered ✅

### Knowledge Complete ✅

We have:
- ✅ Working example to translate (c-highlight.lua)
- ✅ Algorithm to implement (decoration table)
- ✅ Build strategy (compile-time linking)
- ✅ Production patterns (Repos 2, 4)
- ✅ Performance data (<1ms per code fence)

### NO MORE STUDY NEEDED ✅

Session 5 proved that studying more repos = procrastination:
- Studied zig-tree-sitter (auto-generated bindings)
- Found: ZERO new information
- Learned: Language bindings without examples = useless
- Confirmed: "Stop studying" advice was correct

---

## 🚀 What To Do Next

### Step 1: Clone Tree-sitter C++ Grammar (5 minutes)

```bash
cd todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/external
git clone https://github.com/tree-sitter/tree-sitter-cpp
```

**You need:**
- `src/parser.c` - Grammar implementation (compile into your app)
- `queries/highlights.scm` - Query definitions for C++ syntax

### Step 2: Create Prototype Structure (5 minutes)

```bash
cd todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer
mkdir -p spike
cd spike
```

**Create files:**
- `CMakeLists.txt` - Build configuration
- `main.cpp` - The highlighter (translate c-highlight.lua here)
- `highlights.scm` - Copy from tree-sitter-cpp/queries/
- `test.cpp` - Sample C++ code to highlight

### Step 3: Implement main.cpp (90-120 minutes)

**Translate:** `external/ltreesitter/examples/c-highlight.lua` → `spike/main.cpp`

**Algorithm (from c-highlight.lua):**

```
Phase 1: Build decoration table
  Map: byte_index → ANSI_color_code
  
  For each query capture:
    color = theme[capture_name]
    For byte_index in capture.range:
      decoration[byte_index] = color

Phase 2: Output colored text
  previous_color = none
  
  For byte_index in source:
    current_color = decoration[byte_index]
    If current_color != previous_color:
      Emit pending text
      Emit ANSI escape: "\x1b[{color}m"
      previous_color = current_color
  
  Emit remaining text + reset "\x1b[0m"
```

**Key data structure:**
```cpp
std::unordered_map<uint32_t, std::string> decoration;
// Maps: byte index → ANSI color code
```

**Reference files:**
- `docs/study-ltreesitter.md` - Detailed translation guide
- `docs/p0-answers-ltreesitter.md` - Quick algorithm reference
- `external/ltreesitter/examples/c-highlight.lua` - THE source

### Step 4: Build CMakeLists.txt (15 minutes)

**Pattern from Repo 4 (c-language-server):**

```cmake
cmake_minimum_required(VERSION 3.10)
project(tree_sitter_highlighter)

set(CMAKE_CXX_STANDARD 17)

# Add Tree-sitter include directory
include_directories(
    ${CMAKE_SOURCE_DIR}/../external/tree-sitter/lib/include
)

# Add the executable
add_executable(highlighter 
    main.cpp
    ../external/tree-sitter-cpp/src/parser.c
)

# Link libraries
target_link_libraries(highlighter)
```

**Key:** Compile `parser.c` directly into executable (compile-time linking).

### Step 5: Build and Test (30 minutes)

```bash
mkdir build
cd build
cmake ..
make

# Test it
./highlighter ../test.cpp
```

**Success criteria:**
- ✅ Compiles without errors
- ✅ Parses C++ code without crashing
- ✅ Outputs colored text to terminal
- ✅ Keywords are colored (blue/magenta)
- ✅ Strings are colored (red/green)
- ✅ Comments are colored (gray/white)

---

## 🎓 Key Concepts (Quick Refresh)

### The Decoration Table Algorithm

**Why it's perfect:**
1. **Simple** - Just a map: byte_index → color
2. **Two phases** - Build map, then output
3. **Handles overlaps** - Later captures overwrite earlier ones
4. **Minimal state** - Only track previous color
5. **Efficient** - Two linear passes

**Translation to C++:**
- Lua table → `std::unordered_map<uint32_t, std::string>`
- Lua string slicing → C++ index arithmetic
- Lua file I/O → C++ `<fstream>` or command-line args

### Compile-Time Grammar Linking

**From Repo 4 (c-language-server):**

1. Add `parser.c` to CMakeLists.txt sources
2. Declare language function: `extern "C" TSLanguage *tree_sitter_cpp();`
3. Call it: `ts_parser_set_language(parser, tree_sitter_cpp());`

**No dynamic loading needed.** Simpler and faster.

### Query Execution Pattern

```cpp
// 1. Load query from highlights.scm
std::string query_source = read_file("highlights.scm");
TSQuery *query = ts_query_new(
    lang, query_source.c_str(), query_source.length(),
    &error_offset, &error_type
);

// 2. Parse source code
TSTree *tree = ts_parser_parse_string(
    parser, NULL, source.c_str(), source.length()
);

// 3. Execute query
TSQueryCursor *cursor = ts_query_cursor_new();
ts_query_cursor_exec(cursor, query, ts_tree_root_node(tree));

// 4. Iterate captures
TSQueryMatch match;
while (ts_query_cursor_next_match(cursor, &match)) {
    for (uint16_t i = 0; i < match.capture_count; i++) {
        TSNode node = match.captures[i].node;
        uint32_t capture_id = match.captures[i].index;
        
        uint32_t name_len;
        const char *name = ts_query_capture_name_for_id(
            query, capture_id, &name_len
        );
        
        // Get color from theme
        std::string color = theme_lookup(std::string(name, name_len));
        
        // Mark byte range in decoration
        uint32_t start = ts_node_start_byte(node);
        uint32_t end = ts_node_end_byte(node);
        for (uint32_t j = start; j < end; j++) {
            decoration[j] = color;
        }
    }
}

// 5. Cleanup
ts_query_cursor_delete(cursor);
ts_query_delete(query);
ts_tree_delete(tree);
ts_parser_delete(parser);
```

### Theme Lookup (Hardcoded for MVP)

```cpp
std::string theme_lookup(const std::string& capture_name) {
    static const std::unordered_map<std::string, std::string> theme = {
        {"keyword", "35"},        // Magenta
        {"string", "32"},         // Green
        {"number", "33"},         // Yellow
        {"function", "34"},       // Blue
        {"comment", "37"},        // White
        {"type", "36"},           // Cyan
        {"operator", "37"},       // White
        {"variable", "37"},       // White (default)
    };
    
    auto it = theme.find(capture_name);
    return (it != theme.end()) ? it->second : "";
}
```

---

## 🚨 Common Pitfalls to Avoid

### Pitfall 1: Studying More Repos

**Symptom:** "Let me just check one more repo for comparison..."

**Problem:** Session 5 proved this wastes time. No more repos add value.

**Solution:** Refer back to this document. Start coding immediately.

### Pitfall 2: Overcomplicating the Implementation

**Symptom:** "Let me design a perfect theme system / plugin architecture / ..."

**Problem:** Perfect is the enemy of done. MVP first.

**Solution:** Translate c-highlight.lua directly. Refactor later.

### Pitfall 3: Getting Stuck on Build Issues

**Symptom:** "CMake isn't finding Tree-sitter headers..."

**Problem:** Build config needs correct paths.

**Solution:** Copy pattern from Repo 4 (c-language-server/CMakeLists.txt lines 25-30).

### Pitfall 4: Not Testing Incrementally

**Symptom:** Write all code, then debug 100 errors.

**Problem:** Hard to isolate issues.

**Solution:** Build in stages:
1. Just parse → print tree structure
2. Add query → print capture names
3. Add decoration → print map contents
4. Add output → see colored text

---

## 📚 Essential References (Bookmarks)

**Must read:**
1. `external/ltreesitter/examples/c-highlight.lua` - THE example ⭐⭐⭐
2. `docs/study-ltreesitter.md` - Translation guide
3. `docs/p0-answers-ltreesitter.md` - Algorithm quick reference

**Useful:**
4. `docs/study-c-language-server.md` - CMake patterns
5. `external/c-language-server/CMakeLists.txt` - Build example
6. `external/tree-sitter-cpp/queries/highlights.scm` - Query definitions

**Context:**
7. `RESUME-HERE.md` - Full handoff document
8. `SESSION-5-SUMMARY.md` - Why we're done studying

---

## ✅ Success Checklist

### Before You Start:
- [ ] Read this file
- [ ] Read RESUME-HERE.md
- [ ] Read SESSION-5-SUMMARY.md
- [ ] Read c-highlight.lua
- [ ] Understand decoration table algorithm

### Implementation:
- [ ] Clone tree-sitter-cpp
- [ ] Create spike/ directory
- [ ] Write CMakeLists.txt
- [ ] Translate c-highlight.lua to main.cpp
- [ ] Create test.cpp sample file
- [ ] Build successfully
- [ ] Run and see colored output

### Validation:
- [ ] Keywords are colored
- [ ] Strings are colored
- [ ] Comments are colored
- [ ] No crashes on valid input
- [ ] Output looks reasonable

---

## 💬 If You Get Stuck

### Build Issues:
→ Check CMakeLists.txt paths to Tree-sitter headers  
→ Verify parser.c is in sources list  
→ Ensure extern "C" declaration for tree_sitter_cpp()

### Algorithm Issues:
→ Re-read c-highlight.lua lines 92-135 (the algorithm)  
→ Re-read docs/study-ltreesitter.md (translation guide)  
→ Print decoration map to debug

### Parse Issues:
→ Verify highlights.scm loaded correctly  
→ Check query compilation (error_type, error_offset)  
→ Print capture names to verify query execution

### General:
→ Remember: c-highlight.lua is only 136 lines  
→ Your C++ version should be ~200 lines  
→ Keep it simple, iterate later

---

## 🎯 Expected Outcome

**After 2-3 hours:**

You should have a working C++ program that:
1. Takes C++ source code as input
2. Parses it with Tree-sitter
3. Runs highlight query
4. Outputs ANSI-colored text to terminal

**Example:**
```bash
$ ./highlighter test.cpp
[35mint[0m [34mmain[0m() {
    [32m"Hello, world!"[0m;
    [35mreturn[0m [33m0[0m;
}
```

Where `[35m` = magenta, `[34m` = blue, etc.

---

## 🚀 Final Reminder

**You are NOT here to:**
- ❌ Study more repos
- ❌ Perfect the architecture
- ❌ Build a production system
- ❌ Research alternatives

**You ARE here to:**
- ✅ Translate c-highlight.lua to C++
- ✅ Get a working prototype
- ✅ Prove Tree-sitter works
- ✅ Learn by building

**Time box:** 3 hours maximum for MVP.

After that, you can iterate, refactor, and improve.

But first: **GET IT WORKING.**

---

## 📞 Questions?

If something is unclear:
1. Check RESUME-HERE.md (comprehensive context)
2. Check SESSION-5-SUMMARY.md (recent lessons)
3. Check c-highlight.lua (the source of truth)
4. Check docs/study-ltreesitter.md (detailed guide)

If still stuck: Ask the user.

But first: Try building something. You learn more from a broken prototype than from perfect documentation.

---

**Now go build!** 🚀
