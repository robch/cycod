# --inpu

The `--inpu` option is a shortened alias for `--instructions` that allows you to provide multiple sequential inputs to the AI model.

## Syntax

```bash
chatx --inpu "INPUT1" "INPUT2" [other options]
```

## Description

The `--inpu` option is a convenient shorthand for `--instructions`. It allows you to send multiple sequential inputs to the AI model, with each input being processed before the next one is sent. This creates a conversational flow similar to what you would experience in an interactive session, but in a single command.

When using this option:
- Each string argument is sent as a separate user message to the AI model
- The AI model will respond to each message in sequence
- Unlike `--inp` (or `--input`), which joins multiple arguments into a single message
- This is useful for simulating a multi-turn conversation in a single command

This option is particularly valuable for:
- Creating script-like interactions with the AI model
- Executing a sequence of related queries
- Testing how the model's responses evolve through a conversation
- Automation scenarios requiring multiple interaction steps

## Examples

### Basic Sequential Inputs

Ask a series of related questions:

```bash
chatx --inpu "What is a neural network?" "Explain backpropagation" "How does this relate to deep learning?"
```

### Building on Previous Responses

Create a progressive conversation:

```bash
chatx --inpu "Write a short story about a robot" "Make the ending happier" "Now add a twist"
```

### Refining Generated Content

Generate and refine content step by step:

```bash
chatx --inpu "Write a Python function to calculate Fibonacci numbers" "Optimize it for performance" "Add clear comments"
```

### Combined with Variables

Use variables across multiple inputs:

```bash
chatx --var language=Python --inpu "Create a {language} class for a bank account" "Add deposit and withdrawal methods"
```

### Using in an Alias

Create an alias for a common multi-step interaction:

```bash
chatx --inpu "Write a unit test for the following code:" "Now find potential edge cases" --save-alias code-test
```

## Related Options

- [`--instructions`](instructions.md): The full form of this option (identical functionality)
- [`--inp`](inp.md): For providing a single input (possibly with multiple lines)
- [`--input`](input.md): The full form of `--inp`
- [`--inputs`](inputs.md): The full form of `--instructions`/`--inpu`
- [`--question`](question.md): Similar to `--input` but also sets `--interactive false --quiet`
- [`--questions`](questions.md): Similar to `--inputs` but also sets `--interactive false --quiet`