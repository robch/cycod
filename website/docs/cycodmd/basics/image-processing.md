---
hide:
- toc
icon: material/image
---

--8<-- "snippets/ai-generated.md"

# Image Processing with CYCODMD

CYCODMD can convert images to rich markdown descriptions using Azure OpenAI's vision capabilities. This feature extracts both visual descriptions and any visible text from images, providing detailed, accessible content for your markdown documents.

## How Image Processing Works

When you provide an image file to CYCODMD, it:

1. Sends the image to Azure OpenAI's vision API
2. Extracts a detailed description of the image content
3. Identifies and extracts any text visible in the image
4. Generates structured markdown with both the description and text
5. Includes relevant details about the image format, dimensions, and other metadata

## Setup Requirements

To use CYCODMD's image processing capabilities, you need an Azure OpenAI resource with a vision-compatible model deployed.

### Azure OpenAI Setup

1. Create an Azure OpenAI resource in the Azure AI Foundry portal:

    ``` bash title="Create Azure OpenAI resource"
    # Either visit https://ai.azure.com/ or use the Azure CLI:
    az cognitiveservices account create --name your-resource-name --resource-group your-resource-group --kind OpenAI --sku s0 --location your-location
    ```

2. Deploy a vision-compatible model (like GPT-4o):

    ``` bash title="Deploy vision model"
    az cognitiveservices account deployment create --name your-resource-name --resource-group your-resource-group --deployment-name your-deployment-name --model-name gpt-4o --model-version 2023-06-01
    ```

3. Get your API key and endpoint from the Azure portal or using Azure CLI:

    ``` bash title="Get API key"
    az cognitiveservices account keys list --name your-resource-name --resource-group your-resource-group
    ```

### Environment Configuration

Set up your environment variables either in your active shell or in a `.env` file in your working directory:

``` bash title="Configure environment variables"
AZURE_OPENAI_API_KEY=your_api_key_here
AZURE_OPENAI_ENDPOINT=https://your-resource-name.cognitiveservices.azure.com/
AZURE_OPENAI_CHAT_DEPLOYMENT=your-deployment-name
```

## Basic Usage

Once set up, you can process images with simple commands:

``` bash title="Process a single image"
cycodmd image.png
```

``` bash title="Process multiple images"
cycodmd *.jpg *.png
```

``` bash title="Process with instructions"
cycodmd image.png --file-instructions "Keep only a formatted summary of the text in markdown"
```

## Advanced Options

### Filtering Images

``` bash title="Filter by file type"
cycodmd **/*.jpg **/*.png --exclude "**/thumbnail/*"
```

``` bash title="Filter by content"
cycodmd **/*.jpg --file-contains "diagram"
```

### Custom Output Formatting

``` bash title="Save output to separate files"
cycodmd **/*.png --save-file-output "outputs/{fileBase}.md"
```

``` bash title="Apply specific instructions"
cycodmd **/*.png --file-instructions "Create an accessible description for screen readers"
```

## Performance Considerations

Processing images with AI vision capabilities is resource-intensive. Consider these tips:

- Process images in batches using the `--threads` option to control parallelism
- Start with smaller images before processing large or high-resolution images
- Consider image compression if you're processing many large images

``` bash title="Control parallel processing"
cycodmd **/*.png --threads 4
```
