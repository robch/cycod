# --continue

The `--continue` option allows you to automatically load and continue your most recent chat history without having to specify a filename.

## Syntax

```bash
chatx --continue [other options]
```

## Parameters

This option doesn't take any parameters.

## Behavior

When the `--continue` flag is used:

- ChatX searches for the most recently modified chat history file in the following locations:
  1. The current directory
  2. Local scope history directory (`.chatx/history/`)
  3. User scope history directory (`%USERPROFILE%\.chatx\history/` on Windows or `~/.chatx/history/` on Unix/Mac)
- Both regular chat history files (`chat-history-*.jsonl`) and exception chat history files (`exception-chat-history-*.jsonl`) are considered
- Files are sorted by last write time to determine the most recent one
- The most recent chat history is automatically loaded, allowing your conversation to continue where you left off

## Examples

### Basic Usage

```bash
# Continue the most recent chat
chatx --continue --question "What were we talking about?"
```

### Interactive Mode

```bash
# Continue the most recent chat in interactive mode
chatx --continue --interactive
```

### With Other Options

```bash
# Continue with a different model
chatx --continue --use-openai --openai-chat-model-name gpt-4
```

### Continue and Save to the Same File

```bash
# Continue the most recent chat and save new messages back to it
chatx --continue --output-chat-history auto
```

## Notes

- The `--continue` flag is overridden by `--chat-history` or `--input-chat-history` if they are specified in the same command
- If no chat history files are found, ChatX will start a new conversation
- ChatX will automatically trim the chat history if needed to stay within token limits
- Using `--continue` with `--output-chat-history auto` will save back to the same file that was loaded

## Related Options

| Option | Description |
|--------|-------------|
| `--chat-history [FILE]` | Load from and save to the same file (overrides `--continue`) |
| `--input-chat-history [FILE]` | Load chat history from a specific file (overrides `--continue`) |
| `--output-chat-history [FILE]` | Save chat history to a specific file |
| `--trim-token-target [TOKENS]` | Set target for maximum tokens in history |

## See Also

- [Chat History](../../../usage/chat-history.md) - Detailed guide on managing chat history
- [Continuing Conversations](../../../usage/chat-history.md#continuing-recent-conversations) - Learn about continuing conversations with `--continue`
- [Token Management](../../../usage/chat-history.md#token-management) - How to manage token usage in long conversations