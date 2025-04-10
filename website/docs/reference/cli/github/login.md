# github login

Authenticates with GitHub to access Copilot features.

## Syntax

```bash
chatx github login
```

## Description

The `chatx github login` command initiates the GitHub device authentication flow to obtain access to GitHub Copilot features. This authentication process is required before you can use ChatX with GitHub Copilot as your AI provider.

The command will:
1. Generate a device code and verification URL
2. Prompt you to visit the URL and enter the code
3. Wait for successful authentication on GitHub
4. Save the received token to your configuration

## Authentication Process

1. The command initiates the GitHub device authentication flow
2. A device code and verification URL are displayed in your terminal
3. You need to visit the URL in a web browser and enter the displayed code
4. After entering the code, GitHub will ask for permission to authorize ChatX
5. Once authorized, the command will automatically receive and save the token
6. The token is stored in your configuration for future use

## Examples

Authenticate with GitHub:

```bash
chatx github login
```

## Output

The command outputs information about the authentication process:

```
Please visit https://github.com/login/device and enter code ABCD-1234 to authenticate.
GitHub authentication successful!
GitHub login successful! You can now use chatx with GitHub Copilot.
```

If authentication fails or times out, you'll need to run the command again to generate a new device code.

## Notes

- The GitHub token is stored securely in your user configuration
- The token is used for accessing GitHub Copilot features
- After authentication, you can use `--use-copilot` to select GitHub Copilot as your AI provider
- You can check your authentication status with `chatx config get GitHub.Token --any`
- To log out or revoke access, use `chatx config clear GitHub.Token --user`
- A valid GitHub Copilot subscription is required to use this feature

## See Also

- [GitHub Copilot Provider](/providers/github-copilot.md) - Learn more about using GitHub Copilot with ChatX
- [--use-copilot](../options/use-copilot.md) - Flag to use GitHub Copilot as the AI provider