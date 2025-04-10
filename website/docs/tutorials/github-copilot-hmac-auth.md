# Using HMAC Authentication with GitHub Copilot

This tutorial explains how to set up and use HMAC authentication for GitHub Copilot in ChatX, which requires the `--copilot-integration-id` and `--copilot-hmac-key` options.

## Introduction

While token-based authentication ([covered in the main GitHub authentication guide](github-copilot-auth.md)) is recommended for most users, GitHub Copilot also supports HMAC authentication for specific use cases.

HMAC authentication uses a pre-shared key and integration ID to authenticate with GitHub Copilot, making it suitable for:

- CI/CD pipelines and automation scripts
- Environments where the OAuth flow isn't feasible
- Air-gapped environments
- Corporate environments with specific authentication policies
- Shared service accounts

## Prerequisites

To use HMAC authentication, you'll need:

- ChatX installed ([installation guide](../getting-started.md))
- A Copilot HMAC key (provided by GitHub)
- A Copilot integration ID (provided by GitHub)

!!! note "Obtaining Credentials"
    HMAC keys and integration IDs are typically provided by GitHub to enterprise customers or through special arrangements. These aren't available through standard GitHub Copilot subscriptions. Contact your GitHub account representative or administrator if you need these credentials.

## Setting Up HMAC Authentication

HMAC authentication requires both the integration ID and HMAC key to work properly.

### Option 1: Configure Credentials in Your Profile

The recommended approach is to save these credentials in your user configuration:

```bash
# Set the HMAC key
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user

# Set the integration ID
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user
```

After setting these values, you can use GitHub Copilot with HMAC authentication by simply adding the `--use-copilot-hmac` flag:

```bash
chatx --use-copilot-hmac --question "What is HMAC authentication?"
```

### Option 2: Provide Credentials in the Command

You can also provide the credentials directly in your commands:

```bash
chatx --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --question "Explain HMAC authentication"
```

This approach is useful for testing or when you need to use different credentials for different commands.

## Understanding the Integration ID

The `--copilot-integration-id` is a unique identifier that:

1. Identifies your organization or team to the GitHub Copilot service
2. Tracks usage and billing for your HMAC authentication
3. Controls access permissions and feature availability
4. Links requests to your specific Copilot configuration

Think of the integration ID as an account identifier for your organization's Copilot service.

## Creating an Alias for HMAC Authentication

To make using HMAC authentication more convenient, consider creating an alias:

```bash
# Create an alias with your HMAC credentials
chatx --use-copilot-hmac --copilot-model-name claude-3-opus --save-alias copilot-hmac

# Now you can use the alias
chatx --copilot-hmac --question "Generate a Python function"
```

## Best Practices for HMAC Credentials

Since HMAC credentials provide direct access to GitHub Copilot's services, follow these security best practices:

1. **Store credentials securely**: Use the user-level configuration to avoid exposing credentials in script files
2. **Don't share credentials**: Each team or organization should use their own integration ID
3. **Rotate credentials**: If your HMAC key is compromised, request a new one from GitHub
4. **Use environment variables for automation**: For CI/CD pipelines, use environment variables instead of hardcoded values

## Troubleshooting

Common issues with HMAC authentication:

- **Authentication failed**: Verify that both your HMAC key and integration ID are correct
- **Invalid format**: Ensure there are no extra spaces or characters in your credentials 
- **Access denied**: Confirm that your integration ID is active with GitHub
- **Rate limiting**: HMAC authentication may have different rate limits than token-based authentication

## Related Documentation

- [GitHub Copilot Provider](../providers/github-copilot.md) - Complete GitHub Copilot documentation
- [`--copilot-integration-id` Reference](../reference/cli/options/copilot-integration-id.md) - Detailed option reference
- [`--copilot-hmac-key` Reference](../reference/cli/options/copilot-hmac-key.md) - Detailed option reference
- [`--use-copilot-hmac` Reference](../reference/cli/options/use-copilot-hmac.md) - Provider selection flag reference