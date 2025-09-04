import * as path from 'path';
import { ConsoleHelpers } from './ConsoleHelpers';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';

/**
 * Provides common display methods for consistent output formatting across commands.
 */
export class CommonDisplayHelpers {
    // Constants for content display
    public static readonly DefaultIndentLevel = 2;
    public static readonly DefaultMaxContentLines = 2;
    public static readonly DefaultMaxContentWidth = 60;

    /**
     * Formats a location header with scope information.
     * @param directoryPath Directory path to display
     * @param scope The configuration scope
     * @returns Formatted location string
     */
    public static formatLocationHeader(directoryPath: string, scope: ConfigFileScope): string {
        return `${directoryPath} (${ConfigFileScope[scope].toLowerCase()})`;
    }

    /**
     * Writes a location header to the console.
     * @param locationHeader The formatted location header
     */
    public static writeLocationHeader(locationHeader: string): void {
        ConsoleHelpers.writeLine(`LOCATION: ${locationHeader}`, true);
        ConsoleHelpers.writeLine('', true);
    }

    /**
     * Displays a list of items with a location header.
     * @param locationHeader The location header to display
     * @param items List of item names to display
     * @param indentLevel Indentation level
     */
    public static displayItemList(locationHeader: string, items: string[], indentLevel: number = CommonDisplayHelpers.DefaultIndentLevel): void {
        CommonDisplayHelpers.writeLocationHeader(locationHeader);
        
        if (items.length === 0) {
            const indent = ' '.repeat(indentLevel);
            ConsoleHelpers.writeLine(`${indent}No items found in this scope`, true);
            return;
        }

        for (const item of items) {
            CommonDisplayHelpers.displayItem(item, undefined, undefined, false, indentLevel);
        }
    }

    /**
     * Displays a single item with its details.
     * @param name Name of the item
     * @param value Content of the item
     * @param location Location where the item is stored
     * @param limitValue Whether to show a preview of content rather than full content
     * @param indentLevel Indentation level
     */
    public static displayItem(
        name: string, 
        value?: string, 
        location?: string, 
        limitValue: boolean = false,
        indentLevel: number = 0): void {
        
        // Display location
        if (location && location.trim().length > 0) {
            ConsoleHelpers.writeLine(CommonDisplayHelpers.indentContent(`LOCATION: ${location}`, indentLevel), true);
            ConsoleHelpers.writeLine('', true);
            indentLevel += 2;
        }

        // Display item name
        ConsoleHelpers.writeLine(CommonDisplayHelpers.indentContent(name, indentLevel), true);
        indentLevel += 2;
        
        // Display item value
        if (value !== undefined) {
            const content = limitValue
                ? CommonDisplayHelpers.truncateContent(value, CommonDisplayHelpers.DefaultMaxContentLines, CommonDisplayHelpers.DefaultMaxContentWidth, indentLevel)
                : CommonDisplayHelpers.indentContent(value, indentLevel);
            ConsoleHelpers.writeLine(`\n${content}`, true);
        }
    }

    /**
     * Truncates content to a specified number of lines and width.
     * @param content Content to truncate
     * @param maxLines Maximum number of lines to show
     * @param maxWidth Maximum width per line
     * @param indentLevel Indentation level for content
     * @returns Truncated content string
     */
    public static truncateContent(
        content: string, 
        maxLines: number = CommonDisplayHelpers.DefaultMaxContentLines, 
        maxWidth: number = CommonDisplayHelpers.DefaultMaxContentWidth,
        indentLevel: number = CommonDisplayHelpers.DefaultIndentLevel): string {
        
        const contentLines = content
            .split(/[\n\r]/)
            .map(line => line.trim());
            
        if (contentLines.length === 0) {
            return ' '.repeat(indentLevel) + '(empty)';
        }
        
        const indent = ' '.repeat(indentLevel);
        const linesToShow = contentLines
            .slice(0, maxLines)
            .map(line => line.length > maxWidth ? line.substring(0, maxWidth - 3) + '...' : line)
            .map(line => indent + line);
            
        // Remove trailing empty lines
        while (linesToShow.length > 0 && !linesToShow[linesToShow.length - 1].trim()) {
            linesToShow.pop();
        }
        
        const totalLineCount = contentLines.length;
        let result = linesToShow.join('\n');
        
        // Add indicator of additional lines if content was truncated
        if (totalLineCount > maxLines) {
            result += `\n${indent}... (${totalLineCount - maxLines} more lines)`;
        }
        
        return result;
    }

    /**
     * Indents all lines of content with consistent spacing.
     * @param content Content to indent
     * @param indentLevel Indentation level for content
     * @returns Consistently indented content string
     */
    public static indentContent(
        content: string, 
        indentLevel: number = CommonDisplayHelpers.DefaultIndentLevel): string {
        
        // Handle empty content
        if (!content || content.length === 0) {
            return ' '.repeat(indentLevel) + '(empty)';
        }
        
        const indent = ' '.repeat(indentLevel);
        const contentLines = content
            .split(/[\n\r]/)
            .map(line => indent + line);
            
        return contentLines.join('\n');
    }

    /**
     * Displays information about saved files.
     * @param filesSaved List of saved files
     * @param usagePattern Usage pattern to display
     */
    public static displaySavedFiles(filesSaved: string[], usagePattern: string): void {
        if (filesSaved.length === 0) return;
        
        const firstFileSaved = filesSaved[0];
        const additionalFiles = filesSaved.slice(1);
        
        ConsoleHelpers.writeLine(`Saved: ${firstFileSaved}`, true);
        ConsoleHelpers.writeLine('', true);
        
        if (additionalFiles.length > 0) {
            for (const additionalFile of additionalFiles) {
                ConsoleHelpers.writeLine(`  and: ${additionalFile}`, true);
            }
            
            ConsoleHelpers.writeLine('', true);
        }
        
        const itemName = path.parse(firstFileSaved).name;
        ConsoleHelpers.writeLine(`USAGE: ${usagePattern.replace('{name}', itemName)}`, true);
    }
}