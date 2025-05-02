# --prompt

The `--prompt` option is an alias for [`--add-user-prompt`](add-user-prompt.md) that provides a convenient way to use custom prompts.

## Syntax

```bash
cycod --prompt "NAME_OR_TEXT" [other options]
```

## Description

The `--prompt` option serves as an alias for `--add-user-prompt`, allowing you to specify a custom prompt to be used at the beginning of your chat session. There are two key ways to use this option:

1. **With a named prompt**: Provide the name of a previously created prompt (without the `.prompt` extension)
2. **With direct text**: Provide the prompt text directly

When using a named prompt, the `--prompt` option automatically adds a slash prefix if one is not present, making it convenient to reference your saved prompts.

## Examples

### Using a Named Prompt

If you have a saved prompt named "code-review":

```bash
cycod --prompt "code-review"
```

This will use the content of your saved "code-review" prompt.

### Using Direct Text

You can also provide prompt text directly:

```bash
cycod --prompt "Explain this code to me as if I'm a beginner:"
```

### Combining with Input

Use a custom prompt with specific content:

```bash
cycod --prompt "code-review" --input "function sum(a, b) { return a + b; }"
```

### Using with Variables

If your prompt contains variable placeholders:

```bash
cycod --prompt "translate" --var source_lang=English --var target_lang=Spanish --var "text=Hello, how are you today?"
```

## Behavior Notes

- When using `--prompt` with a prompt name (e.g., "code-review"), it will automatically be treated as a slash command (`/code-review`) if the slash isn't already present
- Unlike `--add-user-prompt`, the `--prompt` option is specifically designed to work seamlessly with the custom prompt system
- This option is particularly useful for integrating named prompts into command-line workflows and scripts

## Related Options

- [`--add-user-prompt`](add-user-prompt.md): The full version of this option
- [`--var`](var.md): Set variables for template substitution in prompts
- [`--input`](input.md): Provide content for your prompt to process