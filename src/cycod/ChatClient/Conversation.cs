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
    /// Persistent user messages that survive conversation clearing.
    /// These are typically added via --add-user-prompt functionality.
    /// </summary>
    private readonly List<ChatMessage> _persistentUserMessages = new();

    /// <summary>
    /// Initializes a new conversation with empty messages and default metadata.
    /// </summary>
    public Conversation()
    {
        Messages = new List<ChatMessage>();
        Metadata = ConversationMetadataHelpers.CreateDefault();
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
        // Helper does pure I/O - load and return data
        var (metadata, newMessages) = AIExtensionsChatHelpers.ReadChatHistoryFromFile(fileName, useOpenAIFormat);
        
        // If loaded messages have system message, do complete replacement
        var hasSystemMessage = newMessages.Any(x => x.Role == ChatRole.System);
        if (hasSystemMessage)
        {
            Messages.Clear(); // Complete replacement - file contains full conversation
        }
        Messages.AddRange(newMessages);
        Messages.FixDanglingToolCalls();
        Messages.TryTrimToTarget(maxPromptTokenTarget, maxToolTokenTarget, maxChatTokenTarget);
        
        // Update metadata (use loaded metadata if present, otherwise keep default)
        if (metadata != null)
        {
            Metadata = metadata;
        }
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
    /// Adds a transient user message to the conversation.
    /// Transient messages are cleared when ClearChatHistory is called.
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
    /// Adds a persistent user message to the conversation.
    /// Persistent messages survive conversation clearing and are typically used for context like --add-user-prompt.
    /// </summary>
    /// <param name="userMessage">The user message to add</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    public void AddPersistentUserMessage(string userMessage, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        var message = new ChatMessage(ChatRole.User, userMessage);
        _persistentUserMessages.Add(message);
        Messages.Add(message);
        
        // Trim both the persistent cache and the main messages
        _persistentUserMessages.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);
        Messages.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);
    }

    /// <summary>
    /// Adds multiple persistent user messages to the conversation.
    /// Persistent messages survive conversation clearing and are typically used for context like --add-user-prompt.
    /// </summary>
    /// <param name="userMessages">The user messages to add</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    public void AddPersistentUserMessages(IEnumerable<string> userMessages, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        foreach (var userMessage in userMessages)
        {
            AddPersistentUserMessage(userMessage, maxPromptTokenTarget, maxChatTokenTarget);
        }
    }

    /// <summary>
    /// Adds multiple transient user messages to the conversation.
    /// Transient messages are cleared when ClearChatHistory is called.
    /// </summary>
    /// <param name="userMessages">The user messages to add</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt tokens to keep</param>
    /// <param name="maxChatTokenTarget">Maximum chat tokens to keep</param>
    public void AddUserMessages(IEnumerable<string> userMessages, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        foreach (var userMessage in userMessages)
        {
            AddUserMessage(userMessage, maxPromptTokenTarget, maxChatTokenTarget);
        }
    }

    /// <summary>
    /// Clears conversation history and reinitializes with system message and persistent user messages.
    /// </summary>
    /// <param name="systemPrompt">The system prompt to add</param>
    public void Clear(string systemPrompt)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        Messages.AddRange(_persistentUserMessages);
    }



    #region Title Management

    /// <summary>
    /// Sets a user-provided title and locks it from AI regeneration.
    /// </summary>
    /// <param name="title">User-provided title</param>
    public void SetUserTitle(string title)
    {
        if (Metadata == null) throw new InvalidOperationException("Metadata should never be null after constructor");
        Metadata.Title = title;
        Metadata.IsTitleLocked = true;
    }

    /// <summary>
    /// Sets an AI-generated title without locking it (only if not already locked).
    /// </summary>
    /// <param name="title">AI-generated title</param>
    public void SetGeneratedTitle(string title)
    {
        if (Metadata == null) throw new InvalidOperationException("Metadata should never be null after constructor");
        if (!Metadata.IsTitleLocked) // Only set if not locked by user
        {
            Metadata.Title = title?.Trim();
            // IsTitleLocked remains false
        }
    }

    /// <summary>
    /// Gets display-friendly title with fallback to filename.
    /// </summary>
    /// <param name="filePath">File path for fallback title generation</param>
    /// <returns>Display title</returns>
    public string GetDisplayTitle(string filePath = "")
    {
        // Use metadata title if available
        if (!string.IsNullOrEmpty(Metadata?.Title))
        {
            return Metadata.Title;
        }

        // Extract from filename: "chat-history-1234567890.jsonl" → "conversation-1234567890"
        if (!string.IsNullOrEmpty(filePath))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith("chat-history-"))
            {
                var timestamp = fileName.Substring("chat-history-".Length);
                return $"conversation-{timestamp}";
            }
        }

        // Ultimate fallback
        return "Untitled Conversation";
    }

    /// <summary>
    /// Determines if this conversation needs a title to be generated.
    /// </summary>
    /// <returns>True if title generation is needed</returns>
    public bool NeedsTitleGeneration()
    {
        return Metadata != null && 
               string.IsNullOrEmpty(Metadata.Title) && 
               !Metadata.IsTitleLocked &&
               Messages.Any(m => m.Role == ChatRole.Assistant);
    }

    /// <summary>
    /// Locks the title to prevent AI regeneration.
    /// </summary>
    public void LockTitle()
    {
        if (Metadata == null) throw new InvalidOperationException("Metadata should never be null after constructor");
        Metadata.IsTitleLocked = true;
    }

    /// <summary>
    /// Unlocks the title to allow AI regeneration.
    /// </summary>
    public void UnlockTitle()
    {
        if (Metadata == null) throw new InvalidOperationException("Metadata should never be null after constructor");
        Metadata.IsTitleLocked = false;
    }

    /// <summary>
    /// Gets the current title or empty string if none.
    /// </summary>
    public string Title => Metadata?.Title ?? string.Empty;

    /// <summary>
    /// Gets whether the title is currently locked.
    /// </summary>
    public bool IsTitleLocked => Metadata?.IsTitleLocked == true;

    #endregion
}