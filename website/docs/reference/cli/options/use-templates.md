# --use-templates

The `--use-templates` option controls whether template processing is enabled for input texts provided to the AI model. This option determines if variable substitutions, conditional statements, and other template features are processed before sending content to the AI.

## Syntax

```bash
chatx --use-templates TRUE/FALSE [other options]
```

You can also use the shorthand form to disable templates:

```bash
chatx --no-templates [other options]
```

## Description

By default, ChatX processes templates in all inputs to:
- Replace variables in curly braces (e.g., `{name}`)
- Process conditional statements (`{{if}}`, `{{else}}`, `{{endif}}`)
- Evaluate expressions and set variables (`{{set x = 5}}`)
- Include file contents (`{@file.txt}`)

Using `--use-templates false` or `--no-templates` disables this processing entirely, causing all inputs to be sent to the AI as literal text without any substitutions or processing.

This option is useful when:
- You need to show literal curly braces in your text
- You're discussing template syntax itself and don't want it to be processed
- You're experiencing unexpected behavior due to template processing
- You need maximum performance for very large inputs

## Examples

### Disable Template Processing

Disable template processing to show literal curly braces or template syntax:

```bash
chatx --use-templates false --input "The variable {name} will appear with curly braces."
```

Or using the alias:

```bash
chatx --no-templates --input "Template syntax uses {{if condition}}...{{endif}} blocks."
```

### Conditional Enable/Disable

Enable or disable template processing based on a variable:

```bash
chatx --var debug=true --input "{{if debug == 'true'}}--use-templates false{{else}}--use-templates true{{endif}}"
```

### When Working with Code Examples

Disable templates when providing code examples that contain template-like syntax:

```bash
chatx --no-templates --input "In JavaScript, we use template literals like `Hello ${name}`."
```

### Default Behavior (Templates Enabled)

When templates are enabled (the default), you can use the full templating system:

```bash
chatx --var name=Alice --var language=Python --input "Hello, {name}! Show me an example of a function in {language}."
```

## Best Practices

1. **Keep Templates Enabled By Default**: The template system provides powerful features that are useful in most cases.

2. **Disable Only When Needed**: Only disable templates when you specifically need to show template-like syntax or literal curly braces.

3. **Escape Braces When Possible**: For simple cases where you just need literal braces, consider doubling them (`{{literal}}`) instead of disabling templates altogether.

4. **Document Template Usage**: When sharing complex commands with others, document whether templates are enabled or disabled to avoid confusion.

5. **Combine With Variables**: The template system is most powerful when combined with the `--var`, `--vars`, and `--foreach var` options.

## Related Options

- [`--var`](var.md): Define a variable for template substitution
- [`--vars`](vars.md): Define multiple variables for template substitution
- [`--foreach`](foreach.md): Define loop variables for batch commands
- [`--no-templates`](use-templates.md): Alias for `--use-templates false`