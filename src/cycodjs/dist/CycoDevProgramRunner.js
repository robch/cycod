"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CycoDevProgramRunner = void 0;
const cycod_common_1 = require("cycod-common");
const CycoDevCommandLineOptions_1 = require("./CommandLine/CycoDevCommandLineOptions");
const CycoDevProgramInfo_1 = require("./CycoDevProgramInfo");
class CycoDevProgramRunner extends cycod_common_1.ProgramRunner {
    constructor() {
        super(...arguments);
        this._programInfo = new CycoDevProgramInfo_1.CycoDevProgramInfo();
    }
    static async runAsync(args) {
        try {
            const program = new CycoDevProgramRunner();
            return await program.runProgramAsync(args);
        }
        finally {
            cycod_common_1.ShellSession.disposeAll();
        }
    }
    parseCommandLine(args) {
        return CycoDevCommandLineOptions_1.CycoDevCommandLineOptions.parse(args);
    }
    displayBanner() {
        // TODO: Implement cycod banner display
        console.log('CycoDJS - AI-powered CLI');
    }
}
exports.CycoDevProgramRunner = CycoDevProgramRunner;
//# sourceMappingURL=CycoDevProgramRunner.js.map