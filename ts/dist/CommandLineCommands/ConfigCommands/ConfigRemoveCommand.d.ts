import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to remove a value from a list configuration setting.
 */
export declare class ConfigRemoveCommand extends ConfigBaseCommand {
    key?: string;
    value?: string;
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeRemove;
    private getScopeDisplayName;
}
//# sourceMappingURL=ConfigRemoveCommand.d.ts.map