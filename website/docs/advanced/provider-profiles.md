---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Provider Profiles

When working with different AI providers, it's helpful to create dedicated profiles. This approach lets you quickly switch between providers and custom configurations.

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

* [Profiles](../advanced/profiles.md) - Learn more about using profiles to manage different configurations
* [Configuration](../usage/configuration.md) - Learn more about configuring CycoD
* [Command-Line Options](../reference/cli/index.md) - Reference for all command-line options