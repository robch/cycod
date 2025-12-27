# cycod `chat` Command - Layer 8: AI Processing

## Overview

**Layer 8** focuses on AI-assisted analysis and processing of content. For the `chat` command, this is the **core functionality** - enabling conversational interaction with AI models that can understand context, use tools, and generate intelligent responses.

## Command

```bash
cycod chat [options]
cycod [options]  # chat is the default command
```

## Purpose

Layer 8 (AI Processing) in the `chat` command orchestrates:
1. **AI Model Selection** - Choose and configure the AI provider and model
2. **System Prompt Configuration** - Define the AI's role, constraints, and behavior
3. **User Input Processing** - Handle user messages and instructions
4. **Template Processing** - Expand variables and dynamic content in prompts
5. **Tool Integration** - Enable AI access to functions (MCP servers, built-in helpers)
6. **Conversation Management** - Load, maintain, and save conversation history
7. **Multi-Modal Support** - Process text and image inputs
8. **Token Management** - Control context window usage across prompt, tools, and history
9. **Response Generation** - Stream AI responses with function calling support

## Layer 8 Options

### AI Provider Selection

Select which AI service to use:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--use-anthropic` | Use Anthropic (Claude) models | L585-589 |
| `--use-azure-anthropic` | Use Azure-hosted Anthropic | L590-594 |
| `--use-aws`, `--use-bedrock`, `--use-aws-bedrock` | Use AWS Bedrock | L595-599 |
| `--use-azure-openai`, `--use-azure` | Use Azure OpenAI | L600-604 |
| `--use-google`, `--use-gemini`, `--use-google-gemini` | Use Google Gemini | L605-609 |
| `--use-grok`, `--use-x.ai` | Use Grok (X.AI) | L610-614 |
| `--use-openai` | Use OpenAI | L615-619 |
| `--use-test` | Use test provider | L620-624 |
| `--use-copilot` | Use GitHub Copilot | L625-629 |
| `--use-copilot-token` | Use Copilot with explicit token | L630-633 |

**Mechanism**: Sets `KnownSettings.AppPreferredProvider` and `CYCOD_AI_PROVIDER` environment variable.

**Source**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs` lines 585-633

### Model Configuration

Configure model-specific settings:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--grok-api-key <key>` | Set Grok API key | L634-641 |
| `--grok-model-name <name>` | Specify Grok model | L642-649 |
| `--grok-endpoint <url>` | Set Grok API endpoint | L650-657 |

**Note**: Similar configuration exists for other providers via `KnownSettings` (see proof document).

### System Prompt Management

Define the AI's behavior and constraints:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--system-prompt <text>` | Set complete system prompt | L470-476 |
| `--add-system-prompt <text>` | Add to system prompt | L477-486 |

**Processing Flow**:
1. Parse command-line option → Store in `ChatCommand.SystemPrompt` or `SystemPromptAdds`
2. Ground/expand templates → `GroundSystemPrompt()` (ChatCommand.cs L98)
3. Load AGENTS.md → `AddAgentsFileContentToTemplateVariables()` (ChatCommand.cs L58)
4. Create FunctionCallingChat → Pass to constructor (ChatCommand.cs L120)

**Source**: 
- Parsing: `CycoDevCommandLineOptions.cs` L470-486
- Execution: `ChatCommand.cs` L98, L120

### User Prompt Management

Add persistent user messages:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--add-user-prompt <text>` | Add persistent user message | L487-498 |
| `--prompt <text>` | Add user message (auto-prefixes `/` for slash commands) | L487-498 |

**Processing Flow**:
1. Parse option → Store in `ChatCommand.UserPromptAdds`
2. Ground/expand templates → `GroundUserPromptAdds()` (ChatCommand.cs L99)
3. Add to conversation → `chat.Conversation.AddPersistentUserMessages()` (ChatCommand.cs L129-132)

**Behavior Note**: `--prompt` automatically adds `/` prefix if not present, making it convenient for slash commands.

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L487-498
- Execution: `ChatCommand.cs` L99, L129-132

### Input Instructions

Provide the primary user input for AI processing:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--input <text>`, `--instruction <text>` | Single input instruction | L499-523 |
| `--question <text>`, `-q <text>` | Quiet, non-interactive input | L499-523 |
| `--inputs <text>...`, `--instructions <text>...` | Multiple inputs | L524-548 |
| `--questions <text>...` | Multiple quiet inputs | L524-548 |

**Special Features**:
- **File expansion**: If input is a file path, content is read automatically (L502-504, L526-528)
- **Stdin auto-loading**: `--question` and `--questions` with no args read from stdin (L513-517, L538-542)
- **Quiet mode**: `--question` and `--questions` set `Quiet = true` and `Interactive = false` (L507-511, L532-536)

**Processing Flow**:
1. Parse option → Expand files or read stdin
2. Store in `ChatCommand.InputInstructions`
3. Ground/expand templates → `GroundInputInstructions()` (ChatCommand.cs L100)
4. Feed to conversation loop → `ReadLineOrSimulateInput()` (ChatCommand.cs L158-160)

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L499-548
- Execution: `ChatCommand.cs` L100, L158-160

### Template Processing

Expand variables and dynamic content:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--var NAME=VALUE` | Set single template variable | L405-412 |
| `--vars NAME1=VALUE1 NAME2=VALUE2 ...` | Set multiple variables | L413-423 |
| `--use-templates [true\|false]` | Enable/disable template processing | L432-438 |
| `--no-templates` | Disable template processing | L439-442 |

**Variable Syntax**: Variables are referenced as `{NAME}` in prompts and instructions.

**Variable Sources**:
1. Command-line (`--var`, `--vars`)
2. Config file (`Var.*` settings)
3. AGENTS.md content
4. Built-in values (time, date, etc.)

**Processing**:
- Variables stored in `TemplateVariables` class (ChatCommand.cs L57)
- Expansion happens during "grounding" phase (ChatCommand.cs L98-100)
- `ReplaceValues()` extension method performs substitution

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L405-442
- Execution: `ChatCommand.cs` L57, L98-100
- Proof: See [proof document](cycod-chat-filtering-pipeline-catalog-layer-8-proof.md)

### Tool Integration (MCP Servers)

Enable AI access to external tools via Model Context Protocol:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--use-mcps [names...]`, `--mcp [names...]` | Use configured MCP servers | L443-452 |
| `--no-mcps` | Disable MCP servers | L453-456 |
| `--with-mcp <command> [args...]` | Use ad-hoc MCP server | L457-469 |

**MCP Server Types**:
1. **Configured servers** (`--use-mcps`): Loaded from config files
2. **Ad-hoc servers** (`--with-mcp`): Defined inline for one-off use
3. **Wildcard** (`--use-mcps` with no args or `*`): Load all configured servers

**Processing Flow**:
1. Parse options → Store in `ChatCommand.UseMcps` or `WithStdioMcps`
2. Load MCP configurations → `AddMcpFunctions()` (ChatCommand.cs L116)
3. Register tools → `factory.AddFunctions()` (ChatCommand.cs L103-113)
4. AI can call tools → Function calling loop (ChatCommand.cs L188-192)

**Built-in Functions** (Always Available):
- Date/time helpers (L104)
- Shell commands (L105)
- Background processes (L106)
- File editing (StrReplace) (L107)
- Thinking tool (L108)
- Code exploration (L109)
- Image helpers (L110)
- Screenshot capture (L111)
- Shell/process management (L112)
- GitHub search (L113)

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L443-469
- Execution: `ChatCommand.cs` L103-116
- Proof: See [proof document](cycod-chat-filtering-pipeline-catalog-layer-8-proof.md)

### Conversation History Management

Load and save conversation context:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--chat-history [file]` | Use file for input and output | L549-557 |
| `--input-chat-history <file>` | Load history from file | L558-565 |
| `--continue` | Continue most recent conversation | L566-570 |
| `--output-chat-history [file]` | Save history to file | L571-577 |
| `--output-trajectory [file]` | Save trajectory log | L578-584 |

**Default Filenames**:
- `--chat-history`: `chat-history.jsonl` (L728)
- `--output-chat-history`: `chat-history-{time}.jsonl` (L729)
- `--output-trajectory`: `trajectory-{time}.md` (L730)

**History Loading** (ChatCommand.cs L134-146):
1. Check if `InputChatHistory` is set
2. Load from JSONL file
3. Apply token limits (MaxPromptTokenTarget, MaxToolTokenTarget, MaxChatTokenTarget)
4. Update console title with conversation metadata

**History Saving**:
- **Explicit save**: `OutputChatHistory` (written at conversation end)
- **Auto-save**: `AutoSaveOutputChatHistory` (written incrementally)

**Trajectory Saving** (ChatCommand.cs L94-95):
- Markdown format execution trace
- Captures tool calls, results, and conversation flow

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L549-584, L728-730
- Execution: `ChatCommand.cs` L80-95, L134-146
- Proof: See Layer 7 for persistence details, Layer 8 for AI integration

### Multi-Modal Input

Add visual context to conversations:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--image <pattern>...` | Add images via glob pattern | L658-664 |

**Processing Flow**:
1. Parse glob patterns → Store in `ChatCommand.ImagePatterns`
2. Resolve patterns to file paths → `ImageResolver.ResolveImagePatterns()` (ChatCommand.cs L185)
3. Pass to AI → `CompleteChatStreamingAsync()` (ChatCommand.cs L188)
4. Clear patterns after use → (ChatCommand.cs L186)

**Supported Formats**: Depends on AI provider (typically PNG, JPEG, WebP)

**Use Cases**:
- Screenshot analysis
- Diagram understanding
- Code visualization
- UI/UX review

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L658-664
- Execution: `ChatCommand.cs` L185-188

### Token Management

Control context window usage:

| Option | Description | Config Key |
|--------|-------------|------------|
| N/A (config-based) | Maximum output tokens | `app.max_output_tokens` |
| N/A (config-based) | Maximum prompt tokens | `app.max_prompt_tokens` |
| N/A (config-based) | Maximum tool/function tokens | `app.max_tool_tokens` |
| N/A (config-based) | Maximum chat history tokens | `app.max_chat_tokens` |

**Default Values** (ChatCommand.cs):
- `MaxPromptTokenTarget`: 80000 (L75)
- `MaxToolTokenTarget`: 40000 (L76)
- `MaxChatTokenTarget`: 100000 (L77)
- `MaxOutputTokens`: Provider default (L72-73)

**Token Budget Enforcement**:
- **Prompt tokens**: System prompt + user prompts (L131-132)
- **Tool tokens**: Function definitions (L140-141)
- **Chat tokens**: Conversation history (L138-141)

**Pruning Strategy**:
- Older messages pruned first
- Persistent messages protected
- Tool results may be summarized

**Source**:
- Config loading: `ChatCommand.cs` L72-77
- Token limits applied: `ChatCommand.cs` L131-132, L138-141

### Title Generation

Automatically generate conversation titles:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--auto-generate-title [true\|false]` | Enable/disable auto-titling | L665-672 |

**Mechanism**: Uses AI to generate a concise title from conversation content.

**Configuration**: Stored in `KnownSettings.AppAutoGenerateTitles`

**Source**: `CycoDevCommandLineOptions.cs` L665-672

### Foreach Loops

Process multiple inputs with variable substitution:

| Option | Description | Line Reference |
|--------|-------------|----------------|
| `--foreach <spec>` | Iterate over variable values | L424-431 |

**Syntax**: `--foreach VAR in VALUE1 VALUE2 ...` or `--foreach VAR in @file`

**Behavior**:
- Automatically sets `Interactive = false` (L429)
- Executes command once per value
- Replaces `{VAR}` in instructions

**Use Cases**:
- Batch file processing
- Multiple API queries
- Test case generation

**Source**:
- Parsing: `CycoDevCommandLineOptions.cs` L424-431
- Execution: Handled by `CommandWithVariables` base class

## Processing Flow

### Initialization Phase

1. **Parse command-line** → Store options in `ChatCommand` object (L9-29)
2. **Load configuration** → Apply settings from config files (L72-77)
3. **Ground file paths** → Expand templates in file names (L80-84)
4. **Setup slash commands** → Register command handlers (L61-69)
5. **Initialize trajectory** → Prepare execution logging (L94-95)

### Context Building Phase

6. **Ground prompts** → Expand templates in system/user prompts (L98-100)
7. **Load AGENTS.md** → Add project context (L58)
8. **Create function factory** → Register all available tools (L103-116)
9. **Create chat client** → Initialize AI provider with system prompt (L119-120)
10. **Set trajectory metadata** → Record conversation metadata (L124)

### Conversation Phase

11. **Add persistent user messages** → Apply `--add-user-prompt` (L129-132)
12. **Load chat history** → Restore previous conversation (L134-146)
13. **Check for input** → Verify interactive mode or input instructions (L149-153)
14. **Conversation loop** → Process user inputs (L155-234)
    - Read user input (L158-160)
    - Handle slash commands (L169)
    - Display assistant label (L183)
    - Resolve images (L185)
    - **Call AI with tool access** (L188-192) ← **Core AI Processing**
    - Display response (streamed)
    - Save trajectory incrementally

### Finalization Phase

15. **Save chat history** → Write to `OutputChatHistory` (if specified)
16. **Finalize trajectory** → Complete execution log
17. **Clean up resources** → Close MCP connections, dispose clients

## Data Flow

```
Command Line Options
        ↓
CycoDevCommandLineOptions.Parse()
        ↓
    ChatCommand object
        ↓
        ├→ SystemPrompt/SystemPromptAdds ────────┐
        ├→ UserPromptAdds ───────────────────────┤
        ├→ InputInstructions ────────────────────┤
        ├→ Variables ────────────────────────────┤
        ├→ UseMcps / WithStdioMcps ──────────────┤
        ├→ ImagePatterns ────────────────────────┤
        ├→ InputChatHistory / OutputChatHistory ─┤
        └→ OutputTrajectory ─────────────────────┤
                                                 ↓
                                         ChatCommand.ExecuteAsync()
                                                 ↓
                                         GroundSystemPrompt()
                                         GroundUserPromptAdds()
                                         GroundInputInstructions()
                                                 ↓
                                         AddMcpFunctions()
                                                 ↓
                                         ChatClientFactory.CreateChatClient()
                                                 ↓
                                         FunctionCallingChat (with tools)
                                                 ↓
                                         Conversation loop
                                                 ↓
                                         CompleteChatStreamingAsync() ← AI CALL
                                                 ↓
                                         ├→ Handle function calls
                                         ├→ Stream response to console
                                         ├→ Save to trajectory
                                         └→ Auto-save chat history
                                                 ↓
                                         Save final chat history (if specified)
```

## Key Source Files

| File | Purpose | Key Lines |
|------|---------|-----------|
| `CycoDevCommandLineOptions.cs` | Parse Layer 8 options | L405-672 |
| `ChatCommand.cs` | Execute AI conversation | L54-234 |
| `FunctionCallingChat.cs` | AI interaction with tools | (see proof) |
| `ChatClientFactory.cs` | Create AI provider clients | (see proof) |
| `McpFunctionFactory.cs` | Register MCP tools | (see proof) |
| `TemplateVariables.cs` | Variable expansion | (see proof) |

## Cross-Layer Integration

### With Layer 1 (Target Selection)
- Input instructions select "what to process" (user input text)
- Chat history selects "previous conversation turns"

### With Layer 2 (Container Filter)
- Token limits filter which history messages to include
- Scope filtering (`--global`, etc.) not applicable to chat command

### With Layer 3 (Content Filter)
- Template variable substitution filters/transforms prompt content
- System prompt defines what content types AI should process

### With Layer 5 (Context Expansion)
- Chat history provides conversational context
- AGENTS.md provides project context
- MCP tools provide external context sources

### With Layer 6 (Display Control)
- Streaming display of AI responses (console output)
- Quiet mode affects display verbosity

### With Layer 7 (Output Persistence)
- Chat history saved to JSONL files
- Trajectory saved to Markdown files
- Auto-save for safety

### With Layer 9 (Actions on Results)
- Function calling enables AI to take actions
- MCP tools extend available actions
- Slash commands provide user-initiated actions

## Proof Document

For detailed source code evidence, line-by-line analysis, and call stack traces, see:
- **[Layer 8 Proof Document](cycod-chat-filtering-pipeline-catalog-layer-8-proof.md)**

## Related Documentation

- [Layer 1: Target Selection](cycod-chat-filtering-pipeline-catalog-layer-1.md)
- [Layer 3: Content Filter](cycod-chat-filtering-pipeline-catalog-layer-3.md)
- [Layer 5: Context Expansion](cycod-chat-filtering-pipeline-catalog-layer-5.md)
- [Layer 6: Display Control](cycod-chat-filtering-pipeline-catalog-layer-6.md)
- [Layer 7: Output Persistence](cycod-chat-filtering-pipeline-catalog-layer-7.md)
- [Layer 9: Actions on Results](cycod-chat-filtering-pipeline-catalog-layer-9.md)
- [Main Catalog](cycod-filter-pipeline-catalog-README.md)
