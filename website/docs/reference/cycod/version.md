# version

Display the current version of CycoD.

## Syntax

```bash
cycod version
```

## Description

The `cycod version` command displays the current version number of the CycoD tool. This is useful for:

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
cycod version
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

- [help](help.md) - Access the CycoD help system
- [Getting Started](../../install-cycod-cli.md) - Installation and first steps