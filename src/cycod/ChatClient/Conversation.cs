using Microsoft.Extensions.AI;

/// <summary>
/// Represents a conversation with metadata and message history.
/// Encapsulates conversation data and related operations.
/// </summary>
public class Conversation
{
    /// <summary>
    /// Metadata for the current conversation.
    /// </summary>
    public ConversationMetadata? Metadata { get; private set; }

    /// <summary>
    /// The conversation messages.
    /// </summary>
    public List<ChatMessage> Messages { get; private set; }

    /// <summary>
    /// Initializes a new conversation with empty messages.
    /// </summary>
    public Conversation()
    {
        Messages = new List<ChatMessage>();
    }

    /// <summary>
    /// Updates the conversation metadata.
    /// </summary>
    /// <param name="metadata">New metadata to set</param>
    public void UpdateMetadata(ConversationMetadata? metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Loads conversation history from file, including metadata and messages.
    /// </summary>
    /// <param name="fileName">The file to load from</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxToolTokenTarget">Maximum tool tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    /// <param name="useOpenAIFormat">Whether to use OpenAI format</param>
    public void LoadFromFile(string fileName, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
    {
        // Load messages and metadata
        var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFile(fileName, useOpenAIFormat);
        
        // Store metadata, create default if missing
        Metadata = metadata ?? ConversationMetadataHelpers.CreateDefault();

        // Clear and repopulate messages
        var hasSystemMessage = messages.Any(x => x.Role == ChatRole.System);
        if (hasSystemMessage) Messages.Clear();

        Messages.AddRange(messages);
        Messages.FixDanglingToolCalls();
        Messages.TryTrimToTarget(maxPromptTokenTarget, maxToolTokenTarget, maxChatTokenTarget);
    }

    /// <summary>
    /// Saves conversation history to file with metadata.
    /// </summary>
    /// <param name="fileName">The file to save to</param>
    /// <param name="useOpenAIFormat">Whether to use OpenAI format</param>
    /// <param name="saveToFolderOnAccessDenied">Fallback folder if access denied</param>
    public void SaveToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    {
        // Initialize metadata if not present
        Metadata ??= ConversationMetadataHelpers.CreateDefault();

        // Save with metadata
        Messages.SaveChatHistoryToFile(fileName, Metadata, useOpenAIFormat, saveToFolderOnAccessDenied);
    }

    /// <summary>
    /// Saves trajectory to file (messages only, no metadata).
    /// </summary>
    /// <param name="fileName">The file to save to</param>
    /// <param name="useOpenAIFormat">Whether to use OpenAI format</param>
    /// <param name="saveToFolderOnAccessDenied">Fallback folder if access denied</param>
    public void SaveTrajectoryToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    {
        Messages.SaveTrajectoryToFile(fileName, useOpenAIFormat, saveToFolderOnAccessDenied);
    }

    /// <summary>
    /// Adds a user message to the conversation.
    /// </summary>
    /// <param name="userMessage">The user message to add</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    public void AddUserMessage(string userMessage, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        Messages.Add(new ChatMessage(ChatRole.User, userMessage));
        Messages.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);
    }

    /// <summary>
    /// Adds multiple user messages to the conversation.
    /// </summary>
    /// <param name="userMessages">The user messages to add</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    public void AddUserMessages(IEnumerable<string> userMessages, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        foreach (var userMessage in userMessages)
        {
            Messages.Add(new ChatMessage(ChatRole.User, userMessage));
        }

        Messages.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);
    }

    /// <summary>
    /// Clears conversation history and reinitializes with system message and user additions.
    /// </summary>
    /// <param name="systemPrompt">The system prompt to add</param>
    /// <param name="userMessageAdds">Additional user messages to include</param>
    public void Clear(string systemPrompt, List<ChatMessage> userMessageAdds)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        Messages.AddRange(userMessageAdds);
    }
}