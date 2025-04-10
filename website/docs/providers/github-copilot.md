# GitHub Copilot Provider

CHATX can leverage GitHub Copilot's AI capabilities through its API. This guide will help you set up and use GitHub Copilot with CHATX.

## Prerequisites

1. A GitHub account with an active GitHub Copilot subscription
2. CHATX installed on your system

## Authentication

CHATX offers two authentication methods for GitHub Copilot:

### Method 1: GitHub Token Authentication (Recommended)

Unlike OpenAI and Azure OpenAI which use API keys, GitHub Copilot's primary authentication method is handled through GitHub's OAuth flow. ChatX makes this process easy with the `github login` command:

```bash
chatx github login
```

This command will:

1. Generate a device code and verification URL for you to authorize ChatX with GitHub
2. Save the GitHub token to your user configuration
3. Allow CHATX to use GitHub Copilot for AI interactions

After successful authentication, CHATX will automatically use your GitHub credentials when needed.

#### When to Use Token Authentication

Token authentication is recommended for most users because:

- It's the standard GitHub authentication mechanism
- It handles token refresh and expiration automatically
- It integrates with your existing GitHub account
- It's more secure than storing static credentials

#### Verifying Token Authentication

To check if your token authentication is working:

```bash
# Test with a simple query
chatx --use-copilot-token --question "Hello, am I authenticated?"
```

If you see an error message about authentication, run `chatx github login` again to refresh your token.

### Method 2: HMAC Authentication

For specialized scenarios or environments where the OAuth flow isn't feasible, CHATX also supports HMAC-based authentication. This requires:

1. A Copilot HMAC key
2. A Copilot integration ID

To use HMAC authentication:

```bash
# Configure the required credentials
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Then use with the appropriate flag
chatx --use-copilot-hmac --question "What is ChatX?"
```

You can also provide these credentials directly in the command:

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --question "What is ChatX?"
```

HMAC authentication may be necessary in environments like:
- CI/CD pipelines
- Automated scripts
- Air-gapped environments
- Corporate environments with specific authentication policies

For detailed instructions on setting up and using HMAC authentication, see the [HMAC Authentication Tutorial](../tutorials/github-copilot-hmac-auth.md).

## Command-Line Options

### Provider Selection Flags

You can explicitly tell CHATX to use GitHub Copilot as the provider:

```bash
# Use GitHub Copilot (either token or HMAC auth, depending on available credentials)
chatx --use-copilot --question "What is GitHub Copilot?"

# Specifically use token-based authentication (requires 'chatx github login' first)
chatx --use-copilot-token --question "What is GitHub Copilot?"

# Specifically use HMAC-based authentication (requires HMAC key and integration ID)
chatx --use-copilot-hmac --question "What is GitHub Copilot?"
```

#### Token vs HMAC Authentication Flags

These flags give you control over the authentication method:

- **`--use-copilot`**: Uses either token or HMAC auth, whichever is available (token is preferred)
- **`--use-copilot-token`**: Explicitly uses token authentication only
- **`--use-copilot-hmac`**: Explicitly uses HMAC authentication only

When to use `--use-copilot-token` specifically:
- When you have both token and HMAC configured but want to ensure token auth is used
- In environments where you want to fail if token auth isn't available
- When creating aliases or profiles specifically for token-based authentication
- When you want to be explicit about which authentication method is being used

### Model and Configuration Options

You can specify which Copilot model to use:

```bash
chatx --use-copilot --copilot-model-name claude-3.7-sonnet --question "Explain quantum computing"
```

When using HMAC authentication, you'll need to provide the required credentials:

```bash
chatx --use-copilot-hmac --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_ID --question "Hello world"
```

## Available Models

GitHub Copilot currently offers access to these models through CHATX:

| Model | Description | Use Cases |
|-------|-------------|-----------|
| claude-3.7-sonnet | Default model | General purpose, code, reasoning |
| claude-3-opus | More capable model | Complex reasoning, detailed outputs |
| claude-3-sonnet | Balanced model | Good mix of speed and capability |

By default, CHATX uses `claude-3.7-sonnet` with GitHub Copilot, but you can change this using the `--copilot-model-name` option.

## Example Usage

Basic query using GitHub Copilot:

```bash title="Basic query"
chatx --use-copilot --question "Explain what GitHub Copilot is"
```

Interactive chat with GitHub Copilot:

```bash title="Interactive chat"
chatx --use-copilot --interactive
```

### Token Authentication Examples

If you've authenticated with `chatx github login`, you can use these examples with token authentication:

```bash title="Explicit token authentication"
# Basic usage with token auth
chatx --use-copilot-token --question "Write a Python function to calculate Fibonacci numbers"

# Using a specific model with token auth
chatx --use-copilot-token --copilot-model-name claude-3-opus --question "Explain quantum computing"

# Interactive chat with token auth
chatx --use-copilot-token --interactive

# Saving a profile for token auth
chatx --use-copilot-token --save-profile copilot-token
```

You can also provide a specific GitHub token directly (useful for automation):

```bash title="Using a specific GitHub token"
chatx --use-copilot-token --github-token ghp_YOUR_TOKEN_HERE --question "Create a React component"
```

For most users, the regular `--use-copilot` flag is sufficient as it will use token authentication if available.

## Advanced Configuration

### Basic Settings

If needed, you can manually configure GitHub Copilot settings:

```bash
# Set the Copilot model
chatx config set copilot.modelName claude-3.7-sonnet --user

# Set custom API endpoint (rarely needed)
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user

# Alternatively, you can specify the endpoint directly in the command
chatx --copilot-api-endpoint https://api.githubcopilot.com --question "Write a Python function that sorts a list"
```

### HMAC Authentication Setup

To configure HMAC authentication for GitHub Copilot:

```bash
# Set the HMAC key (required for HMAC auth)
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user

# Set the integration ID (required for HMAC auth)
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Set the model name (optional)
chatx config set copilot.modelName claude-3-opus --user

# Set custom API endpoint (optional)
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

With these settings in place, you can use:

```bash
chatx --use-copilot-hmac --question "Hello world"
```

Or continue to provide the credentials directly:

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_ID --question "Hello world"
```

### Custom API Endpoint

The default GitHub Copilot API endpoint (`https://api.githubcopilot.com`) works for most users. However, you might need to specify a different endpoint if:

1. Your organization has a custom GitHub Copilot deployment
2. You're working in an air-gapped environment with a local Copilot service
3. You're testing against a staging or development Copilot environment

To use a custom endpoint:

```bash
# For a single command
chatx --use-copilot --copilot-api-endpoint https://custom-endpoint.example.com --question "Hello"

# Or permanently in your configuration
chatx config set copilot.apiEndpoint https://custom-endpoint.example.com --user
```

## Renewing Authentication

GitHub tokens eventually expire. If you encounter authentication errors, simply run the login command again:

```bash
chatx github login
```

## Troubleshooting

If you encounter issues with GitHub Copilot in ChatX, try these steps:

1. Verify your GitHub account has an active Copilot subscription
2. Re-authenticate using `chatx github login`
3. Check your internet connection
4. Verify your GitHub token hasn't expired

For more detailed information about GitHub Copilot, refer to the [GitHub Copilot documentation](https://docs.github.com/en/copilot).

## Related Topics

- [Authenticating with GitHub Copilot Tutorial](../tutorials/github-copilot-auth.md) - Step-by-step guide to setting up GitHub authentication
- [`github login` Command Reference](../reference/cli/github/login.md) - Detailed documentation of the login command
- [`--use-copilot` Option](../reference/cli/options/use-copilot.md) - How to specify GitHub Copilot as your AI provider