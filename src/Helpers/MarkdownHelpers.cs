using System;
using System.Text;

class MarkdownHelpers
{
    public static int GetCodeBlockBacktickCharCountRequired(string? content)
    {
        int maxConsecutiveBackticks = 0;
        int currentStreak = 0;

        content ??= string.Empty;
        foreach (char c in content)
        {
            if (c == '`')
            {
                currentStreak++;
                if (currentStreak > maxConsecutiveBackticks)
                {
                    maxConsecutiveBackticks = currentStreak;
                }
            }
            else
            {
                currentStreak = 0;
            }
        }

        return Math.Max(3, maxConsecutiveBackticks + 1);
    }

    public static string GetCodeBlockBackticks(string? content)
    {
        int backtickCount = GetCodeBlockBacktickCharCountRequired(content);
        return new string('`', backtickCount);
    }

    public static string GetCodeBlock(string? content, string? lang = null, bool leadingLF = false, bool trailingLF = false)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        var sb = new StringBuilder();
        if (leadingLF) sb.AppendLine();

        var backticks = GetCodeBlockBackticks(content);
        sb.AppendLine($"{backticks}{lang}");
        sb.Append(content);
        sb.AppendLine(backticks);

        if (trailingLF) sb.AppendLine();

        return sb.ToString();
    }
}