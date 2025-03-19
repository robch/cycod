# Troubleshooting

This guide provides solutions for common issues you might encounter when using ChatX.

## API Connection Issues

### Issue: Unable to connect to OpenAI API

**Symptoms:**
- Error messages about failed API calls
- "Connection refused" or timeout errors

**Possible Solutions:**
1. **Check API Key**: Ensure your OpenAI API key is correctly set in the environment variable `OPENAI_API_KEY`.
   ```bash
   # Check if the key is set (Linux/macOS)
   echo $OPENAI_API_KEY
   
   # Set the key if needed
   export OPENAI_API_KEY=your_api_key_here
   ```

2. **Check Internet Connection**: Verify you have a stable internet connection.

3. **Check API Status**: Visit [OpenAI Status Page](https://status.openai.com/) to see if there are any ongoing service disruptions.

4. **Check Firewall Settings**: Ensure your firewall allows outgoing connections to the OpenAI API.

## Function Calling Issues

### Issue: Shell Commands Not Working

**Symptoms:**
- Shell commands return errors or don't execute
- Permission errors when running commands

**Possible Solutions:**
1. **Check Permissions**: Ensure ChatX has the necessary permissions to execute commands.

2. **Path Issues**: For commands that rely on specific tools, ensure they are installed and in your PATH.
   ```bash
   # Check if a command is available (e.g., git)
   which git
   ```

3. **Shell Availability**: Ensure the requested shell is available on your system:
   - For `RunBashCommandAsync`: Bash must be installed
   - For `RunCmdCommandAsync`: Windows Command Prompt (only works on Windows)
   - For `RunPowershellCommandAsync`: PowerShell must be installed

### Issue: File Operations Failed

**Symptoms:**
- Error messages when trying to create or modify files
- "Access denied" or "File not found" errors

**Possible Solutions:**
1. **Check File Permissions**: Ensure you have read/write permissions for the directories you're working with.

2. **Check File Paths**: Ensure you're using correct absolute or relative paths.

3. **File Locks**: Check if files are locked by other applications.

## Command Line Options Issues

### Issue: Command Line Arguments Not Working

**Symptoms:**
- Command doesn't behave as expected
- Arguments seem to be ignored

**Possible Solutions:**
1. **Check Syntax**: Ensure you're using the correct syntax for options.
   ```bash
   # Correct syntax for system prompt
   chatx --system-prompt "Your prompt here"
   
   # Incorrect (missing quotes for multi-word prompt)
   chatx --system-prompt Your prompt here
   ```

2. **Escape Special Characters**: If your arguments contain special characters, ensure they're properly escaped.

3. **Check for Conflicting Options**: Some options might conflict with each other.

### Issue: Aliases Not Working

**Symptoms:**
- Alias commands aren't recognized
- Error messages when trying to use aliases

**Possible Solutions:**
1. **Check Alias Path**: Ensure the alias file exists in the `.chatx/aliases` directory.

2. **Check Alias Format**: Verify the alias file contains valid command options.

3. **Permissions**: Ensure you have read permissions for the alias file.

## Chat History Issues

### Issue: Unable to Save or Load Chat History

**Symptoms:**
- Error messages when trying to save or load history
- Missing chat history files

**Possible Solutions:**
1. **Check File Permissions**: Ensure you have read/write permissions for the directory where history files are stored.

2. **Check File Path**: Ensure the specified path for history files is valid.

3. **File Format**: If manually editing history files, ensure they maintain the correct JSONL format.

## Performance Issues

### Issue: Slow Responses

**Symptoms:**
- AI responses take a long time to generate
- Commands execute slowly

**Possible Solutions:**
1. **Check Internet Connection**: Slow or unstable internet connections can cause delays.

2. **Resource Intensive Commands**: Some shell commands might take a long time to execute.

3. **Large Chat History**: Very large chat histories can slow down the AI's response time.

## Environment and Dependency Issues

### Issue: Application Crashes or Won't Start

**Symptoms:**
- Application crashes immediately
- Error messages about missing dependencies

**Possible Solutions:**
1. **Check .NET Installation**: Ensure you have .NET 8.0 SDK or later installed.
   ```bash
   # Check .NET version
   dotnet --version
   ```

2. **Rebuild the Application**: Try rebuilding the application from source.
   ```bash
   dotnet build
   ```

3. **Check for Updates**: Ensure you're using the latest version of ChatX.

## Getting More Help

If you're experiencing issues not covered in this guide:

1. **Check GitHub Issues**: Visit the [GitHub Issues page](https://github.com/username/chatx/issues) to see if others have reported similar problems.

2. **Create a New Issue**: If your problem hasn't been reported, consider creating a new issue with:
   - A clear description of the problem
   - Steps to reproduce
   - Error messages (if any)
   - Your environment details (OS, .NET version, etc.)

3. **Check SUPPORT.md**: See the [SUPPORT.md](../SUPPORT.md) file for additional support resources.