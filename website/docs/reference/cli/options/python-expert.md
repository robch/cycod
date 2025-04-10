# --python-expert Option

The `--python-expert` option is an alias that configures ChatX to act as a Python programming expert, providing specialized assistance with Python-related tasks, code explanations, and best practices.

## Syntax

```bash
chatx --python-expert [additional options]
```

## Description

When you use the `--python-expert` alias, ChatX will:

1. Apply predefined system prompts that instruct the AI to act as a Python expert
2. Potentially use a model configuration optimized for code-related tasks
3. Provide Python-focused responses with appropriate code examples and explanations

This alias is designed to help users get high-quality Python programming assistance without having to repeatedly specify detailed instructions about how the AI should approach Python questions.

## Examples

### Basic Usage

```bash
# Ask a Python question using the python-expert alias
chatx --python-expert --question "How do I read JSON files in Python?"
```

### Interactive Mode

```bash
# Start an interactive Python-focused chat session
chatx --python-expert
> How do I implement a binary search in Python?
> What's the difference between a list and a tuple?
> How can I efficiently process large CSV files?
```

### With File Reference

```bash
# Have the Python expert analyze a Python file
chatx --python-expert --question "Please review this code and suggest improvements" my_script.py
```

### Additional Options

```bash
# Combine with a specific model request
chatx --python-expert --use-openai --openai-chat-model-name gpt-4o --question "Explain Python decorators"
```

## Creating Your Own Python Expert Alias

If the built-in `--python-expert` alias doesn't exist or you want to customize it, you can create your own:

```bash
chatx --add-system-prompt "You are a Python expert with deep knowledge of the language, standard library, and ecosystem. Always provide practical code examples that follow PEP 8 style guidelines. Explain concepts clearly as if teaching an intermediate programmer." --save-alias python-expert
```

## Related Options

- [`--ALIAS`](/reference/cli/options/alias.md) - How to use saved aliases
- [`--save-alias`](/reference/cli/options/save-alias.md) - How to save aliases
- [`--add-system-prompt`](/reference/cli/options/add-system-prompt.md) - Add text to the system prompt

## See Also

- [Managing aliases](/advanced/aliases.md)
- [Creating expert roles](/tutorials/expert-roles.md) (if available)