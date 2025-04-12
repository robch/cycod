The `mdx` CLI is used to dynamically generate `.md` content for LLM context.

Slash commands powered by `mdx`:  
- `/file <filename>` - Include single file, or part of a file  
- `/files <pattern>` - Include multiple files, or parts of those files  
- `/search <query>` - Include web search results and page content  
- `/get <url>` - Include content from a URL, with or without HTML tags  
- `/run <command>` - Include the output of a command/script  

In the future, `mdx` functionallity will be integrated directly into the `chatx` CLI.

Comming soon:  
- `chatx mdx files` - list/find files by glob + regex  
- `chatx mdx run` - run commands/scripts via Bash, Cmd, or Powershell  
- `chatx mdx web get ` - retrieve content from URLs, with or wo/ HTML tags  
- `chatx mdx web search` - search for content via Bing, DuckDuckGo, Google, Yahoo, etc.  

You can read more about `mdx` at [https://github.com/robch/mdx](https://github.com/robch/mdx).  
