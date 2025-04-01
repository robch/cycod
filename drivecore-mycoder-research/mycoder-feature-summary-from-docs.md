I'll provide a comprehensive summary of all the key features of MyCoder, with examples where appropriate. I'll be as complete and verbose as possible to give you a thorough understanding of MyCoder's capabilities.

# MyCoder: A Comprehensive Feature Overview

MyCoder is an AI-powered coding assistant designed to help developers accomplish a wide range of software development tasks through natural language commands. Based on the documentation provided, here's a detailed breakdown of its capabilities, features, and use cases.

## Core Functionality

### AI Integration

MyCoder leverages advanced AI models to understand natural language prompts and transform them into actionable coding tasks. It's designed as a command-line tool that integrates seamlessly with existing development workflows.

### Multi-Provider Support

One of MyCoder's standout features is its support for multiple AI providers, giving developers flexibility in choosing which AI model powers their coding assistant:

- **Anthropic (Claude)**
  - Recommended model: `claude-3-7-sonnet-20250219`
  - Strong reasoning capabilities with 200K context window
  - Excellent tool-calling abilities
  - Models include: claude-3-7-sonnet, claude-3-5-sonnet, claude-3-opus, claude-3-haiku

- **OpenAI**
  - Recommended model: `gpt-4o`
  - Support for models like gpt-4-turbo, gpt-4, gpt-3.5-turbo
  - Robust function calling capabilities

- **Mistral AI**
  - Models include mistral-large and mistral-medium

- **xAI/Grok**
  - Support for grok-1

- **Ollama**
  - Enables running models locally for privacy and offline use
  - Requires models with tool calling support (e.g., `medragondot/Sky-T1-32B-Preview:latest`)

Example configuration for selecting a provider:
```javascript
// mycoder.config.js
export default {
  provider: 'openai',
  model: 'gpt-4o',
};
```

### Token Caching

MyCoder implements token caching, particularly for Anthropic's Claude models, to optimize performance and reduce API costs:

```javascript
export default {
  provider: 'anthropic',
  model: 'claude-3-7-sonnet-20250219',
  tokenCache: true, // Enable token caching (default is true)
};
```

## GitHub Integration

### GitHub Mode

MyCoder's GitHub mode enables seamless integration with GitHub workflows, transforming it from a simple coding assistant into a fully-fledged development partner:

```bash
# Enable GitHub mode for a single session
mycoder --githubMode true "Fix the bug described in issue #42"

# Or set it as default in configuration
# mycoder.config.js
export default {
  githubMode: true,
};
```

When GitHub mode is enabled, MyCoder can:

- **Work with GitHub Issues**: Read existing issues, create new ones, and mark them as completed
  ```bash
  # Work on an existing issue
  mycoder "Fix issue #42"
  
  # Create a new issue
  mycoder "Create an issue for the memory leak in the cache system"
  ```

- **Create Feature Branches**: Automatically create appropriately named branches
  ```
  # Example branch names
  fix/issue-42-memory-leak
  feature/add-dark-mode
  refactor/improve-error-handling
  ```

- **Make Commits**: Create commits with detailed, descriptive messages
  ```
  # Example commit message
  Fix memory leak in cache system (#42)
  
  - Added proper cleanup of cached objects in the dispose method
  - Implemented weak references for large objects
  - Added unit tests to verify memory is properly released
  ```

- **Create Pull Requests**: Generate comprehensive PRs that include:
  - Descriptive titles
  - Detailed descriptions of changes
  - References to original issues
  - Test results and validation steps

### GitHub Actions Integration

MyCoder can be integrated into GitHub workflows through GitHub Actions, particularly via the `issue-comment.yml` action that allows triggering MyCoder from PR or issue comments:

```yaml
name: MyCoder Issue Comment Action

on:
  issue_comment:
    types: [created]

permissions:
  contents: write
  issues: write
  pull-requests: write
  discussions: write
  statuses: write
  checks: write
  actions: read
  packages: read

jobs:
  process-comment:
    runs-on: ubuntu-latest
    if: |
      contains(github.event.comment.body, '/mycoder') && 
      contains(fromJson('["trusted-user1", "trusted-user2"]'), github.event.comment.user.login)
    steps:
      # Setup steps...
      
      - env:
          ANTHROPIC_API_KEY: ${{ secrets.ANTHROPIC_API_KEY }}
        run: |
          mycoder --userWarning false --upgradeCheck false --githubMode true --userPrompt false "On issue #${{ github.event.issue.number }} in comment ${{ steps.extract-prompt.outputs.comment_url }} the user invoked the mycoder CLI via /mycoder. Can you try to do what they requested or if it is unclear, respond with a comment to that effect to encourage them to be more clear."
```

This allows developers to trigger MyCoder with comments like:
```
/mycoder Please analyze this issue and suggest a solution.
```

## Configuration System

MyCoder includes a robust configuration system that allows for extensive customization:

### Configuration File

Create a `mycoder.config.js` file in your project root:

```javascript
// mycoder.config.js
export default {
  // GitHub integration
  githubMode: true,

  // Browser settings
  headless: true,
  userSession: false,
  pageFilter: 'readability', // 'simple', 'none', or 'readability'

  // Model settings
  provider: 'anthropic',
  model: 'claude-3-7-sonnet-20250219',
  maxTokens: 4096,
  temperature: 0.7,

  // Custom settings
  customPrompt: 'Always write TypeScript code with proper type annotations.',
  profile: false,
  tokenCache: true,
};
```

### Command-Line Configuration

Configuration can also be managed through command-line commands:

```bash
# List all configuration values
mycoder config list

# Get a specific configuration value
mycoder config get modelProvider

# Set a configuration value
mycoder config set modelProvider openai
mycoder config set modelName gpt-4o
```

### Configuration Options

MyCoder supports numerous configuration options across different categories:

**AI Model Selection**
```javascript
export default {
  provider: 'openai',
  model: 'gpt-4o',
};
```

**Logging and Debugging**
```javascript
export default {
  logLevel: 'verbose', // 'debug', 'verbose', 'info', 'warn', 'error'
  tokenUsage: true,
  profile: true,
};
```

**Browser Integration**
```javascript
export default {
  headless: false, // Show browser windows
  userSession: true, // Use existing browser session
  pageFilter: 'readability', // Better web content parsing
};
```

**Behavior Customization**
```javascript
export default {
  customPrompt: "Always use TypeScript when writing code. Prefer functional programming patterns.",
  githubMode: true,
};
```

## Custom System Prompts

MyCoder allows developers to customize the system prompt used to guide the AI's behavior:

```bash
# Set a custom prompt to prefer TypeScript
mycoder config set customPrompt "Always use TypeScript when writing code. Prefer functional programming patterns when possible."
```

This enables creating specialized versions of MyCoder tailored to specific team standards, technologies, or project requirements.

## Browser Integration

MyCoder can leverage web browsers for research and accessing online documentation:

```javascript
export default {
  // Show browser windows instead of running in headless mode
  headless: false,
  
  // Use existing browser session (cookies, login state)
  userSession: true,
  
  // Method to process webpage content
  // Options: 'simple', 'none', 'readability'
  pageFilter: 'readability',
};
```

The browser integration works across operating systems:
- **Windows**: Chrome/Edge
- **macOS**: Chrome/Safari
- **Linux**: Chromium/Chrome/Firefox

## Performance Profiling

MyCoder includes performance profiling to help diagnose startup times and identify bottlenecks:

```bash
# Enable profiling for any command
mycoder --profile "Fix the build errors"

# Or use with other commands
mycoder --profile --interactive
```

Sample profiling output:
```
📊 Performance Profile:
=======================
Module initialization: 10.12ms (10.12ms)
After imports: 150.34ms (140.22ms)
Main function start: 269.99ms (119.65ms)
After dotenv config: 270.10ms (0.11ms)
After Sentry init: 297.57ms (27.48ms)
Before package.json load: 297.57ms (0.00ms)
After package.json load: 297.78ms (0.21ms)
Before yargs setup: 297.78ms (0.00ms)
After yargs setup: 401.45ms (103.67ms)
Total startup time: 401.45ms
=======================
```

## Model Context Protocol (MCP) Support

MyCoder supports the Model Context Protocol (MCP), enabling access to external context sources and tools during conversations:

```javascript
// mycoder.config.js
export default {
  // MCP configuration
  mcp: {
    // MCP Servers to connect to
    servers: [
      {
        name: 'company-docs',
        url: 'https://mcp.example.com/docs',
        // Optional authentication
        auth: {
          type: 'bearer',
          token: process.env.MCP_SERVER_TOKEN,
        },
      },
    ],

    // Optional: Default context resources to load
    defaultResources: ['company-docs://api/reference'],
  },
};
```

This allows MyCoder to:
- Access documentation and knowledge bases
- Retrieve up-to-date information from external systems
- Use specialized tools and services provided by MCP servers

## Use Cases and Examples

The documentation shows that MyCoder can handle a wide variety of development tasks:

### Feature Implementation

```bash
# Implementing a configuration system
mycoder "Create a persistent configuration system that allows users to set default values for command line options"

# Adding multi-provider support
mycoder "Add support for OpenAI o3 mini and GPT-4o models"
```

### Bug Fixing

```bash
# Cross-platform compatibility issues
mycoder "Replace shell commands with Node.js APIs for cross-platform compatibility"

# Fixing specific bugs
mycoder "Fix newline escape characters in GitHub messages"
```

### Code Refactoring

```bash
# Structural refactoring
mycoder "Refactor toolAgent structure"

# Removing deprecated code
mycoder "Remove deprecated toolAgent.ts file and fix direct imports"
```

### Documentation

```bash
# Updating documentation
mycoder "Update documentation to include new configuration options and multi-provider support"
```

### Project Management

```bash
# Creating multiple related issues
mycoder "Can you create a Github issue for removing the base tsconfig.json file and instead just using fully defined tsconfig.json files in each package in both the packages folder and the services folder? Complex tsconfig.json strategies with shared settings can introduce a lot of unnecessary complexity.

Can you also make a Github issue for combining all packages into a single packages folder rather than having them split between packages and services? There isn't enough packages to warrant the split here.

Third can you create an issue for updating the root README.md so that it describes the project, what each package does and the main ways developers (and agentic agents) should interact with it?"

# Implementing multiple issues in a single PR
mycoder "Can you implement github issue #31 and also #30 and do a combined PR back to Github?"
```

### PR Review and Analysis

```bash
# Reviewing a PR for potential duplication
mycoder "In the current PR #45, which fixes issue #44 and it is also currently checked out as the current branch, there isn't duplication of the checks are there? In your writeup you say that \"added pre-push hook with the same validation\". It seems that we have both a pre-commit hook and a pre-push hook that do the same thing? Won't that slow things down?"
```

### DevOps and CI/CD

```bash
# Setting up GitHub Actions workflows
mycoder "Can you implement the recommendations 2 and 3 from issue #44. You can look at the CI Github Actions workflow in ../mycoder-websites/.github as guide to setting up a similar CI action that validates the build and runs lint, etc for this repo."

# Investigating build failures
mycoder "It seems that the latest GitHub action failed, can you investigate it and make a GitHub issue with the problem and then push a PR that fixes the issue? Please wait for the new GitHub action to complete before declaring success."
```

### Architectural Changes

```bash
# Refactoring an SDK implementation
mycoder "Recently this project was converted from using the Anthropic SDK directly to using the Vercel AI SDK. Since then it has created reliability problems. That change was made 4 days ago in this PR: https://github.com/drivecore/mycoder/pull/55/files

And it was built upon by adding support for ollama, grok/xai and openai in subsequent PRs. I would like to back out the adoption of the Vercel AI SDK, both the 'ai' npm library as well as the '@ai-sdk' npm libraries and thus also back out support for Ollama, OpenAI and Grok.

In the future I will add back these but the Vercel AI SDK is not working well. While we back this out I would like to, as we re-implement using the Anthropic SDK, I would like to keep some level of abstraction around the specific LLM..."
```

## Best Practices for Using MyCoder

The documentation outlines several effective techniques for using MyCoder:

### Using GitHub as External Memory

- **Maintains Context:** GitHub issues, PRs, and commits create a persistent record
- **Tracks Evolution:** Changes over time are documented and accessible
- **Structures Work:** Breaking work into issues and PRs creates natural task boundaries

Techniques:
1. **Create Issues for Tasks:** Break work into discrete issues that MyCoder can reference by number
2. **Reference Issue Numbers:** Use `#issue-number` in prompts
3. **Link Related Work:** Reference previous PRs or issues
4. **Document Decisions:** Use issue comments to record decisions and rationale

### Clear Task Definition

- Explicitly state what you expect (e.g., "create a GitHub issue and PR")
- Explain why you want something done a certain way
- Define what's in scope and out of scope
- Point to existing code or documentation as examples

### Breaking Down Complex Tasks

- Break large features into multiple GitHub issues
- Ask MyCoder to implement one piece at a time
- Check each implementation before moving forward
- Start with core functionality, then add refinements

### Providing References and Examples

- Point to similar implementations in your codebase
- Be specific about where to find reference implementations
- Call attention to specific aspects of the examples
- Reference patterns from other repositories when applicable

### Iterative Refinement

- Begin with a basic implementation
- Provide specific feedback on particular aspects
- Recognize what's working well
- Use successful patterns as references for future work

## Installation and Setup

MyCoder is installed via npm:

```bash
# Global installation
npm install -g mycoder

# Or use directly with npx
npx mycoder "Your prompt here"
```

### Prerequisites

MyCoder requires:
- **Node.js version 20.0.0 or higher**
- **Git** for version control
- **GitHub CLI** for GitHub integration

### API Keys Setup

```bash
# For Anthropic Claude
export ANTHROPIC_API_KEY=your-api-key

# For OpenAI
export OPENAI_API_KEY=your-api-key

# For Mistral AI
export MISTRAL_API_KEY=your-api-key

# For xAI/Grok
export XAI_API_KEY=your-api-key
```

## Self-Improving Capabilities

One of the most interesting aspects of MyCoder is how it's used to improve itself. According to the blog post "How We Use MyCoder to Build MyCoder":

- MyCoder has addressed 20+ issues and merged 15+ pull requests to its own codebase
- It implements complete features based on high-level descriptions
- It diagnoses and fixes bugs in its own code
- It refactors its own code structure
- It generates and updates its own documentation

Examples of MyCoder improving itself:
```bash
# Feature implementation
mycoder "Create a persistent configuration system that allows users to set default values for command line options"

# Bug fixing
mycoder "Replace shell commands with Node.js APIs for cross-platform compatibility"

# Code refactoring
mycoder "Refactor toolAgent structure"

# Documentation
mycoder "Update documentation to include new configuration options and multi-provider support"
```

## Conclusion

MyCoder represents a powerful AI-powered coding assistant with extensive capabilities across multiple development domains. Its ability to integrate with GitHub workflows, support multiple AI providers, and adapt to specific project requirements makes it a versatile tool for developers.

The documentation shows that MyCoder excels at:
- Implementing features and fixing bugs
- Managing projects through GitHub issues and PRs
- Reviewing and analyzing code
- Setting up DevOps configurations and CI/CD pipelines
- Refactoring and improving codebases
- Creating and maintaining documentation

With its configurable nature and support for various AI models, MyCoder can be tailored to fit different development workflows, team preferences, and project requirements, making it a highly adaptable tool for modern software development.

