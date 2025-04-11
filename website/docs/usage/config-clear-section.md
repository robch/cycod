--8<-- "snippets/ai-generated.md"

### Clearing Configuration Settings

The `config clear` command removes a configuration setting entirely from a specified scope. Unlike setting a value to empty or null, clearing a setting removes the key completely from the configuration file.

#### Basic Syntax

```bash
chatx config clear <key> [--scope]
```

Where:
- `<key>` is the configuration setting name (using dot notation)
- `[--scope]` is optional and can be `--local` (default), `--user`, or `--global`

#### When to Use config clear

There are several scenarios where clearing a configuration setting is more appropriate than setting it to an empty value:

**1. Removing Sensitive Information**

When you need to remove API keys or other sensitive information:

```bash
# Remove an API key from user configuration
chatx config clear openai.apiKey --user

# Remove Azure credentials
chatx config clear azure.openai.apiKey --user
```

**2. Resetting to Default Behavior**

When you want to revert to the default behavior provided by the application:

```bash
# Reset to default model selection
chatx config clear openai.chatModelName --user

# Reset token management to default
chatx config clear app.trimTokenTarget --user
```

**3. Removing Project-Specific Overrides**

When you want to remove local settings that override user or global settings:

```bash
# Remove project-specific provider preference
chatx config clear app.preferredProvider

# Remove custom project deployment
chatx config clear azure.openai.chatDeployment
```

**4. Cleaning Configuration Before Sharing**

When preparing a project directory to be shared with others:

```bash
# Clean up any API keys or endpoints
chatx config clear azure.openai.endpoint
chatx config clear openai.apiKey
```

#### Scope Considerations

When clearing configuration settings, selecting the appropriate scope is important:

- **Local scope** (default): Affects only the current directory
- **User scope** (`--user`): Affects the current user across all directories
- **Global scope** (`--global`): Affects all users on the system

```bash
# Clear from local scope (default)
chatx config clear app.autoSaveChatHistory

# Clear from user scope
chatx config clear app.preferredProvider --user

# Clear from global scope (requires admin privileges)
chatx config clear app.historyDirectory --global
```

#### Troubleshooting

If you're experiencing issues after clearing a configuration:

**Setting Still Seems Active**

Check if the setting exists in another scope:

```bash
# Check if the setting exists in any scope
chatx config get app.preferredProvider --any
```

**Unexpected Default Behavior**

After clearing a setting, you might see a different default than expected. This often happens because:

1. There's another setting in a different scope (check with `config get KEY --any`)
2. The application has built-in defaults different from what you expected
3. Environment variables might be overriding configuration settings

To verify where a setting is coming from:

```bash
# List all settings to see where values are coming from
chatx config list
```

#### Example Workflow: Switching Configurations

A common use case is switching between different configurations:

```bash
# First clear existing settings
chatx config clear openai.chatModelName --user
chatx config clear app.preferredProvider --user

# Then apply new settings
chatx config set app.preferredProvider azure-openai --user
chatx config set azure.openai.chatDeployment custom-model --user
```

#### Best Practices

1. **Scope Appropriately**: Clear settings in the same scope where they were set
2. **Check After Clearing**: Use `config get` to verify the setting was cleared
3. **Security Considerations**: Always clear sensitive information like API keys when no longer needed
4. **Documentation**: When sharing project settings with a team, document any settings that team members will need to configure