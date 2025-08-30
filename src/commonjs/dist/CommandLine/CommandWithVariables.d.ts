import { Command } from './Command';
import { ForEachVariable } from './ForEachVariable';
export declare abstract class CommandWithVariables extends Command {
    variables: Map<string, string>;
    forEachVariables: ForEachVariable[];
    abstract clone(): CommandWithVariables;
}
//# sourceMappingURL=CommandWithVariables.d.ts.map