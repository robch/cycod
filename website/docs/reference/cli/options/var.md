---
title: --var
description: Define a variable for template substitution
---

# --var

## Description

The `--var` option allows you to define a variable for template substitution in your inputs to the AI model. Variables are referenced in your inputs using curly braces `{variable_name}` and are automatically replaced with their values when the input is processed.

## Syntax

```bash
cycod --var NAME=VALUE [other options]
```

## Parameters

- `NAME`: The name of the variable
- `VALUE`: The value to assign to the variable

## Details

When you provide input to CycoD using options like `--input`, `--question`, or in interactive mode, you can include placeholders for variables using curly braces. The `--var` option lets you define these variables and their values.

Variables defined with `--var` are:

- Case-insensitive (e.g., `{Name}` and `{name}` refer to the same variable)
- Available for template substitution in all inputs in the current command
- Stored in the command's variable dictionary
- Also stored in the configuration store with a `Var.` prefix

By default, template processing is enabled. If you want to use literal curly braces in your input without variable substitution, you can disable templates using the `--use-templates false` or `--no-templates` options.

## Examples

### Basic Variable Usage

Define a single variable and use it in your input:

```bash
cycod --var name=Alice --input "Hello, {name}!"
```

Output:
```
Hello, Alice!
```

### Using Multiple Variables

Define multiple variables using multiple `--var` options:

```bash
cycod --var name=Bob --var location="New York" --input "Hello, {name} from {location}!"
```

Output:
```
Hello, Bob from New York!
```

### In Template Conditions

Variables can be used in conditional template syntax:

```bash
cycod --var name=Alice --var is_member=true --input "Welcome, {name}! {{if is_member}}Thank you for being a member.{{else}}Consider becoming a member!{{endif}}"
```

Output:
```
Welcome, Alice! Thank you for being a member.
```

### With Code Generation

Use variables to customize code generation:

```bash
cycod --var language=Python --var function_name=calculate_area --input "Write a {language} function called {function_name} that calculates the area of a circle given its radius."
```

### Combining with Input Files

Use variables when processing file contents:

```bash
cycod --var format=JSON --input "Convert the following data to {format}:
$(cat data.txt)"
```

### In Aliases

Save common commands with variable placeholders:

```bash
cycod --var language=English --input "Translate the following text to {language}:" --save-alias translate
```

Later use:

```bash
cycod --translate --var language=Spanish "Hello world"
```

## Related Options

- [`--vars`](vars.md): Define multiple variables at once
- [`--foreach`](foreach.md): Define loop variables with multiple values
- [`--use-templates`](use-templates.md): Enable or disable template processing
- [`--no-templates`](use-templates.md): Disable template processing