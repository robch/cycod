import { ConfigBaseCommand } from './ConfigBaseCommand';
/**
 * Command to list configuration settings.
 */
export declare class ConfigListCommand extends ConfigBaseCommand {
    constructor();
    getCommandName(): string;
    executeAsync(_interactive: boolean): Promise<number>;
    private executeList;
    private displayConfigSettings;
    private getScopeDisplayName;
}
//# sourceMappingURL=ConfigListCommand.d.ts.map