# The Unifying Pattern: "Context In, Insights Out"

## A Conceptual Framework for Cycod

The "Context In, Insights Out" pattern provides a powerful unifying concept that makes Cycod's command structure cohesive and logical. This framework demonstrates why commands like `chat`, `find`, `run`, and `web` belong together as parts of an integrated tool.

## Every Command Follows the Same Pattern

Each command in Cycod follows an identical conceptual flow:

| Command | Context In | Processing | Insights Out |
|---------|------------|------------|--------------|
| `cycod chat` | Questions, instructions, piped content | AI processes and reasons | AI responses |
| `cycod find` | File paths/patterns, content filters | Locate files, filter content, optional AI processing | Raw or AI-processed file insights |
| `cycod run` | Commands to execute | Execute commands, capture output, optional AI processing | Raw or AI-processed command insights |
| `cycod web` | Search terms or URLs | Retrieve web content, optional AI processing | Raw or AI-processed web insights |

This consistent pattern reveals that all commands are variations of the same fundamental operation: transforming different types of context into insights.

## Why These Commands Form a Cohesive Tool

This framework clarifies why these commands naturally belong together:

1. **They're all context processors** - Different ways to gather, filter, and process context for AI interactions
2. **They all can produce insights** - Either raw content or AI-processed understanding
3. **They all follow the same workflow** - Input → Processing → Output
4. **They all serve the same end goal** - More effective AI-assisted development

## The Pipeline Architecture

The separability of context gathering and insight generation enables powerful workflows:

```bash
# Gather context once
cycod find "**/*.js" --contains "TODO" > todos.md

# Branch into multiple insight paths
cycod chat --question "Prioritize these TODOs" < todos.md
cycod chat --question "Group these TODOs by component" < todos.md
cycod chat --question "Which TODOs impact performance?" < todos.md
```

Or create complex pipelines:
```bash
# Command output → Find filtering → Web context → Chat
cycod run "git log --since='1 week ago'" | \
cycod find "**/*.js" --changed-in-commit | \
cycod web search "javascript best practices" | \
cycod chat --question "How can I improve this code?"
```

## Command-Line Design Coherence

While traditional command-line tools often separate different functionality into distinct commands, Cycod's commands share a unifying purpose that makes them cohesive. The difference between commands isn't about different functions - it's about different *sources of context* for the same fundamental operation: transforming context into insights for AI-assisted development.

Just as Git has various commands (`commit`, `push`, `pull`) that all serve the unified purpose of version control, Cycod's commands all serve the unified purpose of transforming context into insights, simply from different sources.

## Recommended Messaging

This framing suggests introducing Cycod as:

> **Cycod: Transform context into insights with AI**
> 
> - Get insights from **files** with `cycod find`
> - Get insights from **commands** with `cycod run`  
> - Get insights from the **web** with `cycod web`
> - Get insights from **conversation** with `cycod chat`
> 
> All commands follow the same pattern: feed context in, get AI-powered insights out.

This presentation makes it immediately clear what unifies these commands and why they belong together as part of a cohesive developer tool.

## Application to Tool Design and Documentation

This unifying pattern should guide:

1. **Command Structure**: Ensure all commands maintain consistent parameter patterns where appropriate
2. **Documentation**: Organize documentation to emphasize the common pattern
3. **User Experience**: Design workflows that leverage this pipeline architecture
4. **Future Extensions**: Ensure new commands or features follow this same pattern

By emphasizing this "Context In, Insights Out" pattern, Cycod presents a coherent model that developers can quickly understand, making the tool's capabilities more discoverable and its purpose more clear.