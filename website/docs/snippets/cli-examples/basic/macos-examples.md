### Basic Question

Ask CycoD a simple question:

```bash
# macOS Terminal
cycod --question "What is the capital of France?"
```

### Interactive Mode

Start an interactive chat session:

```bash
# macOS Terminal
cycod --interactive
```

### Using Files as Input

Read content from a file and use it as input:

```bash
# macOS Terminal
cycod --input ~/Documents/myfile.txt --question "Summarize this text"
```

### Saving Output

Save chat history to continue later:

```bash
# macOS Terminal
cycod --question "Tell me about AI" --output-chat-history ~/Documents/chat.jsonl
```