# GitHub Copilot Provider

CHATX can leverage GitHub Copilot's AI capabilities through its API. This guide will help you set up and use GitHub Copilot with CHATX.

## Prerequisites

1. A GitHub account with an active GitHub Copilot subscription
2. CHATX installed on your system

## Authentication

Unlike OpenAI and Azure OpenAI which use API keys, GitHub Copilot authentication is handled through GitHub's OAuth flow. CHATX makes this process easy with the `github login` command:

```bash
chatx github login
```

This command will:

1. Open a browser window for you to authorize CHATX with GitHub
2. Save the GitHub token to your user configuration
3. Allow CHATX to use GitHub Copilot for AI interactions

After successful authentication, CHATX will automatically use your GitHub credentials when needed.

## Command-Line Options

You can explicitly tell CHATX to use GitHub Copilot as the provider:

```bash
chatx --use-copilot --question "What is GitHub Copilot?"
```

You can also specify which Copilot model to use:

```bash
chatx --use-copilot --copilot-model-name claude-3.7-sonnet --question "Explain quantum computing"
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

## Advanced Configuration

If needed, you can manually configure GitHub Copilot settings:

```bash
# Set the Copilot model
chatx config set copilot.modelName claude-3.7-sonnet --user

# Set custom API endpoint (rarely needed)
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

## Renewing Authentication

GitHub tokens eventually expire. If you encounter authentication errors, simply run the login command again:

```bash
chatx github login
```

## Troubleshooting

If you encounter issues with GitHub Copilot in CHATX, try these steps:

1. Verify your GitHub account has an active Copilot subscription
2. Re-authenticate using `chatx github login`
3. Check your internet connection
4. Verify your GitHub token hasn't expired

For more detailed information about GitHub Copilot, refer to the [GitHub Copilot documentation](https://docs.github.com/en/copilot).