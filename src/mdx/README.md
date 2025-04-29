# MDX - AI-Powered Markdown Generator CLI

MDX is a command-line tool that helps build markdown files from various sources. It can process files, search the web, and apply AI processing to create markdown content. The tool supports file and line filtering, line formatting, AI processing, and output options. It can be used to create markdown files for documentation, research, and other purposes.

## Features
- Integrates AI processing for applying instructions to files, pages, or command outputs.
- Supports glob patterns for specifying multiple files.
- Outputs filenames as markdown headers followed by content in code blocks.
- Handles relative and absolute file paths efficiently.
- Allows filtering of files and lines based on regular expressions.
- Provides options to include or exclude specific lines and add line numbers.
- Capable of handling multiple threads for file processing.
- Supports web search and retrieval with markdown formatting.
- Allows headless browsing and HTML stripping for web content.
- Enables saving output and configuration options to specified files.
- Supports aliasing of options for easy reuse.

## Installation

There are several ways to install and run MDX.

### OPTION 1: Install as .NET Tool

MDX is available as a .NET global tool that can be installed from NuGet.

For global installation (available from any directory):
```bash
dotnet tool install --global MDX --prerelease
```

For local installation (available only in current directory):
```bash
dotnet tool install --local MDX --prerelease
```

After installation, you can run MDX directly from your terminal:
```bash
mdx --help
```

### OPTION 2: Local Build

To build and run MDX locally:

1. Install [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Clone this repository
```bash
git clone https://github.com/robch/mdx
```
3. Build the project:
```bash
cd mdx
dotnet build
```

### OPTION 3: Docker Build

To run MDX in a Docker container with all dependencies pre-installed:

1. Clone this repository
```bash
git clone https://github.com/robch/mdx
```
2. Build the Docker image:
```bash
cd mdx
docker build -t mdx .
```
3. Run MDX commands using the container:
```bash
docker run mdx [command arguments]
```

### OPTION 4: VS Code Dev Container

1. Install [VS Code](https://code.visualstudio.com/) and the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
2. Clone this repository
```bash
git clone https://github.com/robch/mdx
```
3. Open in VS Code and click "Reopen in Container" when prompted
```bash
code mdx
```

### OPTION 5: GitHub Codespaces

- Visit [codespaces.new/robch/mdx](https://codespaces.new/robch/mdx?quickstart=1)
- Or open in GitHub and click the "Code" button > "Create codespace"

## Usage

`mdx`

```
MDX - The AI-Powered Markdown Generator CLI, Version 1.0.0
Copyright(c) 2024, Rob Chambers. All rights reserved.

Welcome to MDX, the AI-Powered Markdown Generator!

Using MDX, you can:

  - Convert files to markdown
  - Run scripts and convert output to markdown
  - Search the web and convert search results to markdown
  - Get web pages and convert them to markdown

  AND ... You can apply AI processing to the output!

USAGE: mdx FILE1 [FILE2 [...]] [...]
   OR: mdx PATTERN1 [PATTERN2 [...]] [...]
   OR: mdx run [COMMAND1 [COMMAND2 [...]]] [...]
   OR: mdx web search "TERMS" ["TERMS2" [...]] [...]
   OR: mdx web get "URL" ["URL2" [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown for one or more files

    mdx BackgroundInfo.docx
    mdx Presentation2.pptx
    mdx *.pdf *.png *.jpg *.gif *.bmp

  EXAMPLE 2: Find files recursively and create markdown

    mdx **/*.cs

  EXAMPLE 3: Create markdown running a script

    mdx run --powershell "Get-Process" --instructions "list running processes"

  EXAMPLE 4: Create markdown from a web search

    mdx web search "yaml site:learnxinyminutes.com" --max 1 --get --strip

SEE ALSO

  mdx help
  mdx help examples
  mdx help options
```

`mdx help examples`

```
USAGE: mdx FILE1 [FILE2 [...]] [...]
   OR: mdx PATTERN1 [PATTERN2 [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown for one or more files

    mdx BackgroundInfo.docx
    mdx Presentation2.pptx
    mdx ResearchPaper.pdf
    mdx "../plans/*.md"
    mdx *.png *.jpg *.gif *.bmp

  EXAMPLE 2: Find files recursively, exclude certain files

    mdx "**/*.cs" "**/*.md"
    mdx "**/*.cs" --exclude "**/bin/" "**/obj/"

  EXAMPLE 3: Filter and format based on file or line content

    mdx "**/*.js" --file-contains "export"
    mdx "**/*.cs" --file-contains "public class"
    mdx "**/*.cs" --remove-all-lines "^\s//"

    mdx "**/*.md" --contains "TODO" --line-numbers
    mdx "**/*.md" --contains "(?i)LLM" --lines-after 10

  EXAMPLE 4: Apply AI processing on each found file

    mdx "**/*.json" --file-instructions "convert the JSON to YAML"
    mdx "**/*.json" --file-instructions @instructions.md --threads 5

  EXAMPLE 5: Apply AI to specific file types; multi-step instructions

    mdx --cs-file-instructions @cs-instructions.txt --md-file-instructions @md-instructions.txt
    mdx --file-instructions @step1-instructions.md @step2-instructions.md

  EXAMPLE 6: Apply AI to the final output

    mdx "**/*.md" --instructions "Create a markdown summary table for each file"
    mdx README.md "**/*.cs" --instructions "Output only an updated README.md"

  EXAMPLE 7: Save each file output to a specified template file

    mdx "**/*.cs" --save-file-output "outputs/{fileBase}.md"

  EXAMPLE 8: Save the current options as an alias, and then use it

    mdx "**/*.cs" --save-alias cs
    mdx --cs

SEE ALSO

  mdx help options

  mdx help web search
  mdx help web search examples
  mdx help web search options

  mdx help web get
  mdx help web get examples
  mdx help web get options
  
```

`mdx help options`

```
USAGE: mdx FILE1 [FILE2 [...]] [...]
   OR: mdx PATTERN1 [PATTERN2 [...]] [...]

OPTIONS

  FILE/LINE FILTERING

    --exclude PATTERN              Exclude files that match the specified pattern

    --contains REGEX               Match only files and lines that contain the specified regex pattern
    --file-contains REGEX          Match only files that contain the specified regex pattern
    --file-not-contains REGEX      Exclude files that contain the specified regex pattern

    --line-contains REGEX          Match only lines that contain the specified regex pattern
    --remove-all-lines REGEX       Remove lines that contain the specified regex pattern

  LINE FORMATTING

    --lines N                      Include N lines both before and after matching lines
    --lines-after N                Include N lines after matching lines (default 0)
    --lines-before N               Include N lines before matching lines (default 0)

    --line-numbers                 Include line numbers in the output

  AI PROCESSING

    --file-instructions "..."      Apply the specified instructions to each file (uses AI CLI)
    --EXT-file-instructions "..."  Apply the specified instructions to each file with the specified extension

    --instructions "..."           Apply the specified instructions to command output (uses AI CLI)

    --built-in-functions           Enable built-in functions (AI CLI can use file system)
    --threads N                    Limit the number of concurrent file processing threads

    --save-chat-history [FILE]     Save the chat history to the specified file
                                   (e.g. chat-history-{time}.jsonl)

  OUTPUT

    --save-file-output [FILE]      Save each file output to the specified template file
                                   (e.g. {filePath}/{fileBase}-output.md)

    --save-output [FILE]           Save command output to the specified template file
    --save-alias ALIAS             Save current options as an alias (usable via --{ALIAS})

SUB COMMANDS

  run [...]                        Create markdown from shell commands output
  web search [...]                 Create markdown from web search results
  web get [...]                    Create markdown from web page content

SEE ALSO

  mdx help
  mdx help examples

  mdx help run
  mdx help run examples
  mdx help run options

  mdx help web search
  mdx help web search examples
  mdx help web search options

  mdx help web get
  mdx help web get examples
  mdx help web get options
  
```

`mdx help run`

```
MDX RUN

  Use the 'mdx run' command to execute scripts or commands and create markdown from the output.

USAGE: mdx run [COMMAND1 [COMMAND2 [...]]] [...]

EXAMPLES

  EXAMPLE 1: Run a simple command and process the output

    mdx run "echo Hello, World!" --instructions "translate strings to german"

  EXAMPLE 2: Run a script using PowerShell and process the output

    mdx run --powershell "Get-Process" --instructions "list running processes"

  EXAMPLE 3: Run a bash script and apply multi-step AI instructions

    mdx run --bash "ls -la" --instructions @step1-instructions.txt @step2-instructions.txt

SEE ALSO

  mdx help run examples
  mdx help run options

```

`mdx help run examples`

```
MDX RUN

  Use the 'mdx run' command to execute scripts or commands and create markdown from the output.

USAGE: mdx run [COMMAND1 [COMMAND2 [...]]] [...]

EXAMPLES

  EXAMPLE 1: Run a simple command and process the output

    mdx run "echo Hello, World!" --instructions "translate strings to german"

  EXAMPLE 2: Run a script using PowerShell and process the output

    mdx run --powershell "Get-Process" --instructions "list running processes"

  EXAMPLE 3: Run a bash script and apply multi-step AI instructions

    mdx run --bash "ls -la" --instructions @step1-instructions.txt @step2-instructions.txt

  EXAMPLE 4: Run multiple commands

    mdx run "echo Hello, World!" "echo Goodbye, World!"
    
SEE ALSO

  mdx help run
  mdx help options

```

`mdx help run options`

```
MDX RUN

  Use the 'mdx run' command to execute scripts or commands and create markdown from the output.

USAGE: mdx run [COMMAND1 [COMMAND2 [...]]] [...]

OPTIONS

  SCRIPT

    --script [COMMAND]            Specify the script or command to run
                                  (On Windows, the default is cmd. On Linux/Mac, the default is bash)

    --cmd [COMMAND]               Specify the script or command to run
    --bash [COMMAND]              Specify the script or command to run
    --powershell [COMMAND]        Specify the script or command to run

  AI PROCESSING

    --instructions "..."          Apply the specified instructions to command output (uses AI CLI).
    --built-in-functions          Enable built-in functions (AI CLI can use file system).

  OUTPUT

    --save-output [FILE]          Save command output to the specified template file.
    --save-alias ALIAS            Save current options as an alias (usable via --{ALIAS}).

SEE ALSO

  mdx help run
  mdx help run examples

```

`mdx help web get`

```
MDX WEB GET

  Use the 'mdx web get' command to create markdown from one or more web pages.

USAGE: mdx web get "URL" ["URL2" [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown from a web page, keeping HTML tags

    mdx web get "https://learnxinyminutes.com/docs/yaml/"

  EXAMPLE 2: Create markdown from a web page, stripping HTML tags

    mdx web get "https://learnxinyminutes.com/docs/yaml/" --strip

SEE ALSO

  mdx help web get options
  mdx help web get examples

  mdx help web search
  mdx help web search examples
  mdx help web search options
  
```

`mdx help web get examples`

```
MDX WEB GET

  Use the 'mdx web get' command to create markdown from one or more web pages.

USAGE: mdx web get "URL" ["URL2" [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown for web page content

    mdx web get https://example.com
    mdx web get https://mbers.us/bio --strip

  EXAMPLE 2: Apply AI processing on each web page

    mdx web get https://example.com https://mbers.us/bio --page-instructions "what's the title of this page?"

  EXAMPLE 3: Apply AI multi-step instructions

    mdx web get https://learnxinyminutes.com/yaml/ --page-instructions @step1-instructions.txt @step2-instructions.txt

  EXAMPLE 4: Apply AI to the final output

    mdx web get https://example.com https://mbers.us/bio --instructions "style example.com as the other site"    

SEE ALSO

  mdx help web get
  mdx help web get options

  mdx help web search
  mdx help web search examples
  mdx help web search options


```

`mdx help web get options`

```
MDX WEB GET

  Use the 'mdx web get' command to create markdown from one or more web pages.

USAGE: mdx web get "URL" ["URL2" [...]] [...]

OPTIONS

  BROWSER/HTML

    --interactive                      Run in browser interactive mode (default: false)
    --chromium                         Use Chromium browser (default)
    --firefox                          Use Firefox browser
    --webkit                           Use WebKit browser
    --strip                            Strip HTML tags from downloaded content (default: false)

  AI PROCESSING

    --page-instructions "..."          Apply the specified instructions to each page (uses AI CLI)
    --SITE-page-instructions "..."     Apply the specified instructions to each page (for matching SITEs)

    --instructions "..."               Apply the specified instructions to command output (uses AI CLI)

    --built-in-functions               Enable built-in functions (AI CLI can use file system)
    --threads N                        Limit the number of concurrent file processing threads

    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)

  OUTPUT

    --save-page-output [FILE]          Save each web page output to the specified template file
                                       (e.g. {filePath}/{fileBase}-output.md)

    --save-output [FILE]               Save command output to the specified template file
    --save-alias ALIAS                 Save current options as an alias (usable via --{ALIAS})

SEE ALSO

  mdx help web get
  mdx help web get examples

  mdx help web search
  mdx help web search examples
  mdx help web search options
  
```

`mdx help web search`

```
MDX WEB SEARCH

  Use the 'mdx web search' command to search the web and create markdown from the results.

USAGE: mdx web search "TERMS" ["TERMS2" [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown for web search URL results

    mdx web search "Azure AI" --google
    mdx web search "Azure AI" --bing

  EXAMPLE 2: Create markdown for web search result content

    mdx web search "yaml site:learnxinyminutes.com" --max 1 --get --strip

SEE ALSO

  mdx help web search examples
  mdx help web search options

  mdx help web get
  mdx help web get examples
  mdx help web get options
  
  mdx help bing api
  mdx help google api
```

`mdx help web search examples`

```
MDX WEB SEARCH

  Use the 'mdx web search' command to search the web and create markdown from the results.

USAGE: mdx web search "TERMS" ["TERMS2" [...]] [...]

EXAMPLES

  EXAMPLE 1: Create markdown for web search URL results

    mdx web search "Azure AI"
    mdx web search "Azure AI" --bing
    mdx web search "Azure AI" --exclude youtube.com reddit.com

  EXAMPLE 2: Create markdown for web search result content

    mdx web search "Azure AI" --max 5 --get --strip
    mdx web search "yaml site:learnxinyminutes.com" --max 1 --get --strip

  EXAMPLE 3: Apply AI processing on each web page

    mdx web search "web components" --get --strip --page-instructions "reformat markdown"

  EXAMPLE 4: Apply AI multi-step instructions

    mdx web search "how to fly kite" --get --strip --page-instructions @step1-instructions.txt @step2-instructions.txt

  EXAMPLE 5: Apply AI to the final output

    mdx web search "how to fly kite" --max 2 --get --strip --instructions "Create a markdown summary from all pages"

SEE ALSO

  mdx help web search
  mdx help web search options

  mdx help web get
  mdx help web get examples
  mdx help web get options

  mdx help bing api
  mdx help google api
```

`mdx help web search options`

```
MDX WEB SEARCH

  Use the 'mdx web search' command to search the web and create markdown from the results.

USAGE: mdx web search "TERMS" ["TERMS2" [...]] [...]

OPTIONS

  BROWSER/HTML

    --interactive                      Run in browser interactive mode (default: false)
    --chromium                         Use Chromium browser (default)
    --firefox                          Use Firefox browser
    --webkit                           Use WebKit browser
    --strip                            Strip HTML tags from downloaded content (default: false)

  SEARCH ENGINE

    --bing                             Use Bing search engine
    --duckduckgo                       Use DuckDuckGo search engine
    --google                           Use Google search engine (default)
    --yahoo                            Use Yahoo search engine

    --bing-api                         Use Bing search API (requires API key and endpoint)
    --google-api                       Use Google search API (requires API key, endpoint, and engine ID)

    --get                              Download content from search results (default: false)

    --exclude REGEX                    Exclude URLs that match the specified regular expression
    --max NUMBER                       Maximum number of search results (default: 10)

  AI PROCESSING

    --page-instructions "..."          Apply the specified instructions to each page (uses AI CLI)
    --SITE-page-instructions "..."     Apply the specified instructions to each page (for matching SITEs)

    --instructions "..."               Apply the specified instructions to command output (uses AI CLI)

    --built-in-functions               Enable built-in functions (AI CLI can use file system)
    --threads N                        Limit the number of concurrent file processing threads

    --save-chat-history [FILE]         Save the chat history to the specified file
                                       (e.g. chat-history-{time}.jsonl)

  OUTPUT

    --save-page-output [FILE]          Save each web page output to the specified template file
                                       (e.g. {filePath}/{fileBase}-output.md)

    --save-output [FILE]               Save command output to the specified template file
    --save-alias ALIAS                 Save current options as an alias (usable via --{ALIAS})

SEE ALSO

  mdx help web search
  mdx help web search examples

  mdx help web get
  mdx help web get examples
  mdx help web get options
  
  mdx help bing api
  mdx help google api
```

`mdx help images`

```
MDX IMAGES

  MDX can convert images to markdown by extracting a rich description and all visible text
  using Azure OpenAI's vision capabilities.

USAGE: mdx IMAGE_FILE1 [FILE2 [...]] [...]
   OR: mdx IMAGE_PATTERN1 [PATTERN2 [...]] [...]

SETUP

  To use the Azure OpenAI vision capabilities, you'll need to create and deploy a resource and
  vision compatible model in the Azure AI Foundry portal or using the Azure AI CLI.

    TRY: https://ai.azure.com/
     OR: https://thebookof.ai/setup/openai/

  Once you have created your resource and deployed a compatible model, you can get your API key
  from the Azure portal or using the `ai dev new .env` command. Using those values, you can set
  these environment variables, either in the active shell or in a file called `.env` in the
  current directory.

    AZURE_OPENAI_API_KEY=********************************
    AZURE_OPENAI_ENDPOINT=https://{resource}.cognitiveservices.azure.com/
    AZURE_OPENAI_CHAT_DEPLOYMENT=gpt-4o

EXAMPLES

  EXAMPLE 1: Setup resource, deployment, and environment variables

    ai init openai
    ai dev new .env

  EXAMPLE 2: Convert an image to markdown

    mdx test.png

  EXAMPLE 3: Convert multiple images to markdown

    mdx **\*.png **\*.jpg **\*.jpeg **\*.gif **\*.bmp

SEE ALSO

  mdx help
  mdx help examples
  mdx help options
```

`mdx help bing api`

```
MDX BING API

  The `--bing-api` option allows you to use the Bing Web Search API for web searches
  instead of UI automated scraping of Bing or Google search results (the default).

USAGE: mdx web search "TERMS" --bing-api [...]

SETUP

  To use the Bing Web Search API, you need to get an API key and endpoint from Microsoft. You can
  use the free tier, which allows for up to 3 requests per second and 1000 requests per month, or
  you can upgrade to a paid tier for more requests.

  https://learn.microsoft.com/bing/search-apis/bing-web-search/create-bing-search-service-resource

  Once you have created your resource, you can get your API key from the Azure portal on the Keys
  and Endpoint page. Using those values, you can set these two environment variables, either in
  the active shell or in a file called `.env` in the current directory.

    BING_SEARCH_V7_ENDPOINT=https://api.bing.microsoft.com/v7.0/search
    BING_SEARCH_V7_KEY=436172626F6E20697320636F6F6C2121

EXAMPLE

  mdx web search "yaml site:learnxinyminutes.com" --bing-api --max 1 --get --strip

SEE ALSO

  mdx help web search
  mdx help web search examples
  mdx help web search options
```

`mdx help google api`

```
MDX GOOGLE API

  The `--google-api` option allows you to use the Google Custom Web Search API for web searches
  instead of UI automated scraping of Bing or Google search results (the default).

USAGE: mdx web search "TERMS" --google-api [...]

SETUP

  To use the Google Custom Web Search API, you need to get an API key and endpoint from Google. You can
  use the free tier, which allows for up to 100 requests per day, or you can upgrade to a paid tier for
  more requests.

  https://developers.google.com/custom-search/v1/overview

  Once you have created your resource, you can get your API key from the Google Cloud Console on the
  Credentials page. Using that value, you can set these three environment variables, either in the
  active shell or in a file called `.env` in the current directory.

    GOOGLE_SEARCH_API_KEY=********************************
    GOOGLE_SEARCH_ENGINE_ID=********************************
    GOOGLE_SEARCH_ENDPOINT=https://www.googleapis.com/customsearch/v1
    
EXAMPLE

  mdx web search "yaml site:learnxinyminutes.com" --google-api --max 1 --get --strip

SEE ALSO

  mdx help web search
  mdx help web search examples
  mdx help web search options
```
