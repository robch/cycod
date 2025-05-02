# CycoD Continue Flag Examples

Here are examples of using the `--continue` flag with various other CycoD features:

## Basic Usage

```bash
# Continue the most recent conversation
cycod --continue --question "What's next?"
```

## With Interactive Mode

```bash
# Continue in interactive mode
cycod --continue --interactive
```

## Saving Back to the Same File

```bash
# Continue and save back to the same file that was loaded
cycod --continue --output-chat-history auto --question "Let's continue"
```

## With Different AI Providers

```bash
# Continue with OpenAI
cycod --continue --use-openai --question "Can you explain this differently?"

# Continue with Azure OpenAI
cycod --continue --use-azure-openai --question "Let's try a different approach"

# Continue with GitHub Copilot
cycod --continue --use-copilot --question "What's your perspective on this?"
```

## With Template Variables

```bash
# Continue with variables
cycod --continue --var language=Python --question "How do I implement this in {language}?"
```

## With System Prompt Additions

```bash
# Continue but add a system prompt instruction
cycod --continue --add-system-prompt "Please be more concise in your answers" --question "Can you summarize?"
```

## With Token Management

```bash
# Continue but adjust token target 
cycod --continue --trim-token-target 8000 --question "Let's keep this focused"
```

## Creating Multiple Branches

```bash
# Create multiple branches from the same conversation
cycod --continue --output-chat-history approach1.jsonl --question "Let's try approach #1"
cycod --continue --output-chat-history approach2.jsonl --question "Let's try approach #2 instead"
```

## In Pipelines

```bash
# Use in a pipeline with other tools
cat document.txt | cycod --continue --question "What do you think about this document?"
```

## With Foreach loops

```bash
# Continue but iterate over multiple variables
cycod --continue --foreach var language in Python JavaScript Go --question "How would I do this in {language}?"
```

## Combined with Multiple Input Methods

```bash
# Continue and provide multiple sequential inputs
cycod --continue --inputs "First question" "Second follow-up question"
```

## In Aliases

```bash
# Create an alias for continuing with specific settings
cycod --continue --use-openai --openai-chat-model-name gpt-4o --save-alias continue-4o

# Later use the alias
cycod --continue-4o --question "Let's continue our discussion"
```