#!/usr/bin/env node
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const CycoDevProgramRunner_1 = require("../CycoDevProgramRunner");
async function main() {
    const args = process.argv.slice(2);
    return await CycoDevProgramRunner_1.CycoDevProgramRunner.runAsync(args);
}
// Run the main function and exit with the appropriate code
main().then(exitCode => {
    process.exit(exitCode);
}).catch(error => {
    console.error('Fatal error:', error);
    process.exit(1);
});
//# sourceMappingURL=cycodjs.js.map