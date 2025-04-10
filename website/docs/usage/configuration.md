# Configuration

CHATX offers multiple configuration options to customize its behavior. This guide explains how to manage these settings effectively, with special focus on the `config set` command.

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

## Using the config set Command

The `config set` command is your primary tool for configuring CHATX. It allows you to customize everything from AI model preferences to application behavior.

### Basic Syntax

```bash
chatx config set <key> <value> [--scope]
```

Where:
- `<key>` is the configuration setting name (using dot notation)
- `<value>` is the value to assign
- `[--scope]` is optional and can be `--local` (default), `--user`, or `--global`

### Setting Provider Configuration

#### OpenAI Configuration

```bash
# Set your API key (use --user for security)
chatx config set openai.apiKey YOUR_API_KEY --user

# Set your preferred model
chatx config set openai.chatModelName gpt-4o --user

# Set token limits for responses
chatx config set openai.maxTokens 2048 --user
```

#### Azure OpenAI Configuration

```bash
# Set your Azure OpenAI endpoint
chatx config set azure.openai.endpoint https://your-resource.openai.azure.com --user

# Set your API key
chatx config set azure.openai.apiKey YOUR_API_KEY --user

# Set your deployment name
chatx config set azure.openai.chatDeployment gpt-4 --user
```

#### GitHub Copilot Configuration

```bash
# Set your preferred model for Copilot
chatx config set copilot.modelName claude-3.7-sonnet --user

# Custom API endpoint (if needed)
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

### Application Behavior Settings

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

Set token management preferences:

```bash
# Set token limit for chat history trimming
chatx config set app.trimTokenTarget 18000 --user
```

UI preferences:

```bash
# Reduce console output
chatx config set app.quietMode true --user

# Default to non-interactive mode
chatx config set app.interactiveMode false --user
```

### Project-Specific Configuration

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

### Boolean Settings

For boolean settings, use `true` or `false`:

```bash
# Enable a feature
chatx config set app.featureName true --user

# Disable a feature
chatx config set app.featureName false --user
```

### Working with Lists

Some configuration settings accept lists. Use the `config add` and `config remove` commands for these:

```bash title="Add to a list"
chatx config add app.allowedDomains example.com --user
```

```bash title="Remove from a list"
chatx config remove app.allowedDomains example.com --user
```

For detailed information and examples of working with list-type settings, see the [Working with List Settings](./config-list-settings.md) guide.

### Removing Settings

To remove a configuration setting entirely:

```bash title="Clear a setting"
chatx config clear openai.apiKey --user
```

## Real-World Configuration Recipes

Here are some common configuration scenarios:

### Setting Up for Production Use

```bash
# Set API key securely
chatx config set openai.apiKey YOUR_PRODUCTION_KEY --user

# Use the most capable model
chatx config set openai.chatModelName gpt-4o --user

# Set production-appropriate defaults
chatx config set app.quietMode true --user
chatx config set app.autoSaveChatHistory true --user
chatx config set app.historyDirectory "C:/Production/ChatLogs" --user
```

### Development Configuration

```bash
# Use test API key
chatx config set openai.apiKey YOUR_TEST_KEY --user

# Use development-focused settings
chatx config set app.quietMode false --user
chatx config set app.debugMode true --user

# Project-specific test deployment
chatx config set azure.openai.chatDeployment dev-deployment
```

### Team Configuration

Create a global configuration (requires admin privileges):

```bash
# Set team-wide Azure endpoint
chatx config set azure.openai.endpoint https://team-resource.openai.azure.com --global

# Set default company provider
chatx config set app.preferredProvider azure-openai --global

# Set appropriate history directory
chatx config set app.historyDirectory "\\server\shared\ChatLogs" --global
```

## Troubleshooting Configuration

### Common Issues

**Invalid Key Format**

If you get errors about invalid keys, check that you're using the correct dot notation format:

```bash
# Correct
chatx config set openai.apiKey YOUR_KEY

# Incorrect (missing dot)
chatx config set openaiApiKey YOUR_KEY
```

**Value Type Mismatch**

Ensure you're using the correct value type for the setting:

```bash
# Correct boolean
chatx config set app.autoSaveChatHistory true

# Incorrect boolean (will be treated as a string)
chatx config set app.autoSaveChatHistory "true"
```

**Permission Issues**

When setting global configuration, make sure you have the appropriate permissions:

```bash
# On Windows, run as Administrator
# On Linux/macOS, use sudo
sudo chatx config set app.preferredProvider openai --global
```

### View Current Configuration

If something isn't working as expected, check your current configuration:

```bash
# View all settings
chatx config list

# View specific setting
chatx config get app.preferredProvider
```

## Configuration Precedence

Settings are applied with the following precedence (highest priority first):

1. Command-line options
2. Environment variables
3. Local scope configuration
4. User scope configuration
5. Global scope configuration

This means you can override any configured setting directly on the command line:

```bash
# This uses gpt-4 even if openai.chatModelName is configured differently
chatx --openai-chat-model-name gpt-4 --question "What is the capital of France?"
```

## Best Practices

1. **Security**: Store sensitive values like API keys in the user scope (`--user`), not in local or global scopes
2. **Organization**: Use profiles for different projects or use cases
3. **Efficiency**: Set common settings in the user scope, and project-specific settings in the local scope
4. **Sharing**: Use the global scope for settings that should apply to all users on a shared system

## When to Use User Scope Configuration

The user scope (`--user` option) is particularly valuable for:

1. **API Keys**: Store your personal API keys securely
2. **Personal Preferences**: Set your preferred models and settings
3. **Cross-Project Settings**: Configure settings that should apply to all your work

Example:

```bash
# Store API keys
chatx config set openai.apiKey YOUR_API_KEY --user

# Set personal preferences
chatx config set app.preferredProvider azure-openai --user

# Configure default behavior
chatx config set app.autoSaveChatHistory true --user
```

## When to Use Local Scope Configuration

The local scope (default) is best for:

1. **Project-Specific Settings**: Configure settings for a specific project
2. **Team Collaboration**: Store shared project settings (but avoid secrets)
3. **Project Overrides**: Override user or global settings for a specific project

Example:

```bash
# Use a specific model for this project
chatx config set openai.chatModelName gpt-4-turbo

# Disable auto-saving for this project
chatx config set app.autoSaveChatHistory false
```

## Advanced Usage

### Environment Variables

Environment variables can override configuration files. They use a `CHATX_` prefix followed by the uppercase key with dots replaced by underscores:

```bash
# Set OpenAI API key
export CHATX_OPENAI_API_KEY=YOUR_API_KEY

# Set preferred provider
export CHATX_APP_PREFERRED_PROVIDER=azure-openai
```

### External Configuration Files

You can load settings from external configuration files:

```bash title="Load configuration from file"
chatx --config my-settings.yaml --question "What is the capital of France?"
```

These can be in YAML format:

```yaml title="YAML format (my-settings.yaml)"
app:
  preferredProvider: openai
  autoSaveChatHistory: true

openai:
  chatModelName: gpt-4o
  apiKey: sk-your-api-key
```

Or INI format:

```ini title="INI format (my-settings.ini)"
[app]
preferredProvider = openai
autoSaveChatHistory = true

[openai]
chatModelName = gpt-4o
apiKey = sk-your-api-key
```

For detailed information on using external configuration files, including advanced strategies, practical examples, and best practices, see the [External Configuration Files](./external-config-files.md) guide.

## Configuration Profiles

For more complex configurations, you can use profiles:

```bash title="Save a profile"
chatx --use-openai --openai-chat-model-name gpt-4 --save-profile gpt4
```

```bash title="Use saved profile"
chatx --profile gpt4 --question "What is the capital of France?"
```

For more details on profiles, see the [Profiles documentation](../advanced/profiles.md).

## Further Reading

- [CLI Reference: config set](../reference/cli/config/set.md)
- [CLI Reference: config get](../reference/cli/config/get.md)
- [CLI Reference: config list](../reference/cli/config/list.md)
- [CLI Reference: config clear](../reference/cli/config/clear.md)
- [Configuration Profiles](../advanced/profiles.md)