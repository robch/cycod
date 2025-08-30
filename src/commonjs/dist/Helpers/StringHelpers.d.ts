export declare class StringHelpers {
    static isNullOrEmpty(value?: string | null): boolean;
    static isNullOrWhitespace(value?: string | null): boolean;
    static trimStart(value: string, chars?: string[]): string;
    static trimEnd(value: string, chars?: string[]): string;
    static trim(value: string, chars?: string[]): string;
    static split(value: string, separator: string | string[], options?: {
        removeEmptyEntries?: boolean;
    }): string[];
    static join(separator: string, values: string[]): string;
    static format(template: string, ...args: any[]): string;
    static padLeft(value: string, totalWidth: number, paddingChar?: string): string;
    static padRight(value: string, totalWidth: number, paddingChar?: string): string;
    static contains(value: string, substring: string, ignoreCase?: boolean): boolean;
    static startsWith(value: string, prefix: string, ignoreCase?: boolean): boolean;
    static endsWith(value: string, suffix: string, ignoreCase?: boolean): boolean;
    private static escapeRegex;
}
//# sourceMappingURL=StringHelpers.d.ts.map