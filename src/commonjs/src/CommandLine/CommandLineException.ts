export class CommandLineException extends Error {
    private _helpTopic?: string;

    constructor(message?: string, helpTopic?: string) {
        super(message);
        this.name = 'CommandLineException';
        this._helpTopic = helpTopic;
    }

    getHelpTopic(): string | undefined {
        return this._helpTopic;
    }
}