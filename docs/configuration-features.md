# Configuration Features in CycoD

This document explains the differences and relationships between various configuration features in CycoD: aliases, custom prompts, config files, and profiles.

## Overview

CycoD offers multiple ways to configure and customize your experience:

- **Aliases**: Command-line shortcuts for launching CycoD with specific parameters
- **Custom Prompts**: Text templates used within a chat session
- **Config Files**: Persistent application settings
- **Profiles**: Collections of settings that can be loaded as a unit

All these features use a common scoping system with three levels:
- **Local**: Specific to the current directory
- **User**: Specific to the current user
- **Global**: Available to all users system-wide

## Aliases

Aliases are shortcuts for frequently used command-line options.

### Key Characteristics

- **Purpose**: Allow users to save a set of command-line parameters under a name for easy reuse
- **Usage**: Called with `--aliasname` from the command line
- **Storage**: Stored as `.alias` files in `aliases` directories
- **Scopes**: Support local, user, and global scopes
- **Creation**: Created using `--save-alias`, `--save-user-alias`, or `--save-global-alias`
- **Management**: Managed with dedicated commands:
  - `cycod alias list` - Lists all aliases
  - `cycod alias get` - Views a specific alias
  - `cycod alias delete` - Deletes an alias
- **Content**: Contains command-line arguments that would otherwise be typed manually

### Example

```bash
# Create an alias
cycod --system-prompt "You are an expert in Python programming." --save-alias python-expert

# Use the alias
cycod --python-expert --input "Write a function to sort a list"
```

## Custom Prompts

Prompts are reusable text templates that can be quickly inserted into chat conversations.

### Key Characteristics

- **Purpose**: Provide quick access to frequently used text templates during chat sessions
- **Usage**: Called with `/promptname` during a chat session
- **Storage**: Stored as `.prompt` files in `prompts` directories 
- **Scopes**: Support local, user, and global scopes
- **Creation**: Created using `cycod prompt create promptname "text"`
- **Management**: Managed with dedicated commands:
  - `cycod prompt list` - Lists all prompts
  - `cycod prompt get` - Views a specific prompt
  - `cycod prompt delete` - Deletes a prompt
- **Content**: Contains text templates that can contain instructions or complex queries
- **Handling**: Supports multiline content by storing it in separate files

### Example

```bash
# Create a prompt
cycod prompt create code-review "Please review this code and provide feedback:"

# In a chat session
/code-review
```

## Config Files

Config files store persistent settings for the application.

### Key Characteristics

- **Purpose**: Store configuration settings for the application
- **Format**: Supports YAML and INI formats
- **Scopes**: Support local, user, global and filename scopes
- **Management**: Managed with dedicated commands:
  - `cycod config list` - Lists all settings
  - `cycod config get` - Gets a specific setting
  - `cycod config set` - Sets a setting
  - `cycod config clear` - Clears a setting
  - `cycod config add` - Adds a value to a list setting
  - `cycod config remove` - Removes a value from a list setting
- **Usage**: Used internally by the application to maintain state and preferences
- **Loading**: All appropriate config files are loaded at startup, with local overriding user, which overrides global

### Example

```bash
# Set a configuration value
cycod config set default-model gpt-4o

# Set a timeout for chat completions (in seconds)
cycod config set App.ChatCompletionTimeout 120

# View configuration 
cycod config list
```

## Profiles

Profiles are collections of configuration settings that can be loaded as a unit.

### Key Characteristics

- **Purpose**: Bundle multiple configuration settings for easy loading
- **Usage**: Loaded using `--profile profilename`
- **Storage**: Stored as files in `profiles` directories, can use `.yaml` extension or no extension
- **Scopes**: Can be stored in any scope but loaded across all scopes
- **Content**: Contains configuration settings that are loaded into the configuration store
- **Loading**: When a profile is loaded, its settings are added to the configuration store
- **Searching**: When looking for a profile, CycoD searches all scopes and parent directories

### Example

```bash
# Create a profile file in .cycod/profiles/development.yaml
# Then load it
cycod --profile development
```

## Key Differences and Relationships

1. **Purpose and Usage**:
   - **Aliases**: Command-line shortcuts for launching CycoD with specific parameters
   - **Prompts**: Text templates used within a chat session
   - **Config Files**: Persistent application settings
   - **Profiles**: Collections of settings that can be loaded as a unit

2. **Scoping System**:
   - All four features use a common scoping system (local, user, global)
   - The scope affects visibility and precedence of the items
   - Local items override user items, which override global items

3. **Management**:
   - Aliases, prompts, and configs have dedicated management commands
   - Profiles are loaded through the command line but don't have specific management commands

4. **Relationship to Configuration**:
   - Config files provide the underlying storage mechanism for application settings
   - Profiles are a way to load multiple settings into the configuration system
   - Aliases and prompts are specialized features that use similar file organization but serve different purposes

## When to Use Each Feature

- **Aliases**: When you want to save command-line parameters for reuse when launching CycoD
- **Prompts**: When you want quick access to text templates during chat sessions
- **Config Files**: For persistent application settings
- **Profiles**: When you want to quickly switch between different sets of configuration settings

This flexible and powerful configuration system supports multiple use cases and workflows within the CycoD application.