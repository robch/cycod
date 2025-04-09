# config list

Lists all configuration settings.

## Syntax

```bash
chatx config list [options]
```

## Description

The `chatx config list` command displays all configuration settings. By default, it shows settings from all scopes.

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global settings (all users) |
| `--user`, `-u` | Show only user settings (current user) |
| `--local`, `-l` | Show only local settings (current directory) |
| `--any`, `-a` | Show settings from all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

List all configuration settings from all scopes:

```bash
chatx config list
```

List only user configuration settings:

```bash
chatx config list --user
```

Output all settings in JSON format:

```bash
chatx config list --json
```

## Output

The command outputs a list of configuration settings:

```
SETTINGS (scope: user)
  openai.apiKey: sk-***
  openai.chatModelName: gpt-4o
  app.preferredProvider: openai

SETTINGS (scope: local)
  azure.openai.endpoint: https://example.openai.azure.com
  azure.openai.apiKey: ***
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "user": {
    "openai.apiKey": "sk-***",
    "openai.chatModelName": "gpt-4o",
    "app.preferredProvider": "openai"
  },
  "local": {
    "azure.openai.endpoint": "https://example.openai.azure.com",
    "azure.openai.apiKey": "***"
  }
}
```

If no settings are found in the specified scope, the command will indicate this:

```
No settings found in local scope.
```