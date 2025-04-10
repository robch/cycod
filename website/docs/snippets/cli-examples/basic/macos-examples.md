### Basic Question

Ask ChatX a simple question:

```bash
# macOS Terminal
chatx --question "What is the capital of France?"
```

### Interactive Mode

Start an interactive chat session:

```bash
# macOS Terminal
chatx --interactive
```

### Using Files as Input

Read content from a file and use it as input:

```bash
# macOS Terminal
chatx --input ~/Documents/myfile.txt --question "Summarize this text"
```

### Saving Output

Save chat history to continue later:

```bash
# macOS Terminal
chatx --question "Tell me about AI" --output-chat-history ~/Documents/chat.jsonl
```