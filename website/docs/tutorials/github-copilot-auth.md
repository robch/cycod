# Authenticating with GitHub Copilot

This tutorial walks you through the process of setting up GitHub authentication for ChatX to use GitHub Copilot as your AI provider.

## Prerequisites

Before starting this tutorial, make sure you have:

- ChatX installed ([installation guide](../install-chatx-cli.md))
- A GitHub account with an active GitHub Copilot subscription

## Understanding GitHub Authentication

ChatX offers integration with GitHub Copilot, giving you access to powerful AI models like Claude from Anthropic.

This tutorial covers the standard token-based authentication using the `github login` command.

The token-based authentication process:

1. Creates a secure connection between ChatX and your GitHub account
2. Grants ChatX permission to access GitHub Copilot on your behalf
3. Enables you to use Copilot's AI capabilities through ChatX

## Step 1: Run the authentication command

To authenticate with GitHub, open your terminal and run:

```bash
chatx github login
```

## Step 2: Complete the device authentication flow

When you run the command, you'll see output similar to:

```
Please visit https://github.com/login/device and enter code ABCD-1234 to authenticate.
```

1. Open the provided URL in your web browser
2. Sign in to your GitHub account if prompted
3. Enter the device code shown in your terminal
4. Review and confirm the permissions requested by ChatX

## Step 3: Wait for confirmation

After you authorize ChatX in your browser, the command will automatically detect the successful authentication and store the token securely.

You'll see a confirmation message in your terminal:

```
GitHub authentication successful!
GitHub login successful! You can now use chatx with GitHub Copilot.
```

## Step 4: Using GitHub Copilot with ChatX

Now that you've authenticated, you can start using GitHub Copilot with ChatX:

```bash
# Ask a simple question using GitHub Copilot
chatx --use-copilot --question "What is GitHub Copilot?"

# Start an interactive chat session with GitHub Copilot
chatx --use-copilot
```

You can also create an alias for easier access:

```bash
# Create an alias for using GitHub Copilot
chatx --use-copilot --save-alias copilot

# Then use it like this
chatx --copilot --question "Explain quantum computing"
```

## Understanding Token Storage

By default, the GitHub token is stored in your user-level configuration, making it available across all directories and projects for your user account.

You can check if your token is properly stored:

```bash
chatx config get GitHub.Token --any
```

If you need to log out or revoke ChatX's access to your GitHub account:

```bash
chatx config clear GitHub.Token --user
```

## Troubleshooting

If you encounter issues with GitHub authentication:

- **Authentication timeout**: If you take too long to complete the authentication process, the device code might expire. Simply run `chatx github login` again to generate a new code.

- **Already authenticated**: If you see an error indicating you're already authenticated, you may want to clear the existing token first with `chatx config clear GitHub.Token --user` and then run the login command again.

- **Subscription issues**: If you encounter "subscription required" errors, verify your GitHub Copilot subscription is active in your GitHub account settings.

- **Token expires**: If you find that ChatX can no longer access GitHub Copilot, your token may have expired. Simply run `chatx github login` again to get a fresh token.

## Next Steps

Now that you've authenticated with GitHub Copilot, learn more about:

- [GitHub Copilot Provider Options](../providers/github-copilot.md)
- [Creating Custom Aliases](creating-user-aliases.md)
- [Working with Configuration](../usage/configuration.md)

Remember that GitHub Copilot requires a valid subscription. If you don't have one, you can still use ChatX with other providers like OpenAI or Azure OpenAI.