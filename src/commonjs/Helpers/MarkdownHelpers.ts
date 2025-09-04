/**
 * Helper class for working with Markdown formatting, particularly code blocks.
 */
export class MarkdownHelpers {
    /**
     * Calculates the required number of backticks to safely wrap content in a code block.
     * @param content The content to be wrapped in a code block
     * @returns The minimum number of backticks needed (at least 3)
     */
    public static GetCodeBlockBacktickCharCountRequired(content: string | null): number {
        let maxConsecutiveBackticks = 0;
        let currentStreak = 0;

        const actualContent = content ?? '';
        
        for (const char of actualContent) {
            if (char === '`') {
                currentStreak++;
                if (currentStreak > maxConsecutiveBackticks) {
                    maxConsecutiveBackticks = currentStreak;
                }
            } else {
                currentStreak = 0;
            }
        }

        return Math.max(3, maxConsecutiveBackticks + 1);
    }

    /**
     * Gets the appropriate backtick string for wrapping content in a code block.
     * @param content The content to be wrapped
     * @returns A string of backticks
     */
    public static GetCodeBlockBackticks(content: string | null): string {
        const backtickCount = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        return '`'.repeat(backtickCount);
    }

    /**
     * Creates a Markdown code block with the specified content and language.
     * @param content The code content to wrap
     * @param lang The language identifier for syntax highlighting (optional)
     * @param leadingLF Whether to add a leading line feed
     * @param trailingLF Whether to add a trailing line feed
     * @returns The formatted Markdown code block
     */
    public static GetCodeBlock(content: string | null, lang: string | null = null, leadingLF: boolean = false, trailingLF: boolean = false): string {
        if (!content || content.trim() === '') return '';

        let result = '';
        
        if (leadingLF) result += '\n';

        const backticks = MarkdownHelpers.GetCodeBlockBackticks(content);
        result += `${backticks}${lang ?? ''}\n`;

        result += content;
        
        // Check if content needs a trailing newline before closing backticks
        const contentNeedsLF = content.length > 0 && !content.endsWith('\n');
        if (contentNeedsLF) result += '\n';

        result += backticks;

        if (trailingLF) result += '\n';
        
        return result;
    }
}