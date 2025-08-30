export declare abstract class Command {
    constructor();
    abstract isEmpty(): boolean;
    abstract getCommandName(): string;
    getHelpTopic(): string;
    validate(): Command;
    abstract executeAsync(interactive: boolean): Promise<any>;
}
//# sourceMappingURL=Command.d.ts.map