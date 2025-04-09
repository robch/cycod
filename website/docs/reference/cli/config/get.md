# config get

Retrieves the value of a specific configuration setting.

## Syntax

```bash
chatx config get <key> [options]
```

## Description

The `chatx config get` command displays the value of a specified configuration setting. By default, it searches for the setting in all scopes.

## Arguments

| Argument | Description |
|----------|-------------|
| `<key>` | Name of the configuration setting to retrieve |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Look only in global scope (all users) |
| `--user`, `-u` | Look only in user scope (current user) |
| `--local`, `-l` | Look only in local scope (current directory) |
| `--any`, `-a` | Look in all scopes (default) |
| `--json`, `-j` | Output result in JSON format |

## Examples

Get the value of the OpenAI API key:

```bash
chatx config get openai.apiKey
```

Get the preferred provider setting from user scope only:

```bash
chatx config get app.preferredProvider --user
```

Get the Azure OpenAI endpoint in JSON format:

```bash
chatx config get azure.openai.endpoint --json
```

## Output

The command outputs the value of the specified setting:

```
VALUE: https://example.openai.azure.com
SCOPE: user
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "key": "azure.openai.endpoint",
  "value": "https://example.openai.azure.com",
  "scope": "user"
}
```

If the setting is not found, the command will display an error message:

```
Error: Setting 'unknown.setting' not found in any scope.
```