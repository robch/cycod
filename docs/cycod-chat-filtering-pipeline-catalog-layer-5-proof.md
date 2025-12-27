# cycod chat - Layer 5: Context Expansion - PROOF

This document provides source code evidence for all context expansion mechanisms in the cycod chat command.

## Source Files Referenced

- **Primary Parser**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`
- **Command Implementation**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- **Related Helpers**: 
  - `src/cycod/Helpers/ChatHistoryFileHelpers.cs`
  - `src/common/Helpers/AgentsFileHelpers.cs`
  - `src/common/Helpers/TemplateVariables.cs`

---

## 1. Chat History Loading (Conversational Context)

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--chat-history` Option
```csharp
Lines 549-557:
        else if (arg == "--chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
            command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
            command.OutputChatHistory = chatHistory;
            command.LoadMostRecentChatHistory = false;
            i += max1Arg.Count();
        }
```

**Evidence**:
- Line 551: Retrieves optional filename argument (max 1 arg)
- Line 552: Falls back to `DefaultSimpleChatHistoryFileName` if no arg provided
- Line 553: Sets `InputChatHistory` if file exists
- Line 554: Sets same file for output (continue mode)
- Line 555: Disables auto-loading most recent history

#### `--input-chat-history` Option
```csharp
Lines 558-565:
        else if (arg == "--input-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
            command.InputChatHistory = inputChatHistory;
            command.LoadMostRecentChatHistory = false;
            i += max1Arg.Count();
        }
```

**Evidence**:
- Line 560: Retrieves required filename argument
- Line 561: Validates file exists (throws if not)
- Line 562: Sets `InputChatHistory` property
- Line 563: Disables auto-loading most recent history

#### `--continue` Option
```csharp
Lines 566-570:
        else if (arg == "--continue")
        {
            command.LoadMostRecentChatHistory = true;
            command.InputChatHistory = null;
        }
```

**Evidence**:
- Line 568: Sets flag to load most recent history file
- Line 569: Clears explicit history file (auto-detection mode)

#### `--output-chat-history` Option
```csharp
Lines 571-577:
        else if (arg == "--output-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
            command.OutputChatHistory = outputChatHistory;
            i += max1Arg.Count();
        }
```

**Evidence**:
- Line 573: Retrieves optional filename/template argument
- Line 574: Falls back to `DefaultOutputChatHistoryFileNameTemplate` (`chat-history-{time}.jsonl`)
- Line 575: Sets output history file path/template

#### Default Templates
```csharp
Lines 728-730:
    private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
    private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
    private const string DefaultOutputTrajectoryFileNameTemplate = "trajectory-{time}.md";
```

**Evidence**:
- Default input history filename: `chat-history.jsonl`
- Default output template: `chat-history-{time}.jsonl` (timestamped)

### History Loading Implementation

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Grounding History Filenames
```csharp
Lines 80-84:
        // Ground the filenames (in case they're templatized, or auto-save is enabled).
        InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
        AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
        AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
        OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
```

**Evidence**:
- Line 81: Resolves input history filename (handles `--continue` auto-detection and template variables)
- Line 82-83: Sets up auto-save filenames for history and trajectory
- Line 84: Resolves output history template with variables
- All filenames support template variable expansion

#### Loading History into Conversation
```csharp
Lines 134-146:
            // Load the chat history from the file.
            var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
            if (loadChatHistory)
            {
                chat.Conversation.LoadFromFile(InputChatHistory!,
                    maxPromptTokenTarget: MaxPromptTokenTarget,
                    maxToolTokenTarget: MaxToolTokenTarget,
                    maxChatTokenTarget: MaxChatTokenTarget,
                    useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                // Update console title with loaded conversation title
                ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
            }
```

**Evidence**:
- Line 135: Checks if history filename was resolved
- Line 138: Loads history from file into conversation
- Lines 139-141: Applies token limits during loading (context filtering)
- Line 142: Uses OpenAI-compatible format
- Line 145: Updates window title with conversation metadata

**Context Expansion Behavior**: All loaded messages become part of the conversation context for subsequent AI interactions.

---

## 2. System Prompt Context

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--system-prompt` Option
```csharp
Lines 470-476:
        else if (arg == "--system-prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "system prompt");
            command.SystemPrompt = prompt;
            i += promptArgs.Count();
        }
```

**Evidence**:
- Line 472: Retrieves all arguments until next option
- Line 473: Joins multiple args with double newlines
- Line 474: Validates and sets system prompt (overrides default/config)
- **Override behavior**: This replaces any existing system prompt

#### `--add-system-prompt` Option
```csharp
Lines 477-486:
        else if (arg == "--add-system-prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional system prompt");
            if (!string.IsNullOrEmpty(prompt))
            {
                command.SystemPromptAdds.Add(prompt);
            }
            i += promptArgs.Count();
        }
```

**Evidence**:
- Line 479: Retrieves all arguments until next option
- Line 480: Joins multiple args with double newlines
- Line 483: Adds to `SystemPromptAdds` list (accumulative)
- **Accumulative behavior**: Multiple `--add-system-prompt` options accumulate

### System Prompt Assembly

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Grounding System Prompt
```csharp
Line 98:
        // Ground the system prompt, added user messages, and InputInstructions.
        SystemPrompt = GroundSystemPrompt();
```

**Evidence**: System prompt is processed through `GroundSystemPrompt()` method

#### System Prompt Properties
```csharp
Lines 27-28 (from Clone method):
        clone.SystemPrompt = this.SystemPrompt;
        clone.SystemPromptAdds = new List<string>(this.SystemPromptAdds);
```

**Evidence**:
- `SystemPrompt`: Single string (override)
- `SystemPromptAdds`: List of strings (additions)

#### System Prompt Usage in AI Client
```csharp
Line 120:
        var chat = new FunctionCallingChat(chatClient, SystemPrompt, factory, options, MaxOutputTokens);
```

**Evidence**: System prompt is passed to `FunctionCallingChat` constructor as foundational context

**Context Expansion Behavior**: System prompt establishes the AI's role and constraints for the entire conversation.

---

## 3. User Prompt Additions (Instruction Context)

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--add-user-prompt` and `--prompt` Options
```csharp
Lines 487-498:
        else if (arg == "--add-user-prompt" || arg == "--prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional user prompt");
            if (!string.IsNullOrEmpty(prompt))
            {
                var needsSlashPrefix = arg == "--prompt" && !prompt.StartsWith("/");
                var prefix = needsSlashPrefix ? "/" : string.Empty;
                command.UserPromptAdds.Add($"{prefix}{prompt}");
            }
            i += promptArgs.Count();
        }
```

**Evidence**:
- Line 489: Retrieves all arguments until next option
- Line 490: Joins multiple args with double newlines
- Line 493: Special handling for `--prompt` variant
- Line 494: Auto-prefixes with `/` if using `--prompt` and no slash present
- Line 495: Adds to `UserPromptAdds` list (accumulative)

**Context Expansion Behavior**: User prompt additions provide instruction context before main input.

### User Prompt Assembly

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Grounding User Prompts
```csharp
Line 99:
        UserPromptAdds = GroundUserPromptAdds();
```

**Evidence**: User prompts are processed through `GroundUserPromptAdds()` method

#### Adding User Prompts to Conversation
```csharp
Lines 128-132:
            // Add the user prompt messages to the chat.
            chat.Conversation.AddPersistentUserMessages(
                UserPromptAdds,
                maxPromptTokenTarget: MaxPromptTokenTarget,
                maxChatTokenTarget: MaxChatTokenTarget);
```

**Evidence**:
- Line 129: Adds user prompt additions as "persistent" messages
- Lines 131-132: Applies token limits (context filtering)
- **Persistent**: These messages remain in context throughout conversation

---

## 4. Variable Context (Template Variable Expansion)

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--var` Option
```csharp
Lines 405-412:
        else if (arg == "--var")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var assignment = ValidateAssignment(arg, max1Arg.FirstOrDefault());
            command.Variables[assignment.Item1] = assignment.Item2;
            ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
            i += max1Arg.Count();
        }
```

**Evidence**:
- Line 407: Retrieves single argument
- Line 408: Validates `NAME=VALUE` format
- Line 409: Stores in command's `Variables` dictionary
- Line 410: Also stores in global config store with `Var.` prefix
- **Storage**: Both command-local and global scopes

#### `--vars` Option
```csharp
Lines 413-423:
        else if (arg == "--vars")
        {
            var varArgs = GetInputOptionArgs(i + 1, args);
            var assignments = ValidateAssignments(arg, varArgs);
            foreach (var assignment in assignments)
            {
                command.Variables[assignment.Item1] = assignment.Item2;
                ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
            }
            i += varArgs.Count();
        }
```

**Evidence**:
- Line 415: Retrieves all arguments until next option
- Line 416: Validates multiple `NAME=VALUE` assignments
- Lines 417-421: Iterates and stores each variable
- **Batch processing**: Multiple variables in one option

### Variable Usage

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Template Variables Initialization
```csharp
Lines 56-58:
        // Setup the named values
        _namedValues = new TemplateVariables(Variables);
        AddAgentsFileContentToTemplateVariables();
```

**Evidence**:
- Line 57: Creates `TemplateVariables` instance from command's `Variables` dictionary
- Line 58: Adds AGENTS.md content as additional variables
- **Template system**: Variables available for `{NAME}` substitution

#### Variable Expansion in Filenames
```csharp
Lines 80-84:
        InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
        AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
        AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
        OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
```

**Evidence**: All filename paths are expanded with `.ReplaceValues(_namedValues)`

**Context Expansion Behavior**: Variables provide reusable contextual values throughout command execution.

---

## 5. AGENTS.md File Context

### File Discovery and Loading

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Adding AGENTS.md to Template Variables
```csharp
Lines 228-250:
    /// <summary>
    /// Adds AGENTS.md file content to template variables
    /// </summary>
    private void AddAgentsFileContentToTemplateVariables()
    {
        if (_namedValues == null)
            return;

        var agentsFile = AgentsFileHelpers.FindAgentsFile();
        if (agentsFile != null && FileHelpers.FileExists(agentsFile))
        {
            var agentsContent = FileHelpers.ReadAllText(agentsFile);
            if (!string.IsNullOrEmpty(agentsContent))
            {
                // Store the AGENTS.md content as a template variable
                _namedValues.Set("agents.md", agentsContent);
                _namedValues.Set("agents.file", Path.GetFileName(agentsFile));
                _namedValues.Set("agents.path", agentsFile);

                ConsoleHelpers.WriteDebugLine($"Added AGENTS.md content from {agentsFile} as template variable");
            }
        }
    }
```

**Evidence**:
- Line 237: Searches for AGENTS.md file (current and parent directories)
- Line 238: Validates file exists
- Line 240: Reads entire file content
- Line 243-245: Stores as template variables:
  - `{agents.md}`: Full file content
  - `{agents.file}`: Just the filename
  - `{agents.path}`: Full file path
- Line 247: Debug logging of AGENTS.md discovery

#### Invocation During Initialization
```csharp
Lines 56-58:
        // Setup the named values
        _namedValues = new TemplateVariables(Variables);
        AddAgentsFileContentToTemplateVariables();
```

**Evidence**: Called immediately after template variables initialization, before any prompt processing

**Context Expansion Behavior**: AGENTS.md content is automatically loaded and made available as a template variable, providing project-specific context without explicit command-line options.

---

## 6. MCP Server Context (Tool Context)

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--use-mcps` / `--mcp` Options
```csharp
Lines 443-452:
        else if (arg == "--use-mcps" || arg == "--mcp")
        {
            var mcpArgs = GetInputOptionArgs(i + 1, args).ToList();
            i += mcpArgs.Count();

            var useAllMcps = mcpArgs.Count == 0;
            if (useAllMcps) mcpArgs.Add("*");

            command.UseMcps.AddRange(mcpArgs);
        }
```

**Evidence**:
- Line 445: Retrieves MCP server names (0 or more arguments)
- Line 448: If no args, uses wildcard `*` (all configured MCPs)
- Line 450: Adds MCP names to `UseMcps` list
- **Default behavior**: Without args, activates all MCPs

#### `--no-mcps` Option
```csharp
Lines 453-456:
        else if (arg == "--no-mcps")
        {
            command.UseMcps.Clear();
        }
```

**Evidence**: Clears the MCP list (disables all MCPs)

#### `--with-mcp` Option (Inline MCP Definition)
```csharp
Lines 457-469:
        else if (arg == "--with-mcp")
        {
            var mcpCommandAndArgs = GetInputOptionArgs(i + 1, args);
            var mcpCommand = ValidateString(arg, mcpCommandAndArgs.FirstOrDefault(), "command to execute with MCP");
            var mcpName = $"mcp-{command.WithStdioMcps.Count + 1}";
            command.WithStdioMcps[mcpName] = new StdioMcpServerConfig
            {
                Command = mcpCommand!,
                Args = mcpCommandAndArgs.Skip(1).ToList(),
                Env = new Dictionary<string, string?>()
            };
            i += mcpCommandAndArgs.Count();
        }
```

**Evidence**:
- Line 459: Retrieves command and arguments
- Line 460: Validates command is present
- Line 461: Generates unique name (`mcp-1`, `mcp-2`, etc.)
- Lines 462-467: Creates inline MCP configuration
  - Command path
  - Arguments list
  - Environment variables (empty dict)
- **Dynamic MCPs**: Allows ad-hoc MCP server definition

### MCP Integration Implementation

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Adding MCP Functions
```csharp
Lines 115-116:
        // Add MCP functions if any are configured
        await AddMcpFunctions(factory);
```

**Evidence**: MCPs are loaded asynchronously and added to function factory

#### Function Factory with MCPs
```csharp
Lines 103-113:
        // Create the function factory and add functions.
        var factory = new McpFunctionFactory();
        factory.AddFunctions(new DateAndTimeHelperFunctions());
        factory.AddFunctions(new ShellCommandToolHelperFunctions());
        factory.AddFunctions(new BackgroundProcessHelperFunctions());
        factory.AddFunctions(new StrReplaceEditorHelperFunctions());
        factory.AddFunctions(new ThinkingToolHelperFunction());
        factory.AddFunctions(new CodeExplorationHelperFunctions());
        factory.AddFunctions(new ImageHelperFunctions(this));
        factory.AddFunctions(new ScreenshotHelperFunctions(this));
        factory.AddFunctions(new ShellAndProcessHelperFunctions());
        factory.AddFunctions(new GitHubSearchHelperFunctions());
```

**Evidence**:
- Line 103: Creates `McpFunctionFactory` for managing AI tools
- Lines 104-113: Adds built-in helper functions
- Line 116: Adds MCP-provided functions
- **Tool context**: MCPs expand AI capabilities with external tools

#### Chat Client with Function Factory
```csharp
Line 120:
        var chat = new FunctionCallingChat(chatClient, SystemPrompt, factory, options, MaxOutputTokens);
```

**Evidence**: Function factory (including MCP tools) is passed to chat client

**Context Expansion Behavior**: MCP servers provide tools that expand the AI's context by allowing it to query external data sources, execute commands, and interact with external systems.

---

## 7. Image Context (Multi-Modal Context)

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--image` Option
```csharp
Lines 658-664:
        else if (arg == "--image")
        {
            var imageArgs = GetInputOptionArgs(i + 1, args);
            var imagePatterns = ValidateStrings(arg, imageArgs, "image pattern");
            command.ImagePatterns.AddRange(imagePatterns);
            i += imageArgs.Count();
        }
```

**Evidence**:
- Line 660: Retrieves all arguments until next option
- Line 661: Validates each as a string pattern
- Line 662: Adds patterns to `ImagePatterns` list
- **Glob support**: Patterns can include wildcards (`*.png`, `**/*.jpg`, etc.)

### Image Processing Implementation

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Image Patterns Property
```csharp
Line 49 (from Clone method):
        clone.ImagePatterns = new List<string>(this.ImagePatterns);
```

**Evidence**: `ImagePatterns` is a list of strings (glob patterns)

#### Image Helper Functions
```csharp
Line 110:
        factory.AddFunctions(new ImageHelperFunctions(this));
```

**Evidence**: Image functions are added to tool factory (passing `this` command for access to `ImagePatterns`)

**Context Expansion Behavior**: Images are included in conversation messages, providing visual context alongside text. Glob patterns allow batch image inclusion (e.g., all screenshots from a test run).

---

## 8. Output Trajectory Context

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### `--output-trajectory` Option
```csharp
Lines 578-584:
        else if (arg == "--output-trajectory")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputTrajectory = max1Arg.FirstOrDefault() ?? DefaultOutputTrajectoryFileNameTemplate;
            command.OutputTrajectory = outputTrajectory;
            i += max1Arg.Count();
        }
```

**Evidence**:
- Line 580: Retrieves optional filename/template argument
- Line 581: Falls back to `DefaultOutputTrajectoryFileNameTemplate` (`trajectory-{time}.md`)
- Line 582: Sets output trajectory file path/template

### Trajectory File Initialization

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Trajectory File Setup
```csharp
Lines 93-95:
        // Initialize trajectory files
        _trajectoryFile = new TrajectoryFile(OutputTrajectory);
        _autoSaveTrajectoryFile = new TrajectoryFile(AutoSaveOutputTrajectory);
```

**Evidence**:
- Line 94: Creates trajectory file for explicit output
- Line 95: Creates auto-save trajectory file
- **Dual tracking**: Both explicit and auto-save trajectories

#### Setting Trajectory Metadata
```csharp
Line 124:
        SetTrajectoryMetadata(_currentChat.Conversation.Metadata);
```

**Evidence**: Trajectory files receive conversation metadata as context

**Context Expansion Behavior**: Trajectory files capture the full conversation flow including tool calls, providing a detailed record of context usage.

---

## Summary of Context Expansion Sources

| Context Source | Command-Line Option(s) | Property/Field | Implementation Location |
|----------------|------------------------|----------------|-------------------------|
| **Chat History** | `--chat-history`, `--input-chat-history`, `--continue`, `--output-chat-history` | `InputChatHistory`, `OutputChatHistory`, `LoadMostRecentChatHistory` | Lines 549-577 (parser), Lines 134-146 (executor) |
| **System Prompts** | `--system-prompt`, `--add-system-prompt` | `SystemPrompt`, `SystemPromptAdds` | Lines 470-486 (parser), Lines 98, 120 (executor) |
| **User Prompts** | `--add-user-prompt`, `--prompt` | `UserPromptAdds` | Lines 487-498 (parser), Lines 99, 128-132 (executor) |
| **Variables** | `--var`, `--vars` | `Variables` dictionary, `_namedValues` | Lines 405-423 (parser), Lines 56-58 (executor) |
| **AGENTS.md** | *(automatic)* | `_namedValues` template variables | Lines 58, 228-250 (executor) |
| **MCP Servers** | `--use-mcps`, `--mcp`, `--no-mcps`, `--with-mcp` | `UseMcps`, `WithStdioMcps` | Lines 443-469 (parser), Lines 103-116 (executor) |
| **Images** | `--image` | `ImagePatterns` | Lines 658-664 (parser), Line 110 (executor) |
| **Trajectory** | `--output-trajectory` | `OutputTrajectory`, `_trajectoryFile` | Lines 578-584 (parser), Lines 93-95, 124 (executor) |

---

## Context Accumulation Flow

### Execution Order in `ExecuteAsync()`

```csharp
// File: src/cycod/CommandLineCommands/ChatCommand.cs
// Method: ExecuteAsync()

Lines 56-146 (condensed):
    1. Initialize template variables (Line 57)
    2. Load AGENTS.md into variables (Line 58)
    3. Ground/expand filenames with variables (Lines 80-84)
    4. Ground system prompt (Line 98)
    5. Ground user prompt additions (Line 99)
    6. Ground input instructions (Line 100)
    7. Create function factory (Line 103)
    8. Add built-in helper functions (Lines 104-113)
    9. Add MCP functions (Line 116)
    10. Create chat client with system prompt (Line 120)
    11. Add user prompt messages to conversation (Lines 128-132)
    12. Load chat history into conversation (Lines 134-146)
```

**Evidence**: Context is layered in a specific order, with each layer building on previous ones:
1. Variables foundation
2. Project context (AGENTS.md)
3. Prompts (system → user)
4. Tools (built-in + MCP)
5. History (previous conversations)

This layering ensures that each context source can reference variables and previous context sources.

---

## Conclusion

All context expansion mechanisms are fully implemented and evidenced in the source code. The chat command provides rich context expansion through:
- Historical context (chat history)
- Instructional context (system/user prompts)
- Variable context (template variables)
- Project context (AGENTS.md)
- Tool context (MCP servers)
- Visual context (images)
- Execution context (trajectory files)

Unlike file search tools, cycod's context expansion is about **enriching the AI's understanding** rather than **displaying surrounding lines**.
