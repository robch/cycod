import { Command } from './Command';
import { CommandWithVariables } from './CommandWithVariables';
import { ForEachVariable } from './ForEachVariable';
import { CommandLineException } from './CommandLineException';
import { ConfigStore } from '../Configuration/ConfigStore';

/**
 * Utilities for expanding foreach variables in commands.
 * Provides functionality to parse and expand foreach variable definitions
 * into multiple command instances with different variable values.
 */
export class ForEachVarHelpers {
    /**
     * Expands all commands that contain foreach variables into multiple command instances.
     * For commands with foreach variables, creates a Cartesian product of all variable values.
     * @param commands The list of commands to expand.
     * @returns A new list with foreach variables expanded into individual commands.
     */
    public static ExpandForEachVars(commands: Command[]): Command[] {
        return commands.flatMap(command => {
            if (command instanceof CommandWithVariables) {
                return this.ExpandCommandWithVars(command);
            } else {
                return [command];
            }
        });
    }

    /**
     * Parses a foreach variable option from command line arguments.
     * Expected format: "var name in value1 value2..."
     * @param args The command line arguments starting with the foreach variable definition.
     * @returns An object containing the parsed ForEachVariable and the number of arguments consumed.
     */
    public static ParseForeachVarOption(args: string[]): { variable: ForEachVariable; skipCount: number } {
        let skipCount = 0;

        const badLength = args.length < 4;
        if (badLength) {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        const missingVarKeyword = args[0] !== "var";
        if (missingVarKeyword) {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        const varName = args[1];

        const missingInKeyword = args[2] !== "in";
        if (missingInKeyword) {
            throw new CommandLineException("Invalid foreach variable format. Expected: 'var name in value1 value2...'");
        }

        skipCount += args.length;
        const variableArgs = args.slice(3);

        const treatLinesAsValues = variableArgs.length === 1 && variableArgs[0].includes("\n");
        let values = treatLinesAsValues
            ? variableArgs[0].split(/[\r\n]+/).filter(line => line.trim() !== "")
            : variableArgs;

        const rangeResult = this.IsIntRangeSeparatedByTwoDots(values[0]);
        const treatAsIntRangeSeparatedByTwoDots = values.length === 1 && rangeResult.isRange;
        if (treatAsIntRangeSeparatedByTwoDots) {
            const { start, end } = rangeResult;
            values = Array.from({ length: end - start + 1 }, (_, i) => (start + i).toString());
        }

        return {
            variable: new ForEachVariable(varName, values),
            skipCount: skipCount
        };
    }

    /**
     * Checks if a string represents an integer range in the format "start..end".
     * @param value The string to check.
     * @returns An object indicating if it's a range and the start/end values if applicable.
     */
    private static IsIntRangeSeparatedByTwoDots(value: string): { isRange: boolean; start: number; end: number } {
        if (!value) {
            return { isRange: false, start: 0, end: 0 };
        }

        const parts = value.split("..");
        if (parts.length !== 2) {
            return { isRange: false, start: 0, end: 0 };
        }

        const start = parseInt(parts[0], 10);
        const end = parseInt(parts[1], 10);
        
        if (isNaN(start) || isNaN(end) || start > end) {
            return { isRange: false, start: 0, end: 0 };
        }

        return { isRange: true, start, end };
    }

    /**
     * Expands a single command with foreach variables into multiple commands.
     * Creates a Cartesian product of all foreach variable values.
     * @param commandWithVars The command to expand.
     * @returns An array of commands, one for each combination of variable values.
     */
    private static ExpandCommandWithVars(commandWithVars: CommandWithVariables): Command[] {
        const foreachVars = commandWithVars.ForEachVariables;
        if (foreachVars.length === 0) {
            return [commandWithVars];
        }

        // Generate all combinations of variable values (Cartesian product)
        const combinations = this.GenerateValueCombinations(foreachVars);
        
        // Create a new command for each combination
        const expandedCommands: CommandWithVariables[] = [];
        for (const combination of combinations) {
            const clonedCommand = commandWithVars.Clone();
            
            // Set variables for this combination
            for (let i = 0; i < foreachVars.length; i++) {
                const varName = foreachVars[i].Name;
                const varValue = combination[i];
                clonedCommand.Variables.set(varName, varValue);
                
                // Also set in ConfigStore so it's available for any templating
                ConfigStore.Instance.SetFromCommandLine(`Var.${varName}`, varValue);
            }
            
            expandedCommands.push(clonedCommand);
        }
        
        return expandedCommands;
    }

    /**
     * Generates all possible combinations of values from a list of foreach variables.
     * Creates a Cartesian product of all variable values.
     * @param foreachVars The list of foreach variables.
     * @returns An array of arrays, where each inner array represents one combination of values.
     */
    private static GenerateValueCombinations(foreachVars: ForEachVariable[]): string[][] {
        // Start with a single empty combination
        let combinations: string[][] = [[]];
        
        // For each variable, extend all existing combinations with each value of the variable
        for (const foreachVar of foreachVars) {
            const newCombinations: string[][] = [];
            
            for (const existingCombination of combinations) {
                for (const value of foreachVar.Values) {
                    const newCombination = [...existingCombination, value];
                    newCombinations.push(newCombination);
                }
            }
            
            combinations = newCombinations;
        }
        
        return combinations;
    }
}
