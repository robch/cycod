# --no-templates

The `--no-templates` option disables template processing in all text inputs to ChatX.

## Syntax

```bash
chatx --no-templates [other options]
```

## Description

By default, ChatX processes templates in inputs, allowing for variable substitution, conditionals, and other template features. The `--no-templates` option turns this feature off completely, causing all curly braces and template expressions to be treated as literal text.

This option is a shorthand for `--use-templates false` and is particularly useful when:

- Your input contains curly braces that you want to preserve literally
- You're working with programming code examples that use curly braces
- You want to bypass template processing for performance reasons
- Your input contains text that might be incorrectly interpreted as template markup

## Examples

### Basic Usage

Send text with curly braces without processing templates:

```bash
chatx --no-templates --input "function example() { return {id: 1, name: 'Test'}; }"
```

Without `--no-templates`, the text inside the curly braces would be interpreted as template variables.

### Working with Programming Languages

When asking about code that uses curly braces:

```bash
chatx --no-templates --question "Explain the following CSS: .container { display: flex; justify-content: center; }"
```

### Using with Configuration Files

When loading from a configuration file that might contain template syntax:

```bash
chatx --no-templates --input @config_template.txt --question "Help me understand this configuration template"
```

### Using with Variables

When you have variables defined but want them ignored in a specific input:

```bash
chatx --var name=Alice --no-templates --input "How to use {name} as a variable in templates?"
```

This will send the literal string "{name}" rather than substituting "Alice".

## Notes

- `--no-templates` affects all inputs in the command, including system prompts and user inputs
- It only affects the current command and does not change any configuration settings
- It is a shorthand for `--use-templates false`

## Related Options

- [`--use-templates`](use-templates.md) - Explicit control over template processing
- [`--var`](var.md) - Set a variable for template substitution
- [`--vars`](vars.md) - Set multiple variables for template substitution

## See Also

- [Templates and Variables](../../../usage/templates-and-variables.md) - Detailed guide on using templates in ChatX