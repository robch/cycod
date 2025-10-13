# TODO: Meta-Debugging and Self-Analysis Instructions for AGENTS.md

## Log File Analysis and Self-Debugging Techniques

### Finding Your Current Session Log
When you need to debug or analyze your own function calls:

1. **Identify your current log file**: Look for the most recent `log-cycod-*.log` file in the current directory
2. **Verify it's yours**: Search the log for text from your current conversation to confirm it's your active session
3. **Use recursive verification**: You can grep your own log file for the commands you're currently running
4. **Expect rapid growth**: Even short sessions can generate large log files (1MB+) due to verbose logging
5. **Understand session types**: Determine if you're in a fresh session or continuing from loaded chat history

### Chat History and Session Continuity
- **Chat history location**: Stored in `C:\Users\{user}\.cycod\history\chat-history-{timestamp}.jsonl`
- **Current logs**: Stored in current working directory as `log-cycod-{timestamp}.log`
- **Meta-debugging**: You can examine your own previous function calls by viewing the chat history that was loaded into your current session
- **Log file growth**: Logs grow very rapidly (can reach 1MB+ quickly) due to detailed API request/response logging, function call traces, and verbose internal logging
- **New session = new log**: Each restart creates a fresh log file with a new timestamp, even if continuing from chat history
- **Session continuity vs. fresh start**: Distinguish between loading previous chat history (session continuation) vs. truly fresh restart

### Function Call Testing and Validation Techniques

#### Before Writing Tests
1. **Test functions manually first**: Use the actual functions to understand their real behavior
2. **Document unexpected behavior**: Log discrepancies between expected and actual function outputs
3. **Don't trust documentation alone**: Always verify function behavior through direct experimentation

#### Test Design Principles
1. **Prevent AI cheating**: Only approve specific functions needed for the test, not alternatives that could produce false positives
2. **Use negative assertions**: Include `not-expect-regex` to prevent unwanted function usage
3. **Be specific with expectations**: Match exact JSON structures and responses, not just presence of text
4. **Test one behavior at a time**: Focus tests on single, specific behaviors rather than complex scenarios

#### Security-First Function Approval
- **Never use blanket approval**: Don't approve `--auto-approve all` or broad categories
- **Approve by specific function names**: `--auto-approve RunShellCommand --auto-approve GetShellOrProcessOutput`
- **Question everything**: If a test needs a dangerous function, ask why and find safer alternatives

### Bug Discovery Through Testing
- **Real-world testing reveals bugs**: Manual function testing often exposes issues that unit tests miss
- **Output capture problems**: Pay attention to functions that should return data but return empty results
- **Auto-conversion edge cases**: Test timeout scenarios to ensure state transitions work correctly
- **Resource management**: Verify that long-running processes are properly managed and cleaned up

### Meta-Analysis Capabilities
You can:
- Examine your own function calls in real-time by viewing your current log
- Load and analyze previous conversations from chat history files
- Create recursive debugging scenarios where you analyze your own analysis
- Use the logging system to understand the internal flow of function calls and responses

This creates powerful self-debugging and meta-analysis capabilities that should be leveraged for continuous improvement and bug discovery.

## Action Items for AGENTS.md Integration
- Add section on log file analysis techniques
- Include function testing methodology before test creation
- Emphasize security-first approach to function approval
- Document the meta-debugging capabilities available
- Add guidelines for recursive self-analysis and validation
- Document log file growth patterns and size expectations
- Clarify session continuity vs. fresh restart scenarios
- Add notes about the recursive nature of analyzing logs while generating logs