@echo off

if "%1" == "alias" goto DoAlias
if "%1" == "prompt" goto DoPrompt
if "%1" == "config" goto DoConfig
if "%1" == "mcp" goto DoMcp

:DoPrompt
chatx help topics --expand | mdx run "chatx prompt create test-prompt @-"
echo.
mdx run "chatx prompt list"
echo.
mdx run "chatx prompt get test-prompt"
echo.
mdx run "chatx prompt delete test-prompt"
echo.
mdx run "chatx prompt list"
echo.
mdx run "chatx prompt get test-prompt"
echo.
mdx run "chatx prompt delete test-prompt"
echo.
if "%1" == "prompt" goto end

:DoAlias
mdx run "chatx alias list --save-alias test-alias"
echo.
mdx run "chatx --test-alias"
echo.
mdx run "chatx alias list"
echo.
mdx run "chatx alias get test-alias"
echo.
mdx run "chatx alias delete test-alias"
echo.
mdx run "chatx alias list"
echo.
mdx run "chatx alias get test-alias"
echo.
mdx run "chatx alias delete test-alias"
echo.
mdx run "chatx --test-alias"
echo.
if "%1" == "alias" goto end

:DoConfig
mdx run "chatx config set foobar 123"
echo.
mdx run "chatx config list"
echo.
mdx run "chatx config get foobar"
echo.
mdx run "chatx config clear foobar"
echo.
mdx run "chatx config list"
echo.
mdx run "chatx config get foobar"
echo.
if "%1" == "config" goto end

:DoMcp
mdx run "chatx mcp add test-mcp --command echo --arg test-mcp"
echo.
mdx run "chatx mcp list"
echo.
mdx run "chatx mcp get test-mcp"
echo.
mdx run "chatx mcp remove test-mcp"
echo.
mdx run "chatx mcp list"
echo.
mdx run "chatx mcp get test-mcp"
echo.
if "%1" == "mcp" goto end

:end
