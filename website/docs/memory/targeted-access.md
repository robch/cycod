---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Targeted Memory Access

One of the most powerful aspects of CycoD's memory system is the ability to selectively access specific memories based on your current needs. This targeted approach offers several benefits:

- **Reduces token usage** by only loading relevant memories
- **Focuses the AI** on the most pertinent information
- **Prevents information overload** and conflicting guidance
- **Enables specialized workflows** for different tasks

## Selecting Memories by Pattern

CycoD supports glob pattern matching to select specific memory files:

### By Filename

```bash
cycod --input "/files .memories/**/*architecture*.md" \
      --add-system-prompt "Consider the architectural information from memory." \
      --save-local-alias architecture-memory
```

This selects all files with "architecture" in their name, regardless of directory depth.

### By Directory

```bash
cycod --input "/files .memories/coding/**/*.md" \
      --add-system-prompt "Consider the coding standards and patterns from memory." \
      --save-local-alias coding-memory
```

This selects all files within the "coding" subdirectory.

### By Multiple Patterns

You can combine patterns to select files that match specific criteria:

```bash
cycod --input "/files .memories/**/{api,backend,database}/*.md" \
      --add-system-prompt "Consider the backend system information from memory." \
      --save-local-alias backend-memory
```

This selects all files within directories named "api", "backend", or "database".

## Topic-Specific Memory Aliases

Create specialized aliases for different domains or tasks:

### Frontend Development

```bash
cycod --input "/files .memories/**/{frontend,ui,design}/*.md" \
      --input "/files .memories/**/react*.md" \
      --add-system-prompt "Consider the frontend development knowledge from memory when discussing UI elements, React components, or design patterns." \
      --save-local-alias frontend-memory
```

### Security Analysis

```bash
cycod --input "/files .memories/**/{security,auth}/*.md" \
      --input "/files .memories/**/compliance*.md" \
      --add-system-prompt "Consider security best practices and compliance requirements from memory when discussing security concerns or authentication mechanisms." \
      --save-local-alias security-memory
```

### Project Management

```bash
cycod --input "/files .memories/**/{planning,team,process}/*.md" \
      --input "/files .memories/**/roadmap*.md" \
      --add-system-prompt "Consider project management processes, team agreements, and roadmap details from memory when discussing project planning or team coordination." \
      --save-local-alias pm-memory
```

## Role-Based Memory Access

Create memory configurations for different roles:

### Developer Role

```bash
cycod --input "/files .memories/**/coding-standards.md" \
      --input "/files .memories/**/architecture.md" \
      --input "/files .memories/**/api-specs.md" \
      --add-system-prompt "You are acting as a senior developer. Consider the coding standards, architecture, and API specifications from memory." \
      --save-local-alias dev-memory
```

### Code Reviewer Role

```bash
cycod --input "/files .memories/**/code-review-checklist.md" \
      --input "/files .memories/**/coding-standards.md" \
      --input "/files .memories/**/security-guidelines.md" \
      --add-system-prompt "You are acting as a code reviewer. Consider the review checklist, coding standards, and security guidelines from memory when reviewing code." \
      --save-local-alias review-memory
```

## Context-Sensitive Memory Loading

You can create system prompts that specify when and how to use different memories:

```bash
cycod --input "/files .memories/**/*.md" \
      --add-system-prompt "Consider the information in memory according to these guidelines:
      - For architecture questions, prioritize *architecture.md files
      - For API questions, prioritize *api*.md files
      - For coding standards, prioritize *coding*.md files
      - For process questions, prioritize *process*.md files" \
      --save-local-alias smart-memory
```

## Dynamic Memory Loading in Conversations

You can also load specific memories during a conversation:

```
> Tell me about our authentication flow.
I'd need more specific information about your authentication system.

> /file .memories/security/authentication.md
I've loaded information about your authentication system.

> Now can you explain our authentication flow?
Based on the memory file, your authentication system uses OAuth 2.0 with the following flow:
1. User is redirected to the identity provider
2. After authentication, the identity provider returns an authorization code
3. Your backend exchanges this code for an access token
4. The token is validated and user information is retrieved
5. A session is created and a JWT is issued to the client
```

## Combining Fixed and Dynamic Memories

For the most flexibility, you can start with a base set of memories and add specific ones as needed:

```bash
# Start with base project context
cycod --memory --question "What's the overall architecture of our system?"

# Add specific domain knowledge during the conversation
/file .memories/database/schema.md

# Ask a more specific question
> Can you explain our database schema for user records?
```

## Memory Selection Best Practices

To get the most from targeted memory access:

1. **Group related information**: Organize memories by domain, topic, or function
2. **Use consistent naming**: Follow naming conventions that support pattern matching
3. **Create task-specific aliases**: Define aliases for common workflows
4. **Be specific in system prompts**: Tell the AI exactly how to use the selected memories
5. **Start minimal**: Begin with only essential memories and add more as needed

## Next Steps

Now that you can selectively access memories, learn how to create [self-updating memories](self-updating.md) that evolve over time.