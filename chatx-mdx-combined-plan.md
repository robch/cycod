# CycoDev Integration Plan

## Vision

CycoDev (`cycod`) will unify the complementary capabilities of MDX and CHATX into a single, cohesive tool that empowers developers to leverage AI throughout their workflow. By combining MDX's powerful context gathering with CHATX's robust chat capabilities, CycoDev will create a seamless experience for:

1. Gathering context from files, commands, and the web
2. Interacting with AI through chat
3. Processing and transforming information with AI assistance
4. Managing configurations, prompts, and tool integrations

The unified tool will maintain the distinct workflows that make each original tool powerful while creating a consistent interface, terminology, and user experience.

## Core Principles

1. **Purpose-Specific Terminology**: Use terms that reflect the different mental models of context gathering vs. chat operations
2. **Command Consistency**: Provide a predictable command structure across all operations
3. **Provider Unification**: Use a single, consistent framework for AI provider management
4. **Configuration Coherence**: Build on a unified configuration system with scope awareness
5. **Progressive Complexity**: Simple operations should be simple; complexity should only be added as needed
6. **Backward Compatibility**: Support migration paths for users of existing tools

## Command Structure

CycoDev will organize commands according to their function:

```
cycod chat                    # Chat with AI (interactive or non-interactive)
cycod find                    # Find files and process with AI
cycod run                     # Run commands and process output with AI
cycod web search              # Search the web and process results with AI
cycod web get                 # Get web page content and process with AI

cycod config                  # Configuration management
cycod alias                   # Command alias management
cycod prompt                  # Prompt template management
cycod mcp                     # Model Context Protocol server management
cycod github                  # GitHub integration
cycod help                    # Help system
```

## Feature-by-Feature Plan

### 1. Command Structure Reorganization

**Motivation**:
- Creating a consistent command structure improves discoverability and learnability
- Moving chat to a subcommand clarifies that the tool serves multiple purposes
- Keeping `find` and `run` at the top level acknowledges their distinct roles
- Grouping web commands reflects their related nature

**Work Items**:
- Move CHATX's default behavior to `cycod chat`
- Implement `cycod find` for file finding/processing operations
- Implement `cycod run` for command execution/processing
- Create `cycod web` subcommand group with `search` and `get` operations
- Ensure all configuration commands (`config`, `alias`, `prompt`, `mcp`) maintain their current structure
- Make `cycod` with no arguments default to `cycod chat` for smoother transition
- Create built-in aliases for common transitions (e.g., `cycod c` → `cycod chat`)

### 2. Chat Operations

**Motivation**:
- Chat interactions have different usage patterns and needs than context gathering
- Users need both interactive and non-interactive chat capabilities
- Adding summarization capabilities enhances chat functionality
- Maintaining consistent history management is critical for ongoing conversations

**Work Items**:
- Implement `cycod chat` as the primary interface for AI conversation
- Preserve all current CHATX chat functionality:
  - Interactive mode with slash commands
  - Non-interactive mode with `--input`/`--inputs`/`--question`/etc.
  - Provider selection and configuration
- Add new summarization capabilities:
  - `--summary-instructions` - For providing directions on how to summarize/condense a chat
  - `--save-summary` - For saving the summarized chat output
  - Default summarization behavior when only `--save-summary` is provided
- Maintain history management options:
  - `--continue` - Continue most recent chat
  - `--chat-history` - Load from and save to same file
  - `--input-chat-history` - Load from specific file
  - `--output-chat-history` - Save to specific file
  - `--output-trajectory` - Save human-readable history
- Ensure slash commands continue to integrate with context gathering operations

### 3. Context Gathering Operations

**Motivation**:
- Context gathering operations have different workflows than chat
- Users need specialized options for different context sources
- Each context type (files, commands, web) has unique requirements
- Consistent processing patterns improve user experience

**Work Items**:
- **File Finding (`cycod find`)**:
  - Implement core file finding functionality from MDX
  - Maintain file filtering options (`--contains`, `--file-contains`, etc.)
  - Preserve line filtering options (`--lines`, `--lines-after`, etc.)
  - Support file pattern matching and exclusions
  
- **Command Execution (`cycod run`)**:
  - Implement command execution functionality from MDX
  - Support different shells (`--bash`, `--cmd`, `--powershell`)
  - Enable multi-step command execution
  
- **Web Search (`cycod web search`)**:
  - Implement web search functionality from MDX
  - Maintain search engine options (`--google`, `--bing`, etc.)
  - Support API-based search (`--google-api`, `--bing-api`)
  - Preserve web search filtering options
  
- **Web Content Retrieval (`cycod web get`)**:
  - Implement web page retrieval from MDX
  - Support browser options and interaction modes
  - Maintain HTML processing options (`--strip`)

### 4. AI Processing Model

**Motivation**:
- Different operations have different AI processing needs
- Maintaining domain-specific terminology improves clarity
- A unified provider framework simplifies configuration
- Consistent parameter formats improve learnability

**Work Items**:
- **For Context Gathering**:
  - Preserve `--instructions` for final processing directions
  - Maintain `--file-instructions`/`--page-instructions` for per-item processing
  - Implement consistent multi-step instruction handling
  
- **For Chat Operations**:
  - Maintain `--input`/`--inputs` for conversational messages
  - Add `--summary-instructions` for post-conversation processing
  
- **Provider Framework**:
  - Implement unified provider management based on CHATX's system
  - Support all current providers (OpenAI, Azure OpenAI, GitHub Copilot)
  - Ensure consistent parameter formats across all operations
  - Build common abstractions for token management

### 5. Output and History Management

**Motivation**:
- Different operations produce different types of output
- Domain-specific terminology clarifies the purpose of each output
- Consistent patterns within domains improve user experience
- Supporting backward compatibility eases migration

**Work Items**:
- **For Context Gathering**:
  - Maintain `--save-output` for final processing results
  - Preserve `--save-file-output` for per-file results
  - Keep `--save-page-output` for per-page results
  
- **For Chat Operations**:
  - Keep `--output-chat-history` for JSONL conversation recording
  - Maintain `--output-trajectory` for human-readable recording
  - Add `--save-summary` for saving condensed chat output
  
- **Support System**:
  - Implement backward compatibility for renamed options
  - Create a consistent templating system for output file paths
  - Ensure proper token management across all outputs

### 6. Configuration and Profile System

**Motivation**:
- A unified configuration system simplifies user experience
- Scope awareness (local/user/global) provides flexibility
- Profile support enables switching between configurations
- Alias support streamlines common operations

**Work Items**:
- Build on CHATX's robust configuration system:
  - Maintain scope awareness (local/user/global)
  - Preserve configuration commands and structure
- Extend the profile system to cover all functionality:
  - Enable profiles to configure both chat and context gathering options
  - Support provider selection via profiles
- Enhance the alias system:
  - Extend aliases to work with all commands
  - Maintain scope-aware alias storage

### 7. Extensibility and Integration

**Motivation**:
- Tool extensibility is crucial for evolving workflows
- MCP provides a robust framework for integration
- Consistent integration patterns improve usability
- Building on existing extensibility reduces development effort

**Work Items**:
- Build on the MCP framework for tool integration:
  - Extend MCP to support context gathering operations
  - Ensure consistent function calling across operations
- Implement slash command support in chat:
  - Maintain existing integrations with context gathering
  - Ensure consistent behavior between interactive and non-interactive modes
- Create APIs for third-party extensions:
  - Develop a plugin architecture based on current integration points
  - Document extension points and interfaces

## Development Priorities

While specific timelines are outside the scope of this plan, the following prioritization is recommended:

1. Command structure reorganization and basic functionality
2. Core feature parity with existing tools
3. Output and history management unification
4. New summarization capabilities
5. Configuration and profile unification
6. Extensibility enhancements

This phased approach ensures that critical functionality is available early while allowing for iterative enhancement.

## Conclusion

The CycoDev integration plan creates a clear roadmap for unifying MDX and CHATX into a powerful, cohesive tool. By respecting the different workflows and mental models of context gathering and chat operations, while providing a consistent command structure and user experience, CycoDev will offer developers a comprehensive platform for AI-assisted development.

The plan maintains the strengths of both original tools while creating new capabilities through their integration. The unified tool will simplify installation, configuration, and learning, while enabling powerful new workflows that combine context gathering and chat operations seamlessly.