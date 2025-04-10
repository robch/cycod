To configure OpenAI as your provider:

1. Get an API key from [OpenAI Platform](https://platform.openai.com/api-keys)
2. Configure ChatX with your key:

```bash
# Set your OpenAI API key
chatx config set openai.apiKey YOUR_API_KEY --user

# Set OpenAI as your default provider (optional)
chatx config set app.preferredProvider openai --user

# Optionally set a specific model
chatx config set openai.chatModelName gpt-4o --user
```

You can also provide your API key directly in commands:

```bash
chatx --use-openai --openai-api-key YOUR_API_KEY --question "What is ChatX?"
```