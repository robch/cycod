# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CycoD is an AI-powered CLI application written in C# (.NET 8.0+) that provides chat-based interaction with multiple AI providers. The codebase consists of three main projects plus MCP server implementations:

- **cycod**: Main interactive CLI for AI chat with function calling
- **cycodmd**: Markdown document generator for files, web content, and command outputs
- **cycodt**: YAML-based test framework for validation
- **common**: Shared infrastructure (configuration, logging, process execution, shell sessions)
- **MCP servers**: weather, geolocation, mxlookup, osm, whois

## Build and Test Commands

### Building
```bash
# Standard build (generates dev version with username/timestamp)
./scripts/build.sh

# Build with specific version and configuration
./scripts/build.sh 1.0.0 Release

# Standard dotnet build
dotnet build
```

### Running
```bash
# Run cycod from source
dotnet run --project src/cycod/cycod.csproj

# Run cycodmd from source
dotnet run --project src/cycodmd/cycodmd.csproj -- <args>

# Run cycodt from source (only if cycodt not in PATH)
dotnet run --project src/cycodt/cycodt.csproj -- <args>
```

### Testing with cycodt
The project uses a custom YAML-based test framework. Tests are located in `tests/cycod-yaml/`, `tests/cycodmd-yaml/`, and `tests/cycodt-yaml/`.

```bash
# List all tests in a file
cycodt list --file tests/cycod-yaml/config-tests.yaml

# Run a specific test
cycodt run --file tests/cycod-yaml/config-tests.yaml --test "test name"

# Run all tests in a file
cycodt run --file tests/cycod-yaml/config-tests.yaml

# Include optional/broken tests
cycodt list --include-optional broken-test
```

**Important**: Use `cycodt` directly (not `dotnet run`) when possible to avoid rebuild delays and file locking issues.

### Unit Tests
```bash
# Run xUnit tests
dotnet test tests/cycod/Tests.csproj
```

## High-Level Architecture

### Core Components and Data Flow

```
User Input → ChatCommand → ChatClientFactory → IChatClient (Provider)
                ↓
         FunctionCallingChat (orchestrator)
                ↓
    McpFunctionFactory + Built-in Functions
                ↓
         Function Execution
                ↓
         AI Response → User
```

### 1. Chat System Architecture

**Entry Point**: `Program.cs` → `CycoDevProgramRunner.RunAsync()` → `ChatCommand.ExecuteAsync()`

**Key Flow**:
- Configuration loaded from three-tier hierarchy: Global → User → Local (plus environment variables and command-line)
- AI provider selected via `ChatClientFactory` based on config or environment
- `McpFunctionFactory` registers all tools (built-in functions + MCP server tools)
- `FunctionCallingChat` wraps the `IChatClient` and orchestrates:
  - Message history management
  - Streaming response handling
  - Function call detection and execution
  - Conversation loop with function results

**Provider Selection Strategy**:
1. Check `app.preferred-provider` config setting
2. Fall back to environment variable detection
3. Try providers in order: Copilot → Anthropic → AWS Bedrock → Gemini → Grok → Azure OpenAI → OpenAI

**Supported AI Providers** (all use `IChatClient` from Microsoft.Extensions.AI):
- GitHub Copilot (with auto-refresh token, vision headers)
- OpenAI (GPT-4o)
- Azure OpenAI
- Anthropic (Claude models)
- Google Gemini
- AWS Bedrock
- Grok/X.ai

### 2. Function Calling System

**Core Architecture**:
- `FunctionFactory`: Base class using reflection to discover and register functions via `[Description]` attributes
- `McpFunctionFactory`: Extends base to add MCP server tools, routes calls to appropriate MCP client
- `FunctionCallDetector`: Monitors streaming responses, accumulates partial function calls, extracts complete calls with JSON arguments

**Built-in Tool Categories**:
- Shell operations: `ShellCommandToolHelperFunctions`, `BackgroundProcessHelperFunctions`, `ShellAndProcessHelperFunctions`
- File operations: `StrReplaceEditorHelperFunctions`
- Code analysis: `CodeExplorationHelperFunctions`
- Debugging: `DebuggerFunctions`, `DebugLifecycleFunctions` (preview feature)
- Image processing: `ImageHelperFunctions`
- Utilities: `DateAndTimeHelperFunctions`, `ThinkingToolHelperFunction`

**Function Execution Flow**:
1. AI requests function call in streaming response
2. `FunctionCallDetector` accumulates streaming chunks and detects complete calls
3. `McpFunctionFactory.TryCallFunction()` routes to MCP server or invokes built-in function
4. Results added to message history as tool result messages
5. Conversation continues with function results until AI provides final response

### 3. Configuration System

**ConfigStore (Singleton)**:
- Three-tier hierarchy with dot notation keys (`app.preferred-provider`)
- **Command-line** (highest priority) → **Environment variables** (includes `.env` files) → **Config files** (lowest priority)
- Config file scopes: Local (`.cycod/config.*`) → User (`~/.cycod/config.*`) → Global (`/etc/.cycod/config.*`)
- Supports YAML and INI formats
- Features: secret masking, list operations (add/remove), nested dictionaries

**Key Settings**:
- `app.preferred-provider`: AI provider selection (openai, azure-openai, copilot, anthropic, etc.)
- `app.max-output-tokens`, `app.max-prompt-tokens`, `app.max-tool-tokens`: Token limits
- `app.max-chat-tokens`: Conversation trimming threshold
- `app.chat-completion-timeout`: Timeout for chat completion

**Helper Classes** (located in `src/common/Helpers/`):
- `FileHelpers`: Core file operations (reading, writing, finding files, path manipulation)
- `AgentsFileHelpers`: Handles AGENTS.md and agent instruction files
- `ChatHistoryFileHelpers`: Chat history file management
- `ScopeFileHelpers`: Configuration scope file handling
- `PromptFileHelpers`: Prompt template management
- `McpFileHelpers`: MCP server configuration handling

### 4. MCP (Model Context Protocol) Integration

**Lifecycle**:
1. Configuration loaded from `.cycod/mcp.json` or config files
2. `McpClientManager.CreateClientsAsync()` instantiates servers
3. For each server: create transport (stdio or SSE), initialize client, list available tools
4. Tools registered with `McpFunctionFactory.AddMcpClientToolsAsync()`
5. Tools callable like built-in functions with `{serverName}_{toolName}` prefix

**Error Handling**: Partial server failures don't block the system; graceful degradation with logging.

### 5. Debugger Integration (Preview)

Located in `src/cycod/Debugging/`. Implements DAP (Debug Adapter Protocol) for .NET debugging:

**Components**:
- `DebugSessionManager`: Manages active debug sessions
- `DapClient`: DAP protocol communication
- `DebugSession`: Individual session state
- `NetcoredbgLocator`: Finds .NET debugger executable (netcoredbg)

**Available Operations**:
- Session: `StartDebugSession(programPath)`, `ListSessions`, `TerminateSession`, `CleanupIdleSessions`
- Breakpoints: `SetBreakpoint`, `DeleteBreakpoint`, `ListBreakpoints`
- Execution: `Continue`, `StepOver`, `StepIn`, `StepOut`
- Inspection: `StackTrace`, `Scopes`, `Variables`, `Evaluate`
- Mutation: `SetVariable` (capability dependent)
- Events: `FetchEvents(sinceSeq)`

See `docs/debug-tools.md` for details.

## Code Style and Conventions

**Critical**: The project has specific C# coding standards. Refer to AGENTS.md for detailed guidance. Key points:

- **Use `var`** consistently for local variables (unless type clarity is needed)
- **Prefer concise code**: ternary operators for simple conditionals, LINQ/functional patterns
- **Smaller, focused methods** over large multi-purpose ones
- **Singular methods** (e.g., `ProcessFile`) over batch methods (e.g., `ProcessFiles`)
- **XML docs for public members only**; let code be self-documenting
- **Console output**: Use `ConsoleHelpers` methods, not direct `Console` calls
  - Red for errors (`ConsoleHelpers.WriteErrorLine`)
  - Yellow for warnings
  - White/default for standard output
- **Async/await**: Always use `Async` suffix, never `ConfigureAwait(false)` in app code
- **Resource management**: Prefer `using` declarations (C# 8.0+) over try/finally
- **Error handling**: Use specific exception types, follow existing patterns in codebase
- **Organize by feature/functionality**, not by type

## Testing Best Practices

### cycodt YAML Tests

**File Creation and Content Verification**:
- Use `cycodmd` with patterns to verify both file creation and content in one step
- Example: `dotnet run --project ../../src/cycodmd/cycodmd.csproj -- log-*.log`

**Side Effect Detection**:
- Use `not-expect-regex` to catch unwanted files ("turd files")
- Example: `not-expect-regex: "## exception-log-.*\.log"`

**Test Structure**:
- Clean up only at the end to allow debugging failed tests
- Use minimal comments; step names should be self-documenting
- Use `|` for multi-line scripts/commands to preserve line breaks

**Handling Broken Tests**:
- Mark as optional with `broken-test` category: `optional: broken-test`
- Document in `todo-{problem}.md` with reproduction steps

See `src/cycodt/TestFramework/README.md` for full documentation.

## Important Files and Locations

### Core Source Files
- `src/cycod/Program.cs`: Entry point for cycod CLI
- `src/cycod/CycoDevProgramRunner.cs`: Main program orchestration
- `src/cycod/CommandLineCommands/ChatCommand.cs`: Chat command implementation (1,075 lines)
- `src/cycod/ChatClient/ChatClientFactory.cs`: AI provider selection and client creation
- `src/cycod/FunctionCalling/FunctionCallingChat.cs`: Orchestrates function calling loop
- `src/cycod/FunctionCalling/FunctionFactory.cs`: Base function registration via reflection
- `src/cycod/FunctionCalling/McpFunctionFactory.cs`: Extends with MCP server tools
- `src/cycod/FunctionCalling/FunctionCallDetector.cs`: Detects function calls in streaming responses
- `src/common/Configuration/ConfigStore.cs`: Singleton configuration manager

### Helper Classes (src/common/Helpers/)
- `FileHelpers.cs`: Core file operations
- `AgentsFileHelpers.cs`: Agent instruction files
- `ChatHistoryFileHelpers.cs`: Chat history management
- `ScopeFileHelpers.cs`: Configuration scope handling
- `PromptFileHelpers.cs`: Prompt templates
- `McpFileHelpers.cs`: MCP configuration

### Configuration Files
- `.cycod/config.yaml` or `.cycod/config`: Local project configuration
- `~/.cycod/config.yaml` or `~/.cycod/config`: User configuration
- `/etc/.cycod/config.yaml` or `/etc/.cycod/config`: Global configuration
- `.cycod/mcp.json`: MCP server configuration
- `.cycod/profiles/<name>.yaml`: Named configuration profiles

### Documentation
- `docs/getting-started.md`: Installation and basic usage
- `docs/cli-options.md`: Command-line options reference
- `docs/function-calling.md`: Function calling capabilities
- `docs/debug-tools.md`: Debugger integration documentation
- `AGENTS.md`: Detailed coding standards and development guidelines
- `src/cycodt/TestFramework/README.md`: Test framework documentation

## Cross-Platform Considerations

The application runs on Windows, macOS, and Linux. Be mindful of:
- Path separators (`\` vs `/`)
- Line endings (CRLF vs LF)
- Process execution differences across platforms
- File system permissions

Shell commands execute through helper methods that handle platform-specific concerns.

## Key Architectural Patterns

- **Singleton**: `ConfigStore`, `NamedShellProcessManager`
- **Factory**: `ChatClientFactory`, `FunctionFactory`, `McpClientManager`
- **Strategy**: Provider selection logic
- **Adapter**: Provider-to-IChatClient conversion (e.g., `AsIChatClient()`)
- **Decorator**: `FunctionCallingChat` wraps `IChatClient`
- **Template Method**: `ProgramRunner` base class
- **Reflection-based Registration**: Functions discovered via `[Description]` attributes
