export class CycoDevProgramInfo {
    readonly name: string = "cycodjs";
    readonly version: string = "1.0.0";
    readonly description: string = "AI-powered CLI application for chat-based interaction with AI assistants";
    readonly author: string = "Rob Chambers";

    getDisplayName(): string {
        return `${this.name} v${this.version}`;
    }

    getFullDescription(): string {
        return `${this.name} - ${this.description}`;
    }
}