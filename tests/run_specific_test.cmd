@echo off
rem Run a specific YAML-based test file for chatx
rem Usage: run_specific_test.cmd test_core_chat_basic.yaml

if "%~1"=="" (
  echo Usage: run_specific_test.cmd test_file_name.yaml
  echo Example: run_specific_test.cmd test_core_chat_basic.yaml
  exit /b 1
)

echo Running test file: %1
ai test tests/%1

if %ERRORLEVEL% NEQ 0 (
  echo Test failed with exit code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)

echo Test completed successfully.
exit /b 0