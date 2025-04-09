# prompt list

Lists all defined prompts.

## Syntax

```bash
chatx prompt list [options]
```

## Description

The `chatx prompt list` command displays all defined prompts. By default, it shows prompts from all scopes.

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global prompts (all users) |
| `--user`, `-u` | Show only user prompts (current user) |
| `--local`, `-l` | Show only local prompts (current directory) |
| `--any`, `-a` | Show prompts from all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

List all prompts from all scopes:

```bash
chatx prompt list
```

List only user prompts:

```bash
chatx prompt list --user
```

Output all prompts in JSON format:

```bash
chatx prompt list --json
```

## Output

The command outputs a list of prompt definitions:

```
PROMPTS (scope: user)
  translate: Translate the following text to {language}: {text}
  code-review: Please review this code and suggest improvements: {code}

PROMPTS (scope: local)
  bug-fix: Debug the following code and identify issues: {code}
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "user": {
    "translate": "Translate the following text to {language}: {text}",
    "code-review": "Please review this code and suggest improvements: {code}"
  },
  "local": {
    "bug-fix": "Debug the following code and identify issues: {code}"
  }
}
```

If no prompts are found in the specified scope, the command will indicate this:

```
No prompts found in local scope.
```