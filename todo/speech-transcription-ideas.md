# Speech Transcription Ideas for Cycod CLI

## Overview

Inspired by the recent addition of the `ai speech transcribe` command in Azure AI CLI (added January 30, 2025), this document outlines ideas for implementing speech transcription and audio processing capabilities in the Cycod CLI toolset.

## Background & Motivation

The Azure AI CLI recently introduced a "Fast Transcription" feature (`ai speech transcribe`) that uses REST APIs instead of the streaming Speech SDK, optimized for non-streaming audio file processing. This provides:

- Direct REST API calls to `https://{region}.api.cognitive.microsoft.com/speechtotext/transcriptions:transcribe`
- Optimized for batch file processing without streaming overhead
- Supports diarization, multiple languages, profanity filtering
- Outputs in text, SRT, and VTT formats

### Personal Use Case

Inspired by the "You Are Not So Smart" podcast episode on intellectual humility, the goal is to create a workflow for:
1. Downloading audio content (YouTube videos, podcasts)
2. Transcribing the audio
3. Analyzing the content for insights
4. Potentially using those insights to improve AI system prompts (e.g., incorporating intellectual humility principles)

## Existing Related Code

### robch/ytd (YouTube Downloader + Transcriber)
**Repository**: https://github.com/robch/ytd  
**Description**: "Downloads YouTube.com audio, and transcribes it using Azure Speech Recognition"

**Key Features**:
- Uses YoutubeExplode library for audio extraction
- Azure Cognitive Services Speech SDK integration
- Downloads audio streams and transcribes to console

**Files**:
- `Program.cs` - Main entry point
- `ConsoleProgress.cs` - Progress reporting
- `ytd.csproj` - Project file

### robch/searchy (Web Search & Content Tool)
**Repository**: https://github.com/robch/searchy  
**Description**: Playwright-based web search and content extraction

**Key Features**:
- Google and Bing search automation
- Content downloading from search results
- HTML stripping capabilities
- Integration with Azure AI CLI via helper functions

**Potential Integration**: Could be used to find podcast RSS feeds, episode links, or related content.

## Proposed Cycod Speech Extension: `cycodsp`

### Command Structure Ideas

```bash
# Basic transcription
cycodsp transcribe --file audio.wav

# YouTube video download + transcribe
cycodsp youtube --url "https://youtube.com/watch?v=VIDEO_ID" --transcribe

# Podcast episode download + transcribe
cycodsp podcast --url "https://podcast-url/episode.mp3" --transcribe

# Batch processing
cycodsp batch --files "*.mp3" --output-format srt

# Analysis pipeline
cycodsp analyze --transcript "episode.txt" --prompt "Extract key insights about intellectual humility"
```

### Core Features

1. **Audio Download**
   - YouTube video audio extraction
   - Podcast episode downloading
   - Support for various audio formats

2. **Transcription**
   - Integration with Azure Speech Fast Transcription API
   - Multiple output formats (text, SRT, VTT, JSON)
   - Diarization support for multi-speaker content
   - Language auto-detection

3. **Content Analysis**
   - AI-powered content summarization
   - Key insight extraction
   - Topic identification
   - Sentiment analysis

4. **Workflow Integration**
   - Save transcripts to structured formats
   - Integration with existing Cycod chat/AI features
   - Export to various formats for further processing

### Technical Implementation

#### Dependencies
- **YoutubeExplode**: For YouTube audio extraction (from ytd repo)
- **Azure.CognitiveServices.Speech** or **HttpClient**: For transcription
- **Podcast parsing libraries**: For RSS/podcast handling
- **Cycod infrastructure**: CLI parsing, logging, etc.

#### Architecture
```
cycodsp/
├── Commands/
│   ├── TranscribeCommand.cs
│   ├── YouTubeCommand.cs
│   ├── PodcastCommand.cs
│   ├── BatchCommand.cs
│   └── AnalyzeCommand.cs
├── Services/
│   ├── AudioDownloadService.cs
│   ├── TranscriptionService.cs
│   ├── ContentAnalysisService.cs
│   └── OutputFormatService.cs
├── Models/
│   ├── TranscriptionResult.cs
│   ├── AudioMetadata.cs
│   └── AnalysisResult.cs
└── Parsers/
    └── CycodSpCommandParsers.cs
```

#### Configuration
```bash
# Azure Speech Service setup
cycodsp config --speech-key "KEY" --speech-region "REGION"

# Default output preferences
cycodsp config --default-format "srt" --output-dir "./transcripts"

# AI analysis setup (using existing Cycod chat infrastructure)
cycodsp config --analysis-model "gpt-4" --analysis-endpoint "..."
```

### Integration with Existing Cycod Features

1. **Chat Integration**: Use transcribed content as context for Cycod chat sessions
2. **Template System**: Create templates for common analysis tasks
3. **Logging**: Leverage existing Cycod logging infrastructure
4. **Configuration**: Use Cycod's configuration management

### Example Workflows

#### Workflow 1: Podcast Analysis
```bash
# Download and transcribe latest "You Are Not So Smart" episode
cycodsp podcast --feed "https://youarenotsosmart.com/feed/" --latest --transcribe --output episode-transcript.txt

# Analyze for insights about cognitive biases
cycodsp analyze --file episode-transcript.txt --prompt "Identify key cognitive biases discussed and practical applications"

# Use insights in a chat session
cycod chat --context-file analysis-results.json --prompt "How can I apply these bias insights to improve decision-making?"
```

#### Workflow 2: YouTube Content Processing
```bash
# Process educational video
cycodsp youtube --url "https://youtube.com/watch?v=EDUCATIONAL_VIDEO" --transcribe --format srt

# Create summary and key points
cycodsp analyze --file video-transcript.txt --prompt "Create a structured summary with key learning points"

# Generate follow-up questions
cycod chat --context-file video-analysis.json --prompt "Generate 5 thoughtful questions for deeper understanding"
```

## Implementation Phases

### Phase 1: Basic Transcription
- Implement `cycodsp transcribe` command using Azure Speech Fast Transcription API
- Support for local audio files
- Multiple output formats (text, SRT, VTT)

### Phase 2: Download Integration
- Integrate YouTube audio download from ytd repo
- Add basic podcast downloading capabilities
- File format conversion support

### Phase 3: Analysis & Intelligence
- AI-powered content analysis
- Integration with Cycod chat features
- Template-based analysis workflows

### Phase 4: Advanced Features
- Batch processing capabilities
- RSS feed monitoring
- Automated workflow triggers
- Web interface integration

## Questions & Considerations

1. **Licensing**: Ensure compliance with YouTube ToS and podcast usage rights
2. **Storage**: How to manage downloaded audio files and transcripts
3. **Performance**: Optimize for large audio files and batch processing
4. **Privacy**: Handle sensitive audio content appropriately
5. **Costs**: Manage Azure Speech API costs for large-scale usage

## Next Steps

1. **Research existing code**: Deep dive into ytd and searchy implementations
2. **Prototype basic transcription**: Start with simple file-based transcription
3. **Test integration**: Ensure compatibility with Cycod infrastructure
4. **Design CLI interface**: Define command structure and parameters
5. **Implementation planning**: Detailed technical design and timeline

## Related Links

- [Azure AI CLI Speech Transcribe Documentation](https://github.com/Azure/azure-ai-cli)
- [robch/ytd - YouTube Downloader](https://github.com/robch/ytd)
- [robch/searchy - Web Search Tool](https://github.com/robch/searchy)
- [You Are Not So Smart Podcast](https://youarenotsosmart.com/)

---

*Created: December 5, 2024*  
*Branch: `robch/2512-dec05-speech-transcription-ideas`*