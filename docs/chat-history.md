# Chat History

CycoD provides functionality to save and load chat histories, allowing you to maintain context across sessions and refer back to previous conversations. It also supports saving conversations in a human-readable trajectory format.

## Understanding Chat History Files

Chat histories in CycoD are stored in JSONL (JSON Lines) format, where each line contains a JSON object representing a message in the conversation. This format makes it easy to append new messages to the history file and to parse individual messages without loading the entire file.

## Saving Chat History

### Automatic Saving

By default, CycoD automatically saves your chat history and trajectory files to the `history` directory under your user profile:

- Windows: `%USERPROFILE%\.cycod\history\`
- Mac/Linux: `~/.cycod/history/`

Files are saved with default names in the format:
- Chat history: `chat-history-{time}.jsonl`
- Trajectory: `trajectory-{time}.jsonl`

You can disable this feature using the config commands:

```bash
cycod config set App.AutoSaveChatHistory false
cycod config set App.AutoSaveTrajectory false
```

To re-enable automatic saving:

```bash
cycod config set App.AutoSaveChatHistory true
cycod config set App.AutoSaveTrajectory true
```

### Manual Saving with CLI Options

You can also explicitly specify where to save your chat history using the `--output-chat-history` option:

```bash
cycod --output-chat-history "my-project-chat.jsonl"
```

If no filename is specified, CycoD uses a default template: `chat-history-{time}.jsonl`, where `{time}` is replaced with the current date and time.

When you explicitly provide an output path with the CLI option, it takes precedence over the automatic saving setting.

### Filename Templates

You can use the following placeholders in your output filename:

- `{fileName}` or `{filename}`: Full file name
- `{filePath}` or `{filepath}`: Directory path
- `{fileBase}` or `{filebase}`: File name without extension
- `{fileExt}` or `{fileext}`: File extension without dot
- `{timeStamp}` or `{timestamp}`: Current timestamp (yyyyMMddHHmmss format)
- `{time}`: Unix timestamp in milliseconds

Example:
```bash
cycod --output-chat-history "chats/{filebase}-{timestamp}.jsonl"
```

### Auto-saving

When you use the `--output-chat-history` option, CycoD automatically saves each message (both your inputs and the AI's responses) to the specified file as they occur. This ensures that your conversation is preserved even if the program unexpectedly terminates.

## Loading Chat History

To continue a previous conversation, you can load a chat history using the `--input-chat-history` option:

```bash
cycod --input-chat-history "my-project-chat.jsonl"
```

Alternatively, you can use the `--continue` option to automatically load the most recent chat history file:

```bash
cycod --continue
```

This will search for the most recent chat history file (matching the pattern "chat-history-*.jsonl") in both:
1. The user-scoped history folder (where auto-saved histories are stored)
2. The current directory

You can use this to quickly pick up where you left off without having to remember the full filename.

This loads all messages from the specified file and provides them as context to the AI assistant, allowing the conversation to continue as if it had never ended.

## Combining History Operations

You can both load and save chat history in the same command, which is useful for continuing and extending an existing conversation:

```bash
cycod --input-chat-history "previous-conversation.jsonl" --output-chat-history "continued-conversation.jsonl"
```

This loads the history from `previous-conversation.jsonl` and saves all new messages to `continued-conversation.jsonl`.

## Managing Token Usage

Long conversations can exceed the token limit of AI models. CycoD provides a token management feature to help with this:

```bash
cycod --input-chat-history "long-conversation.jsonl" --output-chat-history "long-conversation.jsonl" --max-token-target 160000
```

The `--max-token-target` option sets a target token count. When the history approaches this limit, CycoD will automatically trim content, prioritizing:

1. Reducing tool call outputs that are very large
2. Maintaining the most recent messages for context

When reducing tool call content, CycoD will replace it with a "...snip..." indicator to maintain the conversational flow while reducing token usage.

### When Token Trimming Occurs

CycoD performs token trimming at several key moments:

1. When loading chat history from a file (via `--input-chat-history`)
2. When adding user messages to the conversation
3. Before sending the conversation to the AI model

This proactive approach ensures that your conversation never exceeds the model's token limits, preventing errors while preserving the most important context.

### How Token Trimming Works

The token trimming process:

1. Estimates token count based on byte length of all messages
2. If estimated tokens exceed the target, begins the reduction process
3. First targets tool call outputs, especially those with large content
4. Keeps the most important and recent content intact

This approach helps maintain the AI's understanding of the conversation while keeping within token limits.

## Chat History and Context

Without token management, very long chat histories may exceed the context window of the AI model. If you're not using `--max-token-target`, you might need to:

1. Start a new history file with a summary of the previous conversation
2. Edit the history file manually to remove less relevant parts
3. Split long conversations into multiple focused sessions

## Examples

### Starting a New Project with Saved History

```bash
cycod --system-prompt "You are an AI assistant helping me plan and implement a web application." --output-chat-history "web-app-project.jsonl"
```

### Continuing a Project Later

```bash
cycod --input-chat-history "web-app-project.jsonl" --output-chat-history "web-app-project.jsonl"
```

### Managing a Long-Running Conversation

```bash
cycod --input-chat-history "long-project.jsonl" --output-chat-history "long-project.jsonl" --max-token-target 120000
```

### Creating a New Branch of a Conversation

```bash
cycod --input-chat-history "project-brainstorm.jsonl" --output-chat-history "project-implementation.jsonl"
```

## File Format Details

Each line in a chat history file contains a JSON object representing a message. The format varies slightly depending on the message type:

### User Messages
```json
{
  "role": "user",
  "content": "The user's message content as a string"
}
```

### Assistant Messages
```json
{
  "role": "assistant",
  "content": "The assistant's response as a string"
}
```

### System Messages
```json
{
  "role": "system",
  "content": "The system prompt as a string"
}
```

### Tool Messages (Function Call Results)
## Trajectory Format

In addition to JSONL-formatted chat history, CycoD can also save your conversation in a more human-readable trajectory format using the `--output-trajectory` option:

```bash
cycod --output-trajectory "conversation.md"
```

Alternatively, you can rely on the automatic saving feature (enabled by default) which saves trajectory files to your user profile's `.cycod/history` directory.

### Understanding Trajectory Format

The trajectory format is designed to be more readable than JSONL, making it easier to review conversations. It uses a Markdown-like syntax that shows:

1. User messages in plain text
2. Assistant responses in plain text
3. Function calls in XML-formatted blocks
4. Function results in XML-formatted blocks

For example:

```
I need to know the current date.

The current date is October 3, 2023.

<function_calls>
<invoke name="GetCurrentTime">
</invoke>
</function_calls>

<function_results>
The current time is 14:25:36 UTC.
</function_results>

Thank you for providing the time.
```

### When to Use Trajectory Format

The trajectory format is particularly useful when:

1. You want to share conversations with others in a readable format
2. You need to review your chat history yourself
3. You're documenting interactions for training or reference purposes

It's important to note that unlike JSONL chat history files, trajectory files cannot be loaded back into CycoD as context for future conversations.

### Using Both Formats Together

You can use both history formats simultaneously:

```bash
cycod --output-chat-history "conversation.jsonl" --output-trajectory "conversation.md"
```

This gives you both a machine-readable format (JSONL) for continuing conversations and a human-readable format (trajectory) for review and documentation.

```json
{
  "role": "tool",
  "tool_call_id": "unique-tool-call-id",
  "content": "The result of the function call"
}
```

This format is compatible with the OpenAI Chat API message format, making it easy to use the files with other tools or to edit them manually if needed.