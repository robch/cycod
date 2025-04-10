---
hide:
- toc
icon: material/brain
---

# OpenAI API Provider
??? tip "Prerequisites"

    Before you begin:
    
    1. Make sure you have [installed CHATX](/getting-started.md)

CHATX can connect to the OpenAI API to access models like GPT-4o, GPT-4, and GPT-3.5-Turbo. This guide will help you set up and use the OpenAI API with CHATX.

## Prerequisites

1. An OpenAI account
2. An API key from the [OpenAI platform](https://platform.openai.com/api-keys)

## Configuration

There are multiple ways to configure your OpenAI API settings in CHATX:

### Method 1: Using the Configuration System (Recommended)

You can configure the OpenAI API in CHATX using the `config` command:

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

This will store your API key in the user-level configuration, making it available for all your CHATX sessions.

You can also set the model to use:

```bash
chatx config set openai.chatModelName gpt-4o --user
```

### Method 2: Using Command-Line Options

For one-time use or testing, you can specify your API key directly in the command:

```bash
chatx --use-openai --openai-api-key YOUR_API_KEY --question "What is GPT-4?"
```

While convenient for quick tests, this method is not recommended for regular use as it exposes your API key in command history and logs.

### Method 3: Using Environment Variables

You can set the OpenAI API key using environment variables:

```bash
# Set API key as environment variable
export CHATX_OPENAI_API_KEY=YOUR_API_KEY

# Run ChatX without needing to specify the key
chatx --use-openai --question "What is GPT-4?"
```

This approach is useful for CI/CD pipelines or scripted usage.

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

### Basic Examples

Basic query using the OpenAI API (using configured API key):

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

### API Key Examples

Explicitly providing the API key for a one-time query:

```bash title="Using API key directly"
chatx --use-openai --openai-api-key sk-your-api-key --question "What are LLMs?"
```

Storing the API key in configuration for future use:

```bash title="Configuring API key"
# Store the API key
chatx config set openai.apiKey sk-your-api-key --user

# Use it implicitly
chatx --use-openai --question "What are LLMs?"
```

Using environment variable for the API key:

```bash title="Using environment variable"
export CHATX_OPENAI_API_KEY=sk-your-api-key
chatx --use-openai --question "What are LLMs?"
```

## Model Selection

### Available Models

The OpenAI API offers several models with different capabilities and price points:

| Model | Description | Use Cases | Token Context Window |
|-------|-------------|-----------|---------------------|
| gpt-4o | Latest multimodal model | General purpose, images, complex tasks | 128K tokens |
| gpt-4-turbo | Fast version of GPT-4 | Complex reasoning, code generation | 128K tokens |
| gpt-4 | Original powerful model | Detailed analysis, research | 8K tokens |
| gpt-4-32k | Extended context GPT-4 | Document analysis, long conversations | 32K tokens |
| gpt-3.5-turbo | Fast and economical model | Simple queries, high-volume use | 16K tokens |

By default, CHATX uses `gpt-4o` with the OpenAI API, but you can change this using the `--openai-chat-model-name` option or by setting `openai.chatModelName` in your configuration.

### Selecting Models with Command Line Options

To specify which model to use for a chat session, use the `--openai-chat-model-name` option:

```bash
chatx --use-openai --openai-chat-model-name gpt-4 --question "Explain quantum computing"
```

### Configuring Default Model

You can set your preferred default model using the configuration system:

```bash
# Set default model in user configuration
chatx config set openai.chatModelName gpt-4-turbo --user
```

### Model Selection Guidance

Here are some guidelines for choosing the appropriate model:

1. **For everyday use**: `gpt-4o` offers a good balance of capabilities and performance
2. **For budget-conscious use**: `gpt-3.5-turbo` is significantly cheaper but less capable
3. **For complex reasoning or code**: `gpt-4` or `gpt-4-turbo` provide the best results
4. **For processing long documents**: Models with larger context windows like `gpt-4-32k`

### Examples for Different Use Cases

```bash title="Using GPT-4o for general tasks (default)"
chatx --use-openai --question "Explain what LLMs are"
```

```bash title="Using GPT-3.5-Turbo for simple, cost-effective queries"
chatx --use-openai --openai-chat-model-name gpt-3.5-turbo --question "What is the capital of France?"
```

```bash title="Using GPT-4 for complex reasoning"
chatx --use-openai --openai-chat-model-name gpt-4 --question "Compare and contrast quantum computing with classical computing"
```

```bash title="Using GPT-4-32K for document analysis"
chatx --use-openai --openai-chat-model-name gpt-4-32k --question "Summarize this long document"
```

## API Key Security Best Practices

Your OpenAI API key provides access to paid services and should be handled securely:

1. **Use the configuration system**: Store your API key using `chatx config set` with the `--user` scope instead of directly in commands
2. **Don't commit API keys**: Never store API keys in source code repositories
3. **Rotate keys regularly**: Change your API keys periodically, especially if you suspect they might have been compromised
4. **Set usage limits**: Configure [usage limits in the OpenAI dashboard](https://platform.openai.com/account/billing/limits) to prevent unexpected charges
5. **Use separate keys**: Consider using different API keys for different projects or environments

## Troubleshooting

If you encounter issues with the OpenAI API, try these steps:

1. Verify your API key is valid
2. Check your OpenAI account has sufficient credits
3. Ensure you're using a supported model name
4. Check for API rate limits or quotas
5. Try explicitly using the `--openai-api-key` parameter to rule out configuration issues

For more detailed information, refer to the [OpenAI API documentation](https://platform.openai.com/docs/api-reference).