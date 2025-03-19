# API Reference

This document provides a reference for the various APIs and interfaces used in ChatX.

## Chat Client API

The Chat Client API is responsible for handling communication with AI service providers.

### ChatClientFactory

```csharp
// Creates chat clients based on configuration
static class ChatClientFactory {
    // Creates a chat client instance based on the environment variables and configuration
    public static FunctionCallingChat Create();
}
```

### FunctionCallingChat

```csharp
// Main chat client class that handles communication with AI models
class FunctionCallingChat {
    // Send a message to the AI and receive a response
    public async Task<ChatMessage> SendMessageAsync(ChatMessage message, 
                                                  List<ChatMessage> history,
                                                  IReadOnlyList<ToolDefinition> tools);
    
    // Stream a message to the AI and receive a streaming response
    public async Task StreamMessageAsync(ChatMessage message, 
                                       List<ChatMessage> history,
                                       IReadOnlyList<ToolDefinition> tools,
                                       Action<string> onContent,
                                       Action<ToolCall> onToolCall,
                                       Action<ChatMessage> onComplete);
}
```

## Function Calling API

The Function Calling API provides mechanisms to define and execute tools that the AI can use.

### FunctionFactory

```csharp
// Creates function definitions from helper classes
static class FunctionFactory {
    // Creates a list of tool definitions from helper function classes
    public static List<ToolDefinition> CreateToolDefinitions(IEnumerable<Type> helperFunctionClasses);
    
    // Creates a function context for executing functions
    public static FunctionCallContext CreateFunctionCallContext(IEnumerable<Type> helperFunctionClasses);
}
```

### FunctionCallContext

```csharp
// Context for executing function calls
class FunctionCallContext {
    // Execute a function call with the given name and arguments
    public async Task<string> ExecuteFunction(string functionName, string arguments);
}
```

### Helper Function Attributes

```csharp
// Describes a helper function class
[AttributeUsage(AttributeTargets.Class)]
class HelperFunctionDescriptionAttribute : Attribute {
    public string Description { get; }
}

// Describes a parameter for a helper function
[AttributeUsage(AttributeTargets.Parameter)]
class HelperFunctionParameterDescriptionAttribute : Attribute {
    public string Description { get; }
}
```

## Shell Helper API

The Shell Helper API provides classes for interacting with different shell environments.

### ShellSession

```csharp
// Base class for shell sessions
abstract class ShellSession : IDisposable {
    // Executes a command in the shell
    public abstract Task<string> ExecuteCommand(string command, int timeoutMs = 30000);
    
    // Disposes the shell session
    public void Dispose();
}
```

### Shell Session Implementations

```csharp
// Bash shell implementation (Linux/macOS)
class BashShellSession : ShellSession { }

// CMD shell implementation (Windows)
class CmdShellSession : ShellSession { }

// PowerShell implementation (cross-platform)
class PowershellShellSession : ShellSession { }
```

## Helper Functions

ChatX includes several helper function classes that provide functionality for the AI assistant:

### Time and Date Helper Functions

```csharp
class DateAndTimeHelperFunctions {
    public string GetCurrentDate();
    public string GetCurrentTime();
}
```

### Shell Command Tool Helper Functions

```csharp
class ShellCommandToolHelperFunctions {
    public async Task<string> RunBashCommandAsync(string command, int timeoutMs = 30000);
    public async Task<string> RunCmdCommandAsync(string command, int timeoutMs = 30000);
    public async Task<string> RunPowershellCommandAsync(string command, int timeoutMs = 30000);
}
```

### File Editor Helper Functions

```csharp
class StrReplaceEditorHelperFunctions {
    public string ListFiles(string path);
    public string ViewFile(string path, string startLine = "", string endLine = "", bool lineNumbers = false);
    public string CreateFile(string path, string fileText);
    public string StrReplace(string path, string oldStr, string newStr);
    public string Insert(string path, int insertLine, string newStr);
    public string UndoEdit(string path);
}
```

## Command Line API

The Command Line API handles parsing and executing command line arguments.

### Command

```csharp
// Base class for commands
abstract class Command {
    // Execute the command
    public abstract Task Execute();
    
    // Get the command name
    public abstract string GetCommandName();
    
    // Check if the command has required parameters
    public abstract bool IsEmpty();
}
```

### Command Implementations

```csharp
// Chat command implementation
class ChatCommand : Command {
    // Properties for chat options
    public string? SystemPrompt { get; set; }
    public List<string> InputInstructions { get; } = new();
    public string? InputChatHistory { get; set; }
    public string? OutputChatHistory { get; set; }
}

// Help command implementation
class HelpCommand : Command { }
```

### CommandLineOptions

```csharp
// Parses and stores command line options
class CommandLineOptions {
    // Parse command line arguments
    public static bool Parse(string[] args, 
                           out CommandLineOptions? options, 
                           out CommandLineException? ex);
    
    // Save the current command options as an alias
    public List<string> SaveAlias(string aliasName);
}
```

For more detailed information about using these APIs, refer to the source code or specific documentation for each component.