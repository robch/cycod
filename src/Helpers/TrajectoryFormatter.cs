//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Text;
using OpenAI.Chat;
using System.Text.Json;
using System.ClientModel.Primitives;

#pragma warning disable CS0618 // Type or member is obsolete

public static class TrajectoryFormatter
{
    /// <summary>
    /// Formats a chat message into a trajectory format
    /// </summary>
    public static string? FormatMessage(ChatMessage message)
    {
        var sb = new StringBuilder();

        var isUserMessage = message is UserChatMessage;
        if (isUserMessage) return null;

        var messageContent = string.Join("", message.Content
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text));
        var hasContent = !string.IsNullOrWhiteSpace(messageContent);

        var assistantMessage = message as AssistantChatMessage;
        var isAssistantMessage = assistantMessage != null && hasContent;
        if (isAssistantMessage)
        {
            sb.Append(FormatMessage("assistant", messageContent));
            var toolCalls = assistantMessage!.ToolCalls;
            var hasToolCalls = toolCalls != null && toolCalls.Count > 0;
            if (hasToolCalls)
            {
                foreach (var toolCall in toolCalls!)
                {
                    var functionName = toolCall.FunctionName;
                    var functionArguments = toolCall.FunctionArguments.ToArray().Length > 0
                        ? toolCall.FunctionArguments.ToString()
                        : "{}";
                    sb.Append(FormatToolCall(functionName, functionArguments));
                }
            }
        }

        var toolMessage = message as ToolChatMessage;
        var isToolMessage = toolMessage != null && hasContent;
        if (isToolMessage)
        {
            sb.Append(FormatToolResult(messageContent));
        }

        var functionMessage = message as FunctionChatMessage;
        var isFunctionMessage = functionMessage != null && hasContent;
        if (isFunctionMessage)
        {
            sb.Append(FormatToolResult(messageContent));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format a tool call and its parameters into XML format
    /// </summary>
    private static string FormatToolCall(string functionName, string arguments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<function_calls>");
        sb.AppendLine($"<invoke name=\"{EscapeXml(functionName)}\">");

        // Parse arguments as JSON and add each parameter
        try
        {
            var argsDoc = JsonDocument.Parse(arguments);
            foreach (var property in argsDoc.RootElement.EnumerateObject())
            {
                var value = property.Value.ValueKind == JsonValueKind.String 
                    ? property.Value.GetString() 
                    : property.Value.ToString();
                
                var escapedValue = EscapeXml(value ?? string.Empty);
                sb.AppendLine($"<parameter name=\"{EscapeXml(property.Name)}\">{SurroundMultiLineWithLFs(escapedValue)}</parameter>");
            }
        }
        catch (JsonException)
        {
            // If parsing fails, add the whole arguments string as is
            sb.AppendLine($"<parameter name=\"arguments\">{EscapeXml(arguments)}</parameter>");
        }

        sb.AppendLine("</invoke>");
        sb.AppendLine("</function_calls>");
        return sb.ToString();
    }

    /// <summary>
    /// Format a tool result into XML format
    /// </summary>
    private static string FormatToolResult(string result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<function_results>");
        
        if (!string.IsNullOrWhiteSpace(result))
        {
            sb.AppendLine(EscapeXml(result.Trim()));
        }
        else
        {
            sb.AppendLine("<system>Tool ran without output or errors</system>");
        }
        
        sb.AppendLine("</function_results>");
        return sb.ToString();
    }

    /// <summary>
    /// Format an AI message into a trajectory format
    /// </summary>
    private static string FormatMessage(string role, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }
        
        return $"\n{content.Trim()}\n";
    }

    /// <summary>
    /// Escapes special characters for XML
    /// </summary>
    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
    
    /// <summary>
    /// Surrounds multi-line text with line feeds if needed
    /// </summary>
    private static string SurroundMultiLineWithLFs(string text)
    {
        if (text.Contains('\n') && !text.StartsWith('\n'))
        {
            return $"\n{text}\n";
        }
        
        return text;
    }
}