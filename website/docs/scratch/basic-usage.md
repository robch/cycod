## Basic Usage

Now that you've installed CycoD and configured an AI provider, you can start using it. Here are some basic examples:

```bash
# Ask a simple question
cycod --question "What time is it?"

# Start an interactive chat session
cycod --interactive

# Save chat history and continue later
cycod --question "Tell me about AI" --output-chat-history chat.jsonl
cycod --input-chat-history chat.jsonl --question "Tell me more"

# Use a specific provider
cycod --use-openai --question "What is GPT-4?"
cycod --use-azure-openai --question "What are Azure OpenAI models?"
cycod --use-copilot --question "Explain GitHub Copilot"
```

For more detailed information on using CycoD, check out the [Chat Basics](/basics/chat.md) guide.