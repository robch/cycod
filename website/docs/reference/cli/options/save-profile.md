# --save-profile

The `--save-profile` option allows you to save the current command-line options as a named profile for future use. This makes it easy to reuse complex configurations without having to remember and retype all the options each time.

## Syntax

```
--save-profile PROFILE_NAME [--scope]
```

## Arguments

- **PROFILE_NAME**: The name to give the new profile. This will create a YAML file named `PROFILE_NAME.yaml` in the appropriate profiles directory.

## Options

- **--local** (default): Save the profile in the local scope (`.chatx/profiles/` in the current directory)
- **--user**: Save the profile in the user scope (`~/.chatx/profiles/` in the user's home directory)
- **--global**: Save the profile in the global scope (system-wide profiles directory)

## Description

The `--save-profile` option captures all the command-line options used in the current command and saves them to a YAML file in the appropriate profiles directory. This profile can then be loaded in future commands using the [`--profile`](profile.md) option.

The profile is saved in one of three locations depending on the specified scope:

1. **Local scope** (default): `.chatx/profiles/` in the current directory
2. **User scope**: `~/.chatx/profiles/` in the user's home directory
3. **Global scope**: System-wide profiles directory

The created profile file contains all the settings from the command line, organized in the appropriate configuration structure.

## Examples

### Basic Usage

```bash title="Save a profile with basic settings"
chatx --use-openai --openai-chat-model-name gpt-4-turbo --save-profile gpt4
```

This saves a profile named `gpt4` that sets OpenAI as the provider and uses the GPT-4 Turbo model.

### Saving with System Prompts

```bash title="Save a profile with system prompt"
chatx --use-openai --add-system-prompt "You are an expert programmer. Provide clear, concise code with explanations." --save-profile programmer
```

This saves a profile that includes both the OpenAI provider setting and a specific system prompt for programming assistance.

### Saving to User Scope

```bash title="Save a profile to user scope"
chatx --use-azure-openai --azure-openai-chat-deployment gpt-4 --save-profile work --user
```

This saves a profile in the user's home directory, making it available across all projects for that user.

### Saving to Global Scope

```bash title="Save a profile to global scope"
chatx --use-copilot --copilot-model-name claude-3.7-sonnet --save-profile company-standard --global
```

This saves a profile in the global scope, making it available to all users on the system.

## Generated Profile Example

When you use `--save-profile gpt4` with the options from the first example, it creates a file like this:

```yaml title="gpt4.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4-turbo"
```

## Notes

- Any command-line options used alongside `--save-profile` will be included in the saved profile
- The profile is saved as a YAML file with the structure matching CHATX's configuration hierarchy
- If a profile with the same name already exists in the specified scope, it will be overwritten
- The `--save-profile` option is usually the last option on the command line

## See Also

- [--profile](profile.md) - Load a profile
- [Profiles](../../../advanced/profiles.md) - Detailed guide on creating and managing profiles
- [Configuration](../../../usage/configuration.md) - General configuration options
- [--save-local-profile](save-local-profile.md) - Alias for `--save-profile --local`
- [--save-user-profile](save-user-profile.md) - Alias for `--save-profile --user`
- [--save-global-profile](save-global-profile.md) - Alias for `--save-profile --global`