# config remove

Removes a value from a list configuration setting.

## Syntax

```bash
chatx config remove <key> <value> [options]
```

## Description

The `chatx config remove` command removes a specific value from a configuration setting that stores a list of values. By default, it operates on the local scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the list configuration setting |
| `<value>` | Value to remove from the list |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Remove from setting in global scope (all users) |
| `--user`, `-u` | Remove from setting in user scope (current user) |
| `--local`, `-l` | Remove from setting in local scope (current directory, default) |

## Examples

Remove a domain from trusted domains list in local scope:

```bash
chatx config remove app.trustedDomains example.com
```

Remove a source from trusted sources in user scope:

```bash
chatx config remove app.trustedSources github.com --user
```

Remove a model from allowed models in global scope:

```bash
chatx config remove app.allowedModels gpt-3.5-turbo --global
```

## Output

The command confirms successful removal:

```
Value 'example.com' has been removed from setting 'app.trustedDomains' in local scope.
```

If the value doesn't exist in the list or the setting doesn't exist, the command indicates this:

```
Value 'unknown.com' not found in setting 'app.trustedDomains' in local scope.
```

or

```
Setting 'app.unknownSetting' not found in local scope.
```