---
title: Templates and Variables
description: Learn how to use variables and generate multiple commands with ChatX
---

--8<-- "snippets/ai-generated.md"

# Templates and Variables in ChatX

ChatX provides powerful templating and variable substitution capabilities that can help you automate repetitive tasks, batch process similar queries, and create dynamic interactions with AI models.

## Basic Variable Substitution

You can define variables on the command line and use them within your inputs:

```bash
chatx --var name=Alice --input "Hello, {name}!"
```

This will send "Hello, Alice!" as input to the AI model.

## Multiple Variables

Define multiple variables at once using the `--vars` option:

```bash
chatx --vars name=Alice location="New York" --input "Hello, {name} from {location}!"
```

This will send "Hello, Alice from New York!" as input to the AI model.

### Advanced --vars Usage

The `--vars` option is particularly useful when working with complex templates that require multiple related variables:

```bash
chatx --vars language=Python version=3.11 framework=Django --input "Show me how to create a basic {framework} project with {language} {version}"
```

You can use variables in system prompts as well as inputs:

```bash
chatx --vars expertise=advanced domain="machine learning" --system-prompt "You are an {expertise} {domain} expert." --input "Explain neural networks"
```

Variables can also be used in conditional templates:

```bash
chatx --vars os=Windows format=PDF --input "{{if os == 'Windows'}}Show me how to view {format} files on Windows.{{else}}Show me how to view {format} files on {os}.{{endif}}"
```

### When to Use --vars vs --var

- Use `--var` when you only need to set a single variable
- Use `--vars` when setting multiple related variables at once
- Both options can be used together in the same command if needed

## Looping with --foreach

One of the most powerful features of ChatX is the ability to loop over sets of values using the `--foreach` option. This allows you to run multiple similar commands with different inputs.

### Basic Usage

The basic syntax for `--foreach` is:

```bash
chatx --foreach var NAME in VALUE1 VALUE2 VALUE3 ... --input "Your text with {NAME}"
```

For example:

```bash
chatx --foreach var name in Alice Bob Charlie --input "Hello, {name}!"
```

This will run three separate commands:
1. `chatx --input "Hello, Alice!"`
2. `chatx --input "Hello, Bob!"`
3. `chatx --input "Hello, Charlie!"`

### Numeric Ranges

For sequential numbers, you can use the range syntax:

```bash
chatx --foreach var num in 1..5 --input "This is iteration {num}"
```

This will run five commands with `{num}` taking values from 1 to 5.

### Reading Values from Files

For larger sets of values, you can read them from a file:

```bash
chatx --foreach var customer in @customers.txt --input "Dear {customer}, we're writing to inform you..."
```

This processes each line in `customers.txt` as a separate value.

### Combining Multiple Loops

When you specify multiple `--foreach` options, ChatX creates all possible combinations (Cartesian product):

```bash
chatx --foreach var language in Python JavaScript Go --foreach var topic in "functions" "loops" --input "Show me how to use {topic} in {language}"
```

This generates 6 commands (3 languages × 2 topics).

### Parallel Processing

For efficiency, you can combine `--foreach` with the `--threads` option to process multiple commands in parallel:

```bash
chatx --threads 4 --foreach var topic in "algorithms" "data structures" "design patterns" "sorting" --question "Explain {topic} concisely"
```

This will run up to 4 commands simultaneously, potentially saving time on larger batches.

## Real-World Examples

### Example 1: Language Translations

Translate a phrase into multiple languages at once:

```bash
chatx --foreach var language in Spanish French German Japanese --input "Translate 'Welcome to our website' into {language}"
```

### Example 2: Code Examples in Different Languages

Get implementations of the same concept across multiple programming languages:

```bash
chatx --foreach var language in "Python" "JavaScript" "C#" "Java" "Go" --input "Write a function to check if a string is a palindrome in {language}. Include comments."
```

### Example 3: Batch Content Generation

Generate multiple social media posts:

```bash
chatx --foreach var topic in "productivity tips" "work-life balance" "mindfulness" "time management" --input "Write a short LinkedIn post about {topic}"
```

### Example 4: Processing Multiple Files

Analyze the content of multiple files:

```bash
chatx --foreach var file in @file-list.txt --input "Summarize the content of {file} in 3 bullet points"
```

Where `file-list.txt` contains paths to the files you want to analyze.

## Template Processing Control

ChatX's template system is enabled by default, allowing you to use variables, conditionals, and other template features in your inputs. However, there may be times when you want to disable template processing.

### Disabling Templates

If you need to show literal curly braces or discuss template syntax without it being processed:

```bash
chatx --use-templates false --input "This will show literal {curly braces} and template syntax like {{if condition}}"
```

You can also use the shorthand:

```bash
chatx --no-templates --input "This will show literal {curly braces}"
```

### When to Disable Templates

Disabling templates is useful when:

1. **Discussing Template Syntax**: When you want to explain how templates work
2. **Working with Code**: When the code you're discussing uses curly braces or similar syntax
3. **Troubleshooting**: When you suspect templates are causing unexpected behavior

### Example: Working with Programming Languages

Some programming languages use syntax that conflicts with ChatX templates. For example, JavaScript template literals:

```bash
chatx --no-templates --input "In JavaScript, you can use template literals like this:
const greeting = `Hello, ${name}!`;"
```

### Example: Showing JSON

When working with JSON that contains many curly braces:

```bash
chatx --no-templates --input '{
  "user": {
    "name": "Alice",
    "age": 30,
    "preferences": {
      "theme": "dark"
    }
  }
}'
```

## Tips for Working with Templates

1. **Quoting Values**: When values contain spaces, be sure to quote them: `--foreach var phrase in "hello world" "good morning"`

2. **Escaping Braces**: If you need literal curly braces in your text, you can either disable templates or escape them by doubling: `{{literal braces}}`

3. **Variable Scope**: Variables defined with `--foreach` are only available within the expanded commands, not across your entire session

4. **Combining with aliases**: You can save complex foreach loops as aliases for reuse

## See Also

- [--foreach reference](../reference/cli/options/foreach.md)
- [--var reference](../reference/cli/options/var.md)
- [--vars reference](../reference/cli/options/vars.md)
- [--threads reference](../reference/cli/options/threads.md)