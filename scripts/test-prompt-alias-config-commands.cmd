@echo off

if "%1" == "alias" goto DoAlias
if "%1" == "prompt" goto DoPrompt
if "%1" == "config" goto DoConfig
if "%1" == "mcp" goto DoMcp

:DoPrompt
cycod help topics --expand | mdx run "cycod prompt create test-prompt @-"
echo.
mdx run "cycod prompt list"
echo.
mdx run "cycod prompt get test-prompt"
echo.
mdx run "cycod prompt delete test-prompt"
echo.
mdx run "cycod prompt list"
echo.
mdx run "cycod prompt get test-prompt"
echo.
mdx run "cycod prompt delete test-prompt"
echo.
if "%1" == "prompt" goto end

:DoAlias
mdx run "cycod alias list --save-alias test-alias"
echo.
mdx run "cycod --test-alias"
echo.
mdx run "cycod alias list"
echo.
mdx run "cycod alias get test-alias"
echo.
mdx run "cycod alias delete test-alias"
echo.
mdx run "cycod alias list"
echo.
mdx run "cycod alias get test-alias"
echo.
mdx run "cycod alias delete test-alias"
echo.
mdx run "cycod --test-alias"
echo.
if "%1" == "alias" goto end

:DoConfig
mdx run "cycod config set foobar 123"
echo.
mdx run "cycod config list"
echo.
mdx run "cycod config get foobar"
echo.
mdx run "cycod config clear foobar"
echo.
mdx run "cycod config list"
echo.
mdx run "cycod config get foobar"
echo.
if "%1" == "config" goto end

:DoMcp
mdx run "cycod mcp add test-mcp --command echo --arg test-mcp"
echo.
mdx run "cycod mcp list"
echo.
mdx run "cycod mcp get test-mcp"
echo.
mdx run "cycod mcp remove test-mcp"
echo.
mdx run "cycod mcp list"
echo.
mdx run "cycod mcp get test-mcp"
echo.
if "%1" == "mcp" goto end

:end
