# --google-gemini-api-key

The `--google-gemini-api-key` option allows you to specify your Google Gemini API key when using CYCOD.

## Syntax

```bash
--google-gemini-api-key KEY
```

## Description

This option sets the API key used to authenticate with Google Gemini services. When provided, CYCOD will use this key to authenticate requests to the Gemini API, overriding any keys stored in your configuration.

The API key is required for accessing Google Gemini models and must be obtained from the [Google AI Studio](https://ai.google.dev/) or the Google Cloud Console if using Vertex AI.

## Parameters

| Parameter | Description |
|-----------|-------------|
| `KEY` | Your Google Gemini API key |

## Examples

### Example 1: Basic usage with Gemini

```bash
cycod --use-gemini --google-gemini-api-key "your-api-key" --question "What is Google Gemini?"
```

### Example 2: Complete Gemini configuration

```bash
cycod --use-gemini \
      --google-gemini-api-key "your-api-key" \
      --google-gemini-model-id "gemini-pro" \
      --question "Explain how Gemini models work"
```

### Example 3: Interactive chat with Gemini

```bash
cycod --use-gemini \
      --google-gemini-api-key "your-api-key" \
      --interactive
```

### Example 4: Using an environment variable (recommended for scripts)

```bash
# Set the key in an environment variable
export GOOGLE_GEMINI_API_KEY="your-api-key"

# Use the environment variable in your command
cycod --use-gemini --google-gemini-api-key "$GOOGLE_GEMINI_API_KEY" --question "What is Gemini?"
```

## Configuration Alternative

For regular use, it's recommended to store your API key in the CYCOD configuration instead of passing it on the command line:

```bash
# Store your Gemini API key in the user configuration
cycod config set Google.Gemini.ApiKey "your-api-key" --user
```

## Configuration Precedence

When CYCOD looks for the Gemini API key, it checks sources in this order:

1. Command-line option (`--google-gemini-api-key`)
2. Environment variable (`GOOGLE_GEMINI_API_KEY`)
3. Configuration files (local, then user, then global scope)

Using the command-line option will override any value set in environment variables or configuration files.

## Security Considerations

Your Gemini API key is a sensitive credential that grants access to paid services. Consider these security best practices:

- Store your API key in user configuration rather than passing it directly in commands
- Never store your API key in source control or public repositories
- Regularly rotate your API keys following your organization's security policies
- Use environment variables when scripting

## Related Options

| Option | Description |
|--------|-------------|
| `--use-gemini` | Explicitly select Google Gemini as the provider |
| `--google-gemini-model-id` | Specify which Gemini model to use |

## See Also

- [Google Gemini Provider](../../../providers/gemini.md)
- [Configuration](../../../usage/configuration.md)