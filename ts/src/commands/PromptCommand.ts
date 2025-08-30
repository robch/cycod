import { Command } from 'commander';
import { PromptStore } from '../prompts/PromptStore';

export class PromptCommand {
  static createCommand(): Command {
    const promptCmd = new Command('prompt');
    const promptStore = new PromptStore();

    promptCmd
      .command('create <name> <content...>')
      .description('Create a new prompt')
      .action(async (name: string, contentParts: string[]) => {
        try {
          let content = contentParts.join(' ');
          
          // Remove surrounding quotes if present (from shell quoting)
          if (content.startsWith('"') && content.endsWith('"')) {
            content = content.slice(1, -1);
          } else if (content.startsWith("'") && content.endsWith("'")) {
            content = content.slice(1, -1);
          }
          
          // Check if content starts with @ (file reference)
          if (content.startsWith('@')) {
            const filePath = content.substring(1);
            await promptStore.createPromptFromFile(name, filePath);
            console.log(`Created prompt '${name}' from file: ${filePath}`);
          } else {
            await promptStore.createPrompt(name, content);
            console.log(`Created prompt '${name}'`);
          }
        } catch (error) {
          console.error('Error creating prompt:', error);
          process.exit(1);
        }
      });

    promptCmd
      .command('list')
      .description('List all available prompts')
      .action(async () => {
        try {
          const prompts = await promptStore.listPrompts();
          
          if (prompts.length === 0) {
            console.log('No prompts found. Use "prompt create <name> <content>" to create your first prompt.');
            return;
          }
          
          console.log('Available prompts:');
          for (const prompt of prompts) {
            console.log(`  ${prompt}`);
          }
        } catch (error) {
          console.error('Error listing prompts:', error);
          process.exit(1);
        }
      });

    promptCmd
      .command('show <name>')
      .description('Display the content of a prompt')
      .action(async (name: string) => {
        try {
          const content = await promptStore.getPrompt(name);
          console.log(`${name}:`);
          console.log(content);
        } catch (error) {
          console.error('Error showing prompt:', error);
          process.exit(1);
        }
      });

    promptCmd
      .command('delete <name>')
      .description('Delete a prompt')
      .action(async (name: string) => {
        try {
          await promptStore.deletePrompt(name);
          console.log(`Deleted prompt '${name}'`);
        } catch (error) {
          console.error('Error deleting prompt:', error);
          process.exit(1);
        }
      });

    return promptCmd;
  }
}