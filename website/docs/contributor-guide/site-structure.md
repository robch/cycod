---
title: Site Structure
icon: material/sitemap
---

# Understanding the CYCODEV Documentation Structure

This guide explains the organization of the CYCODEV documentation website, including how navigation works and where different types of content should be placed.

## Directory Structure

The documentation site has the following main directories:

```
docs/
├── advanced/         # Advanced features documentation
├── assets/           # Images, CSS, and other static assets
├── basics/           # Basic usage documentation
├── contributor-guide/ # Documentation for contributors (this section)
├── cycodmd/          # CYCODMD tool documentation
│   ├── basics/       # Basic CYCODMD usage
│   └── advanced/     # Advanced CYCODMD features
├── js/               # JavaScript files
├── memory/           # Documentation on memory features (currently disabled)
├── providers/        # Provider-specific documentation
├── reference/        # Command reference documentation
│   ├── cycod/        # CYCOD command references
│   └── cycodmd/      # CYCODMD command references
├── snippets/         # Reusable content snippets
├── tools/            # Tools documentation
├── tutorials/        # Tutorial content
├── usage/            # Usage documentation
├── workflows/        # Workflow documentation
└── index.md          # Home page
```

## Navigation Structure

The site navigation is defined in the `nav` section of `mkdocs.yml`. It organizes content into a hierarchical structure that appears in the site's navigation menu.

### Main Sections

The documentation is organized into these main sections:

- **HOME**: Landing page
- **PRE-REQS**: Installation and setup instructions
- **CYCOD**: Documentation for the main CYCOD CLI tool
- **CYCODMD**: Documentation for the CYCODMD markdown processing tool
- **WORKFLOWS**: Use cases and workflow documentation
- **ADVANCED**: Advanced features and configuration
- **REFERENCE**: Detailed command reference for all tools
- **CONTRIBUTOR GUIDE**: Documentation for contributors (this section)

Note: The **MEMORY** section is currently disabled (commented out) in the navigation.

### How Pages Appear in Navigation

For a page to appear in the navigation, it must be included in the `nav` section of `mkdocs.yml`:

```yaml
nav:
  - HOME: index.md
  - SECTION NAME:
    - Subsection:
      - Page Title: path/to/page.md
```

The navigation structure has these levels:

1. **Main sections**: Top-level categories (all caps by convention)
2. **Subsections**: Groups of related pages within a section
3. **Pages**: Individual documentation pages

## Special Pages

### Index Pages

Each section typically has an index page (e.g., `section/index.md`) that provides an overview of that section. These pages use the `navigation.indexes` feature to serve as landing pages for their sections.

### Hidden Pages

Some pages are intentionally excluded from navigation but still accessible via direct links. These pages typically have front matter like:

```yaml
---
hide:
  - navigation
---
```

## Adding New Content

When adding new content to the documentation:

### For a New Page in an Existing Section

1. Create your Markdown file in the appropriate directory
2. Add it to the `nav` section in `mkdocs.yml`

Example for CYCOD:

```yaml
nav:
  - CYCOD:
    - The Basics:
      - Existing Page: basics/existing-page.md
      - Your New Page: basics/new-page.md  # Add this line
```

Example for CYCODMD:

```yaml
nav:
  - CYCODMD:
    - The Basics:
      - Existing Page: cycodmd/basics/existing-page.md
      - Your New Page: cycodmd/basics/new-page.md  # Add this line
```

### For a New Section

1. Create a new directory for your section
2. Create an `index.md` file in that directory
3. Add additional pages as needed
4. Add the new section to the `nav` in `mkdocs.yml`

Example:

```yaml
nav:
  # Existing sections...
  - YOUR NEW SECTION:
    - Overview: new-section/index.md
    - First Topic: new-section/topic1.md
```

## Tool-Specific Documentation

### CYCOD Documentation

CYCOD documentation is organized in the `basics/`, `advanced/`, and `reference/cycod/` directories.

### CYCODMD Documentation

CYCODMD documentation is organized in the `cycodmd/basics/`, `cycodmd/advanced/`, and `reference/cycodmd/` directories.

## Cross-Linking Between Pages

To create links between pages in the documentation:

```markdown
[Link to another page](../path/to/page.md)
```

Use relative paths based on the file structure in the `docs/` directory.

## Best Practices for Organization

When organizing content:

1. **Group related content**: Keep related topics together in the same section
2. **Use consistent naming**: Follow existing patterns for filenames and paths
3. **Consider the user journey**: Organize content in a logical progression
4. **Balance breadth and depth**: Avoid creating too many sections or nesting too deeply
5. **Use index pages effectively**: Provide good overviews at each section level
6. **Maintain tool separation**: Keep documentation for different tools (CYCOD, CYCODMD) in their respective directories

## Next Steps

Now that you understand the site structure:

- Learn about the [deployment process](deployment.md)
- Check out additional [resources](resources.md) for documentation