# --github-token

Specifies a GitHub authentication token to use for GitHub Copilot integration.

## Syntax

```bash
chatx --github-token TOKEN [options]
```

## Description

The `--github-token` option allows you to directly specify a GitHub authentication token for accessing GitHub Copilot services. This is an alternative to the recommended `chatx github login` flow, which handles token acquisition and storage automatically.

This option is primarily useful in automated environments, CI/CD pipelines, or other scenarios where interactive authentication isn't possible or desirable.

## Parameters

| Parameter | Description |
|-----------|-------------|
| `TOKEN`   | A valid GitHub authentication token with access to GitHub Copilot services |

## Examples

Use a specific GitHub token for a single command:

```bash
chatx --use-copilot --github-token ghp_1234567890abcdefghijklmnopqrstuvwxyz --question "What is GitHub Copilot?"
```

Use an environment variable to provide the token:

```bash
# Bash/PowerShell
chatx --use-copilot --github-token $GITHUB_TOKEN --question "What is GitHub Copilot?"

# Windows Command Prompt
chatx --use-copilot --github-token %GITHUB_TOKEN% --question "What is GitHub Copilot?"
```

## Notes

- For interactive use, the `chatx github login` command is recommended instead of manually providing a token
- The token provided with `--github-token` takes precedence over any token stored via `chatx github login`
- Be cautious when using this option in scripts to avoid exposing your token:
  - Don't hardcode tokens in script files
  - Use environment variables or secure secret management tools
  - Consider using more secure authentication methods when possible
- If you need to use GitHub Copilot in CI/CD pipelines, consider if HMAC authentication might be more appropriate

## Related Options

| Option | Description |
|--------|-------------|
| `--use-copilot` | Use GitHub Copilot as the AI provider |
| `--use-copilot-token` | Explicitly use token-based authentication with GitHub Copilot |
| `--copilot-model-name` | Specify which Copilot model to use |
| `--copilot-api-endpoint` | Use a custom Copilot API endpoint |

## Related Commands

| Command | Description |
|---------|-------------|
| `chatx github login` | Interactive authentication with GitHub (recommended for most cases) |

## See Also

- [GitHub Copilot Provider](../../../providers/github-copilot.md)
- [github login](../github/login.md)