# version

Display the current version of ChatX.

## Syntax

```bash
chatx version
```

## Description

The `chatx version` command displays the current version number of the ChatX tool. This is useful for:

- Verifying which version you have installed
- Reporting issues with specific version details
- Checking if you have the latest version of the tool

The version information displays either:
- Just the version number (for release builds)
- Version number plus a commit hash (for development or pre-release builds)

## Options

This command doesn't accept any additional options.

## Examples

Display the current version:

```bash
chatx version
```

## Output

### Example output for release build:

```
Version: 1.0.0
```

### Example output for development build:

```
Version: 1.0.0+89178f5a0a8bac990aac639ba5261610daaddd40
```

## Related Topics

- [help](help.md) - Access the ChatX help system
- [Getting Started](../../install-chatx-cli.md) - Installation and first steps