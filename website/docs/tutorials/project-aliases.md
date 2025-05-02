---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Project-Specific Aliases with CycoD

This tutorial guides you through creating and using project-specific aliases with CycoD, with a special focus on the `--save-local-alias` option.

## Introduction

When working on a project with CycoD, you might want to create custom AI assistants that:

- Understand your project's specific domain
- Are familiar with your codebase
- Follow your project's coding standards
- Provide responses tailored to your team's needs

Project-specific aliases, created using `--save-local-alias`, are perfect for this purpose because they:

- Are stored in the project directory (`.cycod/aliases/`)
- Can be shared with your team through version control
- Only apply when working within that project
- Create consistent AI interaction experiences across your team

## Setting Up Your Project Directory

First, let's set up a project directory with some code files:

```bash
mkdir my-project
cd my-project
```

Create a simple Python file to work with:

```bash
echo 'def hello_world():
    print("Hello, World!")

if __name__ == "__main__":
    hello_world()' > app.py
```

## Creating Your First Project Alias

Let's create a Python expert alias that's specific to this project:

```bash
cycod --add-system-prompt "You are a Python expert familiar with this project. The project follows PEP 8 standards and uses pytest for testing." --save-local-alias py-expert
```

You should see output similar to:

```
Alias 'py-expert' created at: /path/to/my-project/.cycod/aliases/py-expert.alias
```

Let's verify that the alias was created:

```bash
cycod alias list --local
```

This should display your newly created alias.

## Using Your Project Alias

Now you can use this alias in your project:

```bash
cycod --py-expert --question "How should I expand the hello_world function to accept a name parameter?"
```

The AI will respond with advice tailored to your project's standards.

## Creating Multiple Project Aliases

For a real-world project, you might want several different aliases for different tasks:

### Create a Code Review Alias

```bash
cycod --add-system-prompt "You are a code reviewer for this Python project. Focus on PEP 8 compliance, security best practices, and test coverage. Be thorough but constructive in your feedback." --save-local-alias code-review
```

### Create a Documentation Assistant

```bash
cycod --add-system-prompt "You specialize in writing clear Python documentation. Generate docstrings that follow Google's Python Style Guide. For each function, explain parameters, return values, and include examples." --save-local-alias doc-helper
```

### Create a Testing Specialist

```bash
cycod --add-system-prompt "You are a testing expert for Python using pytest. Suggest comprehensive test cases that achieve high coverage. Consider edge cases and error conditions." --save-local-alias test-helper
```

## Sharing Project Aliases with Your Team

One of the main advantages of local aliases is that they can be shared with your team through version control.

### Including Aliases in Version Control

Ensure your `.cycod/aliases/` directory is included in your version control system:

For Git, check that `.cycod/aliases/` is not in your `.gitignore` file.

Then commit your aliases:

```bash
git add .cycod/aliases/
git commit -m "Add CycoD project aliases for development assistance"
```

### Using Shared Aliases

When a team member clones the repository, they'll automatically have access to all the project aliases:

```bash
git clone https://github.com/username/my-project.git
cd my-project

# They can immediately use the aliases
cycod --py-expert --question "How do I improve the performance of this code?"
```

## Viewing Alias Contents

To see what's in a specific alias:

```bash
cycod alias get py-expert --local
```

This allows team members to understand the exact instructions being given to the AI.

## Updating Project Aliases

As your project evolves, you may want to update your aliases:

```bash
cycod --add-system-prompt "You are a Python expert familiar with this project. The project follows PEP 8 standards, uses pytest for testing, and requires type hints using the typing module." --save-local-alias py-expert
```

This will overwrite the existing alias with updated instructions.

## Deleting Project Aliases

If an alias is no longer needed:

```bash
cycod alias delete py-expert --local
```

This will remove the alias file from your project's `.cycod/aliases/` directory. For more details about deleting aliases, see the [cycod alias delete reference](/reference/cli/alias/delete.md).

## Best Practices for Project Aliases

1. **Be specific**: Include project-specific information in your prompts
2. **Document standards**: Reference coding standards, testing frameworks, and other project requirements
3. **Maintain consistency**: Ensure aliases don't contradict each other
4. **Keep up-to-date**: Regularly update aliases as project requirements evolve
5. **Document your aliases**: Consider adding a README section explaining available aliases

## Project Alias Use Cases

Here are some real-world examples of how project aliases can be used:

### New Developer Onboarding

Create an onboarding alias to help new team members:

```bash
cycod --add-system-prompt "You are a guide for new developers joining this project. Explain the project structure, coding conventions, and development workflow clearly. Provide helpful examples and best practices." --save-local-alias onboarding
```

New team members can then use:

```bash
cycod --onboarding --question "How do I set up my development environment?"
```

### Project-Specific Documentation

Generate documentation that follows your project's style:

```bash
cycod --doc-helper --question "Generate docstrings for app.py"
```

### Code Reviews

Use the code review alias to get feedback on your code:

```bash
cycod --code-review --question "Review this code for security issues: $(cat auth.py)"
```

## Conclusion

Project-specific aliases created with `--save-local-alias` provide a powerful way to standardize AI interactions within your team. They ensure that everyone gets consistent, project-appropriate responses while reducing the need to type lengthy prompts repeatedly.

By including these aliases in version control, you ensure that your team has access to the same carefully crafted prompts, improving collaboration and maintaining consistent standards throughout your project.

## Next Steps

- Learn about [user-level aliases](/reference/cli/options/save-user-alias.md) for personal preferences
- Explore [global aliases](/reference/cli/options/save-global-alias.md) for organization-wide standards
- Check out [configuration profiles](/advanced/profiles.md) for more complex setups