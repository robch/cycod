# Setting Up AI Providers
??? tip "Prerequisites"

    Before you begin:
    
    1. Make sure you have [installed CHATX](/getting-started.md)

ChatX supports multiple AI providers. This guide shows you how to set up each supported provider.

## Provider Configuration

Choose your preferred provider from the tabs below for configuration instructions:

=== "OpenAI"
    
    --8<-- "snippets/configure-providers/openai/configure-openai.md"

=== "Azure OpenAI"
    
    --8<-- "snippets/configure-providers/azure-openai/configure-azure-openai.md"

=== "GitHub Copilot"
    
    --8<-- "snippets/configure-providers/github-copilot/configure-github-copilot.md"

## Setting a Default Provider

You can set your default AI provider in the ChatX configuration:

```bash
# Set OpenAI as default
chatx config set app.preferredProvider openai --user

# Set Azure OpenAI as default
chatx config set app.preferredProvider azure-openai --user

# Set GitHub Copilot as default
chatx config set app.preferredProvider copilot --user
```

## Overriding the Provider for a Specific Command

You can override your default provider for a specific command using the appropriate flag:

```bash
# Use OpenAI for this command
chatx --use-openai --question "What is ChatX?"

# Use Azure OpenAI for this command
chatx --use-azure-openai --question "What is ChatX?"

# Use GitHub Copilot for this command
chatx --use-copilot --question "What is ChatX?"
```

## Creating Provider-Specific Profiles

You can create dedicated profiles for different providers by creating YAML files in your profiles directory:

```yaml title="openai-profile.yaml (in .chatx/profiles directory)"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4o"
```

```yaml title="azure-profile.yaml (in .chatx/profiles directory)"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    chatDeployment: "gpt-4"
```

```yaml title="copilot-profile.yaml (in .chatx/profiles directory)"
app:
  preferredProvider: "copilot"

copilot:
  modelName: "claude-3-opus"
```

Then use them with:

```bash
chatx --profile openai-profile --question "What is ChatX?"
```

## Verifying Provider Configuration

To verify your current configuration:

```bash
# List all configuration settings
chatx config list

# Get specific provider settings
chatx config get openai
chatx config get azure.openai
chatx config get copilot
```

## See Also

* [Configuration](../usage/configuration.md) - Learn more about configuring ChatX
* [Command-Line Options](../reference/cli/index.md) - Reference for all command-line options
* [Profiles](../advanced/profiles.md) - Using profiles to manage different configurations