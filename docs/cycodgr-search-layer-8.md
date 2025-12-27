# cycodgr search - Layer 8: AI PROCESSING

## Purpose

Layer 8 enables **AI-assisted analysis and processing** of search results.

## Command-Line Options

### `--instructions <instruction...>`

**Purpose**: Apply AI instructions to the entire combined output

**Examples**:
```bash
cycodgr --file-contains "async" --instructions "Summarize the async patterns used"
```

**Storage**: `SearchCommand.InstructionsList` (List<string>)

---

### `--file-instructions <instruction...>`

**Purpose**: Apply AI instructions to each file's output

**Examples**:
```bash
cycodgr --file-contains "error handling" --file-instructions "Explain the error handling strategy"
```

**Storage**: `SearchCommand.FileInstructionsList` (List<Tuple<string, string>>)  
Format: `(instruction, criteria)` where criteria is empty string

---

### `--{ext}-file-instructions <instruction...>`

**Purpose**: Apply AI instructions to files matching specific extension

**Examples**:
```bash
cycodgr --file-contains "API" --cs-file-instructions "Document the API endpoints"
```

**Storage**: `SearchCommand.FileInstructionsList` (List<Tuple<string, string>>)  
Format: `(instruction, extension)` where extension filters which files get the instruction

---

### `--repo-instructions <instruction...>`

**Purpose**: Apply AI instructions to each repository's combined output

**Examples**:
```bash
cycodgr --repo-contains "ml" --repo-instructions "Describe the machine learning approach"
```

**Storage**: `SearchCommand.RepoInstructionsList` (List<string>)

---

## Processing Hierarchy

```
1. File-level instructions (--file-instructions, --{ext}-file-instructions)
     Applied to each file individually
     ↓
2. Repository-level instructions (--repo-instructions)
     Applied to combined output of all files in a repo
     ↓
3. Global instructions (--instructions)
     Applied to final combined output of all repos
```

## Data Flow

```
Search Results (CodeMatch list)
  ↓
FormatAndOutputCodeResults()
  ↓
For each repo:
  ↓
  For each file:
    ↓
    Format file output
    ↓
    Apply file instructions (if criteria matches)
    ↓
    AiInstructionProcessor.ApplyAllInstructions(fileInstructions, fileOutput)
    ↓
  Combine all files in repo
  ↓
  Apply repo instructions
  ↓
  AiInstructionProcessor.ApplyAllInstructions(repoInstructions, repoOutput)
  ↓
Combine all repos
  ↓
Apply global instructions
  ↓
AiInstructionProcessor.ApplyAllInstructions(globalInstructions, combinedOutput)
  ↓
Final output
```

---

## Key Characteristics

1. **Hierarchical processing**: File → Repo → Global
2. **Criteria-based filtering**: `--{ext}-file-instructions` only applies to matching files
3. **No built-in functions**: Unlike cycodmd, does NOT support `--built-in-functions` option
4. **No chat history saving**: Unlike cycodmd, does NOT support `--save-chat-history` option

---

For detailed source code evidence, see: [**Layer 8 Proof Document**](cycodgr-search-layer-8-proof.md)
