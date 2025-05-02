---
hide:
- toc
icon: material/github
---

# Setup GitHub Copilot manually

The `cycod config set` command allows you to configure your GitHub Copilot settings.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Setup GitHub configuration"
cycod config set github.token YOUR_GITHUB_TOKEN --user
cycod config set copilot.modelName YOUR_MODEL_NAME --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View Copilot config values"
cycod config get github.token
cycod config get copilot.apiEndpoint
cycod config get copilot.modelName
```

<!-- 
## Authentication

CYCOD provides authentication for GitHub Copilot through GitHub's OAuth flow.

### GitHub Token Authentication

Unlike OpenAI and Azure OpenAI which use API keys, GitHub Copilot's authentication method is handled through GitHub's OAuth flow. CycoD makes this process easy with the `github login` command:

```bash
cycod github login
```

This command will:

1. Generate a device code and verification URL for you to authorize CycoD with GitHub
2. Save the GitHub token to your user configuration
3. Allow CYCOD to use GitHub Copilot for AI interactions

After successful authentication, CYCOD will automatically use your GitHub credentials when needed.

Token authentication is recommended because:

- It's the standard GitHub authentication mechanism
- It handles token refresh and expiration automatically
- It integrates with your existing GitHub account
- It's more secure than storing static credentials

#### Verifying Token Authentication

To check if your token authentication is working:

```bash
# Test with a simple query
cycod --use-copilot --question "Hello, am I authenticated?"
```

If you see an error message about authentication, run `cycod github login` again to refresh your token.

## Command-Line Options

### Provider Selection Flags

You can explicitly tell CYCOD to use GitHub Copilot as the provider:

```bash
# Use GitHub Copilot with token authentication
cycod --use-copilot --question "What is GitHub Copilot?"
```

When to use `--use-copilot`:
- For standard GitHub Copilot interactions
- In environments where you've authenticated using `cycod github login`
- When creating aliases or profiles for GitHub Copilot
- When you want to explicitly specify GitHub Copilot as your provider

### Model and Configuration Options

You can specify which Copilot model to use:

```bash
cycod --use-copilot --copilot-model-name claude-3.7-sonnet --question "Explain quantum computing"
```

## Available Models

GitHub Copilot currently offers access to these models through CYCOD:

| Model | Description | Use Cases |
|-------|-------------|-----------|
| claude-3.7-sonnet | Default model | General purpose, code, reasoning |
| claude-3-opus | More capable model | Complex reasoning, detailed outputs |
| claude-3-sonnet | Balanced model | Good mix of speed and capability |

By default, CYCOD uses `claude-3.7-sonnet` with GitHub Copilot, but you can change this using the `--copilot-model-name` option.

## Example Usage

Basic query using GitHub Copilot:

```bash title="Basic query"
cycod --use-copilot --question "Explain what GitHub Copilot is"
```

Interactive chat with GitHub Copilot:

```bash title="Interactive chat"
cycod --use-copilot --interactive
```

### Token Authentication Examples

If you've authenticated with `cycod github login`, you can use these examples:

```bash title="GitHub Copilot usage"
# Basic usage
cycod --use-copilot --question "Write a Python function to calculate Fibonacci numbers"

# Using a specific model
cycod --use-copilot --copilot-model-name claude-3-opus --question "Explain quantum computing"

# Interactive chat
cycod --use-copilot --interactive
```

You can also provide a specific GitHub token directly (useful for automation):

```bash title="Using a specific GitHub token"
cycod --use-copilot --github-token ghp_YOUR_TOKEN_HERE --question "Create a React component"
```

## Advanced Configuration

### Basic Settings

If needed, you can manually configure GitHub Copilot settings:

```bash
# Set the Copilot model
cycod config set copilot.modelName claude-3.7-sonnet --user

# Set custom API endpoint (rarely needed)
cycod config set copilot.apiEndpoint https://api.githubcopilot.com --user

# Alternatively, you can specify the endpoint directly in the command
cycod --copilot-api-endpoint https://api.githubcopilot.com --question "Write a Python function that sorts a list"
```

### Custom API Endpoint

The default GitHub Copilot API endpoint (`https://api.githubcopilot.com`) works for most users. However, you might need to specify a different endpoint if:

1. Your organization has a custom GitHub Copilot deployment
2. You're working in an air-gapped environment with a local Copilot service
3. You're testing against a staging or development Copilot environment

To use a custom endpoint:

```bash
# For a single command
cycod --use-copilot --copilot-api-endpoint https://custom-endpoint.example.com --question "Hello"

# Or permanently in your configuration
cycod config set copilot.apiEndpoint https://custom-endpoint.example.com --user
```

## Renewing Authentication

GitHub tokens eventually expire. If you encounter authentication errors, simply run the login command again:

```bash
cycod github login
```

## Troubleshooting

If you encounter issues with GitHub Copilot in CycoD, try these steps:

1. Verify your GitHub account has an active Copilot subscription
2. Re-authenticate using `cycod github login`
3. Check your internet connection
4. Verify your GitHub token hasn't expired

For more detailed information about GitHub Copilot, refer to the [GitHub Copilot documentation](https://docs.github.com/en/copilot).

## Related Topics

- [Authenticating with GitHub Copilot Tutorial](../tutorials/github-copilot-auth.md) - Step-by-step guide to setting up GitHub authentication
- [`github login` Command Reference](../reference/cli/github/login.md) - Detailed documentation of the login command
- [`--use-copilot` Option](../reference/cli/options/use-copilot.md) - How to specify GitHub Copilot as your AI provider -->