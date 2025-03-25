# Comprehensive Guide to Claude Code: Commands & Features

Claude Code is an AI-powered terminal assistant developed by Anthropic that integrates directly with your development environment. This guide provides detailed explanations and examples for each major command and feature.

## Core Terminal Commands

### `claude` - Interactive Mode

The most basic command launches Claude Code in interactive mode, establishing a conversation where Claude can understand your project context.

**Usage:**
```bash
claude
```

**Example workflow:**
```
$ cd my-nodejs-project
$ claude
👋 Hello! I'm Claude Code. How can I help with your project today?
> tell me about this project
```

Claude will explore your directory structure, examine key files, and provide an analysis of your project.

### `claude "query"` - Initial Prompt

Starts Claude Code with an initial query, saving you a step when you already know what you want to ask.

**Usage:**
```bash
claude "your query here"
```

**Example:**
```bash
claude "explain how authentication works in this codebase"
```

This immediately launches Claude and has it analyze authentication patterns in your code, looking for files related to user login, session management, and security.

### `claude -p "query"` - Non-Interactive Mode

Executes a one-off query without entering interactive mode, printing the response directly to the terminal and then exiting. This is particularly useful for scripts, CI/CD pipelines, or quick checks.

**Usage:**
```bash
claude -p "your query here"
```

**Examples:**
```bash
# Get a quick summary of recent changes
claude -p "summarize what changed in the last commit"

# Check code quality non-interactively
claude -p "find any security vulnerabilities in the authentication module"

# Use in scripts
echo "Automated Code Review Results:" > report.md
claude -p "review the changes in PR #143 and highlight any issues" >> report.md
```

### `cat file | claude -p "query"` - Pipe Content

Process piped content through Claude, enabling Unix-style workflows where Claude becomes part of a command pipeline.

**Usage:**
```bash
cat file | claude -p "your instruction"
```

**Examples:**
```bash
# Explain error logs
cat error_logs.txt | claude -p "explain what's causing these errors" > error_analysis.md

# Analyze test results
npm test | claude -p "summarize these test results and suggest fixes for failing tests"

# Convert formats
cat data.json | claude -p "convert this JSON to a markdown table" > data_table.md
```

### `claude config` - Configuration Management

View and modify Claude Code settings, with support for both global and project-level configurations.

**Usage:**
```bash
claude config list                      # List all settings
claude config get <key>                 # Get a specific setting
claude config set <key> <value>         # Change a setting
claude config set -g <key> <value>      # Change a global setting
claude config add <key> <value>         # Add to a list setting
claude config remove <key> <value>      # Remove from a list setting
```

**Examples:**
```bash
# Switch to dark theme globally
claude config set -g theme dark

# Allow npm test to run without approval for this project
claude config add allowedTools "Bash(npm test)"

# Ignore node_modules and build directories in this project
claude config add ignorePatterns "node_modules/**"
claude config add ignorePatterns "build/**"

# Disable auto-updater globally
claude config set -g autoUpdaterStatus disabled
```

### `claude update` - Version Management

Updates Claude Code to the latest version, ensuring you have access to the newest features and fixes.

**Usage:**
```bash
claude update
```

**Example workflow:**
```bash
$ claude update
Checking for updates...
New version available: 0.2.1 → 0.2.2
Updating Claude Code...
✓ Update complete! Claude Code is now at version 0.2.2.
```

### `claude mcp` - Model Context Protocol

Configure and manage Model Context Protocol (MCP) servers, which extend Claude's capabilities by connecting to external tools and data sources.

**Usage:**
```bash
claude mcp add <name> <command> [args...]     # Add MCP server
claude mcp list                               # List servers
claude mcp get <name>                         # Get server details
claude mcp remove <name>                      # Remove server
```

**Examples:**
```bash
# Add a Postgres database server
claude mcp add postgres-db /path/to/postgres-mcp-server --connection-string "postgresql://user:pass@localhost:5432/mydb"

# Add a weather API
claude mcp add weather-api '{"type":"stdio","command":"/path/to/weather-cli","args":["--api-key","abc123"]}'

# Import servers from Claude Desktop
claude mcp add-from-claude-desktop

# Share a server with your team (creates .mcp.json)
claude mcp add shared-tooling -s project /path/to/shared-tool-server
```

## Interactive Slash Commands

These commands are available within an interactive Claude Code session.

### `/init` - Project Initialization

Creates a CLAUDE.md file that stores important project information, conventions, and frequently used commands.

**Usage:**
```
> /init
```

Claude will analyze your project and generate a CLAUDE.md file with sections like:
- Project overview
- Key files and architecture
- Build and test instructions
- Common workflows
- Coding standards

This file helps Claude maintain context about your project across sessions.

### `/help` - Command Reference

Displays available commands and quick usage information.

**Usage:**
```
> /help
```

### `/clear` - Reset Context

Clears the conversation history, starting with a fresh context. Useful when switching to a different task or to reduce token usage.

**Usage:**
```
> /clear
```

### `/compact` - Optimize Context Window

Compresses the conversation history to save context space while preserving important information. This helps manage token usage during long sessions.

**Usage:**
```
> /compact
```

### `/cost` - Token Usage Statistics

Shows detailed information about token consumption and estimated costs for the current session.

**Usage:**
```
> /cost
```

**Example output:**
```
Current session stats:
- Prompt tokens: 12,450
- Completion tokens: 4,322
- Total tokens: 16,772
- Estimated cost: $0.82

Note: These costs are estimates and actual billing may vary.
```

### `/config` - In-session Configuration

View or modify configuration settings without leaving the Claude session.

**Usage:**
```
> /config
```

This will display an interactive configuration interface within the session.

### `/bug` - Report Issues

Report bugs directly to Anthropic, sending the conversation transcript to help improve Claude Code.

**Usage:**
```
> /bug
```

### `/review` - Code Review

Requests a comprehensive code review of recent changes or specified files.

**Usage:**
```
> /review
```

**Example:**
```
> /review
I'll review your recent changes. Would you like me to focus on any specific aspects (e.g., performance, security, readability)?
```

### `/pr_comments` - Pull Request Comments

View comments from pull requests, useful when addressing feedback.

**Usage:**
```
> /pr_comments
```

### `/vim` - Vim Mode

Enables vim mode for alternating between insert and command modes within the Claude Code interface.

**Usage:**
```
> /vim
```

### `/terminal-setup` - Configure Key Bindings

Installs Shift+Enter key binding for newlines in iTerm2 and VSCode terminals.

**Usage:**
```
> /terminal-setup
```

## Advanced Functionality

### Extended Thinking

Explicitly ask Claude to think deeply about complex problems, which triggers its extended thinking capabilities.

**Usage:**
```
> think deeply about <complex task>
```

**Examples:**
```
> think deeply about the architecture for our new microservice
> think hard about potential security vulnerabilities in our authentication flow
> think about the edge cases we need to handle in this payment processing code
```

When prompted with "think" commands, Claude will display its reasoning process as italic gray text before providing a conclusion.

### Custom Slash Commands

Create reusable project-specific commands that all team members can access, or personal commands for your own use across projects.

**Project Commands:**
```bash
# Create command directory
mkdir -p .claude/commands

# Create command file
echo "Analyze the performance of this code and suggest three specific optimizations:" > .claude/commands/optimize.md

# Use in Claude
> /project:optimize
```

**Commands with Arguments:**
```bash
# Create command with $ARGUMENTS placeholder
echo "Fix issue #$ARGUMENTS with the following steps:
1. Understand the issue description
2. Locate relevant code
3. Implement a solution
4. Add tests
5. Prepare a PR description" > .claude/commands/fix-issue.md

# Use with arguments
> /project:fix-issue 123
```

**Personal Commands:**
```bash
# Create personal command directory
mkdir -p ~/.claude/commands

# Create personal command
echo "Review this code for security vulnerabilities, focusing on:" > ~/.claude/commands/security-review.md

# Use across any project
> /user:security-review
```

### Permission System

Claude Code implements a tiered permission system:

1. **Read-only operations** (file reads, directory listings): No approval required

2. **Shell commands**: Require approval, with option to remember permissions per directory and command
   ```
   > run tests for the auth module
   ⚠️ Claude wants to run: npm test -- --grep "auth"
   Allow this command? [y/n/a] a
   ✓ Allowing this command for the current session
   ```

3. **File modifications**: Require approval until session end
   ```
   > fix the type errors in user.ts
   ⚠️ Claude wants to edit file: src/models/user.ts
   Allow this edit? [y/n/a] y
   ✓ Applying changes to src/models/user.ts
   ```

You can pre-approve tools to improve workflow:
```bash
# Allow specific npm commands
claude config add allowedTools "Bash(npm test)"
claude config add allowedTools "Bash(npm run lint:*)"
```

## Common Workflows

### Codebase Understanding

```
> give me an overview of this codebase
> what are the main components in this project?
> how does data flow from the frontend to the database?
> draw a diagram of our system architecture
```

### Bug Fixing

```
> I'm getting an error: "TypeError: Cannot read property 'id' of undefined"
> find where we're not handling null values properly in the user module
> fix the null reference in user.ts line A
```

### Code Generation

```
> create a React component for a user profile form
> write a function that validates email addresses
> generate unit tests for the authentication service
```

### Git Operations
```
> what's changed since my last commit?
> commit my changes with a descriptive message
> create a PR for my current branch with a summary of changes
> find commits related to the payment system from last month
```

### Documentation

```
> generate JSDoc comments for this function
> create README documentation for this module
> explain how this algorithm works in simple terms
```

## Using Claude Code with Images

Claude Code can analyze screenshots, diagrams, mockups, and other visual content.

**Adding Images:**
1. Drag and drop an image into the Claude Code window
2. Copy an image and paste it with Ctrl+V
3. Provide an image path: `claude "Analyze this image: /path/to/image.png"`

**Example Prompts:**
```
> What does this screenshot show?
> Describe the UI elements in this mockup
> Generate CSS to match this design
> Here's a screenshot of the error. What's causing it?
> This is our current database schema. How should we modify it?
```

## Cost Management

Claude Code typically costs $5-10 per developer per day, but can exceed $100 per hour during intensive use. To manage costs:    

1. **Track usage:**
   - Use `/cost` to see current session usage
   - Check historical usage in Anthropic Console

2. **Reduce token usage:**
   - Use `/compact` when context gets large
   - Write specific queries to avoid unnecessary scanning
   - Break down complex tasks into focused interactions
   - Use `/clear` between distinct tasks

3. **Set spend limits** in Anthropic Console to prevent unexpected costs

## Model Context Protocol (MCP) Integration

MCP allows Claude to access external tools and data sources:

### Database Access

```bash
# Add PostgreSQL database access
claude mcp add postgres-db /path/to/postgres-mcp-server --connection-string "postgresql://user:pass@localhost:5432/mydb"        
```

Then within Claude:
```
> describe the schema of our users table
> what are the most recent orders in the system?
> show me the relationship between customers and invoices
```

### Sharing MCP Servers with Team

```bash
# Add a shared server (creates .mcp.json file)
claude mcp add shared-tool -s project /path/to/tool-server

# View project servers
claude mcp list
```

This `.mcp.json` file can be committed to version control, allowing all team members to access the same MCP servers.

## CI/CD Integration

Claude Code supports headless execution for integration into continuous integration pipelines:

```bash
# Add to GitHub Actions workflow
- name: Lint Code with Claude
  run: |
    export ANTHROPIC_API_KEY=${{ secrets.ANTHROPIC_API_KEY }}
    claude -p "review the code changes and identify potential issues" --allowedTools "Bash(git diff:*)" > claude-review.md      
```

Or as a pre-commit hook:
```bash
#!/bin/bash
# .git/hooks/pre-commit
claude -p "review my staged changes for potential issues" --allowedTools "Bash(git diff --staged)" > review.txt
cat review.txt
```

This comprehensive guide covers the major commands and features of Claude Code, providing both explanations and practical examples to help you integrate this powerful AI assistant into your development workflow.
