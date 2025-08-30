import { Command } from './Command';
import { CommandWithVariables } from './CommandWithVariables';
import { ForEachVariable } from './ForEachVariable';
import { CommandLineException } from './CommandLineException';

export class ForEachVarHelpers {
    static expandForEachVars(commands: Command[]): Command[] {
        return commands.flatMap(command => 
            command instanceof CommandWithVariables
                ? this.expandCommandWithVars(command)
                : [command]
        );
    }

    static parseForeachVarOption(args: string[]): { variable: ForEachVariable; skipCount: number } {
        let skipCount = 0;

        if (args.length < 4) {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        if (args[0] !== "var") {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        const varName = args[1];

        if (args[2] !== "in") {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        skipCount += args.length;
        const variableArgs = args.slice(3);

        const treatLinesAsValues = variableArgs.length === 1 && variableArgs[0].includes('\n');
        let values: string[];
        
        if (treatLinesAsValues) {
            values = variableArgs[0].split(/[\r\n]/).filter(line => line.trim() !== '');
        } else {
            values = variableArgs;
        }

        // Check for integer range (e.g., "1..10")
        if (values.length === 1) {
            const rangeMatch = this.parseIntRange(values[0]);
            if (rangeMatch) {
                const { start, end } = rangeMatch;
                values = Array.from({ length: end - start + 1 }, (_, i) => (start + i).toString());
            }
        }

        return {
            variable: new ForEachVariable(varName, values),
            skipCount
        };
    }

    private static parseIntRange(value: string): { start: number; end: number } | null {
        const parts = value.split('..');
        if (parts.length !== 2) return null;

        const start = parseInt(parts[0], 10);
        const end = parseInt(parts[1], 10);
        
        if (isNaN(start) || isNaN(end) || start > end) {
            return null;
        }

        return { start, end };
    }

    private static expandCommandWithVars(commandWithVars: CommandWithVariables): CommandWithVariables[] {
        const foreachVars = commandWithVars.forEachVariables;
        if (foreachVars.length === 0) {
            return [commandWithVars];
        }

        // Generate all combinations of variable values (Cartesian product)
        const combinations = this.generateValueCombinations(foreachVars);
        
        // Create a new command for each combination
        const expandedCommands: CommandWithVariables[] = [];
        
        for (const combination of combinations) {
            const clonedCommand = commandWithVars.clone();
            
            // Set variables for this combination
            for (let i = 0; i < foreachVars.length; i++) {
                const varName = foreachVars[i].name;
                const varValue = combination[i];
                clonedCommand.variables.set(varName, varValue);
            }
            
            expandedCommands.push(clonedCommand);
        }
        
        return expandedCommands;
    }

    private static generateValueCombinations(foreachVars: ForEachVariable[]): string[][] {
        // Start with a single empty combination
        let combinations: string[][] = [[]];
        
        // For each variable, extend all existing combinations with each value of the variable
        for (const foreachVar of foreachVars) {
            const newCombinations: string[][] = [];
            
            for (const existingCombination of combinations) {
                for (const value of foreachVar.values) {
                    const newCombination = [...existingCombination, value];
                    newCombinations.push(newCombination);
                }
            }
            
            combinations = newCombinations;
        }
        
        return combinations;
    }
}