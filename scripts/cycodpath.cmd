@echo off

REM ===== CYCODPATH.CMD =====
REM Finds all cycod executables (main apps and MCP servers) and adds their folders to PATH
REM Prioritizes Debug over Release, newest timestamp as fallback

echo Finding all cycod executables (main apps and MCP servers)...
echo.

REM Initialize variables
set "folders_to_add="
set "found_cycod="
set "found_cycodt="
set "found_cycodmd="
set "found_cycodgr="
set "found_cycod-mcp-geolocation="
set "found_cycod-mcp-mxlookup="
set "found_cycod-mcp-osm="
set "found_cycod-mcp-weather="
set "found_cycod-mcp-whois="
set "folder_count=0"

REM ===== MAIN SEARCH LOOP =====
for %%e in (cycod cycodt cycodmd cycodgr cycod-mcp-geolocation cycod-mcp-mxlookup cycod-mcp-osm cycod-mcp-weather cycod-mcp-whois) do (
    call :FindExecutable %%e
)

REM ===== UPDATE PATH =====
call :UpdatePath

REM ===== SUMMARY =====
call :ShowSummary

goto :EOF

REM ===== SUBROUTINES =====

:FindExecutable
set "exe_name=%1"
set "found_path="

REM Try direct paths first - Debug priority
if exist "bin\Debug\net9.0\%exe_name%.exe" (
    set "found_path=bin\Debug\net9.0\%exe_name%.exe"
    goto :FoundDirect
)

if exist "bin\Release\net9.0\%exe_name%.exe" (
    set "found_path=bin\Release\net9.0\%exe_name%.exe"
    goto :FoundDirect
)

REM Try other common debug/release patterns
for %%p in ("bin\Debug\%exe_name%.exe" "bin\Release\%exe_name%.exe" "bin\x64\Debug\%exe_name%.exe" "bin\x64\Release\%exe_name%.exe") do (
    if exist %%p (
        set "found_path=%%~p"
        goto :FoundDirect
    )
)

REM Fallback: Use forfiles to find newest by timestamp
set "temp_file=%TEMP%\cycodpath_temp_%RANDOM%.txt"
forfiles /m "%exe_name%.exe" /s /c "cmd /c echo @path" 2>nul > "%temp_file%"

if exist "%temp_file%" (
    for /f "usebackq tokens=*" %%f in ("%temp_file%") do (
        set "found_path=%%~f"
        goto :FoundFallback
    )
)

del /q "%temp_file%" 2>nul
echo %exe_name%.exe not found
set "found_%exe_name%="
goto :EOF

:FoundDirect
echo Found %exe_name%.exe: %CD%\%found_path%
for %%d in ("%found_path%") do set "dir_path=%%~dpd"
set "dir_path=%dir_path:~0,-1%"
call :AddFolder "%CD%\%dir_path%"
set "found_%exe_name%=YES"
goto :EOF

:FoundFallback
echo Found %exe_name%.exe: %found_path%
for %%d in ("%found_path%") do set "dir_path=%%~dpd"
set "dir_path=%dir_path:~0,-1%"
call :AddFolder "%dir_path%"
set "found_%exe_name%=YES"
del /q "%temp_file%" 2>nul
goto :EOF

:AddFolder
set "new_folder=%~1"
REM Check if folder already in our list
echo %folders_to_add% | findstr /C:"%new_folder%" >nul
if errorlevel 1 (
    if defined folders_to_add (
        set "folders_to_add=%folders_to_add%;%new_folder%"
    ) else (
        set "folders_to_add=%new_folder%"
    )
    echo   -> Added %new_folder% to PATH
    set /a folder_count+=1
)
goto :EOF

:UpdatePath
if defined folders_to_add (
    set "PATH=%folders_to_add%;%PATH%"
    echo PATH updated.
) else (
    echo No folders to add to PATH.
)
goto :EOF

:ShowSummary
echo.
set "summary=Summary: "
if defined found_cycod (set "summary=%summary%cycod YES") else (set "summary=%summary%cycod NO")
if defined found_cycodt (set "summary=%summary%, cycodt YES") else (set "summary=%summary%, cycodt NO")
if defined found_cycodmd (set "summary=%summary%, cycodmd YES") else (set "summary=%summary%, cycodmd NO")
if defined found_cycodgr (set "summary=%summary%, cycodgr YES") else (set "summary=%summary%, cycodgr NO")
if defined found_cycod-mcp-geolocation (set "summary=%summary%, mcp-geo YES") else (set "summary=%summary%, mcp-geo NO")
if defined found_cycod-mcp-mxlookup (set "summary=%summary%, mcp-mx YES") else (set "summary=%summary%, mcp-mx NO")
if defined found_cycod-mcp-osm (set "summary=%summary%, mcp-osm YES") else (set "summary=%summary%, mcp-osm NO")
if defined found_cycod-mcp-weather (set "summary=%summary%, mcp-weather YES") else (set "summary=%summary%, mcp-weather NO")
if defined found_cycod-mcp-whois (set "summary=%summary%, mcp-whois YES") else (set "summary=%summary%, mcp-whois NO")

echo %summary%
if %folder_count% gtr 0 (
    echo PATH updated with %folder_count% new entries.
) else (
    echo No executables found - PATH unchanged.
)
goto :EOF