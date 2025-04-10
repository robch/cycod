# --trim-token-target

Set a maximum target for tokens in chat history to manage context length.

## Syntax

```
--trim-token-target <NUMBER>
```

## Description

The `--trim-token-target` option allows you to specify a maximum token count target for your chat history. AI models have context length limitations, and exceeding these limits can cause errors or truncated responses.

When you use this option, ChatX will automatically trim the chat history to stay under the specified token limit while preserving the most important parts of the conversation. This is particularly useful for long-running conversations where the context could grow beyond model limits.

ChatX optimizes token management by:
- Trimming histories before loading them from files
- Trimming during the conversation as needed
- Preserving essential context while removing less important details
- Focusing on keeping the most recent messages intact

## Parameters

- `<NUMBER>`: The maximum number of tokens to target in the chat history. Default is 160000.

## Examples

### Set a conservative token limit for a large chat history

```bash
chatx --chat-history my-long-conversation.jsonl --trim-token-target 8000 --question "Continue our discussion"
```

### Use with older models that have smaller context windows

```bash
chatx --use-openai --openai-chat-model-name gpt-3.5-turbo --trim-token-target 4000 --input-chat-history previous-chat.jsonl
```

### Continue a conversation with a specific token limit

```bash
chatx --continue --trim-token-target 16000 --question "Let's continue our discussion about the project architecture"
```

### Interactive session with token management

```bash
chatx --interactive --trim-token-target 100000
```

## Notes

- The default value is 160000 tokens, which works well for most modern models like GPT-4o.
- For models with smaller context windows, consider using a lower value:
  - GPT-3.5 models: 4000-8000 tokens
  - Claude-2 models: 60000-100000 tokens
  - GPT-4 models: 8000-32000 tokens depending on the specific model
- Setting this value too low might cause the model to lose important context.
- Setting this value too high might exceed the model's capabilities and cause errors.

## Related Options

- [`--chat-history`](chat-history.md): Load from and save to the same file
- [`--input-chat-history`](input-chat-history.md): Load chat history from a file
- [`--output-chat-history`](output-chat-history.md): Save chat history to a file
- [`--continue`](continue.md): Continue the most recent chat

## See Also

- [Chat History Usage Guide](../../../usage/chat-history.md): Comprehensive guide on managing chat history
- [Using Different Models](../../../providers/models.md): Information on context windows for different models