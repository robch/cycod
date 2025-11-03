# File Changes
 - ChatCommand.cs - DisplayUserFunctionCall was made public. Is this necessary? Is there another way to achieve the same functionality through a private function call?
 - ✅COMPLETED✅ - ImageHelperFunctions.cs - Re-add its "catch invalid operation exception". This logic was removed unintentionally.
 - ChatMessageHelpers.cs - is an absolute git diff nightmare. Generally go through the entire file and make sure it makes sense.
 - SlashCycoDmdCommandHandler.cs - is an absolute git diff nightmare. Generally go through the entire file and make sure it makes sense.
 - ✅COMPLETED✅ - SlashPromptCommandHandler.cs - return the "found" variable on line 24 for readability
 - GenerationResult.cs - "Success" method might not be the best name. Maybe "resolve"?
 - FunctionCallingChat.cs - Ensure that _messages is no longer tracked in any way. All messages logic should be in Conversation.cs
 - AIInstructionProcessor.cs - New logic to pass on the provider to cycodmd should exist in a function, not in `ApplyInstructions`

# Test Bot
 - This isn't extensible. Ideally this could be used for other tests as well.
 - Just generally make sure that this is implemented well, as it will be necessary for future tests that involve waiting inside the cycod interaction.

# AiInstructionprocessor.cs - 
 - Is a separate environment variable is really the best way to track the Ai-provider?
 - Could the environment variable replace the need for the ConfigStore? 
 - Is cross-contamination across multiple instances of cycodev possible? Etc.

# Test Files
 - Make sure that they are still accurate and haven't been contaminated by running the tests.
 - In fact, make them read-only somehow and only allow tests to copy and make changes to the copy, so that we can better test the metadata titles.

# ChatClientFactory.cs
 - Some ChatClients `out options`, while some do not. `CreateTestChatClient` does. Why?
 - Is the help message on 339 accurate? Can you use `TEST_DEFAULT_RESPONSE` and `TEST_TITLE_RESPONSE`?

# Generation State Machine
 - Go through GenerationResult.cs, GenerationState.cs, GenerationStateMachine.cs, and ensure good naming conventions for all. 
 - Make sure they follow best practices for state machines.

# Comments
 - There are various comments throughout that leak the implementation progress. EG, Line 74 of SlashCycoDmdCommandHandler.cs says: `Display function result (like original master branch)`. These comments should be removed.
 - There are some comments that are just incredibly obvious and unhelpful.
 - Doc comments are needed above most method signatures, except for extremely short functions that are very obvious

# Title output needs to wait for AI to be totally done, not just complete with one message.
```example
Assistant: Now let me check the git differences to understand what changed in this branch:
assistant-function: RunBashCommand {"command":"git branch -a","timeoutMs":10000,"maxCharsPassistant-function: RunBashCommand {"command":"git branch -a","timeoutMs":10000,"maxCharsPassistant-function: RunBashCommand {"command":"git branch -a","timeoutMs":10000,"maxCharsPerLine":500,"maxTotalChars":10000} => Approved (once)
assistant-function: RunBashCommand {"command":"git branch -a","timeoutMs":10000,"maxCharsPassistant-function: RunBashCommand {"command":"git branch -a","timeoutMs":10000,"maxCharsPerLine":500,"maxTotalChars":10000} => Code Quality Review Documentation

...
```
Sometimes, the title returns early and is inserted into an empty place after a function returns no output. In this instance, "Code Quality Review Documentation" is inserted after the bash command `=>`. This is obviously unexpected and should be patched.

```example part 2
...

The document serves as a comprehensive guide for understanding the current state of the title-tests branch and provides a roadmap for addressing the quality issues before merging.

[✓ Title: Code Quality Review Documentation]

User:
```
Although, the title is properly notified later. This could indicate some really wacky behavior.
