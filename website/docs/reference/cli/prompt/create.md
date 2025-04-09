# prompt create

Creates a new custom prompt.

## Syntax

```bash
chatx prompt create <prompt-name> <content> [options]
```

## Description

The `chatx prompt create` command creates a new custom prompt with the specified name and content. By default, it creates the prompt in the local scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<prompt-name>` | Name for the new prompt |
| `<content>` | The content of the prompt |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Create in global scope (all users) |
| `--user`, `-u` | Create in user scope (current user) |
| `--local`, `-l` | Create in local scope (current directory, default) |

## Examples

Create a translation prompt in local scope:

```bash
chatx prompt create translate "Translate the following text to {language}: {text}"
```

Create a code review prompt in user scope:

```bash
chatx prompt create code-review "Please review this code and suggest improvements: {code}" --user
```

Create a debugging prompt in global scope:

```bash
chatx prompt create debug "Debug the following code and explain the issues: {code}" --global
```

## Prompt Variables

Prompts support variable placeholders in the format `{variable_name}` that are replaced with values when the prompt is used.

Examples of variable usage:

```bash
# Define a prompt with variables
chatx prompt create translate "Translate the following text to {language}: {text}"

# Use the prompt with variable values
chatx --prompt translate --var language=Spanish --var text="Hello, how are you?"
```

## Output

The command confirms successful creation:

```
Prompt 'translate' has been created in local scope.
```

If a prompt with the same name already exists in the specified scope, the command will display an error:

```
Error: Prompt 'translate' already exists in local scope. Use 'prompt delete' first to replace it.
```