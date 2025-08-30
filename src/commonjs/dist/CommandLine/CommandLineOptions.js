"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.CommandLineOptions = void 0;
const fs = __importStar(require("fs"));
const CommandLineException_1 = require("./CommandLineException");
const HelpCommand_1 = require("../CommandLineCommands/HelpCommand");
const VersionCommand_1 = require("../CommandLineCommands/VersionCommand");
class CommandLineOptions {
    constructor() {
        this.interactive = true;
        this.debug = false;
        this.verbose = false;
        this.quiet = false;
        this.helpTopic = '';
        this.expandHelpTopics = false;
        this.threadCount = 0;
        this.workingDirectory = undefined;
        this.commands = [];
        this.allOptions = [];
        this.saveAliasName = undefined;
        this.saveAliasScope = undefined;
    }
    parse(args) {
        try {
            const allInputs = this.expandedInputsFromCommandLine(args);
            this.parseInputOptions(allInputs);
            return { success: this.commands.length > 0 };
        }
        catch (error) {
            if (error instanceof CommandLineException_1.CommandLineException) {
                return { success: false, exception: error };
            }
            throw error;
        }
    }
    peekCommandName(args, i) {
        const name1 = this.getInputOptionArgs(i, args, 1)[0] || '';
        const name2 = this.getInputOptionArgs(i + 1, args, 1)[0] || '';
        switch (name1) {
            case "version":
                return "version";
            case "help":
                return "help";
            default:
                return `${name1} ${name2}`.trim();
        }
    }
    checkPartialCommandNeedsHelp(commandName) {
        return false;
    }
    newCommandFromName(commandName) {
        switch (commandName) {
            case "help":
                return new HelpCommand_1.HelpCommand();
            case "version":
                return new VersionCommand_1.VersionCommand();
            default:
                return null;
        }
    }
    newDefaultCommand() {
        return null;
    }
    tryParseOtherCommandOptions(command, args, i, arg) {
        return false;
    }
    tryParseOtherCommandArg(command, arg) {
        return false;
    }
    expandedInputsFromCommandLine(args) {
        return args.flatMap(arg => this.expandedInput(arg));
    }
    expandedInput(input) {
        if (input.startsWith("@@")) {
            return this.expandedAtAtFileInput(input);
        }
        else if (input.startsWith("@")) {
            return [this.expandedAtFileInput(input)];
        }
        else {
            return [input];
        }
    }
    expandedAtAtFileInput(input) {
        if (!input.startsWith("@@")) {
            throw new Error("Not an @@ file input");
        }
        const fileName = input.substring(2);
        if (fs.existsSync(fileName)) {
            const content = fs.readFileSync(fileName, 'utf-8');
            const lines = content.split(/\r?\n/).filter(line => line.trim() !== '');
            return lines.flatMap(line => this.expandedInput(line));
        }
        return [input];
    }
    expandedAtFileInput(input) {
        if (!input.startsWith("@")) {
            throw new Error("Not an @ file input");
        }
        const fileName = input.substring(1);
        if (fs.existsSync(fileName)) {
            return fs.readFileSync(fileName, 'utf-8').trim();
        }
        return input;
    }
    parseInputOptions(allInputs) {
        let command = null;
        const args = this.allOptions = allInputs;
        // Expand aliases
        for (let i = 0; i < args.length; i++) {
            if (args[i].startsWith("--")) {
                this.expandAliasOptions(command, args, i, args[i].substring(2));
            }
        }
        for (let i = 0; i < args.length; i++) {
            const iRef = { value: i };
            const parsed = this.tryParseInputOptions(command, args, iRef, args[i]);
            i = iRef.value;
            if (!parsed) {
                throw this.invalidArgException(command, args[i]);
            }
            if (command && !command.isEmpty()) {
                command = this.updateCommand(command);
            }
        }
        if (args.length === 0) {
            command = this.newDefaultCommand();
        }
        if (!this.helpTopic && command && command.isEmpty()) {
            this.helpTopic = command.getHelpTopic();
        }
        if (command && !command.isEmpty()) {
            this.commands.push(command.validate());
        }
    }
    updateCommand(command) {
        // Logic for updating command would go here
        return command;
    }
    tryParseInputOptions(command, args, i, arg) {
        const isEndOfCommand = arg === "--" && command && !command.isEmpty();
        if (isEndOfCommand) {
            this.commands.push(command.validate());
            command = null;
            return true;
        }
        const needNewCommand = command === null;
        if (needNewCommand) {
            const commandName = this.peekCommandName(args, i.value);
            const partialCommandNeedsHelp = this.checkPartialCommandNeedsHelp(commandName);
            if (partialCommandNeedsHelp) {
                command = new HelpCommand_1.HelpCommand();
                this.helpTopic = commandName;
                return true;
            }
            command = this.newCommandFromName(commandName);
            if (command) {
                const skipCount = commandName.split(' ').length - 1;
                i.value += skipCount;
                return true;
            }
            command = this.newDefaultCommand();
        }
        const parsedOption = this.tryParseGlobalCommandLineOptions(args, i, arg) ||
            this.tryParseHelpCommandOptions(command, args, i, arg) ||
            this.tryParseVersionCommandOptions(command, args, i, arg) ||
            this.tryParseOtherCommandOptions(command, args, i, arg);
        if (parsedOption)
            return true;
        if (arg === "--help") {
            this.helpTopic = command?.getHelpTopic() || "usage";
            command = new HelpCommand_1.HelpCommand();
            i.value = args.length;
            return true;
        }
        else if (arg === "--version") {
            command = new VersionCommand_1.VersionCommand();
            i.value = args.length;
            return true;
        }
        else if (command instanceof HelpCommand_1.HelpCommand) {
            this.helpTopic = `${this.helpTopic} ${arg}`.trim();
            return true;
        }
        return this.tryParseOtherCommandArg(command, arg);
    }
    expandAliasOptions(command, args, currentIndex, alias) {
        // TODO: Implement alias expansion
    }
    tryParseGlobalCommandLineOptions(args, i, arg) {
        switch (arg) {
            case "--and":
                // ignore --and ... used when combining @@ files
                return true;
            case "--interactive":
                const interactiveArgs = this.getInputOptionArgs(i.value + 1, args, 1);
                const interactive = interactiveArgs[0] || "true";
                this.interactive = interactive.toLowerCase() === "true" || interactive === "1";
                i.value += interactiveArgs.length;
                return true;
            case "--debug":
                this.debug = true;
                return true;
            case "--verbose":
                this.verbose = true;
                return true;
            case "--quiet":
                this.quiet = true;
                return true;
            case "--threads":
                if (i.value + 1 < args.length) {
                    this.threadCount = this.validateInt(arg, args[++i.value], "thread count");
                }
                return true;
            case "--working-dir":
            case "--folder":
            case "--dir":
            case "--cwd":
                const dirArgs = this.getInputOptionArgs(i.value + 1, args, 1);
                this.workingDirectory = this.validateString(arg, dirArgs[0], "directory path");
                i.value += dirArgs.length;
                return true;
            default:
                return false;
        }
    }
    tryParseHelpCommandOptions(helpCommand, args, i, arg) {
        if (!helpCommand)
            return false;
        if (arg === "--expand") {
            this.expandHelpTopics = true;
            return true;
        }
        return false;
    }
    tryParseVersionCommandOptions(versionCommand, args, i, arg) {
        if (!versionCommand)
            return false;
        return false;
    }
    getInputOptionArgs(startAt, args, max = Number.MAX_SAFE_INTEGER) {
        const result = [];
        let found = 0;
        for (let i = startAt; i < args.length && found < max; i++, found++) {
            if (args[i].startsWith("--") && found >= 0) {
                break;
            }
            result.push(args[i]);
        }
        return result;
    }
    validateString(arg, argStr, argDescription) {
        if (!argStr || !argStr.trim()) {
            throw new CommandLineException_1.CommandLineException(`Missing ${argDescription} for ${arg}`);
        }
        return argStr;
    }
    validateInt(arg, countStr, argDescription) {
        if (!countStr || !countStr.trim()) {
            throw new CommandLineException_1.CommandLineException(`Missing ${argDescription} for ${arg}`);
        }
        const count = parseInt(countStr, 10);
        if (isNaN(count)) {
            throw new CommandLineException_1.CommandLineException(`Invalid ${argDescription} for ${arg}: ${countStr}`);
        }
        return count;
    }
    invalidArgException(command, arg) {
        const message = `Invalid argument: ${arg}`;
        return new CommandLineException_1.CommandLineException(message, command?.getHelpTopic() || "");
    }
}
exports.CommandLineOptions = CommandLineOptions;
//# sourceMappingURL=CommandLineOptions.js.map