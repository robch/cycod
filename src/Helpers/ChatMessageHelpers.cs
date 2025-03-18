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
            var messageText = message switch
            {
                UserChatMessage userMessage => ModelReaderWriter.Write(userMessage, ModelReaderWriterOptions.Json).ToString(),
                AssistantChatMessage assistantMessage => ModelReaderWriter.Write(assistantMessage, ModelReaderWriterOptions.Json).ToString(),
                FunctionChatMessage functionMessage => ModelReaderWriter.Write(functionMessage, ModelReaderWriterOptions.Json).ToString(),
                SystemChatMessage systemMessage => ModelReaderWriter.Write(systemMessage, ModelReaderWriterOptions.Json).ToString(),
                ToolChatMessage toolMessage => ModelReaderWriter.Write(toolMessage, ModelReaderWriterOptions.Json).ToString(),
                _ => null
            };

            if (!string.IsNullOrEmpty(messageText))
            {
                history.AppendLine(messageText);
            }
        }

        FileHelpers.WriteAllText(fileName, history.ToString());
    }

    public static void ReadChatHistoryFromFile(this List<ChatMessage> messages, string fileName)
    {
        var historyFile = FileHelpers.ReadAllText(fileName);

        var historyFileLines = historyFile.Split(Environment.NewLine);
        var clearIfSystem = () =>
        {
            messages.Clear();
            return typeof(SystemChatMessage);
        };

        foreach (var line in historyFileLines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            
            var jsonObject = JsonDocument.Parse(line);
            JsonElement roleObj;

            if (!jsonObject.RootElement.TryGetProperty("role", out roleObj))
            {
                continue;
            }

            var role = roleObj.GetString();

            var type = role?.ToLowerInvariant() switch
            {
                "user" => typeof(UserChatMessage),
                "assistant" => typeof(AssistantChatMessage),
                "function" => typeof(FunctionChatMessage),
                "system" => clearIfSystem(),
                "tool" => typeof(ToolChatMessage),
                _ => throw new Exception($"Unknown chat role {role}")
            };

            var message = ModelReaderWriter.Read(BinaryData.FromString(line), type, ModelReaderWriterOptions.Json) as ChatMessage;
            messages.Add(message!);
        }
    }
}
