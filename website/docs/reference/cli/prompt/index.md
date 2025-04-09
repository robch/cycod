# prompt Command

The `chatx prompt` command allows you to manage custom prompts for CHATX.

## Syntax

```bash
chatx prompt SUBCOMMAND [options]
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`list`](list.md) | List defined prompts |
| [`get`](get.md) | Get details of a specific prompt |
| [`create`](create.md) | Create a new prompt |
| [`delete`](delete.md) | Delete a prompt |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Operate on global prompts (all users) |
| `--user`, `-u` | Operate on user prompts (current user) |
| `--local`, `-l` | Operate on local prompts (default) |
| `--any`, `-a` | Include prompts from all scopes (default for 'list' and 'get' commands) |

## Prompt Scopes

CHATX supports three prompt scopes:

- **Local**: Prompts apply only to the current directory, stored in `.chatx/prompts.json`
- **User**: Prompts apply to the current user across all directories, stored in `~/.chatx/prompts.json`
- **Global**: Prompts apply to all users on the system, stored in a system-wide location

## Examples

List all prompts from all scopes:

```bash
chatx prompt list
```

List only user prompts:

```bash
chatx prompt list --user
```

Get details of a specific prompt:

```bash
chatx prompt get code-review
```

Create a new prompt:

```bash
chatx prompt create translate "Translate the following text to {language}: {text}"
```

Delete a prompt:

```bash
chatx prompt delete code-review
```

## Notes

- Prompts from the local scope take precedence over user scope, which takes precedence over global scope
- Prompts can include variable placeholders in the format `{variable_name}` that are replaced with values when used
- Use the `--var` option when using prompts to provide values for placeholders