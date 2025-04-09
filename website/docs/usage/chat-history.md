# Chat History

CHATX allows you to save, load, and continue conversations across sessions. This guide explains how to manage your chat history effectively.

## Automatic History Saving

By default, CHATX automatically saves both your chat history and trajectory files to a 'history' directory under your user profile:

- Windows: `%USERPROFILE%\.chatx\history\`
- Mac/Linux: `~/.chatx/history\`

Files are saved with timestamp-based names:
- Chat history: `chat-history-{time}.jsonl`
- Trajectory: `trajectory-{time}.jsonl`

## Chat History Formats

CHATX provides two ways to save your conversation history:

1. **Chat History (JSONL format)** - Machine-readable format for reloading context
2. **Trajectory (formatted text)** - Human-readable format for reviewing conversations

### JSONL Format

The JSONL format stores each message as a JSON object on a separate line. This is ideal for reloading conversation context:

```jsonl
{"role":"system","content":"You are a helpful assistant."}
{"role":"user","content":"What is the capital of France?"}
{"role":"assistant","content":"The capital of France is Paris."}
{"role":"user","content":"What about Germany?"}
{"role":"assistant","content":"The capital of Germany is Berlin."}
```

### Trajectory Format

The trajectory format is a human-readable text file that's easier to review:

```markdown
# Conversation: 2025-01-15T14:30:00Z

## System
You are a helpful assistant.

## User
What is the capital of France?

## Assistant
The capital of France is Paris.

## User
What about Germany?

## Assistant
The capital of Germany is Berlin.
```

## Manual History Management

### Saving Chat History

To explicitly save a chat history to a specific file:

```bash title="Save chat history"
chatx --question "What is the capital of France?" --output-chat-history my-history.jsonl
```

For an interactive session:

```bash title="Save history from interactive session"
chatx --interactive --output-chat-history my-history.jsonl
```

### Saving Trajectory

To save a human-readable trajectory file:

```bash title="Save trajectory"
chatx --question "What is the capital of France?" --output-trajectory my-conversation.md
```

### Loading Chat History

To continue a conversation from a previously saved history:

```bash title="Load chat history"
chatx --input-chat-history my-history.jsonl --question "What about Italy?"
```

You can also load history for an interactive session:

```bash title="Interactive with history"
chatx --interactive --input-chat-history my-history.jsonl
```

### Convenient Combined Option

You can use the `--chat-history` flag to both load from and save to the same file:

```bash title="Combined load and save"
chatx --chat-history my-history.jsonl --question "What is the capital of France?"
```

In a second session, continuing the same conversation:

```bash
chatx --chat-history my-history.jsonl --question "What about Italy?"
```

## Continuing Recent Conversations

To continue your most recently saved chat history, use the `--continue` flag:

```bash title="Continue recent chat"
chatx --continue --question "What about Spain?"
```

This automatically loads the most recent chat history file.

## Managing Automatic Saving

You can disable automatic saving using the config command:

### Disable for current directory (local scope):

```bash
chatx config set App.AutoSaveChatHistory false --local
chatx config set App.AutoSaveTrajectory false --local
```

### Disable for current user (user scope):

```bash
chatx config set App.AutoSaveChatHistory false --user
chatx config set App.AutoSaveTrajectory false --user
```

### Disable for all users (global scope):

```bash
chatx config set App.AutoSaveChatHistory false --global
chatx config set App.AutoSaveTrajectory false --global
```

To re-enable automatic saving, use the same commands with `true` instead of `false`.

## Token Management

CHATX automatically manages token usage for long conversations to prevent errors from exceeding model context limits.

You can set a token target with the `--trim-token-target` option:

```bash
chatx --trim-token-target 16000 --chat-history my-history.jsonl --question "Next question"
```

The default token target is 18000 tokens, which works well for most models.

CHATX optimizes token usage by:
- Trimming histories before loading them
- Trimming during the conversation as needed
- Preserving essential context while removing less important details
- Focusing on keeping the most recent messages intact

## Example Workflows

### Project-Specific History

Create a project-specific history file:

```bash
# In your project directory
chatx --chat-history project-chat.jsonl --question "How should I structure this project?"
```

Later, continue the conversation:

```bash
chatx --chat-history project-chat.jsonl --question "How should I implement feature X?"
```

### Daily Work Log

Use a daily chat history to track your work:

```bash
# Start of the day
chatx --chat-history worklog-$(date +%Y-%m-%d).jsonl --question "What are my priorities for today?"
```

Later in the day:

```bash
chatx --chat-history worklog-$(date +%Y-%m-%d).jsonl --question "How should I approach this problem?"
```

## Security Considerations

Chat history files contain all messages exchanged with AI models, which may include sensitive information. Keep these security considerations in mind:

1. Don't store sensitive information (passwords, API keys, etc.) in chats
2. Secure access to history files, especially in shared environments
3. Use local scope for project-specific histories that contain confidential information
4. Delete history files when they're no longer needed