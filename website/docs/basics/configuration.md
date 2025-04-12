---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Configuration System

ChatX provides a flexible, scoped configuration system that lets you manage settings at different levels.

## Configuration Scopes

``` { .bash .cli-command title="Three levels of configuration" }
chatx config list-scopes
```

``` { .plaintext .cli-output }
Configuration scopes:
  Local: Settings for the current directory (.chatx/)
  User: Settings for the current user (~/.chatx/)
  Global: Settings for all users on the system
```

## Getting Settings

``` { .bash .cli-command title="List all settings" }
chatx config list
```

``` { .plaintext .cli-output }
Configuration settings:

Global (C:\ProgramData\chatx\config.ini):
  No settings

User (C:\Users\username\.chatx\config.ini):
  openai-chat-model-name = gpt-4o
  use-openai = true
  chat-history = C:\Users\username\.chatx\history\recent.jsonl

Local (.\.chatx\config.ini):
  default-prompt = code-review
  save-chat-history = true
```

``` { .bash .cli-command title="Get a specific setting" }
chatx config get openai-chat-model-name
```

``` { .plaintext .cli-output }
openai-chat-model-name = gpt-4o (source: User)
```

``` { .bash .cli-command title="Get a setting with its scope" }
chatx config get save-chat-history --show-scope
```

``` { .plaintext .cli-output }
save-chat-history = true (source: Local)
```

## Setting Values

``` { .bash .cli-command title="Set a value (defaults to Local scope)" }
chatx config set default-prompt code-review
```

``` { .plaintext .cli-output }
✓ default-prompt = code-review (scope: Local)
```

``` { .bash .cli-command title="Set a value in User scope" }
chatx config set openai-chat-model-name gpt-4o --user
```

``` { .plaintext .cli-output }
✓ openai-chat-model-name = gpt-4o (scope: User)
```

``` { .bash .cli-command title="Set a value in Global scope (requires admin/sudo)" }
chatx config set max-output-tokens 4096 --global
```

``` { .plaintext .cli-output }
✓ max-output-tokens = 4096 (scope: Global)
```

## Managing Lists

``` { .bash .cli-command title="Add a value to a list" }
chatx config add-to-list allowed-models gpt-3.5-turbo
```

``` { .plaintext .cli-output }
✓ Added 'gpt-3.5-turbo' to allowed-models (scope: Local)
```

``` { .bash .cli-command title="Remove a value from a list" }
chatx config remove-from-list allowed-models gpt-3.5-turbo
```

``` { .plaintext .cli-output }
✓ Removed 'gpt-3.5-turbo' from allowed-models (scope: Local)
```

## Clearing Settings

``` { .bash .cli-command title="Clear a setting" }
chatx config clear default-prompt
```

``` { .plaintext .cli-output }
✓ Cleared default-prompt (scope: Local)
```

``` { .bash .cli-command title="Clear a setting from a specific scope" }
chatx config clear openai-chat-model-name --user
```

``` { .plaintext .cli-output }
✓ Cleared openai-chat-model-name (scope: User)
```

## Using Profiles

Profiles let you save and load collections of settings for different contexts.

``` { .bash .cli-command title="Save current configuration as a profile" }
chatx config save-profile work
```

``` { .plaintext .cli-output }
✓ Saved profile 'work' to C:\Users\username\.chatx\profiles\work.ini
```

``` { .bash .cli-command title="Load a saved profile" }
chatx --profile work
```

``` { .plaintext .cli-output }
✓ Loaded profile 'work' from C:\Users\username\.chatx\profiles\work.ini
```

``` { .bash .cli-command title="List available profiles" }
chatx config list-profiles
```

``` { .plaintext .cli-output }
Available profiles:
  work - Last used: 2025-04-12
  home - Last used: 2025-04-10
  python-dev - Last used: 2025-04-05
```

## Configuration File Format

ChatX uses INI-style configuration files. You can edit them manually if needed:

```ini
[general]
save-chat-history = true
default-prompt = code-review

[openai]
chat-model-name = gpt-4o
api-key = sk-XXXX

[azure-openai]
endpoint = https://my-endpoint.openai.azure.com
api-key = XXX
deployment-name = gpt4
```

You can also use environment variables for configuration:

```bash
# Set OpenAI API key via environment variable
export CHATX_OPENAI_API_KEY=sk-XXXX

# Or on Windows
set CHATX_OPENAI_API_KEY=sk-XXXX
```