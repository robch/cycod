# Unified Tool Concept: Cyco Dev (`cdv`)

I understand your proposal to merge `mdx` and `chatx` into a single, more comprehensive tool called "Cyco Dev" (`cdv`). Let me organize the key aspects of this unified tool to facilitate our discussion.

## Current State Overview

### CHATX
- **Primary Focus**: AI chat interactions
- **Key Functionality**:
  - Interactive chat sessions with AI
  - Configuration management (config, aliases, prompts)
  - Provider management (OpenAI, Azure OpenAI, GitHub Copilot)
  - GitHub integration
  - MCP (Model Context Protocol) server management
- **Current Command Structure**: Default behavior is chat, with noun/verb subcommands for configuration

### MDX
- **Primary Focus**: Context gathering and processing
- **Key Functionality**:
  - Converting files to markdown
  - Running commands and processing output
  - Web search result processing
  - Web page content processing
  - AI-powered processing of gathered context
- **Current Command Structure**: Default behavior is file processing, with subcommands for other operations

### Current Integration Points
- MDX uses CHATX when users specify `--instructions` or `--file-instructions`
- CHATX has slash commands (`/files`, `/find`, `/search`, `/get`, `/run`) that spawn MDX

## Proposed Unified Structure: Cyco Dev (`cdv`)

### Core Command Categories

1. **Chat Operations**
   - `cdv chat` (moved from top-level CHATX behavior)
   - Interactive and non-interactive chat with AI models

2. **Context Gathering Operations** (from MDX)
   - `cdv find` (default MDX file finding behavior)
   - `cdv run` (from MDX run)
   - `cdv web search` (from MDX web search)
   - `cdv web get` (from MDX web get)

3. **Configuration Operations** (kept from CHATX)
   - `cdv alias` (command aliases)
   - `cdv config` (settings management)
   - `cdv prompt` (prompt templates)
   - `cdv mcp` (Model Context Protocol servers)
   - `cdv github` (GitHub integration)
   - `cdv help` (help system)

### Command Structure Examples

```
# Chat operations
cdv chat                                         # Interactive chat
cdv chat --question "What is the time?"          # Simple question
cdv chat --instructions "Review this code"       # Process with instructions

# Context gathering operations
cdv find **/*.ts --contains "console.log"        # Find TypeScript files with console.log
cdv run "git status" --instructions "summarize"  # Run command and process output
cdv web search "AI development trends"           # Search web and process results
cdv web get https://example.com                  # Process web page content

# Configuration operations
cdv config set app.preferredProvider azure-openai
cdv prompt create explain "Explain this code:"
cdv alias list
cdv mcp add tool-server --command /path/to/tool
```

## Discussion Topics

Let's discuss these key aspects to refine the specification:

1. **Command Structure**
   - Is moving CHATX to `cdv chat` the right approach?
   - Should all MDX functionality be at the top level, or should some be under subcommands?

2. **Naming**
   - Is "Cyco Dev" the right name? What does it convey?
   - Is `cdv` or `cycod` better as the executable name?

3. **Feature Integration**
   - How should options from both tools be harmonized (e.g., AI processing options)?
   - Are there redundant features that should be consolidated?

4. **Migration Path**
   - How should existing users of both tools transition?
   - Should there be compatibility modes or aliases?

5. **User Experience**
   - How do we ensure the combined tool remains intuitive?
   - What documentation structure would best support users?

How would you like to proceed with discussing these topics?
