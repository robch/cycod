# --output-chat-history

Save chat history to a specified file in JSONL format.

## Syntax

```
--output-chat-history <PATH>
```

## Description

The `--output-chat-history` option allows you to explicitly specify a file where ChatX will save the conversation history in JSONL (JSON Lines) format. This machine-readable format preserves the chat messages and can be used later to reload the conversation context.

When this option is used, it takes precedence over the automatic chat history saving feature of ChatX.

## Parameters

- `<PATH>`: The file path where chat history will be saved. Typically uses a `.jsonl` extension.

## Examples

### Save chat history to a specific file

```bash
chatx --question "Tell me a joke" --output-chat-history jokes.jsonl
```

### Save an interactive session to a file

```bash
chatx --interactive --output-chat-history project-chat.jsonl
```

### Use with other history-related options

```bash
chatx --input-chat-history old-chat.jsonl --question "Continue our discussion" --output-chat-history new-chat.jsonl
```

### Save chat history with custom naming pattern

```bash
chatx --question "What day is it today?" --output-chat-history "chat-$(date +%Y-%m-%d).jsonl"
```

## File Format

The generated JSONL file contains one JSON object per line, with each object representing a message in the conversation:

```jsonl
{"role":"system","content":"You are a helpful assistant."}
{"role":"user","content":"What is the capital of France?"}
{"role":"assistant","content":"The capital of France is Paris."}
```

## Notes

- This option is useful when you want to control exactly where your chat history is saved.
- When combined with `--input-chat-history`, you can read from one file and write to another.
- If you need both input and output to the same file, consider using the [`--chat-history`](chat-history.md) option instead.
- Use [`--output-trajectory`](output-trajectory.md) if you need a human-readable format instead of JSONL.

## Related Options

- [`--chat-history`](chat-history.md): Load from and save to the same file
- [`--input-chat-history`](input-chat-history.md): Load chat history from a file
- [`--output-trajectory`](output-trajectory.md): Save human-readable history
- [`--continue`](continue.md): Continue the most recent chat

## See Also

- [Chat History Usage Guide](../../../usage/chat-history.md): Comprehensive guide on managing chat history
- [Configuration](../../../usage/configuration.md): How to configure automatic history saving