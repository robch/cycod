# Image Generation as AI Function Tool

## Overview

Add `GenerateImages` as an AI function tool to enable natural language image generation during chat sessions, complementing the existing `cycod imagine` CLI command.

## Current State

- `cycod imagine "prompt"` - CLI command for explicit image generation
- `--image FILE` / `/image FILE` - Add existing images to conversation (input)
- No way to generate images naturally during chat without exiting

## Proposed Solution

Implement `GenerateImages` as a function tool that AI can invoke during conversation.

### User Experience

**Instead of:**
```bash
# Exit chat or use complex slash command
cycod imagine "weather app icon" --count 3 --size 1024x1024
```

**Natural conversation:**
```
User: Can you create me three weather app icons? Make them minimalist and blue.

AI: I'll generate three weather app icons for you.
[Function call: GenerateImages approved]
✓ Generated weather-icon-20250113-143022-1.png
✓ Generated weather-icon-20250113-143022-2.png  
✓ Generated weather-icon-20250113-143022-3.png

I've created three minimalist blue weather app icons...
```

## Benefits

1. **More Natural** - Users describe intent, AI handles implementation
2. **Better Iteration** - Conversational refinement workflow
   - "Create a logo" → "Make it more vintage" → "Add warm colors"
3. **AI Value-Add** - Prompt engineering, smart defaults, context awareness
4. **Consistent** - Matches existing function tool patterns
5. **Flexible** - CLI command remains for scripting/batch operations

## Function Tool Design

### Schema

```typescript
{
  name: "GenerateImages",
  description: "Generate images from text descriptions using AI (DALL-E)",
  parameters: {
    prompt: {
      type: "string",
      description: "Detailed image description. Be specific about style, colors, composition.",
      required: true
    },
    count: {
      type: "integer", 
      description: "Number of variations (1-10)",
      default: 1
    },
    size: {
      type: "string",
      enum: ["1024x1024", "1792x1024", "1024x1792"],
      description: "Dimensions. Use 1792x1024 for hero/landscape",
      default: "1024x1024"
    },
    quality: {
      type: "string",
      enum: ["standard", "hd"],
      default: "standard"
    },
    style: {
      type: "string",
      enum: ["vivid", "natural"],
      description: "vivid=dramatic, natural=photorealistic",
      default: "vivid"
    },
    add_to_conversation: {
      type: "boolean",
      description: "Add generated images to conversation for analysis",
      default: false
    },
    output_directory: {
      type: "string",
      description: "Save location",
      default: "."
    }
  }
}
```

### Tool Response Format

```json
{
  "success": true,
  "images": [
    {
      "path": "./weather-icon-20250113-143022-1.png",
      "prompt": "weather app icon, minimalist design, blue and white",
      "size": "1024x1024",
      "format": "png"
    }
  ],
  "count": 1,
  "added_to_conversation": false
}
```

## Implementation Plan

### 1. Extract Image Generation Logic

- Move logic from `ImagineCommand` to shared service
- Create `ImageGenerationService.cs` (or similar)
- Service handles both CLI and function tool calls

### 2. Create Function Tool

- Add to function tool catalog
- Implement tool handler (e.g., `ImageGenerationFunctionTool.cs`)
- Handle parameter validation and defaults

### 3. Integration

- Register tool in function tool system
- Ensure provider compatibility (Azure OpenAI, OpenAI)
- Handle errors gracefully with user-friendly messages

### 4. Documentation

- Update function calls help
- Add examples to help system
- Document auto-approval options

### 5. Testing

- Unit tests for service extraction
- Integration tests for tool invocation
- Test with different providers
- Test auto-add to conversation feature

## Key Design Decisions to Consider

### 1. Cost Control

Image generation costs money:
- **Option A**: Require approval by default (like write operations)
- **Option B**: Covered by `--auto-approve write`
- **Option C**: Specific approval: `--auto-approve GenerateImages`
- **Recommendation**: Default requires approval, can be auto-approved per user preference

### 2. Auto-Add to Conversation

When should generated images be added to conversation?
- **Option A**: Always auto-add (AI can "see" what it created)
- **Option B**: Never auto-add (keeps conversation light)
- **Option C**: AI decides via `add_to_conversation` parameter
- **Recommendation**: Option C - context-dependent via parameter

### 3. Prompt Engineering

Who crafts the DALL-E prompt?
- **Option A**: Pass user request verbatim
- **Option B**: AI enhances prompt for better results
- **Recommendation**: Option B - AI adds details for optimal generation

### 4. File Management

Where do generated images go?
- Default to current directory (like CLI)
- AI can specify `output_directory` parameter
- Could add smart defaults based on prompt context

## Use Cases

### Iterative Refinement
```
User: Create a coffee shop logo
AI: [generates]
User: Make it more vintage
AI: [regenerates with vintage style]
User: Add warm brown tones
AI: [refines further]
```

### Batch Generation with Context
```
User: I need icons for sunny, rainy, and cloudy weather
AI: [generates 3 with consistent style]
```

### Smart Defaults
```
User: Generate a hero image for my landing page
AI: [infers 1792x1024 landscape, hd quality, natural style]
```

## Comparison: CLI vs Function Tool

| Aspect | `cycod imagine` | Function Tool |
|--------|-----------------|---------------|
| Explicitness | High | Low |
| Convenience | Low (exit chat) | High (in flow) |
| Natural language | No | Yes |
| AI enhancement | No | Yes |
| Iteration | Awkward | Natural |
| Control | Full | Delegated |
| Scripting | Excellent | N/A |

**Both should coexist** - different use cases.

## Open Questions

1. Should there be rate limiting or cost warnings?
2. How verbose should approval prompts be?
3. Should we track and report cumulative costs?
4. File naming: timestamps vs AI-suggested names?
5. Multi-image batching limits?
6. Integration with existing `--image` / `/image` features?

## Related Work

- Existing `ImagineCommand` implementation
- Function tool infrastructure
- Image handling in conversation context
- Provider abstraction (Azure OpenAI, OpenAI)

## Priority

**Medium** - Nice quality-of-life improvement, not critical functionality.

## Related Considerations: `--add-image` Naming

While implementing this, also consider:
- Renaming `--image` to `--add-image` for consistency with `--add-system-prompt`, `--add-user-prompt`
- Could keep `--image` as alias for convenience
- Slash command `/image` should probably stay short (all slash commands are brief)

## Notes

- Image generation is marked as experimental (MEAI001)
- Requires provider with DALL-E support
- Generated files include timestamps in names
- Generation takes 10-30 seconds per image
