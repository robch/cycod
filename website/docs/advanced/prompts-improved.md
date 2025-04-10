# Custom Prompts

Custom prompts are reusable text templates that can be quickly inserted into your chat conversations with CHATX. They help streamline common interactions and tasks.

## Understanding Custom Prompts

Custom prompts are different from aliases:

- **Aliases** save command-line options (like `--use-openai` or `--system-prompt`)
- **Custom prompts** save text templates that you can insert during chat conversations

Custom prompts are stored as text files in the following locations:

- Local: `.chatx/prompts/` in the current directory
- User: `.chatx/prompts/` in the user's home directory
- Global: `.chatx/prompts/` in the system-wide location

## Creating Custom Prompts

### Using the `prompt create` Command

You can create custom prompts using the `prompt create` command:

```bash title="Create a simple prompt"
chatx prompt create summarize "Please summarize the following text in three bullet points:"
```

### Creating Multi-line Prompts

For more complex prompts, you can include multiple lines:

```bash title="Create a multi-line prompt"
chatx prompt create review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability"
```

### Prompt Naming Requirements

When creating prompts, follow these naming guidelines:

- Names should be descriptive yet concise
- Names cannot contain spaces or special characters
- If a name starts with a slash (`/`), it will automatically be removed
- Each prompt name must be unique within its scope

### Prompt Scopes

Like aliases and configuration, prompts can be created in different scopes:

```bash title="Create a user-level prompt"
chatx prompt create translate "Translate the following text to Spanish:" --user
```

```bash title="Create a global prompt"
chatx prompt create explain "Explain this concept in simple terms as if I'm a beginner:" --global
```

### Adding Variables to Prompts

You can make your prompts more flexible by including variable placeholders:

```bash title="Create a prompt with variables"
chatx prompt create translate "Translate the following text to {language}:
{text}"
```

Variables are enclosed in curly braces (`{variable_name}`) and can be replaced with actual values when using the prompt.

## Using Custom Prompts

### In Interactive Chat Sessions

During an interactive chat session, you can use prompts by typing a forward slash (`/`) followed by the prompt name:

```plaintext title="Using a prompt in chat"
CHATX - AI-powered CLI, Version 1.0.0
Copyright(c) 2025, Rob Chambers. All rights reserved.

Type 'exit' or 'quit' to end the session. Press Ctrl+C to cancel the current request.

user@CHAT: /translate
Please translate the following text to Spanish:

user@CHAT: Hello, how are you today?

A: Hola, ¿cómo estás hoy?
```

### Prompt Auto-completion

In interactive sessions, CHATX offers tab-completion for prompts. Simply type `/` and press Tab to see a list of available prompts:

```plaintext
user@CHAT: /[TAB]
explain   review   summarize   translate
```

### Using Prompts with Variables

If your prompt contains variables, you'll need to provide values for them:

```plaintext title="Using a prompt with variables"
user@CHAT: /translate
Translate the following text to {language}:
{text}

user@CHAT: {language=French} {text=Hello, how are you today?}

A: Bonjour, comment allez-vous aujourd'hui?
```

## Managing Custom Prompts

### Listing Prompts

To list all available prompts:

```bash title="List all prompts"
chatx prompt list
```

This shows prompts from all scopes (equivalent to using `--any`). To list prompts from a specific scope:

```bash title="List user prompts"
chatx prompt list --user
```

### Viewing Prompt Content

To see the content of a specific prompt:

```bash title="View a prompt"
chatx prompt get translate
```

To view a prompt from a specific scope:

```bash title="View a user prompt"
chatx prompt get translate --user
```

### Deleting Prompts

To delete a prompt:

```bash title="Delete a prompt"
chatx prompt delete translate
```

To delete a prompt from a specific scope:

```bash title="Delete a user prompt"
chatx prompt delete translate --user
```

You can bypass the confirmation prompt with the `--yes` or `-y` flag:

```bash title="Delete without confirmation"
chatx prompt delete translate --yes
```

For detailed information about prompt deletion, including best practices and examples, see our [Deleting Custom Prompts](prompt-deletion.md) guide.

### Modifying Prompts

To modify an existing prompt, you need to delete it first and then create it again:

```bash title="Update a prompt"
# Delete the existing prompt
chatx prompt delete translate --user

# Create it again with the updated content
chatx prompt create translate "Translate the following text to {language} and provide pronunciation tips:" --user
```

## Example Prompts

Here are some useful prompts you can create for different scenarios:

### Code Review Prompt

```bash
chatx prompt create code-review "Please review the following code:
1. Identify potential bugs or edge cases
2. Suggest any performance improvements
3. Comment on readability and maintainability
4. Suggest any best practices that should be applied"
```

### Brainstorming Prompt

```bash
chatx prompt create brainstorm "Let's brainstorm ideas for {topic}.
Please generate 10 creative ideas. For each idea, provide:
- A concise title
- A brief description (1-2 sentences)
- One potential challenge or consideration"
```

### Meeting Notes Prompt

```bash
chatx prompt create meeting "Please organize these meeting notes into a structured format:
1. Meeting summary (2-3 sentences)
2. Key decisions made
3. Action items with owners (if specified)
4. Open questions or issues
5. Next steps"
```

### Language Learning Prompt

```bash
chatx prompt create learn-language "I'm learning {language}. For the following English phrase:
1. Provide the {language} translation
2. Break down the grammar structure
3. Offer pronunciation tips (using phonetic spelling)
4. Give an example of how to use it in a different context"
```

### Debug Helper Prompt

```bash
chatx prompt create debug "I'm getting the following error in my {language} code:
{error}

Here's the relevant code:
{code}

Please help me:
1. Understand what's causing the error
2. Fix the issue with the minimal necessary changes
3. Explain why the error occurred"
```

## Prompt Search Order

When looking for a prompt, CHATX searches in the following order:

1. Local scope (current directory)
2. User scope (user's home directory)
3. Global scope (system-wide)

This means that a local prompt takes precedence over a user prompt with the same name, which takes precedence over a global prompt.

## Best Practices

1. **Use descriptive names**: Choose names that clearly indicate what the prompt does
2. **Keep prompts focused**: Design each prompt for a specific task or purpose
3. **Structure complex prompts**: Use numbered lists or bullet points for multi-step prompts
4. **Choose the right scope**: Use local for project-specific prompts, user for personal prompts, and global for shared prompts
5. **Use variables wisely**: Include variables to make prompts flexible but don't overuse them
6. **Include instructions**: If your prompt requires specific input format, include instructions in the prompt
7. **Review and refine**: Periodically update your prompts based on how well they work in practice

## Practical Applications

### Creating Project-Specific Prompts

For software development projects, create local prompts tailored to your codebase:

```bash
chatx prompt create pr-review "Please review this pull request for our {project} project:
1. Check if it follows our coding standards
2. Identify any potential issues with the database schema changes
3. Suggest any optimizations for our specific architecture
4. Verify compatibility with our existing APIs"
```

### Creating Team Standards

For teams, create global prompts that enforce standards:

```bash
chatx prompt create company-docs "Please format this documentation according to company standards:
1. Use clear, concise language
2. Include examples for all API endpoints
3. Follow our terminology guidelines
4. Add a 'See Also' section with related documentation" --global
```

### Creating Personal Workflows

For personal productivity, create user prompts that fit your workflow:

```bash
chatx prompt create daily-summary "Please summarize my notes from today's meetings into:
1. Action items assigned to me
2. Key decisions that affect my projects
3. Important deadlines mentioned
4. Questions I need to follow up on" --user
```

## See Also

- [chatx prompt create](../reference/cli/prompt/create.md)
- [chatx prompt list](../reference/cli/prompt/list.md)
- [chatx prompt get](../reference/cli/prompt/get.md)
- [chatx prompt delete](../reference/cli/prompt/delete.md)
- [Aliases](../usage/aliases.md)
- [Slash Commands](../usage/slash-commands.md)