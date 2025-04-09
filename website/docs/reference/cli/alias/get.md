# alias get

Retrieves information about a specific command alias.

## Syntax

```bash
chatx alias get <alias-name> [options]
```

## Description

The `chatx alias get` command displays details about a specified alias. By default, it searches for the alias in all scopes.

## Arguments

| Argument | Description |
|----------|-------------|
| `<alias-name>` | Name of the alias to retrieve |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Look only in global scope (all users) |
| `--user`, `-u` | Look only in user scope (current user) |
| `--local`, `-l` | Look only in local scope (current directory) |
| `--any`, `-a` | Look in all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

Get details about an alias named "creative":

```bash
chatx alias get creative
```

Get details about an alias named "quickchat" from local scope only:

```bash
chatx alias get quickchat --local
```

Get details about an alias named "debug" in JSON format:

```bash
chatx alias get debug --json
```

## Output

The command outputs detailed information about the specified alias:

```
ALIAS: creative

Command: chat --creative
Scope:   user
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "name": "creative",
  "command": "chat --creative",
  "scope": "user"
}
```

If the alias is not found, the command will display an error message:

```
Error: Alias 'unknown-alias' not found in any scope.
```