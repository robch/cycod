# alias delete

Deletes a command alias from a specific scope or from any scope.

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

## Examples

Delete an alias named "creative" from any scope:

```bash
chatx alias delete creative
```

Delete an alias named "quickchat" from local scope only:

```bash
chatx alias delete quickchat --local
```

Delete an alias named "python-expert" from user scope only:

```bash
chatx alias delete python-expert --user
```

Delete an alias named "debug-mode" from global scope only:

```bash
chatx alias delete debug-mode --global
```

## Output

When an alias is successfully deleted, the command displays confirmation:

```
Deleted: C:\Users\username\.chatx\aliases\creative.alias
```

If the alias contained multiple files (for complex aliases), it will also delete and list those:

```
Deleted: C:\Users\username\.chatx\aliases\creative.alias
Deleted: C:\Users\username\.chatx\aliases\creative-1.alias
```

If the alias is not found in the specified scope, the command will display an error message:

```
Error: Alias 'unknown-alias' not found in specified scope.
```

Or if searching in any scope and not found:

```
Error: Alias 'unknown-alias' not found in any scope.
```

## Related Commands

- [chatx alias list](list.md) - List all available aliases
- [chatx alias get](get.md) - Display the content of a specific alias

## See Also

- [Using Aliases in ChatX](/usage/aliases.md)