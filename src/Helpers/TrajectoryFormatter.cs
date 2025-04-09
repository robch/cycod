//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

#pragma warning disable CS0618 // Type or member is obsolete

public static class TrajectoryFormatter
{
    /// <summary>
    /// Formats a chat message into a trajectory format
    /// </summary>
    public static string? FormatMessage(ChatMessage message)
    {
        var sb = new StringBuilder();

        var messageContent = string.Join("", message.Contents
            .Where(x => x is TextContent)
            .Cast<TextContent>()
            .Select(x => x.Text));
        var hasContent = !string.IsNullOrWhiteSpace(messageContent);

        var isUserMessage = message.Role == ChatRole.User;
        if (isUserMessage && hasContent)
        {
            sb.Append(FormatMessage("user", messageContent));
        }

        var assistantMessage = message.Role == ChatRole.Assistant ? message : null;
        var isAssistantMessage = assistantMessage != null && hasContent;
        if (isAssistantMessage)
        {
            sb.Append(FormatMessage("assistant", messageContent));
            var toolCalls = assistantMessage!.Contents
                .Where(x => x is FunctionCallContent)
                .Cast<FunctionCallContent>()
                .ToList();
            var hasToolCalls = toolCalls != null && toolCalls.Count > 0;
            if (hasToolCalls)
            {
                foreach (var toolCall in toolCalls!)
                {
                    var functionName = toolCall.Name;
                    var functionArguments = toolCall?.Arguments?.Count > 0
                        ? toolCall!.Arguments
                        : new Dictionary<string, object?>();
                    sb.Append(FormatToolCall(functionName, functionArguments));
                }
            }
        }

        var toolMessage = message.Role == ChatRole.Tool ? message : null;
        var isToolMessage = toolMessage != null;
        if (isToolMessage)
        {
            var functionResults = toolMessage!.Contents
                .Where(x => x is FunctionResultContent)
                .Cast<FunctionResultContent>()
                .ToList();
                
            foreach (var result in functionResults)
            {
                var resultContent = result.Result?.ToString() ?? string.Empty;
                sb.Append(FormatToolResult(resultContent));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format a tool call and its parameters into XML format
    /// </summary>
    private static string FormatToolCall(string functionName, IDictionary<string, object?> arguments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<function_calls>");
        sb.AppendLine($"<invoke name=\"{EscapeXml(functionName)}\">");

        try
        {
            foreach (var kvp in arguments)
            {
                var escapedValue = EscapeXml(kvp.Value?.ToString() ?? string.Empty);
                sb.AppendLine($"<parameter name=\"{EscapeXml(kvp.Key)}\">{SurroundMultiLineWithLFs(escapedValue)}</parameter>");
            }
        }
        catch (Exception)
        {
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

        if (role == "user")
        {
            content = string.Join("\n", content
                .Split(new[] { '\n' }, StringSplitOptions.None)
                .Select(line => line.TrimEnd(' ', '\r', '\t'))
                .Select(line => $"> {line}"));
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