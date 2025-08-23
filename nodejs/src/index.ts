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
    
    session.on('ready-for-input', () => {
      ui.setReadyForInput();
    });
    
    session.on('prompt-ready', (prompt: string) => {
      ui.setPromptAndEnableInput(prompt);
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
    
    session.on('exit', (exitCode: number) => {
      // Don't display error message - it's handled in the session
      ui.stop();
      process.exit(exitCode === 0 ? 0 : 1);
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
