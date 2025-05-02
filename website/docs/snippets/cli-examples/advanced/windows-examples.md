### Advanced Provider Options

Specify a particular model and provider:

```bash
# Windows PowerShell
cycod --use-openai --openai-chat-model-name gpt-4o --question "What new features are in GPT-4o?"
```

### Using System Prompts

Add a system prompt to customize the AI's behavior:

```bash
# Windows PowerShell
cycod --system-prompt "You are a helpful coding assistant specialized in Python" --question "How do I create a web server in Python?"
```

### Creating and Using Profiles

Create a profile file for frequently used settings:

```bash
# Windows PowerShell
# Create a file named my-profile.yaml in %USERPROFILE%\.cycod\profiles\ with the following content:
# app:
#   preferredProvider: "openai"
# openai:
#   chatModelName: "gpt-4o"
# prompts:
#   system:
#     - "You are a helpful assistant"

cycod --profile my-profile --question "What's the weather like today?"
```

### Using Templates and Variables

Use templates with variables for flexible prompts:

```bash
# Windows PowerShell
cycod --question "Translate this to {{language}}: {{text}}" --var language=French --var text="Hello, how are you?"
```