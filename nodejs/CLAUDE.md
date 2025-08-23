# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TypeScript bridge application that provides a rich terminal interface for interacting with a C# CLI application (`cycod`). The app uses React/Ink for the UI, node-pty for process management, and implements a streaming chat interface with ANSI escape sequence processing.

## Common Development Commands

### Build and Run
```bash
npm run build      # Compile TypeScript to JavaScript
npm run dev        # Run with auto-reload (tsx watch)
npm start          # Start the compiled application
npm run clean      # Remove dist directory
npm run typecheck  # Type check without emitting files
```

### Running the Application
```bash
# Interactive mode (chat interface)
npm start interactive
npm start interactive -- --path /path/to/cycod

# Single command mode
npm start run "your command here"
npm start run "your command" -- --path /path/to/cycod
```

## Architecture

### Core Components

**Bridge Layer** (`src/bridge/`)
- `csharp-session.ts`: Manages PTY session with C# process, handles process lifecycle and events
- `output-parser.ts`: Parses output stream to identify content types (prompts, code blocks, responses)
- `stream-buffer.ts`: Handles partial writes and buffering for streaming output
- `ansi-processor.ts`: Processes ANSI escape sequences for terminal colors and formatting

**UI Layer** (`src/ui/`)
- `interactive-ui.ts`: React/Ink-based terminal UI, manages user input and display formatting

**Entry Point** (`src/index.ts`)
- Commander-based CLI with `interactive` and `run` commands
- Connects session events to UI handlers

### Event Flow
1. C# process output → PTY → StreamBuffer → OutputParser → AnsiProcessor → UI events
2. User input → UI → CSharpChatInterface → PTY → C# process

### Key Configuration
- Default C# executable path: `/opt/homebrew/bin/cycod` (configured in `src/bridge/csharp-session.ts:16`)
- PTY settings: xterm-color terminal, dynamic column/row sizing
- Interactive mode flag: `--interactive` passed to C# process

## Development Notes

- The application uses CommonJS module system (not ES modules)
- React is used for terminal UI via Ink library
- TypeScript strict mode is enabled
- Source maps and declarations are generated during build