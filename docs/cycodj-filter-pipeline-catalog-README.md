# cycodj CLI: Filter Pipeline Catalog

## Overview

This document catalogs the **9-layer filtering pipeline** for the `cycodj` CLI tool, which manages and searches chat conversation history files.

## Commands

cycodj has 6 distinct commands, each with their own filtering pipeline implementation:

### 1. [list](cycodj-list-catalog-README.md)
Lists conversations from chat history with time-based filtering and preview options.

**Quick Summary:**
- **Layer 1 (Target Selection)**: Time-based filtering (`--date`, `--last`, `--today`, `--yesterday`, `--after`, `--before`)
- **Layer 2 (Container Filter)**: Implicit - filters conversation files by timestamp
- **Layer 3 (Content Filter)**: Message preview with configurable count (`--messages`)
- **Layer 4 (Content Removal)**: None
- **Layer 5 (Context Expansion)**: None
- **Layer 6 (Display Control)**: `--stats`, `--branches`, `--messages`
- **Layer 7 (Output Persistence)**: `--save-output`
- **Layer 8 (AI Processing)**: `--instructions`, `--use-built-in-functions`, `--save-chat-history`
- **Layer 9 (Actions on Results)**: None (read-only)

---

### 2. [search](cycodj-search-catalog-README.md)
Searches for content within conversations with pattern matching and context display.

**Quick Summary:**
- **Layer 1 (Target Selection)**: Time-based filtering (same as list)
- **Layer 2 (Container Filter)**: Searches through conversation files, shows only those with matches
- **Layer 3 (Content Filter)**: Query matching with `--regex`, `--case-sensitive`, `--user-only`, `--assistant-only`
- **Layer 4 (Content Removal)**: None
- **Layer 5 (Context Expansion)**: `--context` (lines before/after matches)
- **Layer 6 (Display Control)**: `--stats`, `--branches`, `--messages`
- **Layer 7 (Output Persistence)**: `--save-output`
- **Layer 8 (AI Processing)**: `--instructions`, `--use-built-in-functions`, `--save-chat-history`
- **Layer 9 (Actions on Results)**: None (read-only)

---

### 3. [show](cycodj-show-catalog-README.md)
Displays a single conversation in full detail.

**Quick Summary:**
- **Layer 1 (Target Selection)**: Conversation ID (positional arg)
- **Layer 2 (Container Filter)**: Exact match on conversation ID
- **Layer 3 (Content Filter)**: System message hiding (unless `--verbose`)
- **Layer 4 (Content Removal)**: Content truncation (controlled by `--max-content-length`)
- **Layer 5 (Context Expansion)**: None (shows full conversation)
- **Layer 6 (Display Control)**: `--show-tool-calls`, `--show-tool-output`, `--max-content-length`, `--stats`
- **Layer 7 (Output Persistence)**: `--save-output`
- **Layer 8 (AI Processing)**: `--instructions`, `--use-built-in-functions`, `--save-chat-history`
- **Layer 9 (Actions on Results)**: None (read-only)

---

### 4. [branches](cycodj-branches-catalog-README.md)
Displays conversation branch tree showing parent-child relationships.

**Quick Summary:**
- **Layer 1 (Target Selection)**: Time-based filtering + optional specific conversation (`--conversation`)
- **Layer 2 (Container Filter)**: Builds tree of all conversations, filters by time
- **Layer 3 (Content Filter)**: Optional message preview (`--messages`)
- **Layer 4 (Content Removal)**: None
- **Layer 5 (Context Expansion)**: None
- **Layer 6 (Display Control)**: `--verbose`, `--messages`, `--stats`
- **Layer 7 (Output Persistence)**: `--save-output`
- **Layer 8 (AI Processing)**: `--instructions`, `--use-built-in-functions`, `--save-chat-history`
- **Layer 9 (Actions on Results)**: None (read-only)

---

### 5. [stats](cycodj-stats-catalog-README.md)
Shows statistics about conversations (message counts, tool usage, activity by date).

**Quick Summary:**
- **Layer 1 (Target Selection)**: Time-based filtering
- **Layer 2 (Container Filter)**: Aggregates all matching conversations
- **Layer 3 (Content Filter)**: Aggregated statistics only (no individual message display)
- **Layer 4 (Content Removal)**: N/A (statistical output)
- **Layer 5 (Context Expansion)**: N/A
- **Layer 6 (Display Control)**: `--show-tools`, `--no-dates`
- **Layer 7 (Output Persistence)**: `--save-output`
- **Layer 8 (AI Processing)**: `--instructions`, `--use-built-in-functions`, `--save-chat-history`
- **Layer 9 (Actions on Results)**: None (read-only)

---

### 6. [cleanup](cycodj-cleanup-catalog-README.md)
Finds and removes duplicate, empty, or old conversations.

**Quick Summary:**
- **Layer 1 (Target Selection)**: All history files, with age filter (`--older-than-days`)
- **Layer 2 (Container Filter)**: Duplicate detection, empty conversation detection
- **Layer 3 (Content Filter)**: N/A (operates on files, not content)
- **Layer 4 (Content Removal)**: N/A
- **Layer 5 (Context Expansion)**: N/A
- **Layer 6 (Display Control)**: Lists files to be cleaned
- **Layer 7 (Output Persistence)**: None (operates on files directly)
- **Layer 8 (AI Processing)**: None
- **Layer 9 (Actions on Results)**: `--find-duplicates`, `--remove-duplicates`, `--find-empty`, `--remove-empty`, `--execute` (vs dry-run)

---

## Common Patterns Across Commands

### Shared Base Class Properties

All cycodj commands inherit from `CycoDjCommand` which provides:

**Time Filtering (Layer 1):**
- `After` property (DateTime?)
- `Before` property (DateTime?)

**AI Processing (Layer 8):**
- `Instructions` property (string?)
- `UseBuiltInFunctions` property (bool)
- `SaveChatHistory` property (string?)

**Output Persistence (Layer 7):**
- `SaveOutput` property (string?)

### Shared Command Line Options

From `CycoDjCommandLineOptions.cs`:

#### Time Filtering
- `--today`: Sets After/Before to today's date range
- `--yesterday`: Sets After/Before to yesterday's date range
- `--after`, `--time-after`: Sets After property
- `--before`, `--time-before`: Sets Before property
- `--date-range`, `--time-range`: Sets both After and Before
- `--date`, `-d`: Legacy date filtering (backward compat)
- `--last`: Smart detection - either N conversations OR time specification

#### Display Control
- `--messages [N|all]`: Controls message preview count (list, search, branches)
- `--stats`: Shows statistical summary (list, search, show, branches)
- `--branches`: Shows branch information (list, search)

#### Output Persistence
- `--save-output <file>`: Saves command output to file

#### AI Processing
- `--instructions <text>`: AI instructions to process output
- `--use-built-in-functions`: Enables AI to use built-in functions
- `--save-chat-history <file>`: Saves AI processing chat history

---

## Documentation Structure

Each command has:

1. **Command README** (`cycodj-{command}-catalog-README.md`)
   - Overview of command
   - Links to all 9 layer documents
   - Quick reference summary

2. **Layer Documents** (9 per command)
   - `cycodj-{command}-filtering-pipeline-catalog-layer-{1-9}.md`
   - Detailed description of how that layer works for this command
   - Links to proof document

3. **Proof Documents** (9 per command)
   - `cycodj-{command}-filtering-pipeline-catalog-layer-{1-9}-proof.md`
   - Source code evidence with line numbers
   - Data flow tracing
   - Option/argument handling details

---

## Layer Definitions

### Layer 1: TARGET SELECTION
**Purpose**: What conversations to search/process (the primary search space)

### Layer 2: CONTAINER FILTERING
**Purpose**: Which conversation containers to include/exclude based on properties

### Layer 3: CONTENT FILTERING
**Purpose**: What content within conversations to show/highlight

### Layer 4: CONTENT REMOVAL
**Purpose**: What content to actively remove from display

### Layer 5: CONTEXT EXPANSION
**Purpose**: How to expand around matches (before/after context)

### Layer 6: DISPLAY CONTROL
**Purpose**: How to present results (formatting, stats, verbose mode)

### Layer 7: OUTPUT PERSISTENCE
**Purpose**: Where to save results (files, formats)

### Layer 8: AI PROCESSING
**Purpose**: AI-assisted analysis and transformation of results

### Layer 9: ACTIONS ON RESULTS
**Purpose**: What to do with results (delete files, execute operations)

---

## Source Code References

### Key Files

**Command Line Parsing:**
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` - Parses all CLI options
- `src/cycodj/CommandLine/CycoDjCommand.cs` - Base class for all commands

**Commands:**
- `src/cycodj/CommandLineCommands/ListCommand.cs`
- `src/cycodj/CommandLineCommands/SearchCommand.cs`
- `src/cycodj/CommandLineCommands/ShowCommand.cs`
- `src/cycodj/CommandLineCommands/BranchesCommand.cs`
- `src/cycodj/CommandLineCommands/StatsCommand.cs`
- `src/cycodj/CommandLineCommands/CleanupCommand.cs`

**Helper Classes:**
- `src/cycodj/Helpers/HistoryFileHelpers.cs` - File discovery and filtering
- `src/cycodj/Helpers/JsonlReader.cs` - Conversation parsing
- `src/cycodj/Helpers/TimestampHelpers.cs` - Timestamp parsing and formatting
- `src/common/Helpers/TimeSpecHelpers.cs` - Time specification parsing

**Models:**
- `src/cycodj/Models/Conversation.cs` - Conversation data structure
- `src/cycodj/Models/ChatMessage.cs` - Message data structure
- `src/cycodj/Models/ConversationTree.cs` - Branch tree structure

---

## Next Steps

Click on any command above to see detailed layer-by-layer documentation with source code proof.
