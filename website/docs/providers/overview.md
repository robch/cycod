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
| [Amazon Bedrock](bedrock.md) | Access to various foundation models through AWS Bedrock | `--use-bedrock` | `app.preferredProvider=bedrock` |
| [Anthropic](anthropic.md) | Direct integration with Anthropic Claude models | `--use-anthropic` or `--use-claude` | `app.preferredProvider=anthropic` |
| [Azure OpenAI](azure-openai.md) | Microsoft's Azure-based OpenAI service | `--use-azure` or `--use-azure-openai` | `app.preferredProvider=azure-openai` |
| [Google Gemini](gemini.md) | Integration with Google Gemini models | `--use-gemini` | `app.preferredProvider=gemini` |
| [OpenAI](openai.md) | Direct integration with OpenAI API | `--use-openai` | `app.preferredProvider=openai` |