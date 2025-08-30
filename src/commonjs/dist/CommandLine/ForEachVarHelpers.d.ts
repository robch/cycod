import { Command } from './Command';
import { ForEachVariable } from './ForEachVariable';
export declare class ForEachVarHelpers {
    static expandForEachVars(commands: Command[]): Command[];
    static parseForeachVarOption(args: string[]): {
        variable: ForEachVariable;
        skipCount: number;
    };
    private static parseIntRange;
    private static expandCommandWithVars;
    private static generateValueCombinations;
}
//# sourceMappingURL=ForEachVarHelpers.d.ts.map