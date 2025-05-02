---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# External Configuration Files

CYCOD provides a powerful way to manage settings through external configuration files using the `--config` option. This approach offers flexibility for different projects, teams, and environments.

## Using the --config Option

The `--config` option allows you to load settings from one or more YAML or INI files:

```bash
cycod --config my-settings.yaml --question "What is the capital of France?"
```

This loads all settings from `my-settings.yaml` before executing the command.

## Why Use External Configuration Files?

External configuration files offer several advantages:

- **Portability**: Easily share configurations across machines and team members
- **Version Control**: Track configuration changes alongside your project
- **Separation of Concerns**: Keep sensitive settings (like API keys) separate from other configurations
- **Environment Management**: Switch between different environments (dev, test, prod) with ease
- **Project-Specific Settings**: Maintain different settings for different projects
- **Complex Configurations**: Store complex settings that would be cumbersome on the command line

## Supported Formats

CYCOD supports both YAML and INI formats for external configuration files.

### YAML Format

YAML provides a clean, hierarchical structure that's easy to read and maintain:

```yaml
# Example YAML configuration file
app:
  preferredProvider: openai
  autoSaveChatHistory: true
  quietMode: false

openai:
  chatModelName: gpt-4o
  maxTokens: 2048

azure:
  openai:
    endpoint: https://your-resource.openai.azure.com
    chatDeployment: gpt-4
```

### INI Format

INI format offers a simpler, more traditional configuration style:

```ini
; Example INI configuration file
[app]
preferredProvider = openai
autoSaveChatHistory = true
quietMode = false

[openai]
chatModelName = gpt-4o
maxTokens = 2048

[azure.openai]
endpoint = https://your-resource.openai.azure.com
chatDeployment = gpt-4
```

## Basic Usage Patterns

### Single Configuration File

Load settings from a single configuration file:

```bash
cycod --config project-config.yaml --question "Explain Docker containers"
```

### Multiple Configuration Files

Load settings from multiple files, with later files overriding earlier ones:

```bash
cycod --config base.yaml project.yaml --question "What's new in Python 3.11?"
```

This is useful for creating a layered approach to configuration:

1. `base.yaml` contains common settings shared across projects
2. `project.yaml` contains project-specific overrides

### Mixing with Command-Line Options

Settings from configuration files can be overridden by options specified on the command line:

```bash
cycod --config team-settings.yaml --use-azure-openai --question "What is the capital of France?"
```

In this example, even if `team-settings.yaml` sets `openai` as the preferred provider, the `--use-azure-openai` option will override it.

## Advanced Configuration Strategies

### Separating Sensitive Information

Keep API keys and other sensitive information in a separate file that isn't checked into version control:

```bash
# Base configuration (checked into version control)
cycod --config base-config.yaml secrets.yaml --question "Generate a project plan"
```

Example `base-config.yaml`:
```yaml
app:
  preferredProvider: azure-openai
  autoSaveChatHistory: true

azure:
  openai:
    chatDeployment: gpt-4
```

Example `secrets.yaml` (not in version control):
```yaml
azure:
  openai:
    endpoint: https://your-resource.openai.azure.com
    apiKey: your-api-key
```

### Environment-Specific Configurations

Create different configuration files for different environments:

```bash
# Development environment
cycod --config base.yaml dev.yaml --question "Debug this code"

# Production environment
cycod --config base.yaml prod.yaml --question "Analyze production logs"
```

### Team Settings with Personal Preferences

In team environments, combine shared team settings with personal preferences:

```bash
cycod --config team-settings.yaml personal-settings.yaml --question "Review this PR"
```

### Feature-Specific Configurations

Create configuration files for specific features or tasks:

```bash
# For code generation tasks
cycod --config base.yaml code-gen.yaml --question "Write a Python class for user authentication"

# For content creation tasks
cycod --config base.yaml content.yaml --question "Write a blog post about AI"
```

## Practical Examples

### Configuring Different AI Providers

#### OpenAI Configuration (openai.yaml)

```yaml
app:
  preferredProvider: openai

openai:
  chatModelName: gpt-4o
  maxTokens: 2048
```

Usage:
```bash
cycod --config openai.yaml --question "Explain quantum computing"
```

#### Azure OpenAI Configuration (azure.yaml)

```yaml
app:
  preferredProvider: azure-openai

azure:
  openai:
    endpoint: https://your-resource.openai.azure.com
    chatDeployment: gpt-4
```

Usage:
```bash
cycod --config azure.yaml --question "Explain quantum computing"
```

### Project-Specific Configuration

```yaml
# project.yaml
app:
  historyDirectory: "./project-history"
  autoSaveChatHistory: true
  
openai:
  chatModelName: gpt-4-turbo
  
azure:
  openai:
    chatDeployment: project-deployment
```

Usage:
```bash
cycod --config project.yaml --question "Explain the project architecture"
```

### Debug Configuration

```yaml
# debug.yaml
app:
  debugMode: true
  quietMode: false
  verboseOutput: true
```

Usage:
```bash
cycod --config normal.yaml debug.yaml --question "Why is this failing?"
```

## Managing Configuration Files

### File Organization

Consider organizing your configuration files in a structured way:

```
project/
├── .cycod/
│   ├── configs/
│   │   ├── base.yaml
│   │   ├── dev.yaml
│   │   ├── prod.yaml
│   │   └── secrets.yaml (not in version control)
│   └── ...
├── ...
```

### Sample Template Files

Provide template files for team members without including sensitive information:

```yaml
# template-config.yaml
app:
  preferredProvider: openai
  autoSaveChatHistory: true

openai:
  # Add your API key here
  apiKey: YOUR_API_KEY_HERE
  chatModelName: gpt-4o
```

### Configuration File Documentation

Add comments to your configuration files to explain settings:

```yaml
# Production Configuration
app:
  # Set preferred provider for production
  preferredProvider: azure-openai
  
  # Always save chat history in production for audit purposes
  autoSaveChatHistory: true
  
  # Use corporate history directory
  historyDirectory: "//server/cycod-logs"
```

## Configuration Precedence

When using the `--config` option, it's important to understand how it fits into the overall configuration precedence:

1. Command-line options (highest priority)
2. Environment variables
3. Settings from `--config` files (later files override earlier ones)
4. Local scope configuration (`.cycod/config.json` in current directory)
5. User scope configuration (`.cycod/config.json` in user's home directory)
6. Global scope configuration (`.cycod/config.json` in system-wide location)

This means you can use `--config` files as a middle ground between permanent configuration and command-line overrides.

## When to Use --config vs. Profiles

CYCOD supports both external configuration files (`--config`) and profiles (`--profile`). Here's when to use each:

**Use `--config` when:**
- You want to mix and match configuration files flexibly
- You need to share configurations across machines or team members
- You want to organize settings by environment or feature
- You need to override some settings temporarily

**Use `--profile` when:**
- You want a named, reusable configuration
- You frequently switch between the same configuration sets
- You want to create shortcuts for common configurations

You can even combine them:

```bash
# Load a profile plus additional settings
cycod --profile work --config project-specific.yaml --question "Explain microservices"
```

## Best Practices

1. **Version Control**: Include non-sensitive configuration files in version control
2. **Sensitive Data**: Keep API keys and secrets in separate files excluded from version control
3. **Documentation**: Add comments to your configuration files explaining each setting
4. **Layered Approach**: Use multiple configuration files in a layered approach
5. **Templates**: Provide template configuration files for team members
6. **Naming Conventions**: Use consistent naming conventions for your configuration files
7. **Configuration Testing**: Test your configurations in isolation before using them in production
8. **Environment Variables**: For the most sensitive data, consider using environment variables instead of configuration files

## Troubleshooting

### Common Issues

**Configuration Not Applied**

If settings from your configuration file aren't being applied:

1. Check that the file path is correct
2. Ensure the file format (YAML or INI) is valid
3. Verify the configuration hierarchy matches CYCOD's expected structure
4. Remember that command-line options override configuration files

**YAML Parsing Errors**

YAML is sensitive to indentation. If you get parsing errors:

1. Check for consistent indentation (use spaces, not tabs)
2. Ensure there are no special characters that need to be escaped
3. Verify that all values have the correct format (strings, numbers, booleans)

**INI Parsing Errors**

For INI format issues:

1. Check that section headers are properly formatted with brackets `[section]`
2. Ensure all values have the correct format
3. Verify the hierarchical sections use proper dot notation `[section.subsection]`

## See Also

- [CLI Reference: --config option](../reference/cli/options/config.md)
- [CLI Reference: --profile option](../reference/cli/options/profile.md)
- [Configuration Overview](./configuration.md)
- [Configuration Profiles](../advanced/profiles.md)