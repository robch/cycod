@echo off
REM Console GUI Implementation - Iterative Script (Windows)

set MEMENTO_FILE=console-gui-implementation-memento.md
set MAX_ITERATIONS=100

if not exist "%MEMENTO_FILE%" (
    echo ERROR: %MEMENTO_FILE% not found!
    echo Make sure you're in the correct directory
    exit /b 1
)

echo === Console GUI Iterative Implementation ===
echo Memento file: %MEMENTO_FILE%
echo Max iterations: %MAX_ITERATIONS%
echo.

for /L %%i in (1,1,%MAX_ITERATIONS%) do (
    echo ==========================================
    echo ITERATION #%%i
    echo ==========================================
    echo.
    
    echo ^>^>^> Step 1: Reading memento and executing next action...
    cycod --input "Read the file %MEMENTO_FILE% carefully. Look at 'Current Position' and 'Next Action Required'. Execute the next action. When complete: 1. Test it works 2. Document in console-gui-day-N.md 3. Update memento with new position 4. Commit to git. If complete, respond with: DONE"
    
    echo.
    echo ^>^>^> Step 2: Verifying completion...
    cycod --input "Read %MEMENTO_FILE% again. Double-check the work is complete: Did you update memento? Create daily file? Commit to git? Test it? If TRULY complete, respond with: VERIFIED COMPLETE"
    
    echo.
    echo ^>^>^> Iteration %%i complete!
    echo.
    
    timeout /t 2 /nobreak >nul
)

echo.
echo ==========================================
echo FINAL CHECK
echo ==========================================
cycod --input "Read %MEMENTO_FILE%. Summarize: What phase? What percentage complete? What's been done? What's next? If all phases complete, respond with: PROJECT COMPLETE!"

echo.
echo Script complete!
