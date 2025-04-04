//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

public static class AIExtensionsChatHelpers
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

            var isSystemMessage = message.Role == ChatRole.System;
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

    private static string? JsonFromMessage(ChatMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonlOptions);
    }

    private static ChatMessage? MessageFromJson(string line)
    {
        try
        {
            return JsonSerializer.Deserialize<ChatMessage>(line, AIJsonUtilities.DefaultOptions);
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