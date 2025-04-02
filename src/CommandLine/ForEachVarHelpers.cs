using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ForEachVarHelpers
{
    public static List<Command> ExpandForEachVars(List<Command> commands)
    {
        return commands.SelectMany(command => command is ChatCommand chatCommand
            ? ExpandChatCommand(chatCommand)
            : new List<Command> { command }).ToList();
    }

    public static ForEachVariable ParseForeachVarOption(string[] args, out int skipCount)
    {
        skipCount = 0;

        var badLength = args.Length < 4;
        if (badLength) throw new CommandLineException($"Invalid foreach variable format. Expected: 'var name in value1 value2...'");

        var missingVarKeyword = args[0] != "var";
        if (missingVarKeyword) throw new CommandLineException($"Invalid foreach variable format. Expected: 'var name in value1 value2...'");

        var varName = args[1];

        var missingInKeyword = args[2] != "in";
        if (missingInKeyword) throw new CommandLineException($"Invalid foreach variable format. Expected: 'var name in value1 value2...'");

        skipCount += args.Length;
        var variableArgs = args.Skip(3).ToArray();

        var treatLinesAsValues = variableArgs.Length == 1 && variableArgs[0].Contains("\n");
        var values = treatLinesAsValues
            ? variableArgs[0].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            : variableArgs;

        int start = 0, end = 0;
        var treatAsIntRangeSeparatedByTwoDots = values.Length == 1 && IsIntRangeSeparatedByTwoDots(values[0], out start, out end);
        if (treatAsIntRangeSeparatedByTwoDots)
        {
            values = Enumerable.Range(start, end - start + 1).Select(i => i.ToString()).ToArray();
        }

        return new ForEachVariable(varName, values.ToList());
    }

    private static bool IsIntRangeSeparatedByTwoDots(string v, out int start, out int end)
    {
        start = end = 0;

        var parts = v.Split("..");
        if (parts.Length != 2) return false;

        start = int.Parse(parts[0]);
        end = int.Parse(parts[1]);
        if (start > end) return false;

        return true;
    }

    private static IEnumerable<Command> ExpandChatCommand(ChatCommand chatCommand)
    {
        var foreachVars = chatCommand.ForEachVariables;
        if (foreachVars.Count == 0)
        {
            return new List<ChatCommand> { chatCommand };
        }

        // Generate all combinations of variable values (Cartesian product)
        var combinations = GenerateValueCombinations(foreachVars);
        
        // Create a new command for each combination
        var expandedCommands = new List<ChatCommand>();
        foreach (var combination in combinations)
        {
            var clonedCommand = chatCommand.Clone();
            
            // Set variables for this combination
            for (int i = 0; i < foreachVars.Count; i++)
            {
                var varName = foreachVars[i].Name;
                var varValue = combination[i];
                clonedCommand.Variables[varName] = varValue;
                
                // Also set in ConfigStore so it's available for any templating
                ConfigStore.Instance.SetFromCommandLine($"Var.{varName}", varValue);
            }
            
            expandedCommands.Add(clonedCommand);
        }
        
        return expandedCommands;
    }

    private static List<List<string>> GenerateValueCombinations(List<ForEachVariable> foreachVars)
    {
        // Start with a single empty combination
        var combinations = new List<List<string>> { new List<string>() };
        
        // For each variable, extend all existing combinations with each value of the variable
        foreach (var foreachVar in foreachVars)
        {
            var newCombinations = new List<List<string>>();
            
            foreach (var existingCombination in combinations)
            {
                foreach (var value in foreachVar.Values)
                {
                    var newCombination = new List<string>(existingCombination);
                    newCombination.Add(value);
                    newCombinations.Add(newCombination);
                }
            }
            
            combinations = newCombinations;
        }
        
        return combinations;
    }
}
