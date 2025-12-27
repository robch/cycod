# cycod chat - Layer 3: Content Filter

**Layer Purpose**: Define what content WITHIN selected containers (chat history, conversation messages) to show, filter, or process.

## Overview

The `chat` command in cycod is the primary interactive AI chat interface. Layer 3 content filtering controls:

1. **Message filtering within loaded chat history**
2. **System prompt content filtering (via template processing)**
3. **User prompt content filtering (via template processing)**  
4. **Input instruction content (via template processing and variable substitution)**

## Content Filter Mechanisms

### 1. Chat History Message Filtering

When loading chat history via `--input-chat-history` or `--continue`, the command applies token-based filtering:

**Token-Based Message Selection**:
- `MaxPromptTokenTarget`: Controls how many tokens from persistent prompts (system, user prefix messages)
- `MaxToolTokenTarget`: Controls how many tokens from tool call history
- `MaxChatTokenTarget`: Controls how many tokens from chat history messages

**Configuration**:
```bash
cycod config set app.max.prompt.tokens 4000
cycod config set app.max.tool.tokens 8000
cycod config set app.max.chat.tokens 16000
```

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#token-based-filtering)

### 2. Template Variable Substitution (Content Transformation)

All system prompts, user prompts, and input instructions undergo template variable substitution, which filters/transforms content:

**Variable Expansion**:
- `--var NAME=VALUE`: Define variables for template expansion
- `--vars NAME1=VALUE1 NAME2=VALUE2 ...`: Define multiple variables

**Template Processing**:
- System prompts: `SystemPrompt = GroundSystemPrompt()`
- User prompt adds: `UserPromptAdds = GroundUserPromptAdds()`
- Input instructions: `InputInstructions = GroundInputInstructions()`

**Example**:
```bash
cycod --var PROJECT=myproject --input "Analyze {PROJECT} codebase"
# Result: "Analyze myproject codebase"
```

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#template-variable-substitution)

### 3. System Prompt Content Control

The system prompt can be controlled and filtered through several mechanisms:

**Options**:
- `--system-prompt <text>`: Override default system prompt entirely
- `--add-system-prompt <text>`: Append additional system prompt content

**Processing Flow**:
1. Load base system prompt (from configuration or default)
2. Apply any `--system-prompt` override
3. Append any `--add-system-prompt` additions
4. Perform template variable substitution via `GroundSystemPrompt()`

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#system-prompt-control)

### 4. User Prompt Content Control

User prompts (prefix messages that appear before every interaction) can be filtered and controlled:

**Options**:
- `--add-user-prompt <text>`: Add prefix message to user prompts
- `--prompt <text>`: Shorthand for `--add-user-prompt` with `/` prefix handling

**Processing Flow**:
1. Collect all `--add-user-prompt` and `--prompt` values
2. For `--prompt`, add `/` prefix if not present (to support slash commands)
3. Perform template variable substitution via `GroundUserPromptAdds()`

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#user-prompt-control)

### 5. Input Instruction Content

Input instructions are the primary content being filtered and processed:

**Options**:
- `--input <text>`, `--instruction <text>`: Single input instruction
- `--inputs <text1> <text2> ...`, `--instructions <text1> <text2> ...`: Multiple instructions
- `--question <text>`, `-q <text>`: Question mode (single, non-interactive, quiet)
- `--questions <text1> <text2> ...`: Multiple questions (non-interactive, quiet)

**Auto-loading from stdin**:
- When using `--question`/`-q` or `--questions` with no arguments
- When chat command has no input and stdin is redirected

**File content expansion**:
- If instruction text is a valid file path, the file content is read and used

**Processing Flow**:
1. Parse all input options
2. Check if values are file paths; if so, read file content
3. For question modes with no args, read from stdin
4. Join multiple inputs with newlines
5. Perform template variable substitution via `GroundInputInstructions()`

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#input-instruction-content)

### 6. Template File Content Filtering

When `--use-templates` is enabled (default), prompts and instructions can contain template references:

**Template Processing**:
- Templates use `{variable}` syntax
- `--no-templates`: Disable template processing
- `--use-templates true|false`: Explicitly control template processing

**Available Variables**:
- Command-line defined: via `--var`, `--vars`
- AGENTS.md content: automatically added
- Built-in variables: timestamps, paths, etc.

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#template-processing)

### 7. Image Content Filtering

Images can be included as content, with pattern-based filtering:

**Options**:
- `--image <pattern1> <pattern2> ...`: Glob patterns for image files

**Processing Flow**:
1. Parse image patterns from command line
2. During chat loop, resolve patterns to actual files via `ImageResolver.ResolveImagePatterns()`
3. Clear patterns after each message (images apply to next message only)
4. Images are embedded in the user message content

**Source**: See [Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md#image-content)

## Content Filter Relationship to Other Layers

### From Layer 2 (Container Filter)
- Layer 2 determines WHICH chat history file to load
- Layer 3 determines WHICH messages within that file to include (via token limits)

### To Layer 4 (Content Removal)
- Layer 3 selects content to include
- Layer 4 (if implemented) would remove specific content from what Layer 3 selected

### To Layer 5 (Context Expansion)
- Layer 3 defines base content
- Layer 5 (if implemented) would expand context around selected content

## Command-Line Options Summary

| Option | Purpose | Layer 3 Impact |
|--------|---------|----------------|
| `--var NAME=VALUE` | Define template variable | Enables content transformation via templates |
| `--vars NAME=VALUE ...` | Define multiple template variables | Enables content transformation via templates |
| `--system-prompt <text>` | Override system prompt | Filters/replaces system prompt content |
| `--add-system-prompt <text>` | Append to system prompt | Adds to system prompt content |
| `--add-user-prompt <text>` | Add user prefix message | Adds to user prompt content |
| `--prompt <text>` | Add user prefix (slash-aware) | Adds to user prompt content with `/` handling |
| `--input <text>` | Single input instruction | Primary content to process |
| `--inputs <text> ...` | Multiple input instructions | Primary content to process |
| `--instruction <text>` | Alias for `--input` | Primary content to process |
| `--instructions <text> ...` | Alias for `--inputs` | Primary content to process |
| `--question <text>`, `-q` | Question mode input | Primary content, auto-stdin support |
| `--questions <text> ...` | Multiple questions | Primary content, auto-stdin support |
| `--use-templates` | Enable template processing | Controls content transformation |
| `--no-templates` | Disable template processing | Disables content transformation |
| `--image <patterns>` | Include image files | Embeds image content in messages |

**Configuration Settings** (impact Layer 3):
- `app.max.prompt.tokens`: Token limit for persistent prompts
- `app.max.tool.tokens`: Token limit for tool call history  
- `app.max.chat.tokens`: Token limit for chat history messages

## Non-Filtering Options (Other Layers)

The following options are NOT part of Layer 3 content filtering:

- `--input-chat-history`, `--chat-history`, `--continue`: Layer 2 (Container Selection)
- `--output-chat-history`, `--output-trajectory`: Layer 7 (Output Persistence)
- Provider selection (`--use-anthropic`, etc.): Layer 8 (AI Processing)
- MCP options (`--use-mcps`, `--with-mcp`): Layer 8 (AI Processing)

## Implementation Notes

1. **Template Variable System**: The `TemplateVariables` class (stored in `_namedValues`) handles all template substitution
2. **Token Limiting**: The `Conversation.LoadFromFile()` method applies token limits during chat history loading
3. **Content Grounding**: The `Ground*()` methods perform template variable substitution on prompts and instructions
4. **Stdin Auto-Detection**: Special logic in `Parse()` method (lines 14-26 of `CycoDevCommandLineOptions.cs`) auto-loads stdin for question modes

## See Also

- **[Layer 3 Proof](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md)**: Detailed source code evidence
- **[Layer 2: Container Filter](cycod-chat-filtering-pipeline-catalog-layer-2.md)**: How chat history files are selected
- **[Layer 4: Content Removal](cycod-chat-filtering-pipeline-catalog-layer-4.md)**: Mechanisms to remove content (if any)
- **[Layer 8: AI Processing](cycod-chat-filtering-pipeline-catalog-layer-8.md)**: How content is processed by AI
