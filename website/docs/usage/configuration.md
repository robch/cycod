# Configuration

CHATX offers multiple configuration options to customize its behavior. This guide explains how to manage these settings effectively.

## Configuration System

CHATX uses a flexible configuration system with three different scopes:

1. **Local scope**: Settings apply only to the current directory
2. **User scope**: Settings apply to the current user across all directories
3. **Global scope**: Settings apply to all users on the system

The settings are stored in configuration files:

- Local: `.chatx/config.json` in the current directory
- User: `.chatx/config.json` in the user's home directory
- Global: `.chatx/config.json` in the system-wide location

## Viewing Configuration

### List All Settings

To view all configuration settings:

```bash title="List all settings"
chatx config list
```

This shows settings from all scopes, with local settings taking precedence over user settings, which take precedence over global settings.

To view settings from a specific scope:

```bash title="List local settings"
chatx config list --local
```

```bash title="List user settings"
chatx config list --user
```

```bash title="List global settings"
chatx config list --global
```

### View a Specific Setting

To view a specific configuration setting:

```bash title="Get a setting"
chatx config get openai.apiKey
```

By default, this looks in all scopes and returns the first match. To specify a scope:

```bash title="Get from user scope"
chatx config get openai.apiKey --user
```

## Modifying Configuration

### Set a Value

To set a configuration value:

```bash title="Set a value"
chatx config set openai.apiKey YOUR_API_KEY --user
```

By default, values are set in the local scope. Use `--user` or `--global` to set values in other scopes.

Common settings include:

```bash
# Set the default AI provider
chatx config set app.preferredProvider openai --user

# Set the OpenAI model
chatx config set openai.chatModelName gpt-4o --user

# Set Azure OpenAI settings
chatx config set azure.openai.endpoint YOUR_ENDPOINT --user
chatx config set azure.openai.apiKey YOUR_API_KEY --user
chatx config set azure.openai.chatDeployment YOUR_DEPLOYMENT --user

# Set GitHub Copilot model
chatx config set copilot.modelName claude-3.7-sonnet --user

# Configure auto-saving
chatx config set app.autoSaveChatHistory true --user
chatx config set app.autoSaveTrajectory true --user
```

### Clear a Value

To remove a configuration setting:

```bash title="Clear a setting"
chatx config clear openai.apiKey --user
```

### List Settings

CHATX supports list-type settings that can have multiple values. You can add and remove values from these lists:

```bash title="Add to a list"
chatx config add app.allowedDomains example.com --user
```

```bash title="Remove from a list"
chatx config remove app.allowedDomains example.com --user
```

## Using Environment Variables

You can also configure CHATX using environment variables. The environment variables take precedence over the configuration files:

```bash
# Set OpenAI API key
export CHATX_OPENAI_API_KEY=YOUR_API_KEY

# Set preferred provider
export CHATX_PREFERRED_PROVIDER=openai

# Run CHATX
chatx --question "What is the capital of France?"
```

## Configuration Profiles

Profiles allow you to save collections of settings as a unit. Profiles are YAML files stored in:

- `.chatx/profiles/` (local)
- `~/.chatx/profiles/` (user)
- [system]/.chatx/profiles/ (global)

### Using Profiles

To use a profile:

```bash title="Use a profile"
chatx --profile development --question "What is the capital of France?"
```

This will load all the settings from the profile before executing the command.

### Creating Profiles

The easiest way to create a profile is by using the `--save-profile` option:

```bash title="Save a profile"
chatx --use-openai --openai-chat-model-name gpt-4 --save-profile gpt4
```

You can then use this profile later:

```bash title="Use saved profile"
chatx --profile gpt4 --question "What is the capital of France?"
```

## Config Files Format

CHATX configuration files use JSON format. Here's an example of what a configuration file might look like:

```json
{
  "app": {
    "preferredProvider": "openai",
    "autoSaveChatHistory": true,
    "autoSaveTrajectory": true
  },
  "openai": {
    "apiKey": "sk-...",
    "chatModelName": "gpt-4o"
  },
  "azure": {
    "openai": {
      "endpoint": "https://your-resource.openai.azure.com",
      "apiKey": "...",
      "chatDeployment": "gpt-4"
    }
  }
}
```

## Best Practices

1. **Security**: Store sensitive values like API keys in the user scope, not in local or global scopes
2. **Organization**: Use profiles for different projects or use cases
3. **Efficiency**: Set common settings in the user scope, and project-specific settings in the local scope
4. **Sharing**: Use the global scope for settings that should apply to all users on a shared system

## Command-Line Options vs. Configuration

Settings provided on the command line take precedence over configuration files:

```bash
# This will use gpt-4 regardless of what's in the configuration
chatx --openai-chat-model-name gpt-4 --question "What is the capital of France?"
```

This makes it easy to override specific settings for individual commands while keeping your default configuration intact.