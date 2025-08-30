"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CycoDevCommandLineOptions = void 0;
const cycod_common_1 = require("cycod-common");
const ChatCommand_1 = require("../CommandLineCommands/ChatCommand");
class CycoDevCommandLineOptions extends cycod_common_1.CommandLineOptions {
    static parse(args) {
        const options = new CycoDevCommandLineOptions();
        const result = options.parse(args);
        if (result.success && options.commands.length === 1) {
            const command = options.commands[0];
            const oneChatCommandWithNoInput = command instanceof ChatCommand_1.ChatCommand &&
                command.inputInstructions.length === 0;
            const inOrOutRedirected = process.stdin.isTTY === false || process.stdout.isTTY === false;
            const implicitlyUseStdIn = oneChatCommandWithNoInput && inOrOutRedirected;
            if (implicitlyUseStdIn) {
                const stdinLines = cycod_common_1.ConsoleHelpers.getAllLinesFromStdin();
                const joined = stdinLines.join('\n');
                command.inputInstructions.push(joined);
            }
        }
        return { success: result.success, commandLineOptions: options, exception: result.exception };
    }
    peekCommandName(args, i) {
        const name1 = this.getInputOptionArgs(i, args, 1)[0];
        switch (name1) {
            case "chat":
                return "chat";
            default:
                return super.peekCommandName(args, i);
        }
    }
    checkPartialCommandNeedsHelp(commandName) {
        return commandName === "alias" ||
            commandName === "config" ||
            commandName === "github" ||
            commandName === "mcp" ||
            commandName === "prompt";
    }
    newCommandFromName(commandName) {
        switch (commandName) {
            case "chat":
            case "":
                return new ChatCommand_1.ChatCommand();
            default:
                return super.newCommandFromName(commandName);
        }
    }
    newDefaultCommand() {
        return new ChatCommand_1.ChatCommand();
    }
}
exports.CycoDevCommandLineOptions = CycoDevCommandLineOptions;
//# sourceMappingURL=CycoDevCommandLineOptions.js.map