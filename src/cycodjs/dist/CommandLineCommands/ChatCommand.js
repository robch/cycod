"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ChatCommand = void 0;
const cycod_common_1 = require("cycod-common");
class ChatCommand extends cycod_common_1.CommandWithVariables {
    constructor() {
        super();
        this.systemPrompt = '';
        this.systemPromptAdds = [];
        this.userPromptAdds = [];
        this.maxPromptTokenTarget = 0;
        this.maxToolTokenTarget = 0;
        this.maxOutputTokens = 0;
        this.maxChatTokenTarget = 0;
        this.loadMostRecentChatHistory = false;
        this.inputChatHistory = '';
        this.outputChatHistory = '';
        this.outputTrajectory = '';
        this.autoSaveOutputChatHistory = false;
        this.autoSaveOutputTrajectory = false;
        this.inputInstructions = [];
        this.useTemplates = true;
        this.useMcps = [];
        this.withStdioMcps = new Map(); // TODO: Define proper type
        this.imagePatterns = [];
    }
    isEmpty() {
        return false;
    }
    getCommandName() {
        return "";
    }
    clone() {
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
    async executeAsync(interactive) {
        // TODO: Implement chat execution logic
        console.log('Chat command executed with instructions:', this.inputInstructions);
        return 0;
    }
}
exports.ChatCommand = ChatCommand;
//# sourceMappingURL=ChatCommand.js.map