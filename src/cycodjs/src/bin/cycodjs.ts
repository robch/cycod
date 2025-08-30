#!/usr/bin/env node

import { CycoDevProgramRunner } from '../CycoDevProgramRunner';

async function main(): Promise<number> {
    const args = process.argv.slice(2);
    return await CycoDevProgramRunner.runAsync(args);
}

// Run the main function and exit with the appropriate code
main().then(exitCode => {
    process.exit(exitCode);
}).catch(error => {
    console.error('Fatal error:', error);
    process.exit(1);
});