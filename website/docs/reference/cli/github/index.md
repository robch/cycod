# github Command

The `chatx github` command allows you to manage GitHub-related features in CHATX, particularly for GitHub Copilot integration.

## Syntax

```bash
chatx github SUBCOMMAND [options]
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`login`](login.md) | Authenticate with GitHub to access Copilot features |

## Options

Each subcommand has its own specific options. See the subcommand's documentation for details.

## GitHub Integration

CHATX integrates with GitHub to provide access to GitHub Copilot features. This integration requires authentication with GitHub to obtain the necessary access tokens.

## Examples

Authenticate with GitHub:

```bash
chatx github login
```

Authenticate with GitHub without automatically opening a browser:

```bash
chatx github login --no-browser
```

## Notes

- GitHub authentication is required to use GitHub Copilot features in CHATX
- The GitHub token is stored securely in your user configuration
- You can check your authentication status with `chatx config get github.token`
- To log out, use `chatx config clear github.token --user`