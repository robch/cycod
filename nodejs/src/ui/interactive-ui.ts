import { EventEmitter } from 'events';
import * as readline from 'readline';
import chalk from 'chalk';
import ora from 'ora';
import { marked } from 'marked';
import TerminalRenderer from 'marked-terminal';

export class InteractiveUI extends EventEmitter {
  private rl: readline.Interface | null = null;
  private spinner = ora();
  private isWaitingForResponse = false;
  private isReadyForInput = false;
  
  async start(): Promise<void> {
    // Configure marked for terminal output
    marked.setOptions({
      renderer: new TerminalRenderer({
        // Colors and styling
        code: chalk.yellow,
        blockquote: chalk.gray.italic,
        html: chalk.gray,
        heading: chalk.green.bold,
        firstHeading: chalk.magenta.bold.underline,
        hr: chalk.gray,
        listitem: chalk.white,
        paragraph: chalk.white,
        table: chalk.gray,
        strong: chalk.bold.white,
        em: chalk.italic,
        codespan: chalk.cyan,
        del: chalk.strikethrough.gray,
        link: chalk.blue.underline,
        href: chalk.blue.underline,
        
        // Layout settings
        width: process.stdout.columns - 4, // Leave some margin
        reflowText: true,
        showSectionPrefix: false,
        tableOptions: {
          chars: {
            'top': '─', 'top-mid': '┬', 'top-left': '┌', 'top-right': '┐',
            'bottom': '─', 'bottom-mid': '┴', 'bottom-left': '└', 'bottom-right': '┘',
            'left': '│', 'left-mid': '├', 'mid': '─', 'mid-mid': '┼',
            'right': '│', 'right-mid': '┤', 'middle': '│'
          }
        }
      }) as any
    } as any);

    this.rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
      prompt: '' // No prompt - let cycod handle all prompting
    });
    
    this.rl.on('line', (input) => {
      if (!this.isReadyForInput) {
        console.log(chalk.yellow('Please wait for cycod to be ready...'));
        return;
      }
      
      if (input.trim()) {
        this.isWaitingForResponse = true;
        this.isReadyForInput = false; // Disable input until next User: prompt
        this.emit('input', input);
      }
    });
    
    this.rl.on('SIGINT', () => {
      this.emit('exit');
    });
    
    // Welcome message
    console.log(chalk.cyan('═'.repeat(50)));
    console.log(chalk.cyan.bold('  Welcome to C# CLI Bridge'));
    console.log(chalk.cyan('  Type "exit" to quit, Ctrl+C to force exit'));
    console.log(chalk.cyan('  Waiting for cycod to start...'));
    console.log(chalk.cyan('═'.repeat(50)));
    console.log();
  }
  
  showPrompt(): void {
    this.isWaitingForResponse = false;
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    // Don't show any prompt - cycod handles that
  }
  
  setReadyForInput(): void {
    this.isReadyForInput = true;
    // Don't call showPrompt() or set any prompt - cycod handles all display
  }
  
  setPromptAndEnableInput(prompt: string): void {
    this.isReadyForInput = true;
    if (this.rl) {
      // Set the prompt (with colors) so readline protects it during editing
      this.rl.setPrompt(prompt);
      // Show the prompt
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
    // Stop spinner if it's running since we have real content
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    
    // Display content immediately without newline to preserve streaming
    process.stdout.write(content);
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

  displayMarkdown(content: string): void {
    if (this.spinner.isSpinning) {
      this.spinner.stop();
    }
    
    try {
      const rendered = marked(content);
      console.log(rendered);
    } catch (error) {
      // Fallback to plain text if markdown parsing fails
      console.log(chalk.white(content));
    }
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
