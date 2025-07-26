# config set

Sets the value of a configuration setting.

## Syntax

```bash
cycod config set <key> <value> [options]
```

## Description

The `cycod config set` command sets the value of a specified configuration setting. By default, it sets the value in the local scope (current directory). Configuration settings are stored in JSON files and can be used to customize CYCOD behavior.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the configuration setting to set. Keys use dot notation (e.g., `openai.apiKey`). |
| `<value>` | Value to assign to the setting. Can be a string, number, boolean, or JSON-compatible value. |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Set in global scope (all users) |
| `--user`, `-u` | Set in user scope (current user) |
| `--local`, `-l` | Set in local scope (current directory, default) |

## Examples

### Provider Configuration

Set OpenAI settings in user scope:

```bash
cycod config set openai.apiKey YOUR_API_KEY --user
cycod config set openai.chatModelName gpt-4o --user
cycod config set openai.maxTokens 2048 --user
```

Set Azure OpenAI settings in user scope:

```bash
cycod config set azure.openai.endpoint https://example.openai.azure.com --user
cycod config set azure.openai.apiKey YOUR_AZURE_API_KEY --user
cycod config set azure.openai.chatDeployment gpt-4-deployment --user
```

Set GitHub Copilot settings in user scope:

```bash
cycod config set copilot.modelName claude-3.7-sonnet --user
```

### Application Preferences

Choose your preferred AI provider:

```bash
cycod config set app.preferredProvider openai --user
```

Configure auto-saving features:

```bash
cycod config set app.autoSaveChatHistory true --user
cycod config set app.autoSaveTrajectory true --user
cycod config set app.historyDirectory "C:/Users/username/chat-histories" --user
```

Set default behavior:

```bash
cycod config set app.quietMode false --user
cycod config set app.interactiveMode true --user
cycod config set app.trimTokenTarget 18000 --user
```

### Local Project Configuration

Project-specific settings (in local scope):

```bash
cycod config set app.preferredProvider azure-openai
cycod config set azure.openai.chatDeployment custom-deployment-name
```

Disable auto-saving for a specific project:

```bash
cycod config set app.autoSaveChatHistory false
cycod config set app.autoSaveTrajectory false
```

## Common Configuration Keys

### Application Settings

| Key | Description | Example Value |
|-----|-------------|---------------|
| `app.preferredProvider` | Default AI provider | `"openai"`, `"azure-openai"`, `"copilot"` |
| `app.autoSaveChatHistory` | Enable/disable auto-save of chat history | `true`, `false` |
| `app.autoSaveTrajectory` | Enable/disable auto-save of trajectory | `true`, `false` |
| `app.historyDirectory` | Custom directory for saving histories | `"C:/Chats"` |
| `app.trimTokenTarget` | Target token limit for trimming | `16000` |
| `app.quietMode` | Reduce console output | `true`, `false` |
| `app.interactiveMode` | Enable interactive mode by default | `true`, `false` |

### OpenAI Settings

| Key | Description | Example Value |
|-----|-------------|---------------|
| `openai.apiKey` | API key for OpenAI | `"sk-..."` |
| `openai.chatModelName` | Default model for chat | `"gpt-4o"`, `"gpt-4-turbo"` |
| `openai.maxTokens` | Maximum tokens for responses | `2048` |

### Azure OpenAI Settings

| Key | Description | Example Value |
|-----|-------------|---------------|
| `azure.openai.endpoint` | Azure OpenAI service endpoint | `"https://example.openai.azure.com"` |
| `azure.openai.apiKey` | API key for Azure OpenAI | `"..."` |
| `azure.openai.chatDeployment` | Deployment name for chat model | `"gpt-4"` |

### GitHub Copilot Settings

| Key | Description | Example Value |
|-----|-------------|---------------|
| `copilot.modelName` | Model to use with Copilot | `"claude-3.7-sonnet"` |
| `copilot.apiEndpoint` | Custom API endpoint | `"https://api.githubcopilot.com"` |

## Output

The command confirms successful configuration:

```
Setting 'openai.apiKey' has been set in user scope.
```

If the operation fails, an error message is displayed:

```
Error: Failed to set setting 'invalid.key'. Invalid configuration key.
```

## Notes

### Security

For security-sensitive settings like API keys, it's recommended to use the user scope (`--user`) rather than local or global scopes to limit exposure.

```bash
cycod config set openai.apiKey YOUR_API_KEY --user
```

This ensures your API keys are stored in your user profile only and not in project directories that might be shared with others.

### Configuration Precedence

Settings follow this order of precedence (highest to lowest):

1. Command-line options
2. Environment variables
3. Local scope configuration
4. User scope configuration
5. Global scope configuration

This means settings specified in the command line will override any configured values.

### Related Commands

- `cycod config get <key>` - Get the value of a configuration setting
- `cycod config list` - List all configuration settings
- `cycod config clear <key>` - Remove a configuration setting