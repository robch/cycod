## .cycodmdignore

Modified: 2 minutes ago
Size: 43 bytes

```plaintext
**/chat-history*.jsonl
```

## .git

Modified: 2 minutes ago
Size: 62 bytes

```plaintext
gitdir: C:/src/cycod/.git/worktrees/cycod-always-chat-history
```

## CHANGELOG.md

Modified: 2 minutes ago
Size: 2 KB

```markdown
  - Ensures chat history doesn't exceed token limits, particularly important when using --input-chat-history
- Chat history loading and saving
```

## README.md

Modified: 2 minutes ago
Size: 6 KB

````markdown
- **Chat History**: Load and save chat histories for later reference
- `--input-chat-history <file>`: Load previous chat history from a file
- `--output-chat-history <file>`: Save chat history to a file
- `--max-chat-tokens <n>`: Set a target for trimming chat history when it gets too large
Save and load chat history:
cycod --output-chat-history "linux-help-session.jsonl"
cycod --input-chat-history "linux-help-session.jsonl"
- [Chat History](docs/chat-history.md)
- `/clear` - Clear the current chat history
- `/save` - Save the current chat history to a file
````

## SUPPORT.md

Modified: 2 minutes ago
Size: 1 KB

```markdown
   - Chat history problems: Ensure the specified directories are writable
```

## docs\aliases.md

Modified: 2 minutes ago
Size: 7 KB

````markdown
cycod --tech-writer --input "Write documentation for a REST API endpoint that creates user accounts" --output-chat-history "api-docs.jsonl"
````

## docs\api-reference.md

Modified: 2 minutes ago
Size: 17 KB

````markdown
    // Clears the chat history
    public void ClearChatHistory();
    // Loads chat history from a file
    public void LoadChatHistory(string fileName);
    // Save chat history to a file
    public static void SaveChatHistoryToFile(this IList<ChatMessage> messages, string fileName);
    // Read chat history from a file
    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName);
    public string? InputChatHistory;
    public string? OutputChatHistory;
````

## docs\chat-history.md

Modified: 2 minutes ago
Size: 9 KB

````markdown
# Chat History
## Understanding Chat History Files
Chat histories in CycoD are stored in JSONL (JSON Lines) format, where each line contains a JSON object representing a message in the conversation. This format makes it easy to append new messages to the history file and to parse individual messages without loading the entire file.
## Saving Chat History
By default, CycoD automatically saves your chat history and trajectory files to the `history` directory under your user profile:
- Chat history: `chat-history-{time}.jsonl`
cycod config set App.AutoSaveChatHistory false
cycod config set App.AutoSaveChatHistory true
You can also explicitly specify where to save your chat history using the `--output-chat-history` option:
cycod --output-chat-history "my-project-chat.jsonl"
If no filename is specified, CycoD uses a default template: `chat-history-{time}.jsonl`, where `{time}` is replaced with the current date and time.
cycod --output-chat-history "chats/{filebase}-{timestamp}.jsonl"
When you use the `--output-chat-history` option, CycoD automatically saves each message (both your inputs and the AI's responses) to the specified file as they occur. This ensures that your conversation is preserved even if the program unexpectedly terminates.
## Loading Chat History
To continue a previous conversation, you can load a chat history using the `--input-chat-history` option:
cycod --input-chat-history "my-project-chat.jsonl"
Alternatively, you can use the `--continue` option to automatically load the most recent chat history file:
This will search for the most recent chat history file (matching the pattern "chat-history-*.jsonl") in both:
You can both load and save chat history in the same command, which is useful for continuing and extending an existing conversation:
cycod --input-chat-history "previous-conversation.jsonl" --output-chat-history "continued-conversation.jsonl"
cycod --input-chat-history "long-conversation.jsonl" --output-chat-history "long-conversation.jsonl" --max-token-target 160000
1. When loading chat history from a file (via `--input-chat-history`)
## Chat History and Context
cycod --system-prompt "You are an AI assistant helping me plan and implement a web application." --output-chat-history "web-app-project.jsonl"
cycod --input-chat-history "web-app-project.jsonl" --output-chat-history "web-app-project.jsonl"
cycod --input-chat-history "long-project.jsonl" --output-chat-history "long-project.jsonl" --max-token-target 120000
cycod --input-chat-history "project-brainstorm.jsonl" --output-chat-history "project-implementation.jsonl"
Each line in a chat history file contains a JSON object representing a message. The format varies slightly depending on the message type:
In addition to JSONL-formatted chat history, CycoD can also save your conversation in a more human-readable trajectory format using the `--output-trajectory` option:
2. You need to review your chat history yourself
It's important to note that unlike JSONL chat history files, trajectory files cannot be loaded back into CycoD as context for future conversations.
cycod --output-chat-history "conversation.jsonl" --output-trajectory "conversation.md"
````

## docs\cli-options.md

Modified: 2 minutes ago
Size: 9 KB

````markdown
| `--add-user-prompt <text>` | Add a user prompt that gets inserted when chat history is cleared (can be used multiple times) |
| `--input-chat-history <file>` | Load previous chat history from a JSONL file |
| `--continue` | Continue the most recent chat history (auto-finds latest chat history file) |
| `--output-chat-history <file>` | Save chat history to a JSONL file |
| `--output-trajectory <file>` | Save chat history in a more readable trajectory format |
| `--max-token-target <n>` | Set a target token count for trimming chat history when it gets too large |
### Loading and Saving Chat History
cycod --output-chat-history "programming-help.jsonl"
cycod --input-chat-history "programming-help.jsonl" --input "Can you explain that last example again?"
cycod --max-token-target 120000 --input-chat-history "long-conversation.jsonl" --output-chat-history "long-conversation.jsonl"
cycod --python-expert --input "Explain decorators in Python" --output-chat-history "python-learning.jsonl"
````

## docs\getting-started.md

Modified: 2 minutes ago
Size: 4 KB

````markdown
## Managing Chat History
cycod --output-chat-history "my-chat.jsonl"
cycod --input-chat-history "my-chat.jsonl"
- Learn how to manage [Chat History](chat-history.md)
````

## src\cycodmd\assets\help\options.txt

Modified: 2 minutes ago
Size: 2 KB

```plaintext
    --save-chat-history [FILE]     Save the chat history to the specified file
                                   (e.g. chat-history-{time}.jsonl)
```

## docs\troubleshooting.md

Modified: 2 minutes ago
Size: 5 KB

````markdown
## Chat History Issues
### Issue: Unable to Save or Load Chat History
- Missing chat history files
3. **Large Chat History**: Very large chat histories can slow down the AI's response time.
````

## src\cycodt\TestFramework\README.md

Modified: 2 minutes ago
Size: 9 KB

````markdown
    output-chat-history: chat-history-${{ matrix.__matrix_id__ }}.jsonl
In this example the test case will run three times with `assistant-id` set to `asst_TqfFCksyWK83VKe76kiBYWGt`, and `question` set to the three questions specified in the `foreach` list, and the `output-chat-history` specifies where to save the chat history, using a filename that is unique to the "matrix" combination.
````

## src\common\Configuration\KnownSettings.cs

Modified: 2 minutes ago
Size: 18 KB

```csharp
    public const string AppAutoSaveChatHistory = "App.AutoSaveChatHistory";
        { AppAutoSaveChatHistory, "CYCOD_AUTO_SAVE_CHAT_HISTORY" },
        { AppAutoSaveChatHistory, "--auto-save-chat-history" },
        AppAutoSaveChatHistory,
```

## src\cycod\assets\help\chat history.txt

Modified: 2 minutes ago
Size: 2 KB

```plaintext
CYCOD CHAT HISTORY
  1. Chat History (JSONL format) - Machine-readable format for reloading context
  By default, CycoD automatically saves both your chat history and trajectory
    - Chat history: chat-history-{time}.jsonl
    cycod config set App.AutoSaveChatHistory false --local
    cycod config set App.AutoSaveChatHistory false --user
    cycod config set App.AutoSaveChatHistory false --global
    cycod config get App.AutoSaveChatHistory
    --output-chat-history <path>    Save chat history to specified file
  To continue a previous conversation, load a chat history using:
    --input-chat-history <path>     Load chat history from specified file
    --continue                      Continue the most recent chat history
```

## src\cycod\assets\help\examples.txt

Modified: 2 minutes ago
Size: 2 KB

```plaintext
  EXAMPLE 4: Continue the most recent chat history
  EXAMPLE 5: Save chat history in JSONL format
    cycod --question "Tell me a joke" --output-chat-history chat-history.jsonl
  EXAMPLE 6: Continue chat, after loading chat history from a JSONL file
    cycod --input-chat-history chat-history.jsonl --question "Tell me another"
  EXAMPLE 7: Save chat history in human readable trajectory format
```

## src\cycod\assets\help\options.txt

Modified: 2 minutes ago
Size: 7 KB

```plaintext
  CHAT HISTORY                            (see: cycod help chat history)
    --continue                            Continue the most recent chat history
    --chat-history [FILE]                 Load from and save to the same JSONL file
    --input-chat-history [FILE]           Load chat history from the specified JSONL file
    --output-chat-history [FILE]          Save chat history to the specified file
    --output-trajectory [FILE]            Save chat history in human readable trajectory format
```

## src\cycod\assets\help\settings.txt

Modified: 2 minutes ago
Size: 6 KB

```plaintext
  AUTO-SAVE CHAT HISTORY
    EXAMPLE 1: Control auto-saving chat history via config
      cycod config set App.AutoSaveChatHistory true
      cycod config set App.AutoSaveChatHistory false --local
      cycod config set App.AutoSaveChatHistory false --user
      cycod config set App.AutoSaveChatHistory false --global
    EXAMPLE 2: Control auto-saving chat history via environment variable
      Set CYCOD_AUTO_SAVE_CHAT_HISTORY environment variable to true/false
    EXAMPLE 3: Control auto-saving chat history via command line
      cycod chat --auto-save-chat-history true
      cycod chat --auto-save-chat-history false
  cycod help chat history
```

## src\cycod\assets\help\slash commands.txt

Modified: 2 minutes ago
Size: 2 KB

```plaintext
    /clear                  Clear the current chat history
    /save                   Save the current chat history to a file
```

## src\cycod\ChatClient\FunctionCallingChat.cs

Modified: 2 minutes ago
Size: 11 KB

```csharp
        ClearChatHistory();
    public void ClearChatHistory()
    public void LoadChatHistory(string fileName, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
        _messages.ReadChatHistoryFromFile(fileName, useOpenAIFormat);
    public void SaveChatHistoryToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
        _messages.SaveChatHistoryToFile(fileName, useOpenAIFormat, saveToFolderOnAccessDenied);
    public void SaveTrajectoryToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
```

## src\cycod\CommandLine\CycoDevCommandLineOptions.cs

Modified: 2 minutes ago
Size: 26 KB

```csharp
        else if (arg == "--chat-history")
            var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
            command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
            command.OutputChatHistory = chatHistory;
            command.LoadMostRecentChatHistory = false;
        else if (arg == "--input-chat-history")
            var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
            command.InputChatHistory = inputChatHistory;
            command.LoadMostRecentChatHistory = false;
            command.LoadMostRecentChatHistory = true;
            command.InputChatHistory = null;
        else if (arg == "--output-chat-history")
            var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
            command.OutputChatHistory = outputChatHistory;
    private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
    private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
```

## src\cycod\CommandLineCommands\ChatCommand.cs

Modified: 2 minutes ago
Size: 35 KB

```csharp
        clone.LoadMostRecentChatHistory = this.LoadMostRecentChatHistory;
        clone.InputChatHistory = this.InputChatHistory;
        clone.OutputChatHistory = this.OutputChatHistory;
        InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
        OutputChatHistory = ChatHistoryFileHelpers.GroundOutputChatHistoryFileName(OutputChatHistory)?.ReplaceValues(_namedValues);
        OutputTrajectory = ChatHistoryFileHelpers.GroundOutputTrajectoryFileName(OutputTrajectory)?.ReplaceValues(_namedValues);
            // Load the chat history from the file.
            var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
            if (loadChatHistory)
                chat.LoadChatHistory(InputChatHistory!,
                    useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            skipAssistant = HandleSaveChatHistoryCommand(chat, userPrompt.Substring("/save".Length).Trim());
            skipAssistant = HandleClearChatHistoryCommand(chat);
    private bool HandleClearChatHistoryCommand(FunctionCallingChat chat)
        chat.ClearChatHistory();
        ConsoleHelpers.WriteLine("Cleared chat history.\n", ConsoleColor.Yellow, overrideQuiet: true);
    private bool HandleSaveChatHistoryCommand(FunctionCallingChat chat, string? fileName = null)
        if (useDefaultFileName) fileName = "chat-history.jsonl";
        chat.SaveChatHistoryToFile(fileName!, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
        helpBuilder.AppendLine("  /save     Save chat history to file");
        helpBuilder.AppendLine("  /clear    Clear chat history");
        if (OutputChatHistory != null)
            messages.SaveChatHistoryToFile(OutputChatHistory, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
        SaveExceptionChatHistory(chat);
    private static void SaveExceptionChatHistory(FunctionCallingChat chat)
            var fileName = FileHelpers.GetFileNameFromTemplate("exception-chat-history.jsonl", "{filebase}-{time}.{fileext}")!;
            chat.SaveChatHistoryToFile(fileName, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat, saveToFolderOnAccessDenied);
            ConsoleHelpers.WriteWarning("Failed to save exception chat history.");
            chat.SaveTrajectoryToFile(fileName, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat, saveToFolderOnAccessDenied);
    public bool LoadMostRecentChatHistory = false;
    public string? InputChatHistory;
    public string? OutputChatHistory;
```

## src\cycod\Helpers\ChatHistoryDefaults.cs

Modified: 2 minutes ago
Size: 94 bytes

```csharp
public static class ChatHistoryDefaults
```

## src\cycod\Helpers\ChatHistoryFileHelpers.cs

Modified: 2 minutes ago
Size: 5 KB

```csharp
/// Provides methods for working with chat history files.
public static class ChatHistoryFileHelpers
    /// Finds the most recent chat history file across all scopes.
    /// <returns>The full path to the most recent chat history file, or null if none found</returns>
    public static string? FindMostRecentChatHistoryFile()
        // Find regular chat history files
        var userScopeHistoryFiles = ScopeFileHelpers.FindFilesInScope("chat-history*.jsonl", "history", ConfigFileScope.User);
        var localScopeHistoryFiles = ScopeFileHelpers.FindFilesInScope("chat-history*.jsonl", "history", ConfigFileScope.Local);
        var localFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "chat-history*.jsonl");
        // Find exception chat history files - these can also be valid to continue from
        var userScopeExceptionFiles = ScopeFileHelpers.FindFilesInScope("exception-chat-history*.jsonl", "history", ConfigFileScope.User);
        var localScopeExceptionFiles = ScopeFileHelpers.FindFilesInScope("exception-chat-history*.jsonl", "history", ConfigFileScope.Local);
        var localExceptionFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "exception-chat-history*.jsonl");
    /// Ensures the chat history directory exists in the specified scope.
    /// Grounds an input chat history filename.
    /// <param name="loadMostRecent">Whether to load the most recent chat history file</param>
    public static string? GroundInputChatHistoryFileName(string? inputFileName, bool loadMostRecent)
        var mostRecentChatHistoryFileName = loadMostRecent
            ? FindMostRecentChatHistoryFile()
        var mostRecentChatHistoryFileExists = FileHelpers.FileExists(mostRecentChatHistoryFileName);
        if (mostRecentChatHistoryFileExists)
            inputFileName = mostRecentChatHistoryFileName;
        return FileHelpers.GetFileNameFromTemplate(inputFileName ?? "chat-history.jsonl", inputFileName);
    /// Grounds an output chat history filename.
    /// <param name="autoSave">Whether to auto-save the chat history</param>
    public static string? GroundOutputChatHistoryFileName(string? outputFileName)
        var shouldAutoSave = !userSpecified && ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveChatHistory).AsBool(true);
            outputFileName = Path.Combine(historyDir, "chat-history-{time}.jsonl");
        return FileHelpers.GetFileNameFromTemplate(outputFileName ?? "chat-history.jsonl", outputFileName);
```

## src\cycod\Helpers\ChatMessageHelpers.cs

Modified: 2 minutes ago
Size: 25 KB

```csharp
    public static void SaveChatHistoryToFile(this IList<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
    public static void SaveTrajectoryToFile(this IList<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
```

## src\cycodmd\AiInstructionProcessor.cs

Modified: 2 minutes ago
Size: 7 KB

```csharp
    public const string DefaultSaveChatHistoryTemplate = "chat-history-{time}.jsonl";
    public static string ApplyAllInstructions(List<string> instructionsList, string content, bool useBuiltInFunctions, string? saveChatHistory, int retries = 1)
            return instructionsList.Aggregate(content, (current, instruction) => ApplyInstructions(instruction, current, useBuiltInFunctions, saveChatHistory, retries));
    public static string ApplyInstructions(string instructions, string content, bool useBuiltInFunctions, string? saveChatHistory, int retries = 1)
            ApplyInstructions(instructions, content, useBuiltInFunctions, saveChatHistory, out var returnCode, out var stdOut, out var stdErr, out var exception);
    private static void ApplyInstructions(string instructions, string content, bool useBuiltInFunctions, string? saveChatHistory, out int returnCode, out string stdOut, out string stdErr, out Exception? exception)
            if (!string.IsNullOrEmpty(saveChatHistory))
                var fileName = FileHelpers.GetFileNameFromTemplate(DefaultSaveChatHistoryTemplate, saveChatHistory);
                arguments += $" --output-chat-history \"{fileName}\"";
```

## src\cycodmd\Program.cs

Modified: 2 minutes ago
Size: 28 KB

```csharp
                    commandOutput = AiInstructionProcessor.ApplyAllInstructions(cycoDmdCommand!.InstructionsList, commandOutput, cycoDmdCommand.UseBuiltInFunctions, cycoDmdCommand.SaveChatHistory);
                findFilesCommand.SaveChatHistory,
        var saveChatHistory = command.SaveChatHistory;
            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
        var saveChatHistory = command.SaveChatHistory;
            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
        //     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
    private static Task<string> GetCheckSaveFileContentAsync(string fileName, SemaphoreSlim throttler, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
                saveChatHistory,
    private static string GetCheckSaveFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
                saveChatHistory);
    private static string GetFinalFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisFile, formatted, useBuiltInFunctions, saveChatHistory)
    private static async Task<string> GetCheckSaveWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? savePageOutput)
            var finalContent = await GetFinalWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory);
    private static async Task<string> GetFinalWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisPage, formatted, useBuiltInFunctions, saveChatHistory)
```

## src\cycodmd\README.md

Modified: 2 minutes ago
Size: 23 KB

````markdown
    --save-chat-history [FILE]     Save the chat history to the specified file
                                   (e.g. chat-history-{time}.jsonl)
    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)
    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)
````

## docs\index.md

Modified: 2 minutes ago
Size: 1 KB

````markdown
- [Chat History](chat-history.md)
- **Chat History**: Load and save chat histories for later reference
````

## src\cycodmd\assets\help\web get options.txt

Modified: 2 minutes ago
Size: 1 KB

```plaintext
    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)
```

## src\cycodmd\assets\help\web search options.txt

Modified: 2 minutes ago
Size: 2 KB

```plaintext
    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)
```

## src\cycodmd\CommandLine\CycoDmdCommand.cs

Modified: 2 minutes ago
Size: 585 bytes

```csharp
        SaveChatHistory = string.Empty;
    public string SaveChatHistory;
```

## src\cycodmd\CommandLine\CycoDmdCommandLineOptions.cs

Modified: 2 minutes ago
Size: 13 KB

```csharp
        else if (arg == "--save-chat-history")
            var saveChatHistory = max1Arg.FirstOrDefault() ?? AiInstructionProcessor.DefaultSaveChatHistoryTemplate;
            command.SaveChatHistory = saveChatHistory;
```

## docs\claude-code\claude-code-vs-chatx-feature-summary-comparison.md

Modified: 2 minutes ago
Size: 13 KB

````markdown
   cycodmd web search "anthropic claude code tutorials" --get --strip --max 5 --instructions "Summarize in markdown, features of claude code terminal/console application, including brief tutorial on key features." --duckduckgo --interactive --save-chat-history "claude-code-tutorial-summary.jsonl"
   cycodmd src\**\*.cs | cycod --input-chat-history "claude-code-tutorial-summary.jsonl" --input - "See the feature summary for claude code? I want you to make a point by point comparison/analysis for claude code vs the cycod codebase you see here. At the end list the tasks to update the cycod codebase to have all the features that claude code has."
- More explicit about I/O with `--input-chat-history` and `--output-chat-history` parameters
- Only has `/save` for saving chat history
  - Chat history persistence `--input-chat-history` and `--output-chat-history`
    - ✅ Add chat history persistence (implemented with Save/LoadChatHistory)
````

## tests\cycod-yaml\config-set-boolean-values.yaml

Modified: 2 minutes ago
Size: 4 KB

```yaml
#     - name: Set App.AutoSaveChatHistory to false
#       run: cycod config set App.AutoSaveChatHistory false --local
#         App.AutoSaveChatHistory: false
#     - run: cycod config clear App.AutoSaveChatHistory --local
```

