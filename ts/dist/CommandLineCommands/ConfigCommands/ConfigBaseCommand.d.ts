import { ConfigStore } from '../../Configuration/ConfigStore';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
/**
 * Base class for configuration commands.
 */
export declare abstract class ConfigBaseCommand {
    protected readonly _configStore: ConfigStore;
    scope?: ConfigFileScope;
    configFileName?: string;
    constructor();
    /**
     * Gets the command name.
     */
    abstract getCommandName(): string;
    /**
     * Executes the command.
     */
    abstract executeAsync(interactive: boolean): Promise<number>;
    /**
     * Checks if the command has all required parameters.
     */
    isEmpty(): boolean;
}
//# sourceMappingURL=ConfigBaseCommand.d.ts.map