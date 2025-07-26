# `--inputs` Option

## Description

The `--inputs` option allows you to provide multiple inputs to be processed sequentially by the AI model, as if you were having a multi-turn conversation. Each input is sent as a separate user message, and the AI responds to each one before moving to the next input.

## Syntax

```bash
cycod --inputs "INPUT1" "INPUT2" [...]
```

## Aliases

- `--instructions`
- `--inpu`
- `--questions` (also sets `--interactive false --quiet`)

## Examples

### Example 1: Basic Sequential Inputs

```bash
cycod --inputs "What's today's date?" "Show me a calendar for this month"
```

The AI will first answer the question about today's date, then respond to the request to show a calendar.

### Example 2: Step-by-Step Problem Solving

```bash
cycod --inputs "Write a Python function to find prime numbers" "Now modify it to be more efficient" "Add comments to explain the algorithm"
```

The AI will first provide a basic Python function, then improve it with a more efficient algorithm, and finally add explanatory comments.

### Example 3: Progressive Refinement

```bash
cycod --inputs "Write a short story about a robot" "Make the ending more uplifting" "Now add more descriptive language"
```

This allows you to progressively refine the AI's output through sequential instructions.

### Example 4: Using File Content

```bash
cycod --inputs "Explain this code:" "$(cat mycode.py)" "How can I optimize it?"
```

This example first asks the AI to explain some code, provides the code from a file, then asks for optimization suggestions.

### Example 5: Using with Non-Interactive Mode

```bash
cycod --questions "What is Machine Learning?" "Give me three examples of ML applications" "How can beginners start learning ML?"
```

Using the `--questions` alias sets both `--interactive false` and `--quiet` flags, making the output suitable for script usage.

## Input Processing

When using `--inputs`:

1. Each argument is treated as a separate user message
2. The AI model responds to each input sequentially
3. If an argument is a file path and the file exists, the file's content is used as the input
4. Arguments can be multi-line by providing multiple sequential arguments

## Using with Templates

`--inputs` works well with template variables:

```bash
cycod --var language=Python --inputs "Explain {language} classes" "Show an example of inheritance in {language}"
```

## Differences from `--input`

- `--input` combines all arguments into a single user message
- `--inputs` treats each argument as a separate user message in sequence

## See Also

- [`--input`](input.md): For providing a single input to the AI model
- [`--instruction`](instruction.md): Alias for `--input`
- [`--instructions`](instructions.md): Alias for `--inputs`
- [`--inpu`](inpu.md): Alias for `--instructions`
- [`--questions`](questions.md): Alias for `--inputs` with additional flags