# config remove

Removes a value from a list configuration setting.

## Syntax

```bash
cycod config remove <key> <value> [--scope]
```

## Description

The `cycod config remove` command removes a specific value from a configuration setting that stores a list of values. This command is specifically designed for list-type settings where multiple values are stored together, not for single-value settings.

Unlike `config clear` which removes an entire setting, `config remove` only removes a single value from a list while preserving the rest of the list.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the list configuration setting |
| `<value>` | Specific value to remove from the list |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Remove from setting in global scope (all users) |
| `--user`, `-u` | Remove from setting in user scope (current user) |
| `--local`, `-l` | Remove from setting in local scope (current directory, default) |

## Examples

Remove a domain from trusted domains list:

```bash
cycod config remove app.trustedDomains example.com
```

Remove a file extension from allowed file types in user scope:

```bash
cycod config remove app.allowedFileTypes .exe --user
```

Disable an experimental feature by removing it from enabled features:

```bash
cycod config remove app.enabledFeatures beta-ui --user
```

Remove a model from globally allowed models:

```bash
cycod config remove app.allowedModels gpt-3.5-turbo --global
```

Remove a source from trusted sources:

```bash
cycod config remove app.trustedSources github.com --user
```

## Output

When successful, the command confirms the removal:

```
Value 'example.com' has been removed from setting 'app.trustedDomains' in local scope.
```

If the value doesn't exist in the list, the command will indicate this:

```
Value 'unknown.com' not found in setting 'app.trustedDomains' in local scope.
```

If the setting itself doesn't exist, you'll see:

```
Setting 'app.unknownSetting' not found in local scope.
```

## When to Use

Use `config remove` when you need to:

- Remove a domain from your trusted domains list
- Remove a file extension from allowed file types
- Disable an experimental feature by removing it from enabled features
- Remove an AI model from your allowed models list
- Remove any item from any list-type configuration setting

For removing entire settings (not just individual list items), use [`config clear`](./clear.md) instead.

## Common List Settings

This command is commonly used with these list-type settings:

| Setting Name | Description |
|--------------|-------------|
| `app.trustedDomains` | Domains allowed for external connections |
| `app.allowedFileTypes` | File extensions CycoD can process |
| `app.allowedModels` | AI models permitted for use |
| `app.enabledFeatures` | Experimental features to enable |
| `app.disabledFeatures` | Features to disable |

## Related Commands

- [`cycod config add`](./add.md) - Add a value to a list setting
- [`cycod config get`](./get.md) - View the current values in a setting
- [`cycod config clear`](./clear.md) - Remove an entire setting (not just a single value)
- [`cycod config set`](./set.md) - Set the value of a single-value setting

## See Also

- [Working with List Settings](../../../usage/config-list-settings.md) - Guide on managing list settings
- [Configuration](../../../usage/configuration.md) - General configuration guide