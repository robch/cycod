//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Text;
using System.Text.Json;
using System.ClientModel.Primitives;
using OpenAI.Chat;

#pragma warning disable CS0618 // Type or member is obsolete

public static class OpenAIChatHelpers
{
    public static void SaveChatHistoryToFile(this IList<ChatMessage> messages, string fileName)
    {
        var history = new StringBuilder();

        foreach (var message in messages)
        {
            var json = JsonFromMessage(message);
            if (!string.IsNullOrEmpty(json))
            {
                history.AppendLine(json);
            }
        }

        FileHelpers.WriteAllText(fileName, history.ToString());
    }

    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName)
    {
        var historyFile = FileHelpers.ReadAllText(fileName);

        var historyFileLines = historyFile.Split(Environment.NewLine);
        foreach (var line in historyFileLines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var message = MessageFromJson(line);
            if (message == null) continue;

            var isSystemMessage = message is SystemChatMessage;
            if (isSystemMessage) messages.Clear();

            messages.Add(message);
        }
    }

    public static bool IsTooBig(this IList<ChatMessage> messages, int trimTokenTarget)
    {
        // Loop thru the messages and get the size of each message
        // and add them up to get the total size
        var totalBytes = 0;
        foreach (var message in messages)
        {
            var json = JsonFromMessage(message);
            if (string.IsNullOrEmpty(json)) continue;

            var jsonBytes = Encoding.UTF8.GetByteCount(json);
            totalBytes += jsonBytes;
        }

        var estimatedTotalTokens = totalBytes / ESTIMATED_BYTES_PER_TOKEN;
        var isTooBig = estimatedTotalTokens > trimTokenTarget;
        ConsoleHelpers.WriteDebugLine($"Total bytes: {totalBytes}, estimated tokens: {estimatedTotalTokens}, trim target: {trimTokenTarget}, is too big: {isTooBig}");

        return isTooBig;
    }

    public static void ReduceToolCallContent(this IList<ChatMessage> messages, int trimTokenTarget, int maxToolCallContentTokens, string replaceToolCallContentWith)
    {
        // If the total size of the messages is not too big, we don't need to do anything
        if (!messages.IsTooBig(trimTokenTarget)) return;

        // If assistant messages, there also won't be any tool calls
        var lastAssistantMessage = messages.LastOrDefault(x => x is AssistantChatMessage);
        if (lastAssistantMessage == null) return;

        // We're going to focus on the messages before the last assistant message
        var lastAssistantMessageIndex = messages.IndexOf(lastAssistantMessage);

        // Loop thru those messages and replace the content of tool calls with the replaceToolCallContentWith string
        // if the content is too big
        for (int i = 0; i < lastAssistantMessageIndex; i++)
        {
            var toolChatMessage = messages[i] as ToolChatMessage;
            if (toolChatMessage != null && IsTooBig(toolChatMessage, maxToolCallContentTokens))
            {
                ConsoleHelpers.WriteDebugLine($"Tool call content is too big, replacing with: {replaceToolCallContentWith}");
                messages[i] = new ToolChatMessage(toolChatMessage!.ToolCallId, replaceToolCallContentWith);
            }
        }

    }

    private static bool IsTooBig(ToolChatMessage toolChatMessage, int maxToolCallContentTokens)
    {
        var content = string.Join("", toolChatMessage.Content
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text));

        var isTooBig = content.Length > maxToolCallContentTokens;
        ConsoleHelpers.WriteDebugLine($"Tool call content size: {content.Length}, max size: {maxToolCallContentTokens}, is too big: {isTooBig}");

        return isTooBig;
    }

    private static string? JsonFromMessage(ChatMessage message)
    {
        return message switch
        {
            UserChatMessage userMessage => ModelReaderWriter.Write(userMessage, ModelReaderWriterOptions.Json).ToString(),
            AssistantChatMessage assistantMessage => ModelReaderWriter.Write(assistantMessage, ModelReaderWriterOptions.Json).ToString(),
            FunctionChatMessage functionMessage => ModelReaderWriter.Write(functionMessage, ModelReaderWriterOptions.Json).ToString(),
            SystemChatMessage systemMessage => ModelReaderWriter.Write(systemMessage, ModelReaderWriterOptions.Json).ToString(),
            ToolChatMessage toolMessage => ModelReaderWriter.Write(toolMessage, ModelReaderWriterOptions.Json).ToString(),
            _ => null
        };
    }

    private static ChatMessage? MessageFromJson(string line)
    {
        try
        {
            var jsonObject = JsonDocument.Parse(line);
            if (!jsonObject.RootElement.TryGetProperty("role", out var roleObj))
            {
                return null;
            }

            var role = roleObj.GetString();
            var type = role?.ToLowerInvariant() switch
            {
                "user" => typeof(UserChatMessage),
                "assistant" => typeof(AssistantChatMessage),
                "function" => typeof(FunctionChatMessage),
                "system" => typeof(SystemChatMessage),
                "tool" => typeof(ToolChatMessage),
                _ => throw new Exception($"Unknown chat role {role}")
            };

            return ModelReaderWriter.Read(BinaryData.FromString(line), type, ModelReaderWriterOptions.Json) as ChatMessage;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error reading chat message from json: {ex.Message}");
            return null;
        }
    }

    private const int ESTIMATED_BYTES_PER_TOKEN = 4; // This is an estimate, actual bytes per token may vary
    
    public static void AppendMessageToTrajectoryFile(this ChatMessage message, string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        
        var trajectoryContent = TrajectoryFormatter.FormatMessage(message);
        if (!string.IsNullOrEmpty(trajectoryContent))
        {
            FileHelpers.AppendAllText(fileName, trajectoryContent);
        }
    }
}
