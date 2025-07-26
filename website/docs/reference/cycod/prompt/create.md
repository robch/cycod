# prompt create

Creates a new custom prompt that can be quickly accessed during chat sessions.

## Syntax

```bash
cycod prompt create <prompt-name> <content> [options]
```

## Description

The `cycod prompt create` command creates a new custom prompt with the specified name and content. Custom prompts are reusable text templates that can be quickly inserted into chat conversations by typing `/promptname`.

When you create a prompt, it is stored as a text file in a location determined by the scope option:

- **Local scope** (default): `.cycod/prompts/` in the current directory
- **User scope**: `.cycod/prompts/` in the user's home directory
- **Global scope**: `.cycod/prompts/` in the system-wide location

## Arguments

| Argument | Description |
|----------|-------------|
| `<prompt-name>` | Name for the new prompt (used as `/prompt-name` during chat) |
| `<content>` | The content of the prompt |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Create in global scope (all users) |
| `--user`, `-u` | Create in user scope (current user) |
| `--local`, `-l` | Create in local scope (current directory, default) |

## Using Prompts

Once created, prompts can be used in interactive chat sessions by typing a forward slash (`/`) followed by the prompt name:

```
/translate
Please translate the following text to Spanish:

Hello, how are you today?
```

In interactive sessions, CYCOD offers tab-completion for prompts. Simply type `/` and press Tab to see a list of available prompts.

## Prompt Variables

Prompts support variable placeholders in the format `{variable_name}` that are replaced with values when the prompt is used:

```bash
# Define a prompt with variables
cycod prompt create translate "Translate the following text to {language}: {text}"

# Use the prompt with variable values
cycod --prompt translate --var language=Spanish --var text="Hello, how are you?"
```

## Examples

### Create a Simple Prompt in Local Scope

```bash
cycod prompt create summarize "Please summarize the following text in three bullet points:"
```

### Create a Prompt in User Scope

```bash
cycod prompt create translate "Translate the following text to Spanish:" --user
```

### Create a Multi-line Prompt

```bash
cycod prompt create review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations 
3. Comment on style and readability"
```

### Create a Prompt with Variables

```bash
cycod prompt create translate "Translate the following text to {language}: {text}"
```

## Naming Requirements

When creating prompts:

- Prompt names cannot contain spaces or special characters
- If a prompt name starts with a slash (`/`), it will automatically be removed
- Prompt names should be descriptive yet concise

## Output

The command confirms successful creation:

```
Prompt 'translate' has been created in local scope.
```

If a prompt with the same name already exists in the specified scope, the command will display an error:

```
Error: Prompt 'translate' already exists in local scope. Use 'prompt delete' first to replace it.
```

## Modifying Prompts

To update an existing prompt, you must first delete it using `cycod prompt delete` and then create it again:

```bash
cycod prompt delete translate --user
cycod prompt create translate "New improved translation prompt" --user
```

## See Also

- [Custom Prompts](../../../advanced/prompts.md)
- [cycod prompt list](../prompt/list.md)
- [cycod prompt get](../prompt/get.md)
- [cycod prompt delete](../prompt/delete.md)