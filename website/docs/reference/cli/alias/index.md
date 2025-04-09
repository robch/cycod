# alias Command

The `chatx alias` command allows you to manage command aliases for CHATX.

## Syntax

```bash
chatx alias SUBCOMMAND [options]
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

CHATX supports three alias scopes:

- **Local**: Aliases apply only to the current directory, stored in `.chatx/aliases.json`
- **User**: Aliases apply to the current user across all directories, stored in `~/.chatx/aliases.json`
- **Global**: Aliases apply to all users on the system, stored in a system-wide location

## Examples

List all aliases from all scopes:

```bash
chatx alias list
```

List only user aliases:

```bash
chatx alias list --user
```

Get details of a specific alias:

```bash
chatx alias get myalias
```

Delete an alias:

```bash
chatx alias delete myalias
```

## Notes

- Aliases from the local scope take precedence over user scope, which takes precedence over global scope
- Aliases provide a convenient way to create shortcuts for frequently used commands
- Aliases can include command options and arguments