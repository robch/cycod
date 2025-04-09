---
hide:
- navigation
- toc
---
# WELCOME TO CHATX

CHATX is an AI-powered CLI tool that brings the power of large language models to your command line interface. With CHATX, you can chat with AI models, create and manage custom prompts, save conversation history, and much more.

??? abstract "Why use CHATX ..."

    <div class="grid cards" markdown>

    -   **CHATX FEATURES**  

        :octicons-terminal-16: Chat with AI directly from your terminal.  
        :material-chat-processing: Save and resume conversations with ease.  
        :material-translate: Use custom prompts for common tasks.  
        :material-cog-refresh: Configure multiple AI providers.  

    -   **MULTIPLE AI PROVIDERS**  

        :material-microsoft-azure: Azure OpenAI API!  
        :material-openai: OpenAI API!  
        :material-github: GitHub Copilot!  
        :material-account-group: Custom model providers with MCP!  

    -  **FLEXIBLE CONFIGURATION**  

        :fontawesome-solid-wrench: Local, user, and global configuration scopes.  
        :material-profile-detail: Custom profiles for different use cases.  
        :material-alias: Create aliases for common commands.  
        :material-message-text-outline: Manage custom prompts for quick use.  

    </div>

<div class="grid cards" markdown>

-   :material-clock-fast:{ .lg .middle } __Get Started__

    ---

    **Installation and Setup**  
    [:material-download: CHATX Installation](/getting-started.md)  
    [:material-openai:{ .med } OpenAI Setup](/providers/openai.md)  
    [:material-microsoft-azure:{ .med } Azure OpenAI Setup](/providers/azure-openai.md)  
    [:material-github:{ .med } GitHub Copilot Setup](/providers/github-copilot.md)  

-   :material-console-line:{ .lg .middle } __Basic Usage__

    ---

    **Core Functionality**  
    [:material-chat: Chat Basics](/usage/basics.md)  
    [:material-history: Chat History](/usage/chat-history.md)  
    [:material-cog: Configuration](/usage/configuration.md)  

-   :material-hammer-wrench:{ .lg .middle } __Advanced Features__

    ---

    **Power User Tools**  
    [:material-alias: Aliases](/advanced/aliases.md)  
    [:material-message-text: Custom Prompts](/advanced/prompts.md)  
    [:material-profile-details: Profiles](/advanced/profiles.md)  
    [:material-api: Model Context Protocol (MCP)](/advanced/mcp.md)  

</div>

## Quick Examples

```bash title="Ask a simple question"
chatx --question "What time is it?"
```

```bash title="Start an interactive chat"
chatx --interactive
```

```bash title="Continue your most recent chat"
chatx --continue
```

```bash title="Use a system instruction"
chatx --add-system-prompt "Always answer in French." --question "What is the capital of Spain?"
```

```bash title="Create and use an alias"
# Create an alias for Python help
chatx --add-system-prompt "You are a Python expert. Always provide code examples." --save-alias python-help

# Use the alias
chatx --python-help --question "How do I read a CSV file in Python?"
```