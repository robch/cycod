---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Basic Memory Setup

This guide will walk you through setting up your first memory system with ChatX. Follow these steps to create, access, and utilize memories in your conversations.

## Creating Your First Memory File

Let's start by creating a basic memory file:

1. Create a `.memories` directory in your project:

   ```bash
   mkdir -p .memories
   ```

2. Create a file called `project.md` within this directory:

   ```bash
   touch .memories/project.md
   ```

3. Open this file in your preferred editor and add some basic project information:

   ```markdown
   # Project Memory

   ## Project Overview
   - This is a web application for managing inventory
   - Uses React frontend with Node.js backend
   - PostgreSQL database stores product and order information

   ## Key Requirements
   - Support for barcode scanning via mobile devices
   - Real-time inventory updates across multiple locations
   - Reporting capabilities for sales and inventory trends
   ```

## Creating a Memory Alias

Now, let's create an alias that makes it easy to include this memory in your conversations:

```bash
chatx --input "/file .memories/project.md" \
      --add-system-prompt "Always consider the project information in memory when answering questions. Refer to details from memory when relevant." \
      --save-local-alias memory
```

This alias does two things:
1. Loads the contents of your memory file
2. Adds a system prompt instructing the AI to use this information

## Using Your Memory

Now you can use your memory in conversations simply by adding the `--memory` flag:

```bash
chatx --memory --question "What database does our project use?"
```

The AI will now be aware of the project details you stored in memory and will respond accordingly:

```
Based on the project memory, your project uses PostgreSQL as its database
to store product and order information.
```

## Updating Your Memory

As your project evolves, you'll want to update your memories. Simply edit the markdown files in your `.memories` directory:

```bash
# Open the memory file in your editor
vi .memories/project.md
```

Add new information, refine existing details, or remove outdated content. The next time you use the `--memory` alias, ChatX will load the updated information.

## Multiple Memory Files

As your project grows, you might want to organize memories into multiple files. Create additional files in your `.memories` directory:

```
.memories/
├── project.md
├── architecture.md
├── api.md
└── team.md
```

To access all memories, create an enhanced alias:

```bash
chatx --input "/files .memories/*.md" \
      --add-system-prompt "Always consider the project information in memory when answering questions. Refer to details from memory when relevant." \
      --save-local-alias all-memory
```

Now you can use `--all-memory` to include all of your memory files in a conversation.

## Adding Memory to Existing Conversations

You can also bring memory into an ongoing conversation:

```bash
# Start a conversation
chatx --question "Tell me about our project"

# Later in the same conversation, add memory
/file .memories/project.md

# Now the AI has access to the memory content
What database are we using?
```

## Default System Prompt

For a more seamless experience, you can create a default system prompt that always instructs the AI to consider memory:

```bash
chatx config set DefaultSystemPrompt "You are a helpful assistant. When provided with information from memory files, always consider this information in your responses. Refer to specific details from memory when relevant." --local
```

## Verifying Memory Integration

To verify that your memories are being properly integrated:

```bash
chatx --memory --question "What do you know about this project?"
```

The AI should respond with information drawn from your memory files, demonstrating that the memory system is working correctly.

## Next Steps

With basic memory set up, you're ready to explore more advanced concepts:

- Learn about [tiered memories](tiered-memories.md) for managing memories across scopes
- Discover [targeted memory access](targeted-access.md) for selective context loading
- Explore ways to create [self-updating memories](self-updating.md)