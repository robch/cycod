import { PromptBaseCommand } from './PromptBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { PromptDisplayHelpers } from '../../Helpers/PromptDisplayHelpers';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to list all available prompts.
 */
export class PromptListCommand extends PromptBaseCommand {
  
  constructor() {
    super();
    this.scope = ConfigFileScope.Any;
  }

  getCommandName(): string {
    return 'prompt list';
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    const scope = this.scope ?? ConfigFileScope.Any;
    const isAnyScope = scope === ConfigFileScope.Any;

    if (isAnyScope || scope === ConfigFileScope.Global) {
      PromptDisplayHelpers.displayPrompts(ConfigFileScope.Global);
      if (isAnyScope) ConsoleHelpers.writeLine('');
    }

    if (isAnyScope || scope === ConfigFileScope.User) {
      PromptDisplayHelpers.displayPrompts(ConfigFileScope.User);
      if (isAnyScope) ConsoleHelpers.writeLine('');
    }

    if (isAnyScope || scope === ConfigFileScope.Local) {
      PromptDisplayHelpers.displayPrompts(ConfigFileScope.Local);
    }

    return 0;
  }
}