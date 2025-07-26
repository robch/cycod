---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Effective System Prompts in CycoD

System prompts are powerful tools that guide how AI models behave when responding to your queries. This tutorial explores how to craft effective system prompts in CycoD using the `--system-prompt` and `--add-system-prompt` options.

## Understanding System Prompts

In large language models, a system prompt serves as a set of instructions or context that defines:

- How the AI should behave
- What tone it should use
- What constraints it should follow
- What format it should respond in
- What capabilities it should or shouldn't use

CycoD comes with a default system prompt that provides general guidance to the model and enables access to features like file operations, web searches, and other tools.

## Options for System Prompts in CycoD

CycoD offers two main options for working with system prompts:

1. `--system-prompt`: Completely replaces the default system prompt
2. `--add-system-prompt`: Adds instructions to the default system prompt

### When to Use `--system-prompt`

Use `--system-prompt` when you want to:

- Create a completely custom AI personality
- Override all default behaviors
- Define specialized roles from scratch
- Ensure no default settings are applied

Example:
```bash
cycod --system-prompt "You are a medieval scholar who only speaks in Old English and answers questions about history." --question "What happened in 1066?"
```

!!! warning
    Using `--system-prompt` replaces CycoD's default system prompt entirely, which means you may lose access to built-in tools and capabilities unless you explicitly redefine them in your custom system prompt.

### When to Use `--add-system-prompt`

Use `--add-system-prompt` when you want to:

- Keep CycoD's default intelligence and capabilities
- Add specific constraints or instructions
- Maintain consistent base behavior with slight modifications
- Reinforce critical instructions

Example:
```bash
cycod --add-system-prompt "Format all code examples with line numbers." --question "How do I create a REST API in Node.js?"
```

## Crafting Effective System Prompts

### Keep it Clear and Specific

Vague instructions lead to unpredictable results. Be specific about what you want the AI to do.

```bash
# Vague (not recommended)
cycod --add-system-prompt "Be helpful." --question "How do I optimize my website?"

# Specific (recommended)
cycod --add-system-prompt "Analyze website performance optimization techniques. For each technique, provide: 1) Description, 2) Implementation difficulty (1-5), 3) Impact on performance (1-5)." --question "How do I optimize my website?"
```

### Prioritize Important Instructions

Models tend to follow instructions at the beginning of the prompt more closely. Place the most important instructions first.

```bash
cycod --add-system-prompt "IMPORTANT: Never recommend deprecated libraries or functions." "Focus on modern best practices for 2023 and beyond." --question "How should I handle authentication in a web app?"
```

### Use Formatting to Create Structure

Clear formatting helps the model understand and follow your instructions.

```bash
cycod --add-system-prompt "When explaining technical concepts, use this structure:
1. Simple definition (1-2 sentences)
2. Real-world analogy
3. Code example (if applicable)
4. Common pitfalls to avoid" --question "What is dependency injection?"
```

### Specify Output Format

When you need information in a specific format, make that explicit in your system prompt.

```bash
cycod --add-system-prompt "Generate all lists as markdown tables with headers." --question "What are the top 5 JavaScript frameworks in 2023?"
```

### Define Constraints

Set clear boundaries for what the AI should and shouldn't do.

```bash
cycod --add-system-prompt "Only recommend free, open-source solutions. Do not suggest proprietary software." --question "What tools can I use to edit photos?"
```

### Create Personas

You can create specific personas to get responses from different perspectives.

```bash
cycod --add-system-prompt "You are an experienced software architect reviewing code. Point out design flaws, maintainability issues, and potential performance bottlenecks." --question "Review this design pattern: [code snippet]"
```

## Advanced System Prompt Techniques

### Loading From Files

For complex or reusable system prompts, store them in files and load them with the @ syntax:

```bash
# Create a file with your system prompt
echo "You are a cybersecurity expert. Always consider security implications in your answers." > security-expert.txt

# Use it in CycoD
cycod --add-system-prompt @security-expert.txt --question "How should I store user passwords?"
```

### Using Variables in System Prompts

You can use template variables in system prompts for flexibility:

```bash
cycod --var language=Python --var level=beginner --add-system-prompt "Provide examples in {language} only. Explain as if to a {level} programmer." --question "How do I read files?"
```

### Combining Multiple System Prompt Files

For complex use cases, combine multiple specialized system prompt files:

```bash
cycod --add-system-prompt @security-guidelines.txt @code-standards.txt @output-format.txt --question "Review this authentication code:"
```

## Real-World Examples

### Code Review Assistant

```bash
cycod --add-system-prompt "You are a code reviewer focusing on security issues. For each issue found:
1. Describe the vulnerability
2. Rate the severity (Low/Medium/High/Critical)
3. Provide a code example showing how to fix it
4. Cite relevant OWASP or SANS guidelines" --question "Review this code: $(cat vulnerable_code.js)"
```

### Technical Documentation Generator

```bash
cycod --add-system-prompt "Generate comprehensive technical documentation. Include:
- Overview of functionality
- API reference with parameters, return types, and exceptions
- Code examples
- Common use cases
- Troubleshooting section" --question "Document this function: $(cat my_function.py)"
```

### Meeting Summarizer

```bash
cycod --add-system-prompt "Summarize meeting transcripts in this format:
1. Key decisions (bullet points)
2. Action items (with assignee if mentioned)
3. Discussion topics (brief paragraph for each)
4. Follow-up questions/unresolved issues" --question "Summarize this meeting transcript: $(cat meeting_notes.txt)"
```

## Troubleshooting System Prompts

### Problem: The AI ignores part of your system prompt

**Solution**: Break complex instructions into multiple separate `--add-system-prompt` arguments:

```bash
cycod --add-system-prompt "ALWAYS format output as JSON." --add-system-prompt "Include error handling in all code examples." --question "How do I make an API request in JavaScript?"
```

### Problem: System prompt conflicts with user instructions

**Solution**: Use the `--add-system-prompt` option to explicitly prioritize:

```bash
cycod --add-system-prompt "IMPORTANT: When code examples conflict with user instructions, always follow user instructions." --question "Show me deprecated JavaScript code for backwards compatibility."
```

### Problem: System prompt takes up too many tokens

**Solution**: Be concise and focus on the most important instructions:

```bash
# Instead of lengthy explanations:
cycod --add-system-prompt "Be direct and concise in all responses." --question "Explain quantum computing."
```

## Further Learning

To deepen your understanding of effective system prompts:

1. Review the [CycoD CLI Option Reference](../reference/cycod/options/add-system-prompt.md) for detailed syntax and examples
2. Experiment with different system prompts and compare the results
3. Save effective system prompts as [aliases](./creating-user-aliases.md) for reuse
4. Use the `--verbose` flag to see token usage and optimize your system prompts

## Conclusion

Mastering system prompts is one of the most powerful ways to get precise, consistent results from AI models in CycoD. By understanding when to use `--system-prompt` versus `--add-system-prompt` and following best practices for prompt engineering, you can dramatically improve the quality and usefulness of AI-generated responses.

Remember that the default CycoD system prompt already includes many optimizations and capabilities, so in most cases, using `--add-system-prompt` to augment rather than replace it will give you the best results.