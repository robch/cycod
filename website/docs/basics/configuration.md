---
hide:
- toc
icon: material/cog-outline
---

--8<-- "snippets/ai-generated.md"

# Configuration System

ChatX provides a flexible configuration system that helps you customize its behavior at different scopes.

## Basic Configuration

Let's start by listing all available configuration settings:

``` { .bash .cli-command title="List all settings (from all scopes)" }
chatx config list
```

``` { .plaintext .cli-output }
LOCATION: C:\ProgramData\.chatx\config.yaml (global)

  No configuration settings found.

LOCATION: C:\Users\username\.chatx\config.yaml (user)

  No configuration settings found.

LOCATION: C:\project\.chatx\config (local)

  github.token: gh************************************wU
```

After logging in with GitHub, you can see your token configured:

``` { .bash .cli-command title="Authenticate with GitHub" }
chatx github login
```

``` { .plaintext .cli-output }
Please complete authentication in your browser...
Authentication successful!
GitHub token saved to user configuration.
```

``` { .bash .cli-command title="List settings from user scope" }
chatx config list --user
```

``` { .plaintext .cli-output }
LOCATION: C:\Users\username\.chatx\config.yaml (user)

  github.token: gh************************************wU
```

??? tip "Understanding Configuration Scopes"

    ChatX supports three configuration scopes:
    
    - **Local** - Settings for the current directory (`.chatx/config.yaml`)
    - **User** - Settings for the current user (`~/.chatx/config.yaml`)
    - **Global** - Settings for all users on the system (`/etc/chatx/config.yaml` or `C:\ProgramData\.chatx\config.yaml`)
    
    The scopes follow a precedence order: Local overrides User, which overrides Global.
    This allows you to:
    
    - Set organization-wide defaults in Global scope
    - Configure personal preferences in User scope
    - Override settings for specific projects in Local scope

## Getting Settings

Use the `get` command to view specific settings:

``` { .bash .cli-command title="Get a setting (searches all scopes by default)" }
chatx config get github.token
```

``` { .plaintext .cli-output }
github.token: gh************************************wU
```

Notice how sensitive information is automatically obfuscated for security. This happens for all API keys and tokens.

``` { .bash .cli-command title="Get a setting from a specific scope" }
chatx config get github.token --user
```

``` { .plaintext .cli-output }
github.token: gh************************************wU
```

## Setting Values

Let's configure the Copilot model to use:

``` { .bash .cli-command title="Set Copilot model preference" }
chatx config set copilot.modelName claude-3.7-sonnet --user
```

``` { .plaintext .cli-output }
copilot.modelName: claude-3.7-sonnet
```

Now we can verify our change:

``` { .bash .cli-command title="List settings again to verify" }
chatx config list --user
```

``` { .plaintext .cli-output }
LOCATION: C:\Users\username\.chatx\config.yaml (user)

  copilot.modelName: claude-3.7-sonnet
  github.token: gh************************************wU
```

``` { .bash .cli-command title="Get the specific setting" }
chatx config get copilot.modelName
```

``` { .plaintext .cli-output }
copilot.modelName: claude-3.7-sonnet
```

You can set values in different scopes depending on your needs:

``` { .bash .cli-command title="Set a value in local scope (default)" }
chatx config set app.preferredProvider copilot
```

``` { .plaintext .cli-output }
app.preferredProvider: copilot
```

``` { .bash .cli-command title="Set a value in user scope" }
chatx config set app.maxTokens 4000 --user
```

``` { .plaintext .cli-output }
app.maxTokens: 4000
```

``` { .bash .cli-command title="Set a value in global scope (requires admin/sudo)" }
chatx config set app.trimTokenTarget 32000 --global
```

``` { .plaintext .cli-output }
app.trimTokenTarget: 32000
```

## Clearing Settings

If you need to remove a setting:

``` { .bash .cli-command title="Clear a setting" }
chatx config clear app.preferredProvider
```

``` { .plaintext .cli-output }
app.preferredProvider: (cleared)
```

``` { .bash .cli-command title="Clear a setting from a specific scope" }
chatx config clear copilot.modelName --user
```

``` { .plaintext .cli-output }
copilot.modelName: (cleared)
```

## Managing Lists

Some configuration settings can be lists of values.

``` { .bash .cli-command title="Add a value to a list" }
chatx config set app.allowedDomains example.com --user
```

``` { .plaintext .cli-output }
LOCATION: C:\Users\username\.chatx\config.yaml (user)

  app.allowedDomains: example.com
```

``` { .bash .cli-command title="Add another value to the list" }
chatx config add app.allowedDomains trusted-site.org --user
```

``` { .plaintext .cli-output }
app.allowedDomains:
- example.com
- trusted-site.org
```

``` { .bash .cli-command title="Remove a value from a list" }
chatx config remove app.allowedDomains example.com --user
```

``` { .plaintext .cli-output }
app.allowedDomains:
  - trusted-site.org
```

## Common Configuration Scenarios

### GitHub/Copilot Settings

``` { .bash .cli-command title="Configure GitHub Copilot" }
chatx github login
```

``` { .plaintext .cli-output }
Please complete authentication in your browser...
Authentication successful!
GitHub token saved to user configuration.
```

``` { .bash .cli-command title="Set Copilot model preferences" }
chatx config set copilot.modelName claude-3.7-sonnet --user
chatx config set copilot.apiEndpoint https://api.githubcopilot.com --user
```

``` { .plaintext .cli-output }
copilot.modelName: claude-3.7-sonnet
copilot.apiEndpoint: https://api.githubcopilot.com
```

### Provider Selection

``` { .bash .cli-command title="Set your preferred AI provider" }
chatx config set app.preferredProvider copilot --user
```

``` { .plaintext .cli-output }
app.preferredProvider: copilot
```

### OpenAI Settings

``` { .bash .cli-command title="Configure OpenAI" }
chatx config set openai.apiKey sk-yourapikeyhere --user
chatx config set openai.chatModelName gpt-4o --user
```

``` { .plaintext .cli-output }
openai.apiKey: sk-****************
openai.chatModelName: gpt-4o
```

### Azure OpenAI Settings

``` { .bash .cli-command title="Configure Azure OpenAI" }
chatx config set azure.openai.endpoint https://your-resource.openai.azure.com --user
chatx config set azure.openai.apiKey yourazureapikey --user
chatx config set azure.openai.chatDeployment gpt-4 --user
```

``` { .plaintext .cli-output }
azure.openai.endpoint: https://your-resource.openai.azure.com
azure.openai.apiKey: *****************
azure.openai.chatDeployment: gpt-4
```

### Chat History Settings

``` { .bash .cli-command title="Configure auto-saving behavior" }
chatx config set app.autoSaveChatHistory true --user
chatx config set app.autoSaveTrajectory true --user
```

``` { .plaintext .cli-output }
app.autoSaveChatHistory: true
app.autoSaveTrajectory: true
```

## Using Configuration Profiles

Profiles let you load collections of settings for different contexts.

Create a YAML profile file in `.chatx/profiles/`:

```yaml title=".chatx/profiles/work.yaml"
app:
  preferredProvider: azure-openai
  autoSaveChatHistory: true
  
azure:
  openai:
    chatDeployment: gpt-4
    endpoint: https://your-work-endpoint.openai.azure.com
```

Then use it with the `--profile` option:

``` { .bash .cli-command title="Use a configuration profile" }
chatx --profile work --question "What's on my agenda today?"
```

``` { .plaintext .cli-output }
I don't have direct access to your agenda or calendar, but I can help you organize your day or remind you of common work tasks. 

If you have specific meetings or deadlines today that you'd like to review, please let me know and I can help you create a plan or checklist for your workday.

Would you like me to help you create a general work agenda template?
```

## Configuration File Format

ChatX uses YAML configuration files. Here's an example of what a typical configuration file might look like:

```yaml title="Example configuration file (.chatx/config.yaml)"
app:
  preferredProvider: copilot
  autoSaveChatHistory: true
  autoSaveTrajectory: true
  trimTokenTarget: 18000
  
openai:
  chatModelName: gpt-4o
  apiKey: sk-yourapikeyhere
  
azure:
  openai:
    endpoint: https://your-resource.openai.azure.com
    apiKey: yourazureapikey
    chatDeployment: gpt-4
    
copilot:
  modelName: claude-3.7-sonnet
  
github:
  token: ghu_yourgithubtoken
```

## Configuration Reference

### Common Settings

| Setting (dot notation) | Command Line Option | Environment Variable |
|------------------------|---------------------|----------------------|
| **Application Settings** |||
| app.preferredProvider | --preferred-provider | CHATX_PREFERRED_PROVIDER |
| app.maxTokens | --max-tokens | APP_MAX_TOKENS |
| app.trimTokenTarget | --trim-token-target | CHATX_TRIM_TOKEN_TARGET |
| app.autoSaveChatHistory | --auto-save-chat-history | CHATX_AUTO_SAVE_CHAT_HISTORY |
| app.autoSaveTrajectory | --auto-save-trajectory | CHATX_AUTO_SAVE_TRAJECTORY |
| app.chatCompletionTimeout | --chat-completion-timeout | CHATX_CHAT_COMPLETION_TIMEOUT |
| **GitHub/Copilot Settings** |||
| github.token | --github-token | GITHUB_TOKEN |
| copilot.modelName | --copilot-model-name | COPILOT_MODEL_NAME |
| copilot.apiEndpoint | --copilot-api-endpoint | COPILOT_API_ENDPOINT |
| copilot.integrationId | --copilot-integration-id | COPILOT_INTEGRATION_ID |
| copilot.hmacKey | --copilot-hmac-key | COPILOT_HMAC_KEY |
| **OpenAI Settings** |||
| openai.apiKey | --openai-api-key | OPENAI_API_KEY |
| openai.chatModelName | --openai-chat-model-name | OPENAI_CHAT_MODEL_NAME |
| **Azure OpenAI Settings** |||
| azure.openai.apiKey | --azure-openai-api-key | AZURE_OPENAI_API_KEY |
| azure.openai.endpoint | --azure-openai-endpoint | AZURE_OPENAI_ENDPOINT |
| azure.openai.chatDeployment | --azure-openai-chat-deployment | AZURE_OPENAI_CHAT_DEPLOYMENT |

??? tip "Configuration Precedence Example"

    Let's say you want to configure the Copilot model name in different ways:
    
    ```bash
    # In your user config file (~/.chatx/config.yaml)
    copilot:
      modelName: claude-3.5-sonnet
    
    # In a profile file (.chatx/profiles/work.yaml)
    copilot:
      modelName: claude-3.7-sonnet
    
    # As an environment variable
    set COPILOT_MODEL_NAME=claude-3.7-haiku
    
    # On the command line
    chatx --copilot-model-name claude-3.7-opus --profile work
    ```
    
    The precedence order would be:
    
    1. Command line option: `claude-3.7-opus` (highest precedence)
    2. Environment variable: `claude-3.7-haiku` 
    3. Profile setting: `claude-3.7-sonnet`
    4. User config: `claude-3.5-sonnet` (lowest precedence)
    
    So in this example, `claude-3.7-opus` would be used since the command line has the highest precedence.