export interface INamedValues {
    getValue(name: string): string | undefined;
    hasValue(name: string): boolean;
    getKeys(): string[];
}