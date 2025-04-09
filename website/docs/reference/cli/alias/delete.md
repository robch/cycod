# alias delete

Deletes a command alias.

## Syntax

```bash
chatx alias delete <alias-name> [options]
```

## Description

The `chatx alias delete` command removes a specified alias. By default, it searches for the alias in all scopes and deletes the first occurrence found.

## Arguments

| Argument | Description |
|----------|-------------|
| `<alias-name>` | Name of the alias to delete |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Delete from global scope only (all users) |
| `--user`, `-u` | Delete from user scope only (current user) |
| `--local`, `-l` | Delete from local scope only (current directory) |
| `--any`, `-a` | Search in all scopes (default) |
| `--force`, `-f` | Delete without confirmation prompt |

## Examples

Delete an alias named "creative":

```bash
chatx alias delete creative
```

Delete an alias named "quickchat" from local scope only:

```bash
chatx alias delete quickchat --local
```

Delete an alias named "debug" without confirmation:

```bash
chatx alias delete debug --force
```

## Output

When an alias is successfully deleted, the command displays a confirmation message:

```
Alias 'creative' has been deleted from user scope.
```

If the alias is not found in the specified scope, the command will display an error message:

```
Error: Alias 'unknown-alias' not found in local scope.
```

## Confirmation

By default, the command will prompt for confirmation before deleting an alias:

```
Are you sure you want to delete alias 'creative' from user scope? (y/n): 
```

To skip this confirmation, use the `--force` option.