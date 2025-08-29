import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to set a configuration setting.
 */
export declare class ConfigSetCommand extends ConfigBaseCommand {
    key?: string;
    value?: string;
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeSet;
    private displayConfigValue;
    private displayList;
}
//# sourceMappingURL=ConfigSetCommand.d.ts.map