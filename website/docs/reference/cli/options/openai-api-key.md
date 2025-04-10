# --openai-api-key

The `--openai-api-key` option allows you to specify an OpenAI API key directly in your CHATX command. This is used to authenticate with the OpenAI API when using it as your chat provider.

## Syntax

```bash
chatx --openai-api-key KEY [other options]
```

Where `KEY` is your OpenAI API key (typically starting with "sk-").

## Description

When using OpenAI as your AI provider, CHATX needs to authenticate with the OpenAI API. The `--openai-api-key` option provides a way to specify your API key directly in the command.

While this option is convenient for quick testing or one-off commands, for regular use it's generally better to store your API key using the configuration system for security reasons.

## Examples

### Basic Usage

```bash
chatx --use-openai --openai-api-key sk-your-api-key-here --question "What is GPT-4?"
```

### Using with Other OpenAI Options

```bash
chatx --use-openai --openai-api-key sk-your-api-key-here --openai-chat-model-name gpt-4 --question "Explain quantum computing"
```

## Security Considerations

The `--openai-api-key` option is not recommended for regular use as it:

1. Makes your API key visible in command history
2. May expose the key to other users on multi-user systems
3. Risks the key being accidentally shared if commands are logged or shared

Instead, consider using the configuration system to store your API key securely:

```bash
# Store API key in user configuration (recommended)
chatx config set openai.apiKey YOUR_API_KEY --user

# Then use ChatX without needing to specify the key each time
chatx --use-openai --question "What is the capital of France?"
```

## Environment Variables

You can also set your OpenAI API key using the `CHATX_OPENAI_API_KEY` environment variable:

```bash
export CHATX_OPENAI_API_KEY=sk-your-api-key-here
chatx --use-openai --question "What is the capital of France?"
```

## Related Options

- `--use-openai`: Selects OpenAI as the provider
- `--openai-chat-model-name`: Specifies which OpenAI model to use

## See Also

- [OpenAI Provider Configuration](/providers/openai)
- [Configuration Guide](/usage/configuration)