# Journal Entry 002: Understanding 2shell - Our Starting Point

**Date:** 2025-12-15  
**Status:** 📚 Code Review Complete

## What is 2shell?

A **tmux-like terminal multiplexer** written in C++ that works on Windows (ConPTY) and Linux/macOS (traditional PTY).

**Key Features:**
- Creates up to 2 terminal sessions
- Ctrl+B to toggle between sessions
- RAW terminal mode for keystroke interception
- Cross-platform (Windows 10+ and Unix)

## Architecture Overview

### Clean Abstraction Layers

```
TerminalMultiplexer (main.cpp)
├── SessionManager (manages PTY sessions)
├── HotkeyDetector (detects Ctrl+B, Ctrl+C)
├── InputLogger (debug logging)
└── ITerminalDevice (platform-specific)
    ├── WindowsTerminalDevice (ConPTY)
    └── PosixTerminalDevice (traditional PTY)
```

### Key Interfaces

**`IPtySession`** - Abstract PTY session
```cpp
virtual bool start(const std::string& shell_command) = 0;
virtual ssize_t write_input(const uint8_t* data, size_t len) = 0;
virtual ssize_t read_output(uint8_t* buffer, size_t capacity) = 0;
virtual bool is_active() const = 0;
virtual void terminate() = 0;
```

**`ITerminalDevice`** - Abstract terminal I/O
```cpp
virtual bool initialize_raw_mode() = 0;
virtual void restore_normal_mode() = 0;
virtual ssize_t read_raw_input(uint8_t* buffer, size_t capacity) = 0;
virtual ssize_t write_output(const uint8_t* data, size_t len) = 0;
virtual void get_terminal_size(int& width, int& height) = 0;
```

## What We Can Reuse

### ✅ **The Good Stuff**

1. **PTY Management** - platform/windows_pty.cpp
   - ConPTY creation and management
   - Pipe setup (input/output)
   - Process spawning with PTY attached
   - Terminal size detection
   - Clean shutdown

2. **Cross-Platform Abstraction** - platform/pty_interface.h
   - Clean interfaces for PTY and Terminal
   - Factory methods: `create_pty_session()`, `create_terminal_device()`

3. **RAW Mode Management** - platform/windows_pty.cpp (WindowsTerminalDevice)
   - Save/restore console modes
   - RAW input reading without VT interference
   - Keystroke to VT sequence conversion

4. **Stream I/O Pattern**
   - Read from user input buffer
   - Write to PTY input
   - Read from PTY output
   - Write to terminal output

### ❌ **What We DON'T Need**

1. **Session Management** - We only need ONE session
2. **Hotkey Detection** - We don't toggle sessions
3. **Input Logger** - Debug-specific feature
4. **The main loop structure** - We need different processing

## Windows ConPTY Implementation Highlights

### PTY Creation (windows_pty.cpp:159-233)
```cpp
// 1. Create pipes for input/output
CreatePipe(&pty_input_read, &pty_input_write, ...)
CreatePipe(&pty_output_read, &pty_output_write, ...)

// 2. Get terminal size
COORD size = { 120, 30 };  // with dynamic detection

// 3. Create pseudo-console
CreatePseudoConsole(size, pty_input_read, pty_output_write, 0, &pseudo_console)

// 4. Set up process attributes
InitializeProcThreadAttributeList(...)
UpdateProcThreadAttribute(..., PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE, pseudo_console, ...)

// 5. Spawn process with PTY
CreateProcessW(nullptr, command, ..., EXTENDED_STARTUPINFO_PRESENT, ...)
```

### I/O Operations
```cpp
// Write to PTY (send keystrokes)
WriteFile(pty_input_write, data, len, &written, nullptr)

// Read from PTY (get output)
ReadFile(pty_output_read, buffer, capacity, &bytes_read, nullptr)
```

### Clean Shutdown
```cpp
// 1. Terminate child process
TerminateProcess(child_process.hProcess, 0)

// 2. Wait for process to exit
WaitForSingleObject(child_process.hProcess, 2000)

// 3. Close pseudo-console
ClosePseudoConsole(pseudo_console)

// 4. Close handles
CloseHandle(pty_input_write)
CloseHandle(pty_output_read)
CloseHandle(child_process.hProcess)
CloseHandle(child_process.hThread)
```

## What's Missing for Our Needs

### 🔴 Code Fence Detection
2shell has NO concept of parsing output content. It's a pure PTY proxy.

**We need:**
- Real-time stream parsing
- State machine for fence detection
- Buffer management for incomplete lines

### 🔴 Syntax Highlighting
2shell forwards output as-is, no transformations.

**We need:**
- Tree-sitter integration
- Parse tree to ANSI color conversion
- Language detection from fence info strings

### 🔴 Rendering Pipeline
2shell writes directly to stdout.

**We need:**
- Differentiate between normal output and fenced code
- Apply highlighting only to fenced regions
- Preserve original output for non-code content

## The Main Loop Pattern We Can Adapt

```cpp
// Current 2shell pattern (simplified)
while (true) {
    // 1. Read user input
    bytes = terminal->read_raw_input(buffer, sizeof(buffer));
    
    // 2. Process input (detect hotkeys)
    if (is_hotkey(buffer)) {
        handle_hotkey();
    } else {
        // Forward to PTY
        pty->write_input(buffer, bytes);
    }
    
    // 3. Read PTY output
    bytes = pty->read_output(buffer, sizeof(buffer));
    
    // 4. Write to terminal
    terminal->write_output(buffer, bytes);
}
```

**Our modified pattern:**
```cpp
while (true) {
    // 1. Read user input
    bytes = terminal->read_raw_input(buffer, sizeof(buffer));
    
    // 2. Detect Ctrl-C for fence termination
    if (in_fence && is_ctrl_c(buffer)) {
        stop_fence_rendering();
        continue;
    }
    
    // 3. Forward to PTY
    pty->write_input(buffer, bytes);
    
    // 4. Read PTY output
    bytes = pty->read_output(buffer, sizeof(buffer));
    
    // 5. PARSE OUTPUT FOR FENCES
    for each chunk:
        if (fence_detector.update(chunk)) {
            if (fence_detected) {
                // Start highlighting
                highlighted = tree_sitter.parse(chunk);
                terminal->write_output(highlighted);
            } else {
                // Pass through
                terminal->write_output(chunk);
            }
        }
}
```

## Key Differences from 2shell

| Feature | 2shell | Our Tool |
|---------|--------|----------|
| Sessions | 2 sessions, toggle | 1 session only |
| Output Processing | Pass-through | Parse & highlight |
| State Machine | Hotkey detection | Fence detection |
| Rendering | Raw output | ANSI-colorized |
| Language Support | N/A | Tree-sitter grammars |

## Technical Debt We're Avoiding

1. **Multiple sessions** - Complexity we don't need
2. **Session restoration** - 2shell saves/restores sessions, we don't
3. **Terminal emulation** - 2shell has ANSI processor, cell grid, virtual terminal - we can be simpler
4. **Input processing** - 2shell converts key events to VT sequences, we can pass through

## What We're Copying Wholesale

The `platform/` directory can be copied almost as-is:
- `pty_interface.h` - Clean interfaces ✅
- `windows_pty.cpp` - ConPTY implementation ✅
- `posix_pty.cpp` - Unix PTY implementation ✅

We'll strip out the terminal emulation (cell grid, ANSI processor) since we don't need it.

## Build System

2shell uses:
- **CMake** for cross-platform builds
- **C++17** standard
- Platform detection via `#ifdef _WIN32`

We can reuse this pattern or switch to C# (which might be easier for Tree-sitter bindings).

## Decision Point: C++ or C#?

**C++ Pros:**
- Direct reuse of 2shell PTY code
- Better performance for real-time parsing
- Tree-sitter is C, easier to bind

**C# Pros:**
- Easier Tree-sitter bindings via NuGet
- Better string handling
- Cleaner async/await for I/O
- Faster development

**Recommendation:** Start with **C#**, use P/Invoke for ConPTY if needed, or find existing C# PTY libraries.

## Next Steps

1. **Decision:** C++ or C#?
2. **If C++:** Copy `platform/` from 2shell
3. **If C#:** Research C# PTY libraries via cycodgr
4. **Spike:** Fence detection state machine
5. **Spike:** Tree-sitter integration

## Questions to Answer

- Can we find existing C# PTY libraries?
- How do we integrate Tree-sitter in C#?
- What's the best way to do real-time stream parsing?
- Should we buffer output or process byte-by-byte?

---

## My Take

2shell gives us a **fantastic starting point** for the PTY layer. The abstraction is clean, the Windows implementation is solid, and the cross-platform pattern is proven.

But we need to decide: **reuse the C++ PTY code directly, or find/build C# equivalents?**

Given our codebase is C#, I lean toward C# with P/Invoke or existing libraries. Let's use **cycodgr to find C# PTY examples!** 🔍

---

**Next entry:** Research C# PTY libraries and Tree-sitter C# bindings.
