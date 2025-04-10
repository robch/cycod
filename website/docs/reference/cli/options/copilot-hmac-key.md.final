# --copilot-hmac-key

## Description

The `--copilot-hmac-key` option provides an alternative authentication method for the GitHub Copilot API. While the recommended method is to use GitHub token authentication via `chatx github login`, this option allows direct HMAC-based authentication for specialized scenarios.

When using HMAC authentication, you must also provide a Copilot integration ID using the [`--copilot-integration-id`](copilot-integration-id.md) option or the corresponding environment variable.

!!! note
    For a complete guide on using HMAC authentication with GitHub Copilot, see the [HMAC Authentication Tutorial](../../tutorials/github-copilot-hmac-auth.md).

## Syntax

```
--copilot-hmac-key KEY
```

Where `KEY` is your GitHub Copilot HMAC authentication key.

## Environment Variable

This option corresponds to the `COPILOT_HMAC_KEY` environment variable.

## Use Cases

HMAC authentication is particularly useful in the following scenarios:

- **CI/CD pipelines**: For automation where interactive GitHub login isn't possible
- **Server applications**: For backend services that need Copilot access
- **Air-gapped environments**: Where direct GitHub authentication isn't feasible
- **Organization-managed credentials**: When credentials are centrally managed and distributed
- **Custom integrations**: For specialized enterprise integrations with GitHub Copilot

## Examples

### Example 1: Basic usage with HMAC authentication

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --question "Explain quantum computing"
```

### Example 2: Using with a specific model

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --copilot-model-name claude-3-opus --question "Generate a Python class for database connections"
```

### Example 3: Saving HMAC key in configuration for reuse

```bash
# First, save your credentials securely in user configuration
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Then use the dedicated provider flag
chatx --use-copilot-hmac --question "Create a React component for a navigation bar"
```

## Configuration

You can set a default HMAC key in your configuration:

```bash
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
```

After configuring this setting along with the required integration ID, you can use GitHub Copilot with HMAC authentication by just specifying `--use-copilot-hmac` without having to provide the key each time.

## Provider Selection

To explicitly use Copilot with HMAC authentication, you can use the `--use-copilot-hmac` flag:

```bash
chatx --use-copilot-hmac --question "What is ChatX?"
```

This flag assumes that you have already set the required environment variables or configuration values:
- `COPILOT_HMAC_KEY` 
- `COPILOT_INTEGRATION_ID`

## Security Notes

- The HMAC key is a sensitive credential. Avoid exposing it in command history or scripts.
- When passing the key directly in the command line, be aware that it may be visible in process lists or command history.
- For better security, store the key using `chatx config set` or as an environment variable.

## Obtaining HMAC Credentials

HMAC keys and integration IDs are typically provided to enterprise customers or organizations with specific GitHub Copilot arrangements. Contact your GitHub account representative or administrator if you need these credentials.

## Related Options

| Option | Description |
|--------|-------------|
| `--use-copilot-hmac` | Explicitly selects Copilot with HMAC authentication |
| [`--copilot-integration-id`](copilot-integration-id.md) | Specifies the integration ID (required for HMAC auth) |
| `--copilot-api-endpoint` | Specifies the API endpoint (default: https://api.githubcopilot.com) |
| `--copilot-model-name` | Specifies the model to use (default: claude-3.7-sonnet) |

## See Also

- [--copilot-integration-id](copilot-integration-id.md)
- [--use-copilot-hmac](use-copilot-hmac.md)
- [GitHub Copilot Provider](../../providers/github-copilot.md)
- [HMAC Authentication Tutorial](../../tutorials/github-copilot-hmac-auth.md)