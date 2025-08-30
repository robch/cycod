import { Command } from '../CommandLine/Command';
export declare class VersionCommand extends Command {
    constructor();
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(interactive: boolean): Promise<any>;
}
//# sourceMappingURL=VersionCommand.d.ts.map