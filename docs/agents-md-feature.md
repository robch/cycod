# AGENTS.md Support in cycod CLI

The cycod CLI now supports the AGENTS.md standard for AI coding agents. This document explains how to use this feature and configure it to your needs.

## What is AGENTS.md?

AGENTS.md is an open standard for providing project-specific context to AI coding agents. It serves as a communication bridge between development teams and AI tools, helping the AI understand project requirements, conventions, and structure.

## How cycod Uses AGENTS.md

When you run the cycod CLI in a directory containing an AGENTS.md file (or one of the supported alternative filenames), the content of that file is automatically loaded as a template variable. This variable is then available in the system prompt template.

The system prompt template includes a conditional section that looks like this:
```
{{if !ISEMPTY("{agents.md}")}}
## Project Context from {agents.file}

{agents.md}

---
{{endif}}
```

This means the AGENTS.md content will only be included if an AGENTS.md file is found (using the ISEMPTY function to check), and it will be labeled with the actual file name that was found.

The cycod CLI will search for AGENTS.md files in the following order:
1. The current directory
2. Parent directories (if nested mode is enabled)

## Available Template Variables

When an AGENTS.md file is found, the following template variables are set:

- `{agents.md}` - The content of the AGENTS.md file
- `{agents.file}` - The filename of the found file (e.g., "AGENTS.md", "CLAUDE.md")
- `{agents.path}` - The full path to the found file

## Supported File Names

By default, cycod looks for the following files (in order of priority):
- AGENTS.md
- CLAUDE.md
- GEMINI.md
- .cursorrules
- AGENT.md
- agent.md
- .windsurfrules

## Configuration

You can configure AGENTS.md support through settings in your cycod configuration:

```
# Enable or disable AGENTS.md support
App.Agents.Enabled: true

# Define which files to look for (in order of priority)
App.Agents.FileNames: 
  - AGENTS.md
  - CLAUDE.md
  - GEMINI.md
  - .cursorrules
  - AGENT.md
  - agent.md
  - .windsurfrules

# Enable or disable searching in parent directories
App.Agents.Nested.Enabled: true

# Enable or disable processing @import syntax
App.Agents.ProcessImports: true
```

These settings can be configured in any of the cycod configuration scopes (local, user, or global).

## Command Line Options

You can also configure AGENTS.md support via command-line options:

```
--agents-enabled=true|false
--agents-file-names=AGENTS.md,CLAUDE.md
--agents-nested-enabled=true|false
--agents-process-imports=true|false
```

## Advanced Features

### @import Syntax

cycod supports the @import syntax in AGENTS.md files, which allows you to include content from other files:

```
Consult @CONTRIBUTING.md for project development guidelines.
```

When the AI encounters this line, it will replace `@CONTRIBUTING.md` with the actual content of the CONTRIBUTING.md file.

### Nested AGENTS.md Files

If you have a monorepo or a complex project structure, you can place different AGENTS.md files in different subdirectories. When you run cycod in a subdirectory, it will use the closest AGENTS.md file in the directory hierarchy.

## Checking AGENTS.md Status

To see which AGENTS.md file is being used, run the `/help` command in a chat session. It will show you the current status of AGENTS.md support, including which file is being used and how to reference it in templates.

## Examples

- Place an AGENTS.md file in your project root with general project guidelines
- Place specific AGENTS.md files in subdirectories for module-specific guidelines
- Use @import syntax to include detailed information from other documentation files