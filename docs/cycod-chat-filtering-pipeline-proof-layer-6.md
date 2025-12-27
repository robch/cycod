# cycod chat - Layer 6: DISPLAY CONTROL - Proof

## Overview

This document provides **source code evidence** for all display control mechanisms in the `chat` command, including file names, line numbers, and code excerpts.

---

## 1. Interactive Mode {#interactive-mode}

### 1.1 Definition and Default Value

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 5-11**: Constructor sets default `Interactive = true`
```csharp
protected CommandLineOptions()
{
    Interactive = true;

    Debug = false;
    Verbose = false;
    Quiet = false;
```

**Line 25**: Public field declaration
```csharp
public bool Interactive;
```

### 1.2 Command-Line Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 334-340**: Parsing `--interactive [true|false]`
```csharp
else if (arg == "--interactive")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var interactive = max1Arg.FirstOrDefault() ?? "true";
    this.Interactive = interactive.ToLower() == "true" || interactive == "1";
    i += max1Arg.Count();
}
```

**Explanation**: 
- Accepts optional boolean argument
- Defaults to `true` if no argument provided
- Accepts `"true"`, `"1"`, or any case variation of `"true"`

### 1.3 Auto-Detection of Redirection

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 17-19**: Detects stdin/stdout redirection
```csharp
var command = options.Commands.FirstOrDefault();
var oneChatCommandWithNoInput =  command is ChatCommand chatCommand && chatCommand.InputInstructions.Count == 0;
var inOrOutRedirected = Console.IsInputRedirected || Console.IsOutputRedirected;
```

**File**: `src/common/ProgramRunner.cs`

**Lines 114-116**: Determines truly interactive mode
```csharp
var inOrOutRedirected = Console.IsInputRedirected || Console.IsOutputRedirected;
var isTrulyInteractive = commandLineOptions.Interactive && !inOrOutRedirected;
if (isTrulyInteractive && parallelism > 1)
```

**Explanation**: Interactive mode is disabled if stdin or stdout is redirected, even if `--interactive` is not explicitly set to `false`.

### 1.4 Interactive Mode Usage in ChatCommand

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 54**: ExecuteAsync receives interactive flag
```csharp
public override async Task<object> ExecuteAsync(bool interactive)
```

**Lines 148-150**: Check for valid execution mode
```csharp
// Check to make sure we're either in interactive mode, or have input instructions.
if (!interactive && InputInstructions.Count == 0)
{
```

**Lines 158-159**: Interactive input handling
```csharp
var userPrompt = interactive && !Console.IsInputRedirected
    ? InteractivelyReadLineOrSimulateInput(InputInstructions, "exit")
```

**Lines 558-567**: Interactive read implementation
```csharp
private string? InteractivelyReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
{
    var havePendingInput = inputInstructions.Count > 0;
    if (havePendingInput) return GetNextInputInstruction(inputInstructions);
    if (defaultOnEndOfInput != null) return defaultOnEndOfInput;

    string? line = Console.ReadLine();
    if (line == null) return defaultOnEndOfInput;
    
    var isMultiLine = line.StartsWith("```") || line.StartsWith("\"\"\"");
    return isMultiLine ? InteractivelyReadMultiLineInput(line) : line;
}
```

**Explanation**: In interactive mode, chat reads from `Console.ReadLine()` in a loop, processing user input turn-by-turn.

---

## 2. Quiet Mode {#quiet-mode}

### 2.1 Definition and Default Value

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 5-11**: Constructor sets default `Quiet = false`
```csharp
protected CommandLineOptions()
{
    Interactive = true;

    Debug = false;
    Verbose = false;
    Quiet = false;
```

**Line 29**: Public field declaration
```csharp
public bool Quiet;
```

### 2.2 Command-Line Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 350-353**: Parsing `--quiet`
```csharp
else if (arg == "--quiet")
{
    this.Quiet = true;
}
```

### 2.3 Question/Query Shortcut

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 506-510**: `--question` / `-q` enables quiet mode
```csharp
var isQuietNonInteractiveAlias = arg == "--question" || arg == "-q";
if (isQuietNonInteractiveAlias)
{
    this.Quiet = true;
    this.Interactive = false;
}
```

**Lines 531-535**: `--questions` enables quiet mode
```csharp
var isQuietNonInteractiveAlias = arg == "--questions";
if (isQuietNonInteractiveAlias)
{
    this.Quiet = true;
    this.Interactive = false;
}
```

### 2.4 Configuration in ConsoleHelpers

**File**: `src/common/ProgramRunner.cs`

**Lines 76-78**: Sets quiet mode from command line options
```csharp
var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
ConsoleHelpers.Configure(debug, verbose, quiet);
```

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 17-27**: Configure method sets quiet flag
```csharp
public static void Configure(bool debug, bool verbose, bool quiet)
{
    Console.OutputEncoding = Encoding.UTF8;

    _debug = debug;
    _verbose = verbose;
    _quiet = quiet;

    WriteDebugLine($"Debug: {_debug}");
    WriteDebugLine($"Verbose: {_verbose}");
    WriteDebugLine($"Quiet: {_quiet}");
}
```

**Lines 30-33**: IsQuiet() accessor
```csharp
public static bool IsQuiet()
{
    return _quiet;
}
```

**Line 329**: Private static field
```csharp
private static bool _quiet = false;
```

### 2.5 Quiet Mode Implementation in Write Methods

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 72-86**: Write() method with overrideQuiet parameter
```csharp
public static void Write(string message, ConsoleColor? foregroundColor = null, bool overrideQuiet = false)
{
    Write(message, foregroundColor, null, overrideQuiet);
}

public static void Write(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
{
    if (_quiet && !overrideQuiet) return;

    // ... rest of implementation
}
```

**Lines 87-95**: WriteLine() method with overrideQuiet parameter
```csharp
public static void WriteLine(string message = "", ConsoleColor? color = null, bool overrideQuiet = false)
{
    WriteLine(message, color, null, overrideQuiet);
}

public static void WriteLine(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
{
    if (_quiet && !overrideQuiet) return;
    Write(message + '\n', foregroundColor, backgroundColor, overrideQuiet);
}
```

**Explanation**: All console output respects quiet mode unless `overrideQuiet: true` is passed.

### 2.6 Examples of Quiet-Respecting Output

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 840, 850-851**: AI response output respects quiet mode
```csharp
ConsoleHelpers.Write(text, ConsoleColor.White, overrideQuiet: true);  // Line 840

// ...

var oneLineFeedOrTwo = ConsoleHelpers.IsQuiet() ? "\n\n" : "\n";
ConsoleHelpers.Write(oneLineFeedOrTwo, overrideQuiet: true);
```

**Explanation**: AI responses override quiet mode (`overrideQuiet: true`) but adjust spacing based on quiet mode.

**Lines 180, 195, 199**: Empty lines respect quiet mode
```csharp
ConsoleHelpers.WriteLine("", overrideQuiet: true);  // Line 180
ConsoleHelpers.WriteLine("\n", overrideQuiet: true);  // Line 195
ConsoleHelpers.WriteLine("", overrideQuiet: true);  // Line 199
```

### 2.7 Examples of Non-Essential Output Suppressed in Quiet Mode

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 47-54**: Status messages suppressed in verbose/debug mode, not quiet
```csharp
public static void WriteStatus(string status)
{
    if (!_debug && !_verbose) return;
    if (Console.IsOutputRedirected) return;

    // ...
    Console.Write("\r" + status);
}
```

**Explanation**: Status messages are only shown in debug/verbose, never in quiet mode.

---

## 3. Verbose Mode {#verbose-mode}

### 3.1 Definition and Default Value

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 5-11**: Constructor sets default `Verbose = false`
```csharp
protected CommandLineOptions()
{
    Interactive = true;

    Debug = false;
    Verbose = false;
    Quiet = false;
```

**Line 28**: Public field declaration
```csharp
public bool Verbose;
```

### 3.2 Command-Line Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 346-349**: Parsing `--verbose`
```csharp
else if (arg == "--verbose")
{
    this.Verbose = true;
}
```

### 3.3 Configuration in ConsoleHelpers

**File**: `src/common/ProgramRunner.cs`

**Lines 76-78**: Sets verbose mode from command line options
```csharp
var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
ConsoleHelpers.Configure(debug, verbose, quiet);
```

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 35-38**: IsVerbose() accessor
```csharp
public static bool IsVerbose()
{
    return _verbose;
}
```

**Line 328**: Private static field
```csharp
private static bool _verbose = false;
```

### 3.4 Verbose Logging via Logger

**File**: `src/common/Logger/Logger.cs`

**Lines 60-64**: Verbose logging methods
```csharp
public static void Verbose(string message, params object[] args) => 
    LogMessage(LogLevel.Verbose, "VERBOSE:", format: message, args: args);

public static void Verbose(string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0) => 
    LogMessage(LogLevel.Verbose, "VERBOSE:", fileName, lineNumber, message);
```

### 3.5 Verbose Mode Usage Examples

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 733**: Check verbose mode before detailed output
```csharp
if (ConsoleHelpers.IsVerbose())
```

**Line 984**: MCP server logging
```csharp
Logger.Verbose($"MCP: Found {allServers.Count} total MCP servers in configuration");
```

**Line 1080**: MCP server selection logging
```csharp
Logger.Verbose($"MCP: Selected server '{name}' of type {item.Type} (matched criteria)");
```

**File**: `src/cycod/FunctionCallingTools/CycoDmdCliWrapper.cs`

**Lines 86, 96-99, 130-135**: Verbose logging in cycodmd wrapper
```csharp
if (ConsoleHelpers.IsVerbose())  // Line 86

// ...

.WithVerboseLogging(ConsoleHelpers.IsVerbose());  // Line 96

// Add callbacks if verbose logging is enabled
if (ConsoleHelpers.IsVerbose())  // Line 99

// ...

// Write debug files if verbose is enabled
if (ConsoleHelpers.IsVerbose())  // Line 130
{
    // ...
    ConsoleHelpers.WriteLine($"ExecuteCycoDmdCommandAsync inputs/outputs: {baseFileName}-*", 
        ConsoleColor.DarkMagenta, overrideQuiet: true);
}
```

**File**: `src/cycod/FunctionCallingTools/GitHubSearchHelperFunctions.cs`

**Lines 55, 65**: Verbose logging in GitHub search
```csharp
if (ConsoleHelpers.IsVerbose())  // Line 55
{
    // ...
}

// ...

.WithVerboseLogging(ConsoleHelpers.IsVerbose());  // Line 65
```

**File**: `src/cycod/McpHelpers/McpClientManager.cs`

**Line 64**: MCP client creation timing
```csharp
Logger.Verbose($"MCP: Client creation for '{serverName}' took {sw.ElapsedMilliseconds}ms");
```

**File**: `src/cycod/FunctionCalling/McpFunctionFactory.cs`

**Lines 168, 173, 178**: MCP client disposal logging
```csharp
Logger.Verbose($"MCP: Disposing client '{clientName}'");  // Line 168

// ...

Logger.Verbose($"MCP: Async disposed client '{clientName}'");  // Line 173

// ...

Logger.Verbose($"MCP: Disposed client '{clientName}'");  // Line 178
```

**File**: `src/common/Helpers/LineHelpers.cs`

**Lines 14, 23-24, 34-35**: Line filtering verbose output
```csharp
// Log detailed information at verbose level
if (ConsoleHelpers.IsVerbose())  // Line 14

// ...

Logger.Verbose($"Line excluded because it doesn't match include patterns: [{string.Join(", ", failedPatterns)}]");
Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");  // Lines 23-24

// ...

Logger.Verbose($"Line excluded because it matches exclude patterns: [{string.Join(", ", matchedPatterns)}]");
Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");  // Lines 34-35
```

---

## 4. Debug Mode {#debug-mode}

### 4.1 Definition and Default Value

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 5-11**: Constructor sets default `Debug = false`
```csharp
protected CommandLineOptions()
{
    Interactive = true;

    Debug = false;
    Verbose = false;
    Quiet = false;
```

**Line 27**: Public field declaration
```csharp
public bool Debug;
```

### 4.2 Command-Line Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 341-345**: Parsing `--debug`
```csharp
else if (arg == "--debug")
{
    this.Debug = true;
    ConsoleHelpers.ConfigureDebug(true);
}
```

**Explanation**: Setting debug mode also configures ConsoleHelpers immediately.

### 4.3 Debug Mode Configuration

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 40-45**: ConfigureDebug method
```csharp
public static void ConfigureDebug(bool debug)
{
    _debug = debug;
}
```

**Line 327**: Private static field
```csharp
private static bool _debug = false;
```

### 4.4 Debug Output Usage

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 165-166**: Debug output during command-line parsing
```csharp
for (var j = 0; j < i; j++) ConsoleHelpers.WriteDebugLine($"arg[{j}] = {args[j]}");
ConsoleHelpers.WriteDebugLine($"(INVALID) arg[{i}] = {args[i]}");
```

**Lines 197, 218, 224, 303**: Debug output for known settings
```csharp
ConsoleHelpers.WriteDebugLine($"Set known setting from CLI: {settingName} = {value}");  // Line 197

ConsoleHelpers.WriteDebugLine($"Set known setting from CLI: {settingName} = {settingValue}");  // Line 218

ConsoleHelpers.WriteDebugLine($"Set known setting from CLI: {settingName} = [{string.Join(", ", arguments)}]");  // Line 224

ConsoleHelpers.WriteDebugLine($"Unknown command line option: {arg}");  // Line 303
```

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 130-139**: WriteDebugLine and WriteVerboseLine methods
```csharp
public static void WriteDebugLine(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
{
    Logger.LogMessage(LogLevel.Verbose, "VERBOSE:", callerFilePath, callerLineNumber, message);
}

public static void WriteVerboseLine(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
{
    if (!_verbose) return;
    Logger.LogMessage(LogLevel.Verbose, "VERBOSE:", callerFilePath, callerLineNumber, message);
}
```

**Explanation**: Debug lines are always logged, verbose lines only when verbose mode is enabled.

**File**: `src/common/ProgramRunner.cs`

**Line 118**: Debug line in program runner
```csharp
ConsoleHelpers.WriteDebugLine($"Max 1 thread in truly interactive mode");
```

---

## 5. Streaming Output {#streaming-output}

### 5.1 Streaming Response Display

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 835-855**: DisplayStreamingResponse method
```csharp
private void DisplayStreamingResponse(IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates)
{
    var isSomethingToDisplay = false;
    await foreach (var update in streamingUpdates)
    {
        var text = update.Text ?? string.Empty;
        ConsoleHelpers.Write(text, ConsoleColor.White, overrideQuiet: true);
        
        if (!string.IsNullOrEmpty(text))
        {
            isSomethingToDisplay = true;
            _autoSaveTrajectoryFile?.AddAssistantChunk(text);
            _trajectoryFile?.AddAssistantChunk(text);
        }
    }
    
    if (isSomethingToDisplay)
    {
        var oneLineFeedOrTwo = ConsoleHelpers.IsQuiet() ? "\n\n" : "\n";
        ConsoleHelpers.Write(oneLineFeedOrTwo, overrideQuiet: true);
    }
}
```

**Explanation**: 
- Iterates through `IAsyncEnumerable<StreamingChatCompletionUpdate>`
- Displays each text chunk immediately with `ConsoleHelpers.Write()`
- Always overrides quiet mode for AI responses
- Adjusts line spacing based on quiet mode
- Records chunks to trajectory files in real-time

### 5.2 Streaming API Usage

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 219-233**: Streaming chat completion call
```csharp
var streamingUpdates = chat.CompleteStreamingAsync(
    userPrompt: userPromptGrounded,
    maxTurns: 50,
    functionCallApprovalHandler: _functionCallApprovalHandler);

// ...

await foreach (var update in streamingUpdates)
{
    result.Append(update.Text);
}
```

**Explanation**: Uses `CompleteStreamingAsync` which returns `IAsyncEnumerable<StreamingChatCompletionUpdate>` for real-time token delivery.

---

## 6. Console Output Formatting {#console-output-formatting}

### 6.1 Color-Coded Output

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 365**: User prompt substitution (dark gray)
```csharp
ConsoleHelpers.WriteLine($"\rUser: {userPrompt} => {replaceUserPrompt}", ConsoleColor.DarkGray, overrideQuiet: true);
```

**Lines 375, 384, 386, 393**: Various colored messages
```csharp
ConsoleHelpers.WriteLine("Cleared chat history.\n", ConsoleColor.Yellow, overrideQuiet: true);  // Line 375

ConsoleHelpers.Write($"Saving {fileName} ...", ConsoleColor.Yellow, overrideQuiet: true);  // Line 384

ConsoleHelpers.WriteLine("Saved!\n", ConsoleColor.Yellow, overrideQuiet: true);  // Line 386

ConsoleHelpers.WriteLine($"Tokens: {_totalTokensIn} in, {_totalTokensOut} out\n", ConsoleColor.Yellow, overrideQuiet: true);  // Line 393
```

**Line 445**: Function result display (indented)
```csharp
ConsoleHelpers.WriteLine(indented, overrideQuiet: true);
```

**Line 840**: AI response (white)
```csharp
ConsoleHelpers.Write(text, ConsoleColor.White, overrideQuiet: true);
```

**File**: `src/cycod/FunctionCallingTools/CycoDmdCliWrapper.cs`

**Line 135**: Debug output (dark magenta)
```csharp
ConsoleHelpers.WriteLine($"ExecuteCycoDmdCommandAsync inputs/outputs: {baseFileName}-*", 
    ConsoleColor.DarkMagenta, overrideQuiet: true);
```

**File**: `src/common/Helpers/ConsoleHelpers.cs`

**Lines 106-124**: Error and warning output (colored backgrounds)
```csharp
public static void WriteError(string message)
{
    Write(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
}

public static void WriteErrorLine(string message = "")
{
    WriteLine(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
}

public static void WriteWarning(string message)
{
    Write(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
}

public static void WriteWarningLine(string message = "")
{
    WriteLine(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
}
```

### 6.2 Line Spacing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 180, 195, 199**: Empty lines between turns
```csharp
ConsoleHelpers.WriteLine("", overrideQuiet: true);  // Line 180
ConsoleHelpers.WriteLine("\n", overrideQuiet: true);  // Line 195
ConsoleHelpers.WriteLine("", overrideQuiet: true);  // Line 199
```

**Lines 850-851**: Context-sensitive line spacing
```csharp
var oneLineFeedOrTwo = ConsoleHelpers.IsQuiet() ? "\n\n" : "\n";
ConsoleHelpers.Write(oneLineFeedOrTwo, overrideQuiet: true);
```

**Lines 902, 904**: Double newlines for visual separation
```csharp
ConsoleHelpers.Write("\n\n", overrideQuiet: true);  // Line 902
ConsoleHelpers.Write("\n", overrideQuiet: true);  // Line 904
```

---

## 7. Function Call Display {#function-call-display}

### 7.1 Function Result Formatting

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 441-446**: DisplayFunctionResult method
```csharp
private void DisplayFunctionResult(ChatMessage functionResultMessage)
{
    var text = functionResultMessage.Text;
    var functionName = GetFunctionName(functionResultMessage);
    var indented = IndentAndPrefixLines(text, $"{functionName} → Result:\n  ", "  ");
    ConsoleHelpers.WriteLine(indented, overrideQuiet: true);
}
```

**Explanation**: Formats function results with indentation and prefix showing the function name.

### 7.2 Function Name Extraction

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 448-453**: GetFunctionName method
```csharp
private string GetFunctionName(ChatMessage message)
{
    var toolCallId = message.AuthorName ?? string.Empty;
    var functionName = _toolCallIdToFunctionName.TryGetValue(toolCallId, out var name) ? name : string.Empty;
    return functionName;
}
```

### 7.3 Indentation and Prefixing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 455-478**: IndentAndPrefixLines method
```csharp
private string IndentAndPrefixLines(string text, string firstLinePrefix, string otherLineIndent)
{
    if (string.IsNullOrEmpty(text)) return firstLinePrefix;

    var lines = text.Split('\n');
    var sb = new StringBuilder();

    var isFirst = true;
    foreach (var line in lines)
    {
        if (isFirst)
        {
            sb.Append(firstLinePrefix);
            isFirst = false;
        }
        else
        {
            sb.Append('\n');
            sb.Append(otherLineIndent);
        }

        sb.Append(line.TrimEnd('\r'));
    }

    return sb.ToString();
}
```

**Explanation**: Adds prefix to first line and indents subsequent lines for structured display.

---

## 8. Token Usage Display {#token-usage-display}

### 8.1 Token Tracking Fields

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 1229-1230**: Private fields for token tracking
```csharp
private int _totalTokensIn = 0;
private int _totalTokensOut = 0;
```

### 8.2 Token Display

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 393**: DisplayTokenCount method
```csharp
ConsoleHelpers.WriteLine($"Tokens: {_totalTokensIn} in, {_totalTokensOut} out\n", ConsoleColor.Yellow, overrideQuiet: true);
```

**Explanation**: Displays cumulative token usage in yellow, respecting quiet mode via `overrideQuiet: true`.

### 8.3 Token Accumulation

Token counting happens within the AI SDK and FunctionCallingChat classes. The totals are accumulated after each turn.

---

## 9. Console Title Updates {#console-title-updates}

### 9.1 ConsoleTitleHelper Class

**File**: `src/cycod/Helpers/ConsoleTitleHelper.cs`

**Lines 15-35**: UpdateWindowTitle methods
```csharp
public static void UpdateWindowTitle(ConversationMetadata? metadata)
{
    if (metadata == null || string.IsNullOrWhiteSpace(metadata.Title))
    {
        SetDefaultTitle();
        return;
    }

    var title = metadata.Title!.Trim();
    try
    {
        Console.Title = title;
    }
    catch
    {
        // Console.Title can fail in some environments (e.g., CI/CD, certain terminals)
        // Silently ignore failures
    }
}

private static void SetDefaultTitle()
{
    try
    {
        Console.Title = DefaultTitle;
    }
    catch { }
}
```

**Explanation**: Updates console title with conversation title, failing gracefully if not supported.

### 9.2 Title Update Triggers

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 144-146**: After loading chat history
```csharp
// Update console title with loaded conversation title
ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
```

**File**: `src/cycod/SlashCommands/SlashTitleCommandHandler.cs`

Title updates happen after `/title` commands execute (implementation in SlashTitleCommandHandler class).

---

## 10. Multi-Line Input Detection {#multi-line-input-detection}

### 10.1 Multi-Line Detection

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 558-567**: InteractivelyReadLineOrSimulateInput
```csharp
private string? InteractivelyReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
{
    var havePendingInput = inputInstructions.Count > 0;
    if (havePendingInput) return GetNextInputInstruction(inputInstructions);
    if (defaultOnEndOfInput != null) return defaultOnEndOfInput;

    string? line = Console.ReadLine();
    if (line == null) return defaultOnEndOfInput;
    
    var isMultiLine = line.StartsWith("```") || line.StartsWith("\"\"\"");
    return isMultiLine ? InteractivelyReadMultiLineInput(line) : line;
}
```

**Explanation**: Detects multi-line input by checking for triple backticks or triple quotes.

### 10.2 Multi-Line Input Reading

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 570-595**: InteractivelyReadMultiLineInput method
```csharp
private string? InteractivelyReadMultiLineInput(string firstLine)    
{
    var delimiter = firstLine.StartsWith("```") ? "```" : "\"\"\"";
    var lines = new List<string> { firstLine };

    while (true)
    {
        var line = ReadLineOrSimulateInput();
        if (line == null)
        {
            break;
        }

        lines.Add(line);

        if (line.TrimEnd() == delimiter)
        {
            break;
        }
    }

    return string.Join("\n", lines);
}
```

**Explanation**: Reads lines until matching closing delimiter is found.

---

## Summary

This proof document provides comprehensive source code evidence for all display control mechanisms in the `chat` command:

1. **Interactive Mode**: Defined in `CommandLineOptions.cs`, parsed at lines 334-340, used throughout `ChatCommand.cs`
2. **Quiet Mode**: Defined in `CommandLineOptions.cs`, enforced in `ConsoleHelpers.Write/WriteLine` via `overrideQuiet` parameter
3. **Verbose Mode**: Defined in `CommandLineOptions.cs`, used via `ConsoleHelpers.IsVerbose()` and `Logger.Verbose()` throughout codebase
4. **Debug Mode**: Defined in `CommandLineOptions.cs`, enables verbose output and debug logging
5. **Streaming Output**: Implemented in `DisplayStreamingResponse` at lines 835-855 of `ChatCommand.cs`
6. **Console Output Formatting**: Color-coded output throughout codebase using `ConsoleHelpers` methods
7. **Function Call Display**: Formatted display in `DisplayFunctionResult` at lines 441-446 of `ChatCommand.cs`
8. **Token Usage Display**: Tracked in private fields, displayed at line 393 of `ChatCommand.cs`
9. **Console Title Updates**: Handled by `ConsoleTitleHelper.cs`, triggered after history load and title commands
10. **Multi-Line Input**: Detected and read in `InteractivelyReadMultiLineInput` at lines 570-595 of `ChatCommand.cs`

All line numbers and code excerpts are current as of the latest codebase version.
