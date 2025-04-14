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
User: /code-review => Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

User: ▌
```

When you use a slash command for a prompt in interactive mode, ChatX now immediately replaces the command with the prompt content and sends it to the assistant. This provides a smoother experience when working with custom prompts.

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

=== "Bash"
    ``` { .bash .cli-command title="Create a multi-line prompt in Bash" }
    chatx prompt create code-review "Please review this code and suggest improvements:
    1. Identify any bugs or edge cases
    2. Suggest performance optimizations
    3. Comment on style and readability"
    ```

=== "CMD"
    ``` { .cmd .cli-command title="Create a multi-line prompt in CMD" }
    chatx prompt create code-review "Please review this code and suggest improvements:^
    1. Identify any bugs or edge cases^
    2. Suggest performance optimizations^
    3. Comment on style and readability"
    ```

=== "PowerShell"
    ``` { .powershell .cli-command title="Create a multi-line prompt in PowerShell" }
    chatx prompt create code-review "Please review this code and suggest improvements:`n1. Identify any bugs or edge cases`n2. Suggest performance optimizations`n3. Comment on style and readability"
    ```

``` { .plaintext .cli-output }
Prompt 'code-review' saved to: C:\Users\username\.chatx\prompts\code-review.prompt
```

### Creating Prompts from Files

=== "Bash"
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

=== "CMD"
    ``` { .cmd .cli-command title="Create a file with your prompt text" }
    (
    echo Based on the following git diff, generate a clear and concise commit message with:
    echo - A short summary (50 chars or less^)
    echo - A more detailed explanation if needed
    echo - Reference any relevant issue numbers
    echo.
    echo DIFF:
    echo {diff}
    ) > commit-msg-prompt.txt
    ```

    ``` { .cmd .cli-command title="Create a prompt from file content" }
    chatx prompt create commit-msg @commit-msg-prompt.txt
    ```

=== "PowerShell"
    ``` { .powershell .cli-command title="Create a file with your prompt text" }
    @"
    Based on the following git diff, generate a clear and concise commit message with:
    - A short summary (50 chars or less)
    - A more detailed explanation if needed
    - Reference any relevant issue numbers

    DIFF:
    {diff}
    "@ | Out-File -FilePath commit-msg-prompt.txt
    ```

    ``` { .powershell .cli-command title="Create a prompt from file content" }
    chatx prompt create commit-msg @commit-msg-prompt.txt
    ```

``` { .plaintext .cli-output }
Prompt 'commit-msg' saved to: C:\Users\username\.chatx\prompts\commit-msg.prompt
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

## Using Prompts with Input Flags

You can use custom prompts with the `--input` and `--inputs` flags directly from the command line:

``` { .bash .cli-command title="Use a prompt directly with --input" }
chatx --input "/code-review
function calculateTotal(items) {
  return items.reduce((sum, item) => sum + item.price, 0);
}"
```

``` { .plaintext .cli-output }
User: Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

function calculateTotal(items) {
  return items.reduce((sum, item) => sum + item.price, 0);
}
```

The `--inputs` flag allows you to chain multiple inputs, including slash commands:

``` { .bash .cli-command title="Use multiple prompts with --inputs" }
chatx --inputs "/code-review" "function getUser(id) { return users.find(u => u.id === id); }" "/refactor"
```

``` { .plaintext .cli-output }
User: Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

User: function getUser(id) { return users.find(u => u.id === id); }

User: Please suggest ways to refactor this code to make it more:
1. Readable
2. Maintainable
3. Efficient
4. Robust
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

## Using the --prompt Alias

ChatX provides a convenient `--prompt` alias for working with custom prompts. This alias works similarly to `--add-user-prompt` but is specifically designed for seamless integration with the custom prompt system.

``` { .bash .cli-command title="Use a prompt by name" }
chatx --prompt code-review --input "function sum(a, b) { return a + b; }"
```

The `--prompt` alias has two key behaviors that make it especially useful:

1. It automatically adds a slash prefix to prompt names when needed
2. It immediately resolves named prompts to their content

For example, these commands are equivalent:

``` { .bash .cli-command title="Using --add-user-prompt with full slash syntax" }
chatx --add-user-prompt "/code-review" --input "function sum(a, b) { return a + b; }"
```

``` { .bash .cli-command title="Using --prompt with simplified syntax" }
chatx --prompt code-review --input "function sum(a, b) { return a + b; }"
```

This makes `--prompt` particularly useful in scripts and command aliases where you want to reference your custom prompts by name.