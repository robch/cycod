# CHATX Documentation Website

This is the documentation website for CHATX, an AI-powered CLI tool. The website is built using [MkDocs](https://www.mkdocs.org/) with the [Material theme](https://squidfunk.github.io/mkdocs-material/).

## Getting Started

### Prerequisites

- Python 3.7 or higher
- pip (Python package installer)

### Installation

1. Clone this repository:

```bash
git clone https://github.com/robch/chatx.git
cd chatx/website
```

2. Create and activate a virtual environment:

```bash
# Windows
python -m venv venv
.\venv\Scripts\activate

# macOS/Linux
python -m venv venv
source venv/bin/activate
```

3. Install MkDocs and required packages:

```bash
pip install mkdocs mkdocs-material pymdown-extensions
```

## Development

### Running Locally

To start the development server:

```bash
mkdocs serve
```

This will start a local development server at http://localhost:8000.

### Building the Site

To build the static site:

```bash
mkdocs build
```

This will create a `site` directory with the built website.

## Project Structure

```
website/
├── docs/                 # Documentation content
│   ├── assets/           # Images, CSS, and other assets
│   │   ├── chatx.png     # CHATX logo
│   │   ├── extra.css     # Custom CSS
│   │   └── cli-toggle.css # CSS for CLI toggle functionality
│   ├── js/               # JavaScript files
│   │   └── cli-toggle.js # JS for CLI toggle functionality
│   ├── index.md          # Homepage
│   ├── install-chatx-cli.md # Installation and setup guide
│   ├── providers/        # Provider-specific documentation
│   ├── usage/            # Usage documentation
│   ├── advanced/         # Advanced features documentation
│   └── reference/        # Command reference
├── snippets/             # Reusable content snippets
│   ├── code-blocks/      # Code examples
│   └── tips/             # Tips and notes
└── mkdocs.yml            # MkDocs configuration file
```

## Adding Content

### Adding a New Page

1. Create a new Markdown file in the appropriate directory under `docs/`.
2. Add the page to the navigation in `mkdocs.yml`.

### Creating Snippets

For reusable content, create snippet files in the `snippets/` directory and include them in your Markdown files using the `--8<--` syntax:

```markdown
--8<-- "path/to/snippet.md"
```

## Customizing the Theme

The theme is configured in `mkdocs.yml`. You can customize:

- Colors by modifying the `palette` section
- Navigation by updating the `nav` section
- Features by adding or removing items in the `features` section

## Deployment

The site can be deployed to GitHub Pages:

```bash
mkdocs gh-deploy
```

Or to any static hosting service by copying the contents of the `site` directory.

## License

Copyright (c) 2025 Rob Chambers. All rights reserved.