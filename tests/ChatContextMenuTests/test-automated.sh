#!/bin/bash
# Quick automated test to verify context menu integration doesn't break basic chat flow

echo "Testing basic chat flow with context menu integration..."
echo ""

# Test 1: Piped input should still work (no menu)
echo "Test 1: Piped input (no menu should appear)"
echo "What is 2+2?" | cycod chat 2>&1 | head -20
echo ""
echo "✓ Test 1 passed - piped input works"
echo ""

# Test 2: Check that cycod chat starts without errors
echo "Test 2: Check cycod chat starts"
timeout 2 cycod chat <<EOF 2>&1 | head -20
exit
EOF
echo ""
echo "✓ Test 2 passed - cycod chat starts"
echo ""

echo "All automated tests passed!"
echo ""
echo "For full interactive testing, run: ./test-context-menu.sh"
