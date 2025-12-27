# cycodj CLI Filtering Pipeline Catalog

This directory contains detailed documentation of the cycodj CLI filtering pipeline implementation across all commands and layers.

## Overview

The cycodj CLI provides commands for managing and analyzing chat conversation history files. Each command implements a subset of the 9-layer filtering pipeline pattern.

## Commands

cycodj has 6 commands:

1. **[list](cycodj-list-filtering-pipeline-catalog-README.md)** - List conversations with optional filtering and previews
2. **[show](cycodj-show-filtering-pipeline-catalog-README.md)** - Show a specific conversation in detail
3. **[search](cycodj-search-filtering-pipeline-catalog-README.md)** - Search for text within conversations
4. **[branches](cycodj-branches-filtering-pipeline-catalog-README.md)** - Display conversation branching structure
5. **[stats](cycodj-stats-filtering-pipeline-catalog-README.md)** - Generate statistics about conversations
6. **[cleanup](cycodj-cleanup-filtering-pipeline-catalog-README.md)** - Find and optionally remove duplicate/empty/old conversations

## The 9 Filtering Pipeline Layers

Each command implements a subset of these conceptual layers:

1. **TARGET SELECTION** - Specify what to search IN (conversation files, time ranges)
2. **CONTAINER FILTERING** - Filter which conversations to include/exclude
3. **CONTENT FILTERING** - Filter what content within conversations to show
4. **CONTENT REMOVAL** - Actively remove content from display
5. **CONTEXT EXPANSION** - Expand around matches (messages/lines before/after)
6. **DISPLAY CONTROL** - Control how results are presented
7. **OUTPUT PERSISTENCE** - Save results to files
8. **AI PROCESSING** - AI-assisted analysis of results
9. **ACTIONS ON RESULTS** - Perform operations on results

## Layer Implementation Matrix

| Command | L1 | L2 | L3 | L4 | L5 | L6 | L7 | L8 | L9 |
|---------|----|----|----|----|----|----|----|----|-----|
| **list** | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ |
| **show** | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ |
| **search** | ✅ | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **branches** | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ |
| **stats** | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ |
| **cleanup** | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ✅ |

Legend:
- ✅ = Layer is implemented
- ❌ = Layer is not implemented

## Source Code Organization

### Command Line Parsing
- **Parser**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`
- **Base Command**: `src/cycodj/CommandLine/CycoDjCommand.cs`

### Command Implementations
- `src/cycodj/CommandLineCommands/ListCommand.cs`
- `src/cycodj/CommandLineCommands/ShowCommand.cs`
- `src/cycodj/CommandLineCommands/SearchCommand.cs`
- `src/cycodj/CommandLineCommands/BranchesCommand.cs`
- `src/cycodj/CommandLineCommands/StatsCommand.cs`
- `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### Helper Classes
- `src/cycodj/Helpers/HistoryFileHelpers.cs` - File finding and filtering
- `src/cycodj/Helpers/JsonlReader.cs` - Conversation parsing
- `src/cycodj/Helpers/TimestampHelpers.cs` - Timestamp handling
- `src/common/Helpers/TimeSpecHelpers.cs` - Time specification parsing

### Data Models
- `src/cycodj/Models/Conversation.cs`
- `src/cycodj/Models/ChatMessage.cs`
- `src/cycodj/Models/ConversationTree.cs`
- `src/cycodj/Models/ConversationMetadata.cs`

## Key Shared Patterns

### Time Filtering (Layer 1)
All display commands (list, show, search, branches, stats) support consistent time-based filtering:
- `--today`, `--yesterday` - Convenience shortcuts
- `--after <timespec>`, `--before <timespec>` - Explicit boundaries
- `--date-range <range>`, `--time-range <range>` - Range specification
- `--date <date>` - Legacy date filtering (backward compatibility)
- `--last <N|timespec>` - Smart detection (count or time specification)

### Display Options (Layer 6)
Common display control options across commands:
- `--messages [N|all]` - Control message preview count
- `--stats` - Show statistics
- `--branches` - Show branch information (list/search)
- `--verbose`, `-v` - Verbose output (branches)

### Output Options (Layer 7)
- `--save-output <file>` - Save command output to file

### AI Processing (Layer 8)
- `--instructions <text>` - Provide AI processing instructions
- `--use-built-in-functions` - Enable AI to use built-in functions
- `--save-chat-history <file>` - Save AI interaction history

## Documentation Structure

Each command has its own README that links to layer-specific documentation:

```
docs/
├── cycodj-filtering-pipeline-catalog-README.md (this file)
├── cycodj-{command}-filtering-pipeline-catalog-README.md
├── cycodj-{command}-filtering-pipeline-catalog-layer-{N}.md
└── cycodj-{command}-filtering-pipeline-catalog-layer-{N}-proof.md
```

### Per-Command Files

For each command (list, show, search, branches, stats, cleanup):
- **README**: Overview and links to all 9 layers
- **Layer N**: Description of how that layer is implemented
- **Layer N Proof**: Source code evidence with line numbers

## Navigation

- [← Back to Main Catalog](CLI-Filtering-Patterns-Catalog.md)
- [list command →](cycodj-list-filtering-pipeline-catalog-README.md)
- [show command →](cycodj-show-filtering-pipeline-catalog-README.md)
- [search command →](cycodj-search-filtering-pipeline-catalog-README.md)
- [branches command →](cycodj-branches-filtering-pipeline-catalog-README.md)
- [stats command →](cycodj-stats-filtering-pipeline-catalog-README.md)
- [cleanup command →](cycodj-cleanup-filtering-pipeline-catalog-README.md)
