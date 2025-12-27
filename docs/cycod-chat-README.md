# cycod chat Command - Filtering Pipeline Catalog

## Overview

The `chat` command is the primary (and default) command of the `cycod` CLI. It provides interactive AI conversation with:
- Tool/function calling capabilities
- Chat history persistence and loading
- Template variable substitution
- MCP (Model Context Protocol) server integration
- Image attachment support
- System and user prompt customization

## Command Identification

**Source**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs:26`

```csharp
override protected string PeekCommandName(string[] args, int i)
{
    var name1 = GetInputOptionArgs(i, args, max: 1).FirstOrDefault();
    return name1 switch
    {
        "chat" => "chat",  // ← Line 28
        _ => base.PeekCommandName(args, i)
    };
}
```

**Default command**: Line 43-46
```csharp
override protected Command? NewDefaultCommand()
{
    return new ChatCommand();  // ← Line 45
}
```

## 9-Layer Pipeline Implementation

| Layer | Status | Description |
|-------|--------|-------------|
| [Layer 1: TARGET SELECTION](cycod-chat-layer-1.md) | ✅ | Conversation history loading, input instructions |
| [Layer 2: CONTAINER FILTER](cycod-chat-layer-2.md) | ⚠️ | Provider selection, MCP filtering |
| [Layer 3: CONTENT FILTER](cycod-chat-layer-3.md) | ⚠️ | Template processing, variable substitution |
| [Layer 4: CONTENT REMOVAL](cycod-chat-layer-4.md) | 🔍 | Implicit through template conditionals |
| [Layer 5: CONTEXT EXPANSION](cycod-chat-filtering-pipeline-catalog-layer-5.md) | ✅ COMPLETE | Chat history context, system/user prompts, variables, AGENTS.md, MCPs, images | [Proof](cycod-chat-filtering-pipeline-catalog-layer-5-proof.md) |
| [Layer 6: DISPLAY CONTROL](cycod-chat-layer-6.md) | ✅ | Quiet mode, streaming output, console formatting |
| [Layer 7: OUTPUT PERSISTENCE](cycod-chat-layer-7.md) | ✅ | Chat history saving, trajectory logging |
| [Layer 8: AI PROCESSING](cycod-chat-layer-8.md) | ✅ | Core purpose - AI chat completion |
| [Layer 9: ACTIONS ON RESULTS](cycod-chat-layer-9.md) | ✅ | Tool/function calling, slash commands |

## Command-Line Options Summary

### Input/Instruction Options
- `--input`, `--instruction`, `--question`, `-q`: Add input instruction(s)
- `--inputs`, `--instructions`, `--questions`: Add multiple input instructions
- Positional stdin: Implicit input when redirected and no other input provided

### System/User Prompt Options
- `--system-prompt`: Override system prompt
- `--add-system-prompt`: Add to system prompt
- `--add-user-prompt`, `--prompt`: Add persistent user message

### Chat History Options
- `--chat-history`: Load and save to same file (default: `chat-history.jsonl`)
- `--input-chat-history`: Load from specific file
- `--output-chat-history`: Save to specific file (default: `chat-history-{time}.jsonl`)
- `--continue`: Load most recent chat history
- `--output-trajectory`: Save trajectory log (default: `trajectory-{time}.md`)

### Variable Options
- `--var NAME=VALUE`: Set template variable
- `--vars NAME=VALUE ...`: Set multiple template variables
- `--foreach VAR=value1,value2`: For-each variable expansion
- `--use-templates`, `--no-templates`: Enable/disable template processing

### MCP Options
- `--use-mcps [name...]`, `--mcp [name...]`: Use configured MCP servers
- `--no-mcps`: Disable all MCP servers
- `--with-mcp <command> [args...]`: Add ad-hoc stdio MCP server

### Provider Selection Options
- `--use-anthropic`: Use Anthropic Claude
- `--use-azure-anthropic`: Use Azure Anthropic
- `--use-aws`, `--use-bedrock`, `--use-aws-bedrock`: Use AWS Bedrock
- `--use-azure-openai`, `--use-azure`: Use Azure OpenAI
- `--use-google`, `--use-gemini`, `--use-google-gemini`: Use Google Gemini
- `--use-grok`, `--use-x.ai`: Use Grok (X.AI)
- `--use-openai`: Use OpenAI
- `--use-copilot`, `--use-copilot-token`: Use GitHub Copilot
- `--use-test`: Use test provider

### Provider-Specific Options
- `--grok-api-key`: Set Grok API key
- `--grok-model-name`: Set Grok model name
- `--grok-endpoint`: Set Grok endpoint URL

### Image Options
- `--image <pattern>`: Attach images matching pattern

### Display Options
- `--auto-generate-title`: Auto-generate conversation titles

## Source Files

- **Parser**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs` (lines 209-489)
- **Implementation**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- **Base**: `src/common/CommandLineCommands/CommandWithVariables.cs`

## Examples

```bash
# Basic chat
cycod "What is the weather today?"

# Chat with history continuation
cycod --continue "Tell me more"

# Chat with specific provider
cycod --use-anthropic "Explain recursion"

# Chat with template variables
cycod --var name=John "Hello, {name}!"

# Chat with MCP integration
cycod --use-mcps filesystem "List files in current directory"

# Chat with image attachment
cycod --image "screenshot.png" "What do you see in this image?"

# Non-interactive with multiple inputs
cycod --questions "What is 2+2?" "What is 3+3?" --quiet
```

## Related Documentation

- [ChatCommand Implementation](cycod-chat-layer-8-proof.md#implementation)
- [Template Variable System](cycod-chat-layer-3-proof.md#template-processing)
- [MCP Integration](cycod-chat-layer-2-proof.md#mcp-filtering)
- [Tool/Function System](cycod-chat-layer-9-proof.md#function-calling)
