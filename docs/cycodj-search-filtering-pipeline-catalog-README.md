# cycodj search Command - Filtering Pipeline Catalog

## Overview

The `search` command searches for text within conversation messages and displays results with context. It implements 6 of the 9 filtering pipeline layers.

## Command Syntax

```bash
cycodj search <query> [options]
```

## Layer Implementation Status

| Layer | Status | Description |
|-------|--------|-------------|
| **1. Target Selection** | ✅ Implemented | Time-based filtering of conversation files |
| **2. Container Filtering** | ❌ Not Implemented | No conversation-level filtering based on properties |
| **3. Content Filtering** | ✅ Implemented | Message role and content matching |
| **4. Content Removal** | ❌ Not Implemented | No explicit content removal |
| **5. Context Expansion** | ✅ Implemented | Line-level context around matches |
| **6. Display Control** | ✅ Implemented | Statistics, formatting, message previews |
| **7. Output Persistence** | ✅ Implemented | Save results to file |
| **8. AI Processing** | ✅ Implemented | AI-assisted analysis of results |
| **9. Actions on Results** | ❌ Not Implemented | No actions performed on results |

## Layer Documentation

### Layer 1: Target Selection
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-1.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-1-proof.md)**

Select which conversation files to search based on time criteria.

**Options:**
- `--today` - Search conversations from today
- `--yesterday` - Search conversations from yesterday
- `--after <timespec>`, `--time-after <timespec>` - After specific time
- `--before <timespec>`, `--time-before <timespec>` - Before specific time
- `--date-range <range>`, `--time-range <range>` - Time range
- `--date <date>`, `-d <date>` - Specific date (backward compatibility)
- `--last <N|timespec>` - Last N conversations or time specification

### Layer 2: Container Filtering
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-2.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-2-proof.md)**

❌ **Not Implemented** - The search command searches all conversations in the time range. There is no filtering based on conversation properties (e.g., by title, branch status, etc.).

### Layer 3: Content Filtering
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-3.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-3-proof.md)**

Filter which messages to search and what content to match.

**Options:**
- Positional `<query>` - Text or pattern to search for
- `--case-sensitive`, `-c` - Case-sensitive search
- `--regex`, `-r` - Use regex pattern matching
- `--user-only`, `-u` - Only search user messages
- `--assistant-only`, `-a` - Only search assistant messages

### Layer 4: Content Removal
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-4.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-4-proof.md)**

❌ **Not Implemented** - The search command does not actively remove content from display. All matched content and context is shown.

### Layer 5: Context Expansion
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-5.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-5-proof.md)**

Expand around matched lines to show surrounding context.

**Options:**
- `--context <N>`, `-C <N>` - Show N lines before/after matches (default: 2)

### Layer 6: Display Control
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-6.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-6-proof.md)**

Control how search results are presented.

**Options:**
- `--messages [N|all]` - Control message preview count
- `--stats` - Show statistics summary
- `--branches` - Show branch information

### Layer 7: Output Persistence
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-7.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-7-proof.md)**

Save search results to files.

**Options:**
- `--save-output <file>` - Save search results to file

### Layer 8: AI Processing
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-8.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-8-proof.md)**

Apply AI-assisted processing to search results.

**Options:**
- `--instructions <text>` - Provide AI processing instructions
- `--use-built-in-functions` - Enable AI to use built-in functions
- `--save-chat-history <file>` - Save AI interaction history

### Layer 9: Actions on Results
**[Documentation](cycodj-search-filtering-pipeline-catalog-layer-9.md)** | **[Proof](cycodj-search-filtering-pipeline-catalog-layer-9-proof.md)**

❌ **Not Implemented** - The search command is read-only. It does not modify conversations or perform actions on results.

## Complete Option Reference

### Positional Arguments
- `<query>` - Search query (required)

### Time Filtering (Layer 1)
- `--today` - Conversations from today
- `--yesterday` - Conversations from yesterday  
- `--after <timespec>`, `--time-after <timespec>` - After time
- `--before <timespec>`, `--time-before <timespec>` - Before time
- `--date-range <range>`, `--time-range <range>` - Time range
- `--date <date>`, `-d <date>` - Specific date
- `--last <N|timespec>` - Last N or time spec

### Content Filtering (Layer 3)
- `--case-sensitive`, `-c` - Case-sensitive search
- `--regex`, `-r` - Use regex patterns
- `--user-only`, `-u` - Only search user messages
- `--assistant-only`, `-a` - Only search assistant messages

### Context Expansion (Layer 5)
- `--context <N>`, `-C <N>` - Context lines (default: 2)

### Display Control (Layer 6)
- `--messages [N|all]` - Message preview count
- `--stats` - Show statistics
- `--branches` - Show branch info

### Output Persistence (Layer 7)
- `--save-output <file>` - Save to file

### AI Processing (Layer 8)
- `--instructions <text>` - AI instructions
- `--use-built-in-functions` - Enable AI functions
- `--save-chat-history <file>` - Save AI chat

## Usage Examples

### Basic Search
```bash
cycodj search "error"
```

### Case-Sensitive Regex Search
```bash
cycodj search "Error|Exception" --regex --case-sensitive
```

### Search with More Context
```bash
cycodj search "API" --context 5
```

### Search Only User Messages
```bash
cycodj search "question" --user-only
```

### Time-Filtered Search
```bash
cycodj search "bug" --today
cycodj search "feature" --last 7d
cycodj search "TODO" --after 2024-01-01
```

### Search with Statistics
```bash
cycodj search "refactor" --stats
```

### Save Search Results
```bash
cycodj search "performance" --save-output search-results.md
```

### AI-Assisted Analysis
```bash
cycodj search "error" --instructions "Summarize the error patterns"
```

## Source Code

**Command Implementation**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Parser**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (lines 407-481)

**Base Class**: `src/cycodj/CommandLine/CycoDjCommand.cs`

## Navigation

- [← Back to cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)
- [→ Layer 1: Target Selection](cycodj-search-filtering-pipeline-catalog-layer-1.md)
- [→ Layer 3: Content Filtering](cycodj-search-filtering-pipeline-catalog-layer-3.md)
- [→ Layer 5: Context Expansion](cycodj-search-filtering-pipeline-catalog-layer-5.md)
- [→ Layer 6: Display Control](cycodj-search-filtering-pipeline-catalog-layer-6.md)
- [→ Layer 7: Output Persistence](cycodj-search-filtering-pipeline-catalog-layer-7.md)
- [→ Layer 8: AI Processing](cycodj-search-filtering-pipeline-catalog-layer-8.md)
