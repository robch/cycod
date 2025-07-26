---
hide:
- toc
icon: material/aws
---

The `cycod config set` command allows you to configure your Amazon Bedrock settings to access AI models including Claude, Llama 2, and Titan.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up Amazon Bedrock configuration"
cycod config set AWS.Bedrock.AccessKey YOUR_ACCESS_KEY_ID --user
cycod config set AWS.Bedrock.SecretKey YOUR_SECRET_ACCESS_KEY --user
cycod config set AWS.Bedrock.Region us-east-1 --user
cycod config set AWS.Bedrock.ModelId anthropic.claude-3-sonnet-20240229-v1:0 --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View Bedrock config values"
cycod config get AWS.Bedrock.AccessKey
cycod config get AWS.Bedrock.SecretKey
cycod config get AWS.Bedrock.Region
cycod config get AWS.Bedrock.ModelId
```
