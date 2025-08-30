import { CommandWithVariables } from 'cycod-common';
export declare class ChatCommand extends CommandWithVariables {
    systemPrompt: string;
    systemPromptAdds: string[];
    userPromptAdds: string[];
    maxPromptTokenTarget: number;
    maxToolTokenTarget: number;
    maxOutputTokens: number;
    maxChatTokenTarget: number;
    loadMostRecentChatHistory: boolean;
    inputChatHistory: string;
    outputChatHistory: string;
    outputTrajectory: string;
    autoSaveOutputChatHistory: boolean;
    autoSaveOutputTrajectory: boolean;
    inputInstructions: string[];
    useTemplates: boolean;
    useMcps: string[];
    withStdioMcps: Map<string, any>;
    imagePatterns: string[];
    constructor();
    isEmpty(): boolean;
    getCommandName(): string;
    clone(): ChatCommand;
    executeAsync(interactive: boolean): Promise<any>;
}
//# sourceMappingURL=ChatCommand.d.ts.map