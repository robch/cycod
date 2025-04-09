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

To add instructions to the default system prompt instead of replacing it, use `--add-system-prompt`:

```bash title="Add to system prompt"
chatx --add-system-prompt "Always include examples in your answers." --question "What is an array?"
```

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

For minimal output, use the `--quiet` flag:

```bash title="Quiet mode"
chatx --quiet --question "What is the capital of France?"
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

- [Chat History](chat-history.md): Learn how to save and continue conversations
- [Configuration](configuration.md): Configure CHATX to suit your preferences
- [Aliases](../advanced/aliases.md): Create shortcuts for common commands
- [Custom Prompts](../advanced/prompts.md): Create reusable text templates for conversations