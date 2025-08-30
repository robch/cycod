"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ProgramRunner = void 0;
const ConsoleHelpers_1 = require("./Helpers/ConsoleHelpers");
class ProgramRunner {
    async runProgramAsync(args) {
        try {
            this.saveConsoleColor();
            return await this.doProgram(this.processDirectives(args));
        }
        catch (error) {
            if (error instanceof Error) {
                ConsoleHelpers_1.ConsoleHelpers.writeError(`Error: ${error.message}`);
                return 1;
            }
            return 1;
        }
        finally {
            this.restoreConsoleColor();
        }
    }
    async doProgram(args) {
        const parseResult = this.parseCommandLine(args);
        if (!parseResult.success) {
            this.displayBanner();
            if (parseResult.exception) {
                ConsoleHelpers_1.ConsoleHelpers.writeError(parseResult.exception.message);
                const helpTopic = parseResult.exception.getHelpTopic();
                if (helpTopic) {
                    // Display help for the specific topic
                    console.log(`\nFor more help, see: ${helpTopic}`);
                }
            }
            return parseResult.exception ? 2 : 1;
        }
        const commandLineOptions = parseResult.commandLineOptions;
        this.displayBanner();
        this.configureConsoleHelpers(commandLineOptions);
        if (this.shouldShowHelp(commandLineOptions)) {
            return await this.showHelp(commandLineOptions);
        }
        if (this.shouldShowVersion(commandLineOptions)) {
            return await this.showVersion(commandLineOptions);
        }
        return await this.runCommands(commandLineOptions);
    }
    async runCommands(commandLineOptions) {
        let exitCode = 0;
        for (const command of commandLineOptions.commands) {
            try {
                const result = await command.executeAsync(commandLineOptions.interactive);
                if (typeof result === 'number') {
                    exitCode = result;
                    if (exitCode !== 0)
                        break;
                }
            }
            catch (error) {
                ConsoleHelpers_1.ConsoleHelpers.writeError(`Command execution failed: ${error.message}`);
                exitCode = 1;
                break;
            }
        }
        return exitCode;
    }
    processDirectives(args) {
        // Simple implementation - in C# this was more complex
        return args;
    }
    shouldShowHelp(commandLineOptions) {
        return !!commandLineOptions.helpTopic || commandLineOptions.commands.length === 0;
    }
    shouldShowVersion(commandLineOptions) {
        return commandLineOptions.commands.some(cmd => cmd.getCommandName() === 'version');
    }
    async showHelp(commandLineOptions) {
        // TODO: Implement help display logic
        console.log('Help information would be displayed here');
        return 0;
    }
    async showVersion(commandLineOptions) {
        // TODO: Implement version display logic
        console.log('Version: 1.0.0');
        return 0;
    }
    displayBanner() {
        // Override in derived classes to show application banner
    }
    configureConsoleHelpers(commandLineOptions) {
        ConsoleHelpers_1.ConsoleHelpers.configure(commandLineOptions.debug, commandLineOptions.verbose, commandLineOptions.quiet);
    }
    saveConsoleColor() {
        // TODO: Implement console color saving if needed
    }
    restoreConsoleColor() {
        // TODO: Implement console color restoration if needed
    }
}
exports.ProgramRunner = ProgramRunner;
//# sourceMappingURL=ProgramRunner.js.map