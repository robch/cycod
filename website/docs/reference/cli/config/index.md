# config Command

The `chatx config` command allows you to manage CHATX configuration settings.

## Syntax

```bash
chatx config SUBCOMMAND [options]
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`list`](list.md) | List configuration settings |
| [`get`](get.md) | Get the value of a configuration setting |
| [`set`](set.md) | Set the value of a configuration setting |
| [`clear`](clear.md) | Clear a configuration setting |
| [`add`](add.md) | Add a value to a list setting |
| [`remove`](remove.md) | Remove a value from a list setting |

## Options

### Scope Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Use global scope (all users) |
| `--user`, `-u` | Use user scope (current user) |
| `--local`, `-l` | Use local scope (default for most commands) |
| `--any`, `-a` | Include settings from all scopes (default for 'list' and 'get' commands) |

## Configuration Scopes

CHATX supports three configuration scopes:

- **Local**: Settings apply only to the current directory, stored in `.chatx/config.json`
- **User**: Settings apply to the current user across all directories, stored in `~/.chatx/config.json`
- **Global**: Settings apply to all users on the system, stored in a system-wide location

## Common Configuration Keys

### App Settings

| Setting | Description | Default | Example |
|---------|-------------|---------|---------|
| `app.preferredProvider` | Default AI provider | `"openai"` | `"azure-openai"` |
| `app.autoSaveChatHistory` | Automatically save chat history | `true` | `false` |
| `app.autoSaveTrajectory` | Automatically save trajectory | `true` | `false` |

### OpenAI Settings

| Setting | Description | Example |
|---------|-------------|---------|
| `openai.apiKey` | OpenAI API key | `"sk-..."` |
| `openai.chatModelName` | OpenAI chat model name | `"gpt-4o"` |

### Azure OpenAI Settings

| Setting | Description | Example |
|---------|-------------|---------|
| `azure.openai.endpoint` | Azure OpenAI endpoint | `"https://example.openai.azure.com"` |
| `azure.openai.apiKey` | Azure OpenAI API key | `"..."` |
| `azure.openai.chatDeployment` | Azure OpenAI chat deployment name | `"gpt-4"` |

### GitHub Copilot Settings

| Setting | Description | Example |
|---------|-------------|---------|
| `copilot.modelName` | Copilot model name | `"claude-3.7-sonnet"` |
| `github.token` | GitHub token for Copilot access | `"ghu_..."` |

## Examples

List all configuration settings from all scopes:

```bash
chatx config list
```

List only user configuration settings:

```bash
chatx config list --user
```

Get the OpenAI API key:

```bash
chatx config get openai.apiKey
```

Set the OpenAI API key in user scope:

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

Set the preferred provider in user scope:

```bash
chatx config set app.preferredProvider azure-openai --user
```

Clear the Azure OpenAI endpoint in local scope:

```bash
chatx config clear azure.openai.endpoint
```

Add a trusted domain to a list:

```bash
chatx config add app.trustedDomains example.com
```

Remove a domain from the trusted domains list:

```bash
chatx config remove app.trustedDomains example.com
```

## Notes

- Settings from the local scope take precedence over user scope, which takes precedence over global scope
- For security-sensitive settings like API keys, use user scope rather than local or global scope
- Configuration settings can be overridden by command-line options
- Environment variables can also be used to override configuration settings