# Smart PTY with Code Fence Renderer

**Status:** 🚀 **STARTING** - The Journey Begins  
**Date:** 2025-12-15

## The Vision

A multi-platform PTY (pseudo-terminal) manager that:
1. Launches ANY program as a shell (bash, powershell, cmd, python REPL, etc.)
2. Sits in the middle, intercepting output streams
3. Detects code fences in real-time (triple backticks or longer sequences)
4. Renders fenced code with syntax highlighting using Tree-sitter
5. Stops rendering on: matching close fence OR user Ctrl-C/termination
6. Works on Windows, macOS, and Linux

## The Magic Moment

```bash
$ cat README.md
```

Instead of seeing raw markdown with ugly backticks, you see:
- Beautiful syntax-highlighted code blocks
- Proper indentation and colors
- Language-specific highlighting (detected from fence info strings)
- Seamless integration with ANY tool that outputs code

It's like having a **markdown renderer built into your terminal** for ANY command output!

## Use Cases

### Daily Developer Scenarios
- `cat README.md` → Fenced code blocks render beautifully
- `curl https://api.example.com/docs` → JSON/code examples highlighted
- `git diff` → Syntax-highlighted diffs with fence detection
- `npm run test` → Test output with code snippets highlighted
- `python` REPL → Docstrings with code fences render properly
- ANY CLI tool output → Automatic code prettification

### The "Why This Matters" Story
Developers spend hours in terminals. We `cat` files, `curl` APIs, read logs, debug output. When that output contains code (and it often does), we're reading PLAIN TEXT. 

This tool makes the terminal **understand code** in real-time, just like your editor or browser does.

## Technical Challenges

### 1. Cross-Platform PTY Management
**Challenge:** Different PTY implementations across OSes
- **Windows:** ConPTY (Windows 10+)
- **Unix/Linux/macOS:** Traditional PTY via /dev/pts

**What We Need to Learn:**
- How to spawn processes with PTY on each platform
- How to intercept/proxy output streams
- How to preserve interactivity (stdin/stdout/stderr)
- Signal handling (Ctrl-C, Ctrl-Z, Ctrl-D, etc.)

**Existing Reference:**
- `C:\src\2shell` has base PTY functionality
- Need to extract and understand the core patterns

### 2. Real-Time Code Fence Detection
**Challenge:** Detect fences in streaming output (not files)

**State Machine Needed:**
```
NORMAL → (detect ```) → IN_FENCE → (detect ```) → NORMAL
                    ↓
                (detect Ctrl-C) → STOP
```

**Edge Cases:**
- Incomplete fences (stream cuts off mid-fence)
- Nested fences (rare but possible)
- False positives (backticks that aren't fences)
- Language detection from info strings (```javascript, ```python, etc.)
- Non-standard fence markers (````, ``````, etc.)

### 3. Tree-sitter Integration
**Challenge:** Use Tree-sitter for syntax highlighting in real-time

**What We Need:**
- Stream-based parsing (not file-based)
- Language grammar loading based on fence info string
- Incremental parsing as chunks arrive
- Conversion of parse tree to ANSI color codes

**Questions:**
- Does Tree-sitter work well with streaming input?
- How do we map Tree-sitter node types to colors?
- Performance implications of real-time parsing?

### 4. Terminal Rendering
**Challenge:** Output syntax-highlighted code to terminal

**Requirements:**
- Generate ANSI escape codes for colors
- Preserve original output when NOT in fence
- Handle terminal resizing
- Cross-platform terminal control
- Don't break existing terminal features (scrollback, copy/paste, etc.)

**Libraries to Research:**
- ANSI color code generation
- Terminal size detection
- Cursor control for live updates

## Base Code Available

**Location:** `C:\src\2shell`

**What It Has:**
- PTY management (likely Windows-focused via ConPTY)
- Process spawning
- Basic I/O handling

**What It Lacks:**
- Code fence detection
- Tree-sitter integration
- Syntax highlighting
- Real-time stream parsing

## The cycodgr Connection

This project IS the killer demo for cycodgr's three-level filtering!

**To build this, we need to use cycodgr to find:**

### Phase 1: PTY Management Patterns
```bash
# Find projects doing cross-platform PTY
cycodgr --repo-contains "pty terminal cross-platform" \
        --file-contains "ConPty" \
        --max-results 20 \
        --save-repos pty-examples.txt
```

### Phase 2: Tree-sitter Integration
```bash
# Find projects using Tree-sitter for syntax highlighting
cycodgr --repo-contains "tree-sitter syntax highlighting" \
        --cs-file-contains "TreeSitter" \
        --line-contains "Parse" \
        --max-results 20
```

### Phase 3: Stream Parsing Patterns
```bash
# Find real-time stream processing with state machines
cycodgr --repo-contains "stream parser state machine" \
        --cs-file-contains "IAsyncEnumerable" \
        --line-contains "yield return" \
        --max-results 20
```

### Phase 4: Terminal Rendering
```bash
# Find ANSI color code generation
cycodgr --repo-contains "ansi terminal color" \
        --cs-file-contains "Console.Write" \
        --line-contains "\\u001b\\[" \
        --max-results 20
```

**The Meta:** We're using cycodgr to build itself a killer demo app!

## Architecture Vision

```
User Terminal
     ↓
Smart PTY Manager (our app)
     ↓
  ┌─────────────────────┐
  │  Stream Interceptor │
  │   (output watcher)  │
  └─────────────────────┘
     ↓
  ┌─────────────────────┐
  │  Fence Detector     │
  │  (state machine)    │
  └─────────────────────┘
     ↓
  ┌─────────────────────┐
  │  Tree-sitter Parser │
  │  (syntax highlight) │
  └─────────────────────┘
     ↓
  ┌─────────────────────┐
  │  ANSI Renderer      │
  │  (colorized output) │
  └─────────────────────┘
     ↓
Original Process (bash, pwsh, etc.)
```

## Success Criteria

### Minimum Viable Product (MVP)
- ✅ Launch a shell (bash on Linux/Mac, pwsh on Windows)
- ✅ Detect triple-backtick fences in output
- ✅ Syntax highlight detected fences using Tree-sitter
- ✅ Output highlighted code to terminal with ANSI codes
- ✅ Handle Ctrl-C to stop rendering/kill process

### V2 Features
- Support custom fence markers (````, etc.)
- Auto-detect language without info string
- Handle nested fences
- Configuration file for color schemes
- Plugin system for custom renderers

### V3 Features
- Split-screen mode (original + rendered side-by-side)
- Recording/playback of sessions
- Export rendered output as HTML/PDF
- Integration with existing terminals (plugin mode)

## Next Steps

1. **Examine 2shell code** - Understand existing PTY implementation
2. **Research via cycodgr** - Find reference implementations
3. **Spike: Fence Detection** - Build simple state machine
4. **Spike: Tree-sitter** - Test syntax highlighting in C#
5. **Integration** - Combine PTY + Detection + Highlighting
6. **Cross-platform Testing** - Validate on Windows/Mac/Linux
7. **Polish** - Error handling, edge cases, UX

## Why This Will Be EPIC

**For Us:**
- Solves a real problem we have (reading code in terminals)
- Perfect showcase for cycodgr's power
- Fun technical challenge (PTY + parsing + rendering)
- Multi-platform systems programming

**For Others:**
- Every developer uses terminals daily
- Code-in-output is universal (READMEs, API docs, logs, diffs)
- "I didn't know I needed this until I saw it" factor
- Open source contribution opportunity

**The Tagline:**
*"Your terminal, but it finally understands code."*

---

## Journal

See `journal/` folder for our discovery process, problems solved, and aha moments.

## Documentation

See `docs/` folder for research findings, API references, and architectural decisions.

## Source Code

See `src/` folder for the actual implementation (coming soon!).

---

**Let's build this thing!** 🚀
