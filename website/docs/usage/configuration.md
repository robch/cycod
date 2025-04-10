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

This shows settings from all scopes (global, user, and local). The output is grouped by scope and includes information about where each setting comes from (configuration file, environment variable, or command line).

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

Example output of `chatx config list`:

```
SETTINGS (scope: global)
  app.allowedDomains: [example.com, trusted-site.org]

SETTINGS (scope: user)
  openai.apiKey: sk-***
  openai.chatModelName: gpt-4o
  app.preferredProvider: openai

SETTINGS (scope: local)
  azure.openai.chatDeployment: gpt-4
  app.autoSaveChatHistory: false
```

### View a Specific Setting

To view a specific configuration setting:

```bash title="Get a setting"
chatx config get openai.apiKey
```

By default, this looks in all scopes and returns the first match (equivalent to using the `--any` option). You can specify a particular scope:

```bash title="Get from user scope"
chatx config get openai.apiKey --user
```

```bash title="Get from any scope (same as default)"
chatx config get openai.apiKey --any
```

When using `--any`, CHATX searches in the local scope first, then user, then global.

## Modifying Configuration

### Using the config set Command

The `config set` command is your primary tool for configuring CHATX. It allows you to customize everything from AI model preferences to application behavior.

#### Basic Syntax

```bash
chatx config set <key> <value> [--scope]
```

Where:
- `<key>` is the configuration setting name (using dot notation)
- `<value>` is the value to assign
- `[--scope]` is optional and can be `--local` (default), `--user`, or `--global`

By default, values are set in the local scope. Use `--user` or `--global` to set values in other scopes.

#### Setting Provider Configuration

**OpenAI Configuration**

```bash
# Set your API key (use --user for security)
chatx config set openai.apiKey YOUR_API_KEY --user

# Set your preferred model
chatx config set openai.chatModelName gpt-4o --user

# Set token limits for responses
chatx config set openai.maxTokens 2048 --user
```

**Azure OpenAI Configuration**

```bash
# Set your Azure OpenAI endpoint
chatx config set azure.openai.endpoint https://your-resource.openai.azure.com --user

# Set your API key
chatx config set azure.openai.apiKey YOUR_API_KEY --user

# Set your deployment name
chatx config set azure.openai.chatDeployment gpt-4 --user
```

**GitHub Copilot Configuration**

```bash
# Set your preferred model for Copilot
chatx config set copilot.modelName claude-3.7-sonnet --user

# Custom API endpoint (if needed)
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

#### Application Behavior Settings

Choose your default AI provider:

```bash
# Set OpenAI as your preferred provider
chatx config set app.preferredProvider openai --user

# Or set Azure OpenAI as your preferred provider
chatx config set app.preferredProvider azure-openai --user

# Or set GitHub Copilot as your preferred provider
chatx config set app.preferredProvider copilot --user
```

Configure automatic saving of chat history and trajectory:

```bash
# Enable auto-saving chat history
chatx config set app.autoSaveChatHistory true --user

# Enable auto-saving trajectory (human-readable format)
chatx config set app.autoSaveTrajectory true --user

# Set custom directory for saved histories
chatx config set app.historyDirectory "C:/Users/username/Documents/ChatHistories" --user
```

#### Boolean Settings

For boolean settings, use `true` or `false`:

```bash
# Enable a feature
chatx config set app.featureName true --user

# Disable a feature
chatx config set app.featureName false --user
```

#### Project-Specific Configuration

For project-specific settings, use the local scope (default):

```bash
# Set project-specific provider
chatx config set app.preferredProvider azure-openai

# Use a specific deployment for this project
chatx config set azure.openai.chatDeployment my-project-deployment

# Disable auto-saving for this project only
chatx config set app.autoSaveChatHistory false
chatx config set app.autoSaveTrajectory false
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

For detailed information and examples of working with list settings, see the [Working with List Settings](./config-list-settings.md) guide.

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

## Using External Configuration Files

You can also load settings from external configuration files using the `--config` option. This allows you to maintain different configuration sets for various projects or scenarios.

```bash title="Load configuration from file"
chatx --config my-settings.yaml --question "What is the capital of France?"
```

You can load multiple configuration files, with later files taking precedence over earlier ones:

```bash title="Load multiple configuration files"
chatx --config base-settings.yaml project-settings.yaml --question "What is the capital of France?"
```

For detailed information on using external configuration files, including advanced strategies, practical examples, and best practices, see the [External Configuration Files](./external-config-files.md) guide.

### Supported File Formats

CHATX supports both YAML and INI formats for external configuration files:

```yaml title="YAML format (my-settings.yaml)"
app:
  preferredProvider: openai
  autoSaveChatHistory: true

openai:
  chatModelName: gpt-4o
  apiKey: sk-your-api-key
```

```ini title="INI format (my-settings.ini)"
[app]
preferredProvider = openai
autoSaveChatHistory = true

[openai]
chatModelName = gpt-4o
apiKey = sk-your-api-key
```

### Mixing with Command-Line Options

Settings from configuration files can be overridden by options specified on the command line:

```bash
chatx --config team-settings.yaml --use-azure-openai --question "What is the capital of France?"
```

In this example, even if `team-settings.yaml` sets `openai` as the preferred provider, the `--use-azure-openai` option will override it.

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

This will load all the settings from the profile before executing the command. For more details and examples, see the [--profile option reference](../../reference/cli/options/profile.md).

### Creating Profiles

The easiest way to create a profile is by using the `--save-profile` option:

```bash title="Save a profile"
chatx --use-openai --openai-chat-model-name gpt-4 --save-profile gpt4
```

For more details on creating profiles, see the [--save-profile option reference](../../reference/cli/options/save-profile.md).

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

## When to Use User Scope Configuration

The user scope (`--user` option) is particularly valuable for configuration settings because:

1. **Persistence**: Settings follow you across projects and directories
2. **Privacy**: Settings are only accessible to your user account
3. **Centralization**: Creates a single source of truth for your personal settings

Common configuration tasks that should use the `--user` option:

```bash
# Store API keys
chatx config set openai.apiKey YOUR_API_KEY --user

# Set personal preferences
chatx config set app.preferredProvider azure-openai --user

# Configure default behavior
chatx config set app.autoSaveChatHistory true --user

# Set up personal UI preferences
chatx config set app.quietMode false --user
```

Since these settings are stored in your user profile's `.chatx` directory, they won't be accidentally committed to version control systems and will remain consistent as you work across different projects.

## Command-Line Options vs. Configuration

Settings provided on the command line take precedence over configuration files:

```bash
# This will use gpt-4 regardless of what's in the configuration
chatx --openai-chat-model-name gpt-4 --question "What is the capital of France?"
```

This makes it easy to override specific settings for individual commands while keeping your default configuration intact.