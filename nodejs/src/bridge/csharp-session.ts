import { spawn, IPty } from 'node-pty';
import { EventEmitter } from 'events';
import { OutputParser } from './output-parser';
import { StreamBuffer } from './stream-buffer';
import { AnsiProcessor } from './ansi-processor';
import chalk from 'chalk';
import * as fs from 'fs';
import stripAnsi from 'strip-ansi';

export class CSharpChatInterface extends EventEmitter {
  private pty: IPty | null = null;
  private parser = new OutputParser();
  private ansiProcessor = new AnsiProcessor();
  private execPath: string;
  private isSessionActive = false;
  private isWaitingForInitialPrompt = true;
  private lastSentMessage = '';
  private receivedBuffer = '';
  private userRequestedExit = false;
  private responseBuffer = '';
  private isCollectingResponse = false;
  
  constructor(execPath?: string) {
    super();
    this.execPath = execPath || '/opt/homebrew/bin/cycod'; // CHANGE THIS TO YOUR CLI PATH
  }
  
  async start(): Promise<void> {
    // Check if executable exists
    if (!fs.existsSync(this.execPath)) {
      console.error(chalk.red(`\n❌ Cycod executable not found at: ${this.execPath}`));
      console.error(chalk.yellow(`\nPlease ensure cycod is installed. You can:`));
      console.error(chalk.yellow(`1. Install it globally: npm install -g cycod`));
      console.error(chalk.yellow(`2. Specify a custom path: npm start interactive -- --path /path/to/cycod`));
      console.error(chalk.yellow(`3. Update the default path in src/bridge/csharp-session.ts`));
      throw new Error(`Executable not found: ${this.execPath}`);
    }
    
    try {
      // Start cycod in interactive chat mode with auto-approval for function calls
      // Force interactive mode explicitly to ensure it doesn't exit
      const args = ['chat', '--auto-approve', '*', '--interactive', 'true'];
      
      this.pty = spawn(this.execPath, args, {
        name: 'xterm-color',
        cols: process.stdout.columns || 80,
        rows: process.stdout.rows || 30,
        cwd: process.cwd(),
        env: {
          ...process.env as { [key: string]: string },
          // Ensure cycod knows it's in an interactive terminal
          TERM: 'xterm-256color'
        },
        useConpty: false // Disable Windows ConPTY for better compatibility
      });
      
      this.pty.onData((data: string) => {
        // If we're expecting an echo, accumulate data to find it
        if (this.lastSentMessage) {
          this.receivedBuffer += data;
          
          // Check if buffer contains the complete echo with line ending
          const echoWithLineEnding = this.lastSentMessage + '\r\n';
          if (this.receivedBuffer.includes(echoWithLineEnding)) {
            // Found the echo! Remove it from the buffer
            this.receivedBuffer = this.receivedBuffer.replace(echoWithLineEnding, '');
            this.lastSentMessage = ''; // Clear it after filtering
            
            // Process the remaining data
            if (this.receivedBuffer) {
              this.processReceivedData(this.receivedBuffer);
              this.receivedBuffer = ''; // Clear buffer after processing
            }
            return;
          }
          
          // If buffer is getting too long without finding echo, give up and process
          if (this.receivedBuffer.length > 1000) {
            this.lastSentMessage = '';
            this.processReceivedData(this.receivedBuffer);
            this.receivedBuffer = '';
          }
          
          return; // Don't process data while waiting for echo
        }
        
        // No echo expected, process data directly
        this.processReceivedData(data);
      });
      
      this.pty.onExit(({ exitCode, signal }) => {
        this.isSessionActive = false;
        
        // Only show warning if user didn't request exit
        if (!this.userRequestedExit) {
          console.error(chalk.red(`\n⚠️  Cycod chat session ended unexpectedly!`));
          console.error(chalk.yellow(`Exit code: ${exitCode}, Signal: ${signal || 'none'}`));
          
          if (exitCode === 127) {
            console.error(chalk.yellow(`This usually means the cycod executable was not found.`));
          } else if (exitCode === 0) {
            console.error(chalk.yellow(`Cycod exited normally - this might indicate it's not staying in interactive mode.`));
            console.error(chalk.yellow(`Try running 'cycod chat' directly to see if it shows a User> prompt.`));
          }
        } else {
          // User requested exit - this is expected
          console.log(chalk.green(`\nGoodbye!`));
        }
        
        this.emit('exit', exitCode);
        this.cleanup();
      });
      
      this.isSessionActive = true;
      // Don't emit 'ready' here - wait for the User> prompt instead
    } catch (error) {
      this.emit('error', `Failed to start C# process: ${error}`);
      throw error;
    }
  }
  
  private processReceivedData(data: string): void {
    // Check if this contains a User: prompt
    if (data.includes('User:')) {
      // Find where User: starts (in the actual text, not ANSI codes)
      const userIndex = data.indexOf('User:');
      
      // Add content before User: to response buffer
      const beforeUser = data.substring(0, userIndex);
      if (beforeUser) {
        this.responseBuffer += beforeUser;
      }
      
      // Process the complete response buffer
      if (this.responseBuffer.trim()) {
        const cleanContent = stripAnsi(this.responseBuffer);
        const isMarkdown = this.detectMarkdown(cleanContent);
        
        if (isMarkdown) {
          this.emit('markdown', cleanContent);
        } else {
          // Display the complete response as is
          process.stdout.write(this.responseBuffer);
        }
        
        // Clear the response buffer
        this.responseBuffer = '';
      }
      
      // Hard-code the green color for the prompt to match cycod's Assistant color
      // \u001b[32m is standard green (same as Assistant), \u001b[0m resets color
      const greenPrompt = '\u001b[32mUser: \u001b[0m';
      
      // Don't display the prompt here - let readline handle it
      // Send the colored prompt to readline
      this.emit('prompt-ready', greenPrompt);
      
      // Skip past "User: " in the original data and display any remaining content
      const afterPromptIndex = data.indexOf('User:') + 6; // "User: " is 6 chars
      const remainingData = data.substring(afterPromptIndex);
      
      // Skip any ANSI reset codes or whitespace immediately after prompt
      let contentStart = 0;
      for (let i = 0; i < remainingData.length; i++) {
        if (remainingData[i] === '\u001b') {
          // Skip ANSI sequence
          while (i < remainingData.length && remainingData[i] !== 'm') i++;
        } else if (remainingData[i] !== ' ' && remainingData[i] !== '\r' && remainingData[i] !== '\n') {
          contentStart = i;
          break;
        }
      }
      
      const afterPrompt = remainingData.substring(contentStart);
      if (afterPrompt && afterPrompt.trim()) {
        process.stdout.write(afterPrompt);
      }
    } else {
      // Accumulate response data until we see User: prompt
      if (data) {
        this.responseBuffer += data;
      }
    }
    
    // Also check for User: prompts to enable input
    this.handleBufferedOutput(data);
  }

  private handleBufferedOutput(raw: string): void {
    // Process ANSI codes
    const processed = this.ansiProcessor.process(raw);
    
    // Check if this is the initial User: prompt we're waiting for
    if (this.isWaitingForInitialPrompt && processed.text.includes('User:')) {
      this.isWaitingForInitialPrompt = false;
      // Signal that we're ready for user input
      this.emit('ready-for-input');
      return;
    }
    
    // Check for User: prompt after responses to re-enable input
    if (!this.isWaitingForInitialPrompt && processed.text.includes('User:')) {
      this.emit('ready-for-input');
    }
  }
  
  sendMessage(message: string): void {
    if (!this.pty || !this.isSessionActive) {
      console.error(chalk.red('Cannot send message: Cycod session is not active'));
      return;
    }
    
    // Check if user is requesting exit
    if (message.trim().toLowerCase() === 'exit') {
      this.userRequestedExit = true;
    }
    
    // Store the message to filter out echo
    this.lastSentMessage = message;
    
    // Send message with just \n (not \r\n) to match what a real terminal would send
    const fullMessage = message + '\n';
    this.pty.write(fullMessage);
  }
  
  resize(cols: number, rows: number): void {
    if (this.pty) {
      this.pty.resize(cols, rows);
    }
  }
  
  stop(): void {
    if (this.pty) {
      this.isSessionActive = false;
      this.pty.kill();
      this.cleanup();
    }
  }
  
  isActive(): boolean {
    return this.isSessionActive;
  }
  
  
  private detectMarkdown(text: string): boolean {
    // Quick checks for common markdown patterns
    const indicators = [
      text.includes('**') || text.includes('__'),  // Bold
      /^#{1,6}\s/m.test(text),                     // Headers
      /^\s*[\*\-\+]\s/m.test(text),                // Unordered lists
      /^\s*\d+\.\s/m.test(text),                   // Numbered lists
      /\[.*\]\(.*\)/.test(text),                   // Links
      /^```/m.test(text),                          // Code blocks
      /^\|.*\|$/m.test(text),                      // Tables
      /^>\s/m.test(text)                           // Blockquotes
    ];
    
    // Check for strong indicators that should trigger markdown rendering alone
    const strongIndicators = [
      /^#{1,6}\s/m.test(text),                     // Headers
      /^```/m.test(text),                          // Code blocks  
      /^\|.*\|$/m.test(text) && /^\|[-\s\|:]+\|$/m.test(text), // Tables with separator row
    ];
    
    // Trigger if we have strong indicators OR 2+ regular indicators
    return strongIndicators.some(Boolean) || indicators.filter(Boolean).length >= 2;
  }

  private cleanup(): void {
    this.pty = null;
  }
}
