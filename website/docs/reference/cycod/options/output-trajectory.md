# --output-trajectory

Save chat history to a specified file in human-readable Markdown format.

## Syntax

```
--output-trajectory <PATH>
```

## Description

The `--output-trajectory` option allows you to explicitly specify a file where CycoD will save the conversation history in a human-readable Markdown format (trajectory format). Unlike the JSONL format, trajectory files are designed for easy reading and reviewing by humans, making them ideal for documentation, sharing, or archival purposes.

When this option is used, it takes precedence over the automatic trajectory saving feature of CycoD.

## Parameters

- `<PATH>`: The file path where the trajectory will be saved. Typically uses a `.md` extension.

## Examples

### Save trajectory to a specific file

```bash
cycod --question "What time is it?" --output-trajectory conversation.md
```

### Save an interactive session as a trajectory

```bash
cycod --interactive --output-trajectory project-discussion.md
```

### Combine with chat history options

```bash
cycod --chat-history data.jsonl --output-trajectory human-readable.md --question "Summarize our progress"
```

### Save trajectory with timestamp in filename

```bash
cycod --question "What's new in Python 3.12?" --output-trajectory "python-discussion-$(date +%Y-%m-%d).md"
```

## File Format

The generated trajectory file uses a Markdown format with clear sections for each participant in the conversation:

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

## Notes

- Trajectory files are ideal when you need to share conversation logs with others or review them yourself.
- They're formatted for human readability with Markdown headings and proper spacing.
- Unlike JSONL files, trajectory files cannot be used to reload conversation context into CycoD.
- By default, CycoD automatically saves both formats (JSONL and trajectory) to your history directory unless configured otherwise.

## Related Options

- [`--chat-history`](chat-history.md): Load from and save to the same JSONL file
- [`--input-chat-history`](input-chat-history.md): Load chat history from a JSONL file
- [`--output-chat-history`](output-chat-history.md): Save machine-readable history
- [`--continue`](continue.md): Continue the most recent chat

## See Also

- [Chat History Usage Guide](../../../usage/chat-history.md): Comprehensive guide on managing chat history
- [Configuration](../../../usage/configuration.md): How to configure automatic history saving