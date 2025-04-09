# config set

Sets the value of a configuration setting.

## Syntax

```bash
chatx config set <key> <value> [options]
```

## Description

The `chatx config set` command sets the value of a specified configuration setting. By default, it sets the value in the local scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the configuration setting to set |
| `<value>` | Value to assign to the setting |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Set in global scope (all users) |
| `--user`, `-u` | Set in user scope (current user) |
| `--local`, `-l` | Set in local scope (current directory, default) |

## Examples

Set the OpenAI API key in user scope:

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

Set the preferred provider in local scope:

```bash
chatx config set app.preferredProvider azure-openai
```

Set the Azure OpenAI endpoint:

```bash
chatx config set azure.openai.endpoint https://example.openai.azure.com
```

## Output

The command confirms successful configuration:

```
Setting 'openai.apiKey' has been set in user scope.
```

If the operation fails, an error message is displayed:

```
Error: Failed to set setting 'invalid.key'. Invalid configuration key.
```

## Security Note

For security-sensitive settings like API keys, it's recommended to use the user scope (`--user`) rather than local or global scopes to limit exposure.

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

This ensures your API keys are stored in your user profile only and not in project directories that might be shared with others.