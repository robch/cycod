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

    public static IList<ChatMessage> ChatMessagesFromJsonl(string jsonl)
    {
        var messages = new List<ChatMessage>();

        var lines = jsonl.Split(new [] { '\n', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var message = ChatMessageFromJson(line);
            if (message == null) continue;

            messages.Add(message);
        }

        return messages;
    }

    public static string? AsJson(this ChatMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonlOptions);
    }

    public static string AsJsonl(this IList<ChatMessage> messages)
    {
        var asJsonList = messages
            .Select(m => m.AsJson())
            .Where(m => !string.IsNullOrEmpty(m))
            .Select(m => m!)
            .ToList();
        var history = string.Join('\n', asJsonList);
        return history;
    }

    public static void SaveChatHistoryToFile(this IList<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
    {
        var jsonl = useOpenAIFormat
            ? messages.ToOpenAIChatMessages(_jsonlOptions).AsJsonl()
            : messages.AsJsonl();
        FileHelpers.WriteAllText(fileName, jsonl);
    }

    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
    {
        var jsonl = FileHelpers.ReadAllText(fileName);
        var newMessages = useOpenAIFormat
            ? OpenAIChatHelpers.ChatMessagesFromJsonl(jsonl).ToExtensionsAIChatMessages()
            : ChatMessagesFromJsonl(jsonl);

        var hasSystemMessage = newMessages.Any(x => x.Role == ChatRole.System);
        if (hasSystemMessage) messages.Clear();

        messages.AddRange(newMessages);
    }

    public static void AppendMessageToTrajectoryFile(this ChatMessage message, string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        
        var trajectoryContent = TrajectoryFormatter.FormatMessage(message);
        if (!string.IsNullOrEmpty(trajectoryContent))
        {
            FileHelpers.AppendAllText(fileName, trajectoryContent);
        }
    }

    public static bool IsTooBig(this IList<ChatMessage> messages, int trimTokenTarget)
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
        var isTooBig = estimatedTotalTokens > trimTokenTarget;
        ConsoleHelpers.WriteDebugLine($"Total bytes: {totalBytes}, estimated tokens: {estimatedTotalTokens}, trim target: {trimTokenTarget}, is too big: {isTooBig}");

        return isTooBig;
    }

    public static bool TryTrimToTarget(this IList<ChatMessage> messages, int trimTokenTarget)
    {
        if (trimTokenTarget <= 0) return false;

        const int whenTrimmingToolContentTarget = 10;
        const string snippedIndicator = "...snip...";

        if (messages.IsTooBig(trimTokenTarget))
        {
            messages.ReduceToolCallContent(trimTokenTarget, whenTrimmingToolContentTarget, snippedIndicator);
            return true;
        }

        return false;
    }

    public static void ReduceToolCallContent(this IList<ChatMessage> messages, int trimTokenTarget, int maxToolCallContentTokens, string replaceToolCallContentWith)
    {
        // If the total size of the messages is not too big, we don't need to do anything
        if (!messages.IsTooBig(trimTokenTarget)) return;

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
            if (toolChatMessage != null && IsTooBig(toolChatMessage, maxToolCallContentTokens))
            {
                ConsoleHelpers.WriteDebugLine($"Tool call content is too big, replacing with: {replaceToolCallContentWith}");
                messages[i] = new ChatMessage(ChatRole.Tool, replaceToolCallContentWith);
            }
        }

    }

    private static bool IsTooBig(ChatMessage toolChatMessage, int maxToolCallContentTokens)
    {
        var content = string.Join("", toolChatMessage.Contents
            .Where(x => x is TextContent)
            .Cast<TextContent>()
            .Select(x => x.Text));
        if (string.IsNullOrEmpty(content)) return false;

        var isTooBig = content.Length > maxToolCallContentTokens;
        ConsoleHelpers.WriteDebugLine($"Tool call content size: {content.Length}, max size: {maxToolCallContentTokens}, is too big: {isTooBig}");

        return isTooBig;
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

    public static IList<OpenAI.Chat.ChatMessage> ChatMessagesFromJsonl(string jsonl)
    {
        var messages = new List<OpenAI.Chat.ChatMessage>();

        var lines = jsonl.Split(new [] { '\n', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var message = ChatMessageFromJson(line);
            if (message == null) continue;

            messages.Add(message);
        }

        return messages;
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

    public static string AsJsonl(this IEnumerable<OpenAI.Chat.ChatMessage> messages)
    {
        var asJsonList = messages
            .Select(m => m.AsJson())
            .Where(m => !string.IsNullOrEmpty(m))
            .Select(m => m!)
            .ToList();
        var history = string.Join('\n', asJsonList);
        return history;
    }

    public static IEnumerable<ChatMessage> ToExtensionsAIChatMessages(this IEnumerable<OpenAI.Chat.ChatMessage> messages)
    {
        return messages.Select(x => x.ToExtensionsAIChatMessage());
    }

    public static ChatMessage ToExtensionsAIChatMessage(this OpenAI.Chat.ChatMessage message)
    {
        // Determine the role
        var role = message switch
        {
            OpenAI.Chat.SystemChatMessage => ChatRole.System,
            OpenAI.Chat.UserChatMessage => ChatRole.User,
            OpenAI.Chat.AssistantChatMessage => ChatRole.Assistant,
            OpenAI.Chat.ToolChatMessage => ChatRole.Tool,
            OpenAI.Chat.DeveloperChatMessage => new ChatRole("developer"),
            _ => throw new Exception($"Unknown chat message type {message.GetType().Name}")
        };

        // Create the message
        var extensionsMessage = new ChatMessage { Role = role };

        // Add the participant name if available
        if (message is OpenAI.Chat.UserChatMessage userMessage && !string.IsNullOrEmpty(userMessage.ParticipantName))
        {
            extensionsMessage.AuthorName = userMessage.ParticipantName;
        }

        // Add the content
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

        // Handle tool message case specifically
        if (message is OpenAI.Chat.ToolChatMessage toolMessage)
        {
            extensionsMessage.Contents.Add(new FunctionResultContent(toolMessage.ToolCallId, toolMessage.Content[0].Text));
        }

        // Handle assistant message with tool calls
        if (message is OpenAI.Chat.AssistantChatMessage assistantMessage && assistantMessage.ToolCalls.Count > 0)
        {
            foreach (var toolCall in assistantMessage.ToolCalls)
            {
                if (toolCall.Kind == OpenAI.Chat.ChatToolCallKind.Function)
                {
                    // Parse the function arguments
                    var argsBinary = toolCall.FunctionArguments;
                    var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsBinary);
                    args ??= new Dictionary<string, object?>();

                    // Create a function call content
                    extensionsMessage.Contents.Add(new FunctionCallContent(toolCall.Id, toolCall.FunctionName, args));
                }
            }
        }

        return extensionsMessage;
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