# github Command

The `cycod github` command allows you to manage GitHub-related features in CycoD, particularly for GitHub Copilot integration.

## Syntax

```bash
cycod github SUBCOMMAND
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`login`](login.md) | Authenticate with GitHub to access Copilot features |

## GitHub Integration

CycoD integrates with GitHub to provide access to GitHub Copilot features. This integration requires authentication with GitHub to obtain the necessary access tokens.

The `github login` command initiates the GitHub device authentication flow, which:

1. Generates a device code and verification URL
2. Walks you through the authentication process
3. Stores the authentication token in your configuration

## Examples

Authenticate with GitHub:

```bash
cycod github login
```

## Notes

- GitHub authentication is required to use GitHub Copilot features in CycoD
- The GitHub token is stored securely in your configuration
- After authentication, use the `--use-copilot` flag to select GitHub Copilot as your AI provider
- You can check your authentication status with `cycod config get GitHub.Token --any`
- To log out, use `cycod config clear GitHub.Token --user`
- A valid GitHub Copilot subscription is required to use this feature

## See Also

- [GitHub Copilot Provider](/providers/github-copilot.md) - Learn more about using GitHub Copilot with CycoD
- [--use-copilot](../options/use-copilot.md) - Flag to use GitHub Copilot as the AI provider