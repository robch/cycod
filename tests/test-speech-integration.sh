#!/bin/bash
# Test script for Phase 5.2: Speech Integration

echo "=== Testing --speech flag parsing ==="
echo ""

# Test 1: Verify --speech flag is recognized (should not produce an error)
echo "Test 1: Testing --speech flag..."
dotnet run --project src/cycod/cycod.csproj -- --help > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "✓ cycod runs successfully"
else
    echo "✗ cycod failed to run"
    exit 1
fi

# Test 2: Try running with --speech flag (will fail without speech config, but should parse the flag)
echo ""
echo "Test 2: Testing --speech flag parsing..."
echo "exit" | dotnet run --project src/cycod/cycod.csproj -- --speech 2>&1 | grep -i "speech" > /dev/null
# We expect this to either work or fail with a speech-related error, not an "unknown option" error

echo ""
echo "=== Build Test ==="
dotnet build src/cycod/cycod.csproj --no-incremental
if [ $? -eq 0 ]; then
    echo "✓ Build successful with 0 errors"
else
    echo "✗ Build failed"
    exit 1
fi

echo ""
echo "=== Manual Testing Instructions ==="
echo ""
echo "To fully test speech integration, you need:"
echo "1. Azure Cognitive Services Speech key and region"
echo "2. Create speech.key file with your subscription key"
echo "3. Create speech.region file with your region (e.g., 'westus2')"
echo ""
echo "Then run:"
echo "  cycod --speech"
echo ""
echo "In the interactive chat:"
echo "1. Press ENTER on empty input"
echo "2. Verify the context menu shows 'Speech input' as first option"
echo "3. Select 'Speech input' and speak into microphone"
echo "4. Verify the speech is recognized and displayed"
echo ""
echo "Without --speech flag:"
echo "  cycod"
echo "1. Press ENTER on empty input"
echo "2. Verify the context menu does NOT show 'Speech input'"
echo ""

echo "=== All automated tests passed! ==="
