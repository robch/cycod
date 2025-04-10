# --copilot-api-endpoint

The `--copilot-api-endpoint` option allows you to specify a custom API endpoint for GitHub Copilot interactions.

## Syntax

```bash
chatx --copilot-api-endpoint URL [other options]
```

## Description

When using GitHub Copilot as the AI provider, this option lets you override the default API endpoint used to communicate with the Copilot service. This is typically only needed in specialized scenarios such as:

- Using a corporate or organization-specific GitHub Copilot endpoint
- Working with GitHub Copilot in air-gapped environments
- Testing against development or staging environments

## Parameters

| Parameter | Description |
|-----------|-------------|
| `URL`     | The full URL of the GitHub Copilot API endpoint you want to use. |

## Default Value

If not specified, CHATX uses `https://api.githubcopilot.com` as the default endpoint.

## Examples

### Specify a custom GitHub Copilot API endpoint

```bash
chatx --use-copilot --copilot-api-endpoint https://custom-copilot-api.example.com --question "What is GitHub Copilot?"
```

### Set a custom endpoint in configuration

You can also set this value permanently in your configuration:

```bash
# Set custom API endpoint in user configuration
chatx config set copilot.apiEndpoint https://custom-copilot-api.example.com --user
```

## Related Options

| Option | Description |
|--------|-------------|
| `--use-copilot` | Prefer use of GitHub Copilot |
| `--copilot-model-name` | Use a specific model by name (default: claude-3.7-sonnet) |
| [`--copilot-integration-id`](copilot-integration-id.md) | Use a specific integration id |
| `--copilot-hmac-key` | Use a specific authentication key |
| `--github-token` | Use a specific GitHub authentication token |

## See Also

- [GitHub Copilot Provider](../../../providers/github-copilot.md)
- [chatx github login](../github/login.md)