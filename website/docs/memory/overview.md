---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Memory Overview

Memory is a powerful capability that allows ChatX to maintain context and knowledge across conversations. By leveraging ChatX's file operations and system prompts, you can create a sophisticated memory system without any special-purpose code.

## What is Memory in ChatX?

In the context of AI assistants, memory refers to persistent information that influences how the AI responds to your questions. Unlike chat history, which preserves the back-and-forth conversation, memory stores knowledge, preferences, and important facts the AI should consistently consider.

With ChatX, memory is implemented using standard markdown files that can be:

- Organized by topic or domain
- Referenced selectively as needed
- Maintained across conversation sessions
- Scoped to projects, users, or globally

## Why Memory Matters

Memory dramatically improves interactions with AI by:

- **Reducing repetition**: You don't need to state the same facts over and over
- **Ensuring consistency**: The AI maintains awareness of previously established facts
- **Providing context**: Complex projects benefit from persistent knowledge
- **Enabling personalization**: The AI can adapt to your preferences and needs

## ChatX's Memory Philosophy

While some AI tools have built-in, black-box memory systems, ChatX takes a different approach:

1. **Transparent**: Memory is stored in plain text files you can view and edit
2. **Flexible**: Organize memory however works best for your needs
3. **Controlled**: You decide exactly which memories to include in each conversation
4. **Composable**: Built from existing ChatX capabilities rather than special-purpose code

This approach follows the Unix philosophy of using simple, composable tools to build powerful capabilities.

## Memory System Components

The ChatX memory system consists of three core components:

1. **Memory Files**: Markdown files containing knowledge, facts, and preferences
2. **Memory Access**: Commands to selectively load relevant memories
3. **Memory Instructions**: System prompts that tell the AI how to use the memories

In the following sections, we'll explore how to organize, implement, and leverage memories for more effective AI interactions.

## Memory vs. Chat History

It's important to understand the distinction between memory and chat history:

| Feature | Memory | Chat History |
|---------|--------|--------------|
| Purpose | Store persistent knowledge | Record conversations |
| Format | Structured information | Sequential exchanges |
| Longevity | Long-term, across sessions | Typically single session |
| Selectivity | Can be partial/selective | Usually all-or-nothing |
| Editability | Easily edited/refined | Typically not edited |

Both are valuable, and ChatX provides tools for managing both effectively.

## Next Steps

In the following pages, you'll learn:

- How to [organize your memories](organization.md) for maximum effectiveness
- How to [set up basic memory](setup.md) for your projects
- How to implement [tiered memories](tiered-memories.md) across scopes
- How to use [targeted memory access](targeted-access.md) for specific contexts
- How to create [self-updating memories](self-updating.md) that evolve over time
- Advanced [memory patterns](patterns.md) for specific workflows