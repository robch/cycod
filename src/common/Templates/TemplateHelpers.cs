using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class TemplateHelpers
{
    public static string ProcessTemplate(string template, INamedValues values)
    {
        var calculator = new ExpressionCalculator();

        Func<string, string> interpolate = (line) => line.ReplaceValues(values);
        Func<string, bool> evaluateCondition = (condition) =>
        {
            condition = condition.ReplaceValues(values).Trim();
            var evaluated = condition switch
            {
                "" => false,
                "true" => true,
                "false" => false,
                _ => calculator.Evaluate(condition)
            };

            ConsoleHelpers.WriteDebugLine($"Evaluating condition: {condition} => {evaluated}");
            return evaluated;
        };
        Action<string> setVariable = (s) => {
            var result = calculator.Evaluate(s);
            var name = s.Split('=')[0].Trim();
            values.Set(name, result.ToString());
        };
        Func<string, string> include = (nakedAtFile) => AtFileHelpers.ExpandAtFileValue($"@{nakedAtFile}", values);

        return ProcessTemplate(template, evaluateCondition, interpolate, setVariable, include);
    }

    public static string ProcessTemplate(string template, Func<string, bool> evaluateCondition, Func<string, string> interpolate, Action<string> setVariable, Func<string, string> include)
    {
        var safeEvaluateCondition = TryCatchHelpers.NoThrowWrapException(evaluateCondition, false);

        var lines = template.Split('\n').ToList();
        var output = new StringBuilder();

        var inTrueBranchNow = new Stack<bool>();
        inTrueBranchNow.Push(true);

        var skipElseBranches = new Stack<bool>();
        skipElseBranches.Push(true);

        for (var i = 0; i < lines.Count(); i++)
        {
            var line = lines[i];
            var trimmedLine = line.Trim('\n', '\r', ' ', '\t');

            if (trimmedLine.StartsWith("{{if ") && trimmedLine.EndsWith("}}") && !trimmedLine.EndsWith("{{endif}}"))
            {
                var condition = trimmedLine[5..^2].Trim();
                var evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex == null)
                {
                    inTrueBranchNow.Push(evaluated.value);
                    skipElseBranches.Push(evaluated.value);
                    continue;
                }
            }
            else if (trimmedLine.StartsWith("{{else if ") && trimmedLine.EndsWith("}}"))
            {
                if (inTrueBranchNow.Peek())
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(false);
                    // skipElseBranches.Peek() should already be true
                    continue;
                }
                else if (skipElseBranches.Peek())
                {
                    continue;
                }

                var condition = trimmedLine[10..^2].Trim();
                var evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex == null)
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(evaluated.value);
                    skipElseBranches.Pop();
                    skipElseBranches.Push(evaluated.value);
                    continue;
                }
            }
            else if (trimmedLine.StartsWith("{{else}}"))
            {
                if (inTrueBranchNow.Peek())
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(false);
                    // skipElseBranches.Peek() should already be true
                    continue;
                }
                else if (skipElseBranches.Peek())
                {
                    continue;
                }

                inTrueBranchNow.Pop();
                inTrueBranchNow.Push(true);
                skipElseBranches.Pop();
                skipElseBranches.Push(true);
                continue;
            }
            else if (trimmedLine.StartsWith("{{endif}}"))
            {
                inTrueBranchNow.Pop();
                skipElseBranches.Pop();
                continue;
            }
            else if (trimmedLine.StartsWith("{{set ") && trimmedLine.EndsWith("}}"))
            {
                if (inTrueBranchNow.All(b => b))
                {
                    var assignment = trimmedLine[6..^2].Trim();
                    setVariable(assignment);
                }
                continue;
            }
            else if (trimmedLine.StartsWith("{{include ") && trimmedLine.EndsWith("}}"))
            {
                if (inTrueBranchNow.All(b => b))
                {
                    var includeWhat = trimmedLine[10..^2].Trim();
                    var expanded = include(includeWhat)
                        .Split('\n', StringSplitOptions.None)
                        .Select(line => line.TrimEnd('\r').TrimEnd())
                        .ToList();
                    lines.InsertRange(i + 1, expanded);
                }
                continue;
            }
            if (inTrueBranchNow.All(b => b))
            {
                var updated = line.TrimEnd('\n', '\r');

                var firstLine = output.Length == 0;
                if (!firstLine) output.AppendLine();

                var inlineIfPos = updated.IndexOf("{{if ");
                var inlineEndIfPos = updated.IndexOf("{{endif}}");
                var inlineIfEndIf = inlineIfPos >= 0 && inlineEndIfPos >= 0 && inlineIfPos < inlineEndIfPos;
                if (inlineIfEndIf)
                {
                    updated = HandleInlineIf(updated, evaluateCondition, interpolate);
                }

                output.Append(interpolate(updated));
            }
        }

        return output.ToString();
    }

    private static string HandleInlineIf(string line, Func<string, bool> evaluateCondition, Func<string, string> interpolate)
    {
        var safeEvaluateCondition = TryCatchHelpers.NoThrowWrapException(evaluateCondition, false);

        var output = new StringBuilder();

        var inTrueBranchNow = new Stack<bool>();
        inTrueBranchNow.Push(true);

        var skipElseBranches = new Stack<bool>();
        skipElseBranches.Push(true);

        var chars = line.ToCharArray();
        var position = 0;
        while (position < chars.Length)
        {
            var cch = CountCharsToCheck(chars, position);
            if (cch == 1)
            {
                if (inTrueBranchNow.All(b => b))
                {
                    output.Append(chars[position]);
                }
                position++;
                continue;
            }

            var check = new string(chars, position, cch);
            position += cch;

            if (check.StartsWith("{{if ") && check.EndsWith("}}"))
            {
                var condition = check[5..^2].Trim();
                var evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex == null)
                {
                    inTrueBranchNow.Push(evaluated.value);
                    skipElseBranches.Push(evaluated.value);
                    continue;
                }
            }
            else if (check.StartsWith("{{else if ") && check.EndsWith("}}"))
            {
                if (inTrueBranchNow.Peek())
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(false);
                    // skipElseBranches.Peek() should already be true
                    continue;
                }
                else if (skipElseBranches.Peek())
                {
                    continue;
                }

                var condition = check[10..^2].Trim();
                var evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex == null)
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(evaluated.value);
                    skipElseBranches.Pop();
                    skipElseBranches.Push(evaluated.value);
                    continue;
                }
            }
            else if (check.StartsWith("{{else}}") && check.EndsWith("}}"))
            {
                if (inTrueBranchNow.Peek())
                {
                    inTrueBranchNow.Pop();
                    inTrueBranchNow.Push(false);
                    // skipElseBranches.Peek() should already be true
                    continue;
                }
                else if (skipElseBranches.Peek())
                {
                    continue;
                }

                inTrueBranchNow.Pop();
                inTrueBranchNow.Push(true);
                skipElseBranches.Pop();
                skipElseBranches.Push(true);
                continue;
            }
            else if (check.StartsWith("{{endif}}") && check.EndsWith("}}"))
            {
                inTrueBranchNow.Pop();
                skipElseBranches.Pop();
                continue;
            }
            else if (inTrueBranchNow.All(b => b))
            {
                output.Append(interpolate(check));
            }
        }

        return output.ToString();
    }

    private static int CountCharsToCheck(char[] chars, int position)
    {
        if (chars[position] != '{') return 1;

        var cch = 1;
        var braces = 1;
        for (var i = position + 1; i < chars.Length; i++)
        {
            if (chars[i] == '{')
            {
                braces++;
            }
            else if (chars[i] == '}')
            {
                braces--;
                if (braces == 0)
                {
                    cch = i - position + 1;
                    break;
                }
            }
        }

        return cch;
    }
}