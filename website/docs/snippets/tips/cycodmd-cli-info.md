The `cycodmd` CLI is used to dynamically generate `.md` content for LLM context.

Slash commands powered by `cycodmd`:  
- `/file <filename>` - Include single file, or part of a file  
- `/files <pattern>` - Include multiple files, or parts of those files  
- `/search <query>` - Include web search results and page content  
- `/get <url>` - Include content from a URL, with or without HTML tags  
- `/run <command>` - Include the output of a command/script  

In the future, `cycodmd` functionallity will be integrated directly into the `cycod` CLI.

Comming soon:  
- `cycod cycodmd files` - list/find files by glob + regex  
- `cycod cycodmd run` - run commands/scripts via Bash, Cmd, or Powershell  
- `cycod cycodmd web get ` - retrieve content from URLs, with or wo/ HTML tags  
- `cycod cycodmd web search` - search for content via Bing, DuckDuckGo, Google, Yahoo, etc.  

You can read more about `cycodmd` at [https://github.com/robch/cycod](https://github.com/robch/cycod).  
