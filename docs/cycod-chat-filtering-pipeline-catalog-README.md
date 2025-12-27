# cycod chat - Filtering Pipeline Catalog

## Command Overview

The `chat` command is cycod's primary AI interaction command. It processes user input, manages conversation history, interacts with AI models, and executes tool functions.

## Default Command

`chat` is the default command when no command name is specified.

## Pipeline Layers

### Layer 1: [TARGET SELECTION](cycod-chat-layer-1.md)
- [Proof](cycod-chat-layer-1-proof.md)
- Input instructions, chat history, system/user prompts, images

### Layer 2: [CONTAINER FILTER](cycod-chat-layer-2.md)
- [Proof](cycod-chat-layer-2-proof.md)
- ❌ Not applicable - chat doesn't filter containers

### Layer 3: [CONTENT FILTER](cycod-chat-layer-3.md)
- [Proof](cycod-chat-layer-3-proof.md)
- Template processing, slash command handling

### Layer 4: [CONTENT REMOVAL](cycod-chat-layer-4.md)
- [Proof](cycod-chat-layer-4-proof.md)
- Token-based message trimming, `/clear` command

### Layer 5: [CONTEXT EXPANSION](cycod-chat-layer-5.md)
- [Proof](cycod-chat-layer-5-proof.md)
- System prompt additions, user prompt additions

### Layer 6: [DISPLAY CONTROL](cycod-chat-layer-6.md) ✅
- [Proof](cycod-chat-layer-6-proof.md) ✅
- Interactive/quiet/verbose/debug modes, function call display

### Layer 7: [OUTPUT PERSISTENCE](cycod-chat-layer-7.md)
- [Proof](cycod-chat-layer-7-proof.md)
- Chat history files, trajectory files, auto-save

### Layer 8: [AI PROCESSING](cycod-chat-layer-8.md)
- [Proof](cycod-chat-layer-8-proof.md)
- AI model selection, MCP functions, tool calling, streaming

### Layer 9: [ACTIONS ON RESULTS](cycod-chat-filtering-pipeline-catalog-layer-9.md)
- [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md)
- Interactive conversation, function calling, slash commands, history management, title generation

## Command-Specific Features

- **Interactive Mode**: Multi-turn conversation with the assistant
- **Chat History**: Load and save conversation history
- **MCP Integration**: Connect to Model Context Protocol servers
- **Tool Functions**: Execute filesystem, shell, code exploration, and other operations
- **Templating**: Variable substitution and template processing
- **Slash Commands**: Built-in commands like `/save`, `/clear`, `/help`, `/title`
- **Image Support**: Include images in conversations
- **Title Generation**: Automatic conversation titling

## Related Files

- Parser: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`
- Command: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Helper Functions: Various in `src/cycod/HelperFunctions/`
