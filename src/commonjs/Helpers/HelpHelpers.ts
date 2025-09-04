import { EmbeddedFileHelpers } from './EmbeddedFileHelpers';
import { ProgramInfo } from './ProgramInfo';
import { ConsoleHelpers } from './ConsoleHelpers';

/**
 * Helper class for managing help topics and displaying help content.
 */
export class HelpHelpers {
    private static readonly UsageHelpTopic = "usage";

    /**
     * Gets all available help topics from embedded resources.
     * @returns Array of help topic names
     */
    public static GetHelpTopics(): string[] {
        const allResourceNames = EmbeddedFileHelpers.GetEmbeddedStreamFileNames();

        const helpPrefix = `${ProgramInfo.Name}.help.`;
        const helpTopics = allResourceNames
            .filter(name => name.toLowerCase().startsWith(helpPrefix.toLowerCase()))
            .map(name => name.substring(helpPrefix.length))
            .map(name => name.substring(0, name.length - '.txt'.length))
            .filter((value, index, self) => self.indexOf(value) === index) // Distinct
            .sort((a, b) => {
                // Sort by space count first, then alphabetically
                const aSpaceCount = (a.match(/ /g) || []).length;
                const bSpaceCount = (b.match(/ /g) || []).length;
                const spaceDiff = aSpaceCount - bSpaceCount;
                return spaceDiff !== 0 ? spaceDiff : a.localeCompare(b);
            });

        return helpTopics;
    }

    /**
     * Checks if a help topic exists.
     * @param topic The help topic name
     * @returns True if the topic exists
     */
    public static HelpTopicExists(topic: string): boolean {
        return EmbeddedFileHelpers.EmbeddedStreamExists(`help.${topic}.txt`);
    }

    /**
     * Gets the text content for a help topic.
     * @param topic The help topic name
     * @returns The help topic text, or null if not found
     */
    public static GetHelpTopicText(topic: string): string | null {
        return EmbeddedFileHelpers.ReadEmbeddedStream(`help.${topic}.txt`);
    }

    /**
     * Displays usage information for a specific help topic or general usage.
     * @param helpTopic The help topic to display, or null for general usage
     */
    public static DisplayUsage(helpTopic: string | null): void {
        const validTopic = helpTopic && helpTopic.trim() !== '' && HelpHelpers.HelpTopicExists(helpTopic);
        const helpContent = validTopic
            ? HelpHelpers.GetHelpTopicText(helpTopic)
            : HelpHelpers.GetHelpTopicText(HelpHelpers.UsageHelpTopic);

        const finalContent = helpContent ?? `USAGE: ${ProgramInfo.Name} [...]`;

        ConsoleHelpers.WriteLine(finalContent.trimEnd(), undefined, true);
    }

    /**
     * Displays a specific help topic or handles special topic commands.
     * @param topic The help topic to display
     * @param expandTopics Whether to expand topic listings with full content
     */
    public static DisplayHelpTopic(topic: string, expandTopics: boolean = false): void {
        const actualTopic = topic ?? HelpHelpers.UsageHelpTopic;

        const helpTopicExists = HelpHelpers.HelpTopicExists(actualTopic);
        if (!helpTopicExists) {
            if (!actualTopic || actualTopic.trim() === '') {
                HelpHelpers.DisplayHelpTopic("help");
                return;
            }

            if (actualTopic === "topics" || actualTopic === "topics expand") {
                const shouldExpand = expandTopics || actualTopic === "topics expand";
                HelpHelpers.DisplayHelpTopics(shouldExpand);
                return;
            }

            if (actualTopic.startsWith("find")) {
                const searchTerm = actualTopic.substring("find".length).trim();
                const matchingTopics = HelpHelpers.GetHelpTopics().filter(t => 
                    HelpHelpers.HelpTopicContains(t, searchTerm));
                
                if (matchingTopics.length > 0) {
                    HelpHelpers.DisplayHelpTopicsArray(matchingTopics, expandTopics);
                    return;
                }
            }

            const helpTopicText = HelpHelpers.GetHelpTopicText("help");
            const indentedHelp = helpTopicText?.replace(/\n/g, "\n    ") || "";
            
            ConsoleHelpers.WriteLine(
                `  WARNING: No help topic found for '${actualTopic}'\n\n    ${indentedHelp}`,
                undefined,
                true
            );
            return;
        }

        const helpContent = HelpHelpers.GetHelpTopicText(actualTopic);
        if (helpContent) {
            ConsoleHelpers.WriteLine(helpContent.trimEnd(), undefined, true);
        }
    }

    /**
     * Displays all available help topics.
     * @param expandTopics Whether to expand topics with full content
     */
    public static DisplayHelpTopics(expandTopics: boolean): void {
        const helpTopics = HelpHelpers.GetHelpTopics();
        HelpHelpers.DisplayHelpTopicsArray(helpTopics, expandTopics);
    }

    /**
     * Displays a specific array of help topics.
     * @param topics The topics to display
     * @param expandTopics Whether to expand topics with full content
     */
    public static DisplayHelpTopicsArray(topics: string[], expandTopics: boolean): void {
        const formattedTopics = topics.map(t => expandTopics
            ? `## \`${ProgramInfo.Name} help ${t}\`\n\n\`\`\`\n${HelpHelpers.GetHelpTopicText(t) || ''}\n\`\`\`\n`
            : `  ${ProgramInfo.Name} help ${t}`);
        
        ConsoleHelpers.WriteLine(formattedTopics.join("\n"), undefined, true);
    }

    /**
     * Checks if a help topic contains the specified search term.
     * @param topic The topic name to check
     * @param searchFor The search term
     * @returns True if the topic name or content contains the search term
     */
    private static HelpTopicContains(topic: string, searchFor: string): boolean {
        const nameMatches = topic.toLowerCase().includes(searchFor.toLowerCase());
        const topicText = HelpHelpers.GetHelpTopicText(topic);
        const contentMatches = topicText?.toLowerCase().includes(searchFor.toLowerCase()) ?? false;
        return nameMatches || contentMatches;
    }
}
