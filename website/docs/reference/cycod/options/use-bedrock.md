# --use-bedrock

The `--use-bedrock` option explicitly selects Amazon Bedrock as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-bedrock [other options]
```

## Description

When you specify `--use-bedrock`, CYCOD will:

- Use Amazon Bedrock for the chat session
- Override any default provider settings
- Expect proper AWS credentials and Bedrock configuration to be available

This option is useful when you want to explicitly use Amazon Bedrock for a specific command, regardless of your default provider settings.

## Related Bedrock Options

The `--use-bedrock` option is typically used with the following Bedrock-specific options:

- [`--aws-bedrock-access-key`](./bedrock-access-key-id.md): Specifies the AWS access key ID
- [`--aws-bedrock-secret-key`](./bedrock-secret-access-key.md): Provides the AWS secret access key
- [`--aws-bedrock-region`](./bedrock-region.md): Specifies the AWS region to use
- [`--aws-bedrock-model-id`](./bedrock-model.md): Specifies which foundation model to use

## Examples

**Basic usage:**

```bash
cycod --use-bedrock --question "What is Amazon Bedrock?"
```

**Using with specific AWS credentials:**

```bash
cycod --use-bedrock \
      --aws-bedrock-access-key "YOUR_ACCESS_KEY_ID" \
      --aws-bedrock-secret-key "YOUR_SECRET_ACCESS_KEY" \
      --aws-bedrock-region "us-east-1" \
      --question "Explain quantum computing"
```

**Specifying a foundation model:**

```bash
cycod --use-bedrock --aws-bedrock-model-id "anthropic.claude-3-sonnet-20240229-v1:0" --question "Tell me about Claude models"
```

**Interactive chat using Amazon Bedrock:**

```bash
cycod --use-bedrock --interactive
```

## Configuration

Instead of specifying this option on each command, you can set Amazon Bedrock as your default provider using:

```bash
cycod config set app.preferredProvider bedrock --user
```

Or create a profile for using Amazon Bedrock by creating a YAML file:

```yaml title="bedrock.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "bedrock"

bedrock:
  model: "anthropic.claude-3-sonnet-20240229-v1:0"
  region: "us-east-1"
```

Then use it with:

```bash
cycod --profile bedrock --question "What is Amazon Bedrock?"
```

## Notes

- Make sure you have valid AWS credentials and Bedrock access before using this option.
- This option works best if you have already configured AWS credentials using the AWS CLI (`aws configure`).
- You need to have access to the specific models you're trying to use in your AWS account.

## See Also

- [--aws-bedrock-model-id](./bedrock-model.md)
- [--aws-bedrock-region](./bedrock-region.md)
- [--use-openai](./use-openai.md)
- [--use-anthropic](./use-anthropic.md)
- [--use-gemini](./use-gemini.md)
- [Amazon Bedrock Provider Documentation](../../../providers/bedrock.md)
- [Provider Selection Guide](../../../providers/overview.md)