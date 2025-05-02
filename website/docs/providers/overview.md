---
hide:
- toc
icon: material/server-network
---

CycoD supports multiple AI providers, allowing you to choose the service that best fits your needs.

## Available Providers

CycoD currently supports the following AI providers:

| Provider | Description | Option Flag | Configuration Setting |
|----------|-------------|-------------|------------------------|
| [GitHub Copilot](github-copilot.md) | GitHub's AI coding assistant service | `--use-copilot` | `app.preferredProvider=copilot` |
| [Azure OpenAI](azure-openai.md) | Microsoft's Azure-based OpenAI service | `--use-azure` or `--use-azure-openai` | `app.preferredProvider=azure-openai` |
| [OpenAI](openai.md) | Direct integration with OpenAI API | `--use-openai` | `app.preferredProvider=openai` |

