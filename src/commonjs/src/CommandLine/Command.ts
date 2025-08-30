export abstract class Command {
    constructor() {}

    abstract isEmpty(): boolean;
    abstract getCommandName(): string;

    getHelpTopic(): string {
        const topic = this.getCommandName();
        const ok = !!(topic && topic.trim());
        return ok ? topic : "usage";
    }

    validate(): Command {
        return this;
    }

    abstract executeAsync(interactive: boolean): Promise<any>;
}