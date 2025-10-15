using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

#pragma warning disable CS0618 // Type or member is obsolete

public static class TrajectoryFormatter
{
    public static string FormatUserInput(string? messageContent)
    {
        return FormatMessage("user", messageContent);
    }

    public static string FormatAssistantOutput(string? messageContent)
    {
        return FormatMessage("assistant", messageContent);
    }

    public static string FormatFunctionCall(string functionName, IDictionary<string, object?>? arguments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<function_calls>");
        sb.AppendLine($"<invoke name=\"{EscapeXml(functionName)}\">");

        try
        {
            arguments ??= new Dictionary<string, object?>();
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

    public static string FormatFunctionResult(string result)
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

    private static string FormatMessage(string role, string? content)
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

    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
    
    private static string SurroundMultiLineWithLFs(string text)
    {
        if (text.Contains('\n') && !text.StartsWith('\n'))
        {
            return $"\n{text}\n";
        }
        
        return text;
    }
}