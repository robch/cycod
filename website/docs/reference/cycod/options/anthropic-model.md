# --anthropic-model-name

The `--anthropic-model-name` option allows you to specify which Claude model to use when using Anthropic as your provider.

## Syntax

```bash
--anthropic-model-name MODEL_NAME
```

## Parameters

| Parameter | Description |
|-----------|-------------|
| `MODEL_NAME` | The name of the Claude model to use (e.g., `claude-3-7-sonnet-20250219`, `claude-3-5-haiku-20241022`) |

## Description

This option specifies which model to use for generating responses when using Anthropic's Claude API. Different models offer varying capabilities, performance characteristics, and pricing.

When you specify `--anthropic-model-name`, CYCOD will:

- Use the specified Claude model for this chat session
- Override any default model configured in your settings

## Examples

### Basic usage with Direct Anthropic API

```bash
cycod --use-anthropic --anthropic-model-name claude-3-7-sonnet-20250219 --question "Explain quantum computing"
```

### Using a different model for a specific task

```bash
# Use a faster model for simple questions
cycod --use-anthropic --anthropic-model-name claude-3-5-haiku-20241022 --question "What is the capital of France?"

# Use a more powerful model for complex reasoning
cycod --use-anthropic --anthropic-model-name claude-3-7-sonnet-20250219 --question "Analyze the economic impacts of climate change"
```

## Configuration Alternative

For regular use, it's recommended to store your preferred model in the CYCOD configuration instead of passing it on the command line:

```bash
# Set your preferred Claude model in the user configuration
cycod config set Anthropic.ModelName claude-3-7-sonnet-20250219 --user
```

## Related Options

| Option | Description |
|--------|-------------|
| `--use-anthropic` | Explicitly select Anthropic Claude as the provider |
| `--anthropic-api-key` | Specify the authentication key for Anthropic API |

## See Also

- [Anthropic Provider Documentation](../../../providers/anthropic.md)
- [Provider Selection Guide](../../../providers/overview.md)