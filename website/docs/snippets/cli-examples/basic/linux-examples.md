### Basic Question

Ask ChatX a simple question:

```bash
# Linux Terminal
chatx --question "What is the capital of France?"
```

### Interactive Mode

Start an interactive chat session:

```bash
# Linux Terminal
chatx --interactive
```

### Using Files as Input

Read content from a file and use it as input:

```bash
# Linux Terminal
chatx --input ~/myfile.txt --question "Summarize this text"
```

### Saving Output

Save chat history to continue later:

```bash
# Linux Terminal
chatx --question "Tell me about AI" --output-chat-history ~/chat.jsonl
```