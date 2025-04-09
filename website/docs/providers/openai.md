# OpenAI API Provider

CHATX can connect to the OpenAI API to access models like GPT-4o, GPT-4, and GPT-3.5-Turbo. This guide will help you set up and use the OpenAI API with CHATX.

## Prerequisites

1. An OpenAI account
2. An API key from the [OpenAI platform](https://platform.openai.com/api-keys)

## Configuration

You can configure the OpenAI API in CHATX using the `config` command:

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

This will store your API key in the user-level configuration, making it available for all your CHATX sessions.

You can also set the model to use:

```bash
chatx config set openai.chatModelName gpt-4o --user
```

## Configuration Scopes

CHATX supports three different configuration scopes:

1. **Local scope** (default): Configuration applies only to the current directory
2. **User scope** (`--user`): Configuration applies to all directories for the current user
3. **Global scope** (`--global`): Configuration applies to all users on the system

For API keys, we recommend using the user scope for security and convenience.

## Command-Line Options

You can also provide OpenAI API settings directly in your commands:

```bash
chatx --use-openai --openai-api-key YOUR_API_KEY --openai-chat-model-name gpt-4 --question "What is GPT-4?"
```

Using the `--use-openai` flag explicitly tells CHATX to use the OpenAI API as the provider.

## Example Usage

Basic query using the OpenAI API:

```bash title="Basic query"
chatx --use-openai --question "Explain what LLMs are"
```

Interactive chat with OpenAI:

```bash title="Interactive chat"
chatx --use-openai --interactive
```

Specify a different model:

```bash title="Using GPT-4 specifically"
chatx --use-openai --openai-chat-model-name gpt-4 --question "Explain quantum computing"
```

## Available Models

The OpenAI API offers several models with different capabilities and price points:

| Model | Description | Use Cases |
|-------|-------------|-----------|
| gpt-4o | Latest multimodal model | General purpose, images, complex tasks |
| gpt-4 | Powerful instruction-following model | Complex reasoning, detailed outputs |
| gpt-3.5-turbo | Fast and economical model | Simple queries, high-volume use |

By default, CHATX uses `gpt-4o` with the OpenAI API, but you can change this using the `--openai-chat-model-name` option or by setting `openai.chatModelName` in your configuration.

## Troubleshooting

If you encounter issues with the OpenAI API, try these steps:

1. Verify your API key is valid
2. Check your OpenAI account has sufficient credits
3. Ensure you're using a supported model name
4. Check for API rate limits or quotas

For more detailed information, refer to the [OpenAI API documentation](https://platform.openai.com/docs/api-reference).