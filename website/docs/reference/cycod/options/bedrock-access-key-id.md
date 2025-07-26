# --aws-bedrock-access-key

The `--aws-bedrock-access-key` option allows you to specify an AWS Access Key ID for Amazon Bedrock when using CYCOD.

## Syntax

```bash
--aws-bedrock-access-key ACCESS_KEY_ID
```

## Description

This option sets the AWS Access Key ID used to authenticate with Amazon Bedrock services. When provided, CYCOD will use this key to authenticate requests to Amazon Bedrock, overriding any keys stored in your configuration or AWS credentials file.

The AWS Access Key ID is part of your AWS security credentials and works together with your Secret Access Key to authenticate your requests to AWS services.

## Examples

### Example 1: Basic usage with Amazon Bedrock

```bash
cycod --use-bedrock --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" --question "What is Amazon Bedrock?"
```

### Example 2: Complete Amazon Bedrock configuration

```bash
cycod --use-bedrock \
      --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" \
      --aws-bedrock-secret-key "YOUR_SECRET_ACCESS_KEY" \
      --aws-bedrock-region "us-east-1" \
      --aws-bedrock-model-id "anthropic.claude-3-sonnet-20240229-v1:0" \
      --question "Explain how Amazon Bedrock works"
```

### Example 3: Interactive chat with Amazon Bedrock

```bash
cycod --use-bedrock \
      --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" \
      --aws-bedrock-secret-key "YOUR_SECRET_ACCESS_KEY" \
      --interactive
```

### Example 4: Using an environment variable (recommended for scripts)

```bash
# Set the key in an environment variable
export AWS_BEDROCK_ACCESS_KEY="YOUR_ACCESS_KEY_ID"

# Use Bedrock without needing to specify the key on command line
cycod --use-bedrock --question "What is Amazon Bedrock?"
```

## Configuration Alternative

For regular use, it's recommended to store your AWS credentials in the standard AWS credentials file or in the CYCOD configuration instead of passing it on the command line:

```bash
# Store AWS credentials in CYCOD configuration
cycod config set AWS.Bedrock.AccessKey "YOUR_ACCESS_KEY_ID" --user

# Or use the AWS CLI to configure credentials (recommended)
aws configure
```

## Security Considerations

AWS Access Key IDs are sensitive security credentials:

- Avoid specifying the Access Key ID directly on the command line in production environments
- Never commit scripts containing the Access Key ID to version control
- Consider using IAM roles for EC2 instances when running CYCOD on AWS
- Use the AWS CLI to configure credentials when possible

## Configuration Precedence

When CYCOD looks for the AWS Access Key ID, it checks sources in this order:

1. Command-line option (`--aws-bedrock-access-key`)
2. Environment variable (`AWS_BEDROCK_ACCESS_KEY`)
3. CYCOD configuration files (local, then user, then global scope)
4. AWS credentials file (`~/.aws/credentials`)
5. IAM role (if running on an EC2 instance)

Using the command-line option will override any value set in environment variables or configuration files.

## Related Options

| Option | Description |
|--------|-------------|
| `--use-bedrock` | Explicitly select Amazon Bedrock as the provider |
| `--aws-bedrock-secret-key` | Specify the AWS Secret Access Key |
| `--aws-bedrock-region` | Specify the AWS region to use |
| `--aws-bedrock-model-id` | Specify which foundation model to use |

## See Also

- [Amazon Bedrock Provider](../../../providers/bedrock.md)
- [`--aws-bedrock-secret-key`](bedrock-secret-access-key.md)
- [`--aws-bedrock-region`](bedrock-region.md)
- [`--aws-bedrock-model-id`](bedrock-model.md)
- [`--use-bedrock`](use-bedrock.md)