# prompt delete

Deletes a custom prompt.

## Syntax

```bash
chatx prompt delete <prompt-name> [options]
```

## Description

The `chatx prompt delete` command removes a specified prompt. By default, it attempts to delete from the local scope first, then user scope, then global scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<prompt-name>` | Name of the prompt to delete |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Delete from global scope (all users) |
| `--user`, `-u` | Delete from user scope (current user) |
| `--local`, `-l` | Delete from local scope (current directory, default) |
| `--any`, `-a` | Delete from all scopes where found |
| `--yes`, `-y` | Skip confirmation prompt |

## Examples

Delete a prompt named "translate" from local scope:

```bash
chatx prompt delete translate
```

Delete a prompt named "code-review" from user scope:

```bash
chatx prompt delete code-review --user
```

Delete a prompt named "debug" from global scope without confirmation:

```bash
chatx prompt delete debug --global --yes
```

Delete a prompt named "legacy" from all scopes where it exists:

```bash
chatx prompt delete legacy --any
```

## Output

The command confirms successful deletion:

```
Prompt 'translate' has been deleted from local scope.
```

If the prompt is not found in the specified scope, the command will display an error:

```
Error: Prompt 'unknown-prompt' not found in local scope.
```

By default, the command will ask for confirmation before deleting:

```
Are you sure you want to delete prompt 'translate' from local scope? [y/N]:
```

Use the `--yes` or `-y` option to skip the confirmation.