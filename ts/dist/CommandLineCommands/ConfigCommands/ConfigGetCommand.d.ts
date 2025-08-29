import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to get a configuration setting.
 */
export declare class ConfigGetCommand extends ConfigBaseCommand {
    key?: string;
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeGet;
    private displayConfigValue;
    private getLocationPath;
}
//# sourceMappingURL=ConfigGetCommand.d.ts.map