# --use-copilot

The `--use-copilot` option explicitly selects GitHub Copilot as the chat provider for your CHATX session.

## Synopsis

```bash
chatx --use-copilot [other options]
```

## Description

When you specify `--use-copilot`, CHATX will:

- Use GitHub Copilot for the chat session
- Override any default provider settings
- Use either token-based or HMAC authentication, depending on what's configured
- Default to using the `claude-3.7-sonnet` model unless otherwise specified

This option is useful when you want to explicitly use GitHub Copilot for a specific command, regardless of your default provider settings.

## Related Copilot Options

The `--use-copilot` option can be used with the following GitHub Copilot specific options:

- `--copilot-model-name`: Specifies which model to use (default: claude-3.7-sonnet)
- `--copilot-api-endpoint`: Specifies a custom API endpoint (default: https://api.githubcopilot.com)
- `--copilot-integration-id`: Provides the integration ID (for HMAC authentication)
- `--copilot-hmac-key`: Provides the HMAC key (for HMAC authentication)
- `--github-token`: Provides a specific GitHub authentication token

## Authentication

Before using `--use-copilot`, you need to authenticate with GitHub using one of these methods:

1. **Token Authentication** (Recommended):
   ```bash
   chatx github login
   ```
   This will open a browser for you to authorize CHATX with GitHub.

2. **HMAC Authentication** (For specialized scenarios):
   ```bash
   chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
   chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user
   ```

## Examples

**Basic usage:**

```bash
chatx --use-copilot --question "What is GitHub Copilot?"
```

**Specifying a different model:**

```bash
chatx --use-copilot --copilot-model-name claude-3-opus --question "Explain quantum computing"
```

**Interactive chat with GitHub Copilot:**

```bash
chatx --use-copilot --interactive
```

**Using with custom API endpoint:**

```bash
chatx --use-copilot \
      --copilot-api-endpoint "https://custom-endpoint.example.com" \
      --question "How does GitHub Copilot work?"
```

## Configuration

Instead of specifying this option on each command, you can set GitHub Copilot as your default provider using:

```bash
chatx config set app.preferredProvider copilot --user
```

Or create a profile for using GitHub Copilot by creating a YAML file:

```yaml title="copilot.yaml (in .chatx/profiles directory)"
app:
  preferredProvider: "copilot"

copilot:
  modelName: "claude-3.7-sonnet"
```

Then use it with:

```bash
chatx --profile copilot --question "Your question"
```

## Notes

- If both configuration and command-line options are present, the command-line option takes precedence.
- Make sure you have a valid GitHub Copilot subscription and have authenticated before using this option.
- `--use-copilot` will use either token or HMAC authentication, depending on what's configured.
- For more specific authentication methods, use `--use-copilot-token` or `--use-copilot-hmac`.

## See Also

- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-azure-openai](./use-azure-openai.md) (for using the Azure OpenAI API instead)
- [GitHub Copilot Provider Documentation](../../../providers/github-copilot.md)
- [Provider Selection Guide](../../../providers/index.md)