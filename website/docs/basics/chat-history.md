---
hide:
- toc
icon: material/history
---

# Chat History

??? question "Why use chat history?"

    Chat history is useful for maintaining context in conversations, especially when dealing with complex topics or multiple questions.
    
    It allows you to save and load previous conversations, making it easier to pick up where you left off.

    You can also use it to create branches in your conversation, allowing for different paths or topics to be explored.

Use `--output-chat-history`, `--input-chat-history`, `--chat-history`, and `--continue` to manage your chat history.  

``` bash title="Output chat history using the default file name, a specific filename, or a templatized filename"
chatx --output-chat-history # uses "chat-history-{time}.jsonl" by default
chatx --output-chat-history chat-history.jsonl
chatx --output-chat-history chat-history-{time}.jsonl
```

``` bash title="Input chat history from a file"
chatx --input-chat-history chat-history-1744466974845.jsonl
```

``` bash title="Input and output chat history using same filename"
chatx --chat-history chat-history-project-details.jsonl
```

``` bash title="Continue the most recent chat history"
chatx --continue
```

## Conversation branching

??? question "What is conversation branching?"

    Conversation branching allows you to create different paths in your conversation history.

    It's the JSONL equivalent of threading in messaging apps, like Outlook, Teams, or Slack, but for the AI.


```bash title="Pre-create the beginning of a conversation"
chatx --input "Research how `chatx help` works" --output-chat-history how-help-works.jsonl
```

```bash title="Continue the conversation from the branch point"
chatx --input-chat-history how-help-works.jsonl --input "add a help topic for `mdx integration`"
```

```bash title="Create an alias for the branch point, and use it more easily"
chatx --input-chat-history how-help-works.jsonl --save-alias how-help-works
chatx --how-help-works --input "review the help topic for `mdx integration`"
```

```bash title="Process many conversations from that point in the conversation"
chatx --how-help-works --input "review the {x} topic" --foreach var x in "usage" "options" "mdx integration"
```

## JSONL Format

The `--output-chat-history`, `--input-chat-history`, `--chat-history` and `--continue` options use JSONL file format.

```jsonl
{"role":"system","content":"You are a helpful assistant."}
{"role":"user","content":"What is the capital of France?"}
{"role":"assistant","content":"The capital of France is Paris."}
{"role":"user","content":"What about Germany?"}
{"role":"assistant","content":"The capital of Germany is Berlin."}
```

## Trajectories

??? question "What are trajectories?"

    Trajectories are a human-readable format for saving your conversation history.

    They are useful for reviewing conversations or sharing them with others.

``` bash title="Save trajectories using the default file name, a specific filename, or a templatized filename"
chatx --output-trajectory # uses "trajectory-{time}.md" by default
chatx --output-trajectory trajectory.md
chatx --output-trajectory trajectory-{time}.md
```

### Trajectory Format

The `--output-trajectory` option uses a human-readable markdown format.

```bash
chatx --output-trajectory trajectory.md --question "what's the date?"
```

````markdown title="trajectory.md"

> what's the date?

I can get the current date for you using the GetCurrentDate function.

```xml
<function_calls>
<invoke name="GetCurrentDate">
</invoke>
</function_calls>
<function_results>
2025-4-12
</function_results>
```

The current date is April 12, 2025.
````

