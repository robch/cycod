# --instruction

The `--instruction` option is an alias for `--input` that allows you to provide one or more lines of input to the AI model.

## Syntax

```bash
cycod --instruction "TEXT" [other options]
```

You can provide one or more lines of input:

```bash
cycod --instruction "TEXT1" "TEXT2" [other options]
```

## Description

The `--instruction` option is an alias for `--input`. It provides a more semantically clear way to give instructions to the AI model, but functions identically to `--input`.

When using this option:
- The provided text is sent as a user message to the AI model
- Multiple text arguments are joined with newline characters
- If a file path is provided and the file exists, its contents are read and used as input
- The AI model will process your instruction and generate a response

This option is particularly useful when:
- You want your command to read more clearly as an instruction
- You're creating scripts where semantic clarity is important
- You want to distinguish between different types of inputs in your workflows

## Examples

### Basic Usage

Give a simple instruction:

```bash
cycod --instruction "Write a haiku about programming"
```

### Multiple Line Instruction

Provide a detailed instruction:

```bash
cycod --instruction "Create a JavaScript function" "that validates email addresses" "and returns a Boolean result"
```

### Reading from Files

Read content from a file to use as an instruction:

```bash
cycod --instruction task_description.txt
```

### Combining with Other Options

Use with other options for more specific instructions:

```bash
cycod --instruction "Explain this code:" --add-system-prompt "Use simple terms"
```

### Using Template Variables

Include template variables in your instruction:

```bash
cycod --instruction "Create a {language} class for {concept}" --var language=Python --var concept="binary search tree"
```

### Using in an Alias

Create and use a custom alias:

```bash
cycod --instruction "Create documentation for the following code:" --save-alias document
cycod --document "function calculateTotal(items) { ... }"
```

## Related Options

- [`--input`](input.md): The base option (identical functionality) 
- [`--inp`](inp.md): Shortened alias for `--input`
- [`--instructions`](instructions.md): For providing multiple sequential inputs to the AI model
- [`--inpu`](inpu.md): Shortened alias for `--instructions`
- [`--question`](question.md): Similar to `--input` but also sets `--interactive false --quiet`