import { CommandWithVariables } from 'cycod-common';

export class ChatCommand extends CommandWithVariables {
    systemPrompt: string = '';
    systemPromptAdds: string[] = [];
    userPromptAdds: string[] = [];
    maxPromptTokenTarget: number = 0;
    maxToolTokenTarget: number = 0;
    maxOutputTokens: number = 0;
    maxChatTokenTarget: number = 0;
    loadMostRecentChatHistory: boolean = false;
    inputChatHistory: string = '';
    outputChatHistory: string = '';
    outputTrajectory: string = '';
    autoSaveOutputChatHistory: boolean = false;
    autoSaveOutputTrajectory: boolean = false;
    inputInstructions: string[] = [];
    useTemplates: boolean = true;
    useMcps: string[] = [];
    withStdioMcps: Map<string, any> = new Map(); // TODO: Define proper type
    imagePatterns: string[] = [];

    constructor() {
        super();
    }

    isEmpty(): boolean {
        return false;
    }

    getCommandName(): string {
        return "";
    }

    clone(): ChatCommand {
        const clone = new ChatCommand();
        
        // Copy all properties
        clone.systemPrompt = this.systemPrompt;
        clone.systemPromptAdds = [...this.systemPromptAdds];
        clone.userPromptAdds = [...this.userPromptAdds];
        clone.maxPromptTokenTarget = this.maxPromptTokenTarget;
        clone.maxToolTokenTarget = this.maxToolTokenTarget;
        clone.maxOutputTokens = this.maxOutputTokens;
        clone.maxChatTokenTarget = this.maxChatTokenTarget;
        clone.loadMostRecentChatHistory = this.loadMostRecentChatHistory;
        clone.inputChatHistory = this.inputChatHistory;
        clone.outputChatHistory = this.outputChatHistory;
        clone.outputTrajectory = this.outputTrajectory;
        clone.autoSaveOutputChatHistory = this.autoSaveOutputChatHistory;
        clone.autoSaveOutputTrajectory = this.autoSaveOutputTrajectory;
        clone.inputInstructions = [...this.inputInstructions];
        clone.useTemplates = this.useTemplates;
        
        // Deep copy variables Map
        clone.variables = new Map(this.variables);
        
        // Deep copy other properties
        clone.useMcps = [...this.useMcps];
        clone.withStdioMcps = new Map(this.withStdioMcps);
        clone.imagePatterns = [...this.imagePatterns];
        
        return clone;
    }

    async executeAsync(interactive: boolean): Promise<any> {
        // TODO: Implement chat execution logic
        console.log('Chat command executed with instructions:', this.inputInstructions);
        return 0;
    }
}