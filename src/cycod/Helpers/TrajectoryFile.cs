using System.Text;
using Microsoft.Extensions.AI;

public class TrajectoryFile
{
    /// <summary>
    /// Conversation metadata to include in trajectory header.
    /// When set, triggers metadata header update.
    /// </summary>
    public ConversationMetadata? Metadata
    {
        get => _metadata;
        set
        {
            _metadata = value;
            if (_metadata != null && !string.IsNullOrEmpty(_filePath))
            {
                UpdateMetadataHeader();
            }
        }
    }

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

        // Ensure metadata header exists before writing first message
        EnsureMetadataHeaderExists();

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

    /// <summary>
    /// Updates the metadata header in the trajectory file.
    /// Rewrites the entire file with new metadata + existing content.
    /// </summary>
    private void UpdateMetadataHeader()
    {
        if (string.IsNullOrEmpty(_filePath) || _metadata == null)
            return;

        try
        {
            // Read existing content (skip old metadata header if present)
            var existingContent = ReadContentWithoutMetadata();
            
            // Create new header + existing content
            var header = CreateMetadataHeader();
            var fullContent = header + existingContent;
            
            FileHelpers.WriteAllText(_filePath, fullContent);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to update trajectory metadata: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a metadata header string for the trajectory file.
    /// </summary>
    /// <returns>Formatted metadata header</returns>
    private string CreateMetadataHeader()
    {
        if (_metadata == null) return string.Empty;

        var header = new StringBuilder();
        
        // Always add title line (show "Untitled" if no title set)
        var title = string.IsNullOrEmpty(_metadata.Title) ? "Untitled" : _metadata.Title;
        header.AppendLine($"# Title: \"{title}\"");
        
        // Add lock status
        var lockStatus = _metadata.IsTitleLocked ? "Locked" : "Unlocked";
        header.AppendLine($"**Status:** {lockStatus}");
        
        // Add separator
        header.AppendLine();
        header.AppendLine("---");
        header.AppendLine();
        
        return header.ToString();
    }

    /// <summary>
    /// Reads trajectory file content excluding any existing metadata header.
    /// </summary>
    /// <returns>Content without metadata header</returns>
    private string ReadContentWithoutMetadata()
    {
        if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            return string.Empty;

        var lines = File.ReadAllLines(_filePath);
        
        // Skip metadata header if present (look for "# Title:" and "---" separator)
        int contentStartIndex = 0;
        bool foundTitle = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("# Title:"))
            {
                foundTitle = true;
            }
            else if (foundTitle && lines[i].Trim() == "---")
            {
                // Skip the separator and any empty lines after it
                contentStartIndex = i + 1;
                while (contentStartIndex < lines.Length && string.IsNullOrWhiteSpace(lines[contentStartIndex]))
                {
                    contentStartIndex++;
                }
                break;
            }
        }
        
        if (contentStartIndex < lines.Length)
        {
            return string.Join(Environment.NewLine, lines.Skip(contentStartIndex)) + Environment.NewLine;
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Ensures metadata header is written before first message.
    /// </summary>
    private void EnsureMetadataHeaderExists()
    {
        if (_metadata != null && !string.IsNullOrEmpty(_filePath) && !_metadataHeaderWritten)
        {
            // Check if file is empty or needs metadata header
            if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
            {
                var header = CreateMetadataHeader();
                FileHelpers.WriteAllText(_filePath, header);
            }
            _metadataHeaderWritten = true;
        }
    }

    private readonly string? _filePath;
    private readonly bool _backTickFunctionCalls;
    private readonly bool _cacheFunctionCallsUntilResults;
    private readonly Dictionary<string, FunctionCallContent> _pendingCallsById;
    private ConversationMetadata? _metadata;
    private bool _metadataHeaderWritten;

}