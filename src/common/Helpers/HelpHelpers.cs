using System;
using System.Collections.Generic;
using System.Linq;

public class HelpHelpers
{
    public static IEnumerable<string> GetHelpTopics()
    {
        var allResourceNames = EmbeddedFileHelpers.GetEmbeddedStreamFileNames();

        var helpPrefix = $"{ProgramInfo.Name}.help.";
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
        return EmbeddedFileHelpers.EmbeddedStreamExists($"help.{topic}.txt");
    }

    public static string? GetHelpTopicText(string topic)
    {
        return EmbeddedFileHelpers.ReadEmbeddedStream($"help.{topic}.txt");
    }

    public static void DisplayUsage(string command)
    {
        var validTopic = !string.IsNullOrEmpty(command) && HelpTopicExists(command);
        var helpContent = validTopic
            ? GetHelpTopicText(command)
            : GetHelpTopicText(UsageHelpTopic);

        helpContent ??= $"USAGE: {ProgramInfo.Name} [...]";

        ConsoleHelpers.WriteLine(helpContent.TrimEnd(), overrideQuiet: true);
    }

    public static void DisplayHelpTopic(string topic, bool expandTopics = false)
    {
        topic ??= UsageHelpTopic;

        var helpTopicExists = HelpTopicExists(topic);
        if (!helpTopicExists)
        {
            if (string.IsNullOrEmpty(topic))
            {
                DisplayHelpTopic("help");
                return;
            }

            if (topic == "topics" || topic == "topics expand")
            {
                expandTopics = expandTopics || topic == "topics expand";
                DisplayHelpTopics(expandTopics);
                return;
            }

            if (topic.StartsWith("find"))
            {
                topic = topic.Substring("find".Length).Trim();
                var helpTopics = GetHelpTopics().Where(t => HelpTopicContains(t, topic)).ToList();
                if (helpTopics.Count > 0)
                {
                    DisplayHelpTopics(helpTopics, expandTopics);
                    return;
                }
            }

            ConsoleHelpers.WriteLine(
                $"  WARNING: No help topic found for '{topic}'\n\n" +
                "    " + GetHelpTopicText("help")?.Replace("\n", "\n    "),
                overrideQuiet: true
                );
            return;
        }

        var helpContent = GetHelpTopicText(topic)!;
        ConsoleHelpers.WriteLine(helpContent.TrimEnd(), overrideQuiet: true);
    }

    public static void DisplayHelpTopics(bool expandTopics)
    {
        var helpTopics = GetHelpTopics();
        DisplayHelpTopics(helpTopics, expandTopics);
    }

    public static void DisplayHelpTopics(IEnumerable<string> topics, bool expandTopics)
    {
        topics = topics.Select(t => expandTopics
            ? $"## `{ProgramInfo.Name} help {t}`\n\n```\n{GetHelpTopicText(t)}\n```\n"
            : $"  {ProgramInfo.Name} help {t}").ToList();
        ConsoleHelpers.WriteLine(string.Join("\n", topics), overrideQuiet: true);
    }

    private static bool HelpTopicContains(string topic, string searchFor)
    {
        var nameMatches = topic.Contains(searchFor, StringComparison.OrdinalIgnoreCase);
        var contentMatches = GetHelpTopicText(topic)?.Contains(searchFor, StringComparison.OrdinalIgnoreCase) ?? false;
        return nameMatches || contentMatches;
    }

    private const string UsageHelpTopic = "usage";
}
