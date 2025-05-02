# --chat-history

The `--chat-history` option is a convenient way to both load from and save to the same chat history file. It combines the functionality of `--input-chat-history` and `--output-chat-history` into a single option.

## Syntax

```bash
cycod --chat-history [FILE]
```

## Parameters

- `FILE`: Optional. Path to the chat history file in JSONL format. If not specified, defaults to `chat-history.jsonl` in the current directory.

## Behavior

- If the specified file exists:
  - Loads the chat history from this file (similar to `--input-chat-history`)
  - Saves all new messages to the same file (similar to `--output-chat-history`)
- If the specified file does not exist yet:
  - Creates a new file when the first message is saved
  - Does not attempt to load any history (since the file doesn't exist)

## Default Values

- When no filename is provided: Defaults to `chat-history.jsonl` in the current directory
- When a filename template is provided with special placeholders:
  - `{time}`: Replaced with timestamp (e.g., `20250415123045`)
  - `{date}`: Replaced with date (e.g., `20250415`)
  - `{filebase}`: Base name of the file without extension
  - `{fileext}`: Extension of the file

## Examples

### Basic Usage

```bash
# Load from and save to "my-project.jsonl"
cycod --chat-history my-project.jsonl --question "What is CycoD?"
```

### Continuing a Conversation

```bash
# First session
cycod --chat-history project-chat.jsonl --question "How do I structure a Python project?"

# Later session continuing the conversation
cycod --chat-history project-chat.jsonl --question "How should I organize the test files?"
```

### With Templates

```bash
# Use a date-based filename
cycod --chat-history chat-{date}.jsonl --question "What's the weather like today?"

# This creates a file like "chat-20250415.jsonl"
```

## Related Options

| Option | Description |
|--------|-------------|
| `--input-chat-history [FILE]` | Load chat history from the specified JSONL file |
| `--output-chat-history [FILE]` | Save chat history to the specified file |
| `--continue` | Continue the most recent chat history |
| `--trim-token-target [TOKENS]` | Set target for maximum tokens in history |

## Notes

- The `--chat-history` option overrides the `--continue` flag if both are specified
- The specified file must be in JSONL format
- Chat history files can grow large with extended conversations, but CycoD includes automatic token management

## See Also

- [Chat History](../../../usage/chat-history.md) - Detailed guide on managing chat history
- [Continuing Conversations](../../../usage/chat-history.md#continuing-recent-conversations) - Learn about continuing conversations with `--continue`
- [Token Management](../../../usage/chat-history.md#token-management) - How to manage token usage in long conversations