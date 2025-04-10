# --openai-chat-model-name

The `--openai-chat-model-name` option allows you to specify which OpenAI model to use for your chat session. This option lets you choose between different GPT models with varying capabilities, performance characteristics, and pricing.

## Syntax

```bash
chatx --openai-chat-model-name NAME [other options]
```

Where `NAME` is the OpenAI model identifier (e.g., "gpt-4o", "gpt-4", "gpt-3.5-turbo").

## Description

When using OpenAI as your AI provider, CHATX defaults to using the "gpt-4o" model. However, you might want to use different models for various reasons, such as:

- Using a more powerful model for complex tasks
- Using a cheaper, faster model for simpler queries
- Testing your prompts with different model capabilities
- Using specialized models for specific tasks

The `--openai-chat-model-name` option lets you override the default and specify exactly which model to use.

## Common Model Options

| Model | Description | Use Cases |
|-------|-------------|-----------|
| gpt-4o | Latest multimodal model (default) | General purpose, images, complex tasks |
| gpt-4 | Powerful instruction-following model | Complex reasoning, detailed outputs |
| gpt-4-turbo | Faster variant of GPT-4 | Complex tasks with better performance |
| gpt-3.5-turbo | Fast and economical model | Simple queries, high-volume use |

For the most up-to-date list of available models, refer to the [OpenAI Models documentation](https://platform.openai.com/docs/models).

## Examples

### Specifying a More Powerful Model

```bash
chatx --use-openai --openai-chat-model-name gpt-4 --question "Explain quantum computing in detail"
```

### Using a Faster, More Economical Model

```bash
chatx --use-openai --openai-chat-model-name gpt-3.5-turbo --question "What is the capital of France?"
```

### Using with Other OpenAI Options

```bash
chatx --use-openai --openai-api-key YOUR_API_KEY --openai-chat-model-name gpt-4-turbo --question "Analyze this code snippet"
```

### In an Interactive Session

```bash
chatx --use-openai --openai-chat-model-name gpt-4o --interactive
```

## Configuration

Instead of specifying the model in each command, you can set a default model in your configuration:

```bash
# Set default model in user configuration
chatx config set openai.chatModelName gpt-4-turbo --user

# Then use ChatX without needing to specify the model each time
chatx --use-openai --question "What is machine learning?"
```

### Configuration Scopes

You can set the default model at different configuration scopes:

```bash
# For the current directory only (local scope)
chatx config set openai.chatModelName gpt-4o --local

# For all directories for the current user (user scope)
chatx config set openai.chatModelName gpt-4o --user

# For all users on the system (global scope)
chatx config set openai.chatModelName gpt-4o --global
```

## Environment Variables

You can also set your default OpenAI model using the `CHATX_OPENAI_CHAT_MODEL_NAME` environment variable:

```bash
export CHATX_OPENAI_CHAT_MODEL_NAME=gpt-4-turbo
chatx --use-openai --question "What is the theory of relativity?"
```

## Cost Considerations

Different models have different pricing structures. More capable models like "gpt-4" tend to cost more per token than models like "gpt-3.5-turbo". Consider your needs and budget when selecting a model.

To view token usage and estimated cost for your session, you can use the `/cost` slash command during an interactive chat.

## Related Options

- `--use-openai`: Selects OpenAI as the provider
- `--openai-api-key`: Specifies your OpenAI API key
- `--max-tokens`: Limits the maximum number of tokens in the response

## See Also

- [OpenAI Provider Configuration](/providers/openai)
- [Configuration Guide](/usage/configuration)