# prompt delete

Deletes a custom prompt from a specific scope.

## Syntax

```bash
cycod prompt delete <prompt-name> [options]
```

## Description

The `cycod prompt delete` command removes a specified custom prompt. When a prompt is found and deleted, the command:
- Deletes the main prompt file
- Deletes any referenced files (if the prompt uses external content) 
- Displays the paths of all deleted files

By default, it attempts to delete from any scope, searching in this order: local, user, then global.

## Arguments

| Argument | Description |
|----------|-------------|
| `<prompt-name>` | Name of the prompt to delete |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Delete only from global scope (all users) |
| `--user`, `-u` | Delete only from user scope (current user) |
| `--local`, `-l` | Delete only from local scope (current directory) |
| `--any`, `-a` | Delete from the first scope where found (default) |
| `--yes`, `-y` | Skip confirmation prompt |

## Examples

Delete a prompt named "translate" from any scope (first match):

```bash
cycod prompt delete translate
```

Delete a prompt named "code-review" from user scope:

```bash
cycod prompt delete code-review --user
```

Delete a prompt named "debug" from global scope without confirmation:

```bash
cycod prompt delete debug --global --yes
```

Delete a prompt named "legacy" specifically from local scope:

```bash
cycod prompt delete legacy --local
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

## Notes

- Prompt deletions are permanent and cannot be undone
- If you need to modify a prompt, you'll need to delete it first then create it again with new content
- When using `--any` (the default), CycoD will delete the first matching prompt found when searching in this order: local, user, global

## See Also

- [prompt create](create.md)
- [prompt list](list.md)
- [prompt get](get.md)
- [Custom Prompts](../../../advanced/prompts.md)