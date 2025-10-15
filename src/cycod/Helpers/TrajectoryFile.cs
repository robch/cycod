using System.Text;
using Microsoft.Extensions.AI;

public class TrajectoryFile
{
    public TrajectoryFile(string? filePath, bool cacheFunctionCallsUntilResults = true, bool backTickFunctionCalls = true)
    {
        _filePath = filePath;
        _backTickFunctionCalls = backTickFunctionCalls;

        _cacheFunctionCallsUntilResults = cacheFunctionCallsUntilResults;
        _pendingCallsById = new Dictionary<string, FunctionCallContent>();
    }

    public void AppendMessage(ChatMessage? message, bool cacheFunctionCallsUntilResults = true)
    {
        if (message == null) return;
        if (string.IsNullOrEmpty(_filePath)) return;

        var textContent = string.Join("", message.Contents
            .Where(x => x is TextContent)
            .Cast<TextContent>()
            .Select(x => x.Text));
        var hasTextContent = !string.IsNullOrWhiteSpace(textContent);

        var isUserMessage = message.Role == ChatRole.User;
        var isAssistantMessage = message.Role == ChatRole.Assistant;
        var isToolMessage = message.Role == ChatRole.Tool;

        if (isUserMessage && hasTextContent)
        {
            AppendUserContent(textContent);
        }
        else if (isAssistantMessage)
        {
            AppendAssistantContent(textContent, message.Contents);
        }
        else if (isToolMessage)
        {
            AppendToolContent(message.Contents);
        }
    }

    private void AppendUserContent(string text)
    {
        if (string.IsNullOrEmpty(_filePath)) return;

        var formatted = TrajectoryFormatter.FormatUserInput(text);
        FileHelpers.AppendAllText(_filePath!, formatted);
    }

    private void AppendAssistantContent(string text, IList<AIContent> aiContents)
    {
        if (string.IsNullOrEmpty(_filePath)) return;

        var functionCallContents = aiContents
            .Where(x => x is FunctionCallContent)
            .Cast<FunctionCallContent>()
            .ToList();

        var sbFunctionCalls = new StringBuilder();
        foreach (var call in functionCallContents)
        {
            if (_cacheFunctionCallsUntilResults)
            {
                _pendingCallsById[call.CallId] = call;
                continue;
            }

            sbFunctionCalls.Append(TrajectoryFormatter.FormatFunctionCall(call.Name, call.Arguments));
        }

        var formattedFunctionCalls = _backTickFunctionCalls
            ? WrapWithBackTicks(sbFunctionCalls.ToString())
            : sbFunctionCalls.ToString();

        var formatted = TrajectoryFormatter.FormatAssistantOutput(text);
        var withFunctionCalls = formatted + formattedFunctionCalls;
        FileHelpers.AppendAllText(_filePath!, withFunctionCalls);
    }

    private void AppendToolContent(IList<AIContent> aiContents)
    {
        if (string.IsNullOrEmpty(_filePath)) return;

        var functionResultContents = aiContents
            .Where(x => x is FunctionResultContent)
            .Cast<FunctionResultContent>()
            .ToList();

        var sb = new StringBuilder();
        foreach (var result in functionResultContents)
        {
            var call = _pendingCallsById[result.CallId];
            var callFormatted = TrajectoryFormatter.FormatFunctionCall(call.Name, call.Arguments);
            var resultFormatted = TrajectoryFormatter.FormatFunctionResult(result.Result?.ToString() ?? string.Empty);
            sb.Append(callFormatted + resultFormatted);
        }

        var formattedMessage = _backTickFunctionCalls
            ? WrapWithBackTicks(sb.ToString())
            : sb.ToString();

        FileHelpers.AppendAllText(_filePath!, formattedMessage);
    }

    private string WrapWithBackTicks(string xml, string? lang = "xml")
    {
        return MarkdownHelpers.GetCodeBlock(xml, lang: lang, leadingLF: true);
    }

    private readonly string? _filePath;
    private readonly bool _backTickFunctionCalls;
    private readonly bool _cacheFunctionCallsUntilResults;
    private readonly Dictionary<string, FunctionCallContent> _pendingCallsById;

}