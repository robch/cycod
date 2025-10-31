# File Changes
 - ChatCommand.cs - DisplayUserFunctionCall was made public. Is this necessary?
 - ImageHelperFunctions.cs - Re-add its "catch invalid operation exception"
 - ChatMessageHelpers.cs - is an absolute git diff nightmare
 - SlashCycoDmdCommandHandler.cs - is an absolute git diff nightmare
 - SlashPromptCommandHandler.cs - return the "found" variable on line 24 for readability
 - GenerationResult.cs - "Success" method might not be the best name. Maybe "resolve"?

# Test Bot
 - This isn't extensible. Ideally this could be used for other tests as well.

# AiInstructionprocessor.cs - 
 - Is a separate environment variable is really the best way to track the Ai-provider?
 - Could the environment variable replace the need for the ConfigStore? 
 - Is cross-contamination across multiple instances of cycodev possible? Etc.

# Test Files
Ensure tests files match ones that Rob would want. Make sure that they are still accurate and haven't been contaminated. In fact, maybe make them read-only somehow and only allow tests to copy and make changes to the copy, so that we can better test the metadata titles and such. Testing notifications might not be enough.

# Don't commit
 - chat-history.jsonl
 - title-test-consolidation-analysis.md
 - locked-title-full-conversation.jsonl.backup
 - malformed-metadata.jsonl.backup
 - unlocked-title-full-conversation.jsonl.backup
 - context.md
 - fooey