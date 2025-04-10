# Chat Basics

CHATX offers both interactive and non-interactive modes for chatting with AI models. This guide covers the core commands and options for effective communication with AI through the terminal.

## Simple Queries

The simplest way to use CHATX is to ask a question with the `--question` flag:

```bash title="Basic question"
chatx --question "What is the capital of France?"
```

```plaintext title="Output"
The capital of France is Paris.
```

You can also use the `--input` flag, which works the same way:

```bash
chatx --input "What is the capital of France?"
```

## System Instructions

System instructions help guide the AI's behavior. Use the `--system-prompt` flag to replace the default system prompt:

```bash title="System prompt example"
chatx --system-prompt "You are a helpful assistant who speaks French." --question "What is the capital of France?"
```

```plaintext title="Output"
La capitale de la France est Paris.
```

### Adding to the System Prompt

Instead of completely replacing the default system prompt, you can add additional instructions using `--add-system-prompt`. This preserves CHATX's default intelligence while adding your specific requirements:

```bash title="Add to system prompt"
chatx --add-system-prompt "Always include examples in your answers." --question "What is an array?"
```

You can add multiple instructions by providing multiple arguments:

```bash title="Multiple system prompt additions"
chatx --add-system-prompt "Answer in a casual, friendly tone." "Include examples in your responses." --question "Explain quantum computing."
```

#### Common Use Cases for --add-system-prompt

**Controlling Response Format:**
```bash title="Format control"
chatx --add-system-prompt "Format your answers as bullet points." --question "What are the benefits of exercise?"
```

**Adding Security Constraints:**
```bash title="Security constraint"
chatx --add-system-prompt "Never access files outside the current directory." --question "List the files in this directory."
```

**Setting Language Preferences:**
```bash title="Language preference"
chatx --add-system-prompt "Respond in Spanish only." --question "What is the weather like today?"
```

**Personality Adjustment:**
```bash title="Personality adjustment"
chatx --add-system-prompt "Respond like a pirate." --question "How do I make a sandwich?"
```

The `--add-system-prompt` option is ideal when you want to fine-tune the AI's behavior without losing CHATX's built-in capabilities and safeguards.

## User Context with --add-user-prompt

While system prompts define how the AI behaves, user prompts are treated as if they came from you. The `--add-user-prompt` option adds text that's prepended to your first input or question, establishing context before your main query:

```bash title="Add user context"
chatx --add-user-prompt "I'm working with Python 3.10." --question "How do I use match statements?"
```

You can add multiple user prompts, which are processed in order:

```bash title="Multiple user prompts"
chatx --add-user-prompt "I use VS Code." "I'm on Windows 11." --question "How do I set up a Python virtual environment?"
```

### Common Use Cases for --add-user-prompt

**Setting a Role or Persona:**
```bash title="Define your role"
chatx --add-user-prompt "I'm a junior developer learning web development." --question "Explain how CSS flexbox works."
```

**Providing Background Information:**
```bash title="Background context"
chatx --add-user-prompt "I'm troubleshooting a Node.js application." --question "What could cause ECONNREFUSED errors?"
```

**Language Preference:**
```bash title="Language preference"
chatx --add-user-prompt "Please respond in French." --question "What are the top tourist attractions in Paris?"
```

**Combining with System Prompts:**
```bash title="Combining prompt types"
chatx --add-system-prompt "Be concise." --add-user-prompt "I need help with coding." --question "How do I sort an array in Python?"
```

The key difference between `--add-system-prompt` and `--add-user-prompt` is that system prompts are instructions to the AI about how to behave, while user prompts are treated as content you've already shared, providing context for your actual question.

## Interactive Chat

For multi-turn conversations, use the `--interactive` flag:

```bash title="Start interactive chat"
chatx --interactive
```

This opens an interactive session where you can have a back-and-forth conversation with the AI. You can exit by typing `exit`, `quit`, or pressing Ctrl+C.

You can also start an interactive session with an initial question:

```bash title="Interactive with initial question"
chatx --interactive --question "What is the capital of France?"
```

```plaintext title="Interactive chat example"
CHATX - AI-powered CLI, Version 1.0.0
Copyright(c) 2025, Rob Chambers. All rights reserved.

Type 'exit' or 'quit' to end the session. Press Ctrl+C to cancel the current request.

user@CHAT: What is the capital of France?

assistant: The capital of France is Paris.

user@CHAT: What about Italy?

assistant: The capital of Italy is Rome.

user@CHAT: exit
```

## Multiple Sequential Inputs

You can provide multiple inputs to be processed sequentially using `--inputs`:

```bash title="Multiple inputs"
chatx --inputs "What is the capital of France?" "What is the capital of Italy?" "What is the capital of Spain?"
```

Each input is processed as a separate turn in the conversation, with the AI responding to each one.

## Input from Files

You can read inputs from files using the `@` symbol:

```bash title="Read from file"
chatx --question @question.txt
```

You can do the same for system prompts:

```bash title="System prompt from file"
chatx --system-prompt @system.txt --question "What is the capital of France?"
```

## Input from Standard Input (stdin)

You can pipe content directly into CHATX:

```bash title="Pipe input"
echo "What is the capital of France?" | chatx
```

Or use the `-` symbol to read from stdin:

```bash title="Use stdin"
cat question.txt | chatx -
```

## Using Variables

CHATX supports variable substitution in prompts:

```bash title="Using variables"
chatx --var country=France --question "What is the capital of {country}?"
```

You can set multiple variables:

```bash title="Multiple variables"
chatx --var country=France --var landmark="Eiffel Tower" --question "Where is the {landmark} located in {country}?"
```

## Foreach Loops

You can run multiple versions of a command using foreach loops:

```bash title="Foreach loop"
chatx --foreach var country in France Italy Spain --question "What is the capital of {country}?"
```

This will run the command three times, once for each country.

## Saving Output

To save the AI's response to a file:

```bash title="Save answer to file"
chatx --question "What is the capital of France?" --output-answer answer.txt
```

## Streaming Output

By default, CHATX streams the AI's response in real-time. You can disable this with the `--stream` flag:

```bash title="Disable streaming"
chatx --stream false --question "What is the capital of France?"
```

## Verbose and Quiet Modes

For more output details, use the `--verbose` flag:

```bash title="Verbose mode"
chatx --verbose --question "What is the capital of France?"
```

For minimal output that shows only the AI's response and critical messages, use the `--quiet` flag:

```bash title="Quiet mode"
chatx --quiet --input "What is the capital of France?"
```

Note that the `--question` option already includes quiet mode automatically:

```bash title="Question includes quiet mode"
chatx --question "What is the capital of France?"  # --quiet is implied here
```

## Selecting Providers

You can explicitly select which AI provider to use:

```bash title="Use OpenAI"
chatx --use-openai --question "What is the capital of France?"
```

```bash title="Use Azure OpenAI"
chatx --use-azure-openai --question "What is the capital of France?"
```

```bash title="Use GitHub Copilot"
chatx --use-copilot --question "What is the capital of France?"
```

## Next Steps

Now that you understand the basics of using CHATX, explore these advanced topics:

- [Usage Examples](examples.md): See platform-specific examples for common tasks
- [Chat History](chat-history.md): Learn how to save and continue conversations
- [Configuration](configuration.md): Configure CHATX to suit your preferences
- [Aliases](../advanced/aliases.md): Create shortcuts for common commands
- [Custom Prompts](../advanced/prompts.md): Create reusable text templates for conversations