# prompt Command

The `cycod prompt` command allows you to manage custom prompts for CycoD.

## Syntax

```bash
cycod prompt SUBCOMMAND [options]
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

## Using Prompts

Custom prompts can be used in several ways:

1. **In interactive mode with slash commands**:
   ```bash
   # In an interactive session
   /code-review
   ```

2. **With the --prompt alias**:
   ```bash
   cycod --prompt code-review --input "function sum(a, b) { return a + b; }"
   ```

3. **Within --input and --inputs commands**:
   ```bash
   cycod --input "/code-review" --input "function sum(a, b) { return a + b; }"
   ```

4. **With variables**:
   ```bash
   cycod --prompt translate --var source_lang=English --var target_lang=Spanish
   ```

## Prompt Scopes

CycoD supports three prompt scopes:

- **Local**: Prompts apply only to the current directory, stored in `.cycod/prompts.json`
- **User**: Prompts apply to the current user across all directories, stored in `~/.cycod/prompts.json`
- **Global**: Prompts apply to all users on the system, stored in a system-wide location

## Examples

List all prompts from all scopes:

```bash
cycod prompt list
```

List only user prompts:

```bash
cycod prompt list --user
```

Get details of a specific prompt:

```bash
cycod prompt get code-review
```

Create a new prompt:

```bash
cycod prompt create translate "Translate the following text to {language}: {text}"
```

Delete a prompt:

```bash
cycod prompt delete code-review
```

## Notes

- Prompts from the local scope take precedence over user scope, which takes precedence over global scope
- Prompts can include variable placeholders in the format `{variable_name}` that are replaced with values when used
- Use the `--var` option when using prompts to provide values for placeholders
- When using the `--prompt` alias, prompt names without a leading slash will automatically have one added