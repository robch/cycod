# API Reference

This document provides a reference for the various APIs and interfaces used in ChatX.

## Chat Client API

The Chat Client API is responsible for handling communication with AI service providers.

### ChatClientFactory

```csharp
// Creates chat clients based on configuration
static class ChatClientFactory {
    // Creates a chat client instance based on available environment variables
    public static ChatClient CreateChatClientFromEnv();
    
    // Creates an OpenAI chat client
    public static ChatClient CreateOpenAIChatClient();
    
    // Creates an Azure OpenAI chat client
    public static ChatClient CreateAzureOpenAIChatClient();
    
    // Creates a GitHub Copilot chat client with token authentication
    public static ChatClient CreateCopilotChatClientWithToken();
    
    // Creates a GitHub Copilot chat client with HMAC authentication
    public static ChatClient CreateCopilotChatClient();
}
```

### FunctionCallingChat

```csharp
// Main chat client class that handles communication with AI models
class FunctionCallingChat {
    // Constructor
    public FunctionCallingChat(ChatClient openAIClient, string systemPrompt, FunctionFactory factory);
    
    // Clears the chat history
    public void ClearChatHistory();
    
    // Loads chat history from a file
    public void LoadChatHistory(string fileName);
    
    // Gets a streaming chat completion and handles function calls
    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<StreamingChatCompletionUpdate>? streamingCallback = null,
        Action<string, string, string?>? functionCallCallback = null);
}
```

## Function Calling API

The Function Calling API provides mechanisms to define and execute tools that the AI can use.

### FunctionFactory

```csharp
// Creates function definitions from helper classes
public class FunctionFactory {
    // Constructors
    public FunctionFactory();
    public FunctionFactory(Assembly assembly);
    public FunctionFactory(Type type);
    public FunctionFactory(Type type1, params Type[] types);
    public FunctionFactory(IEnumerable<Type> types);
    
    // Add functions from various sources
    public void AddFunctions(Assembly assembly);
    public void AddFunctions(Type type);
    public void AddFunctions(Type type1, params Type[] types);
    public void AddFunctions(IEnumerable<Type> types);
    public void AddFunctions(object instance);
    
    // Add a specific function
    public void AddFunction(MethodInfo method, object? instance = null);
    
    // Get the defined tools
    public IEnumerable<ChatTool> GetChatTools();
    public IEnumerable<ToolDefinition> GetToolDefinitions();
    
    // Call a function by name
    public bool TryCallFunction(string functionName, string functionArguments, out string? result);
    
    // Combine function factories
    public static FunctionFactory operator +(FunctionFactory a, FunctionFactory b);
}
```

### FunctionCallContext

```csharp
// Context for executing function calls
public class FunctionCallContext {
    // Constructor
    public FunctionCallContext(FunctionFactory functionFactory, IList<ChatMessage> messages);
    
    // Check for updates in a streaming response
    public bool CheckForUpdate(StreamingChatCompletionUpdate streamingUpdate);
    
    // Process tool calls and execute functions
    public bool TryCallFunctions(string content, Action<string, string, string?>? funcionCallback, Action<IList<ChatMessage>>? messageCallback);
    
    // Clear the context
    public void Clear();
}
```

### Helper Function Attributes

```csharp
// Describes a helper function class
[AttributeUsage(AttributeTargets.Class)]
public class HelperFunctionDescriptionAttribute : Attribute {
    public HelperFunctionDescriptionAttribute();
    public HelperFunctionDescriptionAttribute(string description);
    public string? Description { get; set; }
}

// Describes a parameter for a helper function
[AttributeUsage(AttributeTargets.Parameter)]
public class HelperFunctionParameterDescriptionAttribute : Attribute {
    public HelperFunctionParameterDescriptionAttribute();
    public HelperFunctionParameterDescriptionAttribute(string? description = null);
    public string? Description { get; set; }
}
```

## Shell Helper API

The Shell Helper API provides classes for interacting with different shell environments.

### ShellSession

```csharp
// Base class for shell sessions
public abstract class ShellSession {
    // Marker to identify the end of command output
    public abstract string Marker { get; }
    
    // Executes a command with timeout
    public async Task<(string stdout, string stderr, int exitCode)> ExecuteCommandAsync(string command, int timeoutMs = 60000);
    
    // Configure the process for the shell
    protected abstract ProcessStartInfo GetProcessStartInfo();
    
    // Wrap a command to include the marker and exit code
    protected abstract string WrapCommand(string command);
    
    // Parse the exit code from the marker output
    protected abstract int ParseExitCode(string markerOutput);
    
    // Ensure the process is running
    protected void EnsureProcess();
    
    // Shut down the shell session
    public void Shutdown();
}
```

### Shell Session Implementations

```csharp
// Bash shell implementation (Linux/macOS)
public class BashShellSession : ShellSession {
    // Singleton instance
    public static BashShellSession Instance { get; }
}

// CMD shell implementation (Windows)
public class CmdShellSession : ShellSession {
    // Singleton instance
    public static CmdShellSession Instance { get; }
}

// PowerShell implementation (cross-platform)
public class PowershellShellSession : ShellSession {
    // Singleton instance
    public static PowershellShellSession Instance { get; }
}
```

## Helper Classes

### Environment Helpers

```csharp
public class EnvironmentHelpers {
    // Find an environment variable in environment or config files
    public static string? FindEnvVar(string variable);
}
```

### File Helpers

```csharp
class FileHelpers {
    // Directory operations
    public static string EnsureDirectoryExists(string folder);
    public static string FindOrCreateDirectory(params string[] paths);
    public static string? FindDirectory(params string[] paths);
    
    // File operations
    public static void EnsureDirectoryForFileExists(string fileName);
    public static bool FileExists(string? fileName);
    public static string ReadAllText(string fileName);
    public static void WriteAllText(string fileName, string content);
    
    // Path operations
    public static string? GetFileNameFromTemplate(string fileName, string? template);
    public static string MakeRelativePath(string fullPath);
    
    // Embedded resources
    public static IEnumerable<string> GetEmbeddedStreamFileNames();
    public static bool EmbeddedStreamExists(string fileName);
    public static string? ReadEmbeddedStream(string fileName);
}
```

### ChatMessage Extensions

```csharp
public static class OpenAIChatHelpers {
    // Save chat history to a file
    public static void SaveChatHistoryToFile(this IList<ChatMessage> messages, string fileName);
    
    // Read chat history from a file
    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName);
    
    // Token management
    public static bool IsTooBig(this IList<ChatMessage> messages, int maxTokens);
    public static void ReduceToolCallContent(this IList<ChatMessage> messages, int maxTokens, int maxToolCallContentTokens, string replaceToolCallContentWith);
}
```

### Console Helpers

```csharp
class ConsoleHelpers {
    // Configure console output
    public static void Configure(bool debug, bool verbose, bool quiet);
    
    // Output state checks
    public static bool IsQuiet();
    public static bool IsVerbose();
    public static bool IsDebug();
    
    // Display and clear status messages
    public static void DisplayStatus(string status);
    public static void DisplayStatusErase();
    
    // Write to console with color control
    public static void Write(string message = "", ConsoleColor? color = null, bool overrideQuiet = false);
    public static void WriteLine(string message = "", ConsoleColor? color = null, bool overrideQuiet = false);
    
    // Debug output
    public static void WriteDebug(string message);
    public static void WriteDebugLine(string message);
    
    // Stdin reading
    public static IEnumerable<string> GetAllLinesFromStdin();
}
```

### Custom Pipeline Policies

```csharp
// Adds custom headers to requests
public class CustomHeaderPolicy : PipelinePolicy {
    // Constructor
    public CustomHeaderPolicy(string headerName, string headerValue);
    
    // Process a pipeline message synchronously
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex);
    
    // Process a pipeline message asynchronously
    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex);
}

// Logs traffic for debugging
public class LogTrafficEventPolicy : TrafficEventPolicy {
    // Constructor
    public LogTrafficEventPolicy();
}

// Base class for traffic event policies
public class TrafficEventPolicy : PipelinePolicy {
    // Events
    public event EventHandler<PipelineRequest>? OnRequest;
    public event EventHandler<PipelineResponse>? OnResponse;
    
    // Process a pipeline message synchronously
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex);
    
    // Process a pipeline message asynchronously
    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex);
    
    // Fire events
    public void FireEvents(PipelineMessage message);
}
```

### HMAC Helper

```csharp
public static class HMACHelper {
    // Encode an HMAC header value
    public static string Encode(string key);
}
```

### String Helpers

```csharp
public class StringHelpers {
    // Replace one occurrence of a string in text
    public static string? ReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound);
    
    // Exact replacement (only if there's exactly one occurrence)
    public static string? ExactlyReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound);
    
    // Fuzzy replacement (handles whitespace variations)
    public static string? FuzzyReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound);
}
```

## Function Calling Tools

### Date and Time Helper Functions

```csharp
public class DateAndTimeHelperFunctions {
    [HelperFunctionDescription("Gets the current date.")]
    public string GetCurrentDate();
    
    [HelperFunctionDescription("Gets the current time.")]
    public string GetCurrentTime();
}
```

### Shell Command Tool Helper Functions

```csharp
public class ShellCommandToolHelperFunctions {
    [HelperFunctionDescription("Run commands in a bash shell. This persistent session maintains state across commands.")]
    public async Task<string> RunBashCommandAsync(
        [HelperFunctionParameterDescription("The bash command to run.")] string command,
        [HelperFunctionParameterDescription("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000);
    
    [HelperFunctionDescription("Run commands in a cmd shell. This persistent session maintains state across commands.")]
    public async Task<string> RunCmdCommandAsync(
        [HelperFunctionParameterDescription("The cmd command to run.")] string command,
        [HelperFunctionParameterDescription("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000);
    
    [HelperFunctionDescription("Run commands in a PowerShell shell. This persistent session maintains state across commands.")]
    public async Task<string> RunPowershellCommandAsync(
        [HelperFunctionParameterDescription("The PowerShell command to run.")] string command,
        [HelperFunctionParameterDescription("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000);
}
```

### File Editor Helper Functions

```csharp
public class StrReplaceEditorHelperFunctions {
    [HelperFunctionDescription("Returns a list of non-hidden files and directories up to 2 levels deep.")]
    public string ListFiles([HelperFunctionParameterDescription("Absolute or relative path to directory.")] string path);
    
    [HelperFunctionDescription("If `path` is a file, returns the file content (optionally in a specified line range) with line numbers.")]
    public string ViewFile(
        [HelperFunctionParameterDescription("Absolute or relative path to file or directory.")] string path,
        [HelperFunctionParameterDescription("Optional start line number (1-indexed) to view.")] int? startLine = null,
        [HelperFunctionParameterDescription("Optional end line number. Use -1 to view all remaining lines.")] int? endLine = null,
        [HelperFunctionParameterDescription("Optional flag to view the file with line numbers.")] bool lineNumbers = false);
    
    [HelperFunctionDescription("Creates a new file at the specified path with the given content. The `create` command cannot be used if the file already exists.")]
    public string CreateFile(
        [HelperFunctionParameterDescription("Absolute or relative path to file.")] string path,
        [HelperFunctionParameterDescription("Content to be written to the file.")] string fileText);
    
    [HelperFunctionDescription("Replaces the text specified by `oldStr` with `newStr` in the file at `path`. If the provided old string is not unique, no changes are made.")]
    public string StrReplace(
        [HelperFunctionParameterDescription("Absolute or relative path to file.")] string path,
        [HelperFunctionParameterDescription("Existing text in the file that should be replaced. Must match exactly one occurrence.")] string oldStr,
        [HelperFunctionParameterDescription("New string content that will replace the old string.")] string newStr);
    
    [HelperFunctionDescription("Inserts the specified string `newStr` into the file at `path` after the specified line number (`insertLine`). Use 0 to insert at the beginning of the file.")]
    public string Insert(
        [HelperFunctionParameterDescription("Absolute path to file.")] string path,
        [HelperFunctionParameterDescription("Line number (1-indexed) after which to insert the new string.")] int insertLine,
        [HelperFunctionParameterDescription("The string to insert into the file.")] string newStr);
    
    [HelperFunctionDescription("Reverts the last edit made to the file at `path`, undoing the last change if available.")]
    public string UndoEdit(
        [HelperFunctionParameterDescription("Absolute path to file.")] string path);
}
```

## Command Line API

The Command Line API handles parsing and executing command line arguments.

### Command

```csharp
// Base class for commands
abstract class Command {
    // Constructor
    public Command();
    
    // Check if the command has required parameters
    public abstract bool IsEmpty();
    
    // Get the command name
    public abstract string GetCommandName();
    
    // Thread count for parallelism
    public int ThreadCount;
}
```

### Command Implementations

```csharp
// Chat command implementation
class ChatCommand : Command {
    // Execute the chat command
    public async Task<List<Task<int>>> ExecuteAsync(bool interactive);
    
    // Properties for chat options
    public string? SystemPrompt { get; set; }
    public int? TrimTokenTarget { get; set; }
    public string? InputChatHistory;
    public string? OutputChatHistory;
    public List<string> InputInstructions = new();
}

// Help command implementation
class HelpCommand : Command { }
```

### CommandLineOptions

```csharp
// Parses and stores command line options
class CommandLineOptions {
    // Options
    public bool Interactive;
    public bool Debug;
    public bool Verbose;
    public bool Quiet;
    public string HelpTopic;
    public bool ExpandHelpTopics;
    public List<Command> Commands;
    public string[] AllOptions;
    public string? SaveAliasName;
    
    // Parse command line arguments
    public static bool Parse(string[] args, 
                           out CommandLineOptions? options, 
                           out CommandLineException? ex);
    
    // Save the current command options as an alias
    public List<string> SaveAlias(string aliasName);
}
```

### Help Helpers

```csharp
class HelpHelpers {
    // Get available help topics
    public static IEnumerable<string> GetHelpTopics();
    
    // Check if a help topic exists
    public static bool HelpTopicExists(string topic);
    
    // Get help topic text
    public static string? GetHelpTopicText(string topic);
    
    // Display usage information
    public static void DisplayUsage(string command);
    
    // Display help topic
    public static void DisplayHelpTopic(string topic, bool expandTopics = false);
    
    // Display help topics list
    public static void DisplayHelpTopics(bool expandTopics);
    public static void DisplayHelpTopics(IEnumerable<string> topics, bool expandTopics);
}
```

For more detailed information about using these APIs, refer to the source code or specific documentation for each component.