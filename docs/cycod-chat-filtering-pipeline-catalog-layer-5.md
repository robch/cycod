# cycod chat - Layer 5: Context Expansion

## Overview

Layer 5 (Context Expansion) defines how the chat command provides or expands context around the primary content being processed. Unlike file search tools that expand lines before/after a match, the chat command's "context expansion" is about providing conversational and historical context to the AI model.

## Context Expansion Mechanisms

The chat command implements context expansion through several mechanisms:

### 1. Chat History Loading (Conversational Context)

**Purpose**: Provide previous conversation turns as context for the current interaction.

**Command-Line Options**:
- `--chat-history [file]` - Load and continue from a chat history file (default: `chat-history.jsonl`)
- `--input-chat-history <file>` - Load specific chat history file as input context
- `--continue` - Automatically load the most recent chat history file
- `--output-chat-history [file]` - Save updated history (default: `chat-history-{time}.jsonl`)

**Context Behavior**:
- When history is loaded, all previous messages become context for the AI
- The AI can reference and build upon previous conversations
- Message history is chronologically ordered
- Both user and assistant messages provide context

**Source**: `CycoDevCommandLineOptions.cs`, lines 549-577

### 2. System Prompt Context

**Purpose**: Provide persistent instructional context that applies to the entire conversation.

**Command-Line Options**:
- `--system-prompt <prompt...>` - Override default system prompt (replaces all)
- `--add-system-prompt <prompt...>` - Add additional system prompt segments

**Context Behavior**:
- System prompts are prepended to the conversation
- They establish the AI's role, capabilities, and constraints
- Multiple `--add-system-prompt` options accumulate
- System prompt context applies to all subsequent messages

**Source**: `CycoDevCommandLineOptions.cs`, lines 470-485

### 3. User Prompt Additions (Instruction Context)

**Purpose**: Add contextual instructions that precede the main user input.

**Command-Line Options**:
- `--add-user-prompt <prompt...>` - Add user-role prompts before main input
- `--prompt <prompt>` - Add prompt (automatically prefixed with `/` if missing)

**Context Behavior**:
- Added prompts appear before the main `--input` instruction
- Can be used to provide context like "You are analyzing code" or "Focus on security"
- Multiple prompts accumulate in order
- The `--prompt` variant auto-prefixes with `/` for slash-command style

**Source**: `CycoDevCommandLineOptions.cs`, lines 487-497

### 4. Variable Context (Template Variable Expansion)

**Purpose**: Provide contextual values that can be referenced throughout the conversation.

**Command-Line Options**:
- `--var NAME=VALUE` - Define a single variable
- `--vars NAME1=VALUE1 NAME2=VALUE2...` - Define multiple variables

**Context Behavior**:
- Variables are available for template substitution as `{NAME}`
- Can be used in system prompts, user prompts, and input instructions
- Provides contextual configuration without hardcoding
- Variables persist across the command execution

**Source**: `CycoDevCommandLineOptions.cs`, lines 405-422

### 5. AGENTS.md File Context

**Purpose**: Automatically load project-specific context from AGENTS.md files.

**Mechanism**: 
- No explicit command-line option (automatic)
- Searches for AGENTS.md in current and parent directories
- Loads as system prompt context if found

**Context Behavior**:
- Provides project-specific instructions to the AI
- Establishes codebase conventions, file structures, patterns
- Layered: local > parent directories
- Combines with other system prompt sources

**Source**: Not directly in command-line parsing; implemented in chat command execution

### 6. MCP Server Context (Tool Context)

**Purpose**: Expand AI capabilities by providing access to external tools/data sources.

**Command-Line Options**:
- `--use-mcps [names...]` / `--mcp [names...]` - Use configured MCP servers (default: all if no names)
- `--no-mcps` - Disable all MCP servers
- `--with-mcp <command> [args...]` - Inline MCP server definition

**Context Behavior**:
- MCP servers provide additional context through tools (functions AI can call)
- Tools expand what the AI can "see" and do
- Tool results become part of the conversation context
- Multiple MCP servers can be active simultaneously

**Source**: `CycoDevCommandLineOptions.cs`, lines 443-469

### 7. Image Context (Multi-Modal Context)

**Purpose**: Add visual context to text-based conversations.

**Command-Line Options**:
- `--image <pattern...>` - Add images to the conversation (supports glob patterns)

**Context Behavior**:
- Images are included as message content alongside text
- AI can reference and analyze visual content
- Multiple images can be provided
- Glob patterns allow batch image inclusion

**Source**: `CycoDevCommandLineOptions.cs`, lines 658-664

## Context Expansion vs. Traditional Tools

Unlike file search tools (cycodmd, cycodgr) that expand context by showing N lines before/after a match, the chat command expands context through:

1. **Temporal expansion**: Previous conversation turns (chat history)
2. **Instructional expansion**: System prompts and user prompt additions
3. **Variable expansion**: Template variables that inject contextual values
4. **Project expansion**: AGENTS.md files providing project-specific context
5. **Tool expansion**: MCP servers providing access to external context
6. **Multi-modal expansion**: Images adding visual context

## No Traditional Line-Based Context Expansion

The chat command **does not** implement:
- `--lines`, `--lines-before`, `--lines-after` (not applicable)
- `--context` with numeric parameter (not applicable)
- Expanding around search matches (not a search command)

This is intentional: chat is a conversation/generation tool, not a search/filter tool.

## Context Accumulation Order

Context is accumulated in this order:

1. **Base system prompt** (from config or default)
2. **AGENTS.md content** (if found)
3. **System prompt override** (`--system-prompt` if provided)
4. **System prompt additions** (`--add-system-prompt` accumulates)
5. **Chat history messages** (`--chat-history`, `--input-chat-history`, or `--continue`)
6. **User prompt additions** (`--add-user-prompt`, `--prompt` accumulate)
7. **Main input instructions** (`--input`, `--instruction`, `--question`)
8. **Images** (`--image` patterns resolved)
9. **Variables** (`--var`, `--vars` available for template expansion throughout)
10. **MCP tools** (`--use-mcps`, `--mcp`, `--with-mcp` provide callable context)

## Detailed Documentation

See proof file for source code evidence: [cycod-chat-filtering-pipeline-catalog-layer-5-proof.md](cycod-chat-filtering-pipeline-catalog-layer-5-proof.md)

## Related Layers

- **Layer 1 (Target Selection)**: Selects which AI provider and model to use
- **Layer 3 (Content Filter)**: Filters which messages/tokens are included
- **Layer 5 (Context Expansion)**: ← **YOU ARE HERE** - Expands context around content
- **Layer 6 (Display Control)**: Controls how responses are displayed
- **Layer 7 (Output Persistence)**: Saves chat history and trajectory
- **Layer 8 (AI Processing)**: Processes the accumulated context with AI

## Summary

The chat command's Layer 5 is fundamentally about **contextual enrichment** rather than **spatial expansion**. It provides multiple mechanisms to give the AI model relevant context:
- Historical context (chat history)
- Instructional context (system/user prompts)
- Variable context (template variables)
- Project context (AGENTS.md)
- Tool context (MCP servers)
- Visual context (images)

This is appropriate for a conversational AI tool where "context" means "relevant information for the AI to consider" rather than "lines of text around a match."
