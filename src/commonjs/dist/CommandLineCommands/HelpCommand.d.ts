import { Command } from '../CommandLine/Command';
export declare class HelpCommand extends Command {
    constructor();
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(interactive: boolean): Promise<any>;
}
//# sourceMappingURL=HelpCommand.d.ts.map