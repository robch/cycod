#!/bin/bash
# Run all YAML-based tests for chatx

echo "Running all ChatX YAML tests..."
ai test tests/test_*.yaml

if [ $? -ne 0 ]; then
  echo "Tests failed with exit code $?"
  exit $?
fi

echo "All tests completed successfully."
exit 0