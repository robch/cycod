--8<-- "snippets/ai-generated.md"

# Tutorial: Creating User Aliases for Personal Productivity

This tutorial shows how to create and use user aliases in ChatX to boost your personal productivity across all your projects.

## What are User Aliases?

User aliases are command shortcuts saved at the user scope, meaning they're available to you in any directory on your system. They're perfect for creating personal AI assistants tailored to your specific needs.

## Why Use User Aliases?

User aliases offer several benefits:

- **Consistency**: Access the same AI configuration across all your projects
- **Efficiency**: Save time by avoiding typing the same command options repeatedly
- **Personalization**: Create assistants tailored to your specific needs and preferences
- **Portability**: Your aliases follow you across projects, unlike local aliases

## Creating Your First User Alias

Let's create a user alias for a programming assistant:

```bash
chatx --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "You are an expert programmer who writes concise, efficient, and well-documented code." --save-user-alias code-helper
```

This creates a user alias called `code-helper` that's available in any directory.

## Using Your User Alias

Now you can use this alias from any directory on your system:

```bash
chatx --code-helper --question "How would I implement a binary search in Python?"
```

## Creating Multiple Specialized Assistants

User aliases are particularly useful for creating multiple specialized assistants:

### JavaScript Expert

```bash
chatx --add-system-prompt "You are a JavaScript expert who specializes in modern ES6+ features, React, and Node.js best practices." --save-user-alias js-expert
```

### SQL Helper

```bash
chatx --add-system-prompt "You are a SQL expert who writes efficient, optimized database queries. When providing SQL code, always explain performance considerations." --save-user-alias sql-helper
```

### Technical Writer

```bash
chatx --add-system-prompt "You are a technical documentation expert who creates clear, concise, and well-structured documentation for software projects." --save-user-alias tech-writer
```

## Managing Your Aliases

To see all your user aliases:

```bash
chatx alias list --user
```

To view what a specific alias does:

```bash
chatx alias get js-expert --user
```

To delete an alias you no longer need:

```bash
chatx alias delete sql-helper --user
```

This command removes the alias from your user scope. See the [chatx alias delete reference](/reference/cli/alias/delete.md) for more details on deleting aliases.

## Advanced User Alias Example: Daily Standup Helper

Here's a more complex example that creates a user alias for helping with daily standup reports:

```bash
chatx --add-system-prompt "You help software engineers prepare for daily standup meetings. When I share my accomplishments from yesterday and my plans for today, help me articulate them clearly and concisely in a professional format. Also suggest any blockers I should mention based on my work." --save-user-alias standup-helper
```

Now you can quickly prepare for standups:

```bash
chatx --standup-helper --question "Yesterday I fixed the authentication bug and started working on the new reporting feature. Today I plan to finish the reporting feature and start on the export functionality. I'm a bit stuck on getting the export to handle special characters."
```

## Best Practices for User Aliases

1. **Use descriptive names** that remind you of the alias's purpose
2. **Create focused aliases** for specific tasks rather than general-purpose ones
3. **Update aliases periodically** as your needs evolve
4. **Document complex aliases** by saving their descriptions elsewhere

## Where to Go From Here

- Try creating user aliases for other repetitive tasks you perform
- Experiment with different system prompts to refine your assistants
- Consider creating [local aliases](/reference/cli/options/save-local-alias.md) for project-specific needs

## Related Resources

- [Using Aliases in ChatX](/usage/aliases.md)
- [Understanding Scopes](/usage/scopes.md)
- [chatx alias delete Reference](/reference/cli/alias/delete.md)
- [chatx alias get Reference](/reference/cli/alias/get.md)
- [chatx alias list Reference](/reference/cli/alias/list.md)
- [--save-user-alias Reference](/reference/cli/options/save-user-alias.md)