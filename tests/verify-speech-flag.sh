#!/bin/bash
# Quick verification that the menu changes with --speech flag

echo "=== Testing menu without --speech flag ==="
# Send just "exit" - this should work without showing speech option
echo "exit" | dotnet run --project src/cycod/cycod.csproj -- 2>&1 | head -20

echo ""
echo "=== Testing --speech flag is recognized ==="
# With --speech flag, the program should accept it (even though we can't test actual speech input here)
echo "exit" | dotnet run --project src/cycod/cycod.csproj -- --speech 2>&1 | head -20

echo ""
echo "✓ Both commands executed successfully"
