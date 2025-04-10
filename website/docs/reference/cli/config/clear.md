# config clear

Clears a configuration setting.

## Syntax

```bash
chatx config clear <key> [options]
```

## Description

The `chatx config clear` command removes a specified configuration setting completely. Unlike setting a value to empty or null, clearing a setting removes the key entirely from the configuration file. By default, it clears the setting from the local scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the configuration setting to clear |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Clear from global scope (all users) |
| `--user`, `-u` | Clear from user scope (current user) |
| `--local`, `-l` | Clear from local scope (current directory, default) |

## Examples

### Provider Configuration

Clear OpenAI API key from user scope:

```bash
chatx config clear openai.apiKey --user
```

Clear Azure OpenAI settings:

```bash
chatx config clear azure.openai.endpoint --user
chatx config clear azure.openai.chatDeployment --user
```

Clear GitHub Copilot settings:

```bash
chatx config clear copilot.modelName --user
```

### Application Preferences

Reset preferred provider to default:

```bash
chatx config clear app.preferredProvider --user
```

Disable auto-saving features by clearing them:

```bash
chatx config clear app.autoSaveChatHistory
chatx config clear app.autoSaveTrajectory
```

Reset custom history directory:

```bash
chatx config clear app.historyDirectory --user
```

### Project Configuration

Clear project-specific Azure deployment:

```bash
chatx config clear azure.openai.chatDeployment
```

Clear local project settings to revert to user defaults:

```bash
chatx config clear app.preferredProvider
```

## Common Use Cases

### Removing Sensitive Information

Clear API keys and credentials when they're no longer needed:

```bash
chatx config clear openai.apiKey --user
chatx config clear azure.openai.apiKey --user
```

### Resetting to Defaults

Clear custom settings to revert to system defaults:

```bash
chatx config clear openai.chatModelName --user
chatx config clear app.trimTokenTarget --user
```

### Cleaning Up Project Configuration

Remove project-specific overrides before sharing a directory:

```bash
chatx config clear azure.openai.chatDeployment
chatx config clear app.preferredProvider
```

## When to Use config clear vs. Other Commands

- Use `config clear` when you want to completely remove a setting
- Use `config set KEY ""` when you want to keep the key but set an empty value
- Use `config set KEY null` when you want to explicitly set a null value

## Output

The command confirms successful clearing:

```
Setting 'azure.openai.endpoint' has been cleared from user scope.
```

If the setting is not found in the specified scope, the command indicates this:

```
Setting 'unknown.setting' not found in local scope.
```

## Related Commands

- `chatx config set <key> <value>` - Set the value of a configuration setting
- `chatx config get <key>` - Get the value of a configuration setting
- `chatx config list` - List all configuration settings
- `chatx config add <key> <value>` - Add a value to a list setting
- `chatx config remove <key> <value>` - Remove a value from a list setting