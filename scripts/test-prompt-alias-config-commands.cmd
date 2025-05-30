@echo off

if "%1" == "alias" goto DoAlias
if "%1" == "prompt" goto DoPrompt
if "%1" == "config" goto DoConfig
if "%1" == "mcp" goto DoMcp

:DoPrompt
cycod help topics --expand | cycodmd run "cycod prompt create test-prompt @-"
echo.
cycodmd run "cycod prompt list"
echo.
cycodmd run "cycod prompt get test-prompt"
echo.
cycodmd run "cycod prompt delete test-prompt"
echo.
cycodmd run "cycod prompt list"
echo.
cycodmd run "cycod prompt get test-prompt"
echo.
cycodmd run "cycod prompt delete test-prompt"
echo.
if "%1" == "prompt" goto end

:DoAlias
cycodmd run "cycod alias list --save-alias test-alias"
echo.
cycodmd run "cycod --test-alias"
echo.
cycodmd run "cycod alias list"
echo.
cycodmd run "cycod alias get test-alias"
echo.
cycodmd run "cycod alias delete test-alias"
echo.
cycodmd run "cycod alias list"
echo.
cycodmd run "cycod alias get test-alias"
echo.
cycodmd run "cycod alias delete test-alias"
echo.
cycodmd run "cycod --test-alias"
echo.
if "%1" == "alias" goto end

:DoConfig
cycodmd run "cycod config set foobar 123"
echo.
cycodmd run "cycod config list"
echo.
cycodmd run "cycod config get foobar"
echo.
cycodmd run "cycod config clear foobar"
echo.
cycodmd run "cycod config list"
echo.
cycodmd run "cycod config get foobar"
echo.
if "%1" == "config" goto end

:DoMcp
cycodmd run "cycod mcp add test-mcp --command \"echo test-mcp\""
echo.
cycodmd run "cycod mcp list"
echo.
cycodmd run "cycod mcp get test-mcp"
echo.
cycodmd run "cycod mcp remove test-mcp"
echo.
cycodmd run "cycod mcp list"
echo.
cycodmd run "cycod mcp get test-mcp"
echo.
if "%1" == "mcp" goto end

:end
