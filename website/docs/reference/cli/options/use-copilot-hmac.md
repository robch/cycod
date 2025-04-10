# --use-copilot-hmac

The `--use-copilot-hmac` option explicitly selects GitHub Copilot with HMAC authentication as the chat provider for your CHATX session.

## Synopsis

```bash
chatx --use-copilot-hmac [other options]
```

## Description

When you specify `--use-copilot-hmac`, CHATX will:

- Use GitHub Copilot for the chat session
- Specifically use HMAC authentication (rather than token-based)
- Override any default provider settings
- Default to using the `claude-3.7-sonnet` model unless otherwise specified

HMAC authentication provides an alternative to the standard OAuth token authentication for GitHub Copilot. This option is particularly useful in environments where the normal login flow isn't possible or practical.

## Required Configuration

To use `--use-copilot-hmac`, you need to configure these two required values:

- `copilot.hmacKey`: The HMAC authentication key
- `copilot.integrationId`: Your integration ID

You can provide these either through configuration or directly in the command.

## Related Options

The `--use-copilot-hmac` option is typically used with the following options:

- `--copilot-hmac-key`: Provides the HMAC key directly in the command
- `--copilot-integration-id`: Provides the integration ID directly in the command
- `--copilot-model-name`: Specifies which model to use (default: claude-3.7-sonnet)
- `--copilot-api-endpoint`: Specifies a custom API endpoint (default: https://api.githubcopilot.com)

## When to Use HMAC Authentication

HMAC authentication is more suitable than token-based authentication in scenarios such as:

- **CI/CD pipelines**: When running automated scripts without user interaction
- **Air-gapped environments**: Where direct GitHub authentication isn't possible
- **Corporate environments**: With specific authentication policies
- **Automated systems**: Where persistent authentication is needed without browser flow
- **Headless environments**: Where opening a browser for authentication isn't possible

## Examples

**Basic usage with HMAC authentication:**

```bash
# Using configured HMAC credentials
chatx --use-copilot-hmac --question "What is GitHub Copilot?"

# Providing HMAC credentials directly
chatx --use-copilot-hmac \
      --copilot-hmac-key YOUR_HMAC_KEY \
      --copilot-integration-id YOUR_INTEGRATION_ID \
      --question "What is GitHub Copilot?"
```

**Using a specific model:**

```bash
chatx --use-copilot-hmac \
      --copilot-model-name claude-3-opus \
      --question "Explain quantum computing"
```

**Using in a CI pipeline:**

```bash
# Example in a GitHub Actions workflow
chatx --use-copilot-hmac \
      --copilot-hmac-key ${{ secrets.COPILOT_HMAC_KEY }} \
      --copilot-integration-id ${{ secrets.COPILOT_INTEGRATION_ID }} \
      --question "Analyze this code"
```

**Interactive chat with custom API endpoint:**

```bash
chatx --use-copilot-hmac \
      --copilot-api-endpoint "https://custom-endpoint.example.com" \
      --interactive
```

## Configuration

Instead of providing the HMAC credentials with each command, you can configure them:

```bash
# Configure HMAC authentication details
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Optionally set other Copilot settings
chatx config set copilot.modelName claude-3-opus --user
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

You can also create a profile for using GitHub Copilot with HMAC:

```bash
chatx --use-copilot-hmac --save-profile copilot-hmac
```

Then use it with:

```bash
chatx --profile copilot-hmac --question "Your question"
```

## Notes

- `--use-copilot-hmac` explicitly forces HMAC authentication, unlike the general `--use-copilot` option
- HMAC keys are sensitive credentials and should be handled securely
- Command-line options take precedence over configured values
- When using in CI/CD pipelines, store HMAC credentials as secrets

## See Also

- [--use-copilot](./use-copilot.md) (for using Copilot with either authentication method)
- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-azure-openai](./use-azure-openai.md) (for using the Azure OpenAI API instead)
- [GitHub Copilot Provider Documentation](/providers/github-copilot)