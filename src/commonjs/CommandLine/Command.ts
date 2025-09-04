/**
 * Base abstract class for all commands in the application.
 * Provides the foundation for command structure and execution.
 */
export abstract class Command {
    /**
     * Creates a new instance of the Command class.
     */
    public constructor() {
    }

    /**
     * Determines whether this command is empty (has no meaningful content).
     * @returns True if the command is empty, false otherwise.
     */
    public abstract IsEmpty(): boolean;

    /**
     * Gets the name of this command.
     * @returns The command name as a string.
     */
    public abstract GetCommandName(): string;

    /**
     * Gets the help topic for this command.
     * @returns The help topic name, or "usage" if no specific topic is available.
     */
    public GetHelpTopic(): string {
        const topic = this.GetCommandName();
        const ok = !!(topic && topic.trim());
        return ok ? topic : "usage";
    }

    /**
     * Validates this command's configuration and parameters.
     * @returns This command instance after validation.
     */
    public Validate(): Command {
        return this;
    }

    /**
     * Executes this command asynchronously.
     * @param interactive Whether the command should run in interactive mode.
     * @returns A promise that resolves to the command execution result.
     */
    public abstract ExecuteAsync(interactive: boolean): Promise<any>;
}
