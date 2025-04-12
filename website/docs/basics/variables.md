---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Variable Substitution

ChatX supports variable substitution in your inputs, prompts, and commands.

## Using Variables

``` { .bash .cli-command title="Define and use a simple variable" }
chatx --var name=Alice --input "Hello, {name}!"
```

``` { .plaintext .cli-output }
User: Hello, Alice!
```