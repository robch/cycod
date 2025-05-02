# --add-system-prompt

The `--add-system-prompt` option allows you to add additional text to the system prompt sent to the AI model without replacing the default system prompt entirely.

## Syntax

```bash
cycod --add-system-prompt "INSTRUCTION" [...] [other options]
```

You can provide one or more instructions to add to the system prompt:

```bash
cycod --add-system-prompt "INSTRUCTION1" "INSTRUCTION2" [other options]
```

You can also load system prompt additions from files:

```bash
cycod --add-system-prompt @file.txt "INSTRUCTION2" [other options]
```

## Description

The system prompt is a special set of instructions given to the AI model that defines its behavior, capabilities, and constraints. CYCOD comes with a default system prompt that provides general guidance to the model.

While the `--system-prompt` option completely replaces the default system prompt, `--add-system-prompt` appends additional instructions to it. This is useful when you want to:

- Maintain the default system prompt's functionality
- Add specific behavior modifications or constraints
- Reinforce critical instructions without rewriting the entire system prompt

Multiple `--add-system-prompt` instructions are combined and appended to the default system prompt.

## Examples

### Basic Usage

Add a single instruction to the system prompt:

```bash
cycod --add-system-prompt "Always include examples in your answers." --question "What is an array?"
```

### Multiple Instructions

Add multiple instructions to the system prompt:

```bash
cycod --add-system-prompt "Answer in a casual, friendly tone." "Include examples in your responses." --question "Explain quantum computing."
```

### Security Constraint

Add a security constraint to the system prompt:

```bash
cycod --add-system-prompt "Never access files outside the current directory." --question "List the files in this directory."
```

### Language Constraint

Force the model to use a specific language:

```bash
cycod --add-system-prompt "Respond in Spanish only." --question "What is the weather like today?"
```

### Response Format

Control the format of responses:

```bash
cycod --add-system-prompt "Format your answers as bullet points." --question "What are the benefits of exercise?"
```

### Reading from Files

Load system prompt additions from files:

```bash
cycod --add-system-prompt @security-constraints.txt @formatting-rules.txt --question "Help me debug this code snippet."
```

### Combining with Templates and Variables

Use template variables within your system prompt additions:

```bash
cycod --var role=Python --add-system-prompt "You are a {role} expert. Provide code examples in {role} only." --question "How do I read a file?"
```

### Using in Interactive Mode

Apply system prompt additions to an interactive session:

```bash
cycod --add-system-prompt "Always cite your sources." --interactive
```

### Combining with Other Options

You can combine `--add-system-prompt` with other options:

```bash
cycod --add-system-prompt "Be concise." --add-user-prompt "I need help with coding." --question "How do I sort an array in Python?"
```

### Complex Example: Expert Role with File Analysis

```bash
cycod --add-system-prompt "You are a security expert reviewing code." "Focus on identifying potential security vulnerabilities." "Format your findings in a structured report with severity levels." --question "Review this code: $(cat vulnerable_code.js)"
```

## When to Use --add-system-prompt vs --system-prompt

| Use `--add-system-prompt` when you want to... | Use `--system-prompt` when you want to... |
|----------------------------------------------|-------------------------------------------|
| Keep CycoD's default intelligence and capabilities | Create a completely custom AI personality |
| Add specific constraints or instructions | Define the AI's entire behavior from scratch |
| Maintain consistent base behavior with slight modifications | Override default settings entirely |
| Reinforce critical instructions | Create specialized tools with specific instructions |

## Best Practices

1. **Be Specific**: Provide clear, specific instructions that the model can follow.
2. **Avoid Contradictions**: Make sure your added instructions don't contradict the default system prompt or each other.
3. **Test and Refine**: Experiment with different instructions to find what works best for your needs.
4. **Remember Context Limits**: Very long system prompts consume token context. Keep instructions concise.
5. **Prioritize Important Instructions**: Place the most important instructions first, as models tend to weigh earlier instructions more heavily.

## Related Options

- [`--system-prompt`](system-prompt.md): Replace the default system prompt entirely
- [`--add-user-prompt`](add-user-prompt.md): Add user prompt(s) prepended to the first input