#!/bin/bash
# Console GUI Implementation - Iterative Memento Script
# This script reads the memento, executes the next step, and repeats

MEMENTO_FILE="console-gui-implementation-memento.md"
MAX_ITERATIONS=100

# Check if memento file exists
if [ ! -f "$MEMENTO_FILE" ]; then
    echo "ERROR: $MEMENTO_FILE not found!"
    echo "Make sure you're in the correct directory: /c/src/cycod-console-gui"
    exit 1
fi

echo "=== Console GUI Iterative Implementation ==="
echo "Memento file: $MEMENTO_FILE"
echo "Max iterations: $MAX_ITERATIONS"
echo

for i in $(seq 1 $MAX_ITERATIONS); do
    echo "=========================================="
    echo "ITERATION #$i"
    echo "=========================================="
    echo
    
    # Step 1: Read memento and execute next step
    echo ">>> Step 1: Reading memento and executing next action..."
    cycod --input "
Read the file $MEMENTO_FILE carefully.

Look at the 'Current Position' and 'Next Action Required' sections at the top.

Execute the next action that's specified. Follow the steps exactly as outlined.

When you complete the work:
1. Test that it works
2. Document what you did
3. Create/update the daily memento file (console-gui-day-N.md)
4. Update the main memento file with the new 'Current Position'
5. Commit your changes to git with a clear message

If the current position says COMPLETE or there's no next action, respond with: DONE
"
    
    RESULT=$?
    
    # Check if we got DONE response
    if grep -qi "DONE" <<< "$(cat ~/.cycod/history/chat-history-*.jsonl | tail -1 2>/dev/null)"; then
        echo
        echo ">>> Work appears complete! Moving to verification step..."
        break
    fi
    
    echo
    echo ">>> Step 1 complete. Now verifying..."
    echo
    
    # Step 2: Double-check completion
    echo ">>> Step 2: Verifying work is complete before moving on..."
    cycod --input "
Read the file $MEMENTO_FILE again.

Double-check that the work you just did is actually complete:
- Did you update the memento with the new 'Current Position'?
- Did you create/update the daily memento file?
- Did you commit to git?
- Are there any compilation errors?
- Did you test that it works?

If everything is TRULY complete, respond with: VERIFIED COMPLETE

If something is incomplete, finish it now, then respond with: NOW COMPLETE

If you encountered a blocker, respond with: BLOCKED - [describe issue]
"
    
    echo
    echo ">>> Iteration $i complete!"
    echo
    
    # Small delay to avoid rate limiting
    sleep 2
done

echo
echo "=========================================="
echo "FINAL VERIFICATION"
echo "=========================================="
echo

# Final check of overall status
cycod --input "
Read the file $MEMENTO_FILE one final time.

Give me a summary:
1. What phase are we in?
2. What percentage is complete?
3. What's been accomplished?
4. What's the next action required?

If Phase 0-7 are all marked complete, respond with: PROJECT COMPLETE! 🎉
"

echo
echo "=========================================="
echo "Script complete! Check the memento for current status."
echo "=========================================="
