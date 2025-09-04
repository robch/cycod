/**
 * String manipulation utilities helper class
 */
export class StringHelpers {
    public static replaceOnce(fileContent: string, oldStr: string, newStr: string): { result: string | null, countFound: number }
    {
        const exactResult = StringHelpers.exactlyReplaceOnce(fileContent, oldStr, newStr);
        if (exactResult.result != null && exactResult.countFound === 1) {
            return exactResult;
        }

        return StringHelpers.fuzzyReplaceOnce(fileContent, oldStr, newStr);
    }

    public static exactlyReplaceOnce(fileContent: string, oldStr: string, newStr: string): { result: string | null, countFound: number }
    {
        let countFound = 0;

        const bothEmpty = (!fileContent || fileContent === '') && (!oldStr || oldStr === '');
        if (bothEmpty) {
            countFound = 1;
            return { result: newStr, countFound };
        }

        const first = fileContent.indexOf(oldStr);
        if (first === -1) {
            return { result: null, countFound };
        }
        
        const second = fileContent.indexOf(oldStr, first + 1);
        if (second !== -1) {
            countFound = 2;
            return { result: null, countFound };
        }

        countFound = 1;
        const newFileContent = fileContent.substring(0, first) + newStr + fileContent.substring(first + oldStr.length);
        return { result: newFileContent, countFound };
    }

    public static fuzzyReplaceOnce(fileContent: string, oldStr: string, newStr: string): { result: string | null, countFound: number }
    {
        let countFound = 0;
        
        const escapedLines = oldStr
            .split('\n')
            .map(line => line.replace(/\r$/, '').trimEnd())
            .map(line => StringHelpers.escapeRegex(line));

        const linesWithTrailingWsPattern = escapedLines
            .slice(0, -1)
            .map(line => `${line}[${StringHelpers.WhitespaceChars}]*$`)
            .concat(escapedLines.slice(-1));

        const joined = linesWithTrailingWsPattern.join('\n');
        const pattern = new RegExp(joined, 'gm');

        const matches = Array.from(fileContent.matchAll(pattern));
        countFound = matches.length;

        const foundOnlyOnce = countFound === 1;
        if (!foundOnlyOnce) {
            return { result: null, countFound };
        }

        const match = matches[0];
        const start = match.index!;
        const end = match.index! + match[0].length;

        return { result: fileContent.substring(0, start) + newStr + fileContent.substring(end), countFound };
    }

    private static readonly WhitespaceChars = '\\t\\f\\v\\r\\u00A0\\u2000-\\u200A\\u2028\\u2029\\u3000 ';
    
    private static escapeRegex(str: string): string {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
}