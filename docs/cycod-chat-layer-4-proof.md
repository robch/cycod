# cycod chat - Layer 4: CONTENT REMOVAL - Source Code Proof

[← Back to Layer 4](cycod-chat-layer-4.md)

This document provides detailed source code evidence for all content removal mechanisms in the cycod chat command.

---

## 1. Token Management Implementation

### Entry Point: ChatCommand.ExecuteAsync

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 72-77**: Configuration loading for token targets
```csharp
// Transfer known settings to the command
var maxOutputTokens = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxOutputTokens).AsInt(defaultValue: 0);
if (maxOutputTokens > 0) MaxOutputTokens = maxOutputTokens;

MaxPromptTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxPromptTokens).AsInt(DefaultMaxPromptTokenTarget);
MaxToolTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxToolTokens).AsInt(DefaultMaxToolTokenTarget);
MaxChatTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxChatTokens).AsInt(DefaultMaxChatTokenTarget);
```

**Lines 1232-1234**: Default token target constants
```csharp
private const int DefaultMaxPromptTokenTarget = 50000;
private const int DefaultMaxToolTokenTarget = 50000;
private const int DefaultMaxChatTokenTarget = 160000;
```

**Lines 1197-1200**: Token target properties
```csharp
public int MaxPromptTokenTarget { get; set; }
public int MaxToolTokenTarget { get; set; }
public int? MaxOutputTokens { get; set; }
public int MaxChatTokenTarget { get; set; }
```

### Invocation 1: History Loading

**Lines 135-146**: Loading history with token limits
```csharp
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

**Call Stack**:
1. `ChatCommand.ExecuteAsync` (line 138)
2. → `Conversation.LoadFromFile` (line 66 in Conversation.cs)
3. → `Messages.TryTrimToTarget` (line 92 in Conversation.cs)
4. → `ChatMessageHelpers.TryTrimToTarget` (line 177 in ChatMessageHelpers.cs)

---

## 2. History Pruning on Load

### Conversation.LoadFromFile Implementation

**File**: `src/cycod/ChatClient/Conversation.cs`

**Lines 66-99**: Complete load and trim implementation
```csharp
public void LoadFromFile(string fileName, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
{
    // Read and parse file content directly
    var jsonl = FileHelpers.ReadAllText(fileName);
    var lines = jsonl.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    
    if (lines.Length == 0)
    {
        return; // Empty file, nothing to load
    }

    // Try to parse metadata from first line
    var (metadata, messageStartIndex) = ConversationMetadataHelpers.TryParseMetadata(lines[0]);

    // Parse remaining lines as messages
    var messageLines = lines.Skip(messageStartIndex);
    var messages = ParseLinesAsMessages(messageLines, useOpenAIFormat);
    
    // If loaded messages have system message, do complete replacement
    var hasSystemMessage = messages.Any(x => x.Role == ChatRole.System);
    if (hasSystemMessage)
    {
        Messages.Clear(); // Complete replacement - file contains full conversation
    }
    Messages.AddRange(messages);
    Messages.FixDanglingToolCalls();
    Messages.TryTrimToTarget(maxPromptTokenTarget, maxToolTokenTarget, maxChatTokenTarget);  // ← PRUNING HERE
    
    // Update metadata (use loaded metadata if present, otherwise keep current)
    if (metadata != null)
    {
        Metadata = metadata;
    }
}
```

**Key Operations**:
1. Line 70: Read file
2. Line 82: Parse messages
3. Line 88: Clear existing if system message present
4. Line 90: Add parsed messages
5. Line 91: Fix dangling tool calls
6. **Line 92: TRIM TO TOKEN TARGETS** ← Content removal happens here

---

## 3. TryTrimToTarget Implementation

### Main Trimming Entry Point

**File**: `src/cycod/Helpers/ChatMessageHelpers.cs`

**Lines 177-183**: Top-level trimming orchestration
```csharp
public static bool TryTrimToTarget(this IList<ChatMessage> messages, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0)
{
    var trimmedPrompt = maxPromptTokenTarget > 0 && messages.TryTrimPromptContentToTarget(maxPromptTokenTarget);
    var trimmedTool = maxToolTokenTarget > 0 && messages.TryTrimToolCallContentToTarget(maxToolTokenTarget);
    var trimmedChat = maxChatTokenTarget > 0 && messages.TryTrimChatToTarget(maxChatTokenTarget);
    return trimmedChat || trimmedPrompt || trimmedTool;
}
```

**Three Trimming Operations**:
1. **Prompt trimming**: Trims individual user prompt messages that are too large
2. **Tool trimming**: Trims individual tool result messages that are too large
3. **Chat trimming**: Removes old tool call content when entire chat is too large

---

## 4. Prompt Content Trimming

### TryTrimPromptContentToTarget

**Lines 185-205**: Trim oversized user prompts
```csharp
public static bool TryTrimPromptContentToTarget(this IList<ChatMessage> messages, int maxPromptTokenTarget)
{
    var didTrim = false;
    for (var i = 0; i < messages.Count; i++)
    {
        var promptChatMessage = messages[i].Role == ChatRole.User
            ? messages[i]
            : null;
        if (promptChatMessage != null && IsUserChatContentTooBig(promptChatMessage, maxPromptTokenTarget))
        {
            didTrim = true;
            ConsoleHelpers.WriteDebugLine($"Prompt content is too big, trimming to {maxPromptTokenTarget} tokens");
            messages[i] = new ChatMessage(ChatRole.User, promptChatMessage.Contents
                .Select(x => x is TextContent textContent
                    ? new TextContent(TrimUserPromptContent(textContent.Text, maxPromptTokenTarget))
                    : x)
                .ToList());
        }
    }
    return didTrim;
}
```

**Lines 276-288**: Size check for user prompts
```csharp
private static bool IsUserChatContentTooBig(ChatMessage userChatMessage, int maxPromptTokenTarget)
{
    var content = string.Join("", userChatMessage.Contents
        .Where(x => x is TextContent)
        .Cast<TextContent>()
        .Select(x => x.Text));
    if (string.IsNullOrEmpty(content)) return false;

    var isTooBig = content.Length > maxPromptTokenTarget * ESTIMATED_BYTES_PER_TOKEN;
    ConsoleHelpers.WriteDebugLine($"User chat content size: {content.Length}, max token size: {maxPromptTokenTarget}, is too big: {isTooBig}");

    return isTooBig;
}
```

**Lines 304-310**: Actual trimming logic
```csharp
private static string? TrimUserPromptContent(string text, int maxPromptTokenTarget, string trimIndicator = "...snip...")
{
    var cchTake = Math.Min(text.Length, maxPromptTokenTarget * ESTIMATED_BYTES_PER_TOKEN - trimIndicator.Length);
    return cchTake > 0
        ? text.Substring(0, cchTake) + trimIndicator
        : trimIndicator;
}
```

**Line 333**: Token estimation constant
```csharp
private const int ESTIMATED_BYTES_PER_TOKEN = 4; // This is an estimate, actual bytes per token may vary
```

**Behavior**:
- Checks each user message
- If content exceeds `maxPromptTokenTarget * 4 bytes`, trims it
- Replaces content with: `first N characters + "...snip..."`
- Preserves message structure, only trims text content

---

## 5. Tool Call Content Trimming

### TryTrimToolCallContentToTarget

**Lines 207-227**: Trim oversized tool results
```csharp
public static bool TryTrimToolCallContentToTarget(this IList<ChatMessage> messages, int maxToolTokenTarget)
{
    var didTrim = false;
    for (var i = 0; i < messages.Count; i++)
    {
        var toolChatMessage = messages[i].Role == ChatRole.Tool
            ? messages[i]
            : null;
        if (toolChatMessage != null && IsToolChatContentTooBig(toolChatMessage, maxToolTokenTarget))
        {
            didTrim = true;
            ConsoleHelpers.WriteDebugLine($"Tool call content is too big, trimming to {maxToolTokenTarget} tokens");
            messages[i] = new ChatMessage(ChatRole.Tool, toolChatMessage.Contents
                .Select(x => x is FunctionResultContent result
                    ? new FunctionResultContent(result.CallId, TrimFunctionResultContent(result.Result, maxToolTokenTarget))
                    : x)
                .ToList());
        }
    }
    return didTrim;
}
```

**Lines 290-302**: Size check for tool messages
```csharp
private static bool IsToolChatContentTooBig(ChatMessage toolChatMessage, int maxToolTokenTarget)
{
    var content = string.Join("", toolChatMessage.Contents
        .Where(x => x is FunctionResultContent)
        .Cast<FunctionResultContent>()
        .Select(x => x.Result));
    if (string.IsNullOrEmpty(content)) return false;

    var isTooBig = content.Length > maxToolTokenTarget * ESTIMATED_BYTES_PER_TOKEN;
    ConsoleHelpers.WriteDebugLine($"Tool call content size: {content.Length}, max token size: {maxToolTokenTarget}, is too big: {isTooBig}");

    return isTooBig;
}
```

**Lines 312-323**: Trim function result content
```csharp
private static object? TrimFunctionResultContent(object? result, int maxToolTokenTarget, string trimIndicator = "...snip...")
{
    if (result is string strResult)
    {
        var cchTake = Math.Min(strResult.Length, maxToolTokenTarget * ESTIMATED_BYTES_PER_TOKEN - trimIndicator.Length);
        return cchTake > 0
            ? strResult.Substring(0, cchTake) + trimIndicator
            : trimIndicator;
    }

    return result;
}
```

**Behavior**:
- Similar to prompt trimming but for tool results
- Trims `FunctionResultContent.Result` field
- Preserves `CallId` for correlation with function calls

---

## 6. Chat-Wide Trimming

### TryTrimChatToTarget

**Lines 229-243**: Replace old tool content when chat is too big
```csharp
public static bool TryTrimChatToTarget(this IList<ChatMessage> messages, int maxChatTokenTarget)
{
    if (maxChatTokenTarget <= 0) return false;

    const int whenTrimmingToolContentTarget = 10;
    const string snippedIndicator = "...snip...";

    if (messages.IsTooBig(maxChatTokenTarget))
    {
        messages.ReplaceTooBigToolCallContent(maxChatTokenTarget, whenTrimmingToolContentTarget, snippedIndicator);
        return true;
    }

    return false;
}
```

**Lines 156-175**: Check if messages exceed token budget
```csharp
public static bool IsTooBig(this IList<ChatMessage> messages, int maxChatTokenTarget)
{
    // Loop thru the messages and get the size of each message
    // and add them up to get the total size
    var totalBytes = 0;
    foreach (var message in messages)
    {
        var json = AsJson(message);
        if (string.IsNullOrEmpty(json)) continue;

        var jsonBytes = Encoding.UTF8.GetByteCount(json);
        totalBytes += jsonBytes;
    }

    var estimatedTotalTokens = totalBytes / ESTIMATED_BYTES_PER_TOKEN;
    var isTooBig = estimatedTotalTokens > maxChatTokenTarget;
    ConsoleHelpers.WriteDebugLine($"Total bytes: {totalBytes}, estimated tokens: {estimatedTotalTokens}, chat token target: {maxChatTokenTarget}, is too big: {isTooBig}");

    return isTooBig;
}
```

### ReplaceTooBigToolCallContent

**Lines 245-274**: Aggressive tool content replacement
```csharp
public static void ReplaceTooBigToolCallContent(this IList<ChatMessage> messages, int maxChatTokenTarget, int maxToolTokenTarget, string replaceToolCallContentWith)
{
    // If the total size of the messages is not too big, we don't need to do anything
    if (!messages.IsTooBig(maxChatTokenTarget)) return;

    // If assistant messages, there also won't be any tool calls
    var lastAssistantMessage = messages.LastOrDefault(x => x.Role == ChatRole.Assistant);
    if (lastAssistantMessage == null) return;

    // We're going to focus on the messages before the last assistant message
    var lastAssistantMessageIndex = messages.IndexOf(lastAssistantMessage);

    // Loop thru those messages and replace the content of tool calls with the replaceToolCallContentWith string
    // if the content is too big
    for (int i = 0; i < lastAssistantMessageIndex; i++)
    {
        var toolChatMessage = messages[i].Role == ChatRole.Tool
            ? messages[i]
            : null;
        if (toolChatMessage != null && IsToolChatContentTooBig(toolChatMessage, maxToolTokenTarget))
        {
            ConsoleHelpers.WriteDebugLine($"Tool call content is too big, replacing with: {replaceToolCallContentWith}");
            messages[i] = new ChatMessage(ChatRole.Tool, toolChatMessage.Contents
                .Select(x => x is FunctionResultContent result
                    ? new FunctionResultContent(result.CallId, replaceToolCallContentWith)
                    : x)
                .ToList());
        }
    }
}
```

**Strategy**:
1. Find last assistant message (most recent AI response)
2. Only process messages BEFORE that (older history)
3. Replace oversized tool results with `"...snip..."`
4. Keep recent messages intact for context continuity

---

## 7. Continuous Trimming During Chat

### HandleUpdateMessages

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 588-618**: Trimming after every message exchange
```csharp
private void HandleUpdateMessages(IList<ChatMessage> messages)
{
    messages.TryTrimToTarget(MaxPromptTokenTarget, MaxToolTokenTarget, MaxChatTokenTarget);  // ← TRIM ON EVERY UPDATE

    // Auto-save with metadata support
    TrySaveChatHistoryToFile(AutoSaveOutputChatHistory);
    if (OutputChatHistory != AutoSaveOutputChatHistory)
    {
        TrySaveChatHistoryToFile(OutputChatHistory);
    }
    
    // Update trajectory metadata to match conversation state
    SetTrajectoryMetadata(_currentChat?.Conversation.Metadata);
    
    // Generate title after first meaningful exchange (if enabled)
    var autoGenerateTitles = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoGenerateTitles).AsBool(defaultValue: true);
    var shouldGenerate = _currentChat?.Conversation.NeedsTitleGeneration() == true;
    
    ConsoleHelpers.WriteDebugLine($"Title generation check: attempted={_titleGenerationAttempted}, shouldGenerate={shouldGenerate}, autoGenerateTitles={autoGenerateTitles}, messageCount={messages.Count}");
    
    if (!_titleGenerationAttempted && shouldGenerate && autoGenerateTitles)
    {
        _titleGenerationAttempted = true;
        ConsoleHelpers.WriteDebugLine($"Triggering title generation for file: {AutoSaveOutputChatHistory}");
        _ = Task.Run(async () => await TryGenerateAndSaveTitle(AutoSaveOutputChatHistory));
    }
    
    var lastMessage = messages.LastOrDefault();
    _autoSaveTrajectoryFile.AppendMessage(lastMessage);
    _trajectoryFile.AppendMessage(lastMessage);
}
```

**Trigger Point**: Line 192 in ExecuteAsync
```csharp
var response = await CompleteChatStreamingAsync(chat, giveAssistant, imageFiles,
    (messages) => HandleUpdateMessages(messages),  // ← Called after assistant responds
    (update) => HandleStreamingChatCompletionUpdate(update),
    (name, args) => HandleFunctionCallApproval(factory, name, args!),
    (name, args, result) => HandleFunctionCallCompleted(name, args, result));
```

**Timing**: Trimming occurs AFTER each assistant response, BEFORE saving to file

---

## 8. Persistent Message Protection

### AddPersistentUserMessage

**File**: `src/cycod/ChatClient/Conversation.cs`

**Lines 245-258**: Persistent messages survive trimming
```csharp
public void AddPersistentUserMessage(string userMessage, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
{
    var message = new ChatMessage(ChatRole.User, userMessage);
    _persistentUserMessages.Add(message);  // ← Stored in separate list
    Messages.Add(message);
    
    // Trim both the persistent cache and the main messages
    _persistentUserMessages.TryTrimToTarget(
        maxPromptTokenTarget: maxPromptTokenTarget,
        maxChatTokenTarget: maxChatTokenTarget);
    Messages.TryTrimToTarget(
        maxPromptTokenTarget: maxPromptTokenTarget,
        maxChatTokenTarget: maxChatTokenTarget);
}
```

**Key Insight**: Persistent messages are stored in `_persistentUserMessages` list, which is NOT cleared during history pruning operations. They are trimmed independently.

**Usage**: Messages added via `--add-user-prompt` use this mechanism (see ChatCommand.cs line 129-132)

---

## 9. Image Pattern Clearing

### Image Resolution and Clearing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 185-186**: Clear patterns after use
```csharp
var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
ImagePatterns.Clear();  // ← REMOVE patterns after resolving
```

**Context** (lines 169-192): Full flow
```csharp
var (skipAssistant, replaceUserPrompt) = await TryHandleChatCommandAsync(chat, userPrompt);
if (skipAssistant) continue; // Some chat commands don't require a response from the assistant.

var shouldReplaceUserPrompt = !string.IsNullOrEmpty(replaceUserPrompt);
if (shouldReplaceUserPrompt) DisplayPromptReplacement(userPrompt, replaceUserPrompt);

var giveAssistant = shouldReplaceUserPrompt ? replaceUserPrompt! : userPrompt;

// Check for notifications before assistant response
if (chat.Notifications.HasPending())
{
    ConsoleHelpers.WriteLine("", overrideQuiet: true);
    CheckAndShowPendingNotifications(chat);
}
DisplayAssistantLabel();

var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
ImagePatterns.Clear();  // ← Patterns cleared here

var response = await CompleteChatStreamingAsync(chat, giveAssistant, imageFiles,
    (messages) => HandleUpdateMessages(messages),
    (update) => HandleStreamingChatCompletionUpdate(update),
    (name, args) => HandleFunctionCallApproval(factory, name, args!),
    (name, args, result) => HandleFunctionCallCompleted(name, args, result));
```

**Purpose**: Prevent images from being re-attached to subsequent messages in the conversation loop

**Property Declaration** (line 1216):
```csharp
public List<string> ImagePatterns = new();
```

**CLI Option** (CycoDevCommandLineOptions.cs lines 485-489):
```csharp
else if (arg == "--image")
{
    var imageArgs = GetInputOptionArgs(i + 1, args);
    var imagePatterns = ValidateStrings(arg, imageArgs, "image pattern");
    command.ImagePatterns.AddRange(imagePatterns);
    i += imageArgs.Count();
}
```

---

## 10. Template Conditionals

### Template Processing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 218-226**: Ground system prompt with template processing
```csharp
private string GroundSystemPrompt()
{
    SystemPrompt ??= GetBuiltInSystemPrompt();
    SystemPrompt = GroundPromptName(SystemPrompt);
    SystemPrompt = GroundSlashPrompt(SystemPrompt);

    var processed = ProcessTemplate(SystemPrompt + "\n\n" + GetSystemPromptAdds());  // ← Template processing
    return _namedValues != null ? processed.ReplaceValues(_namedValues) : processed;
}
```

**Base Class Template Processing** (CommandWithVariables.cs):

The `ProcessTemplate` method is inherited from `CommandWithVariables` which provides Handlebars-style template processing including conditionals:
- `{{#if variable}}...{{/if}}`: Include content if variable is truthy
- `{{#unless variable}}...{{/unless}}`: Include content if variable is falsy
- `{{#each collection}}...{{/each}}`: Loop over collections

**Content Removal Aspect**: 
- When a conditional evaluates to false, its content block is REMOVED from the output
- This is implicit content removal - no explicit CLI flag, but achieved through template variables

**Example Flow**:
```
Input template: "Debug mode: {{#if debug}}ENABLED{{/if}}{{#unless debug}}DISABLED{{/unless}}"
With --var debug=true  → "Debug mode: ENABLED"
With --var debug=false → "Debug mode: DISABLED"
Without --var debug    → "Debug mode: DISABLED"
```

---

## 11. Call Stack Summary

### Complete Trimming Flow

```
User runs: cycod --continue --input "Hello"

1. Main()
2. → CommandLineOptions.Parse()
3.   → Creates ChatCommand with options
4. → ChatCommand.ExecuteAsync()
5.   → Line 80: InputChatHistory = GroundInputChatHistory() 
6.   → Line 138: chat.Conversation.LoadFromFile(InputChatHistory, ...)
7.     → Conversation.cs:66: LoadFromFile()
8.       → Line 82: messages = ParseLinesAsMessages(...)
9.       → Line 90: Messages.AddRange(messages)
10.      → Line 92: Messages.TryTrimToTarget(max..., max..., max...)
11.        → ChatMessageHelpers.cs:177: TryTrimToTarget()
12.          → Line 179: TryTrimPromptContentToTarget()
13.            → Line 193: IsUserChatContentTooBig() [check]
14.            → Line 197: TrimUserPromptContent() [if too big]
15.          → Line 180: TryTrimToolCallContentToTarget()
16.            → Line 215: IsToolChatContentTooBig() [check]
17.            → Line 221: TrimFunctionResultContent() [if too big]
18.          → Line 181: TryTrimChatToTarget()
19.            → Line 236: IsTooBig() [check total size]
20.            → Line 238: ReplaceTooBigToolCallContent() [if too big]
21. → Line 188: CompleteChatStreamingAsync() [user interacts]
22.   → After assistant responds:
23.   → Line 189: HandleUpdateMessages(messages)
24.     → Line 590: messages.TryTrimToTarget(...) [TRIM AGAIN]
25.     → Line 593: TrySaveChatHistoryToFile() [save trimmed history]
```

**Key Points**:
- Trimming occurs at LEAST twice: on load (step 10) and after each exchange (step 24)
- Each trim operation checks prompt size, tool size, and total chat size
- Old content is aggressively replaced with "...snip..." to stay within budget
- Persistent messages (from `--add-user-prompt`) are protected

---

## 12. Configuration Options

### Known Settings

**File**: `src/common/Configuration/KnownSettings.cs`

Token-related settings:
```csharp
public static readonly string AppMaxPromptTokens = "App.MaxPromptTokens";
public static readonly string AppMaxToolTokens = "App.MaxToolTokens";
public static readonly string AppMaxChatTokens = "App.MaxChatTokens";
public static readonly string AppMaxOutputTokens = "App.MaxOutputTokens";
```

**Usage**:
```bash
# Set via config
cycod config set App.MaxChatTokens 100000

# Set via CLI (if KnownSettingsCLIParser supports it)
cycod --max-chat-tokens 100000

# Check current settings
cycod config list | grep Token
```

---

## Summary

The chat command implements comprehensive content removal through:

1. **Automatic Token Management**: 
   - Default limits: 50K prompt, 50K tool, 160K chat
   - Configurable via `App.Max*Tokens` settings
   
2. **Three-Level Trimming**:
   - Individual prompts trimmed to `MaxPromptTokenTarget`
   - Individual tool results trimmed to `MaxToolTokenTarget`
   - Entire chat trimmed to `MaxChatTokenTarget`

3. **Smart Strategies**:
   - Recent content preserved (focus trimming on older messages)
   - Tool results aggressively replaced with `"...snip..."`
   - Persistent messages protected from removal

4. **Implicit Removal**:
   - Template conditionals remove false branches
   - Image patterns cleared after use
   
5. **Continuous Operation**:
   - Trimming on history load
   - Trimming after every assistant response
   - Ensures constraints maintained throughout conversation

**Missing**: No explicit user-facing CLI options like `--remove-messages-matching` or `--keep-only-last-n` found in other tools.

---

[← Back to Layer 4](cycod-chat-layer-4.md) | [View Layer Implementation](cycod-chat-layer-4.md)
