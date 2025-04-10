# config add

Adds a value to a list configuration setting.

## Syntax

```bash
chatx config add <key> <value> [options]
```

## Description

The `chatx config add` command adds a value to a configuration setting that stores a list of values. This command is specifically designed for list-type settings and not for regular single-value settings (which use the [`config set`](./set.md) command). 

If the setting doesn't exist, it creates a new list with the specified value as its only item. By default, the command operates on the local scope (current directory).

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the list configuration setting |
| `<value>` | Value to add to the list |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Add to setting in global scope (all users) |
| `--user`, `-u` | Add to setting in user scope (current user) |
| `--local`, `-l` | Add to setting in local scope (current directory, default) |

## Examples

### Basic Examples

Add a trusted domain to the list:

```bash
chatx config add app.trustedDomains example.com
```

Add a trusted source to the user scope:

```bash
chatx config add app.trustedSources github.com --user
```

Add an allowed model to the global scope:

```bash
chatx config add app.allowedModels gpt-4o --global
```

For more practical scenarios and detailed examples, see the [Working with List Settings](../../../usage/config-list-settings.md) guide.

## Output

The command confirms successful addition:

```
Value 'example.com' has been added to setting 'app.trustedDomains' in local scope.
```

If the value already exists in the list, the command indicates this:

```
Value 'example.com' already exists in setting 'app.trustedDomains' in local scope.
```

## Error Handling

If you try to add a value to a setting that is not a list type, you'll receive an error:

```
Error: Cannot add to 'app.preferredProvider' because it is not a list setting.
Use 'config set' for single-value settings.
```

## Related Commands

- [`chatx config set`](./set.md) - For configuring single-value settings
- [`chatx config remove`](./remove.md) - For removing values from list settings
- [`chatx config list`](./list.md) - For listing all configuration settings
- [`chatx config get`](./get.md) - For viewing a specific configuration setting