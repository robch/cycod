To configure OpenAI as your provider:

1. Get an API key from [OpenAI Platform](https://platform.openai.com/api-keys)
2. Configure CycoD with your key:

```bash
# Set your OpenAI API key
cycod config set openai.apiKey YOUR_API_KEY --user

# Set OpenAI as your default provider (optional)
cycod config set app.preferredProvider openai --user

# Optionally set a specific model
cycod config set openai.chatModelName gpt-4o --user
```

You can also provide your API key directly in commands:

```bash
cycod --use-openai --openai-api-key YOUR_API_KEY --question "What is CycoD?"
```