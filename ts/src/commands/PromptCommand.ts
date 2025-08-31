import { Command } from 'commander';
import { PromptStore } from '../prompts/PromptStore';
import { ConfigFileScope } from '../types';
import { ConsoleHelpers } from '../helpers/ConsoleHelpers';

export class PromptCommand {
  private static parseScopeFromFlags(options: any): ConfigFileScope {
    // Handle Commander.js flag variations (including capitalized short flags)
    if (options.global || options.g || options.G) return ConfigFileScope.Global;
    if (options.user || options.u || options.U) return ConfigFileScope.User;
    if (options.local || options.l || options.L) return ConfigFileScope.Local;
    
    // Default to local scope for create/delete, Any for list/show
    return ConfigFileScope.Local;
  }

  static createCommand(): Command {
    const promptCmd = new Command('prompt');
    const promptStore = new PromptStore();

    promptCmd
      .command('create <name> <content...>')
      .description('Create a new prompt')
      .option('--local', 'Create in local scope')
      .option('--user', 'Create in user scope')
      .option('--global', 'Create in global scope')
      .action(async (name: string, contentParts: string[], options) => {
        try {
          if (!name) {
            console.error('Error: Prompt name is required.');
            process.exit(1);
          }

          let content = contentParts.join(' ');
          
          if (!content) {
            console.error('Error: Prompt text is required.');
            process.exit(1);
          }
          
          // Remove surrounding quotes if present (from shell quoting)
          if (content.startsWith('"') && content.endsWith('"')) {
            content = content.slice(1, -1);
          } else if (content.startsWith("'") && content.endsWith("'")) {
            content = content.slice(1, -1);
          }
          
          const scope = PromptCommand.parseScopeFromFlags(options);
          
          // Check if content starts with @ (file reference)
          if (content.startsWith('@')) {
            const filePath = content.substring(1);
            const savedPath = await promptStore.createPromptFromFile(name, filePath, scope);
            // Match C# output: DisplaySavedPromptFile shows "/{name} (in chat)"
            console.log(`/${name} (in chat)`);
          } else {
            const savedPath = await promptStore.createPrompt(name, content, scope);
            // Match C# output: DisplaySavedPromptFile shows "/{name} (in chat)"
            console.log(`/${name} (in chat)`);
          }
        } catch (error) {
          if (error instanceof Error) {
            console.error(error.message);
          } else {
            console.error('Error creating prompt:', error);
          }
          process.exit(1);
        }
      });

    promptCmd
      .command('list')
      .description('List all available prompts')
      .option('--local', 'List local prompts only')
      .option('--user', 'List user prompts only')
      .option('--global', 'List global prompts only')
      .action(async (options) => {
        try {
          const hasSpecificScope = options.local || options.user || options.global;
          
          if (hasSpecificScope) {
            const scope = PromptCommand.parseScopeFromFlags(options);
            const scopesData = await promptStore.getAllScopesWithPrompts();
            const scopeData = scopesData.find(s => s.scope === scope);
            
            if (scopeData) {
              // Display location header
              ConsoleHelpers.displayLocationHeader(scopeData.location, scopeData.scope);
              
              if (scopeData.prompts.length === 0) {
                // Empty line for empty scope
                console.log('');
              } else {
                for (const prompt of scopeData.prompts) {
                  console.log(`  /${prompt}`);
                }
              }
            }
          } else {
            // Default behavior: show all scopes (matching C# PromptListCommand)
            const scopesData = await promptStore.getAllScopesWithPrompts();
            
            for (let i = 0; i < scopesData.length; i++) {
              const scopeData = scopesData[i];
              
              // Display location header
              ConsoleHelpers.displayLocationHeader(scopeData.location, scopeData.scope);
              
              if (scopeData.prompts.length === 0) {
                // Empty line for empty scope
                console.log('');
              } else {
                for (const prompt of scopeData.prompts) {
                  console.log(`  /${prompt}`);
                }
              }
              
              // Add blank line between scopes (except after the last one)
              if (i < scopesData.length - 1) {
                console.log('');
              }
            }
          }
        } catch (error) {
          console.error('Error listing prompts:', error);
          process.exit(1);
        }
      });

    promptCmd
      .command('show <name>')
      .description('Display the content of a prompt')
      .option('--local', 'Show from local scope only')
      .option('--user', 'Show from user scope only')
      .option('--global', 'Show from global scope only')
      .action(async (name: string, options) => {
        try {
          const hasSpecificScope = options.local || options.user || options.global;
          const scope = hasSpecificScope ? PromptCommand.parseScopeFromFlags(options) : ConfigFileScope.Any;
          
          const promptData = await promptStore.getPrompt(name, scope);
          
          // Match C# output format: DisplayPrompt shows location, then name:, then content
          const location = `${promptData.path} (${promptData.scope.toString().toLowerCase()})`;
          console.log(`LOCATION: ${location}`);
          console.log('');
          console.log(`${name}:`);
          console.log(promptData.content);
        } catch (error) {
          if (error instanceof Error) {
            console.error(error.message);
          } else {
            console.error('Error showing prompt:', error);
          }
          process.exit(1);
        }
      });

    promptCmd
      .command('delete <name>')
      .description('Delete a prompt')
      .option('--local', 'Delete from local scope only')
      .option('--user', 'Delete from user scope only')
      .option('--global', 'Delete from global scope only')
      .action(async (name: string, options) => {
        try {
          const hasSpecificScope = options.local || options.user || options.global;
          const scope = hasSpecificScope ? PromptCommand.parseScopeFromFlags(options) : ConfigFileScope.Any;
          
          await promptStore.deletePrompt(name, scope);
          console.log(`/${name} (deleted)`);
        } catch (error) {
          if (error instanceof Error) {
            console.error(error.message);
          } else {
            console.error('Error deleting prompt:', error);
          }
          process.exit(1);
        }
      });

    return promptCmd;
  }
}