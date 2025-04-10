To configure GitHub Copilot as your provider:

1. Ensure you have a GitHub account with Copilot subscription
2. Authenticate with GitHub:

```bash
# Log in through GitHub authentication flow
chatx github login

# Set GitHub Copilot as your default provider (optional)
chatx config set app.preferredProvider copilot --user

# Optionally set a specific model
chatx config set copilot.modelName claude-3.5-sonnet --user
```

You can also use HMAC authentication for enterprise scenarios:

```bash
# Configure HMAC authentication
chatx config set copilot.hmacKey YOUR_HMAC_KEY --user
chatx config set copilot.integrationId YOUR_INTEGRATION_ID --user

# Use HMAC auth directly in a command
chatx --use-copilot-hmac --copilot-hmac-key YOUR_HMAC_KEY --copilot-integration-id YOUR_INTEGRATION_ID --question "What is ChatX?"
```