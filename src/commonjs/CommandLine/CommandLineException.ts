export class CommandLineException extends Error {
    private _helpTopic: string | null;

    public constructor();
    public constructor(message: string, helpTopic?: string | null);
    public constructor(message?: string, helpTopic?: string | null) {
        super(message);
        this.name = 'CommandLineException';
        this._helpTopic = helpTopic ?? null;
    }

    public GetHelpTopic(): string | null {
        return this._helpTopic;
    }
}
