---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Profiles

CHATX profiles allow you to save collections of settings as a single unit and easily switch between them. This is especially useful when working with different AI providers, models, or use cases.

## Understanding Profiles

Profiles are YAML files that store a combination of settings. They are stored in:

- Local: `.chatx/profiles/` in the current directory
- User: `.chatx/profiles/` in the user's home directory
- Global: `.chatx/profiles/` in the system-wide location

## Using Profiles

### Loading a Profile

To use a profile:

```bash title="Using a profile"
chatx --profile development --question "What is the capital of France?"
```

This loads all the settings from the `development` profile before executing the command. For detailed syntax and examples, see the [--profile option reference](../reference/cli/options/profile.md).

### Combining Profiles with Command-Line Options

You can combine profiles with additional command-line options:

```bash title="Profile with options"
chatx --profile gpt4 --add-system-prompt "Answer briefly" --question "What is the capital of France?"
```

Command-line options take precedence over profile settings when there are conflicts.

## Creating Profiles

### Using YAML Files

Profiles are YAML files stored in the profiles directory. Here's an example structure:

```yaml title="Example profile: gpt4.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4"
  apiKey: "${OPENAI_API_KEY}"  # Environment variable reference
```

Save this file as `.chatx/profiles/gpt4.yaml` to create a user-level profile.

### Creating Profile Files

Profiles must be created manually as YAML files and placed in the appropriate profiles directory:

- Local: `.chatx/profiles/profile_name.yaml` in the current directory
- User: `.chatx/profiles/profile_name.yaml` in the user's home directory
- Global: `.chatx/profiles/profile_name.yaml` in the system-wide location

### Profile Scopes

Like other CHATX features, profiles can be created in different scopes. Simply create the YAML file in the appropriate directory depending on the scope you need.

## Profile Search Order

When looking for a profile, CHATX searches in the following order:

1. Local scope (current directory)
2. User scope (user's home directory)
3. Global scope (system-wide)

This means that a local profile takes precedence over a user profile with the same name, which takes precedence over a global profile.

## Example Profiles

### Provider-Specific Profiles Examples

Here are examples of YAML content for different providers:

```yaml title="openai-profile.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4o"
```

```yaml title="azure-profile.yaml"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    chatDeployment: "gpt-4"
```

```yaml title="github-profile.yaml"
app:
  preferredProvider: "copilot"

copilot:
  modelName: "claude-3.7-sonnet"
```

### Model-Specific Profiles Examples

Creating profiles for different AI models:

```yaml title="gpt4.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4"
```

```yaml title="gpt35.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-3.5-turbo"
```

### Role-Based Profiles Examples

Creating profiles for different assistant roles:

```yaml title="programmer.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4"
  
prompts:
  system:
    - "You are an expert programmer. Provide clear, concise code with helpful explanations."
```

```yaml title="writer.yaml"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4"
  
prompts:
  system:
    - "You are a skilled writer. Help craft engaging, grammatically correct content."
```

## Advanced Profile Features

### Environment Variables in Profiles

Profiles can reference environment variables using `${VARIABLE_NAME}` syntax:

```yaml title="Profile with environment variables"
azure:
  openai:
    endpoint: "${AZURE_OPENAI_ENDPOINT}"
    apiKey: "${AZURE_OPENAI_API_KEY}"
    chatDeployment: "gpt-4"
```

This is useful for keeping sensitive information like API keys out of profile files.

### Combining Profiles

You can create a base profile with common settings and extend it in specialized profiles:

```yaml title="Base profile: base.yaml"
app:
  preferredProvider: "openai"
  autoSaveChatHistory: true
```

```yaml title="Extended profile: development.yaml"
# Include base settings
_extends: "base"

# Add development-specific settings
openai:
  chatModelName: "gpt-4"
  apiKey: "${OPENAI_API_KEY}"
```

## Best Practices

1. **Use descriptive names**: Choose profile names that clearly indicate their purpose
2. **Keep sensitive information in environment variables**: Don't hardcode API keys in profile files
3. **Use the right scope**: Create local profiles for project-specific settings and user profiles for personal preferences
4. **Document your profiles**: Keep notes on what each profile is designed for
5. **Create specialized profiles**: Create profiles for specific tasks or roles rather than one-size-fits-all profiles
6. **Regularly update profiles**: Review and update your profiles as your needs change or new models become available