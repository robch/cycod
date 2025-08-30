import { INamedValues } from './INamedValues';

export class TemplateVariables implements INamedValues {
    private _variables: Map<string, string> = new Map();

    constructor(variables?: Record<string, string>) {
        if (variables) {
            for (const [key, value] of Object.entries(variables)) {
                this._variables.set(key, value);
            }
        }
    }

    getValue(name: string): string | undefined {
        return this._variables.get(name);
    }

    hasValue(name: string): boolean {
        return this._variables.has(name);
    }

    getKeys(): string[] {
        return Array.from(this._variables.keys());
    }

    setValue(name: string, value: string): void {
        this._variables.set(name, value);
    }

    removeValue(name: string): boolean {
        return this._variables.delete(name);
    }

    clear(): void {
        this._variables.clear();
    }

    static fromRecord(variables: Record<string, string>): TemplateVariables {
        return new TemplateVariables(variables);
    }
}