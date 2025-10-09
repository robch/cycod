# Conversation Titling Feature - Complete Implementation Guide

## Project Context

### cycod Application Architecture
**cycod** is a sophisticated AI-powered C# CLI coding tool with the following components:
- **Main CLI**: `cycod` - Primary chat interface with AI for coding assistance
- **Documentation Tool**: `cycodmd` - Markdown processing utility
- **Testing Framework**: `cycodt` - YAML-based test runner
- **Common Library**: Shared utilities across all components
- **MCP Servers**: Model Context Protocol servers for extended capabilities

### Key Technologies & Frameworks
- **.NET/C#**: Modern C# with Microsoft.Extensions.AI framework
- **Chat Integration**: Direct integration with various LLM providers (GitHub Copilot, etc.)
- **Function Calling**: Extensive toolset including shell commands, file operations, code exploration
- **Configuration System**: Multi-scope hierarchy (Local → User → Global)
- **Cross-platform**: Windows, macOS, Linux support

## Current Conversation History System

### File Format & Storage
**File Format**: JSONL (JSON Lines) - each line is a separate JSON message object
**File Extension**: `.jsonl`
**Example Structure**:
```json
{"role":"system","content":"You are a helpful AI assistant..."}
{"role":"user","content":"Hello! How can I implement feature X?"}
{"role":"assistant","content":"I can help you implement feature X..."}
{"role":"tool","content":"function_call_result"}
```

### Storage Locations & Discovery
**Primary Storage**: `~/.cycod/history/chat-history-{timestamp}.jsonl`
**Search Hierarchy** (ChatHistoryFileHelpers.FindMostRecentChatHistoryFile):
1. **Local scope**: Current directory `chat-history*.jsonl`
2. **User scope**: `~/.cycod/history/chat-history*.jsonl` 
3. **Global scope**: System-wide locations
4. **Exception files**: `exception-chat-history*.jsonl` (also valid for continuation)

**File Selection**: Most recent by last write time across all scopes

### Auto-Save Behavior
**Trigger**: Every message exchange via `HandleUpdateMessages()`
**Configuration**: `AppAutoSaveChatHistory` setting (default: true)
**Template**: `chat-history-{time}.jsonl` where `{time}` is timestamp
**Exception Handling**: Crashes save as `exception-chat-history-{timestamp}.jsonl`

### Loading & LLM Integration Process
1. **File Discovery**: `ChatHistoryFileHelpers.FindMostRecentChatHistoryFile()`
2. **Parsing**: `AIExtensionsChatHelpers.ChatMessagesFromJsonl()` 
3. **Message Processing**:
   - System message replacement (if loaded history contains system message)
   - `FixDanglingToolCalls()` - adds dummy responses for incomplete function calls
   - `TryTrimToTarget()` - manages token limits for prompt/tool/chat content
4. **LLM Delivery**: Messages passed to `IChatClient` via Microsoft.Extensions.AI framework

### Current File Operations (Key Classes)
- **ChatHistoryFileHelpers**: File discovery, path resolution, auto-save configuration
- **ChatMessageHelpers/AIExtensionsChatHelpers**: JSONL parsing, message serialization
- **FunctionCallingChat**: Message management, LLM integration
- **ChatCommand**: Main execution flow, handles saving/loading

## Conversation Metadata Design (`_meta` Object)

### Complete Metadata Structure
```json
{"_meta":{
  "title": "Implementing Conversation Titling Feature in AI Coding Tool",
  "description": "Exploring a C# cycod CLI application structure to understand the codebase before implementing conversation titling functionality. Discussion covers chat history management, JSONL format, auto-saving behavior, and integration with LLM providers.",
  "titleLocked": false,
  "descriptionLocked": false,
  "createdAt": "2025-10-09T17:56:54.4337698Z",
  "updatedAt": "2025-10-09T17:58:32.2210007Z",
  "capabilities": {
    "shellCommands": true,
    "fileOperations": false,
    "backgroundProcesses": true,
    "codeExploration": true,
    "imageProcessing": false,
    "mcpServers": {}
  }
}}
```

### Field Definitions
- **title**: Human-readable conversation title (string)
- **description**: Brief summary of conversation content/purpose (string)
- **titleLocked**: If true, AI should never regenerate title (boolean) - set when user manually edits title
- **descriptionLocked**: If true, AI should never regenerate description (boolean) - set when user manually edits description
- **createdAt**: ISO 8601 timestamp when conversation started (string)
- **updatedAt**: ISO 8601 timestamp of last modification (string)
- **capabilities**: Function/capability approvals persisted to conversation (object)
  - Each function type has boolean approval status
  - MCP servers may have nested approval structures

### File Structure with Metadata
```json
{"_meta":{"title":"...","description":"...","createdAt":"...","updatedAt":"...","titleLocked":false,"capabilities":{}}}
{"role":"system","content":"System prompt here..."}
{"role":"user","content":"First user message..."}
{"role":"assistant","content":"First assistant response..."}
```

## Title Generation Strategy

### Initial Title Generation (After First Exchange)
**Trigger**: After first user-assistant message exchange
**Implementation**: Separate cycod instance with specialized system prompt
**System Prompt Focus**: 
- Analyze conversation context and generate concise, descriptive title
- Extract key topics, technologies, or problem domains
- Keep titles under ~80 characters for display purposes

### Subsequent Title Updates
**Capability**: AI can suggest title changes during conversation
**Restrictions**: 
- Cannot update if `titleLocked: true`
- Should only suggest changes when conversation topic significantly shifts
**Implementation**: TBD - could be same instance or separate title-generation instance

### Default Naming Convention
**Fallback**: `"conversation-{timestamp}"` if title generation fails
**Format**: `"conversation-2025-01-09-175654"` (human-readable timestamp)

## Capabilities Persistence System

### Function Approval Tracking
**Concept**: When user approves a function for "this session", approval persists to conversation
**Storage**: Boolean flags in `capabilities` object
**Scope**: Per-conversation, not global or per-user

### Function Categories (Based on Current Codebase)
- **shellCommands**: Shell/bash command execution
- **fileOperations**: File reading, writing, editing (StrReplace, etc.)
- **backgroundProcesses**: Long-running process management
- **codeExploration**: Code analysis, file searching, project exploration  
- **imageProcessing**: Image attachment and analysis
- **mcpServers**: MCP server connections and function calls

### Approval Workflow
1. Function call requested by AI
2. Check `capabilities` in conversation metadata
3. If approved: Execute directly
4. If not approved: Prompt user for approval
5. If user approves "for this conversation": Update metadata and execute
6. Save updated metadata to file

## Implementation Requirements

### Backward Compatibility (Critical)
**Requirement**: All existing conversation files must continue to work
**Implementation Strategy**:
- When loading file without `_meta`: Auto-generate metadata on load
- Default values for missing metadata fields
- No breaking changes to existing message parsing logic

### File Format Changes
**New Structure**: `_meta` object as first line in JSONL file
**Parsing Logic**: 
- Check if first line contains `{"_meta":...}`
- If yes: Parse metadata, continue with remaining lines as messages
- If no: Generate default metadata, parse entire file as messages

### Integration Points (Classes to Modify)
1. **ChatHistoryFileHelpers**: File discovery and path management
2. **ChatMessageHelpers/AIExtensionsChatHelpers**: JSONL parsing with metadata support
3. **FunctionCallingChat**: Metadata-aware loading/saving
4. **ChatCommand**: Title generation triggering and capability checking
5. **Function Approval System**: New capability checking logic

### Auto-Save Behavior Updates
**Metadata Updates**: 
- `updatedAt` timestamp on every save
- `createdAt` only set once when conversation starts
- Capability approvals immediately persisted

### Error Handling & Edge Cases
- **Malformed metadata**: Skip and regenerate default
- **Missing fields**: Use sensible defaults
- **Title generation failures**: Fall back to timestamp-based naming
- **Capability conflicts**: Default to requiring user approval

## Interactive Commands (During Chat Session)

### `/title` Command
**Purpose**: View and modify conversation title during active chat
**Usage Examples**:
- `/title` - Display current title and lock status
- `/title New Title Here` - Update title and set `titleLocked: true`
**Behavior**:
- **View mode**: Shows current title and whether it's locked (user-edited)
- **Update mode**: Updates `_meta.title` field immediately, sets `titleLocked: true`
- Updates `updatedAt` timestamp
- Auto-saves conversation with new metadata
**Display Format** (view mode):
```
Current title: "Implementing Conversation Titling Feature in AI Coding Tool"
Status: Locked (user-edited) | Unlocked (AI-generated)
```

### `/description` Command  
**Purpose**: View and modify conversation description during active chat
**Usage Examples**:
- `/description` - Display current description and lock status
- `/description Updated description of what this conversation covers` - Update description
**Behavior**:
- **View mode**: Shows current description and whether it's locked (user-edited)
- **Update mode**: Updates `_meta.description` field immediately, sets `descriptionLocked: true`
- Updates `updatedAt` timestamp  
- Auto-saves conversation with new metadata
**Display Format** (view mode):
```
Current description: "Exploring C# cycod CLI application structure..."
Status: Locked (user-edited) | Unlocked (AI-generated)
```

## CLI Commands (Outside Chat Session)

### Conversation Listing Command
**Based on existing patterns**: Following `config list`, `alias list`, `prompt list` pattern
**Command Structure**: `cycod chat list` (follows established pattern)
**Output Format** (suggested):
```
Recent Conversations:
  [1] Implementing Conversation Titling Feature in AI Coding Tool
      Created: 2025-01-09 17:56  |  Last Modified: 2025-01-09 18:30
      Description: Exploring C# cycod CLI application structure...
      
  [2] Debug Network Connection Issues  
      Created: 2025-01-08 14:22  |  Last Modified: 2025-01-08 16:45
      Description: Troubleshooting API connectivity problems...
```

### Continue Conversation by Title
**Based on existing patterns**: Extends existing `--continue` flag functionality
**Current mechanism**: `cycod chat --continue` loads most recent
**New mechanism**: `cycod chat --continue "Title Here"` loads by title
**Fallback**: `cycod chat --continue` still works for most recent

#### Unique Title Resolution
**If title is unique**: Load conversation directly and start chat
**If title has duplicates**: Display list and exit (no interactive menu)
```bash
$ cycod chat --continue "Debug Issues"
Error: Multiple conversations found with title "Debug Issues":

  [1] Debug Issues  
      Created: 2025-01-09 10:30  |  File: chat-history-1736431800000.jsonl
      Description: Network connectivity problems with API endpoints
      
  [2] Debug Issues
      Created: 2025-01-08 15:20  |  File: chat-history-1736345200000.jsonl  
      Description: Database query performance optimization

Use the full filename or a more specific title to continue a conversation.

$ cycod chat --input-chat-history chat-history-1736431800000.jsonl
# (this would load the specific conversation)
```

#### Title Matching Strategy
- **Exact match first**: Look for conversations with exact title match
- **Case-insensitive fallback**: If no exact match, try case-insensitive
- **Partial matching** (optional): Could support fuzzy/partial title matching

## Implementation Requirements Updates

### New Slash Command Handlers
**Location**: `SlashCommands/` directory (following existing pattern)
- **SlashTitleCommandHandler**: Handle `/title` and `/title <new title>` commands
- **SlashDescriptionCommandHandler**: Handle `/description` and `/description <new desc>` commands

### New CLI Commands  
**Location**: `CommandLineCommands/` directory
- **ConversationListCommand**: List all conversations with metadata
- **ConversationContinueCommand**: Continue conversation by title with disambiguation

### Metadata Search & Indexing
**Scope Precedence** (based on existing codebase patterns):
- **Mixed listing**: All files from all scopes combined, sorted by timestamp (most recent first)
- **Search priority**: Local → User → Global (follows McpFileHelpers.GetFromAnyScope pattern)
- **Discovery pattern**: Follows ChatHistoryFileHelpers.FindMostRecentChatHistoryFile approach

**Requirements**:
- Scan all conversation files across scopes using ScopeFileHelpers.FindFilesInScope
- Parse `_meta` objects to extract titles, descriptions, timestamps
- Handle files without metadata (generate defaults on-the-fly)
- Sort by last modified time (most recent first) across all scopes
- Enable title-based lookup with duplicate handling

### Updated Integration Points
1. **ChatCommand**: Add slash command routing for `/title` and `/description`
2. **ChatHistoryFileHelpers**: Add conversation discovery by title, extend existing patterns
3. **CycoDevCommandLineOptions**: 
   - Add `"chat list"` to NewCommandFromName switch statement
   - Extend `--continue` parameter parsing to accept title strings  
4. **New Command Classes**:
   - **ChatListCommand**: Following existing `*ListCommand` pattern
   - **ConversationMetadataHelpers**: New utility class for metadata operations

## Command Flow Examples

### Interactive Title Change
```
User: /title
Assistant: Current title: "Implementing Conversation Titling Feature in AI Coding Tool"

User: /title Bug Fix Session
Assistant: Title updated to "Bug Fix Session" (locked from AI changes)
```

### CLI Conversation Continuation
```bash
# List conversations
$ cycod list-conversations

# Continue by unique title  
$ cycod continue "Implementing Conversation Titling Feature"
Loading: ~/.cycod/history/chat-history-1736431800000.jsonl

# Continue with duplicate titles (triggers disambiguation)
$ cycod continue "Debug Issues"
Multiple conversations found... [shows menu]
```

## Testing Strategy

### Test Coverage Areas
1. **Backward compatibility**: Load old conversation files
2. **Metadata generation**: Default metadata creation  
3. **Title generation**: Separate instance invocation
4. **Capability persistence**: Approval workflow
5. **File format validation**: Proper JSONL with metadata
6. **Auto-save integration**: Metadata updates during saves
7. **Interactive commands**: `/title` and `/description` slash commands
8. **CLI commands**: List conversations and continue by title
9. **Disambiguation**: Multiple conversations with same title
10. **Cross-scope search**: Finding conversations across Local/User/Global scopes

### Test File Examples Needed
- Pre-metadata conversation files (current format)
- Post-metadata conversation files (new format)  
- Corrupted/malformed metadata files
- Long conversations for title generation testing
- Multiple conversations with identical titles
- Conversations across different scope directories

This comprehensive knowledge base should provide any new agent with complete context to implement the conversation titling feature successfully.