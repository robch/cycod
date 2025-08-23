# C# CLI Bridge - TypeScript Application

A TypeScript application that provides a rich terminal interface for interacting with a C# CLI application.

## Features

- 🎨 Rich terminal UI with colors and formatting
- 💬 Interactive chat-like interface
- 📊 Progress bar support
- 🔍 ANSI escape sequence processing
- 📝 Syntax highlighting for code blocks
- ⚡ Real-time streaming output handling
- 🔄 Automatic buffer management

## Prerequisites

- Node.js 18+ 
- npm or yarn
- Your C# CLI executable

## Installation

1. Clone or download this project
2. Install dependencies:
   ```bash
   npm install
   ```

3. Update the C# executable path in `src/bridge/csharp-session.ts`:
   ```typescript
   this.execPath = execPath || 'path/to/your-cli.exe';
   ```

## Usage

### Interactive Mode
Start an interactive chat session with your C# CLI:
```bash
npm run build
npm start interactive
```

Or specify a custom path:
```bash
npm start interactive -- --path /path/to/your-cli.exe
```

### Single Command Mode
Run a single command and exit:
```bash
npm start run "your command here"
```

### Development Mode
Run with auto-reload during development:
```bash
npm run dev
```

## Architecture

The application consists of several key components:

- **CSharpChatInterface**: Manages the PTY session with the C# process
- **OutputParser**: Parses the output stream and identifies different content types
- **StreamBuffer**: Handles partial writes and buffering
- **AnsiProcessor**: Processes ANSI escape sequences for colors and formatting
- **InteractiveUI**: Provides the terminal user interface

## Customization

### Modifying Parser Patterns
Edit `src/bridge/output-parser.ts` to adjust how output is parsed:
- Add new patterns for detecting specific output types
- Modify code block detection
- Customize prompt detection

### Changing UI Colors
Edit `src/ui/interactive-ui.ts` to customize the appearance:
- Modify chalk colors for different output types
- Add new syntax highlighting rules
- Customize the welcome message

## Troubleshooting

### "Cannot find module 'node-pty'"
- On Windows: Run `npm install` with administrator privileges
- On Linux/Mac: You may need build tools: `sudo apt-get install build-essential`

### C# process not starting
- Verify the path to your executable is correct
- Ensure the executable has proper permissions
- Check that all C# dependencies are available

### Output not captured properly
- Make sure your C# app uses `Console.Write` or `Console.WriteLine`
- Check that interactive mode flag is being passed correctly

## License

MIT
