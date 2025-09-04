
import { Command } from '../CommandLine/Command';
import { ConsoleHelpers } from '../Helpers/ConsoleHelpers';
import { VersionInfo } from '../Helpers/VersionInfo';

export class VersionCommand extends Command {
    public constructor() {
        super();
    }

    public GetCommandName(): string {
        return "version";
    }

    public IsEmpty(): boolean {
        return false;
    }

    public async ExecuteAsync(interactive: boolean): Promise<any> {
        return new Promise<number>((resolve) => {
            ConsoleHelpers.WriteLine(`Version: ${VersionInfo.GetVersion()}`, true);
            resolve(0);
        });
    }
}