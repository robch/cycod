using System;
using System.Collections.Generic;
using System.Linq;

class HelpHelpers
{
    public static IEnumerable<string> GetHelpTopics()
    {
        var allResourceNames = FileHelpers.GetEmbeddedStreamFileNames();

        var helpPrefix = $"{Program.Name}.help.";
        var helpTopics = allResourceNames
            .Where(name => name.StartsWith(helpPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring(helpPrefix.Length))
            .Select(name => name.Substring(0, name.Length - ".txt".Length))
            .Distinct()
            .OrderBy(name => name.Count(x => x == ' ').ToString("000") + name)
            .ToList();

        return helpTopics;
    }

    public static bool HelpTopicExists(string topic)
    {
        return FileHelpers.EmbeddedStreamExists($"help.{topic}.txt");
    }

    public static string GetHelpTopicText(string topic)
    {
        return FileHelpers.ReadEmbeddedStream($"help.{topic}.txt");
    }

    public static void PrintUsage(string command)
    {
        var validTopic = !string.IsNullOrEmpty(command) && HelpTopicExists(command);
        var helpContent = validTopic
            ? GetHelpTopicText(command)
            : GetHelpTopicText(UsageHelpTopic);

        helpContent ??=
            $"USAGE: {Program.Name} [file1 [file2 [pattern1 [pattern2 [...]]]]] [...]\n" +
            $"   OR: {Program.Name} web search \"TERMS\" [...]\n" +
            $"   OR: {Program.Name} web get \"URL\" [...]";

        ConsoleHelpers.PrintLine(helpContent.TrimEnd());
    }

    public static void PrintHelpTopic(string topic, bool expandTopics = false)
    {
        topic ??= UsageHelpTopic;

        var helpTopicExists = HelpTopicExists(topic);
        if (!helpTopicExists)
        {
            if (string.IsNullOrEmpty(topic))
            {
                PrintHelpTopic("help");
                return;
            }

            if (topic == "topics" || topic == "topics expand")
            {
                expandTopics = expandTopics || topic == "topics expand";
                PrintHelpTopics(expandTopics);
                return;
            }

            if (topic.StartsWith("find"))
            {
                topic = topic.Substring("find".Length).Trim();
                var helpTopics = GetHelpTopics().Where(t => HelpTopicContains(t, topic)).ToList();
                if (helpTopics.Count > 0)
                {
                    PrintHelpTopics(helpTopics, expandTopics);
                    return;
                }
            }

            ConsoleHelpers.PrintLine(
                $"  WARNING: No help topic found for '{topic}'\n\n" +
                "    " + GetHelpTopicText("help").Replace("\n", "\n    ")
                );
            return;
        }

        var helpContent = GetHelpTopicText(topic);
        ConsoleHelpers.PrintLine(helpContent.TrimEnd());
    }

    public static void PrintHelpTopics(bool expandTopics)
    {
        var helpTopics = GetHelpTopics();
        PrintHelpTopics(helpTopics, expandTopics);
    }

    public static void PrintHelpTopics(IEnumerable<string> topics, bool expandTopics)
    {
        topics = topics.Select(t => expandTopics
            ? $"## `{Program.Name} help {t}`\n\n```\n{GetHelpTopicText(t)}\n```\n"
            : $"  {Program.Name} help {t}").ToList();
        ConsoleHelpers.PrintLine(string.Join("\n", topics));
    }

    private static bool HelpTopicContains(string topic, string searchFor)
    {
        var nameMatches = topic.Contains(searchFor, StringComparison.OrdinalIgnoreCase);
        var contentMatches = GetHelpTopicText(topic).Contains(searchFor, StringComparison.OrdinalIgnoreCase);
        return nameMatches || contentMatches;
    }

    private const string UsageHelpTopic = "usage";
}
