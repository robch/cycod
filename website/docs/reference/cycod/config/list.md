# config list

Lists all configuration settings.

## Syntax

```bash
cycod config list [--scope]
```

## Description

The `cycod config list` command displays all configuration settings from the specified scope(s). By default, it shows settings from all scopes (local, user, and global).

When multiple scopes are shown, the settings are displayed in order from broadest to narrowest scope: global, then user, then local. This display order helps you understand which settings might be overriding others.

The command also shows the source of each setting (e.g., configuration file, environment variable, command line).

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global settings (all users) |
| `--user`, `-u` | Show only user settings (current user) |
| `--local`, `-l` | Show only local settings (current directory) |
| `--any`, `-a` | Show settings from all scopes (default) |

## Examples

### List all configuration settings from all scopes

```bash
cycod config list
```

This will display settings from the global, user, and local scopes, as well as any settings specified via command-line options or environment variables.

### List only user configuration settings

```bash
cycod config list --user
```

This will display only settings from the user scope, which are stored in the `.cycod/config.json` file in the user's home directory.

### List only local configuration settings

```bash
cycod config list --local
```

This will display only settings from the local scope, which are stored in the `.cycod/config.json` file in the current directory.

## Output

The command outputs a list of configuration settings grouped by scope:

```
SETTINGS (scope: global)
  azure.openai.endpoint: https://example.azure.com/openai
  app.allowedDomains: [example.com, trusted-site.org]

SETTINGS (scope: user)
  openai.apiKey: sk-***
  openai.chatModelName: gpt-4o
  app.preferredProvider: openai

SETTINGS (scope: local)
  azure.openai.chatDeployment: gpt-4
  app.autoSaveChatHistory: false
```

If no settings are found in a specific scope, the command will indicate this:

```
No settings found in local scope.
```

## Related Commands

- [cycod config get](./get.md) - Get the value of a specific configuration setting
- [cycod config set](./set.md) - Set a configuration value
- [cycod config clear](./clear.md) - Clear a configuration value
- [cycod config add](./add.md) - Add a value to a list setting
- [cycod config remove](./remove.md) - Remove a value from a list setting