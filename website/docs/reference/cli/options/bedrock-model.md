# --aws-bedrock-model-id

The `--aws-bedrock-model-id` option specifies which foundation model to use when interacting with Amazon Bedrock.

## Syntax

```bash
--aws-bedrock-model-id MODEL_ID
```

Where `MODEL_ID` is the full identifier of the foundation model in Amazon Bedrock, such as `anthropic.claude-3-sonnet-20240229-v1:0` or `meta.llama2-13b-chat-v1`.

## Description

When using Amazon Bedrock as your provider, you can select from various foundation models offered by different AI providers. The `--aws-bedrock-model-id` option allows you to specify exactly which model you want to use for your request.

Amazon Bedrock provides access to models from multiple AI providers, including Anthropic's Claude models, Meta's Llama models, Amazon's Titan models, and more. Each model has different capabilities, costs, and performance characteristics.

## Examples

### Basic Usage with Claude

```bash
cycod --use-bedrock \
      --aws-bedrock-model-id "anthropic.claude-3-sonnet-20240229-v1:0" \
      --question "What is Amazon Bedrock?"
```

### Using a Llama Model

```bash
cycod --use-bedrock \
      --aws-bedrock-model-id "meta.llama2-13b-chat-v1" \
      --question "Explain quantum computing in simple terms."
```

### Using Amazon's Titan Model

```bash
cycod --use-bedrock \
      --aws-bedrock-model-id "amazon.titan-text-express-v1" \
      --question "Write a short story about space exploration."
```

### Complete Configuration Example

```bash
cycod --use-bedrock \
      --aws-bedrock-model-id "anthropic.claude-3-sonnet-20240229-v1:0" \
      --aws-bedrock-region "us-east-1" \
      --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" \
      --aws-bedrock-secret-key "YOUR_SECRET_ACCESS_KEY" \
      --question "Explain the differences between various AI models."
```

## Available Models

Below are some common model IDs available in Amazon Bedrock (check AWS documentation for the most current list):

### Anthropic Claude Models

- `anthropic.claude-3-sonnet-20240229-v1:0` - Claude 3 Sonnet, general purpose
- `anthropic.claude-3-haiku-20240307-v1:0` - Claude 3 Haiku, fast and efficient
- `anthropic.claude-instant-v1` - Claude Instant, faster responses

### Meta Llama Models

- `meta.llama2-13b-chat-v1` - Llama 2 (13B parameters)
- `meta.llama2-70b-chat-v1` - Llama 2 (70B parameters)

### Amazon Titan Models

- `amazon.titan-text-express-v1` - Titan Text Express
- `amazon.titan-embed-text-v1` - Titan Text Embeddings

### Other Models

- `cohere.command-text-v14` - Cohere Command
- `ai21.j2-ultra-v1` - AI21 Jurassic-2 Ultra

## Configuration

Instead of specifying the model on each command, you can set a default model in your CYCOD configuration:

```bash
# Set default Bedrock model
cycod config set AWS.Bedrock.ModelId anthropic.claude-3-sonnet-20240229-v1:0 --user
```

## Notes

- You must have access to the specified model in your AWS account
- Different models may incur different costs
- Not all models are available in all AWS regions
- Model versions may change over time; check AWS documentation for the latest IDs
- Bedrock model IDs follow the format: `provider.model-name-version`

## Related Options

| Option | Description |
|--------|-------------|
| `--use-bedrock` | Explicitly select Amazon Bedrock as the provider |
| `--aws-bedrock-access-key` | Specify the AWS Access Key ID |
| `--aws-bedrock-secret-key` | Specify the AWS Secret Access Key |
| `--aws-bedrock-region` | Specify the AWS region to use |

## See Also

- [Amazon Bedrock Provider](../../../providers/bedrock.md)
- [`--aws-bedrock-access-key`](bedrock-access-key-id.md)
- [`--aws-bedrock-secret-key`](bedrock-secret-access-key.md)
- [`--aws-bedrock-region`](bedrock-region.md)
- [`--use-bedrock`](use-bedrock.md)