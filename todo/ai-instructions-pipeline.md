# TODO: AI Instructions Pipeline for cycodgr

**Status**: Not Started  
**Priority**: Medium  
**Reference**: cycodmd instructions feature

---

## Overview

Add AI-powered analysis to cycodgr search results, following cycodmd's instructions pattern.

**Goal**: Process search results through cycod for AI analysis at different levels:
1. Per-repository analysis (--repo-instructions)
2. Per-file analysis (--file-instructions, --EXT-file-instructions)
3. Aggregated output analysis (--instructions)

---

## Reference Implementation

**Study these files from cycodmd**:
- `cycodmd help instructions` - Main instructions help
- `cycodmd help file instructions` - File-level instructions
- `src/cycodmd/CommandLine/CycoDmdCommand.cs` - How instructions are stored (Tuple pattern!)
- `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` - How flags are parsed
- `src/cycodmd/Program.cs` - How instructions are executed with cycod

---

## Key Implementation Pattern (from cycodmd)

### Data Structure - The Tuple Pattern

```csharp
// In CycoDmdCommand.cs (lines 12-13):
public List<Tuple<string, string>> FileInstructionsList { get; set; } = new();
public List<string> InstructionsList { get; set; } = new();
```

**Why Tuple<string, string>?**
- First string: The instruction/prompt
- Second string: The file criteria (extension or pattern)
- Allows --cs-file-instructions to store `("review code", "cs")`
- Allows --file-instructions to store `("explain", "*")` for all files

**For cycodgr, we'll need**:
```csharp
// In SearchCommand.cs
public List<Tuple<string, string>> RepoInstructionsList { get; set; } = new();
    // Tuple: (instruction, repoCriteria)
    // Currently only "*" for all repos, but extensible for future filters

public List<Tuple<string, string>> FileInstructionsList { get; set; } = new();
    // Tuple: (instruction, fileExtension)
    // Example: ("analyze", "cs"), ("review", "rs"), ("explain", "*")

public List<string> InstructionsList { get; set; } = new();
    // Simple list for final aggregated output analysis
```

### Parser Implementation Pattern

**Study how cycodmd parses in CycoDmdCommandLineOptions.cs** (around lines 120-150):

1. **Extract extension from flag name**:
   - `--cs-file-instructions` → extract "cs"
   - `--py-file-instructions` → extract "py"
   - `--file-instructions` → use "*" for all files

2. **Create Tuple with criteria**:
   ```csharp
   command.FileInstructionsList.Add(new Tuple<string, string>(instruction, extension));
   ```

3. **Support @file.md loading**:
   - Check if instruction starts with "@"
   - Load content from file
   - Add to instructions list

### Execution Pattern

**Study cycodmd's Program.cs** to understand:
- When instructions are executed (during output? after?)
- How cycod is invoked
- How content is passed
- How responses are captured and formatted

---

## Implementation Tasks

### 1. Add Properties to SearchCommand.cs

```csharp
public List<Tuple<string, string>> RepoInstructionsList { get; set; } = new();
public List<Tuple<string, string>> FileInstructionsList { get; set; } = new();
public List<string> InstructionsList { get; set; } = new();
```

### 2. Update Parser in CycoGrCommandLineOptions.cs

Parse these flags:
- `--repo-instructions PROMPT` → Add `("PROMPT", "*")` to RepoInstructionsList
- `--file-instructions PROMPT` → Add `("PROMPT", "*")` to FileInstructionsList
- `--cs-file-instructions PROMPT` → Add `("PROMPT", "cs")` to FileInstructionsList
- `--py-file-instructions PROMPT` → Add `("PROMPT", "py")` to FileInstructionsList
- (and similar for all language extensions)
- `--instructions PROMPT` → Add to InstructionsList

**Key**: Extract extension from flag name like cycodmd does!

### 3. Add Execution Logic in Program.cs

Study cycodmd to understand how to:
- Invoke cycod with instructions
- Pass repo/file data to cycod
- Capture AI responses
- Format responses into output
- Handle errors gracefully

### 4. Create Help Files

1. `instructions.txt` - Main concept
2. `repo instructions.txt` - Repo-level help
3. `file instructions.txt` - File-level help
4. `repo instructions examples.txt`
5. `file instructions examples.txt`
6. `instructions examples.txt`

---

## Use Cases

### Terminal Parser Research (Enhanced)

```bash
# Analyze parser architectures
cycodgr microsoft/terminal wez/wezterm alacritty/alacritty \
  --file-contains "StateMachine" \
  --cpp-file-instructions "extract state names and transitions" \
  --rs-file-instructions "extract state names and transitions" \
  --instructions "create comparison table of approaches"

# Document OSC codes
cycodgr --file-contains "OSC" --language cpp \
  --file-instructions "list all OSC codes with their purposes" \
  --instructions "create comprehensive OSC reference"

# Compare terminal quality
cycodgr --repo-contains "terminal emulator" --language rust --min-stars 1000 \
  --repo-instructions "assess parser quality, docs, and maintenance" \
  --instructions "recommend best terminal for learning and why"
```

### Library Comparison

```bash
# Compare JWT libraries
cycodgr --repo-contains "jwt library" --language csharp --min-stars 500 \
  --repo-instructions "analyze features, security, docs" \
  --instructions "create comparison table and recommend"

# Find authentication patterns
cycodgr --file-contains "AddJwtBearer" --language csharp \
  --cs-file-instructions "extract auth config patterns" \
  --instructions "identify best practices"
```

---

## Testing Plan

```bash
# Basic tests
cycodgr --repo-contains "jwt" --max-results 2 --repo-instructions "rate docs 1-10"
cycodgr --file-contains "parser" --max-results 2 --file-instructions "explain approach"
cycodgr --contains "terminal" --max-results 2 --instructions "summarize"

# Extension-specific
cycodgr --file-contains "async" --cs-file-instructions "review async/await"

# Chaining
cycodgr --file-contains "parser" \
  --file-instructions "extract states" \
  --instructions "compare approaches"

# @file loading
echo "Analyze code quality" > analyze.md
cycodgr --file-contains "parser" --file-instructions @analyze.md
```

---

## Success Criteria

- [ ] RepoInstructionsList uses Tuple<string, string> pattern
- [ ] FileInstructionsList uses Tuple<string, string> pattern
- [ ] Parser correctly extracts extension from flag names
- [ ] @file.md loading works for all instruction types
- [ ] Instructions execute in correct order (repo → file → aggregated)
- [ ] cycod integration works seamlessly
- [ ] Error handling when cycod unavailable
- [ ] Output formatting includes AI responses properly
- [ ] Help documentation is complete

---

## Questions to Answer from cycodmd Study

1. How does file criteria matching work with the Tuple?
2. How is cycod invoked (exact command format)?
3. How is content passed to cycod (stdin, file, args)?
4. How are responses captured and parsed?
5. How are responses formatted in output?
6. What happens when cycod fails or isn't available?
7. How does chaining work (multiple instructions)?

---

## Notes

- Makes cycodgr a true "AI-powered research tool"
- Elevates from "search" to "search + analyze"
- Terminal research example shows huge potential value
- Last major feature for feature-completeness
- Must study cycodmd carefully to get the Tuple pattern right!

---

**First Step**: Study `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs` and `src/cycodmd/Program.cs` to understand the exact implementation pattern.
