
import { Command } from '../CommandLine/Command';

export class HelpCommand extends Command {
    public constructor() {
        super();
    }

    public GetCommandName(): string {
        return "help";
    }

    public IsEmpty(): boolean {
        return false;
    }

    public async ExecuteAsync(interactive: boolean): Promise<any> {
        throw new Error("Not implemented");
    }
}