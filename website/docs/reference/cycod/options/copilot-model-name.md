# --copilot-model-name

## Description

The `--copilot-model-name` option allows you to specify which AI model to use when working with GitHub Copilot. This gives you control over the capabilities, performance, and cost of the AI model powering your interactions.

Different models offer varying levels of capabilities, response quality, and processing speeds, allowing you to choose the most appropriate model for your specific needs.

## Syntax

```
--copilot-model-name MODEL_NAME
```

Where `MODEL_NAME` is the name of the GitHub Copilot model you want to use.

## Available Models

As of the current version, GitHub Copilot supports several models including:

- `claude-3.5-sonnet` (default)
- `claude-3-opus`
- `claude-3-sonnet`
- `claude-3-haiku`
- `gpt-4o`
- `gpt-4-turbo`

The availability of specific models may change over time as GitHub Copilot evolves.

## Default Value

If not specified, CYCOD uses `claude-3.5-sonnet` as the default model when using GitHub Copilot.

## Environment Variable

This option corresponds to the `COPILOT_MODEL_NAME` environment variable.

## Examples

### Example 1: Using a specific model

```bash
cycod --use-copilot --copilot-model-name claude-3-opus --question "Explain quantum computing in detail"
```

### Example 2: Saving a preferred model in configuration

```bash
cycod config set copilot.modelName claude-3-opus --user
```

## Configuration

You can set a default model name in your configuration:

```bash
cycod config set copilot.modelName MODEL_NAME --user
```

After configuring this setting, CYCOD will use your specified model when accessing GitHub Copilot, unless overridden by the command line option.

## Choosing the Right Model

Different models have different strengths and characteristics:

| Model | Characteristics | Best For |
|-------|----------------|----------|
| `claude-3.5-sonnet` | Balanced performance and capabilities | General-purpose use, most tasks |
| `claude-3-opus` | Highest capability, may be slower | Complex reasoning, detailed analysis |
| `claude-3-sonnet` | Good balance of speed and quality | Most everyday tasks |
| `claude-3-haiku` | Fastest, less capability | Simple tasks, quick responses |
| `gpt-4o` | Strong general capabilities | Creative writing, coding tasks |
| `gpt-4-turbo` | Good speed and capabilities | Code generation, technical tasks |

## Related Options

| Option | Description |
|--------|-------------|
| `--use-copilot` | Explicitly selects GitHub Copilot as the provider |
| `--copilot-api-endpoint` | Specifies the API endpoint |

## See Also

- [GitHub Copilot Provider](../../../providers/github-copilot.md)
- [`--use-copilot`](use-copilot.md)