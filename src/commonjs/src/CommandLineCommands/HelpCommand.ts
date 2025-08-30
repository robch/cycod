import { Command } from '../CommandLine/Command';

export class HelpCommand extends Command {
    constructor() {
        super();
    }

    getCommandName(): string {
        return "help";
    }

    isEmpty(): boolean {
        return false;
    }

    async executeAsync(interactive: boolean): Promise<any> {
        throw new Error('Not implemented');
    }
}