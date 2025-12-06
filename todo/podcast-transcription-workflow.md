# Podcast Download and Transcription Workflow - YANSS Episode 322

**Date:** December 5, 2024  
**Episode:** You Are Not So Smart #322 - "How the humbling science behind our general lack of intellectual humility can make us better humans"  
**Guest:** Tenelle Porter (psychologist researching intellectual humility)  

## Overview

This document chronicles the complete end-to-end workflow for downloading and transcribing a podcast episode using existing Azure AI CLI tools and infrastructure. This serves as a proof-of-concept for the proposed `cycodsp` (cycod speech) CLI extension.

## Infrastructure Used

### Azure Resources (from macaroni project)
- **Resource Group:** `macaroni-rg`
- **Speech Service:** `macaroni-speech`
- **Region:** `eastus`
- **Endpoint:** `https://eastus.api.cognitive.microsoft.com/`

### Tools and Services
- **Azure AI CLI** (version 1.0.0-preview-20241008.1)
- **Azure CLI** (for resource discovery)
- **curl** (for audio download)
- **Python** (for transcript processing)
- **Existing ytd repo** (cloned for workspace)

## Step-by-Step Workflow

### 1. Resource Discovery and Configuration

#### Find Azure Speech Resource
```bash
# List resource groups containing 'macaroni'
az group list --output table --query "[?contains(name, 'macaroni')]"

# List resources in the group
az resource list --resource-group macaroni-rg --output table

# Get Speech service keys
az cognitiveservices account keys list --name macaroni-speech --resource-group macaroni-rg

# Get Speech service endpoint and region
az cognitiveservices account show --name macaroni-speech --resource-group macaroni-rg --query "{endpoint:properties.endpoint,location:location}"
```

#### Configure Azure AI CLI
```bash
# Set region and key for Speech service
ai config speech @region --set eastus
ai config speech @key --set 9d3f30968560457ea92b90605c88ef17

# Verify configuration
ai config speech @region && ai config speech @key
```

### 2. Podcast Discovery and Audio Download

#### Find the Episode
- **Main site:** https://youarenotsosmart.com/
- **Episode page:** https://youarenotsosmart.com/2025/11/24/yanss-322-...
- **YouTube version:** https://www.youtube.com/watch?v=8tHDVMKMOfI
- **Direct audio URL:** https://stitcher.simplecastaudio.com/.../audio/128/default.mp3

#### Download Audio
```bash
cd /c/src/ytd
curl -L "https://stitcher.simplecastaudio.com/aa9f2648-25e9-472a-af42-4e5017da38cf/episodes/f6442c9c-0b6f-40f1-b624-f04b48b7aa5e/audio/128/default.mp3" -o intellectual-humility-episode.mp3 --progress-bar
```

**Result:** 63MB MP3 file (1h 7m 42s duration)

### 3. Transcription Using Azure Speech Batch API

#### Create Batch Transcription
```bash
ai speech batch transcription create --name "YANSS Intellectual Humility Direct" --content "https://stitcher.simplecastaudio.com/aa9f2648-25e9-472a-af42-4e5017da38cf/episodes/f6442c9c-0b6f-40f1-b624-f04b48b7aa5e/audio/128/default.mp3"
```

**Response:** 
- Transcription ID: `19dce08f-8c04-47a7-a2e9-9fd35a3fa691`
- Status: Created successfully

#### Monitor Progress
```bash
# Check status periodically
ai speech batch transcription status --id "19dce08f-8c04-47a7-a2e9-9fd35a3fa691"
```

**Timeline:**
- **15:41:38Z:** Created (NotStarted)
- **15:59:28Z:** Completed (Succeeded)
- **Total Processing Time:** ~18 minutes

#### Download Results
```bash
# List available files
ai speech batch transcription list --id "19dce08f-8c04-47a7-a2e9-9fd35a3fa691" --files

# Download transcript JSON
curl -L "https://spsvcprodeus.blob.core.windows.net/.../19dce08f-8c04-47a7-a2e9-9fd35a3fa691_0_0.json?..." -o yanss-322-intellectual-humility-transcript.json
```

**Result:** 1.5MB JSON file with detailed transcription data

### 4. Transcript Processing and Analysis

#### Convert JSON to Readable Markdown
```python
import json

# Load the transcript
with open('yanss-322-intellectual-humility-transcript.json', 'r') as f:
    data = json.load(f)

# Extract all display text from channel 0 with timestamps
transcript_text = []
transcript_text.append('# YANSS Episode 322 - Intellectual Humility')
# ... (metadata header)

# Process all recognized phrases
for phrase in data['recognizedPhrases']:
    if phrase['recognitionStatus'] == 'Success' and phrase['channel'] == 0:
        offset_seconds = phrase['offsetMilliseconds'] / 1000
        minutes = int(offset_seconds // 60)
        seconds = int(offset_seconds % 60)
        timestamp = f'{minutes:02d}:{seconds:02d}'
        display_text = phrase['nBest'][0]['display']
        transcript_text.append(f'**[{timestamp}]** {display_text}')

# Write to markdown
with open('yanss-322-intellectual-humility-transcript.md', 'w', encoding='utf-8') as f:
    f.write('\n'.join(transcript_text))
```

**Result:** 67KB markdown file with 611 timestamped phrases

#### Content Analysis Sample
```python
# Search for intellectual humility mentions
for phrase in data['recognizedPhrases']:
    if phrase['recognitionStatus'] == 'Success':
        text = phrase['nBest'][0]['display'].lower()
        if 'intellectual humility' in text or 'humble' in text:
            offset = phrase['offsetMilliseconds']/1000
            print(f'[{offset:.1f}s]: {phrase["nBest"][0]["display"]}')
```

**Key Findings:**
- Multiple references to intellectual humility research
- Guest expert: Tenelle Porter (psychologist)
- References to Carl Sagan, Richard Feynman
- Discussion of cognitive biases and fallibility

## Technical Specifications

### Audio Processing
- **Input Format:** MP3 (128kbps)
- **Duration:** 1h 7m 42s (4,062,690 milliseconds)
- **File Size:** 63MB
- **Source:** Simplecast CDN

### Transcription Results
- **API Used:** Azure Speech Batch Transcription v3.1
- **Model:** Base model (en-US)
- **Features:** 
  - Automatic punctuation
  - Profanity masking
  - Dual channel processing
  - Confidence scoring
  - Word-level timestamps

### Output Formats
1. **JSON:** Complete transcription with metadata (1.5MB)
2. **Markdown:** Human-readable with timestamps (67KB)

## Lessons Learned

### What Worked Well
1. **Direct audio URLs:** Simplecast URLs worked perfectly with batch API
2. **Azure CLI integration:** Seamless configuration and management
3. **MP3 support:** No conversion needed for batch transcription
4. **Processing time:** 18 minutes for 67-minute audio (reasonable)
5. **Quality:** High accuracy transcription with proper punctuation

### Challenges and Solutions
1. **YouTube download issues:** YoutubeExplode library had compatibility issues
   - **Solution:** Used direct podcast audio URL instead
2. **Local file processing:** Batch API requires remote URLs
   - **Solution:** Used direct Simplecast URL (no local HTTP server needed)
3. **CLI version limitations:** Current AI CLI doesn't have `speech transcribe` command
   - **Workaround:** Used batch transcription API successfully

### Performance Metrics
- **Audio Duration:** 67 minutes 42 seconds
- **Processing Time:** ~18 minutes (3.8x real-time speed)
- **Transcript Accuracy:** High (based on spot checks)
- **Total Phrases:** 611 timestamped segments
- **Average Phrase Length:** ~6 seconds

## Future cycodsp Integration

### Proposed Command Structure
```bash
# Download + transcribe podcast
cycodsp podcast --url "https://podcast-episode.mp3" --transcribe --output episode.md

# YouTube workflow
cycodsp youtube --url "https://youtube.com/watch?v=ID" --transcribe --output video.md

# Analysis pipeline
cycodsp analyze --file episode.md --prompt "Extract key insights about intellectual humility"
```

### Technical Architecture
- **Audio Download:** Direct URL support + YouTube integration
- **Transcription:** Azure Speech Fast Transcription API (when available) or Batch API
- **Processing:** JSON → Markdown → Analysis pipeline
- **Integration:** Leverage existing Azure credentials and configuration

## Files Created

1. **intellectual-humility-episode.mp3** (63MB) - Original audio
2. **yanss-322-intellectual-humility-transcript.json** (1.5MB) - Raw transcription
3. **yanss-322-intellectual-humility-transcript.md** (67KB) - Formatted transcript
4. **podcast-transcription-workflow.md** (this file) - Process documentation

## Next Steps

1. **Integrate with cycodsp:** Use this workflow as foundation
2. **Test other content:** Validate process with different audio sources  
3. **Analysis development:** Build AI-powered content analysis features
4. **Automation:** Create end-to-end pipeline scripts
5. **UI/UX:** Consider integration with existing Cycod CLI infrastructure

---

**Total Workflow Time:** ~30 minutes (including discovery and setup)  
**Success Rate:** 100% (from audio URL to readable transcript)  
**Cost:** Minimal (Azure Speech API usage only)  

This workflow proves the viability of the proposed cycodsp extension and provides a concrete foundation for implementation.