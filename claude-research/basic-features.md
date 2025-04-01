# Claude Code Basic Features

Claude Code is a terminal-based AI coding assistant developed by Anthropic that runs directly in your command-line interface. It leverages the Claude 3.7 Sonnet model to understand your codebase and help you perform various coding tasks through natural language commands.

## Core Features

### Terminal Integration
- **Direct CLI Access**: Runs as a terminal application, eliminating the need for separate browser windows or UIs
- **Command-Line Interface**: Accessed through a simple `claude` command in your terminal
- **Persistent Context**: Maintains awareness of your entire project structure

### Code Understanding
- **Codebase Analysis**: Scans and understands code architecture without manually uploading files
- **Cross-File Context**: Comprehends relationships between files and components
- **Language Support**: Works with most major programming languages

### File Operations
- **File Editing**: Directly modifies files in your codebase
- **Read Access**: Views and analyzes files without modification permissions
- **Search Capabilities**: Finds files based on pattern matching or content searching

### Git Integration
- **Basic Commands**: Can execute git commands like commit, push, pull
- **Conflict Resolution**: Helps resolve merge conflicts
- **PR Creation**: Can generate pull requests with appropriate descriptions

### Execution Capabilities
- **Command Execution**: Runs shell commands in your environment (with user permission)
- **Test Execution**: Can run and interpret test results
- **Build Processes**: Can trigger and monitor build processes

### Reasoning and Planning
- **Extended Thinking**: Can tackle complex coding problems with structured reasoning
- **Multi-Step Planning**: Breaks down complex tasks into manageable steps
- **Error Explanation**: Helps diagnose and explain errors in your code

### Security Features
- **Permission System**: Implements a tiered approval system for sensitive operations
- **Direct API Connection**: Connects directly to Anthropic API without intermediate servers
- **Controlled Access**: Only accesses files and runs commands with explicit permission

### Memory Management
- **Project Memory**: Stores project-specific information in CLAUDE.md files
- **User Memory**: Maintains user preferences across different projects
- **Conversation Compacting**: Automatically compacts conversation when context gets too large

## Installation and Setup

```bash
# Install via npm
npm install -g @anthropic-ai/claude-code

# Navigate to your project
cd your-project

# Start Claude Code
claude
```

## Basic Usage Commands

- **General Help**: `/help`
- **Configure Settings**: `/config`
- **Clear Conversation**: `/clear`
- **Compact Context**: `/compact`
- **View Token Usage**: `/cost`
- **Health Check**: `/doctor`
- **Create Project Guide**: `/init`
