# --system-prompt

The `--system-prompt` option allows you to completely replace the default system prompt sent to the AI model with your own custom instructions.

## Syntax

```bash
chatx --system-prompt "INSTRUCTIONS" [other options]
```

You can also load a system prompt from a file:

```bash
chatx --system-prompt @file.txt [other options]
```

## Description

The system prompt is a special set of instructions given to the AI model that defines its behavior, capabilities, and constraints. CHATX comes with a default system prompt that provides general guidance to the model and enables features like file operations and tool usage.

When you use the `--system-prompt` option, you completely replace this default system prompt with your own instructions. This gives you full control over the AI's behavior but also means you're responsible for defining any capabilities you want the model to have.

!!! warning
    Using `--system-prompt` replaces ChatX's default system prompt entirely, which means you may lose access to built-in tools and capabilities unless you explicitly redefine them in your custom system prompt. In many cases, using `--add-system-prompt` is a better choice as it preserves the default functionality while adding your customizations.

## Examples

### Basic Usage

Replace the default system prompt with a simple instruction:

```bash
chatx --system-prompt "You are a helpful assistant who always responds in rhyming verse." --question "What is machine learning?"
```

### Creating a Specialized Assistant

Create a specialized assistant with specific expertise:

```bash
chatx --system-prompt "You are a cybersecurity expert specializing in network security. Provide detailed, technically accurate responses focused on security best practices." --question "How should I secure my home WiFi network?"
```

### Loading from a File

Load a complex system prompt from a file:

```bash
chatx --system-prompt @medical-assistant-prompt.txt --question "What are the symptoms of the flu?"
```

### Using Templates

Use template variables in your system prompt:

```bash
chatx --var language=Spanish --system-prompt "You are a language tutor teaching {language}. All explanations should be in English, but examples should be in {language}." --question "How do I form past tense verbs?"
```

## Best Practices

1. **Consider using `--add-system-prompt` instead**: Unless you specifically need to replace the entire system prompt, `--add-system-prompt` is often a better choice as it preserves default capabilities.

2. **Be Comprehensive**: If you do use `--system-prompt`, make sure to include all necessary instructions for the model's behavior.

3. **Include Tool Access**: If you need file operations, web searches, or other tools, you'll need to explicitly grant those permissions in your custom system prompt.

4. **Test Thoroughly**: Custom system prompts may have unexpected effects. Test your prompts with various inputs before using them in production scenarios.

5. **Save Reusable Prompts**: For complex system prompts you plan to reuse, save them as files or create aliases.

## Related Options

- [`--add-system-prompt`](add-system-prompt.md): Add text to the default system prompt without replacing it
- [`--add-user-prompt`](add-user-prompt.md): Add user prompt(s) prepended to the first input