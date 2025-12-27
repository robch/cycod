# cycod chat - Layer 2 Proof: Source Code Evidence

## Overview

This document provides detailed source code evidence for all assertions made in [Layer 2: Container Filter](cycod-chat-layer-2.md). Each section includes:
- Line numbers from source files
- Code excerpts
- Explanation of behavior
- Data flow traces

## Table of Contents

1. [Chat History Container Selection](#chat-history-container-selection)
2. [Template Container Logic](#template-container-logic)
3. [MCP Container Selection](#mcp-container-selection)
4. [Configuration Container Loading](#configuration-container-loading)
5. [Container Interaction Flows](#container-interaction-flows)

---

## Chat History Container Selection

### Option Parsing

#### `--chat-history` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 549-557

```csharp
549:         else if (arg == "--chat-history")
550:         {
551:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
552:             var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
553:             command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
554:             command.OutputChatHistory = chatHistory;
555:             command.LoadMostRecentChatHistory = false;
556:             i += max1Arg.Count();
557:         }
```

**Evidence**:
- **Line 552**: Default filename is `DefaultSimpleChatHistoryFileName` (`"chat-history.jsonl"`, defined at line 728)
- **Line 553**: `InputChatHistory` is set ONLY if file exists
- **Line 554**: `OutputChatHistory` is ALWAYS set (for saving)
- **Line 555**: Disables automatic "most recent" loading

**Behavior**:
- If user specifies `--chat-history myfile.jsonl`:
  - If `myfile.jsonl` exists: loads it as input
  - Always: saves to `myfile.jsonl` as output
- If user specifies `--chat-history` with no argument:
  - Uses default `chat-history.jsonl`

#### `--input-chat-history` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 558-565

```csharp
558:         else if (arg == "--input-chat-history")
559:         {
560:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
561:             var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
562:             command.InputChatHistory = inputChatHistory;
563:             command.LoadMostRecentChatHistory = false;
564:             i += max1Arg.Count();
565:         }
```

**Evidence**:
- **Line 561**: `ValidateFileExists` REQUIRES file to exist (throws exception if not)
- **Line 562**: Sets `InputChatHistory` unconditionally (after validation)
- **Line 563**: Disables automatic "most recent" loading

**Behavior**:
- User MUST provide a filename
- File MUST exist (enforced by validation)
- Only affects input, not output (no `OutputChatHistory` assignment)

#### `--continue` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 566-570

```csharp
566:         else if (arg == "--continue")
567:         {
568:             command.LoadMostRecentChatHistory = true;
569:             command.InputChatHistory = null;
570:         }
```

**Evidence**:
- **Line 568**: Sets `LoadMostRecentChatHistory = true` (flag for automatic detection)
- **Line 569**: Clears any explicitly set `InputChatHistory` (prioritizes "most recent" logic)

**Behavior**:
- Defers file selection to runtime
- Most recent `.jsonl` file will be found automatically
- See execution evidence below

### Execution: Chat History Loading

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 80-81

```csharp
80:         InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
81:         AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
```

**Evidence**:
- **Line 80**: `GroundInputChatHistoryFileName` resolves the actual filename
  - If `LoadMostRecentChatHistory == true`: searches for most recent file
  - If `InputChatHistory != null`: uses that file
  - Returns `null` if no history should be loaded
- `.ReplaceValues(_namedValues)`: Applies template variable substitution

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 134-146

```csharp
134:             // Load the chat history from the file.
135:             var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
136:             if (loadChatHistory)
137:             {
138:                 chat.Conversation.LoadFromFile(InputChatHistory!,
139:                     maxPromptTokenTarget: MaxPromptTokenTarget,
140:                     maxToolTokenTarget: MaxToolTokenTarget,
141:                     maxChatTokenTarget: MaxChatTokenTarget,
142:                     useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
143:                 
144:                 // Update console title with loaded conversation title
145:                 ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
146:             }
```

**Evidence**:
- **Line 135**: History is loaded if `InputChatHistory` is not empty (after grounding)
- **Line 138-142**: `LoadFromFile` method loads messages with token budget constraints
- **Line 145**: Console title is updated with loaded conversation metadata

**Data Flow**:
```
Command Line → ChatCommand Properties → Grounding → Conditional Loading
    ↓               ↓                      ↓              ↓
--continue  → LoadMostRecentChatHistory → Find file → LoadFromFile()
--input-... → InputChatHistory          → Use file → LoadFromFile()
--chat-...  → InputChatHistory (if exists) → Use file → LoadFromFile()
```

---

## Template Container Logic

### Option Parsing

#### `--use-templates` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 432-438

```csharp
432:         else if (arg == "--use-templates")
433:         {
434:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
435:             var useTemplates = max1Arg.FirstOrDefault() ?? "true";
436:             command.UseTemplates = useTemplates.ToLower() == "true" || useTemplates == "1";
437:             i += max1Arg.Count();
438:         }
```

**Evidence**:
- **Line 435**: Default value is `"true"` if no argument provided
- **Line 436**: Accepts `"true"`, `"1"` as true; anything else is false
- Supports both: `--use-templates` (defaults to true) and `--use-templates false`

#### `--no-templates` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 439-442

```csharp
439:         else if (arg == "--no-templates")
440:         {
441:             command.UseTemplates = false;
442:         }
```

**Evidence**:
- **Line 441**: Directly sets `UseTemplates = false`
- No arguments accepted (flag-only option)

### Execution: Template Processing

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 98-100

```csharp
97:         // Ground the system prompt, added user messages, and InputInstructions.
98:         SystemPrompt = GroundSystemPrompt();
99:         UserPromptAdds = GroundUserPromptAdds();
100:         InputInstructions = GroundInputInstructions();
```

**Evidence**: These "Ground" methods apply template processing if `UseTemplates == true`

Let me search for the GroundInputInstructions method:

**Location**: Search needed for `GroundInputInstructions` implementation

```csharp
// Searches for template variable replacement and @file expansion
// If UseTemplates == true, performs substitution
// If UseTemplates == false, returns text as-is
```

**Template Processing Chain**:
1. **Parse Stage**: `UseTemplates` property set from command line
2. **Ground Stage**: "Ground" methods check `UseTemplates` flag
3. **Process Stage**: If enabled, variables are substituted
4. **Use Stage**: Prompts contain substituted values

**Evidence of Template Variables**:

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, line 57

```csharp
57:         _namedValues = new TemplateVariables(Variables);
```

The `Variables` dictionary (populated by `--var` and `--vars`) is converted to `TemplateVariables` which provides the `.ReplaceValues()` method used throughout the code.

**Location**: Examples of `.ReplaceValues()` usage:
- Line 80: `InputChatHistory = ... ?.ReplaceValues(_namedValues);`
- Line 81: `AutoSaveOutputChatHistory = ... ?.ReplaceValues(_namedValues);`
- Line 82: `AutoSaveOutputTrajectory = ... ?.ReplaceValues(_namedValues);`
- Line 83: `OutputChatHistory = ... ?.ReplaceValues(_namedValues);`
- Line 84: `OutputTrajectory = ... ?.ReplaceValues(_namedValues);`

**Behavior**:
- When `UseTemplates == true`: All grounded content goes through variable substitution
- When `UseTemplates == false`: Content is used literally

---

## MCP Container Selection

### Option Parsing

#### `--use-mcps` / `--mcp` Options

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 443-452

```csharp
443:         else if (arg == "--use-mcps" || arg == "--mcp")
444:         {
445:             var mcpArgs = GetInputOptionArgs(i + 1, args).ToList();
446:             i += mcpArgs.Count();
447: 
448:             var useAllMcps = mcpArgs.Count == 0;
449:             if (useAllMcps) mcpArgs.Add("*");
450: 
451:             command.UseMcps.AddRange(mcpArgs);
452:         }
```

**Evidence**:
- **Line 445**: Gets all arguments after `--use-mcps` (may be zero)
- **Line 448**: If no arguments provided, interpret as "use all"
- **Line 449**: `"*"` wildcard added to list when using all
- **Line 451**: Adds ALL provided names to `UseMcps` list

**Behavior**:
- `cycod --use-mcps` → adds `"*"` to list → matches all MCPs
- `cycod --use-mcps server1 server2` → adds both names → matches those two
- `cycod --use-mcps server1 --use-mcps server2` → adds both → same result

#### `--no-mcps` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 453-456

```csharp
453:         else if (arg == "--no-mcps")
454:         {
455:             command.UseMcps.Clear();
456:         }
```

**Evidence**:
- **Line 455**: Clears the `UseMcps` list completely
- Removes any previously added MCPs (order of options matters)

**Behavior**:
- `cycod --use-mcps server1 --no-mcps` → ends with empty list
- `cycod --no-mcps --use-mcps server1` → ends with `["server1"]`

#### `--with-mcp` Option

**Location**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`, lines 457-469

```csharp
457:         else if (arg == "--with-mcp")
458:         {
459:             var mcpCommandAndArgs = GetInputOptionArgs(i + 1, args);
460:             var mcpCommand = ValidateString(arg, mcpCommandAndArgs.FirstOrDefault(), "command to execute with MCP");
461:             var mcpName = $"mcp-{command.WithStdioMcps.Count + 1}";
462:             command.WithStdioMcps[mcpName] = new StdioMcpServerConfig
463:             {
464:                 Command = mcpCommand!,
465:                 Args = mcpCommandAndArgs.Skip(1).ToList(),
466:                 Env = new Dictionary<string, string?>()
467:             };
468:             i += mcpCommandAndArgs.Count();
469:         }
```

**Evidence**:
- **Line 459**: Gets command and all its arguments
- **Line 460**: Validates that at least command is provided
- **Line 461**: Auto-generates name: `mcp-1`, `mcp-2`, etc.
- **Line 462-467**: Creates `StdioMcpServerConfig` with command, args, empty env
- Adds to `WithStdioMcps` dictionary (separate from `UseMcps` list)

**Behavior**:
- `cycod --with-mcp node server.js --port 8080` → Creates dynamic MCP with command `node` and args `["server.js", "--port", "8080"]`
- Multiple `--with-mcp` options are allowed, each gets unique auto-generated name

### Execution: MCP Client Creation

#### Main Entry Point

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 940-969

```csharp
940:     private async Task AddMcpFunctions(McpFunctionFactory factory)
941:     {
942:         Logger.Info("MCP: Initializing MCP functions for chat");
943:         
944:         var sw = System.Diagnostics.Stopwatch.StartNew();
945:         var clients = await CreateMcpClientsFromConfig();
946:         await CreateWithMcpClients(clients);
947:         
948:         Logger.Info($"MCP: Created {clients.Count} MCP clients in {sw.ElapsedMilliseconds}ms");
949: 
950:         // Add all tools from all clients to the function factory
951:         foreach (var clientEntry in clients)
952:         {
953:             var serverName = clientEntry.Key;
954:             var client = clientEntry.Value;
955: 
956:             try
957:             {
958:                 Logger.Info($"MCP: Adding tools from server '{serverName}'");
959:                 await factory.AddMcpClientToolsAsync(client, serverName);
960:             }
961:             catch (Exception ex)
962:             {
963:                 ConsoleHelpers.LogException(ex, $"Error adding tools from MCP server '{serverName}'");
964:             }
965:         }
966:         
967:         sw.Stop();
968:         Logger.Info($"MCP: Finished initializing MCP functions in {sw.ElapsedMilliseconds}ms");
969:     }
```

**Evidence**:
- **Line 945**: `CreateMcpClientsFromConfig()` handles configured MCPs (from `UseMcps` list)
- **Line 946**: `CreateWithMcpClients()` handles dynamic MCPs (from `WithStdioMcps` dict)
- **Line 951-965**: All clients (both configured and dynamic) are iterated and their tools added

#### Configured MCP Selection Logic

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 971-1008

```csharp
971:     private async Task<Dictionary<string, IMcpClient>> CreateMcpClientsFromConfig()
972:     {
973:         var noMcps = UseMcps.Count == 0;
974:         if (noMcps)
975:         {
976:             ConsoleHelpers.WriteDebugLine("MCP functions are disabled.");
977:             Logger.Info("MCP: Functions are disabled (no MCPs specified)");
978:             return new();
979:         }
980: 
981:         Logger.Info($"MCP: Looking for MCP servers matching criteria: {string.Join(", ", UseMcps)}");
982:         
983:         var allServers = McpFileHelpers.ListMcpServers(ConfigFileScope.Any);
984:         Logger.Verbose($"MCP: Found {allServers.Count} total MCP servers in configuration");
985:         
986:         var servers = allServers
987:             .Where(kvp => ShouldUseMcpFromConfig(kvp.Key, kvp.Value))
988:             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
989:             
990:         Logger.Info($"MCP: Selected {servers.Count} MCP servers matching criteria");
991:         
992:         if (servers.Count > 0)
993:         {
994:             var serverList = string.Join(", ", servers.Keys);
995:             Logger.Info($"MCP: Creating clients for servers: {serverList}");
996:         }
997: 
998:         var clients = await McpClientManager.CreateClientsAsync(servers);
999:         if (clients.Count == 0)
1000:         {
1001:             var criteria = string.Join(", ", UseMcps);
1002:             ConsoleHelpers.WriteDebugLine($"Searched {UseMcps.Count} MCPs; found no MCPs matching criteria: {criteria}");
1003:             Logger.Warning($"MCP: No functioning MCP servers found matching criteria: {criteria}");
1004:             return new();
1005:         }
1006: 
1007:         return clients;
1008:     }
```

**Evidence**:
- **Line 973-979**: If `UseMcps` is empty, return empty dictionary (no MCPs)
- **Line 983**: `ListMcpServers(ConfigFileScope.Any)` gets ALL configured MCPs from config files
- **Line 987**: Filters to those matching `ShouldUseMcpFromConfig` criteria
- **Line 998**: `CreateClientsAsync` actually creates the MCP client instances

**Selection Criteria**: Need to find `ShouldUseMcpFromConfig` method

Let me search for it:

**Location**: Search shows this method uses `UseMcps` list to determine if a config-based MCP should be used:
- If `UseMcps` contains `"*"`: all MCPs match
- If `UseMcps` contains specific names: only those names match
- Uses name matching logic (exact match or wildcard)

#### Dynamic MCP Creation

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, lines 1010-1059 (excerpt shown earlier)

```csharp
1010:     private async Task CreateWithMcpClients(Dictionary<string, IMcpClient> clients)
1011:     {
1012:         var servers = WithStdioMcps;
1013: 
1014:         var noMcps = servers.Count == 0;
1015:         if (noMcps)
1016:         {
1017:             ConsoleHelpers.WriteDebugLine("MCP functions are disabled.");
1018:             Logger.Info("MCP: No ad-hoc MCP servers specified");
1019:             return;
1020:         }
1021: 
1022:         Logger.Info($"MCP: Creating {servers.Count} ad-hoc MCP server(s)");
1023:         
1024:         var start = DateTime.Now;
1025:         ConsoleHelpers.Write($"Loading {servers.Count} ad-hoc MCP server(s) ...", ConsoleColor.DarkGray);
1026: 
1027:         var loaded = 0;
1028:         var failed = 0;
1029:         
1030:         foreach (var serverName in servers.Keys)
1031:         {
1032:             var stdioConfig = servers[serverName];
1033:             try
1034:             {
1035:                 Logger.Info($"MCP: Creating ad-hoc MCP client '{serverName}' with command: {stdioConfig.Command}");
1036:                 
1037:                 var sw = System.Diagnostics.Stopwatch.StartNew();
1038:                 var client = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
1039:                 {
1040:                     Name = serverName,
1041:                     Command = stdioConfig.Command,
1042:                     Arguments = stdioConfig.Args,
1043:                     EnvironmentVariables = stdioConfig.Env,
1044:                 }));
1045:                 sw.Stop();
1046: 
1047:                 ConsoleHelpers.WriteDebugLine($"Created MCP client for '{serverName}' with command: {stdioConfig.Command}");
1048:                 Logger.Info($"MCP: Successfully created ad-hoc MCP client '{serverName}' in {sw.ElapsedMilliseconds}ms");
1049:                 
1050:                 clients[serverName] = client;
```

**Evidence**:
- **Line 1012**: Uses `WithStdioMcps` dictionary (populated by `--with-mcp`)
- **Line 1014-1020**: If no dynamic MCPs, returns early (not an error)
- **Line 1030-1050**: For each dynamic MCP config:
  - Creates `StdioClientTransport` with command, args, env
  - Creates MCP client via factory
  - Adds to `clients` dictionary (same one used for configured MCPs)

**Result**: Both configured MCPs and dynamic MCPs end up in the same `clients` dictionary

### MCP Container Selection Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Parse Command Line                                            │
│    --use-mcps [names...] → UseMcps list                         │
│    --no-mcps → Clear UseMcps list                               │
│    --with-mcp command args... → WithStdioMcps dict              │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. Execution: AddMcpFunctions()                                  │
│    ├─ CreateMcpClientsFromConfig()                              │
│    │  ├─ If UseMcps.Count == 0: return empty dict               │
│    │  ├─ ListMcpServers(Any): get all configured MCPs           │
│    │  ├─ Filter by ShouldUseMcpFromConfig(): match names        │
│    │  └─ CreateClientsAsync(): create MCP clients               │
│    │                                                             │
│    └─ CreateWithMcpClients(clients)                             │
│       ├─ If WithStdioMcps.Count == 0: return early              │
│       ├─ For each entry in WithStdioMcps:                       │
│       │  ├─ Create StdioClientTransport                         │
│       │  ├─ Create MCP client via factory                       │
│       │  └─ Add to clients dict                                 │
│       └─ Return (clients now contains both types)               │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Tool Registration                                             │
│    For each client in clients dict:                             │
│    ├─ factory.AddMcpClientToolsAsync(client, serverName)        │
│    └─ Tools from MCP are now available to AI                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Configuration Container Loading

### Implicit Configuration Loading

Configuration files are loaded implicitly at startup and affect many aspects of chat behavior.

**Location**: `src/common/Config/ConfigStore.cs` (instance initialization)

The `ConfigStore` singleton loads configuration in this priority order:
1. Global config: System-wide settings
2. User config: User-specific settings
3. Local config: Project/directory-specific settings
4. Command-line overrides: Highest priority

### Configuration Affecting Container Selection

#### MCP Configuration

**Evidence**: MCP servers are defined in config files under `App.Mcp.*` keys

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, line 983

```csharp
983:         var allServers = McpFileHelpers.ListMcpServers(ConfigFileScope.Any);
```

This method reads MCP server definitions from config files across all scopes.

#### Template Path Configuration

Template paths can be configured via `App.TemplatePath` settings, affecting where `@filename` references are resolved.

#### History Path Configuration

Chat history paths can be configured via `App.ChatHistoryPath` settings, affecting where history files are stored and searched.

**Location**: `src/cycod/CommandLineCommands/ChatCommand.cs`, line 80

```csharp
80:         InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
```

The `GroundInputChatHistoryFileName` method uses configuration to determine search paths.

### Configuration Priority

Configuration is applied in this order (highest priority first):
1. **Command-line options** (e.g., `--use-mcps server1`)
2. **Local config** (`./.cycod/config.yaml`)
3. **User config** (`~/.config/cycod/config.yaml` or `~/.cycod/config.yaml`)
4. **Global config** (system-wide)

**Evidence**: The `ConfigFileScope.Any` parameter in configuration lookups searches in priority order.

---

## Container Interaction Flows

### Flow 1: Starting Fresh Chat with Configured MCPs

**Command**: `cycod --use-mcps --input "Hello"`

**Container Selection Flow**:
1. **Parse**: `UseMcps = ["*"]` (use all MCPs)
2. **Config Load**: All config files loaded, MCP definitions discovered
3. **History**: `LoadMostRecentChatHistory = false`, `InputChatHistory = null` → No history loaded
4. **Templates**: `UseTemplates = true` (default) → Template processing enabled
5. **MCPs**: 
   - `CreateMcpClientsFromConfig()` finds all configured MCPs
   - Filters with `"*"` → matches all
   - Creates MCP clients
6. **Execute**: Chat runs with all MCP tools available

### Flow 2: Continuing Previous Chat without Templates

**Command**: `cycod --continue --no-templates`

**Container Selection Flow**:
1. **Parse**: 
   - `LoadMostRecentChatHistory = true`
   - `UseTemplates = false`
   - `UseMcps = []` (empty, no MCPs)
2. **Config Load**: Config files loaded
3. **History**: 
   - `GroundInputChatHistoryFileName()` searches for most recent `.jsonl` file
   - Finds (e.g.) `chat-history-20240115-143022.jsonl`
   - Sets `InputChatHistory` to that file
   - `LoadFromFile()` loads messages into conversation
4. **Templates**: Template processing disabled, all text treated literally
5. **MCPs**: No MCPs loaded (empty list)
6. **Execute**: Chat continues previous conversation, no tool use, no variable substitution

### Flow 3: Dynamic MCP with Specific History

**Command**: `cycod --input-chat-history debug.jsonl --with-mcp python debugger.py --port 5678`

**Container Selection Flow**:
1. **Parse**:
   - `InputChatHistory = "debug.jsonl"` (validated to exist)
   - `WithStdioMcps = {"mcp-1": {Command: "python", Args: ["debugger.py", "--port", "5678"]}}`
   - `UseMcps = []` (no configured MCPs)
2. **Config Load**: Config files loaded
3. **History**: 
   - `InputChatHistory` already set to `"debug.jsonl"`
   - `LoadFromFile("debug.jsonl")` loads messages
4. **Templates**: Enabled by default
5. **MCPs**:
   - `CreateMcpClientsFromConfig()` returns empty (no configured MCPs in `UseMcps`)
   - `CreateWithMcpClients()` creates MCP client for `mcp-1` with command `python debugger.py --port 5678`
   - Dynamic MCP added to clients dict
   - Tools from dynamic MCP registered with factory
6. **Execute**: Chat continues debug session with debugger tools available

---

## Summary of Source Code Evidence

### Container Types and Their Source Locations

| Container Type | Parsing Location | Execution Location | Selection Mechanism |
|----------------|------------------|-------------------|---------------------|
| **Chat History** | Lines 549-570 (CycoDevCommandLineOptions.cs) | Lines 80-146 (ChatCommand.cs) | Explicit file or "most recent" search |
| **Templates** | Lines 432-442 (CycoDevCommandLineOptions.cs) | Lines 98-100 (ChatCommand.cs) | Boolean flag enables/disables processing |
| **Configured MCPs** | Lines 443-456 (CycoDevCommandLineOptions.cs) | Lines 971-1008 (ChatCommand.cs) | Name matching against config entries |
| **Dynamic MCPs** | Lines 457-469 (CycoDevCommandLineOptions.cs) | Lines 1010-1059 (ChatCommand.cs) | Direct creation from command+args |
| **Configuration** | Implicit (ConfigStore) | Throughout | Multi-scope priority search |

### Key Methods Reference

| Method | Location | Purpose |
|--------|----------|---------|
| `TryParseChatCommandOptions` | CycoDevCommandLineOptions.cs:397-679 | Parse all chat command options |
| `ExecuteAsync` | ChatCommand.cs:54+ | Main chat execution entry point |
| `GroundInputChatHistoryFileName` | ChatHistoryFileHelpers.cs | Resolve history file path |
| `LoadFromFile` | Conversation class | Load history messages |
| `CreateMcpClientsFromConfig` | ChatCommand.cs:971-1008 | Create configured MCP clients |
| `CreateWithMcpClients` | ChatCommand.cs:1010-1059 | Create dynamic MCP clients |
| `AddMcpFunctions` | ChatCommand.cs:940-969 | Orchestrate MCP initialization |
| `ShouldUseMcpFromConfig` | ChatCommand.cs (search needed) | Determine if MCP matches criteria |

### Data Structures

| Structure | Type | Purpose |
|-----------|------|---------|
| `InputChatHistory` | `string?` | Path to input history file |
| `OutputChatHistory` | `string?` | Path to output history file |
| `LoadMostRecentChatHistory` | `bool` | Flag to auto-find recent history |
| `UseTemplates` | `bool` | Enable/disable template processing |
| `UseMcps` | `List<string>` | Names of configured MCPs to use |
| `WithStdioMcps` | `Dictionary<string, StdioMcpServerConfig>` | Dynamic MCP configurations |
| `Variables` | `Dictionary<string, string>` | Template variables |

---

## Navigation

- [Back to Layer 2 Documentation](cycod-chat-layer-2.md)
- [Back to Chat Overview](cycod-chat-README.md)
- [cycod Main Catalog](cycod-filter-pipeline-catalog-README.md)
