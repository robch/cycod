# --anthropic-api-key

The `--anthropic-api-key` option allows you to specify an authentication key for Anthropic Claude API when using CYCOD.

## Syntax

```bash
--anthropic-api-key KEY
```

## Description

This option sets the API key used to authenticate with Anthropic's Claude models. When provided, CYCOD will use this key to authenticate requests to the Anthropic API, overriding any keys stored in your configuration.

The API key is sensitive information that grants access to paid services. While you can use this option for one-off commands or testing, for regular use it's recommended to store your key in the configuration system.

## Examples

### Example 1: Basic usage with Anthropic Claude

```bash
cycod --use-anthropic --anthropic-api-key "your-api-key" --question "What is Claude?"
```

### Example 2: Complete Anthropic configuration

```bash
cycod --use-anthropic \
      --anthropic-api-key "your-api-key" \
      --anthropic-model-name "claude-3-7-sonnet-20250219" \
      --question "Explain how Claude works"
```

### Example 3: Interactive chat with Anthropic Claude

```bash
cycod --use-anthropic \
      --anthropic-api-key "your-api-key" \
      --interactive
```

### Example 4: Using an environment variable (recommended for scripts)

```bash
# Set the key in an environment variable
export ANTHROPIC_API_KEY="your-api-key"

# Use the environment variable in your command
cycod --use-anthropic --anthropic-api-key "$ANTHROPIC_API_KEY" --question "What is Claude?"
```

## Configuration Alternative

For regular use, it's recommended to store your API key in the CYCOD configuration instead of passing it on the command line:

```bash
# Store the API key (recommended approach)
cycod config set Anthropic.ApiKey your-api-key --user
```

## Configuration Precedence

When CYCOD looks for the Anthropic API key, it checks sources in this order:

1. Command-line option (`--anthropic-api-key`)
2. Environment variable (`ANTHROPIC_API_KEY`)
3. Configuration files (local, then user, then global scope)

Using the command-line option will override any value set in environment variables or configuration files.

## Security Best Practices

Your Anthropic API key provides access to paid AI services and should be handled carefully:

1. Store your API key securely using the configuration system
2. Never hard-code API keys in scripts that might be shared or committed to version control
3. For automated workflows, use environment variables instead of command-line options
4. Consider using a dedicated key with usage limits for testing or development

## Related Options

| Option | Description |
|--------|-------------|
| `--use-anthropic` | Explicitly select Anthropic Claude as the provider |
| `--anthropic-model-name` | Specify which Claude model to use |

## See Also

- [Anthropic Provider](../../../providers/anthropic.md)
- [Configuration](../../../usage/configuration.md)