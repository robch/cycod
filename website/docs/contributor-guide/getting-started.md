---
title: Getting Started
icon: material/rocket-launch
---

# Getting Started with CYCODEV Documentation Development

This guide will help you set up your local environment for contributing to the CYCODEV documentation website.

## Prerequisites

Before you begin, make sure you have the following installed:

- **Python 3.7+** - Required to run MkDocs and its dependencies
- **pip** - Python package installer
- **Git** - For version control and cloning the repository

## Setting Up Your Development Environment

Follow these steps to set up your local development environment:

### 1. Clone the Repository

```bash
git clone https://github.com/robch/cycod.git
cd cycod/website  # The website code is in this directory
```

### 2. Create and Activate a Virtual Environment

Creating a virtual environment is recommended to keep your dependencies isolated.

=== "Windows"

    ```cmd
    python -m venv venv
    .\venv\Scripts\activate
    ```

=== "macOS/Linux"

    ```bash
    python -m venv venv
    source venv/bin/activate
    ```

You should see your command prompt change to indicate that the virtual environment is active.

### 3. Install Required Packages

With your virtual environment activated, install the required packages:

```bash
pip install mkdocs mkdocs-material pymdown-extensions
```

## Running the Documentation Locally

To preview the documentation on your local machine:

```bash
mkdocs serve
```

This will start a local development server at [http://127.0.0.1:8000](http://127.0.0.1:8000). As you make changes to the documentation files, the website will automatically reload to reflect those changes.

## Making Your First Change

1. Open any markdown file in the `docs/` directory with your favorite text editor
2. Make a small change
3. Save the file
4. Check your local server to see the changes reflected in real-time

## Directory Structure Overview

```
website/
├── docs/                 # Main documentation content
│   ├── basics/           # Basic CYCOD usage
│   ├── cycodmd/          # CYCODMD documentation
│   │   ├── basics/       # Basic CYCODMD usage
│   │   └── advanced/     # Advanced CYCODMD features
│   ├── reference/        # Command references
│   │   ├── cycod/        # CYCOD reference
│   │   └── cycodmd/      # CYCODMD reference
│   └── ...               # Other documentation directories
├── snippets/             # Reusable content snippets
├── mkdocs.yml            # MkDocs configuration file
└── scripts/              # Utility scripts
```

## Understanding the Documentation Scope

The CYCODEV documentation covers multiple tools:

- **CYCOD**: The main AI-powered CLI tool
- **CYCODMD**: A tool for converting and processing markdown content
- **CYCODT**: Additional tooling (upcoming documentation)

When contributing, be aware of which tool your documentation changes affect and place your content in the appropriate directory.

## Next Steps

Now that you have your local environment set up, you can learn more about:

- [MkDocs and its configuration](mkdocs-overview.md)
- [How to style and customize the site](styling.md)
- [Content authoring best practices](content-authoring.md)

Happy contributing!