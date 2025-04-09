# github login

Authenticates with GitHub to access Copilot features.

## Syntax

```bash
chatx github login [options]
```

## Description

The `chatx github login` command initiates the GitHub authentication process to obtain access to GitHub Copilot features. It guides you through the authentication flow, which typically requires a web browser to complete.

## Options

| Option | Description |
|--------|-------------|
| `--no-browser` | Don't automatically open the authentication URL in a web browser |
| `--json`, `-j` | Output results in JSON format |

## Authentication Process

1. The command initiates the GitHub device authentication flow
2. A device code and verification URL are displayed
3. Your default web browser is automatically opened to the verification URL (unless `--no-browser` is used)
4. You will be asked to enter the device code on the GitHub website
5. After authorizing CHATX, GitHub will provide an access token
6. The token is securely stored in your user configuration for future use

## Examples

Authenticate with GitHub:

```bash
chatx github login
```

Authenticate without automatically opening a browser:

```bash
chatx github login --no-browser
```

## Output

The command outputs information about the authentication process:

```
To authenticate, visit: https://github.com/login/device
and enter code: ABCD-1234

Waiting for authentication...
Successfully authenticated with GitHub.
GitHub Copilot access is now enabled.
```

When using `--json` option, the output includes the authentication status in JSON format:

```json
{
  "status": "authenticated",
  "message": "Successfully authenticated with GitHub"
}
```

If authentication fails, an appropriate error message is displayed:

```
Authentication failed: The device code has expired. Please try again.
```

## Notes

- The GitHub token is stored securely in your user configuration
- The token is used for accessing GitHub Copilot features
- You can check your authentication status with `chatx config get github.token`
- To log out, use `chatx config clear github.token --user`