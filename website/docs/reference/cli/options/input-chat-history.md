# --input-chat-history

Load chat history from a specified JSONL file to continue a previous conversation.

## Syntax

```bash
chatx --input-chat-history [FILE]
```

## Parameters

- `FILE`: Required. Path to the chat history file in JSONL format.

## Behavior

When the `--input-chat-history` option is used:

- ChatX loads the conversation history from the specified JSONL file
- The loaded messages become the context for the current chat session
- The AI model has access to this history when generating responses
- CHATX automatically trims the history if needed to stay within token limits
- New messages are not automatically saved back to this file unless `--output-chat-history` is also specified with the same filename

## Examples

### Basic Usage

```bash
# Load chat history and ask a follow-up question
chatx --input-chat-history previous-chat.jsonl --question "Could you elaborate more on that last point?"
```

### With Interactive Mode

```bash
# Start an interactive session with loaded history
chatx --input-chat-history coding-help.jsonl --interactive
```

### Creating a Branch

```bash
# Load history from one file but save to another
chatx --input-chat-history main-discussion.jsonl --output-chat-history alternative-approach.jsonl --question "Let's try a different approach"
```

### With Model Selection

```bash
# Load history and use a specific model
chatx --input-chat-history complex-problem.jsonl --use-openai --openai-chat-model-name gpt-4o --question "Can you solve this more efficiently?"
```

## Notes

- The chat history file must be in JSONL format, with one message per line
- ChatX performs automatic token management to prevent exceeding model context limits
- For long conversations, consider using `--trim-token-target` to control context size
- If you want to both read from and write to the same file, use [`--chat-history`](chat-history.md) instead

## Related Options

| Option | Description |
|--------|-------------|
| `--chat-history [FILE]` | Load from and save to the same file |
| `--output-chat-history [FILE]` | Save chat history to the specified file |
| `--continue` | Continue the most recent chat history |
| `--trim-token-target [TOKENS]` | Set target for maximum tokens in history |

## See Also

- [Chat History](../../../usage/chat-history.md) - Detailed guide on managing chat history
- [--chat-history](chat-history.md) - Reference for the combined load/save option
- [--output-chat-history](output-chat-history.md) - Reference for saving chat history
- [--continue](continue.md) - Reference for continuing recent conversations
- [Token Management](../../../usage/chat-history.md#token-management) - How to manage token usage