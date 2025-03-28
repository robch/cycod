# Provider Selection

ChatX supports multiple AI providers including OpenAI, Azure OpenAI, and GitHub Copilot. This document explains how to select and configure different providers.

## Supported Providers

ChatX currently supports the following AI providers:

| Provider | Required Credentials | Features |
|----------|---------------------|----------|
| OpenAI | `OPENAI_API_KEY` | Standard OpenAI API |
| Azure OpenAI | `AZURE_OPENAI_API_KEY` | OpenAI models hosted on Azure |
| GitHub Copilot | `GITHUB_TOKEN` | GitHub Copilot models |
| GitHub Copilot HMAC | `COPILOT_HMAC_KEY` | GitHub Copilot with HMAC authentication |

## Default Provider Selection

By default, ChatX selects a provider by checking for the necessary environment variables in the following order:

1. GitHub Copilot (if `GITHUB_TOKEN` is set)
2. GitHub Copilot with HMAC authentication (if `COPILOT_HMAC_KEY` is set)
3. Azure OpenAI (if `AZURE_OPENAI_API_KEY` is set)
4. OpenAI (if `OPENAI_API_KEY` is set)

This means that if you have multiple sets of credentials in your environment, ChatX will use the first one it finds according to this priority order.

## Ways to Select a Provider

There are several methods to explicitly select a provider, overriding the default behavior:

### 1. Command-Line Provider Flags

Use these command-line flags to explicitly select a provider:

```bash
chatx --use-openai        # Use OpenAI API
chatx --use-azure-openai  # Use Azure OpenAI API
chatx --use-copilot       # Use GitHub Copilot (either token or HMAC)
chatx --use-copilot-token # Use GitHub Copilot with token authentication
chatx --use-copilot-hmac  # Use GitHub Copilot with HMAC authentication
```

These flags override the default provider selection order. All necessary credentials must still be available in your environment or configuration files.

### 2. Configuration File Setting

Set your preferred provider in any configuration file (global, user, or local):

```yaml
app:
  preferredProvider: "openai"  # Can be "openai", "azure-openai", "copilot", or "copilot-hmac"
```

Or using the config command:

```bash
chatx config set app.preferredProvider openai
```

### 3. Environment Variable

Set the `CHATX_PREFERRED_PROVIDER` environment variable:

```bash
# Windows CMD
set CHATX_PREFERRED_PROVIDER=openai
chatx

# Windows PowerShell
$env:CHATX_PREFERRED_PROVIDER="azure-openai"
chatx

# Linux/macOS
CHATX_PREFERRED_PROVIDER=copilot chatx
```

## Using Named Configurations (Profiles)

ChatX supports named configurations (profiles), allowing you to set up different provider configurations for different scenarios:

### Creating a Named Configuration

Create a file at `.chatx/profiles/<name>.yaml` with your configuration:

```yaml
# .chatx/profiles/work.yaml
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    endpoint: "https://my-work-endpoint.openai.azure.com"
    chatDeployment: "gpt-4"
    # apiKey would typically be provided by environment variable
```

### Using a Named Configuration

```bash
chatx --profile work
```

This will load the work profile configuration before processing other configurations and command-line arguments.

## Priority Order

When determining which provider to use, ChatX uses the following priority order (highest to lowest):

1. Command-line flags (`--use-openai`, etc.)
2. Profile selection (`--profile <name>`)
3. Environment variable (`CHATX_PREFERRED_PROVIDER`)
4. Configuration setting (`app.preferredProvider`)
5. Default detection order based on available credentials

## Example Scenarios

### Scenario 1: Multiple Providers, Default Selection

If you have both OpenAI and Azure OpenAI credentials set up in your environment, but want to use Azure OpenAI by default:

```bash
chatx config set --global app.preferredProvider azure-openai
```

### Scenario 2: Temporary Provider Override

If you normally use Azure OpenAI but want to use OpenAI just for one session:

```bash
chatx --use-openai
```

### Scenario 3: Different Configurations for Different Projects

Create project-specific profiles:

```yaml
# .chatx/profiles/project-a.yaml
app:
  preferredProvider: "azure-openai"
azure:
  openai:
    chatDeployment: "project-a-deployment"
```

```yaml
# .chatx/profiles/project-b.yaml
app:
  preferredProvider: "copilot"
copilot:
  modelName: "claude-3.5-sonnet"
```

Then use:

```bash
chatx --profile project-a
# or
chatx --profile project-b
```

### Scenario 4: Creating Aliases

You can create command aliases for different providers:

```bash
chatx --use-openai --save-alias openai-chat
chatx --use-azure-openai --save-alias azure-chat
chatx --use-copilot --save-alias copilot-chat
```

Then use:

```bash
chatx --openai-chat
# or 
chatx --azure-chat
# or
chatx --copilot-chat
```