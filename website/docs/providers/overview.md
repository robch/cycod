---
hide:
- toc
icon: material/server-network
---

# Provider Overview
??? tip "Prerequisites"

    Before you begin:
    
    1. Make sure you have [installed CHATX](/getting-started.md)


ChatX supports multiple AI providers, allowing you to choose the service that best fits your needs, requirements, and budget.

## Available Providers

ChatX currently supports the following AI providers:

| Provider | Description | Option Flag | Configuration Setting |
|----------|-------------|-------------|------------------------|
| [GitHub Copilot](github-copilot.md) | GitHub's AI coding assistant service | `--use-copilot` | `app.preferredProvider=copilot` |
| [Azure OpenAI](azure-openai.md) | Microsoft's Azure-based OpenAI service | `--use-azure-openai` or `--use-azure` | `app.preferredProvider=azure-openai` |
| [OpenAI](openai.md) | Direct integration with OpenAI API | `--use-openai` | `app.preferredProvider=openai` |

