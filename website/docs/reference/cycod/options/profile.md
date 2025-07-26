# --profile

The `--profile` option allows you to load a predefined set of configurations from a profile file. This enables you to switch between different settings quickly without having to specify each option individually.

## Syntax

```
--profile PROFILE_NAME
```

## Arguments

- **PROFILE_NAME**: The name of the profile to load. This corresponds to a YAML file located in one of the profile directories: `.cycod/profiles/` (local), `~/.cycod/profiles/` (user), or the system-wide profiles directory.

## Description

The `--profile` option loads all the settings from the specified profile before executing the command. When you use `--profile`, CYCOD looks for a file with the given name and `.yaml` extension in the profiles directories, searching in the following order:

1. Local scope (`.cycod/profiles/` in the current directory)
2. User scope (`~/.cycod/profiles/` in the user's home directory)
3. Global scope (system-wide `.cycod/profiles/` directory)

The first matching profile is loaded, and its settings are applied to the command.

Profile files are YAML files that can contain any valid CYCOD configuration settings. These settings can include provider preferences, model choices, system prompts, or any other option that can be specified on the command line.

## Examples

### Basic Usage

```bash title="Using a profile"
cycod --profile work --question "What's on my agenda today?"
```

This loads all the settings from the `work` profile before processing the query.

### Combining with Other Options

```bash title="Override profile settings"
cycod --profile work --openai-chat-model-name gpt-4o --question "Generate a report outline"
```

Any options specified directly on the command line take precedence over settings in the profile.

### Using Provider-Specific Profiles

```bash title="Load an Azure profile"
cycod --profile azure --question "What are the benefits of cloud computing?"
```

This is useful when switching between different AI providers or API endpoints.

### Using Role-Based Profiles

```bash title="Use a programmer profile"
cycod --profile programmer --question "How do I implement a binary search?"
```

Useful when you've created profiles with specific system prompts for different roles.

## Profile File Example

Here's an example of what a profile file might contain:

```yaml title=".cycod/profiles/work.yaml"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    endpoint: "https://my-work-endpoint.openai.azure.com"
    chatDeployment: "gpt-4"

# System prompt for work-related questions
systemPrompt: "You are an assistant for work-related tasks. Keep answers professional and concise."
```

## Notes

- Command-line options override profile settings when there are conflicts
- Profiles are a powerful way to manage different configurations for different use cases
- You can create profiles in different scopes (local, user, global) to manage project-specific, user-specific, or system-wide settings

## See Also

- [Profiles](../../../advanced/profiles.md) - Detailed guide on creating and managing profiles
- [Configuration](../../../usage/configuration.md) - General configuration options
- [--config](config.md) - Load configuration from external files
- [--use-openai](use-openai.md) - Specify OpenAI as the provider
- [--use-azure-openai](use-azure-openai.md) - Specify Azure OpenAI as the provider
- [--use-copilot](use-copilot.md) - Specify GitHub Copilot as the provider