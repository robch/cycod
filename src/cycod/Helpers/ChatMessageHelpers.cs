//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

public static class AIExtensionsChatHelpers
{
    public static ChatMessage? ChatMessageFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ChatMessage>(json, AIJsonUtilities.DefaultOptions);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error reading chat message from json: {ex.Message}");
            return null;
        }
    }

    public static string? AsJson(this ChatMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonlOptions);
    }



    /// <summary>
    /// Parses JSONL content with optional metadata support.
    /// </summary>
    /// <param name="jsonl">JSONL content to parse</param>
    /// <returns>Tuple of (metadata, messages). Metadata is null if not present.</returns>
    public static (ConversationMetadata? metadata, IList<ChatMessage> messages) ChatMessagesFromJsonl(string jsonl)
    {
        var lines = jsonl.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
        {
            return (null, new List<ChatMessage>());
        }

        // Try to parse metadata from first line
        var (metadata, messageStartIndex) = ConversationMetadataHelpers.TryParseMetadata(lines[0]);

        // Parse remaining lines as messages
        var messageLines = lines.Skip(messageStartIndex);
        var messages = new List<ChatMessage>();

        foreach (var line in messageLines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var message = ChatMessageFromJson(line);
            if (message != null)
            {
                messages.Add(message);
            }
        }

        return (metadata, messages);
    }

    /// <summary>
    /// Converts messages and metadata to JSONL format with metadata as first line.
    /// </summary>
    /// <param name="messages">Chat messages to serialize</param>
    /// <param name="metadata">Optional metadata to include. If null, no metadata line added.</param>
    /// <returns>JSONL string with optional metadata first line</returns>
    public static string AsJsonl(this IList<ChatMessage> messages, ConversationMetadata? metadata = null)
    {
        var lines = new List<string>();

        // Add metadata as first line if present
        if (metadata != null)
        {
            lines.Add(ConversationMetadataHelpers.SerializeMetadata(metadata));
        }

        // Add message lines
        var messageJsons = messages
            .Select(m => m.AsJson())
            .Where(m => !string.IsNullOrEmpty(m))
            .Select(m => m!);

        lines.AddRange(messageJsons);

        return string.Join('\n', lines);
    }

    /// <summary>
    /// Saves chat history to file with optional metadata.
    /// </summary>
    public static void SaveChatHistoryToFile(
        this IList<ChatMessage> messages, 
        string fileName, 
        ConversationMetadata? metadata,
        bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, 
        string? saveToFolderOnAccessDenied = null)
    {
        var jsonl = useOpenAIFormat
            ? messages.ToOpenAIChatMessages(_jsonlOptions).AsJsonl(metadata)
            : messages.AsJsonl(metadata);
            
        FileHelpers.WriteAllText(fileName, jsonl, saveToFolderOnAccessDenied);
    }

    public static void SaveTrajectoryToFile(this IList<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    {
        try
        {
            var trajectoryFile = new TrajectoryFile(fileName);
            foreach (var message in messages)
            {
                trajectoryFile.AppendMessage(message);
            }
        }
        catch (Exception)
        {
            var trySavingElsewhere = !string.IsNullOrEmpty(saveToFolderOnAccessDenied);
            if (trySavingElsewhere)
            {
                var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var trySavingToFolder = Path.Combine(userProfileFolder, saveToFolderOnAccessDenied!);

                var fileNameWithoutFolder = Path.GetFileName(fileName);
                fileName = Path.Combine(trySavingToFolder, fileNameWithoutFolder);

                SaveTrajectoryToFile(messages, fileName, useOpenAIFormat, null);
            }
        }
    }

    public static void FixDanglingToolCalls(this List<ChatMessage> messages)
    {
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            if (message.Role != ChatRole.Assistant) continue;
            
            var functionCallContents = message.Contents
                .OfType<FunctionCallContent>()
                .ToList();
            if (!functionCallContents.Any()) continue;
            
            foreach (var functionCall in functionCallContents)
            {
                if (!HasMatchingToolMessage(messages, functionCall.CallId, i))
                {
                    AddDummyToolMessage(messages, functionCall, ref i);
                }
            }
        }
    }

    private static bool HasMatchingToolMessage(List<ChatMessage> messages, string callId, int startIndex)
    {
        for (int j = startIndex + 1; j < messages.Count; j++)
        {
            var nextMessage = messages[j];
            if (nextMessage.Role != ChatRole.Tool) break;
            
            var hasMatchingToolContent = nextMessage.Contents
                .OfType<FunctionResultContent>()
                .Any(c => c.CallId == callId);
            if (hasMatchingToolContent) return true;
        }
        return false;
    }

    private static void AddDummyToolMessage(List<ChatMessage> messages, FunctionCallContent functionCall, ref int currentIndex)
    {
        ConsoleHelpers.WriteDebugLine($"Found dangling tool call ID {functionCall.CallId} for function {functionCall.Name}, adding dummy tool message");
        
        var dummyResult = $"{{\"result\": \"...unknown; tool not called...\"}}";
        var dummyToolContent = new FunctionResultContent(functionCall.CallId, dummyResult);
        messages.Insert(currentIndex + 1, new ChatMessage(ChatRole.Tool, new List<AIContent> { dummyToolContent }));
        
        currentIndex++;
    }

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

    public static bool TryTrimToTarget(this IList<ChatMessage> messages, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        var trimmedPrompt = maxPromptTokenTarget > 0 && messages.TryTrimPromptContentToTarget(maxPromptTokenTarget);
        var trimmedTool = maxToolTokenTarget > 0 && messages.TryTrimToolCallContentToTarget(maxToolTokenTarget);
        var trimmedChat = maxChatTokenTarget > 0 && messages.TryTrimChatToTarget(maxChatTokenTarget);
        return trimmedChat || trimmedPrompt || trimmedTool;
    }

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

    private static string? TrimUserPromptContent(string text, int maxPromptTokenTarget, string trimIndicator = "...snip...")
    {
        var cchTake = Math.Min(text.Length, maxPromptTokenTarget * ESTIMATED_BYTES_PER_TOKEN - trimIndicator.Length);
        return cchTake > 0
            ? text.Substring(0, cchTake) + trimIndicator
            : trimIndicator;
    }

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

    private static JsonSerializerOptions _jsonlOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private const int ESTIMATED_BYTES_PER_TOKEN = 4; // This is an estimate, actual bytes per token may vary
}

public static class OpenAIChatHelpers
{
    public static ChatRole ChatRoleDeveloper { get; } = new ChatRole("developer");

    public static OpenAI.Chat.ChatMessage? ChatMessageFromJson(string json)
    {
        try
        {
            var jsonObject = JsonDocument.Parse(json);
            if (!jsonObject.RootElement.TryGetProperty("role", out var roleObj))
            {
                return null;
            }

            var role = roleObj.GetString();
            var type = role?.ToLowerInvariant() switch
            {
                "user" => typeof(OpenAI.Chat.UserChatMessage),
                "assistant" => typeof(OpenAI.Chat.AssistantChatMessage),
                "system" => typeof(OpenAI.Chat.SystemChatMessage),
                "tool" => typeof(OpenAI.Chat.ToolChatMessage),
                _ => throw new Exception($"Unknown chat role {role}")
            };

            return ModelReaderWriter.Read(BinaryData.FromString(json), type, ModelReaderWriterOptions.Json) as OpenAI.Chat.ChatMessage;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error reading chat message from json: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Parses JSONL content with optional metadata support (OpenAI format).
    /// </summary>
    public static (ConversationMetadata? metadata, IList<OpenAI.Chat.ChatMessage> messages) ChatMessagesFromJsonl(string jsonl)
    {
        var lines = jsonl.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
        {
            return (null, new List<OpenAI.Chat.ChatMessage>());
        }

        // Try to parse metadata from first line
        var (metadata, messageStartIndex) = ConversationMetadataHelpers.TryParseMetadata(lines[0]);

        // Parse remaining lines as messages
        var messageLines = lines.Skip(messageStartIndex);
        var messages = new List<OpenAI.Chat.ChatMessage>();

        foreach (var line in messageLines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var message = ChatMessageFromJson(line);
            if (message != null)
            {
                messages.Add(message);
            }
        }

        return (metadata, messages);
    }

    public static string? AsJson(this OpenAI.Chat.ChatMessage message)
    {
        return message switch
        {
            OpenAI.Chat.UserChatMessage userMessage => ModelReaderWriter.Write(userMessage, ModelReaderWriterOptions.Json).ToString(),
            OpenAI.Chat.AssistantChatMessage assistantMessage => ModelReaderWriter.Write(assistantMessage, ModelReaderWriterOptions.Json).ToString(),
            OpenAI.Chat.SystemChatMessage systemMessage => ModelReaderWriter.Write(systemMessage, ModelReaderWriterOptions.Json).ToString(),
            OpenAI.Chat.ToolChatMessage toolMessage => ModelReaderWriter.Write(toolMessage, ModelReaderWriterOptions.Json).ToString(),
            _ => null
        };
    }

    /// <summary>
    /// Converts OpenAI messages and metadata to JSONL format with metadata as first line.
    /// </summary>
    public static string AsJsonl(this IEnumerable<OpenAI.Chat.ChatMessage> messages, ConversationMetadata? metadata = null)
    {
        var lines = new List<string>();

        // Add metadata as first line if present
        if (metadata != null)
        {
            lines.Add(ConversationMetadataHelpers.SerializeMetadata(metadata));
        }

        // Add message lines
        var messageJsons = messages
            .Select(m => m.AsJson())
            .Where(m => !string.IsNullOrEmpty(m))
            .Select(m => m!);

        lines.AddRange(messageJsons);

        return string.Join('\n', lines);
    }

    public static ChatMessage ToExtensionsAIChatMessage(this OpenAI.Chat.ChatMessage message)
    {
        var role = DetermineRole(message);
        var extensionsMessage = new ChatMessage { Role = role };
        
        SetParticipantNameIfAvailable(message, extensionsMessage);
        ConvertContentParts(message, extensionsMessage);
        HandleSpecialMessageTypes(message, extensionsMessage);
        
        return extensionsMessage;
    }

    private static ChatRole DetermineRole(OpenAI.Chat.ChatMessage message) => message switch
    {
        OpenAI.Chat.SystemChatMessage => ChatRole.System,
        OpenAI.Chat.UserChatMessage => ChatRole.User,
        OpenAI.Chat.AssistantChatMessage => ChatRole.Assistant,
        OpenAI.Chat.ToolChatMessage => ChatRole.Tool,
        OpenAI.Chat.DeveloperChatMessage => new ChatRole("developer"),
        _ => throw new Exception($"Unknown chat message type {message.GetType().Name}")
    };

    private static void SetParticipantNameIfAvailable(OpenAI.Chat.ChatMessage message, ChatMessage extensionsMessage)
    {
        if (message is OpenAI.Chat.UserChatMessage userMessage && !string.IsNullOrEmpty(userMessage.ParticipantName))
        {
            extensionsMessage.AuthorName = userMessage.ParticipantName;
        }
    }

    private static void ConvertContentParts(OpenAI.Chat.ChatMessage message, ChatMessage extensionsMessage)
    {
        foreach (var contentPart in message.Content)
        {
            if (contentPart.Kind == OpenAI.Chat.ChatMessageContentPartKind.Text)
            {
                extensionsMessage.Contents.Add(new TextContent(contentPart.Text));
            }
            else if (contentPart.Kind == OpenAI.Chat.ChatMessageContentPartKind.Image && contentPart.ImageUri != null)
            {
                extensionsMessage.Contents.Add(new UriContent(contentPart.ImageUri, contentPart.ImageBytesMediaType ?? "image/*"));
            }
            else if (contentPart.Kind == OpenAI.Chat.ChatMessageContentPartKind.Image && contentPart.ImageBytes != null)
            {
                extensionsMessage.Contents.Add(new DataContent(contentPart.ImageBytes.ToMemory(), contentPart.ImageBytesMediaType ?? "image/*"));
            }
        }
    }

    private static void HandleSpecialMessageTypes(OpenAI.Chat.ChatMessage message, ChatMessage extensionsMessage)
    {
        HandleToolMessage(message, extensionsMessage);
        HandleAssistantToolCalls(message, extensionsMessage);
    }

    private static void HandleToolMessage(OpenAI.Chat.ChatMessage message, ChatMessage extensionsMessage)
    {
        if (message is OpenAI.Chat.ToolChatMessage toolMessage)
        {
            extensionsMessage.Contents.Add(new FunctionResultContent(toolMessage.ToolCallId, toolMessage.Content[0].Text));
        }
    }

    private static void HandleAssistantToolCalls(OpenAI.Chat.ChatMessage message, ChatMessage extensionsMessage)
    {
        if (message is OpenAI.Chat.AssistantChatMessage assistantMessage && assistantMessage.ToolCalls.Count > 0)
        {
            foreach (var toolCall in assistantMessage.ToolCalls)
            {
                if (toolCall.Kind == OpenAI.Chat.ChatToolCallKind.Function)
                {
                    var argsBinary = toolCall.FunctionArguments;
                    var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsBinary);
                    args ??= new Dictionary<string, object?>();
                    
                    extensionsMessage.Contents.Add(new FunctionCallContent(toolCall.Id, toolCall.FunctionName, args));
                }
            }
        }
    }

    public static IEnumerable<OpenAI.Chat.ChatMessage> ToOpenAIChatMessages(this IEnumerable<ChatMessage> inputs, JsonSerializerOptions options)
    {
        foreach (ChatMessage input in inputs)
        {
            if (input.Role == ChatRole.System ||
                input.Role == ChatRole.User ||
                input.Role == ChatRoleDeveloper)
            {
                var parts = ToOpenAIChatContent(input.Contents);
                yield return
                    input.Role == ChatRole.System ? new OpenAI.Chat.SystemChatMessage(parts) { ParticipantName = input.AuthorName } :
                    input.Role == ChatRoleDeveloper ? new OpenAI.Chat.DeveloperChatMessage(parts) { ParticipantName = input.AuthorName } :
                    new OpenAI.Chat.UserChatMessage(parts) { ParticipantName = input.AuthorName };
            }
            else if (input.Role == ChatRole.Tool)
            {
                foreach (AIContent item in input.Contents)
                {
                    if (item is FunctionResultContent resultContent)
                    {
                        string? result = resultContent.Result as string;
                        if (result is null && resultContent.Result is not null)
                        {
                            try
                            {
                                result = JsonSerializer.Serialize(resultContent.Result, options.GetTypeInfo(typeof(object)));
                            }
                            catch (NotSupportedException)
                            {
                                // If the type can't be serialized, skip it.
                            }
                        }

                        yield return new OpenAI.Chat.ToolChatMessage(resultContent.CallId, result ?? string.Empty);
                    }
                }
            }
            else if (input.Role == ChatRole.Assistant)
            {
                OpenAI.Chat.AssistantChatMessage message = new(ToOpenAIChatContent(input.Contents))
                {
                    ParticipantName = input.AuthorName
                };

                foreach (var content in input.Contents)
                {
                    if (content is FunctionCallContent callRequest)
                    {
                        message.ToolCalls.Add(
                            OpenAI.Chat.ChatToolCall.CreateFunctionToolCall(
                                callRequest.CallId,
                                callRequest.Name,
                                new(JsonSerializer.SerializeToUtf8Bytes(
                                    callRequest.Arguments,
                                    options.GetTypeInfo(typeof(IDictionary<string, object?>))))));
                    }
                }

                if (input.AdditionalProperties?.TryGetValue(nameof(message.Refusal), out string? refusal) is true)
                {
                    message.Refusal = refusal;
                }

                yield return message;
            }
        }
    }

    public static List<OpenAI.Chat.ChatMessageContentPart> ToOpenAIChatContent(IList<AIContent> contents)
    {
        List<OpenAI.Chat.ChatMessageContentPart> parts = [];
        foreach (var content in contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateTextPart(textContent.Text));
                    break;

                case UriContent uriContent when uriContent.HasTopLevelMediaType("image"):
                    parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateImagePart(uriContent.Uri, GetImageDetail(content)));
                    break;

                case DataContent dataContent when dataContent.HasTopLevelMediaType("image"):
                    parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(dataContent.Data), dataContent.MediaType, GetImageDetail(content)));
                    break;

                case DataContent dataContent when dataContent.HasTopLevelMediaType("audio"):
                    var audioData = BinaryData.FromBytes(dataContent.Data);
                    if (dataContent.MediaType.Equals("audio/mpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateInputAudioPart(audioData, OpenAI.Chat.ChatInputAudioFormat.Mp3));
                    }
                    else if (dataContent.MediaType.Equals("audio/wav", StringComparison.OrdinalIgnoreCase))
                    {
                        parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateInputAudioPart(audioData, OpenAI.Chat.ChatInputAudioFormat.Wav));
                    }

                    break;
            }
        }

        if (parts.Count == 0)
        {
            parts.Add(OpenAI.Chat.ChatMessageContentPart.CreateTextPart(string.Empty));
        }

        return parts;
    }

    public static OpenAI.Chat.ChatImageDetailLevel? GetImageDetail(AIContent content)
    {
        if (content.AdditionalProperties?.TryGetValue("detail", out object? value) is true)
        {
            return value switch
            {
                string detailString => new OpenAI.Chat.ChatImageDetailLevel(detailString),
                OpenAI.Chat.ChatImageDetailLevel detail => detail,
                _ => null
            };
        }

        return null;
    }
}