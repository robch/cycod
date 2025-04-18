# Feature Integration in Cyco Dev

When combining MDX and CHATX into Cyco Dev, we need to carefully harmonize features and options to provide a consistent user experience. This document explores how we should approach feature integration and consolidation.

## Overlapping Areas and Harmonization Recommendations

### AI Processing Options

Both tools currently have ways to invoke AI processing, but with different intentions and approaches:

| Domain | Current Options | Purpose |
|--------|----------------|---------|
| **Context Gathering** (find, web, run) | `--instructions`, `--file-instructions`, `--page-instructions` | Processing gathered content |
| **Chat** | `--input`, `--inputs`, `--question`, etc. | Conversational interaction |
| **Missing** | No standard way to summarize chat results | Post-conversation processing |

**Recommendation:** Maintain distinct terminology that reflects the different purposes:

1. **For Context Gathering Commands**:
   - Keep `--instructions` - For providing directions on how to process gathered context
   - Keep `--file-instructions`/`--page-instructions` - For processing individual files/pages
   - Keep `--save-output` - For saving the processed result

2. **For Chat Commands**:
   - Add `--summary-instructions` - For providing directions on how to summarize/condense a chat
   - Add `--save-summary` - For saving the summarized chat output
   - Include a default summarization prompt if only `--save-summary` is provided

3. **Provider Framework**:
   - Use CHATX's robust provider and model management for all AI interactions
   - Maintain consistent parameter formats for model-specific options

### Output and History Management

Both tools manage output files and history, but with different purposes:

| Tool | Output Options | Purpose |
|------|---------------|---------|
| MDX | `--save-output`, `--save-file-output`, `--save-page-output` | Save processed results |
| CHATX | `--output-chat-history`, `--output-trajectory` | Record conversation history |
| **New** | `--save-summary` | Save condensed chat output |

**Recommendation:** Maintain purpose-specific terminology that aligns with user expectations:

- For context gathering: Keep `--save-output` for the processed result
- For chat interaction recording: Keep `--output-chat-history` and `--output-trajectory` 
- For chat summarization: Add `--save-summary`

### Command Execution and Tools

Both tools have ways to run commands and interact with external tools:

| MDX | CHATX | Recommendation |
|-----|-------|----------------|
| `mdx run` | Built-in functions | Consolidate into a single command execution framework |
| `--bash`, `--cmd`, `--powershell` | Shell selection in functions | Maintain shell selection options but use a unified implementation |
| N/A | MCP for tool integration | Use MCP as the framework for extensible tool integration |

**Recommendation:** Use a unified approach to command execution and tool integration:

- Build on MCP for extensible tool integration
- Maintain the user-friendly command execution interface from MDX
- Standardize shell selection options
- Create a consistent approach to displaying and processing command outputs

### Web Interaction

MDX has more developed web interaction capabilities:

| MDX | CHATX | Recommendation |
|-----|-------|----------------|
| `web search`, `web get` with options | `/search`, `/get` slash commands | Keep MDX's rich functionality as commands |
| Browser selection, API options | Limited | Maintain all web options with consistent naming |
| `--strip`, `--get`, etc. | N/A | Keep these specialized options where appropriate |

**Recommendation:** Keep the rich web interaction capabilities from MDX while ensuring:

- Consistent naming conventions for options
- Integration with CHATX's provider framework for AI processing
- Unified history and output management
- Maintaining specialized options where they add value

### Configuration and Profiles

CHATX has a more developed configuration system:

| MDX | CHATX | Recommendation |
|-----|-------|----------------|
| `--save-alias` | Rich alias support | Use CHATX's alias system for all commands |
| Limited config | Robust config with scopes | Use CHATX's configuration system for all settings |
| N/A | Profiles | Extend profiles to cover all functionality |

**Recommendation:** Build on CHATX's robust configuration system:

- Use the scoped configuration system (local/user/global) for all settings
- Extend aliases to work with all commands
- Ensure profiles can configure all aspects of the tool
- Maintain a consistent approach to storing and loading configurations

## Practical Examples

To illustrate how these harmonized features would work in practice:

### Context Gathering with Processing

```bash
# Find files and process with AI
cdv find **/*.md --instructions "Extract key technical concepts" --save-output concepts.md

# Search web and process results
cdv web search "yaml tutorial" --instructions "Create a beginner's guide" --save-output guide.md

# Run command and process output
cdv run "git log --oneline" --instructions "Summarize recent changes" --save-output changes.md
```

### Chat with Summarization

```bash
# Have a conversation and summarize the outcome
cdv chat --inputs @questions.txt --summary-instructions "List the main decisions made" --save-summary decisions.md

# Continue a previous chat and create a summary
cdv chat --continue --summary-instructions "Extract action items" --save-summary actions.md

# Load a chat history and create a summary (with default summarization)
cdv chat --input-chat-history meeting.jsonl --save-summary minutes.md
```

### Tracking Conversation History

```bash
# Save complete chat history in machine-readable format
cdv chat --question "Design a database schema" --output-chat-history schema-design.jsonl

# Save human-readable conversation trajectory
cdv chat --inputs @requirements.txt --output-trajectory project-planning.md
```

## Implementation Strategy

To implement these harmonizations effectively:

1. **Respect Domain-Specific Workflows**: Maintain separate terminology for context gathering vs. chat operations where appropriate.

2. **Start with the Provider Framework**: Use CHATX's provider management as the foundation for all AI interactions.

3. **Implement New Processing Options**: Add the new chat summarization capabilities (`--summary-instructions`, `--save-summary`).

4. **Create Abstraction Layers**: Develop common services for history management, output handling, and command execution that all commands can use.

5. **Extend Configuration System**: Ensure the configuration system can handle settings for all functionality.

6. **Document Consistently**: Create clear documentation that explains the purpose-specific terminology.

## Conclusion

By carefully harmonizing features from both tools while respecting their distinct purposes, Cyco Dev can offer a powerful user experience that leverages the strengths of both MDX and CHATX. Rather than forcing complete terminology unification, we've chosen to maintain purpose-specific terminology where it makes sense, creating a more intuitive experience that aligns with how users think about different operations.