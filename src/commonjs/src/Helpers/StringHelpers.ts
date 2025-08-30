export class StringHelpers {
    static isNullOrEmpty(value?: string | null): boolean {
        return value == null || value.length === 0;
    }

    static isNullOrWhitespace(value?: string | null): boolean {
        return value == null || value.trim().length === 0;
    }

    static trimStart(value: string, chars?: string[]): string {
        if (!chars || chars.length === 0) {
            return value.trimStart();
        }

        let startIndex = 0;
        while (startIndex < value.length && chars.includes(value[startIndex])) {
            startIndex++;
        }

        return value.substring(startIndex);
    }

    static trimEnd(value: string, chars?: string[]): string {
        if (!chars || chars.length === 0) {
            return value.trimEnd();
        }

        let endIndex = value.length - 1;
        while (endIndex >= 0 && chars.includes(value[endIndex])) {
            endIndex--;
        }

        return value.substring(0, endIndex + 1);
    }

    static trim(value: string, chars?: string[]): string {
        return this.trimStart(this.trimEnd(value, chars), chars);
    }

    static split(value: string, separator: string | string[], options?: { removeEmptyEntries?: boolean }): string[] {
        const separators = Array.isArray(separator) ? separator : [separator];
        const regex = new RegExp(`[${separators.map(s => this.escapeRegex(s)).join('')}]`);
        
        const parts = value.split(regex);
        
        if (options?.removeEmptyEntries) {
            return parts.filter(part => part.length > 0);
        }

        return parts;
    }

    static join(separator: string, values: string[]): string {
        return values.join(separator);
    }

    static format(template: string, ...args: any[]): string {
        return template.replace(/{(\d+)}/g, (match, index) => {
            const argIndex = parseInt(index, 10);
            return args[argIndex] !== undefined ? String(args[argIndex]) : match;
        });
    }

    static padLeft(value: string, totalWidth: number, paddingChar: string = ' '): string {
        if (value.length >= totalWidth) {
            return value;
        }

        const padding = paddingChar.repeat(totalWidth - value.length);
        return padding + value;
    }

    static padRight(value: string, totalWidth: number, paddingChar: string = ' '): string {
        if (value.length >= totalWidth) {
            return value;
        }

        const padding = paddingChar.repeat(totalWidth - value.length);
        return value + padding;
    }

    static contains(value: string, substring: string, ignoreCase: boolean = false): boolean {
        if (ignoreCase) {
            return value.toLowerCase().includes(substring.toLowerCase());
        }
        return value.includes(substring);
    }

    static startsWith(value: string, prefix: string, ignoreCase: boolean = false): boolean {
        if (ignoreCase) {
            return value.toLowerCase().startsWith(prefix.toLowerCase());
        }
        return value.startsWith(prefix);
    }

    static endsWith(value: string, suffix: string, ignoreCase: boolean = false): boolean {
        if (ignoreCase) {
            return value.toLowerCase().endsWith(suffix.toLowerCase());
        }
        return value.endsWith(suffix);
    }

    private static escapeRegex(str: string): string {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
}