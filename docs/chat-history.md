# Chat History

ChatX provides functionality to save and load chat histories, allowing you to maintain context across sessions and refer back to previous conversations.

## Understanding Chat History Files

Chat histories in ChatX are stored in JSONL (JSON Lines) format, where each line contains a JSON object representing a message in the conversation. This format makes it easy to append new messages to the history file and to parse individual messages without loading the entire file.

## Saving Chat History

You can save your chat history to a file using the `--output-chat-history` option:

```bash
chatx --output-chat-history "my-project-chat.jsonl"
```

If no filename is specified, ChatX uses a default template: `chat-history-{time}.jsonl`, where `{time}` is replaced with the current date and time.

### Auto-saving

When you use the `--output-chat-history` option, ChatX automatically saves each message (both your inputs and the AI's responses) to the specified file as they occur. This ensures that your conversation is preserved even if the program unexpectedly terminates.

## Loading Chat History

To continue a previous conversation, you can load a chat history using the `--input-chat-history` option:

```bash
chatx --input-chat-history "my-project-chat.jsonl"
```

This loads all messages from the specified file and provides them as context to the AI assistant, allowing the conversation to continue as if it had never ended.

## Combining History Operations

You can both load and save chat history in the same command, which is useful for continuing and extending an existing conversation:

```bash
chatx --input-chat-history "previous-conversation.jsonl" --output-chat-history "continued-conversation.jsonl"
```

This loads the history from `previous-conversation.jsonl` and saves all new messages to `continued-conversation.jsonl`.

## Chat History and Context

It's important to be aware that very long chat histories may exceed the context window of the AI model. ChatX does not automatically truncate histories, so if you're working with a particularly long conversation, you may need to edit the history file manually or start with a new summary message.

## Examples

### Starting a New Project with Saved History

```bash
chatx --system-prompt "You are an AI assistant helping me plan and implement a web application." --output-chat-history "web-app-project.jsonl"
```

### Continuing a Project Later

```bash
chatx --input-chat-history "web-app-project.jsonl" --output-chat-history "web-app-project.jsonl"
```

### Creating a New Branch of a Conversation

```bash
chatx --input-chat-history "project-brainstorm.jsonl" --output-chat-history "project-implementation.jsonl"
```

## File Format Details

Each line in a chat history file contains a JSON object with the following structure:

```json
{
  "role": "user|assistant|system",
  "content": "The message content as a string"
}
```

- `role` can be "user" (your messages), "assistant" (AI responses), or "system" (system prompts)
- `content` contains the actual text of the message

This format is compatible with the OpenAI Chat API message format, making it easy to use the files with other tools or to edit them manually if needed.