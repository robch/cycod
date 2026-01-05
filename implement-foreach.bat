@echo off
REM Simple version using cycod --foreach for iterative implementation

set MEMENTO_FILE=console-gui-implementation-memento.md

echo === Console GUI Implementation - Foreach Loop ===
echo.

REM Use cycod's --foreach to iterate 1-100 times
cycod --rs --threads 1 --foreach var i in 1..100 --input "ITERATION {i} of 100 - Read %MEMENTO_FILE% and check the 'Current Position' and 'Next Action Required'. If the position says COMPLETE or Phase 7 is done, respond with 'ALL DONE' and stop. Otherwise: 1. Execute the next action specified in the memento 2. Test that it works 3. Update/create console-gui-day-N.md with what you did 4. Update %MEMENTO_FILE% with new 'Current Position' 5. Commit to git. Then respond with: DONE WITH STEP: [what you completed] and NEXT: [what comes next]. If blocked, respond with: BLOCKED: [issue]"

echo.
echo Foreach loop complete!
