# --copilot-integration-id

## Description

The `--copilot-integration-id` option allows you to specify a custom integration identifier for GitHub Copilot authentication. This option is primarily used with HMAC authentication when accessing GitHub Copilot's services.

When using HMAC authentication with GitHub Copilot, this option is required alongside the [`--copilot-hmac-key`](copilot-hmac-key.md) option.

The integration ID is a unique identifier typically provided by GitHub to enterprise customers, which links your requests to your organization's GitHub Copilot account.

!!! note
    For a complete guide on using HMAC authentication with GitHub Copilot, including how to obtain and use an integration ID, see the [HMAC Authentication Tutorial](../../tutorials/github-copilot-hmac-auth.md).

## Syntax

```
--copilot-integration-id ID
```

Where `ID` is your GitHub Copilot integration identifier.

## Environment Variable

This option corresponds to the `COPILOT_INTEGRATION_ID` environment variable.

## Examples

### Example 1: Using direct HMAC authentication

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --question "What's the weather today?"
```

### Example 2: Saving integration ID in configuration

```bash
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user
```

### Example 3: Using with provider flag and pre-configured credentials

```bash
# First configure the credentials
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Then use it with the provider flag
chatx --use-copilot-hmac --question "Explain quantum computing"
```

## Configuration

You can set a default integration ID in your configuration:

```bash
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user
```

After configuring this setting, you can use GitHub Copilot with HMAC authentication by just specifying `--use-copilot-hmac` without having to provide the integration ID each time.

## Related Options

| Option | Description |
|--------|-------------|
| `--use-copilot-hmac` | Explicitly selects Copilot with HMAC authentication |
| `--copilot-hmac-key` | Specifies the HMAC key (required for HMAC auth) |
| `--copilot-api-endpoint` | Specifies the API endpoint (default: https://api.githubcopilot.com) |
| `--copilot-model-name` | Specifies the model to use (default: claude-3.7-sonnet) |

## See Also

- [--copilot-hmac-key](copilot-hmac-key.md)
- [--copilot-api-endpoint](copilot-api-endpoint.md)
- [--copilot-model-name](copilot-model-name.md)
- [GitHub Copilot Provider](../../providers/github-copilot.md)
- [HMAC Authentication Tutorial](../../tutorials/github-copilot-hmac-auth.md)