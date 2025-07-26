---
title: --vars
description: Set multiple variables for template substitution
---

# --vars

The `--vars` option allows you to define multiple variables at once for template substitution in CycoD inputs and prompts.

## Syntax

```bash
cycod --vars NAME1=VALUE1 NAME2=VALUE2 [NAME3=VALUE3 ...]
```

## Parameters

- `NAME1=VALUE1, NAME2=VALUE2, ...`: One or more variable name-value pairs.
  - Values containing spaces must be quoted: `name="John Smith"`
  - Multiple pairs are separated by spaces

## Description

The `--vars` option sets multiple variables that can be used for substitution in templates. Variables are referenced in templates using curly braces: `{variable_name}`.

This option is particularly useful when you need to set multiple related variables for complex templates. Variables defined with `--vars` can be used in:

- System prompts (`--system-prompt`)
- User inputs (`--input`, `--instruction`)
- Template conditionals (`{{if variable == "value"}}`)
- Template expressions (`{{set result = variable * 10}}`)

## Examples

### Basic Usage

```bash
cycod --vars name=Alice age=30 --input "Hello, my name is {name} and I am {age} years old."
```

This sends "Hello, my name is Alice and I am 30 years old." to the AI model.

### With Quoted Values

```bash
cycod --vars name="John Smith" location="New York City" --input "Hello, {name} from {location}!"
```

This sends "Hello, John Smith from New York City!" to the AI model.

### With System and User Prompts

```bash
cycod --vars language=Python experience=intermediate --system-prompt "You are helping a {experience} {language} developer." --input "How do I implement a binary search?"
```

### With Template Conditionals

```bash
cycod --vars os=Windows version=11 --input "{{if os == 'Windows'}}You are using Windows {version}{{else}}You are not using Windows{{endif}}"
```

### Combining with Foreach

```bash
cycod --vars format=simple color=blue --foreach var name in Alice Bob Charlie --input "Hello, {name}! Format: {format}, Color: {color}"
```

## Related Options

- [`--var`](var.md) - Set a single variable for template substitution
- [`--foreach`](foreach.md) - Define a loop variable with multiple values
- [`--use-templates`](use-templates.md) - Enable or disable template processing
- [`--no-templates`](no-templates.md) - Shorthand to disable template processing

## See Also

- [Templates and Variables](../../../usage/templates-and-variables.md) - Tutorial on using templates and variables in CycoD