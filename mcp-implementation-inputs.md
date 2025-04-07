# Implementing MCPs inside `chatx` codebase

In previous commits, we've added the ability to "configure" mcp servers.
- `chatx mcp` commands - see help at `chatx help find mcp --expand`

We next need to actually "hook up" MCPs using the new dotnet `ModelContextProtocol` package.
- See these URLs for infomration on how that package works:
  - https://www.nuget.org/packages/ModelContextProtocol
  - https://github.com/modelcontextprotocol/csharp-sdk

Review the web pages above to understand how the `ModelContextProtocol` package works, and how
to start stdio and sse servers, obtaining IMcpClient via McpClientFactory.CreateAsync. Understand
how to list the tools as McpClientTool instances (which inherits from AIFunction, btw). Understand
how to, given an McpClientTool, call it via CallToolAsync.

Next, review the code below to see how our existing `FunctionFactory` works, and how it's called
from the `FunctionCallingChat` class, which is used by the `ChatCommand` class to actually call
the functions that are exposed via the `AIFunction` interface.

Prior to integrating the `ModelContextProtocol` package, when ChatCommand create a function factory,
it adds "built-in" tools ("helper functions", e.g. from `DateAndTimeHelperFunctions` and similar),
via the `FunctionFactory` `AddFunctions` method, which in turn finds all public instance functions,
using reflection to find the `Description` attributes on the methods/parameters. There are other
ways to add functions to the factory, as well, such as adding static methods. 

We should create a derived class from `FunctionFactory` that should have new methods for adding
all the McpClientTools exposed by an IMcpClient, or a specific IMcpClient + McpClientTool. That
first method (just IMcpClient) will enumerate all the tools via ListToolsAsync, and then call
the second method for each tool. The second method will add an anonymous function that calls
`CallToolAsync` to the function factory, using an `AddFunction` method on the `FunctionFactory`
base class.

Below you'll find relevant code from the `chatx` codebase related to these new MCP commands, as
well as the "chat" functionality that is available via the `ChatCommand` class, and how that
utilizes the FunctionFactory and AIFunction capabilities from Microsoft.Extensions.AI packages
to allow "functions" to be called by the LLM model.

## src\Program.cs

Modified: 36 minutes ago
Size: 8 KB

```csharp
124:                 McpListCommand mcpListCommand => mcpListCommand.Execute(commandLineOptions.Interactive),
125:                 McpGetCommand mcpGetCommand => mcpGetCommand.Execute(commandLineOptions.Interactive),
126:                 McpAddCommand mcpAddCommand => mcpAddCommand.Execute(commandLineOptions.Interactive),
127:                 McpRemoveCommand mcpRemoveCommand => mcpRemoveCommand.Execute(commandLineOptions.Interactive),
```

## src\CommandLine\CommandLineOptions.cs

Modified: 36 minutes ago
Size: 35 KB

```csharp
203:             var partialCommandNeedsHelp = commandName == "config" || commandName == "github" || commandName == "alias" || commandName == "prompt" || commandName == "mcp";
229:                 "mcp list" => new McpListCommand(),
230:                 "mcp get" => new McpGetCommand(),
231:                 "mcp add" => new McpAddCommand(),
232:                 "mcp remove" => new McpRemoveCommand(),
253:             TryParseMcpCommandOptions(command as McpBaseCommand, args, ref i, arg) ||
359:         else if (command is McpGetCommand mcpGetCommand && string.IsNullOrEmpty(mcpGetCommand.Name))
361:             mcpGetCommand.Name = arg;
364:         else if (command is McpAddCommand mcpAddCommand && string.IsNullOrEmpty(mcpAddCommand.Name))
366:             mcpAddCommand.Name = arg;
369:         else if (command is McpRemoveCommand mcpRemoveCommand && string.IsNullOrEmpty(mcpRemoveCommand.Name))
371:             mcpRemoveCommand.Name = arg;
604:     private static bool TryParseMcpCommandOptions(McpBaseCommand? command, string[] args, ref int i, string arg)
629:         else if (command is McpAddCommand cmdAddCommand && arg == "--command")
637:         else if (command is McpAddCommand urlAddCommand && arg == "--url")
645:         else if (command is McpAddCommand argsAddCommand && arg == "--arg")
652:         else if (command is McpAddCommand envAddCommand && (arg == "--env" || arg == "-e"))
```

## src\CommandLineCommands\McpCommands\McpAddCommand.cs

Modified: 36 minutes ago
Size: 4 KB

```csharp
8: /// Command to add a new MCP (Model Context Protocol) Server.
10: class McpAddCommand : McpBaseCommand
13:     /// The name of the MCP server to add.
18:     /// The command to execute for the MCP server (for stdio transport).
33:     /// The transport type for the MCP server (stdio or sse).
39:     /// Environment variables for the MCP server (in key=value format).
46:     public McpAddCommand() : base()
75:         return "mcp add";
88:             throw new InvalidOperationException("mcp add requires name and either command or url");
98:     /// <param name="name">The name of the MCP server to add.</param>
104:     /// <param name="scope">The scope to add the MCP server to.</param>
112:                 ? McpFileHelpers.SaveMcpServer(name, command, args, envVars, "stdio", scope: scope)
113:                 : McpFileHelpers.SaveMcpServer(name, null, null, null, transport!, url: url, scope: scope);
115:             Console.WriteLine($"Created MCP server '{name}' at {savedFilePath}.");
120:             ConsoleHelpers.WriteErrorLine($"Error creating MCP server: {ex.Message}");
```

## src\CommandLineCommands\McpCommands\McpBaseCommand.cs

Modified: 36 minutes ago
Size: 662 bytes

```csharp
4: /// Base class for all MCP (Model Context Protocol) related commands.
6: abstract class McpBaseCommand : Command
9:     /// The scope to use for the MCP operation.
16:     public McpBaseCommand()
23:     /// <returns>False, as MCP commands are never empty.</returns>
```

## src\CommandLineCommands\McpCommands\McpGetCommand.cs

Modified: 36 minutes ago
Size: 2 KB

```csharp
6: /// Command to view the details of a specific MCP (Model Context Protocol) Server.
8: class McpGetCommand : McpBaseCommand
11:     /// The name of the MCP server to get.
18:     public McpGetCommand() : base()
38:         return "mcp get";
50:             ConsoleHelpers.WriteErrorLine("Error: MCP server name is required.");
59:     /// Execute the get command for the specified MCP server name and scope.
61:     /// <param name="name">The name of the MCP server to get.</param>
70:             ? McpFileHelpers.GetFromAnyScope(name)
71:             : McpFileHelpers.GetFromScope(name, scope);
76:                 ? $"Error: MCP server '{name}' not found in any scope."
77:                 : $"Error: MCP server '{name}' not found in {scope} scope.");
84:             ConsoleHelpers.WriteErrorLine($"Error: MCP server '{name}' found but its config file is not available.");
89:         McpDisplayHelpers.DisplayMcpServer(name, configFile.FileName, configFile.Scope, serverConfig);
```

## src\CommandLineCommands\McpCommands\McpListCommand.cs

Modified: 36 minutes ago
Size: 2 KB

```csharp
8: /// Command to list all available MCP (Model Context Protocol) Servers.
10: class McpListCommand : McpBaseCommand
15:     public McpListCommand() : base()
26:         return "mcp list";
43:     /// <param name="scope">The scope to list MCP servers for.</param>
51:             DisplayMcpServers(ConfigFileScope.Global);
57:             DisplayMcpServers(ConfigFileScope.User);
63:             DisplayMcpServers(ConfigFileScope.Local);
69:     // Display MCP servers using the McpDisplayHelpers
70:     private void DisplayMcpServers(ConfigFileScope scope)
72:         McpDisplayHelpers.DisplayMcpServers(scope);
```

## src\CommandLineCommands\McpCommands\McpRemoveCommand.cs

Modified: 36 minutes ago
Size: 3 KB

```csharp
6: /// Command to remove an MCP (Model Context Protocol) Server.
8: class McpRemoveCommand : McpBaseCommand
11:     /// The name of the MCP server to remove.
18:     public McpRemoveCommand() : base()
38:         return "mcp remove";
50:             ConsoleHelpers.WriteErrorLine("Error: MCP server name is required.");
59:     /// Execute the remove command for the specified MCP server name and scope.
61:     /// <param name="name">The name of the MCP server to remove.</param>
70:             ? McpFileHelpers.GetFromAnyScope(name)
71:             : McpFileHelpers.GetFromScope(name, scope);
76:                 ? $"Error: MCP server '{name}' not found in any scope."
77:                 : $"Error: MCP server '{name}' not found in {scope} scope.");
84:             ConsoleHelpers.WriteErrorLine($"Error: MCP server '{name}' found but its config file is not available.");
89:         var deleted = McpFileHelpers.DeleteMcpServer(name, deleteFromScope);
92:             ConsoleHelpers.WriteErrorLine($"Error: Failed to delete MCP server '{name}'.");
96:         Console.WriteLine($"Removed MCP server '{name}' from {deleteFromScope} scope.");
```

## src\McpHelpers\IMcpServerConfigItem.cs

Modified: 36 minutes ago
Size: 451 bytes

```csharp
3: /// Interface for MCP server configuration items
5: public interface IMcpServerConfigItem
8:     /// The type of the MCP server (stdio or sse)
16:     McpConfigFile? ConfigFile { get; set; }
```

## src\McpHelpers\McpConfigFile.cs

Modified: 36 minutes ago
Size: 4 KB

```csharp
4: /// Class representing an MCP configuration file that contains multiple servers
6: public class McpConfigFile
8:     public const string DefaultFolderName = "mcp";
9:     public const string DefaultFileName = "mcp.json";
12:     /// Loads an MCP configuration file from a specific scope
16:     public static McpConfigFile? FromScope(ConfigFileScope scope)
18:         var mcpDirectory = McpFileHelpers.FindMcpDirectoryInScope(scope);
19:         if (mcpDirectory == null)
24:         var mcpFilePath = Path.Combine(mcpDirectory, McpConfigFile.DefaultFileName);
25:         if (!File.Exists(mcpFilePath))
32:             var jsonString = File.ReadAllText(mcpFilePath);
36:                 Converters = { new McpServerConfigItemConverter() }
39:             var mcpConfig = JsonSerializer.Deserialize<McpServerConfig>(jsonString, options);
40:             if (mcpConfig == null)
45:             var configFile = new McpConfigFile
48:                 FileName = mcpFilePath,
49:                 Servers = mcpConfig.McpServers
62:             ConsoleHelpers.WriteDebugLine($"Error loading MCP configuration file: {ex.Message}");
75:             ConsoleHelpers.WriteDebugLine("Cannot save MCP configuration file: filename is not set");
81:             // Create a McpServerConfig to maintain format compatibility
82:             var mcpConfig = new McpServerConfig
84:                 McpServers = Servers
95:             jsonOptions.Converters.Add(new McpServerConfigItemConverter());
97:             string jsonString = JsonSerializer.Serialize(mcpConfig, jsonOptions);
103:             ConsoleHelpers.WriteDebugLine($"Error saving MCP configuration file: {ex.Message}");
109:     /// Creates a new MCP configuration file in the specified scope
113:     public static McpConfigFile Create(ConfigFileScope scope)
115:         var mcpDirectory = McpFileHelpers.FindMcpDirectoryInScope(scope, create: true)!;
116:         var mcpFilePath = Path.Combine(mcpDirectory, McpConfigFile.DefaultFileName);
118:         return new McpConfigFile
121:             FileName = mcpFilePath
140:     [JsonPropertyName("McpServers")]
141:     public Dictionary<string, IMcpServerConfigItem> Servers { get; set; } = new Dictionary<string, IMcpServerConfigItem>();
```

## src\McpHelpers\McpDisplayHelpers.cs

Modified: 36 minutes ago
Size: 5 KB

```csharp
7: /// Provides methods for displaying MCP (Model Context Protocol) server information.
9: public static class McpDisplayHelpers
12:     /// Gets a display name for the location of the MCP servers in a scope.
18:         var mcpDir = McpFileHelpers.FindMcpDirectoryInScope(scope);
19:         if (mcpDir != null)
21:             string mcpFilePath = Path.Combine(mcpDir, McpConfigFile.DefaultFileName);
22:             return CommonDisplayHelpers.FormatLocationHeader(mcpFilePath, scope);
28:         return CommonDisplayHelpers.FormatLocationHeader(Path.Combine(scopeDir, McpConfigFile.DefaultFolderName, McpConfigFile.DefaultFileName), scope);
32:     /// Displays all MCP servers in a specific scope.
35:     public static void DisplayMcpServers(ConfigFileScope scope)
43:         // Find the MCP directory in the scope, and get the servers
44:         var mcpDir = McpFileHelpers.FindMcpDirectoryInScope(scope);
45:         var servers = mcpDir != null
46:             ? McpFileHelpers.ListMcpServers(scope)
51:             Console.WriteLine($"  No MCP servers found in this scope.");
81:     /// Displays detailed information about a specific MCP server.
87:     public static void DisplayMcpServer(string serverName, string configPath, ConfigFileScope scope, IMcpServerConfigItem serverConfig)
```

## src\McpHelpers\McpFileHelpers.cs

Modified: 36 minutes ago
Size: 6 KB

```csharp
7: /// Provides methods for working with MCP (Model Context Protocol) server configuration files across different scopes.
9: public static class McpFileHelpers
12:     /// Finds the MCP directory in the specified scope.
16:     /// <returns>The path to the MCP directory, or null if not found</returns>
17:     public static string? FindMcpDirectoryInScope(ConfigFileScope scope, bool create = false)
20:             ? ScopeFileHelpers.EnsureDirectoryInScope(McpConfigFile.DefaultFolderName, scope)
21:             : ScopeFileHelpers.FindDirectoryInScope(McpConfigFile.DefaultFolderName, scope);
25:     /// Saves an MCP server configuration to a file in the specified scope.
27:     /// <param name="serverName">Name of the MCP server</param>
35:     public static string SaveMcpServer(
45:         var configFile = McpConfigFile.FromScope(scope) ?? McpConfigFile.Create(scope);
90:     /// <param name="serverName">The name of the MCP server to find</param>
92:     public static IMcpServerConfigItem? GetFromAnyScope(string serverName)
110:     /// <param name="serverName">The name of the MCP server to find</param>
113:     public static IMcpServerConfigItem? GetFromScope(string serverName, ConfigFileScope scope)
115:         var configFile = McpConfigFile.FromScope(scope);
125:     /// Deletes an MCP server configuration.
127:     /// <param name="serverName">The name of the MCP server to delete</param>
130:     public static bool DeleteMcpServer(string serverName, ConfigFileScope scope = ConfigFileScope.Any)
133:         IMcpServerConfigItem? serverConfig = null;
134:         McpConfigFile? configFile = null;
141:                 configFile = McpConfigFile.FromScope(searchScope);
150:             configFile = McpConfigFile.FromScope(scope);
159:             ConsoleHelpers.WriteDebugLine($"DeleteMcpServer: Server not found: {serverName}");
175:     /// Lists all MCP servers in a specified scope.
179:     public static Dictionary<string, IMcpServerConfigItem> ListMcpServers(ConfigFileScope scope)
181:         var configFile = McpConfigFile.FromScope(scope);
182:         return configFile?.Servers ?? new Dictionary<string, IMcpServerConfigItem>();
```

## src\McpHelpers\McpServerConfig.cs

Modified: 36 minutes ago
Size: 396 bytes

```csharp
3: /// Root configuration class for MCP server JSON files (maintained for serialization compatibility)
5: public class McpServerConfig
10:     public Dictionary<string, IMcpServerConfigItem> McpServers { get; set; } = new Dictionary<string, IMcpServerConfigItem>();
```

## src\McpHelpers\McpServerConfigItemConverter.cs

Modified: 36 minutes ago
Size: 4 KB

```csharp
4: /// JSON converter for IMcpServerConfigItem
6: public class McpServerConfigItemConverter : JsonConverter<IMcpServerConfigItem>
9:     /// Reads and converts JSON to the appropriate MCP server config class
11:     public override IMcpServerConfigItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
65:     /// Writes the MCP server config to JSON
67:     public override void Write(Utf8JsonWriter writer, IMcpServerConfigItem value, JsonSerializerOptions options)
```

## src\McpHelpers\SseServerConfig.cs

Modified: 36 minutes ago
Size: 609 bytes

```csharp
3: /// Configuration for SSE-based MCP servers
5: public class SseServerConfig : IMcpServerConfigItem
8:     /// The type of the MCP server (always "sse")
21:     public McpConfigFile? ConfigFile { get; set; }
```

## src\McpHelpers\StdioServerConfig.cs

Modified: 36 minutes ago
Size: 972 bytes

```csharp
3: /// Configuration for stdio-based MCP servers
5: public class StdioServerConfig : IMcpServerConfigItem
8:     /// The type of the MCP server (always "stdio")
31:     public McpConfigFile? ConfigFile { get; set; }
```

## src\assets\help\mcp add.txt

Modified: 36 minutes ago
Size: 1 KB

```plaintext
1: CHATX MCP ADD
3:   Create a new Model Context Protocol (MCP) server.
5: USAGE: chatx mcp add SERVER_NAME --command COMMAND [--arg ARG...] [--env ENV...] [--url URL] [--scope]
9:   SERVER_NAME       The name for the new MCP server
11:   --command         The command to execute for the MCP server (for STDIO transport)
24:   EXAMPLE 1: Create a basic stdio MCP server
26:     chatx mcp add postgres-server --command /path/to/postgres-mcp-server --arg --connection-string --arg "postgresql://user:pass@localhost:5432/mydb"
28:   EXAMPLE 2: Create an MCP server with environment variables
30:     chatx mcp add weather-api --command /path/to/weather-cli --env API_KEY=abc123 --env CACHE_DIR=/tmp
32:   EXAMPLE 3: Create an SSE MCP server
34:     chatx mcp add sse-backend --url https://example.com/sse-endpoint
36:   EXAMPLE 4: Create a user-level MCP server
38:     chatx mcp add shared-tool --command /usr/local/bin/tool-server --arg --config --arg /path/to/config.json --user
42:   MCP servers provide Claude and other LLMs with access to external tools and data sources.
47:   chatx help mcp
48:   chatx help mcp get
```

## src\assets\help\mcp get.txt

Modified: 36 minutes ago
Size: 907 bytes

```plaintext
1: CHATX MCP GET
3:   Display the details of a specific Model Context Protocol (MCP) server.
5: USAGE: chatx mcp get SERVER_NAME [--scope]
9:   SERVER_NAME       The name of the MCP server to display
20:   EXAMPLE 1: Display an MCP server from any scope
22:     chatx mcp get postgres-server
24:   EXAMPLE 2: Display an MCP server only if it exists in the user scope
26:     chatx mcp get weather-api --user
30:   When an MCP server is found, the command displays:
38:   chatx help mcp
39:   chatx help mcp list
```

## src\assets\help\mcp list.txt

Modified: 36 minutes ago
Size: 822 bytes

```plaintext
1: CHATX MCP LIST
3:   List all available Model Context Protocol (MCP) servers.
5: USAGE: chatx mcp list [--scope]
11:     --global, -g    List only global MCP servers (all users)
12:     --user, -u      List only user MCP servers (current user)
13:     --local, -l     List only local MCP servers (current directory)
14:     --any, -a       List MCP servers from all scopes (default)
18:   EXAMPLE 1: List all available MCP servers
20:     chatx mcp list
22:   EXAMPLE 2: List only MCP servers in the user scope
24:     chatx mcp list --user
28:   This command displays MCP servers from the specified scope(s).
36:   chatx help mcp
37:   chatx help mcp get
```

## src\assets\help\mcp remove.txt

Modified: 36 minutes ago
Size: 967 bytes

```plaintext
1: CHATX MCP REMOVE
3:   Delete a Model Context Protocol (MCP) server configuration.
5: USAGE: chatx mcp remove SERVER_NAME [--scope]
9:   SERVER_NAME       The name of the MCP server to remove
20:   EXAMPLE 1: Remove an MCP server from any scope
22:     chatx mcp remove unused-server
24:   EXAMPLE 2: Remove an MCP server only from the user scope
26:     chatx mcp remove postgres-server --user
31:   If using the --any option, the command will remove the first matching MCP server
36:   chatx help mcp
37:   chatx help mcp list
```

## src\assets\help\mcp.txt

Modified: 36 minutes ago
Size: 1 KB

```plaintext
1: CHATX MCP COMMANDS
3:   These commands allow you to manage Model Context Protocol (MCP) servers.
4:   MCP servers provide capabilities like database access, API integrations, or tool execution.
6: USAGE: chatx mcp list [--scope]
7:    OR: chatx mcp get SERVER_NAME [--scope]
8:    OR: chatx mcp add SERVER_NAME --command COMMAND [--arg ARG...] [--env ENV...] [--url URL] [--scope]
9:    OR: chatx mcp remove SERVER_NAME [--scope]
22:     list            List all available MCP servers
23:     get             Display the details of a specific MCP server
24:     add             Create a new MCP server configuration
25:     remove          Delete an MCP server configuration
29:   chatx help mcp list
30:   chatx help mcp get
31:   chatx help mcp add
32:   chatx help mcp remove
```

## src\ChatClient\FunctionCallingChat.cs

Modified: 37 minutes ago
Size: 6 KB

```csharp
        var useMicrosoftExtensionsAIFunctionCalling = false; // Can't use this for now; (1) doesn't work with copilot w/ all models, (2) functionCallCallback not available
        _chatClient = useMicrosoftExtensionsAIFunctionCalling
```

## src\FunctionCalling\FunctionFactory.cs

Modified: 37 minutes ago
Size: 13 KB

```csharp
            var aiFunction = AIFunctionFactory.Create(method, instance, method.Name, funcDescription);
    private static string? CallFunction(MethodInfo methodInfo, AIFunction function, string argumentsAsJson, object? instance)
    private readonly Dictionary<MethodInfo, (AIFunction Function, object? Instance)> _functions = new();
```

## src\Program.cs

Modified: 41 minutes ago
Size: 8 KB

```csharp
                ChatCommand chatCommand => chatCommand.ExecuteAsync(commandLineOptions.Interactive),
```

## src\CommandLine\CommandLineOptions.cs

Modified: 41 minutes ago
Size: 35 KB

```csharp
            command = new ChatCommand();
        var oneChatCommandWithNoInput = commandLineOptions.Commands.Count == 1 && command is ChatCommand chatCommand && chatCommand.InputInstructions.Count == 0;
        var implictlyUseStdIn = oneChatCommandWithNoInput && Console.IsInputRedirected;
            (command as ChatCommand)!.InputInstructions.Add(joined);
                _ => new ChatCommand()
            var needToRestartLoop = command is not ChatCommand;
            TryParseChatCommandOptions(commandLineOptions, command as ChatCommand, args, ref i, arg) ||
            commandLineOptions.HelpTopic = command is ChatCommand ? "usage" : command!.GetCommandName();
    private static bool TryParseChatCommandOptions(CommandLineOptions commandLineOptions, ChatCommand? command, string[] args, ref int i, string arg)
```

## src\CommandLine\ForEachVarHelpers.cs

Modified: 4 days ago
Size: 4 KB

```csharp
        return commands.SelectMany(command => command is ChatCommand chatCommand
            ? ExpandChatCommand(chatCommand)
    private static IEnumerable<Command> ExpandChatCommand(ChatCommand chatCommand)
            return new List<ChatCommand> { chatCommand };
        var expandedCommands = new List<ChatCommand>();
```

## src\CommandLine\ForEachVariable.cs

Modified: 4 days ago
Size: 952 bytes

```csharp
/// to create multiple ChatCommand instances during expansion.
```

## src\CommandLineCommands\ChatCommand.cs

Modified: 41 minutes ago
Size: 19 KB

```csharp
public class ChatCommand : Command
    public ChatCommand()
    public ChatCommand Clone()
        var clone = new ChatCommand();
            var handled = await TryHandleChatCommandAsync(chat, userPrompt);
    private async Task<bool> TryHandleChatCommandAsync(FunctionCallingChat chat, string userPrompt)
```

## src\ChatClient\FunctionCallingChat.cs

Modified: 43 minutes ago
Size: 6 KB

```csharp
    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
        _functionFactory = factory;
            Tools = _functionFactory.GetAITools().ToList(),
            var functionResult = _functionFactory.TryCallFunction(functionCall.Name, functionCall.Arguments, out var functionResponse)
    private readonly FunctionFactory _functionFactory;
```

## src\CommandLineCommands\ChatCommand.cs

Modified: 43 minutes ago
Size: 19 KB

```csharp
        var factory = new FunctionFactory();
```

## src\FunctionCalling\FunctionFactory.cs

Modified: 43 minutes ago
Size: 13 KB

```csharp
public class FunctionFactory
    public FunctionFactory()
    public FunctionFactory(Assembly assembly)
    public FunctionFactory(Type type1, params Type[] types)
    public FunctionFactory(IEnumerable<Type> types)
    public FunctionFactory(Type type)
            var aiFunction = AIFunctionFactory.Create(method, instance, method.Name, funcDescription);
    public static FunctionFactory operator +(FunctionFactory a, FunctionFactory b)
        var newFactory = new FunctionFactory();
```

