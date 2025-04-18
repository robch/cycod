#!/bin/bash
# Run a specific YAML-based test file for chatx
# Usage: ./run_specific_test.sh test_core_chat_basic.yaml

if [ -z "$1" ]; then
  echo "Usage: ./run_specific_test.sh test_file_name.yaml"
  echo "Example: ./run_specific_test.sh test_core_chat_basic.yaml"
  exit 1
fi

echo "Running test file: $1"
ai test tests/$1

if [ $? -ne 0 ]; then
  echo "Test failed with exit code $?"
  exit $?
fi

echo "Test completed successfully."
exit 0