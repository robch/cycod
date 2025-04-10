# --use-copilot-token

The `--use-copilot-token` option explicitly selects GitHub Copilot with token authentication as the chat provider for your CHATX session.

## Synopsis

```bash
chatx --use-copilot-token [other options]
```

## Description

When you specify `--use-copilot-token`, CHATX will:

- Use GitHub Copilot for the chat session
- Specifically use token-based authentication (OAuth token)
- Override any default provider settings
- Default to using the `claude-3.7-sonnet` model unless otherwise specified

Token authentication is the standard and recommended way to authenticate with GitHub Copilot. This option ensures that CHATX specifically uses a GitHub token (instead of HMAC authentication) even if both authentication methods are configured.

## Authentication Prerequisite

Before using `--use-copilot-token`, you need to authenticate with GitHub:

```bash
chatx github login
```

This will:
1. Open a browser window for you to authorize CHATX with GitHub
2. Save the token to your user configuration
3. Allow CHATX to use GitHub Copilot for AI interactions

## Related Options

The `--use-copilot-token` option can be used with the following GitHub Copilot specific options:

- `--copilot-model-name`: Specifies which model to use (default: claude-3.7-sonnet)
- `--copilot-api-endpoint`: Specifies a custom API endpoint (default: https://api.githubcopilot.com)
- `--github-token`: Provides a specific GitHub authentication token directly

## When to Use Token Authentication

Token authentication is the recommended method for most scenarios:

- **Interactive use**: When you have access to a browser for authentication
- **Personal development**: For everyday use on your development machine
- **Standard environments**: Where GitHub authentication is straightforward
- **Multiple sessions**: The token can be reused across multiple commands

## Examples

**Basic usage with token authentication:**

```bash
# After logging in with 'chatx github login'
chatx --use-copilot-token --question "What is GitHub Copilot?"
```

**Specifying a different model:**

```bash
chatx --use-copilot-token --copilot-model-name claude-3-opus --question "Explain quantum computing"
```

**Interactive chat with GitHub Copilot:**

```bash
chatx --use-copilot-token --interactive
```

**Using with custom API endpoint:**

```bash
chatx --use-copilot-token \
      --copilot-api-endpoint "https://custom-endpoint.example.com" \
      --question "How does GitHub Copilot work?"
```

**Providing a specific GitHub token:**

```bash
chatx --use-copilot-token \
      --github-token ghp_YOUR_TOKEN_HERE \
      --question "Create a sample React component"
```

## Configuration

You can set GitHub Copilot with token authentication as your default provider:

```bash
chatx config set app.preferredProvider copilot --user
chatx config set copilot.authType token --user
```

Or create a profile specifically for using GitHub Copilot with token authentication:

```bash
chatx --use-copilot-token --save-profile copilot-token
```

Then use it with:

```bash
chatx --profile copilot-token --question "Your question"
```

## Renewing Authentication

GitHub tokens eventually expire. If you encounter authentication errors, simply run the login command again:

```bash
chatx github login
```

## Notes

- Token authentication is the recommended method for most users
- Unlike HMAC authentication, token authentication requires browser access for the initial login
- The token is stored securely in your user configuration
- If you have both token and HMAC authentication configured, using `--use-copilot-token` explicitly chooses token authentication
- For specialized scenarios without browser access, consider HMAC authentication with `--use-copilot-hmac` instead

## See Also

- [--use-copilot](./use-copilot.md) (for using Copilot with either authentication method)
- [--use-copilot-hmac](./use-copilot-hmac.md) (for using Copilot with HMAC authentication)
- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-azure-openai](./use-azure-openai.md) (for using the Azure OpenAI API instead)
- [GitHub Copilot Provider Documentation](/providers/github-copilot)