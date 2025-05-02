# Setting Up AI Providers
??? tip "Prerequisites"

    Before you begin:
    
    1. Make sure you have [installed CYCOD](/install-cycod-cli.md)

CycoD supports multiple AI providers. This guide shows you how to set up each supported provider.

## Provider Configuration

Choose your preferred provider from the tabs below for configuration instructions:

=== "OpenAI"
    
    --8<-- "snippets/configure-providers/openai/configure-openai.md"

=== "Azure OpenAI"
    
    --8<-- "snippets/configure-providers/azure-openai/configure-azure-openai.md"

=== "GitHub Copilot"
    
    --8<-- "snippets/configure-providers/github-copilot/configure-github-copilot.md"

## Setting a Default Provider

You can set your default AI provider in the CycoD configuration:

```bash
# Set OpenAI as default
cycod config set app.preferredProvider openai --user

# Set Azure OpenAI as default
cycod config set app.preferredProvider azure-openai --user

# Set GitHub Copilot as default
cycod config set app.preferredProvider copilot --user
```

## Overriding the Provider for a Specific Command

You can override your default provider for a specific command using the appropriate flag:

```bash
# Use OpenAI for this command
cycod --use-openai --question "What is CycoD?"

# Use Azure OpenAI for this command
cycod --use-azure-openai --question "What is CycoD?"

# Use GitHub Copilot for this command
cycod --use-copilot --question "What is CycoD?"
```

## Creating Provider-Specific Profiles

You can create dedicated profiles for different providers by creating YAML files in your profiles directory:

```yaml title="openai-profile.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4o"
```

```yaml title="azure-profile.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    chatDeployment: "gpt-4"
```

```yaml title="copilot-profile.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "copilot"

copilot:
  modelName: "claude-3-opus"
```

Then use them with:

```bash
cycod --profile openai-profile --question "What is CycoD?"
```

## Verifying Provider Configuration

To verify your current configuration:

```bash
# List all configuration settings
cycod config list

# Get specific provider settings
cycod config get openai
cycod config get azure.openai
cycod config get copilot
```

## See Also

* [Configuration](../usage/configuration.md) - Learn more about configuring CycoD
* [Command-Line Options](../reference/cli/index.md) - Reference for all command-line options
* [Profiles](../advanced/profiles.md) - Using profiles to manage different configurations