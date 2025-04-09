# config add

Adds a value to a list configuration setting.

## Syntax

```bash
chatx config add <key> <value> [options]
```

## Description

The `chatx config add` command adds a value to a configuration setting that stores a list of values. If the setting doesn't exist, it creates a new list with the specified value. By default, it operates on the local scope.

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

## Output

The command confirms successful addition:

```
Value 'example.com' has been added to setting 'app.trustedDomains' in local scope.
```

If the value already exists in the list, the command indicates this:

```
Value 'example.com' already exists in setting 'app.trustedDomains' in local scope.
```