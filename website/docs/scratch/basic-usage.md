## Basic Usage

Now that you've installed ChatX and configured an AI provider, you can start using it. Here are some basic examples:

```bash
# Ask a simple question
chatx --question "What time is it?"

# Start an interactive chat session
chatx --interactive

# Save chat history and continue later
chatx --question "Tell me about AI" --output-chat-history chat.jsonl
chatx --input-chat-history chat.jsonl --question "Tell me more"

# Use a specific provider
chatx --use-openai --question "What is GPT-4?"
chatx --use-azure-openai --question "What are Azure OpenAI models?"
chatx --use-copilot --question "Explain GitHub Copilot"
```

For more detailed information on using ChatX, check out the [Chat Basics](/usage/basics.md) guide.