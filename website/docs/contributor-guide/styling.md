---
title: Styling & Theming
icon: material/palette
---

# Styling and Theming the CYCODEV Documentation

This guide explains how styling and theming work in the CYCODEV documentation website, and how to make customizations.

## The Material Theme

The CYCODEV documentation uses [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) as its base theme. This theme provides a clean, responsive design based on Google's Material Design guidelines.

## Theme Configuration

The theme is configured in the `mkdocs.yml` file under the `theme` section:

```yaml
theme:
  logo: assets/cycod.png
  name: material
  icon:
    annotation: material/arrow-right-circle 
  features:
    - navigation.tabs
    - navigation.sections
    # ... more features
  language: en
  palette:
    - scheme: default
      toggle: 
        icon: material/toggle-switch-off-outline
        name: Switch to dark mode
      primary: black
      accent: indigo
    - scheme: slate
      toggle:
        icon: material/toggle-switch
        name: Switch to light mode
      primary: black
      accent: indigo
```

### Key Theme Settings

- **Color Palette**: The `palette` section defines the color schemes for both light and dark modes
- **Features**: The `features` section enables specific theme features
- **Icons**: The `icon` section defines icon sets used throughout the site
- **Logo**: The `logo` property points to the site logo image

## Custom CSS

Custom CSS is added through the `extra_css` section in `mkdocs.yml`:

```yaml
extra_css:
  - assets/extra.css
  - assets/cli-toggle.css
```

### Important CSS Files

- **`assets/extra.css`**: Contains general styling overrides
- **`assets/cli-toggle.css`**: Styles specific to the CLI toggle functionality

## Custom JavaScript

Custom JavaScript is added through the `extra_javascript` section:

```yaml
extra_javascript:
  - js/cli-toggle.js
```

This allows for additional interactive features beyond what the theme provides.

## How to Make Styling Changes

If you need to customize the site's appearance:

### 1. Add CSS Overrides

For small styling tweaks, add your CSS to `docs/assets/extra.css`:

```css
/* Example: Change the font size for code blocks */
.md-typeset code {
  font-size: 0.9em;
}
```

### 2. Modify Color Scheme

To change the color scheme, update the `palette` section in `mkdocs.yml`:

```yaml
palette:
  - scheme: default
    primary: indigo  # Change primary color
    accent: pink     # Change accent color
```

Material for MkDocs supports these [primary colors](https://squidfunk.github.io/mkdocs-material/setup/changing-the-colors/#primary-color) and [accent colors](https://squidfunk.github.io/mkdocs-material/setup/changing-the-colors/#accent-color).

### 3. Add Custom Fonts

To use custom fonts, add the font files to `docs/assets/fonts/` and reference them in your CSS:

```css
@font-face {
  font-family: 'CustomFont';
  src: url('../fonts/custom-font.woff2') format('woff2');
}

body {
  font-family: 'CustomFont', sans-serif;
}
```

## Using Icons

The Material theme includes over 8,000 icons from various icon sets:

- [Material Design icons](https://pictogrammers.com/library/mdi/)
- [FontAwesome icons](https://fontawesome.com/icons)
- [Simple Icons](https://simpleicons.org/)

You can use these icons in your Markdown content:

```markdown
:material-rocket-launch: Getting Started
:fontawesome-brands-github: GitHub Repository
```

## Custom Components

The CYCODEV documentation includes some custom UI components:

### CLI Toggle

The site uses a custom CLI toggle component that allows users to switch between different command-line interfaces. This is implemented through:

- CSS in `assets/cli-toggle.css`
- JavaScript in `js/cli-toggle.js`

## Styling for Different Tools

When styling content for different tools in the CYCODEV suite:

### Tool-Specific Colors

Consider using consistent colors for each tool to help users visually distinguish between them:

```css
/* CYCOD-specific styling */
.cycod-element {
  color: #3d5afe;
}

/* CYCODMD-specific styling */
.cycodmd-element {
  color: #00b0ff;
}
```

### Tool Icons

Use different icons for different tools to help with visual identification:

- CYCOD: `:material-console:` or `:material-chat:`
- CYCODMD: `:material-file-document:` or `:material-markdown:`

## Best Practices

When making styling changes:

1. **Start Small**: Make incremental changes and test them thoroughly
2. **Use Browser DevTools**: Inspect elements and test CSS changes before implementing them
3. **Respect Responsiveness**: Ensure your changes work on both desktop and mobile devices
4. **Maintain Consistency**: Follow the existing design patterns and color schemes
5. **Tool Consistency**: Maintain consistent styling patterns across different tool documentation

## Next Steps

Now that you understand how styling works:

- Learn about [content authoring](content-authoring.md)
- Understand the [site structure](site-structure.md)
- Explore [deployment](deployment.md) processes