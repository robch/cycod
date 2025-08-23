#!/bin/bash

# C# CLI Bridge Setup Script
# Run this script to create all necessary files

echo "Creating C# CLI Bridge project structure..."

mkdir -p src/bridge
mkdir -p src/ui

cat > 'package.json' << 'EOF'
{
  "name": "csharp-cli-bridge",
  "version": "1.0.0",
  "description": "TypeScript bridge for C# CLI with interactive terminal UI",
  "main": "dist/index.js",
  "scripts": {
    "build": "tsc",
    "start": "node dist/index.js",
    "dev": "tsx watch src/index.ts",
    "clean": "rimraf dist",
    "typecheck": "tsc --noEmit"
  },
  "keywords": ["cli", "terminal", "interactive"],
  "author": "",
  "license": "MIT",
  "dependencies": {
    "node-pty": "^1.0.0",
    "strip-ansi": "^7.1.0",
    "ansi-regex": "^6.0.1",
    "chalk": "^5.3.0",
    "ink": "^4.4.1",
    "ink-text-input": "^5.0.1",
    "ink-select-input": "^5.0.0",
    "react": "^18.2.0",
    "commander": "^11.1.0",
    "ora": "^7.0.1"
  },
  "devDependencies": {
    "@types/node": "^20.10.5",
    "typescript": "^5.3.3",
    "tsx": "^4.7.0",
    "rimraf": "^5.0.5",
    "@types/react": "^18.2.45"
  }
}
EOF

cat > 'tsconfig.json' << 'EOF'
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "commonjs",
    "lib": ["ES2022"],
    "jsx": "react",
    "outDir": "./dist",
    "rootDir": "./src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true,
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true,
    "moduleResolution": "node"
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist"]
}
EOF

cat > 'src/index.ts' << 'EOF'
import { Command } from 'commander';
import chalk from 'chalk';
import { CSharpChatInterface } from './bridge/csharp-session';
import { InteractiveUI } from './ui/interactive-ui';

const program = new Command();

program
  .name('csharp-bridge')
  .description('TypeScript bridge for C# CLI')
  .version('1.0.0');

program
  .command('interactive')
  .description('Start interactive chat mode')
  .option('-p, --path ', 'Path to C# executable')
  .action(async (options) => {
    console.log(chalk.cyan('🚀 Starting interactive session...'));
    
    const ui = new InteractiveUI();
    const session = new CSharpChatInterface(options.path);
    
    // Connect UI to session
    session.on('prompt', () => {
      ui.showPrompt();
    });
    
    session.on('response', (content: string) => {
      ui.displayResponse(content);
    });
    
    session.on('streaming', (content: string) => {
      ui.displayStreaming(content);
    });
    
    session.on('error', (error: string) => {
      ui.displayError(error);
    });
    
    session.on('code', (code: string, language: string) => {
      ui.displayCode(code, language);
    });
    
    ui.on('input', (message: string) => {
      session.sendMessage(message);
    });
    
    ui.on('exit', () => {
      session.stop();
      process.exit(0);
    });
    
    // Handle resize
    process.stdout.on('resize', () => {
      session.resize(process.stdout.columns!, process.stdout.rows!);
    });
    
    // Start both components
    await session.start();
    await ui.start();
  });

program
  .command('run')
  .description('Run a single command')
  .argument('', 'Command to execute')
  .option('-p, --path ', 'Path to C# executable')
  .action(async (command, options) => {
    const session = new CSharpChatInterface(options.path);
    
    session.on('response', (content: string) => {
      console.log(content);
    });
    
    session.on('error', (error: string) => {
      console.error(chalk.red('Error:'), error);
    });
    
    await session.start();
    session.sendMessage(command);
    
    // Wait for response and exit
    setTimeout(() => {
      session.stop();
      process.exit(0);
    }, 5000);
  });

program.parse();
EOF

cat > 'src/bridge/csharp-session.ts' << 'EOF'
import { spawn, IPty } from 'node-pty';
import { EventEmitter } from 'events';
import { OutputParser } from './output-parser';
import { StreamBuffer } from './stream-buffer';
import { AnsiProcessor } from './ansi-processor';

export class CSharpChatInterface extends EventEmitter {
  private pty: IPty | null = null;
  private parser = new OutputParser();
  private buffer: StreamBuffer;
  private ansiProcessor = new AnsiProcessor();
  private execPath: string;
  
  constructor(execPath?: string) {
    super();
    this.execPath = execPath || 'your-cli.exe'; // CHANGE THIS TO YOUR CLI PATH
    this.buffer = new StreamBuffer(this.handleBufferedOutput.bind(this));
  }
  
  async start(): Promise {
    try {
      this.pty = spawn(this.execPath, ['--interactive'], {
        name: 'xterm-color',
        cols: process.stdout.columns || 80,
        rows: process.stdout.rows || 30,
        cwd: process.cwd(),
        env: process.env as { [key: string]: string }
      });
      
      this.pty.onData((data: string) => {
        // Raw output from C# app
        this.buffer.append(data);
      });
      
      this.pty.onExit(({ exitCode }) => {
        this.emit('exit', exitCode);
        this.cleanup();
      });
      
      this.emit('ready');
    } catch (error) {
      this.emit('error', `Failed to start C# process: ${error}`);
      throw error;
    }
  }
  
  private handleBufferedOutput(raw: string): void {
    // Process ANSI codes
    const processed = this.ansiProcessor.process(raw);
    
    // Parse the content
    const parsed = this.parser.parse(processed.text);
    
    // Emit different events based on parse results
    parsed.forEach(item => {
      switch(item.type) {
        case 'prompt':
          this.emit('prompt', item.content);
          break;
        case 'response':
          this.emit('response', item.content);
          break;
        case 'code':
          this.emit('code', item.content, item.metadata?.language);
          break;
        case 'streaming':
          this.emit('streaming', item.content);
          break;
        case 'error':
          this.emit('error', item.content);
          break;
        case 'progress':
          this.emit('progress', item.content, item.metadata?.percentage);
          break;
      }
    });
  }
  
  sendMessage(message: string): void {
    if (!this.pty) {
      throw new Error('Session not started');
    }
    this.pty.write(message + '\r\n');
  }
  
  resize(cols: number, rows: number): void {
    if (this.pty) {
      this.pty.resize(cols, rows);
    }
  }
  
  stop(): void {
    if (this.pty) {
      this.pty.kill();
      this.cleanup();
    }
  }
  
  private cleanup(): void {
    this.buffer.flush();
    this.pty = null;
  }
}
EOF

cat > 'src/bridge/output-parser.ts' << 'EOF'
export enum OutputState {
  WaitingForPrompt,
  ReceivingResponse,
  InCodeBlock,
  InProgress,
  InError
}

export interface ParsedOutput {
  type: 'prompt' | 'response' | 'code' | 'streaming' | 'error' | 'progress';
  content: string;
  metadata?: {
    language?: string;
    percentage?: number;
  };
}

export class OutputParser {
  private state = OutputState.WaitingForPrompt;
  private currentBlock = '';
  private blockType = '';
  private codeLanguage = '';
  private codeBuffer = '';
  
  parse(chunk: string): ParsedOutput[] {
    const results: ParsedOutput[] = [];
    const lines = chunk.split('\n');
    
    for (const line of lines) {
      // Handle code blocks
      if (line.trim().startsWith('```')) {
        if (this.state !== OutputState.InCodeBlock) {
          // Starting code block
          this.codeLanguage = line.trim().slice(3) || 'text';
          this.state = OutputState.InCodeBlock;
          this.codeBuffer = '';
        } else {
          // Ending code block
          results.push({
            type: 'code',
            content: this.codeBuffer,
            metadata: { language: this.codeLanguage }
          });
          this.state = OutputState.ReceivingResponse;
          this.codeBuffer = '';
        }
        continue;
      }
      
      if (this.state === OutputState.InCodeBlock) {
        this.codeBuffer += line + '\n';
        continue;
      }
      
      // Detect prompts
      if (line.endsWith('> ') || line.endsWith(': ')) {
        results.push({ type: 'prompt', content: line });
        this.state = OutputState.WaitingForPrompt;
      }
      // Detect error patterns
      else if (line.includes('Error:') || line.includes('Exception:')) {
        results.push({ type: 'error', content: line });
        this.state = OutputState.InError;
      }
      // Detect progress bars
      else if (this.detectProgressBar(line)) {
        const percentage = this.extractPercentage(line);
        results.push({
          type: 'progress',
          content: line,
          metadata: { percentage }
        });
      }
      // Detect streaming indicators
      else if (this.detectStreamingPattern(line)) {
        results.push({ type: 'streaming', content: line });
      }
      // Regular response
      else if (line.trim()) {
        results.push({ type: 'response', content: line });
        this.state = OutputState.ReceivingResponse;
      }
    }
    
    return results;
  }
  
  private detectStreamingPattern(text: string): boolean {
    return /\.\.\.$/.test(text) || 
           text.includes('Processing') ||
           text.includes('Thinking') ||
           text.includes('Loading');
  }
  
  private detectProgressBar(text: string): boolean {
    return /\[[\=\-\s]+\]/.test(text) || 
           /\d+%/.test(text);
  }
  
  private extractPercentage(text: string): number {
    const match = text.match(/(\d+)%/);
    return match ? parseInt(match[1]) : 0;
  }
  
  reset(): void {
    this.state = OutputState.WaitingForPrompt;
    this.currentBlock = '';
    this.codeBuffer = '';
    this.codeLanguage = '';
  }
}
EOF

cat > 'src/bridge/stream-buffer.ts' << 'EOF'
export class StreamBuffer {
  private buffer = '';
  private timeout: NodeJS.Timeout | null = null;
  private readonly FLUSH_DELAY = 50; // ms
  
  constructor(private onFlush: (content: string) => void) {}
  
  append(chunk: string): void {
    this.buffer += chunk;
    
    // Clear existing timeout
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
    
    // Check for immediate flush conditions
    if (this.shouldFlushImmediately()) {
      this.flush();
    } else {
      // Schedule a flush for partial content
      this.timeout = setTimeout(() => this.flush(), this.FLUSH_DELAY);
    }
  }
  
  private shouldFlushImmediately(): boolean {
    // Flush on complete lines or prompts
    return this.buffer.endsWith('\n') ||
           this.buffer.endsWith('\r\n') ||
           this.buffer.endsWith('> ') ||
           this.buffer.endsWith(': ') ||
           this.buffer.includes('\n') ||
           this.buffer.length > 1000; // Prevent overflow
  }
  
  flush(): void {
    if (this.buffer) {
      this.onFlush(this.buffer);
      this.buffer = '';
    }
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
  }
  
  clear(): void {
    this.buffer = '';
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
  }
  
  get length(): number {
    return this.buffer.length;
  }
  
  get isEmpty(): boolean {
    return this.buffer.length === 0;
  }
}
EOF

cat > 'src/bridge/ansi-processor.ts' << 'EOF'
import stripAnsi from 'strip-ansi';
import ansiRegex from 'ansi-regex';

export interface ProcessedOutput {
  text: string;
  formatting: Formatting[];
  cursorOps: CursorOperation[];
}

export interface Formatting {
  type: 'color' | 'bold' | 'italic' | 'underline';
  value: string;
  position?: number;
}

export interface CursorOperation {
  type: 'move' | 'clear' | 'save' | 'restore';
  value?: { x?: number; y?: number };
}

export class AnsiProcessor {
  process(raw: string): ProcessedOutput {
    // Detect ANSI sequences
    const regex = ansiRegex();
    const sequences = raw.match(regex) || [];
    const cleanText = stripAnsi(raw);
    
    return {
      text: cleanText,
      formatting: this.extractFormatting(sequences),
      cursorOps: this.extractCursorOps(sequences)
    };
  }
  
  private extractFormatting(sequences: string[]): Formatting[] {
    const formatting: Formatting[] = [];
    
    sequences.forEach(seq => {
      // Color codes
      if (seq.includes('[30m')) formatting.push({ type: 'color', value: 'black' });
      if (seq.includes('[31m')) formatting.push({ type: 'color', value: 'red' });
      if (seq.includes('[32m')) formatting.push({ type: 'color', value: 'green' });
      if (seq.includes('[33m')) formatting.push({ type: 'color', value: 'yellow' });
      if (seq.includes('[34m')) formatting.push({ type: 'color', value: 'blue' });
      if (seq.includes('[35m')) formatting.push({ type: 'color', value: 'magenta' });
      if (seq.includes('[36m')) formatting.push({ type: 'color', value: 'cyan' });
      if (seq.includes('[37m')) formatting.push({ type: 'color', value: 'white' });
      
      // Bright colors
      if (seq.includes('[90m')) formatting.push({ type: 'color', value: 'gray' });
      if (seq.includes('[91m')) formatting.push({ type: 'color', value: 'brightRed' });
      if (seq.includes('[92m')) formatting.push({ type: 'color', value: 'brightGreen' });
      if (seq.includes('[93m')) formatting.push({ type: 'color', value: 'brightYellow' });
      if (seq.includes('[94m')) formatting.push({ type: 'color', value: 'brightBlue' });
      if (seq.includes('[95m')) formatting.push({ type: 'color', value: 'brightMagenta' });
      if (seq.includes('[96m')) formatting.push({ type: 'color', value: 'brightCyan' });
      if (seq.includes('[97m')) formatting.push({ type: 'color', value: 'brightWhite' });
      
      // Text styles
      if (seq.includes('[1m')) formatting.push({ type: 'bold', value: 'true' });
      if (seq.includes('[3m')) formatting.push({ type: 'italic', value: 'true' });
      if (seq.includes('[4m')) formatting.push({ type: 'underline', value: 'true' });
    });
    
    return formatting;
  }
  
  private extractCursorOps(sequences: string[]): CursorOperation[] {
    const ops: CursorOperation[] = [];
    
    sequences.forEach(seq => {
      // Cursor movement
      const upMatch = seq.match(/\[(\d+)A/);
      if (upMatch) {
        ops.push({ type: 'move', value: { y: -parseInt(upMatch[1]) } });
      }
      
      const downMatch = seq.match(/\[(\d+)B/);
      if (downMatch) {
        ops.push({ type: 'move', value: { y: parseInt(downMatch[1]) } });
      }
      
      const rightMatch = seq.match(/\[(\d+)C/);
      if (rightMatch) {
        ops.push({ type: 'move', value: { x: parseInt(rightMatch[1]) } });
      }
      
      const leftMatch = seq.match(/\[(\d+)D/);
      if (leftMatch) {
        ops.push({ type: 'move', value: { x: -parseInt(leftMatch[1]) } });
      }
      
      // Clear operations
      if (seq.includes('[2J')) ops.push({ type: 'clear', value: undefined });
      if (seq.includes('[K')) ops.push({ type: 'clear', value: undefined });
      
      // Save/restore cursor
      if (seq.includes('[s')) ops.push({ type: 'save', value: undefined });
      if (seq.includes('[u')) ops.push({ type: 'restore', value: undefined });
    });
    
    return ops;
  }
}
EOF

cat > 'src/ui/interactive-ui.ts' << 'EOF'
import { EventEmitter } from 'events';
import * as readline from 'readline';
import chalk from 'chalk';
import ora from 'ora';

export class InteractiveUI extends EventEmitter {
  private rl: readline.Interface | null = null;
  private spinner = ora();
  private isWaitingForResponse = false;
  
  async start(): Promise {
    this.rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
      prompt: chalk.green('> ')
    });
    
    this.rl.on('line', (input) => {
      if (input.trim()) {
        this.isWaitingForResponse = true;
        this.emit('input', input);
      } else {
        this.showPrompt();
      }
    });
    
    this.rl.on('SIGINT', () => {
      this.emit('exit');
    });
    
    // Welcome message
    console.log(chalk.cyan('═'.repeat(50)));
    console.log(chalk.cyan.bold('  Welcome to C# CLI Bridge'));
    console.log(chalk.cyan('  Type "exit" to quit, Ctrl+C to force exit'));
    console.log(chalk.cyan('═'.repeat(50)));
    console.log();
  }
  
  showPrompt(): void {
    this.isWaitingForResponse = false;
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    if (this.rl) {
      this.rl.prompt();
    }
  }
  
  displayResponse(content: string): void {
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    console.log(chalk.white(content));
  }
  
  displayStreaming(content: string): void {
    if (!this.spinner.isSpinning && this.isWaitingForResponse) {
      this.spinner.start(chalk.gray(content));
    } else {
      this.spinner.text = chalk.gray(content);
    }
  }
  
  displayError(error: string): void {
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    console.log(chalk.red('❌ ' + error));
  }
  
  displayCode(code: string, language: string): void {
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    
    console.log(chalk.gray('─'.repeat(40)));
    console.log(chalk.yellow(`📝 Code (${language}):`));
    console.log(chalk.gray('─'.repeat(40)));
    
    // Simple syntax highlighting based on language
    let highlighted = code;
    if (language === 'javascript' || language === 'typescript') {
      highlighted = this.highlightJS(code);
    } else if (language === 'csharp' || language === 'cs') {
      highlighted = this.highlightCSharp(code);
    }
    
    console.log(highlighted);
    console.log(chalk.gray('─'.repeat(40)));
  }
  
  private highlightJS(code: string): string {
    // Simple JS/TS highlighting
    return code
      .replace(/\b(const|let|var|function|return|if|else|for|while|class|import|export|from)\b/g, 
        chalk.blue('$1'))
      .replace(/\b(true|false|null|undefined)\b/g, 
        chalk.yellow('$1'))
      .replace(/(["'])(?:(?=(\\?))\2.)*?\1/g, 
        chalk.green('$&'))
      .replace(/\/\/.*/g, 
        chalk.gray('$&'));
  }
  
  private highlightCSharp(code: string): string {
    // Simple C# highlighting
    return code
      .replace(/\b(public|private|class|interface|namespace|using|var|const|static|void|return|if|else|for|foreach|while)\b/g, 
        chalk.blue('$1'))
      .replace(/\b(true|false|null)\b/g, 
        chalk.yellow('$1'))
      .replace(/(["'])(?:(?=(\\?))\2.)*?\1/g, 
        chalk.green('$&'))
      .replace(/\/\/.*/g, 
        chalk.gray('$&'));
  }
  
  stop(): void {
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    if (this.rl) {
      this.rl.close();
    }
  }
}
EOF

cat > '.gitignore' << 'EOF'
node_modules/
dist/
*.log
.DS_Store
.env
.vscode/
*.exe
*.dll
bin/
obj/
EOF

cat > 'README.md' << 'EOF'
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
EOF

echo "Project structure created successfully!"
echo "Next steps:"
echo "  1. Run: npm install"
echo "  2. Update the path to your C# executable in src/bridge/csharp-session.ts"
echo "  3. Run: npm run build"
echo "  4. Run: npm start interactive"
