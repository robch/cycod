# TODO - Catch exception, deal with better

Repro:
1. Log in with `chatx github login`
2. Wait a long time (days?!)
3. Try to do `chatx` (it will force token refresh, which fails)

Result:

```
EXCEPTION: Failed to get Copilot token: Response status code does not indicate success: 401 (Unauthorized).

  System.InvalidOperationException

     at GitHubCopilotHelper.GetCopilotTokenSync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 159
     at ChatClientFactory.CreateCopilotChatClientWithGitHubToken() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 41
     at ChatClientFactory.TryCreateChatClientFromEnv() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 116
     at ChatClientFactory.CreateChatClient() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 143
     at ChatCommand.ExecuteAsync(Boolean interactive) in C:\src\chatx\src\CommandLineCommands\ChatCommand.cs:line 68
     at Program.<>c__DisplayClass2_0.<<WrapRunAndRelease>b__0>d.MoveNext() in C:\src\chatx\src\Program.cs:line 126
  --- End of stack trace from previous location ---
     at Program.DoProgram(String[] args) in C:\src\chatx\src\Program.cs:line 114
     at Program.Main(String[] args) in C:\src\chatx\src\Program.cs:line 10﻿EXCEPTION: Response status code does not indicate success: 401 (Unauthorized).

  System.Net.Http.HttpRequestException

     at System.Net.Http.HttpResponseMessage.EnsureSuccessStatusCode()
     at GitHubCopilotHelper.GetCopilotTokenAsync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 129
     at GitHubCopilotHelper.GetCopilotTokenSync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 155﻿EXCEPTION: Failed to get Copilot token details: Response status code does not indicate success: 401 (Unauthorized).

  System.InvalidOperationException

     at GitHubCopilotHelper.GetCopilotTokenDetailsSync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 183
     at ChatClientFactory.CreateCopilotChatClientWithGitHubToken() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 46
     at ChatClientFactory.TryCreateChatClientFromEnv() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 136
     at ChatClientFactory.CreateChatClient() in C:\src\chatx\src\ChatClient\ChatClientFactory.cs:line 163
     at ChatCommand.ExecuteAsync(Boolean interactive) in C:\src\chatx\src\CommandLineCommands\ChatCommand.cs:line 80
     at Program.<>c__DisplayClass2_0.<<WrapRunAndRelease>b__0>d.MoveNext() in C:\src\chatx\src\Program.cs:line 157
  --- End of stack trace from previous location ---
     at Program.DoProgram(String[] args) in C:\src\chatx\src\Program.cs:line 145
     at Program.Main(String[] args) in C:\src\chatx\src\Program.cs:line 16﻿EXCEPTION: Response status code does not indicate success: 401 (Unauthorized).

  System.Net.Http.HttpRequestException

     at System.Net.Http.HttpResponseMessage.EnsureSuccessStatusCode()
     at GitHubCopilotHelper.GetCopilotTokenDetailsAsync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 129
     at GitHubCopilotHelper.GetCopilotTokenDetailsSync(String githubToken) in C:\src\chatx\src\Helpers\GitHubCopilotHelper.cs:line 179
```

Expected:

Not that exception message. Instead, it should either:
1. Better error message, and/or
2. Automatically execute same code as `github login`, and retry the retrieval of access token

