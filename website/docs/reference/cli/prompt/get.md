# prompt get

Retrieves a specific prompt definition.

## Syntax

```bash
chatx prompt get <prompt-name> [options]
```

## Description

The `chatx prompt get` command displays the content of a specified prompt. By default, it searches for the prompt in all scopes.

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

Content: Translate the following text to {language}: {text}
Scope:   user
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "name": "translate",
  "content": "Translate the following text to {language}: {text}",
  "scope": "user"
}
```

If the prompt is not found, the command will display an error message:

```
Error: Prompt 'unknown-prompt' not found in any scope.
```