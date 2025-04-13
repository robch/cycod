---
hide:
- toc
icon: material/chat
---

# Chat Basics

The `chatx` CLI allows you to interact with AI models from the command line.

```bash
chatx [ENTER]

User: ▌
```

> Hey

```plaintext
User: Hey

Assistant: Hello! I'm here to help you with any tasks or questions you might have.

I can assist you with Bash, CMD or PowerShell commands, or help with a wide range of other tasks like:

- Creating or modifying files
- Searching through codebases
- Running commands and formatting their output
- Web searches or content extraction
- Converting files to markdown
- And more!

Is there something specific you'd like help with today?

User: ▌
```

## Using `--input`

Use `--input` to input a single question or request.  

``` { .bash .cli-command title="Provide one input" }
chatx --input "What is the capital of France?"
```

``` { .plaintext .cli-output }
User: What is the capital of France?

Assistant: The capital of France is Paris. Paris is one of the most famous and
culturally significant cities in the world, serving as France's capital city
and largest metropolitan area. It's known for landmarks like the Eiffel Tower,
the Louvre Museum, Notre-Dame Cathedral, and its rich history, cuisine, fashion,
and art.

Is there anything specific about Paris or France that you'd like to know more about?

User: ▌
```

``` { .bash .cli-command title="Each argument is treated as a separate 'line'." }
chatx --input "What's the date?" "" "and the time?"
```

``` { .plaintext .cli-output }
User: What's the date?

and the time?

Assistant: I can get the current date and time for you.
assistant-function: GetCurrentDate {} => 2025-4-12

Assistant: 
assistant-function: GetCurrentTime {} => 8:51:46

Assistant: The current date is April 12, 2025, and the current time is 8:51:46.

User: ▌
```

``` { .bash .cli-command title="Input contents from a file" }
echo What is the capital of France > question.txt
chatx --input question.txt
```

``` { .plaintext .cli-output }
User: What is the capital of France

Assistant: The capital of France is Paris. It is one of the most famous cities in the world, known for its iconic landmarks like the Eiffel Tower, the Louvre Museum, and Notre-Dame Cathedral. Paris is also a global center for art, fashion, gastronomy, and culture.

User: ▌
```

``` { .bash .cli-command title="Combine multiple types of inputs" }
chatx --input question.txt "What language do they speak there?"
```

``` { .plaintext .cli-output }
User: What is the capital of France

What language do they speak there?

Assistant: I'll answer your questions about France.

## What is the capital of France?

The capital of France is Paris.

## What language do they speak there?

The official language of France is French. While there are regional languages and dialects in different parts of France, French is the primary language spoken throughout the country, including in Paris, the capital.

User: ▌
```

## Using stdin

``` { .bash .cli-command title="Implicitly input from stdin" }
echo What is the capital of France | chatx
```

``` { .plaintext .cli-output }
User: What is the capital of France

Assistant: The capital of France is Paris.
```

``` { .bash .cli-command title="Explicitly input from stdin using '-'" }
echo What is the capital of France | chatx --input -
```

``` { .plaintext .cli-output }
User: What is the capital of France

Assistant: The capital of France is Paris.
```

``` { .bash .cli-command title="Combine using stdin and additional input" }
echo France | chatx --input "What's the capital of:" -
```

``` { .plaintext .cli-output }
User: What's the capital of:
"France"

Assistant: The capital of France is Paris.
```

## Using `--inputs`

Use `--inputs` to chain multiple questions or requests into a conversation pipeline.

``` { .bash .cli-command title="Provide multiple inputs" }
chatx --inputs "What's the capital of France?" "Italy?" "Spain?"
```

``` { .plaintext .cli-output }

User: What's the capital of France?

Assistant: The capital of France is Paris.

User: Italy?

Assistant: The capital of Italy is Rome.

User: Spain?

Assistant: The capital of Spain is Madrid.

User: ▌
```

``` { .bash .cli-command title="Input contents from files" }
echo What is the capital of France? > q1.txt
echo What is the capital of Italy? > q2.txt
echo What is the capital of Spain? > q3.txt
chatx --inputs q1.txt q2.txt q3.txt
```

``` { .plaintext .cli-output }
User: What is the capital of France? 

Assistant: The capital of France is Paris.

User: What is the capital of Italy?

Assistant: The capital of Italy is Rome.

User: What is the capital of Spain?

Assistant: The capital of Spain is Madrid.

User: ▌
```

``` { .bash .cli-command title="Combine using stdin and additional input" }
echo United States | chatx --inputs q1.txt q2.txt q3.txt -
```

``` { .plaintext .cli-output }
User: What is the capital of France? 

Assistant: The capital of France is Paris.

User: What is the capital of Italy?

Assistant: The capital of Italy is Rome.

User: What is the capital of Spain?

Assistant: The capital of Spain is Madrid.

User: United States

Assistant: The capital of the United States is Washington, D.C. (District of Columbia). 
```

## Non-interactive Mode

By default, `chatx` runs in interactive mode, allowing you to have a back-and-forth conversation.

To run in non-interactive mode, you can use the `--interactive` option, setting it to `false`:

```bash
chatx --input "What is the capital of France?" --interactive false
```

When piping input into `chatx`, it will automatically run in non-interactive mode.

```bash
echo "What is the capital of France?" | chatx
```

When piping the output from `chatx`, it will also run in non-interactive mode.

```bash
echo "What is the capital of France?" | chatx > output.txt
```

## Quiet Mode

To suppress the majority of outputs (e.g. `User:`, `Assistant:`, etc.), use the `--quiet` flag:

``` { .bash .cli-command title="Quiet mode" }
echo "What is the capital of France?" | chatx --quiet --interactive false
```

``` { .plaintext .cli-output }
I'd be happy to answer your question about the capital of France.

The capital of France is Paris. Paris is not only the capital city but also the largest city in France, known for its art, culture, cuisine, and landmarks like the Eiffel Tower, the Louvre Museum, and Notre-Dame Cathedral.
```

??? warning "`--quiet` does not disable interactivity"

    When using `--quiet` mode, unless interactivity is turned off, `chatx` will still wait for user input. This means that if you provide a question, it will respond and then wait for further input.

## Using `--question` or `-q`

The `--question` option and the `-q` shortcut are built-in aliases for: `--interactive false --quiet` and `--input`.

``` { .bash .cli-command title="Provide one question" }
chatx -q "What is the capital of France?"
```

``` { .plaintext .cli-output }
The capital of France is Paris. 
```

This is typically useful for quick questions, or if you want to capture the output into a file, or pipe the output to another program.

```bash
git log -5 | chatx -q "Summarize the last 5 commits in this repository." > changes.txt
```

```bash title="Code review changes, open in VSCode"
git status | chatx -q "Code review my changes" - | chatx -q "format concisely as code review" | code -
```