---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Using Expert Roles in ChatX

ChatX allows you to define and use expert roles to get specialized assistance for specific domains or tasks. This tutorial focuses on using predefined expert roles and creating your own custom roles.

## What Are Expert Roles?

Expert roles in ChatX are aliases that configure the AI assistant to specialize in a particular domain, skill set, or knowledge area. They typically:

1. Set specific system prompts that define the AI's role and expertise
2. May specify optimal model configurations for the task
3. Sometimes include additional context or instructions

## Using the Python Expert Role

ChatX comes with several predefined expert roles, including the Python expert role. This role configures the AI to provide specialized Python programming assistance.

### Basic Python Expert Usage

To use the Python expert role, simply add the `--python-expert` option to your ChatX command:

```bash
chatx --python-expert --question "How do I handle exceptions in Python?"
```

This will activate the Python expert mode, instructing the AI to respond as a Python specialist with appropriate code examples and best practices.

### Interactive Python Expert Session

For ongoing Python assistance, start an interactive session with the Python expert:

```bash
chatx --python-expert
```

This opens an interactive chat where you can ask multiple Python-related questions:

```
> How do generators work in Python?
[AI responds with explanation of Python generators]

> Can you show me an example of a context manager?
[AI responds with context manager examples]

> What's the difference between __str__ and __repr__ methods?
[AI responds with explanation of the difference]
```

### Learning with the Python Expert

The Python expert role is particularly useful for learning Python concepts:

```bash
chatx --python-expert --question "Explain Python's list comprehensions with 5 progressively complex examples"
```

### Debugging with the Python Expert

You can use the Python expert to help with debugging by sharing code:

```bash
chatx --python-expert --question "What's wrong with this code?" buggy_script.py
```

## Other Built-in Expert Roles

ChatX includes several other expert roles that you can use:

- `--javascript-expert` - For JavaScript programming assistance
- `--sql-expert` - For database and SQL query help
- `--linux-expert` - For Linux system administration guidance
- `--writing-expert` - For writing assistance and content creation

## Creating Your Own Expert Role

You can easily create custom expert roles using the alias system in ChatX.

### Basic Custom Expert Creation

To create a custom expert role, use the `--add-system-prompt` option with `--save-alias`:

```bash
chatx --add-system-prompt "You are a data science expert with deep knowledge of statistics, machine learning, pandas, scikit-learn, and visualization libraries. Always include code examples with explanations of the statistical concepts involved. Suggest best practices for data analysis workflows." --save-alias data-science-expert
```

Now you can use your new expert:

```bash
chatx --data-science-expert --question "How do I perform principal component analysis in Python?"
```

### Advanced Expert Role Creation

For more advanced expert roles, you can combine multiple options:

```bash
chatx --use-openai --openai-chat-model-name gpt-4 --add-system-prompt "You are a cybersecurity expert specializing in penetration testing, security audits, and secure coding practices. Always emphasize security best practices and provide detailed explanations of vulnerabilities and their remediation." --add-user-prompt "Please analyze security issues thoroughly and provide actionable recommendations." --save-user-alias security-expert
```

This creates a user-level alias that:
- Uses a specific AI model (GPT-4)
- Sets a detailed system prompt defining the expert role
- Adds an initial user prompt to guide responses
- Saves at the user level so it's available in all directories

### Expert Role with Model Optimization

Some tasks benefit from specific model settings:

```bash
chatx --use-openai --openai-chat-model-name gpt-4o --max-tokens 4000 --add-system-prompt "You are a creative writing expert. You excel at crafting engaging narratives, developing interesting characters, and creating vivid descriptions. Provide thoughtful feedback on writing samples and suggest improvements for style, structure, and clarity." --save-alias writing-coach
```

## Best Practices for Expert Roles

1. **Be specific about the expertise**: Clearly define the domain knowledge and expectations
2. **Include output preferences**: Specify how you want information presented (code examples, diagrams, step-by-step guides)
3. **Consider the appropriate scope**: Save personal expert roles at the user level, project-specific ones at the local level
4. **Document your expert roles**: Keep notes about what each expert role does and when to use it
5. **Refine over time**: Update your expert roles as you learn what instructions work best

## Combining Expert Roles with Other Features

Expert roles work well with other ChatX features:

### With Chat History

```bash
# Continue a previous Python expert session
chatx --python-expert --continue
```

### With File Input

```bash
# Have the Python expert analyze multiple files
chatx --python-expert --question "Review these scripts and explain how they work together" script1.py script2.py config.json
```

### With Templates and Variables

```bash
# Use variables with an expert role
chatx --python-expert --var version=3.9 --question "What are the new features in Python {version}?"
```

## Example: Creating a Python Expert Role

If the built-in `--python-expert` role doesn't exist or you want to create your own custom version:

```bash
chatx --add-system-prompt "You are a Python expert with deep knowledge of Python programming language, libraries, frameworks, and best practices. You have experience with Python 2.x through Python 3.x, and are familiar with popular libraries such as NumPy, Pandas, Django, Flask, PyTorch, TensorFlow, and more.

When responding to questions:
1. Always provide runnable code examples when appropriate
2. Follow PEP 8 style guidelines in all code
3. Explain key concepts clearly
4. Point out common pitfalls or edge cases
5. Suggest best practices and optimizations
6. When relevant, mention version differences (e.g., Python 2 vs 3)

If a question is ambiguous, ask for clarification rather than making assumptions." --save-user-alias python-expert
```

## Conclusion

Expert roles are a powerful way to get specialized assistance without having to repeatedly specify detailed instructions. By creating custom expert roles for your common tasks, you can significantly streamline your workflow and get more consistent, high-quality responses from ChatX.

Whether you're using the built-in `--python-expert` option or creating your own custom experts, this approach helps you get the most out of AI assistance for your specific needs.

## See Also

- [Managing aliases](/advanced/aliases.md)
- [Using system prompts](/usage/input-options.md)
- [Creating profiles](/advanced/profiles.md)