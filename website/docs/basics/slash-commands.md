---
hide:
- toc
icon: material/console-line
---

--8<-- "snippets/ai-generated.md"

# Slash Commands

You can use slash commands interactively for quick access to functionality.

```bash
cycod [ENTER]

User: ▌
```

> /help

```plaintext
User: /help

  BUILT-IN

    /save     Save chat history to file
    /clear    Clear chat history
    /cost     Show token usage statistics
    /help     Show this help message

  EXTERNAL

    /files    List files matching pattern
    /file     Get contents of a file
    /find     Find content in files

    /search   Search the web
    /get      Get content from URL

    /run      Run a command

  PROMPTS

    No custom prompts found.

User: ▌
```

## Basic Commands

``` { .plaintext .cli-command title="Save chat history" }
/save
```

``` { .plaintext .cli-output }
Chat history saved to: chat-history.jsonl
```

``` { .plaintext .cli-command title="Save with a specific filename" }
/save my-important-chat.jsonl
```

``` { .plaintext .cli-output }
Chat history saved to: my-important-chat.jsonl
```

``` { .plaintext .cli-command title="Clear chat history" }
/clear
```

``` { .plaintext .cli-output }
Chat history cleared.
```

## File Operations

``` { .plaintext .cli-command title="Include file content" }
/file README.md
```

`````` { .plaintext .cli-output }
user-function: /file => ## README.md

Modified: 1 day ago
Size: 6 KB

```markdown
...
```

User: ▌
``````

``` { .plaintext .cli-command title="Include multiple files" }
/files **/*.md
```

`````` { .plaintext .cli-output }
user-function: /files => ## ...file1...

...

## ...file2...

...

User: ▌
``````


``` { .plaintext .cli-command title="Find occurrences of a pattern in files" }
/find TODO
```

``` { .plaintext .cli-output }
Found 5 matches:
  src/main.py:42: # TODO: Implement error handling
  src/utils.py:17: # TODO: Optimize this algorithm
  src/utils.py:89: function getData() { // TODO: Add caching
  tests/test_main.py:28: # TODO: Add more test cases
  tests/test_main.py:56: # TODO: Fix flaky test
```

## Command Execution

``` { .plaintext .cli-command title="Execute a shell command" }
/run --bash "ls -la"
```

```` { .plaintext .cli-output }
user-function: /run => ## `ls -la`

Output:
```
total 2468
drwxrwxrwx 1 robch robch   4096 Apr 13 22:57 .
drwxrwxrwx 1 robch robch   4096 Apr 11 18:59 ..
-rwxrwxrwx 1 robch robch   1088 Apr  2 17:28 LICENSE
-rwxrwxrwx 1 robch robch   7150 Apr 12 21:26 README.md
...
```

User: ▌
````

``` { .plaintext .cli-command title="Execute with output processing" }
/run git status
```

``` { .plaintext .cli-output }
On branch main
Your branch is up to date with 'origin/main'.

Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
	modified:   src/main.py

Untracked files:
  (use "git add <file>..." to include in what will be committed)
	new-feature.py

no changes added to commit (use "git add" and/or "git commit -a")
```

## Web Operations

``` { .plaintext .cli-command title="Search the web for information" }
/search AI chatbot trends
```

``` { .plaintext .cli-output }
Searching for: AI chatbot trends

Results:
1. "AI Chatbot Trends in 2025: What's New and Next" - https://example.com/ai-trends-2025
   AI chatbots have evolved significantly over the past year with improvements in context...

2. "Top 10 Chatbot Implementation Strategies" - https://example.com/chatbot-strategies
   Organizations implementing AI chatbots are seeing increased ROI when focusing on...

3. "The Future of Conversational AI" - https://example.com/conversational-ai-future
   Language models continue to improve in reasoning capabilities, with the latest...
```

``` { .plaintext .cli-command title="Get content from a specific URL" }
/get https://example.com/ai-trends-2025
```

``` { .plaintext .cli-output }
# AI Chatbot Trends in 2025: What's New and Next

The landscape of AI chatbots has transformed dramatically in the past year. Key trends include:

## 1. Multi-modal Interactions
Chatbots now seamlessly integrate text, voice, and visual inputs/outputs.

## 2. Domain-Specific Expertise
Rather than general-purpose assistants, we're seeing more specialized bots with deep knowledge in specific fields.

## 3. Improved Reasoning Capabilities
The latest models demonstrate enhanced logical reasoning and problem-solving skills.
```

## Custom Prompts

``` { .plaintext .cli-command title="Use a custom prompt template" }
/code-review
```

``` { .plaintext .cli-output }
Please paste or specify the code you'd like me to review. I'll provide feedback on:

1. Potential bugs or edge cases
2. Performance considerations
3. Style and readability
4. Best practices

User: ▌
```

``` { .plaintext .cli-command title="List available custom prompts" }
/prompts
```

``` { .plaintext .cli-output }
Available prompts:
  code-review - Review code for issues and improvements
  commit-msg - Generate a good commit message
  explain-code - Explain how code works
  refactor - Suggest refactoring for cleaner code
```

## Command Help

``` { .plaintext .cli-command title="Get help for interactive commands" }
/help
```

``` { .plaintext .cli-output }
Available commands:
  /clear - Clear the current chat history
  /save [filename] - Save the current chat history
  /file <pattern> - Search for files matching pattern
  /find <pattern> - Find occurrences of pattern in files
  /run <command> - Execute a command and show results
  /search <query> - Search the web for information
  /get <url> - Get content from a specific URL
  /help - Show this help message
  /prompts - List available custom prompts
  /<promptname> - Insert a custom prompt template
```
