# File Changes
 - ChatCommand.cs - DisplayUserFunctionCall was made public. Is this necessary?
 - ImageHelperFunctions.cs - Re-add its "catch invalid operation exception"
 - ChatMessageHelpers.cs - is an absolute git diff nightmare
 - SlashCycoDmdCommandHandler.cs - is an absolute git diff nightmare
 - SlashPromptCommandHandler.cs - return the "found" variable on line 24 for readability
 - GenerationResult.cs - "Success" method might not be the best name. Maybe "resolve"?
 - FunctionCallingChat.cs - Ensure that _messages is no longer tracked in any way. All messages logic should be in Conversation.cs
 - NotificationFormat.cs - Is this file really necessary? What purpose does it serve?
 - AIInstructionProcessor.cs - New logic to pass on the provider to cycodmd should exist in a function, not in `ApplyInstructions`

# Test Bot
 - This isn't extensible. Ideally this could be used for other tests as well.
 - Just generally make sure that this is implemented well, as it will be necessary for future tests that involve waiting inside the cycod interaction.

# AiInstructionprocessor.cs - 
 - Is a separate environment variable is really the best way to track the Ai-provider?
 - Could the environment variable replace the need for the ConfigStore? 
 - Is cross-contamination across multiple instances of cycodev possible? Etc.

# Test Files
Ensure tests files match ones that Rob would want. Make sure that they are still accurate and haven't been contaminated. In fact, maybe make them read-only somehow and only allow tests to copy and make changes to the copy, so that we can better test the metadata titles and such. Testing notifications might not be enough.

# ChatClientFactory.cs
 - Some ChatClients `out options`, while some do not. `CreateTestChatClient` does. Why?
 - Is the help message on 339 accurate? Can you use `TEST_DEFAULT_RESPONSE` and `TEST_TITLE_RESPONSE`?

# Generation State Machine
 - Go through GeneratinoResult.cs, GenerationState.cs, GenerationStateMachine.cs, and ensure good naming conventions for all. 
 - Make sure they follow best practices for state machines.

# Don't commit
 - chat-history.jsonl
 - title-test-consolidation-analysis.md
 - locked-title-full-conversation.jsonl.backup
 - malformed-metadata.jsonl.backup
 - unlocked-title-full-conversation.jsonl.backup
 - context.md
 - fooey
 - pull-request-todos.md

# Comments
 - There are various comments throughout that leak the implementation progress. EG, Line 74 of SlashCycoDmdCommandHandler.cs says: `Display function result (like original master branch)`. These comments should be removed.
 - There are some comments that are just incredibly obvious and unhelpful.
 - Doc comments are needed above most method signatures, except for extremely short functions that are very obvious
