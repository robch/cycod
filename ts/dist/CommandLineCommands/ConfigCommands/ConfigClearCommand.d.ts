import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to clear a configuration setting.
 */
export declare class ConfigClearCommand extends ConfigBaseCommand {
    key?: string;
    getCommandName(): string;
    isEmpty(): boolean;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeClear;
    private getScopeDisplayName;
}
//# sourceMappingURL=ConfigClearCommand.d.ts.map