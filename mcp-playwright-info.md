## Web Search for 'how to launch playwright mcp via npx?' using DuckDuckGo

https://deepwiki.com/microsoft/playwright-mcp/2-getting-started
https://github.com/microsoft/playwright-mcp

## GitHub - microsoft/playwright-mcp: Playwright MCP server

url: https://github.com/microsoft/playwright-mcp

```

  GitHub - microsoft/playwright-mcp: Playwright MCP server

      Skip to content

  Navigation Menu

    Toggle navigation

            Sign in

Appearance settings

        Product

            GitHub Copilot

        Write better code with AI

            GitHub Spark

                New

        Build and deploy intelligent apps

            GitHub Models

                New

        Manage and compare prompts

            GitHub Advanced Security

        Find and fix vulnerabilities

            Actions

        Automate any workflow

            Codespaces

        Instant dev environments

            Issues

        Plan and track work

            Code Review

        Manage code changes

            Discussions

        Collaborate outside of code

            Code Search

        Find more, search less

                    Explore

      Why GitHub

      All features

      Documentation

      GitHub Skills

      Blog

        Solutions

                    By company size

      Enterprises

      Small and medium teams

      Startups

      Nonprofits

                    By use case

      DevSecOps

      DevOps

      CI/CD

      View all use cases

                    By industry

      Healthcare

      Financial services

      Manufacturing

      Government

      View all industries

              View all solutions

        Resources

                    Topics

      AI

      DevOps

      Security

      Software Development

      View all

                    Explore

      Learning Pathways

      Events & Webinars

      Ebooks & Whitepapers

      Customer Stories

      Partners

      Executive Insights

        Open Source

            GitHub Sponsors

        Fund open source developers

            The ReadME Project

        GitHub community articles

                    Repositories

      Topics

      Trending

      Collections

        Enterprise

            Enterprise platform

        AI-powered developer platform

                    Available add-ons

            GitHub Advanced Security

        Enterprise-grade security features

            Copilot for business

        Enterprise-grade AI features

            Premium Support

        Enterprise-grade 24/7 support

    Pricing

        Search or jump to...

      Search code, repositories, users, issues, pull requests...

        Search

          Clear

            Search syntax tips            

        Provide feedback

          We read every piece of feedback, and take your input very seriously.

          Include my email address so I can be contacted

                    Cancel
              Submit feedback

        Saved searches

        Use saved searches to filter your results more quickly

            Name

            Query

            To see all available qualifiers, see our documentation.

                    Cancel
              Create saved search

                Sign in

                Sign up

Appearance settings

          Resetting focus

        You signed in with another tab or window. Reload to refresh your session.
        You signed out in another tab or window. Reload to refresh your session.
        You switched accounts on another tab or window. Reload to refresh your session.

Dismiss alert

      {{ message }}

        microsoft

    /

      playwright-mcp

    Public

Notifications
    You must be signed in to change notification settings

Fork
    1.3k

          Star
          17.5k

        Playwright MCP server

          www.npmjs.com/package/@playwright/mcp

      License

     Apache-2.0 license

          17.5k
          stars

          1.3k
          forks

          Branches

          Tags

          Activity

          Star

Notifications
    You must be signed in to change notification settings

        Code

        Issues
          44

        Pull requests
          8

        Actions

        Security

              Uh oh!
              There was an error while loading. Please reload this page.

        Insights

Additional navigation options

          Code

          Issues

          Pull requests

          Actions

          Security

          Insights

  microsoft/playwright-mcp

      main9 Branches27 TagsGo to fileCodeOpen more actions menuFolders and filesNameNameLast commit messageLast commit dateLatest commityury-schore: check version in page, link to instructions (#918)successAug 19, 2025f6862a3 · Aug 19, 2025History334 CommitsOpen commit details.github/workflows.github/workflowsdevops: extension publishing job (#888)Aug 15, 2025examplesexampleschore: roll Playwright, remove localOutputDir (#471)May 24, 2025extensionextensionchore: check version in page, link to instructions (#918)Aug 19, 2025srcsrcchore: check extension version on connect (#907)Aug 18, 2025teststestschore: handle list roots in the server, with timeout (#898)Aug 15, 2025utilsutilsdevops: update extension manifest version (#904)Aug 15, 2025.gitignore.gitignorechore(extension): build into dist directory (#825)Aug 4, 2025.npmignore.npmignorechore: allow passing config file (#281)Apr 28, 2025DockerfileDockerfilechore: run w/ sandbox by default (#412)May 13, 2025LICENSELICENSEchore: initial code commitMar 21, 2025README.mdREADME.mdchore(extension): add readme file, recommend --extension option (#894)Aug 14, 2025SECURITY.mdSECURITY.mdAdding Microsoft SECURITY.MD (#1)Mar 21, 2025cli.jscli.jschore: migrate to ESM (#303)Apr 30, 2025config.d.tsconfig.d.tschore: save session log (#740)Jul 22, 2025eslint.config.mjseslint.config.mjschore: align lint w/ playwright (#729)Jul 21, 2025index.d.tsindex.d.tschore: do not double close connection (#744)Jul 23, 2025index.jsindex.jsfix: import from cjs (#476)May 26, 2025package-lock.jsonpackage-lock.jsonchore: mark 0.0.34 (#901)Aug 15, 2025package.jsonpackage.jsonchore: mark 0.0.34 (#901)Aug 15, 2025playwright.config.tsplaywright.config.tschore: try macos15 runners (#892)Aug 15, 2025tsconfig.all.jsontsconfig.all.jsonchore(extension): use react for connect dialog (#777)Jul 28, 2025tsconfig.jsontsconfig.jsonchore: unset skipLibCheck in tsconfig.json (#386)May 9, 2025View all filesRepository files navigationREADMECode of conductApache-2.0 licenseSecurityPlaywright MCP
A Model Context Protocol (MCP) server that provides browser automation capabilities using Playwright. This server enables LLMs to interact with web pages through structured accessibility snapshots, bypassing the need for screenshots or visually-tuned models.
Key Features
Fast and lightweight. Uses Playwright's accessibility tree, not pixel-based input.
LLM-friendly. No vision models needed, operates purely on structured data.
Deterministic tool application. Avoids ambiguity common with screenshot-based approaches.
Requirements
Node.js 18 or newer
VS Code, Cursor, Windsurf, Claude Desktop, Goose or any other MCP client
Getting started
First, install the Playwright MCP server with your client.
Standard config works in most of the tools:
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "@playwright/mcp@latest"
      ]
    }
  }
}

Claude Code
Use the Claude Code CLI to add the Playwright MCP server:
claude mcp add playwright npx @playwright/mcp@latest

Claude Desktop
Follow the MCP install guide, use the standard config above.
Cursor
Click the button to install:
Or install manually:
Go to Cursor Settings -> MCP -> Add new MCP Server. Name to your liking, use command type with the command npx @playwright/mcp. You can also verify config or add command like arguments via clicking Edit.
Gemini CLI
Follow the MCP install guide, use the standard config above.
Goose
Click the button to install:
Or install manually:
Go to Advanced settings -> Extensions -> Add custom extension. Name to your liking, use type STDIO, and set the command to npx @playwright/mcp. Click "Add Extension".
LM Studio
Click the button to install:
Or install manually:
Go to Program in the right sidebar -> Install -> Edit mcp.json. Use the standard config above.
opencode
Follow the MCP Servers documentation. For example in ~/.config/opencode/opencode.json:
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "playwright": {
      "type": "local",
      "command": [
        "npx",
        "@playwright/mcp@latest"
      ],
      "enabled": true
    }
  }
}

Qodo Gen
Open Qodo Gen chat panel in VSCode or IntelliJ → Connect more tools → + Add new MCP → Paste the standard config above.
Click Save.
VS Code
Click the button to install:

Or install manually:
Follow the MCP install guide, use the standard config above. You can also install the Playwright MCP server using the VS Code CLI:
# For VS Code
code --add-mcp '{"name":"playwright","command":"npx","args":["@playwright/mcp@latest"]}'

After installation, the Playwright MCP server will be available for use with your GitHub Copilot agent in VS Code.
Windsurf
Follow Windsurf MCP documentation. Use the standard config above.
Configuration
Playwright MCP server supports following arguments. They can be provided in the JSON configuration above, as a part of the "args" list:
> npx @playwright/mcp@latest --help
  --allowed-origins <origins>  semicolon-separated list of origins to allow the
                               browser to request. Default is to allow all.
  --blocked-origins <origins>  semicolon-separated list of origins to block the
                               browser from requesting. Blocklist is evaluated
                               before allowlist. If used without the allowlist,
                               requests not matching the blocklist are still
                               allowed.
  --block-service-workers      block service workers
  --browser <browser>          browser or chrome channel to use, possible
                               values: chrome, firefox, webkit, msedge.
  --caps <caps>                comma-separated list of additional capabilities
                               to enable, possible values: vision, pdf.
  --cdp-endpoint <endpoint>    CDP endpoint to connect to.
  --config <path>              path to the configuration file.
  --device <device>            device to emulate, for example: "iPhone 15"
  --executable-path <path>     path to the browser executable.
  --extension                  Connect to a running browser instance
                               (Edge/Chrome only). Requires the "Playwright MCP
                               Bridge" browser extension to be installed.
  --headless                   run browser in headless mode, headed by default
  --host <host>                host to bind server to. Default is localhost. Use
                               0.0.0.0 to bind to all interfaces.
  --ignore-https-errors        ignore https errors
  --isolated                   keep the browser profile in memory, do not save
                               it to disk.
  --image-responses <mode>     whether to send image responses to the client.
                               Can be "allow" or "omit", Defaults to "allow".
  --no-sandbox                 disable the sandbox for all process types that
                               are normally sandboxed.
  --output-dir <path>          path to the directory for output files.
  --port <port>                port to listen on for SSE transport.
  --proxy-bypass <bypass>      comma-separated domains to bypass proxy, for
                               example ".com,chromium.org,.domain.com"
  --proxy-server <proxy>       specify proxy server, for example
                               "http://myproxy:3128" or "socks5://myproxy:8080"
  --save-session               Whether to save the Playwright MCP session into
                               the output directory.
  --save-trace                 Whether to save the Playwright Trace of the
                               session into the output directory.
  --storage-state <path>       path to the storage state file for isolated
                               sessions.
  --user-agent <ua string>     specify user agent string
  --user-data-dir <path>       path to the user data directory. If not
                               specified, a temporary directory will be created.
  --viewport-size <size>       specify browser viewport size in pixels, for
                               example "1280, 720"

User profile
You can run Playwright MCP with persistent profile like a regular browser (default), in isolated contexts for testing sessions, or connect to your existing browser using the browser extension.
Persistent profile
All the logged in information will be stored in the persistent profile, you can delete it between sessions if you'd like to clear the offline state.
Persistent profile is located at the following locations and you can override it with the --user-data-dir argument.
# Windows
%USERPROFILE%\AppData\Local\ms-playwright\mcp-{channel}-profile
# macOS
- ~/Library/Caches/ms-playwright/mcp-{channel}-profile
# Linux
- ~/.cache/ms-playwright/mcp-{channel}-profile

Isolated
In the isolated mode, each session is started in the isolated profile. Every time you ask MCP to close the browser,
the session is closed and all the storage state for this session is lost. You can provide initial storage state
to the browser via the config's contextOptions or via the --storage-state argument. Learn more about the storage
state here.
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "@playwright/mcp@latest",
        "--isolated",
        "--storage-state={path/to/storage.json}"
      ]
    }
  }
}

Browser Extension
The Playwright MCP Chrome Extension allows you to connect to existing browser tabs and leverage your logged-in sessions and browser state. See extension/README.md for installation and setup instructions.
Configuration file
The Playwright MCP server can be configured using a JSON configuration file. You can specify the configuration file
using the --config command line option:
npx @playwright/mcp@latest --config path/to/config.json

Configuration file schema
{
  // Browser configuration
  browser?: {
    // Browser type to use (chromium, firefox, or webkit)
    browserName?: 'chromium' | 'firefox' | 'webkit';
    // Keep the browser profile in memory, do not save it to disk.
    isolated?: boolean;
    // Path to user data directory for browser profile persistence
    userDataDir?: string;
    // Browser launch options (see Playwright docs)
    // @see https://playwright.dev/docs/api/class-browsertype#browser-type-launch
    launchOptions?: {
      channel?: string;        // Browser channel (e.g. 'chrome')
      headless?: boolean;      // Run in headless mode
      executablePath?: string; // Path to browser executable
      // ... other Playwright launch options
    };
    // Browser context options
    // @see https://playwright.dev/docs/api/class-browser#browser-new-context
    contextOptions?: {
      viewport?: { width: number, height: number };
      // ... other Playwright context options
    };
    // CDP endpoint for connecting to existing browser
    cdpEndpoint?: string;
    // Remote Playwright server endpoint
    remoteEndpoint?: string;
  },
  // Server configuration
  server?: {
    port?: number;  // Port to listen on
    host?: string;  // Host to bind to (default: localhost)
  },
  // List of additional capabilities
  capabilities?: Array<
    'tabs' |    // Tab management
    'install' | // Browser installation
    'pdf' |     // PDF generation
    'vision' |  // Coordinate-based interactions
  >;
  // Directory for output files
  outputDir?: string;
  // Network configuration
  network?: {
    // List of origins to allow the browser to request. Default is to allow all. Origins matching both `allowedOrigins` and `blockedOrigins` will be blocked.
    allowedOrigins?: string[];
    // List of origins to block the browser to request. Origins matching both `allowedOrigins` and `blockedOrigins` will be blocked.
    blockedOrigins?: string[];
  };

  /**
   * Whether to send image responses to the client. Can be "allow" or "omit". 
   * Defaults to "allow".
   */
  imageResponses?: 'allow' | 'omit';
}

Standalone MCP server
When running headed browser on system w/o display or from worker processes of the IDEs,
run the MCP server from environment with the DISPLAY and pass the --port flag to enable HTTP transport.
npx @playwright/mcp@latest --port 8931

And then in MCP client config, set the url to the HTTP endpoint:
{
  "mcpServers": {
    "playwright": {
      "url": "http://localhost:8931/mcp"
    }
  }
}

Docker
NOTE: The Docker implementation only supports headless chromium at the moment.
{
  "mcpServers": {
    "playwright": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "--init", "--pull=always", "mcr.microsoft.com/playwright/mcp"]
    }
  }
}

You can build the Docker image yourself.
docker build -t mcr.microsoft.com/playwright/mcp .

Programmatic usage
import http from 'http';
import { createConnection } from '@playwright/mcp';
import { SSEServerTransport } from '@modelcontextprotocol/sdk/server/sse.js';
http.createServer(async (req, res) => {
  // ...
  // Creates a headless Playwright MCP server with SSE transport
  const connection = await createConnection({ browser: { launchOptions: { headless: true } } });
  const transport = new SSEServerTransport('/messages', res);
  await connection.sever.connect(transport);
  // ...
});

Tools
Core automation
browser_click
Title: Click
Description: Perform click on a web page
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
ref (string): Exact target element reference from the page snapshot
doubleClick (boolean, optional): Whether to perform a double click instead of a single click
button (string, optional): Button to click, defaults to left
Read-only: false
browser_close
Title: Close browser
Description: Close the page
Parameters: None
Read-only: true
browser_console_messages
Title: Get console messages
Description: Returns all console messages
Parameters: None
Read-only: true
browser_drag
Title: Drag mouse
Description: Perform drag and drop between two elements
Parameters:
startElement (string): Human-readable source element description used to obtain the permission to interact with the element
startRef (string): Exact source element reference from the page snapshot
endElement (string): Human-readable target element description used to obtain the permission to interact with the element
endRef (string): Exact target element reference from the page snapshot
Read-only: false
browser_evaluate
Title: Evaluate JavaScript
Description: Evaluate JavaScript expression on page or element
Parameters:
function (string): () => { /* code / } or (element) => { / code */ } when element is provided
element (string, optional): Human-readable element description used to obtain permission to interact with the element
ref (string, optional): Exact target element reference from the page snapshot
Read-only: false
browser_file_upload
Title: Upload files
Description: Upload one or multiple files
Parameters:
paths (array): The absolute paths to the files to upload. Can be a single file or multiple files.
Read-only: false
browser_handle_dialog
Title: Handle a dialog
Description: Handle a dialog
Parameters:
accept (boolean): Whether to accept the dialog.
promptText (string, optional): The text of the prompt in case of a prompt dialog.
Read-only: false
browser_hover
Title: Hover mouse
Description: Hover over element on page
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
ref (string): Exact target element reference from the page snapshot
Read-only: true
browser_navigate
Title: Navigate to a URL
Description: Navigate to a URL
Parameters:
url (string): The URL to navigate to
Read-only: false
browser_navigate_back
Title: Go back
Description: Go back to the previous page
Parameters: None
Read-only: true
browser_navigate_forward
Title: Go forward
Description: Go forward to the next page
Parameters: None
Read-only: true
browser_network_requests
Title: List network requests
Description: Returns all network requests since loading the page
Parameters: None
Read-only: true
browser_press_key
Title: Press a key
Description: Press a key on the keyboard
Parameters:
key (string): Name of the key to press or a character to generate, such as ArrowLeft or a
Read-only: false
browser_resize
Title: Resize browser window
Description: Resize the browser window
Parameters:
width (number): Width of the browser window
height (number): Height of the browser window
Read-only: true
browser_select_option
Title: Select option
Description: Select an option in a dropdown
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
ref (string): Exact target element reference from the page snapshot
values (array): Array of values to select in the dropdown. This can be a single value or multiple values.
Read-only: false
browser_snapshot
Title: Page snapshot
Description: Capture accessibility snapshot of the current page, this is better than screenshot
Parameters: None
Read-only: true
browser_take_screenshot
Title: Take a screenshot
Description: Take a screenshot of the current page. You can't perform actions based on the screenshot, use browser_snapshot for actions.
Parameters:
type (string, optional): Image format for the screenshot. Default is png.
filename (string, optional): File name to save the screenshot to. Defaults to page-{timestamp}.{png|jpeg} if not specified.
element (string, optional): Human-readable element description used to obtain permission to screenshot the element. If not provided, the screenshot will be taken of viewport. If element is provided, ref must be provided too.
ref (string, optional): Exact target element reference from the page snapshot. If not provided, the screenshot will be taken of viewport. If ref is provided, element must be provided too.
fullPage (boolean, optional): When true, takes a screenshot of the full scrollable page, instead of the currently visible viewport. Cannot be used with element screenshots.
Read-only: true
browser_type
Title: Type text
Description: Type text into editable element
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
ref (string): Exact target element reference from the page snapshot
text (string): Text to type into the element
submit (boolean, optional): Whether to submit entered text (press Enter after)
slowly (boolean, optional): Whether to type one character at a time. Useful for triggering key handlers in the page. By default entire text is filled in at once.
Read-only: false
browser_wait_for
Title: Wait for
Description: Wait for text to appear or disappear or a specified time to pass
Parameters:
time (number, optional): The time to wait in seconds
text (string, optional): The text to wait for
textGone (string, optional): The text to wait for to disappear
Read-only: true
Tab management
browser_tab_close
Title: Close a tab
Description: Close a tab
Parameters:
index (number, optional): The index of the tab to close. Closes current tab if not provided.
Read-only: false
browser_tab_list
Title: List tabs
Description: List browser tabs
Parameters: None
Read-only: true
browser_tab_new
Title: Open a new tab
Description: Open a new tab
Parameters:
url (string, optional): The URL to navigate to in the new tab. If not provided, the new tab will be blank.
Read-only: true
browser_tab_select
Title: Select a tab
Description: Select a tab by index
Parameters:
index (number): The index of the tab to select
Read-only: true
Browser installation
browser_install
Title: Install the browser specified in the config
Description: Install the browser specified in the config. Call this if you get an error about the browser not being installed.
Parameters: None
Read-only: false
Coordinate-based (opt-in via --caps=vision)
browser_mouse_click_xy
Title: Click
Description: Click left mouse button at a given position
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
x (number): X coordinate
y (number): Y coordinate
Read-only: false
browser_mouse_drag_xy
Title: Drag mouse
Description: Drag left mouse button to a given position
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
startX (number): Start X coordinate
startY (number): Start Y coordinate
endX (number): End X coordinate
endY (number): End Y coordinate
Read-only: false
browser_mouse_move_xy
Title: Move mouse
Description: Move mouse to a given position
Parameters:
element (string): Human-readable element description used to obtain permission to interact with the element
x (number): X coordinate
y (number): Y coordinate
Read-only: true
PDF generation (opt-in via --caps=pdf)
browser_pdf_save
Title: Save as PDF
Description: Save page as PDF
Parameters:
filename (string, optional): File name to save the pdf to. Defaults to page-{timestamp}.pdf if not specified.
Read-only: true

  About

        Playwright MCP server

          www.npmjs.com/package/@playwright/mcp

    Topics

  mcp

  playwright

    Resources

        Readme

    License

     Apache-2.0 license

    Code of conduct

        Code of conduct

    Security policy

        Security policy

              Uh oh!
              There was an error while loading. Please reload this page.

      Activity  

        Custom properties    
  Stars

      17.5k
      stars  
  Watchers

      73
      watching  
  Forks

      1.3k
      forks  

          Report repository

  Releases
      26

        v0.0.34

          Latest

      Aug 16, 2025

      + 25 releases

              Uh oh!
              There was an error while loading. Please reload this page.

  Contributors
      37

    + 23 contributors

                Languages

          TypeScript
          92.2%

          JavaScript
          5.9%

        Other
        1.9%

  Footer

        © 2025 GitHub, Inc.

      Footer navigation

            Terms

            Privacy

            Security

            Status

            Docs

            Contact

       Manage cookies

      Do not share my personal information

    You can’t perform that action at this time.
```


## Getting Started | microsoft/playwright-mcp | DeepWiki

url: https://deepwiki.com/microsoft/playwright-mcp/2-getting-started

```
Getting Started | microsoft/playwright-mcp | DeepWikiGet free private DeepWikis in DevinDeepWikiDeepWikimicrosoft/playwright-mcpGet free private DeepWikis withDevinShareLast indexed: 17 August 2025 (2fc4e8)OverviewGetting StartedAPI ReferenceCore ArchitectureProgram Entry Points and CLIServer Backend and Context ManagementTransport LayerBrowser Context FactoriesTool SystemTool Architecture and FrameworkCore Browser ToolsNavigation and Tab ManagementFile Operations and ScreenshotsKeyboard Input and Form InteractionVision Mode and Advanced FeaturesConfiguration SystemConfiguration Resolution and SourcesBrowser Options and CapabilitiesBrowser Extension IntegrationExtension ArchitectureCDP Relay SystemTesting FrameworkTest Infrastructure and FixturesTool Testing and CoverageDevelopment and ContributingBuild System and CI/CDProject Structure and ArchitectureMenuGetting Started
Relevant source files
README.md
src/config.ts
src/program.ts
This document guides you through installing, configuring, and taking your first steps with the Playwright MCP server. It covers basic installation, configuration options, and initial usage patterns to help you start automating browser interactions through the Model Context Protocol.
For detailed information about specific tools and their parameters, see Core Browser Tools. For advanced configuration options and browser connection strategies, see Configuration System and Browser Context Factories.
Installation Overview
The Playwright MCP server can be installed and configured through various MCP clients. The installation process involves configuring your MCP client to launch the server using npx @playwright/mcp@latest.
Quick Installation
The standard configuration works across most MCP clients:
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "@playwright/mcp@latest"
      ]
    }
  }
}
This configuration tells your MCP client to execute the Playwright MCP server using the command specified in src/program.ts34-93
Client-Specific Installation
The system supports installation across multiple MCP clients, each with their own configuration approach:
ClientConfiguration MethodAdditional NotesVS CodeMCP extension or CLISupports deeplink installationCursorMCP settings or deeplinkClick-to-install buttons availableClaude DesktopManual JSON configurationFollow MCP quickstart guideGooseExtension managerCustom extension setupLM StudioMCP server managerJSON configuration editing
Sources: README.md24-158
System Architecture Overview
Core Components Flow
Tool_ExecutionContext_ManagementMCP_Transport_LayerCLI_Entryprogram.tsconfig.tsmcpTransport.start()ProxyBackend | BrowserServerBackendcontextFactory()BrowserContextFactory implementationsmcpServer.createServer()Browser automation tools
This diagram shows the flow from CLI entry through to tool execution, mapping directly to the code entities in the system.
Sources: src/program.ts66-93 src/config.ts91-100
Configuration Resolution Pipeline
Runtime_ComponentsResolution_ProcessConfig_SourcesdefaultConfigJSON config fileconfigFromEnv()configFromCLIOptions()mergeConfig()FullConfigBrowserContextFactoryMCPProvider[]
The configuration system resolves settings from multiple sources with CLI options taking highest precedence.
Sources: src/config.ts54-72 src/config.ts91-100 src/config.ts257-290
Basic Configuration
Command Line Options
The system accepts configuration through command-line arguments defined in src/program.ts37-62 Key options include:
OptionTypeDescriptionExample--browserStringBrowser type or channelchrome, firefox, webkit--headlessBooleanRun without GUI--headless--isolatedBooleanIn-memory profile--isolated--capsString[]Additional capabilities--caps=vision,pdf--portNumberHTTP transport port--port=8931
Environment Variables
Configuration can also be provided through environment variables using the PLAYWRIGHT_MCP_ prefix pattern implemented in src/config.ts200-228:
export PLAYWRIGHT_MCP_BROWSER=chrome
export PLAYWRIGHT_MCP_HEADLESS=true
export PLAYWRIGHT_MCP_CAPS=vision,pdf
Configuration File
For complex setups, use a JSON configuration file specified with --config:
{
  "browser": {
    "browserName": "chromium",
    "isolated": false,
    "launchOptions": {
      "headless": false,
      "channel": "chrome"
    },
    "contextOptions": {
      "viewport": { "width": 1280, "height": 720 }
    }
  },
  "capabilities": ["vision", "pdf"],
  "server": {
    "port": 8931
  }
}
Sources: src/config.ts230-239 README.md264-345
Browser Connection Methods
The system supports multiple browser connection strategies through different context factory implementations:
Connection Strategy Selection
Browser_ConnectionsContext_Factory_SelectionCLI_Options--extension flag--cdp-endpoint--isolated flag--user-data-dirExtensionContextFactoryCdpContextFactoryIsolatedContextFactoryPersistentContextFactoryBrowser Extension BridgeCDP Endpoint ConnectionIsolated Browser ContextPersistent User Profile
Connection Configuration Examples
Persistent Profile (Default)
npx @playwright/mcp@latest --browser=chrome
Isolated Session
npx @playwright/mcp@latest --isolated --storage-state=/path/to/auth.json
Browser Extension Connection
npx @playwright/mcp@latest --extension
CDP Endpoint Connection
npx @playwright/mcp@latest --cdp-endpoint=ws://localhost:9222
Sources: src/program.ts76-81 src/program.ts111-113 src/browserContextFactory.ts
First Steps
Starting the Server
Standard STDIO Transport: Most MCP clients use STDIO by default
{
  "command": "npx",
  "args": ["@playwright/mcp@latest"]
}
HTTP Transport: For remote or headless environments
npx @playwright/mcp@latest --port=8931
Then configure client with: "url": "http://localhost:8931/mcp"
Extension Mode: Connect to existing browser
npx @playwright/mcp@latest --extension
Basic Usage Flow
Once connected, the MCP client can use browser automation tools. The typical flow involves:
Navigation: Use browser_navigate to load pages
Inspection: Use browser_snapshot to get page structure
Interaction: Use browser_click, browser_type for user actions
Data Extraction: Use browser_take_screenshot or content tools
Tool Capabilities
The system provides tools through a capability-based system defined in src/config.ts186:
CapabilityTools EnabledUse CaseCore (always)browser_navigate, browser_click, browser_snapshotBasic automationtabsbrowser_tab_new, browser_tab_selectMulti-tab workflowsvisionbrowser_mouse_click_xy, coordinate toolsPixel-perfect interactionpdfbrowser_pdf_saveDocument generationinstallbrowser_installBrowser setup
Sources: src/program.ts41 README.md178-179
Output and Session Management
Output Directory
The system creates output files (screenshots, PDFs, traces) in directories resolved by src/config.ts241-249:
Explicit --output-dir path
.playwright-mcp in MCP client root path
Temporary directory with timestamp
Session Persistence
Session Logging: Use --save-session to record MCP interactions
Trace Recording: Use --save-trace to capture Playwright traces for debugging
npx @playwright/mcp@latest --save-session --save-trace --output-dir=./sessions
Profile Management
Browser profiles are stored in platform-specific locations as defined in README.md225-236:
Windows: %USERPROFILE%\AppData\Local\ms-playwright\mcp-{channel}-profile
macOS: ~/Library/Caches/ms-playwright/mcp-{channel}-profile
Linux: ~/.cache/ms-playwright/mcp-{channel}-profile
Sources: src/config.ts241-249 README.md218-243
Transport Configuration
Transport Selection Logic
Server_BackendsTransport_TypesTransport_Detection--port flag providedprocess.stdin availableconfig.server.portSSEServerTransportStdioServerTransportProxyBackendBrowserServerBackend
The transport layer automatically selects between STDIO and HTTP/SSE based on configuration, as implemented in src/mcp/transport.js
Sources: src/program.ts92 src/mcp/transport.js
Next Steps
After completing basic setup:
Explore Tools: Review available browser automation tools in Core Browser Tools
Advanced Configuration: Learn about browser-specific options in Browser Options and Capabilities
Extension Integration: Set up browser extension for existing sessions in Browser Extension Integration
Development: Understand the architecture in Core Architecture for customization
Sources: README.md413-719DismissRefresh this wikiThis wiki was recently refreshed. Please wait 5 days to refresh again.On this pageGetting StartedInstallation OverviewQuick InstallationClient-Specific InstallationSystem Architecture OverviewCore Components FlowConfiguration Resolution PipelineBasic ConfigurationCommand Line OptionsEnvironment VariablesConfiguration FileBrowser Connection MethodsConnection Strategy SelectionConnection Configuration ExamplesFirst StepsStarting the ServerBasic Usage FlowTool CapabilitiesOutput and Session ManagementOutput DirectorySession PersistenceProfile ManagementTransport ConfigurationTransport Selection LogicNext StepsAsk Devin about microsoft/playwright-mcpDeep Research
```


