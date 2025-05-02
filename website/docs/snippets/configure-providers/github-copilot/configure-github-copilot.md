To configure GitHub Copilot as your provider:

1. Ensure you have a GitHub account with Copilot subscription
2. Authenticate with GitHub:

```bash
# Log in through GitHub authentication flow
cycod github login

# Set GitHub Copilot as your default provider (optional)
cycod config set app.preferredProvider copilot --user

# Optionally set a specific model
cycod config set copilot.modelName claude-3.5-sonnet --user
```
