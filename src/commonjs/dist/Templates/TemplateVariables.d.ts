import { INamedValues } from './INamedValues';
export declare class TemplateVariables implements INamedValues {
    private _variables;
    constructor(variables?: Record<string, string>);
    getValue(name: string): string | undefined;
    hasValue(name: string): boolean;
    getKeys(): string[];
    setValue(name: string, value: string): void;
    removeValue(name: string): boolean;
    clear(): void;
    static fromRecord(variables: Record<string, string>): TemplateVariables;
}
//# sourceMappingURL=TemplateVariables.d.ts.map