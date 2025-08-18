---
title: MkDocs Overview
icon: material/file-cog
---

# Understanding MkDocs and Its Configuration

This guide explains how MkDocs works and how it's configured for the CYCODEV documentation website.

## What is MkDocs?

[MkDocs](https://www.mkdocs.org/) is a fast, simple static site generator designed for building project documentation. It takes Markdown files and converts them into a static website with features like navigation, search, and theming.

## The `mkdocs.yml` File

The `mkdocs.yml` file is the heart of any MkDocs project. It contains all the configuration options that determine how your site is built and how it behaves.

### Key Sections in Our Configuration

The CYCODEV documentation's `mkdocs.yml` has several important sections:

#### Basic Settings

```yaml
site_name: CYCODEV Documentation
repo_url: https://github.com/robch/cycod
```

- `site_name`: The name that appears in the browser title and top bar
- `repo_url`: Link to the GitHub repository

#### Theme Configuration

```yaml
theme:
  logo: assets/cycod.png
  name: material
  features:
    - navigation.tabs
    - navigation.sections
    - navigation.indexes
    # ... more features
```

- `name: material`: Uses the Material for MkDocs theme
- `logo`: Path to the site logo
- `features`: Enables specific theme features like tabbed navigation

#### Customization

```yaml
extra_css:
  - assets/extra.css
  - assets/cli-toggle.css

extra_javascript:
  - js/cli-toggle.js
```

These sections define custom CSS and JavaScript files that extend the theme's capabilities.

#### Markdown Extensions

```yaml
markdown_extensions:
  - pymdownx.highlight
  - pymdownx.superfences
  - pymdownx.tabbed
  # ... more extensions
```

These extensions enhance Markdown with features like syntax highlighting, tabbed content, admonitions, and more.

#### Navigation Structure

The `nav` section defines the website's navigation structure and hierarchy. This is where you add new pages to make them appear in the site navigation.

Here's a simplified example of the current navigation structure:

```yaml
nav:
  - HOME: index.md
  - PRE-REQS:
      - INSTALL:
        - CycoD CLI Installation: install-cycod-cli.md
        # ... more items
  - CYCOD:
      - The Basics: basics/chat.md
      # ... more CYCOD items
  - CYCODMD:
      - Overview: cycodmd/index.md
      - The Basics: cycodmd/basics/getting-started.md
      # ... more CYCODMD items
  - REFERENCE:
      - cycod:
        # ... CYCOD reference items
      - cycodmd:
        # ... CYCODMD reference items
```

## How MkDocs Builds the Site

When you run `mkdocs build` or `mkdocs serve`, this is what happens:

1. MkDocs reads the `mkdocs.yml` configuration file
2. It processes all Markdown files in the `docs/` directory
3. It applies the specified theme and extensions
4. It generates HTML files according to the navigation structure
5. It copies static assets like images and CSS

## How to Make Changes to the Configuration

If you need to modify the configuration:

1. Open `mkdocs.yml` in your text editor
2. Make your changes
3. Save the file
4. Restart the MkDocs server (if running) to see the changes

!!! warning "Be Careful with YAML Syntax"
    YAML is sensitive to indentation and whitespace. Make sure to maintain the proper indentation when editing the configuration file.

## Common Configuration Tasks

### Adding a New Page to CYCOD Documentation

To add a new page to the CYCOD section:

```yaml
nav:
  - CYCOD:
      - The Basics: basics/chat.md
      - Your New Page: basics/your-new-page.md  # Add this line
```

### Adding a New Page to CYCODMD Documentation

To add a new page to the CYCODMD section:

```yaml
nav:
  - CYCODMD:
      - Overview: cycodmd/index.md
      - The Basics: cycodmd/basics/getting-started.md
      - Your New Page: cycodmd/basics/your-new-page.md  # Add this line
```

### Adding a New Section to Navigation

To add a completely new section to the site navigation:

```yaml
nav:
  # ... existing sections
  - NEW SECTION:
      - Overview: new-section/index.md
      - Page 1: new-section/page1.md
```

### Enabling a New Feature

To enable a new theme feature:

```yaml
theme:
  # ... existing settings
  features:
    # ... existing features
    - new.feature
```

### Adding a New Markdown Extension

To add a new Markdown extension:

```yaml
markdown_extensions:
  # ... existing extensions
  - new_extension
```

## Next Steps

Now that you understand how MkDocs works and how it's configured:

- Learn about [styling and customization](styling.md)
- Explore [content authoring](content-authoring.md) techniques
- See how the [site structure](site-structure.md) is organized