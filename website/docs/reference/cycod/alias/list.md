# alias list

Lists all defined command aliases for CYCOD.

## Syntax

```bash
cycod alias list [options]
```

## Description

The `cycod alias list` command displays a list of all defined aliases. By default, it shows aliases from all scopes.

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global aliases (all users) |
| `--user`, `-u` | Show only user aliases (current user) |
| `--local`, `-l` | Show only local aliases (current directory) |
| `--any`, `-a` | Show aliases from all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

List all aliases from all scopes:

```bash
cycod alias list
```

List only user aliases:

```bash
cycod alias list --user
```

List all aliases in JSON format:

```bash
cycod alias list --json
```

## Output

The command outputs a table with the following columns:

- **Name**: The alias name
- **Command**: The command that the alias runs
- **Scope**: Where the alias is defined (local, user, or global)

Sample output:

```
ALIASES

Name       Command                        Scope
---------- ------------------------------ ------
creative   chat --creative                user
debug      chat --verbose --debug         user
quickchat  chat --model gpt-4o --minimal  local
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "aliases": [
    {
      "name": "creative",
      "command": "chat --creative",
      "scope": "user"
    },
    {
      "name": "debug",
      "command": "chat --verbose --debug",
      "scope": "user"
    },
    {
      "name": "quickchat",
      "command": "chat --model gpt-4o --minimal",
      "scope": "local"
    }
  ]
}
```