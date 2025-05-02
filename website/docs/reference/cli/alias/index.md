# alias Command

The `cycod alias` command allows you to manage command aliases for CYCOD.

## Syntax

```bash
cycod alias SUBCOMMAND [options]
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`list`](/reference/cli/alias/list.md) | List defined aliases |
| [`get`](/reference/cli/alias/get.md) | Get details of a specific alias |
| [`delete`](/reference/cli/alias/delete.md) | Delete an alias |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Operate on global aliases (all users) |
| `--user`, `-u` | Operate on user aliases (current user) |
| `--local`, `-l` | Operate on local aliases (default) |
| `--any`, `-a` | Include aliases from all scopes (default for 'list' and 'get' commands) |

## Alias Scopes

CYCOD supports three alias scopes:

- **Local**: Aliases apply only to the current directory, stored in `.cycod/aliases.json`
- **User**: Aliases apply to the current user across all directories, stored in `~/.cycod/aliases.json`
- **Global**: Aliases apply to all users on the system, stored in a system-wide location

## Examples

List all aliases from all scopes:

```bash
cycod alias list
```

List only user aliases:

```bash
cycod alias list --user
```

Get details of a specific alias:

```bash
cycod alias get myalias
```

Delete an alias:

```bash
cycod alias delete myalias
```

## Notes

- Aliases from the local scope take precedence over user scope, which takes precedence over global scope
- Aliases provide a convenient way to create shortcuts for frequently used commands
- Aliases can include command options and arguments