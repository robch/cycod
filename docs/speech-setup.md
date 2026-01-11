# Speech Recognition Setup Guide

This guide explains how to set up and use speech recognition in cycod.

## Overview

cycod supports speech-to-text input using Azure Cognitive Services Speech SDK. This allows you to speak your prompts instead of typing them in interactive chat mode.

## Prerequisites

- Azure subscription (free tier available)
- Microphone hardware
- Internet connection

## Azure Speech Service Setup

### 1. Create Azure Speech Resource

1. Go to the [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Speech"
4. Select "Speech" (Cognitive Services)
5. Click "Create"
6. Fill in the required fields:
   - **Subscription**: Select your Azure subscription
   - **Resource group**: Create new or use existing
   - **Region**: Select a region close to you (e.g., `westus2`, `eastus`, `westeurope`)
   - **Name**: Choose a unique name for your resource
   - **Pricing tier**: Select Free (F0) for testing, or Standard (S0) for production
7. Click "Review + create", then "Create"

### 2. Get Your Credentials

After the resource is created:

1. Go to your Speech resource in the Azure Portal
2. Click "Keys and Endpoint" in the left menu
3. You'll see two keys (KEY 1 and KEY 2) and a region
4. Copy **KEY 1** (or KEY 2 - either works)
5. Note your **REGION** (e.g., `westus2`)

## Configure cycod

### Option 1: User-Level Configuration (Recommended)

Store credentials in your user profile so all projects can use them:

```bash
# Create config directory if it doesn't exist
mkdir -p ~/.cycod

# Save your speech key
echo "your-subscription-key-here" > ~/.cycod/speech.key

# Save your region
echo "westus2" > ~/.cycod/speech.region
```

### Option 2: Project-Level Configuration

Store credentials in a specific project:

```bash
# Create config directory in your project
mkdir -p .cycod

# Save your speech key
echo "your-subscription-key-here" > .cycod/speech.key

# Save your region
echo "westus2" > .cycod/speech.region
```

### Option 3: Global Configuration

Store credentials system-wide (Windows only):

```powershell
# Create global config directory
New-Item -Path "$env:ProgramData\cycod" -ItemType Directory -Force

# Save your speech key
"your-subscription-key-here" | Out-File -FilePath "$env:ProgramData\cycod\speech.key" -Encoding UTF8

# Save your region
"westus2" | Out-File -FilePath "$env:ProgramData\cycod\speech.region" -Encoding UTF8
```

## Usage

### Enable Speech Input

Use the `--speech` flag when starting cycod:

```bash
cycod --speech
```

### Using Speech Input

1. Start cycod with `--speech` flag
2. Press **Enter** on an empty line to open the context menu
3. Select **"Speech input"** from the menu
4. Wait for the "(listening)..." prompt
5. Speak your prompt clearly into the microphone
6. The recognized text will appear on screen as you speak (interim results)
7. When you stop speaking, the final text will be sent to the chat

### Example Session

```bash
$ cycod --speech

Welcome to cycod chat! Type your message and press Enter.
Press Enter on empty line for menu, or Ctrl+C to exit.

You> [Press Enter]

[Context menu appears]
  Continue chatting
  Speech input       ← Select this
  Reset conversation
  Exit

(listening)...
Interim: "what is"
Interim: "what is the"
Interim: "what is the weather"
Interim: "what is the weather today"

You> what is the weather today

AI> I don't have access to real-time weather data...
```

## Troubleshooting

### "Speech configuration not found" Error

**Problem**: You see this error when trying to use speech input.

**Solution**: 
- Verify that `speech.key` and `speech.region` files exist in one of the config locations
- Check that the files contain valid credentials
- Make sure there are no extra spaces or newlines in the files

### "Speech recognition failed" Error

**Problem**: Recognition fails or produces no output.

**Solution**:
- Check your internet connection (speech recognition requires online access)
- Verify your Azure subscription is active
- Check that you have enough quota in your Speech resource
- Make sure your microphone is working and not muted
- Try speaking louder or moving closer to the microphone
- Ensure no other applications are using the microphone exclusively

### No Microphone Detected

**Problem**: System can't find your microphone.

**Solution**:
- Check that your microphone is physically connected
- Verify microphone permissions in your OS settings:
  - **Windows**: Settings → Privacy → Microphone
  - **macOS**: System Preferences → Security & Privacy → Microphone
  - **Linux**: Check ALSA/PulseAudio settings
- Test your microphone in other applications
- Try restarting your system

### Poor Recognition Quality

**Problem**: Speech recognition produces incorrect or incomplete text.

**Solution**:
- Speak clearly and at a moderate pace
- Reduce background noise
- Use a better quality microphone
- Position the microphone 6-12 inches from your mouth
- Avoid shouting or whispering
- Check your internet connection speed

### Region Mismatch

**Problem**: Authentication fails even with correct key.

**Solution**:
- Verify your region matches your Azure resource region exactly
- Common regions: `westus2`, `eastus`, `westeurope`, `southeastasia`
- The region is case-sensitive and should be lowercase
- Check for typos in the region file

## Configuration File Locations

cycod searches for configuration files in this order (first found wins):

1. **Local**: `./.cycod/` (current directory)
2. **User**: `~/.cycod/` (user home directory)
3. **Global**: 
   - Windows: `%ProgramData%\cycod\`
   - macOS/Linux: `/etc/cycod/`

This allows you to:
- Override credentials per-project (local)
- Use default credentials across projects (user)
- Share credentials system-wide (global)

## Security Best Practices

### Protect Your Credentials

- **Never commit** `speech.key` or `speech.region` files to version control
- Add `.cycod/speech.key` to your `.gitignore` file
- Use file permissions to restrict access:
  ```bash
  chmod 600 ~/.cycod/speech.key
  chmod 600 ~/.cycod/speech.region
  ```

### Rotate Keys Regularly

- Azure provides two keys so you can rotate without downtime
- Rotate keys every 90 days or if compromised
- To rotate:
  1. Regenerate KEY 2 in Azure Portal
  2. Update your local `speech.key` file with KEY 2
  3. Test that speech input still works
  4. Regenerate KEY 1 in Azure Portal

### Use Free Tier for Development

- Free (F0) tier provides 5,000 transactions per month
- Perfect for development and testing
- Upgrade to Standard (S0) only when needed for production

## Azure Pricing

### Free Tier (F0)
- **Cost**: $0
- **Limit**: 5,000 transactions per month (5 hours of audio)
- **Best for**: Development, testing, personal use

### Standard Tier (S0)
- **Cost**: $1 per hour of audio processed
- **Limit**: Pay-as-you-go (no monthly limit)
- **Best for**: Production use, high volume

For current pricing details, see: https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/

## Supported Languages

Azure Speech Service supports 100+ languages and dialects. By default, cycod uses `en-US` (US English).

For a complete list of supported languages, see:
https://learn.microsoft.com/azure/cognitive-services/speech-service/language-support

## Additional Resources

- [Azure Speech Service Documentation](https://learn.microsoft.com/azure/cognitive-services/speech-service/)
- [Speech SDK Quickstart](https://learn.microsoft.com/azure/cognitive-services/speech-service/get-started-speech-to-text)
- [Speech Service FAQ](https://learn.microsoft.com/azure/cognitive-services/speech-service/faq-stt)

## Need Help?

- Check the [cycod documentation](./getting-started.md)
- Review the [troubleshooting section](#troubleshooting) above
- Open an issue on the cycod GitHub repository
- Consult Azure Speech Service documentation

---

**Note**: Speech recognition is an optional feature. cycod works perfectly fine without it if you prefer typing your prompts.
