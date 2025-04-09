# Aliases

CHATX aliases allow you to save and reuse sets of options for common commands. This feature helps you avoid typing the same long commands repeatedly.

## Creating Aliases

You can create aliases using the `--save-alias` option. The alias saves all the options and flags you used in the command.

### Basic Alias Creation

```bash title="Create a simple alias"
chatx --question "What time is it?" --save-alias time
```

Now you can use this alias:

```bash title="Use the alias"
chatx --time
```

### Advanced Alias Creation

You can create more complex aliases with multiple options:

```bash title="Create an advanced alias"
chatx --use-openai --openai-chat-model-name gpt-4 --add-system-prompt "You are a Python expert. Always provide code examples." --save-alias python-expert
```

Then use it:

```bash title="Use the advanced alias"
chatx --python-expert --question "How do I read a CSV file in Python?"
```

### Alias Scopes

CHATX supports three scopes for aliases:

1. **Local scope** (default): Available only in the current directory
2. **User scope**: Available to the current user in all directories
3. **Global scope**: Available to all users on the system

To create an alias in a specific scope:

```bash title="Create a user-level alias"
chatx --question "Tell me a joke" --save-user-alias joke
```

```bash title="Create a global alias"
chatx --question "What is the weather today?" --save-global-alias weather
```

```bash title="Create a local alias (explicit)"
chatx --question "What are the latest tech news?" --save-local-alias news
```

## Managing Aliases

### Listing Aliases

To list all available aliases:

```bash title="List all aliases"
chatx alias list
```

This shows aliases from all scopes. To list aliases from a specific scope:

```bash title="List user-level aliases"
chatx alias list --user
```

```bash title="List global aliases"
chatx alias list --global
```

```bash title="List local aliases"
chatx alias list --local
```

### Viewing Alias Details

To see the details of a specific alias:

```bash title="View alias details"
chatx alias get python-expert
```

This shows the command options that the alias contains. To check an alias in a specific scope:

```bash title="View user-level alias"
chatx alias get python-expert --user
```

### Deleting Aliases

To delete an alias:

```bash title="Delete an alias"
chatx alias delete python-expert
```

By default, this will search for the alias in all scopes and delete the first one found. To delete from a specific scope:

```bash title="Delete user-level alias"
chatx alias delete python-expert --user
```

## Using Aliases with Additional Options

You can combine aliases with additional command-line options:

```bash title="Combine alias with options"
chatx --python-expert --question "How do I sort a list in Python?"
```

The options given on the command line are merged with those in the alias, with command-line options taking precedence if there are conflicts.

## Alias Search Order

When looking for an alias, CHATX searches in the following order:

1. Local scope (current directory)
2. User scope (user's home directory)
3. Global scope (system-wide)

This means that a local alias takes precedence over a user alias with the same name, which takes precedence over a global alias.

## Example Use Cases

### Creating a Role-Based Alias

```bash title="Create a role-based alias"
chatx --add-system-prompt "You are a creative writer who excels at storytelling. Create vivid descriptions and engaging narratives." --save-user-alias writer
```

### Creating a Domain Expert Alias

```bash title="Create a domain expert alias"
chatx --add-system-prompt "You are a financial expert. Provide detailed, accurate information about investments, markets, and financial planning." --save-user-alias finance
```

### Creating a Programming Language-Specific Alias

```bash title="Create a language-specific alias"
chatx --add-system-prompt "You are a JavaScript expert. Always provide code examples using modern ES6+ syntax and explain best practices." --save-user-alias javascript
```

### Creating a Model-Specific Alias

```bash title="Create a model-specific alias"
chatx --use-openai --openai-chat-model-name gpt-4 --save-user-alias gpt4
```

## Best Practices

1. **Name clearly**: Use descriptive names that indicate the alias's purpose
2. **Choose the right scope**: Use local for project-specific aliases, user for personal preferences, and global for shared settings
3. **Document aliases**: Keep track of what each alias does, especially for complex ones
4. **Review regularly**: Periodically review and clean up aliases you no longer use
5. **Avoid sensitive information**: Don't include API keys or sensitive data in aliases, especially in shared environments