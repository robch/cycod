# --aws-bedrock-region

This option allows you to specify the AWS region to use when connecting to Amazon Bedrock services.

## Syntax

```bash
cycod --aws-bedrock-region REGION
```

Where `REGION` is the AWS region identifier, such as `us-east-1` or `us-west-2`.

## Description

When using Amazon Bedrock as your provider, you need to specify which AWS region to connect to. The `--aws-bedrock-region` option allows you to:

1. Specify the region directly in the command line
2. Override any region that may be configured in your profile or AWS configuration files
3. Target specific AWS regions where your Bedrock models are enabled

Amazon Bedrock may not be available in all AWS regions, and your AWS account may have access to different models in different regions. This option lets you select the appropriate region for your needs.

## Examples

### Basic Usage

```bash
cycod --use-bedrock \
      --aws-bedrock-region "us-east-1" \
      --question "What is Amazon Bedrock?"
```

### Complete Bedrock Configuration

```bash
cycod --use-bedrock \
      --aws-bedrock-region "us-west-2" \
      --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" \
      --aws-bedrock-secret-key "YOUR_SECRET_ACCESS_KEY" \
      --aws-bedrock-model-id "anthropic.claude-3-sonnet-20240229-v1:0" \
      --question "Explain quantum computing"
```

### Using with Environment Variables

```bash
# Set AWS region in environment variable
export AWS_BEDROCK_REGION=us-east-1

# Use the environment variable implicitly
cycod --use-bedrock --question "What is a large language model?"
```

## Setting in Configuration

For regular use, you can store the region in your CYCOD configuration:

```bash
# Set the AWS region for Bedrock in user configuration
cycod config set AWS.Bedrock.Region us-east-1 --user
```

## Available Regions

As of mid-2023, Amazon Bedrock is available in these regions (check AWS documentation for the most current information):

- `us-east-1` (US East, N. Virginia)
- `us-west-2` (US West, Oregon)
- `ap-northeast-1` (Asia Pacific, Tokyo)
- `eu-central-1` (Europe, Frankfurt)

Your access to specific models may vary by region. Not all models are available in all regions.

## Notes

- If you don't specify a region, CYCOD will attempt to use:
  1. The region from your CYCOD configuration
  2. The region from AWS environment variables (AWS_BEDROCK_REGION or AWS_DEFAULT_REGION)
  3. The region from your AWS config file
  4. The default region configured in your AWS CLI (`aws configure`)

- If you're unsure which regions you have access to, you can check with:
  ```bash
  aws bedrock list-foundation-models --region us-east-1
  ```
  (Repeat for different regions to see available models)

## Related Options

| Option | Description |
|--------|-------------|
| `--use-bedrock` | Explicitly select Amazon Bedrock as the provider |
| `--aws-bedrock-access-key` | Specify the AWS Access Key ID |
| `--aws-bedrock-secret-key` | Specify the AWS Secret Access Key |
| `--aws-bedrock-model-id` | Specify which foundation model to use |

## See Also

- [Amazon Bedrock Provider](../../../providers/bedrock.md)
- [`--aws-bedrock-access-key`](bedrock-access-key-id.md)
- [`--aws-bedrock-secret-key`](bedrock-secret-access-key.md)
- [`--aws-bedrock-model-id`](bedrock-model.md)
- [`--use-bedrock`](use-bedrock.md)