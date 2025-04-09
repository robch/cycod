# config clear

Clears a configuration setting.

## Syntax

```bash
chatx config clear <key> [options]
```

## Description

The `chatx config clear` command removes a specified configuration setting. By default, it clears the setting from the local scope.

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

Clear the Azure OpenAI endpoint from local scope:

```bash
chatx config clear azure.openai.endpoint
```

Clear the OpenAI API key from user scope:

```bash
chatx config clear openai.apiKey --user
```

Clear the preferred provider setting from global scope:

```bash
chatx config clear app.preferredProvider --global
```

## Output

The command confirms successful clearing:

```
Setting 'azure.openai.endpoint' has been cleared from local scope.
```

If the setting is not found in the specified scope, the command indicates this:

```
Setting 'unknown.setting' not found in local scope.
```