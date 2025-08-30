export class ForEachVariable {
    readonly name: string;
    readonly values: string[];

    constructor(name: string, values: string[]) {
        this.name = name;
        this.values = values;
    }
}