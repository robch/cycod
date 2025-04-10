# prompt get

Retrieves and displays the content of a specific custom prompt.

## Syntax

```bash
chatx prompt get <prompt-name> [options]
```

## Description

The `chatx prompt get` command displays the content and details of a specified prompt. When a prompt is found, the command shows:

- The prompt name
- The file location and scope
- The prompt content
- Usage example for the prompt in chat

By default, it searches for the prompt in all scopes (local, user, and global).

## Arguments

| Argument | Description |
|----------|-------------|
| `<prompt-name>` | Name of the prompt to retrieve |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Look only in global scope (all users) |
| `--user`, `-u` | Look only in user scope (current user) |
| `--local`, `-l` | Look only in local scope (current directory) |
| `--any`, `-a` | Look in all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

Get details of a prompt named "translate":

```bash
chatx prompt get translate
```

Get details of a prompt named "code-review" from user scope only:

```bash
chatx prompt get code-review --user
```

Get details of a prompt named "bug-fix" in JSON format:

```bash
chatx prompt get bug-fix --json
```

## Output

The command outputs detailed information about the specified prompt:

```
PROMPT: translate

File:    /Users/username/.chatx/prompts/translate.txt
Scope:   user
Content: Translate the following text to {language}: {text}

Example usage in chat: /translate
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "name": "translate",
  "file": "/Users/username/.chatx/prompts/translate.txt",
  "content": "Translate the following text to {language}: {text}",
  "scope": "user"
}
```

If the prompt is not found, the command will display an error message:

```
Error: Prompt 'unknown-prompt' not found in any scope.
```

## Usage in Chat Sessions

After viewing the prompt details, you can use it in an interactive chat session by typing a forward slash followed by the prompt name:

```
user@CHAT> /translate
Translate the following text to {language}: {text}

user@CHAT> 
```

## See Also

- [prompt list](list.md) - List all available prompts
- [prompt create](create.md) - Create a new prompt
- [prompt delete](delete.md) - Delete a prompt
- [Custom Prompts Guide](/advanced/prompts/) - Learn more about using custom prompts