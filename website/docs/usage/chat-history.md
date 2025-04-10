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

#### Using `--output-chat-history`

When using the [`--output-chat-history`](../reference/cli/options/output-chat-history.md) option:

- If the specified file doesn't exist, ChatX creates it
- If the file already exists, ChatX appends new messages to it
- The output is in JSONL format with one message per line
- This option overrides the default automatic history saving location

Additional examples:

```bash title="Save with timestamp in filename"
chatx --question "What's new in Python 3.11?" --output-chat-history "python-$(date +%Y%m%d).jsonl"
```

```bash title="Branching a conversation"
chatx --input-chat-history main-chat.jsonl --output-chat-history branch-chat.jsonl --question "Let's explore alternative approach"
```

#### When to use `--output-chat-history` vs. `--chat-history`

- Use [`--output-chat-history`](../reference/cli/options/output-chat-history.md) when:
  - You want to save to a file but load from a different file (or no file)
  - You're creating a "branch" of an existing conversation
  - You want explicit control over where the output goes

- Use [`--chat-history`](../reference/cli/options/chat-history.md) when:
  - You want both input and output to use the same file
  - You're continuing a conversation in the same file over time

#### Using with `--continue`

When combining `--continue` with `--output-chat-history`:

```bash title="Continue and save to new file"
chatx --continue --output-chat-history new-conversation.jsonl --question "Next question"
```

ChatX will load the most recent chat history but save to your specified file. This is useful for creating branches from your most recent conversation.

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

### Using the Combined `--chat-history` Option

The `--chat-history` option provides a convenient way to both load from and save to the same file in a single command:

```bash title="Combined load and save"
chatx --chat-history my-history.jsonl --question "What is the capital of France?"
```

In a second session, continuing the same conversation:

```bash title="Continue with the same file"
chatx --chat-history my-history.jsonl --question "What about Italy?"
```

#### How It Works

When you use `--chat-history [FILE]`:

1. If the file exists:
   - ChatX loads the conversation history from this file
   - All new messages are appended to the same file
2. If the file doesn't exist yet:
   - ChatX creates the file when the first message is saved
   - No history is loaded (since there's nothing to load)

This is equivalent to using both `--input-chat-history` and `--output-chat-history` with the same filename:

```bash
# This command:
chatx --chat-history conversation.jsonl

# Is equivalent to:
chatx --input-chat-history conversation.jsonl --output-chat-history conversation.jsonl
```

#### When to Use `--chat-history`

The [`--chat-history`](../reference/cli/options/chat-history.md) option is ideal for:

- Project-specific conversations that you want to continue over time
- Daily work logs where you want to maintain continuity
- One-off conversations where you want the ability to continue later
- Any scenario where you want the same file for both input and output

If you need separate files for input and output (for example, to create a "branch" of a conversation), use the separate `--input-chat-history` and `--output-chat-history` options instead.

#### Additional Examples

Using `--chat-history` with token management:

```bash title="Chat history with token management"
chatx --chat-history my-long-conversation.jsonl --trim-token-target 16000 --question "Continue our discussion"
```

Using `--chat-history` in an interactive session:

```bash title="Interactive session with chat history"
chatx --interactive --chat-history coding-help.jsonl
```

Using `--chat-history` with a template filename:

```bash title="Template filename"
chatx --chat-history "project-{date}.jsonl" --question "Let's start planning the project"
```

## Continuing Recent Conversations

To continue your most recently saved chat history, use the `--continue` flag:

```bash title="Continue recent chat"
chatx --continue --question "What about Spain?"
```

This automatically loads the most recent chat history file without needing to specify a filename.

### How it works

When you use the `--continue` flag:

1. ChatX searches for chat history files in multiple locations:
   - Current directory (any `chat-history-*.jsonl` files)
   - Local scope history directory (`.chatx/history/`)
   - User scope history directory (`%USERPROFILE%\.chatx\history/` on Windows or `~/.chatx/history/` on Unix/Mac)

2. It finds all chat history files (including both regular and exception chat histories)
3. It sorts them by last modification time
4. It loads the most recently modified file

### Common uses for `--continue`

#### Quick follow-ups

Perfect for when you've just closed a session but have a follow-up question:

```bash title="Quick follow-up"
# First session
chatx --question "How do I create a Python virtual environment?"

# Later (after closing the first session)
chatx --continue --question "How do I activate it on Windows?"
```

#### Resuming work sessions

When you're working on a project over multiple days:

```bash title="Resume work session"
# Start a new day
chatx --continue --interactive

# Your chat history from yesterday is automatically loaded
```

#### Combining with other options

You can combine `--continue` with other options for more control:

```bash title="Continue with specific model"
chatx --continue --use-azure-openai --question "Continue our discussion"
```

```bash title="Continue and save to the same file"
chatx --continue --output-chat-history auto --question "Next question"
```

### Notes

- If multiple chat histories have the same modification time, ChatX selects one based on internal sorting.
- If no chat history files are found, a new conversation will start.
- The `--continue` flag is overridden if you also specify `--chat-history` or `--input-chat-history`.
- Using `--continue` with `--output-chat-history auto` will save back to the same file that was loaded.

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

### Using the `--trim-token-target` Option

You can control how ChatX manages tokens by using the `--trim-token-target` option to set a maximum target:

```bash
chatx --trim-token-target 16000 --chat-history my-history.jsonl --question "Next question"
```

The default token target is 160000 tokens, which works well for models with large context windows like GPT-4o.

#### How Token Management Works

When you use `--trim-token-target`:

1. ChatX tracks the token count of your conversation
2. When loading chat history files, it trims the history if needed before loading
3. During conversation, it monitors total tokens and trims as necessary
4. The trimming algorithm prioritizes keeping recent messages intact 
5. Essential context (like system prompts) is preserved while less important details may be removed

#### Choosing the Right Token Target

Different AI models have different context window sizes:

| Model Type | Recommended Token Target | Notes |
|------------|--------------------------|-------|
| GPT-3.5 models | 4000-8000 | Smaller context window |
| GPT-4 (earlier versions) | 8000-32000 | Depends on specific model |
| Claude-2 | 60000-100000 | Large context window |
| GPT-4o, Claude-3 | 128000-160000 | Very large context window |

For example, if working with GPT-3.5-turbo:

```bash
chatx --use-openai --openai-chat-model-name gpt-3.5-turbo --trim-token-target 4000
```

#### Balancing Context and Performance

Setting the token target involves a trade-off:

- **Too low**: The model may lose important context from earlier in the conversation
- **Too high**: May exceed model limits, causing errors or unnecessary token usage
- **Just right**: Maintains enough context for coherence while staying within limits

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

## See Also

- [--output-chat-history](../reference/cli/options/output-chat-history.md): Reference for saving chat history
- [--input-chat-history](../reference/cli/options/input-chat-history.md): Reference for loading chat history
- [--chat-history](../reference/cli/options/chat-history.md): Reference for combined history loading/saving
- [--output-trajectory](../reference/cli/options/output-trajectory.md): Reference for saving human-readable history
- [--continue](../reference/cli/options/continue.md): Reference for continuing recent conversations
- [Configuration](configuration.md): How to configure ChatX's behavior