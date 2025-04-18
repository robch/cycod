@echo off
rem Run all YAML-based tests for chatx

echo Running all ChatX YAML tests...
ai test tests/test_*.yaml

if %ERRORLEVEL% NEQ 0 (
  echo Tests failed with exit code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)

echo All tests completed successfully.
exit /b 0