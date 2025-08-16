---
hide:
- toc
icon: material/api
---

--8<-- "snippets/ai-generated.md"

# API Integration for Web Search

CYCODMD provides integration with search APIs from both Bing and Google, allowing you to perform powerful web searches directly from the command line without relying on browser automation.

## Overview

By default, CYCODMD performs web searches using browser automation to scrape search results from search engines. While this works for many scenarios, using official search APIs offers several advantages:

- **Higher reliability**: Not affected by UI changes to search engines
- **Better rate limits**: Official APIs have documented rate limits
- **No browser dependencies**: No need for browser installation
- **More consistent results**: API responses follow a consistent format

CYCODMD supports two major search APIs:

1. **Bing Web Search API**
2. **Google Custom Search API**

## Bing Web Search API

### Setup

To use the Bing Web Search API, you need to:

1. Create a Bing Search resource in Azure
2. Obtain your API key and endpoint
3. Configure CYCODMD to use them

#### Creating a Bing Search Resource

1. Go to the [Azure Portal](https://portal.azure.com/)
2. Click on "Create a resource" and search for "Bing Search"
3. Select "Bing Search v7"
4. Create a new resource with your preferred settings
5. After deployment, navigate to the "Keys and Endpoint" section
6. Copy your API key and endpoint URL

#### Configuration

You can provide your Bing Search API credentials in three ways:

1. **Environment variables** (recommended):

``` bash title="Set environment variables"
# Set in your environment
export BING_SEARCH_V7_ENDPOINT=https://api.bing.microsoft.com/v7.0/search
export BING_SEARCH_V7_KEY=your_api_key_here
```

2. **.env file** (recommended for local development):

Create a `.env` file in your project directory or any parent directory:

```bash title=".env file"
BING_SEARCH_V7_ENDPOINT=https://api.bing.microsoft.com/v7.0/search
BING_SEARCH_V7_KEY=your_api_key_here
```

3. **Command line arguments**:

``` bash title="Use command line arguments"
cycodmd web search "search query" --bing-api --bing-api-key "your_api_key" --bing-endpoint "https://api.bing.microsoft.com/v7.0/search"
```

### Usage

Once configured, you can use the Bing API for searches with the `--bing-api` flag:

``` bash title="Search using Bing API"
cycodmd web search "yaml tutorial" --bing-api --max 5
```

This performs a search using the Bing API instead of browser automation, returning the top 5 results.

To fetch and process the content from search results:

``` bash title="Process Bing search results"
cycodmd web search "yaml tutorial" --bing-api --max 3 --get --strip
```

## Google Custom Search API

### Setup

To use the Google Custom Search API:

1. Create a Custom Search Engine
2. Set up Google Cloud credentials
3. Configure CYCODMD to use them

#### Creating a Custom Search Engine

1. Go to [Google Programmable Search Engine](https://programmablesearchengine.google.com/create/new)
2. Create a new search engine
3. Configure your search engine settings (sites to search, etc.)
4. Note your Search Engine ID

#### Setting up Google Cloud credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project (or use an existing one)
3. Enable the "Custom Search API"
4. Create an API key
5. Note your API key

#### Configuration

Configure CYCODMD to use your Google Search credentials:

1. **Environment variables** (recommended):

``` bash title="Set Google API environment variables"
# Set in your environment
export GOOGLE_SEARCH_KEY=your_api_key_here
export GOOGLE_SEARCH_ENGINE_ID=your_engine_id_here
export GOOGLE_SEARCH_ENDPOINT=https://www.googleapis.com/customsearch/v1
```

2. **.env file** (recommended for local development):

Create a `.env` file in your project directory or any parent directory:

```bash title=".env file"
GOOGLE_SEARCH_KEY=your_api_key_here
GOOGLE_SEARCH_ENGINE_ID=your_engine_id_here
GOOGLE_SEARCH_ENDPOINT=https://www.googleapis.com/customsearch/v1
```

3. **Command line arguments**:

``` bash title="Use Google API arguments"
cycodmd web search "search query" --google-api --google-api-key "your_api_key" --google-engine-id "your_engine_id"
```

### Usage

Use the Google API for searches with the `--google-api` flag:

``` bash title="Search using Google API"
cycodmd web search "python list comprehension" --google-api --max 5
```

To fetch and process content from search results:

``` bash title="Process Google search results"
cycodmd web search "python list comprehension" --google-api --max 3 --get --strip --instructions "create a tutorial"
```

## API Rate Limits and Best Practices

### Bing Web Search API

- Free tier: 3 queries per second, 1,000 queries per month
- Paid tiers: Various options with higher rate limits
- [Bing Web Search API documentation](https://learn.microsoft.com/bing/search-apis/bing-web-search/reference/endpoints)

### Google Custom Search API

- Free tier: 100 queries per day
- Paid tier: $5 per 1,000 queries, up to 10,000 queries per day
- [Google Custom Search API documentation](https://developers.google.com/custom-search/v1/introduction)

### Best Practices

1. **Cache results** for frequent searches to avoid hitting rate limits
2. **Be specific** with search queries to get more relevant results
3. **Use the `--max` parameter** judiciously to limit API usage
4. **Consider storing API keys** in environment variables or .env files for security
5. **Monitor your usage** to avoid unexpected charges

## Combining API Search with AI Processing

One of CYCODMD's most powerful features is combining API-based web searches with AI processing:

``` bash title="Combine API search with AI processing"
cycodmd web search "latest javascript frameworks 2025" --bing-api --max 5 --get --strip --instructions "Create a comparison table of the top JavaScript frameworks mentioned in these search results"
```

This command:
1. Searches for "latest javascript frameworks 2025" using the Bing API
2. Gets the content from the top 5 search results
3. Strips HTML tags
4. Passes the content to an AI model with instructions to create a comparison table

## Troubleshooting

### Common Issues

1. **API Key Authentication Errors**
   
   Check that your API keys are correctly set and haven't expired.

2. **Rate Limit Exceeded**
   
   Reduce the number of queries or upgrade your API tier.

3. **No Results Returned**
   
   Try broadening your search terms or check API configuration.

4. **Search Engine ID Not Found** (Google)
   
   Verify your Custom Search Engine ID.

### Checking API Configuration

To verify your API settings are correctly configured:

``` bash title="Debug API configuration"
cycodmd web search "test" --bing-api --debug
```

or

``` bash title="Debug Google API configuration"
cycodmd web search "test" --google-api --debug
```

The `--debug` flag will show detailed information about the API request and response.

## Related Resources

- [Bing Web Search API Documentation](https://learn.microsoft.com/bing/search-apis/bing-web-search/overview)
- [Google Custom Search JSON API Documentation](https://developers.google.com/custom-search/v1/overview)
- [CYCODMD Web Search Reference](../../reference/cycodmd/commands/web-search.md)
- [CYCODMD Web Get Reference](../../reference/cycodmd/commands/web-get.md)