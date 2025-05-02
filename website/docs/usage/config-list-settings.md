---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Working with List Settings

Some CycoD configuration settings can store multiple values in a list. This guide explains how to work with these list-type settings using the `config add` and `config remove` commands.

## Understanding List Settings

While many configuration settings store a single value (like an API key or a boolean flag), list settings allow you to store multiple values. Common examples include:

- Lists of allowed domains
- Collections of trusted sources
- Permitted file extensions
- Feature flags
- Allowed models

## Adding Values to Lists

The `cycod config add` command lets you add new items to a list setting:

```bash
cycod config add <key> <value> [--scope]
```

When you add a value:
- If the list doesn't exist yet, it will be created with the single value
- If the list exists but doesn't contain the value, the value will be added
- If the list exists and already contains the value, no change is made

### Basic Examples

```bash
# Add a trusted domain to a list
cycod config add app.trustedDomains example.com

# Add an allowed file extension
cycod config add app.allowedFileExtensions .md --user

# Add an allowed model to the global scope
cycod config add app.allowedModels gpt-4o --global
```

## Removing Values from Lists

To remove a value from a list setting, use the `cycod config remove` command:

```bash
cycod config remove <key> <value> [--scope]
```

This will:
- Remove the specified value from the list if it exists
- Leave other values in the list unchanged
- Do nothing if the value isn't in the list

### Basic Examples

```bash
# Remove a domain from trusted domains
cycod config remove app.trustedDomains example.com

# Remove a file extension from allowed types
cycod config remove app.allowedFileExtensions .exe --user

# Remove a model from allowed models
cycod config remove app.allowedModels gpt-3.5-turbo --global
```

## Viewing List Settings

To view the contents of a list setting, use the `cycod config get` command:

```bash
cycod config get app.trustedDomains
```

This will display the setting with all current values:

```
app.trustedDomains: [example.com, api.openai.com, github.com]
```

## Practical Use Cases

### Managing Allowed Resources

Control which external resources CycoD can access:

```bash
# Add trusted domains
cycod config add app.trustedDomains api.openai.com --user
cycod config add app.trustedDomains github.com --user
cycod config add app.trustedDomains huggingface.co --user

# Remove a domain if needed
cycod config remove app.trustedDomains oldapi.example.com --user

# View your trusted domains
cycod config get app.trustedDomains --user
```

### Setting Up Allowed File Types

Specify which file types CycoD should process:

```bash
# Allow markdown files
cycod config add app.allowedFileTypes .md --user

# Allow code files
cycod config add app.allowedFileTypes .py --user
cycod config add app.allowedFileTypes .js --user
cycod config add app.allowedFileTypes .cs --user

# Remove a file type if necessary
cycod config remove app.allowedFileTypes .exe --user
```

### Managing Feature Flags

Enable or disable experimental features:

```bash
# Enable experimental features
cycod config add app.enabledFeatures experimental-search --user
cycod config add app.enabledFeatures beta-ui --user

# Disable features when needed
cycod config remove app.enabledFeatures experimental-search --user
```

### Team Configuration

Set up team-wide permissions (requires admin privileges):

```bash
# Set up allowed providers for the whole team
cycod config add app.allowedProviders openai --global
cycod config add app.allowedProviders azure-openai --global

# Set up allowed models
cycod config add app.allowedModels gpt-4o --global
cycod config add app.allowedModels claude-3-sonnet --global
```

## Best Practices

1. **Choose the Right Scope**
   - Use `--user` for personal preferences that apply across projects
   - Use local scope (default) for project-specific lists
   - Use `--global` only for system-wide policies

2. **Security Considerations**
   - Be cautious about adding items to security-related lists like `trustedDomains`
   - Regularly review your list settings to ensure they remain appropriate
   - For security lists, prefer being restrictive rather than permissive

3. **Maintenance Tips**
   - Periodically clean up unused or outdated list items
   - Document why certain items are added to lists (especially for team settings)
   - Use consistent naming patterns for similar items

## Common List Settings

| Setting Name | Description | Typical Use |
|--------------|-------------|-------------|
| `app.trustedDomains` | Domains allowed for external connections | External APIs and services |
| `app.allowedFileTypes` | File extensions CycoD can process | Controlling which files are processed |
| `app.allowedModels` | AI models permitted for use | Access control for AI models |
| `app.enabledFeatures` | Experimental features to enable | Beta testing new capabilities |
| `app.disabledFeatures` | Features to disable | Turning off unwanted features |

## Working with Complex List Values

For more complex scenarios, you might need to add structured values to lists. In such cases, you may need to use JSON formatting:

```bash
# Adding a complex value
cycod config add app.complexSetting '{"name":"feature1","priority":5}' --user
```

Check the specific setting's documentation to understand the required format.

## Related Commands

- `cycod config set` - For single-value settings
- `cycod config get` - For viewing settings
- `cycod config list` - For listing all settings
- `cycod config clear` - For removing settings entirely