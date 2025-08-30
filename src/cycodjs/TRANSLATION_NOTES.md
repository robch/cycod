# CycoDJS - C# to TypeScript Translation Notes

This document describes the translation of the main cycod CLI application from C# to TypeScript (cycodjs).

## Project Overview

**CycoDJS** is the TypeScript port of the cycod CLI application - an AI-powered CLI tool for chat-based interaction with AI assistants including OpenAI, Claude, GitHub Copilot, and others.

## Project Structure

The TypeScript project maintains a similar structure to the C# original:

```
src/cycodjs/
├── src/
│   ├── bin/cycodjs.ts              # CLI entry point
│   ├── CycoDevProgramRunner.ts     # Main program runner
│   ├── CycoDevProgramInfo.ts       # Program metadata
│   ├── CommandLine/
│   │   └── CycoDevCommandLineOptions.ts
│   ├── CommandLineCommands/
│   │   ├── ChatCommand.ts          # Main chat command
│   │   ├── AliasCommands/          # Alias management
│   │   ├── ConfigCommands/         # Configuration management
│   │   ├── McpCommands/            # Model Context Protocol
│   │   └── PromptCommands/         # Prompt management
│   ├── ChatClient/                 # AI client factories
│   ├── FunctionCalling/            # Function calling support
│   ├── FunctionCallingTools/       # Built-in tools
│   ├── Helpers/                    # Chat and AI helpers
│   ├── McpHelpers/                 # MCP integration helpers
│   ├── Services/                   # Core services
│   ├── SlashCommands/              # Slash command handlers
│   └── Utilities/                  # Utility classes
├── package.json
├── tsconfig.json
└── TRANSLATION_NOTES.md
```

## Dependencies

The project depends on:
- **cycod-common** - The shared utilities library (local dependency)
- **TypeScript** - Type safety and compilation
- **Node.js built-ins** - fs, path, process, etc.

### Planned AI/Chat Dependencies (Not Yet Implemented)
- **@anthropic-ai/sdk** - For Claude integration
- **openai** - For OpenAI integration  
- **@azure/openai** - For Azure OpenAI
- **@google-ai/generativelanguage** - For Gemini
- **aws-sdk** - For AWS Bedrock

## Translation Status

### ✅ **Core Infrastructure (Completed)**

| C# Class | TypeScript Class | Status |
|----------|------------------|---------|
| `Program` | `bin/cycodjs.ts` | ✅ CLI entry point |
| `CycoDevProgramRunner` | `CycoDevProgramRunner.ts` | ✅ Main runner |
| `CycoDevProgramInfo` | `CycoDevProgramInfo.ts` | ✅ Program metadata |
| `CycoDevCommandLineOptions` | `CycoDevCommandLineOptions.ts` | ✅ Command parsing |
| `ChatCommand` | `ChatCommand.ts` | ✅ Basic structure |

### ⚠️ **Partially Implemented**

| C# Module | TypeScript Status | Notes |
|-----------|-------------------|-------|
| **CommandLineCommands** | Basic chat command only | Need alias, config, mcp, prompt commands |
| **ChatClient** | Not implemented | Need AI provider factories |
| **FunctionCalling** | Not implemented | Function calling detection and execution |
| **FunctionCallingTools** | Not implemented | Built-in tools (shell, file editing, etc.) |
| **Helpers** | Not implemented | Chat history, response rendering, etc. |
| **McpHelpers** | Not implemented | Model Context Protocol integration |
| **Services** | Not implemented | Core interaction services |
| **SlashCommands** | Not implemented | Slash command processing |

### ❌ **Not Yet Implemented**

1. **AI Provider Integration**
   - OpenAI client setup
   - Claude/Anthropic client
   - GitHub Copilot integration
   - Azure OpenAI support
   - AWS Bedrock support
   - Google Gemini support

2. **Chat Functionality**
   - Message history management
   - Conversation persistence
   - Response streaming
   - Markdown rendering
   - Token counting and limits

3. **Function Calling**
   - Tool definition and registration
   - Function call detection
   - Tool execution framework
   - Built-in tools (shell, editor, etc.)

4. **Model Context Protocol (MCP)**
   - MCP client management
   - Server configuration
   - Tool discovery and execution

5. **Configuration Management**
   - Settings persistence
   - Provider configuration
   - Alias management
   - Prompt template system

6. **Advanced Features**
   - Image handling and vision
   - File attachments
   - Trajectory logging
   - Interactive mode
   - Streaming responses

## Current Functionality

### ✅ **Working Features**
- CLI application starts and runs
- Basic command-line parsing
- Default chat command creation
- Program runner infrastructure
- TypeScript compilation and execution

### 🔧 **Usage Example**
```bash
# Install dependencies
npm install

# Build the project
npm run build

# Run the CLI
node dist/bin/cycodjs.js

# Test basic functionality
npm start
```

### 🧪 **Basic Test Results**
```bash
$ node dist/bin/cycodjs.js
CycoDJS - AI-powered CLI
Chat command executed with instructions: []
```

## Translation Challenges & Solutions

### 1. **Circular Import Dependencies**
- **Issue**: ConfigFile imported IniConfigFile which extended ConfigFile
- **Solution**: Used dynamic require() in static factory method

### 2. **Complex C# AI SDK Dependencies**
- **Issue**: Multiple .NET AI SDKs not available in Node.js
- **Solution**: Plan to use equivalent npm packages (openai, @anthropic-ai/sdk, etc.)

### 3. **Advanced .NET Features**
- **Issue**: Some C# features don't have direct TypeScript equivalents
- **Solution**: Implement equivalent functionality using Node.js patterns

## Next Development Steps

### **Phase 1: Basic Chat Implementation**
1. Implement OpenAI client integration
2. Add basic chat message handling
3. Implement response rendering
4. Add configuration management

### **Phase 2: Advanced Features**  
1. Add Claude/Anthropic integration
2. Implement function calling framework
3. Add built-in tools (shell, editor)
4. Implement MCP support

### **Phase 3: Full Feature Parity**
1. Add all AI provider support
2. Implement complete command set
3. Add advanced features (vision, streaming)
4. Performance optimization

## Development Commands

```bash
# Development
npm run dev          # Watch mode compilation
npm run build        # Build TypeScript
npm run clean        # Clean dist folder
npm run lint         # Run ESLint
npm run test         # Run tests (when implemented)

# Usage
npm start            # Run the CLI
node dist/bin/cycodjs.js --help   # Show help
```

## Architecture Notes

- **Modular Design**: Each major feature area has its own module
- **Dependency Injection**: Use of factories and builders for extensibility
- **Error Handling**: Comprehensive error handling with typed exceptions
- **Configuration**: Hierarchical configuration system (global, user, local)
- **Extensibility**: Plugin-like architecture for AI providers and tools

## Known Limitations

1. **AI Integration**: No actual AI calls implemented yet
2. **Function Calling**: Tool framework not implemented
3. **Configuration**: Basic parsing only, no persistence
4. **Help System**: Placeholder help text only
5. **Error Handling**: Basic error handling, needs improvement
6. **Testing**: No unit tests implemented yet

## Conclusion

The basic TypeScript project structure is established and compiles successfully. The core infrastructure is in place to support the full implementation of the cycod CLI features. The next phase will focus on implementing the AI provider integration and basic chat functionality.