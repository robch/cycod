# Using Environment Variables with MCP Servers

This tutorial explains how to use environment variables to safely configure MCP servers in ChatX.

## Overview

Environment variables are a crucial feature when working with Model Context Protocol (MCP) servers. They allow you to:

- Pass sensitive information securely
- Configure your MCP servers dynamically
- Keep credentials out of your code

## Basic Usage

When adding an MCP server to ChatX, you can set environment variables using the `--env` (or `-e`) option:

```bash
chatx mcp add server-name --command ./your-server --env "KEY=VALUE"
```

You can set multiple environment variables by using the `--env` option multiple times:

```bash
chatx mcp add server-name --command ./your-server --env "KEY1=VALUE1" --env "KEY2=VALUE2"
```

## Creating a Weather API MCP Server

Let's create a practical example: an MCP server that accesses a weather API.

### Step 1: Create a simple Python MCP server

First, let's create a simple Python script that will act as our MCP server:

```python title="weather_mcp.py"
#!/usr/bin/env python3
import json
import sys
import os
import requests

# This would normally be imported from a formal MCP library
def send_response(message_id, response_type, content):
    response = {
        "type": response_type,
        "id": message_id,
        "result": content
    }
    print(json.dumps(response))
    sys.stdout.flush()

def send_error(message_id, error_message):
    response = {
        "type": "error",
        "id": message_id,
        "error": error_message
    }
    print(json.dumps(response))
    sys.stdout.flush()

def main():
    # Get API key from environment variable
    api_key = os.environ.get("WEATHER_API_KEY")
    if not api_key:
        print(json.dumps({
            "protocol": "mcp-0.1",
            "capabilities": {},
            "error": "WEATHER_API_KEY environment variable not set"
        }))
        sys.stdout.flush()
        return
    
    # Announce capabilities
    print(json.dumps({
        "protocol": "mcp-0.1",
        "capabilities": {
            "get_weather": {
                "description": "Get weather information for a location",
                "parameters": {
                    "location": "The city or location to get weather for"
                }
            }
        }
    }))
    sys.stdout.flush()
    
    # Process incoming messages
    while True:
        try:
            line = sys.stdin.readline()
            if not line:
                break
                
            message = json.loads(line)
            message_id = message.get("id")
            
            # Handle get_weather capability
            if message.get("type") == "get_weather":
                location = message.get("location", "")
                if not location:
                    send_error(message_id, "Location not provided")
                    continue
                
                # Make API call - this is a placeholder, replace with actual API call
                try:
                    # Example API call
                    url = f"https://api.weatherapi.com/v1/current.json?key={api_key}&q={location}"
                    response = requests.get(url)
                    weather_data = response.json()
                    
                    # Format and return the weather data
                    result = {
                        "location": location,
                        "temperature": weather_data["current"]["temp_c"],
                        "condition": weather_data["current"]["condition"]["text"],
                        "humidity": weather_data["current"]["humidity"],
                        "wind": f"{weather_data['current']['wind_kph']} kph"
                    }
                    
                    send_response(message_id, "get_weather_result", result)
                except Exception as e:
                    send_error(message_id, f"Failed to get weather: {str(e)}")
                
        except Exception as e:
            # Send general error response
            error_response = {
                "type": "error",
                "id": message.get("id") if "message" in locals() else None,
                "error": str(e)
            }
            print(json.dumps(error_response))
            sys.stdout.flush()

if __name__ == "__main__":
    main()
```

Save this file as `weather_mcp.py` and make it executable:

```bash
chmod +x weather_mcp.py
```

### Step 2: Add the MCP server with environment variables

Now, add the MCP server to ChatX, setting the required API key as an environment variable:

```bash
chatx mcp add weather --command "./weather_mcp.py" --env "WEATHER_API_KEY=your_actual_api_key"
```

### Step 3: Use the MCP server in a chat

Now you can use your weather MCP server in a chat:

```bash
chatx --use-mcp weather
```

In the chat session, you can ask about the weather:

```
> What's the weather like in New York right now?
```

The ChatX AI will use your MCP server to retrieve real-time weather data from the API.

## Using Environment Variables for Different Configurations

You can create multiple MCP server configurations for different environments:

```bash
# Development environment
chatx mcp add weather-dev --command "./weather_mcp.py" \
  --env "WEATHER_API_KEY=dev_api_key" \
  --env "DEBUG=true" \
  --env "CACHE_TIMEOUT=0"

# Production environment
chatx mcp add weather-prod --command "./weather_mcp.py" \
  --env "WEATHER_API_KEY=prod_api_key" \
  --env "DEBUG=false" \
  --env "CACHE_TIMEOUT=3600"
```

## Best Practices

1. **Don't hardcode sensitive keys**: Always use environment variables for API keys, passwords, and tokens
   
2. **Use descriptive variable names**: Name your environment variables clearly (e.g., `API_KEY_WEATHER` is better than just `KEY`)
   
3. **Set appropriate scopes**: Use `--local` for project-specific MCP servers and `--user` for personal tools
   
4. **Document required environment variables**: If sharing your MCP server code, document all required environment variables
   
5. **Validate environment variables**: Have your MCP server validate that all required environment variables are set
   
6. **Use default values where appropriate**: For non-critical settings, provide reasonable defaults

## Troubleshooting

If your MCP server isn't working correctly, check these common issues:

1. **Environment variable not set**: Ensure you've set all required environment variables
   
2. **Incorrect variable name**: Double-check the spelling and case of your environment variables
   
3. **Permission issues**: Make sure your script has execute permissions (`chmod +x script.py`)
   
4. **Dependency issues**: Ensure all required libraries are installed

## Conclusion

Environment variables provide a secure and flexible way to configure MCP servers in ChatX. By using them effectively, you can:

- Keep sensitive information secure
- Create reusable MCP servers that work across different environments
- Avoid hardcoding configuration values in your scripts

For more information, see the [--env option reference](../reference/cli/options/env.md) and the [advanced MCP documentation](../advanced/mcp.md).