---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Custom Prompts

Custom prompts are reusable text templates that can be quickly inserted into your chat conversations with CYCOD. They help streamline common interactions and tasks.

## Understanding Custom Prompts

Custom prompts are different from aliases:

- **Aliases** save command-line options (like `--use-openai` or `--system-prompt`)
- **Custom prompts** save text templates that you can insert during chat conversations

Custom prompts are stored as text files in the following locations:

- Local: `.cycod/prompts/` in the current directory
- User: `.cycod/prompts/` in the user's home directory
- Global: `.cycod/prompts/` in the system-wide location

## Creating Custom Prompts

### Using the `prompt create` Command

You can create custom prompts using the `prompt create` command:

```bash title="Create a simple prompt"
cycod prompt create summarize "Please summarize the following text in three bullet points:"
```

### Creating Multi-line Prompts

For more complex prompts, you can include multiple lines:

```bash title="Create a multi-line prompt"
cycod prompt create review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability"
```

### Prompt Scopes

Like aliases and configuration, prompts can be created in different scopes:

```bash title="Create a user-level prompt"
cycod prompt create translate "Translate the following text to Spanish:" --user
```

```bash title="Create a global prompt"
cycod prompt create explain "Explain this concept in simple terms as if I'm a beginner:" --global
```

## Using Custom Prompts

### In Interactive Chat Sessions

During an interactive chat session, you can use prompts by typing a forward slash (`/`) followed by the prompt name:

```plaintext title="Using a prompt in chat"
CYCOD - AI-powered CLI, Version 1.0.0
Copyright(c) 2025, Rob Chambers. All rights reserved.

Type 'exit' or 'quit' to end the session. Press Ctrl+C to cancel the current request.

user@CHAT: /translate
Please translate the following text to Spanish:

user@CHAT: Hello, how are you today?

A: Hola, ¿cómo estás hoy?
```

### Prompt Auto-completion

In interactive sessions, CYCOD offers tab-completion for prompts. Simply type `/` and press Tab to see a list of available prompts:

```plaintext
user@CHAT: /[TAB]
explain   review   summarize   translate
```

## Managing Custom Prompts

### Listing Prompts

To list all available prompts:

```bash title="List all prompts"
cycod prompt list
```

This shows prompts from all scopes (equivalent to using `--any`). To list prompts from a specific scope:

```bash title="List user prompts"
cycod prompt list --user
```

### Viewing Prompt Content

To see the content of a specific prompt:

```bash title="View a prompt"
cycod prompt get translate
```

To view a prompt from a specific scope:

```bash title="View a user prompt"
cycod prompt get translate --user
```

### Deleting Prompts

To delete a prompt:

```bash title="Delete a prompt"
cycod prompt delete translate
```

To delete a prompt from a specific scope:

```bash title="Delete a user prompt"
cycod prompt delete translate --user
```

## Example Prompts

### Code Review Prompt

```bash
cycod prompt create code-review "Please review the following code:
1. Identify potential bugs or edge cases
2. Suggest any performance improvements
3. Comment on readability and maintainability
4. Suggest any best practices that should be applied"
```

### Brainstorming Prompt

```bash
cycod prompt create brainstorm "Let's brainstorm ideas for:
Please generate 10 creative ideas. For each idea, provide:
- A concise title
- A brief description (1-2 sentences)
- One potential challenge or consideration"
```

### Meeting Notes Prompt

```bash
cycod prompt create meeting "Please organize these meeting notes into a structured format:
1. Meeting summary (2-3 sentences)
2. Key decisions made
3. Action items with owners (if specified)
4. Open questions or issues
5. Next steps"
```

### Language Learning Prompt

```bash
cycod prompt create learn-spanish "I'm learning Spanish. For the following English phrase:
1. Provide the Spanish translation
2. Break down the grammar structure
3. Offer pronunciation tips (using phonetic spelling)
4. Give an example of how to use it in a different context"
```

## Prompt Search Order

When looking for a prompt, CYCOD searches in the following order:

1. Local scope (current directory)
2. User scope (user's home directory)
3. Global scope (system-wide)

This means that a local prompt takes precedence over a user prompt with the same name, which takes precedence over a global prompt.

## Best Practices

1. **Use descriptive names**: Choose names that clearly indicate what the prompt does
2. **Keep prompts focused**: Design each prompt for a specific task or purpose
3. **Structure complex prompts**: Use numbered lists or bullet points for multi-step prompts
4. **Choose the right scope**: Use local for project-specific prompts, user for personal prompts, and global for shared prompts
5. **Review and refine**: Periodically update your prompts based on how well they work in practice