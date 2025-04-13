---
hide:
- toc
icon: material/text-box-outline
---

--8<-- "snippets/ai-generated.md"

# Custom Prompts

Custom prompts are reusable templates that help streamline common interactions with ChatX.

## Using Prompts Interactively

``` { .bash .cli-command title="Start an interactive chat session" }
chatx
```

``` { .plaintext .cli-output }
User: ▌
```

``` { .plaintext .cli-command title="Use a slash command to activate a prompt" }
/code-review
```

``` { .plaintext .cli-output }
Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

User: ▌
```

``` { .plaintext .cli-command title="Paste code for review after the prompt" }
function add(a, b) { return a + b }
```

``` { .plaintext .cli-output }
Assistant: I've reviewed the code and here are my suggestions:

1. **Bugs and Edge Cases:**
   - There's no type checking for inputs. If non-number values are passed (strings, objects, etc.), unexpected results may occur.
   - No handling for missing parameters - if either `a` or `b` is undefined, you'll get `NaN`.

2. **Performance Optimizations:**
   - This simple function is already optimal in terms of performance.

3. **Style and Readability:**
   - Consider adding a function comment describing purpose and parameters
   - For consistency with modern JS practices, use arrow function syntax
   - Add explicit return type if using TypeScript

Improved version:
```javascript
/**
 * Adds two numbers together
 * @param {number} a - First number
 * @param {number} b - Second number
 * @return {number} Sum of a and b
 */
const add = (a, b) => {
  if (typeof a !== 'number' || typeof b !== 'number') {
    throw new TypeError('Both arguments must be numbers');
  }
  return a + b;
};
```

User: ▌
```

``` { .bash .cli-command title="Use a prompt in CLI with --input" }
chatx --input "/code-review
function multiply(a, b) { return a * b }"
```

``` { .plaintext .cli-output }
User: Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

function multiply(a, b) { return a * b }
```

## Creating Prompts

``` { .bash .cli-command title="Create a simple prompt" }
chatx prompt create hello "Hello! How can I help you today?"
```

``` { .plaintext .cli-output }
Prompt 'hello' saved to: C:\Users\username\.chatx\prompts\hello.prompt
```

### Multi-line Prompts

#### Bash (Linux/macOS)

``` { .bash .cli-command title="Create a multi-line prompt in Bash" }
chatx prompt create code-review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability"
```

#### CMD (Windows)

``` { .cmd .cli-command title="Create a multi-line prompt in CMD" }
chatx prompt create code-review "Please review this code and suggest improvements:^
1. Identify any bugs or edge cases^
2. Suggest performance optimizations^
3. Comment on style and readability"
```

#### PowerShell (Windows)

``` { .powershell .cli-command title="Create a multi-line prompt in PowerShell" }
chatx prompt create code-review "Please review this code and suggest improvements:`n1. Identify any bugs or edge cases`n2. Suggest performance optimizations`n3. Comment on style and readability"
```

### Creating Prompts from Files

``` { .bash .cli-command title="Create a file with your prompt text" }
echo "Based on the following git diff, generate a clear and concise commit message with:
- A short summary (50 chars or less)
- A more detailed explanation if needed
- Reference any relevant issue numbers

DIFF:
{diff}" > commit-msg-prompt.txt
```

``` { .bash .cli-command title="Create a prompt from file content" }
chatx prompt create commit-msg @commit-msg-prompt.txt
```

``` { .plaintext .cli-output }
Prompt 'commit-msg' saved to: C:\Users\username\.chatx\prompts\commit-msg.prompt
```

### Prompts with Variables

``` { .bash .cli-command title="Create a prompt with placeholder variables" }
chatx prompt create translate "Translate the following text from {source_lang} to {target_lang}:

{text}"
```

``` { .plaintext .cli-output }
Prompt 'translate' saved to: C:\Users\username\.chatx\prompts\translate.prompt
```

## Using Prompts with Variables

``` { .bash .cli-command title="Use a prompt with variables" }
chatx --prompt translate --var source_lang=English --var target_lang=Spanish --var "text=Hello, how are you today?"
```

``` { .plaintext .cli-output }
User: Translate the following text from English to Spanish:

Hello, how are you today?
```

``` { .plaintext .cli-output }
Assistant: Hola, ¿cómo estás hoy?

User: ▌
```

## Managing Prompts

``` { .bash .cli-command title="List all available prompts" }
chatx prompt list
```

``` { .plaintext .cli-output }
Available prompts:
  code-review - Review code for issues and improvements
  commit-msg - Generate a good commit message
  hello - Simple greeting
  translate - Translate text between languages
```

``` { .bash .cli-command title="Show a specific prompt" }
chatx prompt show code-review
```

``` { .plaintext .cli-output }
Prompt 'code-review':
Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability
```

``` { .bash .cli-command title="Delete a prompt" }
chatx prompt delete hello
```

``` { .plaintext .cli-output }
Prompt 'hello' deleted.
```

## Prompt Storage

Prompts are stored as text files in your ChatX user directory:

```
Windows: %USERPROFILE%\.chatx\prompts\
macOS/Linux: ~/.chatx/prompts/
```

Each prompt is saved with a `.prompt` extension, although you don't need to include this extension when referencing prompts.

You can manually edit these files or create new ones by adding text files to this directory.

??? tip "Sharing Prompts with Your Team"

    You can share prompts with your team by adding them to a shared repository and configuring 
    ChatX to look for prompts in that location using the `--config-dir` option or by setting up
    a global prompt directory.