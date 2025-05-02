---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Tiered Memories

CycoD supports a flexible tiered memory system that allows memories to exist at different scopes. This enables you to have project-specific, user-specific, and even global memories that work together seamlessly.

## Memory Scopes

CycoD memory can be organized in three distinct scopes:

| Scope | Location | Purpose | Example Use |
|-------|----------|---------|-------------|
| Local (Project) | `./.memories/` | Project-specific knowledge | Architecture, API docs, team agreements |
| User | `~/.cycod/.memories/` | Personal preferences | Your coding style, workflow preferences |
| Global | `/etc/cycod/.memories/` | Organization-wide standards | Company coding standards, shared tools |

These scopes mirror CycoD's configuration system, providing a consistent approach across features.

## Creating Tiered Memories

Let's set up memories at different tiers:

### Local (Project) Memories

```bash
# Create project-level memories directory
mkdir -p .memories

# Create a project memory file
echo "# Project Standards
- Use feature branches named feature/xxx
- All PRs require at least one review
- Tests must pass before merging" > .memories/standards.md
```

### User Memories

```bash
# Create user-level memories directory
mkdir -p ~/.cycod/.memories

# Create a user preferences memory file
echo "# Personal Preferences
- Prefer detailed explanations with examples
- Use bullet points for lists
- Include code snippets when explaining code
- Highlight key insights at the end of responses" > ~/.cycod/.memories/preferences.md
```

### Global Memories (Admin Only)

```bash
# Create global memories directory (requires admin privileges)
sudo mkdir -p /etc/cycod/.memories

# Create a global standards memory file
echo "# Organization Standards
- All code must follow the company style guide
- Security vulnerabilities must be reported immediately
- Documentation must be updated with code changes" | sudo tee /etc/cycod/.memories/org-standards.md
```

## Accessing Tiered Memories

Create aliases to access memories at different tiers:

### Project Memories Alias

```bash
cycod --input "/files .memories/**/*.md" \
      --add-system-prompt "Consider the project standards and information in memory." \
      --save-local-alias project-memory
```

### User Memories Alias

```bash
cycod --input "/files ~/.cycod/.memories/**/*.md" \
      --add-system-prompt "Consider my personal preferences in memory." \
      --save-user-alias user-memory
```

### Global Memories Alias

```bash
cycod --input "/files /etc/cycod/.memories/**/*.md" \
      --add-system-prompt "Consider organization standards in memory." \
      --save-user-alias global-memory
```

### Combined Memories Alias

The real power comes from combining memories across tiers:

```bash
cycod --input "/files .memories/**/*.md" \
      --input "/files ~/.cycod/.memories/**/*.md" \
      --input "/files /etc/cycod/.memories/**/*.md" \
      --add-system-prompt "Consider all memory information. Prioritize project memory for project-specific questions, personal preferences for style questions, and organization standards for compliance questions." \
      --save-local-alias all-memory
```

## Tiered Memory Inheritance

When memories at different tiers contain related information, CycoD will have access to all of them, but you can provide guidance on how to handle conflicts:

```bash
cycod --input "/files .memories/**/*.md" \
      --input "/files ~/.cycod/.memories/**/*.md" \
      --input "/files /etc/cycod/.memories/**/*.md" \
      --add-system-prompt "When information conflicts between memory levels, prioritize in this order: 1) Project memory for technical details 2) User preferences for interaction style 3) Organization standards for compliance and security matters." \
      --save-local-alias prioritized-memory
```

## Scoped Memory Files

You can also create aliases that access specific memory files across tiers:

```bash
cycod --input "/files {.,.cycod,/etc/cycod}/.memories/**/standards.md" \
      --add-system-prompt "Consider all standards from memory when advising on process questions." \
      --save-local-alias standards-memory
```

This uses glob pattern matching to find all files named `standards.md` across the different memory tiers.

## Memory Inheritance Best Practices

When using tiered memories, follow these best practices:

1. **Avoid Duplication**: Store information at the most appropriate tier, rather than duplicating
2. **Clear Scoping**: Project memories should be specific to the project, not general preferences
3. **Explicit Priority**: When using multiple tiers, give clear guidance on how to resolve conflicts
4. **Consistent Naming**: Use consistent file naming across tiers for better organization
5. **Regular Maintenance**: Review and update memories at all tiers periodically

## Example: Language-Specific Settings

Here's an example of how to use tiered memories for language-specific settings:

```
.memories/python-project.md        # Project-specific Python practices
~/.cycod/.memories/python-style.md # Your personal Python style preferences
/etc/cycod/.memories/python-std.md # Organization Python standards
```

Then create a Python-specific memory alias:

```bash
cycod --input "/files **/*python*.md" \
      --add-system-prompt "Use these Python-specific memories when discussing Python code." \
      --save-local-alias python-memory
```

## Next Steps

Now that you understand tiered memories, learn how to be even more selective with [targeted memory access](targeted-access.md).