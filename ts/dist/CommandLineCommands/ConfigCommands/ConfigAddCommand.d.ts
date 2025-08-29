import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to add a value to a list configuration setting.
 */
export declare class ConfigAddCommand extends ConfigBaseCommand {
    key?: string;
    value?: string;
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeAdd;
    private getScopeDisplayName;
}
//# sourceMappingURL=ConfigAddCommand.d.ts.map